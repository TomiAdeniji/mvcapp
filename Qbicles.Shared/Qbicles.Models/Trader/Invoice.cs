using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_invoices")]
    public class Invoice : DataModelBase
    {

        public virtual TraderSale Sale { get; set; }

        public virtual TraderPurchase Purchase { get; set; }

        public virtual List<InvoiceTransactionItems> InvoiceItems { get; set; } = new List<InvoiceTransactionItems>();

        public DateTime DueDate { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual ApprovalReq InvoiceApprovalProcess { get; set; }

        public virtual WorkGroup Workgroup { get; set; }

        public virtual TraderInvoiceStatusEnum Status { get; set; } = TraderInvoiceStatusEnum.Draft;

        public string InvoicePDF { get; set; }

        public decimal TotalInvoiceAmount { get; set; }

        public virtual List<CashAccountTransaction> Payments { get; set; } = new List<CashAccountTransaction>();

        public string InvoiceAddress { get; set; }

        public string PaymentDetails { get; set; }

        public virtual List<QbicleMedia> AssociatedFiles { get; set; }

        public virtual TraderReference Reference { get; set; }

        public virtual List<InvoiceLog> Logs { get; set; } = new List<InvoiceLog>();
    }


    public enum TraderInvoiceStatusEnum
    {
        [Description("Draft")]
        Draft = 0,
        [Description("Awaiting Review")]
        PendingReview = 1,
        [Description("Awaiting Approval")]
        PendingApproval = 2,
        [Description("Denied")]
        InvoiceDenied = 3,
        [Description("Approved")]
        InvoiceApproved = 4,
        [Description("Discarded")]
        InvoiceDiscarded = 5,
        [Description("Issued")]
        InvoiceIssued = 6
    }
}
