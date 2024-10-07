namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingunmatched")]
    public partial class transactionmatchingunmatched
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public int TransactionMatchingUnMatchGroupId { get; set; }
        public long TransactionId { get; set; }

        public virtual task tasks { get; set; }
        public virtual transactionmatchingunmatchgroup transactionmatchingunmatchgroups { get; set; }
        public virtual transaction transactions { get; set; }
    }
}
