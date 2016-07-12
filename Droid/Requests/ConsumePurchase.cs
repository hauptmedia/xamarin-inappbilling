using System;
using System.Threading.Tasks;
using Android.OS;

namespace Hauptmedia.InAppBilling.Android.Requests
{
	public class ConsumePurchase : Request<bool>
	{
		protected string _token;

		public ConsumePurchase(string token) : base()
		{
			_token = token;
		}

		public override Task<bool> Execute(InAppBillingService service)
		{
			TCS = new TaskCompletionSource<bool>();

			try
			{
				if (string.IsNullOrEmpty(_token))
				{
					TCS.SetResult(false);
					return TCS.Task;
				}
					
				int response = service.InAppService.ConsumePurchase(
					InAppBillingService.GPS_API_VERSION, 
					service.Context.PackageName, 
					_token
				);
				
				if (response == Consts.BILLING_RESPONSE_RESULT_OK)					
				{
					Utils.LogDebug(string.Format("Successfully consumed token: {0}", _token));
					TCS.SetResult(true);
				}
				else
				{
					Utils.LogError(string.Format("Error consuming consuming token: {0}, message: {1}", _token, Utils.GetResponseDesc(response)));
					TCS.SetResult(false);
				}
			}		
			catch (RemoteException e)
			{
				Utils.LogError(string.Format("RemoteException while consuming token: {0}, message: {1}", _token, e.Message));
				TCS.SetResult(false);
			}
			catch (System.Exception e)
			{
				Utils.LogError(string.Format("Exception while consuming token: {0}, message: {1}", _token, e.Message));
				TCS.SetResult(false);
			}

			return TCS.Task;
		}
	}
}