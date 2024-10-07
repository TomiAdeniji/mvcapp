using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.B2C_C2C
{

    /// <summary>
    /// This is a QBicle that is used only in THE SystemDomain with a type of B2C.
    /// 
    /// A Qbicle in THE B2C SystemDomain must reference a Domain (Business) and an ApplicationUser (Customer)
    /// </summary>
    public class B2CQbicle : CQbicle
    {
        /// <summary>
        /// This is the Business Domain in the relationship
        /// </summary>
        public virtual QbicleDomain Business { get; set; }

        /// <summary>
        /// This is the Customer in the relationship
        /// </summary>
        public virtual ApplicationUser Customer { get; set; }


        /// <summary>
        /// If the status of the relation ship is Blocked, if it was blocked by the doamin this is set  
        /// </summary>
        public virtual QbicleDomain BlockerDomain { get; set; }

        [Column(TypeName = "bit")]
        public bool? IsNewContact { get; set; } = true;

        [Column(TypeName = "bit")]
        public bool? BusinessViewed { get; set; } = true;

        [Column(TypeName = "bit")]
        public bool? CustomerViewed { get; set; } = true;
    }
}
