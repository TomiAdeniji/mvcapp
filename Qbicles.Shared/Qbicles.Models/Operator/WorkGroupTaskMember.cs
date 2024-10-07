using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Operator
{
    [Table("op_wgtaskmembers")]
    public class WorkGroupTaskMember
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual OperatorWorkGroup WorkGroup { get; set; }

        [Required]
        public virtual ApplicationUser Member { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool IsTaskCreator { get; set; }
    }
}
