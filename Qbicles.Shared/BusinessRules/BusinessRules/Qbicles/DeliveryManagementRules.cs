using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class DeliveryManagementRules
    {
        private ApplicationDbContext dbContext;

        public DeliveryManagementRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public object GetDeliveries(IDataTablesRequest requestModel, int domainId, DeliveryParameter parameter, UserSetting userSetting)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, requestModel, parameter, domainId);


                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                var deliveries = dbContext.Deliveries.Where(e => e.DeliveryQueue.Location.Domain.Id == domainId || e.DeliveryQueueArchive.Location.Domain.Id == domainId);

                if (!string.IsNullOrEmpty(parameter.Keyword))
                {
                    try
                    {
                        var dStatus = (DeliveryStatus)Enum.Parse(typeof(DeliveryStatus), char.ToUpper(parameter.Keyword[0]) + parameter.Keyword.Substring(1));
                        deliveries = deliveries.Where(d => (d.Reference != null && d.Reference.FullRef.Contains(parameter.Keyword))
                                    || (d.Driver != null && d.Driver.User.User.DisplayUserName.Contains(parameter.Keyword))
                                    || (d.Driver != null && d.Driver.User.User.Forename.Contains(parameter.Keyword))
                                    || (d.Driver != null && d.Driver.User.User.Surname.Contains(parameter.Keyword))
                                    || (d.DriverArchived != null && d.DriverArchived.User.User.DisplayUserName.Contains(parameter.Keyword))
                                    || (d.DriverArchived != null && d.DriverArchived.User.User.Forename.Contains(parameter.Keyword))
                                    || (d.DriverArchived != null && d.DriverArchived.User.User.Surname.Contains(parameter.Keyword))
                                    || d.DeliveryQueue.Location.Name.Contains(parameter.Keyword)
                                    || d.Status == dStatus
                                    );
                    }
                    catch
                    {
                        deliveries = deliveries.Where(d => (d.Reference != null && d.Reference.FullRef.Contains(parameter.Keyword))
                                    || (d.Driver != null && d.Driver.User.User.DisplayUserName.Contains(parameter.Keyword))
                                    || (d.Driver != null && d.Driver.User.User.Forename.Contains(parameter.Keyword))
                                    || (d.Driver != null && d.Driver.User.User.Surname.Contains(parameter.Keyword))
                                    || (d.DriverArchived != null && d.DriverArchived.User.User.DisplayUserName.Contains(parameter.Keyword))
                                    || (d.DriverArchived != null && d.DriverArchived.User.User.Forename.Contains(parameter.Keyword))
                                    || (d.DriverArchived != null && d.DriverArchived.User.User.Surname.Contains(parameter.Keyword))
                                    || d.DeliveryQueue.Location.Name.Contains(parameter.Keyword)
                                    );
                    }


                }

                if (!string.IsNullOrEmpty(parameter.DateStart) && !string.IsNullOrEmpty(parameter.DateEnd))
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(userSetting.Timezone);

                    DateTime startDate; DateTime endDate;

                    try
                    {
                        startDate = DateTime.ParseExact(parameter.DateStart, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture).ConvertTimeToUtc(timeZone);
                    }
                    catch
                    {
                        startDate = DateTime.ParseExact(parameter.DateStart, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture).ConvertTimeToUtc(timeZone);
                    }

                    try
                    {
                        endDate = DateTime.ParseExact(parameter.DateEnd, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture).ConvertTimeToUtc(timeZone).AddMinutes(1);
                    }
                    catch
                    {
                        endDate = DateTime.ParseExact(parameter.DateEnd, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture).ConvertTimeToUtc(timeZone).AddMinutes(1);
                    }

                    deliveries = deliveries.Where(q => q.CreatedDate >= startDate && q.CreatedDate < endDate);

                }

                if (parameter.Locations != null && parameter.Locations.Count > 0 && parameter.Locations[0] > 0)
                {
                    deliveries = deliveries.Where(e => parameter.Locations.Contains(e.DeliveryQueue.Location.Id) || parameter.Locations.Contains(e.DeliveryQueueArchive.Location.Id));
                }

                if (parameter.Drivers != null && !parameter.Drivers.Any(d => d == 0))
                {
                    if (!parameter.Drivers.Any(e => e == -1))
                        deliveries = deliveries.Where(d => parameter.Drivers.Contains(d.DriverArchived.Id) || parameter.Drivers.Contains(d.Driver.Id));
                    else
                        deliveries = deliveries.Where(d =>
                        parameter.Drivers.Contains(d.DriverArchived.Id)
                        || parameter.Drivers.Contains(d.Driver.Id)
                        || (d.DriverArchived == null && d.Driver == null)
                        );
                }

                if (parameter.Status != null && parameter.Status.Count > 0 && parameter.Status[0] > 0)
                {
                    deliveries = deliveries.Where(d => parameter.Status.Contains(d.Status));
                }

                if (!parameter.ShowCompleted)
                {
                    deliveries = deliveries.Where(d => d.Status != DeliveryStatus.Completed && d.Status != DeliveryStatus.CompletedWithProblems);
                }

                var totalDeliveries = deliveries.Count();

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Reference":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Location":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "DeliveryQueue.Location.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Driver":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "DriverArchived.User.User.DisplayUserName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ", Driver.User.User.DisplayUserName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                deliveries = deliveries.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion

                #region Paging
                var list = deliveries.Skip(requestModel.Start).Take(requestModel.Length).ToList();


                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.Key,
                    DiscussionKey = q.Discussion?.Key ?? "",
                    Reference = q.Reference?.FullRef,
                    Date = q.CreatedDate.ConvertTimeFromUtc(userSetting.Timezone).ToString(userSetting.DateTimeFormat),
                    Location = q.DeliveryQueue != null ? q.DeliveryQueue.Location.Name : q.DeliveryQueueArchive?.Location.Name,
                    Driver = q.Driver != null ? q.Driver.User.User.GetFullName() : q.DriverArchived?.User.User.GetFullName(),
                    q.Status,
                    StatusText = q.Status.GetDescription()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalDeliveries, totalDeliveries);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, parameter, domainId);
                return null;
            }
        }

        public List<BaseModel> GetDeliveryLocations(int domainId)
        {
            return dbContext.DeliveryQueues.Where(l => l.Location.Domain.Id == domainId).Select(e => new BaseModel { Id = e.Location.Id, Name = e.Location.Name }).OrderBy(n => n.Name).ToList();
        }
        public List<BaseModel> GetDriversByDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDriversByDomain", null, null, domainId);


                var drivers = dbContext.Deliveries.
                    Where(e => e.DeliveryQueue.Location.Domain.Id == domainId || e.DeliveryQueueArchive.Location.Domain.Id == domainId);

                var drivers1 = drivers.Where(e => e.DriverArchived != null)
                    .Select(d => d.DriverArchived).Select(d => new BaseModel
                    {
                        Id = d.Id,
                        Name = string.IsNullOrEmpty(d.User.User.DisplayUserName) ? d.User.User.UserName : d.User.User.DisplayUserName
                    });

                var drivers2 = drivers.Where(e => e.Driver != null)
                   .Select(d => d.Driver).Select(d => new BaseModel
                   {
                       Id = d.Id,
                       Name = string.IsNullOrEmpty(d.User.User.DisplayUserName) ? d.User.User.UserName : d.User.User.DisplayUserName
                   });

                var ds = drivers1.Union(drivers2).Distinct().OrderBy(e => e.Name).ToList();

                ds.Insert(0, new BaseModel { Id = -1, Key = "Not assigned", Name = "Not assigned" });
                return ds;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId);
                return new List<BaseModel>();
            }
        }

        public Delivery GetDelivery(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDelivery", null, null, id);

                return dbContext.Deliveries.FirstOrDefault(d => d.Id == id);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                return null;
            }
        }


        /// <summary>
        ///     This allow to update the status for an order: Jumping forward/Jumping back bypass the intermediate steps
        ///     When an order status is Jumping forward that skips intermediate steps, the datetimes for those steps are to be set
        ///     to the date-times for the new status.
        ///     When an order status is Jumping backward that skips intermediate steps, the datetimes for those skipped steps are
        ///     to be set to null.
        /// </summary>
        /// <param name="lstOrderIds">List PdsOrderUpdates Id</param>
        /// <param name="currentUserId"></param>
        /// <param name="upcommingStatus"></param>
        /// <param name="problemDescription"></param>
        /// <returns></returns>
        public ReturnJsonModel UpdateDeliveryStatus(List<int> lstDeliveryIds, string currentUserId, DeliveryStatus upcommingStatus, string problemDescription)
        {
            var resultJsonModel = new ReturnJsonModel { actionVal = 2, result = false };
            try
            {
                //if (ConfigManager.LoggingDebugSet)
                //    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, lstDeliveryIds, currentUserId, upcommingStatus, problemDescription);

                //var queueOrders = new List<QueueOrder>();

                //var loggingRules = new TradeOrderLoggingRules(dbContext);

                //foreach (var orderId in lstDeliveryIds)
                //{
                //    var queueOrder = dbContext.QueueOrders.FirstOrDefault(e => e.Id == orderId);
                //    if (queueOrder == null)
                //        continue;

                //    if (queueOrder.Status == upcommingStatus)
                //        continue;

                //    new OrderProcessingHelper(dbContext).QueueOrderApplyDate(queueOrder, upcommingStatus,
                //        queueOrder.QueuedDate, queueOrder.PrepStartedDate, queueOrder.PreparedDate, queueOrder.CompletedDate);

                //    queueOrder.Status = upcommingStatus;
                //    //When the Order Status is set to Completing
                //    if (upcommingStatus == PrepQueueStatus.Completing)
                //        //If the Order is associated with a Delivery
                //        if (queueOrder.Delivery !=
                //            null) //orders on Delivery are Status = Completing and there is a Driver associated with Delivery
                //            if (queueOrder.Delivery.Orders.Count > 0 &&
                //                queueOrder.Delivery.Orders.TrueForAll(s => s.Status == PrepQueueStatus.Completing) && queueOrder.Delivery.Driver != null)
                //            {
                //                queueOrder.Delivery.Status = DeliveryStatus.Started;
                //                queueOrder.Delivery.TimeStarted = null;
                //                loggingRules.TradeOrderLogging(TradeOrderLoggingType.DeliveryStatus, queueOrder.Delivery.Id, currentUserId);
                //            }

                //    if (upcommingStatus == PrepQueueStatus.Completed || upcommingStatus == PrepQueueStatus.CompletedWithProblems)
                //    {
                //        //(1) get the PrepQueue from the QueueOrder)
                //        var prepQueue = queueOrder.PrepQueue;
                //        var prepQueueId = prepQueue?.Id ?? 0;

                //        //(2) Get the PrepQueueArchive with the PrepQueue as ParentPrepQueue (var archivePrepQueue)
                //        var archivePrepQueue = dbContext.PrepQueueArchives.FirstOrDefault(a => a.ParentPrepQueue.Id == prepQueueId);

                //        //(3) Then, to move the QueueOrder to archive
                //        queueOrder.PrepQueue = null;
                //        queueOrder.PrepQueueArchive = archivePrepQueue;
                //        queueOrder.ArchivedDate = DateTime.UtcNow;
                //        queueOrder.OrderProblemNote = problemDescription;
                //    }


                //    if (dbContext.Entry(queueOrder).State == EntityState.Detached)
                //        dbContext.QueueOrders.Attach(queueOrder);
                //    dbContext.Entry(queueOrder).State = EntityState.Modified;
                //    dbContext.SaveChanges();
                //    queueOrders.Add(queueOrder);
                //}

                //if (queueOrders.Count > 0)
                //    loggingRules.TradeOrderLogging(TradeOrderLoggingType.Preparation, 0, currentUserId, queueOrders.Select(e => e.Id).ToList());

                resultJsonModel.result = true;
                return resultJsonModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, lstDeliveryIds, currentUserId, upcommingStatus, problemDescription);
                resultJsonModel.result = false;
                resultJsonModel.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJsonModel;
            }
        }

        public ReturnJsonModel SaveDeliveryDiscussion(Delivery delivery, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1, msgId = "0" };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveDiscussion", userId, null, delivery);

                var deliveryDb = dbContext.Deliveries.Find(delivery.Id);
                if (delivery.Discussion == null || deliveryDb == null)
                {
                    result.actionVal = 3;
                    result.msg = "Discussion or delivery is empty";
                    return result;
                }
                if (delivery.Discussion.ActivityMembers.Any())
                {
                    for (int i = 0; i < delivery.Discussion.ActivityMembers.Count; i++)
                    {
                        delivery.Discussion.ActivityMembers[i] = dbContext.QbicleUser.Find(delivery.Discussion.ActivityMembers[i].Id);
                    }
                }

                if (delivery.Discussion.Topic != null && delivery.Discussion.Topic.Id > 0)
                {
                    delivery.Discussion.Topic = dbContext.Topics.Find(delivery.Discussion.Topic.Id);
                }
                delivery.Discussion.Qbicle = delivery.Discussion.Topic.Qbicle;
                delivery.Discussion.DiscussionType = QbicleDiscussion.DiscussionTypeEnum.QbicleDiscussion;
                delivery.Discussion.TimeLineDate = DateTime.UtcNow;
                delivery.Discussion.App = QbicleActivity.ActivityApp.Trader;
                delivery.Discussion.ActivityType = QbicleActivity.ActivityTypeEnum.DiscussionActivity;
                delivery.Discussion.StartedDate = DateTime.UtcNow;
                delivery.Discussion.StartedBy = dbContext.QbicleUser.Find(userId);
                if (delivery.Discussion.Id == 0)
                {
                    deliveryDb.Discussion = delivery.Discussion;
                    dbContext.SaveChanges();
                    result.msgId = delivery.Id.ToString();
                }
                else
                {
                    deliveryDb.Discussion.Name = delivery.Discussion.Name + $" <Delivery #{delivery.Reference?.FullRef ?? delivery.Id.ToString()}>";
                    deliveryDb.Discussion.ExpiryDate = delivery.Discussion.ExpiryDate;
                    deliveryDb.Discussion.DiscussionType = delivery.Discussion.DiscussionType;
                    deliveryDb.Discussion.FeaturedImageUri = delivery.Discussion.FeaturedImageUri;
                    deliveryDb.Discussion.Summary = delivery.Discussion.Summary;
                    deliveryDb.Discussion.ActivityMembers.Clear();
                    dbContext.SaveChanges();
                    deliveryDb.Discussion.ActivityMembers = delivery.Discussion.ActivityMembers;
                    dbContext.SaveChanges();
                    result.actionVal = 2;
                }
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, delivery);
                result.actionVal = 3;
                result.msg = ex.Message;
                return result;
            }
        }


        //Delivery Management


        public QueueOrder GetOrder(int id)
        {
            return dbContext.QueueOrders.FirstOrDefault(e => e.Id == id);
        }


        /// <summary>
        /// Get orders
        /// </summary>
        /// <param name="request"></param>
        /// <param name="domainId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public PaginationResponse GetOrdersForDeliveryWeb(PaginationRequest request, string deliveryKey, int domainId, string userId)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, request);

                var user = new UserRules(dbContext).GetById(userId);
                if (user == null) return null;
                var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                var currentDeliveryId = deliveryKey.Decrypt2Int();


                var query = dbContext.QueueOrders.Where(e =>
                    e.Delivery == null &&
                    e.PrepQueue.Location.Domain.Id == domainId &&
                    e.Classification == OrderTypeClassification.HomeDelivery
                    && (e.Status != PrepQueueStatus.Completed && e.Status != PrepQueueStatus.CompletedWithProblems));

                // get order not in delivery and in other delivery NEW
                //query = query.Where(e => e.Delivery == null || (e.Delivery != null && e.Delivery.Id != currentDeliveryId && e.Delivery.Status == DeliveryStatus.New));



                if (!string.IsNullOrEmpty(request.keyword))
                    query = query.Where(n => n.OrderRef.Contains(request.keyword));

                response.totalNumber = query.Count();
                var ddsQueueOrders = query.OrderBy(s => s.QueuedDate).Skip((request.pageNumber - 1) * request.pageSize).Take(request.pageSize).ToList();

                var model = new List<OrderQueueCustom>();

                ddsQueueOrders.ForEach(queueOrder =>
                {
                    var qOrder = new OrderQueueCustom
                    {
                        Id = queueOrder.Id,
                        Key = queueOrder.Key,
                        OrderRef = queueOrder.OrderRef,
                        OrderTotal = queueOrder.OrderTotal.ToCurrencySymbol(currencySetting),
                        OrderItems = queueOrder.OrderItems.Count,
                        Status = queueOrder.Status.GetDescription(),
                        StatusLabel = queueOrder.Status.GetClass(),
                        Customer = $"{queueOrder.Customer?.CustomerName}, {queueOrder.Customer?.FullAddress.ToAddress()}",
                        CustomerLatitude = queueOrder.Customer?.FullAddress.Latitude ?? 0,
                        CustomerLongitude = queueOrder.Customer?.FullAddress.Longitude ?? 0,
                        DeliveryStatus = queueOrder.Delivery?.Status.GetDescription()
                    };

                    qOrder.ItemsInfo = GetOrderItemsInfo(queueOrder, user, currencySetting);

                    model.Add(qOrder);

                });

                response.items = model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                response.items = new List<Order>();
            }
            return response;
        }

        private string GetOrderItemsInfo(QueueOrder order, ApplicationUser user, CurrencySetting currencySetting)
        {
            var html = $"";
            html += $"<h1 style='line-height: 1.2;'>Order {order.OrderRef}</h1>";
            html += $"<span class='label label-lg ostatus label-{order.Status.GetClass()}'>{order.Status.GetDescription()}</span>";
            html += $"<br /><br /><br /><div class='row'><div class='col-xs-6'>";
            html += $"<label>Queued</label><br />";
            html += $"<p>{order.QueuedDate.Value.ToString(user.DateFormat + " " + user.TimeFormat)}</p>";
            html += $"</div>";
            html += $"<div class='col-xs-6'>";
            html += $"<label>Started</label>";

            // Pending
            var time = "";
            if (order.PrepStartedDate != null && order.QueuedDate != null)
            {
                time = order.PrepStartedDate.Value.ToString(user.DateFormat + " " + user.TimeFormat);
            }
            else if (order.PrepStartedDate == null && order.QueuedDate != null)
            {
                time = order.QueuedDate.Value.ToString(user.DateFormat + " " + user.TimeFormat);
            }

            html += $"<p>{time}</p>";
            html += $"</div></div><br />";
            html += $"<div class='row'><div class='col-xs-12'>";
            html += $"<label>Progress</label><br />";
            // Completion
            if (order.PreparedDate != null)
            {
                if (order.CompletedDate != null)
                {
                    time = "Completed #" + order.CompletedDate.Value.ToString(user.DateFormat + " " + user.TimeFormat);
                }
                else
                {
                    time = "Prepared #" + order.PreparedDate.Value.ToString(user.DateFormat + " " + user.TimeFormat);
                }
            }
            html += $"<p>{time}</p>";
            html += $"</div></div><br />";
            html += $"<div class='row'>";
            html += $"<div class='col-xs-6'>";
            html += $"<label>Distance</label><br />";
            html += $"<p id='order-distance-{order.Id}'></p>";
            html += $"</div>";
            html += $"<div class='col-xs-6'>";
            html += $"<label>Duration</label><br />";
            html += $"<p id='order-duration-{order.Id}'></p>";
            html += $"</div>";
            html += $"</div><br />";

            html += $"<label>Order info</label>";

            html += $"<div style='padding: 0 1px;'>";
            html += $"<table class='table table-condensed table-borderless' style='margin: 0 0 15px 0; color: #828da0; font-size: 12px; font-family: 'Roboto';'>";
            html += $"<tbody>";
            order.OrderItems.ForEach(item =>
            {
                html += $"<tr>";
                html += $"<td>{item.Variant.Name}</td>";
                html += $"<td>{item.Quantity}</td>";
                html += $"<td>{item.Variant.Unit.Name}</td>";
                html += $"<td>{(item.Variant.Price?.NetPrice ?? 0).ToCurrencySymbol(currencySetting)}</td>";
                html += $"</tr>";
                item.Extras.ForEach(ex =>
                {
                    html += $"<tr>";
                    html += $"<td>{ex.Extra.Name}</td>";
                    html += $"<td>{item.Quantity}</td>";
                    html += $"<td>{ex.Extra.Unit.Name}</td>";
                    html += $"<td>{(ex.Extra.Price?.NetPrice ?? 0).ToCurrencySymbol(currencySetting)}</td>";
                    html += $"</tr>";
                });





            });


            html += $"</tbody>";
            html += $"</table>";
            html += $"</div>";



            return html;
        }

        /// <summary>
        /// driver list server side - 
        /// if existed retun only- 
        /// else return page size, 
        /// input filter -> return page size, 
        /// change -> reload, assign -> reload
        /// </summary>
        /// <param name="request"></param>
        /// <param name="deliveryKey"></param>
        /// <param name="domainId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public PaginationResponse GetDriverDeliveryWeb(PaginationRequest request, string deliveryKey)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, request);


                var deliveryId = int.Parse(deliveryKey.Decrypt());
                var delivery = dbContext.Deliveries.FirstOrDefault(d => d.Id == deliveryId);

                var locationId = delivery.DeliveryQueue?.Location.Id ?? 0;
                if (delivery.DeliveryQueueArchive != null)
                    locationId = delivery.DeliveryQueueArchive.Location.Id;

                var driver = delivery.Driver ?? delivery.DriverArchived;

                if (driver != null)
                {
                    response.totalNumber = 1;
                    response.items = new List<DsDriverResult> { new DsDriverResult
                    {
                        Id = driver.Id,
                        Key = driver.Key,
                        Name = driver.User.User.GetFullName(),
                        Status = (int)driver.Status,
                        DriverStatus = driver.Status,
                        DriverStatusName=driver.Status.GetDescription(),
                        DriverStatusLable=driver.Status.GetClass(),
                        DriverDeliveryStatus = driver.DeliveryStatus,
                        DriverDeliveryStatusName= driver.DeliveryStatus.GetDescription(),
                        DriverDeliveryStatusLable=driver.DeliveryStatus.GetClass(),
                        DeliveryStatus = (int)driver.DeliveryStatus,
                        Time = DateTime.UtcNow,
                        AvatarUrl = driver.User.User.ProfilePic.ToUri(),
                        IsBlocked = !driver.User.User.IsEnabled ?? false,
                        Email = driver.User.User.Email,
                        Telephone = driver.User.User.Tell,
                        LastUpdateDate = driver.LastUpdatedDate,
                        Latitude = driver.Latitude,
                        Longitude = driver.Longitude,
                        DeliveryId=deliveryId,
                        CanChangeDriver= delivery.Status == DeliveryStatus.New
                    }};
                    return response;
                }

                var query = dbContext.Drivers.Where(e => e.EmploymentLocation.Id == locationId && e.AtWork && e.Status == DriverStatus.IsAvailable);

                if (!string.IsNullOrEmpty(request.keyword))
                    query = query.Where(d => d.User.User.DisplayUserName.Contains(request.keyword)
                    || d.User.User.Forename.Contains(request.keyword)
                    || d.User.User.Surname.Contains(request.keyword));

                response.totalNumber = query.Count();
                var drs = query.OrderBy(s => s.User.User.DisplayUserName).Skip((request.pageNumber - 1) * request.pageSize).Take(request.pageSize).ToList();

                var model = new List<DsDriverResult>();

                drs.ForEach(d =>
                {

                    var dr = new DsDriverResult
                    {
                        Id = d.Id,
                        Key = d.Key,
                        Name = d.User.User.GetFullName(),
                        Status = (int)d.Status,
                        DriverStatus = d.Status,
                        DriverStatusName = d.Status.GetDescription(),
                        DriverStatusLable = d.Status.GetClass(),
                        DriverDeliveryStatus = d.DeliveryStatus,
                        DriverDeliveryStatusName = d.DeliveryStatus.GetDescription(),
                        DriverDeliveryStatusLable = d.DeliveryStatus.GetClass(),
                        DeliveryStatus = (int)d.DeliveryStatus,
                        Time = DateTime.UtcNow,
                        AvatarUrl = d.User.User.ProfilePic.ToUri(),
                        IsBlocked = !d.User.User.IsEnabled ?? false,
                        Email = d.User.User.Email,
                        Telephone = d.User.User.Tell,
                        LastUpdateDate = d.LastUpdatedDate,
                        Latitude = d.Latitude,
                        Longitude = d.Longitude,
                        DeliveryId = 0,
                        CanChangeDriver = true
                    };

                    model.Add(dr);

                });
                response.items = model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                response.items = new List<DsDriverResult>();
            }
            return response;
        }
    }
}
