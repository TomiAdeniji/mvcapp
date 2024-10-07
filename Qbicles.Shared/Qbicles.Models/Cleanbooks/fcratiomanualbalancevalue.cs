namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcratiomanualbalancevalue")]
    public partial class fcratiomanualbalancevalue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcratiomanualbalancevalue()
        {
           
        }
        public int Id { get; set; }
        public decimal Value { get; set; }
        public int FcRatioManualBalanceId { get; set; }
        public int FcReportExecutionInstanceId { get; set; }
        public fcratiomanualbalance fcratiomanualbalances { get; set; }
        public virtual fcreportexecutioninstance fcreportexecutioninstances { get; set; }
       
    }
}
