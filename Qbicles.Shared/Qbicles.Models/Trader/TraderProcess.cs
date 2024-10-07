using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader
{
    [Table("trad_process")]
    public class TraderProcess
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public virtual List<WorkGroup> WorkGroups {get; set;}
    }
}
