namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_financialcontroltrendprofiletaskxref")]
    public partial class financialcontroltrendprofiletaskxref
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public financialcontroltrendprofiletaskxref()
        {

        }

        public int Id { get; set; }
        public int FinancialControlProfileTrendId { get; set; }
        public int TaskId { get; set; }       
        public virtual financialcontrolprofiletrend financialcontrolprofiletrends { get; set; }
        public virtual task tasks { get; set; }
    }
}
