using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Network
{
    [Table("sl_group")]
    public class ShortListGroup
    {
        [Required]
        public int Id { get; set; }
        public string IconUri { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        [Required]
        public virtual QbicleDomain AssociatedDomain { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser AssociatedUser { get; set; }
        public virtual List<ApplicationUser> Candidates { get; set; }
    }
}
