namespace CleanBooksData
{
    using Qbicles.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_accountgroup")]
    public partial class accountgroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public accountgroup()
        {
            projects = new HashSet<project>();
            Accounts = new HashSet<Account>();
        }

        public int Id { get; set; }

        public int DomainId { get; set; }

        [StringLength(45)]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedById { get; set; }

        [StringLength(260)]
        public string LogoPath { get; set; }

        public virtual ApplicationUser user { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<project> projects { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Account> Accounts { get; set; }

        public virtual QbicleDomain qbicledomain { get; set; }
    }
}
