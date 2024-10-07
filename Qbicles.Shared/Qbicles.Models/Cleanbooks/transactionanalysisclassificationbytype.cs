namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionanalysisclassificationbytype")]
    public partial class transactionanalysisclassificationbytype
    {
        public int Id { get; set; }

        [StringLength(45)]
        public string ProfileType { get; set; }

        public int? CreditCount { get; set; }

        public decimal? CreditPercentOfCount { get; set; }

        public decimal? CreditPercentOfTotalValue { get; set; }

        public decimal? CreditTotalValue { get; set; }

        public int? DebitCount { get; set; }

        public decimal? DebitPercentOfCount { get; set; }

        public decimal? DebitPercentOfTotalValue { get; set; }

        public decimal? DebitTotalValue { get; set; }

        public int TransactionAnalysisReportGroupId { get; set; }

        public virtual transactionanalysisreportgroup transactionanalysisreportgroup { get; set; }
    }
}
