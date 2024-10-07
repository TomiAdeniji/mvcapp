using System;

namespace Qbicles.Models.Broadcast
{
    public class BroadcastMessage
    {
        /// <summary>
        /// This's connection id, response from SignalR server while user login
        /// If has value, then do not sent notification to the connection, frontend automaticly render UI activity
        /// </summary>
        public string OriginatingConnectionId { get; set; } = string.Empty;
        /// <summary>
        /// connection id,(user created notification) using for case a user log in to multi app
        /// for check on signalR if current connection != originaling creation id then reload
        /// </summary>
        public string OriginatingCreationId { get; set; } = string.Empty;
        public string UserReceived { get; set; }
        public int NotificationId { get; set; }
        public object TradeOder { get; set; }
        public string CreatedId { get; set; }
        public string CreatedEmail { get; set; }
        public string CreatedName { get; set; }
        public Uri CreatedByIcon { get; set; }
        public Notification.NotificationEventEnum BroadcastEvent { get; set; }
    }
}