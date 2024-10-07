using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Provider;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Helper
{
    public class ServiceToken
    {
        public async Task<string> GetTokenAsync()
        {
            var objToken = JwtProvider.GetSystemToken();
            if (objToken == null || objToken.ExpireTime < DateTime.UtcNow)
            {
                var jwtProvider = JwtProvider.Create(ConfigManager.AuthHost);
                return await jwtProvider.GetServiceTokenAsync();
            }
            else
                return objToken.Token;
        }

        public async Task<QbicleJobResult> ExcecuteJobAsync(object job, string endPointName)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var webUrl = $"{ConfigManager.QbiclesJobApi}/api/job/{endPointName}";
                    //Init get Token from IdentityProvider
                    var token = await GetTokenAsync();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    //End
                    var content = new StringContent(JsonConvert.SerializeObject(job), Encoding.UTF8, "application/json");
                    var result = await client.PostAsync(new Uri(webUrl), content);
                    var sResponseObject = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<QbicleJobResult>(sResponseObject);
                }
                catch (Exception ex)
                {
                    return new QbicleJobResult { Status = System.Net.HttpStatusCode.InternalServerError, Message = endPointName +" error:"+ ex.Message, JobId = "-1" };
                }

            }
        }

        public async Task<HubConnection> GetHubConnection()
        {
            var hubConnection = new HubConnection(ConfigManager.HubUrl);
            //Init get Token from IdentityProvider
            var token = await GetTokenAsync();
            hubConnection.Headers.Add("Authorization", "Bearer " + token);
            //End
            return hubConnection;
        }


        //public async Task<QbicleJobResult> ExcecuteJobAsync(object job, string endPointName)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        var webUrl = $"{ConfigManager.QbiclesJobApi}/api/job/{endPointName}";
        //        //Init get Token from IdentityProvider
        //        var token = await GetTokenAsync();
        //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //        //End
        //        var content = new StringContent(JsonConvert.SerializeObject(job), Encoding.UTF8, "application/json");
        //        var result = await client.PostAsync(new Uri(webUrl), content);
        //        var sResponseObject = await result.Content.ReadAsStringAsync();
        //        return JsonConvert.DeserializeObject<QbicleJobResult>(sResponseObject);
        //    }
        //}
    }
}
