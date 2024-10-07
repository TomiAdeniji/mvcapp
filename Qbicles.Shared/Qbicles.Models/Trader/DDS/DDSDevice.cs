using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.DDS
{
    /// <summary>
    /// This is the class for the Delivery Display Devices
    /// </summary>
    [Table("dds_Device")]
    public class DdsDevice
    {
        [Required]
        public int Id { get; set; }


        /// <summary>
        /// This is the name of the Delivery Display Device
        /// It must be unique within the scope of a Location
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// This is the serial number of the AndroidDevice using the Delivery Display App
        /// that will be associated with this virtual device definition
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// This is the Location at which the device is used
        /// </summary>
        public virtual TraderLocation Location { get; set; }


        /// <summary>
        /// This is the Delivery Queue with which this device is associated
        /// </summary>
        public virtual DeliveryQueue Queue { get; set; }


        /// <summary>
        /// This is a list of the administrators of this device.
        /// These are users drawn from the collection of Domain users.
        /// </summary>
        public virtual List<ApplicationUser> Administrators { get; set; } = new List<ApplicationUser>();


        /// <summary>
        /// This is the user who created the PrepDisplayDevice
        /// </summary>
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time when the Delivery Display Device was created
        /// </summary>
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// This is those users who are allowed to access the Devices
        /// </summary>
        public virtual List<DeviceUser> Users { get; set; } = new List<DeviceUser>();
        public virtual List<PosCashier> DeviceCashiers { get; set; } = new List<PosCashier>();
        public virtual List<PosSupervisor> DeviceSupervisors { get; set; } = new List<PosSupervisor>();
        public virtual List<PosTillManager> DeviceManagers { get; set; } = new List<PosTillManager>();
        /// <summary>
        /// 
        /// </summary>
        public virtual OdsDeviceType Type { get; set; }
    }
}
