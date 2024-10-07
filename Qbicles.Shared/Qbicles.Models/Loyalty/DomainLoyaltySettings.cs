using Qbicles.Models.Trader;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Loyalty
{
    [Table("loy_DomainSettings")]
    public class DomainLoyaltySettings
    {
        public int Id { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        [Column(TypeName = "bit")]
        public bool IsPaymentWithStoreCreditActive { get; set; }


        /// <summary>
        /// This workgroup is used to control how the AUTOMATIC approval for the 
        /// conversion of TraderContact balance to StoreCredit by using a Credit note of type Debit i.e. debiting the TraderContact's balance
        /// </summary>
        public virtual WorkGroup DebitProcessWorkGroup { get; set; }

        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
    }
}
