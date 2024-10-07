using Newtonsoft.Json;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Loyalty;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Qbicles.BusinessRules.Model
{
	public class B2bConfigModel
	{
		public List<string> Services { get; set; }
		public int LocationId { get; set; }
		public int SourceQbicleId { get; set; }
		public int DefaultTopicId { get; set; }
		public PrepQueueStatus SettingOrder { get; set; }
		public SalesChannelEnum SalesChannel { get; set; }
		public PrepQueueStatus OrderStatusWhenAddedToQueue { get; set; }
		public ApplicationUser CurrentUser { get; set; }

		public int DefaultSaleWorkGroupId { get; set; }
		public int DefaultInvoiceWorkGroupId { get; set; }
		public int DefaultPaymentWorkGroupId { get; set; }
		public int DefaultTransferWorkGroupId { get; set; }
		public int DefaultPaymentAccountId { get; set; }

		public int DefaultPurchaseWorkGroupId { get; set; }
		public int DefaultBillWorkGroupId { get; set; }
		public int DefaultPurchasePaymentWorkGroupId { get; set; }
		public int DefaultPurchaseTransferWorkGroupId { get; set; }
		public int DefaultPurchasePaymentAccountId { get; set; }
	}
	public class B2bProfileModel
	{
		public int Id { get; set; }
		public QbicleDomain Domain { get; set; }
		public string BusinessName { get; set; }
		public string BusinessEmail { get; set; }
		public string BusinessSummary { get; set; }
		public List<string> AreasOfOperation { get; set; }
		public string LogoUri { get; set; }
		public string BannerUri { get; set; }
		public bool IsDisplayedInB2BListings { get; set; }
		public bool IsDisplayedInB2CListings { get; set; }
		public List<string> UserIdB2BRelationshipManagers { get; set; }
		public List<string> UserIdB2CRelationshipManagers { get; set; }
		public ApplicationUser CurrentUser { get; set; }
	}
	public class B2bPostModel
	{
		public int Id { get; set; }
		public int ProfileId { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public string FeaturedImageUri { get; set; }
		public bool IsFeatured { get; set; }
		public ApplicationUser CurrentUser { get; set; }
	}
	public class B2bProcessesConst
	{
		public const string ProfileEditing = "Profile editing";
		public const string Partnerships = "Partnerships";
		public const string Relationships = "Relationships";
	}
	public class B2bServicesConst
	{
		public const string Logistics = "Logistics";
		public const string Design = "Design";
		public const string Maintenance = "Maintenance";
	}
	public class B2bPriceListModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Summary { get; set; }
		public ApplicationUser CurrentUser { get; set; }
		public string IconUri { get; set; }
		public int LocationId { get; set; }
	}
	public class B2bLogisticsPartnershipModel
	{
		public int RelationshipId { get; set; }
		public List<int> ProviderLocations { get; set; }
		public List<int> ConsumerLocations { get; set; }
		public ApplicationUser CurrentUser { get; set; }
		public QbicleDomain SentDomain { get; set; }
		public QbicleDomain ReceivedDomain { get; set; }
	}
	public class B2bChargingFrameworkProposal
	{
		public int ProviderId { get; set; }
		public int PricelisId { get; set; }
	}
	public class B2bRelationshipsModel
	{
		public int RelationshipId { get; set; }
		public int PartnerDomainId { get; set; }
		public string PartnerDomainKey { get; set; }
		public string PartnerDomainName { get; set; }
		public string PartnerDomainLogoUri { get; set; }
		public B2BQbicle RelationshipHub { get; set; }
		public IEnumerable<Partnership> Partnerships { get; set; }
	}
	public class B2bVerhicleTypeIcon
	{
		public static readonly Dictionary<VehicleType, string> vehicleTypes = new Dictionary<VehicleType, string>()
		{
			{ VehicleType.Car, "~/Content/DesignStyle/img/delivery/car.png" },
			{ VehicleType.Bicycle, "~/Content/DesignStyle/img/delivery/bike.png" },
			{ VehicleType.Lorry, "~/Content/DesignStyle/img/delivery/lorry.png"  },
			{ VehicleType.Motorbike, "~/Content/DesignStyle/img/delivery/motobike.png"},
			{ VehicleType.OnFoot,  "~/Content/DesignStyle/img/delivery/foot.png" },
		};
	}
	public class B2bLocationsModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public TraderLocation Location { get; set; }
		public bool AllowSelect { get; set; }
	}
	public class B2bItemDetailModel
	{
		public int ItemId { get; set; }
		public string ItemName { get; set; }
		public string SKU { get; set; }
		public List<Select2Option> Units { get; set; }
		public List<Select2Option> Locations { get; set; }
		public List<string> StringTaxRates { get; set; }
	}
	public class B2bLinkConsumerItem
	{
		public int TradingItemId { get; set; }
		public string TradingName { get; set; }
		public int ConsumerItemId { get; set; }
		public int ConsumerUnitId { get; set; }
		public List<int> ConsumerLocations { get; set; }
	}
	public class FindBusinesesRequest : PaginationRequest
	{
		public int currentDomainId { get; set; }
		public int locationId { get; set; }
		public List<string> services { get; set; }
	}

	public class CloneDistributorCatalogueItem
	{
		public int itemId { get; set; }
		public int groupId { get; set; }
		public string sku { get; set; }
		[AllowHtml]
		public string description { get; set; }
		public bool isprimaryVendor { get; set; }
		public int destinationDomainId { get; set; }
		public string CurrentUserId { get; set; }
		public int b2bRelationshipId { get; set; }
	}

	public class LoyaltyPromotionAndTypeModel
	{
		public LoyaltyPromotion LoyaltyPromotion { get; set; } = new LoyaltyPromotion();
		public IEnumerable<LoyaltyPromotionType> LoyaltyPromotionTypes { get; set; } = new List<LoyaltyPromotionType>();
		public IEnumerable<TraderLocation> TraderGeoLocations { get; set; } = new List<TraderLocation>();
	}
	public class LoyaltyBulkDealPromotionAndTypeModel
	{
		public LoyaltyBulkDealPromotion LoyaltyBulkDealPromotion { get; set; } = new LoyaltyBulkDealPromotion();
	}

	public class PromotionTypeModel
	{
		public string PromotionTypeKey { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public int Type { get; set; }
		public int Rank { get; set; }
		public int Duration { get; set; }
		public decimal Price { get; set; } = decimal.Zero;
		public bool IsActive { get; set; } = true;

		//Audit
		public DateTime CreatedOn { get; set; }
		public string CreatedBy { get; set; } = string.Empty;
		public DateTime LastModifiedOn { get; set; }
		public string LastModifiedBy { get; set; } = string.Empty;
	}

	public class PromotionModel
	{
		public string PromotionKey { get; set; }
		public string Name { get; set; }
        [JsonIgnore]
        public LoyaltyPromotionType PlanType { get; set; } = new LoyaltyPromotionType();
		public string Description { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime DisplayDate { get; set; }
		public Uri FeaturedImageUri { get; set; }
		public bool IsDraft { get; set; }
		public bool IsHalted { get; set; }
		public bool IsArchived { get; set; }
        [JsonIgnore]
        public VoucherType Type { get; set; } = new VoucherType();
		[JsonIgnore]
		public LoyaltyPromotionAudience Audience { get; set; } = new LoyaltyPromotionAudience();
        [JsonIgnore]
        public LoyaltyPromotionPaymentTransaction PaymentTransaction { get; set; } = new LoyaltyPromotionPaymentTransaction();
        public string CurrentUserId { get; set; }
		public string DaysOfweek { get; set; }
		public string Timezone { get; set; }
		public string DateFormat { get; set; }
		public int CurrentDomainId { get; set; }
		public string FromTime { get; set; }
		public DateTime? VoucherExpiryDate { get; set; }
		public string VoucherExpiryDateString { get; set; }
		public string ToTime { get; set; }
		public string BusinessName { get; set; }
		public int BusinessProfileId { get; set; }
		public string BusinessKey { get; set; }
		public bool IsMarkedLiked { get; set; }
		public bool IsLiked { get; set; }
		public bool AllowClaimNow { get; set; }
		public string RemainHtmlInfo { get; set; }
		public object RemainInfo { get; set; }
		public string DomainLogo { get; set; }
		public int MarkedLikedCount { get; set; }
		public int TotalClaimed { get; set; }
		public bool IsConnected { get; set; }

		public string StartDateString { get; set; }
		public string EndDateString { get; set; }
		public string DisplayDateString { get; set; }
	}
    public class BulkDealPromotionModelItemOverview
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ImageUri { get; set; }
        public string ItemName { get; set; }
        public string SKU { get; set; }
        public string Barcode { get; set; }
        public string Location { get; set; }
        public int DomainId { get; set; }
        public string Business { get; set; }
    }
	public class DomainInBulkDealPromotion
	{
		public int DomainId { get; set; }
		public string DomainName { get; set; }
	}
    public class PromotionFilterModel
	{
		public string keyword { get; set; } = string.Empty;
		public string daterange { get; set; } = string.Empty;
		/// <summary>
		/// 0: All
		/// 1: Item discount
		/// 2: Order discount
		/// </summary>
		public int type { get; set; } = 0;
		/// <summary>
		/// 1: Active
		/// 2: Pending
		/// 3: Expired
		/// 4: Archived
		/// 5: Stoped
		/// 6: Draft
		/// </summary>
		public string[] status { get; set; } = new string[0];
		public int currentDomainId { get; set; } = 0;
		public string dateformat { get; set; } = string.Empty;
		public string timezone { get; set; } = string.Empty ;
	}
	public class PromotionPublishFilterModel : PaginationRequest
	{
		public string businessKey { get; set; }
		public bool isMyFavourites { get; set; }
		public bool isLoadTotalRecord { get; set; }
		public List<int> locationIds { get; set; }
		public string currentUserId { get; set; }
		public string timezone { get; set; }
		public string dateformat { get; set; }
	}
	public class VoucherByUserAndShopModel : PaginationRequest
	{
		public bool isRedeemed { get; set; }
		public bool isExpired { get; set; }
		public string domainkey { get; set; }
		public string currentUserId { get; set; }
		public string dateformat { get; set; }
		public string timezone { get; set; }
		public bool isCountRecords { get; set; }
	}
	public class VoucherOfUserModel
	{
		public string PromotionKey { get; set; }
		public string VourcherKey { get; set; }
		public string Name { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public DateTime RedeemedDate { get; set; }
		//public string DateString { get; set; }
		public VoucherOfUserType Type { get; set; }
		public string TypeName { get { return Type.GetDescription(); } }
		public string DateString
		{
			get
			{
				switch (Type)
				{
					case VoucherOfUserType.IsValid:
						return $"Expires on {EndDate.DatetimeToOrdinalAndTime()}";
					case VoucherOfUserType.IsRedeemed:
						return $"Redeemed on {RedeemedDate.DatetimeToOrdinalAndTime()}";
					case VoucherOfUserType.IsExpired:
						return $"Expired on {EndDate.DatetimeToOrdinal()}";
					default:
						return "";
				}
			}
		}
	}
	public class B2BOrderCreationDiscussionModel
	{
		public string Partnershipkey { get; set; }
		public int PartnershipId { get; set; }
		public int OrderReferenceId { get; set; }
		public string OrderFullRef { get; set; }
		public int CatalogId { get; set; }
		public string OrderNote { get; set; }
		public string CurrentUserId { get; set; }
	}
	public class B2BOrderItemModel
	{
		public string DisKey { get; set; }
		public string OrderKey { get; set; }
		public int DiscussionId { get; set; }
		public int OrderId { get; set; }
		public Variant Variant { get; set; }
		public List<Extra> Extras { get; set; }
		public int Quantity { get; set; }
		public decimal IncludedTaxAmount { get; set; }
		public int ItemId { get; set; }
		public int AssociatedItemId { get; set; }
		public int AssociatedUnitId { get; set; }
		public bool PrimaryVendor { get; set; }
		public string CurrentUserId { get; set; }
	}
	public class B2BCartItemModel
	{
		public int ItemId { get; set; }
		public string ItemName { get; set; }
		public string CategoryName { get; set; }
		public decimal Quantity { get; set; }
		public decimal ItemInitialPrice { get; set; }
		public decimal Discount { get; set; }
		public List<Models.TraderApi.Tax> Taxes { get; set; }
		public string TaxInfo { get; set; }
		public decimal Price { get; set; }
		public string SourceName { get; set; }
		public decimal PriceWithoutDiscount { get; set; }
		public string PriceString { get; set; }
		public decimal TotalPrice { get; set; }
		public bool IsAllowEdit { get; set; }
		public Select2Option AssociatedItem { get; set; }
		public List<Select2Option> AssociatedUnits { get; set; }
		public Models.TraderApi.Variant Variant { get; set; }
		public List<Models.TraderApi.Variant> Extras { get; set; }
	}
	public class B2BSubmitProposal
	{
		public string CurrentUserId { get; set; }
		public string disKey { get; set; }
		public int discussionId { get; set; }
		public string orderKey { get; set; }
		public int tradeOrderId { get; set; }
		public int paymentAccId { get; set; }
		public int saleWGId { get; set; }
		public int purchaseWGId { get; set; }
		public int invoiceWGId { get; set; }
		public int paymentWGId { get; set; }
		public int transferWGId { get; set; }
		public int destinationLocationId { get; set; }
		/// <summary>
		/// Use these settings(default workgroup) for all future orders
		/// </summary>
		public bool SaveSettings { get; set; }
	}

	public class B2BProfileWizardModel
	{
		public int Id { get; set; }
		public int DomainId { get; set; }
		public string BusinessName { get; set; }
		public string BusinessSummary { get; set; }
		public string BusinessEmail { get; set; }
		public string LogoUri { get; set; }
		public bool IsB2BServicesProvided { get; set; } = true;
		public bool IsDisplayedInB2BListings { get; set; } = true;
		public bool IsDisplayedInB2CListings { get; set; } = true;
		public DomainWizardStepMicro WizardStep { get; set; }
		public virtual List<string> AreasOperation { get; set; } = new List<string>();
		public virtual List<string> Tags { get; set; } = new List<string>();
		public virtual List<string> B2BManagers { get; set; } = new List<string>();
		public virtual List<string> B2CManagers { get; set; } = new List<string>();
		public virtual List<InvitationModal> Invitations { get; set; } = new List<InvitationModal>();
		public virtual List<int> Categories { get; set; } = new List<int>();
	}

	public class InvitationModal
	{
		public string Email { get; set; }
		public string Status { get; set; }
		public int StatusId { get; set; }
	}
}
