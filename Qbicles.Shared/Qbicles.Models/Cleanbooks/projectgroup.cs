namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_projectgroup")]
    public partial class projectgroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public projectgroup()
        {
            projects = new HashSet<project>();
        }

        public int Id { get; set; }

        [StringLength(45)]
        public string Name { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedById { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<project> projects { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }
    }
}
