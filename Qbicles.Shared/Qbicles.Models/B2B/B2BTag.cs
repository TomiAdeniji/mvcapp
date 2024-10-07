using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2B
{
    [Table("b2b_tags")]
    public class B2BTag
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string TagName { get; set; }
        [Required]
        public virtual B2BProfile B2BProfile { get; set; }
    }
}
