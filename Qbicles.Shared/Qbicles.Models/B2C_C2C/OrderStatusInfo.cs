using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.B2C_C2C
{
    public class OrderStatusInfo
    {
        public int Id { get; set; }

        public virtual TradeOrder TradeOrder { get; set; }

        public int NumberOfDocuments { get; set; }


        public int NumberOfItems { get; set; }

        [Column(TypeName = "bit")]
        public bool IsPaymentComplete { get; set; } = false;

        [Column(TypeName = "bit")]
        public bool IsInTransit{ get; set; }

        public DateTime ETA { get; set; }
    }
}
