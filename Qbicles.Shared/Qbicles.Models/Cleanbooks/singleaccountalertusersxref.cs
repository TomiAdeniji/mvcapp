namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_singleaccountalertusersxref")]
    public partial class singleaccountalertusersxref
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public singleaccountalertusersxref()
        {
           
          
        }

        public int Id { get; set; }
        public int SingleAccountAlertId { get; set; }
        [Required]
        [StringLength(256)]
        public string UsersId { get; set; }

        public virtual singleaccountalert singleaccountalerts { get; set; }
        public virtual Qbicles.Models.ApplicationUser user { get; set; }
    }
}
