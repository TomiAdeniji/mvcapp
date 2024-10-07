namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_balanceanalysismatch")]
    public partial class balanceanalysismatch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public balanceanalysismatch()
        {
           
         
        }

        public int Id { get; set; }
        public int DataSet1Id { get; set; }
        public int DataSet2Id { get; set; }
        public decimal Difference { get; set; }
        public int WarningLevel { get; set; }
        public int BalanceAnalysisActionId { get; set; }
        public int balanceanalysismappingruleId { get; set; }
        public virtual balanceanalysisdataset balanceanalysisdataset1 { get; set; }
        public virtual balanceanalysisdataset balanceanalysisdataset2 { get; set; }
        public virtual balanceanalysisaction balanceanalysisactions { get; set; }
        public virtual balanceanalysismappingrule balanceanalysismappingrules { get; set; }

    }
}
