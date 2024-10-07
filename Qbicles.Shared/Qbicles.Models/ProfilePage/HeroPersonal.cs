using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.ProfilePage
{
    public class HeroPersonal : Hero
    {
        public HeroPersonal()
        {
            this.Type = BusinessPageBlockType.HeroPersonal;
        }

        [Column(TypeName = "bit")]
        public bool HeroIsIncludeButton { get; set; }

        [StringLength(50)]
        public string HeroButtonText { get; set; }

        [StringLength(50)]
        public string HeroButtonColour { get; set; }
        public string HeroButtonJumpTo { get; set; }

        [StringLength(250)]
        public string HeroExternalLink { get; set; }
    }
}
