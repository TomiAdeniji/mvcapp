namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_balanceanalysisdataset")]
    public partial class balanceanalysisdataset
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public balanceanalysisdataset()
        {

            balanceanalysismatch1s= new HashSet<balanceanalysismatch>();
            balanceanalysismatch2s = new HashSet<balanceanalysismatch>();

        }

        public int Id { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public short IsDataset1 { get; set; }
        public int BalanceAnalysisTaskId { get; set; }
        public virtual balanceanalysistask balanceanalysistasks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysismatch> balanceanalysismatch1s { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysismatch> balanceanalysismatch2s { get; set; }
    }
}
