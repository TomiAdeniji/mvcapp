using Qbicles.Models.Base;
using Qbicles.Models.Trader.CashMgt;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.ODS
{
    /// <summary>
    /// This class represents an order as it is sent from the PoS to a Queue
    /// There can be multiple QueueOrders with the same OrderRef in the same Queue
    /// Example:
    /// The initial QueueOrder would the starters in a meal (with each OrderItem.IsInPrep = false)
    /// The next QueueOrder would have the starters (with each OrderItem.IsInPrep = true) and 
    /// - the mains (with each OrderItem.IsInPrep = false)
    /// The next QueueOrder would have the starters (with each OrderItem.IsInPrep = true) and 
    /// - the mains (with each OrderItem.IsInPrep = true) and
    /// - the desserts (with each OrderItem.IsInPrep = false)
    /// </summary>
    [Table("ods_QueueOrder")]
    public class QueueOrder : DataModelBase
    {

        /// <summary>
        /// This is the reference generated on the POS for the Order
        /// </summary>
        public string OrderRef { get; set; }


        /// <summary>
        /// This is to indicate whether the order is for EatIn, DriveThru etc
        /// </summary>
        public OrderTypeClassification Classification { get; set; }

        [StringLength(40, ErrorMessage = "Queue Order Type should be maximum of 40 characters")]
        public string Type { get; set; }

        /// <summary>
        /// This is to indicate whether the item is being prepared etc
        /// </summary>
        public PrepQueueStatus Status { get; set; } = PrepQueueStatus.NotStarted;


        /// <summary>
        /// This is the date and time (in UTC) at which the order was added to the queue i.e. when the current record was created
        /// </summary>
        public DateTime? QueuedDate { get; set; } = DateTime.UtcNow;



        /// <summary>
        /// This is the date and time (in UTC) at which the order status is set to Complete
        /// </summary>
        public DateTime? CompletedDate { get; set; }
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

        public int? DeliverySequence { get; set; }

        /// <summary>
        /// This is a collection of the payments that are supplied in the Order JSOB set to the api
        /// </summary>
        public virtual List<OrderPayment> Payments { get; set; } = new List<OrderPayment>();

        /// <summary>
        /// This order refers to any previously created orders that were created on which this order is based
        /// Its value is set from LinkedTraderId value supplied from the Order JSON
        /// value is a GUID string
        /// </summary>
        public virtual string LinkedOrderId { get; set; }


        /// <summary>
        /// This is a list of the items that are in the order
        /// </summary>
        public virtual List<QueueOrderItem> OrderItems { get; set; } = new List<QueueOrderItem>();


        /// <summary>
        /// This is a list of the items that are in the order
        /// </summary>
        public virtual List<QueueOrderItem> CancelledOrderItems { get; set; } = new List<QueueOrderItem>();


        /// <summary>
        /// This is the queue with which this order is associated
        /// </summary>
        public virtual PrepQueue PrepQueue { get; set; }


        /// <summary>
        /// When the Order is archived, this is the archive queue with which it is associated when the PrepQueue is set to null
        /// </summary>
        public virtual PrepQueueArchive PrepQueueArchive { get; set; }

        /// <summary>
        /// This is the date and time at which this Order was archived.
        /// </summary>
        public DateTime? ArchivedDate { get; set; }


        /// <summary>
        /// This is the user on the PoS who created the order
        /// </summary>
        public virtual ApplicationUser Cashier { get; set; }

        /// <summary>
        /// This is the a note that can added to an Order in the PoS to register any instructions from the customer
        /// e.g. 'No pickels' might be added as an instruction for a Burger order.
        /// </summary>
        [StringLength(512)]
        public string Notes { get; set; }


        /// <summary>
        /// If the Order is to be delivered using the delivery service then it will have to be attached
        /// to a particular Delivery
        /// </summary>
        public virtual Delivery Delivery { get; set; }

        /// <summary>
        /// This is the total value for the Order
        /// </summary>
        [Required]
        public decimal OrderTotal { get; set; } = 0;

        public decimal AmountExclTax { get; set; }

        public decimal AmountTax { get; set; }

        /// <summary>
        /// This is the customer associated with the Order.
        /// Please note that there may not be a customer associated with the order
        /// </summary>
        public virtual OrderCustomer Customer { get; set; }

        [Column(TypeName = "bit")]
        public bool IsPaid { get; set; }


        /// <summary>
        /// This is to indicate that the ORDER has been cancelled
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsCancelled { get; set; } = false;

        /// <summary>
        /// This is the Sale with which this order is associated.
        /// This value can only be set in the BusinessRules.PoS.PosSaleOrderRules.Order
        /// When a Sale is created for an order that is sent from the PoS (i.e. an order for which payment is taken)
        /// the OrderItem is saved and Sale is associated with the Order
        /// </summary>

        public virtual TraderSale Sale { get; set; }


        public virtual PosDeviceOrderXref PosDeviceOrder { set; get; }

        /// <summary>
        /// A discussion where this Order can be discussed
        /// </summary>
        public virtual QbicleDiscussion Discussion { get; set; }

        public string OrderDeliveryProblemNote { get; set; }
        public string OrderProblemNote { get; set; }

        public SalesChannelEnum SalesChannel { get; set; }

        public virtual List<SplitAmount> SplitAmounts { get; set; }
        public string Table { get; set; }
        public int? NumberAtTable { get; set; }
        //Split-Bill information
        public int? SplitTimes { get; set; }
        public string SplitType { get; set; }
    }

    public enum OrderTypeClassification
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Eat In")]
        EatIn = 1,
        [Description("Drive Thru")]
        DriveThru = 2,
        [Description("Take Away")]
        TakeAway = 3,
        [Description("Home Delivery")]
        HomeDelivery = 4,
        [Description("Retail")]
        Retail = 5
    }

    public enum PrepQueueStatus
    {
        [Description("Not Started")]
        NotStarted = 1,
        Preparing = 2,
        Completing = 3,
        Completed = 4,
        [Description("Completed With Problems")]
        CompletedWithProblems = 5
    }

    public enum GetOrderTimeSpan
    {
        Hours_24 = 1,
        Hours_48 = 2,
        Hours_72 = 3
    }
}
