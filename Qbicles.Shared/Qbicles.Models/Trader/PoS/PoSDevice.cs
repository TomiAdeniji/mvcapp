using Qbicles.Models.Trader.CashMgt;
using Qbicles.Models.Trader.ODS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Catalogs;
using System.ComponentModel;

namespace Qbicles.Models.Trader.PoS
{

    [Table("pos_device")]
    public class PosDevice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        /// <summary>
        /// This is the name of the device. 
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// This is a description of the device. 
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// This is the serial number of the device. 
        /// It is used in the Device registration process to genarate an access token for the device.
        /// It must be entered in Trader BEFORE the device can be registered to work with Trader
        /// </summary>
        public string SerialNumber { get; set; }


        /// <summary>
        /// This is the security token generated when the device is registered with Trader
        /// </summary>
        public string DeviceToken { get; set; }


        /// <summary>
        /// A Device can have only one associated menu
        /// </summary>
        public virtual Catalog Menu { get; set; }

        /// <summary>
        /// A list of the users that can use this device
        /// There must be at least one
        /// </summary>
        public virtual List<DeviceUser> Users { get; set; } = new List<DeviceUser>();

        /// <summary>
        /// A list of users who can do, as yet unspecified administration tasks, from the device.
        /// The users in this group are chose from the Users list, i.e. to be a DeviceManager you must first be a User.
        /// NOTE: These users will NOT be allowed to register the device
        /// 
        /// </summary>
        public virtual List<PosCashier> DeviceCashiers { get; set; } = new List<PosCashier>();

        /// <summary>
        /// A list of users who can do, as yet unspecified administration tasks, from the device.
        /// The users in this group are chose from the Users list, i.e. to be a DeviceManager you must first be a User.
        /// NOTE: These users will NOT be allowed to register the device
        /// 
        /// </summary>
        public virtual List<PosSupervisor> DeviceSupervisors { get; set; } = new List<PosSupervisor>();


        /// <summary>
        /// A list of the user who can register the device in trader.
        /// There must be at least one
        /// </summary>
        public virtual List<PosAdministrator> Administrators { get; set; } = new List<PosAdministrator>();



        /// <summary>
        /// A list of users who can do, as yet unspecified administration tasks, from the device.
        /// The users in this group are chose from the Users list, i.e. to be a DeviceManager you must first be a User.
        /// NOTE: These users will NOT be allowed to register the device
        /// 
        /// </summary>
        public virtual List<PosTillManager> DeviceManagers { get; set; } = new List<PosTillManager>();


        /// <summary>
        /// This indicates the current status of the device
        /// </summary>
        public PosDeviceStatus Status { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        public virtual List<PosPaymentMethodAccountXref> MethodAccount { get; set; } = new List<PosPaymentMethodAccountXref>();

        [Column(TypeName = "bit")]
        public bool Archived { get; set; }


        /// <summary>
        /// This is the prepearation queue to which this PosDevice will send Orders
        /// when the 'Send to prep' (Send to Kitchen) button is clicked
        /// </summary>
        public virtual PrepQueue PreparationQueue { get; set; }


        /// <summary>
        /// This is the prefix that well be attached to the order reference generated 
        /// on the PoS when the PoS is disconnected from the internet.
        /// </summary>
        [Required]
        [StringLength(3)]
        public string TabletPrefix { get; set; }


        /// <summary>
        /// This is the PosDeviceType that contains the PosOrderTypes that are alloed for the device
        /// </summary>
        public virtual PosDeviceType PosDeviceType {get;set;}

        /// <summary>
        /// This is the Till that PosDevice associated with
        /// </summary>
        public virtual Till Till { get; set; }

    }

    public enum PosDeviceStatus
    {
        [Description("Inactive")]
        InActive = 1, 
        [Description("Active")]
        Active = 2,

    }
}
