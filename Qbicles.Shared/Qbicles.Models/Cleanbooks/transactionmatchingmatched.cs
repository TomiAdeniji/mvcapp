namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingmatched")]
    public partial class transactionmatchingmatched
    {
        public int Id { get; set; }

        public int TransactionMatchingRecordId { get; set; }

        public int TransactionMatchingGroupId { get; set; }

        public virtual transactionmatchinggroup transactionmatchinggroup { get; set; }

        public virtual transactionmatchingrecord transactionmatchingrecord { get; set; }
    }
}
