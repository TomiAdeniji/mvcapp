using Qbicles.Models.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Qbicles.Models.Loyalty
{
    public class ItemDiscountVoucherInfo : VoucherInfo

    {

        public const int NO_MAX_ITEMS_PER_ORDER = -1;

        public ItemDiscountVoucherInfo()
        {
            this.Type = VoucherType.ItemDiscount;
        }

        [Required]
        public string ItemSKU { get; set; }

        [Required]
        public int MaxNumberOfItemsPerOrder { get; set; } = NO_MAX_ITEMS_PER_ORDER;
        /// <summary>
        /// %
        /// </summary>
        [Required]
        [DecimalPrecision(5, 2)]
        public decimal ItemDiscount { get; set; }

    }
}
