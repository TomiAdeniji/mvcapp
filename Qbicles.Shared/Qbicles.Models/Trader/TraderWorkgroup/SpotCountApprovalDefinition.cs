namespace Qbicles.Models.Trader
{

    public class SpotCountApprovalDefinition : ApprovalRequestDefinition
    {
        public TraderProcess TraderProcessType { get; set; }
        public virtual WorkGroup WorkGroup { get; set; }
    }
}