using Qbicles.BusinessRules.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using static Qbicles.BusinessRules.HelperClass;

namespace Qbicles.BusinessRules.BusinessRules.CleanBooks
{
    public class TransactionMatchingReportRules
    {
        ApplicationDbContext _db;
        public TransactionMatchingReportRules()
        {
        }

        public TransactionMatchingReportRules(ApplicationDbContext context)
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
        List<AccountTransactionsReport> _data = new List<AccountTransactionsReport>();

        // here we simulate SQL search, sorting and paging operations
        // !!!! DO NOT DO THIS IN REAL APPLICATION !!!!
        /// <summary>
        /// 
        /// </summary>
        /// <param name="recordFiltered"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="search"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="bindProceedOrApplyProfile">0 - Proceed, 1- Apply</param>
        /// <returns></returns>
        public List<AccountTransactionsReport> FilterData(ref int recordFiltered, int start, int length, string search,
            int sortColumn, string sortDirection, bool bindProceedOrApplyProfile, ref int recordsTotal, int accountId1, int accountId2, int transactionmatchingtaskId, int type)
        {
            var list = new List<AccountTransactionsReport>();
            if (type == 1)
            {
                var lstaccount1UnMatch = (from t in _db.transactions
                                          join u in _db.uploads.Where(p => p.AccountId == accountId1) on t.UploadId equals u.Id
                                          join tm in _db.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionmatchingtaskId && !_db.transactionmatchingmatcheds.Select(s => s.TransactionMatchingRecordId).Contains(p.Id)) on t.Id equals tm.TransactionId
                                          select new
                                          {
                                              t.Balance,
                                              t.Date,
                                              t.Debit,
                                              t.Credit,
                                              t.Reference,
                                              t.Description
                                          }).ToList();
                recordsTotal = lstaccount1UnMatch.Count;
                if (_data.Count == 0)
                {
                    foreach (var item in lstaccount1UnMatch)
                    {
                        _data.Add(new AccountTransactionsReport
                        {
                            Date = item.Date.ToString(),
                            Reference = item.Reference,
                            Description = item.Description,
                            Credit = item.Credit == null ? "" : Converter.Obj2Decimal(item.Credit).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),
                            Debit = item.Debit == null ? "" : Converter.Obj2Decimal(item.Debit).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),
                            Balance = item.Balance == null ? "" : Converter.Obj2Decimal(item.Balance).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),

                        });
                    }
                }

            }
            else if (type == 2)
            {
                var lstaccount2UnMatch = (from t in _db.transactions
                                          join u in _db.uploads.Where(p => p.AccountId == accountId2) on t.UploadId equals u.Id
                                          join tm in _db.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionmatchingtaskId && !_db.transactionmatchingmatcheds.Select(s => s.TransactionMatchingRecordId).Contains(p.Id)) on t.Id equals tm.TransactionId
                                          select new
                                          {
                                              t.Balance,
                                              t.Date,
                                              t.Debit,
                                              t.Credit,
                                              t.Reference,
                                              t.Description
                                          }).ToList();
                recordsTotal = lstaccount2UnMatch.Count;
                if (_data.Count == 0)
                {
                    foreach (var item in lstaccount2UnMatch)
                    {
                        _data.Add(new AccountTransactionsReport
                        {
                            Date = item.Date.ToString(),
                            Reference = item.Reference,
                            Description = item.Description,
                            Credit = item.Credit == null ? "" : Converter.Obj2Decimal(item.Credit).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),
                            Debit = item.Debit == null ? "" : Converter.Obj2Decimal(item.Debit).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),
                            Balance = item.Balance == null ? "" : Converter.Obj2Decimal(item.Balance).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),

                        });
                    }
                }
            }
            else if (type == 3)
            {
                var lstaccount1Match = (from t in _db.transactions
                                        join u in _db.uploads.Where(p => p.AccountId == accountId1) on t.UploadId equals u.Id
                                        join tm in _db.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionmatchingtaskId) on t.Id equals tm.TransactionId
                                        join m in _db.transactionmatchingmatcheds on tm.Id equals m.TransactionMatchingRecordId
                                        select new
                                        {
                                            t.Balance,
                                            t.Date,
                                            t.Debit,
                                            t.Credit,
                                            t.Reference,
                                            t.Description
                                        }).ToList();
                recordsTotal = lstaccount1Match.Count;
                if (_data.Count == 0)
                {
                    foreach (var item in lstaccount1Match)
                    {
                        _data.Add(new AccountTransactionsReport
                        {
                            Date = item.Date.ToString(),
                            Reference = item.Reference,
                            Description = item.Description,
                            Credit = item.Credit == null ? "" : Converter.Obj2Decimal(item.Credit).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),
                            Debit = item.Debit == null ? "" : Converter.Obj2Decimal(item.Debit).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),
                            Balance = item.Balance == null ? "" : Converter.Obj2Decimal(item.Balance).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),

                        });
                    }
                }
            }
            else
            {
                var lstaccount2Match = (from t in _db.transactions
                                        join u in _db.uploads.Where(p => p.AccountId == accountId2) on t.UploadId equals u.Id
                                        join tm in _db.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionmatchingtaskId) on t.Id equals tm.TransactionId
                                        join m in _db.transactionmatchingmatcheds on tm.Id equals m.TransactionMatchingRecordId
                                        select new
                                        {
                                            t.Balance,
                                            t.Date,
                                            t.Debit,
                                            t.Credit,
                                            t.Reference,
                                            t.Description
                                        }).ToList();
                recordsTotal = lstaccount2Match.Count;
                if (_data.Count == 0)
                {
                    foreach (var item in lstaccount2Match)
                    {
                        _data.Add(new AccountTransactionsReport
                        {
                            Date = item.Date.ToString(),
                            Reference = item.Reference,
                            Description = item.Description,
                            Credit = item.Credit == null ? "" : Converter.Obj2Decimal(item.Credit).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),
                            Debit = item.Debit == null ? "" : Converter.Obj2Decimal(item.Debit).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),
                            Balance = item.Balance == null ? "" : Converter.Obj2Decimal(item.Balance).ToString("#,###.#0", CultureInfo.InvariantCulture.NumberFormat),

                        });
                    }
                }
            }

            if (search == null)
            {
                list = _data;
            }
            else
            {
                // simulate search
                foreach (AccountTransactionsReport dataItem in _data)
                {
                    if (
                        dataItem.Date.Contains(search.ToUpper()) ||
                        dataItem.Reference.ToUpper().Contains(search.ToUpper()) ||
                        dataItem.Description.ToUpper().Contains(search.ToUpper()) ||
                        dataItem.Credit.Contains(search.ToUpper()) ||
                        dataItem.Debit.Contains(search.ToUpper()) ||
                        dataItem.Balance.Contains(search.ToUpper()))
                    {
                        list.Add(dataItem);
                    }
                }
            }

            // simulate sort
            if (sortColumn == 0)
            {
                list.Sort((x, y) => SortString(x.Date.ToString(), y.Date.ToString(), sortDirection));
            }
            else if (sortColumn == 1)
            {
                list.Sort((x, y) => SortString(x.Reference, y.Reference, sortDirection));
            }
            else if (sortColumn == 2)
            {
                list.Sort((x, y) => SortString(x.Description, y.Description, sortDirection));
            }
            else if (sortColumn == 3)
            {
                list.Sort((x, y) => SortString(x.Credit.ToString(), y.Credit.ToString(), sortDirection));
            }
            else if (sortColumn == 4)
            {
                list.Sort((x, y) => SortString(x.Debit.ToString(), y.Debit.ToString(), sortDirection));
            }
            else if (sortColumn == 5)
            {
                list.Sort((x, y) => SortString(x.Balance.ToString(), y.Balance.ToString(), sortDirection));
            }
            recordFiltered = list.Count;

            // get just one page of data
            if (start > list.Count)
                start = list.Count;
            list = list.GetRange(start, Math.Min(length, list.Count - start));

            return list;
        }


        public ReturnJsonModel BindingForReconciliationStatement(long accountId, long accountId2, int transactionmatchingtaskId, int taskId)
        {
            var refModel = new ReturnJsonModel();
            var accountdb = _db.Accounts.Where(x => x.Id == accountId || x.Id == accountId2).ToList();
            var account1 = accountdb.FirstOrDefault(x => x.Id == accountId);
            var account2 = accountdb.FirstOrDefault(x => x.Id == accountId2);
            var accountTrans = (from acc in _db.Accounts.Where(x => x.Id == accountId || x.Id == accountId2)
                                join ul in _db.uploads on acc.Id equals ul.AccountId
                                join tran in _db.transactions on ul.Id equals tran.UploadId
                                select new
                                {
                                    AccountId = acc.Id,
                                    AccountName = acc.Name,
                                    Credit = tran.Credit,
                                    Debit = tran.Debit,
                                    Balance = tran.Balance
                                }).ToList();
            List<string> listErr = new List<string>();

            if (accountTrans.Where(x => x.AccountId == accountId && x.Balance != null).Count() == 0)
            {
                listErr.Add("Account " + account1.Name + " does not have a balance");
            }
            if (accountTrans.Where(x => x.AccountId == accountId && x.Credit != null).Count() == 0)
            {
                listErr.Add("Account " + account1.Name + " does not have a credit");
            }
            if (accountTrans.Where(x => x.AccountId == accountId && x.Debit != null).Count() == 0)
            {
                listErr.Add("Account " + account1.Name + " does not have a credit");
            }

            if (accountTrans.Where(x => x.AccountId == accountId2 && x.Balance != null).Count() == 0)
            {
                listErr.Add("Account " + account2.Name + " does not have a balance");
            }
            if (accountTrans.Where(x => x.AccountId == accountId2 && x.Credit != null).Count() == 0)
            {
                listErr.Add("Account " + account2.Name + " does not have a credit");
            }
            if (accountTrans.Where(x => x.AccountId == accountId2 && x.Debit != null).Count() == 0)
            {
                listErr.Add("Account " + account2.Name + " does not have a credit");
            }

            var countAmount = (from t in _db.tasks.Where(x => x.Id == taskId)
                               join tranrule in _db.transactionmatchingtaskrules on t.Id equals tranrule.TaskId
                               where tranrule.Amount > 0
                               select tranrule.Amount).Count();
            if (countAmount > 0)
            {
                listErr.Add("An Amount variance was used in the Transaction Matching process");
            }


            if (listErr.Count > 0)
            {
                refModel.Object = listErr;
                refModel.result = false;
                return refModel;
            }
            else
            {
                var Acc1Balance = account1.LastBalance;
                var Acc2Balance = account2.LastBalance;

                var lstaccount1UnMatchdb = (from t in _db.transactions
                                            join u in _db.uploads.Where(p => p.AccountId == accountId || p.AccountId == accountId2) on t.UploadId equals u.Id
                                            join tm in _db.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionmatchingtaskId && !_db.transactionmatchingmatcheds.Select(s => s.TransactionMatchingRecordId).Contains(p.Id)) on t.Id equals tm.TransactionId
                                            select new
                                            {
                                                t.Debit,
                                                t.Credit,
                                                u.AccountId
                                            }).ToList();

                var lstaccount1UnMatch = lstaccount1UnMatchdb.Where(x => x.AccountId == accountId).Select(x => new { x.Credit, x.Debit }).ToList();
                var lstaccount2UnMatch = lstaccount1UnMatchdb.Where(x => x.AccountId == accountId2).Select(x => new { x.Credit, x.Debit }).ToList();

                var Acc1UnMatchedDebits = lstaccount1UnMatch.Select(x => x.Debit ?? 0).Sum();
                var Acc1UnMatchedCredits = lstaccount1UnMatch.Select(x => x.Credit ?? 0).Sum();
                var Acc2UnMatchedDebits = lstaccount2UnMatch.Select(x => x.Debit ?? 0).Sum();
                var Acc2UnMatchedCredits = lstaccount2UnMatch.Select(x => x.Credit ?? 0).Sum();

                var task = _db.tasks.FirstOrDefault(x => x.Id == taskId);
                decimal? Acc1AdjustedBalance = 0;
                if (task.TransactionMatchingTypeId == 3)
                {
                    Acc1AdjustedBalance = Acc1Balance - Acc1UnMatchedDebits + Acc1UnMatchedCredits - Acc2UnMatchedDebits + Acc2UnMatchedCredits;
                }
                else if (task.TransactionMatchingTypeId == 2)
                {
                    Acc1AdjustedBalance = Acc1Balance.Value + Acc1UnMatchedDebits - Acc1UnMatchedCredits - Acc2UnMatchedDebits + Acc2UnMatchedCredits;
                }
                refModel.Object = new { Acc1Balance, Acc1UnMatchedDebits, Acc1UnMatchedCredits, Acc2UnMatchedDebits, Acc2UnMatchedCredits, Acc1AdjustedBalance, Acc2Balance, task.TransactionMatchingTypeId };
                refModel.result = true;
                return refModel;
            }

        }

        public ReturnJsonModel GetMatchingProgress(int taskId, int accountId, int accountId2)
        {
            var refModel = new ReturnJsonModel();
            String analyse = "";
            var taskinstance = DbContext.taskinstances.Where(m => m.TaskId == taskId).ToList();

            if (taskinstance.Count() > 0)
            {
                int countTransactionsMatched = 0, countTransactionsUnMatched = 0, countTransactionsManual = 0;
                int transactionMatchingTaskId = 0;
                foreach (var b in taskinstance)
                {
                       transactionMatchingTaskId = b.transactionmatchingtasks.FirstOrDefault() == null ? 0 :
                       b.transactionmatchingtasks.FirstOrDefault().Id;

                        var lstaccount1UnMatchdb = (from t in DbContext.transactions
                                                    join u in DbContext.uploads.Where(p => p.AccountId == accountId || p.AccountId == accountId2) on t.UploadId equals u.Id
                                                    join tm in DbContext.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionMatchingTaskId && !_db.transactionmatchingmatcheds.Select(s => s.TransactionMatchingRecordId).Contains(p.Id)) on t.Id equals tm.TransactionId
                                                    select t).ToList();


                        countTransactionsMatched = _db.transactionmatchingmatcheds.
                        Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == transactionMatchingTaskId
                        && tm.transactionmatchinggroup.TransactionMatchMethodID != Enums.transactionmatchingmethodId.Manual).Count();

                        countTransactionsManual = _db.transactionmatchingmatcheds.
                        Where(tm => tm.transactionmatchingrecord.TransactionMatchingTaskId == transactionMatchingTaskId
                        && tm.transactionmatchinggroup.TransactionMatchMethodID == Enums.transactionmatchingmethodId.Manual).Count();

                        countTransactionsUnMatched = lstaccount1UnMatchdb.Count;
                        int percentages = countTransactionsUnMatched + countTransactionsManual + countTransactionsMatched;// total transactionmatchingrecords
                        analyse = new TransactionMatchingRules(_db).genProgressViewReport(percentages, countTransactionsMatched, countTransactionsUnMatched, countTransactionsManual);
                        
                }
            }
            refModel.msg = analyse;
            refModel.result = true;
            return refModel;
        }

        public ReturnJsonModel ExportToExcel(int accountId1, string account1Name, int accountId2, string account2Name, int transactionmatchingtaskId, int taskId)
        {
            var refModel = new ReturnJsonModel();

            try
            {
                
                var accountdb = _db.Accounts.Where(x => x.Id == accountId1 || x.Id == accountId2).ToList();
                var account1 = accountdb.FirstOrDefault(x => x.Id == accountId1);
                var account2 = accountdb.FirstOrDefault(x => x.Id == accountId2);

                var accountTrans = (from acc in _db.Accounts.Where(x => x.Id == accountId1 || x.Id == accountId2)
                                    join ul in _db.uploads on acc.Id equals ul.AccountId
                                    join tran in _db.transactions on ul.Id equals tran.UploadId
                                    select new
                                    {
                                        AccountId = acc.Id,
                                        AccountName = acc.Name,
                                        Credit = tran.Credit,
                                        Debit = tran.Debit,
                                        Balance = tran.Balance
                                    }).ToList();
                List<string> listErr = new List<string>();

                if (accountTrans.Where(x => x.AccountId == accountId1 && x.Balance != null).Count() == 0)
                {
                    listErr.Add("Account " + account1.Name + " does not have a balance");
                }
                if (accountTrans.Where(x => x.AccountId == accountId1 && x.Credit != null).Count() == 0)
                {
                    listErr.Add("Account " + account1.Name + " does not have a credit");
                }
                if (accountTrans.Where(x => x.AccountId == accountId1 && x.Debit != null).Count() == 0)
                {
                    listErr.Add("Account " + account1.Name + " does not have a credit");
                }
                if (accountTrans.Where(x => x.AccountId == accountId2 && x.Balance != null).Count() == 0)
                {
                    listErr.Add("Account " + account2.Name + " does not have a balance");
                }
                if (accountTrans.Where(x => x.AccountId == accountId2 && x.Credit != null).Count() == 0)
                {
                    listErr.Add("Account " + account2.Name + " does not have a credit");
                }
                if (accountTrans.Where(x => x.AccountId == accountId2 && x.Debit != null).Count() == 0)
                {
                    listErr.Add("Account " + account2.Name + " does not have a credit");
                }

                var countAmount = (from t in _db.tasks.Where(x => x.Id == taskId)
                                   join tranrule in _db.transactionmatchingtaskrules on t.Id equals tranrule.TaskId
                                   where tranrule.Amount > 0
                                   select tranrule.Amount).Count();
                if (countAmount > 0)
                {
                    listErr.Add("An Amount variance was used in the Transaction Matching process");
                }

                var lstaccount1UnMatchdb = (from t in _db.transactions
                                            join u in _db.uploads.Where(p => p.AccountId == accountId1 || p.AccountId == accountId2) on t.UploadId equals u.Id
                                            join tm in _db.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionmatchingtaskId && !_db.transactionmatchingmatcheds.Select(s => s.TransactionMatchingRecordId).Contains(p.Id)) on t.Id equals tm.TransactionId
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

                var lstaccount1UnMatchrecol = lstaccount1UnMatchdb.Where(x => x.AccountId == accountId1).Select(x => new { x.Credit, x.Debit }).ToList();
                var lstaccount2UnMatchrecol = lstaccount1UnMatchdb.Where(x => x.AccountId == accountId2).Select(x => new { x.Credit, x.Debit }).ToList();

                var Acc1UnMatchedDebits = lstaccount1UnMatchrecol.Sum(x => x.Debit ?? 0);
                var Acc1UnMatchedCredits = lstaccount1UnMatchrecol.Sum(x => x.Credit ?? 0);
                var Acc2UnMatchedDebits = lstaccount2UnMatchrecol.Sum(x => x.Debit ?? 0);
                var Acc2UnMatchedCredits = lstaccount2UnMatchrecol.Sum(x => x.Credit ?? 0);

                var task = _db.tasks.FirstOrDefault(x => x.Id == taskId);
                decimal? Acc1AdjustedBalance = 0;
                if (task.TransactionMatchingTypeId == 3)
                {
                    Acc1AdjustedBalance = account1.LastBalance - Acc1UnMatchedDebits + Acc1UnMatchedCredits - Acc2UnMatchedDebits + Acc2UnMatchedCredits;
                }
                else if (task.TransactionMatchingTypeId == 2)
                {
                    Acc1AdjustedBalance = account1.LastBalance + Acc1UnMatchedDebits - Acc1UnMatchedCredits - Acc2UnMatchedDebits + Acc2UnMatchedCredits;
                }
                List<AccountTransactionsReport> list = new List<AccountTransactionsReport>();
                var lstSheetName = new List<string>();
                var ds = new System.Data.DataSet();
                var dt = new System.Data.DataTable();
                dt.TableName = "";
                lstSheetName.Add("Matching Statement");
                dt.Columns.Add("Date", typeof(string));
                dt.Columns.Add("Reference", typeof(string));
                dt.Columns.Add("Description", typeof(string));
                dt.Columns.Add("Debit", typeof(string));
                dt.Columns.Add("Credit", typeof(string));
                var dt2 = dt.Clone();

                dt2.TableName = ((account1Name + " Unmatched").Length > 31 ? account1Name.Substring(0, 21) : account1Name) + " Unmatched";
                lstSheetName.Add(((account1Name + " Unmatched").Length > 31 ? account1Name.Substring(0, 21) : account1Name) + " Unmatched");
                var dt3 = dt.Clone();
                dt3.TableName = ((account2Name + " Unmatched").Length > 31 ? account2Name.Substring(0, 21) : account2Name) + " Unmatched";
                lstSheetName.Add(((account2Name + " Unmatched").Length > 31 ? account2Name.Substring(0, 21) : account2Name) + " Unmatched");
                var dt4 = dt.Clone();
                dt4.TableName = ((account1Name + " Matched").Length > 31 ? account1Name.Substring(0, 23) : account1Name) + " Matched";
                lstSheetName.Add(((account1Name + " Matched").Length > 31 ? account1Name.Substring(0, 23) : account1Name) + " Matched");
                var dt5 = dt.Clone();
                dt5.TableName = ((account2Name + " Matched").Length > 31 ? account2Name.Substring(0, 23) : account2Name) + " Matched";
                lstSheetName.Add(((account2Name + " Matched").Length > 31 ? account2Name.Substring(0, 23) : account2Name) + " Matched");
                var dtrecol = new DataTable();
                dtrecol.TableName = "Matching Statement";
                dtrecol.Columns.Add("", typeof(string));
                var row = dtrecol.NewRow();
                row[0] = "Matching Report";
                dtrecol.Rows.Add(row);
                row = dtrecol.NewRow();
                dtrecol.Rows.Add(row);
                row = dtrecol.NewRow();
                dtrecol.Rows.Add(row);
                if (listErr.Count > 0)
                {
                    row = dtrecol.NewRow();
                    row[0] = "A Matching report has not been generate for this Transaction Matching task because:";
                    dtrecol.Rows.Add(row);
                    for (var i = 0; i < listErr.Count; i++)
                    {
                        row = dtrecol.NewRow();
                        row[0] = (i + 1) + ". " + listErr[i];
                        dtrecol.Rows.Add(row);
                    }
                }
                else
                {
                    row = dtrecol.NewRow();
                    row[0] = "Balance of " + account1.Name + ": " + Converter.Obj2Decimal(account1.LastBalance).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    row[0] = "Less " + account1.Name + " unmatched debit transactions: " + Converter.Obj2Decimal(Acc1UnMatchedDebits).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    row[0] = "Plus " + account1.Name + " unmatched credit transactions: " + Converter.Obj2Decimal(Acc1UnMatchedCredits).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    row[0] = "Plus " + account2.Name + " unmatched debit transactions: " + Converter.Obj2Decimal(Acc2UnMatchedDebits).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    row[0] = "Less " + account2.Name + " unmatched credit transactions: " + Converter.Obj2Decimal(Acc2UnMatchedCredits).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    row[0] = "Adjusted balance of " + account1.Name + ": " + Converter.Obj2Decimal(Acc1AdjustedBalance).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    row[0] = "Balance of " + account2.Name + ": " + Converter.Obj2Decimal(account2.LastBalance).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    dtrecol.Rows.Add(row);
                    row = dtrecol.NewRow();
                    dtrecol.Rows.Add(row);

                }
                ds.Tables.Add(dtrecol);
                var lstaccount1UnMatch = lstaccount1UnMatchdb.Where(p => p.AccountId == accountId1).ToList();
                lstaccount1UnMatch.Sort((x, y) => SortString(x.Date, y.Date, "asc"));
                foreach (var item in lstaccount1UnMatch)
                {
                    var dr = dt2.NewRow();
                    dr["Date"] = item.Date;
                    dr["Reference"] = item.Reference;
                    dr["Description"] = item.Description;
                    dr["Credit"] = item.Credit == null ? "" : Converter.Obj2Decimal(item.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dr["Debit"] = item.Debit == null ? "" : Converter.Obj2Decimal(item.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dt2.Rows.Add(dr);
                }

                ds.Tables.Add(dt2);
                var lstaccount2UnMatch = lstaccount1UnMatchdb.Where(p => p.AccountId == accountId2).ToList();
                lstaccount2UnMatch.Sort((x, y) => SortString(x.Date, y.Date, "asc"));
                foreach (var item in lstaccount2UnMatch)
                {
                    var dr = dt3.NewRow();
                    dr["Date"] = item.Date;
                    dr["Reference"] = item.Reference;
                    dr["Description"] = item.Description;
                    dr["Credit"] = item.Credit == null ? "" : Converter.Obj2Decimal(item.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dr["Debit"] = item.Debit == null ? "" : Converter.Obj2Decimal(item.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dt3.Rows.Add(dr);
                }
                ds.Tables.Add(dt3);
                var lstaccount1Match = (from t in _db.transactions
                                        join u in _db.uploads.Where(p => p.AccountId == accountId1 || p.AccountId == accountId2) on t.UploadId equals u.Id
                                        join tm in _db.transactionmatchingrecords.Where(p => p.TransactionMatchingTaskId == transactionmatchingtaskId) on t.Id equals tm.TransactionId
                                        join m in _db.transactionmatchingmatcheds on tm.Id equals m.TransactionMatchingRecordId
                                        select new
                                        {
                                            t.Balance,
                                            t.Date,
                                            t.Debit,
                                            t.Credit,
                                            t.Reference,
                                            t.Description,
                                            u.AccountId
                                        }).ToList();
                foreach (var item in lstaccount1Match.Where(p => p.AccountId == accountId1))
                {
                    var dr = dt4.NewRow();
                    dr["Date"] = item.Date;
                    dr["Reference"] = item.Reference;
                    dr["Description"] = item.Description;
                    dr["Credit"] = item.Credit == null ? "" : Converter.Obj2Decimal(item.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dr["Debit"] = item.Debit == null ? "" : Converter.Obj2Decimal(item.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dt4.Rows.Add(dr);
                }
                ds.Tables.Add(dt4);

                foreach (var item in lstaccount1Match.Where(p => p.AccountId == accountId2))
                {
                    var dr = dt5.NewRow();
                    dr["Date"] = item.Date;
                    dr["Reference"] = item.Reference;
                    dr["Description"] = item.Description;
                    dr["Credit"] = item.Credit == null ? "" : Converter.Obj2Decimal(item.Credit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dr["Debit"] = item.Debit == null ? "" : Converter.Obj2Decimal(item.Debit).ToString("#,###.##", CultureInfo.InvariantCulture.NumberFormat);
                    dt5.Rows.Add(dr);
                }
                ds.Tables.Add(dt5);
                refModel.Object = ds;
                refModel.Object2 = lstSheetName;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return refModel;
            }
        }




        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<AccountTransactionsReport> data { get; set; }
        }

        private int SortString(string s1, string s2, string sortDirection)
        {
            return sortDirection == "asc" ? s1.CompareTo(s2) : s2.CompareTo(s1);
        }

        private int SortInteger(string s1, string s2, string sortDirection)
        {
            int i1 = int.Parse(s1);
            int i2 = int.Parse(s2);
            return sortDirection == "asc" ? i1.CompareTo(i2) : i2.CompareTo(i1);
        }

        private int SortDateTime(string s1, string s2, string sortDirection)
        {
            DateTime d1 = DateTime.Parse(s1);
            DateTime d2 = DateTime.Parse(s2);
            return sortDirection == "asc" ? d1.CompareTo(d2) : d2.CompareTo(d1);
        }
    }
}
