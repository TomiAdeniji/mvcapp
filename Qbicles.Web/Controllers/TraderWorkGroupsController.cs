using System;
using System.Linq;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderWorkGroupsController : BaseController
    {
        public ActionResult Edit(int id)
        {
            var domainId = CurrentDomainId();
            var workGroup = new TraderWorkGroupsRules(dbContext).GetById(id);
            ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
            ViewBag.Qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(domainId);
            ViewBag.Topics = new TopicRules(dbContext).GetTopicByQbicle(workGroup.Qbicle.Id);
            ViewBag.Process = new TraderProcessRules(dbContext).GetAll();
            ViewBag.Groups = new TraderGroupRules(dbContext).GetTraderGroupItem(domainId);
            return PartialView("_TraderWorkGroupEdit", workGroup);
        }

        public ActionResult GetWorkGroupUser(int id)
        {
            var wg = new TraderWorkGroupsRules(dbContext).GetWorkgroupUser(id);
            return Json(wg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReInitUsersEdit(int id)
        {
            var wg = new TraderWorkGroupsRules(dbContext).ReInitUsersEdit(id, CurrentDomain());
            return Json(wg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidateName(WorkGroup wg)
        {
            var refModel = new ReturnJsonModel();
            var rule = new TraderWorkGroupsRules(dbContext);
            refModel.result = rule.WorkGroupNameCheck(wg, CurrentDomainId());

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            var refModel = new ReturnJsonModel();
            var rules = new TraderWorkGroupsRules(dbContext);
            refModel.result = rules.Delete(id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(WorkGroup wg)
        {
            var refModel = new ReturnJsonModel { result = true };           

            wg.Domain = CurrentDomain();
            refModel.result = new TraderWorkGroupsRules(dbContext).Create(wg, CurrentUser().Id, "icon_bookkeeping.png");
            refModel.actionVal = CurrentDomain().Workgroups.Any() ? 1 : 2;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(WorkGroup wg)
        {
            var refModel = new ReturnJsonModel { result = true };
            wg.Domain = CurrentDomain();
            refModel.result = new TraderWorkGroupsRules(dbContext).Update(wg, CurrentUser().Id, "icon_bookkeeping.png");

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
    }
}