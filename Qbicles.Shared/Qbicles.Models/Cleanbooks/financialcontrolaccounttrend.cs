namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_financialcontrolaccounttrend")]
    public partial class financialcontrolaccounttrend
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public financialcontrolaccounttrend()
        {
            fcaccounttrendvalue = new HashSet<fcaccounttrendvalues>();
        }

        public int Id { get; set; }        
        public long AccountId { get; set; }       
        public int FinancialControlReportDefinitionId { get; set; }       

        public virtual Account Accounts { get; set; }       
        public virtual financialcontrolreportdefinition financialcontrolreportdefinitions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcaccounttrendvalues> fcaccounttrendvalue { get; set; }
        
    }
}
