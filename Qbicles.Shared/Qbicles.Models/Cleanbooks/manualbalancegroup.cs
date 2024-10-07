namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_manualbalancegroup")]
    public partial class manualbalancegroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public manualbalancegroup()
        {
            manualbalances = new HashSet<manualbalance>();
        }

        public int Id { get; set; }
        public string Name { get; set; }       
       
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<manualbalance> manualbalances { get; set; }
    }
}
