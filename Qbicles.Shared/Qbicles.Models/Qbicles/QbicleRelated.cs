using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Qbicles.Models
{
    [Table("qb_related")]
    public class QbicleRelated
    {
        public QbicleRelated()
        {
        }

        public int Id { get; set; }

        public virtual QbicleSet AssociatedSet { get; set; }

        public virtual QbicleActivity Activity { get; set; }
    }
}