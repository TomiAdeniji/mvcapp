using Qbicles.BusinessRules;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.BusinessRules.Network;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class NetworkController : BaseController
    {
        // GET: Network
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Show Shortlist Group Add - Edit View
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        public ActionResult showAddEditSlGroupView(int groupId)
        {
            var slGroup = new ShortlistRules(dbContext).getSlGroupById(groupId);
            return PartialView("_AddEditShortlistGroupView", slGroup);
        }

        public ActionResult showListSlGroups(string keySearch)
        {
            var userId = CurrentUser().Id;
            var domainId = CurrentDomainId();
            var lstSlGroups = new ShortlistRules(dbContext).getUserSlGroups(userId, domainId, keySearch);
            return PartialView("_ListShortlistGroupView", lstSlGroups);
        }

        public ActionResult saveSlGroup(ShortListGroup slGroup, S3ObjectUploadModel uploadModel = null)
        {
            slGroup.AssociatedDomain = CurrentDomain();
            var saveResult = new ShortlistRules(dbContext).SaveShortlistGroup(slGroup, uploadModel, CurrentUser().Id);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult deleteSlGroup(int slGroupId)
        {
            var deleteResult = new ShortlistRules(dbContext).DeleteShortListGroup(slGroupId);
            return Json(deleteResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadShortlistGroupCandidates([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int slGroupId, string searchKey, int start, int length, int draw)
        {
            try
            {
                var totalRecord = 0;
                List<ShortlistGroupCandidateCustomModel> lstResult = new ShortlistRules(dbContext).GetSlGroupCandidatesPagination(slGroupId, searchKey, ref totalRecord, requestModel, start, length);
                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = lstResult,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };
                return Json(dataTableData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<ShortlistGroupCandidateCustomModel>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ShowAddCandidateView(string userId, string candidateId, int slGroupId = 0)
        {
            if (string.IsNullOrEmpty(userId))
                userId = CurrentUser().Id;
            var slGroup = new ShortlistRules(dbContext).getSlGroupById(slGroupId);
            var selectedDomain = slGroup?.AssociatedDomain ?? null;
            var user = dbContext.QbicleUser.Find(userId);
            ViewBag.lstDomain = dbContext.ShortListGroups.Where(p => p.AssociatedUser.Id == userId).ToList().Select(p => p.AssociatedDomain).Distinct().ToList();
            ViewBag.candidateName = dbContext.QbicleUser.Find(candidateId)?.GetFullName() ?? "";
            ViewBag.SlGroupId = slGroupId;
            ViewBag.SlDomain = selectedDomain;
            return PartialView("_AddShortlistGroupCandidate", user);
        }

        public ActionResult GetUserSlGroupByDomain(string userId, int domainId)
        {
            if (string.IsNullOrEmpty(userId))
                userId = CurrentUser().Id;
            var lstGroups = new ShortlistRules(dbContext).getUserSlGroups(userId, domainId, string.Empty);
            return Json(lstGroups.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Title }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddSlGroupCandidate(string userId, int slGroupId)
        {
            var addResult = new ShortlistRules(dbContext).addCandidate(userId, slGroupId, CurrentUser().Id);
            return Json(addResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveSLGroupCandidate(string userId, int slGroupId)
        {
            var removeResult = new ShortlistRules(dbContext).removeCandidate(userId, slGroupId);
            return Json(removeResult, JsonRequestBehavior.AllowGet);
        }
    }
}