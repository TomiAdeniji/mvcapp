using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_journalentrytemplate")]
    public class JournalEntryTemplate
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public virtual List<JournalEntryTemplateRow> TemplateRows { get; set; } = new List<JournalEntryTemplateRow>();

        public virtual QbicleDomain Domain { get; set; }
    }
}
