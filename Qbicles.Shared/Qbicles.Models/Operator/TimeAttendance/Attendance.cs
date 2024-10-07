using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qbicles.Models.Base;
namespace Qbicles.Models.Operator.TimeAttendance
{
    [Table("op_attendances")]
    public class Attendance: DataModelBase
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public DateTime TimeIn { get; set; }

        public DateTime? TimeOut { get; set; }

        [Required]
        public virtual OperatorWorkGroup WorkGroup { get; set; }

        public virtual ApplicationUser People { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApprovalReq ApprovalTimeIn { get; set; }

        public virtual ApprovalReq ApprovalTimeOut { get; set; }
    }
}
