using Qbicles.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Trader.DDS
{
    [Table("dds_systemsettings")]
    public class DeliverySystemSetting: DataModelBase
    {
        /// <summary>
        /// Time to update location (SECOND)
        /// </summary>
        public int DriverUpdateLocationTimeInterval { get; set; } = 5;
    }

}
