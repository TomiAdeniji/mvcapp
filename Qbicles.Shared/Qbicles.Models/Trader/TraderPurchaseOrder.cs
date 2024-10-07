using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_purchaseorder")]
    public class TraderPurchaseOrder
    {
        public int Id { get; set; }
        public virtual TraderPurchase Purchase { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public string PurchaseOrderPDF { get; set; }
        public string AdditionalInformation { get; set; }

        public virtual TraderReference Reference { get; set; }
    }
}
