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
    [Table("profile_featureitem")]
    public class FeatureItem
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        public string FeatureTitle { get; set; }
        [Required]
        [StringLength(500)]
        public string FeatureContent { get; set; }
        [Required]
        [StringLength(150)]
        public string FeatureIcon { get; set; }
        [Required]
        [StringLength(50)]
        public string FeatureColour { get; set; }
        [Required]
        [JsonIgnore]
        public FeatureList FeatureParent { get; set; }
        [Required]
        public int DisplayOrder { get; set; }
    }
}
