using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class SalesMarketingContactController : BaseController
    {
        // GET: SaleMarketingContact

        public ActionResult GenerateModalCriteriaAddEdit(int criteriaId)
        {
            return PartialView("_ContactCriteriaAdd", new SocialContactRule(dbContext).GetCriteriaDefinitionById(criteriaId));
        }
        public ActionResult LoadContentCriteria()
        {
            return PartialView("_ContactCriteriaContent", new SocialContactRule(dbContext).GetCriteriaDefinitions(CurrentDomainId()));
        }
        [HttpPost]
        public ActionResult SaveContactCriteria(CriteriaCustomeModel model)
        {
            model.Domain = CurrentDomain();

            return Json(new SocialContactRule(dbContext).SaveContactCriteria(model, CurrentUser().Id));
        }
        [HttpPost]
        public ActionResult RemoveCriteria(int criteriaId)
        {
            return Json(new SocialContactRule(dbContext).DeleteContactCriteria(criteriaId));
        }
        [HttpPost]
        public ActionResult SetStatusContactCriteria(int criteriaId, bool status)
        {
            return Json(new SocialContactRule(dbContext).SetStatusContactCriteria(criteriaId, status));
        }
        [HttpPost]
        public ActionResult MoveUpDownOrderCriteria(CriteriaReOrderModel model)
        {
            return Json(new SocialContactRule(dbContext).MoveUpDownOrderCriteria(model));
        }
    }
}