using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_bulkdeal_VoucherInfo")]
    public class BulkDealVoucherInfo : DataModelBase
    {

        public const int CODE_LENGTH = 8;
        public const int NO_MAX_VOUCHER_COUNT = -1;

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public ApplicationUser CreatedBy { get; set; }

        [Required]
        public virtual LoyaltyBulkDealPromotion BulkDealPromotion { get; set; }

        [Required]
        public int MaxVoucherCount { get; set; } = NO_MAX_VOUCHER_COUNT; // infinite claims

        [Required]
        public int MaxVoucherCountPerCustomer { get; set; } = 1;

        [Required]
        public string TermsAndConditions { get; set; }
        [Required]
        public string BusinessesTermsAndConditions { get; set; } 

        [Required]
        public VoucherType Type { get; set; }


        public virtual List<LoyaltyBulkDealWeekDay> DaysAllowed { get; set; } = new List<LoyaltyBulkDealWeekDay>();


        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        //Many to many with Businesses
        public virtual List<QbicleDomain> Businesses { get; set; } = new List<QbicleDomain>();

        public DateTime? VoucherExpiryDate { get; set; } = null;
    }
}
