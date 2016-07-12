using System;
using System.Text;
using System.Collections.Generic;
using Android.Text;
using Android.Widget;
using Hauptmedia.InAppBilling.Android.Requests;
using System.Threading.Tasks;
using System.Json;

namespace Hauptmedia.InAppBilling.Android.Requests
{
	
	public class RequestTransactions : Request<IEnumerable<PaymentTransaction>>
	{
		protected string _productType;
		protected string _publicKey;

		public RequestTransactions(string productType, string publicKey) : base()
		{
			_productType = productType;
			_publicKey = publicKey;
		}

		public override Task<IEnumerable<PaymentTransaction>> Execute(InAppBillingService service)
		{
			TCS = new TaskCompletionSource<IEnumerable<PaymentTransaction>>();

			var responseCode = Consts.BILLING_RESPONSE_RESULT_DEVELOPER_ERROR;

			try
			{
				string continueToken = null;
				var transactions = new List<PaymentTransaction>();

				do
				{
					var purchased = service.InAppService.GetPurchases(
						InAppBillingService.GPS_API_VERSION,
						service.Context.PackageName,
						_productType,
						continueToken
					);

					responseCode = Utils.GetResponseCodeFromBundle(purchased);

					Utils.LogDebug("GetPurchases.Execute response: " + responseCode.ToString());

					if (responseCode != Consts.BILLING_RESPONSE_RESULT_OK)
					{
						Utils.LogDebug("GetPurchases.Execute failed: " + Utils.GetResponseDesc(responseCode));
						TCS.SetResult(null);
						return TCS.Task;
					}

					if (!purchased.ContainsKey("INAPP_PURCHASE_ITEM_LIST")
					    || !purchased.ContainsKey("INAPP_PURCHASE_DATA_LIST")
						|| !purchased.ContainsKey("INAPP_DATA_SIGNATURE_LIST"))
					{
						Utils.LogError("GetPurchases.Execute. Bundle returned doesn't contain the required fields.");
						TCS.SetResult(null);
						return TCS.Task;
					}

					var purchasedSKUs = purchased.GetStringArrayList("INAPP_PURCHASE_ITEM_LIST");
					var purchaseDataList = purchased.GetStringArrayList("INAPP_PURCHASE_DATA_LIST");
					var signatureList = purchased.GetStringArrayList("INAPP_DATA_SIGNATURE_LIST");

					for (int i = 0; i < purchaseDataList.Count; ++i)
					{
						try
						{
							var purchaseData = purchaseDataList[i];
							var signature = signatureList[i];
							var sku = purchasedSKUs[i];


							var transaction = new PaymentTransaction();

							//itemType
							var o = JsonObject.Parse(purchaseData) as JsonObject;

							//A unique order identifier for the transaction. This corresponds to the Google Wallet Order ID.
							transaction.TransactionIdentifier = o["orderId"];

							//A token that uniquely identifies a purchase for a given item and user pair.
							transaction.Token = o["purchaseToken"];

							//The item's product identifier. Every item has a product ID, which you must specify in the application's product list on the Google Play Developer Console.
							transaction.ProductIdentifier = o["productId"];

							//The purchase state of the order. Possible values are 0 (purchased), 1 (canceled), or 2 (refunded).
							if (o.ContainsKey("purchaseState"))
							{
								switch (Convert.ToInt32(o["purchaseState"].ToString())) {
									case 0: //purchased
										transaction.TransactionState = PaymentTransactionState.Purchased;
										break;

									case 1: //canceled
										transaction.TransactionState = PaymentTransactionState.Canceled;
										break;

									case 2: //refunded
										transaction.TransactionState = PaymentTransactionState.Refunded;
										break;
								}
							}
							//The time the product was purchased, in milliseconds since the epoch (Jan 1, 1970).
							//PurchaseTime = Convert.ToInt64(o["purchaseTime"].ToString());

							//The application package from which the purchase originated.
							//o["packageName"]

							//A developer-specified string that contains supplemental information about an order. You can specify a value for this field when you make a getBuyIntent request.
							//if (o.ContainsKey("developerPayload"))
							//	DeveloperPayload = o["developerPayload"];



							if (Security.VerifyPurchase(_publicKey, purchaseData, signature))
							{
								Utils.LogDebug("Has Purchased: " + sku);
								transactions.Add(transaction);

								if (TextUtils.IsEmpty(transaction.Token))
								{
									Utils.LogWarn("BUG: empty/null token!");
									Utils.LogDebug("Purchased data: " + purchaseData);
								}

							}
							else
							{
								Utils.LogWarn("Purchased signature verification **FAILED**. Not adding item.");
								Utils.LogDebug("   Purchase data: " + purchaseData);
								Utils.LogDebug("   Signature: " + signature);
							}
						}
						catch (Exception e)
						{
							Utils.LogError(e.Message);
						}
					}

					continueToken = purchased.GetString("INAPP_CONTINUATION_TOKEN");
					Utils.LogDebug("Continuation token: " + continueToken);

				} while (!TextUtils.IsEmpty(continueToken));

				TCS.SetResult(transactions);

			}
			catch (Exception e)
			{
				Utils.LogError("GetPurchases exception. " + e.ToString());
				TCS.SetResult(null);
			}

			return TCS.Task;
		}
	}
}