using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_purchaselog")]
    public class PurchaseLog
    {
        public int Id { get; set; }

        public virtual TraderPurchase AssociatedPurchase { get; set; }

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
    }




}
