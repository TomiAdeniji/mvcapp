namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_balanceanalysispredefaction")]
    public partial class balanceanalysispredefaction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public balanceanalysispredefaction()
        {
           
         
        }

        public int Id { get; set; }
        public string Name { get; set; }
     
        
    }
}
