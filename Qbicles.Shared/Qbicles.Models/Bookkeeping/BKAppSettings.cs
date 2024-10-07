using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_appsettings")]
    public class BKAppSettings
    {
        public int Id { get; set; }

        public virtual QbicleDomain Domain { get; set; }

        //public virtual ApprovalRequestDefinition JournalEntryApprovalProcess { get; set; }

        //public virtual Qbicle ApprovalQbicle { get; set; }
        //public virtual Topic DefaultTopic { get; set; }

        public virtual Qbicle AttachmentQbicle { get; set; }
        public virtual Topic AttachmentDefaultTopic { get; set; }
    }
}