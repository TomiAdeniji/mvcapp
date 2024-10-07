using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Manufacturing;
using Qbicles.Models.Operator.TimeAttendance;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.Spannered;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Budgets;
using Qbicles.Models.Trader.CashMgt;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.Payments;
using Qbicles.Models.Trader.Returns;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_ApprovalReqs")]
    public class ApprovalReq : QbicleActivity
    {
        public ApprovalReq()
        {
            this.ActivityType = ActivityTypeEnum.ApprovalRequestApp;
            this.App = ActivityApp.Qbicles;
        }
        public ApprovalPriorityEnum Priority { get; set; }
        public string Notes { get; set; }

        public virtual ApprovalRequestDefinition ApprovalRequestDefinition { get; set; }

        

        public virtual List<JournalEntry> JournalEntries { get; set; }

        public virtual List<BKAccount> BKAccounts { get; set; }

        public RequestStatusEnum RequestStatus { get; set; } = RequestStatusEnum.Pending;

        public virtual List<ApplicationUser> ReviewedBy { get; set; }

        public virtual ApplicationUser ApprovedOrDeniedAppBy { get; set; }

        public virtual List<TraderSale> Sale { get; set; }
        
        public virtual List<TraderPurchase> Purchase { get; set; }

        public virtual List<TraderTransfer> Transfer { get; set; }

        public virtual List<TraderContact> TraderContact { get; set; }

        public virtual List<Invoice> Invoice { get; set; }

        public virtual List<CashAccountTransaction> Payments { get; set; }

        public virtual List<SpotCount> SpotCounts { get; set; }

        public virtual List<WasteReport> WasteReports { get; set; }

        public virtual List<ApprovalReqHistory> ApprovalReqHistories { get; set; }

        public virtual List<ManuJob> Manufacturingjobs { get; set; }

        public virtual List<CampaignPostApproval> CampaigPostApproval { get; set; }

        public virtual List<EmailPostApproval> EmailPostApproval { get; set; }

        public virtual List<CreditNote> CreditNotes { get; set; }
        public virtual List<StockAudit> StockAudits { get; set; }

        public virtual List <BudgetScenarioItemGroup> BudgetScenarioItemGroups { get; set; }

        public virtual List<TraderReturn> TraderReturns { get; set; }

        public virtual List<Attendance> OperatorClockIn { get; set; }

        public virtual List<Attendance> OperatorClockOut { get; set; }

        public virtual List<TillPayment> TillPayment { get; set; }

        public virtual List<ConsumptionReport> ConsumptionReports { get; set; }

        public enum ApprovalPriorityEnum
        {
            Low,
            Medium,
            High,
            Urgent
        }
        public enum RequestStatusEnum
        {
            [Description("Awaiting Review")]
            Pending,
            [Description("Awaiting Approval")]
            Reviewed,
            [Description("Approved")]
            Approved,
            [Description("Denied")]
            Denied,
            [Description("Discarded")]
            Discarded
        }
    }
}