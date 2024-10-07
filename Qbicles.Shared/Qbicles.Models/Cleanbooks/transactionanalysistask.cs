namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionanalysistask")]
    public partial class transactionanalysistask
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionanalysistask()
        {
            transactionanalysisrecords = new HashSet<transactionanalysisrecord>();
            transactionanalysisreportgroups = new HashSet<transactionanalysisreportgroup>();
            transactionanalysistaskprofilexrefs = new HashSet<transactionanalysistaskprofilexref>();
        }

        public int Id { get; set; }

        public int TaskInstanceId { get; set; }

        public virtual taskinstance taskinstance { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysisrecord> transactionanalysisrecords { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysisreportgroup> transactionanalysisreportgroups { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysistaskprofilexref> transactionanalysistaskprofilexrefs { get; set; }
    }
}
