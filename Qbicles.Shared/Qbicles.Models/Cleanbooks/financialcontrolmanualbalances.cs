namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_financialcontrolmanualbalances")]
    public partial class financialcontrolmanualbalance
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public financialcontrolmanualbalance()
        {
            fctotalmanualbalancevalues = new HashSet<fctotalmanualbalancevalue>(); 
        }

        public int Id { get; set; }
        public int ManualBalanceId { get; set; }
        public int FinancialControlReportDefinitionId { get; set; }
        public virtual manualbalance manualbalances { get; set; }
        public virtual financialcontrolreportdefinition financialcontrolreportdefinitions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fctotalmanualbalancevalue> fctotalmanualbalancevalues { get; set; }
    }
}
