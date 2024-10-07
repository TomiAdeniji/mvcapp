namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_fcratioprofiletaskxref")]
    public partial class fcratioprofiletaskxref
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public fcratioprofiletaskxref()
        {
           
        }

        public int Id { get; set; }     
        public int TaskId { get; set; }
        public int FcRatioProfileId { get; set; }
        public virtual task tasks { get; set; }
        public virtual fcratioprofile fcratioprofiles { get; set; }
      
    }
}
