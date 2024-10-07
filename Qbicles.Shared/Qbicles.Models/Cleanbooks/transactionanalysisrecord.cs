namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionanalysisrecord")]
    public partial class transactionanalysisrecord
    {
        public int Id { get; set; }

        public int TransactionAnalysisTaskId { get; set; }

        public long TransactionId { get; set; }

        [StringLength(45)]
        public string ProfileValue { get; set; }

        public virtual transaction transaction { get; set; }

        public virtual transactionanalysistask transactionanalysistask { get; set; }
    }
}
