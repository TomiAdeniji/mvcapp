using Qbicles.Models.Form;
using System.Collections.Generic;

namespace Qbicles.Models.Operator.Recruitment
{
    public class ScreeningTemplates : DomainForm
    {
        public virtual List<FormDefinition> Forms { get; set; }
    }
}
