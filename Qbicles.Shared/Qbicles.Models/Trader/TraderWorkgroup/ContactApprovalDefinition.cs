namespace Qbicles.Models.Trader
{

    public class ContactApprovalDefinition : ApprovalRequestDefinition
    {
        public TraderProcess TraderProcessType { get; set; }
        public virtual WorkGroup WorkGroup { get; set; }
    }
}