using Qbicles.Models.Trader.Pricing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Catalogs
{
    [Table("catalog_price")]
    public class CatalogPrice
    {
        public int Id { get; set; }

        [Required]
        public decimal NetPrice { get; set; }

        [Required]
        public decimal GrossPrice { get; set; }

        [Required]
        public decimal TotalTaxAmount { get; set; }

        public virtual List<PriceTax> Taxes { get; set; } = new List<PriceTax>();
        /// <summary>/// This is a bool to indicate that this price bas been updated
        /// because of a tax change to the underlying item's taxes
        /// </summary>
        [Column(TypeName = "bit")]
        public bool FlaggedForTaxUpdate { get; set; } = false;

        /// <summary>
        /// The date and time at which the price was last updated due to a tax update
        /// </summary>
        public DateTime TaxUpdateDate { get; set; }

        /// <summary>
        /// This is a bool to indicate that this price has been updated
        /// because of a change to the underlying item's latest cost at the location at which the catalog is based
        /// </summary>
        [Column(TypeName = "bit")]
        public bool FlaggedForLatestCostUpdate { get; set; } = false;

        /// <summary>
        /// The date and time at which the price was last updated due to a lates cost update
        /// </summary>
        public DateTime LatestCostUpdateDate { get; set; }
    }
}
