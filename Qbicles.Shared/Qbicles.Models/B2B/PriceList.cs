using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.B2B
{
    [Table("b2b_pricelist")]
    public class PriceList
    {
        [Required]
        public int Id { get; set; }

        [StringLength(150)]
        [Required]
        public string Name { get; set; }

        [StringLength(500)]
        [Required]
        public string Summary { get; set; }

        public string Icon { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        [Required]
        public DateTime LastUpdatedDate { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        public virtual List<ChargeFramework> ChargeFrameworks { get; set; } = new List<ChargeFramework>();
    }
}
