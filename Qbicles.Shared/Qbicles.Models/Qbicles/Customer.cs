using Qbicles.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models
{
    [Table("qb_Customer")]
    public class Customer : DataModelBase
    {
        public decimal PaystackCustomerId { get; set; }
        public string PaystackEmail { get; set; }
        public string PaystackFirstName { get; set; }
        public string PaystackLastName { get; set; }
        public string CustomerCode { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual List<Subscription> Subscriptions { get; set; }
    }
}
