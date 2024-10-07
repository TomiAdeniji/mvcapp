namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcratioprofile")]
    public partial class fcratioprofile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcratioprofile()
        {
            fcratioprofiletaskxrefs = new HashSet<fcratioprofiletaskxref>();
            fcratioprofilevalues = new HashSet<fcratioprofilevalue>();
        }

        public int Id { get; set; }
        public string ProfileValue { get; set; }
        public int FinancialControlRatioId { get; set; }
        public virtual financialcontrolratio financialcontrolratios { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratioprofiletaskxref> fcratioprofiletaskxrefs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratioprofilevalue> fcratioprofilevalues { get; set; }

    }
}
