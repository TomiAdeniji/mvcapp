namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionanalysisreportgroup")]
    public partial class transactionanalysisreportgroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionanalysisreportgroup()
        {
            transactionanalysisclassificationbydates = new HashSet<transactionanalysisclassificationbydate>();
            transactionanalysisclassificationbyranges = new HashSet<transactionanalysisclassificationbyrange>();
            transactionanalysisclassificationbytypes = new HashSet<transactionanalysisclassificationbytype>();
            transactionanalysiscomments = new HashSet<transactionanalysiscomment>();
            transactionanalysisreportstatistics = new HashSet<transactionanalysisreportstatistic>();
        }

        public int Id { get; set; }



        [Column(TypeName = "bit")]
        public bool? IsProfile { get; set; }

        [StringLength(1073741823)]
        public string PanelTitle { get; set; }

        public int TransactionAnalysisTaskId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysisclassificationbydate> transactionanalysisclassificationbydates { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysisclassificationbyrange> transactionanalysisclassificationbyranges { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysisclassificationbytype> transactionanalysisclassificationbytypes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysiscomment> transactionanalysiscomments { get; set; }

        public virtual transactionanalysistask transactionanalysistask { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysisreportstatistic> transactionanalysisreportstatistics { get; set; }
    }
}
