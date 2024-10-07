using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader.Budgets;

namespace Qbicles.Models.Trader.Inventory
{
    /// <summary>
    /// The class on for logging changes to BudgetScenarioItemGroup
    /// </summary>
    [Table("trad_budgetscenarioitemgroupLog")]
    public class BudgetScenarioItemGroupLog
    {
        [Required]
        public int Id { get; set; }


        /// <summary>
        /// This is the waste report for which this is a log
        /// </summary>
        [Required]
        public BudgetScenarioItemGroup BudgetScenarioItemGroup { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }


        /// <summary>
        /// For the lot of waste items we do not refer to the 
        /// public virtual List<BudgetScenarioItem> ProductList { get; set; }
        /// 
        /// Instead we refer to the BudgetScenarioItemLogs collection
        /// </summary>
        public virtual List<BudgetScenarioItemLog> BudgetScenarioItemLogs { get; set; } = new List<BudgetScenarioItemLog>();

        
        public virtual ApprovalReq BudgetScenarioItemGroupApprovalProcess { get; set; }

        public virtual WorkGroup Workgroup { get; set; }

        public virtual BudgetScenarioItemGroupStatus Status { get; set; } = BudgetScenarioItemGroupStatus.Draft;


        /// <summary>
        /// This is the current user when the log was created
        /// </summary>
        [Required]
        public virtual ApplicationUser UpdatedBy { get; set; }

        /// <summary>
        /// This is the date on which the LOG is created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public virtual ApplicationUser LastUpdatedBy { get; set; }

        


    }




}
