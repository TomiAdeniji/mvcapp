using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{


	/// <summary>
	/// This class is the definition of the Promotion that will enable Qbicles users to get vouchers
	/// </summary>
	[Table("loy_Promotion")]
	public class LoyaltyPromotion : DataModelBase
	{
		[Required]
		public DateTime CreatedDate { get; set; }

		[Required]
		public ApplicationUser CreatedBy { get; set; }


		/// <summary>
		/// The Domain/Business with which the Promotion is associated
		/// </summary>
		[Required]
		public virtual QbicleDomain Domain { get; set; }

		/// <summary>
		/// The BulkDeal Promotion with which the Promotion is associated,if opted in
		/// </summary>
		public virtual LoyaltyBulkDealPromotion BulkDealPromotion { get; set; }

		/// <summary>
		/// This is the name of the promotion
		/// </summary>
		[Required]
		public string Name { get; set; } // Length??

		/// <summary>
		/// This is the plan type of the promotion
		/// </summary>
		[Required]
		public LoyaltyPromotionType PlanType { get; set; }

        /// <summary>
        /// This is the location visibility audience of the promotion
        /// </summary>
        [Required]
        public LoyaltyPromotionAudience Audience { get; set; }

        /// <summary>
        /// THis is the GUID to associate with the image file uploaded to S3
        /// </summary>
        [Required]
		public string FeaturedImageUri { get; set; }


		/// <summary>
		/// The description of the promotion
		/// </summary>
		[Required]
		public string Description { get; set; } // Length??


		/// <summary>
		/// This is the date and time at which the Promotion will be displayed in the 
		/// Community MoniBac list.
		/// This shold be earlier or equal to the StartDate
		/// This should default to the StartDate unless explicity set
		/// </summary>
		[Required]
		public DateTime DisplayDate { get; set; }

		/// <summary>
		/// The date and time at which it becomes possible to claim the vouchers
		/// </summary>
		[Required]
		public DateTime StartDate { get; set; }


		/// <summary>
		/// <summary>
		/// The date and time at which it stops being possible to claim the vouchers
		/// </summary>
		[Required]
		public DateTime EndDate { get; set; }

		/// <summary>
		/// This is the information directly related to the voucher creation associated with this promotion
		/// </summary>
		public virtual VoucherInfo VoucherInfo { get; set; }


		/// <summary>
		/// This is a boolean that can be set by a user to halt or stop a promotion
		/// When/If this value is set to true No vouchers associated with the Promotion can be claimed OR redeemed 
		/// </summary>
		[Column(TypeName = "bit")]
		public bool IsHalted { get; set; } = false;

		/// <summary>
		/// A description of why the Promotion was halted
		/// </summary>
		public string HaltReason { get; set; }

		/// <summary>
		/// The vouchers that have been claimed as part of this promotion
		/// </summary>
		public virtual List<Voucher> Vouchers { get; set; } = new List<Voucher>();


		/// <summary>
		/// This indicates that the promotion has been archived
		/// this occurs when the end date of the promotion is reached
		/// </summary>
		[Column(TypeName = "bit")]
		public bool IsArchived { get; set; } = false;

		/// <summary>
		/// This indicates that the promotion can be draft
		/// this occurs when the end date of the promotion is reached
		/// </summary>
		[Column(TypeName = "bit")]
		public bool IsDraft { get; set; } = false;

		/// <summary>
		/// This is the date on which the Promotion was archived
		/// </summary>
		public DateTime ArchivedDate { get; set; }


		/// <summary>
		/// This is a list of the users in Qbicles who have bookmarked this promotion
		/// </summary>
		public virtual List<ApplicationUser> LikingUsers { get; set; } = new List<ApplicationUser>();

		/// <summary>
		/// A list of the users in Qbicles who liked the promotion
		/// </summary>
		public virtual List<ApplicationUser> LikedBy { get; set; } = new List<ApplicationUser>();

        /// <summary>
        /// This is the list of payment transactions associated with this promotion
        /// </summary>
        public virtual List<LoyaltyPromotionPaymentTransaction> PaymentTransactions { get; set; } = new List<LoyaltyPromotionPaymentTransaction>();
    }
}
