using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Qbicles.Models.ProfilePage
{
    public class FeatureList : Block
    {
        public FeatureList()
        {
            this.Type = BusinessPageBlockType.FeatureList;
        }
        [Required]
        [StringLength(250)]
        public string HeadingText { get; set; }
        public decimal HeadingFontSize { get; set; }
        [Required]
        [StringLength(50)]
        public string HeadingColour { get; set; }
        [StringLength(50)]
        public string FeatureHeadingAccentColour { get; set; }
        [Required]
        [StringLength(500)]
        public string SubHeadingText { get; set; }
        public decimal SubHeadingFontSize { get; set; }
        [Required]
        [StringLength(50)]
        public string SubHeadingColour { get; set; }
        public virtual List<FeatureItem> FeatureItems { get; set; } = new List<FeatureItem>();
    }
}