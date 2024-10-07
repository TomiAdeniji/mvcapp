
using Qbicles.Models.Base;
using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_Voucher")]
    public class Voucher: DataModelBase
    {
        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual LoyaltyPromotion Promotion { get; set; }

        [Required]
        public virtual ApplicationUser ClaimedBy { get; set; }

        [Required]
        public string Code { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRedeemed { get; set; } = false;

        public DateTime RedeemedDate { get; set; }

        public virtual TraderLocation RedeemedLocation { get; set; }

        public DateTime? VoucherExpiryDate { get; set; } = null;
    }
}
