using Qbicles.Models.Base;
using Qbicles.Models.Manufacturing;
using Qbicles.Models.Spannered;
using Qbicles.Models.Trader.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Movement

{
    public enum TransferReasonEnum
    {
        [Description("")]
        Null = 0,
        [Description("Sale")]
        Sale = 1,
        [Description("Purchase")]
        Purchase = 2,
        [Description("Point To Point")]
        PointToPoint = 3,
        [Description("Waste Report Adjustment")]
        WasteReportAdjustment = 4,
        [Description("Manufacturing Job Adjustment")]
        ManufacturingJobAdjustment = 5,
        [Description("Spot Count Adjustment")]
        SpotCountAdjustment = 6,
        [Description("Inventory Creation")]
        InventoryCreation =7
    }


    [Table("trad_transfer")]
    public class TraderTransfer : DataModelBase
    {
        public virtual WorkGroup Workgroup { get; set; }

        public virtual TraderSale Sale { get; set; }

        public virtual TraderPurchase Purchase { get; set; }


        /// <summary>
        /// The folowing properties have been added to link the Transfer to other processes that could cause a 'hidden' transfer
        /// The Transfer is carried out to update the Inventory, the user has no part in the Transfer, except that they have cause
        /// the transfer by approving one of the processes.
        /// </summary>

        public virtual WasteReport WasteReport { get; set; }

        public virtual ManuJob ManufacturingJob { get; set; }

        public virtual SpotCount SpotCount { get; set; }

        public TransferReasonEnum Reason { get; set; }



        public virtual List<TraderTransferItem> TransferItems { get; set; } = new List<TraderTransferItem>();

        public virtual TraderContact Contact { get; set; }

        public virtual TraderAddress Address { get; set; }

        public virtual TraderLocation OriginatingLocation { get; set; }

        public virtual TraderLocation DestinationLocation { get; set; }
        [Required]
        public virtual DateTime CreatedDate { get; set; }
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        public virtual TransferStatus Status { get; set; } = TransferStatus.Initiated;

        public virtual ApprovalReq TransferApprovalProcess { get; set; }

        public virtual Shipment AssociatedShipment { get; set; }

        /// <summary>
        /// This is a collection of the Log records that are created each time the TRansfer is updated.
        /// </summary>
        public virtual List<TransferLog> Logs { get; set; }

        /// <summary>
        /// This is a collection of the logs created when processing the Transfer through its pseudo 'Approval process' i.e., Picked up, delivered etc
        /// </summary>
        public virtual List<TransferProcessLog> ProcessLogs { get; set; }

        public virtual TraderReference Reference { get; set; }
        public virtual List<Asset> Assets { get; set; } = new List<Asset>();
    }


    public enum TransferStatus
    {
        [Description("Initiated")]
        Initiated = 0,
        [Description("Pending Pickup")]
        PendingPickup = 1,
        [Description("Picked Up")]
        PickedUp = 2,
        [Description("Delivered")]
        Delivered = 3,
        [Description("Draft")]
        Draft = 4,
        [Description("Denied")]
        Denied = 5,
        [Description("Discarded")]
        Discarded = 6
    }


}