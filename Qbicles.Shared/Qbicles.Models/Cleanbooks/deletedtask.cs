namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_deletedtasks")]
    public partial class deletedtask
    {
        public int Id { get; set; }

        [Required]
        [StringLength(45)]
        public string TaskName { get; set; }

        [Required]
        [StringLength(3000)]
        public string TaskDescription { get; set; }

        [Required]
        [StringLength(128)]
        public string DeletedById { get; set; }

        public DateTime DeletedDate { get; set; }

        public virtual Qbicles.Models.ApplicationUser user { get; set; }
    }
}
