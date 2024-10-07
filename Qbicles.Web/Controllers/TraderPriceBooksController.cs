using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Pricing;
using Qbicles.Models.Trader.SalesChannel;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderPriceBooksController : BaseController
    {
        public ActionResult ListPriceBook(int locationId, bool callback = false)
        {
            ViewBag.CallBack = callback;

            var priceBooks = new TraderPriceBooksRules(dbContext).GetPriceBooks(locationId);
            ViewBag.ProductGroups = new TraderGroupRules(dbContext).GetTraderGroupItem(CurrentDomainId());
            return PartialView("_TraderPriceBook", priceBooks);
        }

        public ActionResult PriceBook(int id = 0)
        {
            ViewBag.locationName = new TraderLocationRules(dbContext).GetById(CurrentLocationManage())?.Name;
            var priceBook = new TraderPriceBooksRules(dbContext).GetPriceBookById(id) ?? new PriceBook();
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            return View(priceBook);
        }

        public ActionResult History(int id = 0)
        {
            var priceBookInstance =
                new TraderPriceBooksRules(dbContext).GetPriceBookInstanceById(id);
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            return View(priceBookInstance);
        }

        public ActionResult CheckExistName(int priceBookId, string priceBookName)
        {
            var refModel = new ReturnJsonModel();

            var rules = new TraderPriceBooksRules(dbContext);
            refModel = rules.CheckExistName(priceBookId, priceBookName, CurrentDomainId(), CurrentLocationManage());
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SavePriceBook(PriceBook priceBook)
        {
            var rules = new TraderPriceBooksRules(dbContext);
            var refModel = rules.SavePriceBook(priceBook, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ProductGroupByChannel(int id, SalesChannelEnum channel)
        {
            var locationId = CurrentLocationManage();
            var groups = new TraderPriceBooksRules(dbContext).ProductGroupByChannel(locationId, channel, CurrentDomainId());
            var price = new TraderPriceBooksRules(dbContext).GetPriceBookById(id);

            var editGroup = price?.AssociatedProductGroups ??
                                new List<TraderGroup>();
            ViewBag.editGroup = editGroup;
            if (price?.SalesChannel == channel)
                groups.AddRange(editGroup);
            return PartialView("_TraderProductGroupByChannel", groups);
        }
        //Pricebook Versions

        [HttpPost]
        public ActionResult SavePricebookVersion(PriceBookVersion version)
        {
            var rules = new TraderPriceBooksRules(dbContext);
            var refModel = rules.SavePriceBookVersion(version, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PriceBookVersion(int pricebookId)
        {
            var pb = new TraderPriceBooksRules(dbContext).GetPriceBookById(pricebookId);
            var priceBook = new ItemTopic
            {
                Id = pb.Id,
                Name = pb.Name
            };
            return PartialView("_TraderPriceBookVersions", priceBook);
        }


        public ActionResult PricebookVersionsManagement(int pricebookId)
        {
            var versions = new TraderPriceBooksRules(dbContext).GetPricebookVersionsByPricebookId(pricebookId);
            if (versions != null)
                foreach (var item in versions)
                {
                    foreach (var vs in item.AssociatedInstances)
                    {
                        vs.CreatedDate = vs.CreatedDate.ConvertTimeFromUtc(CurrentUser().Timezone);
                    }
                }
            return PartialView("_TraderPricebookVersionsManagement", versions);
        }


        public ActionResult PricebookVersionsSummerPrices(int instanceId)
        {
            var priceBookInstance =
                new TraderPriceBooksRules(dbContext).CheckProducts(instanceId, CurrentUser().Id);
            if (priceBookInstance == null)
                return PartialView("Error");
            return PartialView("_TraderPricebookVersionsSummerPrices", priceBookInstance);
        }


        public ActionResult ShowHistoryModal(int versionId)
        {
            var priceBookInstance =
                new TraderPriceBooksRules(dbContext).GetPriceBookInstanceByVersionId(versionId);
            return PartialView("_TraderPricebookHistoryTable", priceBookInstance);
        }

        public ActionResult GetProductGroupPriceDefaultId(int id)
        {
            var refModel = new ReturnJsonModel();

            var rules = new TraderPriceBooksRules(dbContext);
            refModel.Object = rules.GetMarkupDiscountModel(id);
            refModel.result = true;
            refModel.actionVal = 1;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApplyMarkupDiscount(MarkupDiscountModel markupDiscount)
        {
            var refModel = new ReturnJsonModel();

            var rules = new TraderPriceBooksRules(dbContext);
            refModel = rules.ApplyMarkupDiscount(markupDiscount);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RenderTablePriceBookPricesByInstance(int instanceId, int groupId)
        {
            var priceBookPrices =
                      new TraderPriceBooksRules(dbContext).GetPriceBookPricesByInstance(instanceId, groupId);
            ViewBag.groupId = groupId;
            return PartialView("_TraderItemPriceBookPricesTable", priceBookPrices);
        }


        public ActionResult RecalculatePrices(int instanceId, int groupId, RecalculatePricesType type)
        {
            var priceBookPrices = new TraderPriceBooksRules(dbContext).RecalculatePrices(instanceId, groupId, type);
            ViewBag.groupId = groupId;
            return PartialView("_TraderItemPriceBookPricesTable", priceBookPrices);
        }

        [HttpPost]
        public ActionResult SavePriceBookPrices(List<PriceBookPrice> prices, string status, string versionName)
        {

            var rules = new TraderPriceBooksRules(dbContext);
            var refModel = rules.SavePriceBookPrices(prices, status, versionName, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult ApplyPriceBookPrices(int instanceId, string versionName)
        {
            var refModel = new ReturnJsonModel();
            var rules = new TraderPriceBooksRules(dbContext);
            refModel = rules.ApplyPriceBookPrices(instanceId, versionName, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CopyPricebookVersion(int instanceId)
        {
            var priceBookInstance = new PriceBookInstance();
            priceBookInstance = new TraderPriceBooksRules(dbContext).CopyPriceBookVersion(instanceId, CurrentUser().Id);

            return PartialView("_TraderPricebookVersionsSummerPrices", priceBookInstance);
        }

        [HttpPost]
        public ActionResult ReCalculatePriceRow(PriceBookPrice priceBookPrice)
        {
            var refModel = new ReturnJsonModel();

            var rules = new TraderPriceBooksRules(dbContext);
            refModel = rules.ReCalculatePriceRow(priceBookPrice, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
    }
}