namespace Qbicles.Models.Trader
{
    
    public class SalesReturnApprovalDefinition : ApprovalRequestDefinition
    {
        public TraderProcess SaleReturnTraderProcessType { get; set; }

        public virtual WorkGroup SaleReturnWorkGroup { get; set; }
    }
 
}
