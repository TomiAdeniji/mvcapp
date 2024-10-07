namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_deleteduploads")]
    public partial class deletedupload
    {
        public long Id { get; set; }

        public long AccountId { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [StringLength(65535)]
        public string UploadName { get; set; }

        [Required]
        [StringLength(128)]
        public string DeletedById { get; set; }

        [Required]
        [StringLength(45)]
        public string NumberOfTransactions { get; set; }

        public DateTime DeletedDate { get; set; }

        public virtual Account account { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }
    }
}
