using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.B2B
{
    [Table("b2b_chargeframework")]
    public class ChargeFramework
    {
        [Required]
        public int Id { get; set; }

        [StringLength(150)]
        [Required]
        public string Name { get; set; }

        [Required]
        public decimal DistanceTravelledFlatFee { get; set; }

        [Required]
        public decimal DistanceTravelPerKm { get; set; }

        [Required]
        public decimal TimeTakenFlatFee { get; set; }

        [Required]
        public decimal TimeTakenPerSecond { get; set; }

        [Required]
        public decimal ValueOfDeliveryFlatFee { get; set; }

        [Required]
        public int ValueOfDeliveryPercentTotal { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        [Required]
        public DateTime LastUpdatedDate { get; set; }

        [Required]
        public virtual PriceList PriceList { get; set; }

        [Required]
        public virtual VehicleType VehicleType { get; set; }
    }
}
