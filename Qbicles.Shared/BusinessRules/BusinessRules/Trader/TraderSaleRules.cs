using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Qbicles;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.ODS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderSaleRules
    {
        private ApplicationDbContext dbContext;

        public TraderSaleRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public TraderSale GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.TraderSales.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderSale();
            }
        }

        public TraderContact GetContactById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var contact = dbContext.TraderContacts.Find(id);
                if (contact == null) return null;
                return new TraderContact()
                {
                    Id = contact.Id,
                    Name = contact.Name,
                    Address = contact.Address
                };
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderContact();
            }
        }

        public ReturnJsonModel SaveTraderSale(TraderSale traderSale, string countryName, string userId, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, traderSale, countryName);

                var user = dbContext.QbicleUser.Find(userId);

                traderSale.Location = dbContext.TraderLocations.FirstOrDefault(q => q.Id == traderSale.Location.Id);

                traderSale.CreatedDate = DateTime.UtcNow;
                if (traderSale.Reference != null)
                    traderSale.Reference = new TraderReferenceRules(dbContext).GetById(traderSale.Reference.Id);

                if (traderSale.SaleItems.Count > 0)
                {
                    foreach (var item in traderSale.SaleItems)
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
                        item.TraderItem = dbContext.TraderItems.Find(item.TraderItem.Id);
                        if (item.TraderItem != null && traderSale.Location != null && item.PriceBookPrice == null)
                        {
                            var traderId = item.TraderItem.Id;
                            item.PriceBookPrice = dbContext.TraderPrices.FirstOrDefault(q => q.Location.Id == traderSale.Location.Id && q.Item.Id == traderId && q.SalesChannel == Models.Trader.SalesChannel.SalesChannelEnum.Trader);
                        }
                        else if (item.PriceBookPrice != null)
                        {
                            item.PriceBookPrice = dbContext.TraderPrices.Find(item.PriceBookPrice.Id);
                        }
                        if (item.PriceBookPrice != null)
                        {
                            item.PriceBookPriceValue = item.PriceBookPrice.NetPrice;
                        }
                        if (item.Dimensions.Count > 0)
                        {
                            var dimensions = new List<TransactionDimension>();
                            foreach (var d in item.Dimensions)
                            {
                                var dimension = dbContext.TransactionDimensions.Find(d.Id);
                                dimensions.Add(dimension);
                            }
                            item.Dimensions.Clear();
                            item.Dimensions.AddRange(dimensions);
                        }
                        if (item.Unit != null)
                        {
                            item.Unit =
                                dbContext.ProductUnits.Find(item.Unit.Id);
                        }
                        //Update Taxes TraderTransactionItem
                        if (item.TraderItem != null)
                        {
                            foreach (var tax in item.TraderItem.TaxRates.Where(s => !s.IsPurchaseTax).ToList())
                            {
                                var staticTaxRate = new TaxRateRules(dbContext).CloneStaticTaxRateById(tax.Id);
                                OrderTax orderTax = new OrderTax
                                {
                                    StaticTaxRate = staticTaxRate,
                                    TaxRate = tax,
                                    Value = item.SalePricePerUnit * (1 - (item.Discount / 100)) * (tax.Rate / 100)
                                };
                                item.Taxes.Add(orderTax);
                            }
                        }
                    }
                }

                if (traderSale.Purchaser.Id != 0)
                {
                    traderSale.Purchaser = dbContext.TraderContacts.Find(traderSale.Purchaser.Id);
                    if (traderSale.Purchaser != null) traderSale.Purchaser.InUsed = true;
                    if (traderSale.Purchaser?.Address != null && traderSale.Purchaser.Address.Id > 0 && traderSale.DeliveryMethod != DeliveryMethodEnum.Delivery)
                    {
                        traderSale.DeliveryAddress = traderSale.Purchaser.Address;
                    }
                }
                if (traderSale.DeliveryAddress.Id != 0)
                {
                    traderSale.DeliveryAddress = dbContext.TraderAddress.Find(traderSale.DeliveryAddress.Id);
                }
                else if (traderSale.DeliveryAddress.Country != null && traderSale.DeliveryAddress.Country.CommonName != "")
                {
                    traderSale.DeliveryAddress.Country = new CountriesRules().GetCountryByName(countryName);
                }
                if (traderSale.Workgroup != null && traderSale.Workgroup.Id > 0)
                {
                    traderSale.Workgroup = dbContext.WorkGroups.Find(traderSale.Workgroup.Id);
                }

                if (traderSale.DeliveryMethod != DeliveryMethodEnum.Delivery)
                    traderSale.DeliveryAddress = null;

                if (traderSale.Id == 0)
                {
                    traderSale.CreatedBy = user;
                    if (traderSale.DeliveryMethod != DeliveryMethodEnum.Delivery)
                        traderSale.DeliveryAddress = null;
                    dbContext.Entry(traderSale).State = EntityState.Added;
                    dbContext.TraderSales.Add(traderSale);
                    dbContext.SaveChanges();

                    foreach (var item in traderSale.SaleItems)
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
                        dbContext.Entry(transItemLog).State = EntityState.Added;
                        dbContext.TraderTransactionItemLogs.Add(transItemLog);
                    }

                    dbContext.SaveChanges();

                    result.msgId = traderSale.Id.ToString();
                }
                else
                {
                    var traderSaleDb = dbContext.TraderSales.Find(traderSale.Id);
                    if (traderSale.Reference != null)
                    {
                        traderSaleDb.Reference = traderSale.Reference;
                    }
                    traderSaleDb.Status = traderSale.Status;
                    traderSaleDb.DeliveryAddress = traderSale.DeliveryAddress;
                    traderSaleDb.DeliveryMethod = traderSale.DeliveryMethod;
                    traderSaleDb.Purchaser = traderSale.Purchaser;
                    traderSaleDb.SaleTotal = traderSale.SaleTotal;
                    traderSaleDb.Workgroup = traderSale.Workgroup;
                    dbContext.Entry(traderSaleDb).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    //Update Transaction Items

                    var itemsUi = traderSale.SaleItems;
                    var itemsDb = traderSaleDb.SaleItems;

                    var itemsNew = itemsUi.Where(c => c.Id == 0).ToList();

                    var itemsDelete = itemsDb.Where(c => itemsUi.All(d => c.Id != d.Id)).ToList();

                    var itemsUpdate = itemsUi.Where(c => itemsDb.Any(d => c.Id == d.Id)).ToList();

                    foreach (var itemDel in itemsDelete)
                    {
                        //remove Order Tax
                        if (itemDel.Taxes != null && itemDel.Taxes.Any())
                            dbContext.OrderTaxs.RemoveRange(itemDel.Taxes);
                        if (itemDel.Logs.Any())
                        {
                            dbContext.TraderTransactionItemLogs.RemoveRange(itemDel.Logs);
                        }
                        traderSaleDb.SaleItems.Remove(itemDel);
                        dbContext.Entry(traderSaleDb).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }

                    foreach (var iDb in traderSaleDb.SaleItems)
                    {
                        var iUpdate = itemsUpdate.FirstOrDefault(e => e.Id == iDb.Id);
                        if (iUpdate == null) continue;
                        iDb.Dimensions.Clear();
                        if (iDb.Logs.Any())
                        {
                            dbContext.TraderTransactionItemLogs.RemoveRange(iDb.Logs);
                        }
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
                            dbContext.OrderTaxs.RemoveRange(iDb.Taxes);
                        iDb.Taxes = iUpdate.Taxes;

                        dbContext.Entry(iDb).State = EntityState.Modified;
                        dbContext.SaveChanges();

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
                        dbContext.Entry(transItemLog).State = EntityState.Added;
                        dbContext.TraderTransactionItemLogs.Add(transItemLog);
                        dbContext.SaveChanges();
                    }
                    if (itemsNew.Count > 0)
                    {
                        traderSaleDb.SaleItems.AddRange(itemsNew);
                        dbContext.Entry(traderSaleDb).State = EntityState.Modified;
                        dbContext.SaveChanges();
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
                            dbContext.Entry(transItemLog).State = EntityState.Added;
                            dbContext.TraderTransactionItemLogs.Add(transItemLog);
                        }

                        dbContext.SaveChanges();
                    }

                    result.msgId = traderSale.Id.ToString();
                    result.actionVal = 2;
                }

                var tradSaleDb = dbContext.TraderSales.Find(traderSale.Id);

                if (tradSaleDb?.SaleApprovalProcess != null)
                    return result;

                if (tradSaleDb == null || tradSaleDb.Status != TraderSaleStatusEnum.PendingReview) return result;
                tradSaleDb.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var appDef =
                    dbContext.SalesApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == tradSaleDb.Workgroup.Id);
                var refFull = tradSaleDb.Reference == null ? "" : tradSaleDb.Reference.FullRef;
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Sale = new List<TraderSale> { tradSaleDb },
                    Name = $"Trader Approval for Sale #{refFull}",
                    Qbicle = tradSaleDb.Workgroup.Qbicle,
                    Topic = tradSaleDb.Workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = user,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader
                };
                tradSaleDb.SaleApprovalProcess = approval;
                tradSaleDb.SaleApprovalProcess.ApprovalRequestDefinition = appDef;
                approval.ActivityMembers.AddRange(tradSaleDb.Workgroup.Members);
                dbContext.Entry(tradSaleDb).State = EntityState.Modified;
                dbContext.SaveChanges();

                var saleLog = new SaleLog
                {
                    AssociatedSale = tradSaleDb,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    DeliveryAddress = tradSaleDb.DeliveryAddress,
                    DeliveryMethod = tradSaleDb.DeliveryMethod,
                    Invoices = tradSaleDb.Invoices,
                    IsInHouse = false,
                    Location = tradSaleDb.Location,
                    Purchaser = tradSaleDb.Purchaser,
                    SaleApprovalProcess = approval,
                    SaleItems = tradSaleDb.SaleItems,
                    SaleTotal = tradSaleDb.SaleTotal,
                    SalesChannel = tradSaleDb.SalesChannel,
                    SalesOrders = tradSaleDb.SalesOrders,
                    Status = tradSaleDb.Status,
                    Transfer = tradSaleDb.Transfer,
                    Workgroup = tradSaleDb.Workgroup
                };

                var saleProcessLog = new SaleProcessLog
                {
                    AssociatedSale = tradSaleDb,
                    AssociatedSaleLog = saleLog,
                    SaleStatus = tradSaleDb.Status,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                dbContext.TraderSaleProcessLogs.Add(saleProcessLog);
                dbContext.Entry(saleProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = Notification.NotificationEventEnum.ApprovalCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = HelperClass.GetFullNameOfUser(approval.StartedBy),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2Activity(activityNotification);
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, traderSale, countryName);
                result.actionVal = 3;
                result.msg = ex.Message;
                return result;
            }
        }

        public List<ApprovalStatusTimeline> SaleApprovalStatusTimeline(int id, string timeZone)
        {
            var timeline = new List<ApprovalStatusTimeline>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);
                var logs = dbContext.TraderSaleProcessLogs.Where(e => e.AssociatedSale.Id == id).OrderByDescending(d => d.CreatedDate).ToList();
                string icon = "";

                foreach (var log in logs)
                {
                    switch (log.SaleStatus)
                    {
                        case TraderSaleStatusEnum.PendingReview:
                            icon = "fa fa-info bg-aqua";
                            break;

                        case TraderSaleStatusEnum.PendingApproval:
                            icon = "fa fa-truck bg-yellow";
                            break;

                        case TraderSaleStatusEnum.SaleApproved:
                            icon = "fa fa-check bg-green";
                            break;

                        case TraderSaleStatusEnum.Draft:
                            icon = "fa fa-warning bg-yellow";
                            break;

                        case TraderSaleStatusEnum.SaleDenied:
                            icon = "fa fa-warning bg-red";
                            break;

                        case TraderSaleStatusEnum.SaleDiscarded:
                            icon = "fa fa-trash bg-red";
                            break;

                        case TraderSaleStatusEnum.SalesOrderedIssued:
                            icon = "fa fa-check bg-green";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,//.ConvertTimeFromUtc(timeZone),
                            Time = log.CreatedDate.ConvertTimeFromUtc(timeZone).ToString("hh:mmtt").ToLower(),
                            Status = log.SaleStatus.GetDescription(),
                            Icon = icon,
                            UserAvatar = log.CreatedBy.ProfilePic
                        }
                    );
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, timeZone);
            }
            return timeline;
        }

        public List<TraderSale> GetByLocation(int locationId, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, domainId);
                return dbContext.TraderSales.Where(l => l.Location.Id == locationId && l.Location.Domain.Id == domainId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId, domainId);
                return new List<TraderSale>();
            }
        }

        public DataTablesResponse GetByLocationPagination(int locationId, int domainId, string daterange, string timezone,
            IDataTablesRequest requestModel, string formatDate)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), MethodBase.GetCurrentMethod().Name, null, null, locationId, domainId);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var query = dbContext.TraderSales.Where(l => l.Location.Id == locationId && l.Location.Domain.Id == domainId && (l.Status == TraderSaleStatusEnum.SaleApproved ||
                     l.Status == TraderSaleStatusEnum.SalesOrderedIssued));
                int totalSale = 0;

                #region Filter

                var keyword = requestModel.Search != null ? requestModel.Search.Value.Replace("#", "") : "";
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q =>
                        q.Purchaser != null && q.Purchaser.Name.ToLower().Contains(keyword)
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
                    daterange.ConvertDaterangeFormat(formatDate, "", out startDate, out endDate);
                    startDate = startDate.AddTicks(1);
                    endDate = endDate.AddDays(1).AddTicks(-1);
                    query = query.Where(q => q.CreatedDate >= startDate && q.CreatedDate <= endDate);
                }
                totalSale = query.Count();

                #endregion Filter

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
                            orderByString += "Purchaser.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                #endregion Sorting

                #region Paging

                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(q => new TraderSaleCustom
                {
                    Id = q.Id,
                    Key = q.Key ?? "",
                    FullRef = q.Reference?.FullRef,
                    WorkgroupName = q.Workgroup?.Name,
                    CreatedDate = q.CreatedDate.ConvertTimeFromUtc(timezone).ToString(formatDate),
                    Contact = q.Purchaser?.Name,
                    Dimensions = q.SaleItems.Count == 0 ? "" : string.Join(", ", q.SaleItems.Where(c => c.Dimensions.Any()).SelectMany(b => b.Dimensions.Select(v => v.Name)).Distinct()),
                    SaleTotal = q.SaleTotal.ToDecimalPlace(currencySettings),
                    Status = q.Status.ToString(),
                    TransferCount = q.Transfer.Count()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalSale, totalSale);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, domainId, daterange, timezone, requestModel, formatDate);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        public List<WorkGroup> GetWorkGroups(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);
                return dbContext.TraderSales.Where(d => d.Location.Id == locationId)
                    .Select(q => q.Workgroup).Distinct().OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationId);
                return new List<WorkGroup>();
            }
        }

        public List<TraerItemByGroup> GetItemsByGroupId(List<int> Ids, int domainId, int locationId, UserSetting userDateTimeFormat, string datetime = "", string timeZone = "", bool isApproved = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, Ids, locationId, datetime, timeZone, userDateTimeFormat);

                #region Get the date range

                var startDate = new DateTime();
                var endDate = new DateTime();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    datetime.ConvertDaterangeFormat(userDateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    startDate = startDate.ConvertTimeToUtc(tz);
                    endDate = endDate.ConvertTimeToUtc(tz);
                }

                #endregion Get the date range

                var traderSalesList = dbContext.TraderSales.Where(q => q.Location.Domain.Id == domainId && (locationId == 0 || q.Location.Id == locationId));
                if (isApproved)
                {
                    traderSalesList = traderSalesList.Where(q => q.Status == TraderSaleStatusEnum.SaleApproved || q.Status == TraderSaleStatusEnum.SalesOrderedIssued);
                }

                var items = traderSalesList.Where(q => startDate <= q.CreatedDate && endDate > q.CreatedDate)
                    .SelectMany(d => d.SaleItems.Where(q => q.TraderItem != null && Ids.Contains(q.TraderItem.Id)))
                    .Select(q => new
                    {
                        q.TraderItem,
                        q.Price,
                        q.Cost,
                        q.Quantity
                    }).ToList();
                var itemGroup = items.GroupBy(q => q.TraderItem.Id).Select(q => new TraerItemByGroup
                {
                    TraderItem = q.FirstOrDefault()?.TraderItem,
                    Price = q.Sum(s => s.Price).ToString("N2"),
                    Cost = q.Sum(s => s.Cost).ToString("N2"),
                    Quantity = q.Sum(s => s.Quantity).ToString("N2"),
                    Margin = (q.Sum(s => s.Price) - q.Sum(s => s.Cost)).ToString("N2")
                }).ToList();
                return itemGroup;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, Ids, locationId);
                return new List<TraerItemByGroup>();
            }
        }

        public TraderSaleDashBoard GetDataDashBoard(int domainId, int locationId, UserSetting dateTimeFormat,
            string keyword = "", int workGroupId = 0, string datetime = "", string channel = "", int contactId = 0)
        {
            var saleResult = new TraderSaleDashBoard();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, dateTimeFormat, keyword,
                        workGroupId, datetime, channel);
                //Get the filtered sales
                var sales = FilteredSales(domainId, locationId, dateTimeFormat, keyword, workGroupId, datetime, channel, contactId, isApproved: true);

                if (sales == null)
                {
                    return new TraderSaleDashBoard();
                }

                if (sales.Any())
                {
                    var saleItems = sales.SelectMany(q => q.SaleItems.Where(s => s.TraderItem != null))
                        .Select(q => new
                        {
                            TraderItemId = q.TraderItem.Id,
                            GroupId = q.TraderItem.Group.Id,
                            GroupName = q.TraderItem.Group.Name,
                            q.Price,
                            q.Cost
                        }).ToList();
                    var totalPrice = saleItems.Sum(q => q.Price);

                    saleResult.TopSells = saleItems.GroupBy(g => g.GroupName).Select(q => new TopValueSale
                    {
                        GroupId = q.FirstOrDefault()?.GroupId ?? 0,
                        TraderItemIds = string.Join("-", q.Select(s => s.TraderItemId)),
                        GroupName = q.FirstOrDefault()?.GroupName,
                        Percent = (totalPrice > 0 ? Math.Round(q.Sum(s => s.Price) / totalPrice * 100, 2).ToString(CultureInfo.InvariantCulture) : "0"),
                        Value = q.Sum(s => s.Price).ToString("N2"),
                        PercentInt = totalPrice > 0 ? Math.Round(q.Sum(s => s.Price) / totalPrice * 100, 2) : 0,
                        ValueInt = q.Sum(s => s.Price)
                    }).OrderByDescending(o => o.ValueInt).ToList();

                    saleResult.TopMargin = saleItems.GroupBy(g => g.GroupName).Select(q => new TopValueSale
                    {
                        GroupId = q.FirstOrDefault()?.GroupId ?? 0,
                        TraderItemIds = string.Join("-", q.Select(s => s.TraderItemId)),
                        GroupName = q.FirstOrDefault()?.GroupName,
                        Percent = q.Sum(s => s.Price) > 0 ? Math.Round((q.Sum(s => s.Price) - q.Sum(s => s.Cost)) / q.Sum(s => s.Price) * 100, 2).ToString(CultureInfo.InvariantCulture) : "0",
                        PercentInt = q.Sum(s => s.Price) > 0 ? Math.Round((q.Sum(s => s.Price) - q.Sum(s => s.Cost)) / q.Sum(s => s.Price) * 100, 2) : 0,
                    }).OrderByDescending(o => o.PercentInt).ToList();

                    saleResult.TopGrossMargion = saleItems.GroupBy(g => g.GroupName).Select(q => new TopValueSale
                    {
                        GroupId = q.FirstOrDefault()?.GroupId ?? 0,
                        TraderItemIds = string.Join("-", q.Select(s => s.TraderItemId)),
                        GroupName = q.FirstOrDefault()?.GroupName,
                        Value = (q.Sum(s => s.Price) - q.Sum(s => s.Cost)).ToString("N2"),
                        ValueInt = q.Sum(s => s.Price) - q.Sum(s => s.Cost)
                    }).OrderByDescending(o => o.ValueInt).ToList();

                    saleResult.TotalSaleValue = sales.SelectMany(q => q.SaleItems.Where(t => t.TraderItem != null).Select(i => i.Price)).Sum(q => q)
                        .ToString("N2");
                    saleResult.TotalApproved = sales.Count().ToString();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, dateTimeFormat, keyword, workGroupId, datetime, channel);
                return new TraderSaleDashBoard();
            }

            return saleResult;
        }

        private IQueryable<TraderSale> FilteredSales(int domainId, int locationId, UserSetting userDateTimeFormat,
            string keyword = "", int workGroupId = 0, string datetime = "", string channel = "", int contactId = 0, bool isApproved = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, userDateTimeFormat, keyword,
                        workGroupId, datetime, channel, isApproved);

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
                var sales = dbContext.TraderSales.Where(d => d.Location.Domain.Id == domainId);
                sales = sales.Where(l => (locationId == 0 || l.Location.Id == locationId));

                #region Filter

                // Filter by list of contacts
                if (contactId > 0)
                {
                    sales = sales.Where(s => s.Purchaser.Id == contactId);
                }

                //Filter by dates
                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    sales = sales.Where(q => q.CreatedDate >= startDate && q.CreatedDate < endDate);
                }

                //Filter by workgroup
                if (workGroupId > 0)
                {
                    sales = sales.Where(q => q.Workgroup.Id == workGroupId);
                }

                //Filter by channel
                if (!string.IsNullOrEmpty(channel.Trim()))
                {
                    sales = sales.Where(q => q.SalesChannel.ToString().ToLower() == channel.Trim().ToLower());
                }

                //If the sales must be filered by Approved
                //Note: Approves => Sale Approved AND Sales Order Issued (If a Sales order is issued it can only be after it is approved.
                if (isApproved)
                {
                    sales = sales.Where(q => q.Status == TraderSaleStatusEnum.SaleApproved || q.Status == TraderSaleStatusEnum.SalesOrderedIssued);
                }

                //Filter by keyword
                keyword = keyword.ToLower().Trim();

                if (!string.IsNullOrEmpty(keyword))
                    sales = sales.Where(q =>
                        (q.Workgroup != null && q.Workgroup.Name.ToLower().Contains(keyword))
                        || ((q.CreatedDate.Day < 10 ? ("0" + q.CreatedDate.Day.ToString()) : q.CreatedDate.Day.ToString()) + "/" + (q.CreatedDate.Month < 10 ? ("0" + q.CreatedDate.Month.ToString()) : q.CreatedDate.Month.ToString()) + "/" + q.CreatedDate.Year).Contains(keyword)
                        || (q.SalesChannel.ToString().ToLower().Contains(keyword))
                        || (q.Purchaser != null && q.Purchaser.Name.ToLower().Contains(keyword))
                        || (q.Reference != null && q.Reference.FullRef.ToLower().Contains(keyword))
                        || q.SaleItems.Any(v => v.Dimensions.Any(d => d.Name.ToLower().Contains(keyword)))
                    );

                #endregion Filter

                return sales;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, userDateTimeFormat, keyword, workGroupId,
                    datetime, channel, isApproved);
                return null;
            }
        }

        public DataTablesResponse TraderSaleSearch(IDataTablesRequest requestModel, string userId, int locationId, int domainId,
            string channel, string keyword, int wgId, string datetime, string timeZone, bool isApproved, UserSetting userDateTimeFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, requestModel, userId, locationId, domainId,
                        channel, keyword, wgId, datetime, timeZone, isApproved, userDateTimeFormat);
                //Get the filtered sales, get all sales not just the approved sales
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var sales = FilteredSales(domainId, locationId, userDateTimeFormat, keyword, wgId, datetime, channel, 0, isApproved);

                if (sales == null)
                {
                    return null;
                }

                var totalSale = sales.Count();

                var workGroupIds = dbContext.WorkGroups.Where(q =>
                    q.Location.Id == locationId
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderSaleProcessName))
                    && q.Members.Select(u => u.Id).Contains(userId)).Select(q => q.Id).ToList();

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
                            orderByString += "Purchaser.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                sales = sales.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = sales.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(q => new TraderSaleCustom
                {
                    Id = q.Id,
                    Key = q.Key ?? "",
                    FullRef = q.Reference?.FullRef,
                    WorkgroupName = q.Workgroup?.Name,
                    CreatedDate = q.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(userDateTimeFormat.DateFormat),
                    SalesChannel = q.SalesChannel.ToString(),
                    Contact = q.Purchaser?.Name,
                    Dimensions = q.SaleItems.Count == 0 ? "" : string.Join(", ", q.SaleItems.Where(c => c.Dimensions.Any()).SelectMany(b => b.Dimensions.Select(v => v.Name)).Distinct()),
                    SaleTotal = q.SaleTotal.ToDecimalPlace(currencySettings),
                    Status = q.Status.GetDescription(),
                    LabelStatus = getLabelStatus(q.Status),
                    AllowEdit = q.Workgroup != null && workGroupIds.Contains(q.Workgroup.Id),
                    SaleOrderId = q.SalesOrders.FirstOrDefault()?.Id.ToString() ?? "",
                    SaleOderRef = q.SalesOrders.FirstOrDefault()?.Reference?.FullRef ?? "",
                    ApprovedOn = q.SaleApprovalProcess?.StartedDate.DatetimeToOrdinalAndTime()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalSale, totalSale);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, requestModel, userId, locationId, domainId, channel, keyword,
                    wgId, timeZone, isApproved, userDateTimeFormat);
                return null;
            }
        }

        public string getLabelStatus(TraderSaleStatusEnum status)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, status);
            var label = "";
            switch (status)
            {
                case TraderSaleStatusEnum.Draft:
                    label = StatusLabelStyle.Draft;
                    break;

                case TraderSaleStatusEnum.SaleApproved:
                    label = StatusLabelStyle.Approved;
                    break;

                case TraderSaleStatusEnum.SaleDenied:
                    label = StatusLabelStyle.Denied;
                    break;

                case TraderSaleStatusEnum.SaleDiscarded:
                    label = StatusLabelStyle.Discarded;
                    break;

                case TraderSaleStatusEnum.PendingReview:
                    label = StatusLabelStyle.Pending;
                    break;

                case TraderSaleStatusEnum.PendingApproval:
                    label = StatusLabelStyle.Reviewed;
                    break;

                case TraderSaleStatusEnum.SalesOrderedIssued:
                    label = StatusLabelStyle.Approved;
                    break;
            }
            return label;
        }

        public ReturnJsonModel UpdateTraderSaleContact(TraderSale traderSale, string countryName)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, traderSale, countryName);
                if (traderSale.Purchaser.Id != 0)
                {
                    traderSale.Purchaser = dbContext.TraderContacts.Find(traderSale.Purchaser.Id);
                    if (traderSale.Purchaser != null)
                        traderSale.Purchaser.InUsed = true;
                    if (traderSale.Purchaser?.Address != null && traderSale.Purchaser.Address.Id > 0 && traderSale.DeliveryMethod != DeliveryMethodEnum.Delivery)
                    {
                        traderSale.DeliveryAddress = traderSale.Purchaser.Address;
                    }
                    else if (traderSale.DeliveryAddress.Id != 0 && traderSale.DeliveryMethod == DeliveryMethodEnum.Delivery)
                    {
                        var add = traderSale.DeliveryAddress;
                        if (string.IsNullOrEmpty(add.AddressLine1) && string.IsNullOrEmpty(add.AddressLine2) &&
                            string.IsNullOrEmpty(add.PostCode) && string.IsNullOrEmpty(add.State) &&
                            string.IsNullOrEmpty(add.City) &&
                            add.Country == null)
                        {
                            if (traderSale.Purchaser != null) traderSale.DeliveryAddress = traderSale.Purchaser.Address;
                        }
                    }
                }
                //update sale contact

                var tradSale = dbContext.TraderSales.Find(traderSale.Id);
                if (tradSale == null) return result;
                if (!string.IsNullOrEmpty(countryName))
                {
                    if (traderSale.DeliveryAddress != null)
                        traderSale.DeliveryAddress.Country =
                            Country.All.FirstOrDefault(s => s.CommonName == countryName);
                }
                tradSale.DeliveryAddress = traderSale.DeliveryAddress;
                tradSale.DeliveryMethod = traderSale.DeliveryMethod;
                tradSale.Purchaser = traderSale.Purchaser;

                dbContext.Entry(tradSale).State = EntityState.Modified;
                dbContext.SaveChanges();
                result.msgId = traderSale.Id.ToString();
                // end update

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, traderSale, countryName);
                result.actionVal = 3;
                result.msg = ex.Message;
                return result;
            }
        }

        public ReturnJsonModel UpdateTraderSaleItems(TraderSale traderSale, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, traderSale);
                //var transactionItemLogs = new List<TransactionItemLog>();

                var user = dbContext.QbicleUser.Find(userId);

                if (traderSale.SaleItems.Count > 0)
                {
                    foreach (var item in traderSale.SaleItems)
                    {
                        item.TraderItem = dbContext.TraderItems.Find(item.TraderItem.Id);
                        if (item.TraderItem != null && traderSale.Location != null && item.PriceBookPrice == null)
                        {
                            var traderId = item.TraderItem.Id;
                            item.PriceBookPrice = dbContext.TraderPrices.FirstOrDefault(q => q.Location.Id == traderSale.Location.Id && q.Item.Id == traderId && q.SalesChannel == Models.Trader.SalesChannel.SalesChannelEnum.Trader);
                        }
                        else if (item.PriceBookPrice != null)
                        {
                            item.PriceBookPrice = dbContext.TraderPrices.Find(item.PriceBookPrice.Id);
                        }

                        if (item.PriceBookPrice != null)
                        {
                            item.PriceBookPriceValue = item.PriceBookPrice.NetPrice;
                        }
                        if (item.Dimensions.Count > 0)
                        {
                            for (int j = 0; j < item.Dimensions.Count; j++)
                            {
                                item.Dimensions[j] =
                                    dbContext.TransactionDimensions.Find(item.Dimensions[j].Id);
                            }
                        }
                        if (item.Unit != null)
                        {
                            item.Unit =
                                dbContext.ProductUnits.Find(item.Unit.Id);
                        }
                        //Update Taxes TraderTransactionItem
                        if (item.TraderItem != null)
                        {
                            foreach (var tax in item.TraderItem.TaxRates.Where(s => !s.IsPurchaseTax).ToList())
                            {
                                var staticTaxRate = new TaxRateRules(dbContext).CloneStaticTaxRateById(tax.Id);
                                OrderTax orderTax = new OrderTax
                                {
                                    StaticTaxRate = staticTaxRate,
                                    TaxRate = tax,
                                    Value = item.SalePricePerUnit * (1 - (item.Discount / 100)) * (tax.Rate / 100)
                                };
                                item.Taxes.Add(orderTax);
                            }
                        }
                    }
                }
                var traderSaleDb = dbContext.TraderSales.Find(traderSale.Id);
                if (traderSaleDb == null) return result;
                traderSaleDb.SaleTotal = traderSale.SaleTotal;
                dbContext.Entry(traderSaleDb).State = EntityState.Modified;
                dbContext.SaveChanges();

                //Update Transaction Items

                var itemsUi = traderSale.SaleItems;
                var itemsDb = traderSaleDb.SaleItems;

                var itemsNew = itemsUi.Where(c => itemsDb.All(d => c.Id != d.Id)).ToList();

                var itemsDelete = itemsDb.Where(c => itemsUi.All(d => c.Id != d.Id)).ToList();

                var itemsUpdate = itemsUi.Where(c => itemsDb.Any(d => c.Id == d.Id)).ToList();

                foreach (var itemDel in itemsDelete)
                {
                    //remove Order Tax
                    if (itemDel.Taxes != null && itemDel.Taxes.Any())
                        dbContext.OrderTaxs.RemoveRange(itemDel.Taxes);
                    traderSaleDb.SaleItems.Remove(itemDel);
                    dbContext.Entry(traderSaleDb).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }

                foreach (var iDb in traderSaleDb.SaleItems)
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
                        dbContext.OrderTaxs.RemoveRange(iDb.Taxes);
                    iDb.Taxes = iUpdate.Taxes;

                    dbContext.Entry(iDb).State = EntityState.Modified;
                    dbContext.SaveChanges();

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
                    dbContext.Entry(transItemLog).State = EntityState.Added;
                    dbContext.TraderTransactionItemLogs.Add(transItemLog);
                    dbContext.SaveChanges();
                }
                if (itemsNew.Count > 0)
                {
                    traderSaleDb.SaleItems.AddRange(itemsNew);
                    dbContext.Entry(traderSaleDb).State = EntityState.Modified;
                    dbContext.SaveChanges();
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
                        dbContext.Entry(transItemLog).State = EntityState.Added;
                        dbContext.TraderTransactionItemLogs.Add(transItemLog);
                    }

                    dbContext.SaveChanges();
                }

                result.msgId = traderSale.Id.ToString();

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, traderSale);
                result.actionVal = 3;
                result.msg = ex.Message;
                return result;
            }
        }

        public void SaleApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var sale = approval.Sale.FirstOrDefault();
                if (sale == null) return;
                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        sale.Status = TraderSaleStatusEnum.PendingReview;
                        break;

                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        sale.Status = TraderSaleStatusEnum.PendingApproval;
                        break;

                    case ApprovalReq.RequestStatusEnum.Approved:
                        sale.Status = TraderSaleStatusEnum.SaleApproved;
                        break;

                    case ApprovalReq.RequestStatusEnum.Denied:
                        sale.Status = TraderSaleStatusEnum.SaleDenied;
                        break;

                    case ApprovalReq.RequestStatusEnum.Discarded:
                        sale.Status = TraderSaleStatusEnum.SaleDiscarded;
                        break;
                }
                dbContext.Entry(sale).State = EntityState.Modified;
                dbContext.SaveChanges();

                var saleLog = new SaleLog
                {
                    AssociatedSale = sale,
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    DeliveryAddress = sale.DeliveryAddress,
                    DeliveryMethod = sale.DeliveryMethod,
                    Invoices = sale.Invoices,
                    IsInHouse = false,
                    Location = sale.Location,
                    Purchaser = sale.Purchaser,
                    SaleApprovalProcess = approval,
                    SaleItems = sale.SaleItems,
                    SaleTotal = sale.SaleTotal,
                    SalesChannel = sale.SalesChannel,
                    SalesOrders = sale.SalesOrders,
                    Status = sale.Status,
                    Transfer = sale.Transfer,
                    Workgroup = sale.Workgroup
                };

                var saleProcessLog = new SaleProcessLog
                {
                    AssociatedSale = sale,
                    AssociatedSaleLog = saleLog,
                    SaleStatus = sale.Status,
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = approval.ApprovedOrDeniedAppBy,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    }
                };

                dbContext.TraderSaleProcessLogs.Add(saleProcessLog);
                dbContext.Entry(saleProcessLog).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }

        public DataTablesResponse GetReportSales(IDataTablesRequest requestModel, string userId, int locationId, int domainId,
            string channel, int contactId, string keyword, string datetime, bool isApproved, UserSetting userDateTimeFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, requestModel, userId, locationId, domainId,
                        channel, keyword, datetime, isApproved, userDateTimeFormat);
                //Get the filtered sales, get all sales not just the approved sales
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                var sales = FilteredSales(domainId, locationId, userDateTimeFormat, keyword, 0, datetime, channel, contactId, isApproved);
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

                        case "SalesChannel":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SalesChannel" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Contact":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Purchaser.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Location":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Location.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                sales = sales.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = sales.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(q => new
                {
                    Id = q.Id,
                    Key = q.Key,
                    FullRef = q.Reference?.FullRef ?? q.Id.ToString(),
                    SalesChannel = q.SalesChannel.ToString(),
                    Contact = q.Purchaser?.Name,
                    ContactId = q.Purchaser?.Id ?? 0,
                    SaleTotal = q.SaleTotal.ToDecimalPlace(currencySettings),
                    Status = q.Status.GetDescription(),
                    LabelStatus = getLabelStatus(q.Status),
                    AllowEdit = (q.Workgroup != null
                        && q.Workgroup.Processes.Any(s => s.Name.Equals(TraderProcessName.TraderSaleProcessName))
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

        public void CreateSaleLog(TraderSale sale, ApprovalReq approval)
        {
            var saleLog = new SaleLog
            {
                AssociatedSale = sale,
                CreatedBy = approval.ApprovedOrDeniedAppBy,
                CreatedDate = DateTime.UtcNow,
                DeliveryAddress = sale.DeliveryAddress,
                DeliveryMethod = sale.DeliveryMethod,
                Invoices = sale.Invoices,
                IsInHouse = false,
                Location = sale.Location,
                Purchaser = sale.Purchaser,
                SaleApprovalProcess = approval,
                SaleItems = sale.SaleItems,
                SaleTotal = sale.SaleTotal,
                SalesChannel = sale.SalesChannel,
                SalesOrders = sale.SalesOrders,
                Status = sale.Status,
                Transfer = sale.Transfer,
                Workgroup = sale.Workgroup
            };

            var saleProcessLog = new SaleProcessLog
            {
                AssociatedSale = sale,
                AssociatedSaleLog = saleLog,
                SaleStatus = sale.Status,
                CreatedBy = approval.ApprovedOrDeniedAppBy,
                CreatedDate = DateTime.UtcNow,
                ApprovalReqHistory = new ApprovalReqHistory
                {
                    ApprovalReq = approval,
                    UpdatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    RequestStatus = approval.RequestStatus
                }
            };

            dbContext.TraderSaleProcessLogs.Add(saleProcessLog);
            dbContext.Entry(saleProcessLog).State = EntityState.Added;
            dbContext.SaveChanges();
        }
    }
}