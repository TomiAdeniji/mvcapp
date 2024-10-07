using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models
{
    [Table("qb_QbicleApplication")]
    public class QbicleApplication
    {

        public string Name { get; set; }

        public int Id { get; set; }

        public virtual List<AppRight> Rights { get; set; } = new List<AppRight>();

        public virtual List<AppInstance> AppInstances { get; set; } = new List<AppInstance>();

        public string Group { get; set; }

        public string AppIcon { get; set; }

        public string AppImage { get; set; }

        public string Description { get; set; }

        public string AdPage { get; set; }

        [Column(TypeName = "bit")]
        public bool IsCore { get; set; }


        public virtual List<QbicleDomain> DomainsSubscribed { get; set; } = new List<QbicleDomain>();

        public virtual List<QbicleDomain> DomainsAvailable { get; set; } = new List<QbicleDomain>();
    }
}