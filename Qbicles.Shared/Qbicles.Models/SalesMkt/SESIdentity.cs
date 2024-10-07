using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_ses_identity")]
    public class SESIdentity
    {
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        public SESIdentityStatus Status { get; set; }
        [Required]
        public QbicleDomain Domain { get; set; }
        public DateTime CreatedDate { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public string FailReason { get; set; }
    }

    public enum SESIdentityStatus
    {
        Pending = 1,
        Verified = 2,
        Failed = 3
    }
}
