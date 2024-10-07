namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fctotalmanualbalancevalue")]
    public partial class fctotalmanualbalancevalue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fctotalmanualbalancevalue()
        {
           
        }

        public int Id { get; set; }
        public decimal Value { get; set; }
        public int FinancialControlManualBalancesId { get; set; }
        public int FcReportExecutionInstanceId { get; set; }
        public DateTime LastUpdated { get; set; }
        public virtual fcreportexecutioninstance fcreportexecutioninstances { get; set; }
        public virtual financialcontrolmanualbalance financialcontrolmanualbalances { get; set; }
      
    }
}
