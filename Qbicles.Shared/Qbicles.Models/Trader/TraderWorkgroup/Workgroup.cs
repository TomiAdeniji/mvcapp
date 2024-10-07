using Qbicles.Models.Manufacturing;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_workgroup")]
    public class WorkGroup
    {

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        [Required]
        public virtual Qbicle Qbicle { get; set; }

        [Required]
        public virtual Topic Topic { get; set; }

        [Required]
        public virtual List<TraderProcess> Processes { get; set; } = new List<TraderProcess>();
        
        [Required]
        public virtual List<TraderGroup> ItemCategories { get; set; } = new List<TraderGroup>();

        public virtual List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> Reviewers { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> Approvers { get; set; } = new List<ApplicationUser>();

        public virtual List<ApprovalRequestDefinition> ApprovalDefs { get; set; } = new List<ApprovalRequestDefinition>();

        public  DateTime CreatedDate { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public virtual List<TraderSale> Sales { get; set; } = new List<TraderSale>();
        public virtual List<TraderPurchase> Purchases { get; set; } = new List<TraderPurchase>();
        public virtual List<TraderTransfer> Transfers { get; set; } = new List<TraderTransfer>();

        public virtual List<TraderContact> Contacts { get; set; } = new List<TraderContact>();
        public virtual List<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual List<CashAccountTransaction> Payments { get; set; } = new List<CashAccountTransaction>();

        public virtual List<WasteReport> WasteReports { get; set; } = new List<WasteReport>();

        public virtual List<SpotCount> SpotCounts { get; set; } = new List<SpotCount>();

        public virtual List<StockAudit> StockAudits { get; set; } = new List<StockAudit>();

        public virtual List<ManuJob> ManufacturingJobs { get; set; } = new List<ManuJob>();
    }


    //32 ref
    //public enum TraderProcessEnum
    //{
    //    Purchase = 1,
    //    Sale = 2,
    //    Transfer = 3,
    //    Payment = 4,
    //    Contact = 5,
    //    Invoice = 6
    //}
}
