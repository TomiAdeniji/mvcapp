using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.Trader;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Manufacturing;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class ManufacturingController : BaseController
    {
        // GET: Manufacturing   
        public ActionResult AddEdit(int id = 0, int locationId = 0, int itemId = 0)
        {
            var manuJob = new ManuJob();
            ViewBag.ItemId = itemId;
            ViewBag.Item = dbContext.TraderItems.FirstOrDefault(p => p.Id == itemId);
            var traderReferenceForManuJob = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.ManuJob);
            if (id > 0)
            {
                manuJob = new TraderManufacturingRules(dbContext).GetManuJobById(id);
                if (manuJob.Reference == null)
                {
                    manuJob.Reference = traderReferenceForManuJob;
                }
            }
            else
            {
                manuJob.Reference = traderReferenceForManuJob;
            }
            var userId = CurrentUser().Id;

            var workgroups = dbContext.WorkGroups.AsNoTracking().Where(q =>
                        q.Location.Id == locationId
                        && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Manufacturing))
                        && q.Members.Select(u => u.Id).Contains(userId)
                        ).ToList();
            var selectedWg = new WorkGroup();
            if (workgroups != null && workgroups.Count > 0 && itemId > 0)
            {
                var productGroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(CurrentDomainId(), locationId);
                foreach (var wg in workgroups)
                {

                    var pg = productGroups.Where(x => x.Items.Select(i => i.Id).Contains(itemId)
                           && x.WorkGroupCategories.Select(w => w.Id).Contains(wg.Id)).SelectMany(c => c.Items.Where(item => item.IsCompoundProduct == true && item.Id == itemId)
                           );
                    if (pg.Any())
                    {
                        selectedWg = wg;
                        break;
                    }
                }
            }
            ViewBag.WorkgroupSelected = selectedWg;

            ViewBag.WorkGroups = workgroups.OrderBy(n => n.Name).ToList();

            if (manuJob.AssemblyUnit == null) manuJob.AssemblyUnit = new ProductUnit();
            if (manuJob.Product == null) manuJob.Product = new TraderItem();
            return PartialView("_TraderManufacturingAddEdit", manuJob);
        }
        public ActionResult GetTraderItems(int wgId = 0, int locationId = 0, int itemId = 0)
        {
            ViewBag.ItemId = itemId;
            ViewBag.ProductGroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(CurrentDomainId(), locationId)
                .Where(x => x.Items.Select(i => i.IsCompoundProduct == true && i.Locations.Select(l => l.Id).Contains(locationId)).Any()
                    && x.WorkGroupCategories.Select(w => w.Id).Contains(wgId)).Select(c => new TraderGroup()
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Items = c.Items.Where(item => item.IsCompoundProduct == true).ToList()
                    }).ToList();
            return PartialView("_TraderManufacturingShowItems");
        }
        public ActionResult GetTraderItem(int itemId = 0, int locationid = 0)
        {
            var item = new TraderItemRules(dbContext).GetById(itemId) ?? new TraderItem();
            ViewBag.Item = item;
            var inventory = item.InventoryDetails.FirstOrDefault(q => q.Location.Id == locationid);
            var recipe = new Recipe();
            if (inventory != null)
            {
                recipe = inventory.CurrentRecipe;
            }

            recipe = item.AssociatedRecipes.FirstOrDefault(q => q.IsCurrent);
            return PartialView("_TraderManufacturingShowItemInfo", recipe ?? new Recipe());
        }
        public ActionResult GetManufacturingTab()
        {
            return PartialView("_TraderManufacturingShowTab");
        }
        public ActionResult AddEditSpecific(int id = 0)
        {
            var manuJob = new ManuJob();
            if (id > 0) manuJob = new TraderManufacturingRules(dbContext).GetManuJobById(id);

            return PartialView("_TraderManufacturingSpecificAddEdit", manuJob);
        }
        public ActionResult ManujobViewer(int id = 0)
        {
            var manuJob = new ManuJob();
            var manuJobs = new List<ManuJob>();
            if (id > 0) manuJob = new TraderManufacturingRules(dbContext).GetManuJobById(id);
            if (manuJob != null)
            {
                manuJobs = new TraderManufacturingRules(dbContext).GetManusByLocation(manuJob.Location.Id)
                    .Where(q => q.AssemblyUnit.Id == manuJob.AssemblyUnit.Id).ToList();
            }
            ViewBag.ManuJobs = manuJobs;

            return PartialView("_TraderManufacturingView", manuJob);
        }
        public ActionResult ShowManujobByUnit(int unitId = 0, int locationId = 0)
        {
            var lstManuJobs = new List<ManuJob>();
            if (unitId > 0)
            {
                lstManuJobs = new TraderManufacturingRules(dbContext).GetManusByLocation(locationId).Where(q => q.AssemblyUnit.Id == unitId).ToList();
            }
            return PartialView("_TraderManufacturingShowHistory", lstManuJobs);
        }
        public ActionResult ShowTableManuJob(int locationId = 0)
        {
            var lstManuJobs = new TraderManufacturingRules(dbContext).GetManusByLocation(locationId);
            var workgroups = CurrentDomain().Workgroups.Where(q =>
                                  q.Location.Id == CurrentLocationManage()
                                  && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Manufacturing))
                                  && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)
                                  ).Select(q => q.Name).ToList();
            ViewBag.WorkGroups = workgroups;
            ViewBag.ManuWorkGroupFilter =
                new TraderManufacturingRules(dbContext).GetManuWorkGroups(CurrentLocationManage());
            return PartialView("_TraderManufacturingManuJobsTable", lstManuJobs);
        }
        public ActionResult ShowTableManuJoAvailable(int locationId = 0)
        {
            var userId = CurrentUser().Id;
            var workgroups = dbContext.WorkGroups.Where(q =>
                        q.Location.Id == locationId
                        && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Manufacturing))
                        && q.Members.Select(u => u.Id).Contains(userId)
                        ).SelectMany(e => e.ItemCategories).Distinct().OrderBy(n => n.Name).ToList();


            ViewBag.WorkGroups = workgroups.OrderBy(n => n.Name).ToList();
            ViewBag.LocationId = locationId;
            return PartialView("_TraderManufacturingManuJobsTableAvailable");
        }

        public ActionResult GetDataManuJoAvailable([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
            int locationId, string keyword = "", int groupId = 0)
        {
            return Json(new TraderManufacturingRules(dbContext).GetDataManuJoAvailable(requestModel, CurrentUser().Id, locationId, keyword, groupId), JsonRequestBehavior.AllowGet);
        }



        public ActionResult ManufacturingHistoryViewer(int id)
        {
            var subTotalNumber = "";
            var manuJobName = "";
            string currencySymbol = "";
            var views = new TraderManufacturingRules(dbContext).ManufacturingHistoryViewer(id, ref subTotalNumber, ref manuJobName, ref currencySymbol);
            ViewBag.ManuJobName = manuJobName;
            ViewBag.SubTotalNumber = subTotalNumber;
            ViewBag.CurrencySymbol = currencySymbol;
            return PartialView("_ManufacturingHistoryViewer", views);
        }




        [HttpPost]
        public ActionResult SaveManujob(ManuJob manujob)
        {
            manujob.Domain = CurrentDomain();

            var result = new TraderManufacturingRules(dbContext).SaveManufacturing(manujob, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ManuJobReview(int id)
        {
            try
            {
                var rule = new TraderManufacturingRules(dbContext);
                var manuJobModel = rule.GetManuJobById(id) ?? new ManuJob();
                var currentDomainId = manuJobModel?.Domain.Id ?? 0;
                ValidateCurrentDomain(manuJobModel?.Domain, manuJobModel.WorkGroup?.Qbicle.Id ?? 0);
                var accessTrader = false;
                var userSetting = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, currentDomainId);
                if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                    accessTrader = true;
                ViewBag.AccessTrader = accessTrader;
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

                ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(manuJobModel.ManuApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, userSetting.Id);


                var timeline = rule.ManuJobApprovalStatusTimeline(manuJobModel.Id, userSetting.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;


                var manuJobItem = manuJobModel.Product;
                var units = new List<UnitModel>();
                if (manuJobItem != null)
                {
                    foreach (var unit in manuJobItem.Units)
                    {
                        var unitModel = new UnitModel()
                        {
                            Id = unit.Id,
                            QuantityOfBaseunit = unit.QuantityOfBaseunit,
                            Group = "BaseUnit",
                            Name = unit.Name
                        };
                        if (unit.IsActive && !units.Any(q => q.Id == unitModel.Id && q.Group == unitModel.Group))
                            units.Add(unitModel);
                    }
                }

                ViewBag.Units = units;

                return View(manuJobModel);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult ManufacturingMasterForMovenmentTrend(string key)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
                var rule = new TraderManufacturingRules(dbContext);
                var manuJobModel = rule.GetManuJobById(id) ?? new ManuJob();
                var currentDomainId = manuJobModel?.Domain.Id ?? 0;
                ValidateCurrentDomain(manuJobModel?.Domain, manuJobModel.WorkGroup?.Qbicle.Id ?? 0);
                var accessTrader = false;
                var userSetting = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, currentDomainId);
                if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                    accessTrader = true;
                ViewBag.AccessTrader = accessTrader;
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

                ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(manuJobModel.ManuApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, userSetting.Id);


                var timeline = rule.ManuJobApprovalStatusTimeline(manuJobModel.Id, userSetting.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;


                var manuJobItem = manuJobModel.Product;
                var units = new List<UnitModel>();
                if (manuJobItem != null)
                {
                    foreach (var unit in manuJobItem.Units)
                    {
                        var unitModel = new UnitModel()
                        {
                            Id = unit.Id,
                            QuantityOfBaseunit = unit.QuantityOfBaseunit,
                            Group = "BaseUnit",
                            Name = unit.Name
                        };
                        if (unit.IsActive && !units.Any(q => q.Id == unitModel.Id && q.Group == unitModel.Group))
                            units.Add(unitModel);
                    }
                }

                ViewBag.Units = units;

                return View("~/Views/Manufacturing/ManuJobReview.cshtml", manuJobModel);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }
        public ActionResult ManuJobReviewContent(int id)
        {
            try
            {
                var userSetting = CurrentUser();
                var accessTrader = false;
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, CurrentDomainId());
                if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                    accessTrader = true;
                ViewBag.AccessTrader = accessTrader;

                var rule = new TraderManufacturingRules(dbContext);
                var manuJobModel = rule.GetManuJobById(id) ?? new ManuJob();
                ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(manuJobModel.ManuApprovalProcess.ApprovalRequestDefinition.Id, userSetting.Id);


                var timeline = rule.ManuJobApprovalStatusTimeline(manuJobModel.Id, userSetting.Timezone);
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;


                var manuJobItem = manuJobModel.Product;
                var units = new List<UnitModel>();
                if (manuJobItem != null)
                {
                    foreach (var unit in manuJobItem.Units)
                    {
                        var unitModel = new UnitModel()
                        {
                            Id = unit.Id,
                            QuantityOfBaseunit = unit.QuantityOfBaseunit,
                            Group = "BaseUnit",
                            Name = unit.Name
                        };
                        if (unit.IsActive && !units.Any(q => q.Id == unitModel.Id && q.Group == unitModel.Group))
                            units.Add(unitModel);
                    }
                }

                ViewBag.Units = units;
                return PartialView("_ManuJobReviewContent", manuJobModel);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult UpdateManuJobReview(ManuJob manuJob)
        {
            var result = new TraderManufacturingRules(dbContext).UpdateManuJobReview(manuJob, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}