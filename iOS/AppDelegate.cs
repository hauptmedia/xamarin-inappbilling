using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using Hauptmedia.InAppBilling;
using Hauptmedia.InAppBilling.Forms;
using Hauptmedia.InAppBilling.iOS;
using UIKit;
using XLabs.Ioc;

namespace InAppBillingTestApp.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{

			var resolverContainer = new SimpleContainer();

			resolverContainer.Register<IInAppBillingService>(t => new InAppBillingService());
			
			Resolver.SetResolver(resolverContainer.GetResolver());

			global::Xamarin.Forms.Forms.Init();

			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}

