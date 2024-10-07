using Newtonsoft.Json;
using Qbicles.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.ProfilePage
{
    [Table("profile_block")]
    public class Block : DataModelBase
    {

        [Required]
        [JsonIgnore]
        public virtual ProfilePage ParentPage { get; set; }

        /// <summary>
        /// This is the order in which the block appears on the business page
        /// </summary>
        [Required]
        public int DisplayOrder { get; set; }

        [Required]
        public BusinessPageBlockType Type { get; set; }

        /// <summary>
        /// Use map for Button jumps to
        /// </summary>
        [NotMapped]
        public string ElmId { get; set; }
    }

    public enum BusinessPageBlockType
    {
        Hero = 1,
        FeatureList = 2,
        TextImage = 3,
        Testimonial = 4,
        Promotion = 5,
        Gallery = 6,
        MasonryGallery = 7,
        HeroPersonal = 8
    }
}
