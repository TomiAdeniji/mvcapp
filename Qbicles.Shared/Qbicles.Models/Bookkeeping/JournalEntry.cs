using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_journalentry")]
    public class JournalEntry : DataModelBase
    {

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        
        [Required]
        public DateTime PostedDate { get; set; }

        public virtual List<BKTransaction> BKTransactions { get; set; } = new List<BKTransaction>();

        public virtual List<QbiclePost> Posts { get; set; } = new List<QbiclePost>();

        public virtual List<QbicleMedia> AssociatedFiles { get; set; } = new List<QbicleMedia>();

        public virtual ApprovalReq Approval { get; set; }

        public virtual JournalGroup Group { get; set; }

        public virtual List<BKAccount> AssociatedAccounts { get; set; } = new List<BKAccount>();

        public int Domain_Id { get; set; }
        /// <summary>
        /// The Domain with which the Reference is associated
        /// </summary>
        [Required]
        [ForeignKey("Domain_Id")]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public int Number { get; set; }

        [Column(TypeName = "bit")]
        public bool IsApproved { get; set; }

        [Required]
        public string Description { get; set; }


        public virtual BKWorkGroup WorkGroup { get; set; }
    }
}