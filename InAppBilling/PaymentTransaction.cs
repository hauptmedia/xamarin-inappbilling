using System;
namespace Hauptmedia.InAppBilling
{
	public enum PaymentTransactionState
	{
		Purchased,
		Canceled,
		Failed,
		Refunded
	}

	/*
	 * Android:
	 * purchaseState	The purchase state of the order. Possible values are 0 (purchased), 1 (canceled), or 2 (refunded).
	 */
	public class PaymentTransaction
	{
		public String TransactionIdentifier { get; set; }

		public String ProductIdentifier { get; set; }

		public Boolean HasError
		{
			get
			{
				return (TransactionState != PaymentTransactionState.Purchased);
			}
		}

		public String ErrorDescription { get; set; }

		public PaymentTransactionState TransactionState { get; set; }

		// A token that uniquely identifies a purchase for a given item and user pair.
		public String Token { get; set; }

		public string TransactionStateString
		{
			get
			{
				switch (TransactionState)
				{
					case PaymentTransactionState.Purchased:
						return "Purchased";
					case PaymentTransactionState.Canceled:
						return "Canceled";						
					case PaymentTransactionState.Failed:
						return "Failed";
					case PaymentTransactionState.Refunded:
						return "Refunded";
					default:
						return "Unknown";
				}
			}
		}

		public PaymentTransaction()
		{
		}
	}
}

