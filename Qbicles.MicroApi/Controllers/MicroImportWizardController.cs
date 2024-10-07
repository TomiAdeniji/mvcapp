using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.MicroApi;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/community")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroImportWizardController : BaseApiController
    {


        /// <summary>
        /// Mapping contact from Phone and contact in Qbicles
        /// </summary>
        /// <param name="phoneContacts"></param>
        /// <returns></returns>
        [Route("import/contact")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ImportPhoneContacts(List<MicroContact> phoneContacts)
        {
            HeaderInformation(Request);

            var refModel = new MicroCommunityRules(_microContext).ImportPhoneContacts(phoneContacts);
            return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

        }

        /// <summary>
        /// Connect contact to C2C
        /// </summary>
        /// <param name="contacts"></param>
        /// <returns></returns>
        [Route("import/connect")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage Connect2Contacts(List<MicroContact> contacts)
        {
            HeaderInformation(Request);

            new MicroCommunityRules(_microContext).Connect2Contacts(contacts);
            return Request.CreateResponse(HttpStatusCode.OK);

        }

        [Route("import/invite")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SendEmailImportInvite(List<MicroContact> contacts)
        {
            HeaderInformation(Request);

            new MicroCommunityRules(_microContext).SendEmailImportInvite(contacts);
            return Request.CreateResponse(HttpStatusCode.OK);

        }


        [Route("import/complete")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ImportWizardComplete()
        {
            HeaderInformation(Request);

            new MicroUserProfileSetupWizardRules(_microContext).FinisUserImportWizard(2);
            return Request.CreateResponse(HttpStatusCode.OK);

        }

        [Route("import/skip")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ImportWizardSkip()
        {
            HeaderInformation(Request);

            new MicroUserProfileSetupWizardRules(_microContext).FinisUserImportWizard(1);
            return Request.CreateResponse(HttpStatusCode.OK);

        }
    }
}