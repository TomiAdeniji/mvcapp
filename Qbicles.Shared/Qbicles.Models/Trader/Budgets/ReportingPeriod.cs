using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Budgets
{
    /// <summary>
    /// This is a period, based on the time span and reporting period,
    /// associated with the parent BudgetScenario,
    /// against which projections of revenue and expenditure and actual revenue and expenditure will be shown
    /// </summary>
    [Table("trad_BudgetReportingPeriod")]
    public class ReportingPeriod
    {
        [Required]
        public int Id { get; set; }


        /// <summary>
        /// This is the parent Budget Scenario
        /// </summary>
        [Required]
        public virtual BudgetScenario BudgetScenario { get; set; }


        /// <summary>
        /// This is the name of the reporting period and will be shown when the reporting period is displayed
        /// </summary>
        [Required]
        public string Name { get; set; }


        /// <summary>
        /// This isth eorder in whcih the reporting period will be displayed
        /// </summary>
        [Required]
        public int Order { get; set; }



        /// <summary>
        /// This is the start date of the reporting period
        /// </summary>
        [Required]
        public DateTime Start { get; set; }


        /// <summary>
        /// This is the end date of the reporting period
        /// </summary>
        [Required]
        public DateTime End { get; set; }
    }
}
