using Qbicles.Models.Form;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Operator.Compliance
{
    /// <summary>
    /// This class creates a collection of Forms associated with a praticular Domain
    /// This inherits from DomainForm.
    /// </summary>
    [Table("compliance_forms")]
    public class ComplianceForms : DomainForm
    {
        public virtual List<FormDefinition> Forms { get; set; }
    }
}
