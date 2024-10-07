using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_domainaccesslog")]
    public class DomainAccessLog : QbicleLog
    {
        public DomainAccessLog(string userId, int domainId) : base(QbicleLogType.DomainAccess, userId)
        {
            DomainId = domainId;
        }

        [Required]
        public int DomainId { get; set; }
    }
}