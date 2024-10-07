using Newtonsoft.Json;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model.Firebase;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Firebase
{
    public static class FirebasePushNotification
    {

        //Server key from FCM console
        private static readonly string _serverKey = $"key={ConfigManager.FirebaseServerKey}";
        private static readonly string _senderId = $"id={ConfigManager.FirebaseSenderId}";
        private static readonly string _requestUri = ConfigManager.FirebaseRequestUri;

        public static void PushList(List<FireBaseMessage> fireBaseMessages)
        {
            fireBaseMessages.ForEach(message =>
            {
                Task firebase = new Task(async () =>
                {
                    await PushAsync(message);
                });
                firebase.Start();
            });

        }

        public static async Task<bool> PushAsync(FireBaseMessage fireBaseMessage)
        {
            try
            {
                var data = new Dictionary<string, string>()
                {
                    {"CallbackMethod" , fireBaseMessage.CallbackMethod},
                    {"CallbackApi" , fireBaseMessage.CallbackApi },
                    {"Parameter" , fireBaseMessage.Parameter },
                    {"ParameterValue" , fireBaseMessage.ParameterValue.ToString()},
                    {"MessageType", fireBaseMessage.Type.GetDescription() }
                };

                var message = new
                {
                    //to, // Recipient device token
                    registration_ids = fireBaseMessage.Tokens,
                    notification = new
                    {
                        fireBaseMessage.Title,
                        fireBaseMessage.Body,
                        sound = "notification05.mp3"
                    },
                    data = data
                };

                var messagePush = JsonConvert.SerializeObject(message);

                using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, _requestUri))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", _serverKey);
                    httpRequest.Headers.TryAddWithoutValidation("Sender", _senderId);
                    httpRequest.Content = new StringContent(messagePush, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        await httpClient.SendAsync(httpRequest);
                    }
                }
                FireBaseLog(fireBaseMessage);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, fireBaseMessage);
                FireBaseLog(fireBaseMessage, ex.Message);
            }


            return true;
        }

        private static void FireBaseLog(FireBaseMessage fireBaseMessage, string exception = "")
        {
            //Get the reference from the TradeOrder and add to the log labels
            dynamic logLabels = new System.Dynamic.ExpandoObject();
            logLabels.Title = fireBaseMessage.Title;
            logLabels.Body = fireBaseMessage.Body;
            logLabels.Parameter = fireBaseMessage.Parameter;
            logLabels.ParameterValue = fireBaseMessage.ParameterValue;
            logLabels.DeviceTokens = fireBaseMessage.Tokens;
            logLabels.Exception = exception;

            LogManager.ApplicationInfo(logLabels, "Starting order processing");
        }

        public static async Task<bool> PushAsync(string notificationContent)
        {
            try
            {

                using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, _requestUri))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", _serverKey);
                    httpRequest.Headers.TryAddWithoutValidation("Sender", _senderId);
                    httpRequest.Content = new StringContent(notificationContent, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        await httpClient.SendAsync(httpRequest);
                        //var result = await httpClient.SendAsync(httpRequest);

                        //if (result.IsSuccessStatusCode)
                        //{
                        //    return true;
                        //}
                        //else
                        //{
                        //    return false;
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
    }
}
