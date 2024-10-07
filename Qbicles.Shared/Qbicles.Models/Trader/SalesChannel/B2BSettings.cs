using Qbicles.Models.Trader.ODS;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.SalesChannel
{
    [Table("settings_b2b")]
    public class B2BSettings: SalesChannelSettings
    {
        public B2BSettings()
        {
            this.SalesChannel = SalesChannelEnum.B2B;
            this.OrderStatusWhenAddedToQueue = PrepQueueStatus.NotStarted;
        }

        [Required]
        public bool IsDesign { get; set; }
        [Required]
        public bool IsLogistics { get; set; }
        [Required]
        public bool IsMaintenance { get; set; }

        public virtual WorkGroup DefaultSaleWorkGroup { get; set; }
        public virtual WorkGroup DefaultInvoiceWorkGroup { get; set; }
        public virtual WorkGroup DefaultPaymentWorkGroup { get; set; }
        public virtual WorkGroup DefaultTransferWorkGroup { get; set; }
        public virtual TraderCashAccount DefaultPaymentAccount { get; set; }

        public virtual WorkGroup DefaultPurchaseWorkGroup { get; set; }
        public virtual WorkGroup DefaultBillWorkGroup { get; set; }
        public virtual WorkGroup DefaultPurchasePaymentWorkGroup { get; set; }
        public virtual WorkGroup DefaultPurchaseTransferWorkGroup { get; set; }
        public virtual TraderCashAccount DefaultPurchasePaymentAccount { get; set; }
    }
}
