using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_AppGroup")]
    public abstract class AppGroup
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual AppInstance AppInstance { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}