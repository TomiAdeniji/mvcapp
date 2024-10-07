namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchinggroup")]
    public partial class transactionmatchinggroup
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionmatchinggroup()
        {
            transactionmatchingmatcheds = new HashSet<transactionmatchingmatched>();
        }

        public int Id { get; set; }

        public int TransactionMatchMethodID { get; set; }

        public int TransactionMatchRelationshipId { get; set; }



        [Column(TypeName = "bit")]
        public bool? IsPartialMatch { get; set; }
        public int? IsDateVarianceUsed { get; set; }
        public int? DateVarianceValue { get; set; }
        public int? IsAmount1VarianceUsed { get; set; }
        public decimal? Amount1VarianceValue { get; set; }
        public int? IsAmount2VarianceUsed { get; set; }
        public decimal? Amount2VarianceValue { get; set; }

        public virtual transactionmatchingmethod transactionmatchingmethod { get; set; }

        public virtual transactionmatchingrelationship transactionmatchingrelationship { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingmatched> transactionmatchingmatcheds { get; set; }
    }
}
