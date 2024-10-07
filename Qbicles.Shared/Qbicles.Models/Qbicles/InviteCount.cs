using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models
{
    [Table("qb_InviteCount")]
    public class InviteCount
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public virtual ApplicationUser User { get; set; }
        [Required]
        public string InviteEmail { get; set; }

        public int Count { get; set; }
        [Required]
        public DateTime CountDate { get; set; }
    }
}
