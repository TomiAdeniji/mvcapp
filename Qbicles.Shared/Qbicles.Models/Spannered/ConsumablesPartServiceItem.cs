using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Qbicles.Models.Spannered.AssetInventory;

namespace Qbicles.Models.Spannered
{
    [Table("sp_consumablepartserviceptems")]
    public class ConsumablesPartServiceItem
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        /// This is the Qbicle Task with this ConsumablesPartServiceItem is associated.
        /// </summary>
        [Required]
        public virtual QbicleTask QbicleTask { get; set; }
        [Required]
        public virtual AssetInventory AssetInventory { get; set; }
        [Required]
        public decimal Allocated { get; set; }
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }
        /// <summary>
        /// This is the date and time on which this spannered ConsumablesPartServiceItem was created
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
