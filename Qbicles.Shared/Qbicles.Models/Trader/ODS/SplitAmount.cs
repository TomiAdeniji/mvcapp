using Qbicles.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.ODS
{
    [Table("ods_SplitAmount")]
    public class SplitAmount : DataModelBase
    {
        public int SplitNo { get; set; }
        public decimal Amount { get; set; }
        public virtual QueueOrder QueueOrder { get; set; }
    }
}
