using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Inventory
{
    /// <summary>
    /// Thic class contains the details of each Product/Item in a Waste Report
    /// </summary>
    /// 
    [Table("trad_wasteitem")]
    public class WasteItem
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual TraderItem Product { get; set; }

        [Required]
        public virtual ProductUnit CountUnit { get; set; }

        public decimal WasteCountValue { get; set; }

        public decimal SavedInventoryCount { get; set; }

        public string Notes { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public virtual ApplicationUser LastUpdatedBy { get; set; }

        public virtual WasteReport WasteReport { get; set; }



    }
}
