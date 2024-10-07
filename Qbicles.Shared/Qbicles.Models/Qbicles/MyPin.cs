using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_MyPin")]
    public class MyPin
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual MyDesk Desk { get; set; }

        public virtual QbicleActivity PinnedActivity { get; set; }

        public virtual QbiclePost PinnerPost { get; set; }
    }
}
