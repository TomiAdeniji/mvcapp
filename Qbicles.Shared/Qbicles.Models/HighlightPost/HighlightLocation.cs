using Qbicles.Models.Qbicles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Highlight
{
    [Table("hl_location")]
    public class HighlightLocation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Country Country { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
}
