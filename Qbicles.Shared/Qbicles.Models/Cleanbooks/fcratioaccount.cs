namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcratioaccount")]
    public partial class fcratioaccount
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcratioaccount()
        {
            fcratioaccountvalues = new HashSet<fcratioaccountvalue>();
        }

        public int Id { get; set; }
        public long AccountId { get; set; }
        public int FinancialControlRatioId { get; set; }
        public virtual financialcontrolratio financialcontrolratios { get; set; }
        public virtual Account Accounts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratioaccountvalue> fcratioaccountvalues { get; set; }
    }
}
