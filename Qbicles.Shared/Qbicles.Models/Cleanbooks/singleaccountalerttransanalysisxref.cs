namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_singleaccountalerttransanalysisxref")]
    public partial class singleaccountalerttransanalysisxref
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public singleaccountalerttransanalysisxref()
        {
          
        }

        public int Id { get; set; }
        public int SingleAccountAlertId { get; set; }
        public int TaskId { get; set; }
        public virtual task tasks { get; set; }
        public virtual singleaccountalert singleaccountalerts { get; set; }

    }
}
