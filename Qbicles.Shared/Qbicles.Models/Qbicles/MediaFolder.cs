using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_mediafolder")]
    public class MediaFolder
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public virtual Qbicle Qbicle { get; set; }

        public virtual List<QbicleMedia> Media { get; set; } = new List<QbicleMedia>();

        public ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}