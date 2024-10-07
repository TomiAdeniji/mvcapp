using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_Paymentadded")]
    public class PaymentAdded : TradeOrderLogEvent
    {
        public PaymentAdded(TradeOrder tradeOrder, CashAccountTransaction payment)
        {
            Event = TradeOrderEventEnum.PaymentAdded;
            TradeOrder = tradeOrder;
            CreatedBy = payment.CreatedBy;
            CreatedDate = DateTime.UtcNow;
            AddedPayment = payment;
        }

        public virtual CashAccountTransaction AddedPayment { get; set; }

        /// <summary>
        /// This method creates the description for the event Payment Added
        /// </summary>
        /// <returns></returns>
        public override void SetDescription(string approvalStatus = "")
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The payment < {AddedPayment.Reference} > has been added.";
        }
    }
}
