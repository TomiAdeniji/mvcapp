using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.ProfilePage
{
    public class TextImage : Block
    {
        public TextImage()
        {
            this.Type = BusinessPageBlockType.TextImage;
        }
        [Required]
        [StringLength(150)]
        public string HeadingText { get; set; }
        public decimal HeadingFontSize { get; set; }
        [Required]
        [StringLength(50)]
        public string HeadingColour { get; set; }
        [Required]
        [StringLength(50)]
        public string HeadingAccentColour { get; set; }
        [Required]
        public string Content { get; set; }
        public decimal ContentFontSize { get; set; }
        [Required]
        [StringLength(50)]
        public string ContentColour { get; set; }
        [Column(TypeName = "bit")]
        public bool IsIncludeButton { get; set; }
        [Required]
        [StringLength(50)]
        public string ButtonText { get; set; }
        [Required]
        [StringLength(50)]
        public string ButtonColour { get; set; }
        public string ButtonJumpTo { get; set; }

        [StringLength(250)]
        public string ExternalLink { get; set; }
        [StringLength(150)]
        public string FeaturedImage { get; set; }
    }
}
