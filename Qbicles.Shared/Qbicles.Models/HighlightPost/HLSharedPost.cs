using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Highlight
{
    [Table("hl_sharedpost")]
    public class HLSharedPost : QbicleActivity
    {
        public virtual HighlightPost SharedPost { get; set; }
        public virtual ApplicationUser SharedWith { get; set; }
        public virtual ApplicationUser SharedBy { get; set; }
        public DateTime ShareDate { get; set; }
        public string SharedWithEmail { get; set; }
    }
}
