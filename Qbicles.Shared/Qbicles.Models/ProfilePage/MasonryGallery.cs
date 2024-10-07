using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.ProfilePage
{
    public class MasonryGallery:Block
    {
        public MasonryGallery()
        {
            this.Type = BusinessPageBlockType.MasonryGallery;
        }
        [Required]
        [StringLength(150)]
        public string HeadingText { get; set; }
        public decimal HeadingFontSize { get; set; }
        [StringLength(50)]
        public string HeadingColour { get; set; }
        [StringLength(50)]
        public string HeadingAccentColour { get; set; }
        public string SubHeadingText { get; set; }
        public decimal SubHeadingFontSize { get; set; }

        [StringLength(50)]
        public string SubHeadingColour { get; set; }
        public virtual List<MasonryGalleryItem> GalleryItems { get; set; } = new List<MasonryGalleryItem>();
    }
}
