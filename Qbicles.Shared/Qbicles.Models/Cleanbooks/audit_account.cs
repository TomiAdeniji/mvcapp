namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    [Table("cb_audit_account")]
    public partial class audit_account
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public audit_account()
        {

        }

        public long id { get; set; }

        public string User_id { get; set; }
        public decimal LastBalance { get; set; }
        public decimal LastBalance_New { get; set; }
        public string Name { get; set; }
        public string Name_New { get; set; }
        public string Number { get; set; }
        public string Number_New { get; set; }
        public string changetype { get; set; }
        public DateTime changetime { get; set; }
        public string Note { get; set; }

    }
    
}
