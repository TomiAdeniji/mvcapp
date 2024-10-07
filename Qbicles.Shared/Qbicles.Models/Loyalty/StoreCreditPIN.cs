using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_StoreCreditPIN")]
    public class StoreCreditPIN
    {
        public int Id { get; set; }
        public ApplicationUser AssociatedUser { get; set; }
        public bool IsArchieved { get; set; }
        public DateTime ArchivedDate { get; set; }
        public StoreCreditTransactionReason Reason { get; set; }

        // Track which TraderContact generated PIN
        // If this equals to null, means associated User requested the change directly
        public TraderContact CreatedByContact { get; set; }

        // 4 digits PIN
        public string PIN { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
