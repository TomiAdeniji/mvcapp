using System.Collections.Generic;

namespace Qbicles.Models.B2B
{
    public class LogisticsPartnership : Partnership
    {
        public LogisticsPartnership()
        {
            this.Type = B2BService.Logistics;
        }
        public virtual List<B2BLogisticsAgreement> LogisticsAgreements { get; set; } = new List<B2BLogisticsAgreement>();
    }
}
