namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_balanceanalysistask")]
    public partial class balanceanalysistask
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public balanceanalysistask()
        {
            balanceanalysiscomments = new HashSet<balanceanalysiscomment>();
            balanceanalysisdatasets = new HashSet<balanceanalysisdataset>();
        }

        public int Id { get; set; }    

        public int TaskInstanceId { get; set; }
        public virtual taskinstance taskinstances { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysiscomment> balanceanalysiscomments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysisdataset> balanceanalysisdatasets { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysisdocument> balanceanalysisdocuments { get; set; }

    }
}
