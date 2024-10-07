namespace Qbicles.Models.Trader
{

    public class WasteReportApprovalDefinition : ApprovalRequestDefinition
    {
        public TraderProcess TraderProcessType { get; set; }
        public virtual WorkGroup WorkGroup { get; set; }
    }
}