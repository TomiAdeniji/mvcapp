using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Form
{
    /// <summary>
    /// This allows many different types of forms to be associated with a Domain
    /// Currently it is used for ComplianceForms only, but could be extended to Skills forms etc
    /// </summary>
    [Table("form_domain")]
    public class DomainForm
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// The Domain with which the form is associated.
        /// </summary>
        [Required]
        public virtual QbicleDomain Domain { get; set; }
    }
}
