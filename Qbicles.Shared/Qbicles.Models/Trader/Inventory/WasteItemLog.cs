using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Qbicles.Models.Trader.Inventory
{
    /// <summary>
    /// Thic class contains the log details of each Product/Item in a Waste Report
    /// i.e. each time a WasteReport is updated a copy of each of the waste items is added to the log
    /// </summary>
    /// 
    [Table("trad_wasteitemlog")]
    public class WasteItemLog
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

