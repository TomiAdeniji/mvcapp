namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionanalysisclassificationbydate")]
    public partial class transactionanalysisclassificationbydate
    {
        public int Id { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

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
