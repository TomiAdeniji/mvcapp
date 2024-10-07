using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.B2B
{
    [Table("b2b_posts")]
    public class B2BPost: DataModelBase
    {
        [Required]
        public virtual B2BProfile Profile { get; set; }
        [Required]
        [StringLength(150)]
        public string Title { get; set; }
        [Required]
        [StringLength(500)]
        public string Content { get; set; }
        public string FeaturedImageUri { get; set; }
        [Required]
        public bool IsFeatured { get; set; }
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }
        [Required]
        public DateTime LastUpdatedDate { get; set; }

    }
}
