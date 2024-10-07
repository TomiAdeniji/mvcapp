using Qbicles.Models.B2B;
using Qbicles.Models.WaitList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("BusinessCategories")]
    public class BusinessCategory
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        public virtual List<B2BProfile> Profiles { get; set; } = new List<B2BProfile>();
        public virtual List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual List<WaitListRequest> WaitListRequests { get; set; } = new List<WaitListRequest>();

    }
}
