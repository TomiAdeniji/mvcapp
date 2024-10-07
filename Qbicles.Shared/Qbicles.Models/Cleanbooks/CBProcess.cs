using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CleanBooksData
{
    [Table("cb_process")]
    public class CBProcess
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual List<CBWorkGroup> WorkGroups {get; set;}
    }
}
