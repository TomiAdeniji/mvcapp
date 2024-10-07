using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Reorder
{
    [Table("trad_reorder")]
    public class Reorder
    {
        public int Id { get; set; }
        /// <summary>
        ///Placed(if Complete)
        ///Last saved(if Incomplete)
        /// </summary>
        public DateTime DateComplete { get; set; }
        /// <summary>
        /// This total cost of Reorder
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// An incomplete order has been saved, but not yet confirmed
        /// A complete order has been confirmed by the user and initiated
        /// Only incomplete reorders have an option - “Continue”, which allows the user to resume working with the reorder.
        /// </summary>
        public StatusEnum Status { get; set; }

        public virtual TraderLocation Location { get; set; }
        /// <summary>
        /// This is Workgroup has associated
        /// A Workgroup has associated Product Groups. 
        /// If any Reorder items are in Product Groups not included in the Workgroup, 
        /// they will be disabled in the Reorder and the user will only be able to perform a partial Reorder
        /// </summary>
        public virtual WorkGroup Workgroup { get; set; }
        /// <summary>
        /// All items from Product Group will be removed completely from the Reorder, and a page refresh will occur to update the view (and totals) accordingly 
        /// </summary>
        public TraderGroup ExcludeProductGroup { get; set; }
        public DeliveryMethodEnum DeliveryMethod { get; set; }

        public virtual List<ReorderItemGroup> ReorderItemGroups { get; set; } = new List<ReorderItemGroup>();

        public virtual TraderReference Reference { get; set; }
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        public enum StatusEnum {
            InComlete=0,
            Complete=1
        }
    }
}
