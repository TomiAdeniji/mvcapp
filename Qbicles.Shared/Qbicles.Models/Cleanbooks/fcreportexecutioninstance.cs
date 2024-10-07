namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcreportexecutioninstance")]
    public partial class fcreportexecutioninstance
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcreportexecutioninstance()
        {
            fctotalaccountbalances = new HashSet<fctotalaccountbalance>();
            fctotalmanualbalancevalues = new HashSet<fctotalmanualbalancevalue>();
            fcratioaccountvalues = new HashSet<fcratioaccountvalue>();
            fctotalprofiletotals = new HashSet<fctotalprofiletotal>();
            fcratiomanualbalancevalues = new HashSet<fcratiomanualbalancevalue>();
            fcratioprofilevalues = new HashSet<fcratioprofilevalue>();
            fcprofiletrendvaluess = new HashSet<fcprofiletrendvalues>();           
        }

        public int Id { get; set; }       
        public int FinancialControlReportDefinitionId { get; set; }        
        public DateTime ExecutionDate { get; set; }
        public string ExecutedBy { get; set; }

        public virtual financialcontrolreportdefinition financialcontrolreportdefinitions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fctotalaccountbalance> fctotalaccountbalances { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fctotalmanualbalancevalue> fctotalmanualbalancevalues { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratioaccountvalue> fcratioaccountvalues { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fctotalprofiletotal> fctotalprofiletotals { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratiomanualbalancevalue> fcratiomanualbalancevalues { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratioprofilevalue> fcratioprofilevalues { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcprofiletrendvalues> fcprofiletrendvaluess { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcaccounttrendvalues> fcaccounttrendvaluess { get; set; }
    }
}
