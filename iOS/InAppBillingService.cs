using System;
using System.Threading.Tasks;
using Hauptmedia.InAppBilling;

// http://developer.xamarin.com/guides/ios/application_fundamentals/in-app_purchasing/
using Foundation;
using StoreKit;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Hauptmedia.InAppBilling.iOS
{
	public class InAppBillingService : SKPaymentTransactionObserver, IInAppBillingService
	{
		public InAppBillingService ()
		{
		}

		public bool CanMakePayments { get; private set; }

		public bool CanMakeSubscriptions { get; private set; }

		protected TaskCompletionSource<IEnumerable<PaymentTransaction>> _transactionsTcs;

		protected TaskCompletionSource<PaymentTransaction> _purchaseTcs;

		public Task<bool> ConsumeProduct(string token)
		{
			throw new NotImplementedException();
		}

		public Task<bool> Initialize ()
		{
			SKPaymentQueue.DefaultQueue.AddTransactionObserver(this);

			CanMakePayments = SKPaymentQueue.CanMakePayments;
			CanMakeSubscriptions = SKPaymentQueue.CanMakePayments;

			var taskCompletionSource = new TaskCompletionSource<bool> ();
			taskCompletionSource.SetResult (false);
			return taskCompletionSource.Task;
		}

		public Task<PaymentTransaction> PurchaseProduct (string productId)
		{
			if (_purchaseTcs != null)
				throw new Exception("Only one concurrent purchase allowed");
			
			//SKMutablePayment payment = SKMutablePayment.PaymentWithProduct (appStoreProductId);
			//payment.Quantity = 4;

			SKPayment payment = SKPayment.PaymentWithProduct (productId);
			SKPaymentQueue.DefaultQueue.AddPayment (payment);

			_purchaseTcs = new TaskCompletionSource<PaymentTransaction>();

			return _purchaseTcs.Task;
		}

		public Task<IEnumerable<Product>> RequestProducts (IEnumerable<string> productIds)
		{
			if (productIds == null || productIds.ToArray().Count() == 0)
				throw new ArgumentException("ProductIds should not be null or empty.");
			
			var taskCompletionSource = new TaskCompletionSource<IEnumerable<Product>> ();

			NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(
				productIds.Select (
					productId => new NSString (productId)
				).ToArray ()
			);

			var request  = new SKProductsRequest(productIdentifiers);

			request.ReceivedResponse += (object sender, SKProductsRequestResponseEventArgs e) => {
				
				var products = e.Response.Products.Select(
					skProduct => new Product() {
						Type = ProductType.Unknown,
						ProductIdentifier = skProduct.ProductIdentifier,
						Title = skProduct.LocalizedTitle,
						Description = skProduct.LocalizedDescription,
						Price = decimal.Parse(skProduct.Price.ToString(), CultureInfo.InvariantCulture)
				});

				foreach (string invalidProductId in e.Response.InvalidProducts) {
					Console.WriteLine("Invalid product id: " + invalidProductId );
				}

				taskCompletionSource.SetResult(products);
			};

			request.Start();

			return taskCompletionSource.Task;
		}

		public Task<IEnumerable<PaymentTransaction>> RequestTransactions()
		{
			if (_transactionsTcs != null)
				throw new Exception("Only one concurrent RequestTransactions allowed");
			
			SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();

			_transactionsTcs = new TaskCompletionSource<IEnumerable<PaymentTransaction>>();

			return _transactionsTcs.Task;

		}

		#region implemented abstract members of SKPaymentTransactionObserver

		public override void UpdatedTransactions (SKPaymentQueue queue, SKPaymentTransaction[] skPaymentTransactions)
		{
			var result = new List<PaymentTransaction>();

			foreach (var skPaymentTransaction in skPaymentTransactions)
			{
				var transaction = createPaymentTransactionFromSKPaymentTransaction(skPaymentTransaction);

				if (transaction != null)
					result.Add(transaction);
			}

			// there is a running Purchase 
			if (_purchaseTcs != null)
			{
				_purchaseTcs.SetResult(
					result.FirstOrDefault()
				);
				_purchaseTcs = null;
			}
			else if (_transactionsTcs != null)
			{
				_transactionsTcs.SetResult(result);
				_transactionsTcs = null;
			}

		}
		#endregion


		protected PaymentTransaction createPaymentTransactionFromSKPaymentTransaction(SKPaymentTransaction skPaymentTransaction)
		{
			bool isRestored = false;

			if (skPaymentTransaction.TransactionState == SKPaymentTransactionState.Purchasing || //transaction is being processed by app store
			    skPaymentTransaction.TransactionState == SKPaymentTransactionState.Deferred // Ask Parents to buy, we will be called again with the final state so ignore this state
			   )
			{
				// ignore transactions that have a pending state. UpdatedTransactions will get called again in this case with their final state
				return null;
			}


			if (skPaymentTransaction.TransactionState == SKPaymentTransactionState.Restored)
			{
				//restore original transaction
				skPaymentTransaction = skPaymentTransaction.OriginalTransaction;
				isRestored = true;
			}


			var paymentTransaction = new PaymentTransaction();

			paymentTransaction.ProductIdentifier = skPaymentTransaction.Payment.ProductIdentifier;
			paymentTransaction.TransactionIdentifier = skPaymentTransaction.TransactionIdentifier;


			if (skPaymentTransaction.TransactionState == SKPaymentTransactionState.Purchased)
			{   
				//The item has been purchased, the application can give the user access to the content.
				paymentTransaction.TransactionState = PaymentTransactionState.Purchased;

				if (!isRestored)
				{
					//mark transaction as finished
					SKPaymentQueue.DefaultQueue.FinishTransaction(skPaymentTransaction);
				}

			}
			else if (skPaymentTransaction.TransactionState == SKPaymentTransactionState.Failed)
			{
				//The transaction failed, check the Error property of the SKPaymentTransaction for actual details.
				paymentTransaction.TransactionState = PaymentTransactionState.Failed;

				if(skPaymentTransaction.Error != null)
					paymentTransaction.ErrorDescription = skPaymentTransaction.Error.LocalizedDescription;

			}

			return paymentTransaction;
		}

	}
}

