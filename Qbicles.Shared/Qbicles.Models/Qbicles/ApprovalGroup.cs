using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_ApprovalGroup")]
    public class ApprovalGroup : AppGroup
    {
        public virtual List<ApprovalRequestDefinition> Approvals { get; set; } = new List<ApprovalRequestDefinition>();
    }
}