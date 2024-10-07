using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_purchaseadded")]
    public class PurchaseAdded : TradeOrderLogEvent
    {
        public PurchaseAdded(TradeOrder tradeOrder, TraderPurchase purchase)
        {
            Event = TradeOrderEventEnum.PurchaseAdded;
            TradeOrder = tradeOrder;
            CreatedBy = purchase.CreatedBy;
            CreatedDate = DateTime.UtcNow;
            AddedPurchase = purchase;
        }

        public virtual TraderPurchase AddedPurchase { get; set; }

        /// <summary>
        /// This method creates the description for the event Purchase Added
        /// </summary>
        /// <returns></returns>
        public override void SetDescription(string approvalStatus="")
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The purchase < {AddedPurchase.Reference.FullRef} > has been added.";
        }
    }
}
