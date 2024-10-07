namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_tmtaskalerteexref")]
    public partial class tmtaskalerteexref
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tmtaskalerteexref(){}
        public int Id { get; set; } 
        public int TransactionMatchingTaskRuleId { get; set; }
        public string UsersId { get; set; } 
        public virtual transactionmatchingtaskrule transactionmatchingtaskrule { get; set; }
        public virtual Qbicles.Models.ApplicationUser user { get; set; } 
    }
}
