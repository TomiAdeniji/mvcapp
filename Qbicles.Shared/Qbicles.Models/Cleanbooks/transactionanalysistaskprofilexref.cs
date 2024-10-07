namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionanalysistaskprofilexref")]
    public partial class transactionanalysistaskprofilexref
    {
        public int Id { get; set; }

        public int ExecutionOrder { get; set; }

        public int TransactionAnalysisTaskId { get; set; }

        public int ProfileId { get; set; }

        public virtual profile profile { get; set; }

        public virtual transactionanalysistask transactionanalysistask { get; set; }
    }
}
