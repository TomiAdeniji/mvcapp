using Newtonsoft.Json;
using Qbicles.Models;
using Qbicles.Models.ProfilePage;
using System.Collections.Generic;

namespace Qbicles.BusinessRules.Model
{
    public class BusinessProfilePageModel
    {
        public int Id { get; set; }
        public string PageTitle { get; set; }
        [JsonIgnore]
        public QbicleDomain Domain { get; set; }
        public BusinessPageStatusEnum Status { get; set; }
        [JsonIgnore]
        public ApplicationUser CreateBy { get; set; }
        public List<Hero> BlockHeroes { get; set; }
        public List<Promotion> BlockPromotions { get; set; }
        public List<TestimonialList> BlockTestimonials { get; set; }
        public List<TextImage> BlockTextImages { get; set; }
        public List<GalleryList> BlockGalleries { get; set; }
        public List<FeatureList> BlockFeatures { get; set; }
    }
    public class UserProfilePageModel
    {
        public int Id { get; set; }
        public string PageTitle { get; set; }
        [JsonIgnore]
        public ApplicationUser user { get; set; }
        public UserPageStatusEnum Status { get; set; }
        [JsonIgnore]
        public ApplicationUser CreateBy { get; set; }
        public List<HeroPersonal> BlockHeroes { get; set; }
        public List<TextImage> BlockTextImages { get; set; }
        public List<GalleryList> BlockGalleries { get; set; }
        public List<MasonryGallery> BlockMasonryGalleries { get; set; }
    }

    public class UserProfileSharedQbicleModel
    {
        public string QbicleKey { get; set; }
        public string LogoUri { get; set; }
        public string QbicleName { get; set; }
        public string DomainOwner { get; set; }
        public string DomainName { get; set; }
    }
}
