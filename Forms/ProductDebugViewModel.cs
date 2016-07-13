using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Hauptmedia.InAppBilling.Forms
{
	public class ProductDebugViewModel
	{
		public IInAppBillingService InAppBillingService { get; private set; }

		public ICommand BuyCommand { get; private set; }

		public Product Product { get; private set; }

		public ProductDebugViewModel (ContentPage contentPage, IInAppBillingService inAppBillingService, Product product)
		{
			Product = product;
			InAppBillingService = inAppBillingService;

			BuyCommand = new Command (async (e) => {
				var trx = await inAppBillingService.PurchaseProduct(
					Product.ProductIdentifier
				);
					
				Device.BeginInvokeOnMainThread(() =>
				{
					contentPage.DisplayAlert(
						"In-App Billing Result",
						"TranscationState:\n" + trx.TransactionStateString + "\n\n" +
						"ErrorDescription:\n" + trx.ErrorDescription,
						"OK"
					);
				});
			});


		}
	}
}

