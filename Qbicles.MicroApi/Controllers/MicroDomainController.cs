using System.Net;
using System.Web.Http;
using System.Net.Http;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Micro.Model;
using System;
using Qbicles.BusinessRules.Micro;
using static Qbicles.BusinessRules.HelperClass;
using System.Collections.Generic;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    [RoutePrefix("api/micro")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroDomainController : BaseApiController
    {

        [Route("passpattern")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage PasswordPattern()
        {
            HeaderInformation(Request);
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, PasswordValidator(), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("domaininvitations")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetDomainInvitations()
        {
            HeaderInformation(Request);
            try
            {
                var user = new UserRules(_microContext.Context).GetById(_microContext.UserId);
                if (user == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "User not found" }, Configuration.Formatters.JsonFormatter);

                var microInvitations = new MicroDomainRules(_microContext).GetDomainInvitations(user.Email);
                return Request.CreateResponse(HttpStatusCode.OK, microInvitations, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }

        }

        [Route("domaininvitationupdate")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage DomainInvitationUpdate(MicroApproverInvitation invitation)
        {
            HeaderInformation(Request);
            try
            {
                var domainInvitation = new MicroDomainRules(_microContext).DomainInvitationUpdate(invitation, _microContext.UserId);

                if (domainInvitation.result == false)
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, new { Message = domainInvitation.msg }, Configuration.Formatters.JsonFormatter);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }

        }


        [Route("domain")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetDomains()
        {
            HeaderInformation(Request);
            try
            {
                var microDomains = new MicroDomainRules(_microContext).GetDomainByUserId(_microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, microDomains, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("domainusers")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DomainUsers(int domainId)
        {
            HeaderInformation(Request);
            try
            {
                var microDomainUsers = new MicroDomainRules(_microContext).DomainUsers(domainId, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, microDomainUsers, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("domainmembers")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DomainMembers(int domainId)
        {
            HeaderInformation(Request);
            try
            {
                var microDomainUsers = new MicroDomainRules(_microContext).DomainMembers(domainId, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, microDomainUsers, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("domaininviteuser")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage DomainInviteUser(int domainId, string email)
        {
            HeaderInformation(Request);
            try
            {
                var invite = new UserRules(_microContext.Context).InviteUserJoinQbicles(email, "", domainId, GenerateCallbackUrl(), _microContext.UserId, false);
                if (invite.result)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = invite.msg }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("domainroles")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DomainRoles(int domainId)
        {
            HeaderInformation(Request);
            try
            {
                var roles = new MicroDomainRules(_microContext).DomainRoles(domainId);
                return Request.CreateResponse(HttpStatusCode.OK, roles, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("domainadminlevel")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DomainAdminLevel()
        {
            HeaderInformation(Request);
            try
            {
                var levels = new List<BaseModel> {
                    new BaseModel { Id = (int)AdminLevel.Administrators, Name = AdminLevel.Administrators.GetDescription() } ,
                    new BaseModel { Id = (int)AdminLevel.QbicleManagers, Name = AdminLevel.QbicleManagers.GetDescription() } ,
                    new BaseModel { Id = (int)AdminLevel.Users, Name = AdminLevel.Users.GetDescription() }
                };

                return Request.CreateResponse(HttpStatusCode.OK, EnumModel.ConvertEnumToList<AdminLevel>(), Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        [Route("domainremoveuser")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage RemoveUserFromDomain(int domainId, string userId)
        {
            HeaderInformation(Request);
            try
            {
                var removed = new MicroDomainRules(_microContext).RemoveUserFromDomain(domainId, _microContext.UserId, userId);
                if (removed)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }

        }

        [Route("domainupdateroleuser")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage UpdateRoleForUser(int domainId, string userId, AdminLevel currentRole, AdminLevel updateRoleTo)
        {

            HeaderInformation(Request);
            try
            {
                var updated = new MicroDomainRules(_microContext).UpdateRoleForUser(domainId, userId, updateRoleTo, currentRole);
                if (updated)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("domain/filter")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DomainsFilter(string searchText, int order)
        {
            HeaderInformation(Request);
            try
            {
                var microDomains = new MicroDomainRules(_microContext).DomainsFilter(searchText, order, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, microDomains, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }


        [Route("domain/subaccount")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetDomainSubAccount(string domainKey)
        {
            HeaderInformation(Request);
            try
            {
                var domainId = string.IsNullOrEmpty(domainKey) ? 0 : int.Parse(domainKey.Decrypt());
                var subAccountCode = new MicroDomainRules(_microContext).GetSubAccountCodeByDomain(domainId);
                return Request.CreateResponse(HttpStatusCode.OK, new { SubAccountCode = subAccountCode }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, null, _microContext.UserId);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

    }
}
