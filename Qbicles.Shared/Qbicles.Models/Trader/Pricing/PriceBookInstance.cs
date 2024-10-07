using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Pricing
{
    /// <summary>
    /// The Pricebook is used to assign prices to a list of items grouped by the Product Group (TraderGroup)
    /// </summary>
    [Table("trad_pricebookinstance")]
    public class PriceBookInstance
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime PricesCreatedDate { get; set;}

        [Column(TypeName = "bit")]
        public bool IsDraft { get; set; }

        [Required]
        public virtual PriceBookVersion ParentPriceBookVersion { get; set; }


        /// <summary>
        /// This is the collection of ProductGroups and their associated Margins and Discounts
        /// </summary>
        public virtual List<ProductGroupPriceDefaults> ProductGroupInfo { get; set; }


        /// <summary>
        /// This is the collection of PriceBookUemPrices associated with the Instance
        /// </summary>
        public virtual List<PriceBookPrice> PriceBookPrices { get; set; }

        /// <summary>
        /// This the version number of this instance of the PriceBook
        /// </summary>
        public int InstanceVersion { get; set; }

        /// <summary>
        /// This property is to be used to store a boolean that indicates 
        /// TRUE - The calculation of prices was last carried out with the Average Cost
        /// FALSE - The calculation of proces was last carried out with the Latest Cost
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsPriceCalWithAvgCost { get; set; }
    }
}
