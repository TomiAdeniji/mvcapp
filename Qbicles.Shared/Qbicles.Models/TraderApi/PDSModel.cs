using System;
using Qbicles.Models.Trader.ODS;

namespace Qbicles.Models.TraderApi
{
    public class PdsOrderUpdate
    {
        public int Id { get; set; }
        public PrepQueueStatus Status { get; set; }
        public DateTime StatusChangeDateTime { get; set; }
    }


    public class PrepQueueModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PosDevices { get; set; }
        public string PdsDevices { get; set; }
        /// <summary>
        /// Delivery devices
        /// </summary>
        public string DdsDevices { get; set; }
        /// <summary>
        /// Delivery order queue( prep queue)
        /// </summary>
        public string OrderQueue { get; set; }

        public bool CanDelete { get; set; }
        public QueueType QueueType { get; set; }
    }

    public enum QueueType
    {
        All = 0,
        Order = 1,
        Delivery = 2
    }

    public class PdsFirebaseTokenUpdate
    {
        public string Token { get; set; }
    }
}
