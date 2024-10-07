using Qbicles.Models.Attributes;
using Qbicles.Models.Trader.PoS;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.ODS
{
    [Table("ods_OrderPayment")]
    public class OrderPayment
    {
        public int Id { get; set; }

        [Required]
        public virtual QueueOrder AssociatedOrder { get; set; }

        [Required]
        public virtual PosPaymentMethodAccountXref MethodAccountXref { get; set; }

        [DecimalPrecision(10, 3)]
        [Required]
        public decimal AmountTendered { get; set; }

        public string Reference { get; set; }
        public decimal AmountAccepted { get; set; }
    }
}
