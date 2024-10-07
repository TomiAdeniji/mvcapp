using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Spannered
{
    [Table("sp_assetinventories")]
    public class AssetInventory
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public virtual Asset ParentAsset { get; set; }
        [Required]
        public virtual TraderItem Item { get; set; }
        [Required]
        public virtual ProductUnit Unit { get; set; }
        [Required]
        public PurposeEnum Purpose { get; set; }
        public enum PurposeEnum
        {
            [Description("Consumable")]
            Consumable =0,
            [Description("Spare part")] 
            SparePart =1,
            [Description("Service")] 
            Service =2
        }
    }
}
