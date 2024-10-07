namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_profile")]
    public partial class profile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public profile()
        {
            transactionanalysistaskprofilexrefs = new HashSet<transactionanalysistaskprofilexref>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(45)]
        public string Name { get; set; }

        [Required]
        [StringLength(45)]
        public string Flag { get; set; }

        [StringLength(3000)]
        public string Expression { get; set; }

        [StringLength(128)]
        public string CreatedById { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(128)]
        public string ModifiedById { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysistaskprofilexref> transactionanalysistaskprofilexrefs { get; set; }

        public virtual Qbicles.Models.ApplicationUser user1 { get; set; }
    }
}
