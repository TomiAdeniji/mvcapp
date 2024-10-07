using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Settings
{
    [Table("trad_groupsetting")]
    public class GroupSetting
    {
        public int Id { get; set; }
        public virtual MasterSetup Master { get; set; }
        /// <summary>
        /// This is value save the settings and And save the data type as JSON
        /// </summary>
        public string SettingsValue { get; set; }
        public SettingTypeEnum SettingType { get; set; }
        public enum SettingTypeEnum
        {
            [Description("Item I buy")]
            IBuy =1,
            [Description("Item I buy & sell")]
            IBuySell =2,
            [Description("Item I sell (Compound)")]
            ISellSCompound =3,
            [Description("Item I sell (service)")]
            ISellService =4
    }
    }
}
