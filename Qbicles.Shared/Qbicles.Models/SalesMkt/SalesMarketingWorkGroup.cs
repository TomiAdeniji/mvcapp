using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.SalesMkt
{
    [Table("sm_workgroup")]
    public class SalesMarketingWorkGroup
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual QbicleDomain Domain { get; set; }

        [Required]
        public virtual List<SalesMarketingProcess> Processes { get; set; } = new List<SalesMarketingProcess>();

        public virtual List<ApplicationUser> Members { get; set; } = new List<ApplicationUser>();

        public virtual List<ApplicationUser> ReviewersApprovers { get; set; } = new List<ApplicationUser>();

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public virtual ApplicationUser CreatedBy { get; set; }
    }
}
