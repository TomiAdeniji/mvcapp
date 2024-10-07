using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.MarketingSocial;
using Qbicles.Models.SalesMkt;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class SalesMarketingSetupController : BaseController
    {
        public ActionResult ShowContacts()
        {
            ViewBag.Contacts = new SocialWorkgroupRules(dbContext).GetCustomOptions(CurrentDomainId(), CurrentDomain()).Any();
            return PartialView("_SMSetupShowContactsPartial");
        }

        public ActionResult ShowTraderContacts()
        {
            var domainId = CurrentDomainId();
            ViewBag.TraderContacts = new SocialWorkgroupRules(dbContext).GetSMContacts(domainId).Any();
            return PartialView("_SMSetupShowTraderContactsPartial");
        }

        public ActionResult ShowQbicle()
        {
            var domain = CurrentDomain();
            ViewBag.qbicles = domain.Qbicles;
            var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId());
            if (setting != null)
            {
                ViewBag.Setting = setting;
                ViewBag.topics = new TopicRules(dbContext).GetTopicByQbicle(setting.SourceQbicle != null ? setting.SourceQbicle.Id : 0);
            }
            ViewBag.Qbicle = setting != null ? true : false;
            return PartialView("_SMSetupShowQbiclePartial");
        }

        public ActionResult ShowWorkgroup()
        {
            ViewBag.Workgroup = new SocialWorkgroupRules(dbContext).GetMarketingWorkGroupsByDomainId(CurrentDomainId()).Any();
            return PartialView("_SMSetupShowWorkgroupPartial");
        }

        public ActionResult ShowComplete()
        {
            //var domain = CurrentDomain();
            //var traderLocations = domain.TraderLocations.Any();
            //var traderGroups = domain.TraderGroups.Any();
            //var workgroups = domain.Workgroups.Any();
            //var isBkk = new TraderSettingRules(dbContext).GetTraderSettingByDomain(CurrentDomainId()).IsQbiclesBookkeepingEnabled;
            //var taxRate = new TaxRateRules(dbContext).GetByDomainId(CurrentDomainId()).Any();
            //var traderAccount = isBkk || taxRate;
            //var isCompleted = traderLocations && traderGroups && workgroups && traderAccount;
            var isCompleted = true;
            return PartialView("_SMSetupShowCompletedPartial", isCompleted == true ? "" : "disabled");
        }

        [HttpPost]
        public ActionResult UpdateSMIsSettingComplete(SMSetupCurrent isComplete)
        {
            try
            {
                var sm = new SalesMaketingSettingRules(dbContext);
                var result = sm.UpdateSMIsSettingComplete(CurrentDomainId(), isComplete);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
    }
}