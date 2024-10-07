using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_Approval")]
    public class Approval : AppInstance
    {
       
        public virtual List<ApprovalGroup> Groups { get; set; } = new List<ApprovalGroup>();
    }
}