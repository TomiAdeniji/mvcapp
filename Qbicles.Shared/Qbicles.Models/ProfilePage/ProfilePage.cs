using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.ProfilePage
{
    [Table("profile_page")]
    public class ProfilePage : DataModelBase
    {
        [Required]
        public ProfilePageType Type { get; set; }
        [Required]
        public string PageTitle { get; set; }
        /// <summary>
        /// This is the order in which the page is displayed
        /// </summary>
        public int DisplayOrder { get; set; }
        [Required]
        public virtual ApplicationUser CreateBy { get; set; }
        [Required]
        public DateTime CreateDate { get; set; }
        public virtual ApplicationUser LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public virtual List<Block> Blocks { get; set; } = new List<Block>();
    }


    public enum ProfilePageType
    {
        User = 1,
        Business = 2
    }
}
