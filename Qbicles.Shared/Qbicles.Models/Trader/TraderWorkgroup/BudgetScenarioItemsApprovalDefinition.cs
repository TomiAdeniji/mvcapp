namespace Qbicles.Models.Trader.TraderWorkgroup
{ 
    public class BudgetScenarioItemsApprovalDefinition : ApprovalRequestDefinition
    {
        public virtual TraderProcess BudgetGroupItemsProcessType { get; set; }
        public virtual WorkGroup BudgetGroupItemsWorkGroup { get; set; }
    }
}
