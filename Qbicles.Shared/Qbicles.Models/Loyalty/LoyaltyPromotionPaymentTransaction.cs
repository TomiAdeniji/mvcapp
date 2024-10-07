using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Loyalty
{
    /// <summary>
    ///
    /// </summary>
    [Table("loy_PromotionPaymentTransaction")]
    public class LoyaltyPromotionPaymentTransaction : DataModelBase
    {
        [Required]
        public virtual LoyaltyPromotion LoyaltyPromotion { get; set; } // Association with LoyaltyPromotion

        [Required]
        public decimal Amount { get; set; } // Payment amount

        [Required]
        public PaymentStatus Status { get; set; } // Status of the payment

        //[Required]
        public string PaymentMethod { get; set; } // Payment method (e.g., "Credit Card", "PayPal")

        public string TransactionReference { get; set; } = string.Empty; // Unique ID to track each transaction

        /// <summary>
		/// This is the date the transaction was created or initialised.
		/// </summary>
		[Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This is the owner of the transaction .
        /// </summary>
        [Required]
        public ApplicationUser CreatedBy { get; set; }

        /// <summary>
        /// This is the most recent modified date of the transaction.
        /// </summary>
        [Required]
        public DateTime LastModifiedDate { get; set; }

        /// <summary>
        /// This is the last user that modified the transaction.
        /// </summary>
        [Required]
        public ApplicationUser LastModifiedBy { get; set; }
    }

    public enum PaymentStatus
    {
        /// <summary>
        /// The payment has been initiated but is waiting for further actions before it can proceed.
        /// This may include waiting for user confirmation, additional verification, or system approval.
        /// </summary>
        [Display(Name = "Pending", Description = "The payment is awaiting further actions or verification.")]
        Pending = 1,

        /// <summary>
        /// The payment is actively being processed. This stage involves steps like authorization,
        /// verification, and fund transfer. The payment is moving through the necessary workflows.
        /// </summary>
        //[Display(Name = "Processing", Description = "The payment is actively being processed.")]
        //Processing,

        /// <summary>
        /// The payment has been successfully completed. All necessary processing has been done,
        /// and the funds have been transferred from the payer to the payee.
        /// </summary>
        [Display(Name = "Completed", Description = "The payment has been successfully completed.")]
        Completed,

        /// <summary>
        /// The payment was attempted but did not succeed. Reasons could include insufficient funds,
        /// incorrect payment details, or other errors during the processing stage.
        /// </summary>
        [Display(Name = "Failed", Description = "The payment attempt failed due to an error.")]
        Failed,

        /// <summary>
        /// The payment has been fully refunded to the payer. This could occur due to various reasons
        /// such as a product return, service cancellation, or dispute resolution.
        /// </summary>
        [Display(Name = "Refunded", Description = "The payment has been refunded to the payer.")]
        Refunded,

        /// <summary>
        /// The payment was cancelled by the user or the system before it could be completed.
        /// No funds were transferred, and the transaction was not finalized.
        /// </summary>
        [Display(Name = "Cancelled", Description = "The payment was cancelled before completion.")]
        Cancelled,

        /// <summary>
        /// The payment is under dispute. This status indicates that there is a disagreement or issue
        /// related to the payment, which needs to be resolved before a final status can be determined.
        /// </summary>
        [Display(Name = "Disputed", Description = "The payment is under dispute and requires resolution.")]
        Disputed,

        /// <summary>
        /// The payment is on hold and not currently being processed. This may be due to pending verification,
        /// risk assessment, or other reasons requiring further review before proceeding.
        /// </summary>
        [Display(Name = "On Hold", Description = "The payment is on hold pending further review.")]
        OnHold,

        /// <summary>
        /// A portion of the payment has been refunded back to the payer, while the remaining amount
        /// is still considered completed. This could happen in cases where only part of a service or product is returned.
        /// </summary>
        [Display(Name = "Partially Refunded", Description = "Part of the payment has been refunded.")]
        PartiallyRefunded
    }
}