using Qbicles.Models.MicroQbicleStream;
using System;
using System.Collections.Generic;

namespace Qbicles.Models.Highlight
{
    public class HighlightModel
    {
        public int Id { get; set; }
        public HighlightPostType Type { get; set; }
        public string TypeName { get; set; }
        public string Title { get; set; }
        public Uri ImageUri { get; set; }
        public string Content { get; set; }
        public virtual List<BaseModel> Tags { get; set; }
        public string HyperLink { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedDateString { get; set; }
        public int HoursAgo { get; set; }
        public int MinutesAgo { get; set; }
        public int DaysAgo { get; set; }
        public int LikedTimes { get; set; }
        public string LikeStatusString { get; set; }
        public bool IsCreatedByCurrentUser { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public bool IsBookmarkedByCurrentUser { get; set; }
        public bool IsDomainFollowedByCurrentUser { get; set; }
        public int LoveNum { get; set; }
        public Uri LogoUri { get; set; }
        public string BusinessName { get; set; }
        public int DomainId { get; set; }
        public string DomainKey { get; set; }

        //News Information
        public string Citation { get; set; }

        //Listing information
        public bool IsFlaggedByCurrentUser { get; set; }
        public string CountryName { get; set; }
        public string AreaName { get; set; }
        public string ListingTypeName { get; set; }
        public ListingType HLListingType { get; set; }
        public string Reference { get; set; }

        //Event information
        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
        public string EventLocation { get; set; }

        //Job information
        public string Salary { get; set; }
        public string ClosingDateString { get; set; }
        public string SkillRequired { get; set; }

        //Real Estate information
        public string Price { get; set; }
        public string SelectedPropertyTypeName { get; set; }
        public List<string> SelectedProperties { get; set; }
        public int BedroomNum { get; set; }
        public int BathroomNum { get; set; }

        public bool ShowPropertyInfo { get; set; }
        public bool ShowReadArticle { get; set; }
        public bool ShowExternalSite{ get; set; }

        public bool IsDomainHiddenByCurrentUser { get; set; }
    }
}
