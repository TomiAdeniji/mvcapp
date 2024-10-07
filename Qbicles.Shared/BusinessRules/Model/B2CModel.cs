using Qbicles.Models;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;

namespace Qbicles.BusinessRules.Model
{
    public class B2CProductMenuDiscussionModel
    {
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; } = string.Empty;
        public int MenuId { get; set; }
        public string OpeningComment { get; set; }
        public ApplicationUser Currentuser { get; set; }
        public int QbicleId { get; set; }
    }
    public class B2BCatalogDiscussionModel
    {
        public int CatalogId { get; set; }
        public int SharedByDomainId { get; set; }
        public int SharedToDomainId { get; set; }
        public int QbicleId { get; set; }
        public string CoveringNote { get; set; }
    }
    public class B2CMenuItemsRequestModel : PaginationRequest
    {
        public string DomainKey { get; set; }
        public string CatalogKey { get; set; }
        /// <summary>
        /// Category id
        /// </summary>
        public List<int> CatIds { get; set; }
        public int bdomainId { get; set; }
    }
    public class B2BCatalogItemsRequestModel : PaginationRequest
    {
        public int menuId { get; set; }
        public List<int> CatIds { get; set; }
        public int bdomainId { get; set; }
    }
    public class B2OrderCreationDiscussionModel
    {
        public int OrderReferenceId { get; set; }
        public string OrderReference { get; set; }
        public int MenuId { get; set; }
        public string OpeningComment { get; set; }
        public ApplicationUser Currentuser { get; set; }
        public int QbicleId { get; set; }
        public List<Catalog> ProductMenus { get; set; }
    }
    public class B2COrderItemsRequestModel : PaginationRequest
    {
        public int tradeOrderId { get; set; }
        public List<int> CatIds { get; set; }
        public int bdomainId { get; set; }
        public bool isProduct { get; set; } = true;
        public string bdomainKey { get; set; }
    }

    public class B2COrderItemModel
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public decimal Quantity { get; set; }
        /// <summary>
        /// Net price
        /// </summary>
        public decimal ItemInitialPrice { get; set; }
        public decimal Discount { get; set; }
        public List<Models.TraderApi.Tax> Taxes { get; set; }
        public string TaxInfo { get; set; }
        public decimal Price { get; set; }
        public string SourceName { get; set; }
        public decimal PriceWithoutDiscount { get; set; }
        public string PriceString { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsAllowEdit { get; set; }
        public Models.TraderApi.Variant Variant { get; set; }
        public List<Models.TraderApi.Variant> Extras { get; set; }
        public int VoucherId { get; set; } = 0;
        public string VoucherName { get; set; }
        public string VoucherCode { get; set; }
        public bool CatItemHasMultipleVariants { get; set; } = true;
    }

    public class B2CTradeOrderCustomModel
    {
        public int TradeOrderId { get; set; }
        public List<Models.TraderApi.Item> Items { get; set; }
    }

    public class B2CSearchParameter : B2CSearchQbicleModel
    {
        public string DomainKey { get; set; }
        public string ContactName { get; set; }
    }


    public class B2CSearchQbicleModel
    {
        /// <summary>
        /// 0: Order by latest activity(Default)
        /// 1: Order by forename A-Z
        /// 2: Order by forename Z-A
        /// 3: Order by surname A-Z
        /// 4: Order by surname Z-A
        /// </summary>
        public int Orderby { get; set; }
        /// <summary>
        /// 1 - show all - excluded blocked connections
        /// 2 - show new connections
        /// 3 - show blocked connections
        /// </summary>
        public int ShownType { get; set; }
    }
    public class FindBusinessStoresRequest : PaginationRequest
    {
        /// <summary>
        /// Country id
        /// </summary>
        public string AreaOfOperation { get; set; }
        /// <summary>
        /// BusinessCategories
        /// </summary>
        public string categoryIds { get; set; }
        public string currentUserId { get; set; }
        public bool limitMyConnections { get; set; }
        public bool IsAllPublicStoreShown { get; set; }
    }

    public class FindProductRequest : PaginationRequest
    {
        public string countryName { get; set; }
        public List<int> BrandIds { get; set; }
        public List<string> ProductTags { get; set; }
    }

    public class B2CBusinessStoreInfo : B2CBusinessInfo
    {
        public int LocationId { get; set; }
        public int CatalogId { get; set; }
    }
    public class B2CBusinessInfo
    {
        public int DomainId { get; set; }
        public string DomainKey { get; set; }
        public string LogoUri { get; set; }
        public string BusinessName { get; set; }
        public string BusinessSummary { get; set; }
    }
    public class B2CCustomerAcceptedInfo
    {
        public int disId { get; set; }
        public string disKey { get; set; }
        /// <summary>
        /// 0- Delivery to me - IsDeliveriedToMe = true
        /// 1- Delivery to someone else - IsDeliveriedToMe = false
        /// 2 - CustomerPickup
        /// </summary>
        public int method { get; set; }
        public string someoneName { get; set; }
        public int delivery { get; set; }
        public string note { get; set; }
    }
    public class B2cConfigModel
    {
        public List<string> Services { get; set; }
        public int LocationId { get; set; }
        public PrepQueueStatus SettingOrder { get; set; }
        public SalesChannelEnum SalesChannel { get; set; }
        public PrepQueueStatus OrderStatusWhenAddedToQueue { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public int DefaultSaleWorkGroupId { get; set; }
        public int DefaultInvoiceWorkGroupId { get; set; }
        public int DefaultPaymentWorkGroupId { get; set; }
        public int DefaultTransferWorkGroupId { get; set; }
        public int DefaultPaymentAccountId { get; set; }
        /// <summary>
        /// Use these settings(default workgroup) for all future orders
        /// </summary>
        public bool SaveSettings { get; set; } = false;
    }

    public class BusinessOrderModel
    {
        public int Id { get; set; } = 0;
        public string Ref { get; set; } = "";
        public string Location { get; set; } = "";
        public string DestinationLocation { get; set; } = "";
        public string Channel { get; set; } = "";
        public string Contact { get; set; } = "";
        public int ContactId { get; set; } = 0;
        public string Total { get; set; } = "0";
        public string Status { get; set; } = "";
        public string OrderProblem { get; set; } = "";
        public string Sale { get; set; } = "";
        public int SaleId { get; set; } = 0;
        public string SaleKey { get; set; } = "";
        public string SaleStatus { get; set; } = "";
        public string Invoice { get; set; } = "";
        public int InvoiceId { get; set; } = 0;
        public string InvoiceKey { get; set; } = "";
        public string InvoiceStatus { get; set; } = "";
        public List<PaymentReportModel> Payments { get; set; } = new List<PaymentReportModel>();
        public string Transfer { get; set; } = "";
        public int TransferId { get; set; } = 0;
        public string TransferKey { get; set; } = "";
        public string TransferStatus { get; set; } = "";
        public string CreatedDate { get; set; } = "";
        public string Purchase { get; set; } = "";
        public int PurchaseId { get; set; } = 0;
        public string PurchaseStatus { get; set; } = "";
        public string Bill { get; set; } = "";
        public int BillId { get; set; } = 0;
        public string BillStatus { get; set; } = "";
    }
    public class PaymentReportModel
    {
        public int Id { get; set; } = 0;
        public string Ref { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class MicroB2CCommList
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string ProfileKey { get; set; }
        public string Name { get; set; }
        public Uri Image { get; set; }
        public string ConnectedString { get; set; }
        public DateTime Connected { get; set; }
        public bool IsNew { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsUnseen { get; set; }
        public int Type { get; set; }
    }

    public enum B2CStatus
    {
        New = 0,
        Pending = 1,
        /// <summary>
        /// approve request
        /// </summary>
        Approved = 2,
        /// <summary>
        /// block request
        /// </summary>
        Blocked = 3,
        /// <summary>
        /// cancel request
        /// </summary>
        Cancel = 4
    }
    public class B2OrderParameter
    {
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; } = string.Empty;
        public string ReferenceKey { get; set; }
        public string MenuKey { get; set; }
        public string DomainKey { get; set; }
        public string OpeningComment { get; set; }
        /// <summary>
        /// Qbicle Key
        /// </summary>
        public string Key { get; set; }
    }


    public class B2COderPayment
    {
        public string Id { get; set; }
        public int TradeId { get; set; }
        public string PaymentId { get; set; }
        public string Reference { get; set; }
        public string Information { get; set; }
        public string PaymentMethod { get; set; }
        public string Date { get; set; }
        public decimal Amount { get; set; }
        public string AmountString { get; set; }
        public string Bank { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
        public bool IsCutomer { get; set; }
        //public string Actions { get; set; }

    }

    public class B2CCategoryItemPriceRequestModel
    {
        public List<int> SelectedOptions { get; set; }
        public int CategoryItemId { get; set; }
        public int Quantity { get; set; }
    }
}
