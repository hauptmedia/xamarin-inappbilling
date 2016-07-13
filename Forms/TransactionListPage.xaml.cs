using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace Hauptmedia.InAppBilling.Forms
{
	public partial class TransactionListPage : ContentPage
	{
		public ObservableCollection<PaymentTransaction> Transactions { get; private set; }

		protected IInAppBillingService _inAppBillingService;

		public TransactionListPage(IInAppBillingService inAppBillingService)
		{
			_inAppBillingService = inAppBillingService;

			Transactions = new ObservableCollection<PaymentTransaction>();

			InitializeComponent();

			transactionListView.ItemsSource = Transactions;

			transactionListView.Refreshing += (sender, e) =>
			{
				_inAppBillingService.RequestTransactions().ContinueWith((t) =>
				{
					Transactions.Clear();
					foreach (var transaction in t.Result)
					{
						Transactions.Add(transaction);
					}

					Device.BeginInvokeOnMainThread(() =>
					{
						transactionListView.IsRefreshing = false;
					});

				});
			};
		}

		protected override void OnAppearing()
		{
			transactionListView.BeginRefresh();
		}

		public void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			// auto de-select the row
			((ListView)sender).SelectedItem = null;
		}

		public void OnTransactionTapped(object sender, ItemTappedEventArgs e)
		{
			Navigation.PushAsync(
				new TransactionDebugPage(
					_inAppBillingService,
					(PaymentTransaction)e.Item)
			);
		}
	}
}

