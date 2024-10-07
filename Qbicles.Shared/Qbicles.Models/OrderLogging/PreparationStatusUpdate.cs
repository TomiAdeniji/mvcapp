using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader.ODS;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_preparationstatusupdate")]
    public class PreparationStatusUpdate : TradeOrderLogEvent
    {
        public PreparationStatusUpdate(TradeOrder tradeOrder, QueueOrder queueOrder)
        {
            Event = TradeOrderEventEnum.PreparationStatusUpdated;
            QueueOrder = queueOrder;
            CreatedDate = System.DateTime.UtcNow;
            TradeOrder = tradeOrder;
            PreparationStatus = queueOrder.Status;
        }

        /// <summary>
        /// The new status of the QueueOrder
        /// </summary>
        public PrepQueueStatus PreparationStatus { get; set; }


        /// <summary>
        /// The QueueOrder that has had its status updated
        /// </summary>
        public virtual QueueOrder QueueOrder { get; set; }


        /// <summary>
        /// This method creates the description for the event Preparation Status Updated
        /// </summary>
        /// <param name="approvalStatus">approval status description</param>
        /// <returns></returns>
        public override void SetDescription(string approvalStatus)
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The preparation status for the queue order < {QueueOrder.OrderRef} > has changed to {approvalStatus}.";
        }
    }
}
