using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Pricing
{
    [Table("trad_pricedefaults")]
    public class ProductGroupPriceDefaults
    {
        [Required]
        public int Id { get; set; }

        [Column(TypeName = "bit")]
        public bool IsMarkupPercentage { get; set; }

        public decimal MarkUp { get; set; }


        [Column(TypeName = "bit")]
        public bool IsDiscountPercentage { get; set; }

        public decimal Discount { get; set; }

        public virtual TraderGroup ProductGroup { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual PriceBookInstance ParentInstance { get; set; }
    }
}
