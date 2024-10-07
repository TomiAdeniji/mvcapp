using Qbicles.Models.Catalogs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.ODS
{
    /// <summary>
    /// This is the class to manage the extras associated with and order item
    /// </summary>
    [Table("ods_QueueExtra")]
    public class QueueExtra
    {
        public int Id { get; set; }

        /// <summary>
        /// This is the extra definition associiated with the order
        /// </summary>
        [Required]
        public virtual Extra Extra { get; set; }

        /// <summary>
        /// This is the quantity of those extras
        /// </summary>
        [Required]
        public decimal Quantity { get; set; }


        /// <summary>
        /// This is the price for the extra 
        /// </summary>
        [Required]
        public decimal GrossPrice { get; set; }

        /// <summary>
        /// These are the taxes associated with this extra
        /// </summary>
        public virtual List<OrderTax> OrderTaxes { get; set; } = new List<OrderTax>();

        /// <summary>
        /// This is the Order Item with which this extra is associated
        /// </summary>
        [Required]
        public virtual QueueOrderItem ParentOrderItem {get;set;}

        public int? SplitNo { get; set; }
    }
}
