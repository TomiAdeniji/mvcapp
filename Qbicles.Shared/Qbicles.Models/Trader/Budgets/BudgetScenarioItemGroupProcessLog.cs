using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader.Budgets;

namespace Qbicles.Models.Trader.Inventory
{
    /// <summary>
    /// This class inherits from the ApprovalReqHistory log because this log is used to record the extra infomation associated with
    /// processing a BudgetScenarioItemGroup through its stages.
    /// </summary>
    [Table("trad_BudgetScenarioItemGroupProcessLog")]
    public class BudgetScenarioItemGroupProcessLog
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// This property records the status of the Associated BudgetScenarioItemGroup at the time the Log is created
        /// </summary>
        [Required]
        public BudgetScenarioItemGroupStatus BudgetScenarioItemGroupStatus { get; set; }

        /// <summary>
        /// This property records the WasteReport with which the log is associated
        /// </summary>
        [Required]
        public virtual BudgetScenarioItemGroup AssociatedBudgetScenarioItemGroup { get; set; }


        /// <summary>
        /// This property associates the Waste Report Log that was created when the Waste Report was updated
        /// </summary>
        [Required]
        public virtual BudgetScenarioItemGroupLog AssociatedBudgetScenarioItemGroupLog { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time at which the update occurred.
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual ApprovalReqHistory ApprovalReqHistory { get; set; }
    }
}
