using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderTransfersController : BaseController
    {
        public ActionResult ShowTransfersContent()
        {
            try
            {
                ViewBag.WorkGroupsOfMember = CurrentDomain().Workgroups.Where(q =>
                    q.Location.Id == CurrentLocationManage() && q.Processes.Any(p => p.Name == TraderProcessName.TraderTransferProcessName) && q.Members.Any(a => a.Id == CurrentUser().Id)).OrderBy(n => n.Name).ToList();
                ViewBag.WorkGroups = new TraderTransfersRules(dbContext).GetWorkGroups(CurrentLocationManage());
                return PartialView("_TraderTransfersContent");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return PartialView("_TraderTransfersContent");
            }
        }
        public ActionResult GetTransfersContent([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keysearch = "", string route = "", string groupid = "0", string date = "")
        {
            keysearch = keysearch.ToLower().Trim();
            route = route.ToLower().Trim();
            date = date.ToLower().Trim();

            int groupId;
            int.TryParse(groupid, out groupId);
            var result = new TraderTransfersRules(dbContext).TraderTransfersSearch(requestModel, CurrentLocationManage(), CurrentDomainId(), CurrentUser(),
                keysearch, route, groupId, date);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);

            return Json(new DataTablesResponse(requestModel.Draw, new List<TransferCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UnitsSelectByItem(int idTraderItem = 0, int idLocation = 0)
        {
            try
            {
                if (idTraderItem == 0) return PartialView("_UnitsSelectByItem", new List<UnitModel>());
                var conversions = new List<UnitModel>();
                var traderItem = new TraderItemRules(dbContext).GetById(idTraderItem);
                if (traderItem.Units.Count > 0)
                    conversions.AddRange(traderItem.Units.Select(u => new UnitModel
                    {
                        Id = u.Id,
                        QuantityOfBaseunit = u.QuantityOfBaseunit,
                        Name = u.Name,
                        Group = "BaseUnit"
                    }));
                return PartialView("_UnitsSelectByItem", conversions);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
            }
            return PartialView("_UnitsSelectByItem", new List<UnitModel>());
        }

        public ActionResult UnitsSelectByTransferItem(int transferItemId = 0, int idLocation = 0)
        {
            try
            {
                if (transferItemId == 0)
                    return PartialView("_UnitsSelectByItem", new List<UnitModel>());

                var transferItem = dbContext.TraderTransferItems.Find(transferItemId);
                if (transferItem == null)
                    return PartialView("_UnitsSelectByItem", new List<UnitModel>());
                ViewBag.selectedUnit = 0;
                var conversions = new List<UnitModel>();
                var traderItem = transferItem.TraderItem;
                if (traderItem.Units.Count > 0)
                {
                    if (transferItem.Unit != null && transferItem.Unit.Id > 0)
                        ViewBag.selectedUnit = transferItem.Unit.Id;
                    conversions.AddRange(traderItem.Units.Select(u => new UnitModel
                    {
                        Id = u.Id,
                        QuantityOfBaseunit = u.QuantityOfBaseunit,
                        Name = u.Name,
                        Group = "BaseUnit"
                    }));
                }

                return PartialView("_UnitsSelectByItem", conversions);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
            }
            return PartialView("_UnitsSelectByItem", new List<UnitModel>());
        }

        public ActionResult TraderTransferAddEdit(int locationId = 0, int id = 0, string onPage = "")
        {
            try
            {

                var domainId = CurrentDomainId();
                var model = new TraderTransfer();
                var traderReferenceForTransfer = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Transfer);
                if (id > 0)
                {
                    model = new TraderTransfersRules(dbContext).GetById(id);
                    if (model.Reference == null)
                    {
                        model.Reference = traderReferenceForTransfer;
                    }
                }
                else
                {
                    model.Reference = traderReferenceForTransfer;
                }
                if (locationId == 0 && model.OriginatingLocation != null) locationId = model.DestinationLocation.Id;

                var locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                ViewBag.Locations = id == 0 ? locations.Where(l => l.Id != locationId).OrderBy(n => n.Name).ToList() : locations;

                ViewBag.Workgroups = CurrentDomain().Workgroups.Where(q =>
                    q.Location.Id == locationId &&
                    q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                    && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

                //ViewBag.ItemGroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(domainId, locationId);
                ViewBag.LocationId = locationId;

                ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
                ViewBag.Contacts = new TraderContactRules(dbContext).GetByDomainId(CurrentDomainId());
                ViewBag.OnPage = onPage;
                return PartialView("_TraderTransferPartial", model);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return View("Error");
            }

        }


        [HttpPost]
        public ActionResult GetCurrentInventory(int locationId = 0, int itemId = 0)
        {
            var inventory = new TraderInventoryRules(dbContext).GetInventoryDetail(itemId, locationId);
            if (inventory != null && inventory.Id > 0)
                return Json(new { id = inventory.Id, itemId, currentInventory = inventory.CurrentInventoryLevel }, JsonRequestBehavior.AllowGet);
            else
                return Json(null, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveTransfer(TraderTransfer transfer)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if(transfer != null && transfer.Sale != null)
                {
                    transfer.Sale.Id = string.IsNullOrEmpty(transfer.Sale.Key) ? 0 : int.Parse(transfer.Sale.Key.Decrypt());
                }
                refModel.actionVal = new TraderTransfersRules(dbContext).SaveTraderTransfer(transfer, CurrentUser().Id);

            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowTransferItemUnit(int id, int locationId)
        {
            var transferItem = dbContext.TraderTransferItems.Find(id);
            var units = new List<UnitModel>();
            var inventoryDetail = transferItem.TraderItem.InventoryDetails.FirstOrDefault();
            if (inventoryDetail != null)
            {
                foreach (var con in inventoryDetail.Item.Units)
                {
                    var unitmodel = new UnitModel()
                    {
                        Id = con.Id,
                        QuantityOfBaseunit = con.QuantityOfBaseunit,
                        Group = "BaseUnit",
                        Name = con.Name
                    };
                    if (con.IsActive && !units.Any(q => q.Id == unitmodel.Id && q.Group == unitmodel.Group))
                        units.Add(unitmodel);
                }
            }
            else
            {
                var unitModel = new UnitModel()
                {
                    Id = transferItem.Unit.Id,
                    QuantityOfBaseunit = transferItem.Unit.QuantityOfBaseunit,
                    Group = "BaseUnit",
                    Name = transferItem.Unit.Name
                };
                if (transferItem.Unit.IsActive && !units.Any(q => q.Id == unitModel.Id && q.Group == unitModel.Group))
                    units.Add(unitModel);
            }
            ViewBag.BaseUnits = units.OrderBy(n => n.Name).ToList();
            return PartialView("_TraderPurchaseTransferEditUnitPartial", transferItem);
        }
        public ActionResult ShowTransactionItemUnit(int id, int locationId, string unitName)
        {
            try
            {
                var transactionItem = dbContext.TraderSaleItems.FirstOrDefault(e => e.Id == id);
                ViewBag.UnitName = unitName;
                var units = new List<UnitModel>();
                if (transactionItem != null)
                {
                    var inventoryDetail = transactionItem.TraderItem.InventoryDetails.FirstOrDefault();//.FirstOrDefault(q => q.Location.Id == locationId);
                    if (inventoryDetail != null)
                    {
                        foreach (var con in inventoryDetail.Item.Units)
                        {
                            var unitModel = new UnitModel()
                            {
                                Id = con.Id,
                                QuantityOfBaseunit = con.QuantityOfBaseunit,
                                Group = "BaseUnit",
                                Name = con.Name
                            };
                            if (con.IsActive && !units.Any(q => q.Id == unitModel.Id && q.Group == unitModel.Group))
                                units.Add(unitModel);
                        }
                    }
                    else
                    {
                        var unitModel = new UnitModel()
                        {
                            Id = transactionItem.Unit.Id,
                            QuantityOfBaseunit = transactionItem.Unit.QuantityOfBaseunit,
                            Group = "BaseUnit",
                            Name = transactionItem.Unit.Name
                        };
                        if (transactionItem.Unit.IsActive && !units.Any(q => q.Id == unitModel.Id && q.Group == unitModel.Group))
                            units.Add(unitModel);
                    }
                }
                ViewBag.BaseUnits = units.OrderBy(n => n.Name).ToList();
                return PartialView("_TraderPurchaseTransferAddUnitPartial", transactionItem);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        public ActionResult EditPurchaseTransfer(int id, int idPurchase, string onPage = "")
        {
            var transfer = new TraderTransfersRules(dbContext).GetById(id);
            var traderReferenceForTransfer = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Transfer);

            if (transfer.Reference == null)
            {
                transfer.Reference = traderReferenceForTransfer;
            }
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(idPurchase);
            var locationId = purchaseModel.Location.Id;
            var workgroupTransfer = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            ViewBag.WorkgroupTransfer = workgroupTransfer;
            var units = new List<UnitModel>();
            foreach (var item in purchaseModel.PurchaseItems)
            {
                if (item.TraderItem.InventoryDetails.Any(q => q.Location.Id == locationId))
                {
                    var inventoryDetail = item.TraderItem.InventoryDetails.FirstOrDefault(q => q.Location.Id == locationId);
                    if (inventoryDetail != null)
                    {
                        foreach (var con in inventoryDetail.Item.Units)
                        {
                            var unitmodel = new UnitModel()
                            {
                                Id = con.Id,
                                QuantityOfBaseunit = con.QuantityOfBaseunit,
                                Group = "BaseUnit",
                                Name = con.Name
                            };
                            if (con.IsActive && !units.Any(q => q.Id == unitmodel.Id && q.Group == unitmodel.Group))
                                units.Add(unitmodel);
                        }
                    }
                }

            }
            ViewBag.BaseUnits = units.OrderBy(n => n.Name).ToList();
            ViewBag.TraderPurchase = purchaseModel;
            ViewBag.OnPage = onPage;
            return PartialView("_TraderPurchaseTransferEditPartial", transfer);
        }

        public ActionResult EditSaleTransfer(int id, string keySale, string onPage = "")
        {
            var idSale = string.IsNullOrEmpty(keySale) ? 0 : int.Parse(keySale.Decrypt());
            var traderReferenceForTransfer = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Transfer);
            var transfer = new TraderTransfer();
            if (id > 0)
            {
                transfer = new TraderTransfersRules(dbContext).GetById(id);
                if (transfer.Reference == null)
                {
                    transfer.Reference = traderReferenceForTransfer;
                }
            }
            else
            {
                transfer.Reference = traderReferenceForTransfer;
            }

            var saleModel = new TraderSaleRules(dbContext).GetById(idSale);
            var locationId = saleModel.Location.Id;
            var workgroupTransfer = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId &&
                q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName)) &&
                q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            ViewBag.WorkgroupTransfer = workgroupTransfer;
            var units = new List<UnitModel>();
            foreach (var item in saleModel.SaleItems)
            {
                if (item.TraderItem.InventoryDetails.Any(q => q.Location.Id == locationId))
                {
                    var inventoryDetail = item.TraderItem.InventoryDetails.FirstOrDefault(q => q.Location.Id == locationId);
                    if (inventoryDetail != null)
                    {
                        foreach (var con in inventoryDetail.Item.Units)
                        {
                            var unitmodel = new UnitModel()
                            {
                                Id = con.Id,
                                QuantityOfBaseunit = con.QuantityOfBaseunit,
                                Group = "BaseUnit",
                                Name = con.Name
                            };
                            if (con.IsActive && !units.Any(q => q.Id == unitmodel.Id && q.Group == unitmodel.Group))
                                units.Add(unitmodel);
                        }
                    }
                }
            }
            ViewBag.BaseUnits = units.OrderBy(n => n.Name).ToList();
            ViewBag.TraderSale = saleModel;
            ViewBag.OnPage = onPage;
            return PartialView("_TraderSaleTransferEditPartial", transfer);
        }
        [HttpDelete]
        public ActionResult DeleteTransfer(int id)
        {
            var rules = new TraderTransfersRules(dbContext);
            return Json(rules.DeleteTraderTransfer(id) ? "OK" : "Fail", JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTableTransfer(int id, bool callBack = false)
        {
            ViewBag.CallBack = callBack;
            var saleModel = new TraderSaleRules(dbContext).GetById(id);
            return PartialView("_TraderSaleTransferTablePartial", saleModel);
        }
        public ActionResult GetTablePurchaseTransfer(int id)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(id);
            return PartialView("_TraderPurchaseTransferTablePartial", purchaseModel);
        }

        public ActionResult InitSaleTransfer(string key)
        {
            var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
            var transfer = new TraderTransfer();
            var traderReferenceForTransfer = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Transfer);

            if (transfer.Reference == null)
            {
                transfer.Reference = traderReferenceForTransfer;
            }

            ViewBag.Transfer = transfer;
            var saleModel = new TraderSaleRules(dbContext).GetById(id);
            var locationId = saleModel.Location.Id;
            var workgroupTransfer = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            ViewBag.WorkgroupTransfer = workgroupTransfer;
            var units = new List<UnitModel>();
            foreach (var item in saleModel.SaleItems)
            {
                if (item.TraderItem.InventoryDetails.Any(q => q.Location.Id == locationId))
                {
                    var inventoryDetail = item.TraderItem.InventoryDetails.FirstOrDefault(q => q.Location.Id == locationId);
                    if (inventoryDetail != null)
                    {
                        foreach (var con in inventoryDetail.Item.Units)
                        {
                            var unitmodel = new UnitModel()
                            {
                                Id = con.Id,
                                QuantityOfBaseunit = con.QuantityOfBaseunit,
                                Group = "BaseUnit",
                                Name = con.Name
                            };
                            if (con.IsActive && !units.Any(q => q.Id == unitmodel.Id && q.Group == unitmodel.Group))
                                units.Add(unitmodel);
                        }
                    }
                }
            }
            ViewBag.BaseUnits = units.OrderBy(n => n.Name).ToList();
            return PartialView("_TraderSaleTransferAddPartial", saleModel);
        }

        public ActionResult InitPurchaseTransfer(string key)
        {
            var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
            var transfer = new TraderTransfer();
            var traderReferenceForTransfer = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Transfer);

            if (transfer.Reference == null)
            {
                transfer.Reference = traderReferenceForTransfer;
            }

            ViewBag.Transfer = transfer;
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(id);
            var locationId = purchaseModel.Location.Id;
            var workgroupTransfer = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            ViewBag.WorkgroupTransfer = workgroupTransfer;
            var units = new List<UnitModel>();
            foreach (var item in purchaseModel.PurchaseItems)
            {
                if (item.TraderItem.InventoryDetails.Any(q => q.Location.Id == locationId))
                {
                    var inventoryDetail = item.TraderItem.InventoryDetails.FirstOrDefault(q => q.Location.Id == locationId);
                    if (inventoryDetail != null)
                    {
                        foreach (var con in inventoryDetail.Item.Units)
                        {
                            var unitmodel = new UnitModel()
                            {
                                Id = con.Id,
                                QuantityOfBaseunit = con.QuantityOfBaseunit,
                                Group = "BaseUnit",
                                Name = con.Name
                            };
                            if (con.IsActive && !units.Any(q => q.Id == unitmodel.Id && q.Group == unitmodel.Group))
                                units.Add(unitmodel);
                        }
                    }
                }

            }
            ViewBag.BaseUnits = units;
            return PartialView("_TraderPurchaseTransferAddPartial", purchaseModel);
        }

        public ActionResult ShowListMemberForWorkGroup(int wgId = 0)
        {
            if (wgId > 0)
            {
                var rules = new TraderWorkGroupsRules(dbContext);
                var wg = rules.GetById(wgId);
                ViewBag.workgroup = wg;
                return PartialView("_TraderTransferShowMember", wg.Members);
            }
            else
            {
                return PartialView("_TraderTransferShowMember", new List<ApplicationUser>());
            }
        }

        [HttpGet]
        public ActionResult GetWorkGroup(int id)
        {
            var result = new ReturnJsonModel();
            try
            {
                var rules = new TraderWorkGroupsRules(dbContext);
                var wg = rules.GetById(id);
                result.Object =
                    new
                    {
                        Location = wg.Location.Name,
                        LocationId = wg.Location.Id,
                        Process = string.Join(", ", wg.Processes.Select(e => e.Name)),
                        Qbicle = wg.Qbicle.Name,
                        Members = wg.Members.Count,
                        wg.Id,
                        GroupNames = wg.ItemCategories.Any() ? string.Join(", ", wg.ItemCategories.Select(q => q.Name)) : ""
                    };
                result.result = true;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                result.result = false;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TransferReview(string key)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
                var transferRules = new TraderTransfersRules(dbContext);
                var transferModel = transferRules.GetById(id);
                if (transferModel == null)
                    return View("Error");

                var user = CurrentUser();
                var currentDomainId = transferModel?.Workgroup?.Qbicle.Domain.Id ?? CurrentDomainId();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                if (transferModel.Workgroup != null)
                    ValidateCurrentDomain(transferModel.Workgroup.Qbicle.Domain, transferModel.Workgroup.Qbicle.Id);

                var traderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(transferModel.TransferApprovalProcess?.ApprovalRequestDefinition?.Id ?? 0, user.Id);
                var isConsumerDomain = transferModel?.Purchase?.Workgroup.Domain.Id == currentDomainId;
                //var isConsumerDomain2 = transferModel?.Purchase?.Workgroup.Domain.Id == transferModel?.Workgroup.Domain.Id;
                ViewBag.IsCustomerDomain = isConsumerDomain;
                ViewBag.TraderApprovalRight = traderApprovalRight;

                SetCurrentApprovalIdCookies(transferModel.TransferApprovalProcess?.Id ?? 0);

                var timeline = transferRules.TransferApprovalStatusTimeline(transferModel.Id, user.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return View(transferModel);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult TransferMaster(string key)
        {
            var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
            ViewBag.GoBackPage = CurrentGoBackPage();
            this.SetCookieGoBackPage();
            var transferRules = new TraderTransfersRules(dbContext);
            var user = CurrentUser();
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            var transferModel = new TraderTransfersRules(dbContext).GetById(id);
            if (transferModel == null)
                return View("Error");

            var currentDomainId = transferModel?.Workgroup?.Qbicle.Domain.Id ?? CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return View("ErrorAccessPage");

            var allowAccessPage = CurrentDomain().Administrators.Any(s => s.Id == user.Id)
                || (transferModel.Workgroup != null
                && transferModel.Workgroup.Members.Any(q => q.Id == user.Id)
                && transferModel.Workgroup.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                );
            if (!allowAccessPage)
                return View("ErrorAccessPage");
            var timeline = transferRules.TransferApprovalStatusTimeline(transferModel.Id, user.Timezone);
            var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return View(transferModel);
        }

        public ActionResult LocationSelectToHtml(int id)
        {
            var result = new ReturnJsonModel();
            try
            {
                var rules = new TraderLocationRules(dbContext);
                var l = rules.GetById(id);
                var str = new StringBuilder();

                str.Append("<div class='content-block-upper'>");
                str.Append($"<smaller id='source-destination-select'></smaller>");
                str.Append($"<h2>{l.Name}</h2><p>");
                if (!string.IsNullOrEmpty(l.Address?.AddressLine1))
                    str.Append($"<span>{l.Address?.AddressLine1}</span><br />");
                if (!string.IsNullOrEmpty(l.Address?.AddressLine2))
                    str.Append($"<span>{l.Address?.AddressLine2}</span><br />");
                if (!string.IsNullOrEmpty(l.Address?.City))
                    str.Append($"<span>{l.Address?.City}</span><br />");
                if (!string.IsNullOrEmpty(l.Address?.State))
                    str.Append($"<span>{l.Address?.State}</span><br />");
                if (!string.IsNullOrEmpty(l.Address?.PostCode))
                    str.Append($"<span>{l.Address?.PostCode}</span><br />");
                if (l.Address?.Country != null)
                    str.Append($"<span>{l.Address?.Country.CommonName}</span><br />");
                str.Append("</p>");
                result.msg = str.ToString();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult TraderTransferAddEditPartial(int id = 0)
        {
            try
            {
                var locationId = CurrentLocationManage();
                var workgroupTransfer = CurrentDomain().Workgroups.Where(q =>
                    q.Location.Id == locationId
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                    && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
                ViewBag.WorkgroupTransfer = workgroupTransfer;
                ViewBag.Location = new TraderLocationRules(dbContext).GetById(locationId);
                var model = new TraderTransfer();
                var traderReferenceForTransfer = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Transfer);
                if (id > 0)
                {
                    model = new TraderTransfersRules(dbContext).GetById(id);
                    if (model.Reference == null)
                    {
                        model.Reference = traderReferenceForTransfer;
                    }
                }
                else
                {
                    model.Reference = traderReferenceForTransfer;
                }

                return PartialView("_TraderTransferAddEditPartial", model);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e);
                return View("Error");
            }

        }

        //point to point  Specifics
        public ActionResult TransferAddEditPointToPointSpecificsTab(int id = 0)
        {
            var locationId = CurrentLocationManage();
            var locations = CurrentDomain().TraderLocations.ToList();
            ViewBag.Locations = id == 0 ? locations.Where(l => l.Id != locationId).OrderBy(n => n.Name).ToList() : locations;

            var model = new TraderTransfer();
            if (id > 0)
                model = new TraderTransfersRules(dbContext).GetById(id);

            return PartialView("_TransferAddEditPointToPointSpecificsTab", model);

        }
        public ActionResult GetByLocationPurchasePagination([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string daterange)
        {
            var result = new TraderPurchaseRules(dbContext).GetByLocationPagination(CurrentLocationManage(), CurrentDomainId(), daterange, CurrentUser().Timezone, requestModel, CurrentUser().DateFormat);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetByLocationPagination([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string daterange)
        {
            var result = new TraderSaleRules(dbContext).GetByLocationPagination(CurrentLocationManage(), CurrentDomainId(), daterange, CurrentUser().Timezone, requestModel, CurrentUser().DateFormat);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TransferAddEditPointToPointItemsTab(int id = 0)
        {
            var locationId = CurrentLocationManage();
            var locations = CurrentDomain().TraderLocations.ToList();
            ViewBag.Locations = id == 0 ? locations.Where(l => l.Id != locationId).OrderBy(n => n.Name).ToList() : locations;

            ViewBag.LocationId = locationId;
            var domain = CurrentDomain();
            var workgroupTransfer = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            var productGroupIds = workgroupTransfer.SelectMany(q => q.ItemCategories.Select(i => i.Id)).Distinct().ToList();
            var model = new TraderTransfer();
            if (id > 0)
                model = new TraderTransfersRules(dbContext).GetById(id);

            return PartialView("_TransferAddEditPointToPointItemsTab", model);

        }

        // Purchase transfer Items tab
        public ActionResult TransferAddEditPurchaseItemsTab(int purchaseId)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(purchaseId);
            var locationId = purchaseModel.Location.Id;

            var units = new List<UnitModel>();
            foreach (var item in purchaseModel.PurchaseItems)
            {
                if (item.TraderItem.InventoryDetails.Any(q => q.Location.Id == locationId))
                {
                    var inventoryDetail = item.TraderItem.InventoryDetails.FirstOrDefault(q => q.Location.Id == locationId);
                    if (inventoryDetail != null)
                    {
                        foreach (var con in inventoryDetail.Item.Units)
                        {
                            var unitmodel = new UnitModel()
                            {
                                Id = con.Id,
                                QuantityOfBaseunit = con.QuantityOfBaseunit,
                                Group = "BaseUnit",
                                Name = con.Name
                            };
                            if (con.IsActive && !units.Any(q => q.Id == unitmodel.Id && q.Group == unitmodel.Group))
                                units.Add(unitmodel);
                        }
                    }
                }

            }
            ViewBag.BaseUnits = units.OrderBy(n => n.Name).ToList();
            return PartialView("_TransferAddEditPurchaseItemsTab", purchaseModel);
        }

        // Sale transfer Items tab
        public ActionResult TransferAddEditSaleItemsTab(string saleKey)
        {
            var saleId = string.IsNullOrEmpty(saleKey) ? 0 : int.Parse(saleKey.Decrypt());
            var saleModel = new TraderSaleRules(dbContext).GetById(saleId);
            var locationId = saleModel.Location.Id;
            var units = new List<UnitModel>();
            foreach (var item in saleModel.SaleItems)
            {
                if (item.TraderItem.InventoryDetails.Any(q => q.Location.Id == locationId))
                {
                    var inventoryDetail = item.TraderItem.InventoryDetails.FirstOrDefault(q => q.Location.Id == locationId);
                    if (inventoryDetail != null)
                    {
                        foreach (var con in inventoryDetail.Item.Units)
                        {
                            var unitmodel = new UnitModel()
                            {
                                Id = con.Id,
                                QuantityOfBaseunit = con.QuantityOfBaseunit,
                                Group = "BaseUnit",
                                Name = con.Name
                            };
                            if (con.IsActive && !units.Any(q => q.Id == unitmodel.Id && q.Group == unitmodel.Group))
                                units.Add(unitmodel);
                        }
                    }
                }
            }
            ViewBag.BaseUnits = units.OrderBy(n => n.Name).ToList();
            return PartialView("_TransferAddEditSaleItemsTab", saleModel);
        }
    }
}