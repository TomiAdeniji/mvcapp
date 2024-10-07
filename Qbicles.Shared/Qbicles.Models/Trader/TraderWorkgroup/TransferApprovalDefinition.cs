namespace Qbicles.Models.Trader
{

    public class TransferApprovalDefinition : ApprovalRequestDefinition
    {
        public TraderProcess TraderProcessType { get; set; }

        public virtual WorkGroup WorkGroup { get; set; }
    }
}