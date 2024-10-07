namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionanalysisreportstatistics")]
    public partial class transactionanalysisreportstatistic
    {
        public int Id { get; set; }

        public int? NumberOfDebits { get; set; }

        public int? NumberOfCredits { get; set; }

        public decimal? BiggestDebit { get; set; }

        public decimal? BiggestCredit { get; set; }

        public decimal? LowestDebit { get; set; }

        public decimal? LowestCredit { get; set; }

        public decimal? TotalValueOfDebits { get; set; }

        public decimal? TotalValueOfCredits { get; set; }

        public int TransactionAnalysisReportGroupId { get; set; }

        public virtual transactionanalysisreportgroup transactionanalysisreportgroup { get; set; }
    }
}
