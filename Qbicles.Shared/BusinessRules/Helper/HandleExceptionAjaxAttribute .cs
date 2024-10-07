using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qbicles.BusinessRules.Helper
{
    public class HandleExceptionAjaxAttribute : HandleErrorAttribute
    {
        public HttpStatusCode httpStatusCode { get; set; }
        public HandleExceptionAjaxAttribute(HttpStatusCode statusCode)
        {
            httpStatusCode = statusCode;
        }
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.Exception != null)
            {
                filterContext.HttpContext.Response.StatusCode = (int)httpStatusCode;
                filterContext.Result = new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        filterContext.Exception.Message,
                        filterContext.Exception.StackTrace
                    }
                };
                filterContext.ExceptionHandled = true;
            }
            else
            {
                base.OnException(filterContext);
            }
        }
    }
}
