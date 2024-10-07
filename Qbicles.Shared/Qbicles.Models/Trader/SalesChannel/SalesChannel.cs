using System.ComponentModel;

namespace Qbicles.Models.Trader.SalesChannel
{

    public enum SalesChannelEnum
    {
        [Description("Trader")]
        Trader = 1,
        [Description("POS")]
        POS = 3, 
        [Description("B2C")]
        B2C = 5,
        [Description("B2B")]
        B2B = 6
    }
}
