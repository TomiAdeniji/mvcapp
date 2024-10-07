using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.Spannered;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Spannered;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using static Qbicles.BusinessRules.Enums;
using static Qbicles.Models.Spannered.Asset;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class SpanneredfreeController : BaseController
    {
        //check permissions and navigate to the required tab
        public ActionResult Index()
        {
            var domainId = CurrentDomainId();
            var userId = CurrentUser().Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userId, domainId);
            if (userRoleRights.All(r => r != RightPermissions.SpanneredAccess))
                return View("ErrorAccessPage");
            SetCurrentPage(QbiclePages.pageSpannered);
            ViewBag.CurrentPage = QbiclePages.pageSpannered;
            ViewBag.locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
            ViewBag.tags = new SpanneredAssetTagRules(dbContext).GetTags(domainId);
            ViewBag.currentLocationId = new QbicleRules(dbContext).LoadUiSettings(QbiclePages.pageSpannered, userId)?.FirstOrDefault(s => s.Key == SpanneredKeyStoredUiSettings.ddlLocationId)?.Value;
            return View();
        }
        public ActionResult Asset(int id)
        {
            var domainId = CurrentDomainId();
            var userId = CurrentUser().Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userId, domainId);
            if (userRoleRights.All(r => r != RightPermissions.SpanneredAccess))
                return View("ErrorAccessPage");
            var assetRules = new SpanneredAssetRules(dbContext);
            var asset = assetRules.GetAssetById(id);
            ViewBag.RelateAssets = assetRules.GetRelatedAssets(id);
            ViewBag.tags = new SpanneredAssetTagRules(dbContext).GetTagsByAssetId(id);
            ViewBag.WorkGroupsOfMember = new TraderWorkGroupsRules(dbContext).GetWorkGroups(asset.Location?.Id ?? 0, CurrentDomain(), userId, TraderProcessName.TraderPurchaseProcessName);
            return View("Detail", asset);
        }
        public ActionResult LoadModalWorkgroup(int id, int locationId)
        {
            var wgRule = new SpanneredWorkgroupRules(dbContext);
            var domain = CurrentDomain();
            var qbicles = domain.Qbicles != null ? domain.Qbicles : new List<Qbicle>();
            var defaultQbicle = qbicles.Any() ? qbicles.FirstOrDefault() : new Qbicle();
            ViewBag.topics = new TopicRules(dbContext).GetTopicOnlyByQbicle(defaultQbicle.Id);
            ViewBag.members = domain.Users;
            ViewBag.qbicles = qbicles;
            ViewBag.process = wgRule.GetProcesses();
            ViewBag.tradergroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(domain.Id, locationId);
            return PartialView("_ModalWorkgroupContent", wgRule.getWorkgroupById(id));
        }
        public ActionResult SaveWorkgroup(SpanneredWorkgroupCustom workgroup)
        {
            workgroup.Domain = CurrentDomain();
            return Json(new SpanneredWorkgroupRules(dbContext).SaveWorkgroup(workgroup, CurrentUser().Id));
        }
        public ActionResult DeleteWorkgroup(int id)
        {
            return Json(new SpanneredWorkgroupRules(dbContext).DeleteWorkgroup(id));
        }
        public ActionResult GetWorkgroupsAll([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int locationId)
        {
            return Json(new SpanneredWorkgroupRules(dbContext).GetWorkgroupAll(requestModel, CurrentDomainId(), locationId, CurrentUser().DateFormat), JsonRequestBehavior.AllowGet);
        }
        //public ActionResult GetallWorkgroupsHasProcess(string process)
        //{
        //    return Json(new SpanneredWorkgroupRules(dbContext).GetallWorkgroupsHasProcess(process, UserSettings().Id, CurrentDomainId()), JsonRequestBehavior.AllowGet);
        //}
        public ActionResult GetTagsAll([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            return Json(new SpanneredAssetTagRules(dbContext).GetTagAll(requestModel, CurrentDomainId(), CurrentUser().DateFormat), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveTag(AssetTag tag)
        {
            tag.Domain = CurrentDomain();

            return Json(new SpanneredAssetTagRules(dbContext).SaveTag(tag, CurrentUser().Id));
        }
        public ActionResult DelectTag(int id)
        {
            return Json(new SpanneredAssetTagRules(dbContext).DeleteTag(id));
        }
        public ActionResult getTagById(int id)
        {
            var tag = new SpanneredAssetTagRules(dbContext).getTagById(id);
            if (tag != null)
                return Json(new { tag.Id, tag.Name, tag.Summary }, JsonRequestBehavior.AllowGet);
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadModalAsset(int id, int lid)
        {
            var wgRules = new SpanneredWorkgroupRules(dbContext);
            var assetRules = new SpanneredAssetRules(dbContext);
            var domain = CurrentDomain();
            ViewBag.workgroups = wgRules.GetallWorkgroupsHasProcess(ProcessesConst.Assets, CurrentUser().Id, domain.Id, lid);
            ViewBag.tags = new SpanneredAssetTagRules(dbContext).GetTags(domain.Id);
            ViewBag.othersAssets = assetRules.GetOthersAssetById(id, domain.Id, lid);
            ViewBag.meters = new SpanneredMeterRules(dbContext).GetMetersByAssetId(id);
            ViewBag.tradergroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(domain.Id, lid);
            return PartialView("_ModalAssetContent", assetRules.GetAssetById(id));
        }
        public ActionResult SpanneredFreeSaveAsset(SpanneredAssetCustom asset)
        {
            asset.Domain = CurrentDomain();
            //if (!string.IsNullOrEmpty(asset.MetersString))
            //    asset.Meters = new JavaScriptSerializer().Deserialize<List<Meter>>(asset.MetersString);

            asset.FeaturedImageUri = asset.MediaObjectKey;
            asset.MediaResponse = new MediaModel
            {
                UrlGuid = asset.MediaObjectKey,
                Name = asset.MediaObjectName,
                Size = HelperClass.FileSize(HelperClass.Converter.Obj2Int(asset.MediaObjectSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(asset.MediaObjectName))
            };

            return Json(new SpanneredAssetRules(dbContext).SpanneredFreeSaveAsset(asset, CurrentUser().Id));
        }
        public ActionResult UpdateOptionAsset(int id, OptionsEnum option)
        {
            return Json(new SpanneredAssetRules(dbContext).UpdateOptionAsset(id, option, CurrentUser().Id));
        }
        public ActionResult LoadAssets(int lid, int skip, int take, string keyword, OptionsEnum Option, List<int> tags)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                int totalRecord = 0;
                var assets = new SpanneredAssetRules(dbContext).LoadAssetsByDomainId(CurrentDomainId(), lid, Option, skip, take, keyword, tags, ref totalRecord);
                var partialView = RenderViewToString("~/Views/Spanneredfree/_AssetsContent.cshtml", assets);
                refModel.Object = new
                {
                    strResult = partialView,
                    totalRecord = totalRecord
                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
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

        public ActionResult LoadMedias(int fid, int qid, string fileType)
        {
            try
            {
                var listMedia = new MediaFolderRules(dbContext).GetMediaItemByFolderId(fid, qid, fileType, CurrentUser().Timezone);
                return PartialView("_AssetResources", listMedia);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult SpanneredFreeSaveResource(string name, string description, int qbicleId, int topicId, int mediaFolderId,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var media = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
                };
                refModel = new SpanneredAssetRules(dbContext).SpanneredFreeSaveResource(media, CurrentUser().Id, qbicleId, mediaFolderId, name, description, topicId);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                refModel.msg = _L("ERROR_MSG_154");
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);

        }

        public ActionResult LoadModalAssetTask(int taskId, int assetId)
        {
            try
            {
                ViewBag.taskId = taskId;
                ViewBag.taskKey = taskId.Encrypt();
                ViewBag.assetId = assetId;
                var asset = new SpanneredAssetRules(dbContext).GetAssetById(assetId);
                var task = new TasksRules(dbContext).GetTaskById(taskId);
                ViewBag.asset = asset;
                ViewBag.workgroups = new SpanneredWorkgroupRules(dbContext).GetallWorkgroupsHasProcess(ProcessesConst.AssetTasks, CurrentUser().Id, CurrentDomainId(), asset.Location?.Id ?? 0);
                return PartialView("_ModalTask", task);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, User.Identity.GetUserId());
                return null;
            }
        }


        public PartialViewResult LoadPeopleTabByQbicleId(int qbicleId)
        {
            var rule = new QbicleRules(dbContext);
            return PartialView("_PeopleTab", rule.GetQbicleById(qbicleId));
        }

        [HttpPost]
        public ActionResult SpanneredFreeSaveQbicleTask(QbicleTask task, string Assignee, string ProgrammedStart, string[] Watchers,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize,
            int qbicleId, int TopicId, int[] ActivitiesRelate, List<QbicleStep> Steplst, long assetId, int WorkgroupId, string inventoriescps)

        {
            var refModel = new ReturnJsonModel();
            try
            {
                string currentDatetimeFormat = CurrentUser().DateTimeFormat;
                DateTime dtLastOccurrence = DateTime.UtcNow;
                if (string.IsNullOrEmpty(task.Name))
                {
                    refModel.result = false;
                    refModel.msg = "Request to enter information!";
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                if (ActivitiesRelate != null && ActivitiesRelate.Count() > 31)
                {
                    refModel.result = false;
                    refModel.msg = "Data associate activities cannot be greater than 31 records!";
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                var media = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
                };

                var tz = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
                if (!string.IsNullOrEmpty(ProgrammedStart))
                {
                    try
                    {
                        task.ProgrammedStart = TimeZoneInfo.ConvertTimeToUtc(ProgrammedStart.ConvertDateFormat(currentDatetimeFormat), tz);

                    }
                    catch
                    {
                        task.ProgrammedStart = DateTime.UtcNow;
                    }
                }

                refModel.result = new SpanneredAssetRules(dbContext).
                    SaveTask(task, Assignee, media, Watchers, qbicleId,
                    CurrentUser().Id, TopicId, ActivitiesRelate, Steplst, assetId, WorkgroupId, inventoriescps);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
            if (refModel.result)
                return Json(refModel, JsonRequestBehavior.AllowGet);
            else
                return null;
        }

        [HttpPost]
        public ActionResult LoadAssetTasks([Bind(Prefix = "order[0][column]")] int column, [Bind(Prefix = "order[0][dir]")] string orderBy,
            string searchName, int draw, int assetId, string status, string assigneeId, int start, int length)
        {
            try
            {
                var totalRecord = 0;
                List<AssetTasksModel> lstResult = new SpanneredAssetRules(dbContext).GetAssetTasks(assetId, searchName, status, assigneeId, ViewBag.CurrentUserId, column, orderBy, start, length, ref totalRecord, CurrentUser().DateFormat);
                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = lstResult,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };
                return Json(dataTableData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<AssetTasksModel>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult LoadModalMeter()
        {
            return PartialView("_MeterAdd");
        }

        public PartialViewResult LoadMeters(string name, int assetId)
        {
            var rule = new SpanneredMeterRules(dbContext);
            return PartialView("_MeterContent", rule.GetMetersByAssetId(assetId, name));
        }

        public ActionResult SaveMeter(Meter meter, int assetId)
        {
            var ruleAsset = new SpanneredAssetRules(dbContext);
            var ruleMeter = new SpanneredMeterRules(dbContext);
            meter.Asset = ruleAsset.GetAssetById(assetId);
            return Json(ruleMeter.SaveMeter(meter, CurrentUser().Id));
        }

        public ActionResult UpdateValueOfUnit(int meterId, decimal valueOfUnit)
        {
            var ruleMeter = new SpanneredMeterRules(dbContext);
            return Json(ruleMeter.UpdateValueOfUnit(meterId, valueOfUnit, CurrentUser().Id));
        }

        public PartialViewResult LoadMeterHistoryModal(int id)
        {
            var ruleMeter = new SpanneredMeterRules(dbContext);
            return PartialView("_MeterHistory", ruleMeter.GetMeterById(id));
        }

        public ActionResult CheckPermissionAddEdit(string process, int workgroupId)
        {
            var rule = new SpanneredWorkgroupRules(dbContext);
            return Json(rule.CheckPermissionAddEdit(process, workgroupId, CurrentDomainId(), CurrentUser().Id));
        }
        public ActionResult GetListAssociatedTraderItem([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int locationId, int spwgid, string keyword, int groupId, int itemLink)
        {
            return Json(new SpanneredAssetRules(dbContext).GetListAssociatedTraderItem(requestModel, CurrentDomainId(), locationId, spwgid, keyword, groupId, itemLink), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetListTraderItem(int locationId, int spwgid, string search, int itemId = 0)
        {
            return Json(new SpanneredAssetRules(dbContext).GetListTraderItem(CurrentDomainId(), locationId, spwgid, search, itemId), JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult LoadModalTransfer(int assetId, int locationId)
        {
            var domainId = CurrentDomainId();
            var userId = CurrentUser().Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userId, domainId);
            if (userRoleRights.All(r => r != RightPermissions.SpanneredAccess))
                return PartialView("ErrorAccessPage");
            ViewBag.locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
            ViewBag.workgroups = new TraderWorkGroupsRules(dbContext).GetWorkGroups(locationId, CurrentDomain(), userId, TraderProcessName.TraderTransferProcessName);
            var rule = new SpanneredAssetRules(dbContext);
            return PartialView("_ModalTransferContent", rule.GetAssetById(assetId));
        }
        public ActionResult SaveTransferAsset(TraderTransfer transfer, int assetId = 0)
        {
            return Json(new SpanneredAssetRules(dbContext).SaveTransferAsset(CurrentDomainId(), assetId, transfer, CurrentUser().Id));
        }
        [HttpPost]
        public ActionResult SaveTraderPurchaseAsset(TraderPurchase traderPurchase, int assetId = 0)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            traderPurchase.Location = CurrentDomain().TraderLocations.FirstOrDefault(q => q.Id == traderPurchase.Location.Id);
            result = new SpanneredAssetRules(dbContext).SaveTraderPurchaseForAsset(assetId, traderPurchase, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReloadRelatedPurchases(int assetId)
        {
            var asset = new SpanneredAssetRules(dbContext).GetAssetById(assetId);
            ViewBag.WorkGroupsOfMember = new TraderWorkGroupsRules(dbContext).GetWorkGroups(asset.Location?.Id ?? 0, CurrentDomain(), CurrentUser().Id, TraderProcessName.TraderPurchaseProcessName);
            return PartialView("_TableRelatedPurchases", asset);
        }
        public ActionResult LoadPurchaseItems(int purchaseId)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(purchaseId);
            if (purchaseModel == null)
                return View("Error");
            return PartialView("_ModalPurchaseItems", purchaseModel);
        }
        public ActionResult GetSpanneredInventoryItems([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int locationId, string keyword, int groupId, int linkTaskId = 0, int itemId = 0, int unitId = 0)
        {
            return Json(new SpanneredAssetRules(dbContext).GetSpanneredInventoryItems(requestModel, CurrentDomainId(), locationId, keyword, groupId, linkTaskId, itemId, unitId), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetTraderGroupsByLocation(int lid)
        {
            var groups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(CurrentDomainId(), lid);
            return Json(groups.Select(s => new { id = s.Id, text = s.Name }).ToList(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetItemProductByWorkgroupIsBought(int wgid, int locationId, int assetId = 0)
        {
            return Json(new SpanneredAssetRules(dbContext).GetItemProductByWorkgroupIsBought(CurrentDomain(), wgid, locationId, CurrentUser().Id, assetId), JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadModalTransferItems(int locationId, int aiId = 0)
        {
            var domain = CurrentDomain();
            ViewBag.CurrentLocationId = locationId;
            ViewBag.AssetInventory = new SpanneredAssetRules(dbContext).GetAssetInventoryById(aiId);
            ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domain.Id);
            ViewBag.WorkgroupTransfer = new TraderWorkGroupsRules(dbContext).GetWorkGroups(locationId, domain, CurrentUser().Id, TraderProcessName.TraderTransferProcessName);
            return PartialView("_ModalTransferItemsContent");
        }
        public ActionResult GetUnitByItemId(int itemId)
        {
            var traderItem = new TraderItemRules(dbContext).GetById(itemId);
            if (traderItem != null)
                return Json(traderItem.Units.Select(s => new { id = s.Id, text = s.Name }).ToList(), JsonRequestBehavior.AllowGet);
            else
                return Json(null, JsonRequestBehavior.AllowGet);
        }
        public ActionResult RemoveItemSpanneredByAIId(int aiId)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.SpanneredAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }

            return Json(new SpanneredAssetRules(dbContext).RemoveItemSpanneredByAssetInventoryId(aiId));
        }
        public ActionResult loadModalConsume(int lId, int itemId = 0)
        {
            var domain = CurrentDomain();
            ViewBag.CurrentLocationId = lId;
            ViewBag.workgroups = new SpanneredWorkgroupRules(dbContext).GetallWorkgroupsHasProcess(ProcessesConst.ConsumptionReports, CurrentUser().Id, domain.Id, lId, true);
            ViewBag.Tasks = new SpanneredConsumeReportRules(dbContext).GetAssetTasksCompleted(CurrentDomainId(), lId);
            ViewBag.ItemId = itemId;
            return PartialView("_ModalConsumeContent");
        }
        public ActionResult SaveConsumeReport(ConsumeReportCustome consumeReport)
        {
            consumeReport.Domain = CurrentDomain();
            return Json(new SpanneredConsumeReportRules(dbContext).SaveConsumeReport(consumeReport, CurrentUser().Id));
        }
        public ActionResult loadTableConsumedStock(int taskId, int lId)
        {
            ViewBag.LocationId = lId;
            return PartialView("_TableConsumedStock", new SpanneredConsumeReportRules(dbContext).GetConsumedStockByTaskId(taskId));
        }
        public ActionResult ConsumeReportReview(int id)
        {
            var rule = new SpanneredConsumeReportRules(dbContext);
            var consume = rule.GetConsumptionById(id);
            if (consume == null)
                return View("Error");
            var currentDomainId = consume.Domain.Id;
            ViewBag.CurrentPage = "Approval"; SetCurrentPage("Approval");
            ValidateCurrentDomain(consume.Domain, consume.Workgroup?.SourceQbicle.Id ?? 0);
            SetCurrentApprovalIdCookies(consume.ConsumptionApprovalProcess.Id);
            ViewBag.ApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(consume.ConsumptionApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, CurrentUser().Id);
            var timeline = rule.ConsumeApprovalStatusTimeline(consume?.Id ?? 0, CurrentUser().Timezone);
            var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();
            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;
            ViewBag.tradergroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(currentDomainId, consume.Location.Id);
            return View(consume);
        }
        public ActionResult GetMembersQbicleByWorkgroupId(int wgId, string search)
        {
            return Json(new SpanneredWorkgroupRules(dbContext).GetMembersQbicleByWorkgroupId(wgId, search), JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadTeamsByWorkgroupId(int wgId)
        {
            return PartialView("_WorkgroupTeams", new SpanneredWorkgroupRules(dbContext).getWorkgroupById(wgId));
        }
        public ActionResult LoadConsumeItemsByTaskId(int taskId, int lId = 0)
        {
            ViewBag.LocationId = lId;
            return PartialView("_TableCPSItemsTask", new TasksRules(dbContext).GetTaskById(taskId));
        }
        public ActionResult UpdateUsedOfConsumeItems(int ciId, decimal value)
        {
            return Json(new SpanneredConsumeReportRules(dbContext).UpdateUsedOfConsumeItems(ciId, value, CurrentUser().Id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveLocationSelected(string lid)
        {
            var settingUiLocation = new UiSetting()
            {
                CurrentPage = QbiclePages.pageSpannered,
                CurrentUser = new UserRules(dbContext).GetById(CurrentUser().Id),
                Key = SpanneredKeyStoredUiSettings.ddlLocationId,
                Value = lid
            };
            var uis = new List<UiSetting>();
            uis.Add(settingUiLocation);
            new QbicleRules(dbContext).StoredUiSettings(uis);
            return Json(true);
        }
    }
}