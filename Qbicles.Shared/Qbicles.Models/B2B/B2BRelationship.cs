using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.B2B
{
    [Table("b2b_relationships")]
    public class B2BRelationship
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        [Required]
        public DateTime LastUpdatedDate { get; set; }


        public virtual B2BQbicle CommunicationQbicle { get; set; }
        public virtual List<Partnership> Partnerships { get; set; } = new List<Partnership>();
        
        [Required]
        public virtual QbicleDomain Domain1 { get; set; }

        public virtual TraderContact Domain1TraderContactForDomain2 { get; set; }

        [Required]
        public virtual QbicleDomain Domain2 { get; set; }
        public virtual TraderContact Domain2TraderContactForDomain1 { get; set; }

    }
    
}
