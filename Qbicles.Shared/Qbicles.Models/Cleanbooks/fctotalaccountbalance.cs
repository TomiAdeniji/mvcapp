namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fctotalaccountbalance")]
    public partial class fctotalaccountbalance
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fctotalaccountbalance()
        {            
        }

        public int Id { get; set; }
        public decimal Value { get; set; }
        public int FinancialControlBalanceAccountId { get; set; }
        public int FcReportExecutionInstanceId { get; set; }    
        public virtual financialcontrolbalanceaccount financialcontrolbalanceaccounts { get; set; }
        public virtual fcreportexecutioninstance fcreportexecutioninstances { get; set; }      
    }
}
