namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionanalysiscomment")]
    public partial class transactionanalysiscomment
    {
        public int Id { get; set; }

        [StringLength(1000)]
        public string Comment { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedBy { get; set; }

        public int TransactionAnalysisReportGroupId { get; set; }

        public virtual transactionanalysisreportgroup transactionanalysisreportgroup { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }
    }
}
