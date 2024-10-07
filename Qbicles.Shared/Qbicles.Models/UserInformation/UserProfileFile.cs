using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.UserInformation
{
    [Table("user_profilefile")]
    public class UserProfileFile : DataModelBase
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string StoredFileName { get; set; }

        public string Description { get; set; }

        [Required]
        public virtual QbicleFileType FileType { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }


        [Required]
        public virtual ApplicationUser AssociatedUser { get; set; }


    }
}