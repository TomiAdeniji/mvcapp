using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.ProfilePage
{
    public class Promotion : Block
    {
        public Promotion()
        {
            this.Type = BusinessPageBlockType.Promotion;
        }
        [Required]
        [StringLength(250)]
        public string HeadingText { get; set; }
        public decimal HeadingFontSize { get; set; }
        [Required]
        [StringLength(50)]
        public string HeadingColour { get; set; }
        public string HeadingAccentColour { get; set; }
        [Required]
        public string SubHeadingText { get; set; }
        public decimal SubHeadingFontSize { get; set; }
        [Required]
        [StringLength(50)]
        public string SubHeadingColour { get; set; }
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
        public string FeaturedImage { get; set; }
        public virtual List<PromotionItem> Items { get; set; } = new List<PromotionItem>();
    }
}
