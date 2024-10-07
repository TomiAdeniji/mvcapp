using Newtonsoft.Json;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Pricing;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.Trader.Settings;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Model.TB_Column;


namespace Qbicles.BusinessRules.Trader
{
    public class TraderGroupRules
    {
        private ApplicationDbContext _db;

        public TraderGroupRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }


        public List<TraderGroup> GetTraderGroupItem(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.TraderGroups.Where(d => d.Domain.Id == domainId).OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderGroup>();
            }
        }



        public List<TraderGroup> GetTraderGroupItemOnly(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.TraderGroups.AsNoTracking().Where(d => d.Domain.Id == domainId).OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderGroup>();
            }
        }

        public List<TraderGroup> GetTraderGroupItemByLocation(int domainId, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, locationId);
                var groups = DbContext.TraderGroups.Where(d => d.Domain.Id == domainId && d.Items.Any(e => e.IsActiveInAllLocations || e.Locations.Any(l => l.Id == locationId))).ToList();

                return groups;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, locationId);
                return new List<TraderGroup>();
            }
        }

        public bool CheckExistName(TraderGroup group)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, group);
                var gName = group.Name.Trim();
                if (group.Id > 0)
                    return DbContext.TraderGroups.Any(x =>
                        x.Id != group.Id && x.Domain.Id == group.Domain.Id && x.Name.Equals(gName, StringComparison.OrdinalIgnoreCase));
                return DbContext.TraderGroups.Any(x => x.Name.Equals(gName,StringComparison.OrdinalIgnoreCase) && x.Domain.Id == group.Domain.Id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, group);
                return false;
            }
        }

        public object GetOnlyById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var group = DbContext.TraderGroups.Find(id);
                if (group != null)
                    return new BaseItemModel
                    {
                        Id = group.Id,
                        Name = group.Name,
                        CreatedBy = HelperClass.GetFullNameOfUser(group.CreatedBy),
                        CreatedDate = group.CreatedDate.ToString("dd/MM/yyyy")
                    };
                return null;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return null;
            }
        }

        public ReturnJsonModel SaveGroup(TraderGroup group, string userId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, group);
                if (group == null || string.IsNullOrEmpty(group.Name)) return refModel;
                if (group.Id > 0)
                {
                    var updateLocation = DbContext.TraderGroups.FirstOrDefault(q => q.Id == group.Id);
                    if (updateLocation != null)
                    {
                        updateLocation.Name = group.Name;
                        if (DbContext.Entry(updateLocation).State == EntityState.Detached)
                            DbContext.TraderGroups.Attach(updateLocation);
                        DbContext.Entry(updateLocation).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        refModel.actionVal = 2;
                        refModel.msg = updateLocation.Name;
                        refModel.msgId = updateLocation.Id.ToString();
                        refModel.msgName = updateLocation.Name;
                    }
                }
                else
                {

                    group.CreatedBy = DbContext.QbicleUser.Find(userId);
                    group.CreatedDate = DateTime.UtcNow;
                    DbContext.TraderGroups.Add(group);
                    DbContext.Entry(group).State = EntityState.Added;
                    DbContext.SaveChanges();
                    refModel.actionVal = 1;
                    //append to select group
                    refModel.msgId = group.Id.ToString();
                    refModel.msgName = group.Name;
                }

                refModel.result = true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, group);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = e.Message;
            }

            return refModel;
        }

        public bool DeleteGroup(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var group = GetById(id);
                var domainId = group.Domain.Id;
                DbContext.TraderGroups.Remove(group);
                DbContext.SaveChanges();

                if (DbContext.TraderGroups.Any(d => d.Domain.Id == domainId))
                    return true;
                var traderSeting = DbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == domainId);
                if (traderSeting == null) return true;
                traderSeting.IsSetupCompleted = TraderSetupCurrent.ProductGroup;
                DbContext.Entry(traderSeting).State = EntityState.Modified;
                DbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public TraderGroup GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.TraderGroups.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderGroup();
            }
        }

        public List<TraderGroup> GetByIds(IEnumerable<int> idsList)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, idsList);
                return DbContext.TraderGroups.Where(e => idsList.Contains(e.Id)).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, idsList);
                return new List<TraderGroup>();
            }
        }

        public List<TraderGroup> GetNotInIds(IEnumerable<int> idsList, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, idsList, domainId);
                return DbContext.TraderGroups.Where(e => !idsList.Contains(e.Id) && e.Domain.Id == domainId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, idsList, domainId);
                return new List<TraderGroup>();
            }
        }
        public DataTablesResponse ItemsTraderGroupMaster([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int groupid, string keyword, int type)
        {
            try
            {
                var query = DbContext.TraderItems.Where(t => t.Group.Id == groupid).AsQueryable();
                #region Filters
                if (!String.IsNullOrEmpty(keyword))
                {
                    query = query.Where(t => t.Name.Trim().ToLower().Contains(keyword.Trim().ToLower()));
                }
                if (type == 1)//Item I buy
                {
                    query = query.Where(t => t.IsBought && !t.IsSold && t.InventoryDetails.Count > 0);
                }
                else if (type == 2)//Item I buy & sell
                {
                    query = query.Where(t => t.IsBought && t.IsSold && t.InventoryDetails.Count > 0);

                }
                else if (type == 3)//Item I sell (compound)
                {
                    query = query.Where(t => !t.IsBought && t.IsSold && t.IsCompoundProduct);
                }
                else if (type == 4)//Item I sell (service)
                {
                    query = query.Where(t => !t.IsBought && t.IsSold && !t.IsCompoundProduct && t.InventoryDetails.Count == 0);
                }
                int totalRecords = query.Count();
                #endregion

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Item":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Type":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "IsBought" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ",IsSold" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ",IsCompoundProduct" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Taxes":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TaxRates.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Accounts":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "PurchaseAccount.Code" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ",SalesAccount.Code" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ",InventoryAccount.Code" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Name asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Item = q.Name,
                    Type = htmlColumnType(q),
                    Taxes = htmlColumnTaxrates(q),
                    Accounts = htmlColumnAccounts(q)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        private string htmlColumnTaxrates(TraderItem item)
        {
            if (item.TaxRates != null && item.TaxRates.Any())
            {
                var salestax = item.TaxRates.Where(s => !s.IsPurchaseTax).ToList();
                var purchasetax = item.TaxRates.Where(s => s.IsPurchaseTax).ToList();
                string _salestax = salestax != null && salestax.Any() ? string.Join(Environment.NewLine, salestax.Select(s => $"<li>Sales Tax: {s.Name}  (Linked Account: {s.AssociatedAccount?.Name ?? "No account"})</li>")) : "";
                string _purchasetax = purchasetax != null && purchasetax.Any() ? string.Join(Environment.NewLine, purchasetax.Select(s => $"<li>Purchase Tax: {s.Name}  (Linked Account: {s.AssociatedAccount?.Name ?? "No account"})</li>")) : "";
                return "<ul style=\"padding-left: 15px;\">" + _salestax + _purchasetax + "</ul>";
            }
            else
                return "";

        }
        private string htmlColumnType(TraderItem item)
        {
            string type = "";
            if (item.IsBought && !item.IsSold)//Item I buy
            {
                type = "Item I buy";
            }
            else if (item.IsBought && item.IsSold)//Item I buy & sell
            {
                type = "Item I buy & sell";
            }
            else if (!item.IsBought && item.IsSold)//Item I sell (compound)
            {
                type = "Item I sell (compound)";
            }
            else if (!item.IsBought && item.IsSold && !item.IsCompoundProduct)//Item I sell (service)
            {
                type = "Item I sell (service)";
            }
            return type;
        }
        private string htmlColumnAccounts(TraderItem item)
        {
            string account = "<ul style=\"padding-left: 15px;\">";
            if (item.IsBought && !item.IsSold && item.InventoryDetails.Count > 0)//Item I buy
            {
                if (item.PurchaseAccount != null)
                    account += $"<li>Purchase account: {item.PurchaseAccount.Name}</li>";
                if (item.InventoryAccount != null)
                    account += $"<li>Inventory account: {item.InventoryAccount.Name}</li>";
            }
            else if (item.IsBought && item.IsSold && item.InventoryDetails.Count > 0)//Item I buy & sell
            {
                if (item.PurchaseAccount != null)
                    account += $"<li>Purchase Account: {item.PurchaseAccount.Name}</li>";
                if (item.SalesAccount != null)
                    account += $"<li>Sales account: {item.SalesAccount.Name}</li>";
                if (item.InventoryAccount != null)
                    account += $"<li>Inventory Account: {item.InventoryAccount.Name}</li>";
            }
            else if (!item.IsBought && item.IsSold && item.IsCompoundProduct)//Item I sell (compound)
            {
                if (item.SalesAccount != null)
                    account += $"<li>Sales account: {item.SalesAccount.Name}</li>";
                if (item.InventoryAccount != null)
                    account += $"<li>Inventory account: {item.InventoryAccount.Name}</li>";
            }
            else if (!item.IsBought && item.IsSold && !item.IsCompoundProduct && item.InventoryDetails.Count == 0)//Item I sell (service)
            {
                if (item.SalesAccount != null)
                    account += $"<li>Sales account: {item.SalesAccount.Name}</li>";
                if (item.InventoryAccount != null)
                    account += $"<li>Inventory account: {item.InventoryAccount.Name}</li>";
            }
            account += "</ul>";
            return account;
        }
        public List<MasterSetup> GetMasterSetup(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.MasterSetups.Where(e => e.Domain.Id == domainId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<MasterSetup>();
            }
        }
        public MasterSetup GetMasterSetupByGroupId(int groupid, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, groupid);
                return DbContext.MasterSetups.FirstOrDefault(e => e.Domain.Id == domainId && e.TraderGroup.Id == groupid);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, groupid);
                return new MasterSetup();
            }
        }
        public void SaveMasterSetup(int groupid, int domainId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, groupid);
                var master = DbContext.MasterSetups.FirstOrDefault(e => e.Domain.Id == domainId && e.TraderGroup.Id == groupid);
                var group = DbContext.TraderGroups.Find(groupid);
                if (master == null && group != null)
                {
                    master = new MasterSetup
                    {
                        Domain = DbContext.Domains.Find(domainId),
                        TraderGroup = group,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = DbContext.QbicleUser.Find(userId)
                    };
                    DbContext.MasterSetups.Add(master);
                    DbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, groupid);
            }
        }
        public void RemoveMasterSetup(int groupid, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, groupid);
                var master = DbContext.MasterSetups.FirstOrDefault(e => e.Domain.Id == domainId && e.TraderGroup.Id == groupid);
                if (master != null)
                {
                    if (master.GroupSettings != null)
                        DbContext.MasterGroupSettings.RemoveRange(master.GroupSettings);
                    DbContext.MasterSetups.Remove(master);
                    DbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, groupid);
            }

        }
        public ReturnJsonModel ApplyMasterSettingsByGroupId(int domainId, string userId, ApplyTypeSettingsModel model)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, userId, model);
                var queryTaxRates = new TaxRateRules(DbContext).GetByDomainId(domainId);
                var PurchaseTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax).ToList();
                var SaleTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax == false).ToList();
                var traderitems = DbContext.TraderItems.Where(t => t.Group.Id == model.GroupId).ToList();
                foreach (var item in traderitems)
                {
                    //Item I buy
                    if (item.IsBought && !item.IsSold && item.InventoryDetails.Count > 0 && (model.ApplyType == ApplyTypeSettingsModel.ApplyTypeEnum.All || model.ApplyType == ApplyTypeSettingsModel.ApplyTypeEnum.IBuy))
                    {
                        item.TaxRates.Clear();
                        if (model.IBuy.ibuy_purchaseTaxRate != null)
                        {
                            var purchasetax = PurchaseTaxRates.Where(s => model.IBuy.ibuy_purchaseTaxRate.Contains(s.Id)).ToList();
                            if (purchasetax != null)
                                item.TaxRates.AddRange(purchasetax);
                        }

                        if (model.IBuy.ibuy_purchaseAccount > 0)
                            item.PurchaseAccount = DbContext.BKAccounts.Find(model.IBuy.ibuy_purchaseAccount);
                        else
                            item.PurchaseAccount = null;
                        if (model.IBuy.ibuy_pnventoryAccount > 0)
                            item.InventoryAccount = DbContext.BKAccounts.Find(model.IBuy.ibuy_pnventoryAccount);
                        else
                            item.InventoryAccount = null;
                        if (DbContext.Entry(item).State == EntityState.Detached)
                            DbContext.TraderItems.Attach(item);
                        DbContext.Entry(item).State = EntityState.Modified;
                        DbContext.SaveChanges();

                    }//Item I buy & sell
                    else if (item.IsBought && item.IsSold && item.InventoryDetails.Count > 0 && (model.ApplyType == ApplyTypeSettingsModel.ApplyTypeEnum.All || model.ApplyType == ApplyTypeSettingsModel.ApplyTypeEnum.IBuySell))
                    {
                        item.TaxRates.Clear();
                        if (model.IBuySell.ibuysell_purchaseTaxRate != null)
                        {
                            var purchaseTax = PurchaseTaxRates.Where(s => model.IBuySell.ibuysell_purchaseTaxRate.Contains(s.Id)).ToList();
                            if (purchaseTax != null)
                                item.TaxRates.AddRange(purchaseTax);
                        }
                        if (model.IBuySell.ibuysell_salesTaxRate != null)
                        {
                            var saleTax = SaleTaxRates.Where(s => model.IBuySell.ibuysell_salesTaxRate.Contains(s.Id)).ToList();
                            if (saleTax != null)
                                item.TaxRates.AddRange(saleTax);
                        }

                        if (model.IBuySell.ibuysell_purchaseAccount > 0)
                            item.PurchaseAccount = DbContext.BKAccounts.Find(model.IBuySell.ibuysell_purchaseAccount);
                        else
                            item.PurchaseAccount = null;
                        if (model.IBuySell.ibuysell_salesAccount > 0)
                            item.SalesAccount = DbContext.BKAccounts.Find(model.IBuySell.ibuysell_salesAccount);
                        else
                            item.SalesAccount = null;
                        if (model.IBuySell.ibuysell_inventoryAccount > 0)
                            item.InventoryAccount = DbContext.BKAccounts.Find(model.IBuySell.ibuysell_inventoryAccount);
                        else
                            item.InventoryAccount = null;
                        if (DbContext.Entry(item).State == EntityState.Detached)
                            DbContext.TraderItems.Attach(item);
                        DbContext.Entry(item).State = EntityState.Modified;
                        DbContext.SaveChanges();

                        item.UpdateCatalogPricingBasedOnTaxEvents();
                    }//Item I sell (compound)
                    else if (!item.IsBought && item.IsSold && item.IsCompoundProduct && (model.ApplyType == ApplyTypeSettingsModel.ApplyTypeEnum.All || model.ApplyType == ApplyTypeSettingsModel.ApplyTypeEnum.ISellSCompound))
                    {
                        item.TaxRates.Clear();
                        if (model.ISellSCompound.isellcompound_salesTaxRate != null)
                        {
                            var saletax = SaleTaxRates.Where(s => model.ISellSCompound.isellcompound_salesTaxRate.Contains(s.Id)).ToList();
                            if (saletax != null)
                                item.TaxRates.AddRange(saletax);
                        }

                        if (model.ISellSCompound.isellcompound_salesAccount > 0)
                            item.SalesAccount = DbContext.BKAccounts.Find(model.ISellSCompound.isellcompound_salesAccount);
                        else
                            item.SalesAccount = null;
                        if (model.ISellSCompound.isellcompound_inventoryAccount > 0)
                            item.InventoryAccount = DbContext.BKAccounts.Find(model.ISellSCompound.isellcompound_inventoryAccount);
                        else
                            item.InventoryAccount = null;
                        if (DbContext.Entry(item).State == EntityState.Detached)
                            DbContext.TraderItems.Attach(item);
                        DbContext.Entry(item).State = EntityState.Modified;
                        DbContext.SaveChanges();

                        item.UpdateCatalogPricingBasedOnTaxEvents();

                    }//Item I sell (service)
                    else if (!item.IsBought && item.IsSold && !item.IsCompoundProduct && item.InventoryDetails.Count == 0 && (model.ApplyType == ApplyTypeSettingsModel.ApplyTypeEnum.All || model.ApplyType == ApplyTypeSettingsModel.ApplyTypeEnum.ISellService))
                    {
                        item.TaxRates.Clear();
                        if (model.ISellService.isellservices_salesTaxRate != null)
                        {
                            var saletax = SaleTaxRates.Where(s => model.ISellService.isellservices_salesTaxRate.Contains(s.Id)).ToList();
                            if (saletax != null)
                                item.TaxRates.AddRange(saletax);
                        }

                        if (model.ISellService.isellservices_salesAccount > 0)
                            item.SalesAccount = DbContext.BKAccounts.Find(model.ISellService.isellservices_salesAccount);
                        else
                            item.SalesAccount = null;
                        if (model.ISellService.isellservices_inventoryAccount > 0)
                            item.InventoryAccount = DbContext.BKAccounts.Find(model.ISellService.isellservices_inventoryAccount);
                        else
                            item.InventoryAccount = null;
                        if (DbContext.Entry(item).State == EntityState.Detached)
                            DbContext.TraderItems.Attach(item);
                        DbContext.Entry(item).State = EntityState.Modified;
                        DbContext.SaveChanges();

                    }
                }
                #region Save MasterSetup
                var master = DbContext.MasterSetups.FirstOrDefault(e => e.Domain.Id == domainId && e.TraderGroup.Id == model.GroupId);
                if (master == null)
                {
                    var group = DbContext.TraderGroups.Find(model.GroupId);
                    if (group != null)
                    {
                        master = new MasterSetup
                        {
                            Domain = DbContext.Domains.Find(domainId),
                            TraderGroup = group,
                            LastUpdateDate = DateTime.UtcNow,
                            LastUpdatedBy = DbContext.QbicleUser.Find(userId)
                        };
                        DbContext.MasterSetups.Add(master);
                        DbContext.Entry(master).State = EntityState.Added;
                        DbContext.SaveChanges();

                    }
                }
                if (master != null && model.IBuy != null)
                {
                    var ibuysetting = master.GroupSettings.FirstOrDefault(s => s.SettingType == GroupSetting.SettingTypeEnum.IBuy);
                    if (ibuysetting == null)
                    {
                        ibuysetting = new GroupSetting
                        {
                            Master = master,
                            SettingType = GroupSetting.SettingTypeEnum.IBuy,
                            SettingsValue = JsonConvert.SerializeObject(model.IBuy)
                        };
                        DbContext.MasterGroupSettings.Add(ibuysetting);
                        DbContext.Entry(ibuysetting).State = EntityState.Added;
                        DbContext.SaveChanges();
                    }
                    else
                    {
                        ibuysetting.SettingsValue = JsonConvert.SerializeObject(model.IBuy);
                        if (DbContext.Entry(ibuysetting).State == EntityState.Detached)
                            DbContext.MasterGroupSettings.Attach(ibuysetting);
                        DbContext.Entry(ibuysetting).State = EntityState.Modified;
                        DbContext.SaveChanges();

                    }

                }
                if (master != null && model.IBuySell != null)
                {
                    var ibuysellsetting = master.GroupSettings.FirstOrDefault(s => s.SettingType == GroupSetting.SettingTypeEnum.IBuySell);
                    if (ibuysellsetting == null)
                    {
                        ibuysellsetting = new GroupSetting
                        {
                            Master = master,
                            SettingType = GroupSetting.SettingTypeEnum.IBuySell,
                            SettingsValue = JsonConvert.SerializeObject(model.IBuySell)
                        };
                        DbContext.MasterGroupSettings.Add(ibuysellsetting);
                        DbContext.Entry(ibuysellsetting).State = EntityState.Added;
                        DbContext.SaveChanges();

                    }
                    else
                    {
                        ibuysellsetting.SettingsValue = JsonConvert.SerializeObject(model.IBuySell);
                        if (DbContext.Entry(ibuysellsetting).State == EntityState.Detached)
                            DbContext.MasterGroupSettings.Attach(ibuysellsetting);
                        DbContext.Entry(ibuysellsetting).State = EntityState.Modified;
                        DbContext.SaveChanges();

                    }
                }
                if (master != null && model.ISellSCompound != null)
                {
                    var isellcompoundsetting = master.GroupSettings.FirstOrDefault(s => s.SettingType == GroupSetting.SettingTypeEnum.ISellSCompound);
                    if (isellcompoundsetting == null)
                    {
                        isellcompoundsetting = new GroupSetting
                        {
                            Master = master,
                            SettingType = GroupSetting.SettingTypeEnum.ISellSCompound,
                            SettingsValue = JsonConvert.SerializeObject(model.ISellSCompound)
                        };
                        DbContext.MasterGroupSettings.Add(isellcompoundsetting);
                        DbContext.Entry(isellcompoundsetting).State = EntityState.Added;
                        DbContext.SaveChanges();

                    }
                    else
                    {
                        isellcompoundsetting.SettingsValue = JsonConvert.SerializeObject(model.ISellSCompound);
                        if (DbContext.Entry(isellcompoundsetting).State == EntityState.Detached)
                            DbContext.MasterGroupSettings.Attach(isellcompoundsetting);
                        DbContext.Entry(isellcompoundsetting).State = EntityState.Modified;
                        DbContext.SaveChanges();

                    }
                }
                if (master != null && model.ISellService != null)
                {
                    var isellservicesetting = master.GroupSettings.FirstOrDefault(s => s.SettingType == GroupSetting.SettingTypeEnum.ISellService);
                    if (isellservicesetting == null)
                    {
                        isellservicesetting = new GroupSetting
                        {
                            Master = master,
                            SettingType = GroupSetting.SettingTypeEnum.ISellService,
                            SettingsValue = JsonConvert.SerializeObject(model.ISellService)
                        };
                        DbContext.MasterGroupSettings.Add(isellservicesetting);
                        DbContext.Entry(isellservicesetting).State = EntityState.Added;
                        DbContext.SaveChanges();
                    }
                    else
                    {
                        isellservicesetting.SettingsValue = JsonConvert.SerializeObject(model.ISellService);
                        if (DbContext.Entry(isellservicesetting).State == EntityState.Detached)
                            DbContext.MasterGroupSettings.Attach(isellservicesetting);
                        DbContext.Entry(isellservicesetting).State = EntityState.Modified;
                        DbContext.SaveChanges();
                    }
                }

                #endregion

                returnJson.result = true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, userId, model);
            }
            return returnJson;
        }
        public ReturnJsonModel AccountSettingsByItemId(int domainId, string userId, AccountingItemSettingsModel model)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, userId, model);
                var queryTaxRates = new TaxRateRules(DbContext).GetByDomainId(domainId);
                var PurchaseTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax).ToList();
                var SaleTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax == false).ToList();
                var traderitem = DbContext.TraderItems.Find(model.TraderItemId);
                if (traderitem != null)
                {
                    if (traderitem.IsBought && !traderitem.IsSold && traderitem.InventoryDetails.Count > 0)//Item I buy
                    {
                        if (model.IBuy.ibuy_purchaseTaxRate != null)
                        {
                            traderitem.TaxRates.Clear();
                            var purchasetax = PurchaseTaxRates.Where(s => model.IBuy.ibuy_purchaseTaxRate.Contains(s.Id)).ToList();
                            if (purchasetax != null)
                                traderitem.TaxRates = purchasetax;
                        }

                        if (model.IBuy.ibuy_purchaseAccount > 0)
                            traderitem.PurchaseAccount = DbContext.BKAccounts.Find(model.IBuy.ibuy_purchaseAccount);
                        else
                            traderitem.PurchaseAccount = null;
                        if (model.IBuy.ibuy_pnventoryAccount > 0)
                            traderitem.InventoryAccount = DbContext.BKAccounts.Find(model.IBuy.ibuy_pnventoryAccount);
                        else
                            traderitem.InventoryAccount = null;
                    }
                    else if (traderitem.IsBought && traderitem.IsSold && traderitem.InventoryDetails.Count > 0)//Item I buy & sell
                    {
                        if (model.IBuySell.ibuysell_purchaseTaxRate != null)
                        {
                            traderitem.TaxRates.Clear();
                            var purchaseTax = PurchaseTaxRates.Where(s => model.IBuySell.ibuysell_purchaseTaxRate.Contains(s.Id)).ToList();
                            if (purchaseTax != null)
                                traderitem.TaxRates.AddRange(purchaseTax);
                        }
                        if (model.IBuySell.ibuysell_salesTaxRate != null)
                        {
                            var saleTax = SaleTaxRates.Where(s => model.IBuySell.ibuysell_salesTaxRate.Contains(s.Id)).ToList();
                            if (saleTax != null)
                                traderitem.TaxRates.AddRange(saleTax);
                        }

                        if (model.IBuySell.ibuysell_purchaseAccount > 0)
                            traderitem.PurchaseAccount = DbContext.BKAccounts.Find(model.IBuySell.ibuysell_purchaseAccount);
                        else
                            traderitem.PurchaseAccount = null;
                        if (model.IBuySell.ibuysell_salesAccount > 0)
                            traderitem.SalesAccount = DbContext.BKAccounts.Find(model.IBuySell.ibuysell_salesAccount);
                        else
                            traderitem.SalesAccount = null;
                        if (model.IBuySell.ibuysell_inventoryAccount > 0)
                            traderitem.InventoryAccount = DbContext.BKAccounts.Find(model.IBuySell.ibuysell_inventoryAccount);
                        else
                            traderitem.InventoryAccount = null;
                    }
                    else if (!traderitem.IsBought && traderitem.IsSold && traderitem.IsCompoundProduct)//Item I sell (compound)
                    {
                        if (model.ISellSCompound.isellcompound_salesTaxRate != null)
                        {
                            traderitem.TaxRates.Clear();
                            var saletax = SaleTaxRates.Where(s => model.ISellSCompound.isellcompound_salesTaxRate.Contains(s.Id)).ToList();
                            if (saletax != null)
                                traderitem.TaxRates.AddRange(saletax);
                        }

                        if (model.ISellSCompound.isellcompound_salesAccount > 0)
                            traderitem.SalesAccount = DbContext.BKAccounts.Find(model.ISellSCompound.isellcompound_salesAccount);
                        else
                            traderitem.SalesAccount = null;
                        if (model.ISellSCompound.isellcompound_inventoryAccount > 0)
                            traderitem.InventoryAccount = DbContext.BKAccounts.Find(model.ISellSCompound.isellcompound_inventoryAccount);
                        else
                            traderitem.InventoryAccount = null;
                    }
                    else if (!traderitem.IsBought && traderitem.IsSold && !traderitem.IsCompoundProduct && traderitem.InventoryDetails.Count == 0)//Item I sell (service)
                    {
                        if (model.ISellService.isellservices_salesTaxRate != null)
                        {
                            traderitem.TaxRates.Clear();
                            var saletax = SaleTaxRates.Where(s => model.ISellService.isellservices_salesTaxRate.Contains(s.Id)).ToList();
                            if (saletax != null)
                                traderitem.TaxRates.AddRange(saletax);
                        }

                        if (model.ISellService.isellservices_salesAccount > 0)
                            traderitem.SalesAccount = DbContext.BKAccounts.Find(model.ISellService.isellservices_salesAccount);
                        else
                            traderitem.SalesAccount = null;
                        if (model.ISellService.isellservices_inventoryAccount > 0)
                            traderitem.InventoryAccount = DbContext.BKAccounts.Find(model.ISellService.isellservices_inventoryAccount);
                        else
                            traderitem.InventoryAccount = null;
                    }
                    if (DbContext.Entry(traderitem).State == EntityState.Detached)
                        DbContext.TraderItems.Attach(traderitem);
                    DbContext.Entry(traderitem).State = EntityState.Modified;
                }
                returnJson.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, userId, model);
            }
            return returnJson;
        }
        public ReturnJsonModel ApplyConfigPriceByGroup(int domainId, string userId, ConfigsPriceModel model)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, userId, model);
                var items = DbContext.TraderItems.Where(s => s.Group.Id == model.GroupId && s.IsSold).ToList();
                decimal priceValue = 0;
                decimal calculatedMarkup = 0;
                decimal calculatedDiscount = 0;
                if (items.Count == 0)
                {
                    returnJson.msg = ResourcesManager._L("WARNING_MSG_PRICEITEMSNOTFOUND");
                    return returnJson;
                }
                foreach (var item in items)
                {
                    foreach (var locationId in model.Locations)
                    {
                        var location = DbContext.TraderLocations.Find(locationId);
                        if (location != null)
                        {
                            var InventoryDetail = item.InventoryDetails.FirstOrDefault(s => s.Location.Id == locationId);
                            var cost = (InventoryDetail != null ? InventoryDetail.AverageCost : 0);
                            calculatedMarkup = model.MarkupMethod == 1 ? (cost * (model.MarkupValue / 100)) : model.MarkupValue;/*1=%; 2=value*/
                            calculatedDiscount = model.DiscountMethod == 1 ? (cost * (model.DiscountValue / 100)) : model.DiscountValue;/*1=%; 2=value*/
                            priceValue = cost + calculatedMarkup - calculatedDiscount;//Price Exclusive of Tax = AverageCost + Markup - Discount
                            foreach (var channel in model.Salechannels)
                            {
                                new TraderPriceRules(DbContext).CreatePriceByLocationIdItemId(location, (SalesChannelEnum)channel, priceValue, priceValue, item, userId, model.IsExistingOverwritten);
                            }
                        }
                    }
                }
                DbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, userId, model);
            }
            return returnJson;
        }
        public DataTablesResponse PricesTraderGroupMaster([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int domainid, int groupid, string keyword, int locationid, int saleschannel)
        {
            try
            {
                var query = from items in DbContext.TraderItems
                            join prices in DbContext.TraderPrices on items.Id equals prices.Item.Id
                            where items.Group.Id == groupid
                            && items.IsSold
                            select prices;


                #region Filters
                if (!String.IsNullOrEmpty(keyword))
                {
                    query = query.Where(t => t.Item.Name.Trim().ToLower().Contains(keyword.Trim().ToLower()));
                }
                if (locationid > 0)
                {
                    query = query.Where(s => s.Location.Id == locationid);
                }
                if (saleschannel > 0)
                {
                    query = query.Where(s => s.SalesChannel == (SalesChannelEnum)saleschannel);
                }
                int totalRecords = query.Count();
                #endregion

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Item":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Item.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Location":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Location.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "SalesChannel":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SalesChannel" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "PriceExcTax":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Value" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Tax":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Value" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "PriceIncTax":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Value" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Item.Name asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                var currencySettings = new CurrencySettingRules(DbContext).GetCurrencySettingByDomain(domainid);
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Item = q.Item.Name,
                    Location = q.Location.Name,
                    AverageCost = CalAverageCost(q.Item, q.Location.Id).ToDecimalPlace(currencySettings),
                    SalesChannel = q.SalesChannel.ToString(),
                    PriceExcTax = q.NetPrice.ToInputNumberFormat(currencySettings),
                    Tax = (q.Taxes.Sum(tx => tx.Amount)).ToInputNumberFormat(currencySettings),
                    GrossPrice = q.GrossPrice.ToInputNumberFormat(currencySettings)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        private decimal CalAverageCost(TraderItem item, int locationId)
        {
            var inventoryDetail = item.InventoryDetails.FirstOrDefault(e => e.Location.Id == locationId);
            if (inventoryDetail != null)
            {
                var unit = item.Units.FirstOrDefault(s => s.IsBase);
                return (inventoryDetail.AverageCost * (unit?.QuantityOfBaseunit ?? 1));
            }
            else
                return 0;

        }
        public ReturnJsonModel UpdateValuePrice(int id, bool isInclusiveTax, decimal value, int domainId, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, isInclusiveTax, value);

                var currentUser = DbContext.QbicleUser.Find(userId);
                var currencySettings = new CurrencySettingRules(DbContext).GetCurrencySettingByDomain(domainId);
                var price = DbContext.TraderPrices.Find(id);
                if (price != null)
                {
                    if (isInclusiveTax)
                    {
                        price.NetPrice = value / (1 + price.Item.SumTaxRates(true));
                        price.GrossPrice = value;
                    }
                    else
                    {
                        price.NetPrice = value;
                        price.GrossPrice = value * (1 + price.Item.SumTaxRates(true));
                    }

                    //Calculate taxes
                    var lstTaxes = price.Item.TaxRates.Where(s => !s.IsPurchaseTax).ToList();

                    if (lstTaxes != null && lstTaxes.Count > 0)
                    {
                        foreach (var taxitem in lstTaxes)
                        {
                            var priceTaxItem = price.Taxes.FirstOrDefault(p => p.TaxName == taxitem.Name);
                            if (priceTaxItem == null || priceTaxItem.Id == 0)
                            {
                                var staticTaxRate = new TaxRateRules(DbContext).CloneStaticTaxRateById(taxitem.Id);
                                priceTaxItem = new PriceTax
                                {
                                    TaxName = taxitem.Name,
                                    Rate = taxitem.Rate,
                                    TaxRate = staticTaxRate
                                };
                                priceTaxItem.Amount = price.NetPrice * (taxitem.Rate / 100);
                                DbContext.Entry(priceTaxItem).State = EntityState.Added;
                                DbContext.TraderPriceTaxes.Add(priceTaxItem);
                                price.Taxes.Add(priceTaxItem);
                            }
                            else
                            {
                                priceTaxItem.Amount = price.NetPrice * (taxitem.Rate / 100);
                                DbContext.Entry(priceTaxItem).State = EntityState.Modified;
                            }
                        }
                        DbContext.SaveChanges();
                    }
                    price.Taxes.RemoveAll(p => !lstTaxes.Any(x => x.Name == p.TaxName));
                    price.TotalTaxAmount = price.Taxes.Sum(p => p.Amount);
                    price.LastUpdateDate = DateTime.UtcNow;
                    price.LastUpdatedBy = currentUser;

                    returnJson.Object = new { price.Id, PriceExcTax = price.NetPrice.ToInputNumberFormat(currencySettings), Tax = (price.Item.SumTaxRates(true) * price.NetPrice).ToInputNumberFormat(currencySettings) };
                }

                var saveResult = DbContext.SaveChanges();
                returnJson.result = saveResult > 0 ? true : false;
                returnJson.Object2 = saveResult;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, isInclusiveTax, value);
            }
            return returnJson;
        }


        public List<TraderConfigurationModel> GetTraderGroupItemConfig(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return (from d in DbContext.TraderGroups
                        where d.Domain.Id == domainId
                        select new TraderConfigurationModel
                        {
                            Id = d.Id,
                            Name = d.Name,
                            CreatedDate = d.CreatedDate,
                            CreatedBy = d.CreatedBy,
                            CanDelete = d.Items.Count == 0 && d.PriceDefaults.Count == 0 && d.WorkGroupCategories.Count == 0
                        }).OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderConfigurationModel>();
            }
        }
        public List<BaseModel> GetTraderGroupItemBase(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.TraderGroups.Where(d => d.Domain.Id == domainId).OrderBy(n => n.Name).Select(l => new BaseModel { Id = l.Id, Name = l.Name }).ToList(); ;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<BaseModel>();
            }
        }

        public List<QbicleDomain> CollectionConsumerDomainforCatalog(int currentDomainId)
        {
            return null;
        }
    }
}