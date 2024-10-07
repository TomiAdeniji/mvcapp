
using CleanBooksData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_bktransaction")]
    public class BKTransaction
    {
        public int Id { get; set; }

        public virtual JournalEntry JournalEntry { get; set; }

        public virtual BKAccount Account { get; set; }

        public decimal? Debit { get; set; }

        public decimal? Credit { get; set; }

        public decimal? Balance { get; set; }
        public string Memo { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual List<TransactionDimension> Dimensions { get; set; } = new List<TransactionDimension>();

        public DateTime PostedDate { get; set; }

        public virtual List<QbiclePost> Posts { get; set; } = new List<QbiclePost>();

        public virtual List<QbicleMedia> AssociatedFiles { get; set; } = new List<QbicleMedia>();

        public virtual BKTransaction Parent { get; set; }

        [StringLength(250)]
        public string Reference { get; set; }


        /// <summary>
        /// A Bookkeeping Account can be associated with one or more CleanBooks accounts.
        /// This property will list those CleanBoosk accounts this BKTransaction has been associated with.
        /// </summary>
        public virtual List<transaction> CBTransactions { get; set; } = new List<transaction>();
    }
}