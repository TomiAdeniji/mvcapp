namespace Qbicles.Models.Trader
{
    
    public class SalesApprovalDefinition : ApprovalRequestDefinition
    {
        public TraderProcess TraderProcessType { get; set; }

        public virtual WorkGroup WorkGroup { get; set; }
    }

}
