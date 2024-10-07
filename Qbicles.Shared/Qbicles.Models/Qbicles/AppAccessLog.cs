using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_appaccesslog")]
    public class AppAccessLog : QbicleLog
    {
        public AppAccessLog(string userId, AppType type, int domainId) : base(QbicleLogType.AppAccess, userId)
        {
            Type = type;
            DomainId = domainId;
        }

        [Required]
        public AppType Type { get; set; }

        [Required]
        public int DomainId { get; set; }
    }

    public enum AppType
    {
        Cleanbooks = 1,
        Bookkeeping = 2,
        Trader = 3,
        SalesMarketing = 4,
        Spannered = 5,
        Core = 6,
        Micro = 7,
        Web = 8,
    }
}