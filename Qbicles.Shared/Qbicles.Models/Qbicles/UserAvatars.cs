using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models
{
    public class UserAvatars
    {
        public int Id { get; set; }
        [Column(TypeName = "bit")]
        public bool isDefault { get; set; }
        [StringLength(256)]
        public string URI { get; set; }
        [StringLength(500)]
        public string AvatarName { get; set; }
        public DateTime CreateDate { get; set; }
        [Required]
        public ApplicationUser User { get; set; }
    }
}
