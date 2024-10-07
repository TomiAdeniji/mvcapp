using System.Collections.Generic;
using System.ComponentModel;

namespace Qbicles.BusinessRules.Model.Firebase
{
    public class DevicesToken
    {
        public List<string> Tokens { get; set; }
    }
    public class FireBaseMessage
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string CallbackMethod { get; set; }
        public string CallbackApi { get; set; }
        public string Parameter { get; set; }
        public object ParameterValue { get; set; }
        public List<string> Tokens { get; set; }
        public MessageType Type { get; set; }
    }

    public enum MessageType
    {
        [Description("NewOrder")]
        NewOrder = 0,
        [Description("ChangeOrderStatus")]
        ChangeOrderStatus = 1
    }
}
