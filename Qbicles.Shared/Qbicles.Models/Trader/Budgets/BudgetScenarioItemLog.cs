using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader.Budgets;


namespace Qbicles.Models.Trader.Inventory
{
    /// <summary>
    /// Thic class contains the log details of each Product/Item in a Waste Report
    /// i.e. each time a WasteReport is updated a copy of each of the waste items is added to the log
    /// </summary>
    /// 
    [Table("trad_BudgetScenarioItemLog")]
    public class BudgetScenarioItemLog
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

