namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcprofiletrendvalues")]
    public partial class fcprofiletrendvalues
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcprofiletrendvalues()
        {
            
        }

        public int Id { get; set; }
        public string Month { get; set; }
        public int? Order { get; set; }
        public decimal? ProfileTotal { get; set; }      
        public int FinancialControlProfileTrendId { get; set; }
        public int FcReportExecutionInstanceId { get; set; }
        public virtual financialcontrolprofiletrend financialcontrolprofiletrends { get; set; }
        public virtual fcreportexecutioninstance fcreportexecutioninstances { get; set; }
        
    }
}
