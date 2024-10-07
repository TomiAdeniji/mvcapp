using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{

    public class BKCoANodesRule
    {
        ApplicationDbContext dbContext;

        public BKCoANodesRule(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public List<BKGroup> GetBKGroupByDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get BKGroup By Domain", null, null, domainId);

                var bkGroup = dbContext.BKGroups.Where(e => e.Domain.Id == domainId);
                if (bkGroup.Any() && bkGroup.FirstOrDefault(q => q.Number == null) != null)
                {
                    foreach (var item in bkGroup)
                    {
                        item.Number = ((int)item.AccountType).ToString();
                    }
                    dbContext.SaveChanges();
                }
                return bkGroup.ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<BKGroup>();
            }

        }

        public List<BKGroup> InitBKGroups(QbicleDomain domain, string userId)
        {
            var groups = new List<BKGroup>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Init BKGroups", userId, null, domain);

                var currentUser = dbContext.QbicleUser.Find(userId);

                #region Init if not exists Assets,Liabilities,Expenses,Equity,Revenue
                //Add Assets
                BKGroup groupAsset = new BKGroup();
                groupAsset.Domain = domain;
                groupAsset.Name = "Assets";
                groupAsset.Number = ((int)BKGroup.BKAccountTypeEnum.Assets).ToString();
                groupAsset.CreatedBy = currentUser;
                groupAsset.CreatedDate = DateTime.UtcNow;
                groupAsset.AccountType = BKGroup.BKAccountTypeEnum.Assets;
                groups.Add(groupAsset);
                //Add Liabilities
                BKGroup groupLiability = new BKGroup();
                groupLiability.Domain = domain;
                groupLiability.Name = "Liabilities";
                groupLiability.Number = ((int)BKGroup.BKAccountTypeEnum.Liabilities).ToString();
                groupLiability.CreatedBy = currentUser;
                groupLiability.CreatedDate = DateTime.UtcNow;
                groupLiability.AccountType = BKGroup.BKAccountTypeEnum.Liabilities;
                groups.Add(groupLiability);
                //Add Expenses
                BKGroup groupExpenses = new BKGroup();
                groupExpenses.Domain = domain;
                groupExpenses.Name = "Expenses";
                groupExpenses.Number = ((int)BKGroup.BKAccountTypeEnum.Expenses).ToString();
                groupExpenses.CreatedBy = currentUser;
                groupExpenses.CreatedDate = DateTime.UtcNow;
                groupExpenses.AccountType = BKGroup.BKAccountTypeEnum.Expenses;
                groups.Add(groupExpenses);
                //Add Equity
                BKGroup groupEquity = new BKGroup();
                groupEquity.Domain = domain;
                groupEquity.Name = "Equity";
                groupEquity.Number = ((int)BKGroup.BKAccountTypeEnum.Equity).ToString();
                groupEquity.CreatedBy = currentUser;
                groupEquity.CreatedDate = DateTime.UtcNow;
                groupEquity.AccountType = BKGroup.BKAccountTypeEnum.Equity;
                groups.Add(groupEquity);
                //Add Equity
                BKGroup groupRevenue = new BKGroup();
                groupRevenue.Domain = domain;
                groupRevenue.Name = "Revenue";
                groupRevenue.Number = ((int)BKGroup.BKAccountTypeEnum.Revenue).ToString();
                groupRevenue.CreatedBy = currentUser;
                groupRevenue.CreatedDate = DateTime.UtcNow;
                groupRevenue.AccountType = BKGroup.BKAccountTypeEnum.Revenue;
                groups.Add(groupRevenue);
                dbContext.BKGroups.AddRange(groups);
                dbContext.SaveChanges();

                #endregion
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, domain);
            }

            return groups;
        }

        public List<CoANode> GetBkGroupById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetBkGroupById", null, null, id);

                return new List<CoANode>() { dbContext.BKCoANodes.FirstOrDefault(q => q.Id == id) };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }

        }

        public BKAccount GetAccountById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetAccountById", null, null, id);

                var account = dbContext.BKAccounts.FirstOrDefault(q => q.Id == id);
                return account;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new BKAccount();
            }

        }

        public CoANode GetBKCoANodeById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetBKCoANodeById", null, null, id);

                return dbContext.BKCoANodes.Find(id);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public List<CoANode> GetChildBKCoANodeByParentId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetChildBKCoANodeByParentId", null, null, id);

                return dbContext.BKCoANodes.Where(s => s.Parent.Id == id).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public bool CheckExistName(BKSubGroup node)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CheckExistName", null, null, node);

                var nodes = dbContext.BKSubGroups.Where(q => q.Name == node.Name && q.Domain.Id == node.Domain.Id);
                if (nodes.Any() && node.Id > 0)
                    nodes = nodes.Where(q => q.Id != node.Id);
                if (nodes.Any() && node.Parent != null && node.Parent.Id > 0)
                    nodes = nodes.Where(q => q.Parent.Id == node.Parent.Id);
                if (nodes.Any()) return true;
                return false;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node);
                return false;
            }

        }

        public bool CheckExistsAccountCode(BKAccount account, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CheckExistsAccountCode", null, null, account, domainId);


                if (account.Id > 0)
                    return dbContext.BKAccounts.Any(x => (x.Id != account.Id && x.Code == account.Code && x.Domain.Id == domainId));
                else
                    return dbContext.BKAccounts.Any(x => (x.Code == account.Code && x.Domain.Id == domainId));

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, account, domainId);
                return false;
            }

        }

        public ReturnJsonModel SaveBKAccount(BKAccount account, QbicleDomain domain, string userId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save BKAccount", userId, null, account, domain);


                if (account != null && !string.IsNullOrEmpty(account.Name) && !string.IsNullOrEmpty(account.Number))
                {
                    if (account.WorkGroup == null || account.WorkGroup.Id == 0)
                    {
                        refModel.actionVal = 3;
                        refModel.msg = ResourcesManager._L("ERROR_MSG_168");
                        refModel.result = true;
                        return refModel;
                    }
                    if (CheckExistsAccountCode(account, domain.Id))
                    {
                        refModel.actionVal = 3;
                        refModel.msg = ResourcesManager._L("ERROR_MSG_627", account.Code);
                        refModel.msgId = account.Id.ToString();
                        refModel.msgName = account.Code;
                        refModel.result = true;
                        return refModel;
                    }

                    account.WorkGroup = dbContext.BKWorkGroups.Find(account.WorkGroup.Id);
                    if (account.Id > 0)
                    {
                        //var domain = account.Domain;

                        var bkAccountUpdate = dbContext.BKAccounts.FirstOrDefault(q => q.Id == account.Id);
                        if (bkAccountUpdate != null && !bkAccountUpdate.Transactions.Any())
                        {
                            var oldAccount = bkAccountUpdate;
                            bkAccountUpdate.WorkGroup = account.WorkGroup;
                            bkAccountUpdate.Name = account.Name;
                            bkAccountUpdate.Number = account.Number;
                            bkAccountUpdate.Code = account.Code;
                            bkAccountUpdate.Balance = account.Balance;
                            bkAccountUpdate.Debit = account.Debit;
                            bkAccountUpdate.Credit = account.Credit;
                            //add Initial Balance,Credit,Debit
                            bkAccountUpdate.InitialBalance = account.Balance.HasValue ? account.Balance.Value : 0;
                            bkAccountUpdate.InitialCredit = account.Credit.HasValue ? account.Credit.Value : 0;
                            bkAccountUpdate.InitialDebit = account.Debit.HasValue ? account.Debit.Value : 0;
                            //end
                            bkAccountUpdate.Description = account.Description;

                            if (dbContext.Entry(bkAccountUpdate).State == EntityState.Detached)
                                dbContext.BKAccounts.Attach(bkAccountUpdate);
                            dbContext.Entry(bkAccountUpdate).State = EntityState.Modified;
                            dbContext.SaveChanges();

                            UpdateCoANodeBalance(oldAccount.Parent, oldAccount.Debit * (-1) ?? 0, oldAccount.Credit * (-1) ?? 0);

                            UpdateCoANodeBalance(account.Parent, account.Debit ?? 0, account.Credit ?? 0);

                            refModel.actionVal = 2;
                            refModel.Object = GetListId(bkAccountUpdate, new List<int>());
                            refModel.Object2 = bkAccountUpdate;
                        }
                        else
                        {
                            refModel.actionVal = 3;
                            refModel.msg = ResourcesManager._L("ERROR_MSG_680");
                            refModel.result = true;
                        }
                    }
                    else
                    {
                        //add Initial Balance,Credit,Debit
                        account.InitialBalance = account.Balance.HasValue ? account.Balance.Value : 0;
                        account.InitialCredit = account.Credit.HasValue ? account.Credit.Value : 0;
                        account.InitialDebit = account.Debit.HasValue ? account.Debit.Value : 0;
                        //end
                        account.CreatedBy = dbContext.QbicleUser.Find(userId);
                        account.Domain = domain;
                        account.CreatedDate = DateTime.UtcNow;
                        account.Parent = dbContext.BKCoANodes.FirstOrDefault(q => q.Id == account.Parent.Id);
                        dbContext.BKAccounts.Add(account);
                        dbContext.Entry(account).State = EntityState.Added;
                        dbContext.SaveChanges();

                        UpdateCoANodeBalance(account.Parent, account.Debit ?? 0, account.Credit ?? 0);

                        refModel.actionVal = 1;
                        //append to select group
                        refModel.Object = GetListId(account, new List<int>());
                        refModel.Object2 = account;
                    }
                    refModel.result = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, account, domain);
                return null;
            }


            return refModel;
        }

        public List<int> GetListId(object node, List<int> ints)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BK Account GetListId", null, null, node, ints);

                if (node.GetType().Name == "BKGroup" || node.GetType().BaseType.Name == "BKGroup")
                {
                    var bkGroup = (BKGroup)node;
                    if (bkGroup.Parent != null)
                    {
                        ints = GetListId(bkGroup.Parent, ints);
                        ints.Add(bkGroup.Id);
                    }
                    else ints.Add(bkGroup.Id);
                }
                else if (node.GetType().Name == "BKSubGroup" || node.GetType().BaseType.Name == "BKSubGroup")
                {
                    var bKSubGroup = (BKSubGroup)node;
                    if (bKSubGroup.Parent != null)
                    {
                        ints = GetListId(bKSubGroup.Parent, ints);
                        ints.Add(bKSubGroup.Id);
                    }
                    else ints.Add(bKSubGroup.Id);
                }
                else if (node.GetType().Name == "BKAccount" || node.GetType().BaseType.Name == "BKAccount")
                {
                    var bKAccount = (BKAccount)node;
                    if (bKAccount.Parent != null)
                    {
                        ints = GetListId(bKAccount.Parent, ints);
                        ints.Add(bKAccount.Id);
                    }
                    else ints.Add(bKAccount.Id);
                }
                // Added by DJN
                // THi sis needed to deal with the problem in QBIC-775
                else if (node.GetType().Name == "CoANode" || node.GetType().BaseType.Name == "CoANode")
                {

                    var bkCoANode = (CoANode)node;
                    if (bkCoANode.NodeType == CoANode.BKCoANodeTypeEnum.SubGroup)
                    {
                        if (bkCoANode.Parent != null)
                        {
                            ints = GetListId(bkCoANode.Parent, ints);
                            ints.Add(bkCoANode.Id);
                        }
                        else ints.Add(bkCoANode.Id);
                    }

                }
                return ints;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node, ints);
                return ints;
            }

        }

        public ReturnJsonModel SaveSubGroup(BKSubGroup node, string userId)
        {
            var refModel = new ReturnJsonModel();

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BK Save SubGroup", null, null, node);


                if (node != null && !string.IsNullOrEmpty(node.Name))
                {
                    if (CheckExistName(node))
                    {
                        refModel.actionVal = 3;
                        refModel.msg = ResourcesManager._L("ERROR_MSG_632", node.Name);
                        refModel.msgId = node.Id.ToString();
                        refModel.msgName = node.Name;
                        refModel.result = true;
                        return refModel;
                    }

                    if (node.CreatedBy == null)
                        node.CreatedBy = dbContext.QbicleUser.Find(userId);

                    if (node.Id > 0)
                    {
                        var domain = node.Domain;

                        var _node = dbContext.BKSubGroups.FirstOrDefault(q => q.Id == node.Id);
                        _node.Name = node.Name;
                        _node.Number = node.Number;
                        if (dbContext.Entry(_node).State == EntityState.Detached)
                            dbContext.BKSubGroups.Attach(_node);
                        dbContext.Entry(_node).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        refModel.actionVal = 2;
                        refModel.Object = GetListId(_node, new List<int>());
                    }
                    else
                    {
                        node.CreatedDate = DateTime.UtcNow;
                        node.Parent = dbContext.BKCoANodes.FirstOrDefault(q => q.Id == node.Parent.Id);
                        node.AccountType = node.Parent != null ? node.Parent.AccountType : 0;
                        dbContext.BKSubGroups.Add(node);
                        dbContext.Entry(node).State = EntityState.Added;
                        dbContext.SaveChanges();
                        refModel.actionVal = 1;
                        //append to select group
                        refModel.msgId = node.Id.ToString();
                        refModel.msgName = node.Name;
                        refModel.Object = GetListId(node, new List<int>());
                    }
                    refModel.result = true;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node);
                return null;
            }
            return refModel;
        }

        /// <summary>
        /// Update CoANode Balance
        /// ∆Debit = tran.Debit
        /// ∆Credit = tran.Credit
        /// ∆Balance = tran.Credit - tran.Debit
        /// </summary>
        /// <param name="node">CoANode</param>
        /// <param name="debit">debit transaction</param>
        /// <param name="credit">credit transaction</param>
        public void UpdateCoANodeBalance(CoANode node, decimal debit, decimal credit)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateCoANodeBalance", null, null, node, debit, credit);

                if (node == null) return;
                var newBalance = debit - credit;

                node.Balance = (node.Balance ?? 0) + newBalance;
                node.Debit = (node.Debit ?? 0) + debit;
                node.Credit = (node.Credit ?? 0) + credit;
                dbContext.SaveChanges();
                while (node.Parent != null)
                {
                    node = node.Parent;
                    node.Balance = (node.Balance ?? 0) + newBalance;
                    node.Debit = (node.Debit ?? 0) + debit;
                    node.Credit = (node.Credit ?? 0) + credit;
                    dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node, debit, credit);
                return;
            }

        }
        public void UpdateCoANodeBalanceNotSaveChange(CoANode node, decimal debit, decimal credit)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateCoANodeBalance", null, null, node, debit, credit);

                if (node == null) return;
                var newBalance = debit - credit;

                node.Balance = (node.Balance ?? 0) + newBalance;
                node.Debit = (node.Debit ?? 0) + debit;
                node.Credit = (node.Credit ?? 0) + credit;
                while (node.Parent != null)
                {
                    node = node.Parent;
                    node.Balance = (node.Balance ?? 0) + newBalance;
                    node.Debit = (node.Debit ?? 0) + debit;
                    node.Credit = (node.Credit ?? 0) + credit;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, node, debit, credit);
                return;
            }

        }


        public List<BKAccountGroupModel> MapingAccountGroup(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Maping BKAccount Group", null, null, domainId);

                var accounts = dbContext.BKAccounts.Where(e => e.Domain.Id == domainId).ToList();
                var groups = dbContext.BKGroups.Where(e => e.Domain.Id == domainId).ToList();
                var subGroups = dbContext.BKSubGroups.Where(e => e.Domain.Id == domainId).ToList();

                var bkGroups = new List<BKAccountGroupModel>();
                bkGroups.AddRange(groups.Select(r => new BKAccountGroupModel { Id = r.Id, Name = r.Name }));
                bkGroups.AddRange(subGroups.Select(r => new BKAccountGroupModel { Id = r.Id, Name = r.Name }));

                foreach (var g in bkGroups)
                {
                    foreach (var a in accounts)
                    {
                        if (g.Id == a.Parent.Id)
                        {
                            g.Accounts.Add(new BKAccountModel
                            {
                                Id = a.Id,
                                Name = a.Name
                            });
                        }
                    }
                }

                return bkGroups.Where(e => e.Accounts.Count > 0).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }

        }

        /// <summary>
        /// Merge from accountIdFrom to accountIdTo
        /// And delete accountIdFrom
        /// </summary>
        /// <param name="accountMergeId">Merge from accountIdFrom</param>
        /// <param name="accountMasterId">to accountIdTo</param>
        /// <param name="accountMergeDetail">object contain list of data associated the account</param>
        /// <returns></returns>
        public ReturnJsonModel BKMergeAccount(int accountMergeId, int accountMasterId, object accountMergeDetail)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BKMergeAccount", null, null, accountMergeId, accountMasterId, accountMergeDetail);



                var accounts = dbContext.BKAccounts.Where(e => e.Id == accountMergeId || e.Id == accountMasterId);

                var mergeAccount = accounts.FirstOrDefault(e => e.Id == accountMergeId);
                var masterAccount = accounts.FirstOrDefault(e => e.Id == accountMasterId);


                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (masterAccount == null || mergeAccount == null)
                        {
                            refModel.result = false;
                            refModel.msg = ResourcesManager._L("ERROR_MSG_640");
                            return refModel;
                        }

                        //Updates all the Transactions in the Merge Account to set their Account property to the Master Account
                        mergeAccount.Transactions.ForEach(t => t.Account = masterAccount);

                        //Updates all the JournalEntries in the Merge Account to set their Account property to the Master Account
                        mergeAccount.JournalEntries.ForEach(j =>
                        {
                            for (var i = 0; i < j.AssociatedAccounts.Count; i++)
                            {
                                if (j.AssociatedAccounts[i].Id == mergeAccount.Id)
                                    j.AssociatedAccounts[i] = masterAccount;
                            }
                            //j.BKTransactions.ForEach(b =>
                            //{
                            //    if (b.Account == mergeAccount)
                            //        b.Account = masterAccount;
                            //});
                        });

                        //Update media
                        mergeAccount.AssociatedFiles.ForEach(m => m.BKAccount = masterAccount);

                        // update all associated
                        var associated = (AccountAssociated)accountMergeDetail;
                        //TaxRates
                        associated.TaxAccounts.ForEach(t =>
                        {
                            var taxRare = dbContext.TaxRates.Find(t.Id);
                            if (taxRare?.AssociatedAccount.Id == accountMergeId)
                                taxRare.AssociatedAccount = masterAccount;
                        });
                        // update sale
                        associated.SaleAccounts.ForEach(s =>
                        {
                            var sale = dbContext.TraderItems.Find(s.Id);
                            if (sale?.SalesAccount.Id == accountMergeId)
                                sale.SalesAccount = masterAccount;
                        });
                        // update purchase
                        associated.PurchaseAccounts.ForEach(p =>
                        {
                            var purchase = dbContext.TraderItems.Find(p.Id);
                            if (purchase?.PurchaseAccount.Id == accountMergeId)
                                purchase.PurchaseAccount = masterAccount;
                        });
                        // update InventoryAccount
                        associated.InventoryAccount.ForEach(i =>
                        {
                            var inventory = dbContext.TraderItems.Find(i.Id);
                            if (inventory?.InventoryAccount.Id == accountMergeId)
                                inventory.InventoryAccount = masterAccount;
                        });
                        // update CashAccount
                        associated.CashAccount.ForEach(c =>
                        {
                            var cash = dbContext.TraderCashAccounts.Find(c.Id);
                            if (cash?.AssociatedBKAccount.Id == accountMergeId)
                                cash.AssociatedBKAccount = masterAccount;
                        });
                        // update ContactAccount
                        associated.ContactAccount.ForEach(c =>
                        {
                            var cash = dbContext.TraderContacts.Find(c.Id);
                            if (cash?.CustomerAccount.Id == accountMergeId)
                                cash.CustomerAccount = masterAccount;
                        });

                        //Find the parent node of the Merge Account
                        //Update the Balance, Debit and Credit totals for the Parent Node of the Merge Account
                        var node = mergeAccount.Parent;
                        var credit = mergeAccount.Credit * (-1) ?? 0;
                        var debit = mergeAccount.Debit * (-1) ?? 0;
                        UpdateCoANodeBalance(node, debit, credit);


                        //Update the Update the Balance, Debit and Credit totals for the Master Account (and parents)
                        masterAccount.Balance += mergeAccount.Balance;
                        masterAccount.Debit += mergeAccount.Debit;
                        masterAccount.Credit += mergeAccount.Credit;

                        node = masterAccount.Parent;
                        credit = masterAccount.Credit ?? 0;
                        debit = masterAccount.Debit ?? 0;
                        UpdateCoANodeBalance(node, debit, credit);


                        //Delete Merge Account
                        dbContext.BKAccounts.Remove(mergeAccount);

                        dbContext.SaveChanges();

                        transaction.Commit();
                        refModel.Object = GetListId(masterAccount, new List<int>());
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        refModel.result = false;
                        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountMergeId, accountMasterId, accountMergeDetail);
                    }
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountMergeId, accountMasterId, accountMergeDetail);
            }

            return refModel;
        }

        public ReturnJsonModel BKDeleteAccount(int accountId)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BKDeleteAccount", null, null, accountId);

                var bkAccount = dbContext.BKAccounts.Find(accountId);
                if (bkAccount == null)
                {
                    return refModel;
                }


                if (bkAccount.Transactions.Any() || bkAccount.JournalEntries.Any())
                {
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_641");
                    return refModel;
                }
                UpdateCoANodeBalance(bkAccount.Parent, bkAccount.Debit * (-1) ?? 0, bkAccount.Credit * (-1) ?? 0);

                var listId = GetListId(bkAccount, new List<int>());

                refModel.Object = listId.Where(e => e != bkAccount.Id);

                if (bkAccount.AssociatedFiles.Any())
                {
                    bkAccount.AssociatedFiles.ForEach(file =>
                    {
                        dbContext.VersionedFiles.RemoveRange(file.VersionedFiles);
                    });
                    bkAccount.AssociatedFiles.Clear();
                }

                dbContext.BKAccounts.Remove(bkAccount);
                dbContext.SaveChanges();


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountId);
                refModel.result = false;
            }
            return refModel;
        }

        public ReturnJsonModel BKDeleteSubGroup(int groupId)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BKDeleteSubGroup", null, null, groupId);

                if (AlreadyTransaction(groupId))
                {
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_641");
                    return refModel;
                }
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var bkGroup = dbContext.BKSubGroups.Find(groupId);
                        if (bkGroup == null) return refModel;

                        if (bkGroup.Children.Any())
                        {
                            while (bkGroup.Children.Any())
                            {
                                refModel = DeleteNode(bkGroup.Children[0].Id);
                                if (!refModel.result)
                                {
                                    transaction.Rollback();
                                    return refModel;
                                }
                            }
                        }
                        var listId = GetListId(bkGroup, new List<int>());

                        refModel.Object = listId.Where(e => e != bkGroup.Id);

                        var incomeReportEntries =
                            dbContext.BKIncomeReportEntries.Where(e => e.CoANode.Id == bkGroup.Id);
                        if (incomeReportEntries.Any())
                            dbContext.BKIncomeReportEntries.RemoveRange(incomeReportEntries);

                        dbContext.BKSubGroups.Remove(bkGroup);
                        dbContext.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {

                        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, groupId);
                        transaction.Rollback();
                        //refModel.msg = ex.Message;
                        refModel.result = false;
                        refModel.actionVal = 3;
                    }
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, groupId);
                return null;
            }

            return refModel;
        }

        public bool AlreadyTransaction(int nodeId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "AlreadyTransaction", null, null, nodeId);

                var node = dbContext.BKCoANodes.Find(nodeId);
                if (node is BKAccount)
                {
                    if (((BKAccount)node).Transactions.Any())
                    {
                        return true;
                    }
                }
                else if (node is BKSubGroup)
                {
                    if (((BKSubGroup)node).Children.Any())
                    {
                        foreach (var subG in ((BKSubGroup)node).Children)
                        {
                            if (AlreadyTransaction(subG.Id))
                            {
                                return true;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, nodeId);
                return false;
            }

            return false;
        }

        public bool AlReadyAccount(int nodeId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "AlReadyAccount", null, null, nodeId);


                var node = dbContext.BKCoANodes.Find(nodeId);
                if (node is BKAccount)
                {
                    return true;
                }
                else if (node is BKSubGroup)
                {
                    if (((BKSubGroup)node).Children.Any())
                    {
                        foreach (var subG in ((BKSubGroup)node).Children)
                        {
                            if (AlReadyAccount(subG.Id))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, nodeId);
                return false;
            }
        }

        public ReturnJsonModel DeleteNode(int id)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeleteNode", null, null, id);

                var node = dbContext.BKCoANodes.Find(id);
                if (node is BKAccount)
                {
                    refModel = BKDeleteAccount(id);
                }
                else if (node is BKSubGroup)
                {
                    var bkGroup = dbContext.BKSubGroups.Find(id);
                    if (bkGroup == null) return refModel;

                    if (bkGroup.Children.Any())
                    {
                        while (bkGroup.Children.Any())
                        {
                            refModel = DeleteNode(bkGroup.Children[0].Id);
                            if (!refModel.result)
                            {
                                return refModel;
                            }
                        }
                    }
                    var incomeReportEntries =
                        dbContext.BKIncomeReportEntries.Where(e => e.CoANode.Id == bkGroup.Id);
                    if (incomeReportEntries.Any())
                        dbContext.BKIncomeReportEntries.RemoveRange(incomeReportEntries);

                    dbContext.BKSubGroups.Remove(bkGroup);
                    dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }


            return refModel;
        }

        /// <summary>
        /// Get list of account, exclude accountMasterId
        /// </summary>
        /// <param name="domainId"></param>
        /// <param name="accountMasterId"></param>
        /// <returns></returns>
        public List<BKAccount> GetListMergeAccount(int domainId, int accountMasterId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetListMergeAccount", null, null, domainId, accountMasterId);

                return dbContext.BKAccounts.Where(e => e.Domain.Id == domainId && e.Id != accountMasterId).ToList();


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, accountMasterId);
                return null;
            }
        }


        public ReturnJsonModel GetAccountAssociatedDetail(int accountId, int domainId)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetAccountAssociatedDetail", null, null, accountId, domainId);

                var bkAccount = dbContext.BKAccounts.Find(accountId);
                if (bkAccount == null) return refModel;
                //

                var journalCount = bkAccount.JournalEntries.Count;
                var transactionCount = bkAccount.Transactions.Count;

                var taxAccount = dbContext.TaxRates.Where(e => e.AssociatedAccount.Id == accountId && e.Domain.Id == domainId).ToList();
                var saleAccount = dbContext.TraderItems.Where(e => e.SalesAccount.Id == accountId && e.Domain.Id == domainId).ToList();
                var purchaseAccount = dbContext.TraderItems.Where(e => e.PurchaseAccount.Id == accountId && e.Domain.Id == domainId).ToList();
                var inventoryAccount = dbContext.TraderItems.Where(e => e.InventoryAccount.Id == accountId && e.Domain.Id == domainId).ToList();
                var cashAccount = dbContext.TraderCashAccounts.Where(e => e.AssociatedBKAccount.Id == accountId && e.Domain.Id == domainId).ToList();
                var contactAccount = dbContext.TraderContacts.Where(e => e.CustomerAccount.Id == accountId && e.ContactGroup.Domain.Id == domainId).ToList();

                var traderLinkCount = saleAccount.Count() + purchaseAccount.Count() + inventoryAccount.Count() + cashAccount.Count() + contactAccount.Count();

                var detailHtml = $"<li><strong>{journalCount}</strong> Journal entries</li>";
                detailHtml += $"<li><strong>{transactionCount}</strong> Transactions</li>";
                detailHtml += $"<li><strong>{taxAccount.Count()}</strong> Tax rates</li>";
                detailHtml += $"<li><strong>{bkAccount.AssociatedFiles.Count}</strong> Qbicles media</li>";
                detailHtml += $"<li><strong>{traderLinkCount}</strong> Trader links</li>";


                refModel.Object = new AccountAssociated
                {
                    TaxAccounts = taxAccount,
                    SaleAccounts = saleAccount,
                    PurchaseAccounts = purchaseAccount,
                    InventoryAccount = inventoryAccount,
                    CashAccount = cashAccount,
                    ContactAccount = contactAccount
                };

                refModel.msg = detailHtml;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountId, domainId);
            }

            return refModel;
        }

        public ReturnJsonModel CloseBookSave(string userId, int domainId, string closureDate, string format)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BK Close Book Save", userId, null, userId, domainId, closureDate, format);


                var closeBook = new BookClosure
                {
                    ClosureDate = closureDate.ConvertDateFormat(format),
                    CreatedBy = new UserRules(dbContext).GetById(userId),
                    CreatedDate = DateTime.UtcNow,
                    Domain = new DomainRules(dbContext).GetDomainById(domainId)
                };
                dbContext.BookClosures.Add(closeBook);
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, domainId, closureDate, format);
                refModel.result = false;
            }

            return refModel;
        }

        public BookClosure CloseBook(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CloseBook", null, null, domainId);

                return dbContext.BookClosures.OrderByDescending(o => o.ClosureDate).FirstOrDefault(e => e.Domain.Id == domainId);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }
        }

        public ReturnJsonModel VerifyJournalPostedDate(string journalPostedDate, string bkPostedDates, string formatDate)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "VerifyJournalPostedDate", null, null, journalPostedDate, bkPostedDates, formatDate);

                var dates = bkPostedDates.Split(',');

                var transactionsDate = new List<DateTime>();
                dates.ForEach(d =>
                {
                    transactionsDate.Add(d.ConvertDateFormat(formatDate));
                });
                var latest = transactionsDate.Max(record => record);
                var jPostedDate = DateTime.ParseExact(journalPostedDate, formatDate, CultureInfo.InvariantCulture);
                if (jPostedDate < latest)
                {
                    refModel.result = false;
                    refModel.msg = latest.ToString(formatDate);
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, journalPostedDate, bkPostedDates, formatDate);
                refModel.result = false;
            }

            return refModel;
        }

        public List<int> GetBkAccountNodesSelected(int accountId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetBkAccountNodesSelected", null, null, accountId);


                var account = dbContext.BKAccounts.Find(accountId);
                return GetListId(account, new List<int>());

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountId);
                return new List<int>();
            }
        }


        public void UpdateNodeBalanceFromJournal(JournalEntry journal)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update Node Balance From Journal", null, null, journal);


                foreach (var tran in journal.BKTransactions)
                    UpdateCoANodeBalanceNotSaveChange(tran.Account, tran.Debit ?? 0, tran.Credit ?? 0);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, journal);
            }
        }
    }
}