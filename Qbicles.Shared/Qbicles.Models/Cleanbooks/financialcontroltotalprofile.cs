namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_financialcontroltotalprofile")]
    public partial class financialcontroltotalprofile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public financialcontroltotalprofile()
        {
            fctotalprofiletotals = new HashSet<fctotalprofiletotal>();
            fctotalprofiletaskxrefs = new HashSet<fctotalprofiletaskxref>();
        }

        public int Id { get; set; }      
        public int FinancialControlReportDefinitionId { get; set; }   
         public string ProfileValue { get; set; }        
        public virtual financialcontrolreportdefinition financialcontrolreportdefinitions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fctotalprofiletotal> fctotalprofiletotals { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fctotalprofiletaskxref> fctotalprofiletaskxrefs { get; set; }
    }
}
