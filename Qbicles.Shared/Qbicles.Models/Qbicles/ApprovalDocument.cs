using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_ApprovalDocument")]
    public class ApprovalDocument : AppDocument
    {
        public virtual ApprovalRequestDefinition Approval { get; set; }
    }
}