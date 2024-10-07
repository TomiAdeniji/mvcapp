using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Budgets
{
    /// <summary>
    /// When doing the projectsions for an item in the Budget Scenario,
    /// it is necessary to know what the start Quantity of an item is
    /// and also what units should be used for the item.
    /// 
    /// A ScenarioItemStartingQuantity is created for each item that is in a BudgetScenario.BudgetGroup.ProductGroup,
    /// and is used when the budget projection process is started as the item is explicitly added as an item to the BudgetScenario
    /// </summary>
    [Table("trad_ScenarioItemStartingQuantity")]
    public class ScenarioItemStartingQuantity
    {

        public int Id { get; set; }


        /// <summary>
        /// This is the BudgetScenarion with which the ScenarioItemStartingQuantity is associated
        /// </summary>
        [Required]
        public virtual BudgetScenario BudgetScenario { get; set; }

        /// <summary>
        /// This is the specific item with which the ScenarioItemStartingQuantity is associated
        /// </summary>
        [Required]
        public virtual TraderItem Item { get; set; }

        /// <summary>
        /// This is the Product Unit of the item to be used when the projection is created
        /// </summary>
        [Required]
        public virtual ProductUnit Unit { get; set; }


        /// <summary>
        /// This is the starting quantity
        /// </summary>
        public decimal Quantity { get; set; }


        /// <summary>
        /// This is the Quantity that remains after each item's budget projection is completed 
        /// </summary>
        public decimal QuantityRemaining { get; set; }


        /// <summary>
        /// This is a collection of the Items that make use of this ScenarioItemStartingQuantity
        /// </summary>
        public virtual List<BudgetScenarioItem> ScenarioItems { get; set; } = new List<BudgetScenarioItem>();

    }
}
