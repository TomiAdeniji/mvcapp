namespace CleanBooksData
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_transactionmatchingunmatchgroup")]
    public partial class transactionmatchingunmatchgroup
    {
        public transactionmatchingunmatchgroup()
        {
            transactionmatchingunmatcheds = new HashSet<transactionmatchingunmatched>();
          
        }
        public int Id { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transactionmatchingunmatched> transactionmatchingunmatcheds { get; set; }
    }
}
