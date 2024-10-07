namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_manualbalance")]
    public partial class manualbalance
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public manualbalance()
        {
            financialcontrolmanualbalances = new HashSet<financialcontrolmanualbalance>();
            fcratiomanualbalances = new HashSet<fcratiomanualbalance>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }
        public int IsDeleted { get; set; }
        public int ManualBalanceGroupId { get; set; }
        public virtual manualbalancegroup manualbalancegroups { get; set; }
        public virtual Qbicles.Models.ApplicationUser users { get; set; }
       
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<financialcontrolmanualbalance> financialcontrolmanualbalances { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<fcratiomanualbalance> fcratiomanualbalances { get; set; }
    }
}
