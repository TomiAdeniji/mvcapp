using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Qbicles.Models.Catalogs
{
    [Table("pos_categoryitem")]
    public class CategoryItem
    {
       
        public int Id { get; set; }

        [AllowHtml]
        public string Description { get; set; }
        /// <summary>
        /// This is a list of the products that are associated with this CategoryItem.
        /// There must be at least one. 
        /// If there is more than one then each of the items is shown as an option to be purchased on the POS.
        /// 
        /// Examples:
        /// Multiple PosVariants
        ///     So we could have a CategoryItem called COKE
        ///     For the Coke category we could have a Large Coke, Medium Coke and Small Coke - each of these would be a PosVariant.
        /// 
        ///     On the POS the CategoryItem would display as Coke and Large, Medium and Small would appear as options when Coke is picked
        /// 
        /// 
        /// Single PosVariants
        ///     We have a CategoryItem called Chicken Burger Supreme
        ///     We have only 1 PosVariant
        ///     
        ///     On the POS the CategoryItem would display as Chicken Burger Supreme, there would not be any options displayed.
        /// 
        /// </summary>
        public virtual List<Variant> PosVariants { get; set; } = new List<Variant>();


        [Required]
        public string Name { get; set; }

        //[Required]
        public virtual Category Category { get; set; }

        /// <summary>
        /// This is a list of the extra products that can be purchased with this CategoryItem.
        /// This collection can be empty to indicate that there aren't any extras.
        /// 
        ///
        /// Examples:
        ///     We have a CategoryItem called Chicken Burger Supreme
        ///     We have extras 
        ///         Cheese
        ///         Lettuce
        ///     
        ///     On the POS the CategoryItem would display as Chicken Burger Supreme, there would two extras dispayed Extra Cheese and Extra Lettuce.
        /// 
        /// </summary>
        public virtual List<Extra> PosExtras { get; set; } = new List<Extra>();

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        //This is the URI for the image after it has been saved through Qbicles.Doc
        [Required]
        [DataType(DataType.Text)]
        public string ImageUri { get; set; }



        /// <summary>
        /// This is a list of the properties that may be defined for this category item IF there are Pos Variants.
        /// These properties can be something like Size, Colour etc.
        /// </summary>
        public virtual List<VariantProperty> VariantProperties { get; set; } = new List<VariantProperty>();

    }
}
