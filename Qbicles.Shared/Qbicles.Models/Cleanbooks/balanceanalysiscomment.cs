namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_balanceanalysiscomment")]
    public partial class balanceanalysiscomment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public balanceanalysiscomment()
        {
           
        }

        public int Id { get; set; }        
        public int BalanceAnalysisTaskId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public virtual balanceanalysistask balanceanalysistasks { get; set; }
        public virtual Qbicles.Models.ApplicationUser users { get; set; }

    }
}
