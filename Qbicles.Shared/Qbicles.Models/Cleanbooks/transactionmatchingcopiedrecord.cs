namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingcopiedrecord")]
    public partial class transactionmatchingcopiedrecord
    {
        public int Id { get; set; }

        public int TransactionMatchingRecordId { get; set; }

        public int CopiedFromId { get; set; }

        public virtual transactionmatchingrecord transactionmatchingrecord { get; set; }

        public virtual transactionmatchingrecord transactionmatchingrecord1 { get; set; }
    }
}
