using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_set")]
    public class QbicleSet
    {
        public QbicleSet()
        {
            Peoples = new HashSet<QbiclePeople>();
            Relateds = new HashSet<QbicleRelated>();
            Activities = new HashSet<QbicleActivity>();
            QbiclePosts = new HashSet<QbiclePost>();
        }
        public int Id { get; set; }

        public virtual ICollection<QbiclePeople> Peoples { get; set; }

        public virtual ICollection<QbicleActivity> Activities { get; set; }

        public virtual QbicleRecurrance Recurrance { get; set; }

        public virtual ICollection<QbicleRelated> Relateds { get; set; }

        public virtual ICollection<QbiclePost> QbiclePosts { get; set; }
    }
}