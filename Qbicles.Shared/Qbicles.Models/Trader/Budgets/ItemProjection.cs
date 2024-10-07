using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Budgets
{
    /// <summary>
    /// This class stores the projected Revenue Quantity and Expenditure for each reporting period,
    /// for an item included in the Budget Scenario
    /// </summary>
    [Table("trad_ItemProjection")]
    public class ItemProjection
    {
        public int Id { get; set; }


        /// <summary>
        /// This is the BudgetScenarioItem with which the project is associated
        /// </summary>
        [Required]
        public virtual BudgetScenarioItem BudgetScenarioItem { get; set; }

        
        /// <summary>
        /// This is the number of items that are expected to be sold in this period
        /// </summary>
        public decimal RevenueQuantity { get; set; }

        /// <summary>
        /// This is the value of the items that are expected to be sold in this period
        /// </summary>
        public decimal RevenueValue { get; set; }


        /// <summary>
        /// This is the number of items that are expected to be bought in this period
        /// </summary>
        public decimal ExpenditureQuantity { get; set; }


        /// <summary>
        /// This is the value of the items that are expected to be bought in this period
        /// </summary>
        public decimal ExpenditureValue { get; set; }


        /// <summary>
        /// This is the reporting period that this project is associated with
        /// </summary>
        [Required]
        public virtual ReportingPeriod ReportingPeriod { get; set; }

    }
}
