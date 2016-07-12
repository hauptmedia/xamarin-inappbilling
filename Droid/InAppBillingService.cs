using System;
using System.Threading.Tasks;

// https://developer.android.com/google/play/billing/billing_reference.html
// https://developer.android.com/google/play/billing/billing_testing.html
// https://developer.android.com/google/play/billing/billing_integrate.html

using Android.Content;
using Android.App;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.OS;
using System.Json;
using Com.Android.Vending.Billing;
using Hauptmedia.InAppBilling.Android.Requests;

namespace Hauptmedia.InAppBilling.Android
{
	public class InAppBillingService : Service, IServiceConnection, IInAppBillingService
	{
		public const int GPS_API_VERSION = 3;
		public const string GPS_ITEM_TYPE_INAPP = "inapp";
		public const string GPS_ITEM_TYPE_SUBS = "subs";

		public Com.Android.Vending.Billing.IInAppBillingService InAppService { get; private set; }

		protected Requests.Purchase _currentPurchaseRequest { get; private set; }

		public Activity Activity  { get; private set; }

		public Context Context { get; private set; }

		protected string _publicKey;

		protected InAppBillingService _service;

		public bool Connected { get; private set; }

		public InAppBillingService (Activity activity, string publicKey)
		{
			Activity = activity;
			Context = activity.ApplicationContext;
			_publicKey = publicKey;
		}

		public bool CanMakePayments { get; private set; }

		public bool CanMakeSubscriptions { get; private set; }

		private TaskCompletionSource<bool> _connectTcs;

		protected Task<bool> _connect()
		{
			if (Connected)
				return Task.Factory.StartNew<bool> (() => true);
			
			_connectTcs = new TaskCompletionSource<bool>();

			try {
				var serviceIntent = new Intent("com.android.vending.billing.InAppBillingService.BIND");
				var services = Context.PackageManager.QueryIntentServices(serviceIntent, 0);

				if (services.Any())
				{
					if (!BindService(serviceIntent, this, Bind.AutoCreate))
					{
						Console.WriteLine("Could not bind to the InAppBillingService");
						_connectTcs.SetResult (false);
					}
				}
				else
				{
					Console.WriteLine("InAppBillingService is not available on this device");
					_connectTcs.SetResult (false);
				}

			} catch (System.Exception e) {
				Console.WriteLine("InAppBillingService is not available on this device.");
				_connectTcs.SetResult (false);
			}


			return _connectTcs.Task;
		}


		public Task<bool> Initialize ()
		{
			AttachBaseContext(Activity);

			return _connect ();
		}

		public async Task<IEnumerable<PaymentTransaction>> RequestTransactions()
		{
			var subscriptionTransactions = await new Requests.RequestTransactions(GPS_ITEM_TYPE_SUBS, _publicKey).Execute(this);
			var inAppTransactions		 = await new Requests.RequestTransactions(GPS_ITEM_TYPE_INAPP, _publicKey).Execute(this);

			return subscriptionTransactions.Union(inAppTransactions).ToList();
		}


		public Task<bool> ConsumeProduct(string token)
		{
			return new Requests.ConsumePurchase(token).Execute(this);
		}

		public Task<PaymentTransaction> PurchaseProduct(string productId)
		{
			if (_currentPurchaseRequest != null)
			{
				throw new Exception("Only one conccurent purchase request allowed!");
			}

			_currentPurchaseRequest = new Requests.Purchase(productId, _publicKey);
			return _currentPurchaseRequest.Execute(this);
		}

		public void PurchaseCompleted()
		{
			//gets called by the current purchase request
			_currentPurchaseRequest = null;
		}

		public async Task<IEnumerable<Product>> RequestProducts (IEnumerable<string> productIds)
		{
			var subscriptionProducts	= await new RequestProducts(productIds, GPS_ITEM_TYPE_SUBS).Execute(this);
			var inAppProducts			= await new RequestProducts(productIds, GPS_ITEM_TYPE_INAPP).Execute(this);

			return subscriptionProducts.Union(inAppProducts).ToList();
		}

		public bool HandleActivityResult(int requestCode, int resultCode, Intent data)
		{
			//passthrough to to the current purchase request if present
			if (_currentPurchaseRequest == null)
				return false;
			else
				return _currentPurchaseRequest.HandleActivityResult(requestCode, resultCode, data);
		}

		public void OnServiceConnected (ComponentName name, IBinder service)
		{
			try
			{
				Connected = true;

				InAppService = IInAppBillingServiceStub.AsInterface(service);
				var packageName = Context.PackageName;

				CanMakePayments = CanMakeSubscriptions = false;

				//check for capabilities
				if (InAppService.IsBillingSupported(GPS_API_VERSION, packageName, GPS_ITEM_TYPE_INAPP) == Consts.BILLING_RESPONSE_RESULT_OK)
				{
					CanMakePayments = true;
				}

				if (InAppService.IsBillingSupported(GPS_API_VERSION, packageName, GPS_ITEM_TYPE_SUBS) == Consts.BILLING_RESPONSE_RESULT_OK)
				{
					CanMakeSubscriptions = true;
				}
			}
			catch (RemoteException e)
			{
				Console.WriteLine("Remote exception occurred in OnServiceConnected; " + e.ToString());
				Connected = false;
			}
			finally
			{
				//Just indicates that the connect request has completed
				// check PurchasesSupported to know whether or not v3 is supported, if successful
				if (_connectTcs != null)
					_connectTcs.SetResult(Connected);
			}
		}

		public void OnServiceDisconnected (ComponentName name)
		{
			Connected = false;

			//The following will get re-initialized in OnServiceConnected
			if (InAppService != null)
			{
				InAppService.Dispose();
				InAppService = null;
			}
		}

		/// <summary>
		/// Dispose of all un-managed resources and unbind the service connection
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			try
			{
				base.Dispose(disposing);

				UnbindService(this);

				if (InAppService != null)
				{
					InAppService.Dispose();
					InAppService = null;
				}
			}
			catch { }
			Connected = false;
		}

		#region implemented abstract members of Service

		/// <summary>
		/// We don't support binding to this service, only starting the service.
		/// </summary>
		/// <param name="intent"></param>
		/// <returns></returns>
		public override IBinder OnBind(Intent intent)
		{
			return null;
		}


		#endregion
	}
}

