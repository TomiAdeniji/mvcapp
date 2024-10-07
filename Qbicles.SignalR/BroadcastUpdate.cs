using Microsoft.AspNet.SignalR;
using Qbicles.Models.Broadcast;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace SignalRServer
{
    /// <summary>
    /// SignalR proxy definition for the qbicle broadcast service.
    /// Endpoints  are called from the main QbicleService whenever a push notification needs
    /// to be broadcast to the clients.  Note there are different endpoints depending on the activity type.  Not sure if this
    /// is a better way of doing it or to merge the messages into one type.
    /// </summary>
    //TODO SignalR requirement: At the moment this class broadcasts ALL new messages to ALL clients that are currentl active.  This may be very inneficien as it will broadcast messages
    // for qbicles that the user is not monitoring
    // For the final version we need to personalise this more by either recording the tasks of interest or the qbicle the user is currently looking at and filtering at this end
    // which messages are broadcast
    public class BroadcastUpdate : Hub
    {
        static List<UserConnected> ConnectedUsers = new List<UserConnected>();

        //[Authorize]
        public void NotificationBroadcast(BroadcastMessage broadcast)
        {
            if (broadcast.UsersReceived != null)
            {
                foreach (var sendTo in broadcast.UsersReceived)
                {
                    var connList = ConnectedUsers.Where(u => u.UserName == sendTo.UserReceived).ToList();
                    if (connList.Count > 0)
                        foreach (var item in connList)
                        {
                            Clients.Client(item.ConnectionId).updateNotifications(
                                broadcast.Event, broadcast.htmlBroadcast,
                                broadcast.AppendToPageId, broadcast.AppendToPageName, broadcast.CreatedById, broadcast.CreatedByName, broadcast.ListMemberid, broadcast.CurrentQbicleId, broadcast.CurrentDomainId);
                        }
                }
            }
            else if (broadcast.UserReceived != "")
            {
                var connList = ConnectedUsers.Where(u => u.UserName == broadcast.UserReceived).ToList();
                if (connList.Count > 0)
                    foreach (var item in connList)
                    {
                        Clients.Client(item.ConnectionId).updateNotifications(
                            broadcast.Event, broadcast.htmlBroadcast,
                            broadcast.AppendToPageId, broadcast.AppendToPageName, broadcast.CreatedById, broadcast.CreatedByName, broadcast.ListMemberid, broadcast.CurrentQbicleId, broadcast.CurrentDomainId);
                    }
            }
        }

        public string getUserIdByClient()
        {
            return Context.QueryString["CurrentUserId"].ToString();
        }
        /// <summary>
        /// Push new message to all clients
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(BroadcastMessage message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(message);
        }

        /// <summary>
        /// Push new task to all clients
        /// </summary>
        /// <param name="task"></param>
        public void SendTask(BroadcastTask task)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastTask(task);
        }

        //manage user connect
        //[Authorize]
        public void Connect(string userName)
        {
            var id = Context.ConnectionId;

            if (ConnectedUsers.Count(x => x.ConnectionId == id) == 0)
            {
                ConnectedUsers.Add(new UserConnected { ConnectionId = id, UserName = userName });

                // send to caller
                Clients.Caller.onConnected(id, userName, ConnectedUsers);

                // send to all except caller client
                Clients.AllExcept(id).onNewUserConnected(id, userName);

            }

        }
        public override Task OnDisconnected(bool stopCall)
        {
            var item = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (item != null)
            {
                ConnectedUsers.Remove(item);

                var id = Context.ConnectionId;
                Clients.All.onUserDisconnected(id, item.UserName);

            }

            return base.OnDisconnected(stopCall);
        }

    }
}