using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_timespent")]
    public class QbicleTimeSpent
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public short Days { get; set; }

        public short Hours { get; set; }

        public short Minutes { get; set; }

        public virtual QbicleTask Task { get; set; }
    }
}