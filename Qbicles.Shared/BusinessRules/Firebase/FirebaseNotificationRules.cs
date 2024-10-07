using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Model.Firebase;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.TraderApi;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Firebase
{
    public class FirebaseNotificationRules
    {
        ApplicationDbContext dbContext;

        public FirebaseNotificationRules()
        {
            dbContext = new ApplicationDbContext();
        }
        public FirebaseNotificationRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public bool AddUpdateDeviceToken(PdsFirebaseTokenUpdate pdsFirebase, PosRequest request)
        {

            var dToken = dbContext.FirebaseNotificationTokens.FirstOrDefault(e => e.DeviceSerialNumber == request.SerialNumber);

            if (dToken != null)
            {
                var associatedDevice = dbContext.PrepDisplayDevices.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);
                if (associatedDevice != null)
                {
                    dToken.DeviceId = associatedDevice.Id;
                    dToken.DeviceName = associatedDevice.Name;
                }

                dToken.FirebaseToken = pdsFirebase.Token;
                dToken.User = dbContext.DeviceUsers.FirstOrDefault(e => e.User.Id == request.UserId);
                dbContext.SaveChanges();
                return true;
            }

            var device = dbContext.PrepDisplayDevices.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);
            if (device == null) return false;

            dToken = new Models.Firebase.FirebaseNotificationToken
            {
                DeviceId = device.Id,
                DeviceName = device.Name,
                DeviceSerialNumber = device.SerialNumber,
                FirebaseToken = pdsFirebase.Token,
                Location = device.Location,
                User = device.Users.FirstOrDefault(e => e.User.Id == request.UserId)
            };

            dbContext.FirebaseNotificationTokens.Add(dToken);

            dbContext.SaveChanges();


            return true;
        }

        public string GetDevicesToken(string serialNumber)
        {
            return dbContext.FirebaseNotificationTokens.FirstOrDefault(e => e.DeviceSerialNumber == serialNumber)?.FirebaseToken ?? string.Empty;
        }


        private List<string> GetDevicesToken(List<string> serialNumbers)
        {
            return dbContext.FirebaseNotificationTokens.AsNoTracking().Where(e => serialNumbers.Contains(e.DeviceSerialNumber)).Select(t => t.FirebaseToken).ToList();
        }

        public List<string> GetFirebaseToken(int queueId, List<string> serialNumbersExcluded = null)
        {
            var pdsDevices = dbContext.PrepDisplayDevices.AsNoTracking().Where(e => e.Queue.Id == queueId);

            if (serialNumbersExcluded != null && serialNumbersExcluded.Any())
                pdsDevices = pdsDevices.Where(e => !serialNumbersExcluded.Contains(e.SerialNumber));

            var serialNumbers = pdsDevices.Select(s => s.SerialNumber).ToList();
            var tokens = GetDevicesToken(serialNumbers);
            return tokens;
        }

    }

    public static class FirebasePushRules
    {
        /// <summary>
        /// PUSH to PDS in the case POS Cancel order
        /// </summary>
        /// <param name="queueOrder"></param>
        /// <param name="type"></param>
        /// <param name="title">MessageType: New/ChangeOrderStatus... </param>
        public static void PushCancelOrderNotification(this QueueOrder queueOrder, MessageType type, string title)
        {
            var prepQueueId = queueOrder.PrepQueue?.Id ?? 0;
            if (prepQueueId > 0)
            {
                var tokens = new FirebaseNotificationRules().GetFirebaseToken(prepQueueId);
                if (tokens.Any())
                {
                    var firebaseMessage = new FireBaseMessage
                    {
                        Title = title,
                        Body = $"OrderRef: {queueOrder.OrderRef}, Table: {queueOrder.Table}",
                        CallbackMethod = "GET",
                        Parameter = "id",
                        ParameterValue = queueOrder.Id.ToString(),
                        Tokens = tokens,
                        CallbackApi = "api/pds/queue/order",
                        Type = type
                    };

                    Task firebase = new Task(async () =>
                    {
                        await FirebasePushNotification.PushAsync(firebaseMessage);
                    });
                    firebase.Start();
                }
            };
        }



        /// <summary>
        /// Status update for order on the queue
        /// </summary>
        /// <param name="queueOrder"></param>
        /// <param name="serialNumbersExcluded"></param>
        public static void PushPdsStatusUpdate(this QueueOrder queueOrder, List<string> serialNumbersExcluded = null)
        {
            var prepQueueId = queueOrder.PrepQueue?.Id ?? 0;
            if (prepQueueId == 0)
                return;

            var tokens = new FirebaseNotificationRules().GetFirebaseToken(prepQueueId, serialNumbersExcluded);
            if (!tokens.Any())
                return;

            var firebaseMessage = new FireBaseMessage
            {
                Title = "Status update for order on the queue",
                Body = $"OrderRef: {queueOrder.OrderRef}, Table: {queueOrder.Table}",
                CallbackMethod = "GET",
                Parameter = "id",
                ParameterValue = queueOrder.Id.ToString(),
                Tokens = tokens,
                CallbackApi = "api/pds/queue/order",
                Type = MessageType.ChangeOrderStatus,
            };

            Task firebase = new Task(async () =>
            {
                await FirebasePushNotification.PushAsync(firebaseMessage);
            });
            firebase.Start();
        }


        /// <summary>
        /// New order on the queue
        /// </summary>
        /// <param name="queueOrder"></param>
        public static void PushPdsNewOrder(this QueueOrder queueOrder)
        {
            var prepQueueId = queueOrder.PrepQueue?.Id ?? 0;
            if (prepQueueId == 0) return;

            var tokens = new FirebaseNotificationRules().GetFirebaseToken(prepQueueId);
            if (tokens == null || tokens.Count == 0)
                return;

            var firebaseMessage = new FireBaseMessage
            {
                Title = "New order on the queue",
                Body = $"OrderRef: {queueOrder.OrderRef}, Table: {queueOrder.Table}",
                CallbackMethod = "GET",
                Parameter = "id",
                ParameterValue = queueOrder.Id.ToString(),
                Tokens = tokens,
                CallbackApi = "api/pds/queue/order",
                Type = MessageType.NewOrder
            };

            Task firebase = new Task(async () =>
            {
                await FirebasePushNotification.PushAsync(firebaseMessage);
            });
            firebase.Start();
        }
    }
}
