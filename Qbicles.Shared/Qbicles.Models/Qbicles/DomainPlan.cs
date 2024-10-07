using Qbicles.Models.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_DomainPlan")]
    public class DomainPlan : DataModelBase
    {
        public decimal ActualCost { get; set; }
        public decimal CalculatedCost { get; set; }
        public int NumberOfExtraUsers { get; set; }
        public string PayStackPlanCreationResponse { get; set; }
        public string PayStackPlanCode { get; set; }
        public string PayStackPlanName { get; set; }
        public virtual QbicleDomain Domain { get; set; }
        public virtual BusinessDomainLevel Level { get; set; }
        public string InitTransactionResponseJSON { get; set; }
        [Column(TypeName = "bit")]
        public bool IsArchived { get; set; }
        public DateTime? ArchivedDate { get; set; }
        public ApplicationUser ArchivedBy { get; set; }

        public string TrialEndNotiHangfireJobId { get; set; } = string.Empty;

        public string SubPaymnetDateNotiHangfireJobId { get; set; } = string.Empty;
    }
}
