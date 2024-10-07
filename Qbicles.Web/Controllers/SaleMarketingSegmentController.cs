using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class SalesMarketingSegmentController : BaseController
    {
        public ActionResult Detail(int id)
        {
            if (id == 0)
                return View("Error");
            ViewBag.Areas = new SocialSegmentRule(dbContext).GetAreas(CurrentDomainId());
            ViewBag.Criterias = new SocialContactRule(dbContext).GetCriteriaDefinitions(CurrentDomainId(), true);
            return View(new SocialSegmentRule(dbContext).GetSegmentById(id));
        }
        public PartialViewResult GenerateModalSegmentAddEdit(int segmentId)
        {
            var domainId = CurrentDomainId();
            ViewBag.Areas = new SocialSegmentRule(dbContext).GetAreas(CurrentDomainId());
            ViewBag.Criterias = new SocialContactRule(dbContext).GetCriteriaDefinitions(CurrentDomainId(), true);
            return PartialView("_SegmentAdd", new SocialSegmentRule(dbContext).GetSegmentById(segmentId));
        }
        [HttpPost]
        public ActionResult ShowOrHideSegment(int id)
        {
            var rule = new SocialSegmentRule(dbContext);
            var refModel = rule.ShowOrHideSegment(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GenerateMoreCriteria(int index, string criterias)
        {
            ViewBag.Index = index;
            var lstcri = new JavaScriptSerializer().Deserialize<int[]>(criterias);
            ViewBag.Criterias = new SocialContactRule(dbContext).GetCriteriaDefinitions(CurrentDomainId(), true, lstcri);
            return PartialView("_MoreCriteria");
        }
        public ActionResult GetOptionValuesByCriteriaId(int criteriaId)
        {
            return Json(new SocialSegmentRule(dbContext).GetOptionValuesByCriteriaId(criteriaId), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GenerateListContact(List<ClauseCriteriaModel> clauses, List<int> areaIds, List<int> cContacts)
        {
            var lstContacts = new SocialContactRule(dbContext).GetContactsBySegment(CurrentDomainId(), clauses, areaIds);
            ViewBag.cContacts = cContacts;
            return PartialView("_LstContacts", lstContacts);
        }
        public ActionResult LoadContentSegment(int skip, int take, int[] types, string keyword, bool isLoadingHide)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false, msg = "An error" };

            int totalRecord = 0;
            var segments = new SocialSegmentRule(dbContext).LoadSegmentsByDomainId(CurrentDomainId(), skip, take, keyword, types, isLoadingHide, ref totalRecord);
            ContentMoreModel response = new ContentMoreModel();
            var partialView = "";
            if (take != 0)
            {
                partialView = RenderViewToString("_SegmentContent", segments);
            }
            else
            {
                partialView = "";
            }

            refModel.Object = new
            {
                strResult = partialView,
                totalRecord = totalRecord
            };
            refModel.result = true;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveSegment(SegmentCustomeModel model, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            var media = new MediaModel
            {
                UrlGuid = mediaObjectKey,
                Name = mediaObjectName,
                Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
            };
            model.Domain = CurrentDomain();
            return Json(new SocialSegmentRule(dbContext).SaveSegment(model, media, CurrentUser().Id));
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