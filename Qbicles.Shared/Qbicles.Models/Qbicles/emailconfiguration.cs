using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_EmailConfiguration")]
    public class EmailConfiguration
    {
        [Key]
        public int Id { get; set; }

        [StringLength(256)]
        public string UserName { get; set; }

        [Column(TypeName = "bit")]
        public bool IsSES { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(256)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string SmtpServer { get; set; }

        public int Port { get; set; }

        public int Timeout { get; set; }

    }
}