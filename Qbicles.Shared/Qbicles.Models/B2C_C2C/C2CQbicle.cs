using System.Collections.Generic;

namespace Qbicles.Models.B2C_C2C
{
    /// <summary>
    /// This is a QBicle that is used only in THE SystemDomain with a type of C2C.
    /// 
    /// A Qbicle in THE C2C SystemDomain must reference two ApplicationUsers i.e. the two Customers in the C2C relationship
    /// </summary>
    public class C2CQbicle : CQbicle
    {
        
        /// <summary>
        /// This collection will have two Customers (ApplicationUsers) only
        /// </summary>
        public virtual List<ApplicationUser> Customers { get; set; } = new List<ApplicationUser>();

        /// <summary>
        /// The user who initiated the relationship
        /// </summary>
        public virtual ApplicationUser Source { get; set; }

        /// <summary>
        /// Contains Users in this relationship (max at 2) who has already viewed all things in the C2C talk
        /// </summary>
        public List<ApplicationUser> NotViewedBy { get; set; }
    }
}
