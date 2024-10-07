namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_alertmultipleprofiles")]
    public partial class alertmultipleprofile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public alertmultipleprofile()
        {
            alertmultipleprofiletaskxrefs = new HashSet<alertmultipleprofiletaskxref>();
        }

        public int Id { get; set; }
        public int MultipleAccountAlertId { get; set; }      
      
        public string Profile { get; set; }      
        
        public virtual multipleaccountalert multipleaccountalerts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<alertmultipleprofiletaskxref> alertmultipleprofiletaskxrefs { get; set; }

    }
}
