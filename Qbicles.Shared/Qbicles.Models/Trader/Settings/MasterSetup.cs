using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.Models.Trader.Settings
{
    [Table("trad_mastersetup")]
    public class MasterSetup
    {
        public int Id { get; set; }
        public virtual QbicleDomain Domain { get; set; }
        public virtual TraderGroup TraderGroup { get; set; }
        public virtual List<GroupSetting> GroupSettings { get; set; } = new List<GroupSetting>();
        /// <summary>
        /// The user from Qbicles who last updated the OperatorWorkGroup, 
        /// This is to be set each time the OperatorWorkGroup is saved.
        /// So, the first setting will equal the CreatedBy
        /// </summary>
        public virtual ApplicationUser LastUpdatedBy { get; set; }

        /// <summary>
        /// This is the date and time on which this OperatorWorkGroup was last edited.
        /// This is to be set each time the OperatorWorkGroup is saved.
        /// So, the first setting will equal the CreatedDate
        /// </summary>
        public DateTime LastUpdateDate { get; set; }
    }
}
