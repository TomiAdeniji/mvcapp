namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_balanceanalysiswarninglevel")]
    public partial class balanceanalysiswarninglevel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public balanceanalysiswarninglevel()
        {
           
         
        }

        public int Id { get; set; }
        public string Name { get; set; }        
        
    }
}
