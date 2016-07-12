using System;

namespace Hauptmedia.InAppBilling
{
	public enum ProductType
	{
		OneTimePurchase,
		Subscription,
		Unknown
	}

	public class Product
	{
		public string ProductIdentifier { get; set; }

		public string Title { get; set; }

		public string Description { get; set; }

		public decimal Price { get; set; }

		public ProductType Type { get; set; }

		public string TypeString
		{
			get
			{
				switch (Type)
				{
					case ProductType.OneTimePurchase:
						return "One-Time Purchase";
						break;

					case ProductType.Subscription:
						return "Subscription";
						break;

					case ProductType.Unknown:
						return "Unknown (not provided by API)";
						break;

					default:
						return "null";
						break;
				}
			}
		}

		public override string ToString()
		{
			return String.Format ("[ProductIdentifier: {0} Title: {1} Description: {2} Price: {3}]", ProductIdentifier, Title, Description, Price);
		}

		public Product ()
		{
		}
	}
}

