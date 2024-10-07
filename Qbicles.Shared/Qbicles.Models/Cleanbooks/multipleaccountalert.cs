namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_multipleaccountalert")]
    public partial class multipleaccountalert
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public multipleaccountalert()
        {
            alertmultipleprofiles = new HashSet<alertmultipleprofile>();
            alertmultipleaccounts = new HashSet<alertmultipleaccount>();
            multipleaccountalertuserxrefs= new HashSet<multipleaccountalertuserxref>();
        }

        public int Id { get; set; } 
        [Required]
        [StringLength(256)]
        public string Name { get; set; }        

        public DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedBy { get; set; }

        public decimal Percentage { get; set; }
        

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<alertmultipleprofile> alertmultipleprofiles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<alertmultipleaccount> alertmultipleaccounts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<multipleaccountalertuserxref> multipleaccountalertuserxrefs { get; set; }
        public virtual Qbicles.Models.ApplicationUser user { get; set; }        
      
    }
}
