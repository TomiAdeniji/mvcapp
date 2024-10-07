namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_upload")]
    public partial class upload
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public upload()
        {
            transactions = new HashSet<transaction>();
        }

        public int Id { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string Name { get; set; }

        [StringLength(100)]
        public string FileName { get; set; }

        [StringLength(300)]
        public string FilePath { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public long AccountId { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedById { get; set; }

        public DateTime? CreatedDate { get; set; }

        public int UploadFormatId { get; set; }

        public virtual Account account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<transaction> transactions { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }

        public virtual UploadFormat UploadFormat { get; set; }
    }
}
