namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingrecord")]
    public partial class transactionmatchingrecord
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionmatchingrecord()
        {
            transactionmatchingcopiedrecords = new HashSet<transactionmatchingcopiedrecord>();
            transactionmatchingcopiedrecords1 = new HashSet<transactionmatchingcopiedrecord>();
            transactionmatchingmatcheds = new HashSet<transactionmatchingmatched>();           
        }

        public int Id { get; set; }

        public int TransactionMatchingTaskId { get; set; }

        public long TransactionId { get; set; }



        [Column(TypeName = "bit")]
        public bool? IsAccountA { get; set; }

        public virtual transaction transaction { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingcopiedrecord> transactionmatchingcopiedrecords { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingcopiedrecord> transactionmatchingcopiedrecords1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingmatched> transactionmatchingmatcheds { get; set; }

        public virtual transactionmatchingtask transactionmatchingtask { get; set; }

      
    }
}
