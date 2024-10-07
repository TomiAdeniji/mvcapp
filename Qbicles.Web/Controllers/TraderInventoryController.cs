using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Org.BouncyCastle.Crypto.Engines;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;

namespace Qbicles.Web.Controllers
{
    public class TraderInventoryController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="keySearch">search string input</param>
        /// <param name="inventoryBasis">average = Average cost (FIFO), latest = Latest cost</param>
        /// <param name="maxDayToLast">Max days to last: integer</param>
        /// <param name="days2Last">Filter by date range if dayToLastOperator = 3</param>
        /// <param name="dayToLastOperator">1: Last one week sales, 2: Last one month sales, 3: custom date range</param>
        /// <returns></returns>
        public ActionResult GetInventoryServerSide([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
            string keySearch = "", string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1, bool hasSymbol = true)
        {

            var userId = CurrentUser().Id;
            var locationId = CurrentLocationManage();
            var cookieName = $"itemUnitsChanged-{locationId}-{userId}";

            var unitsChange = GetDisplayUnitChangeCookies(cookieName);

            var result = new TraderInventoryRules(dbContext).GetInventoryServerSide(requestModel.ToIDataTablesRequest(), locationId,CurrentUser(), unitsChange,
                keySearch, inventoryBasis, maxDayToLast, days2Last, dayToLastOperator, hasSymbol);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);

            return Json(new DataTablesResponse(requestModel.Draw, new List<InventoryModel>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowIngredientsItemAssociated(int itemId)
        {
            var locationId = CurrentLocationManage();

            var itemsAssociated = new TraderInventoryRules(dbContext).ShowIngredientsItemAssociated(itemId, locationId);
            ViewBag.IngredientId = itemId;
            return PartialView("_IngredientsItemAssociated", itemsAssociated);
        }

        public ActionResult ShowChangeItemUnits(int itemId)
        {
            var units = new TraderInventoryRules(dbContext).ShowChangeItemUnits(itemId);
            return PartialView("_ChangeItemUnits", units ?? new List<ProductUnit>());
        }



        public ActionResult UpdateChangeItemUnit(int unitId, int itemId,
            string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1, bool hasSymbol = true)
        {

            var userId = CurrentUser().Id;
            var locationId = CurrentLocationManage();
            var cookieName = $"itemUnitsChanged-{locationId}-{userId}";

            var unitsChange = GetDisplayUnitChangeCookies(cookieName);

            var refModel = new TraderInventoryRules(dbContext).UpdateChangeItemUnit(userId, unitId, itemId, locationId, unitsChange,
                 inventoryBasis, maxDayToLast, days2Last, dayToLastOperator, hasSymbol);

            SetDisplayUnitChangeCookies(cookieName, refModel.Object2);


            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchReorders([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int status, string daterange)
        {
            return Json(new TraderInventoryRules(dbContext).SearchReorders(requestModel, CurrentLocationManage(), keyword, status, daterange, CurrentUser().Timezone, CurrentUser().DateFormat), JsonRequestBehavior.AllowGet);
        }
        public ActionResult TriggeringReorderProcess(string keySearch = "", string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1)
        {
            return Json(new TraderInventoryRules(dbContext).TriggeringReorderProcess(CurrentDomainId(), CurrentLocationManage(), CurrentUser(), keySearch, inventoryBasis, maxDayToLast, days2Last, dayToLastOperator));
        }
        public ActionResult CountReorderItems(string keySearch = "", string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1)
        {
            var count = new TraderInventoryRules(dbContext).CountReorderItems(CurrentLocationManage(), CurrentUser(), keySearch, inventoryBasis, maxDayToLast, days2Last, dayToLastOperator);
            return Json(count, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CreateGroupReorder(ReorderItemGroupCustomModel model)
        {
            return Json(new TraderInventoryRules(dbContext).CreateGroupReorder(model, CurrentLocationManage(), CurrentUser().Timezone, CurrentUser().DateFormat));
        }
        public ActionResult CalculateQuantities(ReorderItemGroupCustomModel model)
        {
            model.DomainId = CurrentDomainId();
            return Json(new TraderInventoryRules(dbContext).CalculateQuantities(CurrentLocationManage(), model, CurrentUser().Timezone, CurrentUser().DateFormat));
        }
        public ActionResult LoadReorderProfileGroup(int groupid)
        {
            var domainId = CurrentDomainId();
            ViewBag.Deliveries = Enum.GetValues(typeof(DeliveryMethodEnum)).Cast<DeliveryMethodEnum>().Select(q => q.ToString()).ToList();
            ViewBag.Contacts = new TraderContactRules(dbContext).GetByDomainId(domainId);
            ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(domainId);
            return PartialView("_ReorderProfiGroup", new TraderInventoryRules(dbContext).GetReorderGroupById(groupid));
        }
        public ActionResult ReorderFinish(ReorderCustomModel model)
        {
            return Json(new TraderInventoryRules(dbContext).ReorderFinish(model, CurrentDomainId(), CurrentUser().Id));
        }
        public ActionResult ExcludeReorderItems(int productGroupId, int reorderId)
        {
            return Json(new TraderInventoryRules(dbContext).ExcludeReorderItems(productGroupId, reorderId));
        }
        public ActionResult UncheckAllReorder(int groupid)
        {
            return Json(new TraderInventoryRules(dbContext).UncheckAllReorder(groupid));
        }
        public ActionResult ChangeContact(int groupid, int contactid)
        {
            return Json(new TraderInventoryRules(dbContext).ChangeContact(groupid, contactid));
        }
        public ActionResult MoveContacts(ReorderItemGroupCustomModel model)
        {
            return Json(new TraderInventoryRules(dbContext).MoveContacts(model));
        }
        public ActionResult SetIsReOrderForItem(int itemId, bool isReOrder)
        {
            return Json(new TraderInventoryRules(dbContext).SetIsReOrderForItem(itemId, isReOrder));
        }

        public ActionResult ChangeDelivery(int reorderid, DeliveryMethodEnum DeliveryMethod)
        {
            return Json(new TraderInventoryRules(dbContext).ChangeDelivery(reorderid, DeliveryMethod));
        }
        public ActionResult ReorderBreakdown(int reorderid)
        {
            ViewBag.Contacts = new TraderContactRules(dbContext).GetByDomainId(CurrentDomainId());
            var reorder = new TraderInventoryRules(dbContext).GetReorderById(reorderid);
            return PartialView("_ReorderBreakdown", reorder);
        }
    }
}