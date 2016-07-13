using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Hauptmedia.InAppBilling.Forms
{
	public class TransactionDebugViewModel
	{
		public IInAppBillingService InAppBillingService { get; private set; }

		public ICommand ConsumeCommand { get; private set; }

		public PaymentTransaction PaymentTransaction { get; private set; }

		public TransactionDebugViewModel(ContentPage contentPage, IInAppBillingService inAppBillingService, PaymentTransaction paymentTransaction)
		{
			InAppBillingService = inAppBillingService;
			PaymentTransaction = paymentTransaction;

			ConsumeCommand = new Command(async (e) =>
			{
				var success = await inAppBillingService.ConsumeProduct(
					PaymentTransaction.Token
				);

				Device.BeginInvokeOnMainThread(() =>
				{
					contentPage.DisplayAlert(
						"In-App Billing Result",
						"Success : " + success.ToString(),
						"OK"
					);
				});
			});

		}
	}
}

