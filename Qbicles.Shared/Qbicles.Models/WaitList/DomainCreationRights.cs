using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.WaitList
{
    [Table("system_domaincreationrights")]
    public class DomainCreationRights : DataModelBase
    {

        [Required]
        public virtual ApplicationUser AssociatedUser { get; set; }

        //[Required]
        //public virtual WaitListRequest AssociatedWaitlistRequest { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool IsApprovedForSubsDomain { get; set; } = false;

        [Required]
        [Column(TypeName = "bit")]
        public bool IsApprovedForCustomDomain { get; set; } = false;

        [Required]
        public DateTime CreatedDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public virtual ApplicationUser LastModifiedBy { get; set; }

    }
}
