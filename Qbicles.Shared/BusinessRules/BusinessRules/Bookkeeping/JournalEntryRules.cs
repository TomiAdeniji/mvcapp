using Newtonsoft.Json;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.Models.ApprovalReq;

namespace Qbicles.BusinessRules
{
    public class JournalEntryRules
    {
        ApplicationDbContext dbContext;
        public JournalEntryRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public DataTablesResponse JournalEntriesFilter(IDataTablesRequest requestModel,
            string accounts, string status, string groups, string dates, int domainId, UserSetting user)
        {
            var lstCashAccountTransactionModel = new List<CashAccountTransactionModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, accounts, status, groups, dates, domainId);


                var journals = dbContext.JournalEntrys.Where(e => e.Domain.Id == domainId);//.ToList();

                var startDate = DateTime.MinValue;
                var endDate = DateTime.MinValue;
                if (!string.IsNullOrWhiteSpace(dates) && dates != ";")
                {
                    try
                    {
                        dates.Replace(';', '-').ConvertDaterangeFormat(user.DateFormat, "", out startDate, out endDate);
                        startDate = startDate.AddDays(-1).AddMinutes(1);
                        endDate = endDate.AddDays(1).AddMinutes(-1);
                    }
                    catch
                    {
                        startDate = DateTime.MinValue;
                    }
                    if (startDate != DateTime.MinValue)
                    {
                        journals = journals.Where(e => e.PostedDate >= startDate && e.PostedDate <= endDate);
                    }
                }
                if (!string.IsNullOrWhiteSpace(groups))
                {
                    var lstGroups = groups.Split(',').Select(Int32.Parse).ToList();
                    journals = journals.Where(g => lstGroups.Contains(g.Group.Id));
                }
                if (!string.IsNullOrWhiteSpace(status))
                {
                    var jStatus = new List<ApprovalReq.RequestStatusEnum>();
                    var lstStatus = status.Split(',');
                    foreach (var item in lstStatus)
                    {
                        switch (item)
                        {
                            case "Reviewed":
                                jStatus.Add(ApprovalReq.RequestStatusEnum.Reviewed);
                                break;
                            case "Approved":
                                jStatus.Add(ApprovalReq.RequestStatusEnum.Approved);
                                break;
                            case "Denied":
                                jStatus.Add(ApprovalReq.RequestStatusEnum.Denied);
                                break;
                            case "Discarded":
                                jStatus.Add(ApprovalReq.RequestStatusEnum.Discarded);
                                break;
                            default:
                                jStatus.Add(ApprovalReq.RequestStatusEnum.Pending);
                                break;
                        }
                    }

                    journals = journals.Where(s => (s.Approval != null && jStatus.Any(p => p == s.Approval.RequestStatus)));
                }

                if (!string.IsNullOrWhiteSpace(accounts))
                {
                    var lstaccounts = accounts.Split(',').Select(Int32.Parse).ToList();
                    lstaccounts.ForEach(accId =>
                    {
                        journals = journals.Where(e => e.AssociatedAccounts.Count(a => a.Id == accId) > 0);//.ToList();
                    });
                }


                var totalJournals = journals.Count();
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Number":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Number" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Group":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Group.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "PostedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "PostedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Description":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Description" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Approval.RequestStatus" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "PostedDate desc";
                            break;
                    }
                }

                journals = journals.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion

                #region Paging
                var list = journals.Skip(requestModel.Start).Take(requestModel.Length).ToList();


                #endregion

                var dataJson = list.Select(q => new JournalEntryModel
                {
                    Id = q.Id,
                    Number = q.Number.ToString(),
                    Description = q.Description,
                    PostedDate = q.PostedDate.ConvertTimeFromUtc(user.Timezone).ToString(user.DateFormat),
                    Status = q.Approval == null ? RequestStatusEnum.Approved.GetDescription() : q.Approval.RequestStatus.GetDescription(),
                    Group = q.Group?.Name,
                    StatusCss = GetLabelStatus(q.Approval?.RequestStatus ?? RequestStatusEnum.Approved)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalJournals, totalJournals);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, requestModel, accounts, status, groups, dates, domainId);
            }

            return null;
        }
        public string GetLabelStatus(RequestStatusEnum status)
        {
            var css = "label-success";
            switch (status)
            {
                case ApprovalReq.RequestStatusEnum.Pending:
                    css = StatusLabelStyle.Pending;
                    break;
                case ApprovalReq.RequestStatusEnum.Reviewed:
                    css = StatusLabelStyle.Reviewed;
                    break;
                case ApprovalReq.RequestStatusEnum.Approved:
                    css = StatusLabelStyle.Approved;
                    break;
                case ApprovalReq.RequestStatusEnum.Denied:
                    css = StatusLabelStyle.Denied;
                    break;
            }

            return css;
        }
        public JournalEntry GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "JournalEntry Get by Id", null, null, id);

                var jEntry = dbContext.JournalEntrys.FirstOrDefault(q => q.Id == id);
                return jEntry;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }


        public JournalEntry GetByApprovalId(int id)
        {
            try
            {
                return dbContext.ApprovalReqs.Find(id)?.JournalEntries.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public ReturnJsonModel SaveJournalEntryTemplate(JournalEntry jEntry, JournalEntryTemplate template, QbicleDomain domain)
        {
            var refModel = new ReturnJsonModel();
            if (jEntry != null)
            {
                var bkAccountRules = new BKCoANodesRule(dbContext);

                jEntry.WorkGroup = dbContext.BKWorkGroups.Find(jEntry.WorkGroup.Id);
                if (jEntry.WorkGroup == null)
                    return refModel;
                //save template                    
                var jTemplate = new JournalEntryTemplate
                {
                    Name = template.Name,
                    Description = template.Description,
                    Domain = domain,
                    TemplateRows = new List<JournalEntryTemplateRow>()
                };
                foreach (var tran in jEntry.BKTransactions)
                {
                    var isDebit = tran.Debit.HasValue;
                    var tempRow = new JournalEntryTemplateRow
                    {
                        IsDebit = isDebit,
                        Parent = bkAccountRules.GetAccountById(tran.Account.Id).Parent,
                        Template = jTemplate
                    };
                    jTemplate.TemplateRows.Add(tempRow);
                }
                dbContext.JournalEntryTemplates.Add(jTemplate);
                dbContext.SaveChanges();
                refModel.msgId = jTemplate.Id.ToString();
                refModel.result = true;
            }
            return refModel;
        }

        public List<QbiclePost> ShowTransactionComment(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show Transaction Comment", null, null, id);

                return dbContext.Posts.Where(e => e.BKTransaction.Id == id).OrderByDescending(d => d.TimeLineDate).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new List<QbiclePost>();
            }
        }


        public List<QbicleMedia> ShowTransactionAttachments(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show Transaction Attachments", null, null, id);
                return dbContext.Medias.Where(e => e.BKTransaction.Id == id).OrderByDescending(d => d.TimeLineDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new List<QbicleMedia>();
            }
        }

        private List<JournalEntry> GetJournalEntriesByDomain(int domainId)
        {
            return dbContext.JournalEntrys.Where(j => j.Domain.Id == domainId).ToList();
        }

        public void JournalEntryApproval(ApprovalReq approval)
        {
            var journal = approval.JournalEntries.FirstOrDefault();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "JournalEntry Approval", null, null, journal);

                if (journal == null) return;
                journal.IsApproved = true;

                if (dbContext.Entry(journal).State == EntityState.Detached)
                    dbContext.JournalEntrys.Attach(journal);
                dbContext.Entry(journal).State = EntityState.Modified;
                dbContext.SaveChanges();

                new BKCoANodesRule(dbContext).UpdateNodeBalanceFromJournal(journal);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, journal);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="jEntry">JournalEntry, Transactions</param>
        /// <param name="user"></param>
        /// Submit for review need check journal if not exist ApprovalReq then create new it and linked to Journal
        /// <returns></returns>
        public ReturnJsonModel CreateJournalEntry(JournalEntry jEntry, List<BKTransactionCustom> bKTransactions, UserSetting userSetting, int domainId)
        {
            var refModel = new ReturnJsonModel();

            var user = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userSetting.Id);

            jEntry.WorkGroup = dbContext.BKWorkGroups.Find(jEntry.WorkGroup.Id);
            if (jEntry.WorkGroup == null)
            {
                refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_168", null));
                return refModel;
            }

            var mediaRules = new MediasRules(dbContext);
            var bkAccountRules = new BKCoANodesRule(dbContext);
            var dimensionRules = new TransactionDimensionRules(dbContext);

            jEntry.Group = new JournalGroupRules(dbContext).GetById(jEntry.Group.Id);
            jEntry.PostedDate = jEntry.PostedDate.ConvertTimeToUtc(userSetting.Timezone);
            jEntry.Domain = new DomainRules(dbContext).GetDomainById(domainId); ;
            jEntry.CreatedBy = user;
            jEntry.WorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;
            jEntry.IsApproved = false;
            jEntry.CreatedDate = DateTime.UtcNow;
            jEntry.BKTransactions = new List<BKTransaction>();
            jEntry.AssociatedAccounts = new List<BKAccount>();

            dbContext.JournalEntrys.Add(jEntry);
            dbContext.Entry(jEntry).State = EntityState.Added;
            dbContext.SaveChanges();


            bKTransactions.ForEach(tran =>
            {
                var transaction = new BKTransaction
                {
                    Account = bkAccountRules.GetAccountById(tran.Account.Id),
                    Reference = tran.Reference,
                    PostedDate = tran.PostedDate.ConvertTimeToUtc(userSetting.Timezone),
                    Debit = tran.Debit,
                    Credit = tran.Credit,
                    Memo = tran.Memo,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    Dimensions = new List<TransactionDimension>()
                };
                tran.Dimensions.ForEach(dm =>
                {
                    transaction.Dimensions.Add(dimensionRules.GetById(dm.Id));
                });
                jEntry.BKTransactions.Add(transaction);

                if (!jEntry.AssociatedAccounts.Any(a => a.Id == tran.Account.Id))
                    jEntry.AssociatedAccounts.Add(transaction.Account);

                dbContext.SaveChanges();

                if (tran.AssociatedFiles.Count > 0)
                    mediaRules.SaveOnReviewNewAttachmentsBKTransaction(transaction, tran.AssociatedFiles, user.Id);

            });

            var approval = new ApprovalReq
            {
                ApprovalRequestDefinition = jEntry.WorkGroup.ApprovalDefs.FirstOrDefault(),
                //ApprovalRequestDefinition = appDef,//or
                ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequestApp,
                Priority = ApprovalReq.ApprovalPriorityEnum.High,
                RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                //Sale = new List<TraderSale> { tradSaleDb },
                Name = $"Bookkeeping Approval for Journal entry #{jEntry.Number}",
                Qbicle = jEntry.WorkGroup.Qbicle,
                Topic = jEntry.WorkGroup.Topic,
                State = QbicleActivity.ActivityStateEnum.Open,
                StartedBy = user,
                StartedDate = DateTime.UtcNow,
                TimeLineDate = DateTime.UtcNow,
                Notes = jEntry.Description,
                IsVisibleInQbicleDashboard = true,
                ActivityMembers = jEntry.WorkGroup.Members,
                JournalEntries = new List<JournalEntry> { jEntry }
            };

            jEntry.Approval = approval;

            dbContext.SaveChanges();


            refModel.result = true;
            refModel.actionVal = 1;
            return refModel;

        }


        public ReturnJsonModel UpdateJournalEntry(JournalEntry jEntry, List<BKTransactionCustom> bKTransactions, UserSetting userSetting)
        {
            var refModel = new ReturnJsonModel();

            var user = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userSetting.Id);
            var mediaRules = new MediasRules(dbContext);
            var bkAccountRules = new BKCoANodesRule(dbContext);
            var dimensionRules = new TransactionDimensionRules(dbContext);
            var associatedAccounts = new List<BKAccount>();

            var jEntryCurrent = dbContext.JournalEntrys.FirstOrDefault(q => q.Id == jEntry.Id);
            jEntryCurrent.PostedDate = jEntry.PostedDate.ConvertTimeToUtc(userSetting.Timezone);
            jEntryCurrent.Description = jEntry.Description;
            jEntryCurrent.Group = dbContext.JournalGroups.Find(jEntry.Group.Id);
            jEntryCurrent.WorkGroup = dbContext.BKWorkGroups.Find(jEntry.WorkGroup.Id);
            jEntryCurrent.AssociatedAccounts.Clear();

            //verify this
            var lstIdCurrent = jEntryCurrent.BKTransactions.Select(q => q.Id).ToList();
            var lstIdUi = bKTransactions.Where(e => e.Id.Length < 36).Select(q => int.Parse(q.Id)).ToList();

            var bkTranIdDels = lstIdCurrent.Except(lstIdUi).ToList();
            foreach (var id in bkTranIdDels)
            {
                var tranDel = jEntryCurrent.BKTransactions.FirstOrDefault(e => e.Id == id);
                if (tranDel != null)
                    dbContext.BKTransactions.Remove(tranDel);
                dbContext.SaveChanges();
            }


            bKTransactions.ForEach(tran =>
            {
                if (tran.Id.Length == 36) // new bk transaction
                {
                    var transactionNew = new BKTransaction
                    {
                        Account = bkAccountRules.GetAccountById(tran.Account.Id),
                        Reference = tran.Reference,
                        PostedDate = tran.PostedDate.ConvertTimeToUtc(userSetting.Timezone),
                        Debit = tran.Debit,
                        Credit = tran.Credit,
                        Memo = tran.Memo,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Dimensions = new List<TransactionDimension>()
                    };
                    tran.Dimensions.ForEach(dm =>
                    {
                        transactionNew.Dimensions.Add(dimensionRules.GetById(dm.Id));
                    });
                    jEntryCurrent.BKTransactions.Add(transactionNew);

                    if (!jEntryCurrent.AssociatedAccounts.Any(a => a.Id == tran.Account.Id))
                        jEntryCurrent.AssociatedAccounts.Add(transactionNew.Account);

                    dbContext.SaveChanges();

                    if (tran.AssociatedFiles.Count > 0)
                        mediaRules.SaveOnReviewNewAttachmentsBKTransaction(transactionNew, tran.AssociatedFiles, user.Id);
                }

                else if (tran.Id.Length < 36) //update transaction
                {
                    var tranUpdate = jEntryCurrent.BKTransactions.FirstOrDefault(e => e.Id == int.Parse(tran.Id));

                    if (tranUpdate != null)
                    {
                        tranUpdate.Account = bkAccountRules.GetAccountById(tran.Account.Id);
                        tranUpdate.PostedDate = tran.PostedDate.ConvertTimeToUtc(userSetting.Timezone);
                        tranUpdate.Debit = tran.Debit;
                        tranUpdate.Credit = tran.Credit;
                        tranUpdate.Memo = tran.Memo;
                        tranUpdate.Reference = tran.Reference;

                        tranUpdate.Dimensions.Clear();
                        tran.Dimensions.ForEach(dm =>
                        {
                            tranUpdate.Dimensions.Add(dimensionRules.GetById(dm.Id));
                        });
                        if (!jEntryCurrent.AssociatedAccounts.Any(a => a.Id == tran.Account.Id))
                            jEntryCurrent.AssociatedAccounts.Add(tranUpdate.Account);

                        dbContext.SaveChanges();

                        if (tran.AssociatedFiles.Count > 0)
                        {
                            var newAttachments = tran.AssociatedFiles.Where(e => e.Id.Length == 36).ToList();
                            if (newAttachments.Count > 0)
                                mediaRules.SaveOnReviewNewAttachmentsBKTransaction(tranUpdate, newAttachments, user.Id);

                            var updateAttachment = tran.AssociatedFiles.Where(e => e.Id.Length < 36).ToList();
                            if (updateAttachment.Count > 0)
                                mediaRules.UpdateAttachmentsBkTransaction(updateAttachment);
                        }
                    }
                }

            });


            refModel.result = true;
            refModel.actionVal = 2;
            return refModel;
        }

    }
}

