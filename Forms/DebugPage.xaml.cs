using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Hauptmedia.InAppBilling.Forms
{
	public partial class DebugPage : ContentPage
	{
		protected IInAppBillingService _inAppBillingService;
		IEnumerable<string> _productIds;

		public DebugPage(IInAppBillingService inAppBillingService, IEnumerable<string> productIds)
		{
			_inAppBillingService = inAppBillingService;
			_productIds = productIds;

			BindingContext = inAppBillingService;
			InitializeComponent();
		}

		public void OnViewProductsClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(
				new ProductsListPage(
					_inAppBillingService,
					_productIds
				)
			);
		}

		public void OnViewTransactionsClicked(object sender, EventArgs e)
		{
			Navigation.PushAsync(
				new TransactionListPage(_inAppBillingService)
			);
		}
	}
}

