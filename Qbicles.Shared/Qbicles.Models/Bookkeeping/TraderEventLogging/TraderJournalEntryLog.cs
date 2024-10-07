using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_TraderJournalEntryLog")]
    public class TraderJournalEntryLog
    {
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public TraderJournalEntryLogType Type { get; set; }

        [Required]
        public virtual JournalEntry JournalEntry { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }

    public enum TraderJournalEntryLogType
    { 
        Manufacturing = 0,
        Payment = 1,
        PurchaseInventory = 2,
        PurchaseNoninventory = 3,
        SaleInvoice = 4,
        SaleTransfer = 5
    }

}
