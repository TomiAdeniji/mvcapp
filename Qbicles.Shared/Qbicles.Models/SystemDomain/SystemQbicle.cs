using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SystemDomain
{
    /// <summary>
    /// This is a QBicle that is used only in THE SystemDomain with a type of B2B.
    /// A Qbicle in THE B2B SystemDomain must reference two ordinary Domains, not SystemDomains
    /// </summary>
    public class B2BQbicle : Qbicle
    {
        /// <summary>
        /// This collection will have two Domains only
        /// </summary>
        public virtual List<QbicleDomain> Domains { get; set; } = new List<QbicleDomain>();
    }
}
