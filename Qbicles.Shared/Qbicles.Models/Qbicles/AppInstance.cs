using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_appinstances")]
    public class AppInstance
    {
        public int Id { get; set; }

        public virtual QbicleDomain Domain { get; set; }

        public virtual QbicleApplication QbicleApplication { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public virtual DateTime CreatedDate { get; set; }

    }
}