using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_paymentapprovalstatusupdate")]
    public class PaymentApprovalStatusUpdate: TradeOrderLogEvent
    {
        public PaymentApprovalStatusUpdate(TradeOrder tradeOrder, CashAccountTransaction payment)
        {
            Event = TradeOrderEventEnum.PaymentApprovalStatusUpdated;
            TradeOrder = tradeOrder;
            Payment = payment;
            PaymentApprovalStatus = payment.Status;
            CreatedDate = DateTime.UtcNow;                    
        }

        public virtual CashAccountTransaction Payment { get; set; }

        public TraderPaymentStatusEnum PaymentApprovalStatus { get; set; }

        /// <summary>
        /// This method creates the description for the event Payment Approved
        /// </summary>
        /// <param name="approvalStatus">approval status description</param>
        public override void SetDescription(string approvalStatus)
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The approval status for the Payment < {Payment.Reference} > has changed to {approvalStatus}.";

        }
    }
}
