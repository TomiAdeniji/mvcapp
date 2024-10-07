using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_MyTag")]
    public class MyTag
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual MyDesk Desk { get; set; }

        [Required]
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public ApplicationUser Creator { get; set; }
        public virtual List<QbicleActivity> Activities { get; set; } = new List<QbicleActivity>();
        public virtual List<QbiclePost> Posts { get; set; } = new List<QbiclePost>();
    }
}
