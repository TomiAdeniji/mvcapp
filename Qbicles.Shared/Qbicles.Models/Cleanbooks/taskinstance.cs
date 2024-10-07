namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_taskinstance")]
    public partial class taskinstance
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public taskinstance()
        {
            transactionanalysistasks = new HashSet<transactionanalysistask>();
            transactionmatchingtasks = new HashSet<transactionmatchingtask>();
            finacialcontroltasks = new HashSet<finacialcontroltask>();
            balanceanalysistasks= new HashSet<balanceanalysistask>();
        }

        public int id { get; set; }

        public DateTime DateExecuted { get; set; }

        [Required]
        [StringLength(128)]
        public string ExecutedById { get; set; }

        public int TaskId { get; set; }

        [Column(TypeName = "date")]
        public DateTime? StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EndDate { get; set; }

        public sbyte? IsComplete { get; set; }

        public int? TaskInstanceDateRangeId { get; set; }

        public virtual task task { get; set; }

        public virtual taskinstancedaterange taskinstancedaterange { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionanalysistask> transactionanalysistasks { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingtask> transactionmatchingtasks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<finacialcontroltask> finacialcontroltasks { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<balanceanalysistask> balanceanalysistasks { get; set; }
    }
}
