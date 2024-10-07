using Newtonsoft.Json;
using Qbicles.Models.Attributes;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml;

namespace Qbicles.Models.TraderApi
{

    public partial class OrderDelivery
    {
        public int Id { get; set; }
        /// <summary>
        /// use QueueOrder.TradeOrderRef IF it is availble
        /// IF it is NOT available, use QueueOrder.Reference
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// use QueueOrder.TradeOrderRef IF it is availble
        /// IF it is NOT available, use QueueOrder.Reference
        /// </summary>
        public string TradeOrderRef { get; set; }
        public int Status { get; set; }
        public string Customer { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        /// <summary>
        /// This is the date and time (in UTC) at which the order was added to the queue i.e. when the current record was created
        /// </summary>
        public DateTime? QueuedDate { get; set; }
        /// <summary>
        /// This is the date and time (in UTC) at which the order status is set to Complete
        /// </summary>
        public string CompletedDate { get; set; }
        /// <summary>
        /// This is the date and time (in UTC) at which the order is created on the Pos
        /// The value is supplied from the Order JSON sent from the PoS
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        /// <summary>
        /// This is the date and time (in UTC) at which the order is set as delivered
        /// The value is supplied from the Order JSON sent from the PoS
        /// </summary>
        public DateTime? DeliveredDate { get; set; }
        /// <summary>
        /// This is the date and time (in UTC) at which the order is set paid
        /// The value is supplied from the Order JSON sent from the PoS
        /// </summary>
        public DateTime? PaidDate { get; set; }
        /// <summary>
        /// This is the date and time (in UTC) at which the order status is set to Prepared
        /// </summary>
        public DateTime? PreparedDate { get; set; }
        /// <summary>
        /// This is the date and time (in UTC) at which the order status is set to InPreparation
        /// </summary>
        public DateTime? PrepStartedDate { get; set; }
        /// <summary>
        /// This is the date and time at which this Order was archived.
        /// </summary>
        public DateTime? ArchivedDate { get; set; }
        public bool IsActive { get; set; }
        public int DeliverySequence { get; set; }
    }

    public partial class Order
    {
        /// <summary>
        /// B2COrderCreation as Discussion .TraderOrder.Id
        /// </summary>
        public int TradeOrderId { get; set; } = 0;
        /// <summary>
        /// B2COrderCreation as Discussion.Id
        /// </summary>
        public long? TraderId { get; set; } = 0;
        /// <summary>
        /// This order refers to any previously created orders that were created on which this order is based
        /// Its value is set from LinkedTraderId value supplied from the Order JSON
        /// value is a GUID string
        /// </summary>
        public string LinkedTraderId { get; set; }
        /// <summary>
        /// use QueueOrder.TradeOrderRef IF it is availble
        /// IF it is NOT available, use QueueOrder.OrderRef
        /// </summary>
        public string Reference { get; set; }
        /// <summary>
        /// use QueueOrder.TradeOrderRef IF it is availble
        /// IF it is NOT available, use QueueOrder.OrderRef
        /// </summary>
        public string TradeOrderRef { get; set; }
        public int Classification { get; set; }
        public string Type { get; set; }
        public int Delivery { get; set; } = 0;
        public string Notes { get; set; }
        public decimal AmountInclTax { get; set; } = 0;
        public decimal AmountTax { get; set; } = 0;
        public decimal AmountExclTax { get; set; } = 0;
        public decimal Discount { get; set; } = 0;
        public int? DeliverySequence { get; set; }
        public Status Status { get; set; }
        public Cashier Cashier { get; set; }
        public Customer Customer { get; set; }
        public List<Item> Items { get; set; }
        public virtual List<Item> CancelledItems { get; set; } 
        public List<Payment> Payments { get; set; }
        public string StoreCreditPIN { get; set; }
        public int VoucherId { get; set; } = 0;
        public string VoucherName { get; set; }
        public SalesChannelEnum? SalesChannel { get; set; }
        public string Table { get; set; }
        public int? NumberAtTable { get; set; }
        public bool? IsCancelled { get; set; }
        public int? SplitTimes { get; set; }
        public string SplitType { get; set; }
        public List<OrderSplitAmount> SplitAmounts { get; set; }

    }
    public partial class OrderSplitAmount
    {
        public int SplitNo { get; set; }
        public decimal Amount { get; set; }
    }

    public partial class Cashier
    {
        public string TraderId { get; set; }
        public int? Pin { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public Uri Avatar { get; set; }
    }

    public partial class Customer
    {
        public int TraderId { get; set; }
        public string ContactRef { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Uri Avatar { get; set; }
        public Address Address { get; set; }
        public List<Address> Addresses { get; set; }
        /// <summary>
        /// StoreCreditAccount.CurrentBalance
        /// </summary>
        public decimal StoreCredit { get; set; }
        public bool IsWalkinCustomer { get; set; }
        //public string PIN { get; set; }

    }

    public partial class Address
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        [DecimalPrecision(10, 7)]
        public decimal? Latitude { get; set; }
        [DecimalPrecision(10, 7)]
        public decimal? Longitude { get; set; }
        public int AssociatedAddressId { get; set; }
        public bool IsDefault { get; set; }
    }

    public partial class Item
    {
        public int Id { get; set; }
        public long TraderId { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public bool? Prepared { get; set; }
        public Variant Variant { get; set; }
        public List<Variant> Extras { get; set; } = new List<Variant>();
        public string ImageUri { get; set; } = "";

        public bool IsNotForPrep { get; set; } = false;

        public bool IsCancelled { get; set; } = false;

        public bool IsCancelledByLaterPrep { get; set; } = false;

        public string Note { get; set; } = "";
        public int? SplitNo { get; set; }
        /// <summary>
        /// When an item is created and added to an order in the POS,
        /// the field LinkedItemId must be added to the Order and given a new unique GUID.
        /// </summary>
        public string LinkedItemId { get; set; }
    }

    public partial class Variant
    {
        public long? TraderId { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        /// <summary>
        /// %
        /// </summary>
        public decimal Discount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal AmountInclTax { get; set; }
        public decimal AmountExclTax { get; set; }
        /// <summary>
        /// Varian/Extra quantity
        /// </summary>
        public decimal Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountWithoutDiscount { get; set; }
        public decimal TotalDiscount { get; set; }
        /// <summary>
        /// Is discount by Business or not
        /// </summary>
        public bool IsBusinessDiscount { get; set; } = false;
        public List<Tax> Taxes { get; set; }


        public decimal NetValue { get; set; }
        public string NetValueText { get; set; }

        public decimal GrossValue { get; set; }
        public string GrossValueText { get; set; }

        /// <summary>
        /// Total tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }
        /// <summary>
        /// Total tax amount text
        /// </summary>
        public string TaxAmountText { get; set; }


    }

    public partial class Tax
    {
        /// <summary>
        /// The Id of PriceTax
        /// </summary>
        public long TraderId { get; set; }
        /// <summary>
        /// Tax value of item( variant/extra)
        /// </summary>
        public decimal AmountTax { get; set; }
        public string TaxName { get; set; }
        public decimal TaxRate { get; set; }
    }

    public partial class Payment
    {
        /// <summary>
        /// PaymentMethod_Id
        /// </summary>
        public int? TraderId { get; set; }
        /// <summary>
        /// PosPaymentMethodAccountXref_Id
        /// </summary>
        public int Method { get; set; }
        public string Reference { get; set; }
        public decimal AmountTendered { get; set; }
        public decimal AmountAccepted { get; set; }
    }

    public partial class Status
    {
        [JsonProperty("Status")]
        public int? OrderStatus { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? PaidDateTime { get; set; }
        public DateTime? StartedDateTime { get; set; }
        public DateTime? PreparedDateTime { get; set; }
        public DateTime? DeliveredDateTime { get; set; }
        public DateTime? CompletedDateTime { get; set; }
    }

    public class DeliveryCharge
    {
        public int TraderId { get; set; }
        public decimal Amount { get; set; }
    }

    public class PinVerify
    {
        /// <summary>
        /// contact id
        /// </summary>
        public int Id { get; set; }
        public string PIN { get; set; }
    }

    public class ContactVoucher
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Uri FeaturedImageUri { get; set; }
        public string Description { get; set; }
        public string TermsAndConditions { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartDateString { get; set; }
        public string EndDateString { get; set; }
        /// <summary>
        /// Discount Item = 1, Order = 2
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public virtual List<BaseModel> DaysAllowed { get; set; } = new List<BaseModel>();

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
        //order
        public decimal OrderDiscount { get; set; }
        public decimal MaxDiscountValue { get; set; }
        //item
        public string ItemSKU { get; set; }
        public decimal ItemDiscount { get; set; }
        public int MaxNumberOfItemsPerOrder { get; set; }
    }


    public class B2CProfileCatalogues
    {
        public string DomainKey { get; set; }
        public string Name { get; set; }
        public Uri ImageUri { get; set; }
        public List<B2CCatalogues> Catalogues { get; set; } = new List<B2CCatalogues>();
    }

    public class B2CCatalogues
    {
        public int CatalogId { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class B2COrder
    {
        public string DomainKey { get; set; }
        public string BusinessKey { get; set; }
        public string BusinessName { get; set; }
        public Uri BusinessLogoUri { get; set; }
        public string OrderName { get; set; }
        public string CatalogName { get; set; }
        public int OrderStatus { get; set; }
        public bool IsAgreedByBusiness { get; set; }
        public bool IsAgreedByCustomer { get; set; }
        public Order Order { get; set; } = new Order();
        public Order OrderOrig { get; set; } = new Order();
        public bool BusinessDiscount { get; set; }
    }


    public class B2COrderItem
    {
        public string Name { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public decimal MinPrice { get; set; }

        public List<ItemGalery> ItemsGaleries { get; set; } = new List<ItemGalery>();

        public List<CategoryVariant> Variants { get; set; }
        public List<CategoryExtras> Extras { get; set; }
        public DefaultVariant DefaultVariant { get; set; }
    }

    public class CategoryVariant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<CategoryVariantOption> VariantOptions { get; set; }
    }

    public class CategoryVariantOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CategoryExtras
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string PriceStr { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string ImageUri { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public List<Tax> Taxes { get; set; }
        public Decimal NetValue { get; set; }
        public string NetValueStr { get; set; }
        public Decimal GrossValue { get; set; }
        public string GrossValueStr { get; set; }
        public Decimal TaxAmount { get; set; }
        public string TaxAmountStr { get; set; }
    }

    public class DefaultVariant
    {
        public List<int> ListVariantOptions { get; set; }
        public string ImageUri { get; set; }
    }


    public class ItemGalery
    {
        public string FileUri { get; set; }
        public int Order { get; set; }
        public Uri Small { get; set; }
        public Uri Large { get; set; }
    }

    public class CategoryItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }
        public string ImageUri { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public decimal PriceVal { get; set; }
        public string Price { get; set; }
        public decimal NetValue { get; set; }
        public string NetValueText { get; set; }


        public decimal GrossValue { get; set; }
        public string GrossValueText { get; set; }

        /// <summary>
        /// Total tax amount
        /// </summary>
        public decimal TaxAmount { get; set; }
        /// <summary>
        /// Total tax amount text
        /// </summary>
        public string TaxAmountText { get; set; }

        public List<ItemGalery> ItemsGaleries { get; set; } = new List<ItemGalery>();


        public List<Tax> Taxes { get; set; }
    }

    public class OrderCancelOrPrintCheckModel
    {
        /// <summary>
        /// reference Qbicles.Models.TraderApi.Cashier object data
        /// </summary>
        public Cashier TillUser { get; set; }
        public Cashier TillManager { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// Reference Qbicles.Models.TraderApi.Order
        /// </summary>
        public Order Order { get; set; }
    }

    public class OrderMessageModel
    {
        /// <summary>
        /// Queue Order Id
        /// </summary>
        public int OrderId { get; set; }
        public string Message { get; set; }
    }

    public class SelectedVariantModel
    {
        public decimal? Price { get; set; }
        public string PriceStr { get; set; }
        public string ImageUri { get; set; }

        public string SKU { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public List<Tax> Taxes { get; set; }
        public Decimal NetValue { get; set; }
        public string NetValueStr { get; set; }
        public Decimal GrossValue { get; set; }
        public string GrossValueStr { get; set; }
        public Decimal TaxAmount { get; set; }
        public string TaxAmountStr { get; set; }
        public string VariantName { get; set; }
        public int VariantId { get; set; }
    }
}
