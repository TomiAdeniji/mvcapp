using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class AccountRules
    {
        ApplicationDbContext dbContext;

        public AccountRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public SubscriptionAccount GetAccountByOwner(string ownerId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get account by owner", null, null, ownerId);

                return dbContext.SubscriptionAccounts.First(o => o.AccountOwner.Id == ownerId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, ownerId);
                return null;
            }

        }
        public bool SaveAccount(CreateAccountMain acc, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save account", userId, null, acc);

                var accountPackage = new AccountPackageRules(dbContext).GetAccountPackage(acc.AccountPakgeId);

                var user = new UserRules(dbContext).GetUser(userId, 0);
                //Create new Account
                var account = new SubscriptionAccount()
                {
                    AccountName = acc.AccountName,
                    AccountCreator = user,
                    AccountOwner = user,
                    CompanyOrganisationName = acc.Company,
                    AccountPackage = accountPackage,
                };
                account.Administrators.Add(user);
                dbContext.SubscriptionAccounts.Add(account);
                dbContext.Entry(account).State = EntityState.Added;

                return dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, acc);
                return false;

            }
        }
        /// <summary>
        /// Check exist info register
        /// </summary>
        /// <param name="displayUserName">Display User Name</param>
        /// <param name="email">email</param>
        /// <param name="accountName">account name</param>
        /// <returns>-1: error check/ 1: duplicated user name/ 2: duplicated email/ 3: Duplicated account name</returns>
        public ReturnJsonModel DuplicateCheck(string displayUserName, string email, string accountName)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Duplicate check", null, null, displayUserName, email, accountName);

                if (dbContext.QbicleUser.Any(x => x.DisplayUserName == displayUserName))
                    return new ReturnJsonModel { msgId = "1", actionVal = 1, result = false, msg = ResourcesManager._L("ERROR_DISPLAYNAME_EXISTED") };
                if (dbContext.QbicleUser.Any(x => x.Email == email))
                    return new ReturnJsonModel { msgId = "2", actionVal = 2, result = false, msg = ResourcesManager._L("ERROR_EMAIL_EXISTED") };
                return new ReturnJsonModel { msgId = "0", actionVal = 0, result = true };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, displayUserName, email, accountName);
                return new ReturnJsonModel { msgId = "-1", actionVal = -1, result = false };
            }
        }

        public bool IsDuplicateAccountName(string accountName)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate account name", null, null, accountName);

                return dbContext.SubscriptionAccounts.Any(x => x.AccountName == accountName);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, accountName);
                return false;
            }

        }

        public SubscriptionAccount GetAccountById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get account by id", null, null, id);

                return dbContext.SubscriptionAccounts.FirstOrDefault(x => x.Id == id);
            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public ReturnJsonModel ChangeNameAccount(int accountId, string accountName, string userId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_300") };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Change account name", userId, null, accountId, accountName);

                var acc = GetAccountById(accountId);
                if (acc != null)
                {
                    if (acc.Administrators.Any(x => x.Id == userId))
                    {
                        if (IsDuplicateAccountName(accountName) == false)
                        {

                            acc.AccountName = accountName;
                            dbContext.SaveChanges();
                            refModel.msg = ResourcesManager._L("ERROR_MSG_656");
                            refModel.result = true;
                        }
                        else
                        {
                            refModel.msg = ResourcesManager._L("ERROR_MSG_302");
                        }
                    }
                    else
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_303");
                    }
                }
                else
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_304");

                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, accountId, accountName);
            }

            return refModel;
        }

        public ReturnJsonModel RemoveAccountAdmin(string userId, SubscriptionAccount account, string currentUserId)
        {
            var refModal = new ReturnJsonModel()
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Remove account admin", null, null, userId, account);

                if (account.Administrators.Any(x => x.Id == currentUserId))
                {
                    var account2 = new AccountRules(dbContext).GetAccountById(account.Id);
                    var user = new UserRules(dbContext).GetUser(userId, 0);
                    //fix lazy loading
                    var create = account2.AccountCreator;
                    var owner = account2.AccountOwner;
                    //end fix
                    account2.Administrators.Remove(user);
                    dbContext.SubscriptionAccounts.Attach(account2);
                    dbContext.Entry(account2).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModal.result = true;
                }
                else
                {
                    refModal.msg = ResourcesManager._L("ERROR_MSG_305");
                }
            }
            catch (Exception ex)
            {
                refModal.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, account);
            }
            return refModal;
        }

        public ReturnJsonModel AddAccountAdmin(string userId, SubscriptionAccount account, string currentUserId)
        {
            var refModal = new ReturnJsonModel()
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add admin account", null, null, userId, account);

                if (account.Administrators.Any(x => x.Id == currentUserId))
                {
                    var user = new UserRules(dbContext).GetUser(userId, 0);
                    var account2 = new AccountRules(dbContext).GetAccountById(account.Id);
                    // fix lazy loading
                    var creater = account2.AccountCreator;
                    var owner = account2.AccountOwner;
                    // end fix
                    account2.Administrators.Add(user);
                    dbContext.SubscriptionAccounts.Attach(account2);
                    dbContext.Entry(account2).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModal.result = true;
                    refModal.Object = user;
                }
                else
                {
                    refModal.msg = refModal.msg = ResourcesManager._L("ERROR_MSG_305");
                }
            }
            catch (Exception ex)
            {
                refModal.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, account);
            }
            return refModal;
        }

        public ReturnJsonModel SetAsAccountOwner(string userId, SubscriptionAccount account, string currentUserId)
        {
            var refModal = new ReturnJsonModel()
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Set account as owner", null, null, userId, account);

                if (account.Administrators.Any(x => x.Id == currentUserId) == true)
                {
                    var user = new UserRules(dbContext).GetUser(userId, 0);
                    var account2 = new AccountRules(dbContext).GetAccountById(account.Id);
                    account2.AccountOwner = user;
                    account2.AccountCreator = user;
                    dbContext.SubscriptionAccounts.Attach(account2);
                    dbContext.Entry(account2).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModal.result = true;
                    refModal.Object = user;
                }
                else
                {
                    refModal.msg = refModal.msg = ResourcesManager._L("ERROR_MSG_305");
                }
            }
            catch (Exception ex)
            {
                refModal.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, account);
            }
            return refModal;
        }

        public bool IsValidNumberOfDomain(SubscriptionAccount account)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Is valid number of domain", null, null, account);

                bool result = account.AccountPackage.NumberOfDomains > account.Domains.Count();
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, account);
                return false;
            }

        }


        public List<AdministratorViewModal> ListAdministrator(QbicleDomain currentDomain, string currentUserId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "List administrator", currentUserId, null, currentDomain);

                var listAdmin = new List<AdministratorViewModal>();

                var account = currentDomain.Account;
                listAdmin.Add(HelperClass.ConvertUserToAdminViewModal(account.AccountOwner, AdministratorViewModal.AccountOwner));
                listAdmin.AddRange(HelperClass.ConvertListUserToListAdminViewModel(account, AdministratorViewModal.AccountAdministrators, currentUserId));
                foreach (var item in account.Domains)
                {
                    listAdmin.AddRange(HelperClass.ConvertListUserToListAdminViewModel(item, AdministratorViewModal.DomainAdministrator, currentUserId));
                }
                return listAdmin;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, currentDomain);
                return new List<AdministratorViewModal>();
            }

        }

        /// <summary>
        /// Verify user
        /// if user null retutrn 0
        /// if user.EmailConfirmed= true return 1
        /// if user.EmailConfirmed= false return 2
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public ReturnJsonModel VerifyStep(string email)
        {
            var refModel = new ReturnJsonModel();
            var user = dbContext.QbicleUser.FirstOrDefault(e => e.Email == email);
            if (user == null)
                refModel.actionVal = 0;

            if (user.EmailConfirmed)
                refModel.actionVal = 1;
            else
                refModel.actionVal = 2;

            return refModel;
        }

    }
}
