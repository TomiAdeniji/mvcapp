using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Qbicles.BusinessRules.Dds
{
    public class DeliveryDriverApiRules
    {
        ApplicationDbContext dbContext;

        public DeliveryDriverApiRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public IPosResult DriverLogout(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DriverLogout id", request.UserId, null, request);

                var drivers = dbContext.Drivers.Where(e => e.User.User.Id == request.UserId).ToList();

                if (drivers == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "driver")
                };

                drivers.ForEach(d =>
                {
                    d.AtWork = false;
                    d.Status = DriverStatus.IsBusy;
                    d.DeliveryStatus = DriverDeliveryStatus.NotSet;
                    d.DeviceSerial = "";
                    d.DeviceName = "";
                });
                dbContext.SaveChanges();


                var posResult = new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Message = ResourcesManager._L("SUCCESSFULLY"),
                    Status = request.Status
                };
                new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);


                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);
                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Status = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }

        }

        public IPosResult DriverUpdateStatus(PosRequest request, DriverStatus status)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Driver Status", request.UserId, null, request);


                if (!status.TryParseEnum<DriverStatus>()) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Status")
                };

                var drivers = dbContext.Drivers.Where(e => e.User.User.Id == request.UserId && e.AtWork).ToList();

                if (drivers == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "driver")
                };


                drivers.ForEach(driver =>
                {
                    driver.Status = status;
                });

                dbContext.SaveChanges();


                var posResult = new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Message = ResourcesManager._L("SUCCESSFULLY"),
                    Status = request.Status
                };

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);

                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);
                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Status = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Call this api to get driver's profile after login tho the app
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DsDriverResult DriverProfile(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DriverLogout id", request.UserId, null, request);

                DsDriverResult ddsDriver;
                // get any Driver associated with the user
                //QBIC-3746: so that when a Driver logs in, the AtWork setting is set to true 
                //and the driver’s Status is set to IsAvailable i.e.Online in the Driver UI is set to on
                var driver = dbContext.Drivers.FirstOrDefault(e => e.User.User.Id == request.UserId);
                driver.AtWork = true;
                driver.Status = DriverStatus.IsAvailable;
                dbContext.SaveChanges();
                var posSetting = dbContext.PosSettings.AsNoTracking().AsNoTracking().FirstOrDefault(e => e.Location.Id == driver.EmploymentLocation.Id);
                var deliverySystemSetting = dbContext.DeliverySystemSettings.AsNoTracking().FirstOrDefault();
                // find any current delivery associated with the user
                var delivery = dbContext.Deliveries.FirstOrDefault(e => e.Driver.User.User.Id == request.UserId && e.DeliveryQueueArchive == null);


                if (delivery == null)//If there is no Delivery associated with the user then
                    ddsDriver = new DsDriverResult
                    {
                        Id = driver.Id,
                        Key = driver.Key,
                        Name = HelperClass.GetFullNameOfUser(driver.User.User),
                        Status = DriverStatus.IsAvailable.GetId(),
                        DeliveryStatus = DriverDeliveryStatus.NotSet.GetId(),
                        DriverStatus = driver.Status,
                        DriverDeliveryStatus = driver.DeliveryStatus,
                        Time = DateTime.UtcNow,
                        AvatarUrl = driver.User.User.ProfilePic.ToUri(),
                        IsBlocked = driver.User.User.IsEnabled ?? false,
                        Email = driver.User.User.Email,
                        Telephone = driver.User.User.Tell,
                        LastUpdateDate = driver.LastUpdatedDate,
                        Latitude = driver.Latitude,
                        Longitude = driver.Longitude,
                        APICallThresholdTimeInterval = posSetting?.APICallThresholdTimeInterval ?? 60,
                        DriverUpdateLocationTimeInterval = deliverySystemSetting?.DriverUpdateLocationTimeInterval ?? 5
                    };
                else
                    ddsDriver = new DsDriverResult
                    {
                        Id = driver.Id,
                        Key = driver.Key,
                        Name = HelperClass.GetFullNameOfUser(driver.User.User),
                        Status = DriverStatus.IsAvailable.GetId(),
                        DeliveryStatus = (int)delivery.Driver.DeliveryStatus,
                        DriverStatus = driver.Status,
                        DriverDeliveryStatus = driver.DeliveryStatus,
                        Time = DateTime.UtcNow,
                        AvatarUrl = delivery.Driver.User.User.ProfilePic.ToUri(),
                        IsBlocked = delivery.Driver.User.User.IsEnabled ?? false,
                        Email = delivery.Driver.User.User.Email,
                        Telephone = delivery.Driver.User.User.Tell,
                        LastUpdateDate = delivery.Driver.LastUpdatedDate,
                        Latitude = delivery.Driver.Latitude,
                        Longitude = delivery.Driver.Longitude,
                        DeliveryId = delivery.Id,
                        DeliveryReference = delivery.Reference?.FullRef ?? delivery.Id.ToString(),
                        APICallThresholdTimeInterval = posSetting?.APICallThresholdTimeInterval ?? 60,
                        DriverUpdateLocationTimeInterval = deliverySystemSetting?.DriverUpdateLocationTimeInterval ?? 5
                    };

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, ddsDriver.ToJson(), MethodBase.GetCurrentMethod().Name);


                return ddsDriver;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);
                return null;
            }
        }

        public DsDeliveries DeliveryInfo(PosRequest request, int deliveryId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Accept Delivery", request.UserId, null, request);

                var d = dbContext.Deliveries.Find(deliveryId);
                if (d == null) return null;

                var user = dbContext.QbicleUser.AsNoTracking().FirstOrDefault(e => e.Id == request.UserId);

                var delivery = new DsDeliveries
                {
                    TraderId = d.Id,
                    Status = (int)d.Status,
                    TimeStarted = d.TimeStarted?.ConvertTimeFromUtc(user.Timezone),
                    TimeFinished = d.TimeFinished?.ConvertTimeFromUtc(user.Timezone),
                    EstimateTime = d.EstimateTime,
                    EstimateDistance = d.EstimateDistance,
                    Routes = d.Routes,
                    DriverId = d.Driver?.Id ?? 0,
                    DriverName = HelperClass.GetFullNameOfUser(d.Driver?.User.User),
                    DriverLatitude = d.Driver?.Latitude,
                    DriverLongitude = d.Driver?.Longitude,
                    Total = d.Total,
                    Orders = new List<OrderDelivery>(),
                    Token = d.Timestamp.ToString().Encrypt(),
                    DriverDeliveryStatus = (int)(d.Driver?.DeliveryStatus ?? 0),
                    DriverStatus = d.Driver?.DeliveryStatus.GetDescription(),
                    Reference = d.Reference?.FullRef ?? d.Id.ToString()
                };


                if (d.DeliveryQueue != null)
                    delivery.Location = new DssLocationModel
                    {
                        DeliveryId = d.Id,
                        Name = d.DeliveryQueue.Location.Name,
                        Phone = d.DeliveryQueue.Location.Address?.Phone,
                        Address = d.DeliveryQueue.Location.TraderLocationToAddress(),
                        Latitude = d.DeliveryQueue.Location.Address?.Latitude,
                        Longitude = d.DeliveryQueue.Location.Address?.Longitude
                    };
                var orderReferences = new OrderProcessingHelper(dbContext).GetTradeOrderRef(d.Orders);
                d.Orders.OrderBy(s => s.DeliverySequence).ForEach(queueOrder =>
                {
                    // SendToPrep queue order not associate with the prepqueu until call send to prep
                    if (queueOrder.PrepQueue == null) return;

                    delivery.Orders.Add(new OrderDelivery
                    {
                        Id = queueOrder.Id,
                        Reference = queueOrder.OrderRef,
                        TradeOrderRef = orderReferences.FirstOrDefault(e => e.LinkedTraderId == queueOrder.LinkedOrderId)?.Reference ?? "",
                        Customer = queueOrder.Customer?.CustomerName,
                        Address = queueOrder.Customer?.FullAddress.ToAddress(),
                        PhoneNumber = queueOrder.Customer.PhoneNumber,
                        Latitude = queueOrder.Customer?.FullAddress?.Latitude,
                        Longitude = queueOrder.Customer?.FullAddress?.Longitude,
                        Status = (int)queueOrder.Status,
                        IsActive = (d.ActiveOrder?.Id ?? 0) == queueOrder.Id,
                        DeliverySequence = queueOrder.DeliverySequence ?? 0,
                        CompletedDate = queueOrder.CompletedDate?.ConvertTimeFromUtc(user.Timezone).FormatDateTimeByUser(user.DateFormat, user.TimeFormat) ?? ""
                    });
                });


                new PosRequestRules(dbContext).HangfirePosRequestLog(request, delivery.ToJson(), MethodBase.GetCurrentMethod().Name);

                return delivery;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);
                return null;
            }
        }

        /// <summary>
        /// Driver update location
        /// </summary>
        /// <param name="request"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public IPosResult DriverUpdateLocation(PosRequest request, DssLocationModel location, bool fromWeb = false, Delivery delivery = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Accept Delivery", request.UserId, null, request);

                if (location.Latitude == null || location.Longitude == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Latitude or Longitude")
                };

                if (!fromWeb)
                    delivery = dbContext.Deliveries.FirstOrDefault(e => e.Driver.User.User.Id == request.UserId
                                   && e.Status != DeliveryStatus.Completed && e.DeliveryQueueArchive == null);

                var drivers = dbContext.Drivers.Where(e => e.AtWork);

                if (!fromWeb)
                    drivers = drivers.Where(e => e.User.User.Id == request.UserId);
                else
                    drivers = drivers.Where(e => e.User.User.Id == delivery.Driver.User.User.Id);


                drivers.ToList().ForEach(driver =>
                {
                    driver.Latitude = (decimal)location.Latitude;
                    driver.Longitude = (decimal)location.Longitude;
                    driver.LastUpdatedDate = DateTime.UtcNow;

                    var driverLog = new DriverLog
                    {
                        CreatedDate = DateTime.UtcNow,
                        Longitude = driver.Longitude,
                        Latitude = driver.Latitude,
                        Driver = driver,
                        DeviceName = driver.DeviceName,
                        DeviceSerial = driver.DeviceSerial,
                        Delivery = delivery?.Driver == driver ? delivery : null
                    };

                    dbContext.DriverLogs.Add(driverLog);
                });

                dbContext.SaveChanges();

                if (delivery?.ActiveOrder != null && location.ETA >= 0)
                {
                    var userId = fromWeb == true ? delivery.Driver.User.User.Id : request.UserId;
                    new PosRequestRules(dbContext).HangfirePosProcessActiveOrder(delivery.ActiveOrder.LinkedOrderId, userId, location);
                }

                var posResult = new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Status = request.Status,
                    TraderId = delivery?.Id.ToString()
                };

                if (!fromWeb)
                    new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);

                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);
                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Status = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }


        /// <summary>
        /// Driver update the delivery status
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ddsDelivery">delivery send to from Driver App</param>
        /// <returns></returns>
        public IPosResult DriverUpdateStatusDelivery(PosRequest request, DsDeliveryParameter ddsDelivery, bool fromWeb = false)
        {
            try
            {
                var userId = request.UserId;

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update Delivery", userId, null, request);


                if (!ddsDelivery.Status.TryParseEnum<DriverDeliveryStatus>()) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Status")
                };

                var delivery = dbContext.Deliveries.Find(ddsDelivery.Id);

                if (delivery == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotFound,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "delivery")
                };


                // Get the status of the delivery in the database so that anx change instatus can be logged
                var originalDeliveryStatus = delivery.Status;

                if (delivery.Driver == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotFound,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "driver")
                };

                if (!fromWeb && (delivery.Driver.DeviceSerial != request.SerialNumber || delivery.Driver.DeviceName != request.DeviceName))
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Status = HttpStatusCode.NotAcceptable,
                        Message = "An other device has logged . Please try log in again!"
                    };

                var loggingRules = new TradeOrderLoggingRules(dbContext);

                var orderStatusUpdated = new List<int>();

                var orderHelper = new OrderProcessingHelper(dbContext);
                var driverUserId = delivery.Driver.User.User.Id;

                var driverStatus = DriverStatus.IsAvailable;

                var driverLocation = "";
                //(3) The action here is that the Driver accepts the Delivery.
                if (ddsDelivery.Status == DriverDeliveryStatus.Accepted)
                {
                    delivery.Status = DeliveryStatus.Accepted;
                    delivery.Driver.DeliveryStatus = DriverDeliveryStatus.Accepted;

                    driverStatus = DriverStatus.IsBusy;

                    delivery.Orders.ForEach(o =>
                    {
                        if (o.Status != PrepQueueStatus.Completing)
                        {
                            o.Status = PrepQueueStatus.Completing;
                            orderStatusUpdated.Add(o.Id);
                            orderHelper.QueueOrderApplyDate(o, PrepQueueStatus.Completing, null, null, DateTime.UtcNow, null);
                        }
                    });

                    dbContext.SaveChanges();
                }
                //(4) The action here is that the Driver rejects the Delivery.
                else if (ddsDelivery.Status == DriverDeliveryStatus.Rejected)
                {
                    delivery.Status = DeliveryStatus.New;
                    delivery.Driver.DeliveryStatus = DriverDeliveryStatus.Rejected;

                    driverStatus = DriverStatus.IsAvailable;

                    delivery.Driver = null;
                    delivery.DriverArchived = null;
                    delivery.Orders.ForEach(o =>
                    {
                        if (o.Status != PrepQueueStatus.Completing)
                        {
                            o.Status = PrepQueueStatus.Completing;
                            orderStatusUpdated.Add(o.Id);
                            orderHelper.QueueOrderApplyDate(o, PrepQueueStatus.Completing, null, null, DateTime.UtcNow, null);
                        }
                    });

                    dbContext.SaveChanges();

                }
                //(5) The action here is that the Driver indicates they are heading to the Depot
                else if (ddsDelivery.Status == DriverDeliveryStatus.HeadingToDepot)
                {
                    if (delivery.Driver.DeliveryStatus != DriverDeliveryStatus.Accepted)
                        return new IPosResult
                        {
                            IsTokenValid = true,
                            Status = HttpStatusCode.NotAcceptable,
                            Message = ResourcesManager._L("ERROR_DRIVER_DID_NOT_ACCEPT_DELIVERY")
                        };
                    delivery.Driver.DeliveryStatus = DriverDeliveryStatus.HeadingToDepot;

                    driverStatus = DriverStatus.IsBusy;

                    //update driver location to the depot location
                    if (fromWeb && ddsDelivery.CompleteOrderId > 0)
                    {
                        var deliveryQueue = delivery.DeliveryQueue == null ? delivery.DeliveryQueueArchive.ParentDeliveryQueue : delivery.DeliveryQueue;
                        var location = new DssLocationModel
                        {
                            DeliveryId = delivery.Id,
                            Latitude = deliveryQueue.Location.Address.Latitude,
                            Longitude = deliveryQueue.Location.Address.Longitude,
                            ETA = 0,
                        };
                        DriverUpdateLocation(new PosRequest { UserId = userId }, location, fromWeb, delivery);
                        driverLocation = $"{location.Latitude}#{location.Longitude}";
                    }
                    dbContext.SaveChanges();
                }
                //(6) The action here is that the Driver indicates they are ready to pickup the delivery at the Depot
                else if (ddsDelivery.Status == DriverDeliveryStatus.ReadyForPickup)
                {
                    if (delivery.Driver.DeliveryStatus != DriverDeliveryStatus.HeadingToDepot)
                        return new IPosResult
                        {
                            IsTokenValid = true,
                            Status = HttpStatusCode.NotAcceptable,
                            Message = ResourcesManager._L("ERROR_DRIVER_DID_NOT_HEADING_TO_DEPOT_DELIVERY")
                        };
                    delivery.Driver.DeliveryStatus = DriverDeliveryStatus.ReadyForPickup;

                    driverStatus = DriverStatus.IsBusy;

                    dbContext.SaveChanges();
                }
                //(7)  The action here is that the Driver indicates they have started the delivery
                else if (ddsDelivery.Status == DriverDeliveryStatus.StartedDelivery)
                {
                    if (delivery.Driver.DeliveryStatus != DriverDeliveryStatus.ReadyForPickup && delivery.Driver.DeliveryStatus != DriverDeliveryStatus.StartedDelivery)
                        return new IPosResult
                        {
                            IsTokenValid = true,
                            Status = HttpStatusCode.NotAcceptable,
                            Message = ResourcesManager._L("ERROR_DRIVER_DID_NOT_READY_TO_PICKUP_DELIVERY")
                        };
                    delivery.Driver.DeliveryStatus = DriverDeliveryStatus.StartedDelivery;

                    driverStatus = DriverStatus.IsBusy;
                    /*
                        checks if the originalDeliveryStatus = DeliveryStatus.Started
                        if it is not then set the Delivery.TimeStarted = UTCNOW
                     */
                    if (originalDeliveryStatus != DeliveryStatus.Started)
                        delivery.TimeStarted = DateTime.UtcNow;

                    delivery.Status = DeliveryStatus.Started;
                    // (8)  The action here is that the Driver indicates that an Order has been delivered
                    // (9)  The action here is that the Driver indicates that an Order has been delivered but there is a problem
                    // update the Active order in Trader
                    if (ddsDelivery.Orders != null && ddsDelivery.Orders.Any())
                    {
                        var activeOrderId = ddsDelivery.Orders.FirstOrDefault(o => o.Status != PrepQueueStatus.Completed && o.Status != PrepQueueStatus.CompletedWithProblems)?.Id ?? ddsDelivery.Orders[0].Id;

                        delivery.Orders.ForEach(queueOrder =>
                        {
                            ddsDelivery.Orders.ForEach(order =>
                            {
                                if (queueOrder.Id == order.Id)
                                {
                                    if (queueOrder.Status != order.Status)
                                        orderStatusUpdated.Add(queueOrder.Id);

                                    queueOrder.DeliverySequence = order.DeliverySequence;
                                    queueOrder.OrderDeliveryProblemNote = order.ProblemNote;

                                    if ((order.Status == PrepQueueStatus.Completed || order.Status == PrepQueueStatus.CompletedWithProblems) && queueOrder.Status != order.Status)
                                        orderHelper.QueueOrderApplyDate(queueOrder, PrepQueueStatus.Completed, queueOrder.QueuedDate, queueOrder.PrepStartedDate, queueOrder.PreparedDate, DateTime.UtcNow);

                                    queueOrder.Status = order.Status;
                                }

                                if (queueOrder.Id == activeOrderId)
                                    delivery.ActiveOrder = queueOrder;

                            });
                        });

                        //update driver location to the order completing
                        if (fromWeb && ddsDelivery.CompleteOrderId > 0)
                        {
                            var routes = delivery.Routes.ParseAs<DeliveryRoutes>();

                            var order = delivery.Orders.FirstOrDefault(e => e.Id == ddsDelivery.CompleteOrderId);
                            if (order != null)
                            {
                                var location = new DssLocationModel
                                {
                                    DeliveryId = delivery.Id,
                                    Latitude = order.Customer.FullAddress.Latitude,
                                    Longitude = order.Customer.FullAddress.Longitude,
                                    ETA = routes.detailed.FirstOrDefault(e => e.to == activeOrderId)?.time ?? 0,
                                };
                                DriverUpdateLocation(new PosRequest { UserId = userId }, location, fromWeb, delivery);
                                driverLocation = $"{location.Latitude}#{location.Longitude}";
                            }

                        }
                    }

                    dbContext.SaveChanges();
                }
                //10.11
                else if (ddsDelivery.Status == DriverDeliveryStatus.Completed || ddsDelivery.Status == DriverDeliveryStatus.CompletedWithProblems)
                {
                    if (delivery.Driver.DeliveryStatus != DriverDeliveryStatus.StartedDelivery)
                        return new IPosResult
                        {
                            IsTokenValid = true,
                            Status = HttpStatusCode.NotAcceptable,
                            Message = ResourcesManager._L("ERROR_DRIVER_DID_NOT_DELIVERIED")
                        };

                    //Check All Orders in the Delivery have been completed
                    if (delivery.Orders.Any(o => o.Status != PrepQueueStatus.Completed && o.Status != PrepQueueStatus.CompletedWithProblems))
                    {
                        return new IPosResult
                        {
                            IsTokenValid = true,
                            Status = HttpStatusCode.NotAcceptable,
                            Message = ResourcesManager._L("ERROR_DRIVER_ALL_ORDER__DID_PROBLEM")
                        };
                    }

                    /*
                        checks if the originalDeliveryStatus = DeliveryStatus.Completed or DeliveryStatus.CompletedWithProblmes
                        if it is not then set the Delivery.TimeFinished= UTCNOW
                     */
                    if (originalDeliveryStatus != DeliveryStatus.Completed && originalDeliveryStatus != DeliveryStatus.CompletedWithProblems)
                        delivery.TimeFinished = DateTime.UtcNow;

                    //10 all Orders in the Delivery have been completed,
                    if (delivery.Orders.All(o => o.Status == PrepQueueStatus.Completed))
                    {
                        delivery.Status = DeliveryStatus.Completed;
                        delivery.Driver.DeliveryStatus = DriverDeliveryStatus.Completed;
                        delivery.DeliveryProblemNote = "";
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        delivery.Status = DeliveryStatus.CompletedWithProblems;
                        delivery.Driver.DeliveryStatus = DriverDeliveryStatus.CompletedWithProblems;
                        dbContext.SaveChanges();
                    }

                    driverStatus = DriverStatus.IsAvailable;

                    //When the Delivery is completed, the Delivery.ActiveOrder will be set to null..
                    delivery.ActiveOrder = null;

                    var deliveryQueueArchive = dbContext.DeliveryQueueArchives.FirstOrDefault(e => e.ParentDeliveryQueue.Id == delivery.DeliveryQueue.Id);
                    if (deliveryQueueArchive == null)
                    {
                        deliveryQueueArchive = new DeliveryQueueArchive
                        {
                            CreatedBy = dbContext.QbicleUser.FirstOrDefault(u => u.Id == userId),
                            CreatedDate = DateTime.UtcNow,
                            Location = delivery.Driver.EmploymentLocation,
                            ParentDeliveryQueue = delivery.DeliveryQueue,
                            PrepQueue = delivery.DeliveryQueue.PrepQueue,
                            Name = $"{delivery.DeliveryQueue.Name} Delivery Queue Archive",
                        };
                        deliveryQueueArchive.Deliveries.Add(delivery);
                    }
                    delivery.DeliveryQueueArchive = deliveryQueueArchive;

                    dbContext.SaveChanges();

                    delivery.DeliveryQueue = null;

                    delivery.Orders.OrderBy(s => s.DeliverySequence).ForEach(queueOrder =>
                    {
                        //set the PrepQueueArchive for the QueueOrder
                        //(1) get the PrepQueue from the QueueOrder)
                        var prepQueue = queueOrder.PrepQueue;
                        //(2) Get the PrepQueueArchive with the PrepQueue as ParentPrepQueue (var archivePrepQueue)
                        var archivePrepQueue = queueOrder.PrepQueueArchive;

                        //(3) Then, to move the QueueOrder to archive

                        queueOrder.PrepQueue = null;
                        queueOrder.PrepQueueArchive = archivePrepQueue;
                        queueOrder.ArchivedDate = DateTime.UtcNow;

                    });

                    dbContext.SaveChanges();
                }


                var driverUsers = dbContext.Drivers.Where(d => d.User.User.Id == driverUserId && d.AtWork).ToList();

                driverUsers.ForEach(driver =>
                {
                    if (driverStatus == DriverStatus.IsAvailable)
                        driver.DeliveryStatus = DriverDeliveryStatus.NotSet;
                    driver.Status = driverStatus;
                    dbContext.SaveChanges();
                });

                if (ddsDelivery.Status == DriverDeliveryStatus.Completed || ddsDelivery.Status == DriverDeliveryStatus.CompletedWithProblems)
                {
                    delivery.DriverArchived = delivery.Driver;
                    delivery.Driver = null;
                    dbContext.SaveChanges();
                }

                //Only log the changes to the order statuses if there are changes to log
                if ((orderStatusUpdated != null) && (orderStatusUpdated.Count > 0))
                    loggingRules.TradeOrderLogging(TradeOrderLoggingType.Preparation, 0, userId, orderStatusUpdated);


                //Only log the change to the delivery status if there is a status to log
                if (delivery.Status != originalDeliveryStatus)
                    loggingRules.TradeOrderLogging(TradeOrderLoggingType.DeliveryStatus, delivery.Id, userId);




                if (!fromWeb)
                {
                    var user = dbContext.QbicleUser.AsNoTracking().FirstOrDefault(e => e.Id == userId);

                    var orderReferences = new OrderProcessingHelper(dbContext).GetTradeOrderRef(delivery.Orders);

                    var orders = delivery.Orders.OrderBy(s => s.DeliverySequence).Select(queueOrder => new OrderDelivery
                    {
                        Id = queueOrder.Id,
                        Reference = queueOrder.OrderRef,
                        TradeOrderRef = orderReferences.FirstOrDefault(e => e.LinkedTraderId == queueOrder.LinkedOrderId)?.Reference ?? "",
                        Customer = queueOrder.Customer?.CustomerName,
                        Address = queueOrder.Customer?.FullAddress.ToAddress(),
                        PhoneNumber = queueOrder.Customer.PhoneNumber,
                        Latitude = queueOrder.Customer?.FullAddress?.Latitude,
                        Longitude = queueOrder.Customer?.FullAddress?.Longitude,
                        Status = (int)queueOrder.Status,
                        IsActive = (delivery.ActiveOrder?.Id ?? 0) == queueOrder.Id,
                        DeliverySequence = queueOrder.DeliverySequence ?? 0,
                        CompletedDate = queueOrder.CompletedDate?.ConvertTimeFromUtc(user.Timezone).FormatDateTimeByUser(user.DateFormat, user.TimeFormat) ?? ""
                    }).ToList();
                    var posResult = new IPosResult
                    {
                        IsTokenValid = true,
                        Message = orders.ToJson(),
                        Status = HttpStatusCode.OK,
                    };

                    new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);

                    posResult.Message = driverLocation;
                    return posResult;
                }


                return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.OK,
                    Message = driverLocation
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);
                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Status = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }
    }
}
