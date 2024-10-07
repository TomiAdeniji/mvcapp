using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Qbicles.Models.Loyalty
{
	/// <summary>
	/// This class is the definition of the Promotion Aidience that will enable 
	/// the business setup the moniback promotion audience
	/// </summary>
	[Table("loy_PromotionAudience")]
	public class LoyaltyPromotionAudience : DataModelBase
	{
		/// <summary>
		/// This is the ID of the loyalty promotion.
		/// </summary>
		[Required]
		public int LoyaltyPromotionId { get; set; }
		public virtual LoyaltyPromotion LoyaltyPromotion { get; set; }

		/// <summary>
		/// This is the visibility setting based on customer location.
		/// </summary>
		[Required]
		public LocationVisibility LocationVisibility { get; set; }

		/// <summary>
		/// This is the distance for location-based visibility.
		/// </summary>
		public int Distance { get; set; }

		/// <summary>
		/// This is the distance measurement unit: feet, kilometers, or miles.
		/// </summary>
		public DistanceFactor DistanceFactor { get; set; }

		/// <summary>
		/// This is the business location used as the basis for visibility.
		/// </summary>
		public string BusinessLocation { get; set; } = string.Empty;

		/// <summary>
		/// This is for managing deletion of the promotion audience to the recycle bin.
		/// </summary>
		[Required]
		public bool IsDeleted { get; set; }

		/// <summary>
		/// This is the date the promotion audience was created.
		/// </summary>
		[Required]
		public DateTime CreatedDate { get; set; }

		/// <summary>
		/// This is the owner of the promotion audience.
		/// </summary>
		[Required]
		public ApplicationUser CreatedBy { get; set; }

		/// <summary>
		/// This is the most recent modified date of the promotion audience.
		/// </summary>
		[Required]
		public DateTime LastModifiedDate { get; set; }

		/// <summary>
		/// This is the last user that modified the promotion audience.
		/// </summary>
		[Required]
		public ApplicationUser LastModifiedBy { get; set; }
	}

	public enum LocationVisibility
	{
		[Description("Unrestricted visibility.")]
		Unrestricted = 0,

		[Description("Location-based visibility.")]
		LocationBased = 1
	}

	public enum DistanceFactor
	{
		[Description("Distance measured in feet.")]
		Feet = 1,

		[Description("Distance measured in kilometers.")]
		Kilometers = 2,

		[Description("Distance measured in miles.")]
		Miles = 3
	}






}
