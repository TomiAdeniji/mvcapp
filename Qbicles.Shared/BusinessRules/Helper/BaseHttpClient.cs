using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Qbicles.BusinessRules.Helper
{
    public class BaseHttpClient
    {
        private string accessToken = "";
        public BaseHttpClient()
        {
        }
        public BaseHttpClient(string authToken)
        {
            accessToken = authToken;
        }

        public T Get<T>(string url)
        {
            var request = CreateRequest(url, HttpMethod.Get);
            return SendRequest<T>(request);
        }

        public HttpResponseMessage Get(string url)
        {
            var request = CreateRequest(url, HttpMethod.Get);
            return SendRequest(request);
        }

        /// <summary>
        /// Post<objInput, objResult>(url, dataInput);
        /// </summary>
        /// <typeparam name="TInput">objInput</typeparam>
        /// <typeparam name="TResult">objResult</typeparam>
        /// <param name="url">url</param>
        /// <param name="input">dataInput</param>
        /// <returns></returns>
        public TResult Post<TInput, TResult>(string url, TInput input)
        {
            var request = CreateRequest(url, HttpMethod.Post, input);
            return SendRequest<TResult>(request);
        }
        /// <summary>
        /// Post(url, item);
        /// </summary>
        /// <typeparam name="TInput">objInput</typeparam>
        /// <param name="url">url</param>
        /// <param name="input">dataInput</param>
        /// <returns></returns>
        public HttpResponseMessage Post<TInput>(string url, TInput input)
        {
            var request = CreateRequest(url, HttpMethod.Post, input);
            return SendRequest(request);
        }
        /// <summary>
        /// Post<Content>(url);
        /// </summary>
        /// <typeparam name="TInput">objInput</typeparam>
        /// <param name="url">url</param>
        /// <returns></returns>
        public TResult Post<TResult>(string url)
        {
            var request = CreateRequest(url, HttpMethod.Post);
            return SendRequest<TResult>(request);
        }
        /// <summary>
        /// Post(url);
        /// </summary>
        /// <param name="url">url</param>
        /// <returns></returns>
        public HttpResponseMessage Post(string url)
        {
            var request = CreateRequest(url, HttpMethod.Post);
            return SendRequest(request);
        }

        protected virtual HttpResponseMessage CheckResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var responseContent = response.Content.ReadAsStringAsync().Result;

            throw new Exception(responseContent);
        }

        #region Helper

        protected HttpRequestMessage CreateRequest(string url, HttpMethod method)
        {
            var request = new HttpRequestMessage(method, url);
            return request;
        }

        private HttpRequestMessage CreateRequest<TInput>(string url, HttpMethod method, TInput input)
        {
            var request = new HttpRequestMessage(method, url)
            {
                Content = input.ToJsonStringContent()
            };

            return request;
        }

        private HttpResponseMessage SendRequest(HttpRequestMessage request)
        {
            using (var client = CreateClient())
            {
                var response = client.SendAsync(request).Result;
                try
                {
                    return CheckResponse(response);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private TResult SendRequest<TResult>(HttpRequestMessage request)
        {
            //var client = await CreateClient();
            using (var client = CreateClient())
            {
                var response = client.SendAsync(request).Result;
                try
                {
                    return CheckResponse(response).Content.ReadAs<TResult>();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }


        private HttpClient CreateClient()
        {
            var httpHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                // CookieContainer = cookieContainer
            };
            var client = new HttpClient(httpHandler);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            if (!string.IsNullOrEmpty(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return client;
        }
    }


    #endregion

}

//public class BaseHttpClient
//{
//    private readonly string cookieUri = ConfigManager.DocumentsApi;
//    private string cookieToken;
//    public BaseHttpClient(EndpointApi endpoint)
//    {
//        switch (endpoint)
//        {
//            case EndpointApi.DocApi:
//                cookieUri = ConfigManager.DocumentsApi;
//                break;
//            case EndpointApi.TraderApi:
//                cookieUri = ConfigManager.TraderApi;
//                break;
//            case EndpointApi.QbiclesJobApi:
//                cookieUri = ConfigManager.QbiclesJobApi;
//                break;
//            case EndpointApi.SignalRApi:
//                cookieUri = ConfigManager.SignalRApi;
//                break;
//            default:
//                throw new ArgumentOutOfRangeException(nameof(endpoint), endpoint, null);
//        }
//    }
//    public HttpResponseMessage Get(string url)
//    {
//        var request = CreateRequest(url, HttpMethod.Get);
//        return SendRequest(request);
//    }

//    public T GetMediaHangfire<T>(string url, string token)
//    {
//        cookieToken = token;
//        var request = CreateRequest(url, HttpMethod.Get);
//        return SendRequest<T>(request);
//    }

//    public HttpResponseMessage GetMediaHangfire(string url, string token)
//    {
//        cookieToken = token;
//        var request = CreateRequest(url, HttpMethod.Get);
//        return SendRequest(request);
//    }

//    public T Get<T>(string url)
//    {
//        var request = CreateRequest(url, HttpMethod.Get);
//        return SendRequest<T>(request);
//    }

//    public TResult Post<TInput, TResult>(string url, TInput input)
//    {
//        var request = CreateRequest(url, HttpMethod.Post, input);
//        return SendRequest<TResult>(request);
//    }


//    protected virtual HttpResponseMessage CheckResponse(HttpResponseMessage response)
//    {
//        if (response.IsSuccessStatusCode)
//        {
//            return response;
//        }

//        var responseContent = response.Content.ReadAsStringAsync().Result;

//        throw new Exception(responseContent);
//    }

//    #region Helper

//    protected HttpRequestMessage CreateRequest(string url, HttpMethod method)
//    {
//        var request = new HttpRequestMessage(method, url);
//        return request;
//    }

//    private HttpRequestMessage CreateRequest<TInput>(string url, HttpMethod method, TInput input)
//    {
//        var request = new HttpRequestMessage(method, url)
//        {
//            Content = input.ToJsonStringContent()
//        };

//        return request;
//    }

//    private HttpResponseMessage SendRequest(HttpRequestMessage request)
//    {
//        using (var client = CreateClient())
//        {
//            var response = client.SendAsync(request).Result;
//            try
//            {
//                return CheckResponse(response);
//            }
//            catch (Exception e)
//            {
//                throw e;
//            }
//        }
//    }

//    private TResult SendRequest<TResult>(HttpRequestMessage request)
//    {
//        using (var client = CreateClient())
//        {
//            var response = client.SendAsync(request).Result;
//            try
//            {
//                return CheckResponse(response).Content.ReadAs<TResult>();
//            }
//            catch (Exception e)
//            {
//                throw e;
//            }
//        }
//    }


//    private HttpClient CreateClient()
//    {
//        var cookie = new Cookie
//        {
//            Name = "AuthCookie",
//            Value = "AuthCookie"
//        };
//        if (!string.IsNullOrEmpty(cookieToken))
//            cookie = cookieToken.DecryptCookie();
//        else if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Session?["AuthCookie"] != null)
//            cookie = (Cookie)System.Web.HttpContext.Current.Session["AuthCookie"];

//        //if (string.IsNullOrEmpty(cookieToken))
//        //{
//        //    cookie = (Cookie)System.Web.HttpContext.Current.Session["AuthCookie"];
//        //}
//        //else
//        //{
//        //    cookie = cookieToken.DecryptCookie();
//        //}
//        var cookieContainer = new CookieContainer();
//        //cookieContainer.Add(new Uri($"{ConfigManager.DocumentsApi}"), cookie);
//        cookieContainer.Add(new Uri(cookieUri), cookie);
//        var httpHandler = new HttpClientHandler
//        {
//            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
//            CookieContainer = cookieContainer
//        };

//        var client = new HttpClient(httpHandler);
//        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

//        return client;
//    }


//    #endregion

//}

//public static class CookieManage
//{
//    public static string EncryptCookie(this Cookie cookie)
//    {
//        return cookie.ToJson().Encrypt();
//    }

//    public static Cookie DecryptCookie(this string cookie)
//    {
//        var decrypt = cookie.Decrypt();
//        return decrypt.ParseAs<Cookie>();
//    }
//}
