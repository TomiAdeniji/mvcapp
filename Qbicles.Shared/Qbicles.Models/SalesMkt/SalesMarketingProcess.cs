using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_process")]
    public class SalesMarketingProcess
    { 
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual List<SalesMarketingWorkGroup> WorkGroups { get; set; }
    }
}
