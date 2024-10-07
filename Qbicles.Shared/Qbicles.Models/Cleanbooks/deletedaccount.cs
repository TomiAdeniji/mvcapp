namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_deletedaccount")]
    public partial class deletedaccount
    {
        public long Id { get; set; }

        [Required]
        [StringLength(256)]
        public string AccountName { get; set; }

        [Required]
        [StringLength(45)]
        public string AccountNumber { get; set; }

        public DateTime? CreatedDate { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedById { get; set; }

        public DateTime DeleteDate { get; set; }

        [Required]
        [StringLength(128)]
        public string DeleteById { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }

        public virtual Qbicles.Models.ApplicationUser user1 { get; set; }
    }
}
