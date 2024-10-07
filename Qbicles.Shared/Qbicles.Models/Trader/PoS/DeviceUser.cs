using Qbicles.Models.Trader.DDS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader.ODS;

namespace Qbicles.Models.Trader.PoS
{
    /// <summary>
    /// POS User
    /// </summary>
    [Table("pos_user")]
    public class DeviceUser
    {
        public int Id { get; set; }

        [Required]
        public virtual ApplicationUser User { get; set; }


        /// <summary>
        /// This pin MUST be unique within the Domain
        /// </summary>
        //[Required]
        [Range(1000, 9999)]
        public int? Pin { get; set; }


        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        public virtual List<PosDevice> Devices { get; set; } = new List<PosDevice>();
        /// <summary>
        /// This indicates the list of DdsDevices with which this user is associated.
        /// It is part of a many to many relationship
        /// </summary>
        public virtual List<DdsDevice> DdsDevices { get; set; } = new List<DdsDevice>();


        public virtual List<PrepDisplayDevice> PrepDisplayDevices { get; set; } = new List<PrepDisplayDevice>();
    }
}
