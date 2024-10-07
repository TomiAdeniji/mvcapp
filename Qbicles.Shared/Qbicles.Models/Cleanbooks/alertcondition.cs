namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_alertconditions")]
    public partial class alertcondition
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public alertcondition()
        {

            singleaccountalerts = new HashSet<singleaccountalert>();
        }

        public int Id { get; set; } 
        [Required]
        [StringLength(256)]
        public string Condition { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<singleaccountalert> singleaccountalerts { get; set; }

    }
}
