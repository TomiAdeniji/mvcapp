using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Community
{
    [Table("com_userprofilefile")]
    public class ProfileFile
    {

        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string StoredFileName { get; set; }
        public string Description { get; set; }

        [Required]
        public virtual UserProfilePage AssociatedProfile { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        public virtual QbicleFileType FileType { get; set; }
       
    }
}
