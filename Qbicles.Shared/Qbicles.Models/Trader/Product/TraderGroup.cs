using Qbicles.Models.Spannered;
using Qbicles.Models.Trader.Budgets;
using Qbicles.Models.Trader.Pricing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    /// <summary>
    /// This is the product group for a collection of Items.
    /// It is used to assign access to the items through the WorkGroup connection and
    /// it is used to assignining to the Items by the Price Book.
    /// </summary>
    [Table("trad_group")]
    public class TraderGroup
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual List<TraderItem> Items { get; set; } = new List<TraderItem>();

        public virtual List<WorkGroup> WorkGroupCategories { get; set; } = new List<WorkGroup>();
        public virtual List<SpanneredWorkgroup> SpanneredWorkgroups { get; set; } = new List<SpanneredWorkgroup>();

        public virtual List<ProductGroupPriceDefaults> PriceDefaults { get; set; }


        /// <summary>
        /// This is to be used to identify the Expenditure BudgetGroup that the TraderGroup is associated with.
        /// Note: A TraderGroup can be associed with AT MOST one BudgetGroup of type Expenditure 
        /// </summary>
        public virtual BudgetGroup ExpenditureBudgetGroup { get; set; }

        /// <summary>
        /// This is to be used to identify the Revenue BudgetGroup that the TraderGroup is associated with.
        /// Note: A TraderGroup can be associed with AT MOST one BudgetGroup of type Revenue 
        /// </summary>
        public virtual BudgetGroup RevenueBudgetGroup { get; set; }


    }
}
