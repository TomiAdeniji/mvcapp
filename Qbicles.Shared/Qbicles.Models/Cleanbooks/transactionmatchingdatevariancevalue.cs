namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingdatevariancevalues")]
    public partial class transactionmatchingdatevariancevalue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionmatchingdatevariancevalue()
        {            
           
        }
        public int Id { get; set; }
        public int NumberOfDays { get; set; }       
    }
}
