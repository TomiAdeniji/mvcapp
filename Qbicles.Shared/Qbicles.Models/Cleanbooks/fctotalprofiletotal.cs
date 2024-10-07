namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fctotalprofiletotal")]
    public partial class fctotalprofiletotal
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fctotalprofiletotal()
        {       
            
        }

        public int Id { get; set; }
        public decimal? Value { get; set; }       
        public int FinancialControlBalanceProfileId { get; set; }
        public int FcReportExecutionInstanceId { get; set; }
        public virtual fcreportexecutioninstance fcreportexecutioninstances { get; set; }
        public virtual financialcontroltotalprofile financialcontroltotalprofiles { get; set; }                  
    }
}
