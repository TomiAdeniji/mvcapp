using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community
{
    [Table("com_article")]
    public class Article
    {
        public int Id { get; set; }

        [Required]
        public string Image { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Source { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public ApplicationUser CreatedBy { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

        [Required]
        public string URL { get; set; }

        [Required]
        public virtual CommunityPage AssociatedPage { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool IsDisplayed { get; set; }
    }
}