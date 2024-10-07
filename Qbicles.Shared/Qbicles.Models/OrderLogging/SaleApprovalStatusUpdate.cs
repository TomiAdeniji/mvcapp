using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_saleapprovalstatusupdate")]
    public class SaleApprovalStatusUpdate : TradeOrderLogEvent
    {
        public SaleApprovalStatusUpdate(TradeOrder tradeOrder, TraderSale sale)
        {
            Event = TradeOrderEventEnum.SaleApprovalStatusUpdated;
            CreatedDate = System.DateTime.UtcNow;
            TradeOrder = tradeOrder; 
            Sale = sale;
            SaleApprovalStatus = sale.Status;
        }

        public virtual TraderSale Sale { get; set; }

        public TraderSaleStatusEnum SaleApprovalStatus { get; set; }

        /// <summary>
        /// This method creates the description for the event Sale Approved
        /// </summary>
        /// <param name="approvalStatus">approval status description</param>
        public override void SetDescription(string approvalStatus)
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The approval status for the sale < {Sale.Reference.FullRef} > has changed to {approvalStatus}.";

        }
    }
}
