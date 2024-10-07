using Qbicles.BusinessRules.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using CleanBooksData;
using Qbicles.Models;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;
using Qbicles.Models.Bookkeeping;
using static Qbicles.BusinessRules.Enums;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class CBAccountRules
    {
        ApplicationDbContext dbContext;

        public CBAccountRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
     

        public List<accountgroup> GetAccountGroup(int domainId)
        {
            var accGroups = dbContext.accountgroups.Where(e => e.DomainId == domainId).ToList();
            return accGroups;
        }


        public IEnumerable<accountupdatefrequency> GetAccountupdatefrequency()
        {
            return dbContext.accountupdatefrequencies.ToList();
        }
        public CBAccountResult GetMaxUploadToAccount(int accountId)
        {
            string result;
            var refModel = new CBAccountResult();

            try
            {
                bool accountExists;
                var uploadName = dbContext.uploads.Where(e => e.AccountId == accountId)
                                    .OrderByDescending(c => c.CreatedDate)
                                    .Select(c => c.Name).FirstOrDefault();
                int pos;
                string index;
                if (uploadName == null)
                {
                    index = "0";
                    accountExists = false;
                }
                else
                {
                    pos = uploadName.LastIndexOf("-", StringComparison.Ordinal);
                    index = uploadName.Substring(pos + 1);
                    accountExists = true;
                }
                var lastBalance = dbContext.Accounts.FirstOrDefault(p => p.Id == accountId)?.LastBalance ?? 0;
                int currentIndex = HelperClass.Converter.Obj2Int(index) + 1;
                // replace lengt of sequence number to 9, i.e: length = 7 then 9999999.
                if (currentIndex < HelperClass.Converter.Obj2Int("9".PadLeft(HelperClass.UploadCountLength, '9')))
                    result = currentIndex.ToString().PadLeft(HelperClass.UploadCountLength, '0');
                else
                {
                    currentIndex++;
                    result = currentIndex.ToString();
                }
                refModel = new CBAccountResult { result = result, existsUpload = accountExists, lastbalance = lastBalance };

                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }

        public Account GetAccount(int id, bool disableLazyloading = false)
        {
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            bool lazyLoad = dbContext.Configuration.LazyLoadingEnabled;
            try
            {
                dbContext.Configuration.LazyLoadingEnabled = disableLazyloading;
                dbContext.Configuration.ProxyCreationEnabled = disableLazyloading;
                var account = dbContext.Accounts.Find(id);
                return account;
            }
            catch
            {
                return new Account();
            }
            finally
            {
                dbContext.Configuration.LazyLoadingEnabled = lazyLoad;
                dbContext.Configuration.ProxyCreationEnabled = proxyCreation;
            }
        }

        public Account GetAccount2Edit(int id, string timeZone)
        {
            try
            {
                var account = dbContext.Accounts.Find(id);
                if (account == null) return new Account();
                var createDate = (account.CreatedDate ?? DateTime.UtcNow).ConvertTimeToUtc(timeZone);
                return new Account
                {
                    Id = account.Id,
                    Name = account.Name,
                    Number = account.Number,
                    UpdateFrequencyId = account.UpdateFrequencyId,
                    DataManagerId = account.DataManagerId,
                    GroupId = account.GroupId,
                    IsActive = account.IsActive,
                    CreatedDate = createDate,
                    CreatedById = account.CreatedById,
                    LastBalance = account.LastBalance
                };
            }
            catch
            {
                return new Account();
            }
        }

        public List<upload> GetUploadsByAccount(int accountId)
        {
            return dbContext.uploads.Where(u => u.AccountId == accountId).ToList();
        }

        public string[] GetUpFieldByAccountId(int accountId)
        {
            return dbContext.uploadfields.Where(uf => uf.AccountId == accountId).Select(e => e.Name).ToArray();
        }
        public int[] GetDomainRoleAccounts(int accountId)
        {
            return dbContext.Accounts.Find(accountId)?.DomainRoles.Select(e => e.Id).ToArray();
        }
        public accountgroup GetAccountGroupById(int id)
        {
            bool proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            try
            {
                dbContext.Configuration.ProxyCreationEnabled = false;
                var accountgroup = dbContext.accountgroups.Find(id);
                dbContext.Entry(accountgroup).State = EntityState.Detached;
                return accountgroup;
            }
            catch
            {
                return new accountgroup();
            }
            finally
            {
                dbContext.Configuration.ProxyCreationEnabled = proxyCreation;
            }
        }
        public bool DuplicateAccountGroup(accountgroup group)
        {
            try
            {
                if (group.Id > 0)
                {
                    return dbContext.accountgroups.Any(x => x.Id != group.Id && x.Name.ToLower() == group.Name.ToLower() && x.DomainId == group.DomainId);
                }
                else
                {
                    return dbContext.accountgroups.Any(x => x.Name.ToLower() == group.Name.ToLower() && x.DomainId == group.DomainId);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return true;
            }
        }
        public ReturnJsonModel SaveCBAccountGroup(accountgroup group)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (group.Id > 0)
                {
                    if (dbContext.Entry(group).State == EntityState.Detached)
                        dbContext.accountgroups.Attach(group);
                    dbContext.Entry(group).State = EntityState.Modified;
                    refModel.actionVal = 2;
                    refModel.msg = group.Name;
                    refModel.msgId = group.Id.ToString();
                    refModel.msgName = group.Name;
                    dbContext.SaveChanges();
                }
                else
                {
                    group.CreatedDate = DateTime.UtcNow;
                    dbContext.accountgroups.Add(group);
                    dbContext.Entry(group).State = EntityState.Added;
                    refModel.actionVal = 1;
                    dbContext.SaveChanges();
                    //append to select group
                    refModel.msgId = group.Id.ToString();
                    refModel.msgName = group.Name;


                    var groupViewer = new StringBuilder();
                    groupViewer.Append($"<button onclick = \"EditGroup('{group.Id}')\" class='btn btn-warning'><i class='fa fa-pencil'></i> &nbsp; Edit group</button>&nbsp;");
                    groupViewer.Append($"<button class='btn btn-success' onclick = \"AddAccount('{group.Id}')\"><i class='fa fa-plus'></i> &nbsp; Add an account</button>");
                    groupViewer.Append($"<br /><br />");

                    var groupGrid = new StringBuilder();
                    groupGrid.Append($"<h5><span id='account-group-name-grid-{group.Id}'>{group.Name}</span></h5><hr />");
                    groupGrid.Append(groupViewer);
                    groupGrid.Append($"<ul id='ul-account-{group.Id}' class='grid-list'></ul>");
                    refModel.msg = groupGrid.ToString();

                    var groupList = new StringBuilder();
                    groupList.Append($"<h5><span id='account-group-name-list-{group.Id}'>{group.Name}</span></h5><hr />");
                    groupList.Append(groupViewer);
                    groupList.Append("<br />");

                    groupList.Append($"<table id='tableList-{group.Id}' class='accgroup-table table table-hover t1style valignm custome-table' style='width:100%' cellspacing='0'>");
                    groupList.Append($"<thead>");
                    groupList.Append($"<tr>");
                    groupList.Append($"<th data-priority='1'>Name</th>");
                    groupList.Append($"<th data-priority='1'>Last Updated</th>");
                    groupList.Append($"<th data-priority='1'>Balance</th>");
                    groupList.Append($"<th data-priority='1'>Data Manager</th>");
                    groupList.Append($"<th data-priority='1'>Linked Tasks</th>");
                    groupList.Append($"<th data-priority='2' data-orderable='false'>Options</th>");
                    groupList.Append($"</tr></thead><tbody></tbody></table>");
                    refModel.Object = groupList.ToString();
                }

                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex, group.CreatedById);
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            return refModel;
        }
        public bool DupplicateAccount(Account account)
        {
            if (account.Id > 0)
                return dbContext.Accounts.Any(m => m.Id != account.Id && account.Name.ToLower() == m.Name.ToLower() && m.GroupId == account.GroupId);
            else
                return dbContext.Accounts.Any(m => (account.Name.ToLower() == m.Name.ToLower() && m.GroupId == account.GroupId));
        }

        public ReturnJsonModel SaveCBAccount(Account account, bool isEditLastbalance, string uploadFields, string rolesGrant,
            string currentUserId, int currentDomainId)
        {


            var refModel = new ReturnJsonModel();
            try
            {
                int actionVal;
                var uploadfields = uploadFields.Split(',');
                var roleGrant = rolesGrant != null ? rolesGrant.Split(',') : null;

                if (account.Id > 0)
                {
                    actionVal = 2;
                    long accId = account.Id;

                    // upload fields
                    var upfields = dbContext.uploadfields.Where(a => a.AccountId == accId).ToList();
                    dbContext.uploadfields.RemoveRange(upfields);
                    foreach (var item in uploadfields)
                    {
                        dbContext.uploadfields.Add(new uploadfield
                        {
                            AccountId = accId,
                            Name = item
                        });
                    }

                    account.WorkGroup = dbContext.CBWorkGroups.Find(account.WorkGroup.Id);


                    var first = dbContext.Accounts.FirstOrDefault(p => p.Id == account.Id);

                    first.BookkeepingAccount = dbContext.BKAccounts.Find(account.BookkeepingAccount.Id);
                    first.Name = account.Name;
                    first.Number = account.Number;
                    first.WorkGroup = account.WorkGroup;
                    //if (first.WorkGroup != null)
                    //    first.WorkGroup.Accounts.Add(first);
                    //DbContext.SaveChanges();


                    // roles grand
                    //string sqlQuery = $"DELETE FROM domainroleaccounts WHERE Account_Id = {account.Id}";
                    
                    //_db.Database.ExecuteSqlCommand(sqlQuery);
                    //foreach (var item in roleGrant)
                    //{
                    //    var domainRole = DbContext.DomainRole.Find(int.Parse(item));
                    //    first.DomainRoles.Add(domainRole);
                    //}


                    //var entry = DbContext.Entry(account);
                    if (dbContext.Entry(first).State == EntityState.Detached)
                        dbContext.Accounts.Attach(first);
                    dbContext.Entry(first).State = EntityState.Modified;
                }
                else
                {
                    actionVal = 1;
                    account.IsActive = 1;
                    account.uploadfields = new List<uploadfield>();
                    foreach (var item in uploadfields)
                    {
                        account.uploadfields.Add(new uploadfield
                        {
                            Name = item
                        });
                    }
                    // roles grand
                    //string sqlQuery = $"DELETE FROM domainroleaccounts WHERE Account_Id = {account.Id}";
                    //_db.Database.ExecuteSqlCommand(sqlQuery);
                    //foreach (var item in roleGrant)
                    //{
                    //    var domainRole = DbContext.DomainRole.Find(int.Parse(item));
                    //    account.DomainRoles.Add(domainRole);
                    //}

                    account.WorkGroup = dbContext.CBWorkGroups.Find(account.WorkGroup.Id);
                    dbContext.SaveChanges();

                    account.BookkeepingAccount = dbContext.BKAccounts.Find(account.BookkeepingAccount.Id);
                    dbContext.Accounts.Add(account);
                    dbContext.Entry(account).State = EntityState.Added;
                }


                dbContext.SaveChanges();



                //refModel = GenerateAccountDisplay(account, userRoleRights);

                refModel.actionVal = actionVal;
                refModel.result = true;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex, currentUserId);
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            return refModel;
        }
        public void DeleteAccount(Account account, string userId)
        {
            var accountRemove = new Account { Id = account.Id };
            dbContext.Accounts.Attach(accountRemove);
            dbContext.Accounts.Remove(accountRemove);
            //create deletedaccount
            deletedaccount delAccount = new deletedaccount()
            {
                AccountName = account.Name,
                AccountNumber = account.Number,
                DeleteById = userId,
                DeleteDate = DateTime.UtcNow,
                CreatedById = account.CreatedById,
                CreatedDate = account.CreatedDate
            };
            dbContext.deletedaccounts.Add(delAccount);
            dbContext.Entry(delAccount).State = EntityState.Added;

            //delete uploadfields
            var delUploadField = dbContext.uploadfields.Where(ta => ta.AccountId == account.Id).ToList();
            if (delUploadField.Count > 0)
                dbContext.uploadfields.RemoveRange(delUploadField);
            var delUpload = dbContext.deleteduploads.Where(d => d.AccountId == account.Id).ToList();
            if (delUpload.Count > 0)
                dbContext.deleteduploads.RemoveRange(delUpload);
            dbContext.SaveChanges();
        }
        public List<accountgroup> LoadAccountGroups(int groupId, int domainId, SortOrderBy orderBy)
        {
            List<accountgroup> accountGroup;
            if (groupId == 0)
                accountGroup = dbContext.accountgroups.Where(e => e.DomainId == domainId).ToList();
            else
                accountGroup = dbContext.accountgroups.Where(e => e.Id == groupId).ToList();
            var wgs = new CBWorkGroupsRules(dbContext).GetAllCbWorkGroup(domainId)
                .Where(q => q.Processes.Any(p => p.Name == CBProcessName.AccountDataProcessName || p.Name == CBProcessName.AccountProcessName)).ToList();
            accountGroup.ForEach(e => e.Accounts = e.Accounts.Where(q => (q.WorkGroup != null && wgs.Any(w => w.Id == q.WorkGroup.Id)) || q.WorkGroup == null).Distinct().ToList());
                    
            switch (orderBy)
            {
                case SortOrderBy.NameAZ:
                    accountGroup.ForEach(e => e.Accounts = e.Accounts.OrderBy(r => r.Name).ToList());
                    break;
                case SortOrderBy.NameZA:
                    accountGroup.ForEach(e => e.Accounts = e.Accounts.OrderByDescending(r => r.Name).ToList());
                    break;
                case SortOrderBy.BalanceHigh:
                    accountGroup.ForEach(e => e.Accounts = e.Accounts.OrderByDescending(r => r.LastBalance).ToList());
                    break;
                case SortOrderBy.BalanceLow:
                    accountGroup.ForEach(e => e.Accounts = e.Accounts.OrderBy(r => r.LastBalance).ToList());
                    break;
                case SortOrderBy.LastUpdateNewest:
                    accountGroup.ForEach(e => e.Accounts = e.Accounts.OrderByDescending(x => x.uploads.Max(s => s.CreatedDate)).ToList());
                    break;
                case SortOrderBy.LastUpdateOldest:
                    accountGroup.ForEach(e => e.Accounts = e.Accounts.OrderBy(x => x.uploads.Min(s => s.CreatedDate)).ToList());
                    break;
                case SortOrderBy.DataManagerAZ:
                    accountGroup.ForEach(e => e.Accounts = e.Accounts.OrderBy(n => n.user.UserName).ToList());
                    break;
                case SortOrderBy.DataManagerZA:
                    accountGroup.ForEach(e => e.Accounts = e.Accounts.OrderByDescending(n => n.user.UserName).ToList());
                    break;
                case SortOrderBy.LinkedTasksMost:
                    accountGroup.ForEach(e => e.Accounts = e.Accounts.OrderByDescending(r => r.taskaccounts.Count).ToList());
                    break;
                case SortOrderBy.LinkedTasksLeast:
                    accountGroup.ForEach(e => e.Accounts = e.Accounts.OrderBy(t => t.taskaccounts.Count).ToList());
                    break;
            }

            return accountGroup;
        }

        public IEnumerable<upload> UploadPreview(int accountId)
        {
            return dbContext.uploads.Where(a => a.AccountId == accountId);
        }

        public Account GetAccountById(int id)
        {
            var account = dbContext.Accounts.Find(id);
            return account ?? new Account();
        }

        public List<BKTransaction> ShowImportFromBookkeeping(int bkAccountId, int domainId)
        {
            var cbAccount = dbContext.Accounts.Find(bkAccountId);
            if (cbAccount?.BookkeepingAccount == null) return new List<BKTransaction>();

            var closeDate = dbContext.BookClosures.OrderByDescending(o => o.ClosureDate).FirstOrDefault(e => e.Domain.Id == domainId)?.ClosureDate ?? DateTime.Today;

            var bkTransactions = cbAccount.BookkeepingAccount.Transactions.Where(t =>
                t.PostedDate > closeDate //with a BKTransaction.PostedDate later than the latest BookClosure.ClosureDate
                && t.CBTransactions.All(e => e.upload.AccountId != cbAccount.Id)
                //&& t.CBTransactions.Count <= 0 //where the BKTransaction is not already imported (Use the relationship BKTransaction.CBTransactions or transaction.BKTransaction) to check
                && t.JournalEntry.Approval.RequestStatus == ApprovalReq.RequestStatusEnum.Approved); //and the BkTransaction is in an Appoved Journal Entry



            return bkTransactions.ToList();
        }
        /// <summary>
        /// All transactions that go into Bookkeeping, have to go through an Approval process
        /// We have double-entry bookkeeping to make sure that there are no errors
        /// So do not calculator the Balance in transaction and account CleanBooks
        /// </summary>
        /// <param name="bkAccountId"></param>
        /// <param name="domainId"></param>
        /// <param name="createdById"></param>
        /// <returns></returns>
        public ReturnJsonModel ImportFromBookkeeping(int bkAccountId, int domainId, string createdById)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var cbAccount = dbContext.Accounts.Find(bkAccountId);
                if (cbAccount?.BookkeepingAccount == null)
                {
                    refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_642", null));
                    return refModel;
                }

                var closeDate = dbContext.BookClosures.OrderByDescending(o => o.ClosureDate).FirstOrDefault(e => e.Domain.Id == domainId)?.ClosureDate ?? DateTime.Today;

                var bkTransactions = cbAccount.BookkeepingAccount.Transactions.Where(t =>
                    t.PostedDate > closeDate //with a BKTransaction.PostedDate later than the latest BookClosure.ClosureDate
                    && t.CBTransactions.All(e => e.upload.AccountId != cbAccount.Id)
                    //&& t.CBTransactions.Count <= 0 //where the BKTransaction is not already imported (Use the relationship BKTransaction.CBTransactions or transaction.BKTransaction) to check
                    && t.JournalEntry.Approval.RequestStatus == ApprovalReq.RequestStatusEnum.Approved).OrderByDescending(d => d.PostedDate).ToList(); //and the BkTransaction is in an Appoved Journal Entry


                var maxUpload = GetMaxUploadToAccount(bkAccountId);

                var startDate = bkTransactions.Min(t => t.PostedDate);
                var endDate = bkTransactions.Max(t => t.PostedDate);

                var upload = new upload
                {
                    Name = $"CleanBooks {cbAccount.Name} {cbAccount.Number }{DateTime.Today:yyyy/MM/dd}-{maxUpload.result}",
                    account = cbAccount,
                    AccountId = cbAccount.Id,
                    CreatedDate = DateTime.UtcNow,
                    CreatedById = createdById,
                    StartDate = startDate,
                    EndDate = endDate,
                    transactions = new List<transaction>()
                };
                var uploadFormat = new UploadFormat
                {
                    Name = upload.Name,
                    CreatedById = createdById,
                    IsDateAscending = false,
                    DateFormatId = dbContext.dateformats.FirstOrDefault().Id,
                    FileTypeId = dbContext.filetypes.FirstOrDefault(t => t.Type == "Excel").Id,
                    DateIndex = 0,
                    ReferenceIndex = 0,
                    DescriptionIndex = 0,
                    DebitIndex = 0,
                    CreditIndex = 0,
                    BalanceIndex = 0,
                    Reference1Index = 0,
                    DescCol1Index = 0,
                    DescCol2Index = 0,
                    DescCol3Index = 0
                };
                upload.UploadFormat = uploadFormat;

                bkTransactions.ForEach(tran =>
                {
                    upload.transactions.Add(new transaction
                    {
                        CreatedDate = DateTime.UtcNow,
                        Description = tran.Memo,
                        IsActive = 1,
                        BKTransaction = tran,
                        Balance = tran.Balance,
                        Credit = tran.Credit,
                        Debit = tran.Debit,
                        Date = tran.PostedDate,
                        Reference = tran.Reference,
                        DescCol1 = "",
                        DescCol2 = "",
                        DescCol3 = "",
                        Reference1 = "",

                        //UploadId = 0,
                        //upload = upload
                    });
                });


                dbContext.uploads.Add(upload);
                dbContext.SaveChanges();
                refModel.msg = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                refModel.result = true;
            }
            catch (Exception e)
            {
                refModel.msg = e.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(),e, createdById);
            }

            return refModel;
        }

    }
}