using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_recurrance")]
    public partial class QbicleRecurrance
    {
        public QbicleRecurrance()
        {
        }

        public int Id { get; set; }

        public RecurranceTypeEnum Type { get; set; }

        public DateTime FirstOccurrence { get; set; }

        public DateTime? LastOccurrence { get; set; }

        public short Pattern { get; set; }

        [StringLength(7)]
        public string Days { get; set; }

        [StringLength(12)]
        public string Months { get; set; }
        public short MonthDate { get; set; }

        public virtual QbicleSet AssociatedSet { get; set; }

        public enum RecurranceTypeEnum
        {
            Daily = 0,
            Weekly = 1,
            Monthly = 2
        }
    }
}