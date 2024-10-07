using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_workgroup")]
    public class BKWorkGroup
    {

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }


        [Required]
        public virtual Qbicle Qbicle { get; set; }

        [Required]
        public virtual Topic Topic { get; set; }

        [Required]
        public virtual List<BookkeepingProcess> Processes { get; set; } = new List<BookkeepingProcess>();
        

        public virtual List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> Reviewers { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> Approvers { get; set; } = new List<ApplicationUser>();

        public virtual List<ApprovalRequestDefinition> ApprovalDefs { get; set; } = new List<ApprovalRequestDefinition>();

        public  DateTime CreatedDate { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }


        public virtual List<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();

        public virtual List<BKAccount> BKAccounts { get; set; } = new List<BKAccount>();
    }


}
