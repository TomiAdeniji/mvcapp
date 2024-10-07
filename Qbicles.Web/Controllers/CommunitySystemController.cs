using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Community;
using Qbicles.Models.Community;
using System;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class CommunitySystemController : BaseController
    {
        public ActionResult ValidationDeletePage(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new CommunityPageRules(dbContext).ValidationDeletePage(id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeletePage(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new CommunityPageRules(dbContext).DeletePage(id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            refModel.result = true;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SuspendReinstatePage(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.actionVal = new CommunityPageRules(dbContext).SuspendReinstatePage(id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            refModel.result = true;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidationDeleteTag(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new TagRules(dbContext).ValidationDeleteTag(id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            refModel.result = true;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteTag(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new TagRules(dbContext).DeleteTag(id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            refModel.result = true;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DuplicateTagName(int id,string Name)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new TagRules(dbContext).DuplicateTagName(id,Name);
            }
            catch (Exception ex)
            {
                refModel.result = true;
                refModel.msg = ex.Message;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveTag(Tag tag, string[] keywords)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new TagRules(dbContext).SaveTag(tag, keywords, CurrentUser().Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetTagToEditView(int id)
        {
            try
            {
                var tag = new TagRules(dbContext).GetTagToEditView(id);
                return Json(tag, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
    }
}