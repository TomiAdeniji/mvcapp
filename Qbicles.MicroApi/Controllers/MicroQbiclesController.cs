using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using static Qbicles.Models.QbicleActivity;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroQbiclesController : BaseApiController
    {
        [Route("qbicle")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetQbiclesByDomainId(int domainId)
        {
            HeaderInformation(Request);
            try
            {
                //var domainKey = new DomainRules(_microContext.Context).GetBusinessProfileUnwizard(domainId.Encrypt(), true);
                //if (string.IsNullOrEmpty(domainKey))
                //{
                //    var microQbicles = new MicroQbiclesRules(_microContext).GetQbiclesByDomainId(domainId);
                //    return Request.CreateResponse(HttpStatusCode.OK, microQbicles, Configuration.Formatters.JsonFormatter);
                //}

                //return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Key = domainKey }, Configuration.Formatters.JsonFormatter);
                var microQbicles = new MicroQbiclesRules(_microContext).GetQbiclesByDomainId(domainId);
                return Request.CreateResponse(HttpStatusCode.OK, microQbicles, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("qbiclebyid")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetQbiclesById(int id)
        {
            HeaderInformation(Request);
            try
            {


                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);
                var microQbicle = new MicroQbiclesRules(_microContext).GetQbicleById(id);
                return Request.CreateResponse(HttpStatusCode.OK, microQbicle, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        [Route("qbiclemember")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetQbicleMembers(int qbicleId)
        {
            HeaderInformation(Request);
            try
            {


                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);
                var microQbicles = new MicroQbiclesRules(_microContext).QbicleMemebers(qbicleId, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, microQbicles, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("qbicleshowhide")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage QbicleShowHide(int qbicleId, bool isHidden)
        {
            HeaderInformation(Request);
            try
            {


                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);
                var result = new MicroQbiclesRules(_microContext).ShowOrHideQbicle(qbicleId, isHidden);
                if (result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("qbiclefilter")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage FilterQbicle(QbicleSearchParameter qbicleParameters)
        {
            HeaderInformation(Request);
            try
            {


                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);
                qbicleParameters.UserId = _microContext.UserId;
                var microQbicles = new MicroQbiclesRules(_microContext).FilterQbilce(qbicleParameters);
                return Request.CreateResponse(HttpStatusCode.OK, microQbicles, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("qbicle")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage CreateQbicle(MicroQbicle qbicle)
        {
            HeaderInformation(Request);
            try
            {


                if (_authorizationInformation.Status != HttpStatusCode.OK)
                    return Request.CreateResponse(_authorizationInformation.Status, _authorizationInformation, Configuration.Formatters.JsonFormatter);

                var rules = new MicroQbiclesRules(_microContext);
                var created = rules.CreateUpdateQbicle(qbicle, _microContext.UserId);

                if (!created.result)
                {
                    if (created.actionVal == 1)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Name of Qbicle already existed." }, Configuration.Formatters.JsonFormatter);
                    else if (created.actionVal == 2)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = created.msg }, Configuration.Formatters.JsonFormatter);
                }

                var newUserGuests = (List<ApplicationUser>)created.Object;

                var emailRules = new EmailRules(_microContext.Context);

                //send email to guest invited
                foreach (var guest in newUserGuests)
                {
                    rules.SednEmailInvite(guest, qbicle.Id, ActivityTypeEnum.QbicleActivity);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { Id = created.result }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

    }
}
