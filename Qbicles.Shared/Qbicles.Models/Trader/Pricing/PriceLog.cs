using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Pricing
{
    [Table("trad_pricelog")]
    public class PriceLog
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual Price ParentPrice { get; set; }

        public virtual TraderLocation Location { get; set; }

        public virtual TraderItem Item { get; set; }

        public decimal Value { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public decimal GrossPrice { get; set; }
        public virtual List<PriceTax> Taxes { get; set; } = new List<PriceTax>();
    }
}
