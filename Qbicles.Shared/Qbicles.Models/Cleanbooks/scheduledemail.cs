namespace CleanBooksData
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("cb_scheduledemails")]
    public partial class scheduledemail
    {
        public long Id { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [StringLength(65535)]
        public string Body { get; set; }

        [Column(TypeName = "date")]
        public DateTime SendDate { get; set; }

        public int SpecifiedIntervalId { get; set; }

        [Column(TypeName = "bit")]
        public bool IsSent { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateSent { get; set; }

        public int TaskId { get; set; }
    }
}
