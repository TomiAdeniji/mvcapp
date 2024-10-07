using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    public class NotificationConnection
    {
        [Required]
        public int Id { get; set; }
        public string ConnectionID { get; set; }

        [Column(TypeName = "bit")]
        public bool Connected { get; set; }
    }
}
