namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_uploadfields")]
    public partial class uploadfield
    {
        public int Id { get; set; }

        [StringLength(45)]
        public string Name { get; set; }

        public long AccountId { get; set; }

        public virtual Account account { get; set; }
    }
}
