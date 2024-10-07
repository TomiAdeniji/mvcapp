using Qbicles.Models.Base;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
	/// <summary>
	/// This class is the definition of the Promotion Type that will enable 
	/// the Qbicles System administrator setup the moniback promotion
	/// </summary>
	[Table("loy_PromotionType")]
	public class LoyaltyPromotionType : DataModelBase
	{
		/// <summary>
		/// This is the name of the promotion type
		/// </summary>
		[Required]
		public string Name { get; set; } // Length??

		/// <summary>
		/// The description of the promotion type
		/// </summary>
		[Required]
		public string Description { get; set; } // Length??

		/// <summary>
		/// The description of the promotion type
		/// </summary>
		[Required]
		public string Icon { get; set; } // Length??

		/// <summary>
		/// This is the plan type of the promotion type
		/// </summary>
		[Required]
		public int Type { get; set; }


		/// <summary>
		/// This is the duration of the promotion type
		/// </summary>
		[Required]
		public int Duration { get; set; } //in days

		/// <summary>
		/// This is the duration of the promotion type
		/// </summary>
		[Required]
		public decimal Price { get; set; }

		/// <summary>
		/// This is the status tracker of the promotion type
		/// </summary>
		[Required]
		public bool IsActive { get; set; } = true;

		/// <summary>
		/// This is the ranking of the promotion type
		/// </summary>
		[Required]
		public int Rank { get; set; }

        /// <summary>
        /// This is for managing deletion of the promotion type
        /// to the recycle bin
        /// </summary>
        [Required]
		public bool IsDeleted { get; set; }

		/// <summary>
		/// This is the date the promotion type was created
		/// </summary>
		[Required]
		public DateTime CreatedDate { get; set; }

		/// <summary>
		/// This is the owner of the promotion type
		/// </summary>
		[Required]
		public ApplicationUser CreatedBy { get; set; }

		/// <summary>
		/// This is the recent modified date of the promotion type
		/// </summary>
		[Required]
		public DateTime LastModifiedDate { get; set; }

		/// <summary>
		/// This is the last user that modified the promotion type
		/// </summary>
		[Required]
		public ApplicationUser LastModifiedBy { get; set; }

		public virtual List<LoyaltyPromotion> LoyaltyPromotions { get; set; }
	}

	public enum PromotionTypeEnums
	{
		[Display(Name = "Premium")]
		Premium = 1,

		[Display(Name = "Pinned")]
		Pinned
	}
}
