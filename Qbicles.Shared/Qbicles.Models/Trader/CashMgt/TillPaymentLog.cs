using Qbicles.Models.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Qbicles.Models.Trader.CashMgt.TillPayment;

namespace Qbicles.Models.Trader.CashMgt
{
    [Table("trad_tillpaymentlog")]
    public class TillPaymentLog
    {
        public int Id { get; set; }

        public virtual TillPayment AssociatedTillPayment { get; set; }

        [Required]
        public TillPaymentDirection Direction { get; set; }

        [Required]
        public decimal Amount { get; set; }


        [Required]
        public DateTime CreatedDate { get; set; }


        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }


        [Required]
        public virtual Till AssociatedTill { get; set; }

        [Required]
        public virtual Safe AssociatedSafe { get; set; }

        public virtual ApprovalReq Approval { get; set; }
    }
}
