using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qbicles.Models;
using Qbicles.Models.Highlight;

namespace Qbicles.Models
{
    [Table("qb_ExtensionRequest")]
    public class DomainExtensionRequest
    {
        public int Id { get; set; }
        public virtual QbicleDomain Domain { get; set; }
        public virtual ApplicationUser CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual ApplicationUser ApprovedOrRejectedBy { get; set; }
        public DateTime ApprovedOrRejectedDate { get; set; }
        public ExtensionRequestType Type { get; set; }
        public ExtensionRequestStatus Status { get; set; }
        public string Note { get; set; }
    }

    public enum ExtensionRequestType
    {
        [Description("Highlight Article")]
        Articles = 1,
        [Description("Highlight Event")]
        Events = 2,
        [Description("Highlight Job")]
        Jobs = 3,
        [Description("Highlight Knowledge")]
        Knowledge = 4,
        [Description("Highlight News")]
        News = 5,
        [Description("Highlight Real Estate")]
        RealEstate = 6,
        [Description("Bolt-on")]
        Bolton = 7
    }

    public enum ExtensionRequestStatus
    {
        None = 1,
        Pending = 2,
        Approved = 3,
        Rejected = 4
    }
}
