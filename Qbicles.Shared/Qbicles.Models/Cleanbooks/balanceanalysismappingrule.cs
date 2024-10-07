namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_balanceanalysismappingrule")]
    public partial class balanceanalysismappingrule
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public balanceanalysismappingrule()
        {
            balanceanalysismatchs = new HashSet<balanceanalysismatch>();

        }

        public int Id { get; set; }    

        public int TaskId { get; set; }
        public string Description1 { get; set; }
        public string Reference1 { get; set; }
        public string Description2 { get; set; }
        public string Reference2 { get; set; }
        public decimal MinDifference { get; set; }
        public decimal MaxDifference { get; set; }
        public virtual task tasks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysismatch> balanceanalysismatchs { get; set; }
    }
}
