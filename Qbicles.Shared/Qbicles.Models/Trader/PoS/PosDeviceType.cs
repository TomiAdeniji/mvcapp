using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.PoS
{
    [Table("pos_devicetype")]
    public class PosDeviceType
    {
        [Key]
        public int Id { get; set; }


        /// <summary>
        /// This is the name of the PosDeviceType
        /// It must be unique within a Location
        /// </summary>
        [Required]
        public string Name { get; set; }



        /// <summary>
        /// This is the collection of PosOrderTypes contained by this Device Type
        /// </summary>
        public virtual ICollection<PosOrderType> PosOrderTypes { get; set; }

        public virtual ICollection<PosDevice> PosDevices { get; set; } 
        /// <summary>
        /// This is the Location with which the type is associated
        /// </summary>
        [Required]
        public virtual TraderLocation Location { get; set; }


        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        public PosDeviceType()
        {
            PosOrderTypes = new HashSet<PosOrderType>();
            PosDevices = new HashSet<PosDevice>();
        }


    }
}
