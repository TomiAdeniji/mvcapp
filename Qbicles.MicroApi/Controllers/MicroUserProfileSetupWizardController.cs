using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.MicroApi;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/user/wizard")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroUserProfileSetupWizardController : BaseApiController
    {
        [Route("launched")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetMicroFirstLaunchedUser()
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfileSetupWizardRules(_microContext).GetMicroFirstLaunchedUser();
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("finsh/step")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage FinisStep(UserProfileWizard step)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfileSetupWizardRules(_microContext).FinisStep((MicroUserWizardStep)step.CurrentStepId);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("finsh")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage FinisUserProfileWizard()
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfileSetupWizardRules(_microContext).FinisUserProfileWizard();
                if (refModel)
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateResponse(HttpStatusCode.BadRequest, new { Message = "Contact or Showcase is required." }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

    }
}