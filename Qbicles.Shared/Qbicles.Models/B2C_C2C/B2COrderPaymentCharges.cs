using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2C_C2C
{
    [Table("b2c_OrderPaymentChargeSettings")]
    public class B2COrderPaymentCharges : DataModelBase
    {
        public decimal QbiclesPercentageCharge { get; set; }
        public decimal QbiclesFlatFee { get; set; }
    }
}
