using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_subgroup")]
    public class BKSubGroup : CoANode
    {

        public BKSubGroup()
        {
            this.NodeType = BKCoANodeTypeEnum.SubGroup;
        }

    }
}