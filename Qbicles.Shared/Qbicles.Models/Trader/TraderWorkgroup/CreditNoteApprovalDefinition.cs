namespace Qbicles.Models.Trader.TraderWorkgroup
{
    public class CreditNoteApprovalDefinition : ApprovalRequestDefinition
    {
        public virtual TraderProcess CreditNoteProcessType { get; set; }
        public virtual WorkGroup CreditNoteWorkGroup { get; set; }
    }
}
