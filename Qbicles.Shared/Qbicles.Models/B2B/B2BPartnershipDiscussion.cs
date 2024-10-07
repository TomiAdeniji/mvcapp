using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.B2B
{
    public class B2BPartnershipDiscussion: QbicleDiscussion
    {

        public B2BPartnershipDiscussion()
        {
            this.DiscussionType = DiscussionTypeEnum.B2BPartnershipDiscussion;
        }

        public virtual B2BRelationship Relationship { get; set; }
    }
}
