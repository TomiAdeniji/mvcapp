using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.MicroApi;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/domain/business")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroDomainBusinessProfileController : BaseApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Domain key</param>
        /// <returns></returns>
        [Route("wizard/launched")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetMicroFirstLaunched(string key)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroDomainBusinessProfileRules(_microContext).GetMicroFirstLaunchedBusiness(key);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("wizard/about")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage BusinessWizardAbout(B2BProfileWizardModel profile)
        {
            HeaderInformation(Request);
            try
            {
                var refmodel = new MicroDomainBusinessProfileRules(_microContext).BusinessWizardAbout(profile);
                if (refmodel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { ProfileId = refmodel.Object }, Configuration.Formatters.JsonFormatter);
                else
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refmodel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("wizard/areas")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage BusinessWizardAreas(B2BProfileWizardModel profile)
        {
            HeaderInformation(Request);
            try
            {
                var refmodel = new MicroDomainBusinessProfileRules(_microContext).BusinessWizardAreas(profile);
                if (refmodel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                else
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = refmodel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("wizard/users")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage BusinessWizardUsers(string key, string email)
        {
            HeaderInformation(Request);
            try
            {
                var domainId = int.Parse(key.Decrypt());
                var invite = new UserRules(_microContext.Context).InviteUserJoinQbicles(email, "", domainId, GenerateCallbackUrl(), _microContext.UserId, false);
                if (invite.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = invite.msg }, Configuration.Formatters.JsonFormatter);


            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("wizard/done")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage BusinessWizardDone(int id)
        {
            HeaderInformation(Request);
            try
            {
                new MicroDomainBusinessProfileRules(_microContext).BusinessWizardDone(id, false);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }



        [Route("profile")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage MicroBusinessProfile(string domainKey)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroDomainBusinessProfileRules(_microContext).MicroBusinessProfile(domainKey, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("public/pages")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage BusinessPages(string domainKey)
        {
            HeaderInformation(Request);
            
            var url = $"{ConfigManager.QbiclesUrl}/Account/BusinessPublicPages?domainKey={domainKey}";
            //var publicPages = new BaseHttpClient(Request.Headers.Authorization.Parameter).Get<List<BusinessRules.Micro.Model.BusinessPageModel>>(url);
            var parameter = new BusinessPageParameter { DomainKey = domainKey, Token = Request.Headers.Authorization.Parameter };
            var publicPages = new BaseHttpClient().Post<BusinessPageParameter,List<BusinessPageModel>>(url, parameter);
            
            return Request.CreateResponse(HttpStatusCode.OK, publicPages, Configuration.Formatters.JsonFormatter);
        }

        /// <summary>
        ///  GetBusinessDomainLevel DomainPlans
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        [Route("domain/level")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetBusinessDomainLevel(string domainKey)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroDomainBusinessProfileRules(_microContext).GetBusinessDomainLevel(domainKey);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


    }
}