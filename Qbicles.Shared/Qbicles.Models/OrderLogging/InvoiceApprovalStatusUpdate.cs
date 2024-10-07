using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_invoiceapprovalstatusupdate")]
    public class InvoiceApprovalStatusUpdate: TradeOrderLogEvent
    {
        public InvoiceApprovalStatusUpdate(TradeOrder tradeOrder, Invoice invoice)
        {
            Event = TradeOrderEventEnum.InvoiceApprovalStatusUpdated;
            TradeOrder = tradeOrder;
            Invoice = invoice;
            InvoiceApprovalStatus = invoice.Status;
            CreatedDate = DateTime.UtcNow;
        }

        public virtual Invoice Invoice { get; set; }

        public TraderInvoiceStatusEnum InvoiceApprovalStatus { get; set; }

        /// <summary>
        /// This method creates the description for the event Invoice Approved
        /// </summary>
        /// <param name="approvalStatus">approval status description</param>
        public override void SetDescription(string approvalStatus)
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The approval status for the Invoice < {Invoice.Reference.FullRef} > has changed to {approvalStatus}.";

        }
    }
}
