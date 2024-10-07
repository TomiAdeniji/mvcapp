using Qbicles.Models.Base;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.PoS;
using System.ComponentModel.DataAnnotations.Schema;

namespace Qbicles.Models.Firebase
{
    [Table("apps_firebasenotificationstokens")]
    public class FirebaseNotificationToken : DataModelBase
    {
        public int DeviceId { get; set; }
        public string DeviceSerialNumber { get; set; }
        public string DeviceName { get; set; }
        public TraderLocation Location { get; set; }
        public DeviceUser User { get; set; }
        public string FirebaseToken { get; set; }
    }
}
