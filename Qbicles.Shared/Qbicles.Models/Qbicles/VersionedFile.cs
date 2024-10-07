using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_versionedfile")]
    public class VersionedFile
    {
        public int Id { get; set; }
        public string Uri { get; set; }
        public string FileSize { get; set; }

        [Column(TypeName = "bit")]
        public bool IsDeleted { get; set; } = false;
        public ApplicationUser UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; }
        [Required]
        public virtual QbicleMedia Media { get; set; }

        public virtual QbicleFileType FileType { get; set; }
    }
}
