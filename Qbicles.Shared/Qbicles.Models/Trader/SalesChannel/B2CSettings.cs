using Qbicles.Models.Trader.ODS;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.SalesChannel
{
    [Table("settings_b2c")]
    public class B2CSettings : SalesChannelSettings
    {
        public B2CSettings()
        {
            this.SalesChannel = SalesChannelEnum.B2C;
            this.OrderStatusWhenAddedToQueue = PrepQueueStatus.NotStarted;
        }
        public virtual WorkGroup DefaultSaleWorkGroup { get; set; }
        public virtual WorkGroup DefaultInvoiceWorkGroup { get; set; }
        public virtual WorkGroup DefaultPaymentWorkGroup { get; set; }
        public virtual WorkGroup DefaultTransferWorkGroup { get; set; }
        public virtual TraderCashAccount DefaultPaymentAccount { get; set; }

        /// <summary>
        /// Use these settings(default workgroup) for all future orders
        /// </summary>
        public bool UseDefaultWorkgroupSettings { get; set; } = false;
    }
}