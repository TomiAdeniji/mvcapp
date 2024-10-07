using System;
using Qbicles.Models.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Attributes;
using Qbicles.Models.B2B;
using Qbicles.Models.Trader.PoS;

namespace Qbicles.Models.Trader.DDS
{
    [Table("dds_DeliveryDriver")]
    public class Driver : DataModelBase
    {


        /// <summary>
        /// This is the Location that Employs the the Driver.
        /// It is a required value and should be initially set to the Location in which the Driver is first created
        /// </summary>

        public virtual TraderLocation EmploymentLocation { get; set; }

        /// <summary>
        /// This collection indicates the TraderLocations that can call on the Driver to do a delivery.
        /// </summary>
        public virtual List<TraderLocation> WorkLocations { get; set; } = new List<TraderLocation>();


        /// <summary>
        /// This property is used to hide a Driver if the driver is deleted.
        /// The Driver cannot be deleted from the database because that would require that all their
        /// associated data such as deliveries would also have to be deleted
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsDeleted { get; set; } = false;


        /// <summary>
        /// POS user
        /// This is the actual Qbicles user associated with the Driver
        /// </summary>
        [Required]
        public virtual DeviceUser User { get; set; }


        /// <summary>
        /// This is the Domain with which Driver is associated.
        /// It is required and there can be only one Domain for a Driver.
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// This is the current status of the Driver
        /// </summary>
        public DriverStatus Status { get; set; }

        /// <summary>
        /// This property is used to indicate whether the Driver is at work or not
        /// </summary>
        [Column(TypeName = "bit")]
        public bool AtWork { get; set; }

        /// <summary>
        /// Set value to serial and name when Driver login from Device
        /// Set is null when driver logout Device
        /// When driver login to a device, check if DeviceSerial is nul then continue login and set value
        /// else if DeviceSerial is not null and Serial provider not same DeviceSerial then 
        /// a message on the device to ask him to confirm to change to his current device
        /// The message should show the Name and serial of the previous device when asking the driver to confirm
        /// </summary>
        public string DeviceSerial { get; set; }
        public string DeviceName { get; set; }


        [DecimalPrecision(10, 7)]
        public decimal Longitude { get; set; }

        [DecimalPrecision(10, 7)]
        public decimal Latitude { get; set; }

        public DateTime LastUpdatedDate { get; set; }


        public virtual Vehicle Vehicle { get; set; }

        public DriverDeliveryStatus DeliveryStatus { get; set; }
    }

    public enum DriverStatus
    {
        [Description("Available")]
        IsAvailable = 1,
        [Description("Busy")]
        IsBusy = 2
    }

    public enum DriverDeliveryStatus
    {
        [Description("Not Set")]
        NotSet = 0,
        [Description("Accepted")]
        Accepted = 1,
        [Description("Rejected")]
        Rejected = 2,
        [Description("Heading To Depot")]
        HeadingToDepot = 3,
        [Description("Ready For Pickup")]
        ReadyForPickup = 4,
        [Description("Started Delivery")]
        StartedDelivery = 5,
        [Description("Completed")]
        Completed = 6,
        [Description("Completed With Problems")]
        CompletedWithProblems = 7
    }
}
