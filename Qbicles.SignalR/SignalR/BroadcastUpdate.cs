using Microsoft.AspNet.SignalR;
using Qbicles.Models.Broadcast;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Microsoft.AspNet.Identity.CoreCompat;
using Qbicles.BusinessRules.Helper;
using System.Data.Entity;
using System.Security.Claims;
using System.Transactions;

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
    [Authorize]
    public class BroadcastUpdate : Hub
    {
        public void NotificationBroadcast(BroadcastMessage broadcast)
        {
            using (var db = new ApplicationDbContext())
            {
                if (string.IsNullOrEmpty(broadcast.UserReceived)) return;

                var connected = db.ConnectedSignalRs.Find(broadcast.UserReceived);
                if (connected == null) return;

                db.Entry(connected).Collection(u => u.Connections).Load();
                if (!connected.Connections.Any()) return;

                var connections = connected.Connections;

                if (!string.IsNullOrEmpty(broadcast.OriginatingConnectionId))
                    connections = connections.Where(e => e.ConnectionID != broadcast.OriginatingConnectionId).ToList();

                foreach (var item in connections)
                {
                    Clients.Client(item.ConnectionID).updateNotifications(broadcast);
                }
            }
        }

        //[Authorize]
        public string getUserIdByClient()
        {
            return Context.QueryString["CurrentUserId"].ToString();
        }

        /// <summary>
        /// Push new message to all clients
        /// </summary>
        /// <param name="message"></param>
        //[Authorize]
        public void SendMessage(BroadcastMessage message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(message);
        }

        /// <summary>
        /// Push new task to all clients
        /// </summary>
        /// <param name="task"></param>
        //[Authorize]
        public void SendTask(BroadcastTask task)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastTask(task);
        }

        //manage user connect
        //[Authorize]

        public override Task OnConnected()
        {
            if (Context.User == null)
                return base.OnConnected();
            var identity = (ClaimsIdentity)Context.User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            if (claims != null && claims.Any())
            {
                string userId = claims.FirstOrDefault(s => s.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
                using (var db = new ApplicationDbContext())
                {
                    var userLog = new UserRules(db).GetUser(userId, 0);
                    if (userLog == null)
                        return base.OnConnected();
                    var connectedSignalR = db.ConnectedSignalRs.Include(u => u.Connections).SingleOrDefault(u => u.UserName == userLog.UserName);

                    if (connectedSignalR == null)
                    {
                        connectedSignalR = new UserConnectedSignalR
                        {
                            UserName = userLog.UserName,
                            Connections = new List<Connection>()
                        };
                        db.ConnectedSignalRs.Add(connectedSignalR);
                    }
                    if (connectedSignalR.Connections == null)
                        connectedSignalR.Connections = new List<Connection>();
                    connectedSignalR.Connections.Add(new Connection
                    {
                        ConnectionID = Context.ConnectionId,
                        UserAgent = Context.Request.Headers["User-Agent"]
                    });
                    db.SaveChanges();
                }
            }

            return base.OnConnected();
        }

        //[Authorize]
        public override Task OnDisconnected(bool stopCall)
        {
            using (var db = new ApplicationDbContext())
            {
                var connection = db.Connections.Find(Context.ConnectionId);
                if (connection != null)
                {
                    db.Connections.Remove(connection);
                    db.SaveChanges();
                }
            }
            return base.OnDisconnected(stopCall);
        }
    }
}