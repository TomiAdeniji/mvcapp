using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    /// <summary>
    /// Model for Media 
    /// </summary>

    [Table("qb_qbiclemedia")]
    public class QbicleMedia : QbicleActivity
    {
        public string Description { get; set; }
        [Required]
        public virtual QbicleFileType FileType { get; set; }

        public virtual List<VersionedFile> VersionedFiles { get; set; } = new List<VersionedFile>();

        public virtual MediaFolder MediaFolder { get; set; }

        public virtual JournalEntry JournalEntry { get; set; }

        public virtual BKAccount BKAccount { get; set; }
        public virtual BKTransaction BKTransaction { get; set; }
        public virtual CashAccountTransaction CashAccountTransaction { get; set; }
        public virtual Invoice Invoice { get; set; }
        [Column(TypeName = "bit")]
        public bool IsPublic { get; set; }
        public QbicleMedia()
        {
            this.ActivityType = ActivityTypeEnum.MediaActivity;
            this.App = ActivityApp.Qbicles;
        }

    }
}
