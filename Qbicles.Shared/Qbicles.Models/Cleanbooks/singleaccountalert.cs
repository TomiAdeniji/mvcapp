namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_singleaccountalert")]
    public partial class singleaccountalert
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public singleaccountalert()
        {
            singleaccountalertusersxrefs = new HashSet<singleaccountalertusersxref>();
            singleaccountalerttransanalysisxrefs = new HashSet<singleaccountalerttransanalysisxref>();
        }

        public int Id { get; set; } 
        [Required]
        [StringLength(256)]
        public string Name { get; set; } 
        public int IsAccount { get; set; }
     
        public long? AccountId { get; set; }
        public string Profile { get; set; }

        public int AlertConditionId { get; set; }

        public decimal? Amount { get; set; }
        public decimal? Percentage { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedBy { get; set; }
        

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<singleaccountalertusersxref> singleaccountalertusersxrefs { get; set; }

        public virtual Account Accounts { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }

        public virtual alertcondition alertconditions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<singleaccountalerttransanalysisxref> singleaccountalerttransanalysisxrefs { get; set; }

    }
}
