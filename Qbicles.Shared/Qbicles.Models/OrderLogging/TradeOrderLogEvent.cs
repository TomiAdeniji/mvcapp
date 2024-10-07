using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Qbicles.Models.OrderLogging
{
    public class TradeOrderLogEvent: DataModelBase
    {
        /// <summary>
        /// The current user when the event is created
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        /// <summary>
        /// The date and time on which the event was created - UTCNOW
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The TradeOrder to which this log refers
        /// </summary>
        [Required]
        public virtual TradeOrder TradeOrder { get; set; }


        /// <summary>
        /// The description of the event that is being logged
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// The type of the event that is being logged
        /// </summary>
        [Required]
        public TradeOrderEventEnum Event { get; set; }


        public virtual void SetDescription(string approvalStatus = "")
        {

        }
    }


    public enum TradeOrderEventEnum
    {
        [Description("Sale Added")]
        SaleAdded = 1, 
        [Description("Sale Approval Status Updated")]
        SaleApprovalStatusUpdated = 2,

        [Description("Invoice Added")]
        InvoiceAdded = 3,
        [Description("Invoice Approval Status Updated")]
        InvoiceApprovalStatusUpdated = 4,

        [Description("Payment Added")]
        PaymentAdded = 5,
        [Description("Payment Approval Status Updated")]
        PaymentApprovalStatusUpdated = 6,

        [Description("Transfer Added")]
        TransferAdded = 7,
        [Description("Transfer Approval Status Updated")]
        TransferApprovalStatusUpdated = 8,

        [Description("Purchase Added")]
        PurchaseAdded = 9,
        [Description("Purchase Approval Status Updated")]
        PurchaseApprovalStatusUpdated = 10,

        [Description("Preparation Status Updated")]
        PreparationStatusUpdated = 11,

        [Description("Delivery Status Updated")]
        DeliveryStatusUpdated = 12
    }
}
