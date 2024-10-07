namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcautoexecute")]
    public partial class fcautoexecute
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcautoexecute()
        {
            financialcontrolreportdefinitions = new HashSet<financialcontrolreportdefinition>();
        }

        public int Id { get; set; } 
        [Required]        
        public string Option { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolreportdefinition> financialcontrolreportdefinitions { get; set; }
    }
}
