namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcratioaccountvalue")]
    public partial class fcratioaccountvalue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcratioaccountvalue()
        {
           
        }

        public int Id { get; set; }
        public decimal Value { get; set; }
        public int FcRatioAccountId { get; set; }
        public int FcReportExecutionInstanceId { get; set; }
        public fcratioaccount fcratioaccounts { get; set; }
        public virtual fcreportexecutioninstance fcreportexecutioninstances { get; set; }
       
    }
}
