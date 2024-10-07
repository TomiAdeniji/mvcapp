namespace CleanBooksData
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_alertmultipleprofiletaskxrefs")]
    public partial class alertmultipleprofiletaskxref
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public alertmultipleprofiletaskxref()
        {            
        }

        public int Id { get; set; }
        public int AlertMultipleProfileId { get; set; }
        public int TaskId { get; set; }
        public virtual alertmultipleprofile alertmultipleprofiles { get; set; }
        public virtual task tasks { get; set; }        
      
    }
}
