using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_qbicleaccesslog")]
    public class QbicleAccessLog : QbicleLog
    {
        public QbicleAccessLog(string userId, int qbicleId, int domainId) : base(QbicleLogType.QbicleAccess, userId)
        {
            QbicleId = qbicleId;
            DomainId = domainId;
        }

        [Required]
        public int QbicleId { get; set; }

        //[Required]
        public int? DomainId { get; set; }
    }
}