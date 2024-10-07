using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_FormManager")]
    public class FormManager
    {
        [Required]
        public int Id { get; set; }
        /// <summary>
        ///  a property to link to the ApplicationUser who is allowed to manage a FormDefinition for the associated Domain
        /// </summary>
        [Required]
        public virtual ApplicationUser User { get; set; }
        /// <summary>
        /// a property to link to the Domain for which the associated user can manage Task Forms
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }


        [Column(TypeName = "bit")]
        public bool Manage { get; set; } = false;


        [Column(TypeName = "bit")]
        public bool QueryOrReport { get; set; } = false;
    }
}
