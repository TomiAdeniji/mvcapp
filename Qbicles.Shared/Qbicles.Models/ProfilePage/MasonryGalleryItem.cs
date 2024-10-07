using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.ProfilePage
{
    [Table("profile_masonrygalleryitem")]
    public class MasonryGalleryItem
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        public string Title { get; set; }
        [Required]
        [StringLength(500)]
        public string Content { get; set; }
        [Required]
        [StringLength(150)]
        public string FeaturedImage { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
        [Required]
        [JsonIgnore]
        public virtual MasonryGallery MasonryGallery { get; set; }
    }
}
