using Qbicles.Models.Base;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_sale")]
    public class TraderSale : DataModelBase
    {
        public virtual List<SaleLog> Logs { get; set; } = new List<SaleLog>();

        public virtual TraderLocation Location { get; set; }

        public virtual List<TraderTransactionItem> SaleItems { get; set; } = new List<TraderTransactionItem>();

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

        public virtual TraderReference Reference { get; set; }


        /// <summary>
        /// This property is to indicate which channel this sale occurs in.
        /// In the Trader web application the SalesChannel = Trader
        /// In the Trader PoS application the SalesChannel = POS
        /// In Community the SalesChannel = Community
        /// </summary>
        [Required]
        public SalesChannelEnum SalesChannel { get; set; }

        [Column(TypeName = "bit")]
        public bool IsInHouse { get; set; }

        public decimal SaleTotal { get; set; }
        public virtual Voucher Voucher { get; set; }
    }


    public enum TraderSaleStatusEnum
    {
        [Description("Draft")]
        Draft = 0,
        [Description("Awaiting review")]
        PendingReview = 1,
        [Description("Awaiting approval")]
        PendingApproval = 2,
        [Description("Denied")]
        SaleDenied = 3,
        [Description("Approved")]
        SaleApproved = 4,
        [Description("Discarded")]
        SaleDiscarded = 5,
        [Description("Ordered Issued")]
        SalesOrderedIssued = 6
    }




    public enum DeliveryMethodEnum
    {
        [Description("Customer Pickup")]
        CustomerPickup = 1,
        [Description("Delivery")]
        Delivery = 2,
        [Description("None")]
        None = 3
    }

}
