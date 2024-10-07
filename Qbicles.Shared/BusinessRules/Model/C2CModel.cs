using Newtonsoft.Json;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Catalogs;
using Qbicles.Models.ProductSearch;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Resources;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Qbicles.BusinessRules.Model
{
    public class PeopleInfoModel
    {
        public string DomainKey { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvatarUri { get; set; }
        public string Summary { get; set; }
        /// <summary>
        /// 1: Only businesses
        /// 2: Only individuals
        /// </summary>
        public int Type { get; set; }
        public bool HasConnected { get; set; }
        public bool HasRemoved { get; set; } = false;

        /// <summary>
        /// Check if aan active B2C Business already has Default Manager;
        /// Always = true for Individuals type
        /// </summary>
        public bool HasDefaultB2CRelationshipManager { get; set; }

    }

    public class ProductInfoModel
    {
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public string ProviderName { get; set; }
        public string ProductPrice { get; set; }
        public string ProductImage { get; set; }
        public string Key { get; set; }
        public string DomainKeyEncrypt { get; set; }
        public string[] Brand { get; set; }
    }

    public class FindPeopleRequest : PaginationRequest
    {
        /// <summary>
        /// 0: Limit to my affiliates
        /// The default value will be “Limit to my affiliates” which limits the list to only people I’m affiliated with i.e. we share a Qbicle or Domain
        /// 1: Show full public list
        /// The other option is “Show full public list” which displays the entire list of Qbicles users, irrespective of their relationship (or lack of) to me.
        /// </summary>
        public FindPeopleType PeopleType { get; set; }
        /// <summary>
        /// 0: Show all types
        /// 1: Only businesses
        /// 2: Only individuals
        /// </summary>
        public FindContactType ContactType { get; set; }
        public string CurrentUserId { get; set; }
        public int CurrentBusinessId { get; set; }
    }


    public enum FindPeopleType
    {
        [Description("Limit to my affiliates")]
        Affiliates = 0,
        [Description("Show full public list")]
        FullPublic = 1
    }

    public enum FindContactType
    {
        [Description("Show all types")]
        All = 0,
        [Description("Only businesses")]
        Businesses = 1,
        [Description("Only individuals")]
        Individuals = 2
    }


    public class B2CC2CModel
    {
        public int Id { get; set; }
        public int QbicleId { get; set; }
        public virtual List<ApplicationUser> LikedBy { get; set; }
        public virtual List<ApplicationUser> LinkUsers { get; set; }
        public virtual B2BProfile LinkBusiness { get; set; }
        public CommsStatus Status { get; set; }
        /// <summary>
        /// 1: Only businesses
        /// 2: Only individuals
        /// </summary>
        public int Type { get; set; }
        public DateTime LastUpdated { get; set; }
        /// <summary>
        /// The user who initiated the relationship
        /// </summary>
        public ApplicationUser SourceUser { get; set; }
        public bool NotViewed { get; set; }

    }
    public class C2CSearchQbicleModel
    {
        public int Orderby { get; set; }
        public int ContactType { get; set; }
        public bool OnlyShowFavourites { get; set; }
        public bool IncludeBlocked { get; set; }
    }
    public class MyOrder
    {
        public int QbicleId { get; set; }
        public int OrderId { get; set; }
        public string FullRef { get; set; }
        public DateTime Placed { get; set; }
        public TradeOrderStatusEnum Status { get; set; }
        public string BusinessName { get; set; }
        public string BusinessLogoUri { get; set; }
        public int BusinessId { get; set; }
        public string BusinessKey { get; set; }
        public int DomainId { get; set; }
        public string DomainKey { get; set; }
        public SalesChannelEnum Channel { get; set; }
        //Get from Description of TraderOrder
        public virtual Invoice Invoice { get; set; }
        public virtual List<CashAccountTransaction> Payments { get; set; }
    }


    public class DiscusionOrder
    {
        public DiscusionOrderBy Type { get; set; }
        public string DisplayName { get; set; }
    }

    public enum DiscusionOrderBy
    {
        Business = 1,
        Customer = 2
    }

    /// <summary>
    /// The model used on filtering in Community > Shopping tab > Product tab
    /// </summary>
    public class FeaturedProductModel
    {
        public int CatItemId { get; set; }
        public string CatItemName { get; set; }
        public string CatItemImageUri { get; set; }
        public string ProductImageUri { get; set; }
        public string SKU { get; set; }
        public B2BProfile B2BProfileItem { get; set; }
        public decimal Price { get; set; }
        public TraderItem AssociatedTraderItem { get; set; }
        public CurrencySetting AssociatedCurrencySetting { get; set; }
        public Catalog AssociatedCatalog { get; set; }
        public int DisplayOrder { get; set; }
        public string DomainKey { get; set; }
        public int DomainId { get; set; }
        public List<AdditionalInfo> AdditionalInfos { get; set; }
        public string Country { get; set; }
    }

    /// <summary>
    /// The model used on showing items initially in Community > Shopping tab > Product tab
    /// </summary>
    public class CommunityFeturedProductModel
    {
        public int Id { get; set; }
        public string DomainKey { get; set; }
        public FeaturedType Type { get; set; }
        public string TypeName { get; set; }
        public string LogoImageUri { get; set; }
        public string BusinessLogo { get; set; }
        public string CategoryItemName { get; set; }
        public string BusinessName { get; set; }
        public string PriceStr { get; set; }
        public decimal Price { get; set; }
        public CurrencySetting AssociatedCurrencySetting { get; set; }
        public Catalog AssociatedCatalog { get; set; }
        public string CatalogName { get; set; }
        public int CatalogId { get; set; }
        public string AssociatedCatalogKey { get; set; }
        public string FeaturedImageURL { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class CommunityFeturedProductMicroModel
    {
        public int Id { get; set; }
        public string DomainKey { get; set; }
        public FeaturedType Type { get; set; }
        public string TypeName { get; set; }
        public string LogoImageUri { get; set; }
        public string BusinessLogo { get; set; }
        public string CategoryItemName { get; set; }
        public string BusinessName { get; set; }
        public string PriceStr { get; set; }
        public decimal Price { get; set; }
        [JsonIgnore]
        public CurrencySetting AssociatedCurrencySetting { get; set; }
        [JsonIgnore]
        public Catalog Catalog { get; set; }
        public string AssociatedCatalogKey { get; set; }
        public string CatalogName { get; set; }
        public int CatalogId { get; set; }
        public string FeaturedImageURL { get; set; }
        public int DisplayOrder { get; set; }
        [JsonIgnore]
        public B2BProfile B2BProfileItem { get; set; }
        [JsonIgnore]
        public List<AdditionalInfo> AdditionalInfos { get; set; }
        //public Select2CustomeModel AssociatedCatalog { get; set; }
        public string Country { get; set; }
        public string Brand { get; set; }
        public List<string> Tags { get; set; }
    }
}
