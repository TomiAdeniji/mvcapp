using System.ComponentModel.DataAnnotations.Schema;
using Qbicles.Models.Trader;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_journalentrytemplaterow")]
    public class JournalEntryTemplateRow
    {
        public int Id { get; set; }

        public virtual JournalEntryTemplate Template { get; set; }

        public virtual CoANode Parent { get; set; }


        [Column(TypeName = "bit")]
        public bool IsDebit { get; set; }

    }
}
