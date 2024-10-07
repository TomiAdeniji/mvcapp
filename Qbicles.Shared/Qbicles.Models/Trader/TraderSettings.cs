using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Base;
using Qbicles.Models.Bookkeeping;


namespace Qbicles.Models.Trader
{
    [Table("trad_tradersettings")]
    public class TraderSettings: DataModelBase
    {

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Column(TypeName = "bit")]
        public bool IsQbiclesBookkeepingEnabled { get; set; }

        public TraderSetupCurrent IsSetupCompleted { get; set; } = TraderSetupCurrent.Location;

        public virtual JournalGroup JournalGroupDefault { get; set; }

        public string SalePrefix { get; set; }
        public string SaleSuffix { get; set; }
        public string SalesOrderPrefix { get; set; }
        public string SalesOrderSuffix { get; set; }

        public string SaleReturnPrefix { get; set; }
        public string SaleReturnSuffix { get; set; }
        public string SalesReturnOrderPrefix { get; set; }
        public string SalesReturnOrderSuffix { get; set; }

        public string PurchaseOrderPrefix { get; set; }
        public string PurchaseOrderSuffix { get; set; }
        public string PurchasePrefix { get; set; }
        public string PurchaseSuffix { get; set; }

        public string InvoicePrefix { get; set; }
        public string InvoiceSuffix { get; set; }

        public string Delimeter { get; set; }

        public string TransferPrefix { get; set; }
        public string TransferSuffix { get; set; }

        public string ManuJobPrefix { get; set; }
        public string ManuJobSuffix { get; set; }

        public string BillPrefix { get; set; }
        public string BillSuffix { get; set; }

        public string AllocationPrefix { get; set; }
        public string AllocationSuffix { get; set; }

        public string CreditNotePrefix { get; set; }
        public string CreditNoteSuffix { get; set; }

        public string DebitNotePrefix { get; set; }
        public string DebitNoteSuffix { get; set; }

        public string ReorderPrefix { get; set; }
        public string ReorderSuffix { get; set; }

        public string OrderPrefix { get; set; }
        public string OrderSuffix { get; set; }

        public string PaymentPrefix { get; set; }
        public string PaymentSuffix { get; set; }

        public string DeliveryPrefix { get; set; }
        public string DeliverySuffix { get; set; }

        public string AlertGroupPrefix { get; set; }
        public string AlertGroupSuffix { get; set; }
        public string AlertReportPrefix { get; set; }
        public string AlertReportSuffix { get; set; }
    }

    public enum TraderSetupCurrent
    {
        Location = 1,
        ProductGroup = 2,
        Workgroup = 3,
        Accounting = 4,
        Complete = 5,
        TraderApp = 6,
        Contact = 7
    }
}
