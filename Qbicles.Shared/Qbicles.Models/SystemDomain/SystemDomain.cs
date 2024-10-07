using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.SystemDomain
{
    /// <summary>
    /// System Domains are used created by the System.
    /// Are only accessed by System Administrators
    /// </summary>
    public class SystemDomain : QbicleDomain
    {

        /// <summary>
        /// This sets the type of the system domain
        /// </summary>
        [Required]
        public SystemDomainType Type { get; set; }
    }


    public enum SystemDomainType
    {
        B2B = 1,
        B2C = 2,
        C2C = 3
    }
}
