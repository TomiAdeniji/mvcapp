using Qbicles.Models.Trader.Pricing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader;

namespace Qbicles.Models.Catalogs
{
    [Table("pos_extra")]
    public class Extra
    {
        public int Id { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// This is the Trader Item associated with the Variant
        /// </summary>
        [Required]
        public virtual TraderItem TraderItem { get; set; }

        /// <summary>
        /// This is the unit with which this item is to be sold.
        /// </summary>
        public virtual ProductUnit Unit { get; set; }

        /// <summary>
        /// This is a link to the price,
        /// associated with the Trader Item, 
        /// at the location indicated by CategoryItem -> Category -> Menu -> Pos Device.Location
        /// for the Sales Channel PoS
        /// </summary>
        //[Required]
        public virtual Price BaseUnitPrice { get; set; }


        /// <summary>
        /// This value is the Price decided on for the Extra.
        /// It defaults to the BaseUnit.Price.Value initially, but the user may change this value through the configuration UI.
        /// This is the Price that is sent to the PoS when product details are requested.
        /// </summary>
        public virtual CatalogPrice Price { get; set; }


        /// <summary>
        /// This is the category item with which this Extra is associated
        /// </summary>
        public virtual CategoryItem CategoryItem { get; set; }


        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
