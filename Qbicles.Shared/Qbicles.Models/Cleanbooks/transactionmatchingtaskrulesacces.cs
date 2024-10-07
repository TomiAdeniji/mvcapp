namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingtaskrulesaccess")]
    public partial class transactionmatchingtaskrulesacces
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionmatchingtaskrulesacces()
        {            
           
        }

        public int Id { get; set; }
        public int TaskId { get; set; }
        public int IsDateVarianceVisible { get; set; }
        public int IsAmountVarianceVisible { get; set; }

        public virtual task tasks { get; set; }
       
    }
}
