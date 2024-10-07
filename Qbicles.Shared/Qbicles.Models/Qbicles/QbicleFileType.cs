using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_qbiclefiletype")]
    public partial class QbicleFileType
    {
        [Key]
        public string Extension { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string ImgPath { get; set; }

        [Required]
        public string IconPath { get; set; }
        [Required]
        public decimal MaxFileSize { get; set; }
    }
}
