using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community
{
    [Table("com_domainprofilepage")]
    public class DomainProfile: Page
    {

        [Required]
        public string StrapLine { get; set; }


        [Required]
        public string ProfileText { get; set; }


        [Required]
        public string StoredLogoName { get; set; }


        [Required]
        public string StoredFeaturedImageName{ get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        public virtual List<ApplicationUser> Followers { get; set; } = new List<ApplicationUser>();
    }
}