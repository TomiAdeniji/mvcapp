using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Invitation
{
    [Table("qb_Invitation")]
    public class Invitation
    {
        public int Id { get; set; }

        [Required]
        //[Encrypted]
        public string Email { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public ApplicationUser CreatedBy { get; set; }

        [Required]
        public  QbicleDomain Domain { get; set; }

        public DateTime LastUpdateDate { get; set; }

        public  ApplicationUser LastUpdatedBy { get; set; }

        [Required]
        public InvitationStatusEnum Status { get; set; }

        public  List<InvitationSentLog> Log { get; set; } = new List<InvitationSentLog>();

        public string Note { get; set; }

    }

    public enum InvitationStatusEnum
    {
        Pending = 1,
        Accepted = 2,  // By Invitee
        Rejected = 3,  // By Invitee
        Discarded = 4  // By Domain Administrator
    }
}
