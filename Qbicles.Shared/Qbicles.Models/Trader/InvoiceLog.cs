using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_InvoiceLog")]
    public class InvoiceLog
    {
        public int Id { get; set; }
        /// <summary>
        /// This is the Invoice with which this InvoiceLog is associated 
        /// </summary>
        [Required]
        public virtual Invoice ParentInvoice { get; set; }

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
    }
}
