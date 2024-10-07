using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2B
{
    [Table("b2b_invitations")]
    public class B2BInvitation
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        [Required]
        public DateTime LastUpdatedDate { get; set; }

        [Required]
        public virtual B2BRelationship Relationship { get; set; }

        [Required]
        public virtual InvitationTypeEnum InvitationType { get; set; }

        [Required]
        public virtual InvitationStatus Status { get; set; }


        public virtual QbicleDomain SentDomain { get; set; }

        
        public virtual QbicleDomain ReceivedDomain { get; set; }
    }
    public enum InvitationTypeEnum
    {
        [Description("Relationship")]
        Relationship = 1,
        [Description("Logistics")]
        Logistics = 2,
        [Description("Logistics partnership")]
        LogisticsPartnership = 3
    }
    public enum InvitationStatus
    {
        Pending = 1,
        Accepted = 2,
        Rejected = 3
    }
}
