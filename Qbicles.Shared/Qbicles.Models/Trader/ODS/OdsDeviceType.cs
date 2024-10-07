using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.ODS
{
    [Table("ods_devicetype")]
    public class OdsDeviceType
    {
        public int Id { get; set; }

        /// <summary>
        /// This is the name of the OdsDeviceType
        /// It must be unique within a Location
        /// </summary>
        [Required]
        public string Name { get; set; }



        public virtual List<DeviceTypeStatus> OrderStatus { get; set; } = new List<DeviceTypeStatus>();

        public virtual List<PosOrderType> AssociatedOrderTypes { get; set; } = new List<PosOrderType>();


        /// <summary>
        /// This is the collection of PosOrderTypes contained by this Device Type
        /// </summary>
        public virtual List<DdsDevice> DdsDevices { get; set; } = new List<DdsDevice>();


        public virtual List<PrepDisplayDevice> PdsDevices { get; set; } = new List<PrepDisplayDevice>();

        /// <summary>
        /// This is the Location with which the type is associated
        /// </summary>
        [Required]
        public virtual TraderLocation Location { get; set; }


        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }

    [Table("ods_devicetypestatus")]
    public class DeviceTypeStatus
    {
        public int Id { get; set; }

        public virtual OdsDeviceType OdsDeviceType { get; set; }

        public PrepQueueStatus Status { get; set; }

    }

}
