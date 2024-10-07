namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_balanceanalysisdocument")]
    public partial class balanceanalysisdocument
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public balanceanalysisdocument()
        {
          
        }

        public int Id { get; set; }
        public string DocPath { get; set; }
        public string DocTitle { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public int BalanceAnalysisTaskId { get; set; }
        public virtual Qbicles.Models.ApplicationUser users { get; set; }
        public virtual balanceanalysistask balanceanalysistasks { get; set; }
    }
}
