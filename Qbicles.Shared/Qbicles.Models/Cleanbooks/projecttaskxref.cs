namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_projecttaskxref")]
    public partial class projecttaskxref
    {
        public int Id { get; set; }

        public int Order { get; set; }

        public int ProjectId { get; set; }

        public virtual project project { get; set; }
    }
}
