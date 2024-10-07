namespace Qbicles.Models.Trader
{

    public class PaymentApprovalDefinition : ApprovalRequestDefinition
    {
        public TraderProcess TraderProcessType { get; set; }

        public virtual WorkGroup WorkGroup { get; set; }
    }
}