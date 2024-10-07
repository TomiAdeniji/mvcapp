using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community
{
    [Table("com_keyword")]
    public class KeyWord
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual List<Tag> AssociatedTags { get; set; } = new List<Tag>();

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }


    }
}
