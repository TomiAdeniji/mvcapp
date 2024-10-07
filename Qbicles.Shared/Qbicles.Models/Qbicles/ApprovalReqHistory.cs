using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    /// <summary>
    /// This class is used to record the history of updates to the associated Approval Request
    /// </summary>
    [Table("qb_approvalreqhistories")]
    public class ApprovalReqHistory
    {
        /// <summary>
        /// This is the unique identifier for the Log
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// This is the associated Approval
        /// </summary>
        [Required]
        public virtual ApprovalReq ApprovalReq { get; set; }

        /// <summary>
        /// This is the status of the ApprovalRequest at the time the Approval Request was updated
        /// </summary>
        [Required]
        public ApprovalReq.RequestStatusEnum RequestStatus { get; set; } = ApprovalReq.RequestStatusEnum.Pending;


        /// <summary>
        /// This is the CURRENT USER at the time the ApprovalRequest was updated i.e. th eperson responsible for the update
        /// </summary>
        [Required]
        public virtual ApplicationUser UpdatedBy { get; set; }


        /// <summary>
        /// This is the date and time at which the update occurred.
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

    }
}
