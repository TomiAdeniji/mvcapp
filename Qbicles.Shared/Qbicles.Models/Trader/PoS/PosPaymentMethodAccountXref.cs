using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.PoS
{
    [Table("pos_paymentmethodaccountxref")]
    public class PosPaymentMethodAccountXref
    {
        public int Id { get; set; }

        public virtual PaymentMethod PaymentMethod { get; set; }

        [Required]
        [StringLength(25)]
        public string TabletDisplayName { get; set; }

        public virtual TraderCashAccount CollectionAccount { get; set; } 

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
