using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.PoS
{
    [Table("pos_table")]
    public class POSTable
    {
        public int Id { get; set; }

        [Required]
        public virtual TraderLocation Location { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// This is the name of the table. 
        /// </summary>
        [Required]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "POS Table name shoud be no more than 20 characters")]
        public string Name { get; set; }

        [StringLength(70, ErrorMessage = "POS Table name shoud be no more than 70 characters")]
        public string Summary { get; set; }

    }
}
