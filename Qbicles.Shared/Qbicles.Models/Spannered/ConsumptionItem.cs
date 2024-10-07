using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Spannered
{
    [Table("sp_consumptionttems")]
    public class ConsumptionItem
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public virtual ConsumptionReport ConsumptionReport { get; set; }
        [Required]
        public virtual TraderItem Item { get; set; }

        [Required]
        public virtual ProductUnit Unit { get; set; }
        [Required]
        public decimal Allocated { get; set; }
        [Required]
        public decimal Used { get; set; }
        public string Note { get; set; }
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public virtual ApplicationUser LastUpdatedBy { get; set; }
    }
}
