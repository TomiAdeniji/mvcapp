using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Budgets
{
    [Table("trad_BudgetScenarioItem")]
    public class BudgetScenarioItem
    {
        public int Id { get; set; }

        [Required]
        public virtual TraderItem Item { get; set; }

        [Required]
        public virtual BudgetScenarioItemGroup BudgetScenarioItemGroup { get; set; }

        public virtual List<ItemProjection> ItemProjections { get; set; }



        public decimal SaleQuantity { get; set; }

        public decimal AverageSalePrice { get; set; }


        public decimal PurchaseQuantity { get; set; }

        public decimal AveragePurchaseCost { get; set; }


        public virtual ScenarioItemStartingQuantity StartingQuantity { get; set; }


    }
}
