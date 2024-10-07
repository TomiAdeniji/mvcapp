namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingamountvariancevalues")]
    public partial class transactionmatchingamountvariancevalue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionmatchingamountvariancevalue()
        {            
           
        }

        public int Id { get; set; }
        public decimal Percentage { get; set; }     
       
    }
}
