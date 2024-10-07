using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_process")]
    public class BookkeepingProcess
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual List<BKWorkGroup> WorkGroups {get; set;}
    }
}
