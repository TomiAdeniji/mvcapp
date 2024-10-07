using Qbicles.Models.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Qbicles.Models.Loyalty
{
    public class OrderDiscountBulkDealVoucherInfo : BulkDealVoucherInfo
    {
        public const decimal NO_MAX_DISCOUNT_VALUE = -1;
        public OrderDiscountBulkDealVoucherInfo()
        {
            this.Type = VoucherType.OrderDiscount;
        }

        /// <summary>
        /// %
        /// </summary>
        [Required]
        [DecimalPrecision(5,2)]
        public decimal OrderDiscount { get; set; }

        [Required]
        [DecimalPrecision(10, 2)]
        public decimal MaxDiscountValue { get; set; } = NO_MAX_DISCOUNT_VALUE; //= Unlimited
    }
}
