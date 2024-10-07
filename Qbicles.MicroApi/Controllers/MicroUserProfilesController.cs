using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Qbicles;
using Qbicles.Models.UserInformation;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro/userprofile")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroUserProfilesController : BaseApiController
    {
        [Route("option")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserProfileOption()
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserProfileOption();
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("preferredqbicles")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetPreferredQbiclesByDomainId(int domainId)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetPreferredQbiclesByDomainId(domainId);
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("user")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserProfile(string id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserProfile(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("sharedqbicles")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserSharedQbicles(string id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserSharedQbicles(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        #region User profile info
        [Route("changeavatar")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage ChangeUserAvatar(MediaModel media)
        {
            HeaderInformation(Request);
            try
            {
                new MicroUserProfilesRules(_microContext).ChangeUserAvatar(media);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("updategeneralinfo")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UpdateGeneralInfo(MicroUserProfile user)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).UpdateGeneralInfo(user);
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

        [Route("updatesetting")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UpdateUserSetting(MicroUserProfile user)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).UpdateUserSetting(user);
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

        [Route("updatesocialnetworks")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UpdateSocialNetworks(MicroUserProfile user)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).UpdateSocialNetworks(user);
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

        [Route("interest")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddInterests(BaseModel model)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).AddRemoveInterest(model.Id, false, _microContext.UserId);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateResponse((HttpStatusCode)refModel.actionVal, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("interests")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetInterests()
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetInterests();
                return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("interest")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage RemoveInterests(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).AddRemoveInterest(id, true, _microContext.UserId);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateResponse((HttpStatusCode)refModel.actionVal, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        #endregion

        #region Showcase

        [Route("showcase")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddEditShowcase(Showcase showcase)
        {
            HeaderInformation(Request);
            try
            {
                if (string.IsNullOrEmpty(showcase.ImageUri))
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = "Showcase image required!" }, Configuration.Formatters.JsonFormatter);

                var refModel = new MicroUserProfilesRules(_microContext).AddEditShowcase(showcase);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.msgId }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("showcase/id")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserShowcaseById(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserShowcaseById(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("showcase")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserShowcase(string id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserShowcase(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("showcase")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage DeleteUserShowcase(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).DeleteUserShowcase(id);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        #endregion

        #region Skill

        [Route("skill")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddEditSkill(Skill skill)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).AddEditSkill(skill);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.msgId }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("skill")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserSkill(string id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserSkill(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("skill/id")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserSkillById(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserSkillById(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("skill")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage DeleteUserSkill(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).DeleteUserSkill(id);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        #endregion

        #region Work

        [Route("experiences/work")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserWorkExperiences(string id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserWorkExperiences(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("experiences/work/id")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserWorkExperienceById(int id)
        {
            HeaderInformation(Request);
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, new MicroUserProfilesRules(_microContext).GetWorkExperienceById(id), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("experiences/work")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddEditWorkExperience(WorkExperience work)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).AddEditWorkExperience(work);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.msgId }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("experiences/work")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage DeleteUserWorkExperience(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).DeleteUserWorkExperience(id);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        #endregion

        #region Education

        [Route("experiences/education/id")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserEducationExperienceById(int id)
        {
            HeaderInformation(Request);
            try
            {
 return Request.CreateResponse(HttpStatusCode.OK, new MicroUserProfilesRules(_microContext).GetEducationExperienceById(id), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("experiences/education")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserEducationExperiences(string id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserEducationExperiences(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("experiences/education")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddEditEducationExperience(EducationExperience education)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).AddEditEduExperience(education);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.msgId }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("experiences/education")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage DeleteUserEducationExperience(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).DeleteEduExperience(id);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        #endregion

        #region Address
        [Route("addresses")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserAddresses(string id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserAddresses(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("addresses")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddEditAddress(Models.Trader.TraderAddress address, CountryCode countryCode)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).AddEditUserAddress(address, countryCode);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.msgId }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("addresses/id")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserAddressById(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserAddressId(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("addresses")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage DeleteUserAddress(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).DeleteUserAddress(id);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        #endregion

        #region Public files

        [Route("publicfiles")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserPublicFiles(string id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserPublicFiles(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("publicfiles")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddEditPublicfile(MicroUserFileShare file)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).AddEditUserPublicFile(file);
                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK, new { Id = refModel.msgId }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("publicfiles/id")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetUserPublicfileById(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).GetUserPublicFileId(id);

                if (refModel != null)
                    return Request.CreateResponse(HttpStatusCode.OK, refModel, Configuration.Formatters.JsonFormatter);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("publicfiles")]
        [AcceptVerbs("DELETE")]
        public HttpResponseMessage DeleteUserPublicfile(int id)
        {
            HeaderInformation(Request);
            try
            {
                var refModel = new MicroUserProfilesRules(_microContext).DeleteUserPublicFile(id);

                if (refModel.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = refModel.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
        #endregion
    }
}