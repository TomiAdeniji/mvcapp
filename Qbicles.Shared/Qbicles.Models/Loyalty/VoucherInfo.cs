using Qbicles.Models.Base;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_VoucherInfo")]
    public class VoucherInfo : DataModelBase
    {

        public const int CODE_LENGTH = 8;
        public const int NO_MAX_VOUCHER_COUNT = -1;

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public ApplicationUser CreatedBy { get; set; }

        [Required]
        public virtual LoyaltyPromotion Promotion { get; set; }

        [Required]
        public int MaxVoucherCount { get; set; } = NO_MAX_VOUCHER_COUNT; // infinite claims

        [Required]
        public int MaxVoucherCountPerCustomer { get; set; } = 1;

        [Required]
        public string TermsAndConditions { get; set; }

        [Required]
        public VoucherType Type { get; set; }


        public virtual List<LoyaltyWeekDay> DaysAllowed { get; set; } = new List<LoyaltyWeekDay>();


        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }


        public virtual List<TraderLocation> Locations { get; set; } = new List<TraderLocation>();

        public DateTime? VoucherExpiryDate { get; set; } = null;
    }
}
