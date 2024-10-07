namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_audit_transaction_matching")]
    public partial class audit_transaction_matching
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public audit_transaction_matching()
        {

        }
        
        public long id { get; set; }
        public string User_id { get; set; }
        public int accountId { get; set; }
        public int accountId2 { get; set; } 
        public int taskId { get; set; }
        public int transactionMatchingTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int matchingGroupId { get; set; } 
        public string changetype { get; set; }
        public DateTime changetime { get; set; }
        public string Note { get; set; }

    }
    
}
