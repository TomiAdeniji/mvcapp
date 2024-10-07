using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader.DDS;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_deliverystatusupdate")]
    public class DeliveryStatusUpdate : TradeOrderLogEvent
    {
        public DeliveryStatusUpdate(TradeOrder tradeOrder, DeliveryStatus deliveryStatus)
        {
            Event = TradeOrderEventEnum.DeliveryStatusUpdated;
            CreatedDate = System.DateTime.UtcNow;
            TradeOrder = tradeOrder;
            DeliveryStatus = deliveryStatus;
        }

        /// <summary>
        /// The new status of the Delivery
        /// </summary>
        public DeliveryStatus DeliveryStatus { get; set; }

        /// <summary>
        /// This method creates the description for the event Delivery Added
        /// </summary>
        /// <returns></returns>
        public override void SetDescription(string approvalStatus = "")
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The status of the delivery including this order has changed to {DeliveryStatus}.";
        }
    }
}
