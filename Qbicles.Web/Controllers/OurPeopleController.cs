using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Invitation;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class OurPeopleController : BaseController
    {
        // GET: OurPeople
        ReturnJsonModel refModel;

        [Authorize]
        public ActionResult SearchPeopleDataTable([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequestTable, string peopleName, int roleLevel, int[] domainRole)
        {
            var userId = CurrentUser().Id;
            if (string.IsNullOrEmpty(peopleName) && roleLevel == 0 && (domainRole == null || domainRole.Length == 0))
            {
                var lst = new OurPeopleRules(dbContext).GetAllOurPeopleByDomainDataTable(dtRequestTable, CurrentDomainId(), CurrentUser().Timezone, userId);
                return Json(lst, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var lst = new OurPeopleRules(dbContext).SearchOurPeopleByDomainDataTable(dtRequestTable, CurrentDomainId(), peopleName, roleLevel, domainRole, CurrentUser().Timezone, userId);
                return Json(lst, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult SearchPeopleData(string peopleName, int roleLevel, int[] domainRole)
        {
            try
            {
                var userId = CurrentUser().Id;

                if (string.IsNullOrEmpty(peopleName) && roleLevel == 0 && (domainRole == null || domainRole.Length == 0))
                {
                    var lst = new OurPeopleRules(dbContext).GetAllOurPeopleByDomain(CurrentDomainId(), CurrentUser().Timezone, userId);
                    return PartialView("~/Views/OurPeople/_PeopleList.cshtml", lst);
                }
                else
                {
                    var lst = new OurPeopleRules(dbContext).SearchOurPeopleByDomain(CurrentDomainId(), peopleName, roleLevel, domainRole, CurrentUser().Timezone, userId);
                    return PartialView("~/Views/OurPeople/_PeopleList.cshtml", lst);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize]
        public ActionResult SearchInvitationPeople(string peopleName, InvitationStatusEnum status)
        {
            try
            {
                var userId = CurrentUser().Id;

                var currentDomain = CurrentDomain();
                if (currentDomain == null)
                    currentDomain = new Qbicles.Models.QbicleDomain();
                var lst = new OurPeopleRules(dbContext).GetAllInvitationByDomain(currentDomain);
                if (!string.IsNullOrEmpty(peopleName))
                    lst = lst.Where(p => (p.Forename?.ToLower()?.Contains(peopleName.ToLower()) ?? false)
                                      || (p.Surname?.ToLower()?.Contains(peopleName.ToLower()) ?? false)
                                      || (p.UserName?.ToLower()?.Contains(peopleName.ToLower()) ?? false)).ToList();
                if (status > 0)
                {
                    lst = lst.Where(p => p.Status == status).ToList();
                }
                return PartialView("_InvatationList", lst);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return null;
            }
        }
        [Authorize]
        public ActionResult PromoteOrDemoteUser(string userId, AdminLevel PromoteOrDemoteTo, AdminLevel currentPossition)
        {
            refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                refModel.result = new OurPeopleRules(dbContext).PromoteOrDemoteUser(CurrentDomainId(), userId, PromoteOrDemoteTo, currentPossition);

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult InvitationJoinQbicles(string email, string message)
        {
            try
            {
                refModel = new ReturnJsonModel();

                return Json(new UserRules(dbContext).InviteUserJoinQbicles(email, message, GenerateCallbackUrl(), CurrentUser().Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult Invitation(string email, string message = "")
        {
            try
            {
                refModel = new ReturnJsonModel();

                return Json(new UserRules(dbContext).InviteUserJoinQbicles(email, message, CurrentDomainId(), GenerateCallbackUrl(),
                    CurrentUser().Id, true), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult ReSendInvitation(int Id, string email, string RecipientName)
        {
            try
            {
                refModel = new ReturnJsonModel();

                var domain = CurrentDomain();
                var user = dbContext.QbicleUser.Find(CurrentUser().Id);

                var invitation = dbContext.Invitations.FirstOrDefault(p => p.Id == Id);
                if (invitation != null)
                {
                    invitation.LastUpdateDate = DateTime.UtcNow;
                    invitation.LastUpdatedBy = user;
                    invitation.Status = InvitationStatusEnum.Pending;
                    var log = new InvitationSentLog
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Invitation = invitation
                    };
                    dbContext.InvitationSentLogs.Add(log);
                    dbContext.SaveChanges();
                }

                if (RecipientName == null || RecipientName == "")
                    RecipientName = email;
                string callbackUrl = GenerateCallbackUrl();
                new EmailRules(dbContext).SendEmailInvitation(Id, user, email, callbackUrl, domain.Id, domain.Name, RecipientName);

                refModel.result = true;

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult ApproverOrRejectInvitation(int Id, int status, int DomainId, string Note)
        {
            var refModel = new OurPeopleRules(dbContext).ApproverOrRejectInvitation(Id, status, DomainId, Note, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);

        }
        [Authorize]
        public ActionResult LoadInvitationPeople()
        {
            try
            {
                var user = new DomainRules(dbContext).GetUser(CurrentUser().Id);
                var lst = new OurPeopleRules(dbContext).GetInvitationApproverByUser(user.Email);
                return PartialView("~/Views/OurPeople/_DomainInvitesList.cshtml", lst);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        [Authorize]
        public ActionResult RemovedUserFromDomain(string userId)
        {

            refModel = new ReturnJsonModel() { result = false, msg = "An error" };

            refModel.result = new OurPeopleRules(dbContext).RemovedUserFromDomain(CurrentDomainId(), CurrentUser().Id, userId, CurrentQbicleId());
            return Json(refModel, JsonRequestBehavior.AllowGet);

        }

        public int GetCurrentUserAllowed()
        {
            var currentDomain = CurrentDomain();
            if (currentDomain == null) return 0;
            var userslot = HelperClass.GetDomainUsersAllowed(currentDomain.Id);
            var actualMembers = userslot.ActualMembers;
            return actualMembers;
        }
        public string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}