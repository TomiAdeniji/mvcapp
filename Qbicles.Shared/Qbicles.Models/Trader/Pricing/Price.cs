using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Pricing
{
    [Table("trad_price")]
    public class Price
    {
        [Required]
        public int Id { get; set; }

        public virtual TraderLocation Location { get; set; }

        public virtual SalesChannelEnum SalesChannel { get; set; }

        public virtual TraderItem Item { get; set; }
     
        [Required]
        public decimal NetPrice { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual DateTime LastUpdateDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        [Required]
        public decimal GrossPrice { get; set; }

        [Required]
        public decimal TotalTaxAmount { get; set; }

        public virtual List<PriceTax> Taxes { get; set; } = new List<PriceTax>();

    }
}
