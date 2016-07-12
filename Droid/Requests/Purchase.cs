using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Json;
using Android.App;
using Android.Content;

namespace Hauptmedia.InAppBilling.Android.Requests
{
	public class Purchase : Request<PaymentTransaction> {

		private readonly int PAYMENT_ACTIVITY_REQUESTCODE = 27051985;

		protected string _publicKey;
		protected string _productId;
		protected InAppBillingService _service;

		public Purchase(string productId, string publicKey) : base()
		{
			_productId = productId;
			_publicKey = publicKey;

			TCS = new TaskCompletionSource<PaymentTransaction>();
		}

		public override Task<PaymentTransaction> Execute(InAppBillingService service)
		{
			_service = service;
			var task = TCS.Task;

			//try
			//{
				var products = service.RequestProducts(new List<string>() { _productId }).ContinueWith((t) =>
				{
					var product = t.Result.FirstOrDefault();

					if (product == null)
					{
						TCS.SetResult(new PaymentTransaction()
						{
							ProductIdentifier = _productId,
							ErrorDescription = "Could not find product with productId",
							TransactionState = PaymentTransactionState.Failed
						});
						service.PurchaseCompleted();

					}
					else if (!service.CanMakePayments || 
					         (product.Type == ProductType.Subscription && !service.CanMakeSubscriptions))
					{
						TCS.SetResult(new PaymentTransaction()
						{
							ProductIdentifier = _productId,
							ErrorDescription = "The operation is not supported",
							TransactionState = PaymentTransactionState.Failed
						});
						service.PurchaseCompleted();

					}
					else {
						var buyIntentBundle = service.InAppService.GetBuyIntent(
							InAppBillingService.GPS_API_VERSION,
							service.Context.PackageName,
							_productId,
							(product.Type == ProductType.Subscription) ? InAppBillingService.GPS_ITEM_TYPE_SUBS : InAppBillingService.GPS_ITEM_TYPE_INAPP,
							"" //Developer Payload
						);

						int response = Utils.GetResponseCodeFromBundle(buyIntentBundle);

						if (response != Consts.BILLING_RESPONSE_RESULT_OK)
						{
							TCS.SetResult(new PaymentTransaction()
							{
								ProductIdentifier = _productId,
								ErrorDescription = Utils.GetResponseDesc(response),
								TransactionState = PaymentTransactionState.Failed
							});
							service.PurchaseCompleted();

						}
						else {
							var pendingIntent = buyIntentBundle.GetParcelable(Consts.RESPONSE_BUY_INTENT) as PendingIntent;
							service.Activity.StartIntentSenderForResult(
								pendingIntent.IntentSender,
								PAYMENT_ACTIVITY_REQUESTCODE,
								new Intent(),
								0,
								0,
								0
							);
						}
					}
				});
			//}
			//catch (Exception e)
			//{
			//	TCS.SetResult(new PaymentTransaction()
			//	{
			//		ProductIdentifier = _productId,
			//		ErrorDescription = e.Message,
			//		TransactionState = PaymentTransactionState.Failed
			//	});
			//}

			return task;
		}

		public bool HandleActivityResult(int requestCode, int resultCode, Intent data)
		{
			//do not handle this activity result if it was not meant for us
			if (requestCode != PAYMENT_ACTIVITY_REQUESTCODE)
				return false;

			var trx = CreatePaymentTransactionFromResult(_publicKey, resultCode, data);

			TCS.SetResult(trx);
			_service.PurchaseCompleted();

			return true;
		}

		private static PaymentTransaction CreatePaymentTransactionFromResult(string publicKey, int resultCode, Intent data)
		{
			var ptrx = new PaymentTransaction();

			if (resultCode == (int)Result.Canceled)
			{
				ptrx.ErrorDescription = "Purchase was cancelled";
				ptrx.TransactionState = PaymentTransactionState.Failed;
				return ptrx;
			}

			if (resultCode != (int)Result.Ok)
			{
				ptrx.ErrorDescription = "Purchase error. Unknown result code: " + resultCode;
				ptrx.TransactionState = PaymentTransactionState.Failed;
				return ptrx;
			}

			if (data == null)
			{
				ptrx.ErrorDescription = "Data intent parameter is null";
				ptrx.TransactionState = PaymentTransactionState.Failed;
				return ptrx;
			}

			//try
			//{
				var responseCode = Utils.GetResponseCodeFromIntent(data);
				string purchaseData = data.GetStringExtra(Consts.RESPONSE_INAPP_PURCHASE_DATA);
				string dataSignature = data.GetStringExtra(Consts.RESPONSE_INAPP_SIGNATURE);

				if (responseCode != Consts.BILLING_RESPONSE_RESULT_OK)
				{
					ptrx.ErrorDescription = "Result code was OK, but InAppService response was not OK. Response Code: " + Utils.GetResponseDesc(responseCode);
					ptrx.TransactionState = PaymentTransactionState.Failed;
					return ptrx;
				}

				if (purchaseData == null || dataSignature == null)
				{
					ptrx.ErrorDescription = string.Format("Null data. PurchaseData: {0}, DataSignature: {1}", purchaseData, dataSignature);
					ptrx.TransactionState = PaymentTransactionState.Failed;
					return ptrx;
				}

				//Check signature
				if (!Security.VerifyPurchase(publicKey, purchaseData, dataSignature))
				{
					ptrx.ErrorDescription = "Purchase signature verification failed.";
					ptrx.TransactionState = PaymentTransactionState.Failed;
					return ptrx;

				}

				var o = JsonObject.Parse(purchaseData) as JsonObject;

				ptrx.TransactionIdentifier = o["orderId"];
				ptrx.Token = o["purchaseToken"];
				ptrx.ProductIdentifier = o["productId"];

				ptrx.TransactionState = PaymentTransactionState.Purchased;
			//}
			//catch
			//{
			//	ptrx.TransactionState = PaymentTransactionState.Failed;
			//}

			return ptrx;
		}

	}
}

