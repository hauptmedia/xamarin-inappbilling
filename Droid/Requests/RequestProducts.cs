using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;

namespace Hauptmedia.InAppBilling.Android.Requests
{
	public class RequestProducts : Request<IEnumerable<Product>>
	{
		protected IEnumerable<string> _productIds;
		protected string _productType;

		public RequestProducts(IEnumerable<string> productIds, string productType) : base()
		{
			_productIds = productIds;
			_productType = productType;
		}

		public override Task<IEnumerable<Product>> Execute(InAppBillingService service)
		{
			if (_productIds == null || _productIds.ToArray().Count() == 0)
				throw new ArgumentException("ProductIds should not be null or empty.");

			if (_productIds.ToArray().Count() > 20)
				throw new Exception("InAppService only allows a maximum of 20 SKUs at one time. Batch your requests.");

			return Task.Factory.StartNew<IEnumerable<Product>>(() =>
			{
				var bundle = new Bundle();
				bundle.PutStringArrayList("ITEM_ID_LIST", _productIds.ToArray());

				var skuDetails = service.InAppService.GetSkuDetails(
					InAppBillingService.GPS_API_VERSION,
					service.Context.PackageName,
					_productType,
					bundle
				);

				if (!skuDetails.ContainsKey("DETAILS_LIST"))
				{
					int response = Utils.GetResponseCodeFromBundle(skuDetails);
					if (response != Consts.BILLING_RESPONSE_RESULT_OK)
					{
						Utils.LogError(string.Format("SkueDetails.Execute failed. Message : {0}", Utils.GetResponseDesc(response)));
						//					this.TCS.SetResult(new GetSkuDetailsResponse(response));
					}
					else
					{
						Utils.LogError("SkueDetails.Execute failed. Neither an error nor a detail list.");
						//					this.TCS.SetResult(new GetSkuDetailsResponse(Consts.BAD_RESPONSE));
					}
				}

				var responseList = skuDetails.GetStringArrayList("DETAILS_LIST");

				return responseList.ToList()
					.Select(jsonSkuDetails => JsonObject.Parse(jsonSkuDetails) as JsonObject)
					.Select(o => new Product()
					{
						//price_currency_code
						Type = (o["type"] == InAppBillingService.GPS_ITEM_TYPE_SUBS) ? ProductType.Subscription : ProductType.OneTimePurchase,
						ProductIdentifier = o["productId"],
						Price = o["price_amount_micros"] / (decimal)1000000,
						Title = o["title"],
						Description = o["description"]
					});
			});
		}

	}
}

