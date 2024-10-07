using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community
{
    [Table("com_userprofilepage")]
    public class UserProfilePage: Page
    {
        [Required(AllowEmptyStrings = true)]
         public string ProfileText { get; set; }

        [Required]
        public string StoredLogoName { get; set; }

        [Required]
        public string StoredFeaturedImageName { get; set; }

        [Required]
        public string StrapLine { get; set; }                

        public virtual List<ProfileFile> ProfileFiles { get; set; } = new List<ProfileFile>();

        //public virtual List<CommunitySkill> Skills { get; set; } = new List<CommunitySkill>();

        [Required]
        public virtual ApplicationUser AssociatedUser { get; set; }

        public virtual List<ApplicationUser> Followers { get; set; } = new List<ApplicationUser>();

        public virtual List<Employment> Employments { get; set; } = new List<Employment>();
    }
}
