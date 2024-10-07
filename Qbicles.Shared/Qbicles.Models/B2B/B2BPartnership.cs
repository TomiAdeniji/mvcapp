using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qbicles.Models.Base;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.PoS;

namespace Qbicles.Models.B2B
{
    [Table("b2b_Partnership")]
    public class Partnership: DataModelBase
    {
        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        [Required]
        public DateTime LastUpdatedDate { get; set; }

        public B2BService Type { get; set; }

        
        public virtual B2BQbicle CommunicationQbicle { get; set; }

        [Required]
        public int B2BRelationship_Id { get; set; }

        [Required]
        [ForeignKey("B2BRelationship_Id")]
        public virtual B2BRelationship ParentRelationship { get; set; }

        public virtual B2BPartnershipDiscussion PartnershipDiscussion { get; set; }
       
        public virtual QbicleDomain ProviderDomain { get; set; }

        public virtual TraderCashAccount ProviderPaymentAccount { get; set; }

        public virtual List<ApplicationUser> ProviderPartnershipManagers { get; set; } = new List<ApplicationUser>();

        [Column(TypeName = "bit")]
        public bool IsProviderConfirmed { get; set; }

        public virtual QbicleDomain ConsumerDomain { get; set; }

        public virtual TraderCashAccount ConsumerPaymentAccount { get; set; }

        public virtual List<ApplicationUser> ConsumerPartnershipManagers { get; set; } = new List<ApplicationUser>();

        [Column(TypeName = "bit")]
        public bool IsConsumerConfirmed { get; set; }
    }

    public enum B2BService
    {
        Logistics = 1,
        Maintenance = 2,
        Products = 3
    }
}
