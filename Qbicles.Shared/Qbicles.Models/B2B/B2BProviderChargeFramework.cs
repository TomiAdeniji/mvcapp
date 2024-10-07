using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2B
{
    /// <summary>
    /// This is copied from ChargeFramework
    /// </summary>
    [Table("b2b_providerchargeframework")]
    public class B2BProviderChargeFramework
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
        public virtual B2BProviderPriceList PriceList { get; set; }

        [Required]
        public virtual VehicleType VehicleType { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
