using Qbicles.BusinessRules;
using Qbicles.Models.Bookkeeping;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class CleanBooksAccountController : BaseController
    {
        public ActionResult BookkeepingChartOfAccounts()
        {
            var domainId = CurrentDomainId();
            var bkGroup = new BKCoANodesRule(dbContext).GetBKGroupByDomain(domainId);
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
            if (bkGroup.Any())
                ViewBag.GroupAccountTree = new BookkeepingRules(dbContext).RenderGroupAccountTree(bkGroup.ToList(), currencySettings);
            else
                ViewBag.GroupAccountTree = "";
            
            return PartialView("_BookkeepingChartOfAccounts");
        }

        public ActionResult BookkeepingChartOfAccountContent(int value)
        {

            var bkRules = new BookkeepingRules(dbContext);
            //var bkgroupRules = new BKCoANodesRule(dbContext);
            var node = new BKCoANodesRule(dbContext).GetBKCoANodeById(value);


            ViewBag.BreadcrumbName = $"{node.Number}-{node.Name}";
            ViewBag.AccountAmount = bkRules.CalculateAccountAmount(node);

            var isAccount = true;

            if (node is BKAccount)
            {
                var theAccount = (BKAccount)node;
                // Parent only, the 'children' of an Accout are the account's transactions

                ViewBag.Breadcrumb = bkRules.BreadcrumbReverse(node);
                //ViewBag.CurrentUser = new UserRules(dbContext).GetUserOnly(CurrentUser().Id);
                ViewBag.AccountAmount = bkRules.CalculateAccountAmount(node);
                ViewBag.AccountNode = theAccount;

            }
            else
            {
                isAccount = false;
            }

            ViewBag.IsAccount = isAccount;

            return PartialView("_BookkeepingAccountChart");

        }

        public ActionResult ShowImportFromBookkeeping(int accountId)
        {
            var model = new CBAccountRules(dbContext).ShowImportFromBookkeeping(accountId, CurrentDomainId());
            ViewBag.AccountId = accountId;
            return PartialView("_ImportFromBookkeeping", model);
        }

        public ActionResult ImportFromBookkeeping(int accountId)
        {
            var refx = new CBAccountRules(dbContext).ImportFromBookkeeping(accountId, CurrentDomainId(), CurrentUser().Id);
            
            return Json(refx, JsonRequestBehavior.AllowGet);
        }
    }
}