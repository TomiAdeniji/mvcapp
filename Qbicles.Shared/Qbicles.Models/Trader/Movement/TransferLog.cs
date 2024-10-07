using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Movement
{
    [Table("trad_transferlog")]
    public class TransferLog
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual TraderTransfer AssociatedTransfer { get; set; }

        [Required]
        public virtual ApplicationUser UpdatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }


        /// <summary>
        /// If the TransferLog was created as a result of processing the Transfer through its pseudo 'Approval process' then,
        /// hhis is a link to ProcesssLog that would have been created at that time.
        /// </summary>
        public virtual TransferProcessLog AssociatedProcessLog { get; set; }




        public virtual WorkGroup Workgroup { get; set; }

        public virtual TraderSale Sale { get; set; }

        public virtual TraderPurchase Purchase { get; set; }

        public virtual List<TraderTransferItem> TransferItems { get; set; } = new List<TraderTransferItem>();

        public virtual TraderContact Contact { get; set; }

        public virtual TraderAddress Address { get; set; }

        public virtual TraderLocation OriginatingLocation { get; set; }

        public virtual TraderLocation DestinationLocation { get; set; }

        public virtual TransferStatus Status { get; set; } = TransferStatus.Initiated;

        public virtual ApprovalReq TransferApprovalProcess { get; set; }

        public virtual Shipment AssociatedShipment { get; set; }

    }
}
