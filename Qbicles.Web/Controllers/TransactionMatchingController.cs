using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Threading.Tasks;
using CleanBooksData;
using Qbicles.BusinessRules;
using System.Text;
using static Qbicles.BusinessRules.TransactionMatchingRules;
using static Qbicles.BusinessRules.HelperClass;
using System.Globalization;
using Qbicles.Models;
using Qbicles.BusinessRules.Helper;

namespace Qbicles.Web.Controllers
{
    public class TransactionMatchingController : BaseController
    {
        public ActionResult TransactionMatchingRecords()
        {
            try
            {
                if (TempData["taskinstanceId"] == null)
                    return Redirect("/Apps/Tasks");
                var transactionRules = new TransactionMatchingRules(dbContext);
                if ((int)TempData["taskinstanceId"] > 0 && TempData["accountIdMatching"] != null && TempData["accountIdMatching2"] != null)
                {
                    var Idtask = (int)TempData["taskinstanceId"];
                    var taskinstance = transactionRules.GetTaskinstance(Idtask);
                    ViewBag.startdate = taskinstance.StartDate;
                    ViewBag.enddate = taskinstance.EndDate;
                }
                var taskId = (int)TempData["taskIdMatching"];
                SetCurrentTaskIdCookies(taskId);
                var task = transactionRules.Gettransactionmatchingtaskrulesaccess(taskId);
                if (task == null)
                    task = new transactionmatchingtaskrulesacces();
                var amountvance = transactionRules.Gettransactionmatchingamountvariancevalue();
                var datevance = transactionRules.Gettransactionmatchingdatevariancevalue();
                var rulers = transactionRules.Gettransactionmatchingrule(taskId);
                if (rulers == null)
                    rulers = new transactionmatchingrule();
                ViewBag.amountvance = amountvance;
                ViewBag.datevance = datevance;
                ViewBag.ruler = rulers;
                ViewBag.IsDateVarianceVisible = task.IsDateVarianceVisible;
                ViewBag.IsAmountVarianceVisible = task.IsAmountVarianceVisible;
                ViewBag.taskinstanceId = (int)TempData["taskinstanceId"];
                var qbTask = new TasksRules(dbContext).GetTaskById(taskId);
                SetCurrentTaskIdCookies(taskId);
                ViewBag.QbTask = qbTask;
                ViewBag.CurrentUserId = CurrentUser().Id;

                ViewBag.CurrentPage = "Task"; SetCurrentPage("Task");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

            return View();
        }
        //[HttpPost]
        public ActionResult TransactionMatchingReport()
        {
            try
            {
                var parameter = new TransactionMatchingReportParameter();
                if (Request.Cookies["tmrParameter"] != null)
                    parameter = Request.Cookies["tmrParameter"].Value.Decrypt().ParseAs<TransactionMatchingReportParameter>();
                ViewBag.ReportParameter = parameter;

                ViewBag.CurrentPage = "TransactionMatchingReport"; SetCurrentPage("TransactionMatchingReport");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return View("Error");
            }

            return View();
        }
        public ActionResult InitTransactionMatchingReport(int taskInsId)
        {
            var parms = new TransactionMatchingRules(dbContext).GetTaskInstanceMatching(taskInsId);
            var cookies = new System.Web.HttpCookie("tmrParameter", parms.ToJson().Encrypt());
            cookies.Expires.AddDays(1);
            HttpContext.Response.Cookies.Add(cookies);
            return RedirectToAction("TransactionMatchingReport");
        }
        public ActionResult InitTransactionMatchingRecords(int taskInsId)
        {
            var parms = new TransactionMatchingRules(dbContext).GetTaskInstanceMatching(taskInsId);
            if (parms != null)
            {
                TempData["taskIdMatching"] = parms.taskid;
                TempData["taskNameMatching"] = parms.taskname;

                TempData["accountNameMatching"] = parms.accountName;
                TempData["accountIdMatching"] = parms.accountId;

                TempData["accountNameMatching2"] = parms.accountName2;
                TempData["accountIdMatching2"] = parms.accountId2;
                TempData["transactionMatchingTypeId"] = parms.transactionMatchingTypeId;
                TempData["transactionmatchingtaskId"] = parms.transactionmatchingTaskId;
                TempData["taskInstanceId"] = parms.taskInstanceId;
            }
            return RedirectToAction("TransactionMatchingRecords");
        }
        /// <summary>
        /// calculate match,unmatch,maual after close view & review
        /// </summary>
        /// <param name="TransactionMatchingTaskId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RecalculatedUnmach(int TransactionMatchingTaskId)
        {
            try
            {
                var analyse = new TransactionMatchingRules(dbContext).RecalculatedUnmach(TransactionMatchingTaskId);
                var _return = Json(new
                {
                    percentagesProgressBar = analyse[0],
                    tableReconciled = analyse[1],
                    result = true
                }, JsonRequestBehavior.AllowGet);
                _return.MaxJsonLength = Int32.MaxValue;
                return _return;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new
                {
                    result = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// Show data tranasction unmatch to manual match
        /// </summary>
        /// <param name="TransactionanalysistaskId"></param>
        /// <returns>table html </returns>
        [HttpPost]
        public ActionResult ShowUnmatch(int TransactionanalysistaskId, int matchingtype)
        {
            try
            {
                var unmatch = new TransactionMatchingRules(dbContext).ShowUnmatch(TransactionanalysistaskId, matchingtype);

                var result = Json(new
                {
                    table_creditA = unmatch.table.table_creditA,
                    table_debitB = unmatch.table.table_debitB,
                    table_debitA = unmatch.table.table_debitA,
                    table_creditB = unmatch.table.table_creditB,
                    Sum_CreditsA = unmatch.sum.Sum_CreditsA,
                    Sum_CreditsB = unmatch.sum.Sum_CreditsB,
                    Sum_DebitA = unmatch.sum.Sum_DebitA,
                    Sum_DebitB = unmatch.sum.Sum_DebitB,
                    result = true
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = Int32.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new
                {
                    result = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ReconcileNow(string TransactionRecordList, int transACount, int transBCount, string reconcileType)
        {
            try
            {
                var transactionRules = new TransactionMatchingRules(dbContext);
                var rs = transactionRules.ReconcileNow(TransactionRecordList, transACount, transBCount, reconcileType);
                return Json(new
                {
                    result = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new
                {
                    result = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// Shown INSTANCES ACTIVE MATCHING
        /// </summary>
        /// <param name="taskid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetTaskInstanceMatching(int taskid = 0, string runTask = "")
        {

            MsgModel model = new MsgModel();
            try
            {
                var transactionRules = new TransactionMatchingRules(dbContext);
                model = transactionRules.GetTaskInstanceMatching(taskid, runTask);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FinishRemaining(string accountName, int accountId, string accountName2, int accountId2, int taskid, string taskname, int transactionMatchingTypeId, int transactionmatchingtaskId, int taskInstanceId)
        {
            try
            {
                var transactionRules = new TransactionMatchingRules(dbContext);
                var tInstance = transactionRules.FinishRemaining(accountName, accountId, accountName2, accountId2, taskid, taskname, transactionMatchingTypeId, transactionmatchingtaskId, taskInstanceId);

                if (tInstance.taskid > 0)
                {
                    Session["taskId"] = taskid;
                    Session["taskName"] = taskname;
                    Session["accountName"] = accountName;
                    Session["accountId"] = accountId;
                    Session["accountName2"] = accountName2;
                    Session["accountId2"] = accountId2;
                    Session["transactionMatchingTypeId"] = transactionMatchingTypeId;
                    Session["transactionmatchingtaskId"] = transactionmatchingtaskId;
                    Session["taskInstanceId"] = taskInstanceId;
                }
                return Json(tInstance, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new taskinstance(), JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public void FinishRemaining_Alerts(int taskInstanceId, string taskName, string account1, string account2)
        {
            try
            {
                var transactionRules = new TransactionMatchingRules(dbContext);
                transactionRules.FinishRemaining_Alerts(taskInstanceId, taskName, account1, account2);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
            }
        }
        [HttpPost]
        public ActionResult CalculatingStartDate(int accountIdA = 0, int accountIdB = 0,
            string selectedDate = "", int taskId = 0)
        {
            var model = new MsgModel();
            try
            {
                var transactionRules = new TransactionMatchingRules(dbContext);
                model = transactionRules.CalculatingStartDate(accountIdA, accountIdB, selectedDate, taskId);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Revise(int TransactionMatchingTaskId)
        {
            try
            {
                var transactionRules = new TransactionMatchingRules(dbContext);
                var rs = transactionRules.Revise(TransactionMatchingTaskId);
                return Json(new
                {
                    result = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new { result = false, }, JsonRequestBehavior.AllowGet);
            }
        }

        // this ajax function is called by the client for each draw of the information on the page (i.e. when paging, ordering, searching, etc.). 
        public ActionResult binTableReviewMatches(int draw, int start, int length, string methodName)
        {
            var transactionRules = new TransactionMatchingRules(dbContext);
            var dataTableData = new TransactionMatchingRules.DataTableData
            {
                draw = draw
            };
            int recordsFiltered = 0;
            int recordsTotal = 0;
            dataTableData.data = transactionRules.FilterData((List<TransactionMatchingRules.objectPage>)Session[methodName], ref recordsFiltered, start, length, ref recordsTotal, methodName);
            dataTableData.recordsTotal = recordsTotal;
            dataTableData.recordsFiltered = recordsFiltered;
            var obj = Json(dataTableData, JsonRequestBehavior.AllowGet);
            return obj;
        }
        public ActionResult binTableReviewMatchesChild(int draw, int start, int length, string methodName, int TransactionanalysistaskId, bool matchedAuto, int TransactionMatchingTypeId, string Reversals, string isA, int matchingGroupId)
        {
            var transactionRules = new TransactionMatchingRules(dbContext);
            var dataTableData = new TransactionMatchingRules.DataTableData
            {
                draw = draw
            };
            int recordsFiltered = 0;
            int recordsTotal = 0;
            dataTableData.data1 = transactionRules.FilterData(ref recordsFiltered, start, length, ref recordsTotal, methodName, TransactionanalysistaskId, matchedAuto, TransactionMatchingTypeId, Reversals, isA, matchingGroupId);
            dataTableData.recordsTotal = recordsTotal;
            dataTableData.recordsFiltered = recordsFiltered;
            var obj = Json(dataTableData, JsonRequestBehavior.AllowGet);
            return obj;
        }


        [HttpPost]
        public ActionResult FindMatchRecord(string name, decimal amount, int TransactionMatchingTaskId, int matchingtype, DateTime date)
        {
            try
            {
                var table = new TransactionMatchingRules(dbContext).FindMatchRecord(name, amount, TransactionMatchingTaskId, matchingtype, date);

                var _return = Json(new
                {
                    table = table,
                    result = true
                }, JsonRequestBehavior.AllowGet);
                _return.MaxJsonLength = Int32.MaxValue;
                return _return;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new
                {
                    result = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// un matched records
        /// </summary>
        /// <param name="matchingGroupId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Unmatched(int matchingGroupId)
        {
            try
            {
                var result = new TransactionMatchingRules(dbContext).Unmatched(matchingGroupId, CurrentUser().Id);
                return Json(new
                {
                    result = result,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new
                {
                    result = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="taskId"></param>
        /// <param name="accountId">Account A</param>
        /// <param name="accountId2">Account B</param>
        /// <param name="transactionMatchingTypeId"></param>
        /// <param name="ConfigureRules"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveTransactionMatching(
            string date, int taskId, int accountId, int accountId2, int transactionMatchingTypeId,
            transactionmatchingrule ConfigureRules, int Datevariance, int taskInstanceId)
        {
            try
            {
                var refModel = new TransactionMatchingRules(dbContext).SaveTransactionMatching(date, taskId, accountId,
                    accountId2, transactionMatchingTypeId,
                    ConfigureRules, Datevariance, taskInstanceId, CurrentUser().Id);
                if (!refModel.result)
                    return Json(new { result = false, }, JsonRequestBehavior.AllowGet);
                var analyse = (List<string>)refModel.Object;
                var _result = Json(new
                {
                    percentagesProgressBar = analyse[0],
                    tableReconciled = analyse[1],
                    instanceId = refModel.msgId,
                    transactionMatchingTaskId = refModel.actionVal,
                    result = refModel.result,
                    message = refModel.msg
                }, JsonRequestBehavior.AllowGet);
                _result.MaxJsonLength = Int32.MaxValue;
                return _result;

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new { result = false, }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Review Matches
        /// </summary>
        /// <param name="TransactionanalysistaskId"></param>
        /// <param name="matchedAuto"> true - get matched auto, false - get matched maual</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ReviewMatches(int TransactionanalysistaskId, bool matchedAuto, int TransactionMatchingTypeId)
        {

            try
            {
                var refModel = this.ReviewMatches(TransactionanalysistaskId, matchedAuto, TransactionMatchingTypeId, CurrentUser().Id);

                var result = Json(new
                {
                    tableCount = refModel.msgId,
                    result = true,
                    retDivMatched = refModel.msg
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = Int32.MaxValue;
                return result;

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new
                {
                    result = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private ReturnJsonModel ReviewMatches(int TransactionanalysistaskId, bool matchedAuto, int TransactionMatchingTypeId, string userId)
        {
            var result = new ReturnJsonModel();
            try
            {

                string queryFilter = "select t.Id as transactionId,tr.Id as transactionMatchingRecordId,t.Date,t.Reference,t.Description,t.Debit,t.Credit,t.Balance,t.Reference1,t.DescCol1,t.DescCol2,t.DescCol3, " +
                              "tg.Id as transactionGroupId,tmd.Id as transactionMethodId, tmd.Name as transactionMethodName,tr.IsAccountA " +
                               " from cb_transaction t " +
                               " join cb_transactionmatchingrecord tr on tr.TransactionId = t.Id " +
                               " join cb_transactionmatchingmatched tm on tm.TransactionMatchingRecordId = tr.Id " +
                               " join cb_transactionmatchinggroup tg on tg.Id = tm.TransactionMatchingGroupId " +
                               " join cb_transactionmatchingmethod tmd on tmd.Id = tg.TransactionMatchMethodID " +
                               " where tr.transactionmatchingtaskid = {0} and tg.TransactionMatchMethodID {1}";
                string methodIdwhere = "", srtMatch = "";
                switch (matchedAuto)
                {
                    case true:
                        methodIdwhere = " != " + transactionmatchingmethodId.Manual.ToString();
                        srtMatch = "Auto";
                        break;
                    case false:
                        methodIdwhere = " = " + transactionmatchingmethodId.Manual.ToString();
                        srtMatch = "Manual";
                        break;
                }
                var transactionMatched = dbContext.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, TransactionanalysistaskId, methodIdwhere)).ToList();


                // gen table Matched
                var matchedMethod = transactionMatched.Select(o => o.transactionMethodName).Distinct().ToList();
                int tableId = 0; string colVal = "";
                var retDivMatched = new StringBuilder();
                var retDivUnMatched = new StringBuilder();
                retDivMatched.Append("<div class='col-xs-12'><ul class='app_subnav'>");
                for (int i = 0; i < matchedMethod.Count; i++)
                {
                    if (i == 0)
                        retDivMatched.AppendFormat("<li class='active'><a href='#auto_tab_{0}' data-toggle='tab'>{1}</a></li>", i, matchedMethod[i]);
                    else
                        retDivMatched.AppendFormat("<li><a href='#auto_tab_{0}' data-toggle='tab'>{1}</a></li>", i, matchedMethod[i]);
                }
                retDivMatched.Append("</ul></div>");
                retDivMatched.Append("<div class='col-md-12 tab-content'>");
                var dateformat = (from c in dbContext.dateformats.Where(p => p.CreatedById == userId)
                                  join df in dbContext.uploadformats on c.Id equals df.DateFormatId
                                  select new { c.Format, df.Id }).OrderByDescending(o => o.Id).FirstOrDefault();
                var str = "";
                List<objectPage> liststring;
                var k = 1;
                for (int i = 0; i < matchedMethod.Count; i++)
                {
                    liststring = new List<objectPage>();
                    if (i == 0)
                        retDivMatched.AppendFormat("<div class='tab-pane fade in active' style='width: 100%;' id='auto_tab_{0}'>", i);
                    else
                        retDivMatched.AppendFormat("<div class='tab-pane fade' style='width: 100%;' id='auto_tab_{0}'>", i);
                    retDivMatched.Append("<table class='popclss' style='width: 100%;' id='tbl" + matchedMethod[i].Replace(" ", "_") + "' >");
                    retDivMatched.Append("<thead>");
                    retDivMatched.Append("<tr style='display:none'>");
                    retDivMatched.Append("<th>Test 1</th>");
                    retDivMatched.Append("</tr>");
                    retDivMatched.Append("</thead>");
                    retDivMatched.Append("<tbody>");
                    retDivMatched.Append("<tr><td></td></tr>");
                    //for group
                    foreach (var matchingGroupId in transactionMatched.Where(t => t.transactionMethodName == matchedMethod[i]).Select(g => g.transactionGroupId).Distinct().ToList())
                    {

                        str = string.Format("<tr id='matchedGroup_{0}_" + matchingGroupId + "' class='well grouping'>", srtMatch);
                        str += "<td>";
                        str += "<div class='row'>";
                        if (matchedMethod[i] == "Reversals")
                        {
                            #region recordA
                            // record A
                            var tranmatchTrue1 = transactionMatched.Where(t => t.transactionMethodName == matchedMethod[i] &&
                                  t.transactionGroupId == matchingGroupId && t.IsAccountA == true).ToList();
                            if (tranmatchTrue1.Count > 0)
                            {

                                str += "<div class='col-md-12 col-lg-12' style='margin-bottom: 10px;'>";
                                str += string.Format("<table id='tableMatched{0}_{1}' class='table app_specific'>" +
                                           "<thead><tr>" + "<th>Reference</th><th>Date</th><th>Description</th><th>" +
                                           "{2}" + "</th></tr></thead><tbody>", srtMatch, tableId, "Amount");

                                foreach (var r in tranmatchTrue1)
                                {
                                    if (r.Debit != null)
                                        colVal = "Debit";
                                    else if (r.Credit != null)
                                        colVal = "Credit";
                                    str += "<tr>";
                                    str += string.Format("<td>{0}</td>", r.Reference);
                                    str += string.Format("<td>{0}</td>", Convert.ToDateTime(r.Date).ToString(dateformat != null ? dateformat.Format : "dd/MM/yyyy hh:mm"));
                                    str += string.Format("<td>{0}</td>", r.Description);

                                    switch (colVal)
                                    {
                                        case "Debit":
                                            str += string.Format("<td>{0} {1}</td>", Converter.Obj2Decimal(r.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), "DR");
                                            break;
                                        case "Credit":
                                            str += string.Format("<td>{0} {1}</td>", Converter.Obj2Decimal(r.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), "CR");
                                            break;
                                    }
                                    str += "</tr>";
                                }
                                str += "</tbody></table></div>";
                                k += 1;
                            }


                            #endregion

                            #region recordB
                            var tranmatchFalse1 = transactionMatched.Where(t => t.transactionMethodName == matchedMethod[i] &&
                                  t.transactionGroupId == matchingGroupId && t.IsAccountA == false).ToList();
                            if (tranmatchFalse1.Count > 0)
                            {
                                str += "<div class='col-md-12 col-lg-12' style='margin-bottom: 10px;'>";
                                str += string.Format("<table id='tableMatchedB{0}_{1}' class='table app_specific'>" +
                                 "<thead><tr>" +
                                 "<th>Reference</th><th>Date</th><th>Description</th><th>"
                                 + "{2}" +
                                 "</th></tr></thead><tbody>", srtMatch, tableId, "Amount");

                                foreach (var r in tranmatchFalse1)
                                {
                                    if (r.Debit != null && r.Debit > 0)
                                        colVal = "Debit";
                                    else if (r.Credit != null && r.Credit > 0)
                                        colVal = "Credit";
                                    str += "<tr>";
                                    str += string.Format("<td>{0}</td>", r.Reference);
                                    str += string.Format("<td>{0}</td>", Convert.ToDateTime(r.Date).ToString(dateformat != null ? dateformat.Format : "dd/MM/yyyy hh:mm"));
                                    str += string.Format("<td>{0}</td>", r.Description);

                                    switch (colVal)
                                    {
                                        case "Debit":
                                            str += string.Format("<td>{0} {1}</td>", Converter.Obj2Decimal(r.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), "DR");
                                            break;
                                        case "Credit":
                                            str += string.Format("<td>{0} {1}</td>", Converter.Obj2Decimal(r.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), "CR");
                                            break;
                                    }
                                    str += "</tr>";
                                }
                                str += "</tbody></table></div>";
                                k += 1;
                            }


                            #endregion
                        }
                        else
                        {
                            #region recordA
                            // record A
                            str += "<div class='col-md-6 col-lg-6 aaaaa' style='margin-bottom: 10px;'>";

                            var tranmatchTrue1 = transactionMatched.Where(t => t.transactionMethodName == matchedMethod[i] &&
                                  t.transactionGroupId == matchingGroupId && t.IsAccountA == true).ToList();
                            if (tranmatchTrue1.Count == 0)
                            {
                                str += string.Format("<table id='tableMatched{0}_{1}_{2}' class='table app_specific'>" +
                                "<thead><tr>" + "<th>Reference</th><th>Date</th><th>Description</th><th>" +
                                "{3}" + "</th></tr></thead><tbody>", srtMatch, tableId, k, "Amount");
                                str += "</tbody></table></div>";
                            }
                            else
                            {
                                str += string.Format("<table id='tableMatched{0}_{1}' class='table app_specific'>" +
                                       "<thead><tr>" + "<th>Reference</th><th>Date</th><th>Description</th><th>" +
                                       "{2}" + "</th></tr></thead><tbody>", srtMatch, tableId, "Amount");

                                foreach (var r in tranmatchTrue1)
                                {
                                    if (r.Debit != null)
                                        colVal = "Debit";
                                    else if (r.Credit != null)
                                        colVal = "Credit";
                                    str += "<tr>";
                                    str += string.Format("<td>{0}</td>", r.Reference);
                                    str += string.Format("<td>{0}</td>", Convert.ToDateTime(r.Date).ToString(dateformat != null ? dateformat.Format : "dd/MM/yyyy hh:mm"));
                                    str += string.Format("<td>{0}</td>", r.Description);
                                    switch (colVal)
                                    {
                                        case "Debit":
                                            str += string.Format("<td>{0} {1}</td>", Converter.Obj2Decimal(r.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), "DR");
                                            break;
                                        case "Credit":
                                            str += string.Format("<td>{0} {1}</td>", Converter.Obj2Decimal(r.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), "CR");
                                            break;
                                    }
                                    str += "</tr>";
                                }
                                str += "</tbody></table></div>";
                                k += 1;
                            }
                            #endregion

                            #region recordB
                            str += "<div class='col-md-6 col-lg-6 bbbbb' style='margin-bottom: 10px;'>";
                            var tranmatchFalse1 = transactionMatched.Where(t => t.transactionMethodName == matchedMethod[i] &&
                                  t.transactionGroupId == matchingGroupId && t.IsAccountA == false).ToList();
                            if (tranmatchFalse1.Count == 0)
                            {
                                str += string.Format("<table id='tableMatchedB{0}_{1}' class='table app_specific'>" +
                                    "<thead><tr>" +
                                    "<th>Reference</th><th>Date</th><th>Description</th><th>"
                                    + "{2}" +
                                    "</th></tr></thead><tbody>", srtMatch, tableId, "Amount");
                                str += "</tbody></table></div>";

                            }
                            else
                            {
                                str += string.Format("<table id='tableMatchedB{0}_{1}' class='table app_specific'>" +
                                 "<thead><tr>" +
                                 "<th>Reference</th><th>Date</th><th>Description</th><th>"
                                 + "{2}" +
                                 "</th></tr></thead><tbody>", srtMatch, tableId, "Amount");

                                foreach (var r in tranmatchFalse1)
                                {
                                    if (r.Debit != null)
                                        colVal = "Debit";
                                    else if (r.Credit != null)
                                        colVal = "Credit";
                                    str += "<tr>";
                                    str += string.Format("<td>{0}</td>", r.Reference);
                                    str += string.Format("<td>{0}</td>", Convert.ToDateTime(r.Date).ToString(dateformat != null ? dateformat.Format : "dd/MM/yyyy hh:mm"));
                                    str += string.Format("<td>{0}</td>", r.Description);

                                    switch (colVal)
                                    {
                                        case "Debit":
                                            str += string.Format("<td>{0} {1}</td>", Converter.Obj2Decimal(r.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), "DR");
                                            break;
                                        case "Credit":
                                            str += string.Format("<td>{0} {1}</td>", Converter.Obj2Decimal(r.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat), "CR");
                                            break;
                                    }
                                    str += "</tr>";
                                }
                                str += "</tbody></table></div>";

                            }

                            #endregion
                        }
                        tableId++;
                        str += "</div>";
                        str += string.Format("<button onclick =\"BtnUnMatchRecordsClick({0},'{1}',false)\" class='btn btn-warning btnremove" + matchingGroupId.ToString() + "'><i class='fa fa-remove'></i> Unmatch</button>", matchingGroupId, srtMatch);
                        str += "</td>";
                        str += "</tr><br><br>";
                        liststring.Add(new objectPage { Code = str });
                    }
                    retDivMatched.Append("</tbody>");
                    retDivMatched.Append("</table>");
                    retDivMatched.Append("</div>");
                    var _script = "<script> var $trs1 = $(\"#tbl" + matchedMethod[i].Replace(" ", "_") + "\"); "
                 + " $trs1.dataTable().fnDestroy();"
                 + " $trs1.DataTable({"
                                  + "\"info\": true,"
                                  + "\"bFilter\": false,"
                                  + " \"pagingType\":\"simple_numbers\","
                                  + "\"bLengthChange\": false,"
                                  + "\"bPaginate\": true,"
                                  + "\"bSearchable\": false,"
                                  + "\"sPaginationType\": \"numbers\","
                                  + "\"pageLength\": 50,"
                                  + " \"processing\": true,"
                                  + "\"serverSide\": true,"
                                  + " \"stateSave\": true,"
                                  + "\"ajax\": {\"url\": \"/TransactionMatching/binTableReviewMatches\",\"type\": \"GET\",\"data\":{methodName:'" + matchedMethod[i] + "'}  },"
                                  + "\"columns\": ["
                                  + "       {\"data\": \"Code\" },"
                                  + "   ]"
                                  + " }); </script>";
                    retDivMatched.Append(_script);
                    Session[matchedMethod[i]] = liststring;
                }

                retDivMatched.Append("</div>");

                result.msgId = tableId.ToString();
                result.msg = retDivMatched.ToString();
                result.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                result.result = true;
            }
            return result;
        }

        public ActionResult LoadMoreMedia(int pageSize)
        {
            try
            {
                try
                {
                    var endOfOlder = false;
                    var task = new TasksRules(dbContext).GetTaskById(CurrentTaskId());
                    var medias = task.SubActivities.Where(s => s.ActivityType == QbicleActivity.ActivityTypeEnum.MediaActivity).OrderByDescending(x => x.StartedDate).Skip(pageSize).ToList();
                    if (medias.Count < pageSize)
                    {
                        endOfOlder = true;
                    }
                    ViewBag.EndOfOlder = endOfOlder;
                    if (medias.Count > 0)
                        return PartialView("_ActivityMedias", medias);
                    else
                        return null;
                }
                catch (Exception ex)
                {
                    LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }
            return View("Error");
        }
    }

}