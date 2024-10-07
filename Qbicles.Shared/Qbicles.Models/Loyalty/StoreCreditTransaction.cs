using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_StoreCreditTransaction")]
    public class StoreCreditTransaction
    {
        public int Id { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public virtual StoreCreditAccount Account { get; set; }

        [Required]
        public StoreCreditTransactionType Type { get; set; }

        [Required]
        public StoreCreditTransactionReason Reason { get; set; }
    }


    public enum StoreCreditTransactionType
    {
        Credit = 1,
        Debit = 2
    }

    public enum StoreCreditTransactionReason
    {
        GeneratedFromUser = 0,
        GeneratedFromStorePoints = 1,
        GeneratedFromAccountBalance = 2,
        UsedInPayment = 3
        // Leave the following out for now
        // TransferedToSomeoneElse = 4,
        // DeletedBySystem = 5,
        // ReducedByPayment = 6,
        // ReducedByTransferToBälance = 7

    }
}
