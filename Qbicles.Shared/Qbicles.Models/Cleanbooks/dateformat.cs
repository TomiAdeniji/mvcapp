namespace CleanBooksData
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_dateformats")]
    public partial class dateformat
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public dateformat()
        {
            uploadformats = new HashSet<UploadFormat>();
        }

        public long Id { get; set; }

        [Required]
        [StringLength(45)]
        public string Format { get; set; }


        [Column(TypeName = "bit")]
        public bool IsDefault { get; set; }

        [StringLength(45)]
        public string DateFormatExpression { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedById { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UploadFormat> uploadformats { get; set; }
    }
}
