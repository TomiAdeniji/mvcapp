namespace Qbicles.Models.Trader.TraderWorkgroup
{
    public class TillPaymentApprovalDefinition : ApprovalRequestDefinition
    {
        public virtual TraderProcess TillPaymentProcessType { get; set; }
        public virtual WorkGroup TillPaymentWorkGroup { get; set; }
    }
}
