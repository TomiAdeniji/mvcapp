using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader.Movement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_transferapprovalstatusupdate")]
    public class TransferApprovalStatusUpdate: TradeOrderLogEvent
    {
        public TransferApprovalStatusUpdate(TradeOrder tradeOrder, TraderTransfer transfer)
        {
            Event = TradeOrderEventEnum.TransferApprovalStatusUpdated;
            CreatedDate = System.DateTime.UtcNow;
            TradeOrder = tradeOrder;
            Transfer = transfer;
            TransferApprovalStatus = transfer.Status;
        }

        public virtual TraderTransfer Transfer { get; set; }

        public TransferStatus TransferApprovalStatus { get; set; }

        /// <summary>
        /// This method creates the description for the event Transfer Approved
        /// </summary>
        /// <param name="approvalStatus">approval status description</param>
        public override void SetDescription(string approvalStatus)
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The approval status for the Transfer < {(Transfer.Reference?.FullRef ?? "")} > has changed to {approvalStatus}.";
        }
    }
}
