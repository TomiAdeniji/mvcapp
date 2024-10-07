using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community

{

    public enum CommunityPageActivityVisibilityEnum
    {
        Disabled = 0,
        Public = 1,
        Premium = 2

    }

    [Table("com_communitypage")]

    public class CommunityPage : Page
    { 
        [Required]
        public string Title { get; set; }
        [Required]
        public string BodyText { get; set; }
        [Required]
        public string FeaturedImage { get; set; }
        [Required]
        public string FeaturedImageCaption { get; set; }

        [Required]
        public virtual QbicleDomain Domain{ get; set; }

        [Required]
        public virtual Qbicle Qbicle { get; set; }

        [Column(TypeName ="bit")]
        public bool IsDisplayedOnDomainProfile { get; set; }

        public int DisplayOrderOnDomainProfile { get; set; }

        [DataType(DataType.EmailAddress)]
        public string PublicContactEmail { get; set; }

        public CommunityPageActivityVisibilityEnum AlertsDisplayStatus { get; set; }

        public CommunityPageActivityVisibilityEnum FilesDisplayStatus { get; set; }

        public CommunityPageActivityVisibilityEnum EventsDisplayStatus { get; set; }

        public CommunityPageActivityVisibilityEnum PostsDisplayStatus { get; set; }

        public CommunityPageActivityVisibilityEnum ArticlesDisplayStatus { get; set; }

        public virtual List<Article> Articles { get; set; } = new List<Article>();

        public virtual List<ApplicationUser> Followers { get; set; } = new List<ApplicationUser>();

        public virtual ApplicationUser Follower_1 { get; set; }
        public virtual ApplicationUser Follower_2 { get; set; }
        public virtual ApplicationUser Follower_3 { get; set; }
        public virtual ApplicationUser Follower_4 { get; set; }
        public virtual ApplicationUser Follower_5 { get; set; }

        [Column(TypeName = "bit")]
        public bool IsFeatured { get; set; }
        public int FeatureOrder { get; set; }
    }
}
