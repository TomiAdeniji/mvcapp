using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using static Qbicles.Models.QbicleDomain;

namespace Qbicles.Models
{
    [Table("qb_DomainRequest")]
    public class QbicleDomainRequest : DataModelBase
    {
        public string DomainRequestJSON { get; set; }
        public string QbicleRequestJSON { get; set; }
        public DomainRequestStatus Status { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser AcceptOrRejectBy { get; set; }
        public DateTime AcceptOrRejectDate { get; set; }
        public DomainTypeEnum DomainType { get; set; }
        public virtual QbicleDomain ExistingDomain { get; set; }
        public string RequestedName { get; set; }
        public virtual DomainPlan DomainPlan { get; set; }
        public string SubAccountInformationJSON { get; set; }
    }

    public enum DomainRequestStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3
    }

    public class DomainRequest
    {
        public string Name { get; set; }
        public string LogoUri { get; set; }
        public string Description { get; set; }
    }

    public class InitialQbicleRequest
    {
        public string Name { get; set; }
        public string LogoUri { get; set; }
        public string Description { get; set; }
    }

    public class SubAccountInformationModel
    {
        public string BusinessName { get; set; }
        public string BankCode { get; set; }
        public string AccountNumber { get; set; }
    }
}
