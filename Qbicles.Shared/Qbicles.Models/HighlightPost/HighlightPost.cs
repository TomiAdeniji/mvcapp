using Qbicles.Models.Community;
using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Highlight
{
    [Table("hl_post")]
    public class HighlightPost: DataModelBase
    {
        public virtual QbicleDomain Domain { get; set; }
        public HighlightPostType Type { get; set; }
        public string Title { get; set; }
        public string ImgUri { get; set; }
        public string Content { get; set; }
        public virtual List<Tag> Tags { get; set; }
        //public string HyperLink { get; set; }
        //public virtual List<User> UserAndHighlightXrefs { get; set; } = new List<UserAndHighlightPostXref>();
        public int LikedTimes { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual List<ApplicationUser> LikedBy { get; set; } = new List<ApplicationUser>();
        public virtual List<ApplicationUser> BookmarkedBy { get; set; } = new List<ApplicationUser>();
        public HighlightPostStatus Status { get; set; }
    }

    public enum HighlightPostType
    {
        [Description("Article")]
        Article = 1,
        [Description("News")]
        News = 2,
        [Description("Knowledge")]
        Knowledge = 3,
        [Description("Listing")]
        Listings = 4
    }

    public enum OrderByType
    {
        Popularity = 1,
        CreatedDate = 2
    }

    public enum HighlightPostStatus
    {
        Active = 1,
        InActive = 2,
    }

    public class HighlightPostStreamModel
    {
        public List<HighlightModel> HighlightPosts { get; set; }
        public int TotalPage { get; set; }
        public int BookmarkedNumber { get; set; }
        public int FlaggedNumber { get; set; }
        public int DomainPosts { get; set; }
        public int DomainFollowers { get; set; }
    }

    public class HighlightPostCustomModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CreatedDate { get; set; }
        public string Status { get; set; }
        public string StatusLabel { get; set; }
        public string Country { get; set; }
        public string Category { get; set; }
        public string Reference { get; set; }
        public string PersonName { get; set; }
        public string PersonImgUri { get; set; }
        public bool IsPublished { get; set; }
        public HighlightPostType HlType { get; set; }
        public ListingType LsType { get; set; }
    }

    public class FlagCustomModel
    {
        public string FlaggedUserId { get; set; }
        public string PersonName { get; set; }
        public string PersonImageUri { get; set; }
        public string Title { get; set; }
        public string PostReference { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreateDateString { get; set; }
        public int DomainId { get; set; }
        public bool HasB2CChat { get; set; }
        public int PostId { get; set; }
        public string BusinessProfileId { get; set; }
    }
    public class PostFlagCustomModel
    {
        public virtual ApplicationUser User { get; set; }
        public virtual ListingHighlight Post { get; set; }
    }
}
