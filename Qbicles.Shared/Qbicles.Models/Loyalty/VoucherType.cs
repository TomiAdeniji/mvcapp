using System.ComponentModel;

namespace Qbicles.Models.Loyalty
{
    public enum VoucherType
    {
        [Description("Item discount")]
        ItemDiscount = 1,
        [Description("Order discount")]
        OrderDiscount = 2
    }
}
