using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Spannered
{
    [Table("sp_process")]
    public class SpanneredProcess
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual List<SpanneredWorkgroup> WorkGroups { get; set; }
    }
}
