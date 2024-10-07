using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_journalgroup")]
    public class JournalGroup
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public string Name { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual List<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();

    }
    public class JournalGroupItem
    { 
        public int Id { get; set; }   
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } 

    }
}