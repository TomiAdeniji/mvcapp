namespace Qbicles.Models.Trader.TraderWorkgroup
{
    public class ShiftAuditApprovalDefinition : ApprovalRequestDefinition
    {
        public virtual TraderProcess ShiftAuditTraderProcessType { get; set; }
        public virtual WorkGroup ShiftAuditWorkGroup { get; set; }
    }
}

