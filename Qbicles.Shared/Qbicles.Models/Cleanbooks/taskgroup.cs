namespace CleanBooksData
{
    using Qbicles.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_taskgroup")]
    public partial class taskgroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public taskgroup()
        {
            tasks = new HashSet<task>();
            qbicledomain = new QbicleDomain();
        }

        public int Id { get; set; }

        [StringLength(45)]
        public string Name { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedById { get; set; }

        public DateTime? CreatedDate { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<task> tasks { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }

        public virtual QbicleDomain qbicledomain { get; set; }
    }
}
