using Qbicles.Models.Trader.Pricing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader;

namespace Qbicles.Models.Catalogs
{
    [Table("pos_variant")]
    public class Variant
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
        [Required]
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
        /// Catalog Price:
        /// This value is the Price decided on for the Variant.
        /// It defaults to the BaseUnit.Price.Value initially, but the user may change this value through the configuration UI.
        /// This is the Price that is sent to the PoS when product details are requested.
        /// </summary>
        public virtual CatalogPrice Price { get; set; }


        /// <summary>
        /// This is the category item with which this Variant is associated
        /// </summary>
        public virtual CategoryItem CategoryItem { get; set; }


        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// If this PosVariant is one of a group of PosVariants associated with a PosCategoryItem,
        /// then this list will have an entry for each of the PosVariantProperties associated with the PosCategory.
        /// E.g.
        /// If the PosCategoryItem has two PosVartaintProperties Size and Colour
        /// If Size has the options - Small, Medium , Large
        /// If Colour has the options - Green, White and Orange
        /// THen this list will have one option from property 1 and one option from proprty 2 i.e.
        /// it could be Small, White 
        /// or
        /// Small, Green or
        /// Medium, Orange
        /// </summary>
        public virtual List<VariantOption> VariantOptions { get; set; } = new List<VariantOption>();


        /// <summary>
        /// This indicated whether the particular Variant is avaliable.
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsActive { get; set; }

        //This is the URI for the image after it has been saved through Qbicles.Doc
        [DataType(DataType.Text)]
        public string ImageUri { get; set; }
        /// <summary>
        /// This indicates whether the particular Variant is the default variant displayed for the PosCategoryItem.
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsDefault { get; set; }
    }
}
