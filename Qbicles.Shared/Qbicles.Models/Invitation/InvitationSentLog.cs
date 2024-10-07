using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Invitation
{
    [Table("qb_InvitationSentLog")]
    public class InvitationSentLog
   {
       public int Id { get; set; }

       [Required]
       public DateTime CreatedDate { get; set; }

       [Required]
       public virtual ApplicationUser CreatedBy { get; set; }

       [Required]
       public virtual Invitation Invitation { get; set; }
   }
}
