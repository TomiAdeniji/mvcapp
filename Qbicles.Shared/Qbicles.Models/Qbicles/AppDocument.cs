using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_appdocument")]
    public abstract class AppDocument
    {

        public int Id { get; set; }

        public string Document { get; set; }

        public string AppDocumentImage { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate{ get; set; }
        
        public virtual QbicleFileType FileType { get; set; }

    }
}