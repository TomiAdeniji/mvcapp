namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_multipleaccountalertuserxref")]
    public partial class multipleaccountalertuserxref
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public multipleaccountalertuserxref()
        {            
        }

        public int Id { get; set; }
        public int MultipleAccountAlertId { get; set; }      
      
        public string UsersId { get; set; }
        public virtual Qbicles.Models.ApplicationUser user { get; set; }
        public virtual multipleaccountalert multipleaccountalerts { get; set; }        
      
    }
}
