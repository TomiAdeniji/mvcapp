using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Catalogs;
using System.ComponentModel;
using Qbicles.Models.Base;

namespace Qbicles.Models.B2C_C2C
{
    [Table("trade_Order")]
    public class TradeOrder : DataModelBase
    {
        [Required]
        public virtual TraderReference OrderReference { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }
        /// <summary>
        /// We need to know the Sales channel tha generated the Order
        /// </summary>
        [Required]
        public SalesChannelEnum SalesChannel { get; set; }

        /// <summary>
        /// The domain that is doing the selling for the order.
        /// 
        /// </summary>
        public virtual QbicleDomain SellingDomain { get; set; }

        /// <summary>
        /// This is the Domainthat is doing the buying, IF it is a Domain that is doing the buying
        /// </summary>
        public virtual QbicleDomain BuyingDomain { get; set; }


        public TradeOrderStatusEnum OrderStatus { get; set; }


        /// <summary>
        /// This is a string for storing and display the JSON of the order based on Qbicles.Models.TraderApi.Order
        /// </summary>
        public string OrderJson { get; set; }
        /// <summary>
        /// Using only on B2C Order process
        /// This is a string for storing the JSON of the Origin order based on Qbicles.Models.TraderApi.Order
        /// Use for create Order to db
        /// remove voucher then set OrderJson = OrderJsonOrig
        /// add/edit/update item order - set values into OrderJsonOrig and set OrderJson = OrderVoucherCalculation2Pos(OrderJsonOrig)
        /// Approval order set OrderJson = OrderVoucherCalculation2Web(OrderJsonOrig)
        /// </summary>
        public string OrderJsonOrig { get; set; }

        /// <summary>
        /// This is the customer that is purchasing the order from the selling Domain
        /// </summary>
        public virtual ApplicationUser Customer { get; set; }

        /// <summary>
        /// OrderCustomer from the POS saleOrder customer, use for CreateQueueOrder
        /// </summary>
        public virtual OrderCustomer OrderCustomer { get; set; }
        /// <summary>
        /// This is Contact that the order
        /// </summary>
        public virtual TraderContact TraderContact { get; set; }

        public DeliveryMethodEnum DeliveryMethod { get; set; }

        public virtual Catalog ProductMenu { get; set; }
        /// <summary>
        /// from the POS saleOrder
        /// </summary>
        public virtual PosDevice PosDevice { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// The Order when it has passed from the B2C QBicle and become a fully fledged Order leading to a Sale, Invoice, Payment and Transfer
        /// </summary>
        public virtual QueueOrder PrepDeliveryOrder { get; set; }

        public virtual TraderSale Sale { get; set; }
        public virtual WorkGroup SaleWorkGroup { get; set; }


        public virtual Invoice Invoice { get; set; }

        public virtual WorkGroup InvoiceWorkGroup { get; set; }

        public virtual List<CashAccountTransaction> Payments { get; set; }
        public virtual TraderCashAccount PaymentAccount { get; set; }
        public virtual WorkGroup PaymentWorkGroup { get; set; }

        public virtual TraderTransfer Transfer { get; set; }
        public virtual WorkGroup TransferWorkGroup { get; set; }


        /// <summary>
        /// Property to manage the order as it is being created in the B2C Qbicle and have it converted to the OrderJson for storing in the database
        /// i.e. it is a shopping cart order
        /// </summary>
        [NotMapped]
        public Order ProvisionalOrder { get; set; }


        [Column(TypeName = "bit")]
        public bool IsAgreedByBusiness { get; set; } = false;

        [Column(TypeName = "bit")]
        public bool IsAgreedByCustomer { get; set; } = false;

        public string ProcessedProblems { get; set; }
        public TradeOrderProblemEnum OrderProblem { get; set; }
        public bool IsDeliveriedToMe { get; set; } = false;


        /// <summary>
        /// This property is used to link the TradeOrder to any QueueOrders that may be related to the TraderOrder.
        /// It is especially important for QueueOrders that are sent to prep from POS that exist BEFORE a TradeOrder is created.
        /// </summary>
        public virtual string LinkedOrderId { get; set; }


        //Order Status Info
        public int NumberOfDocuments { get; set; } = 0;

        public int NumberOfItems { get; set; } = 0;

        [Column(TypeName = "bit")]
        public bool IsPaymentComplete { get; set; } = false;

        [Column(TypeName = "bit")]
        public bool IsInTransit { get; set; } = false;
        /// <summary>
        /// The estimated delivery time (i.e. UTCNOW + time to the ActiveOrder location) is set as the ActiveOrder.TradeOrder.ETA
        /// The calculation for the Time/Distance is to be done by the Google Directions API
        /// UTCNOW + time to the ActiveOrder location
        /// </summary>
        public DateTime? ETA { get; set; }

    }

    public enum TradeOrderStatusEnum
    {
        [Description("Draft")]
        Draft = 0,
        [Description("Awaiting processing")]
        AwaitingProcessing = 1,
        [Description("Processing")]
        InProcessing = 2,
        [Description("Completed")]
        Processed = 3,
        [Description("Completed with problems")]
        ProcessedWithProblems = 4

    }
    public enum TradeOrderProblemEnum
    {
        [Description("Non")]
        Non = 0,
        [Description("Create Sale")]
        CreateSale = 1,
        [Description("Create Invoice")]
        CreateInvoice = 2,
        [Description("Create Payment")]
        CreatePayment = 3,
        [Description("Create Transfer")]
        CreateTransfer = 4,
        [Description("Create Pos Queue Order")]
        CreatePosQueueOrder = 5,
        [Description("Create Purchase")]
        CreatePurchase = 6,
        [Description("Create Bill")]
        CreateBill = 7,
        [Description("Create Purchase Payment")]
        CreatePurchasePayment = 8,
        [Description("Create Purchase Transfer")]
        CreatePurchaseTransfer = 9
    }
}
