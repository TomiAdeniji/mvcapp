using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community
{
    [Table("qb_talentpool")]
    public class TalentPool
    {

        [Required]
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        
        public virtual List<ApplicationUser> Contacts { get; set; } = new List<ApplicationUser>();
    }
}