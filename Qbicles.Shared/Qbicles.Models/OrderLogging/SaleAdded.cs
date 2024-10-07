using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.OrderLogging
{
    [Table("orderlogevent_saleadded")]
    public class SaleAdded : TradeOrderLogEvent
    {
        public SaleAdded(TradeOrder tradeOrder, TraderSale sale)
        {
            Event = TradeOrderEventEnum.SaleAdded;
            TradeOrder = tradeOrder;
            CreatedBy = sale.CreatedBy;
            CreatedDate = DateTime.UtcNow;
            AddedSale = sale;
        }

        public virtual TraderSale AddedSale { get; set; }

        /// <summary>
        /// This method creates the description for the event Sale Added
        /// </summary>
        /// <returns></returns>
        public override void SetDescription(string approvalStatus = "")
        {
            Description = $"Order < {TradeOrder.OrderReference.FullRef} >: The sale < {AddedSale.Reference.FullRef} > has been added.";
        }
    }
}
