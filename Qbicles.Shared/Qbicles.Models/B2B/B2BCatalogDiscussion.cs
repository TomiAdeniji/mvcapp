using Qbicles.Models.Catalogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2B
{
    public class B2BCatalogDiscussion : QbicleDiscussion
    {
        public B2BCatalogDiscussion()
        {
            this.DiscussionType = DiscussionTypeEnum.B2BCatalogDiscussion;
        }
        public virtual QbicleDomain SharedWithDomain { get; set; }
        public virtual QbicleDomain SharedByDomain { get; set; }
        public virtual Catalog AssociatedCatalog { get; set; }
    }
}
