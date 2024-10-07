namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingmethod")]
    public partial class transactionmatchingmethod
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionmatchingmethod()
        {
            transactionmatchinggroups = new HashSet<transactionmatchinggroup>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(45)]
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchinggroup> transactionmatchinggroups { get; set; }
    }
}
