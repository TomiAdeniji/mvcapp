namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_taskaccount")]
    public partial class taskaccount
    {
        public int Id { get; set; }

        public int Order { get; set; }

        public long AccountId { get; set; }

        public int TaskId { get; set; }

        public virtual Account account { get; set; }

        public virtual task task { get; set; }
    }
}
