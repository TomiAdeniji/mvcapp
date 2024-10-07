using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class AccountPackageRules
    {
        ApplicationDbContext _db;
        public AccountPackageRules()
        {

        }
        public AccountPackageRules(ApplicationDbContext context)
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


        public List<AccountPackage> GetAllAccountPackage()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get all account package", null, null);

                var listAcc = new List<AccountPackage>();
                listAcc = DbContext.AccountPackages.ToList();
                return listAcc;
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<AccountPackage>();
            }
            
        }

        public AccountPackage GetAccountPackage(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get account package", null, null, id);

                return DbContext.AccountPackages.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public ReturnJsonModel ChangeToPackage(int idPackage, int accountId, string changeByUserId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false, msg = "Error" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Change to package", changeByUserId, null, idPackage, accountId, changeByUserId);
                
                var userChange = new UserRules(_db).GetUser(changeByUserId, 0);
                var account = new AccountRules(_db).GetAccountById(accountId);
                if (account.Administrators.Any(x => x == userChange))
                {
                    var package = GetAccountPackage(idPackage);
                    // check number domains, users, qbicles or guests
                    if (package.NumberOfDomains == null || (package.NumberOfDomains >= account.Domains.Count()))
                    {
                        if (package.NumberOfQbicles == null || (package.NumberOfQbicles >= account.QbiclesCount))
                        {
                            if (package.NumberOfUsers == null || (package.NumberOfUsers >= account.Users.Count()))
                            {
                                if (package.NumberOfGuests == null )
                                {
                                    account.AccountPackage = package;
                                    _db.SaveChanges();
                                    refModel.result = true;
                                    refModel.Object = new { package.Id, AccessLevel = package.AccessLevel + " package", Cost = "₦" + ((package.Cost == 0 ? "0" : package.Cost.ToString("#,###.##", System.Globalization.CultureInfo.InvariantCulture.NumberFormat)) + "/" + HelperClass.EnumModel.GetDescriptionFromEnumValue(package.PerTimes)) };
                                    refModel.msg = "Success";
                                }
                                else
                                {
                                    refModel.msg = ResourcesManager._L("ERROR_MSG_24", package.NumberOfGuests);
                                }
                            }
                            else
                            {
                                refModel.msg = ResourcesManager._L("ERROR_MSG_25", account.Users.Count() - package.NumberOfUsers);
                            }
                        }
                        else
                        {
                            refModel.msg = ResourcesManager._L("ERROR_MSG_26", account.QbiclesCount - package.NumberOfQbicles);
                        }
                    }
                    else
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_27",  account.Domains.Count() - package.NumberOfDomains); 
                    }
                }
                else
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_28");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, changeByUserId, idPackage, accountId, changeByUserId);
                refModel.msg = ex.Message.ToString();
            }
            return refModel;
        }
    }
}
