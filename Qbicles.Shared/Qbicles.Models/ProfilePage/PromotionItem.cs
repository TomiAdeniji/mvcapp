using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.ProfilePage
{
    [Table("profile_promotionitem")]
    public class PromotionItem
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Icon { get; set; }
        [Required]
        [StringLength(50)]
        public string Colour { get; set; }
        [StringLength(250)]
        public string Text { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
        [Required]
        [JsonIgnore]
        public virtual Promotion Promotion { get; set; }
    }
}
