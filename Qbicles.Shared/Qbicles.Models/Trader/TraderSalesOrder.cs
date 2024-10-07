using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace Qbicles.Models.Trader
{
    [Table("trad_salesorder")]
    public class TraderSalesOrder
    {
        public int Id { get; set; }
        public virtual TraderSale Sale { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public string SalesOrderPDF { get; set; }
        public string AdditionalInformation { get; set; }

        public virtual TraderReference Reference { get; set; }
    }
}