using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_RoleRightAppXref")]
    public class RoleRightAppXref
    {
        public int Id { get; set; }

        public virtual DomainRole Role { get; set; }

        public virtual AppInstance AppInstance { get; set; }

        public virtual AppRight Right { get; set; }
    }
}