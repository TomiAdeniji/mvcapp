using Qbicles.BusinessRules;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CleanBooksData;
using Microsoft.AspNet.Identity;
using static Qbicles.BusinessRules.Enums;
using static Qbicles.Models.QbicleTask;
using Qbicles.BusinessRules.Helper;
using System.Linq.Dynamic;
using Qbicles.BusinessRules.Model;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TasksController : BaseController
    {
        ReturnJsonModel refModel;
        #region CB Task Transaction matching

        [HttpPost]
        public JsonResult LoadTasks(int groupId, SortOrderBy orderBy = SortOrderBy.NameAZ)
        {
            try
            {
                var tkRules = new CBTasksRules(dbContext);
                var currentUserId = CurrentUser().Id;
                var domainId = CurrentDomainId();
                var wgs = new CBWorkGroupsRules(dbContext).GetAllCbWorkGroup(domainId);
                var memberTaskExecution = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.TaskExecutionProcessName))
                    .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);
                var memberTask = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.TaskProcessName))
                    .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);
                ViewBag.MemberTaskExecution = memberTaskExecution;
                ViewBag.MemberTask = memberTask;
                var model = tkRules.LoadTaskGroups(groupId, orderBy, domainId);
                string modelString = RenderViewToString("_TaskContent", model);

                return Json(new { ModelString = modelString });
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }

        }
        private string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            var userId = CurrentUser().Id;
            var UserRoleRights = new AppRightRules(dbContext).UserRoleRights(userId, CurrentDomainId());
            ViewBag.UserRoleRights = UserRoleRights;
            ViewBag.UserRoles = new UserRules(dbContext).GetById(userId).DomainRoles.Select(n => n.Name).ToList();
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }


        public ActionResult save_reconciliationtaskgroups(taskgroup group)
        {
            refModel = new ReturnJsonModel();
            try
            {
                if (group != null)
                {
                    group.qbicledomain = CurrentDomain();
                    if (group.Id <= 0)
                    {
                        group.CreatedById = CurrentUser().Id;
                    }
                    var accountRules = new CBTasksRules(dbContext);
                    refModel = accountRules.SaveTaskGroups(group);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult dupplicateRecTaskGroupCheck(taskgroup recGroup)
        {
            var dupplicate = new CBTasksRules(dbContext).DupplicateRecTaskGroupCheck(recGroup);
            return Json(new { dupplicate = dupplicate }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RedirectTransactionMatching(
         int taskid = 0, string taskname = "", string accountName = "", int accountId = 0, string accountName2 = "", int accountId2 = 0,
         int transactionMatchingTypeId = 0, int transactionmatchingtaskId = 0, int taskInstanceId = 0)
        {
            var result = false;
            if (taskid != 0)
            {

                TempData["taskIdMatching"] = taskid;
                TempData["taskKeyMatching"] = taskid.Encrypt();
                TempData["taskNameMatching"] = taskname;

                TempData["accountNameMatching"] = accountName;
                TempData["accountIdMatching"] = accountId;

                TempData["accountNameMatching2"] = accountName2;
                TempData["accountIdMatching2"] = accountId2;
                TempData["transactionMatchingTypeId"] = transactionMatchingTypeId;
                TempData["transactionmatchingtaskId"] = transactionmatchingtaskId;
                TempData["taskInstanceId"] = taskInstanceId;
                result = true;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RedirectTransactionMatchingReport(TransactionMatchingReportParameter parameter)
        {

            var cookies = new HttpCookie("tmrParameter", parameter.ToJson().Encrypt())
            {
                SameSite = SameSiteMode.Strict
            };
            cookies.Expires.AddDays(1);
            HttpContext.Response.Cookies.Add(cookies);
            var result = true;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
        public ActionResult ManageReconciliationTasks()
        {
            try
            {
                var qb = new CBTasksRules(dbContext);
                var tk = qb.GetTasks();

                var model = qb.GetTaskgroup(CurrentDomainId());
                ViewBag.tasks = tk;
                ViewBag.taskexecutioninterval = qb.GetTaskexecutioninterval();
                ViewBag.accountgroup = qb.GetAccountgroup();
                ViewBag.CurrentPage = "ManageReconciliationTasks"; SetCurrentPage("ManageReconciliationTasks");

                var taskaccount = qb.GetAccounts(CurrentDomainId());
                var transactionmatching = qb.GetTransactionmatchingtype();
                var balanceanalysispredefactions = qb.GetBalanceanalysispredefaction();
                var tasktype = qb.GetTasktype();
                ViewBag.taskaccount = taskaccount;
                ViewBag.transactionmatching = transactionmatching;
                ViewBag.tastype = tasktype;
                ViewBag.balanceanalysispredefactions = balanceanalysispredefactions;
                return View(model);
            }

            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }
        public ActionResult FillterTaskToTaskType(int taskTypeId, int groupId)
        {
            try
            {
                CBTasksRules qb = new CBTasksRules(dbContext);
                var groups = qb.GetTaskToTaskType(taskTypeId, groupId)
                    .Select(tks => new
                    {
                        taskId = tks.Id,
                        taskName = tks.Name,
                        taskTypeId = tks.TaskTypeId,
                        taskDecription = tks.Description,
                        taskinstance = tks.taskinstances.OrderByDescending(m => m.DateExecuted).FirstOrDefault(),
                        userCreate = tks.user.Forename + " " + tks.user.Surname,
                        Email = tks.user.Email,
                        tasktypeName = tks.tasktype.Name,
                        status = tks.taskinstances.FirstOrDefault() != null ? (tks.TaskTypeId == Enums.TypeOfTask.TransactionMatching ? (tks.taskinstances.FirstOrDefault() != null && tks.taskinstances.FirstOrDefault().IsComplete == 0 ? "In Progress" : "Report Completed") : (tks.TaskTypeId == Enums.TypeOfTask.TransactionAnlysis ? (tks.taskinstances.FirstOrDefault() != null && tks.taskinstances.FirstOrDefault().IsComplete == 0 ? "In Progress" : "Report Generated") : "Close")) : "Not run"
                    })
                    .ToList();
                if (taskTypeId != 0)
                {
                    groups = groups.Where(t => t.taskTypeId == taskTypeId).ToList();
                }
                return Json(groups, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        /// <summary>
        /// check validation has delete
        /// </summary>
        /// <param name="taskGroupId"></param>
        /// <returns></returns>
        public ActionResult checkDeleteTaskGroup(int taskGroupId)
        {
            try
            {
                CBTasksRules qb = new CBTasksRules(dbContext);
                bool isDelete = qb.CheckDeleteTaskGroup(taskGroupId);
                return Json(isDelete, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        /// <summary>
        /// delete task group
        /// </summary>
        /// <param name="taskGroupId"></param>
        /// <returns></returns>
        public ActionResult DeleteTaskGroup(int taskGroupId)
        {
            try
            {
                CBTasksRules qb = new CBTasksRules(dbContext);
                var rs = qb.DeleteTaskGroup(taskGroupId);
                return Json(new
                {
                    status = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(new
                {
                    status = false
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult UpdateTask(QbicleTask task)
        {
            var result = new ReturnJsonModel();
            try
            {
                result.actionVal = 1;
                if (task.Id > 0)
                {
                    var _taskAdapter = new TasksRules(dbContext);
                    task = _taskAdapter.UpdateTask(task);
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                result.actionVal = 2;
                result.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult get_reconciliationtaskgroups(int id)
        {
            try
            {
                CBTasksRules qb = new CBTasksRules(dbContext);
                var reconciliationtaskgroups = qb.Get_reconciliationtaskgroups(id);

                return Json(reconciliationtaskgroups, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult get_reconciliationtasks(int id)
        {
            CBTasksRules qb = new CBTasksRules(dbContext);
            var reconciliationtasks = qb.Get_reconciliationtasks(id);
            return Json(reconciliationtasks, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTaskEdit(int id)
        {
            try
            {
                var qb = new CBTasksRules(dbContext);
                var model = qb.GetTaskEdit(id);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }
        public JsonResult checkEdit(int id)
        {
            try
            {
                CBTasksRules qb = new CBTasksRules(dbContext);
                var is_InPresson = qb.CheckEdit(id);
                return Json(new { Status = true, isInPresson = is_InPresson }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }
        public JsonResult GetUserAsign()
        {
            try
            {
                var qb = new CBTasksRules(dbContext);
                var rs = qb.GetUserAsign(CurrentDomainId());
                return Json(rs, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }
        public JsonResult get_editBalanceMapingRuler(int id)
        {
            try
            {
                CBTasksRules qb = new CBTasksRules(dbContext);
                var lstBalanceModel = new List<BalanceanalysisModel>();
                var balance = qb.Get_editBalanceMapingRuler(id, ref lstBalanceModel);
                if (lstBalanceModel.Any())
                    Session["BalanceMappingRuler"] = lstBalanceModel;
                return Json(balance, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }
        public JsonResult get_editBalanceAction(int id)
        {
            try
            {
                CBTasksRules qb = new CBTasksRules(dbContext);
                var balance = qb.Get_editBalanceAction(id);
                return Json(balance, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }
        public JsonResult get_rulesaccess(int id)
        {
            try
            {
                var qb = new CBTasksRules(dbContext);
                var first = qb.Get_rulesaccess(id);
                return Json(first, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return null;
            }
        }
        public ActionResult TaskNameCheck(string name, int id)
        {
            try
            {
                var qb = new CBTasksRules(dbContext);
                var dupplicate = qb.TaskNameCheck(name, id);
                return Json(new { dupplicate = dupplicate }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult Save_ManageReconciliationTasks(task rectask, string InitialTransactionDate, string[] role_grant, int AccountId1 = 0, int AccountId2 = 0,
            int TaskTypeIdOld = 0, int DateVariance = 0, int amounVariance = 0, int transactionmatchingtaskrulesaccessId = 0,
            List<balanceanalysismappingrule> balancemapingruler = null, List<balanceanalysisaction> balanceAction = null,
            int QbicleId = 0, string TopicName = "", TaskPriorityEnum Priority = TaskPriorityEnum.Low, string Deadline = "", int WorkGroupId = 0)
        {
            try
            {
                var qb = new CBTasksRules(dbContext);
                var qb_task = new TasksRules(dbContext).GetTaskById(rectask.Id);
                if (qb_task != null)
                {
                    //qb_task.task = rectask;
                    rectask.QbicleTask = qb_task;
                }

                rectask.WorkGroup = dbContext.CBWorkGroups.Find(rectask.WorkGroup.Id);
                var rs = qb.SaveCBTask((List<BalanceanalysisModel>)Session["BalanceMappingRuler"],
                    rectask, InitialTransactionDate, CurrentUser().Id, AccountId1, AccountId2,
                    TaskTypeIdOld, DateVariance, amounVariance, transactionmatchingtaskrulesaccessId, balancemapingruler, balanceAction
                    , Priority, Deadline, role_grant, WorkGroupId);

                return Json(new
                {
                    status = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult GetDomainRoleAccounts(int taskId)
        {
            try
            {
                var rules = new CBTasksRules(dbContext);
                var obj = rules.GetDomainRoleAccounts(taskId);
                return Json(obj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult DeleteTask(task task)
        {
            try
            {
                var qb = new CBTasksRules(dbContext);
                var rs = qb.DeleteTask(task, User.Identity.GetUserId());

                return Json(new
                {
                    status = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(new
                {
                    status = false
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ValidDeleteTask(int taskId)
        {
            try
            {
                var task = new CBTasksRules(dbContext).GetTask(taskId);
                return Json(new
                { status = task.taskinstances.Any() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(new
                {
                    status = true
                }, JsonRequestBehavior.AllowGet);
            }
        }
        /*Add/ edit task*/

        public ActionResult DuplicateTaskNameCheck(int cubeId, string taskKey, string taskName)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var taskId = string.IsNullOrEmpty(taskKey) ? 0 : int.Parse(taskKey.Decrypt());
                refModel.result = new TasksRules(dbContext).DuplicateTaskNameCheck(cubeId, taskId, taskName);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                refModel.result = false;
                refModel.Object = ex;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add a Task for the Qbicle and display it in the Dashboard panel
        /// </summary>
        /// <param name="task"></param>
        /// <param name="taskAssign"></param>
        /// <param name="DueDate"></param>
        /// <param name="qbicleId"></param>
        /// <param name="taskAttachments"></param>
        /// <returns></returns>

        public ActionResult SaveQbicleTask(QbicleTask task, string Assignee, string ProgrammedStart, string[] Watchers,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize,
             int TopicId, int[] ActivitiesRelate, List<QbicleStep> Steplst, int? Type,
            string LastOccurrence, string DayOrMonth, int? pattern, List<string> listDate, short? monthdates)
        {
            refModel = new ReturnJsonModel();
            try
            {
                string currentDatetimeFormat = CurrentUser().DateTimeFormat;
                DateTime dtLastOccurrence = DateTime.UtcNow;
                var lstDate = new List<CustomDateModel>();
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
                //var media = new MediaModel
                //{
                //    UrlGuid = mediaObjectKey,
                //    Name = mediaObjectName,
                //    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                //    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
                //};
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

                QbicleRecurrance _recurrance = null;

                if (task.isRecurs)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(LastOccurrence))
                            dtLastOccurrence = TimeZoneInfo.ConvertTimeToUtc(LastOccurrence.ConvertDateFormat(CurrentUser().DateFormat), tz);
                        if (listDate != null && listDate.Any())
                        {
                            var arrDate = listDate[0].Split(',');
                            if (arrDate != null)
                            {
                                CustomDateModel cDate;
                                foreach (var item in arrDate)
                                {
                                    cDate = new CustomDateModel
                                    {
                                        StartDate = TimeZoneInfo.ConvertTimeToUtc(item.ConvertDateFormat(currentDatetimeFormat), tz)
                                    };
                                    lstDate.Add(cDate);
                                }
                            }

                        }
                    }
                    catch
                    {
                        dtLastOccurrence = DateTime.UtcNow;
                    }
                    _recurrance = new QbicleRecurrance
                    {
                        Days = Type == 0 || Type == 1 ? DayOrMonth : "",
                        Months = Type == 2 ? DayOrMonth : "",
                        FirstOccurrence = task.ProgrammedStart ?? DateTime.UtcNow,
                        LastOccurrence = dtLastOccurrence,
                        MonthDate = monthdates.HasValue ? monthdates.Value : (short)0
                    };
                    if (Type != null)
                        _recurrance.Type = (QbicleRecurrance.RecurranceTypeEnum)Type;
                    if (Type == 2)
                        _recurrance.Pattern = (short)pattern;
                }


                refModel = new TasksRules(dbContext).
                    SaveTask(task, IsCreatorTheCustomer(), Assignee, null, Watchers, CurrentQbicleId(),
                    CurrentUser().Id, TopicId, ActivitiesRelate, Steplst, _recurrance, lstDate, GetOriginatingConnectionIdFromCookies(), AppType.Web);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
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
        public ActionResult CountCanTasksDelete(int ctaskId)
        {
            try
            {
                return Json(new TasksRules(dbContext).CountCanTasksDelete(ctaskId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult RecurringTasksDelete(int ctaskId)
        {
            try
            {
                var result = new TasksRules(dbContext).RecurringTasksDelete(ctaskId);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        /*End add/ edit task*/

        public ActionResult SetTaskSelected(string key, string goBack)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());

                //Check for activity accessibility
                var checkResult = new QbicleRules(dbContext).CheckActivityAccibility(id, CurrentUser().Id);
                if (checkResult.result && (bool)checkResult.Object == true)
                {
                    refModel = new ReturnJsonModel();
                    var task = new TasksRules(dbContext).GetTaskById(id);
                    this.SetCurrentQbicleIdCookies(task.Qbicle?.Id ?? 0);
                    refModel.msgId = task.Qbicle.Domain.Id.ToString();
                    refModel.Object = task.ComplianceTask != null ? $"/Operator/ComplianceTask?id={task.ComplianceTask.Id}&{task.Id}" : "/Qbicles/Task";
                    refModel.result = true;
                    this.SetCurrentTaskIdCookies(id);
                    this.SetCookieGoBackPage(goBack);
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(checkResult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        /// Task Completion
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>

        public ActionResult CloseTask(int taskId)
        {
            try
            {

                refModel = new ReturnJsonModel();
                var result = new TasksRules(dbContext).CloseTask(taskId, CurrentUser());
                refModel.result = result;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }


        [HttpGet]
        public ActionResult MyDeskLoadMoreTask(int skip, int folderId)
        {
            try
            {

                // task
                string currUserId = CurrentUser().Id;
                var querry = from pin in dbContext.MyPins
                             join desk in dbContext.MyDesks on pin.Desk.Id equals desk.Id
                             where desk.Owner.Id == currUserId && pin.PinnedActivity.ActivityType ==
                                QbicleActivity.ActivityTypeEnum.TaskActivity
                             select pin.PinnedActivity;
                var myPinnedTasks = querry.ToList();
                var tasksAndApprovals = new MyDesksRules(dbContext).GetTasksAndApprovalsByUserId(currUserId, folderId, skip);
                var listobj = new List<object>();
                var days = 0; var _remaining = "";
                foreach (var item in tasksAndApprovals)
                {
                    if (item.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity)
                    {
                        var task = (QbicleTask)item;
                        if (task.DueDate != null)
                        {
                            days = ((TimeSpan)task.DueDate.Value.Subtract(DateTime.UtcNow)).Days;
                            if (days < 0)
                                _remaining = "<span class='label label-danger'>Overdue</span>";
                            else
                                _remaining = $"<span class='label label-warning'>{days.ToString() + "d " + ((TimeSpan)task.DueDate.Value.Subtract(DateTime.UtcNow)).Hours.ToString() + "h left"}</span>";
                        }
                        var obj = new
                        {
                            IsTasks = true,
                            IsPinned = myPinnedTasks.Any(y => y.Id == task.Id),
                            task.Id,
                            IsMember = (task.ActivityMembers.Any(m => m.Id == currUserId) || (task.StartedBy.Id == currUserId) ? true : false),
                            QbicleName = task.Qbicle != null ? task.Qbicle.Name : "",
                            DomainName = task.Qbicle != null ? (task.Qbicle.Domain != null ? task.Qbicle.Domain.Name : "") : "",
                            task.Name,
                            TaskPriority = task.Priority,
                            State = task.State == QbicleActivity.ActivityStateEnum.Open ? "In progress" : "Done",
                            PriorityName = task.Priority == QbicleTask.TaskPriorityEnum.Critical ? "Critical Task" : (task.Priority == QbicleTask.TaskPriorityEnum.General ? "General Task" : "Low Task"),
                            Remaining = _remaining
                        };

                        listobj.Add(obj);
                    }
                    else if (item.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequestApp)
                    {
                        var app = (ApprovalReq)item;
                        var journalApproval = app.JournalEntries.Count > 0;
                        var css = ""; var status = "";
                        switch (app.RequestStatus)
                        {
                            case ApprovalReq.RequestStatusEnum.Pending:
                                css = "label-warning";
                                status = ApprovalReq.RequestStatusEnum.Pending.ToString();
                                if (journalApproval)
                                {
                                    status = "Awaiting Review";
                                }
                                break;
                            case ApprovalReq.RequestStatusEnum.Reviewed:
                                css = "label-primary";
                                if (journalApproval)
                                {
                                    status = "Awaiting Approval";
                                }
                                break;
                            case ApprovalReq.RequestStatusEnum.Approved:
                                css = "label-success";
                                status = ApprovalReq.RequestStatusEnum.Approved.ToString();
                                break;
                            case ApprovalReq.RequestStatusEnum.Denied:
                                css = "label-danger";
                                status = ApprovalReq.RequestStatusEnum.Denied.ToString();
                                break;
                            case ApprovalReq.RequestStatusEnum.Discarded:
                                css = "label-danger";
                                status = ApprovalReq.RequestStatusEnum.Discarded.ToString();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        var obj = new
                        {
                            IsTasks = false,
                            IsPinned = myPinnedTasks.Any(y => y.Id == app.Id),
                            app.Id,
                            IsMember = (app.ActivityMembers.Any(m => m.Id == currUserId) ? true : false),
                            QbicleName = app.Qbicle != null ? app.Qbicle.Name : "",
                            DomainName = app.Qbicle != null ? (app.Qbicle.Domain != null ? app.Qbicle.Domain.Name : "") : "",
                            app.Name,
                            Priority = status,
                            jounralApproval = journalApproval,
                            Css = css,
                            ApprovalRequestDefinition = app.Name
                        };
                        listobj.Add(obj);
                    }

                }

                return Json(listobj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }




        public ActionResult GenerateTaskReport(int taskId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var result = new TasksRules(dbContext).GenerateTaskReport(taskId, CurrentUser().Timezone);
                refModel.Object = result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }



        public ActionResult GenerateAllTaskReport(List<int> listTaskId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var result = "";
                foreach (var taskId in listTaskId)
                {
                    result += new TasksRules(dbContext).GenerateTaskReport(taskId, CurrentUser().Timezone);
                }
                refModel.Object = result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult StartProgress(string taskKey)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var taskId = string.IsNullOrEmpty(taskKey) ? 0 : int.Parse(taskKey.Decrypt());
                var Complete = false;
                DateTime? ActualEnd = null;
                string Countdown = "";
                refModel.result = new TasksRules(dbContext).StartProgress(taskId, CurrentUser(), GetOriginatingConnectionIdFromCookies(), ref Complete, ref ActualEnd, ref Countdown);
                if (Complete)
                {
                    refModel.Object = new { Complete, ActualEnd, Countdown };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                refModel.Object = ex;
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult StepComplete(string taskKey, List<int> stepInstance)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var taskId = string.IsNullOrEmpty(taskKey) ? 0 : int.Parse(taskKey.Decrypt());
                var Complete = true;
                DateTime? ActualEnd = null;
                refModel.result = new TasksRules(dbContext).ConfirmSteps(taskId, stepInstance, CurrentUser(), GetOriginatingConnectionIdFromCookies(), ref Complete, ref ActualEnd);
                refModel.Object = new TasksRules(dbContext).GetStepInstances(taskId);
                refModel.Object2 = new { Complete, ActualEnd = ActualEnd.HasValue ? ActualEnd.Value.FormatDatetimeOrdinal() : "" };
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                refModel.Object = ex;
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult PerformanceReview(string taskKey, short rating)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var taskId = string.IsNullOrEmpty(taskKey) ? 0 : int.Parse(taskKey.Decrypt());
                QbiclePerformance performance = new QbiclePerformance();
                var task = dbContext.QbicleTasks.Find(taskId);
                if (task != null)
                {
                    performance.Task = task;
                    performance.Rating = rating;
                    performance.RatedBy = this.CurrentUser().Id;
                    performance.RatedDateTime = DateTime.UtcNow;
                    refModel.result = new TasksRules(dbContext).PerformanceReview(performance);
                    refModel.Object = new { rating };
                }
                else
                    refModel.result = false;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                refModel.Object = ex;
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult AddWorkLog(QbicleTimeSpent timeSpent, string ActivityKey)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var ActivityId = string.IsNullOrEmpty(ActivityKey) ? 0 : int.Parse(ActivityKey.Decrypt());

                var rules = new TasksRules(dbContext);

                var task = rules.GetTaskById(ActivityId);

                if (!rules.CheckAddWorkLog(task, CurrentUser().Id))
                {
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_392");
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                if (task != null)
                {
                    timeSpent.Task = task;
                    timeSpent.DateTime = DateTime.UtcNow;
                    refModel.result = rules.AddWorkLog(timeSpent, IsCreatorTheCustomer(), GetOriginatingConnectionIdFromCookies(), AppType.Web).result;
                    refModel.Object = new { DateTime = timeSpent.DateTime.ToString("dddd dd\"th\" MMMM yyyy"), timeSpent.Id, timeSpent.Days, timeSpent.Hours, timeSpent.Minutes };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                refModel.msg = ex.Message;
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AtivitiesRelated([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int currentQbicleId, string currentActivityKey)
        {
            try
            {
                var currentActivityId = string.IsNullOrEmpty(currentActivityKey) ? 0 : int.Parse(currentActivityKey.Decrypt());
                var result = new TasksRules(dbContext).GetActivesRelated(requestModel, currentQbicleId, currentActivityId);
                if (result != null)
                    return Json(result, JsonRequestBehavior.AllowGet);
                else
                    return Json(new DataTablesResponse(requestModel.Draw, new List<RelatedActivity>(), 0, 0), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(new DataTablesResponse(requestModel.Draw, new List<RelatedActivity>(), 0, 0), JsonRequestBehavior.AllowGet);
            }

        }
    }
}