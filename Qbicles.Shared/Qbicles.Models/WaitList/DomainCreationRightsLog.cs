using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.WaitList
{
    [Table("system_domaincreationrightslog")]
    public class DomainCreationRightsLog : DataModelBase
    {
        [Required]
        public int AssociatedDomainCreationRightsId { get; set; }

        //[Required]
        public virtual WaitListRequest AssociatedWaitListRequest { get; set; }

        [Required]
        public string AssociatedUserId { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool IsApprovedForSubsDomain { get; set; } = false;
        /// <summary>
        /// Sate of current approval
        /// </summary>
        [Required]
        [Column(TypeName = "bit")]
        public bool IsApprovedForSubsDomainLog { get; set; } = false;

        [Required]
        [Column(TypeName = "bit")]
        public bool IsApprovedForCustomDomain { get; set; } = false;
        /// <summary>
        /// Sate of current approval
        /// </summary>
        [Required]
        [Column(TypeName = "bit")]
        public bool IsApprovedForCustomDomainLog { get; set; } = false;
        /// <summary>
        /// Sate of current approval
        /// </summary>
        [Required]
        [Column(TypeName = "bit")]
        public bool IsRejected { get; set; } = false;

        [Required]
        [Column(TypeName = "bit")]
        public bool IsRejectedLog { get; set; } = false;

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime LastModifiedDate { get; set; }

        [Required]
        public virtual ApplicationUser LastModifiedBy { get; set; }

    }
}
