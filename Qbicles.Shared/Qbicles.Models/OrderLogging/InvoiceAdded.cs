using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_invoiceadded")]
    public class InvoiceAdded : TradeOrderLogEvent
    {
        public InvoiceAdded(TradeOrder tradeOrder, Invoice invoice)
        {
            Event = TradeOrderEventEnum.InvoiceAdded;
            TradeOrder = tradeOrder;
            CreatedBy = invoice.CreatedBy;
            CreatedDate = DateTime.UtcNow;
            AddedInvoice = invoice;
        }

        public virtual Invoice AddedInvoice { get; set; }

        /// <summary>
        /// This method creates the description for the event Invoice Added
        /// </summary>
        /// <returns></returns>
        public override void SetDescription(string approvalStatus = "")
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The invoice < {AddedInvoice.Reference.FullRef} > has been added.";
        }
    }
}
