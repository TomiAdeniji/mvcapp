namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fctotalprofiletaskxref")]
    public partial class fctotalprofiletaskxref
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fctotalprofiletaskxref()
        {
           
        }

        public int Id { get; set; }     
        public int FinancialControlTotalProfileId { get; set; }
        public int TaskId { get; set; }
        public virtual financialcontroltotalprofile financialcontroltotalprofiles { get; set; }
        public virtual task tasks { get; set; }
      
    }
}
