using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.DDS
{
    [Table("dds_DeliveryDriverGroup")]
    public class DriverGroup
    {
        [Required]
        public int Id { get; set; }


        [Required]
        public string Name { get; set; }


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


        public virtual List<Driver> Drivers { get; set; }
    }
}
