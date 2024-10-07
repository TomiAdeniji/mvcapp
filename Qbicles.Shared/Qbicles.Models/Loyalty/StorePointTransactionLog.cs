using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_StorePointTransactionLog")]
    public class StorePointTransactionLog
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual StorePointTransaction AssociatedStorePointTransaction { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public decimal Points { get; set; }

        [Required]
        public virtual StorePointAccount Account { get; set; }

        [Required]
        public StorePointTransactionType Type { get; set; }

        [Required]
        public StorePointTransactionReason Reason { get; set; }

        public virtual CashAccountTransaction Payment { get; set; }

        public virtual TradeOrder Order { get; set; }

        //If this is a Type=Credit transaction then the points are created based on
        //a OrderToPointsConversion. This property references that conversion.
        public virtual OrderToPointsConversion ConversionUsed { get; set; }
    }
}