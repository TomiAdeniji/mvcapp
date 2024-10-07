using Microsoft.AspNet.Identity.CoreCompat;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using Qbicles.Models.Qbicles;
using Qbicles.BusinessRules.Micro.Model;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace Qbicles.MicroApi.Controllers
{
    /// <summary>
    ///
    /// </summary>
    [RoutePrefix("api/micro/account")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MicroAccountRegistrationController : BaseApiController
    {
        private ApplicationDbContext dbContext = new ApplicationDbContext();

        /// <summary>
        ///
        /// </summary>
        /// <param name="acc"></param>
        /// <returns></returns>
        [Route("registration")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> AccountRegistration(CreateAccountMain acc)
        {
            try
            {
                acc.RegistrationType = RegistrationType.Micro;
                var user = new ApplicationUser
                {
                    DisplayUserName = acc.UserName.Trim(),
                    UserName = acc.Email,
                    Email = acc.Email,
                    Forename = acc.Forename,
                    Surname = acc.Surname,
                    DateBecomesMember = DateTime.UtcNow,
                    ProfilePic = ConfigManager.DefaultUserUrlGuid,
                    IsEnabled = true,
                    Timezone = ConfigurationManager.AppSettings["Timezone"],
                    TimeFormat = "HH:mm",
                    DateFormat = "dd/MM/yyyy",
                    Profile = ""
                };
                //The name for the SubscriptionAccount is based on the UserName:<<UserName>>_Account
                acc.AccountName = user.UserName + "_Account";
                //The name of the QbicleDomain is simply to be the DispalyUserName exactly.
                acc.Domain = user.DisplayUserName;

                var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));

                var result = await userManager.CreateAsync(user, acc.Password);
                if (!result.Succeeded)
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Code = result.Succeeded, Message = string.Join(", ", result.Errors) }, Configuration.Formatters.JsonFormatter);

                await userManager.AddToRoleAsync(user.Id, SystemRoles.DomainUser);
                await userManager.UpdateAsync(user);
                new UserRules(dbContext).SetNormalizedUserNameToEmail(user.Id);

                var rs = new AccountRules(dbContext).SaveAccount(acc, user.Id);
                acc.Id = user.Id;
                var url = $"{ConfigManager.QbiclesUrl}/Account/SendEmailConfirmationToken";
                var sendEmail = new BaseHttpClient().Post(url, new ReturnJsonModel { Object = acc.ToJson().Encrypt() });

                if (sendEmail.StatusCode != HttpStatusCode.OK)
                    return Request.CreateResponse(sendEmail.StatusCode, new { Message = sendEmail.RequestMessage }, Configuration.Formatters.JsonFormatter);

                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(acc.Id);
                var key = Convert.ToBase64String(plainTextBytes);
                return Request.CreateResponse(HttpStatusCode.OK, new { UserVerification = plainTextBytes });
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="acc"></param>
        /// <returns></returns>
        [Route("registration/resendemail")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AccountRegistrationResendEmail(CreateAccountMain acc)
        {
            try
            {
                acc.RegistrationType = RegistrationType.Micro;
                var userId = dbContext.QbicleUser.FirstOrDefault(e => e.Email == acc.Email)?.Id ?? "";

                acc.Id = userId;
                var url = $"{ConfigManager.QbiclesUrl}/Account/SendEmailConfirmationToken";
                var sendEmail = new BaseHttpClient().Post(url, new ReturnJsonModel { Object = acc.ToJson().Encrypt() });

                if (sendEmail.StatusCode == HttpStatusCode.OK)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(sendEmail.StatusCode, new { Message = sendEmail.RequestMessage }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verification"></param>
        /// <returns></returns>
        [Route("registration/resendverificationemail")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage VerificationEmailResend(ReturnJsonModel verification)
        {
            try
            {
                var url = $"{ConfigManager.QbiclesUrl}/Account/VerificationEmailResend";
                var sendEmail = new BaseHttpClient().Post(url, verification);

                if (sendEmail.StatusCode == HttpStatusCode.OK)
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(sendEmail.StatusCode, new { Message = sendEmail.RequestMessage }, Configuration.Formatters.JsonFormatter);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pinVerification"></param>
        /// <returns></returns>
        [Route("pinverification")]
        [AcceptVerbs("POST")]
        public async Task<HttpResponseMessage> PinVerification(PinVerification pinVerification)
        {
            try
            {
                var url = $"{ConfigManager.QbiclesUrl}/Account/ProceedPINVerifyMicro";
                var verifications = new BaseHttpClient().Post<PinVerification, VerificationPinModel>(url, pinVerification);
                return Request.CreateResponse(verifications.Status, verifications);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { ex.Message }, Configuration.Formatters.JsonFormatter);
            }
        }
    }
}