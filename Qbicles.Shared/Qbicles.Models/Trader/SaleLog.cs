using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_salelog")]
    public class SaleLog
    {

        public int Id { get; set; }

        public virtual TraderSale AssociatedSale { get; set; }

        public virtual TraderLocation Location { get; set; }

        public virtual List<TraderTransactionItem> SaleItems { get; set; } = new List<TraderTransactionItem>();

        public decimal SaleTotal { get; set; }

        public virtual TraderContact Purchaser { get; set; }

        public virtual TraderAddress DeliveryAddress { get; set; }

        public virtual DeliveryMethodEnum DeliveryMethod { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual ApprovalReq SaleApprovalProcess { get; set; }


        public virtual WorkGroup Workgroup { get; set; }

        public virtual TraderSaleStatusEnum Status { get; set; } = TraderSaleStatusEnum.Draft;

        public virtual List<TraderTransfer> Transfer { get; set; } = new List<TraderTransfer>();

        public virtual List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual List<TraderSalesOrder> SalesOrders { get; set; }


        [Required]
        public SalesChannelEnum SalesChannel { get; set; }

        [Column(TypeName = "bit")]
        public bool IsInHouse { get; set; }
    }


}
