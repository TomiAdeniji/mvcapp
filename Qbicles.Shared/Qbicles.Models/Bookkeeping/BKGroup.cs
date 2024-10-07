using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Bookkeeping
{
    [Table("bk_group")]
    public class BKGroup : CoANode
    {
        public BKGroup()
        {
            this.NodeType = BKCoANodeTypeEnum.Group;
        }

    }
}