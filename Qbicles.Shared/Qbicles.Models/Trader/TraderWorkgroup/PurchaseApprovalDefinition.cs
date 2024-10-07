namespace Qbicles.Models.Trader
{

    public class PurchaseApprovalDefinition : ApprovalRequestDefinition
    {
        public TraderProcess TraderProcessType { get; set; }

        public virtual WorkGroup WorkGroup { get; set; }
    }
}
