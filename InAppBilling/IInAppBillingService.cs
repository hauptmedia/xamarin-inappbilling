using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Hauptmedia.InAppBilling
{
	public interface IInAppBillingService
	{
		bool CanMakePayments { get; }

		bool CanMakeSubscriptions { get; }

		Task<bool> Initialize();

		Task<PaymentTransaction> PurchaseProduct(string productId);

		Task<IEnumerable<PaymentTransaction>> RequestTransactions();

		Task<IEnumerable<Product>> RequestProducts (IEnumerable<string> productIds);

		Task<bool> ConsumeProduct(string token);
	}
}
