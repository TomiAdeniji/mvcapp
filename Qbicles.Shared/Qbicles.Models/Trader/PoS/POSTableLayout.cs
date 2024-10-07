using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.PoS
{
    [Table("pos_tablelayout")]
    public class POSTableLayout
    {
        public int Id { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This is the Image of the table layout. 
        /// </summary>
        public string ImageUri { get; set; }

        public string ImageType { get; set; }

        [Required]
        public DateTime LastUpdatedDate { get; set; }


    }
}