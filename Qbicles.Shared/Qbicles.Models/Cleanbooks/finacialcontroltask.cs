namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_finacialcontroltask")]
    public partial class finacialcontroltask
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public finacialcontroltask()
        {       
            
        }

        public int Id { get; set; }
        public int TaskInstanceId { get; set; }

        public virtual taskinstance taskinstances { get; set; }            
    }
}
