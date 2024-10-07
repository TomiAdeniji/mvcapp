using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Qbicles.Models.Trader.Pricing
{
    [Table("trad_pricebookitemprice")]
    public class PriceBookPrice
    {
        [Required]
        public int Id { get; set; }

        public virtual TraderItem Item { get; set; }

        /// <summary>
        /// This setting is to indicate if the Margin, Discount or FixedPrice have been set
        /// from the AssociatedPriceBook.
        ///     TRUE => The Margin, Discount and/or FixedPrice are set from the PriceBook
        ///     FALSE => The Margin, Discount and/or FixedPrice have been set by the user modifying the values manually.
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsPriceManuallyUpdated { get; set; }

        /// <summary>
        /// This setting is to indicate if the Margin, Discount or FixedPrice have been set
        /// from the AssociatedPriceBook.
        ///     TRUE => The Margin, Discount and/or FixedPrice are set from the PriceBook
        ///     FALSE => The Margin, Discount and/or FixedPrice have been set by the user modifying the values manually.
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsMarkupManuallyUpdated { get; set; }

        /// <summary>
        /// This setting is to indicate if the Margin, Discount or FixedPrice have been set
        /// from the AssociatedPriceBook.
        ///     TRUE => The Margin, Discount and/or FixedPrice are set from the PriceBook
        ///     FALSE => The Margin, Discount and/or FixedPrice have been set by the user modifying the values manually.
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsDiscountManuallyUpdated { get; set; }


        [Column(TypeName = "bit")]
        public bool IsMarkupPercentage { get; set; }

        public decimal MarkUp { get; set; }


        [Column(TypeName = "bit")]
        public bool IsDiscountPercentage { get; set; }
     
        public decimal Discount { get; set; }

        public virtual PriceBookInstance ParentPriceBookInstance  { get; set; }

        public decimal CalculatedPrice { get; set; }


        //public decimal CalculatedPricePlusTax { get; set; }

        //public decimal CalculatedTaxValue { get; set; }

        public decimal TaxValue { get; set; }

        public decimal Price { get; set; }

        public decimal FullPrice { get; set; }


        [Required]
        public virtual DateTime LastUpdateDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// The cost is calculated as an average of the original cost of the items currently available in Inventory.
        /// This is the Average cost calculated from the cost of the inventory batches associated with this item 
        /// remembering that the batches are at the Location associated with the parent PriceBook
        /// </summary>
        public decimal AverageCost { get; set; }

        /// <summary>
        /// This cost is simply the latest CostPerUnit for the Item currently available in Inventory.
        /// This is the Latest cost calculated from the cost of the inventory batches associated with this item 
        /// remembering that the batches are at the Location associated with the parent PriceBook
        /// </summary>
        public decimal LatestCost { get; set; }


    }
}

