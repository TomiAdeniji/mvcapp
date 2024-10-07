using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.ODS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderPurchaseRules
    {
        ApplicationDbContext _db;

        public TraderPurchaseRules(ApplicationDbContext context)
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
        public TraderPurchase GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.TraderPurchases.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return  new TraderPurchase();
            }
        }

        public ReturnJsonModel SaveTraderPurchase(TraderPurchase traderPurchase, string userId, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, traderPurchase);

                var user = DbContext.QbicleUser.Find(userId);


                traderPurchase.Location = DbContext.TraderLocations.FirstOrDefault(q => q.Id == traderPurchase.Location.Id);

                traderPurchase.CreatedDate = DateTime.UtcNow;
                if (traderPurchase.Reference != null)
                    traderPurchase.Reference = new TraderReferenceRules(DbContext).GetById(traderPurchase.Reference.Id);

                int purchaseId;
                if (traderPurchase.PurchaseItems.Count > 0)
                {
                    foreach (var item in traderPurchase.PurchaseItems)
                    {
                        if (item.Id == 0)
                        {
                            item.CreatedDate = DateTime.UtcNow;
                            item.CreatedBy = user;
                        }
                        else
                        {
                            item.LastUpdatedBy = user;
                            item.LastUpdatedDate = DateTime.UtcNow;
                        }
                        item.TraderItem = DbContext.TraderItems.Find(item.TraderItem.Id);
                        if (item.TraderItem != null && traderPurchase.Location != null && item.PriceBookPrice == null)
                        {
                            var traderId = item.TraderItem.Id;
                            item.PriceBookPrice = DbContext.TraderPrices.FirstOrDefault(q => q.Location.Id == traderPurchase.Location.Id && q.Item.Id == traderId && q.SalesChannel == Models.Trader.SalesChannel.SalesChannelEnum.Trader);
                        }
                        else if (item.PriceBookPrice != null)
                        {
                            item.PriceBookPrice = DbContext.TraderPrices.Find(item.PriceBookPrice.Id);
                        }
                        if (item.PriceBookPrice != null)
                        {
                            item.PriceBookPriceValue = item.PriceBookPrice.NetPrice;
                        }
                        if (item.Dimensions.Count > 0)
                        {
                            for (var j = 0; j < item.Dimensions.Count; j++)
                            {
                                item.Dimensions[j] =
                                    DbContext.TransactionDimensions.Find(item.Dimensions[j].Id);
                            }
                        }
                        if (item.Unit != null)
                        {
                            item.Unit =
                                DbContext.ProductUnits.Find(item.Unit.Id);
                        }
                        //Update Taxes TraderTransactionItem
                        if (item.TraderItem != null)
                        {
                            foreach (var tax in item.TraderItem.TaxRates.Where(s => s.IsPurchaseTax).ToList())
                            {
                                var staticTaxRate = new TaxRateRules(DbContext).CloneStaticTaxRateById(tax.Id);
                                OrderTax orderTax = new OrderTax
                                {
                                    StaticTaxRate= staticTaxRate,
                                    TaxRate = tax,
                                    Value = item.CostPerUnit * (1 - (item.Discount / 100)) * (tax.Rate / 100)
                                };
                                item.Taxes.Add(orderTax);
                            }
                        }
                    }
                }


                if (traderPurchase.Vendor.Id != 0)
                {
                    traderPurchase.Vendor = DbContext.TraderContacts.Find(traderPurchase.Vendor.Id);
                    if (traderPurchase.Vendor != null)
                        traderPurchase.Vendor.InUsed = true;
                }
                if (traderPurchase.Workgroup != null && traderPurchase.Workgroup.Id > 0)
                {
                    traderPurchase.Workgroup = DbContext.WorkGroups.Find(traderPurchase.Workgroup.Id);

                }

                if (traderPurchase.Id == 0)
                {
                    traderPurchase.CreatedBy = user;
                    DbContext.TraderPurchases.Add(traderPurchase);
                    DbContext.Entry(traderPurchase).State = EntityState.Added;
                    DbContext.SaveChanges();
                    result.actionVal = 1;
                    purchaseId = traderPurchase.Id;
                    //Whenever a TraderTransactionItem is saved to the database a copy of the TraderTransactionItem is saved to the TransactionItemLog table.
                    foreach (var item in traderPurchase.PurchaseItems)
                    {
                        var transItemLog = new TransactionItemLog
                        {
                            Unit = item.Unit,
                            AssociatedTransactionItem = item,
                            Cost = item.Cost,
                            CostPerUnit = item.CostPerUnit,
                            Dimensions = item.Dimensions,
                            Discount = item.Discount,
                            Price = item.Price,
                            PriceBookPrice = item.PriceBookPrice,
                            PriceBookPriceValue = item.PriceBookPriceValue,
                            Quantity = item.Quantity,
                            SalePricePerUnit = item.SalePricePerUnit,
                            TraderItem = item.TraderItem,
                            TransferItems = item.TransferItems
                        };
                        DbContext.Entry(transItemLog).State = EntityState.Added;
                        DbContext.TraderTransactionItemLogs.Add(transItemLog);
                    }

                    DbContext.SaveChanges();
                }
                else
                {


                    var purchaseDb = DbContext.TraderPurchases.Find(traderPurchase.Id);
                    if (traderPurchase.Reference != null)
                    {
                        purchaseDb.Reference = traderPurchase.Reference;
                    }
                    purchaseDb.Status = traderPurchase.Status;
                    purchaseDb.DeliveryMethod = traderPurchase.DeliveryMethod;
                    purchaseDb.Vendor = traderPurchase.Vendor;
                    purchaseDb.PurchaseTotal = traderPurchase.PurchaseTotal;
                    purchaseDb.Workgroup = traderPurchase.Workgroup;
                    DbContext.Entry(purchaseDb).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    result.actionVal = 2;
                    purchaseId = purchaseDb.Id;

                    //Update Transaction Items

                    var itemsUi = traderPurchase.PurchaseItems;
                    var itemsDb = purchaseDb.PurchaseItems;

                    var itemsNew = itemsUi.Where(c => !itemsDb.Any(d => c.Id == d.Id)).ToList();

                    var itemsDelete = itemsDb.Where(c => itemsUi.All(d => c.Id != d.Id)).ToList();

                    var itemsUpdate = itemsUi.Where(c => itemsDb.Any(d => c.Id == d.Id)).ToList();


                    foreach (var itemDel in itemsDelete)
                    {
                        //remove Order Tax
                        if (itemDel.Taxes != null && itemDel.Taxes.Any())
                            DbContext.OrderTaxs.RemoveRange(itemDel.Taxes);
                        if (itemDel.Logs.Any())
                        {
                            DbContext.TraderTransactionItemLogs.RemoveRange(itemDel.Logs);
                        }
                        purchaseDb.PurchaseItems.Remove(itemDel);
                        DbContext.Entry(purchaseDb).State = EntityState.Modified;
                        DbContext.SaveChanges();
                    }

                    foreach (var iDb in purchaseDb.PurchaseItems)
                    {
                        var iUpdate = itemsUpdate.FirstOrDefault(e => e.Id == iDb.Id);
                        iDb.Dimensions.Clear();
                        if (iDb.Logs.Any())
                        {
                            DbContext.TraderTransactionItemLogs.RemoveRange(iDb.Logs);
                        }
                        if (iUpdate == null) continue;
                        iDb.Unit = iUpdate.Unit;
                        iDb.Cost = iUpdate.Cost;
                        //it.Id = iUi.Id;
                        //it.CreatedDate = DateTime.UtcNow;
                        iDb.Price = iUpdate.Price;
                        //it.CreatedBy = user;
                        iDb.Discount = iUpdate.Discount;
                        iDb.Quantity = iUpdate.Quantity;
                        iDb.TraderItem = iUpdate.TraderItem;
                        iDb.PriceBookPrice = iUpdate.PriceBookPrice;
                        iDb.LastUpdatedBy = user;
                        iDb.TransferItems = iUpdate.TransferItems;
                        iDb.Dimensions = iUpdate.Dimensions;
                        iDb.LastUpdatedDate = DateTime.UtcNow;
                        //iDb.Logs = iUpdate.Logs;
                        iDb.CostPerUnit = iUpdate.CostPerUnit;
                        iDb.PriceBookPriceValue = iUpdate.PriceBookPriceValue;
                        iDb.SalePricePerUnit = iUpdate.SalePricePerUnit;
                        //remove Order Tax
                        if (iDb.Taxes != null && iDb.Taxes.Any())
                            DbContext.OrderTaxs.RemoveRange(iDb.Taxes);
                        iDb.Taxes = iUpdate.Taxes;


                        DbContext.Entry(iDb).State = EntityState.Modified;
                        DbContext.SaveChanges();

                        var transItemLog = new TransactionItemLog
                        {
                            Unit = iDb.Unit,
                            AssociatedTransactionItem = iDb,
                            Cost = iDb.Cost,
                            CostPerUnit = iDb.CostPerUnit,
                            Dimensions = iDb.Dimensions,
                            Discount = iDb.Discount,
                            Price = iDb.Price,
                            PriceBookPrice = iDb.PriceBookPrice,
                            PriceBookPriceValue = iDb.PriceBookPriceValue,
                            Quantity = iDb.Quantity,
                            SalePricePerUnit = iDb.SalePricePerUnit,
                            TraderItem = iDb.TraderItem,
                            TransferItems = iDb.TransferItems
                        };
                        DbContext.Entry(transItemLog).State = EntityState.Added;
                        DbContext.TraderTransactionItemLogs.Add(transItemLog);
                        DbContext.SaveChanges();
                    }
                    if (itemsNew.Count > 0)
                    {
                        purchaseDb.PurchaseItems.AddRange(itemsNew);
                        DbContext.Entry(purchaseDb).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        foreach (var item in itemsNew)
                        {
                            //Whenever a TraderTransactionItem is saved to the database a copy of the TraderTransactionItem is saved to the TransactionItemLog table.
                            var transItemLog = new TransactionItemLog
                            {
                                Unit = item.Unit,
                                AssociatedTransactionItem = item,
                                Cost = item.Cost,
                                CostPerUnit = item.CostPerUnit,
                                Dimensions = item.Dimensions,
                                Discount = item.Discount,
                                Price = item.Price,
                                PriceBookPrice = item.PriceBookPrice,
                                PriceBookPriceValue = item.PriceBookPriceValue,
                                Quantity = item.Quantity,
                                SalePricePerUnit = item.SalePricePerUnit,
                                TraderItem = item.TraderItem,
                                TransferItems = item.TransferItems
                            };
                            DbContext.Entry(transItemLog).State = EntityState.Added;
                            DbContext.TraderTransactionItemLogs.Add(transItemLog);
                        }

                        DbContext.SaveChanges();


                    }
                }



                if (traderPurchase.Status != TraderPurchaseStatusEnum.PendingReview) return result;

                var tradPurchaseDb = GetById(purchaseId);

                if (tradPurchaseDb?.PurchaseApprovalProcess != null)
                    return result;

                tradPurchaseDb.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var refFull = tradPurchaseDb.Reference == null ? "" : tradPurchaseDb.Reference.FullRef;
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = DbContext.PurchaseApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == tradPurchaseDb.Workgroup.Id),
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Purchase = new List<TraderPurchase> { tradPurchaseDb },
                    Name = $"Trader Approval for Purchase #{refFull}",
                    Qbicle = tradPurchaseDb.Workgroup.Qbicle,
                    Topic = tradPurchaseDb.Workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = tradPurchaseDb.CreatedBy,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    App = QbicleActivity.ActivityApp.Trader
                };

                approval.ActivityMembers.AddRange(tradPurchaseDb.Workgroup.Members);
                DbContext.ApprovalReqs.Add(approval);
                DbContext.Entry(approval).State = EntityState.Added;
                tradPurchaseDb.PurchaseApprovalProcess = approval;

                DbContext.Entry(tradPurchaseDb).State = EntityState.Modified;


                DbContext.SaveChanges();


                var purchaseLog = new PurchaseLog
                {
                    AssociatedPurchase = tradPurchaseDb,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    DeliveryMethod = tradPurchaseDb.DeliveryMethod,
                    Invoices = tradPurchaseDb.Invoices,
                    IsInHouse = false,
                    Location = tradPurchaseDb.Location,
                    PurchaseApprovalProcess = approval,
                    PurchaseItems = tradPurchaseDb.PurchaseItems,
                    PurchaseOrder = tradPurchaseDb.PurchaseOrder,
                    PurchaseTotal = tradPurchaseDb.PurchaseTotal,
                    Status = tradPurchaseDb.Status,
                    Transfer = null,
                    Vendor = tradPurchaseDb.Vendor,
                    Workgroup = tradPurchaseDb.Workgroup
                };

                var purchaseProcessLog = new PurchaseProcessLog
                {
                    AssociatedPurchase = tradPurchaseDb,
                    AssociatedPurchaseLog = purchaseLog,
                    PurchaseStatus = tradPurchaseDb.Status,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = tradPurchaseDb.CreatedBy,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }

                };

                DbContext.TraderPurchaseProcessLogs.Add(purchaseProcessLog);
                DbContext.Entry(purchaseProcessLog).State = EntityState.Added;
                DbContext.SaveChanges();

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = NotificationEventEnum.ApprovalCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = HelperClass.GetFullNameOfUser(approval.StartedBy),
                    ReminderMinutes = 0
                };
                new NotificationRules(DbContext).Notification2Activity(activityNotification);

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, traderPurchase);
                result.result = false;
                result.actionVal = 3;
                result.msg = e.Message;
            }
            
            return result;

        }

        public List<TraderPurchase> GetByLocation(int locationId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId);
                return DbContext.TraderPurchases.Where(l => l.Location.Id == locationId && l.Location.Domain.Id == domainId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId, domainId);
                return new List<TraderPurchase>();
            }
        }
        public DataTablesResponse GetByLocationPagination(int locationId, int domainId, string daterange, string timezone,
            IDataTablesRequest requestModel,string dateFortmat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId, daterange, timezone, requestModel, dateFortmat);
                var currencySettings = new CurrencySettingRules(DbContext).GetCurrencySettingByDomain(domainId);
                var query = DbContext.TraderPurchases.Where(l => l.Location.Id == locationId && l.Location.Domain.Id == domainId && (l.Status == TraderPurchaseStatusEnum.PurchaseApproved ||
                     l.Status == TraderPurchaseStatusEnum.PurchaseOrderIssued));
                int totalSale = 0;
                #region Filter
                var keyword = requestModel.Search != null ? requestModel.Search.Value.Replace("#", "") : "";
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q =>
                        q.Vendor != null && q.Vendor.Name.ToLower().Contains(keyword)
                        || (q.Reference != null && q.Reference.FullRef.ToLower().Contains(keyword))
                        || q.Invoices.Any(v => v.Id.ToString() == keyword)
                    );
                if (!string.IsNullOrEmpty(daterange))
                {
                    if (!daterange.Contains('-'))
                    {
                        daterange += "-";
                    }
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    daterange.ConvertDaterangeFormat(dateFortmat, "", out startDate, out endDate);
                    startDate = startDate.AddTicks(1);
                    endDate = endDate.AddDays(1).AddTicks(-1);
                    query = query.Where(q => q.CreatedDate >= startDate && q.CreatedDate <= endDate);
                }
                totalSale = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "FullRef":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "WorkgroupName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Workgroup.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "SalesChannel":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SalesChannel" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Contact":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Vendor.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "SaleTotal":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SaleTotal" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate desc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new TraderSaleCustom
                {
                    Id = q.Id,
                    FullRef = q.Reference?.FullRef,
                    WorkgroupName = q.Workgroup?.Name,
                    CreatedDate = q.CreatedDate.ConvertTimeFromUtc(timezone).ToString(dateFortmat),
                    Contact = q.Vendor?.Name,
                    Dimensions = q.PurchaseItems.Count == 0 ? "" : string.Join(", ", q.PurchaseItems.Where(c => c.Dimensions.Any()).SelectMany(b => b.Dimensions.Select(v => v.Name)).Distinct()),
                    SaleTotal = q.PurchaseTotal.ToDecimalPlace(currencySettings),
                    Status = q.Status.ToString(),
                    TransferCount = q.Transfer.Count()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalSale, totalSale);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, domainId, daterange, timezone, requestModel, dateFortmat);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }

        }

        public List<WorkGroup> GetWorkGroupsByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);
                return DbContext.TraderPurchases.Where(l => l.Location.Id == locationId).Select(w => w.Workgroup).OrderBy(n => n.Name).Distinct().ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId);
                return new List<WorkGroup>();
            }
        }

        public DataTablesResponse GetTraderPurchaseDataTable(IDataTablesRequest requestModel, UserSetting user, int locationId,
            string keysearch, int groupId,int domainId)
        {
            var totalRecords = 0;
            var currencySettings = new CurrencySettingRules(DbContext).GetCurrencySettingByDomain(domainId);
            var response = new DataTablesResponse(requestModel.Draw, new List<PurchaseCustom>(), 0, 0);
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, user.Id, null, requestModel, user, locationId, keysearch,
                        groupId, domainId);

                var dateFormat =string.IsNullOrEmpty(user.DateFormat)?"dd/MM/yyyy": user.DateFormat;
                var workGroupAllIds = DbContext.WorkGroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderPurchaseProcessName))).Select(q => q.Id).ToList();
                var workGroupIds = DbContext.WorkGroups.Where(q =>
                    q.Location.Id == locationId
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderPurchaseProcessName))
                    && q.Members.Select(u => u.Id).Contains(user.Id)).Select(q => q.Id).ToList();

                var purchaseItems = DbContext.TraderPurchases.Where(q => q.Location.Id == locationId);


                if (purchaseItems.Any())
                {
                    if (groupId > 0)
                    {
                        purchaseItems = purchaseItems.Where(q => q.Workgroup.Id == groupId);
                    }
                    if (!string.IsNullOrEmpty(keysearch))
                    {
                        keysearch = keysearch.ToLower();
                        purchaseItems = purchaseItems.Where(q => (q.Reference != null && q.Reference.FullRef.ToLower().Contains(keysearch))
                                                            || (q.Workgroup != null && q.Workgroup.Name.ToLower().Contains(keysearch))
                                                            || ((q.CreatedDate.Day < 10 ? ("0" + q.CreatedDate.Day.ToString()) : q.CreatedDate.Day.ToString()) + "/" + (q.CreatedDate.Month < 10 ? ("0" + q.CreatedDate.Month.ToString()) : q.CreatedDate.Month.ToString()) + "/" + q.CreatedDate.Year).Contains(keysearch)
                                                            || q.PurchaseTotal.ToString().Contains(keysearch)
                                                            || (q.Vendor != null && q.Vendor.Name.ToLower().Contains(keysearch))
                                                            || q.PurchaseItems.Any(id => id.Dimensions.Any(d => d.Name.ToLower().Contains(keysearch)))
                                                            );
                    }

                    if (purchaseItems.Any())
                    {
                        totalRecords = purchaseItems.Count();
                        var sortedColumns = requestModel.Columns.GetSortedColumns();
                        var orderByString = string.Empty;
                        foreach (var column in sortedColumns)
                        {
                            switch (column.Name)
                            {
                                case "WorkGroupName":
                                    orderByString += orderByString != string.Empty ? "," : "";
                                    orderByString += "WorkGroup.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                    break;
                                case "CreatedDate":
                                    orderByString += orderByString != string.Empty ? "," : "";
                                    orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                                    break;
                                default:
                                    orderByString = "CreatedDate asc";
                                    break;
                            }
                        }
                        purchaseItems = purchaseItems.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                        var lstItems = purchaseItems.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                        var returnListItem = lstItems.Select(q => new PurchaseCustom()
                        {
                            Id = q.Id,
                            FullRef = q.Reference == null ? "" : q.Reference.FullRef,
                            Status = q.Status.ToString(),
                            Contact = q.Vendor.Name,
                            CreatedDate = q.CreatedDate.ToString(dateFormat),
                            ReportingFilter = string.Join(", ", q.PurchaseItems.SelectMany(d => d.Dimensions.Select(n => n.Name)).Distinct()),
                            Total = q.PurchaseTotal.ToDecimalPlace(currencySettings),
                            WorkGroupName = q.Workgroup.Name,
                            AllowEdit = workGroupIds.Contains(q.Workgroup.Id)
                        }).ToList();
                        return new DataTablesResponse(requestModel.Draw, returnListItem, totalRecords, totalRecords);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, requestModel, user, locationId, keysearch, groupId, domainId);
            }

            return response;
        }
        public ReturnJsonModel UpdateTraderPurchaseContact(TraderPurchase traderPurchase, string countryName)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, traderPurchase, countryName);
                //update purchase contact

                var tradPurchase = DbContext.TraderPurchases.Find(traderPurchase.Id);
                if (tradPurchase == null) return result;
                tradPurchase.DeliveryMethod = traderPurchase.DeliveryMethod;
                tradPurchase.Vendor = DbContext.TraderContacts.Find(traderPurchase.Vendor.Id);
                DbContext.Entry(tradPurchase).State = EntityState.Modified;
                DbContext.SaveChanges();
                result.msgId = traderPurchase.Id.ToString();

                var contactHtml = " <div class='owner-avatar'>";
                contactHtml += $"<div class='avatar-sm' style='background: url(\"{tradPurchase.Vendor.AvatarUri.ToUri()}\");'></div>";
                contactHtml += $"</div>";
                contactHtml += $" <h5>{tradPurchase.Vendor.Name}<br><small>Purchase Contact</small></h5>";
                result.msg = contactHtml;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, traderPurchase, countryName);
                result.actionVal = 3;
                result.msg = ex.Message;
                return result;
            }
        }

        public ReturnJsonModel UpdateTraderPurchaseItems(TraderPurchase traderPurchase, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, traderPurchase);
                if (traderPurchase.PurchaseItems.Count > 0)
                {

                    foreach (var item in traderPurchase.PurchaseItems)
                    {
                        item.TraderItem = DbContext.TraderItems.Find(item.TraderItem.Id);
                        if (item.Dimensions.Count > 0)
                        {
                            for (var j = 0; j < item.Dimensions.Count; j++)
                            {
                                item.Dimensions[j] =
                                    DbContext.TransactionDimensions.Find(item.Dimensions[j].Id);
                            }
                        }
                        if (item.Unit != null)
                        {
                            item.Unit =
                                DbContext.ProductUnits.Find(item.Unit.Id);
                        }
                        //Update Taxes TraderTransactionItem
                        if (item.TraderItem != null)
                        {
                            foreach (var tax in item.TraderItem.TaxRates.Where(s => s.IsPurchaseTax).ToList())
                            {
                                var staticTaxRate = new TaxRateRules(DbContext).CloneStaticTaxRateById(tax.Id);
                                OrderTax orderTax = new OrderTax
                                {
                                    StaticTaxRate= staticTaxRate,
                                    TaxRate = tax,
                                    Value = item.CostPerUnit * (1 - (item.Discount / 100)) * (tax.Rate / 100)
                                };
                                item.Taxes.Add(orderTax);
                            }
                        }

                    }
                }
                var traderPurchaseDb = DbContext.TraderPurchases.Find(traderPurchase.Id);
                if (traderPurchaseDb == null) return result;
                traderPurchaseDb.PurchaseTotal = traderPurchase.PurchaseTotal;
                DbContext.Entry(traderPurchaseDb).State = EntityState.Modified;
                DbContext.SaveChanges();
                result.msgId = traderPurchase.Id.ToString();

                //Update Transaction Items

                var itemsUi = traderPurchase.PurchaseItems;
                var itemsDb = traderPurchaseDb.PurchaseItems;

                var itemsNew = itemsUi.Where(c => itemsDb.All(d => c.Id != d.Id)).ToList();

                var itemsDelete = itemsDb.Where(c => itemsUi.All(d => c.Id != d.Id)).ToList();

                var itemsUpdate = itemsUi.Where(c => itemsDb.Any(d => c.Id == d.Id)).ToList();


                foreach (var itemDel in itemsDelete)
                {
                    //remove Order Tax
                    if (itemDel.Taxes != null && itemDel.Taxes.Any())
                        DbContext.OrderTaxs.RemoveRange(itemDel.Taxes);
                    traderPurchaseDb.PurchaseItems.Remove(itemDel);
                    DbContext.Entry(traderPurchaseDb).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }

                foreach (var iDb in traderPurchaseDb.PurchaseItems)
                {
                    var iUpdate = itemsUpdate.FirstOrDefault(e => e.Id == iDb.Id);
                    iDb.Dimensions.Clear();
                    if (iUpdate == null) continue;
                    iDb.Unit = iUpdate.Unit;
                    iDb.Cost = iUpdate.Cost;
                    //it.Id = iUi.Id;
                    //it.CreatedDate = DateTime.UtcNow;
                    iDb.Price = iUpdate.Price;
                    //it.CreatedBy = user;
                    iDb.Discount = iUpdate.Discount;
                    iDb.Quantity = iUpdate.Quantity;
                    iDb.TraderItem = iUpdate.TraderItem;
                    iDb.PriceBookPrice = iUpdate.PriceBookPrice;
                    iDb.LastUpdatedBy = DbContext.QbicleUser.Find(userId);
                    iDb.TransferItems = iUpdate.TransferItems;
                    iDb.Dimensions = iUpdate.Dimensions;
                    iDb.LastUpdatedDate = DateTime.UtcNow;
                    //iDb.Logs = iUpdate.Logs;
                    iDb.CostPerUnit = iUpdate.CostPerUnit;
                    iDb.PriceBookPriceValue = iUpdate.PriceBookPriceValue;
                    iDb.SalePricePerUnit = iUpdate.SalePricePerUnit;
                    //remove Order Tax
                    if (iDb.Taxes != null && iDb.Taxes.Any())
                        DbContext.OrderTaxs.RemoveRange(iDb.Taxes);
                    iDb.Taxes = iUpdate.Taxes;

                    DbContext.Entry(iDb).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    var transItemLog = new TransactionItemLog
                    {
                        Unit = iDb.Unit,
                        AssociatedTransactionItem = iDb,
                        Cost = iDb.Cost,
                        CostPerUnit = iDb.CostPerUnit,
                        Dimensions = iDb.Dimensions,
                        Discount = iDb.Discount,
                        Price = iDb.Price,
                        PriceBookPrice = iDb.PriceBookPrice,
                        PriceBookPriceValue = iDb.PriceBookPriceValue,
                        Quantity = iDb.Quantity,
                        SalePricePerUnit = iDb.SalePricePerUnit,
                        TraderItem = iDb.TraderItem,
                        TransferItems = iDb.TransferItems
                    };
                    DbContext.Entry(transItemLog).State = EntityState.Added;
                    DbContext.TraderTransactionItemLogs.Add(transItemLog);
                    DbContext.SaveChanges();
                }
                if (itemsNew.Count > 0)
                {
                    traderPurchaseDb.PurchaseItems.AddRange(itemsNew);
                    DbContext.Entry(traderPurchaseDb).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    foreach (var item in itemsNew)
                    {
                        //Whenever a TraderTransactionItem is saved to the database a copy of the TraderTransactionItem is saved to the TransactionItemLog table.
                        var transItemLog = new TransactionItemLog
                        {
                            Unit = item.Unit,
                            AssociatedTransactionItem = item,
                            Cost = item.Cost,
                            CostPerUnit = item.CostPerUnit,
                            Dimensions = item.Dimensions,
                            Discount = item.Discount,
                            Price = item.Price,
                            PriceBookPrice = item.PriceBookPrice,
                            PriceBookPriceValue = item.PriceBookPriceValue,
                            Quantity = item.Quantity,
                            SalePricePerUnit = item.SalePricePerUnit,
                            TraderItem = item.TraderItem,
                            TransferItems = item.TransferItems
                        };
                        DbContext.Entry(transItemLog).State = EntityState.Added;
                        DbContext.TraderTransactionItemLogs.Add(transItemLog);
                    }

                    DbContext.SaveChanges();
                }

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, traderPurchase);
                result.actionVal = 3;
                result.msg = ex.Message;
                return result;
            }
        }

        public void PurchaseApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var tradPurchaseDb = approval.Purchase.FirstOrDefault();
                if (tradPurchaseDb == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        tradPurchaseDb.Status = TraderPurchaseStatusEnum.PendingReview;
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        tradPurchaseDb.Status = TraderPurchaseStatusEnum.PendingApproval;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        tradPurchaseDb.Status = TraderPurchaseStatusEnum.PurchaseApproved;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        tradPurchaseDb.Status = TraderPurchaseStatusEnum.PurchaseDenied;
                        break;
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        tradPurchaseDb.Status = TraderPurchaseStatusEnum.PurchaseDiscarded;
                        break;
                }
                DbContext.Entry(tradPurchaseDb).State = EntityState.Modified;
                DbContext.SaveChanges();

                var purchaseLog = new PurchaseLog
                {
                    AssociatedPurchase = tradPurchaseDb,
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    DeliveryMethod = tradPurchaseDb.DeliveryMethod,
                    Invoices = tradPurchaseDb.Invoices,
                    IsInHouse = false,
                    Location = tradPurchaseDb.Location,
                    PurchaseApprovalProcess = approval,
                    PurchaseItems = tradPurchaseDb.PurchaseItems,
                    PurchaseOrder = tradPurchaseDb.PurchaseOrder,
                    PurchaseTotal = tradPurchaseDb.PurchaseTotal,
                    Status = tradPurchaseDb.Status,
                    Transfer = null,
                    Vendor = tradPurchaseDb.Vendor,
                    Workgroup = tradPurchaseDb.Workgroup
                };

                var purchaseProcessLog = new PurchaseProcessLog
                {
                    AssociatedPurchase = tradPurchaseDb,
                    AssociatedPurchaseLog = purchaseLog,
                    PurchaseStatus = tradPurchaseDb.Status,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = tradPurchaseDb.CreatedBy,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = approval.ApprovedOrDeniedAppBy,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }

                };

                DbContext.TraderPurchaseProcessLogs.Add(purchaseProcessLog);
                DbContext.Entry(purchaseProcessLog).State = EntityState.Added;
                DbContext.SaveChanges();

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }

        public List<ApprovalStatusTimeline> PurchaseApprovalStatusTimeline(int id, string timezone)
        {
            var timeline = new List<ApprovalStatusTimeline>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timezone);
                var logs = DbContext.TraderPurchaseProcessLogs.Where(e => e.AssociatedPurchase.Id == id).OrderByDescending(d => d.CreatedDate).ToList();
                string icon = "";

                foreach (var log in logs)
                {
                    switch (log.PurchaseStatus)
                    {
                        case TraderPurchaseStatusEnum.Draft:
                            icon = "fa fa-info bg-aqua";
                            break;
                        case TraderPurchaseStatusEnum.PendingReview:
                            icon = "fa fa-info bg-yellow";
                            break;
                        case TraderPurchaseStatusEnum.PendingApproval:
                            icon = "fa fa-truck bg-aqua";
                            break;
                        case TraderPurchaseStatusEnum.PurchaseApproved:
                            icon = "fa fa-check bg-green";
                            break;
                        case TraderPurchaseStatusEnum.PurchaseDenied:
                            icon = "fa fa-warning bg-red";
                            break;
                        case TraderPurchaseStatusEnum.PurchaseDiscarded:
                            icon = "fa fa-trash bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timezone).ToShortTimeString(),
                            Status = log.PurchaseStatus.GetDescription(),
                            Icon = icon,
                            UserAvatar = log.CreatedBy.ProfilePic
                        }
                    );
                }

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, timezone);
            }
            return timeline;
        }

        private IQueryable<TraderPurchase> FilteredPurchase(int domainId, int locationId, UserSetting userDateTimeFormat,
            string keyword = "", int workGroupId = 0, string datetime = "", string channel = "",int contactId=0, bool isApproved = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, userDateTimeFormat, keyword,
                        workGroupId, datetime,isApproved);
                var startDate = new DateTime();
                var endDate = new DateTime();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(userDateTimeFormat.Timezone);

                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    datetime.ConvertDaterangeFormat(userDateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    startDate = startDate.ConvertTimeToUtc(tz);
                    endDate = endDate.ConvertTimeToUtc(tz);
                }

                //Create sales and filter by location
                var purchase = DbContext.TraderPurchases.Where(d => d.Location.Domain.Id == domainId);
                purchase = purchase.Where(l => (locationId == 0 || l.Location.Id == locationId));

                #region Filter

                // Filter by list of contacts
                if (contactId > 0)
                {
                    purchase = purchase.Where(s => s.Vendor.Id == contactId);

                }

                //Filter by dates
                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    purchase = purchase.Where(q => q.CreatedDate >= startDate && q.CreatedDate < endDate);
                }


                //Filter by workgroup
                if (workGroupId > 0)
                {
                    purchase = purchase.Where(q => q.Workgroup.Id == workGroupId);
                }


                //Filter by channel
                if (!string.IsNullOrEmpty(channel.Trim()))
                {
                    purchase = purchase.Where(q => q.PurchaseChannel.ToString().ToLower() == channel.Trim().ToLower());
                }


                //If the sales must be filered by Approved
                //Note: Approves => Sale Approved AND Sales Order Issued (If a Sales order is issued it can only be after it is approved.
                if (isApproved)
                {
                    purchase = purchase.Where(q => q.Status == TraderPurchaseStatusEnum.PurchaseApproved || q.Status == TraderPurchaseStatusEnum.PurchaseOrderIssued);
                }


                //Filter by keyword
                keyword = keyword.ToLower().Trim();

                if (!string.IsNullOrEmpty(keyword))
                    purchase = purchase.Where(q =>
                        (q.Workgroup != null && q.Workgroup.Name.ToLower().Contains(keyword))
                        //|| ((q.CreatedDate.Day < 10 ? ("0" + q.CreatedDate.Day.ToString()) : q.CreatedDate.Day.ToString()) + "/" + (q.CreatedDate.Month < 10 ? ("0" + q.CreatedDate.Month.ToString()) : q.CreatedDate.Month.ToString()) + "/" + q.CreatedDate.Year).Contains(keyword)
                        //|| (q.SalesChannel.ToString().ToLower().Contains(keyword))
                        //|| (q.se != null && q.Purchaser.Name.ToLower().Contains(keyword))
                        || (q.Reference != null && q.Reference.FullRef.ToLower().Contains(keyword))
                        //|| q.PurchaseItems.Any(v => v.Dimensions.Any(d => d.Name.ToLower().Contains(keyword)))
                    );
                #endregion

                return purchase;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, userDateTimeFormat, keyword, workGroupId,
                    datetime, channel, isApproved);
                return null;
            }

        }
        public DataTablesResponse GetReportPurchase(IDataTablesRequest requestModel, string userId, int locationId, int domainId,
            string channel,int contactId, string keyword, string datetime, bool isApproved, UserSetting userDateTimeFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, requestModel, userId, locationId, domainId,
                        channel, keyword, datetime, isApproved, userDateTimeFormat);
                //Get the filtered sales, get all sales not just the approved sales
                var currencySettings = new CurrencySettingRules(DbContext).GetCurrencySettingByDomain(domainId);

                var sales = FilteredPurchase(domainId, locationId, userDateTimeFormat, keyword, 0, datetime, channel, contactId, isApproved);
                if (sales == null)
                {
                    return null;
                }

                var totalSale = sales.Count();

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "FullRef":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Channel":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "PurchaseChannel" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Contact":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Vendor.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Location":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Location.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Total":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "PurchaseTotal" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate desc";
                            break;
                    }
                }

                sales = sales.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion

                #region Paging
                var list = sales.Skip(requestModel.Start).Take(requestModel.Length).ToList();


                #endregion
                var dataJson = list.Select(q => new
                {
                    Id = q.Id,
                    FullRef = q.Reference?.FullRef ?? q.Id.ToString(),
                    PurchaseChannel = q.PurchaseChannel.ToString(),
                    Contact = q.Vendor?.Name??"",
                    ContactId = q.Vendor?.Id??0,
                    PurchaseTotal = q.PurchaseTotal.ToDecimalPlace(currencySettings),
                    Status = q.Status.GetDescription(),
                    LabelStatus = getLabelStatus(q.Status),
                    AllowEdit = (q.Workgroup != null
                        && q.Workgroup.Processes.Any(s => s.Name.Equals(TraderProcessName.TraderPurchaseProcessName))
                        && q.Workgroup.Members.Any(s => s.Id == userId)
                        ),
                    Location = q.Location?.Name ?? ""
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalSale, totalSale);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, requestModel, userId, locationId, domainId, channel, keyword,
                    isApproved, userDateTimeFormat);
                return null;
            }

        }
        public string getLabelStatus(TraderPurchaseStatusEnum status)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, status);
            var label = "";
            switch (status)
            {
                case TraderPurchaseStatusEnum.Draft:
                    label = StatusLabelStyle.Draft;
                    break;
                case TraderPurchaseStatusEnum.PurchaseApproved:
                    label = StatusLabelStyle.Approved;
                    break;
                case TraderPurchaseStatusEnum.PurchaseDenied:
                    label = StatusLabelStyle.Denied;
                    break;
                case TraderPurchaseStatusEnum.PurchaseDiscarded:
                    label = StatusLabelStyle.Discarded;
                    break;
                case TraderPurchaseStatusEnum.PendingReview:
                    label = StatusLabelStyle.Pending;
                    break;
                case TraderPurchaseStatusEnum.PendingApproval:
                    label = StatusLabelStyle.Reviewed;
                    break;
                case TraderPurchaseStatusEnum.PurchaseOrderIssued:
                    label = StatusLabelStyle.Approved;
                    break;
            }
            return label;
        }
    }
}
