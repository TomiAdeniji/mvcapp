using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Budgets
{
    /// <summary>
    /// The BudgetScenario defines the parameters of the Budget,
    /// e,g, its time span, reporting periods, projected revenue and expenditure.
    /// 
    /// It also identifies the Items, the Sales and Purchases of which, 
    /// will be used to compare actual expenditure/revenue against projected expenditure/revenue 
    /// 
    /// </summary>
    [Table("trad_BudgetScenario")]
    public class BudgetScenario
    {
        public int Id { get; set; }

        /// <summary>
        /// The Domain with which the Budget Scenario is assiociated
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        /// <summary>
        /// The title of the BudgetScenario.
        /// This muist be unique within a Location in a Domain.
        /// </summary>
        [Required]
        public string Title { get; set; }



        /// <summary>
        /// A description of the Budget Scenario
        /// </summary>
        public string Description { get; set; }


        /// <summary>
        /// This is a GUID to indicate an Image that has been uploaded and stored using Qbicles.Doc
        /// </summary>
        [Required]
        public string FeaturedImage { get; set; }


        /// <summary>
        /// This is the Start Month and Year of the timespan for the Budget
        /// </summary>
        [Required]
        public DateTime FiscalStartPeriod { get; set; }

        /// <summary>
        /// This is the End Month and Year of the timespan for the Budget
        /// </summary>
        [Required]
        public DateTime FiscalEndPeriod { get; set; }


        /// <summary>
        /// This is the period for which projections of revenue and expenditure will be made 
        /// and actual revenue & expenditure will be calculated
        /// </summary>
        [Required]
        public ReportingPeriodEnum ReportingPeriod { get; set; }

     
        /// <summary>
        /// This is the Location withn a Domain of this BudgetScenario
        /// </summary>
        [Required]
        public virtual TraderLocation Location { get; set; }


        /// <summary>
        /// This is a collection of the BudgetScenarioItemGroups (groups of items that have been added to a BudgetScenario in one go)
        /// </summary>
        public virtual List<BudgetScenarioItemGroup> BudgetScenarioItemGroups { get; set; } = new List<BudgetScenarioItemGroup>();


        /// <summary>
        /// This is a list of the Reporting Periods that are associated with this BudgetScenario
        /// This list is made by combining the timespan of the BudgetScenario (Fiscal Start and End Dates) and the Reporting Period.
        /// If the Reporting Period is Weekly, then the Fiscal Period is broken in to weeks and each named
        /// If the Reporting Period is Monthly, then the Fiscal Period is broken in to months and each named
        /// If the Reporting Period is Wuarterly, then the Fiscal Period is broken in to Quarters and each named
        /// </summary>
        public virtual List<ReportingPeriod> ReportingPeriods { get; set; } = new List<ReportingPeriod>();

        /// <summary>
        /// The user from Qbicles who created the Budget Scenario
        /// </summary>
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Budget Scenario was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The user from Qbicles who last updated the Budget Scenario, this is to be set each time the Budget Scenario is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time on which this Budget Scenario was last edited.
        /// This is to be set each time the Budget Group is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }


        /// <summary>
        /// This is a list of the BudgetGroups that are asssociated with a BudgetScenario
        /// </summary>
        public virtual List<BudgetGroup> BudgetGroups { get; set; } = new List<BudgetGroup>();



        /// <summary>
        /// This is a collection of the StartingQuantities and Units that are to bsed for those Items 
        /// that are added to a BudgetScenatio
        /// </summary>
        public virtual List<ScenarioItemStartingQuantity> ScenarioItemStartingQuantities { get; set; } = new List<ScenarioItemStartingQuantity>();

        public virtual bool IsActive { get; set; }


    }


    public enum ReportingPeriodEnum
    {
        [Description("This Week")]
        Weekly = 1,
        [Description("This Month")]
        Monthly = 2,
        [Description("This Quarter")]
        Quarterly = 3
    }
}
