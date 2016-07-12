using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hauptmedia.InAppBilling.Android.Requests
{
	public abstract class Request<T>
	{
		public TaskCompletionSource<T> TCS;

		public Request()
		{
		}

		public abstract Task<T> Execute(InAppBillingService service);
	}
}