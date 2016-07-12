using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Hauptmedia.InAppBilling.Android
{
	public class Consts
	{
		// Billing response codes
		public const int BILLING_RESPONSE_RESULT_OK = 0;
		public const int BILLING_RESPONSE_RESULT_USER_CANCELED = 1;
		public const int BILLING_RESPONSE_RESULT_BILLING_UNAVAILABLE = 3;
		public const int BILLING_RESPONSE_RESULT_ITEM_UNAVAILABLE = 4;
		public const int BILLING_RESPONSE_RESULT_DEVELOPER_ERROR = 5;
		public const int BILLING_RESPONSE_RESULT_ERROR = 6;
		public const int BILLING_RESPONSE_RESULT_ITEM_ALREADY_OWNED = 7;
		public const int BILLING_RESPONSE_RESULT_ITEM_NOT_OWNED = 8;

		// Manufactured error codes
		public const int ERROR_BASE = -1000;
		public const int REMOTE_EXCEPTION = -1001;
		public const int BAD_RESPONSE = -1002;
		public const int VERIFICATION_FAILED = -1003;
		public const int SEND_INTENT_FAILED = -1004;
		public const int USER_CANCELLED = -1005;
		public const int UNKNOWN_PURCHASE_RESPONSE = -1006;
		public const int UNKNOWN_ERROR = -1008;
		public const int SUBSCRIPTIONS_NOT_AVAILABLE = -1009;
		public const int INVALID_CONSUMPTION = -1010;

		// Keys for the responses from InAppBillingService
		public const string RESPONSE_BUY_INTENT = "BUY_INTENT";
		public const string RESPONSE_INAPP_PURCHASE_DATA = "INAPP_PURCHASE_DATA";
		public const string RESPONSE_INAPP_SIGNATURE = "INAPP_DATA_SIGNATURE";

	}
}