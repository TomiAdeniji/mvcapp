using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader.Movement;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_transferadded")]
    public class TransferAdded : TradeOrderLogEvent
    {
        public TransferAdded(TradeOrder tradeOrder, TraderTransfer transfer)
        {
            Event = TradeOrderEventEnum.TransferAdded;
            TradeOrder = tradeOrder;
            CreatedBy = transfer.CreatedBy;
            CreatedDate = DateTime.UtcNow;
            AddedTransfer = transfer;
        }

        public virtual TraderTransfer AddedTransfer { get; set; }

        /// <summary>
        /// This method creates the description for the event transfer Added
        /// </summary>
        /// <returns></returns>
        public override void SetDescription(string approvalStatus="")
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The transfer < {(AddedTransfer.Reference?.FullRef ?? "")} > has been added.";
        }
    }
}
