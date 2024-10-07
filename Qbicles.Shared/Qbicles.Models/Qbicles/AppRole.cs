using CleanBooksData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_DomainRole")]
    public class DomainRole
    {
        public int Id { get; set; }

        public String Name { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual QbicleDomain Domain { get; set; }

        public virtual List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual List<task> tasks { get; set; } = new List<task>();
        public virtual List<Account> accounts { get; set; } = new List<Account>();
    }
}