namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_audit_users")]
    public partial class audit_user
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public audit_user()
        {

        }

        public long id { get; set; }

        public string User1_id { get; set; }

        public string User2_id { get; set; }

        public string changetype { get; set; }

        public DateTime? changetime { get; set; }
        public string Note { get; set; }

    }
    
}
