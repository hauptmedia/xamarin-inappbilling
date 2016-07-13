using System.Collections.Generic;
using Hauptmedia.InAppBilling;
using Hauptmedia.InAppBilling.Forms;
using Xamarin.Forms;
using XLabs.Ioc;

namespace InAppBillingTestApp
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			var availableProducts = new List<string>() {
				"your.test.sku1",
				"your.test.sku2",
				"android.test.purchased",
				"android.test.canceled",
				"android.test.refunded",
				"android.test.item_unavailable"
			};

			var inAppBillingService = Resolver.Resolve<IInAppBillingService>();

			MainPage = new NavigationPage(
				new DebugPage(inAppBillingService, availableProducts)
			);
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}

