using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_bulkdeal_WeekDay")]
    public class LoyaltyBulkDealWeekDay
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Day { get; set; }

        [Required]
        public virtual BulkDealVoucherInfo BulkDealVoucherInfo { get; set; }
    }
}
