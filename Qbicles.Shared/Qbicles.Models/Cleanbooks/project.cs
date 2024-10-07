namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_project")]
    public partial class project
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public project()
        {
            projecttaskxrefs = new HashSet<projecttaskxref>();
        }

        public int Id { get; set; }

        [StringLength(45)]
        public string Name { get; set; }

        [StringLength(5000)]
        public string Description { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedById { get; set; }

        public int ProjectNotificationIntervalId { get; set; }

        public int? AssignedAccountGroupId { get; set; }

        public int ProjectGroupId { get; set; }

        public virtual accountgroup accountgroup { get; set; }

        public virtual projectgroup projectgroup { get; set; }

        public virtual projectnotificationinterval projectnotificationinterval { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<projecttaskxref> projecttaskxrefs { get; set; }
    }
}
