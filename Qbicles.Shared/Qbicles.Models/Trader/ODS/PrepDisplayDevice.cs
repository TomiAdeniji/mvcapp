using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.ODS
{
    [Table("ods_PrepDisplayDevice")]
    public class PrepDisplayDevice
    {
        [Required]
        public int Id { get; set; }


        /// <summary>
        /// This is the name of the Preparation Display Device
        /// It must be unique within the scope of a Location
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// This is the serial number of the AndroidDevice using the Prep Display App
        /// that will be associated with this virtual device definition
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// This is the Location at which the device is used
        /// </summary>
        public virtual TraderLocation Location { get; set; }


        /// <summary>
        /// This is the Preparation Queue with which this device is associated
        /// </summary>
        public virtual PrepQueue Queue { get; set; }


        /// <summary>
        /// This is a list of the administrators of this device.
        /// These are users drawn from the collection of Domain users.
        /// </summary>
        public virtual List<ApplicationUser> Administrators { get; set; } = new List<ApplicationUser>();



        /// <summary>
        /// This is the user who created the PrepDisplayDevice
        /// </summary>
        // QBIC-1437:  public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time when the PrepDisplayDevice was created
        /// </summary>
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// A list of the users that can use this device
        /// There must be at least one
        /// </summary>
        public virtual List<DeviceUser> Users { get; set; } = new List<DeviceUser>();


        /// <summary>
        /// 
        /// </summary>
        public virtual OdsDeviceType Type { get; set; }


        /// <summary>
        /// This is the collection of CategoryExclusionSets that will be used to determine
        /// which items are displayed on a PDS device.
        /// In a QueueOrderItem, tf the category NAME, with which a Variant is associated, is in any of the associated CategoryExclusionSets then
        /// the OrderItem is NOT supplied to the PDS for display on the PDS
        /// </summary>
        public virtual List<CategoryExclusionSet> CategoryExclusionSets { get; set; }
    }

}
