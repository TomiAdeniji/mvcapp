using Qbicles.Models.Base;
using Qbicles.Models.Spannered;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_purchase")]
    public class TraderPurchase : DataModelBase
    {
        public TraderPurchase()
        {
            this.PurchaseChannel = SalesChannelEnum.Trader;
        }

        public virtual TraderLocation Location { get; set; }

        public virtual List<TraderTransactionItem> PurchaseItems { get; set; } = new List<TraderTransactionItem>();

        public decimal PurchaseTotal { get; set; }

        public virtual TraderContact Vendor { get; set; }

        public virtual DeliveryMethodEnum DeliveryMethod { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual ApprovalReq PurchaseApprovalProcess { get; set; }

        public virtual WorkGroup Workgroup { get; set; }

        public virtual TraderPurchaseStatusEnum Status { get; set; } = TraderPurchaseStatusEnum.Draft;

        public virtual List<TraderTransfer> Transfer { get; set; } = new List<TraderTransfer>();

        public virtual List<Invoice> Invoices { get; set; } = new List<Invoice>();

        [Column(TypeName = "bit")]
        public bool IsInHouse { get; set; }

        public virtual List<TraderPurchaseOrder> PurchaseOrder { get; set; }

        public virtual List<PurchaseLog> Logs { get; set; } = new List<PurchaseLog>();
        public virtual List<Asset> Assets { get; set; } = new List<Asset>();

        [Required]
        public SalesChannelEnum PurchaseChannel { get; set; }

        public virtual TraderReference Reference { get; set; }
    }


    public enum TraderPurchaseStatusEnum
    {
        [Description("Draft")]
        Draft = 0,
        [Description("Awaiting Review")]
        PendingReview = 1,
        [Description("Awaiting Approval")]
        PendingApproval = 2,
        [Description("Denied")]
        PurchaseDenied = 3,
        [Description("Approved")]
        PurchaseApproved = 4,
        [Description("Discarded")]
        PurchaseDiscarded = 5,
        [Description("Order Issued")]
        PurchaseOrderIssued = 6

    }
}
