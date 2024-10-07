using Qbicles.BusinessRules.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Entity;
using System.Globalization;
using System.Text;
using CleanBooksData;
using Newtonsoft.Json;
using Qbicles.Models;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Identity;
using static Qbicles.BusinessRules.HelperClass;
using static Qbicles.BusinessRules.Enums;
using CleanBooksCloud.Helper;
using Qbicles.BusinessRules.Helper;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class TransactionMatchingRules
    {

        ApplicationDbContext _db;

        public TransactionMatchingRules(ApplicationDbContext context)
        {
            _db = context;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }


        public taskinstance GetTaskinstance(int Idtask)
        {
            return DbContext.taskinstances.SingleOrDefault(m => m.id == Idtask);
        }
        public transactionmatchingtaskrulesacces Gettransactionmatchingtaskrulesaccess(int taskId)
        {
            return DbContext.transactionmatchingtaskrulesaccess.Where(p => p.TaskId == taskId).OrderByDescending(o => o.Id).FirstOrDefault();
        }
        public IEnumerable<transactionmatchingamountvariancevalue> Gettransactionmatchingamountvariancevalue()
        {
            return DbContext.transactionmatchingamountvariancevalues.ToList(); ;
        }
        public IEnumerable<transactionmatchingdatevariancevalue> Gettransactionmatchingdatevariancevalue()
        {
            return DbContext.transactionmatchingdatevariancevalues.ToList(); ;
        }
        public transactionmatchingrule Gettransactionmatchingrule(int taskId)
        {
            return DbContext.transactionmatchingrules.FirstOrDefault(p => p.TaskId == taskId);
        }
        public bool ReconcileNow(string TransactionRecordList, int transACount, int transBCount, string reconcileType)
        {
            DbContext.Configuration.AutoDetectChangesEnabled = false;
            DbContext.Configuration.ValidateOnSaveEnabled = false;
            List<int> TransactionRecordId = JsonConvert.DeserializeObject<List<int>>(TransactionRecordList);
            int relationShipId = 0;
            if (transACount == transBCount && transACount == 1)
                relationShipId = Enums.transactionmatchingrelationshipId.OneToOne;
            else if (transACount > transBCount && transBCount == 1)
                relationShipId = Enums.transactionmatchingrelationshipId.ManyToOne;
            else if (transACount < transBCount && transACount == 1)
                relationShipId = Enums.transactionmatchingrelationshipId.OneToMany;
            else if (transACount == transBCount && transACount > 1 && transBCount > 1)
                relationShipId = Enums.transactionmatchingrelationshipId.ManyToMany;
            else if ((transACount == 2 && transBCount == 0) || (transACount == 0 && transBCount == 2))
                relationShipId = Enums.transactionmatchingrelationshipId.OneToOne;
            int methodId = 0;
            switch (reconcileType)
            {
                case "manual":
                    methodId = Enums.transactionmatchingmethodId.Manual;
                    break;
                case "reversal":
                    methodId = Enums.transactionmatchingmethodId.Reversals;
                    break;
            }
            transactionmatchinggroup tGroup = new transactionmatchinggroup
            {
                TransactionMatchMethodID = methodId,
                TransactionMatchRelationshipId = relationShipId,
                IsPartialMatch = true
            };
            DbContext.Entry(tGroup).State = EntityState.Added;
            foreach (var id in TransactionRecordId)
            {
                transactionmatchingmatched tmatched = new transactionmatchingmatched
                {
                    TransactionMatchingRecordId = id
                };
                DbContext.Entry(tmatched).State = EntityState.Added;
                tGroup.transactionmatchingmatcheds.Add(tmatched);
            }

            var lst = (from tr in DbContext.transactionmatchingrecords.Where(p => TransactionRecordId.Contains(p.Id))
                       join tm in DbContext.transactionmatchingtasks on tr.TransactionMatchingTaskId equals tm.Id
                       join ti in DbContext.taskinstances on tm.TaskInstanceId equals ti.id
                       select new { ti.TaskId, tr.TransactionId }).ToList();
            var lstUnmatch = DbContext.transactionmatchingunmatcheds.ToList().Where(p => lst.Any(s => s.TaskId == p.TaskId && s.TransactionId == p.TransactionId)).Select(s => new { s.TransactionMatchingUnMatchGroupId }).Distinct().ToList();

            if (lstUnmatch.Any())
            {
                foreach (var umg in lstUnmatch)
                {
                    var unmatch = DbContext.transactionmatchingunmatcheds.Where(p => p.TransactionMatchingUnMatchGroupId == umg.TransactionMatchingUnMatchGroupId);
                    DbContext.transactionmatchingunmatcheds.RemoveRange(unmatch);
                    var unmatchGroup = DbContext.transactionmatchingunmatchgroups.FirstOrDefault(p => p.Id == umg.TransactionMatchingUnMatchGroupId);
                    DbContext.transactionmatchingunmatchgroups.Remove(unmatchGroup);
                }
                DbContext.SaveChanges();
            }

            DbContext.SaveChanges();

            DbContext.Configuration.AutoDetectChangesEnabled = true;
            DbContext.Configuration.ValidateOnSaveEnabled = true;
            return true;
        }

        private string genTableUnmatch(List<transactionsMatchingModel> transactions, string amountChecked, bool isCredit = false)
        {
            StringBuilder table_append = new StringBuilder();
            StringBuilder infoCol = new StringBuilder();
            if (transactions != null && transactions.Count > 0)
            {
                foreach (var item in transactions)
                {
                    infoCol = new StringBuilder();
                    infoCol.AppendFormat("{0}</br>", item.Date);
                    if (!string.IsNullOrEmpty(item.Reference))
                        infoCol.AppendFormat("{0}</br>", item.Reference);
                    if (!string.IsNullOrEmpty(item.Description))
                        infoCol.AppendFormat("{0}</br>", item.Description);

                    if (!string.IsNullOrEmpty(item.Reference1))
                        infoCol.AppendFormat("{0}</br>", item.Reference1);
                    if (!string.IsNullOrEmpty(item.DescCol1))
                        infoCol.AppendFormat("{0}</br>", item.DescCol1);
                    if (!string.IsNullOrEmpty(item.DescCol2))
                        infoCol.AppendFormat("{0}</br>", item.DescCol2);
                    if (!string.IsNullOrEmpty(item.DescCol2))
                        infoCol.AppendFormat("{0}</br>", item.DescCol3);


                    table_append.AppendFormat("<tr id='trRemoveId-{0}'>", item.transactionMatchingRecordId);
                    table_append.AppendFormat("<td class='valignm'>{0}</td>", infoCol.ToString());
                    if (isCredit)
                    {
                        table_append.AppendFormat("<td class='valignm'>{0}</td>", HelperClass.Converter.Obj2Decimal(item.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat));
                        table_append.AppendFormat("<td class='valignm'> <a href='#' class='adjust' data-hover='tooltip' data-placement='top' title='Find similar records for easy matching' onclick=\"FindMatchrecord('{0}',{1},'{2}',this); return false; \" ><i class='fa fa-search'></i></a></td>", amountChecked.Trim(), item.Credit ?? (decimal)0, item.Date);
                    }
                    else
                    {
                        table_append.AppendFormat("<td class='valignm'>{0}</td>", HelperClass.Converter.Obj2Decimal(item.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat));
                        table_append.AppendFormat("<td class='valignm'> <a href='#' class='adjust' data-hover='tooltip' data-placement='top' title='Find similar records for easy matching' onclick=\"FindMatchrecord('{0}',{1},'{2}',this); return false;\" ><i class='fa fa-search'></i></a></td>", amountChecked.Trim(), item.Debit ?? (decimal)0, item.Date);
                    }
                    table_append.AppendFormat("<td class='valignm'><input type='checkbox' onchange = 'CheckMatchs(this)' name='{0}' value='{1}' debitAmount={2} creditAmount={3} /></td>", amountChecked, item.transactionMatchingRecordId, item.Debit ?? 0, item.Credit ?? 0);
                    table_append.Append("</tr>");
                }
            }
            else
            {
                table_append.Append("<tr >");
                table_append.Append("<td class='valignm'>No matching transactions found.</td>");
                table_append.Append("</tr>");
            }
            return table_append.ToString();
        }





        /// <summary>
        /// genProgressAndTable
        /// </summary>
        /// <typeparam name=""></typeparam>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="percentages">total transactionmatchingrecords</param>
        /// <param name="countTransactionsMatched"></param>
        /// <param name="countTransactionsUnMatched"></param>
        /// <param name="countTransactionsManual"></param>
        private static List<string> genProgressAndTable(int percentages, int countTransactionsMatched = 0, int countTransactionsUnMatched = 0, int countTransactionsManual = 0)
        {
            List<string> result = new List<string>();

            decimal percentagesMatch = 0, percentagesUnMatch = 0, percentagesMalually = 0;
            int _percentages = percentages == 0 ? 1 : percentages;
            percentagesMatch = Math.Round((decimal)(countTransactionsMatched * 100.0) / _percentages);
            percentagesUnMatch = Math.Round((decimal)(countTransactionsUnMatched * 100.0) / _percentages);
            percentagesMalually = Math.Round((decimal)(countTransactionsManual * 100.0) / _percentages);
            // gen progress bar percentages
            var retPercentages = new StringBuilder();
            retPercentages.AppendFormat("<div class='progress-bar progress-bar-success' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' data-toggle='tooltip' data-placement='bottom' title='Automatically matched records' style='width: {1}%;'> {2}% </div>",
                percentagesMatch, percentagesMatch, percentagesMatch);
            retPercentages.AppendFormat("<div class='progress-bar progress-bar-warning' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' data-toggle='tooltip' data-placement='bottom' title='Manually matched records' style='width: {1}%;'> {2}% </div>",
                percentagesMalually, percentagesMalually, percentagesMalually);
            retPercentages.AppendFormat("<div class='progress-bar progress-bar-danger' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' data-toggle='tooltip' data-placement='bottom' title='Unmatched records' style='width: {1}%;'> {2}% </div>",
                percentagesUnMatch, percentagesUnMatch, percentagesUnMatch);
            //den table table_Reconciled
            string buttonDisabled = "";
            if (countTransactionsMatched == 0)
                buttonDisabled = "disabled";

            var retTableReconciled = new StringBuilder();
            retTableReconciled.AppendFormat($"<div class='col-xs-12 col-sm-4'><div class='activity-overview task'><h5>Automatically matched</h5>");
            retTableReconciled.AppendFormat($"<p>{countTransactionsMatched} <button {buttonDisabled} onclick='Review_matches_record(true)' class='btn btn-info' data-toggle='modal' data-target='#review_matches_auto'>Review &amp; edit</button></p>");
            retTableReconciled.AppendFormat($"</div></div>");

            buttonDisabled = "";
            if (countTransactionsManual == 0)
                buttonDisabled = "disabled";
            retTableReconciled.AppendFormat($"<div class='col-xs-12 col-sm-4'><div class='activity-overview task'><h5>Manually Matched</h5>");
            retTableReconciled.AppendFormat($"<p>{countTransactionsManual} <button {buttonDisabled} onclick='Review_matches_record(false)' class='btn btn-info' data-toggle='modal' data-target='#review_matches_manual'>Review &amp; edit</button></p>");
            retTableReconciled.AppendFormat($"</div></div>");

            buttonDisabled = "";
            if (countTransactionsUnMatched == 0)
                buttonDisabled = "disabled";
            retTableReconciled.AppendFormat($"<div class='col-xs-12 col-sm-4'><div class='activity-overview alert-detail'><h5>Unmatched</h5>");
            retTableReconciled.AppendFormat($"<p>{countTransactionsUnMatched} <button  onclick='ManualMatchClick()' {buttonDisabled} class='btn btn-warning'>Manually Match</button></p>");
            retTableReconciled.AppendFormat($"</div></div>");



            result.Add(retPercentages.ToString());
            result.Add(retTableReconciled.ToString());
            return result;
        }
        public string genProgressViewReport(int percentages, int countTransactionsMatched = 0, int countTransactionsUnMatched = 0, int countTransactionsManual = 0)
        {

            StringBuilder retPercentages = new StringBuilder();
            decimal percentagesMatch = 0, percentagesUnMatch = 0, percentagesMalually = 0;
            int _percentages = percentages == 0 ? 1 : percentages;
            percentagesMatch = Math.Round((decimal)(countTransactionsMatched * 100.0) / _percentages);
            percentagesUnMatch = Math.Round((decimal)(countTransactionsUnMatched * 100.0) / _percentages);
            percentagesMalually = Math.Round((decimal)(countTransactionsManual * 100.0) / _percentages);
            // gen progress bar percentages
            retPercentages = new StringBuilder();
            retPercentages.AppendFormat("<div class='progress-bar progress-bar-success' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' data-toggle='tooltip' data-placement='bottom'  style='width: {1}%;'> {2}% </div>",
                percentagesMatch, percentagesMatch, percentagesMatch);
            retPercentages.AppendFormat("<div class='progress-bar progress-bar-warning' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' data-toggle='tooltip' data-placement='bottom'  style='width: {1}%;'> {2}% </div>",
                percentagesMalually, percentagesMalually, percentagesMalually);
            retPercentages.AppendFormat("<div class='progress-bar progress-bar-danger' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' data-toggle='tooltip' data-placement='bottom'  style='width: {1}%;'> {2}% </div>",
                percentagesUnMatch, percentagesUnMatch, percentagesUnMatch);
            return retPercentages.ToString();
        }

        /// <summary>
        /// Shown INSTANCES ACTIVE MATCHING
        /// </summary>
        /// <param name="taskid"></param>
        /// <returns></returns>

        public MsgModel GetTaskInstanceMatching(int taskid = 0, string runTask = "")
        {
            MsgModel model = new MsgModel();
            StringBuilder str = new StringBuilder();
            String accountname = "", accountname2 = "", analyse = "";
            var taskinstance = DbContext.taskinstances.Where(m => m.TaskId == taskid)
                                   .Include(m => m.user)
                                   .Include(m => m.transactionmatchingtasks)
                                   .Include(m => m.task).ToList();
            var accountId = DbContext.taskaccounts.Where(a => a.TaskId == taskid && a.Order == 1).FirstOrDefault() == null ? 0 :
                DbContext.taskaccounts.Where(a => a.TaskId == taskid && a.Order == 1).FirstOrDefault().AccountId;
            if (accountId != 0)
            {
                accountname = DbContext.Accounts.SingleOrDefault(m => m.Id == accountId).Name;
            }
            var accountId2 = DbContext.taskaccounts.Where(a => a.TaskId == taskid && a.Order == 2).FirstOrDefault() == null ? 0 :
                DbContext.taskaccounts.Where(a => a.TaskId == taskid && a.Order == 2).FirstOrDefault().AccountId;
            if (accountId2 != 0)
            {
                accountname2 = DbContext.Accounts.SingleOrDefault(m => m.Id == accountId2).Name;
            }
            if (taskinstance.Count() > 0)
            {
                int countTransactionsMatched = 0, countTransactionsUnMatched = 0, countTransactionsManual = 0;
                int transactionMatchingTaskId = 0;
                foreach (var b in taskinstance)
                {
                    str.Append("<tr>");
                    str.AppendFormat("<td>{0}</td>", b.DateExecuted.ToShortDateString() + " " + b.DateExecuted.ToShortTimeString());
                    str.AppendFormat("<td><a href='mailto:{1}'>{0}</a></td>", b.user.UserName, b.user.Email);
                    if (b.IsComplete == 1)
                    {
                        transactionMatchingTaskId = b.transactionmatchingtasks.FirstOrDefault() == null ? 0 :
                       b.transactionmatchingtasks.FirstOrDefault().Id;

                        var lstaccount1UnMatchdb = (from t in DbContext.transactions
                                                    join u in DbContext.uploads.Where(p => p.AccountId == accountId || p.AccountId == accountId2) on t.UploadId equals u.Id
                                                    join tm in DbContext.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionMatchingTaskId && !_db.transactionmatchingmatcheds.Select(s => s.TransactionMatchingRecordId).Contains(p.Id)) on t.Id equals tm.TransactionId
                                                    select new
                                                    {
                                                        Balance = t.Balance,
                                                        Date = t.Date.ToString(),
                                                        Debit = t.Debit,
                                                        Credit = t.Credit,
                                                        Reference = t.Reference,
                                                        Description = t.Description,
                                                        AccountId = u.AccountId
                                                    }).ToList();


                        countTransactionsMatched = _db.transactionmatchingmatcheds.
                        Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == transactionMatchingTaskId
                        && tm.transactionmatchinggroup.TransactionMatchMethodID != Enums.transactionmatchingmethodId.Manual).Count();

                        countTransactionsManual = _db.transactionmatchingmatcheds.
                        Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == transactionMatchingTaskId
                        && tm.transactionmatchinggroup.TransactionMatchMethodID == Enums.transactionmatchingmethodId.Manual).Count();

                        countTransactionsUnMatched = lstaccount1UnMatchdb.Count;
                        int percentages = countTransactionsUnMatched + countTransactionsManual + countTransactionsMatched;// total transactionmatchingrecords
                        analyse = genProgressViewReport(percentages, countTransactionsMatched, countTransactionsUnMatched, countTransactionsManual);

                        str.AppendFormat("<td><i class='fa fa-circle text-success' data-toggle='tooltip' data-placement='top' title=' Completed - report generated'></i> &nbsp; Completed - report generated</td>");
                        str.AppendFormat("<td><button class='btn btn-success' onclick=\"ViewTransactionMatchingReport('{0}', {1}, '{2}', {3}, {4}, '{5}',{6},{7},{8},'{9}','{10}','{11}',{12},{13},{14})\"><i class='fa fa-eye'></i> View Reports</button>", accountname, accountId, accountname2, accountId2, taskid, b.task.Name, b.task.TransactionMatchingTypeId, b.transactionmatchingtasks.FirstOrDefault().Id, b.id,
                           GetFullNameOfUser(b.user), b.DateExecuted.ToShortDateString(), b.DateExecuted.ToShortTimeString(), countTransactionsMatched, countTransactionsManual, countTransactionsUnMatched);
                        str.AppendFormat("<div id='" + b.id + "' style='display:none'>{0}</div>", analyse);
                        str.Append("</td>");
                    }
                    else if (b.IsComplete == 0)
                    {
                        var css = runTask;
                        if (!string.IsNullOrWhiteSpace(runTask))
                            runTask = "disabled";
                        str.Append("<td><i class='fa fa-circle text-warning' data-toggle='tooltip' data-placement='top' title='In progress'></i> In progress</td>");
                        if (b.transactionmatchingtasks.FirstOrDefault() != null)
                            str.AppendFormat($"<td><button {runTask} class='btn btn-info {css}'" +
                                $" onclick=\"ViewExcuteTransaction('{accountname}', {accountId}, '{accountname2}', {accountId2}, {taskid}, '{b.task.Name}',{b.task.TransactionMatchingTypeId},{b.transactionmatchingtasks.FirstOrDefault().Id},{b.id})\">" +
                                $"<i class='fa fa-eye'></i>&nbsp; View progress</button></td>");
                        else
                            str.AppendFormat($"<td><button {runTask} class='btn btn-info {css}' " +
                                $"onclick=\"ViewExcuteTransaction('{accountname}', {accountId}, '{accountname2}', {accountId2}, {taskid}, '{b.task.Name}',{b.task.TransactionMatchingTypeId},{0},{b.id})\">" +
                                $"<i class='fa fa-eye'></i>&nbsp; View progress</button></td>");
                    }

                    str.Append("</tr>");
                }
            }
            model.msg = str.ToString();
            return model;
        }
        public TransactionMatchingReportParameter GetTaskInstanceMatching(int taskInsid)
        {
            try
            {
                TransactionMatchingReportParameter transactionMatching = new TransactionMatchingReportParameter();
                var taskinstance = DbContext.taskinstances.Find(taskInsid);
                if(taskinstance!=null)
                {
                    transactionMatching.taskid = taskinstance.task.Id;
                    transactionMatching.taskname = taskinstance.task.Name;
                    transactionMatching.transactionMatchingTypeId= taskinstance.task.TransactionMatchingTypeId;
                    transactionMatching.transactionmatchingTaskId= taskinstance.transactionmatchingtasks.FirstOrDefault().Id;
                    transactionMatching.taskInstanceId = taskInsid;
                    transactionMatching.userName = GetFullNameOfUser(taskinstance.user);
                    transactionMatching.date = taskinstance.DateExecuted.ToShortDateString();
                    transactionMatching.time = taskinstance.DateExecuted.ToShortTimeString();
                    var tkaccount1 = DbContext.taskaccounts.Where(a => a.TaskId == transactionMatching.taskid && a.Order == 1).FirstOrDefault();
                    transactionMatching.accountId = tkaccount1 == null ? 0 :tkaccount1.AccountId;
                    if (transactionMatching.accountId != 0)
                    {
                        transactionMatching.accountName = DbContext.Accounts.SingleOrDefault(m => m.Id == transactionMatching.accountId).Name;
                    }
                    var tkaccount2 = DbContext.taskaccounts.Where(a => a.TaskId == transactionMatching.taskid && a.Order == 2).FirstOrDefault();
                    transactionMatching.accountId2 = tkaccount2 == null ? 0 :tkaccount2.AccountId;
                    if (transactionMatching.accountId2 != 0)
                    {
                        transactionMatching.accountName2 = DbContext.Accounts.SingleOrDefault(m => m.Id == transactionMatching.accountId2).Name;
                    }
                    transactionMatching.countTransactionsMatched = DbContext.transactionmatchingmatcheds.
                        Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == transactionMatching.transactionmatchingTaskId
                        && tm.transactionmatchinggroup.TransactionMatchMethodID != Enums.transactionmatchingmethodId.Manual).Count();

                    transactionMatching.countTransactionsManual = DbContext.transactionmatchingmatcheds.
                    Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == transactionMatching.transactionmatchingTaskId
                    && tm.transactionmatchinggroup.TransactionMatchMethodID == Enums.transactionmatchingmethodId.Manual).Count();
                    transactionMatching.countTransactionsUnMatched = (from t in DbContext.transactions
                                                join u in DbContext.uploads.Where(p => p.AccountId == transactionMatching.accountId || p.AccountId == transactionMatching.accountId2) on t.UploadId equals u.Id
                                                join tm in DbContext.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionMatching.transactionmatchingTaskId && !_db.transactionmatchingmatcheds.Select(s => s.TransactionMatchingRecordId).Contains(p.Id)) on t.Id equals tm.TransactionId
                                                select t).Count();
                    return transactionMatching;
                }
                return null;
                
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public TransactionMatchingReportParameter FinishRemaining(
            string accountName, int accountId, string accountName2, int accountId2, int taskid,
            string taskname, int transactionMatchingTypeId, int transactionmatchingtaskId, int taskInstanceId)
        {
            try
            {
                var taskInstance = DbContext.taskinstances.Find(taskInstanceId);
                taskInstance.IsComplete = 1;
                var entry = DbContext.Entry(taskInstance);
                if (DbContext.Entry(taskInstance).State == EntityState.Detached)
                    DbContext.taskinstances.Attach(taskInstance);
                entry.Property(m => m.IsComplete).IsModified = true;
                DbContext.SaveChanges();

                int countTransactionsMatched = 0, countTransactionsUnMatched = 0, countTransactionsManual = 0;

                if (transactionmatchingtaskId == 0)
                    transactionmatchingtaskId = taskInstance.transactionmatchingtasks.FirstOrDefault()?.Id ??0;
                

                var lstaccount1UnMatchdb = (from t in DbContext.transactions
                                            join u in DbContext.uploads.Where(p => p.AccountId == accountId || p.AccountId == accountId2) on t.UploadId equals u.Id
                                            join tm in DbContext.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionmatchingtaskId && !_db.transactionmatchingmatcheds.Select(s => s.TransactionMatchingRecordId).Contains(p.Id)) on t.Id equals tm.TransactionId
                                            select new
                                            {
                                                Balance = t.Balance,
                                                Date = t.Date.ToString(),
                                                Debit = t.Debit,
                                                Credit = t.Credit,
                                                Reference = t.Reference,
                                                Description = t.Description,
                                                AccountId = u.AccountId
                                            }).ToList();


                countTransactionsMatched = _db.transactionmatchingmatcheds.
                Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == transactionmatchingtaskId
                && tm.transactionmatchinggroup.TransactionMatchMethodID != Enums.transactionmatchingmethodId.Manual).Count();

                countTransactionsManual = _db.transactionmatchingmatcheds.
                Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == transactionmatchingtaskId
                && tm.transactionmatchinggroup.TransactionMatchMethodID == Enums.transactionmatchingmethodId.Manual).Count();

                countTransactionsUnMatched = lstaccount1UnMatchdb.Count;
                int percentages = countTransactionsUnMatched + countTransactionsManual + countTransactionsMatched;// total transactionmatchingrecords


                var result = new TransactionMatchingReportParameter
                {
                    accountName = accountName,
                    accountId = accountId,
                    accountId2 = accountId2,
                    accountName2 = accountName2,
                    taskid = taskid,
                    taskname = taskInstance.task.Name,
                    date = taskInstance.DateExecuted.ToShortDateString(),
                    userName = GetFullNameOfUser(taskInstance.user),
                    time = taskInstance.DateExecuted.ToShortTimeString(),
                    taskInstanceId = taskInstance.id,
                    transactionmatchingTaskId = transactionmatchingtaskId,
                    transactionMatchingTypeId = transactionMatchingTypeId,
                    countTransactionsManual = countTransactionsManual,
                    countTransactionsMatched = countTransactionsMatched,
                    countTransactionsUnMatched = countTransactionsUnMatched
                };
                return result;
            }
            catch
            {
                return new TransactionMatchingReportParameter { taskid = 0 };
            }

        }
        private string genEmailBody(string userName, string taskName, string account1, string account2, decimal alert, decimal amount, IEnumerable<FinishRemainingAlerts> lst, bool isamount)
        {
            var str = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">";
            str += "   <html xmlns=\"http://www.w3.org/1999/xhtml\">";
            str += "  <head>";
            str += "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />";
            str += " <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />";
            str += " <title>Cleanbooks email</title>";
            str += " <style type=\"text/css\">    ";
            str += "   #outlook a { padding: 0;}";
            str += " body {width: 100% !important;-webkit-text-size-adjust: 100%; -ms-text-size-adjust: 100%; margin: 0;padding: 0; }  ";
            str += ".ExternalClass {width: 100%;}     ";
            str += ".ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div {line-height: 100%; }";
            str += "   #backgroundTable { margin: 0; padding: 0; width: 100% !important;line-height: 100% !important; }";
            str += "img {outline: none; text-decoration: none;border: none; -ms-interpolation-mode: bicubic; }";
            str += "a img { border: none; }";
            str += ".image_fix {display: block; }";
            str += "p {margin: 0px 0px !important; }";
            str += "a {color: #0a8cce; text-decoration: none; text-decoration: none !important; }    ";
            str += "table[class=full] {width: 100%;clear: both; }      ";
            str += "  @@media only screen and (max-width: 640px) { a[href^=\"tel\"], a[href^=\"sms\"] { text-decoration: none;color: #0a8cce; pointer-events: none; cursor: default; }";
            str += "  .mobile_link a[href^=\"tel\"], .mobile_link a[href^=\"sms\"] { text-decoration: default;color: #0a8cce !important; pointer-events: auto; cursor: default; }";
            str += "table[class=devicewidth] {width: 440px !important; text-align: center !important; }";
            str += "table[class=devicewidthinner] {width: 420px !important;text-align: center !important; }";
            str += "img[class=banner] {width: 440px !important;height: 320px !important; }";
            str += "img[class=colimg2] {width: 440px !important; height: 320px !important;}";
            str += " }";
            str += " @@media only screen and (max-width: 480px) { a[href^=\"tel\"], a[href^=\"sms\"] { text-decoration: none;color: #0a8cce;pointer-events: none;cursor: default;}";
            str += ".mobile_link a[href^=\"tel\"], .mobile_link a[href^=\"sms\"] {text-decoration: default;color: #0a8cce !important; pointer-events: auto; cursor: default; }";
            str += "table[class=devicewidth] { width: 280px !important;text-align: center !important; }";
            str += "table[class=devicewidthinner] { width: 260px !important;text-align: center !important; }";
            str += "img[class=banner] {width: 280px !important;height: 200px !important; }";
            str += "img[class=colimg2] {width: 280px !important;height: 140px !important; }";
            str += "td[class=mobile-hide] { display: none !important; }";
            str += "td[class=\"padding-bottom25\"] { padding-bottom: 25px !important; }";
            str += "}";
            str += "</style>";
            str += "</head>";
            str += "  <body>";
            str += " <table width=\"100%\" bgcolor=\"#ffffff\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" id=\"backgroundTable\" st-sortable=\"full-text\">";
            str += "   <tbody>";
            str += "<tr>";
            str += " <td>";
            str += "<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" align=\"center\" class=\"devicewidth\">";
            str += "<tbody>";
            str += "<tr>";
            str += "<td width=\"100%\">";
            str += "<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" align=\"center\" class=\"devicewidth\">";
            str += "<tbody>   ";
            str += "<tr>";
            str += "<td height=\"20\" style=\"font-size:1px; line-height:1px; mso-line-height-rule: exactly;\">&nbsp;</td>";
            str += "</tr>";
            str += "<tr>";
            str += "<td>";
            str += "<table width=\"100%\" align=\"center\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" class=\"devicewidthinner\">";
            str += "<tbody>";
            str += "<tr>";
            str += "<td width=\"100%\" height=\"20\" style=\"font-size:1px; line-height:1px; mso-line-height-rule: exactly;\">&nbsp;</td>";
            str += "</tr>";
            str += "<tr style=\"font-family: Helvetica, arial, sans-serif; font-size: 16px; color: #666666; text-align:left; line-height: 30px;\" st-content=\"fulltext-content\">";
            str += "Dear " + userName + ", <br /><br />";
            str += "</tr>";
            str += "<tr style=\"font-family: Helvetica, arial, sans-serif; font-size: 14px; color: #666666; text-align:left; line-height: 30px;\" st-content=\"fulltext-content\"><td>";
            str += "You have been sent this email as you are an administrator on the CleanBooks system.<br />";
            str += "A Transaction Matching Unmatched " + (isamount ? "Amount" : "Days ") + " Alert has been triggered in response to the finish of the latest Transaction Matching process for the task: " + taskName + " <br />";
            str += "</td></tr>";
            str += "<tr style=\"font-family: Helvetica, arial, sans-serif; font-size: 14px; color: #666666; text-align:left; line-height: 30px;\" st-content=\"fulltext-content\">";
            str += "<td>Task Name: " + taskName + " ";
            str += "For accounts: " + account1 + " and " + account2 + " <br />";
            if (isamount)
            {
                str += "The alert unmatched amount threshold value: " + HelperClass.Converter.Obj2Decimal(alert).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + " <br />";
                str += "The calculated unmatched transactions amount: " + HelperClass.Converter.Obj2Decimal(amount).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + " <br />";
            }
            else
            {
                str += "The alert unmatched days threshold value: " + alert + " <br />";
            }
            str += "The unmatched transaction(s) exceeding the threshold value:";
            str += "</td></tr>";
            str += "<tr>";
            str += "<td>";
            str += "<table style=\"border:1px;padding:1px\" cellpadding='1' cellspacing='1'>";
            str += "<tr>";
            str += "<th>Account Name</th>";
            str += "<th>Date</th>";
            str += "<th>Reference</th>";
            str += "<th>Description</th>";
            str += "<th>Debit</th>";
            str += "<th>Credit</th>";
            str += "<th>Balance</th>";
            str += "</tr>";

            foreach (var item in lst)
            {
                str += "<tr>";
                str += "<td>" + item.AccountName + "</td>";
                str += "<td>" + item.Date + "</td>";
                str += "<td>" + item.Reference + "</td>";
                str += "<td>" + item.Description + "</td>";
                str += "<td>" + HelperClass.Converter.Obj2Decimal(item.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                str += "<td>" + HelperClass.Converter.Obj2Decimal(item.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                str += "<td>" + HelperClass.Converter.Obj2Decimal(item.Balance).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat) + "</td>";
                str += "</tr>";
            }

            str += "</table>";
            str += "</td>";
            str += "</tr>";
            str += "</tbody>";
            str += "</table>";
            str += "</td>";
            str += "</tr>";
            str += "<tr style=\"font-family: Helvetica, arial, sans-serif; font-size: 14px; color: #666666; text-align:left; line-height: 30px;\" st-content=\"fulltext-content\">";
            str += "<td> Regards, <br />   ";
            str += "</td></tr>";
            str += "<tr style=\"font-family: Helvetica, arial, sans-serif; font-size: 14px; color: #666666; text-align:left; line-height: 30px;\" st-content=\"fulltext-content\">";
            str += "<td> Cleanbooks</td>";
            str += "</tr>";
            str += "<tr>";
            str += "<td height=\"20\" style=\"font-size:1px; line-height:1px; mso-line-height-rule: exactly;\">&nbsp;</td>";
            str += "</tr>   ";
            str += "</tbody>";
            str += "</table>";
            str += "</td>";
            str += "</tr>";
            str += "</tbody>";
            str += " </table>";
            str += " </td>";
            str += "  </tr>";
            str += " </tbody>";
            str += "</table>";
            str += "</body>";
            str += "</html>";
            return str;
        }
        
        public void FinishRemaining_Alerts(int taskInstanceId, string taskName, string account1, string account2)
        {
            var _all = (from c in DbContext.transactionmatchingtasks.Where(p => p.TaskInstanceId == taskInstanceId)
                        join i in DbContext.taskinstances.Where(p => p.id == taskInstanceId) on c.TaskInstanceId equals i.id
                        join m in DbContext.transactionmatchingrecords.Where(r => !_db.transactionmatchingmatcheds.Select(s => s.TransactionMatchingRecordId).Contains(r.Id)) on c.Id equals m.TransactionMatchingTaskId
                        join t in DbContext.transactions on m.TransactionId equals t.Id
                        join u in DbContext.uploads on t.UploadId equals u.Id
                        join a in DbContext.Accounts on u.AccountId equals a.Id
                        select new FinishRemainingAlerts
                        {
                            AccountName = a.Name,
                            Date = t.Date,
                            Reference = t.Reference,
                            Description = t.Description,
                            Debit = t.Debit ?? 0,
                            Credit = t.Credit ?? 0,
                            Balance = t.Balance ?? 0,
                            DateExcute = i.DateExecuted
                        }).ToList();
            //Return Sum Debits and Credits

            var _Alert = DbContext.transactionmatchingtaskrules.Where(x => x.IsUnmatchedDays == 1).ToList();
            var sum_Debit = _all.Sum(s => s.Debit);
            var sum_Credit = _all.Sum(s => s.Credit);
            var finish_total = Math.Abs(sum_Debit) + Math.Abs(sum_Credit);

            var lUser = DbContext.QbicleUser.Include(r => r.DomainRoles).Where(p => p.UserName != "SYSTEM").ToList();
            if (lUser == null)
                lUser = new List<ApplicationUser>();
            lUser = lUser.Where(p => p.DomainRoles.Any(r => r.Name == "Administrator")).ToList();
            
            string body = "";
            foreach (var item in _Alert)
            {
                if (finish_total >= item.Amount)
                {
                    foreach (var user in lUser)
                    {
                        body = genEmailBody(user.UserName, taskName, account1, account2, item.Amount, finish_total, _all, true);
                        new EmailHelperRules(DbContext).SendEmail(body, "Transaction Matching Unmatched Amount Alert", user.Email);
                    }
                }
            }
            //Check With Days

            if (_all.Count() > 0)
            {
                var _AlertDay = DbContext.transactionmatchingtaskrules.Where(x => x.IsUnmatchedDays == 0).ToList();
                List<FinishRemainingAlerts> lst = null;
                FinishRemainingAlerts FinishRemainingAlerts;
                foreach (var item in _AlertDay)
                {
                    lst = new List<FinishRemainingAlerts>();
                    foreach (var item1 in _all)
                    {
                        if ((DateTime.UtcNow - item1.DateExcute).TotalDays >= item.Days)
                        {
                            FinishRemainingAlerts = new FinishRemainingAlerts
                            {
                                AccountName = item1.AccountName,
                                Balance = item1.Balance,
                                Credit = item1.Credit,
                                Date = item1.Date,
                                Debit = item1.Debit,
                                Description = item1.Description,
                                Reference = item1.Reference
                            };
                            lst.Add(FinishRemainingAlerts);
                        }
                    }
                    if (lst.Any())
                    {
                        foreach (var user in lUser)
                        {
                            body = genEmailBody(user.UserName, taskName, account1, account2, item.Days, item.Amount, lst, false);
                            new EmailHelperRules(DbContext).SendEmail(body, "Transaction Matching Unmatched Days Alert", user.Email);
                        }
                    }
                }
            }
        }
        public MsgModel CalculatingStartDate(int accountIdA = 0, int accountIdB = 0, string selectedDate = "", int taskId = 0)
        {
            var model = new MsgModel();
            var transactionMatchingTask = DbContext.transactionmatchingtasks.Any(ta => ta.taskinstance.TaskId == taskId);

            var startDateA = new DateTime();
            var startDateB = new DateTime();
            /*
             If the TMTask has not been run previously
             Get the date of first transaction in AccountA and the date of the first transaction in AccountB. 
             Whichever is the older (further back in the past) is the start date that is fixed on the date range picker.
             */
            if (!transactionMatchingTask)
            {
                var task = DbContext.tasks.FirstOrDefault(p => p.Id == taskId);
                if (task != null)
                {
                    startDateA = task.InitialTransactionDate ?? DbContext.transactions.Where(t => t.upload.AccountId == accountIdA).OrderBy(t => t.Date).FirstOrDefault().Date;
                    startDateB = task.InitialTransactionDate ?? DbContext.transactions.Where(t => t.upload.AccountId == accountIdB).OrderBy(t => t.Date).FirstOrDefault().Date;
                }
            }
            /*
             If the TMTask has been run previously
             Get the date of last transaction from AccountA associated with the TMTask and the date of last transaction from AccountB. 
             Whichever is the younger (newest) is the start date that is fixed on the date range picker.
             */
            else
            {
                startDateA = (from t in DbContext.transactions
                              join tr in DbContext.transactionmatchingrecords on t.Id equals tr.TransactionId
                              join tk in DbContext.transactionmatchingtasks on tr.TransactionMatchingTaskId equals tk.Id
                              join ti in DbContext.taskinstances on tk.TaskInstanceId equals ti.id
                              where t.upload.AccountId == accountIdA && ti.TaskId == taskId
                              select t
                    ).OrderByDescending(t => t.Date).FirstOrDefault().Date;

                startDateB = (from t in DbContext.transactions
                              join tr in DbContext.transactionmatchingrecords on t.Id equals tr.TransactionId
                              join tk in DbContext.transactionmatchingtasks on tr.TransactionMatchingTaskId equals tk.Id
                              join ti in DbContext.taskinstances on tk.TaskInstanceId equals ti.id
                              where t.upload.AccountId == accountIdB && ti.TaskId == taskId
                              select t
                   ).OrderByDescending(t => t.Date).FirstOrDefault().Date;
            }

            int result = DateTime.Compare(startDateA, startDateB);
            if (result < 0)
                model.Object = startDateA.ToString("dd/MM/yyyy");
            else if (result == 0)
                model.Object = startDateA.ToString("dd/MM/yyyy");
            else
                model.Object = startDateB.ToString("dd/MM/yyyy");
            return model;
        }
        public bool Revise(int TransactionMatchingTaskId)
        {
            DbContext.Configuration.AutoDetectChangesEnabled = false;
            DbContext.Configuration.ValidateOnSaveEnabled = false;
            var lst = new List<int>();
            var transactionMatchingRecord = DbContext.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == TransactionMatchingTaskId).ToList();
            foreach (var item in transactionMatchingRecord)
            {
                var transactionmatchingcopiedrecord = DbContext.transactionmatchingcopiedrecords.Where(p => p.TransactionMatchingRecordId == item.Id);
                DbContext.transactionmatchingcopiedrecords.RemoveRange(transactionmatchingcopiedrecord);
                var transactionmatchingmatched = DbContext.transactionmatchingmatcheds.Where(p => p.TransactionMatchingRecordId == item.Id).ToList();
                if (transactionmatchingmatched.Count > 0)
                {
                    DbContext.transactionmatchingmatcheds.RemoveRange(transactionmatchingmatched);
                    lst.AddRange(transactionmatchingmatched.Select(o => o.TransactionMatchingGroupId));
                }
            }
            DbContext.transactionmatchingrecords.RemoveRange(transactionMatchingRecord);
            var transactionmatchingtask = DbContext.transactionmatchingtasks.Find(TransactionMatchingTaskId);
            DbContext.transactionmatchingtasks.Remove(transactionmatchingtask);
            var taskinstance = DbContext.taskinstances.FirstOrDefault(p => p.id == transactionmatchingtask.TaskInstanceId);
            DbContext.taskinstances.Remove(taskinstance);
            if (lst.Any())
            {
                var tangroup = DbContext.transactionmatchinggroups.Where(p => lst.Contains(p.Id));
                DbContext.transactionmatchinggroups.RemoveRange(tangroup);
            }
            DbContext.Configuration.AutoDetectChangesEnabled = true;
            DbContext.Configuration.ValidateOnSaveEnabled = true;
            DbContext.SaveChanges();
            return true;
        }
        #region pagging

        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<objectPage> data { get; set; }
            public List<transactionsMatchingModel> data1 { get; set; }
        }
        public class DataTableDataReport
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<AccountTransactionsReport> data { get; set; }
        }
        public class objectPage
        {
            public string Code { get; set; }
        }

        List<objectPage> _data = new List<objectPage>();

        // here we simulate SQL search, sorting and paging operations
        // !!!! DO NOT DO THIS IN REAL APPLICATION !!!!
        /// <summary>
        /// 
        /// </summary>
        /// <param name="recordFiltered"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>        
        /// <returns></returns>
        public List<objectPage> FilterData(List<objectPage> lst, ref int recordFiltered, int start, int length, ref int recordsTotal, string methedname)
        {
            if (lst == null)
                lst = new List<objectPage>();
            recordsTotal = lst.Count;
            if (_data.Count == 0)
            {
                _data = lst;
            }

            recordFiltered = _data.Count;

            // get just one page of data
            if (start > _data.Count)
                start = _data.Count;
            _data = _data.GetRange(start, Math.Min(length, _data.Count - start));
            return _data;
        }
        public List<transactionsMatchingModel> FilterData(ref int recordFiltered, int start, int length, ref int recordsTotal, string methedname, int TransactionanalysistaskId, bool matchedAuto, int TransactionMatchingTypeId, string Reversals, string isA, int matchingGroupId)
        {
            string queryFilter = "select t.Id as transactionId,tr.Id as transactionMatchingRecordId,t.Date,t.Reference,t.Description,t.Debit,t.Credit,t.Balance,t.Reference1,t.DescCol1,t.DescCol2,t.DescCol3, " +
                           "tg.Id as transactionGroupId,tmd.Id as transactionMethodId, tmd.Name as transactionMethodName,tr.IsAccountA " +
                            " from cb_transaction t " +
                            " join cb_transactionmatchingrecord tr on tr.TransactionId = t.Id " +
                            " join cb_transactionmatchingmatched tm on tm.TransactionMatchingRecordId = tr.Id " +
                            " join cb_transactionmatchinggroup tg on tg.Id = tm.TransactionMatchingGroupId " +
                            " join cb_transactionmatchingmethod tmd on tmd.Id = tg.TransactionMatchMethodID " +
                            " where tr.transactionmatchingtaskid = {0} and tg.TransactionMatchMethodID {1}";
            string methodIdwhere = "";
            switch (matchedAuto)
            {
                case true:
                    methodIdwhere = " != " + Enums.transactionmatchingmethodId.Manual.ToString();
                    break;
                case false:
                    methodIdwhere = " = " + Enums.transactionmatchingmethodId.Manual.ToString();
                    break;
            }
            var transactionMatched = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, TransactionanalysistaskId, methodIdwhere)).ToList();

            var lst = transactionMatched.Where(t => t.transactionMethodName == methedname).Select(g => g.transactionGroupId).Distinct().ToList();
            var lresult = new List<transactionsMatchingModel>();
            if (Reversals == "Reversals")
            {
                if (isA == "A")
                {

                    lresult = transactionMatched.Where(t => t.transactionMethodName == methedname &&
                                 t.transactionGroupId == matchingGroupId && t.IsAccountA == true).ToList();
                }
                else
                {
                    lresult = transactionMatched.Where(t => t.transactionMethodName == methedname &&
                                  t.transactionGroupId == matchingGroupId && t.IsAccountA == false).ToList();
                }
            }
            else
            {
                if (isA == "A")
                {

                    lresult = transactionMatched.Where(t => t.transactionMethodName == methedname &&
                                 t.transactionGroupId == matchingGroupId && t.IsAccountA == true).ToList();
                }
                else
                {
                    lresult = transactionMatched.Where(t => t.transactionMethodName == methedname &&
                                 t.transactionGroupId == matchingGroupId && t.IsAccountA == false).ToList();
                }
            }
            recordFiltered = lresult.Count;

            // get just one page of data
            if (start > lresult.Count)
                start = lresult.Count;
            lresult = lresult.GetRange(start, Math.Min(length, lresult.Count - start));
            return lresult;
        }


        #endregion

        public List<AnalysisModel> ManageTransactionMatching(string userid)
        {

            DateTime dateExcuted = DateTime.MinValue;
            var datetoday = DateTime.UtcNow;
            TimeSpan? time = null;
            List<AnalysisModel> tran = new List<AnalysisModel>();
            var tasks = _db.tasks.Where(m => m.AssignedUserId == userid && m.TaskTypeId == Enums.TypeOfTask.TransactionMatching)
                        .Include(m => m.taskexecutioninterval)
                        .Include(m => m.taskaccounts.Select(a => a.account))
                        .Include(m => m.taskinstances)
                        .ToList();
            taskinstance tins = null;
            foreach (var t in tasks)
            {
                dateExcuted = t.taskinstances.OrderByDescending(m => m.DateExecuted).FirstOrDefault() == null ? DateTime.MinValue
                    : t.taskinstances.OrderByDescending(m => m.DateExecuted).FirstOrDefault().DateExecuted;
                var am = new AnalysisModel()
                {
                    TaskId = t.Id,
                    Name = t.Name,
                    TransactionMatchingTypeId = t.TransactionMatchingTypeId
                };
                var firstinstance = t.taskinstances.OrderByDescending(o => o.id).FirstOrDefault();
                am.TaskInstanceId = firstinstance != null ? firstinstance.id : 0;
                am.Interval = t.taskexecutioninterval.Interval;
                am.dateexcuted = dateExcuted == DateTime.MinValue ? "" : dateExcuted.ToShortDateString();
                am.Description = t.Description;
                tins = t.taskinstances.FirstOrDefault(p => p.IsComplete == 0);
                am.IsComplete = tins == null ? 1 : (int)tins.IsComplete;
                var tNowA = false;
                var tPreviousA = false;
                var tNowB = false;
                var tPreviousB = false;


                var _account = t.taskaccounts.Where(a => a.TaskId == t.Id && a.Order == 1).FirstOrDefault();
                if (_account != null)
                {
                    am.AccountId = _account.AccountId;
                    am.AccountName = _account.account.Name;
                }
                var transactionA = _db.transactions.Include(u => u.upload).Where(tr => tr.upload.AccountId == am.AccountId).ToList();

                _account = t.taskaccounts.Where(a => a.TaskId == t.Id && a.Order == 2).FirstOrDefault();
                if (_account != null)
                {
                    am.AccountId2 = _account.AccountId;
                    am.AccountName2 = _account.account.Name;
                }
                var transactionB = _db.transactions.Include(u => u.upload).Where(tr => tr.upload.AccountId == am.AccountId2).ToList();

                switch (t.TaskExecutionIntervalId)
                {
                    case (int)Enums.TaskExecutionInterval.Daily:
                        tNowA = transactionA.Any(tt => tt.Date.Date == datetoday.Date);
                        tPreviousA = transactionA.Any(tt => tt.Date.Date == datetoday.AddDays(-1).Date);
                        tNowB = transactionB.Any(tt => tt.Date.Date == datetoday.Date);
                        tPreviousB = transactionB.Any(tt => tt.Date.Date == datetoday.AddDays(-1).Date);

                        am.Status = HelperClass.setTrafficLightMatching(tNowA, tPreviousA, tNowB, tPreviousB);

                        time = datetoday.Subtract(dateExcuted);
                        if (time.Value.TotalHours > 36)
                            am.Overdue = "Overdue";
                        break;
                    case (int)Enums.TaskExecutionInterval.Weekly:

                        DateTime firstDayOfPreviousWeek = DateTime.UtcNow.GetFirstBusinessWeek(HelperClass.StartBusinessDay());

                        DayOfWeek weekStart = HelperClass.StartBusinessDay(); // or Sunday, or whenever
                        // starting date = start date of this week
                        DateTime startingDate = DateTime.Today;

                        while (startingDate.DayOfWeek != weekStart)
                            startingDate = startingDate.AddDays(-1);

                        DateTime previousWeekStart = startingDate.AddDays(-1).GetFirstBusinessWeek(weekStart).Date;
                        DateTime previousWeekEnd = previousWeekStart.AddDays(7 - HelperClass.weekends().Length - 1).Date;

                        tNowA = transactionA.Any(tt => tt.Date.Date >= previousWeekEnd && tt.Date.Date <= startingDate.Date);
                        tPreviousA = transactionA.Any(tt => tt.Date.Date >= previousWeekStart && tt.Date.Date <= previousWeekEnd);
                        tNowB = transactionB.Any(tt => tt.Date.Date >= previousWeekEnd && tt.Date.Date <= startingDate.Date);
                        tPreviousB = transactionB.Any(tt => tt.Date.Date >= previousWeekStart && tt.Date.Date <= previousWeekEnd);

                        am.Status = HelperClass.setTrafficLightMatching(tNowA, tPreviousA, tNowB, tPreviousB);

                        time = datetoday.Subtract(dateExcuted);
                        if (time.Value.TotalDays > 8)
                            am.Overdue = "Overdue";
                        break;
                    case (int)Enums.TaskExecutionInterval.Monthly:
                        tNowA = transactionA.Any(tt => tt.Date.Month == datetoday.Month);
                        tPreviousA = transactionA.Any(tt => tt.Date.Month == datetoday.AddMonths(-1).Month);
                        tNowB = transactionB.Any(tt => tt.Date.Month == datetoday.Month);
                        tPreviousB = transactionB.Any(tt => tt.Date.Month == datetoday.AddMonths(-1).Month);

                        am.Status = HelperClass.setTrafficLightMatching(tNowA, tPreviousA, tNowB, tPreviousB);

                        time = datetoday.Subtract(dateExcuted);
                        if (time.Value.TotalDays > 31)
                            am.Overdue = "Overdue";
                        break;
                    case (int)Enums.TaskExecutionInterval.Yearly:
                        tNowA = transactionA.Any(tt => tt.Date.Year == datetoday.Year);
                        tPreviousA = transactionA.Any(tt => tt.Date.Year == datetoday.AddYears(-1).Year);
                        tNowB = transactionB.Any(tt => tt.Date.Year == datetoday.Year);
                        tPreviousB = transactionB.Any(tt => tt.Date.Year == datetoday.AddYears(-1).Year);

                        am.Status = HelperClass.setTrafficLightMatching(tNowA, tPreviousA, tNowB, tPreviousB);

                        time = datetoday.Subtract(dateExcuted);
                        if (time.Value.TotalDays > 366)
                            am.Overdue = "Overdue";
                        break;
                }
                am.InstanceActive = t.taskinstances.Count();
                tran.Add(am);
            }

            return tran;
        }

        public List<string> RecalculatedUnmach(int TransactionMatchingTaskId)
        {
            int countTransactionsMatched = 0, countTransactionsManual = 0;

            string queryFilter = "SELECT tr.id FROM cb_transactionmatchingrecord tr WHERE tr.TransactionMatchingTaskId={0}";

            var percentages = DbContext.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, TransactionMatchingTaskId)).ToList().Count;

            countTransactionsMatched = DbContext.transactionmatchingmatcheds.
                Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == TransactionMatchingTaskId
                && tm.transactionmatchinggroup.TransactionMatchMethodID != transactionmatchingmethodId.Manual).Count();

            countTransactionsManual = DbContext.transactionmatchingmatcheds.
                Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == TransactionMatchingTaskId
                && tm.transactionmatchinggroup.TransactionMatchMethodID == transactionmatchingmethodId.Manual).Count();

            int countTransactionsUnMatched = percentages - countTransactionsMatched - countTransactionsManual;

            return genProgressAndTable(percentages, countTransactionsMatched, countTransactionsUnMatched, countTransactionsManual);
        }

        public MatchingUnmatch ShowUnmatch(int TransactionanalysistaskId, int matchingtype)
        {
            var unmatch = new MatchingUnmatch();
            string queryFilter = "SELECT t.Id as transactionId,tr.Id as transactionMatchingRecordId,t.Date,t.Reference,t.Description,t.Debit, " +
                               "t.Credit,t.Balance,t.Reference1,t.DescCol1,t.DescCol2,t.DescCol3,tr.IsAccountA  " +
                                " FROM cb_transaction t " +
                                " JOIN cb_transactionmatchingrecord tr on tr.TransactionId = t.Id " +
                                " WHERE tr.transactionmatchingtaskid = {0} " +
                                " AND tr.id NOT IN(SELECT transactionmatchingrecordId FROM cb_transactionmatchingmatched)";

            var transactionUnMatched = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, TransactionanalysistaskId)).ToList();

            //Get dataList and Sum CreditsA
            var CreditsA = transactionUnMatched.Where(c => c.Credit != null && c.IsAccountA == true).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
            unmatch.sum.Sum_CreditsA = Converter.Obj2Decimal(transactionUnMatched.Where(c => c.Credit != null && c.IsAccountA == true).Sum(x => x.Credit)).ToString("#,##0.##", CultureInfo.InvariantCulture.NumberFormat);
            //Get dataList and Sum DebitA
            var DebitA = transactionUnMatched.Where(c => c.Debit != null && c.IsAccountA == true).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
            unmatch.sum.Sum_DebitA = Converter.Obj2Decimal(transactionUnMatched.Where(c => c.Debit != null && c.IsAccountA == true).Sum(x => x.Debit)).ToString("#,##0.##", CultureInfo.InvariantCulture.NumberFormat);
            //Get dataList and Sum CreditsB

            var CreditsB = transactionUnMatched.Where(c => c.Credit != null && c.IsAccountA == false).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
            unmatch.sum.Sum_CreditsB = Converter.Obj2Decimal(transactionUnMatched.Where(c => c.Credit != null && c.IsAccountA == false).Sum(x => x.Credit)).ToString("#,##0.##", CultureInfo.InvariantCulture.NumberFormat);
            //Get dataList and Sum DebitB
            var DebitB = transactionUnMatched.Where(c => c.Debit != null && c.IsAccountA == false).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
            unmatch.sum.Sum_DebitB = Converter.Obj2Decimal(transactionUnMatched.Where(c => c.Debit != null && c.IsAccountA == false).Sum(x => x.Debit)).ToString("#,##0.##", CultureInfo.InvariantCulture.NumberFormat);
            if (matchingtype == (int)tranMatchingType.DebitToDebit)
            {
                unmatch.table.table_creditA = genTableUnmatch(CreditsA, "creditsA", true);
                unmatch.table.table_debitB = genTableUnmatch(CreditsB, "creditsB", true);
                unmatch.table.table_debitA = genTableUnmatch(DebitA, "debitA", false);
                unmatch.table.table_creditB = genTableUnmatch(DebitB, "debitB", false);
            }
            else
            {
                unmatch.table.table_creditA = genTableUnmatch(CreditsA, "creditsA", true);
                unmatch.table.table_debitB = genTableUnmatch(DebitB, "debitB", false);
                unmatch.table.table_debitA = genTableUnmatch(DebitA, "debitA", false);
                unmatch.table.table_creditB = genTableUnmatch(CreditsB, "creditsB", true);
            }
            return unmatch;
        }

        public string FindMatchRecord(string name, decimal amount, int TransactionMatchingTaskId, int matchingtype, DateTime date)
        {
            string table;
            string queryFilter = "SELECT t.Id as transactionId,tr.Id as transactionMatchingRecordId,t.Date,t.Reference,t.Description,t.Debit, " +
                               "t.Credit,t.Balance,t.Reference1,t.DescCol1,t.DescCol2,t.DescCol3,tr.IsAccountA  " +
                                " FROM cb_transaction t " +
                                " JOIN cb_transactionmatchingrecord tr on tr.TransactionId = t.Id " +
                                " WHERE tr.transactionmatchingtaskid = {0} " +
                                " AND tr.id NOT IN(SELECT transactionmatchingrecordId FROM cb_transactionmatchingmatched)";
            var transactionUnMatched = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, TransactionMatchingTaskId)).ToList();
            if (matchingtype == (int)tranMatchingType.DebitToDebit)
            {
                if (name == "creditsA")
                {
                    var CreditsB = transactionUnMatched.Where(c => c.Credit != null && c.IsAccountA == false && c.Credit == amount).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
                    if (CreditsB.Any())
                    {
                        CreditsB.All(c => { c.DayNumber = System.Math.Abs((decimal)(c.Date - date).TotalDays); return true; });
                        CreditsB = CreditsB.OrderBy(o => o.DayNumber).ThenBy(t => t.Date).ToList();
                    }
                    table = genTableUnmatch(CreditsB, "creditsB", true);
                }
                else if (name == "debitB")
                {
                    var DebitA = transactionUnMatched.Where(c => c.Debit != null && c.IsAccountA == true && c.Debit == amount).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
                    if (DebitA.Any())
                    {
                        DebitA.All(c => { c.DayNumber = System.Math.Abs((decimal)(c.Date - date).TotalDays); return true; });
                        DebitA = DebitA.OrderBy(o => o.DayNumber).ThenBy(t => t.Date).ToList();
                    }
                    table = genTableUnmatch(DebitA, "debitA", false);
                }
                else if (name == "debitA")
                {
                    var DebitB = transactionUnMatched.Where(c => c.Debit != null && c.IsAccountA == false && c.Debit == amount).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
                    if (DebitB.Any())
                    {
                        DebitB.All(c => { c.DayNumber = System.Math.Abs((decimal)(c.Date - date).TotalDays); return true; });
                        DebitB = DebitB.OrderBy(o => o.DayNumber).ThenBy(t => t.Date).ToList();
                    }
                    table = genTableUnmatch(DebitB, "debitB", false);
                }
                else
                {
                    var CreditsA = transactionUnMatched.Where(c => c.Credit != null && c.IsAccountA == true && c.Credit == amount).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
                    if (CreditsA.Any())
                    {
                        CreditsA.All(c => { c.DayNumber = System.Math.Abs((decimal)(c.Date - date).TotalDays); return true; });
                        CreditsA = CreditsA.OrderBy(o => o.DayNumber).ThenBy(t => t.Date).ToList();
                    }
                    table = genTableUnmatch(CreditsA, "creditsA", true);
                }
            }
            else
            {
                if (name == "creditsA")
                {
                    var DebitB = transactionUnMatched.Where(c => c.Debit != null && c.IsAccountA == false && c.Debit == amount).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
                    if (DebitB.Any())
                    {
                        DebitB.All(c => { c.DayNumber = System.Math.Abs((decimal)(c.Date - date).TotalDays); return true; });
                        DebitB = DebitB.OrderBy(o => o.DayNumber).ThenBy(t => t.Date).ToList();
                    }
                    table = genTableUnmatch(DebitB, "debitB", false);
                }
                else if (name == "debitB")
                {
                    var CreditsA = transactionUnMatched.Where(c => c.Credit != null && c.IsAccountA == true && c.Credit == amount).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
                    if (CreditsA.Any())
                    {
                        CreditsA.All(c => { c.DayNumber = System.Math.Abs((decimal)(c.Date - date).TotalDays); return true; });
                        CreditsA = CreditsA.OrderBy(o => o.DayNumber).ThenBy(t => t.Date).ToList();
                    }
                    table = genTableUnmatch(CreditsA, "creditsA", true);
                }
                else if (name == "debitA")
                {
                    var CreditsB = transactionUnMatched.Where(c => c.Credit != null && c.IsAccountA == false && c.Credit == amount).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
                    if (CreditsB.Any())
                    {
                        CreditsB.All(c => { c.DayNumber = System.Math.Abs((decimal)(c.Date - date).TotalDays); return true; });
                        CreditsB = CreditsB.OrderBy(o => o.DayNumber).ThenBy(t => t.Date).ToList();
                    }
                    table = genTableUnmatch(CreditsB, "creditsB", true);
                }
                else
                {
                    var DebitA = transactionUnMatched.Where(c => c.Debit != null && c.IsAccountA == true && c.Debit == amount).OrderByDescending(o => o.Date).ThenBy(t => t.Reference).ToList();
                    if (DebitA.Any())
                    {
                        DebitA.All(c => { c.DayNumber = System.Math.Abs((decimal)(c.Date - date).TotalDays); return true; });
                        DebitA = DebitA.OrderBy(o => o.DayNumber).ThenBy(t => t.Date).ToList();
                    }
                    table = genTableUnmatch(DebitA, "debitA", false);
                }
            }
            return table;
        }

        public bool Unmatched(int matchingGroupId, string userId)
        {
            try
            {
                _db.Configuration.AutoDetectChangesEnabled = false;
                _db.Configuration.ValidateOnSaveEnabled = false;
                if (matchingGroupId != 0)
                {
                    var lst = from tm in _db.transactionmatchingmatcheds.Where(p => p.TransactionMatchingGroupId == matchingGroupId)
                              join tr in _db.transactionmatchingrecords on tm.TransactionMatchingRecordId equals tr.Id
                              join tt in _db.transactionmatchingtasks on tr.TransactionMatchingTaskId equals tt.Id
                              join ti in _db.taskinstances on tt.TaskInstanceId equals ti.id
                              select new { tr.TransactionId, ti.TaskId };
                    var unmatchGoup = _db.transactionmatchingunmatchgroups.Add(new transactionmatchingunmatchgroup());
                    _db.SaveChanges();
                    var lstUnmatch = new List<transactionmatchingunmatched>();
                    transactionmatchingunmatched unmatch;
                    foreach (var item in lst)
                    {
                        unmatch = new transactionmatchingunmatched
                        {
                            TaskId = item.TaskId,
                            TransactionId = item.TransactionId,
                            TransactionMatchingUnMatchGroupId = unmatchGoup.Id
                        };
                        lstUnmatch.Add(unmatch);
                    }
                    if (lstUnmatch.Any())
                    {
                        _db.transactionmatchingunmatcheds.AddRange(lstUnmatch);
                    }

                    var tranGroup = _db.transactionmatchinggroups.Where(g => g.Id == matchingGroupId).Include(r => r.transactionmatchingmatcheds).FirstOrDefault();
                    _db.transactionmatchingmatcheds.RemoveRange(tranGroup.transactionmatchingmatcheds);
                    _db.transactionmatchinggroups.Remove(tranGroup);

                    //log Unmatched transaction matching
                    var au = new audit_transaction_matching
                    {
                        changetime = DateTime.UtcNow,
                        changetype = changeType.UnMatch,
                        Note = "Unmatched transaction matching",
                        User_id = userId,
                        matchingGroupId = matchingGroupId
                    };
                    new AuditRules(DbContext).audit_transaction_matching(au);
                    _db.SaveChanges();
                }

                return true;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return false;
            }
            finally
            {
                _db.Configuration.AutoDetectChangesEnabled = true;
                _db.Configuration.ValidateOnSaveEnabled = true;
            }
        }

        public ReturnJsonModel SaveTransactionMatching(
           string date, int taskId, int accountId, int accountId2, int transactionMatchingTypeId,
           transactionmatchingrule ConfigureRules, int Datevariance, int taskInstanceId, string userId)
        {
            try
            {
                var message = "";
                int instanceId = 0, transactionMatchingTaskId = 0;
                int countTransactionsMatched = 0, countTransactionsUnMatched = 0, countTransactionsManual = 0;
                var end = date.Replace("-", "/");
                var start = this.CalculatingStartDate(accountId, accountId2, date, taskId);
                DateTime startDate =
                    DateTime.ParseExact(start.Object.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(-1)
                    , endate = DateTime.ParseExact(end, "dd/MM/yyyy", CultureInfo.InvariantCulture).AddDays(1);

                //get transaction and unmated coppy
                transactionParameter.AccountId = accountId;
                transactionParameter.AccountId2 = accountId2;
                transactionParameter.StartDate = startDate;
                transactionParameter.EndDate = endate;
                //For AccountA,
                //The application finds the ID of the last transaction (LastTransactionID_A) that has been processed in the task so far.
                //The application then selects the transactions from AccountA 
                //where the transaction.ID > LastTransactionID_A AND the transaction.Date < the selected date +1 Day
                var transA = new List<transaction>();
                var LastTransactionID_A = _db.transactionmatchingrecords.Where(t => t.transaction.upload.AccountId == accountId
                   && t.transactionmatchingtask.taskinstance.TaskId == taskId)
                   .OrderByDescending(t => t.TransactionId).FirstOrDefault();

                if (LastTransactionID_A != null)
                    transA = (from tr in _db.transactions
                              where tr.upload.AccountId == accountId //The application then selects the transactions from AccountA
                              && tr.Date > startDate && tr.Date < endate //AND the transaction.Date < the selected date + 1 Day
                              && tr.Id > LastTransactionID_A.TransactionId // where the transaction.ID > LastTransactionID_A
                              select tr).ToList();
                else
                    transA = (from tr in _db.transactions
                              where tr.upload.AccountId == accountId && tr.Date > startDate && tr.Date < endate
                              select tr).ToList();

                //For AccountB,
                //The application finds the ID of the last transaction(LastTransactionID_B) that has been processed in the task so far.
                //The application then selects the transactions from AccountB 
                //where the transaction.ID > LastTransactionID_B
                //AND the transaction.Date < the selected date + 1 Day 
                var transB = new List<transaction>();
                var LastTransactionID_B = _db.transactionmatchingrecords.Where(t => t.transaction.upload.AccountId == accountId2
                   && t.transactionmatchingtask.taskinstance.TaskId == taskId)
                   .OrderByDescending(t => t.TransactionId).FirstOrDefault();

                if (LastTransactionID_B != null)
                    transB = (from tr in _db.transactions
                              where tr.upload.AccountId == accountId2 //The application then selects the transactions from AccountB
                              && tr.Date > startDate && tr.Date < endate //AND the transaction.Date < the selected date + 1 Day
                              && tr.Id > LastTransactionID_B.TransactionId // where the transaction.ID > LastTransactionID_B
                              select tr).ToList();
                else
                    transB = (from tr in _db.transactions
                              where tr.upload.AccountId == accountId2 && tr.Date > startDate && tr.Date < endate
                              select tr).ToList();


                // If either of the transaction data sets are found to be empty the user is informed that the TMTask cannot continue.
                if (transA.Count == 0 || transB.Count == 0)
                {
                    return new ReturnJsonModel { result = false };
                }

                string queryFilter = "SELECT t.Id as transactionId,tr.Id as transactionMatchingRecordId,t.Date,t.Reference,t.Description,t.Debit, " +
                                                      "t.Credit,t.Balance,t.Reference1,t.DescCol1,t.DescCol2,t.DescCol3,tr.IsAccountA " +
                                                       " FROM cb_transaction t  join cb_transactionmatchingrecord tr ON tr.TransactionId = t.Id " +
                                                       " JOIN cb_transactionmatchingtask tk ON tk.Id = tr.TransactionMatchingTaskId " +
                                                       " JOIN cb_taskinstance ti ON ti.id = tk.TaskInstanceId " +
                                                       " WHERE ti.TaskId = {0} " +
                                                       " AND tr.id NOT in(SELECT transactionmatchingrecordId FROM cb_transactionmatchingmatched)" +
                                                       " AND tr.id NOT IN(SELECT CopiedFromId FROM  cb_transactionmatchingcopiedrecord) AND tr.TransactionId not in (SELECT TransactionId FROM  cb_transactionmatchingunmatched where TaskId={0});";

                var transactionUnMatched = _db.Database.SqlQuery<transactionsMatchingModel>
                    (string.Format(queryFilter, taskId)).ToList();

                _db.Configuration.AutoDetectChangesEnabled = false;
                _db.Configuration.ValidateOnSaveEnabled = false;
                //taskinstance
                var instance = new taskinstance();
                if (taskInstanceId <= 0)
                {
                    instance.ExecutedById = userId;
                    instance.DateExecuted = DateTime.UtcNow;
                    instance.StartDate = startDate.AddDays(1);
                    instance.EndDate = endate.AddDays(-1);
                    instance.IsComplete = 0;
                    instance.TaskInstanceDateRangeId = null;
                    instance.TaskId = taskId;
                    // transactionmatchingtask
                    instance.transactionmatchingtasks.Add(new transactionmatchingtask());
                    _db.taskinstances.Add(instance);
                    _db.Entry(instance).State = EntityState.Added;

                    //Add Matchingrules
                    if (ConfigureRules.Id > 0)
                    {
                        if (_db.Entry(ConfigureRules).State == EntityState.Detached)
                            _db.transactionmatchingrules.Attach(ConfigureRules);
                        _db.Entry(ConfigureRules).State = EntityState.Modified;
                    }
                    else
                    {
                        _db.transactionmatchingrules.Add(ConfigureRules);
                        _db.Entry(ConfigureRules).State = EntityState.Added;
                    }
                    _db.SaveChanges();
                    taskInstanceId = instance.id;

                }
                else
                    instance = _db.taskinstances.FirstOrDefault(p => p.id == taskInstanceId);


                // get id inserted 
                instanceId = taskInstanceId;
                var transactionMatching = _db.transactionmatchingtasks.FirstOrDefault(p => p.TaskInstanceId == instanceId);
                transactionMatchingTaskId = transactionMatching == null ? 0 :
                    transactionMatching.Id;
                //log edit transaction matching
                var au = new audit_transaction_matching
                {
                    changetime = DateTime.UtcNow,
                    changetype = changeType.TaskRun,
                    Note = "edit transaction matching",
                    User_id = userId,
                    EndDate = endate,
                    StartDate = startDate,
                    accountId = accountId,
                    accountId2 = accountId2,
                    taskId = taskId,
                    transactionMatchingTypeId = transactionMatchingTypeId
                };
                new AuditRules(DbContext).audit_transaction_matching(au);

                // Bring Forward Unmatched Transactions

                var tranRecords = new List<transactionmatchingrecord>();
                var tranCopyRecords = new List<transactionmatchingcopiedrecordsModel>();
                var tranactionCopyRecords = new List<transactionmatchingcopiedrecord>();

                if (transactionUnMatched.Count > 0)
                {
                    var lMatchingRecord = _db.transactionmatchingrecords.Where(t => t.TransactionMatchingTaskId == transactionMatchingTaskId).ToList();
                    foreach (var item in transactionUnMatched)
                    {
                        if (!lMatchingRecord.Any(c => c.TransactionId == item.transactionId))
                        {
                            tranRecords.Add(new transactionmatchingrecord
                            {
                                TransactionId = item.transactionId,
                                IsAccountA = item.IsAccountA,
                                TransactionMatchingTaskId = transactionMatchingTaskId
                            });

                            tranCopyRecords.Add(new transactionmatchingcopiedrecordsModel
                            {
                                CopiedFromId = item.transactionMatchingRecordId,
                                TransactionId = item.transactionId
                            });
                        }

                    }
                    _db.transactionmatchingrecords.AddRange(tranRecords);
                    _db.SaveChanges();
                    // transactionmatchingcopiedrecord
                    foreach (var item in tranCopyRecords)
                    {
                        foreach (var item2 in tranRecords)
                        {
                            if (item2.TransactionId == item.TransactionId)
                            {
                                tranactionCopyRecords.Add(new transactionmatchingcopiedrecord
                                {
                                    CopiedFromId = item.CopiedFromId,
                                    TransactionMatchingRecordId = item2.Id
                                });
                                tranRecords.Remove(item2);
                                break;
                            }
                        }
                    }

                    _db.transactionmatchingcopiedrecords.AddRange(tranactionCopyRecords);
                    _db.SaveChanges();

                }

                //transactionmatchingrecord 
                foreach (var t in transA)
                {
                    var tk = new transactionmatchingrecord()
                    {
                        TransactionMatchingTaskId = transactionMatchingTaskId,
                        TransactionId = t.Id,
                        IsAccountA = true
                    };
                    _db.Entry(tk).State = EntityState.Added;
                    _db.transactionmatchingrecords.Add(tk);
                }
                foreach (var t2 in transB)
                {
                    var tk2 = new transactionmatchingrecord()
                    {
                        TransactionMatchingTaskId = transactionMatchingTaskId,
                        TransactionId = t2.Id,
                        IsAccountA = false
                    };
                    _db.Entry(tk2).State = EntityState.Added;
                    _db.transactionmatchingrecords.Add(tk2);
                }
                _db.SaveChanges();

                _db.Configuration.AutoDetectChangesEnabled = true;
                _db.Configuration.ValidateOnSaveEnabled = true;


                this.TransactionMatchingProcess(transactionMatchingTaskId, transactionMatchingTypeId, ConfigureRules, Datevariance, taskId);
                //return result

                int percentages = transA.Count + transB.Count + tranactionCopyRecords.Count;// total transactionmatchingrecords

                countTransactionsMatched = _db.transactionmatchingmatcheds.
                    Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == transactionMatchingTaskId
                    && tm.transactionmatchinggroup.TransactionMatchMethodID != transactionmatchingmethodId.Manual).Count();

                countTransactionsManual = _db.transactionmatchingmatcheds.
                    Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == transactionMatchingTaskId
                    && tm.transactionmatchinggroup.TransactionMatchMethodID == transactionmatchingmethodId.Manual).Count();
                var A1 = DbContext.Accounts.Find(accountId);
                var A2 = DbContext.Accounts.Find(accountId2);
                countTransactionsUnMatched = percentages - countTransactionsMatched - countTransactionsManual;
                message = $"Transaction Matching transactions identified and included in Transaction Matching process.<br>" +
                    $"{transA.Count()} transactions from {A1.Name} were included.</br>" +
                    $"{transB.Count()} transactions from {A2.Name} were included.</br>" +
                    $"In total {(transA.Count() + transB.Count() + tranactionCopyRecords.Count())} transactions were included in Transaction Matching process.</br>" +
                    $"{countTransactionsUnMatched} previously un-matched transactions were included."
                    ;
                var analyse = genProgressAndTable(percentages, countTransactionsMatched, countTransactionsUnMatched, countTransactionsManual);
                var refModel = new ReturnJsonModel
                {
                    result = true,
                    Object = analyse,
                    msgId = instanceId.ToString(),
                    actionVal = transactionMatchingTaskId,
                    msg = message
                };

                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new ReturnJsonModel { result = false };
            }
            finally
            {
                _db.Configuration.AutoDetectChangesEnabled = true;
                _db.Configuration.ValidateOnSaveEnabled = true;
            }
        }
        public bool TransactionMatchingProcess(int transactionmatchingtaskId, int transactionMatchingTypeId, transactionmatchingrule ConfigureRules, int Datevariance, int taskId)
        {
            try
            {
                _db.Configuration.AutoDetectChangesEnabled = false;
                _db.Configuration.ValidateOnSaveEnabled = false;


                string queryFilter = "SELECT  t.Id as transactionId, tmr.Id as transactionMatchingRecordId, t.Date as Date," +
                    "t.Reference as Reference,t.Debit as Debit ,t.Credit as Credit ,t.Balance as Balance , t.Description as Description ," +
                    "t.Reference1 as Reference1,t.DescCol1 as DescCol1,t.DescCol2 as DescCol2 ,t.DescCol3 as DescCol3 " +
                    "FROM cb_transaction as t join cb_transactionmatchingrecord as tmr on t.Id = tmr.TransactionId "
                    + " where tmr.id not in(select TransactionMatchingRecordId from cb_transactionmatchingmatched) && tmr.TransactionMatchingTaskId = {0}"
                    + " && tmr.IsAccountA = {1} && tmr.TransactionId not in (SELECT TransactionId FROM  cb_transactionmatchingunmatched where TaskId={2})";

                /*
                All transactions in transactionmatchingrecord for this task instance for IsAccountA = 1
               and no attached records in transactionmatchingmatched.
                */
                List<transactionsMatchingModel> TransactionsA;
                /*
                 All transactions in transactionmatchingrecord for this task instance for IsAccountA = 0
                and no attached records in transactionmatchingmatched.
                 */
                List<transactionsMatchingModel> TransactionsB;
                #region 1. Many to Many 
                if (ConfigureRules.IsManyToMany_Set == 1)
                {
                    this.TransactionMatchingByFieldName(transactionmatchingtaskId, transactionMatchingTypeId);
                }

                #endregion 

                #region 2. Reference and Date

                /*  Check if the amounts match: Amounts Match
                    Check if the dates match: IsDateMatchRequired = true
                    Check if the references match: Reference*/
                if (ConfigureRules.IsReferenceAndDate_Set == 1)
                {
                    TransactionsA = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, true, taskId)).ToList();
                    TransactionsB = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, false, taskId)).ToList();
                    this.Matchingprocess(TransactionsA, TransactionsB, transactionMatchingTypeId, true, TransactionMatchingBy.ReferenceAndDate, Datevariance, ConfigureRules);
                }
                #endregion

                #region 3.Reference to Reference1 and Date (Note: this match can only be used if the 'Reference and Date' match has already been carried out)
                //Check if the amounts match: Amounts Match
                //Check if the dates match: IsDateMatchRequired = true
                //Check if the reference and reference1 values match: Reference to Reference1
                if (ConfigureRules.IsRefAndDate_RefToRef1_Set == 1)
                {
                    TransactionsA = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, true, taskId)).ToList();
                    TransactionsB = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, false, taskId)).ToList();
                    this.Matchingprocess(TransactionsA, TransactionsB, transactionMatchingTypeId, true, TransactionMatchingBy.ReferenceToReference1AndDate, Datevariance, ConfigureRules);
                }
                #endregion

                #region 4. Reference to Description and Date (Note: this match can only be used if the 'Reference and Date' match has already been carried out)
                //Check if the amounts match: Amounts Match
                //Check if the dates match: IsDateMatchRequired = true
                //Check if the reference and reference1 values match: Reference To Description
                if (ConfigureRules.IsRefAndDate_RefToDesc_Set == 1)
                {
                    TransactionsA = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, true, taskId)).ToList();
                    TransactionsB = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, false, taskId)).ToList();
                    this.Matchingprocess(TransactionsA, TransactionsB, transactionMatchingTypeId, true, TransactionMatchingBy.ReferenceToDescriptionAndDate, Datevariance, ConfigureRules);
                }
                #endregion

                #region 5. Reference
                //Check if the amounts match: Amounts Match
                //Check if the references match: Reference
                if (ConfigureRules.IsReference_Set == 1)
                {
                    TransactionsA = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, true, taskId)).ToList();
                    TransactionsB = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, false, taskId)).ToList();
                    this.Matchingprocess(TransactionsA, TransactionsB, transactionMatchingTypeId, false, TransactionMatchingBy.Reference, Datevariance, ConfigureRules);
                }
                #endregion

                #region 6. Reference to Reference1  (Note: this match can only be used if the 'Reference' match has already been carried out)
                //Check if the amounts match: Amounts Match
                //Check if the reference and reference1 values match: Reference to Reference1
                if (ConfigureRules.IsRef_RefToRef1_Set == 1)
                {
                    TransactionsA = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, true, taskId)).ToList();
                    TransactionsB = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, false, taskId)).ToList();
                    this.Matchingprocess(TransactionsA, TransactionsB, transactionMatchingTypeId, false, TransactionMatchingBy.ReferenceToReference1, Datevariance, ConfigureRules);
                }
                #endregion

                #region 7. Reference to Description (Note: this match can only be used if the 'Reference' match has already been carried out)
                //Check if the amounts match: Amounts Match
                //Check if the reference and reference1 values match: Reference To Description
                if (ConfigureRules.IsRef_RefToDesc_Set == 1)
                {
                    TransactionsA = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, true, taskId)).ToList();
                    TransactionsB = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, false, taskId)).ToList();
                    this.Matchingprocess(TransactionsA, TransactionsB, transactionMatchingTypeId, false, TransactionMatchingBy.ReferenceToDescription, Datevariance, ConfigureRules);
                }
                #endregion

                #region 8. Description and Date
                //Check if the amounts match: Amounts Match
                //Check if the dates match: IsDateMatchRequired = true
                //Check if the descriptions match: Description
                if (ConfigureRules.IsDescriptionAndDate_Set == 1)
                {
                    TransactionsA = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, true, taskId)).ToList();
                    TransactionsB = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, false, taskId)).ToList();
                    this.Matchingprocess(TransactionsA, TransactionsB, transactionMatchingTypeId, true, TransactionMatchingBy.DescriptionAndDate, Datevariance, ConfigureRules);
                }
                #endregion

                #region 9. Description
                //Check if the amounts match: Amounts Match
                //Check if the descriptions match: Description
                if (ConfigureRules.IsDescription_Set == 1)
                {
                    TransactionsA = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, true, taskId)).ToList();
                    TransactionsB = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, false, taskId)).ToList();
                    this.Matchingprocess(TransactionsA, TransactionsB, transactionMatchingTypeId, false, TransactionMatchingBy.Description, Datevariance, ConfigureRules);
                }
                #endregion

                #region 10. Amount and Date 
                //Check if the amounts match: Amounts Match
                //Check if the dates match: IsDateMatchRequired = true
                if (ConfigureRules.IsAmountAndDate_Set == 1)
                {
                    TransactionsA = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, true, taskId)).ToList();
                    TransactionsB = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, false, taskId)).ToList();
                    this.Matchingprocess(TransactionsA, TransactionsB, transactionMatchingTypeId, true, TransactionMatchingBy.AmountAndDate, Datevariance, ConfigureRules);
                }

                #endregion

                #region 11. Reversals
                if (ConfigureRules.IsReversals_Set == 1)
                {
                    TransactionsA = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, true, taskId)).ToList();
                    TransactionsB = _db.Database.SqlQuery<transactionsMatchingModel>(string.Format(queryFilter, transactionmatchingtaskId, false, taskId)).ToList();
                    this.MatchingForReversals(TransactionsA, TransactionsB);
                }
                #endregion

                _db.Configuration.AutoDetectChangesEnabled = true;
                _db.Configuration.ValidateOnSaveEnabled = true;
                return true;
            }
            catch (Exception ex)
            {

                _db.Configuration.AutoDetectChangesEnabled = true;
                _db.Configuration.ValidateOnSaveEnabled = true;
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return false;
            }
        }
        /// <summary>
        /// transaction matching process by method
        /// </summary>
        /// <param name="TransactionsA"></param>
        /// <param name="TransactionsB"></param>
        /// <param name="transactionMatchingTypeId"></param>
        /// <param name="DateMatchRequired">method is required match Date</param>
        /// <param name="MatchingBy">matching by method</param>
        /// <returns></returns>
        private bool Matchingprocess(List<transactionsMatchingModel> TransactionsA, List<transactionsMatchingModel> TransactionsB,
            int transactionMatchingTypeId, bool DateMatchRequired, string MatchingBy, int Datevariance, transactionmatchingrule ConfigureRules)
        {
            try
            {
                _db.Configuration.AutoDetectChangesEnabled = false;
                _db.Configuration.ValidateOnSaveEnabled = false;

                bool DoAmountsMatch = false, DoDatesMatch = false, DoMatch = false; ;
                if (TransactionsA.Count == 0 || TransactionsB.Count == 0)
                    return false;
                var lastTranA = TransactionsA[TransactionsA.Count - 1].transactionMatchingRecordId;
                var lastTranB = TransactionsB[TransactionsB.Count - 1].transactionMatchingRecordId;
                bool IsAmount1VarianceUsed, IsAmount2VarianceUsed;
                foreach (var TranA in TransactionsA)
                {
                    foreach (var TranB in TransactionsB)
                    {
                        IsAmount1VarianceUsed = false;
                        IsAmount2VarianceUsed = false;
                        //Check if the amounts match (result = DoAmountsMatch) 
                        DoAmountsMatch = MatchingAlgorithms.AmountsMatch(TranA, TranB, transactionMatchingTypeId, ConfigureRules.Amount1VarianceValue ?? 0, ConfigureRules.Amount2VarianceValue ?? 0, ref IsAmount1VarianceUsed, ref IsAmount2VarianceUsed);
                        DoMatch = false;
                        if (!DoAmountsMatch)//If the amounts do not match then
                        {
                            if (TranA.transactionMatchingRecordId == lastTranA)//If we are at the last TranA then GOTO Next Match Method
                                continue;//DoAmountsMatch = MatchingAlgorithms.AmountsMatch(TranA, TranB, transactionMatchingTypeId);
                            if (TranB.transactionMatchingRecordId == lastTranB)//If we are at the last TranB then GOTO Next TranA
                                break;
                            else//Else GOTO Next TranB
                                continue;
                        }
                        else
                        {
                            if (DateMatchRequired)
                            {
                                DoDatesMatch = false;
                                if ((Datevariance == 0 && TranA.Date.ToShortDateString() == TranB.Date.ToShortDateString()) || (Datevariance > 0 && TranA.Date.AddDays(-1 * Datevariance - 1) < TranB.Date && TranA.Date.AddDays(Datevariance + 1) > TranB.Date))////If we have to check that the dates match then i.e. IsDateMatchRequired
                                    DoDatesMatch = true;
                            }
                            else
                                DoDatesMatch = true;

                            if (!DoDatesMatch)
                            {
                                if (TranA.transactionMatchingRecordId == lastTranA)//If we are at the last TranA then GOTO Next Match Method
                                    continue;//DoAmountsMatch = MatchingAlgorithms.AmountsMatch(TranA, TranB, transactionMatchingTypeId);
                                if (TranB.transactionMatchingRecordId == lastTranB)//If we are at the last TranB then GOTO Next TranA
                                    break;
                                else//Else GOTO Next TranB
                                    continue;
                            }
                            else
                            {
                                //Do the transactions match based on the current match method
                                switch (MatchingBy)
                                {
                                    case TransactionMatchingBy.ReferenceAndDate://2
                                        DoMatch = MatchingAlgorithms.Reference(TranA, TranB);
                                        break;
                                    case TransactionMatchingBy.ReferenceToReference1AndDate://3
                                        DoMatch = MatchingAlgorithms.ReferenceToReference1(TranA, TranB);
                                        break;
                                    case TransactionMatchingBy.ReferenceToDescriptionAndDate://4
                                        DoMatch = MatchingAlgorithms.ReferenceToDescription(TranA, TranB);
                                        break;
                                    case TransactionMatchingBy.Reference://5
                                        DoMatch = MatchingAlgorithms.Reference(TranA, TranB);
                                        break;
                                    case TransactionMatchingBy.ReferenceToReference1://6
                                        DoMatch = MatchingAlgorithms.ReferenceToReference1(TranA, TranB);
                                        break;
                                    case TransactionMatchingBy.ReferenceToDescription://7
                                        DoMatch = MatchingAlgorithms.ReferenceToDescription(TranA, TranB);
                                        break;
                                    case TransactionMatchingBy.DescriptionAndDate://8
                                        DoMatch = MatchingAlgorithms.Description(TranA, TranB);
                                        break;
                                    case TransactionMatchingBy.Description://9
                                        DoMatch = MatchingAlgorithms.Description(TranA, TranB);
                                        break;
                                    case TransactionMatchingBy.AmountAndDate://10
                                        DoMatch = true;
                                        break;
                                }

                                if (!DoMatch)//If we don't find a match between TranA and TranB
                                {
                                    if (TranA.transactionMatchingRecordId == lastTranA)//If we are at the last TranA then GOTO Next Match Method
                                        continue;//DoAmountsMatch = MatchingAlgorithms.AmountsMatch(TranA, TranB, transactionMatchingTypeId);
                                    if (TranB.transactionMatchingRecordId == lastTranB)//If we are at the last TranB then GOTO Next TranA
                                        break;
                                    else//Else GOTO Next TranB
                                        continue;
                                }
                                else//If we do find a match
                                {
                                    int methodID = 0;
                                    switch (MatchingBy)
                                    {
                                        case TransactionMatchingBy.ReferenceAndDate:
                                            methodID = transactionmatchingmethodId.ReferenceAndDate;
                                            break;
                                        case TransactionMatchingBy.ReferenceToReference1AndDate:
                                            methodID = transactionmatchingmethodId.ReferenceToReference1AndDate;
                                            break;
                                        case TransactionMatchingBy.ReferenceToDescriptionAndDate:
                                            methodID = transactionmatchingmethodId.ReferenceToDescriptionAndDate;
                                            break;
                                        case TransactionMatchingBy.Reference:
                                            methodID = transactionmatchingmethodId.Reference;
                                            break;
                                        case TransactionMatchingBy.ReferenceToReference1:
                                            methodID = transactionmatchingmethodId.ReferenceToReference1;
                                            break;
                                        case TransactionMatchingBy.ReferenceToDescription:
                                            methodID = transactionmatchingmethodId.ReferenceToDescription;
                                            break;
                                        case TransactionMatchingBy.DescriptionAndDate:
                                            methodID = transactionmatchingmethodId.DescriptionAndDate;
                                            break;
                                        case TransactionMatchingBy.Description:
                                            methodID = transactionmatchingmethodId.Description;
                                            break;
                                        case TransactionMatchingBy.AmountAndDate:
                                            methodID = transactionmatchingmethodId.AmountAndDate;
                                            break;
                                    }

                                    transactionmatchinggroup tranGroup = new transactionmatchinggroup
                                    {
                                        TransactionMatchMethodID = methodID,
                                        TransactionMatchRelationshipId = Enums.transactionmatchingrelationshipId.OneToOne,
                                        IsPartialMatch = false,
                                        Amount1VarianceValue = ConfigureRules.Amount1VarianceValue,
                                        Amount2VarianceValue = ConfigureRules.Amount2VarianceValue,
                                        DateVarianceValue = Datevariance,
                                        IsAmount1VarianceUsed = IsAmount1VarianceUsed ? 1 : 0,
                                        IsAmount2VarianceUsed = IsAmount2VarianceUsed ? 1 : 0,
                                        IsDateVarianceUsed = Datevariance > 0 ? 1 : 0
                                    };
                                    _db.Entry(tranGroup).State = EntityState.Added;
                                    var tranMatched = new transactionmatchingmatched
                                    {
                                        TransactionMatchingRecordId = TranA.transactionMatchingRecordId
                                    };
                                    _db.Entry(tranMatched).State = EntityState.Added;
                                    tranGroup.transactionmatchingmatcheds.Add(tranMatched);
                                    tranMatched = new transactionmatchingmatched
                                    {
                                        TransactionMatchingRecordId = TranB.transactionMatchingRecordId
                                    };
                                    _db.Entry(tranMatched).State = EntityState.Added;
                                    tranGroup.transactionmatchingmatcheds.Add(tranMatched);

                                }
                            }
                        }

                        _db.SaveChanges();
                        TransactionsB.Remove(TranB);
                        break;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _db.Configuration.AutoDetectChangesEnabled = true;
                _db.Configuration.ValidateOnSaveEnabled = true;
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return false;
            }

        }

        /// <summary>
        /// Algorithm matching Reversals: actually do 2 Reversal Matches, with 2 ListOfTransaction
        /// </summary>
        /// <param name="TransactionsA">The first ListOfTransaction is those transactions from AccountA (IsAccountA = 1) that have not yet been matched</param>
        /// <param name="TransactionsB">The second ListOfTransaction is those transactions from AccountB (IsAccountA = 0) that have not yet been matched</param>
        /// <returns></returns>
        private bool MatchingForReversals(List<transactionsMatchingModel> TransactionsA, List<transactionsMatchingModel> TransactionsB)
        {
            //Input a list of transactions: ListOfTransaction
            this.ReversalsAlgorithm(TransactionsA);
            this.ReversalsAlgorithm(TransactionsB);
            return true;
        }
        /// <summary>
        /// Algorithm Reversals
        /// </summary>
        /// <param name="ListOfTransaction">Input a list of transactions: ListOfTransaction</param>
        /// <returns></returns>
        private bool ReversalsAlgorithm(List<transactionsMatchingModel> ListOfTransaction)
        {
            try
            {

                var listOfTransactionsB = ListOfTransaction.ToList();
                var listOfTransactionsA = ListOfTransaction.ToList();

                for (int j = listOfTransactionsA.Count - 1; j >= 0; j--)
                {
                    var TransactionA = listOfTransactionsA[j];

                    for (int i = listOfTransactionsB.Count - 1; i >= 0; i--)
                    {
                        var TransactionB = listOfTransactionsB[i];

                        //If TransactionA = TransactionB  Then Next TransactionB
                        if (TransactionA == TransactionB)
                            continue;
                        //Check if the amounts match
                        bool DoAmountsMatch = false;
                        if (TransactionA.Debit != null)//If TransactionA is a Debit then
                        {
                            if (TransactionB.Credit != null)//If TransactionB is a Debit then
                                if (TransactionA.Debit == TransactionB.Credit)//If TransactionA.Debit = TransactionB.Debit then
                                    DoAmountsMatch = true;
                        }
                        else
                        {
                            if (TransactionA.Credit != null)//If TransactionA is a Credit then
                                if (TransactionB.Debit != null)//If TransactionB is a Credit then
                                    if (TransactionA.Credit == TransactionB.Debit)//If TransactionA.Credit = TransactionB.Credit then
                                        DoAmountsMatch = true;
                        }


                        if (!DoAmountsMatch)//If DoAmountsMatch = False
                            continue;//Next TransactionB
                                     //Check if the references match
                        bool DoReferencesMatch = false;
                        if (PatternMatchingManager.CheckReferenceWithReference(TransactionA.Reference, TransactionB.Reference))
                            DoReferencesMatch = true;
                        if (!DoReferencesMatch)//If DoReferencesMatch = False
                            continue;//Next TransactionB
                                     //Create a transaction matching group (in the transactionmatchinggroup table) where
                        transactionmatchinggroup tranGroup = new transactionmatchinggroup
                        {
                            TransactionMatchRelationshipId = Enums.transactionmatchingrelationshipId.OneToOne,
                            TransactionMatchMethodID = transactionmatchingmethodId.Reversals,
                            IsPartialMatch = false
                        };
                        _db.Entry(tranGroup).State = EntityState.Added;
                        var tranMatched = new transactionmatchingmatched
                        {
                            TransactionMatchingRecordId = TransactionA.transactionMatchingRecordId
                        };
                        _db.Entry(tranMatched).State = EntityState.Added;
                        tranGroup.transactionmatchingmatcheds.Add(tranMatched);
                        tranMatched = new transactionmatchingmatched
                        {
                            TransactionMatchingRecordId = TransactionB.transactionMatchingRecordId
                        };
                        _db.Entry(tranMatched).State = EntityState.Added;
                        tranGroup.transactionmatchingmatcheds.Add(tranMatched);

                        //Remove TransactionB  from ListOfTransactions so it cannot be compared again
                        listOfTransactionsB.Remove(TransactionB);
                        listOfTransactionsB.Remove(TransactionA);
                        listOfTransactionsA.Remove(TransactionB);
                        listOfTransactionsA.Remove(TransactionA);
                        i = -1;

                    }
                    _db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                _db.Configuration.AutoDetectChangesEnabled = true;
                _db.Configuration.ValidateOnSaveEnabled = true;
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return false;
            }
        }

        /// <summary>
        /// TransactionMatchingByFieldName - default set as Reference
        /// </summary>
        /// <param name="transactionmatchingtaskId"></param>
        /// <param name="transactionMatchingTypeId"></param>
        /// <param name="ConfigureRules"></param>
        /// <returns></returns>
        public bool TransactionMatchingByFieldName(int transactionmatchingtaskId, int transactionMatchingTypeId)
        {
            try
            {
                int tranMatch = 0;
                _db.Configuration.AutoDetectChangesEnabled = false;
                _db.Configuration.ValidateOnSaveEnabled = false;
                string columnsMatchingField = TransactionMatchingBy.Reference;
                int recordId = 0;

                //Prepare a list of the transactions in Account 1 (Account1Transactions)
                var Acc1Transactions = (from t in _db.transactions
                                        join tmr in _db.transactionmatchingrecords on t.Id equals tmr.TransactionId
                                        where tmr.TransactionMatchingTaskId == transactionmatchingtaskId && tmr.IsAccountA == true
                                        select new transactionsMatchingModel
                                        {
                                            transactionId = t.Id,
                                            transactionMatchingRecordId = tmr.Id,
                                            Reference = t.Reference,
                                            Debit = t.Debit,
                                            Credit = t.Credit
                                        }).ToList();
                //Prepare a list of the transactions in Account 2(Account2Transactions)
                var Acc2Transactions = (from t in _db.transactions
                                        join tmr in _db.transactionmatchingrecords on t.Id equals tmr.TransactionId
                                        where tmr.TransactionMatchingTaskId == transactionmatchingtaskId && tmr.IsAccountA == false
                                        select new transactionsMatchingModel
                                        {
                                            transactionId = t.Id,
                                            transactionMatchingRecordId = tmr.Id,
                                            Reference = t.Reference,
                                            Debit = t.Debit,
                                            Credit = t.Credit
                                        }).ToList();

                var Account1Transactions = ConvertList2Datatable(Acc1Transactions);
                var Account2Transactions = ConvertList2Datatable(Acc2Transactions);

                //Prepare a list of the unique references in Account1Transactions for Debits(Account1Debits)
                var a1 = ConvertList2Datatable(Acc1Transactions.Where(d => d.Debit != null).ToList());
                var Account1Debits = (from DataRow row in a1.Rows select row[columnsMatchingField].ToString()).ToList().Distinct();
                //Prepare a list of the unique references in Account1Transactions for Credits (Account1Credits)
                var a2 = ConvertList2Datatable(Acc1Transactions.Where(d => d.Credit != null).ToList());
                var Account1Credits = (from DataRow row in a2.Rows select row[columnsMatchingField].ToString()).ToList().Distinct();

                var TransactionsA = Account1Transactions.Clone();
                var TransactionsB = Account1Transactions.Clone();
                #region [For each reference in Account1Debits]
                foreach (var dataMatchingField in Account1Debits)
                {
                    TransactionsA = Account1Transactions.Clone();
                    TransactionsB = Account1Transactions.Clone();
                    //Find the transactions in Account1Transactions with that reference and store them in a list (TransactionsA)
                    foreach (DataRow r in Account1Transactions.Rows)
                    {
                        if (r[columnsMatchingField].ToString() == dataMatchingField)
                            TransactionsA.Rows.Add(r.ItemArray);
                    }
                    // checking transactionmatchingtype
                    switch (transactionMatchingTypeId)
                    {
                        case (int)tranMatchingType.DebitToDebit://If the transactionmatchingtype is Debit to Debit then
                            foreach (DataRow r2 in Account2Transactions.Rows)
                            {//Find the debit transactions in Account2Transactions with that reference and store them in a list (TransactionsB)
                                if (r2[columnsMatchingField].ToString() == dataMatchingField && r2["Debit"] != null)
                                    TransactionsB.Rows.Add(r2.ItemArray);
                            }

                            break;
                        case (int)tranMatchingType.DebitToCredit://If the transactionmatchingtype is Debit to Credit then
                            foreach (DataRow r2 in Account2Transactions.Rows)
                            {//Find the credit transactions in Account2Transactions with that reference and store them in a list (TransactionsB)
                                if (r2[columnsMatchingField].ToString() == dataMatchingField && r2["Credit"] != null)
                                    TransactionsB.Rows.Add(r2.ItemArray);
                            }
                            break;
                    }
                    //If there are 0 transactions in TransactionsB then 
                    if (TransactionsB.Rows.Count == 0)
                    {
                        foreach (DataRow rA in TransactionsA.Rows)//The transactions in TransactionsA are unmatched
                        {
                            recordId = Converter.Obj2Int(rA["transactionMatchingRecordId"]);
                            //Remove the transactions in TransactionsA from Account1Transactions
                            var view = new DataView(Account1Transactions)
                            {
                                RowFilter = "transactionMatchingRecordId = " + recordId // MyValue here is a column name
                            };

                            // Delete these rows.
                            foreach (DataRowView row in view)
                            {
                                row.Delete();
                            }
                            Account1Transactions.AcceptChanges();
                        }

                        continue;//Goto Step "each reference in Account1Debits" and loop with the next reference
                    }


                    bool isPartialMatch = false;
                    int transactionmatchingrelationshipId = 0;

                    if (TransactionsA.Rows.Count > 0)//If there are 1 or more transactions in TransactionA then
                    {

                        //The transactions in TransactionsA and TransactionsB match
                        //Work out if it is a complete match or partial match (result = IsPartialMatch)
                        switch (transactionMatchingTypeId)
                        {
                            case (int)tranMatchingType.DebitToDebit://If the transactionmatchingtype is Debit to Debit then
                                //If the sum of the debits in TransactionsA = the sum of the debits in TransactionsB then we have a complete match IsPartialMatch = FALSE
                                if (Converter.Obj2Decimal(TransactionsA.Compute("Sum(Debit)", "")) ==
                                        Converter.Obj2Decimal(TransactionsB.Compute("Sum(Debit)", "")))
                                    isPartialMatch = false;
                                else//If the sum of the debits in TransactionsA <> the sum of the debits in TransactionsB then we have a partial match IsPartialMatch = TRUE
                                    isPartialMatch = true;
                                break;
                            case (int)tranMatchingType.DebitToCredit://If the transactionmatchingtype is Debit to Credit then
                                if (Converter.Obj2Decimal(TransactionsA.Compute("Sum(Debit)", "")) ==
                                        Converter.Obj2Decimal(TransactionsB.Compute("Sum(Credit)", "")))//If the sum of the debits in TransactionsA = the sum of the credits in TransactionsB then we have a complete match IsPartialMatch = FALSE
                                    isPartialMatch = false;
                                else//If the sum of the debits in TransactionsA <> the sum of the credits in TransactionsB then we have a partial match IsPartialMatch = TRUE
                                    isPartialMatch = true;
                                break;
                        }

                        //Update to ignore partial matches
                        if (isPartialMatch)
                        {
                            continue;//Goto Step "each reference in Account1Debits" and loop with the next reference
                        }

                        //Work out the transaction matching relationship using TransactionsA and TransactionsB (result = transactionmatchingrelationship)

                        // If there is 1 transaction in TransactionsA and 1 transaction in TransactionsB then we have a One to One relationship
                        if (TransactionsA.Rows.Count == 1 && TransactionsB.Rows.Count == 1)
                        {
                            // transactionmatchingrelationshipId = Enums.transactionmatchingrelationshipId.OneToOne;
                            // Change required to stop One to One matchs being considered as Many to Many;
                            continue;//Goto Step "each reference in Account1Debits" and loop with the next reference
                        }
                        else if (TransactionsA.Rows.Count == 1 && TransactionsB.Rows.Count > 1)//If there is 1 transaction in TransactionsA and more than 1 transaction in TransactionsB then we have a One to Many relationship
                            transactionmatchingrelationshipId = Enums.transactionmatchingrelationshipId.OneToMany;
                        else if (TransactionsA.Rows.Count > 1 && TransactionsB.Rows.Count == 1)//If there is more than one transaction in TransactionsA and 1 transaction in TransactionsB then we have a Many to One relationship
                            transactionmatchingrelationshipId = Enums.transactionmatchingrelationshipId.ManyToOne;
                        else if (TransactionsA.Rows.Count > 1 && TransactionsB.Rows.Count > 1)//If there is more than one transaction in TransactionsA and more than one transaction in TransactionsB then we have a Many to Many relationship
                            transactionmatchingrelationshipId = Enums.transactionmatchingrelationshipId.ManyToMany;

                        // Create a transaction matching group (in the transactionmatchinggroup table) where
                        transactionmatchinggroup tranGroup = new transactionmatchinggroup
                        {
                            TransactionMatchMethodID = Enums.transactionmatchingmethodId.ManyToMany,
                            TransactionMatchRelationshipId = transactionmatchingrelationshipId,
                            IsPartialMatch = isPartialMatch
                        };
                        _db.Entry(tranGroup).State = EntityState.Added;
                        _db.SaveChanges();
                        //Add a record to the transactionmatchingmatched table for each transaction in TransactionsA and each record in TransactionsB 
                        //linked to the  transactionmatchinggroup just created
                        foreach (DataRow rA in TransactionsA.Rows)
                        {
                            recordId = Converter.Obj2Int(rA["transactionMatchingRecordId"]);
                            transactionmatchingmatched tm = new transactionmatchingmatched
                            {
                                TransactionMatchingRecordId = recordId,
                                TransactionMatchingGroupId = tranGroup.Id
                            };
                            tranMatch++;
                            _db.Entry(tm).State = EntityState.Added;
                            _db.transactionmatchingmatcheds.Add(tm);
                            //Remove the transactions in TransactionsA from the Account1Transactions so they cannot be compared again
                            var view = new DataView(Account1Transactions)
                            {
                                RowFilter = "transactionMatchingRecordId = " + recordId // MyValue here is a column name
                            };

                            // Delete these rows.
                            foreach (DataRowView row in view)
                            {
                                row.Delete();
                            }
                            Account1Transactions.AcceptChanges();
                        }
                        foreach (DataRow rB in TransactionsB.Rows)
                        {
                            recordId = Converter.Obj2Int(rB["transactionMatchingRecordId"]);
                            transactionmatchingmatched tm = new transactionmatchingmatched
                            {
                                TransactionMatchingRecordId = recordId,
                                TransactionMatchingGroupId = tranGroup.Id
                            };
                            tranMatch++;
                            _db.Entry(tm).State = EntityState.Added;
                            _db.transactionmatchingmatcheds.Add(tm);
                            //Remove the transactions in TransactionsA from the Account1Transactions so they cannot be compared again
                            var view = new DataView(Account2Transactions)
                            {
                                RowFilter = "transactionMatchingRecordId = " + recordId // MyValue here is a column name
                            };

                            // Delete these rows.
                            foreach (DataRowView row in view)
                            {
                                row.Delete();
                            }
                            Account2Transactions.AcceptChanges();
                        }
                    }
                }
                #endregion


                #region [For each reference in Account1Credits]
                //Find the transactions in Account1Transactions with that reference and store them in a list (TransactionsA)
                foreach (var dataMatchingField in Account1Credits)
                {
                    TransactionsA = Account1Transactions.Clone();
                    TransactionsB = Account1Transactions.Clone();
                    //Find the transactions in Account1Transactions with that reference and store them in a list (TransactionsA)
                    foreach (DataRow r in Account1Transactions.Rows)
                    {
                        if (r[columnsMatchingField].ToString() == dataMatchingField)
                            TransactionsA.Rows.Add(r.ItemArray);
                    }
                    //checking transactionmatchingtype 
                    switch (transactionMatchingTypeId)
                    {
                        case (int)tranMatchingType.DebitToDebit://If the transactionmatchingtype is Debit to Debit (which is also Credit to Credit) then
                            foreach (DataRow r2 in Account2Transactions.Rows)
                            {//Find the credit transactions in Account2Transactions with that reference and store them in a list (TransactionsB)
                                if (r2[columnsMatchingField].ToString() == dataMatchingField && r2["Credit"] != null)
                                    TransactionsB.Rows.Add(r2.ItemArray);
                            }

                            break;
                        case (int)tranMatchingType.DebitToCredit://If the transactionmatchingtype is Debit to Credit (which is also Credit to Debit) then
                            foreach (DataRow r2 in Account2Transactions.Rows)
                            {//Find the debit transactions in Account2Transactions with that reference and store them in a list (TransactionsB)
                                if (r2[columnsMatchingField].ToString() == dataMatchingField && r2["Debit"] != null)
                                    TransactionsB.Rows.Add(r2.ItemArray);
                            }
                            break;
                    }

                    //If there are 0 transactions in TransactionsB then 
                    if (TransactionsB.Rows.Count == 0)
                    {
                        foreach (DataRow rA in TransactionsA.Rows)//The transactions in TransactionsA are unmatched
                        {
                            recordId = Converter.Obj2Int(rA["transactionMatchingRecordId"]);
                            //Remove the transactions in TransactionsA from Account1Transactions
                            var view = new DataView(Account1Transactions)
                            {
                                RowFilter = "transactionMatchingRecordId = " + recordId // MyValue here is a column name
                            };

                            // Delete these rows.
                            foreach (DataRowView row in view)
                            {
                                row.Delete();
                            }
                            Account1Transactions.AcceptChanges();
                        }

                        continue;//Goto Step "each reference in Account1Debits" and loop with the next reference
                    }
                    bool isPartialMatch = false;
                    int transactionmatchingrelationshipId = 0;

                    if (TransactionsA.Rows.Count > 0)//If there are 1 or more transactions in TransactionA then
                    {

                        //The transactions in TransactionsA and TransactionsB match
                        //Work out if it is a complete match or partial match (result = IsPartialMatch)
                        switch (transactionMatchingTypeId)
                        {
                            case (int)tranMatchingType.DebitToDebit://If the transactionmatchingtype is Debit to Debit (which is also Credit to Credit) then
                                //If the sum of the credits in TransactionsA = the sum of the credits in TransactionsB then we have a complete match IsPartialMatch = FALSE
                                if (Converter.Obj2Decimal(TransactionsA.Compute("Sum(Credit)", "")) ==
                                        Converter.Obj2Decimal(TransactionsB.Compute("Sum(Credit)", "")))
                                    isPartialMatch = false;
                                else//If the sum of the credits in TransactionsA <> the sum of the credits in TransactionsB then we have a partial match IsPartialMatch = TRUE
                                    isPartialMatch = true;
                                break;
                            case (int)tranMatchingType.DebitToCredit://If the transactionmatchingtype is Debit to Credit (which is also Credit to Debit) then
                                //If the sum of the credits in TransactionsA = the sum of the debits in TransactionsB then we have a complete match IsPartialMatch = FALSE
                                if (Converter.Obj2Decimal(TransactionsA.Compute("Sum(Credit)", "")) ==
                                        Converter.Obj2Decimal(TransactionsB.Compute("Sum(Debit)", "")))
                                    isPartialMatch = false;
                                else//If the sum of the credits in TransactionsA <> the sum of the debits in TransactionsB then we have a partial match IsPartialMatch = TRUE
                                    isPartialMatch = true;
                                break;
                        }
                        //Update to ignore partial matches
                        if (isPartialMatch)
                        {
                            continue;//Goto Step "each reference in Account1Credits" and loop with the next reference
                        }

                        //Work out the transaction matching relationship using TransactionsA and TransactionsB (result = transactionmatchingrelationship)

                        // If there is 1 transaction in TransactionsA and 1 transaction in TransactionsB then we have a One to One relationship
                        if (TransactionsA.Rows.Count == 1 && TransactionsB.Rows.Count == 1)
                            //transactionmatchingrelationshipId = Enums.transactionmatchingrelationshipId.OneToOne;
                            // Change required to stop One to One matchs being considered as Many to Many;
                            continue;//Goto Step "each reference in Account1Credits" and loop with the next reference
                        else if (TransactionsA.Rows.Count == 1 && TransactionsB.Rows.Count > 1)//If there is 1 transaction in TransactionsA and more than 1 transaction in TransactionsB then we have a One to Many relationship
                            transactionmatchingrelationshipId = Enums.transactionmatchingrelationshipId.OneToMany;
                        else if (TransactionsA.Rows.Count > 1 && TransactionsB.Rows.Count == 1)//If there is more than one transaction in TransactionsA and 1 transaction in TransactionsB then we have a Many to One relationship
                            transactionmatchingrelationshipId = Enums.transactionmatchingrelationshipId.ManyToOne;
                        else if (TransactionsA.Rows.Count > 1 && TransactionsB.Rows.Count > 1)//If there is more than one transaction in TransactionsA and more than one transaction in TransactionsB then we have a Many to Many relationship
                            transactionmatchingrelationshipId = Enums.transactionmatchingrelationshipId.ManyToMany;

                        // Create a transaction matching group (in the transactionmatchinggroup table) where
                        transactionmatchinggroup tranGroup = new transactionmatchinggroup
                        {
                            TransactionMatchMethodID = Enums.transactionmatchingmethodId.ManyToMany,
                            TransactionMatchRelationshipId = transactionmatchingrelationshipId,
                            IsPartialMatch = isPartialMatch
                        };
                        _db.Entry(tranGroup).State = EntityState.Added;
                        _db.SaveChanges();
                        //Add a record to the transactionmatchingmatched table for each transaction in TransactionsA and each record in TransactionsB 
                        //linked to the  transactionmatchinggroup just created
                        foreach (DataRow rA in TransactionsA.Rows)
                        {
                            recordId = Converter.Obj2Int(rA["transactionMatchingRecordId"]);
                            transactionmatchingmatched tm = new transactionmatchingmatched
                            {
                                TransactionMatchingRecordId = recordId,
                                TransactionMatchingGroupId = tranGroup.Id
                            };
                            tranMatch++;
                            _db.Entry(tm).State = EntityState.Added;
                            _db.transactionmatchingmatcheds.Add(tm);
                            //Remove the transactions in TransactionsA from the Account1Transactions so they cannot be compared again
                            var view = new DataView(Account1Transactions)
                            {
                                RowFilter = "transactionMatchingRecordId = " + recordId // MyValue here is a column name
                            };

                            // Delete these rows.
                            foreach (DataRowView row in view)
                            {
                                row.Delete();
                            }
                            Account1Transactions.AcceptChanges();
                        }

                        foreach (DataRow rB in TransactionsB.Rows)
                        {
                            recordId = Converter.Obj2Int(rB["transactionMatchingRecordId"]);
                            transactionmatchingmatched tm = new transactionmatchingmatched
                            {
                                TransactionMatchingRecordId = recordId,
                                TransactionMatchingGroupId = tranGroup.Id
                            };
                            tranMatch++;
                            _db.Entry(tm).State = EntityState.Added;
                            _db.transactionmatchingmatcheds.Add(tm);
                            //Remove the transactions in TransactionsA from the Account1Transactions so they cannot be compared again
                            var view = new DataView(Account2Transactions)
                            {
                                RowFilter = "transactionMatchingRecordId = " + recordId // MyValue here is a column name
                            };

                            // Delete these rows.
                            foreach (DataRowView row in view)
                            {
                                row.Delete();
                            }
                            Account2Transactions.AcceptChanges();
                        }
                    }
                }
                #endregion

                _db.SaveChanges();
                _db.Configuration.AutoDetectChangesEnabled = true;
                _db.Configuration.ValidateOnSaveEnabled = true;
                return true;
            }
            catch (Exception ex)
            {
                _db.Configuration.AutoDetectChangesEnabled = true;
                _db.Configuration.ValidateOnSaveEnabled = true;
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return false;
            }
        }


    }
}
