using Qbicles.Models.Highlight;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community
{
    [Table("com_tags")]
    public class Tag
    {
        [Required]
        public int Id { get; set; }



        [Column(TypeName = "bit")]
        public bool IsDomainProfileTag { get; set; }


        [Column(TypeName = "bit")]
        public bool IsUserProfileTag { get; set; }


        [Column(TypeName = "bit")]
        public bool IsCommunityPageTag { get; set; }


        [Column(TypeName = "bit")]
        public bool IsSkillTag { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual List<Page> Pages { get; set; } = new List<Page>();

        //public virtual List<CommunitySkill> Skills { get; set; } = new List<CommunitySkill>();

        public virtual List<KeyWord> AssociatedKeyWords { get; set; } = new List<KeyWord>();

        public virtual List<HighlightPost> HLPosts { get; set; } = new List<HighlightPost>();
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime EditedDate { get; set; }
    }
}
