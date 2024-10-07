using Qbicles.Models.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Catalogs;

namespace Qbicles.Models.Trader.ODS
{
    /// <summary>
    /// This is the class for the individal line items on the order
    /// </summary>
    [Table("ods_QueueOrderItem")]
    public class QueueOrderItem
    {
        public int Id { get; set; }


        /// <summary>
        /// This boolean indicates if this line item has been sent to the queue previously, as part of a previous QueueOrder.
        /// If it is true then this OrderItem will NOT be sent as a response to a 
        /// If the value is true, then it will not be returned when the PrepDisplayDevice reuests current orders
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsInPrep { get; set; }

        /// <summary>
        /// This indicates, if TRUE, that the item is not to be displayed on the PDS
        /// The customer has orderd it but it is not yet to be prepared
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsNotForPrep { get; set; } = false;


        /// <summary>
        /// This indicates that the item has been Cancelled
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsCancelled { get; set; } = false;

        /// <summary>
        /// Update old items on the queue and PDS that have now been deleted
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsCancelledByLaterPrep { get; set; } = false;
        /// <summary>
        /// This is the PoSVariant item on which the Order Item is based
        /// </summary>
        [Required]
        public virtual Variant Variant { get; set; }

        /// <summary>
        /// This is the quantity of those items
        /// </summary>
        [Required]
        public decimal Quantity { get; set; }


        /// <summary>
        /// This is the price for the OrderItem 
        /// </summary>
        [Required]
        public decimal GrossPrice { get; set; }


        /// <summary>
        /// THis is the collection of extras associated with this order item
        /// </summary>
        public virtual List<QueueExtra> Extras { get; set; } = new List<QueueExtra>();

        /// <summary>
        /// These are the taxes associated with this item
        /// </summary>
        public virtual List<OrderTax> OrderTaxes { get; set; } = new List<OrderTax>();

        /// <summary>
        /// This is the order instance with which this order item is associated
        /// </summary>
        [Required]
        public virtual QueueOrder ParentOrder { get; set; }
        /// <summary>
        /// This is the discount applied to the item
        /// It is a percentage i.e. a value of 10 means 10%
        /// It is a record of the discount i.e. the discount has already 
        /// been applied to the GrossPrice, 
        /// this field is simply recording what discount has been applied
        /// </summary>
        [DecimalPrecision(10, 7)]
        public decimal Discount { get; set; }
        public int? SplitNo { get; set; }
        public string Note { get; set; }
        /// <summary>
        /// When an item is created and added to an order in the POS,
        /// the field LinkedItemId must be added to the Order and given a new unique GUID.
        /// </summary>
        public string LinkedItemId { get; set; }
    }
}
