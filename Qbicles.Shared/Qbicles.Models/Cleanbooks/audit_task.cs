namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_audit_task")]
    public partial class audit_task
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public audit_task()
        {

        }

        public long id { get; set; }
        public string User_id { get; set; }
        public string CreatedById { get; set; }       
        public string Name { get; set; }
        public string Name_New { get; set; }
        public DateTime CreatedDate { get; set; }       
        public string changetype { get; set; }
        public DateTime changetime { get; set; }
        public string Note { get; set; }

    }
    
}
