namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_audit_transaction_analysis")]
    public partial class audit_transaction_analysis
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public audit_transaction_analysis()
        {

        }
        
        public long id { get; set; }
        public string User_id { get; set; }
        public int accountId { get; set; }       
        public int taskId { get; set; }
        public int taskInstanceId { get; set; }
        public int transactionsisTaskId { get; set; }       
        public string changetype { get; set; }
        public DateTime changetime { get; set; }
        public string Note { get; set; }

    }
    
}
