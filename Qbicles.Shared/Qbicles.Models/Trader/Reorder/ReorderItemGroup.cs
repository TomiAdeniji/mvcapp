using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Reorder
{
    [Table("trad_reorderitemgroup")]
    public class ReorderItemGroup
    {
        public int Id { get; set; }
        public virtual Reorder Reorder { get; set; }

        public virtual List<ReorderItem> ReorderItems { get; set; } = new List<ReorderItem>();

        public virtual TraderPurchase Purchase { get; set; }
        /// <summary>
        /// This is Primary Contact has associated
        /// if is value null then  Unallocated
        /// </summary>
        public virtual TraderContact PrimaryContact { get; set; }
        public DeliveryMethodEnum? DeliveryMethod { get; set; }
        public int DaysToLastBasis { get; set; }
        public string Days2Last { get; set; }
        public int DaysToLast { get; set; }
        public decimal Total { get; set; }
    }
}
