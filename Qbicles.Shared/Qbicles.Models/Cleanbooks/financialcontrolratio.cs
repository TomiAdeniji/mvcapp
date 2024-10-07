namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_financialcontrolratio")]
    public partial class financialcontrolratio
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public financialcontrolratio()
        {
            fcratioprofiles = new HashSet<fcratioprofile>();
            fcratiomanualbalances = new HashSet<fcratiomanualbalance>();
            fcratioaccounts = new HashSet<fcratioaccount>();
        }

        public int Id { get; set; }

        public int FinancialControlReportDefinitionId { get; set; }
        public virtual financialcontrolreportdefinition financialcontrolreportdefinitions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratioprofile> fcratioprofiles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratiomanualbalance> fcratiomanualbalances { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratioaccount> fcratioaccounts { get; set; }
    }
}
