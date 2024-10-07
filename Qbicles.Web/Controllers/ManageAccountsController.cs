using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using CleanBooksData;
using System.IO;
using Qbicles.BusinessRules;
using Newtonsoft.Json;
using Qbicles.BusinessRules.FilesUploadUtility;
using System.Linq;
using static Qbicles.BusinessRules.Enums;
using Qbicles.BusinessRules.Helper;
//using System.Configuration;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class ManageAccountsController : BaseController
    {
        ReturnJsonModel refModel;

        public ActionResult GetCountUploadByAccount(int id)
        {
            try
            {
                refModel = new ReturnJsonModel();
                var accountRules = new CBAccountRules(dbContext);
                var obj = accountRules.GetUploadsByAccount(id);
                refModel.actionVal = obj.Count();
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new CBAccountResult { result = "", existsUpload = false, lastbalance = 0 }, JsonRequestBehavior.AllowGet);
            }

        }


        /// <summary>
        /// Get next sequence number upload
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public ActionResult GetMaxUploadToAccount(int accountId)
        {
            try
            {
                var accountRules = new CBAccountRules(dbContext);
                var obj = accountRules.GetMaxUploadToAccount(accountId);
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new CBAccountResult { result = "", existsUpload = false, lastbalance = 0 }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// get a Account entity by accountId
        /// </summary>
        /// <param name="id">accountId</param>
        /// <returns>Json account</returns>
        public ActionResult GetAccount(int id)
        {
            try
            {
                var accountRules = new CBAccountRules(dbContext);
                var obj = accountRules.GetAccount(id).BusinessMapping(CurrentUser().Timezone);
                obj.user1 = new Qbicles.Models.ApplicationUser();
                obj.user = new Qbicles.Models.ApplicationUser();
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

        }

        /// <summary>
        /// get a Account entity by accountId to Edit CleanBooks Account
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetAccount2Edit(int id)
        {
            try
            {
                var accountRules = new CBAccountRules(dbContext);
                var obj = accountRules.GetAccount2Edit(id, CurrentUser().Timezone);
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

        }

        public ActionResult GetUpFieldByAccountId(int accountId)
        {
            try
            {
                var accountRules = new CBAccountRules(dbContext);
                var obj = accountRules.GetUpFieldByAccountId(accountId);

                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult GetDomainRoleAccounts(int accountId)
        {
            try
            {
                var accountRules = new CBAccountRules(dbContext);
                var obj = accountRules.GetDomainRoleAccounts(accountId);

                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        /// <summary>
        /// get a accountgroups entity by accountgroups.id
        /// </summary>
        /// <param name="id">accountgroups.id</param>
        /// <returns>json accountgroups</returns>
        public ActionResult GetAccountGroup(int id)
        {
            try
            {
                var accountRules = new CBAccountRules(dbContext);
                var refModel = accountRules.GetAccountGroupById(id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

        }

        /// <summary>
        /// check duplicate accountgroups by accountgroups.name
        /// </summary>
        /// <param name="group">accountgroups entity</param>
        /// <returns>dupplicate: true,false</returns>
        public ActionResult DuplicateAccountGroup(accountgroup group)
        {
            try
            {
                group.DomainId = CurrentDomainId();
                var accountRules = new CBAccountRules(dbContext);
                var obj = accountRules.DuplicateAccountGroup(group);
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

        }

        /// <summary>
        /// Insert or update accountgroup entity
        /// </summary>
        /// <param name="group">accountgroup entity</param>
        /// <param name="logoPath">FileBase logo of accountgroup</param>
        /// <returns>RedirectToAction view ManageAccounts</returns>

        [HttpPost]
        public ActionResult SaveCBAccountGroup(accountgroup group)
        {
            refModel = new ReturnJsonModel();
            try
            {
                if (group != null)
                {
                    group.DomainId = CurrentDomainId();
                    if (group.Id <= 0)
                    {
                        group.CreatedById = CurrentUser().Id;
                    }
                    var accountRules = new CBAccountRules(dbContext);
                    refModel = accountRules.SaveCBAccountGroup(group);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Check duplicate Account
        /// only check duplicate account name in a Subgroup
        /// </summary>
        /// <param name="account">account entity</param>
        /// <returns>dupplicate: true,false</returns>
        [HttpPost]
        public ActionResult DupplicateAccount(Account account)
        {
            try
            {
                var accountRules = new CBAccountRules(dbContext);
                var obj = accountRules.DupplicateAccount(account);
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult CheckUploadFields(string uploadfields)
        {
            try
            {

                var transaction = JsonConvert.DeserializeObject<HashSet<string>>(uploadfields);
                var resultDebit = HelperClass.ContainsAll(transaction, hardColumn.headColRequireDebit());
                var resultCredit = HelperClass.ContainsAll(transaction, hardColumn.headColRequireCredit());

                if (resultDebit || resultCredit)
                    return Json(true, JsonRequestBehavior.AllowGet);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// Insert or update Account entity
        /// </summary>
        /// <param name="account">Account entity</param>
        /// if account.id > 0 then account update
        /// if account.id =0 then account insert
        /// <returns> RedirectToAction view ManageAccounts</returns>

        [HttpPost]
        public ActionResult SaveCBAccount(Account account, bool isEditLastbalance, string uploadFields, string rolesGrant)
        {
            refModel = new ReturnJsonModel();
            try
            {
                if (account.Id <= 0)
                {
                    account.CreatedById = CurrentUser().Id;
                    account.CreatedDate = DateTime.UtcNow;
                }
                var accountRules = new CBAccountRules(dbContext);

                refModel = accountRules.SaveCBAccount(account, isEditLastbalance, uploadFields, rolesGrant, CurrentUser().Id, CurrentDomainId());
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id, account, isEditLastbalance, uploadFields, rolesGrant);
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteAccount(Account account)
        {
            try
            {
                var accountRules = new CBAccountRules(dbContext);
                accountRules.DeleteAccount(account, CurrentUser().Id);

                return Json(new
                {
                    status = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new
                {
                    status = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult LoadAccounts(int groupId, SortOrderBy orderBy = SortOrderBy.NameAZ)
        {
            try
            {
                var rule = new CBAccountRules(dbContext);
                var currentUserId = CurrentUser().Id;
                var domainId = CurrentDomainId();
                var wgs = new CBWorkGroupsRules(dbContext).GetAllCbWorkGroup(domainId);
                var memberAccount = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.AccountProcessName))
                    .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);
                var memberAccountData = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.AccountDataProcessName))
                    .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);
                ViewBag.MemberAccount = memberAccount;
                ViewBag.MemberAccountData = memberAccountData;
                //modify load more here, return date count and data to view
                var model = rule.LoadAccountGroups(groupId, domainId, orderBy);
                string modelString = RenderViewToString("_AccountContent", model);

                return Json(new { ModelString = modelString });
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new { ModelString = "" });
            }

        }

        private string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            var accountRules = new CBAccountRules(dbContext);
            ViewBag.accountupdatefrequency = accountRules.GetAccountupdatefrequency();
            ViewBag.DataManager = CurrentDomain().Users;
            var UserRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            ViewBag.UserRoleRights = UserRoleRights;
            ViewBag.UserRoles = new UserRules(dbContext).GetById(CurrentUser().Id).DomainRoles.Select(n => n.Name).ToList();
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }

        [HttpPost]
        public JsonResult UploadPreview(int accountId, string isDelete)
        {
            try
            {
                var rule = new CBAccountRules(dbContext);

                //modify load more here, return date count and data to view
                var model = rule.UploadPreview(accountId).ToList();
                string modelString = RenderUploadPreview("_UploadPreview", model, isDelete);

                var result = Json(modelString, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = Int32.MaxValue;
                return result;

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return null;
            }

        }

        private string RenderUploadPreview(string viewName, object model, string isDelete)
        {
            ViewData.Model = model;
            ViewBag.rightDeleteHistory = isDelete;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult ValidDeleteAccount(int accountId)
        {
            try
            {
                var accountRules = new CBAccountRules(dbContext);
                var account = accountRules.GetAccount(accountId, true);
                bool delete = account.uploads.Any();
                if (account.taskaccounts.Any())
                    delete = true;
                return Json(new
                {
                    status = delete
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new
                {
                    status = true
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAccount2AddEdit(int id, int groupId = 0)
        {
            try
            {
                ClearAllCurrentActivities();
                ViewBag.Roles = new AppRightRules(dbContext).GrantAccessToGroups(CurrentDomainId(), "View CleanBooks Accounts");
                var accountRules = new CBAccountRules(dbContext);
                ViewBag.AccountUpdateFrequency = accountRules.GetAccountupdatefrequency();
                ViewBag.fileTypes = new TransactionsRules(dbContext).GetFileType();
                ViewBag.AccountGroups = accountRules.GetAccountGroup(CurrentDomainId());
                var wgs = new CBWorkGroupsRules(dbContext).GetAllCbWorkGroup(CurrentDomainId());
                ViewBag.DataManager = wgs.Where(q=>q.Processes.Any(p=>p.Name == CBProcessName.AccountDataProcessName)).SelectMany(s => s.Members).Distinct().ToList();
                var curreentUserId = CurrentUser().Id;
                ViewBag.CBWorkgroups = new List<CBWorkGroup>();
                if (wgs.Any())
                {
                    ViewBag.CBWorkgroups = wgs.Where(q =>
                        q.Processes.Any(p => p.Name == CBProcessName.AccountProcessName) &&
                        q.Members.Any(m => m.Id == curreentUserId)).ToList();
                }

                //var accountRules = new CBAccountRules(dbContext);
                var cbAccount = accountRules.GetAccountById(id);
                if (cbAccount.GroupId == 0) cbAccount.GroupId = groupId;
                var bkRules = new BKCoANodesRule(dbContext);
                var bkAccountNodes = new List<int>();
                if (cbAccount.BookkeepingAccount?.Id > 0)
                {
                    bkAccountNodes = bkRules.GetBkAccountNodesSelected(cbAccount.BookkeepingAccount.Id);
                }

                ViewBag.BkAccountNodes = bkAccountNodes;

                ViewBag.CreatedDate = cbAccount.CreatedDate == null ? "" : cbAccount.CreatedDate.Value.ConvertTimeFromUtc(CurrentUser().Timezone).ToString("dd/MM/yyyy");

                return PartialView("_CbAccountAddEdit", cbAccount);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id, id);
                return View("Error");
            }

        }
    }
}