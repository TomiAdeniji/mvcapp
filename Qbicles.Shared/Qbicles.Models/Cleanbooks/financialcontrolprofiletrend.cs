namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_financialcontrolprofiletrend")]
    public partial class financialcontrolprofiletrend
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public financialcontrolprofiletrend()
        {
            fcprofiletrendvalue = new HashSet<fcprofiletrendvalues>();
            financialcontroltrendprofiletaskxrefs = new HashSet<financialcontroltrendprofiletaskxref>();
        }

        public int Id { get; set; }      
        public string ProfileValue { get; set; }
        public int FinancialControlReportDefinitionId { get; set; }
           
        public virtual financialcontrolreportdefinition financialcontrolreportdefinitions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcprofiletrendvalues> fcprofiletrendvalue { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontroltrendprofiletaskxref> financialcontroltrendprofiletaskxrefs { get; set; }
        
    }
}
