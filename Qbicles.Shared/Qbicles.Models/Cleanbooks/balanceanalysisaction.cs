namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_balanceanalysisaction")]
    public partial class balanceanalysisaction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public balanceanalysisaction()
        {
            balanceanalysismatchs = new HashSet<balanceanalysismatch>();

        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int TaskId { get; set; }       
        public virtual task tasks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysismatch> balanceanalysismatchs { get; set; }
    }
}
