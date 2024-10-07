using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Base;
namespace Qbicles.Models.Trader.Budgets
{
    /// <summary>
    /// This class is a group of Items, 
    /// active at the location of the parent BudgetScenario,
    /// that have been chosen by a member of the Workgroup with which the BudgetScenarioItemGroup is associated.
    /// This group must go through an Approval Process before it can be part of a BudgetScenario
    /// </summary>
    [Table("trad_budgetscenatiogroup")]
    public class BudgetScenarioItemGroup: DataModelBase
    {
        /// <summary>
        /// This is the BudgetScenarion with which the BudgetScenarioItemGroup is associated
        /// </summary>
        [Required]
        public virtual BudgetScenario BudgetScenario { get; set; }


        /// <summary>
        /// This is the WorkGroup, which was selected when the BudgetScenario was created
        /// THis points to the ApprovalDefinition that is used for the approval process for this BudgetScenarioItemGroup
        /// </summary>
        [Required]
        public virtual WorkGroup WorkGroup { get; set; }


        /// <summary>
        /// This is the Approval Reuest used to process this BudgetScenarioItemGroup through approvals
        /// </summary>
        public virtual ApprovalReq ApprovalRequest { get; set; }


        /// <summary>
        /// This is the list of items in the group
        /// </summary>
        public virtual List<BudgetScenarioItem> BudgetScenarioItems { get; set; } = new List<BudgetScenarioItem>();

        


        /// <summary>
        /// The user from Qbicles who created the Budget Scenario Item Group
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Budget Scenario Item Group was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the Budget Scenario Item Group, this is to be set each time the Budget Scenario Item Group is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Budget Scenario Item Group was last edited.
        /// This is to be set each time the Budget Scenario Item Group is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

       
        /// <summary>
        /// This indicates the statzs of the BudgetGroupItems as they progress through the approval process
        /// </summary>
        [Required]
        public BudgetScenarioItemGroupStatus Status { get; set; }

        /// <summary>
        /// This is to indicate which type of item is associated with the group
        /// </summary>
        public ItemGroupType Type { get; set; }
    }


    public enum BudgetScenarioItemGroupStatus
    {
        Draft = 1,
        Pending = 2,
        Reviewed = 3,
        Approved = 4,
        Denied = 5,
        Discarded = 6
    }

    public enum ItemGroupType
    {
        [Description("Items I Buy")]
        ItemsIBuy = 1,
        [Description("Items I Sell")]
        ItemsISell = 2,
        [Description("Items I Buy And Sell")]
        ItemsIBuyAndSell = 3
    }
}
