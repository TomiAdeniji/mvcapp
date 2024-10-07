using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using static Qbicles.Models.QbicleDomain;

namespace Qbicles.Models
{
    [Table("qb_Subscription")]
    public class Subscription : DataModelBase
    {
        public string PayStackAuthorization { get; set; }
        public string PayStackEmailCode { get; set; }
        public string PayStackSubscriptionCode { get; set; }
        public DateTime StartDate { get; set; }
        public virtual DomainPlan Plan { get; set; }
        public DomainSubscriptionStatus Status { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Will be get on getting subscription information from paystack
        /// Need for the cases that customer change from paid plan -> free plan -> paid plan, need to know the next payment date for the newest subscription
        /// </summary>
        public DateTime NexPaymentDate { get; set; }
    }

    public enum DomainSubscriptionStatus
    {
        Valid = 1,
        InValid = 2
    }
}
