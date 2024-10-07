namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcratiomanualbalance")]
    public partial class fcratiomanualbalance
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcratiomanualbalance()
        {
            fcratiomanualbalancevalues = new HashSet<fcratiomanualbalancevalue>();
        }
        public int Id { get; set; }
        public int FinancialControlRatioId { get; set; }
        public int ManualBalanceId { get; set; }
        public financialcontrolratio financialcontrolratios { get; set; }
        public virtual manualbalance manualbalances { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratiomanualbalancevalue> fcratiomanualbalancevalues { get; set; }
    }
}
