namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcratioprofilevalue")]
    public partial class fcratioprofilevalue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcratioprofilevalue()
        {
           
        }

        public int Id { get; set; }
        public decimal Value { get; set; }
        public int FcRatioProfileId { get; set; }
        public int FcReportExecutionInstanceId { get; set; }
        public virtual fcratioprofile fcratioprofiles { get; set; }
        public virtual fcreportexecutioninstance fcreportexecutioninstances { get; set; }
      
    }
}
