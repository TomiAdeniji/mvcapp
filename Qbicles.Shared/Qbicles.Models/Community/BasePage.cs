using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community
{

    public enum CommunityPageTypeEnum
    {
        DomainProfile = 0,
        UserProfile = 1,
        CommunityPage = 2

    }

    [Table("com_page")]
    public abstract class Page
    {

        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        
        public DateTime LastUpdated { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        public virtual List<Tag> Tags { get; set; } = new List<Tag>();

        [Required]
        public CommunityPageTypeEnum PageType { get; set; }




        [Column(TypeName = "bit")]
        public bool IsSuspended { get; set; }


    }

}