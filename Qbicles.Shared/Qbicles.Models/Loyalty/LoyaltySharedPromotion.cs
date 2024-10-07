using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_sharedpromotion")]
    public class LoyaltySharedPromotion : QbicleActivity
    {
        public virtual LoyaltyPromotion SharedPromotion { get; set; }
        public virtual ApplicationUser SharedWith { get; set; }
        public virtual ApplicationUser SharedBy { get; set; }
        public DateTime ShareDate { get; set; }
        public string SharedWithEmail { get; set; }
    }
}