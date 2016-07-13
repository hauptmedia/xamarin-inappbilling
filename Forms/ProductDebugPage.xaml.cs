using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Hauptmedia.InAppBilling.Forms
{
	public partial class ProductDebugPage : ContentPage
	{
		public ProductDebugPage(IInAppBillingService inAppBillingService, Product product)
		{
			BindingContext = new ProductDebugViewModel(this, inAppBillingService, product);
			InitializeComponent();
		}
	}
}

