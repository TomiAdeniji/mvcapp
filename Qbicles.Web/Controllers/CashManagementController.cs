using Qbicles.BusinessRules;
using Qbicles.BusinessRules.CMs;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader.CashMgt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Qbicles.Models.Trader.CashMgt.TillPayment;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class CashManagementController : BaseController
    {
        //public ApplicationDbContext dbContext = new ApplicationDbContext();

        #region Process with Safe
        public ActionResult SaveSafe(Safe safe, int traderCashAccountId)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };

            try
            {
                // The name of the safe is unique with the Domain
                bool isNameExisted = true;
                var safesOfDomain = new CMsRules(dbContext).GetSafesByDomain(CurrentDomainId());
                isNameExisted = safesOfDomain.Any(e => e.Id != safe.Id && e.Name.Equals(safe.Name.Trim()));
                if (isNameExisted)
                {
                    result.msg = "The name of the safe already exists in the current Domain.";
                    result.result = false;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                // Create the safe
                var bankAccount = new TraderCashBankRules(dbContext).GeTraderCashAccountById(traderCashAccountId);
                safe.CashAndBankAccount = bankAccount;
                //safe.Location = location
                result = new CMsRules(dbContext).SaveSafe(safe, CurrentUser().Id, CurrentLocationManage());

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Methods to show menu tabs - add, edit tabs
        public ActionResult CashMangementSafeAddEdit()
        {
            var domainId = CurrentDomainId();
            var locationId = CurrentLocationManage();
            ViewBag.ListCashAndBankAccount = new TraderCashBankRules(dbContext).GetTraderCashAccounts(domainId);
            return PartialView("_CashManagementSafeAddEdit", new CMsRules(dbContext).GetSafeByLocation(locationId));
        }

        // Methods to show sub tabs
        public ActionResult CMGeneralContent()
        {
            try
            {
                // Currently, Reserved space for any potential configuration
                return PartialView("_CMSubGeneral");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult CMDevicesContent(bool isSafe, bool isTill, string key)
        {
            try
            {
                var timezone = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);

                var tills = new CMsRules(dbContext).GetTillsByLocation(CurrentLocationManage());

                var safe = new CMsRules(dbContext).GetSafeByLocation(CurrentLocationManage());
                ViewBag.isSafeNull = safe == null ? true : false;

                if (key != null && !String.IsNullOrEmpty(key.Trim()))
                {
                    tills = tills.Where(t => t.Name.ToLower().Contains(key.ToLower())).ToList();
                    if (!safe.Name.ToLower().Contains(key.ToLower()))
                    {
                        safe = null;
                    }
                }


                ViewBag.ShowSafe = isSafe;
                ViewBag.ShowTill = isTill;
                ViewBag.Key = String.IsNullOrEmpty(key) ? "" : key;

                if (safe == null)
                    ViewBag.SafeBalance = 0;
                else
                {
                    var _safeBalance = new CMsRules(dbContext).getSafeBalance(safe.Id, CurrentUser(), timezone, CurrentDomainId());
                    ViewBag.SafeBalance = _safeBalance;
                }

                if (isSafe && isTill)
                {
                    ViewBag.Tills = tills;
                    ViewBag.Safe = safe;
                }
                else if (isSafe && !isTill)
                {
                    ViewBag.Tills = new List<Till>();
                    ViewBag.Safe = safe;
                }
                else if (isTill && !isSafe)
                {
                    ViewBag.Tills = tills;
                    ViewBag.Safe = null;
                }

                return PartialView("_CMSubDevices");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult TillPaymentReview(int tillPaymentId)
        {
            try
            {
                var tillPayment = dbContext.TillPayments.Find(tillPaymentId);
                ViewBag.TillPayment = tillPayment;
                ViewBag.DocRetrievalUrl = ConfigManager.ApiGetDocumentUri;
                return View("TillPaymentReview", tillPayment);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult TillPaymentReviewContent(int tillPaymentId)
        {
            try
            {
                var tillPayment = dbContext.TillPayments.Find(tillPaymentId);
                var userSetting = CurrentUser();
                ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).
                    GetTraderApprovalRight(tillPayment?.TillPaymentApprovalProcess?.ApprovalRequestDefinition?.Id ?? 0, userSetting.Id);
                ViewBag.CurrentUserId = userSetting.Id;
                ViewBag.CurrentTimeZone = userSetting.Timezone;
                ViewBag.CurrentDateTimeFormat = userSetting.DateTimeFormat;

                var timeline = new CMsRules(dbContext).TillPaymentApprovalStatusTimeline(tillPaymentId, userSetting.Timezone);
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return PartialView("_TillPaymentReview_Content", tillPayment);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        #endregion

        public ActionResult CashManagementTillAddEdit(int tillId)
        {
            // Get Till content
            var tillForEdit = new Till();
            var tillById = new CMsRules(dbContext).GetTillById(tillId);
            tillForEdit = tillById ?? new Till();

            // Get list PosDevices at current Location
            var listPosDevices = new PosDeviceRules(dbContext).GetByLocation(CurrentLocationManage());
            ViewBag.ListPosDevices = listPosDevices;

            return PartialView("_CashManagementTillAddEditTab", tillForEdit);
        }

        public ActionResult SaveTill(Till till, string listPosDeviceString)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };

            try
            {
                result = new CMsRules(dbContext).SaveTill(till, CurrentUser().Id, CurrentLocationManage(), listPosDeviceString);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                result.result = false;
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteTill(int tillId)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };

            try
            {
                var till = new CMsRules(dbContext).GetTillById(tillId);
                if (till == null)
                {
                    result.msg = "The till does not exist anymore.";
                    result.result = false;
                }
                else
                {
                    result = new CMsRules(dbContext).DeleteTill(till);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                result.result = false;
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TillDetail(int tillId)
        {
            var till = new CMsRules(dbContext).GetTillById(tillId);

            var tillPayments = till.Payments.Where(p =>  p.Status == TillPayment.TraderTillPaymentStatusEnum.PendingApproval
                                || p.Status == TillPayment.TraderTillPaymentStatusEnum.PendingReview).Select(d => d.CreatedDate).ToList();


            var user = CurrentUser();

            if (tillPayments.Count > 0)
                ViewBag.UnApproved = string.Join(", ", tillPayments.Select(d => d.ConvertTimeFromUtc(user.Timezone).ToString(user.DateFormat)));
            else
                ViewBag.UnApproved = "";

            if (till == null)
                return View("Error");
            else
                return View("TillDeTail", till);
        }

        public ActionResult TillLeftPanelPartial(int tillId, string unapproved)
        {
            ViewBag.TillId = tillId;
            ViewBag.UnApproved = unapproved;
            if (string.IsNullOrEmpty(unapproved))
                return PartialView("_TillLeftPanel");

            var tillPayments = dbContext.Tills.AsNoTracking().FirstOrDefault(e => e.Id == tillId)?.Payments.Where(p =>
                                   p.Status == TillPayment.TraderTillPaymentStatusEnum.PendingApproval
                                || p.Status == TillPayment.TraderTillPaymentStatusEnum.PendingReview).Select(d => d.CreatedDate).ToList(); ;
            if (tillPayments == null)
                return Json("", JsonRequestBehavior.AllowGet);
            var user = CurrentUser();
            ViewBag.UnApproved = string.Join(", ", tillPayments.Select(d => d.ConvertTimeFromUtc(user.Timezone).ToString(user.DateFormat)));

            return PartialView("_TillLeftPanel");

        }

        // Methods to add, edit till payment
        public ActionResult SaveTillPayment(TillPayment tillPayment, String type)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };

            try
            {


                if (type == "payin")
                {
                    tillPayment.Direction = TillPaymentDirection.InToTill;
                }
                else if (type == "payout")
                {
                    tillPayment.Direction = TillPaymentDirection.OutOfTill;
                }

                tillPayment.Status = TraderTillPaymentStatusEnum.PendingReview;

                result = new CMsRules(dbContext).SaveTillPayment(tillPayment, CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                result.result = false;
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateTillPayment(int tillPaymentId)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };

            try
            {
                result = new CMsRules(dbContext).UpdateTillPayment(tillPaymentId, CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Methods to add till/safe checkpoint
        public ActionResult SaveCheckpoint(Checkpoint checkpoint)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };

            try
            {
                var user = CurrentUser();
                var timezone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
                checkpoint.CheckpointDate = TimeZoneInfo.ConvertTimeToUtc(checkpoint.CheckpointDate, timezone);
                result = new CMsRules(dbContext).SaveCheckpoint(checkpoint, user.Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // Till Detail
        public ActionResult TillDetailTable(int tillId, TillDetailFilterParameter filter)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
            ViewBag.Filter = filter;
            ViewBag.TillId = tillId;
            if (filter != null && filter.DateRange != null)
            {
                if (filter.DateRange.Trim() == "")
                {
                    ViewBag.FromDateTime = "";
                    ViewBag.ToDateTime = "";
                }
                else
                {
                    var dateRange = filter.DateRange.Split('-');

                    //Format FromDateTime
                    var _startTime = dateRange[0].Trim().Split(' ');
                    var fromTime = _startTime[1];
                    var fromDate = _startTime[0].Split('/')[0];
                    var fromMonth = _startTime[0].Split('/')[1];
                    var fromYear = _startTime[0].Split('/')[2];
                    var _fromDateTimeFormatted = fromYear + "-" + fromMonth + "-" + fromDate + "T" + fromTime;

                    //Format ToDateTime
                    var _endTime = dateRange[1].Trim().Split(' ');
                    var toTime = _endTime[1];
                    var toDate = _endTime[0].Split('/')[0];
                    var toMonth = _endTime[0].Split('/')[1];
                    var toYear = _endTime[0].Split('/')[2];
                    var _toDateTimeFormatted = toYear + "-" + toMonth + "-" + toDate + "T" + toTime;

                    ViewBag.FromDateTime = _fromDateTimeFormatted;
                    ViewBag.ToDateTime = _toDateTimeFormatted;
                }
            }
            else
            {
                ViewBag.FromDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).ToString("yyyy-MM-ddT00:00:00");
                ViewBag.ToDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).ToString("yyyy-MM-ddT23:59:59");
            }

            return PartialView("_TillDetailTablePartial");
        }

        public ActionResult GetDataTableTillPayment([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int tillId, bool isApproved,
            string datetime, string[] status)
        {
            var user = CurrentUser();
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
            var rules = new CMsRules(dbContext);

            var domainId = CurrentDomainId();

            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

            var listTillTransactions = rules.GetListTillTransaction(tillId, user, timezone, domainId, currencySettings);

            var result = rules.TillTransactionsSearch(requestModel, user.Id, CurrentLocationManage(), domainId,
                listTillTransactions, keyword, datetime, CurrentUser().Timezone, isApproved, CurrentUser(), status, currencySettings);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        // Discussion for Till transactions
        public ActionResult DiscussionCashManagement(int tillPaymentId, int checkpointId)
        {
            var currentDomainId = CurrentDomainId();

            var setting = new TraderSettingRules(dbContext).GetTraderSettingByDomain(currentDomainId);
            if (setting == null)
            {
                return View("Error");
            }
            var user = CurrentUser();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                return View("ErrorAccessPage");

            if (tillPaymentId > 0)
            {
                var tillPayment = dbContext.TillPayments.Find(tillPaymentId);
                ValidateCurrentDomain(CurrentDomain(), tillPayment.WorkGroup?.Qbicle.Id ?? 0);
                if (tillPayment.Discussion == null)
                {
                    var saveDiscussionResult = new CMsRules(dbContext).CreateTillPaymentDiscussion(tillPaymentId, user.Id);
                    if (!saveDiscussionResult)
                    {
                        return View("Error");
                    }
                }
                var discussionModel = new TillTransaction
                {
                    AssociatedTillId = tillPayment.AssociatedTill.Id,
                    Status = tillPayment.Status.GetDescription(),
                    LabelStatus = new CMsRules(dbContext).GetLabelStatusFromEnum(tillPayment.Status)
                };
                var workgroup = tillPayment.WorkGroup;
                var currentQbicleId = workgroup.Qbicle.Id;

                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(currentQbicleId);

                ViewBag.CurrentQbicleId = tillPayment.Discussion.Qbicle.Id;
                ViewBag.CurrentQbicle = tillPayment.Discussion.Qbicle;
                ViewBag.TransactionName = "Till Payment " + tillPayment.Id;
                ViewBag.CurrentDiscussion = tillPayment.Discussion;

                SetCurrentDiscussionIdCookies(tillPayment.Discussion?.Id ?? 0);
                return View("TillDiscussion", discussionModel);
            }
            else if (checkpointId > 0)
            {
                var tillCheckpoint = dbContext.Checkpoints.Find(checkpointId);
                ValidateCurrentDomain(CurrentDomain(), tillCheckpoint.Discussion?.Qbicle.Id ?? 0);
                if (tillCheckpoint.Discussion == null)
                {
                    var saveDiscussionResult = new CMsRules(dbContext).CreateCheckpointDiscussion(checkpointId, user.Id);
                    if (!saveDiscussionResult)
                        return View("Error");
                }
                var discussionModel = new TillTransaction
                {
                    isCheckpoint = false,
                    AssociatedTillId = tillCheckpoint.VirtualTill.Id,
                    Status = "",
                    LabelStatus = ""
                };

                var currentQbicleId = tillCheckpoint.Discussion.Qbicle.Id;

                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(currentQbicleId);

                ViewBag.CurrentQbicleId = tillCheckpoint.Discussion.Qbicle.Id;
                ViewBag.CurrentQbicle = tillCheckpoint.Discussion.Qbicle;
                ViewBag.TransactionName = "Till Checkpoint " + tillCheckpoint.Id;
                ViewBag.CurrentDiscussion = tillCheckpoint.Discussion;

                SetCurrentDiscussionIdCookies(tillCheckpoint.Discussion?.Id ?? 0);
                return View("TillDiscussion", discussionModel);
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult DiscussionCashManagementShow(string disKey)
        {
            var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());
            var tillPayment = new CMsRules(dbContext).GetTillPaymentByActivityId(disId);
            var Checkpoint = new CMsRules(dbContext).GetCheckpointByActitvity(disId);

            if (tillPayment == null && Checkpoint != null)
            {
                return RedirectToAction("DiscussionCashManagement", new { tillPaymentId = 0, checkpointId = Checkpoint.Id });
            }
            else if (tillPayment != null && Checkpoint == null)
            {
                return RedirectToAction("DiscussionCashManagement", new { tillPaymentId = tillPayment.Id, checkpointId = 0 });
            }
            else
            {
                return View("Error");
            }
        }


        // Discussion for Safe transactions
        public ActionResult DiscussionForSafeTransactions(int tillPaymentId, int checkpointId, int cashAccountTransactionId)
        {
            var user = CurrentUser();
            var currentDomainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                return View("ErrorAccessPage");
            var setting = new TraderSettingRules(dbContext).GetTraderSettingByDomain(currentDomainId);
            if (setting == null)
            {
                return View("Error");
            }

            if (tillPaymentId > 0)
            {
                var tillPayment = dbContext.TillPayments.Find(tillPaymentId);
                ValidateCurrentDomain(CurrentDomain(), tillPayment.Discussion?.Qbicle.Id ?? 0);
                if (tillPayment.Discussion == null)
                {
                    var saveDiscussionResult = new CMsRules(dbContext).CreateTillPaymentDiscussion(tillPaymentId, user.Id);
                    if (!saveDiscussionResult)
                    {
                        return View("Error");
                    }
                }
                var discussionModel = new SafeTransaction
                {
                    AssociatedTillId = tillPayment.AssociatedTill.Id,
                    AssociatedSafeId = tillPayment.AssociatedSafe.Id,
                    Status = tillPayment.Status.GetDescription(),
                    LabelStatus = new CMsRules(dbContext).GetLabelStatusFromEnum(tillPayment.Status)
                };
                var workgroup = tillPayment.WorkGroup;
                var currentQbicleId = workgroup.Qbicle.Id;

                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(currentQbicleId);

                ViewBag.CurrentQbicleId = tillPayment.Discussion.Qbicle.Id;
                ViewBag.CurrentQbicle = tillPayment.Discussion.Qbicle;
                ViewBag.TransactionName = "Till Payment " + tillPayment.Id;
                ViewBag.CurrentDiscussion = tillPayment.Discussion;
                SetCurrentDiscussionIdCookies(tillPayment.Discussion?.Id ?? 0);
                return View("SafeDiscussion", discussionModel);
            }
            else if (checkpointId > 0)
            {
                var safeCheckpoint = dbContext.Checkpoints.Find(checkpointId);
                ValidateCurrentDomain(CurrentDomain(), safeCheckpoint.Discussion?.Qbicle.Id ?? 0);
                if (safeCheckpoint.Discussion == null)
                {
                    var saveDiscussionResult = new CMsRules(dbContext).CreateCheckpointDiscussion(checkpointId, user.Id);
                    if (!saveDiscussionResult)
                        return View("Error");
                }
                var discussionModel = new SafeTransaction
                {
                    isCheckpoint = false,
                    AssociatedSafeId = safeCheckpoint.VirtualSafe.Id,
                    Status = "",
                    LabelStatus = ""
                };

                var currentQbicleId = safeCheckpoint.Discussion.Qbicle.Id;

                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(currentQbicleId);

                ViewBag.CurrentQbicleId = safeCheckpoint.Discussion.Qbicle.Id;
                ViewBag.CurrentQbicle = safeCheckpoint.Discussion.Qbicle;
                ViewBag.TransactionName = "Safe Checkpoint " + safeCheckpoint.Id;
                ViewBag.CurrentDiscussion = safeCheckpoint.Discussion;

                SetCurrentDiscussionIdCookies(safeCheckpoint.Discussion?.Id ?? 0);
                return View("SafeDiscussion", discussionModel);
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult DiscussionForSafeTransactionsShow(int disId)
        {
            var tillPayment = new CMsRules(dbContext).GetTillPaymentByActivityId(disId);
            var Checkpoint = new CMsRules(dbContext).GetCheckpointByActitvity(disId);

            if (tillPayment == null && Checkpoint != null)
            {
                return RedirectToAction("DiscussionForSafeTransactions", new { tillPaymentId = 0, checkpointId = Checkpoint.Id, cashAccountTransactionId = 0 });
            }
            else if (tillPayment != null && Checkpoint == null)
            {
                return RedirectToAction("DiscussionForSafeTransactions", new { tillPaymentId = tillPayment.Id, checkpointId = 0, cashAccountTransactionId = 0 });
            }
            else
            {
                return View("Error");
            }
        }

        // Safe Detail
        /// <summary>
        /// Show the Detail page of the safe
        /// </summary>
        /// <returns></returns>
        public ActionResult SafeDetail(int safeId)
        {
            var user = CurrentUser();
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
            var safe = new CMsRules(dbContext).GetSafeById(safeId);
            var locationId = safe.Location?.Id ?? 0;
            var cashAndBankAccountId = safe.CashAndBankAccount?.Id ?? 0;

            if (safe == null)
                ViewBag.SafeBalance = 0;
            else
            {
                var _safeBalance = new CMsRules(dbContext).getSafeBalance(safe.Id, user, timezone, CurrentDomainId());
                ViewBag.SafeBalance = _safeBalance;
            }
            ViewBag.LocationId = locationId;
            ViewBag.CashAndBankAccountId = cashAndBankAccountId;

            if (safe == null)
                return View("Error");
            else
                return View("SafeDetail", safe);
        }

        public ActionResult SafeDetailTable(int safeId, SafeDetailFilterParameter filter)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
            ViewBag.Filter = filter;
            ViewBag.SafeId = safeId;
            if (filter != null && filter.DateRange != null)
            {
                if (filter.DateRange.Trim() == "")
                {
                    ViewBag.FromDateTime = "";
                    ViewBag.ToDateTime = "";
                }
                else
                {
                    var dateRange = filter.DateRange.Split('-');

                    //Format FromDateTime
                    var _startTime = dateRange[0].Trim().Split(' ');
                    var fromTime = _startTime[1];
                    var fromDate = _startTime[0].Split('/')[0];
                    var fromMonth = _startTime[0].Split('/')[1];
                    var fromYear = _startTime[0].Split('/')[2];
                    var _fromDateTimeFormatted = fromYear + "-" + fromMonth + "-" + fromDate + "T" + fromTime;

                    //Format ToDateTime
                    var _endTime = dateRange[1].Trim().Split(' ');
                    var toTime = _endTime[1];
                    var toDate = _endTime[0].Split('/')[0];
                    var toMonth = _endTime[0].Split('/')[1];
                    var toYear = _endTime[0].Split('/')[2];
                    var _toDateTimeFormatted = toYear + "-" + toMonth + "-" + toDate + "T" + toTime;

                    ViewBag.FromDateTime = _fromDateTimeFormatted;
                    ViewBag.ToDateTime = _toDateTimeFormatted;
                }
            }
            else
            {
                ViewBag.FromDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).ToString("yyyy-MM-ddT00:00:00");
                ViewBag.ToDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).ToString("yyyy-MM-ddT23:59:59");
            }

            return PartialView("_SafeDetailTablePartial");
        }

        public ActionResult GetDataTableSafePayment([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string datetime, int safeId, bool isApproved)
        {
            var user = CurrentUser();
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
            var listSafeTransactions = new CMsRules(dbContext).GetListSafeTransaction(safeId, user, timezone, CurrentDomainId());
            var result = new CMsRules(dbContext).SafeTransactionsSearch(requestModel, user.Id, CurrentLocationManage(), CurrentDomainId(), listSafeTransactions, keyword, datetime, CurrentUser().Timezone, isApproved, CurrentUser());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }
    }
}