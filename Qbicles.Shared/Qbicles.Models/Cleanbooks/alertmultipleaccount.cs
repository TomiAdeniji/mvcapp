namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_alertmultipleaccounts")]
    public partial class alertmultipleaccount
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public alertmultipleaccount()
        {            
        }

        public int Id { get; set; }
        public int MultipleAccountAlertId { get; set; }
        public long AccountId { get; set; }
        public virtual Account Accounts { get; set; }
        public virtual multipleaccountalert multipleaccountalerts { get; set; }        
      
    }
}
