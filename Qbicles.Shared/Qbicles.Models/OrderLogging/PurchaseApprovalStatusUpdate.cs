using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_purchaseapprovalstatusupdate")]
    public class PurchaseApprovalStatusUpdate: TradeOrderLogEvent
    {
        public PurchaseApprovalStatusUpdate(TradeOrder tradeOrder, TraderPurchase purchase)
        {
            Event = TradeOrderEventEnum.PurchaseApprovalStatusUpdated;
            CreatedDate = System.DateTime.UtcNow;
            TradeOrder = tradeOrder;
            Purchase = purchase;
            PurchaseApprovalStatus = purchase.Status;
        }

        public virtual TraderPurchase Purchase { get; set; }

        public TraderPurchaseStatusEnum PurchaseApprovalStatus { get; set; }

        /// <summary>
        /// This method creates the description for the event Purchase Approved
        /// </summary>
        /// <param name="approvalStatus">approval status description</param>
        public override void SetDescription(string approvalStatus)
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The approval status for the Purchase < {Purchase.Reference.FullRef} > has changed to {approvalStatus}.";

        }
    }
}
