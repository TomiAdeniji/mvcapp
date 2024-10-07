namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcaccounttrendvalues")]
    public partial class fcaccounttrendvalues
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcaccounttrendvalues()
        {
            
        }

        public int Id { get; set; }
        public string Month { get; set; }
        public int Order { get; set; }
        public decimal AccountBalance { get; set; }        
        public int FinancialControlAccountTrendId { get; set; }
        public int FcReportExecutionInstanceId { get; set; }
        public virtual financialcontrolaccounttrend financialcontrolaccounttrends { get; set; }
        public virtual fcreportexecutioninstance fcreportexecutioninstances { get; set; }      
    }
}
