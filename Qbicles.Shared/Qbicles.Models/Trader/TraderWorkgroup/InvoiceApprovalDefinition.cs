namespace Qbicles.Models.Trader
{

    public class InvoiceApprovalDefinition : ApprovalRequestDefinition
    {
        public TraderProcess TraderProcessType { get; set; }

        public virtual WorkGroup WorkGroup { get; set; }
    }
}