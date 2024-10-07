using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader.ODS;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Reorder
{
    [Table("trad_reorderitem")]
    public class ReorderItem
    {
        public int Id { get; set; }

        [Required]
        public virtual TraderItem Item { get; set; }
        /// <summary>
        /// This is Primary Contact has associated
        /// Get default value in Primary group
        /// </summary>
        public virtual TraderContact PrimaryContact { get; set; }
        /// <summary>
        /// This is ProductUnit has associated
        /// </summary>
        public virtual ProductUnit Unit { get; set; }

        public decimal CostPerUnit { get; set; }

        public decimal Discount { get; set; }

        public decimal InInventory { get; set; }

        public decimal OnOrder { get; set; }

        public decimal Total { get; set; }

        public decimal Quantity { get; set; }

        public virtual List<TransactionDimension> Dimensions { get; set; } = new List<TransactionDimension>();

        public virtual ReorderItemGroup ReorderItemGroup { get; set; }

        public virtual TraderTransactionItem PurchaseItem { get; set; }

        /// <summary>
        /// This is whether or not the item is disabled. If the value is true then it is used for display purposes only and not for calculation
        /// A Workgroup has associated Product Groups. If any Reorder items are in Product Groups not included in the Workgroup, they will be disabled in the Reorder and the user will only be able to perform a partial Reorder. 
        /// </summary>
        [Column(TypeName = "bit")]
        public bool IsDisabled { get; set; }

        [Column(TypeName = "bit")]
        public bool IsForReorder { get; set; }


    }
}
