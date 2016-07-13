using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Hauptmedia.InAppBilling.Forms
{
	public partial class TransactionDebugPage : ContentPage
	{
		public TransactionDebugPage(IInAppBillingService inAppBillingService, PaymentTransaction paymentTransaction)
		{
			BindingContext = new TransactionDebugViewModel(this, inAppBillingService, paymentTransaction);
			InitializeComponent();
		}
	}
}

