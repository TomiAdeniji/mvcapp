using System;

namespace Qbicles.Models.TraderApi
{
    /// <summary>
    /// Parameter use in API
    /// </summary>
    public class PosRequest
    {
        public string ApiControllerName { get; set; }
        public string ApiActionName { get; set; }
        public string UserId { get; set; }
        public int DeviceId { get; set; }
        public bool IsTokenValid { get; set; }
        public string AccessToken { get; set; }
        public string SerialNumber { get; set; }
        public string DeviceName { get; set; }
        public string Header { get; set; }
        public System.Net.HttpStatusCode Status { get; set; }
        public string Message { get; set; }
        public ClientIdType ClientId { get; set; }
        public DateTime ExpireTime { get; set; }
    }

    public enum ClientIdType
    {
        PosUser = 1,
        PosSerial = 2,
        PosDriver = 3,
        MicroClient = 4
    }
}
