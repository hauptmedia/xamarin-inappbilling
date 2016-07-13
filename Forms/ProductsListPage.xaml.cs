using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Hauptmedia.InAppBilling.Forms
{
	public partial class ProductsListPage : ContentPage
	{
		public ObservableCollection<Product> AvailableProducts { get; private set; }

		protected IInAppBillingService _inAppBillingService;

		public ProductsListPage(IInAppBillingService inAppBillingService, IEnumerable<string> productIds)
		{
			_inAppBillingService = inAppBillingService;

			AvailableProducts = new ObservableCollection<Product>();

			InitializeComponent();

			productListView.ItemsSource = AvailableProducts;

			productListView.Refreshing += (sender, e) =>
			{
				_inAppBillingService.RequestProducts(productIds).ContinueWith((t) =>
				{
					AvailableProducts.Clear();
					foreach (var product in t.Result)
					{
						AvailableProducts.Add(product);
					}

					Device.BeginInvokeOnMainThread(() =>
					{
						productListView.IsRefreshing = false;
					});
				});
			};
		}

		protected override void OnAppearing()
		{
			productListView.BeginRefresh();
		}


		public void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			// auto de-select the row
			((ListView)sender).SelectedItem = null;
		}

		public void OnProductTapped(object sender, ItemTappedEventArgs e)
		{
			Navigation.PushAsync(
				new ProductDebugPage(
					_inAppBillingService,
					(Product)e.Item)
			);
		}
	}
}

