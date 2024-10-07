namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingtaskrule")]
    public partial class transactionmatchingtaskrule
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public transactionmatchingtaskrule()
        {
            tmtaskalerteexref = new HashSet<tmtaskalerteexref>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int IsUnmatchedDays { get; set; }
        public int Days { get; set; }
        public decimal Amount { get; set; }
        public int TaskId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual task task { get; set; }
        public virtual Qbicles.Models.ApplicationUser user { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tmtaskalerteexref> tmtaskalerteexref { get; set; }
    }
}
