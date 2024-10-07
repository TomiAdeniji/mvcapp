using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.Inventory
{
    /// <summary>
    /// Thic class contains the details of each Product/Item in a SpotCOunt
    /// </summary>
    /// 
    [Table("trad_spotcountitem")]
    public class SpotCountItem
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual TraderItem Product { get; set; }

        [Required]
        public virtual ProductUnit CountUnit { get; set; }

        public decimal SpotCountValue { get; set; }

        public decimal SavedInventoryCount { get; set; }

        public string Notes { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// When the spot count item is first created the LastUpdateDate = CreatedDate
        /// </summary>
        [Required]
        public DateTime LastUpdatedDate { get; set; }


        /// <summary>
        /// When the spot count item is first created the LastUpdatedBy = CreatedBy
        /// </summary>
        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        public virtual SpotCount SpotCount { get; set; }

        public decimal Adjustment { get; set; }



    }
}
