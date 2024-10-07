using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.Models.TraderApi;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/home")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroHomeController : BaseApiController
    {
        [Route("firstlaunched")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage MicroFirstLaunchedValidation()
        {
            HeaderInformation(Request);
            try
            {

                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                var refModel = new MicroHomeRules(_microContext).MicroFirstLaunchedValidation();
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("splash")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage LoginSplashInterstitial()
        {
            HeaderInformation(Request);
            try
            {

                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                var refModel = new MicroHomeRules(_microContext).GetLoginSplashInterstitial();
                if (refModel.DomainAction == BusinessRules.Micro.Model.LoginSplashInterstitialAction.UserProfileWizard)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Setup user profile wizard first." }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }



        [Route("routes")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Update(RoutesModel ds)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHomeRules(_microContext).Update(ds);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("routes/delete")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Delete(RoutesModel ds)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHomeRules(_microContext).Delete(ds);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        [Route("routes/delete/all")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DeleteAll()
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroHomeRules(_microContext).DeleteAll();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
    }
}