using Org.BouncyCastle.Crypto;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Qbicles;
using Qbicles.Models.WaitList;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class WaitlistController : BaseController
    {
        public ActionResult JoinTheWaitlist(WaitListRequest waitList, CountryCode countryCode)
        {
            waitList.User = new Qbicles.Models.ApplicationUser { Id = CurrentUser().Id };
            var refModel = new WaitListRules(dbContext).JoinTheWaitlist(waitList, countryCode, GetOriginatingConnectionIdFromCookies());
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult CheckPendingWaitlist()
        {
            var refModel = new WaitListRules(dbContext).CheckPendingWaitlist();
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckApprovalWaitlist(string userId)
        {
            var refModel = new WaitListRules(dbContext).CheckApprovalWaitlist(userId);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWaitListRequests([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
            string keyword = "", string daterange = "", List<CountryCode> countries = null, List<NumberOfEmployees> employees = null, List<DiscoveredVia> discoveredVia = null)
        {
            var currentUser = CurrentUser();
            var refModel = new WaitListRules(dbContext).GetWaitListRequests(requestModel, currentUser.DateFormat, currentUser.Timezone, keyword, daterange, countries, employees, discoveredVia);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetDomainCreationRightsLog([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
            string keyword = "", string daterange = "", List<NumberOfEmployees> employees = null, List<DiscoveredVia> discoveredVia = null, List<int> rights = null)
        {
            var currentUser = CurrentUser();
            var refModel = new WaitListRules(dbContext).GetDomainCreationRightsLog(requestModel, currentUser.DateFormat, currentUser.Timezone, keyword, daterange, employees, discoveredVia, rights);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDomainCreationRightsToRevoke([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
    string keyword = "", string daterange = "", List<int> rights = null)
        {
            var currentUser = CurrentUser();
            var refModel = new WaitListRules(dbContext).GetDomainCreationRightsToRevoke(requestModel, currentUser.DateFormat, currentUser.Timezone, keyword, daterange, rights);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Approval or reject
        /// </summary>
        /// <param name="waitlistIds"></param>
        /// <param name="approval"></param>
        /// <returns></returns>
        public ActionResult ApprovalWaitlist(List<int> waitlistIds, Qbicles.Models.Notification.NotificationEventEnum approvalType)
        {
            var refModel = new WaitListRules(dbContext).ApprovalSubscriptionCustomRejectDomain(waitlistIds, approvalType, CurrentUser().Id, GetOriginatingConnectionIdFromCookies());
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RevokeWaitlist(int waitlistId)
        {
            var refModel = new WaitListRules(dbContext).RevokeWaitlist(waitlistId, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FilterWaitlist(string country, int categoryId, int type)
        {
            var model = new WaitListRules(dbContext).FilterWaitlist(country, categoryId, type);

            return PartialView("_UserWaitlistModal", model);
        }


        public ActionResult FilterWaitlistByIds(string ids)
        {
            var model = new WaitListRules(dbContext).FilterWaitlist(ids);

            return PartialView("_UserWaitlistModal", model);
        }
    }
}