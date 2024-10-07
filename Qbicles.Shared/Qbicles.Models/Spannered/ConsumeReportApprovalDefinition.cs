namespace Qbicles.Models.Spannered
{
    public class ConsumeReportApprovalDefinition : ApprovalRequestDefinition
    {
        public SpanneredProcess SpanneredProcessType { get; set; }
        public SpanneredWorkgroup SpanneredWorkgroup { get; set; }
    }
}
