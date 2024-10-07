namespace Qbicles.Models.Trader
{

    public class ManuApprovalDefinition : ApprovalRequestDefinition
    {
        public TraderProcess TraderProcessType { get; set; }
        public virtual WorkGroup WorkGroup { get; set; }
    }
}