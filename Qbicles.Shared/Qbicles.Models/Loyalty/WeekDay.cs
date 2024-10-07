using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_WeekDay")]
    public class LoyaltyWeekDay
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Day { get; set; }

        [Required]
        public virtual VoucherInfo VoucherInfo { get; set; }
    }
}
