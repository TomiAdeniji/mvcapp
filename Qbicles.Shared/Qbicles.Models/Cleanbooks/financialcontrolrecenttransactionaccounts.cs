namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_financialcontrolrecenttransactionaccounts")]
    public partial class financialcontrolrecenttransactionaccounts
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public financialcontrolrecenttransactionaccounts()
        {
            fcreporttransactionxrefs = new HashSet<fcreporttransactionxref>();
        }

        public int Id { get; set; }        
        public long AccountId { get; set; }
        public int FinancialControlReportDefinitionId { get; set; }       

        public virtual Account Accounts { get; set; }
        public virtual financialcontrolreportdefinition financialcontrolreportdefinitions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcreporttransactionxref> fcreporttransactionxrefs { get; set; }
    }
}
