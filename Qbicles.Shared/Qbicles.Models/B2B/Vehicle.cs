using Qbicles.Models.Trader.DDS;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.B2B
{
    [Table("b2b_Vehicle")]
    public class Vehicle
    {
        [Required]
        public int Id { get; set; }

        [StringLength(150)]
        [Required]
        public string Name { get; set; }

        [StringLength(50)]
        [Required]
        public string RefOrRegistration { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        
        public DateTime LastUpdatedDate { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public VehicleType Type { get; set; }

    }


    public enum VehicleType
    {
        [Description("Bike")]
        Bicycle = 1,
        [Description("Motorbike")]
        Motorbike = 2,
        [Description("On foot")]
        OnFoot = 3,
        [Description("Car")]
        Car = 4,
        [Description("Lorry")]
        Lorry = 5
    }
}
