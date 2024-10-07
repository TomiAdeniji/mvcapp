using Microsoft.Owin;
using Qbicles.TraderAPI.Authentication;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Qbicles.TraderAPI.Helper
{
    public enum TimeUnit
    {
        Minute = 60,
        Hour = 3600,
        Day = 86400
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ThrottleApiAttribute : ActionFilterAttribute
    {
        public TimeUnit TimeUnit { get; set; }
        public int Count { get; set; }

        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            var seconds = Convert.ToInt32(TimeUnit);

            var key = string.Join(
                "-",
                seconds,
                filterContext.Request.Method,
                filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                filterContext.ActionDescriptor.ActionName,
                GetClientIpAddress(filterContext.Request)
            );

            // increment the cache value
            var cnt = 1;
            if (HttpRuntime.Cache[key] != null)
            {
                cnt = (int)HttpRuntime.Cache[key] + 1;
            }
            HttpRuntime.Cache.Insert(
                key,
                cnt,
                null,
                DateTime.UtcNow.AddSeconds(seconds),
                Cache.NoSlidingExpiration,
                CacheItemPriority.Low,
                null
            );

            if (cnt > Count)
            {
                filterContext.Response = new HttpResponseMessage
                {
                    Content = new StringContent($"You are allowed to make only {Count} requests per {TimeUnit.ToString().ToLower()}")
                };
                filterContext.Response.StatusCode = (HttpStatusCode)429; //To Many Requests
            }
        }

        private string GetClientIpAddress(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return IPAddress.Parse(((HttpContextBase)request.Properties["MS_HttpContext"]).Request.UserHostAddress).ToString();
            }
            if (request.Properties.ContainsKey("MS_OwinContext"))
            {
                return IPAddress.Parse(((OwinContext)request.Properties["MS_OwinContext"]).Request.RemoteIpAddress).ToString();
            }
            return String.Empty;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ApiRequestAuthorizationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            //var validationResponse = ValidateAccessToken.ValidateHeader(filterContext.Request);
            //if (validationResponse.Status != HttpStatusCode.OK)
            //{
            //    filterContext.Response = new HttpResponseMessage
            //    {
            //        Content = new StringContent(validationResponse.Message)
            //    };
            //    filterContext.Response.StatusCode = validationResponse.Status;
            //}
            ValidateAccessToken.ValidateHeader(filterContext.Request);
        }


        //private IPosResult StatusCodeValidation(PosRequest tokenValid, ClientIdType clientAccept)
        //{
        //    if (!tokenValid.IsTokenValid)
        //        return new IPosResult
        //        {
        //            Status = tokenValid.Status,
        //            Message = tokenValid.Message,
        //            IsTokenValid = tokenValid.IsTokenValid
        //        };

        //    switch (clientAccept)
        //    {
        //        case ClientIdType.PosUser:
        //            if (tokenValid.ClientId != ClientIdType.PosUser)
        //                return new IPosResult
        //                {
        //                    Status = HttpStatusCode.NotAcceptable,
        //                    Message = ResourcesManager._L("ERROR_NOTACCEPTABLE", "Client type"),
        //                    IsTokenValid = false
        //                };
        //            break;
        //        case ClientIdType.PosSerial:
        //            if (tokenValid.ClientId != ClientIdType.PosSerial)
        //                return new IPosResult
        //                {
        //                    Status = HttpStatusCode.NotAcceptable,
        //                    Message = ResourcesManager._L("ERROR_NOTACCEPTABLE", "Client type"),
        //                    IsTokenValid = false
        //                };
        //            break;
        //        case ClientIdType.PosDriver:
        //            if (tokenValid.ClientId != ClientIdType.PosDriver)
        //                return new IPosResult
        //                {
        //                    Status = HttpStatusCode.NotAcceptable,
        //                    Message = ResourcesManager._L("ERROR_NOTACCEPTABLE", "Client type"),
        //                    IsTokenValid = false
        //                };
        //            break;
        //        default:
        //            return new IPosResult
        //            {
        //                Status = HttpStatusCode.NotAcceptable,
        //                Message = ResourcesManager._L("ERROR_NOTACCEPTABLE", "Client type"),
        //                IsTokenValid = false
        //            };
        //    }
        //    return new IPosResult
        //    {
        //        Status = HttpStatusCode.OK
        //    };
        //}
        //private IPosResult StatusCodeValidation(PosRequest tokenValid)
        //{
        //    if (!tokenValid.IsTokenValid)
        //        return new IPosResult
        //        {
        //            Status = tokenValid.Status,
        //            Message = tokenValid.Message,
        //            IsTokenValid = tokenValid.IsTokenValid
        //        };


        //    if (tokenValid.ClientId != ClientIdType.PosUser
        //        && tokenValid.ClientId != ClientIdType.PosDriver
        //        && tokenValid.ClientId != ClientIdType.PosSerial
        //        && tokenValid.ClientId != ClientIdType.MicroClient)
        //        return new IPosResult
        //        {
        //            Status = HttpStatusCode.NotAcceptable,
        //            Message = ResourcesManager._L("ERROR_NOTACCEPTABLE", "Client type"),
        //            IsTokenValid = false
        //        };

        //    return new IPosResult
        //    {
        //        Status = HttpStatusCode.OK
        //    };
        //}
    }

}