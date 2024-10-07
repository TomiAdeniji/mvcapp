namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingtask")]
    public partial class transactionmatchingtask
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionmatchingtask()
        {
            transactionmatchingrecords = new HashSet<transactionmatchingrecord>();
        }

        public int Id { get; set; }

        public int TaskInstanceId { get; set; }

        public virtual taskinstance taskinstance { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingrecord> transactionmatchingrecords { get; set; }
    }
}
