using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
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
    public class DdsApiRules
    {
        ApplicationDbContext dbContext;

        public DdsApiRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        //============= API DDS =======================
        /// <summary>
        /// API The call returns the list of DDS devices for which the user, identified in the token, is an administrator.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DsDeviceForUser GetDeviceForUser(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/device GetDeviceForUser", request.UserId, null, request);

                var user = dbContext.QbicleUser.AsNoTracking().FirstOrDefault(e => e.Id == request.UserId);
                if (user == null)
                    return null;


                var devicesList = dbContext.DdsDevice.Where(e => e.Administrators.Any(u => u.Id == request.UserId)).ToList();

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/device DdsDevice User", request.UserId, devicesList);


                var devices = new List<OdsDeviceResult>();
                devicesList.ForEach(d =>
                {
                    devices.Add(
                        new OdsDeviceResult
                        {
                            Id = d.Id,
                            Name = d.Name,
                            Location = d.Location?.Name ?? "",
                            QueueName = d.Queue.Name,
                            SerialNumber = d.SerialNumber,
                            Domain = d.Location?.Domain.Name
                        }
                    );

                });

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, devices.ToJson(), MethodBase.GetCurrentMethod().Name);


                var posResult = new DsDeviceForUser
                {
                    Devices = devices
                };
                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);
                return null;
            }


        }

        /// <summary>
        /// API The call sets the SerialNumber for the DDSDevice identified by the supplied DeviceID.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IPosResult TabletForDevice(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/device sets the SerialNumber for the DDSDevice", request.UserId, null, request);

                //Does the Device, identified by the DeviceId, exist? 
                var ddsDevice = dbContext.DdsDevice.Find(request.DeviceId);
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), $"api/dds/device select DDSDevice where deviceid = {request.DeviceId}", request.UserId, ddsDevice, request);

                if (ddsDevice == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device")
                };
                //Is the user, identified by UserID, an Administrator of the device?
                if (ddsDevice.Administrators.All(u => u.Id != request.UserId))
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Status = HttpStatusCode.NotAcceptable,
                        Message = ResourcesManager._L("ERROR_USER_NOT_ADMIN_DEVICE")
                    };

                //Is the SerialNumber used in any device OTHER than the Device identified in 1.
                //If it is then that Device's serial number MUST be set to null.
                var ddsDeviceSerialNumbers = dbContext.DdsDevice.Where(e => e.SerialNumber == request.SerialNumber).ToList();
                if (ddsDeviceSerialNumbers.Any())
                {
                    ddsDeviceSerialNumbers.ForEach(device =>
                    {
                        device.SerialNumber = "";
                    });
                    dbContext.SaveChanges();
                }

                ddsDevice.SerialNumber = request.SerialNumber;
                if (dbContext.Entry(ddsDevice).State == EntityState.Detached)
                    dbContext.DdsDevice.Attach(ddsDevice);
                dbContext.Entry(ddsDevice).State = EntityState.Modified;
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


        //============= API DDS Driver =======================
        /// <summary>
        /// API is required to get Driver information to display in the Delivery Display System.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ReturnJsonModel DriverGet(PosRequest request, DdsDriverParameter parameter)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/driver DriverGet", request.UserId, null, request, parameter);

                var ddsDevice = dbContext.DdsDevice.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);


                var ddsDrivers = GetDrivers(ddsDevice.Location.Id, parameter);

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, ddsDrivers.ToJson(), MethodBase.GetCurrentMethod().Name);

                return new ReturnJsonModel
                {
                    result = true,
                    Object = new DsDriver
                    {
                        Drivers = ddsDrivers
                    }
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);

                return new ReturnJsonModel { result = false, msg = ex.Message, actionVal = (int)HttpStatusCode.InternalServerError };
            }
        }


        private List<DsDriverResult> GetDrivers(int locationId, DdsDriverParameter parameter)
        {
            //Drivers in a location
            var locationDrivers = dbContext.Drivers.Where(e => e.EmploymentLocation.Id == locationId && e.AtWork && e.IsDeleted == false);

            //If Driver.MapWindow is not null
            if (parameter.Window != null)
                locationDrivers = locationDrivers.Where(d => d.Longitude >= parameter.Window.MinLon && d.Longitude <= parameter.Window.MaxLon && d.Latitude >= parameter.Window.MinLat && d.Latitude <= parameter.Window.MaxLat);

            if (parameter.IsAvailable && !parameter.IsBusy)//If Driver.IsAvailable = true & Driver.IsBusy = false then Driver.Status = IsAvaiable
                locationDrivers = locationDrivers.Where(d => d.Status == DriverStatus.IsAvailable);

            if (!parameter.IsAvailable && parameter.IsBusy)//If Driver.IsAvailable = false & Driver.IsBusy = true then Driver.Status = IsBusy
                locationDrivers = locationDrivers.Where(d => d.Status == DriverStatus.IsBusy);
            //else parameter.IsAvailable == parameter.IsBusy => find all Status

            var ddsDrivers = new List<DsDriverResult>();

            // all the ApplicationUsers related to the drivers found
            //var userDrivers = dbContext.Drivers.Where(d => d.AtWork);
            /* Sample data
                                        locationDrivers ={ user1-driver1-location1-Busy}
                                        userDrivers={
                                                    user1-driver2-location1-Busy,
                                                    user1-driver2-location1-Available
                                                    }*/
            locationDrivers.ToList().ForEach(driver =>
            {
                //case of find by status
                if (parameter.IsAvailable != parameter.IsBusy)
                {
                    //check status of ALL drivers associated the ApplicationUser have status = Location driver's status then add to list response
                    if (dbContext.Drivers.Where(d => d.AtWork && d.User.User.Id == driver.User.User.Id).AsNoTracking().All(s => s.Status == driver.Status))
                        ddsDrivers.Add(ToDriverResult(driver));
                }
                else // case of not find by status then add all  to list response
                    ddsDrivers.Add(ToDriverResult(driver));
            });

            return ddsDrivers;

        }

        private DsDriverResult ToDriverResult(Driver driver)
        {
            return new DsDriverResult
            {
                Id = driver.Id,
                Key = driver.Key,
                Name = HelperClass.GetFullNameOfUser(driver.User.User),
                Status = (int)driver.Status,
                DriverStatus = driver.Status,
                DriverDeliveryStatus = driver.DeliveryStatus,
                DeliveryStatus = (int)driver.DeliveryStatus,
                Time = DateTime.UtcNow,
                AvatarUrl = driver.User.User.ProfilePic.ToUri(),
                IsBlocked = !driver.User.User.IsEnabled ?? false,
                Email = driver.User.User.Email,
                Telephone = driver.User.User.Tell,
                LastUpdateDate = driver.LastUpdatedDate,
                Latitude = driver.Latitude,
                Longitude = driver.Longitude
            };
        }

        /// <summary>
        /// API is required to be able to add a driver to a delivery.
        /// The delivery and driver are identified by the supplied parameters.
        /// The Driver property of the identified Delivery is to be set to the Driver identified by the Driver Id.
        /// Delivery Id (Must be supplied in response to call to 'api/dds/delivery')
        /// Id(Driver Id is supplied in the response to 'api/dds/driver' GetDriver) 
        /// </summary>
        /// <returns>
        /// If Driver cannot be found then return an error to say that is not possible.
        /// If Delivery cannot be found then return an error to say that is not possible.
        /// </returns>
        public IPosResult DriverAdd(PosRequest request, DeliveryDriverParameter deliveryDriver, bool fromWeb = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/driver DriverAdd", request.UserId, null, request, deliveryDriver);

                //If Driver cannot be found then return an error to say that is not possible.
                var driver = dbContext.Drivers.Find(deliveryDriver.DriverId);
                if (driver == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotFound,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "driver")
                };
                if (driver.DeliveryStatus != DriverDeliveryStatus.NotSet) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.BadRequest,
                    Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", $"Driver delivery status is {driver.DeliveryStatus} - NotSet required"),
                };
                //If Delivery cannot be found then return an error to say that is not possible.
                var delivery = dbContext.Deliveries.Find(deliveryDriver.DeliveryId);
                if (delivery == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotFound,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "delivery")
                };
                //if the Delivery status is not New or Start, return NotAcceptable response

                if (delivery.Status != DeliveryStatus.New && delivery.Status != DeliveryStatus.Started) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.BadRequest,
                    Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", $"POS - Delivery status {delivery.Status} - New required"),
                };

                //if the ApplicationUser associated with the Driver(Driver-- > DeviceUser-- > ApplicationUser)
                //    is a Driver at another Location
                //    with a status of Busy
                //    If they are, throw an error to indicate that the Driver cannot be assigned to the Delivery because they are Busy.
                var userDriver = dbContext.Drivers.FirstOrDefault(d => d.User.User.Id == driver.User.User.Id && d.Status == DriverStatus.IsBusy);
                if (userDriver != null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_DRIVER_BUSY", new string[] { userDriver.User.User.GetFullName(), userDriver.EmploymentLocation.Name }),
                };
                //Add Driver to Delivery
                driver.DeliveryStatus = DriverDeliveryStatus.NotSet;
                delivery.Driver = driver;
                delivery.Status = DeliveryStatus.New;

                //Status = Completing (All Orders in Delivery)
                delivery.Orders.ForEach(o =>
                {
                    if (o.Status != PrepQueueStatus.Completing)
                    {
                        o.Status = PrepQueueStatus.Completing;
                        new OrderProcessingHelper(dbContext).QueueOrderApplyDate(o, o.Status, null, null, DateTime.UtcNow, null);
                    }
                });

                if (dbContext.Entry(delivery).State == EntityState.Detached)
                    dbContext.Deliveries.Attach(delivery);
                dbContext.Entry(delivery).State = EntityState.Modified;
                dbContext.SaveChanges();

                new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.DeliveryStatus, delivery.Id, request.UserId);

                var posResult = new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Message = ResourcesManager._L("SUCCESSFULLY"),
                    Status = request.Status
                };

                if (!fromWeb)
                    new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);


                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, deliveryDriver);

                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Status = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// API is required to be able to delete the Driver from a Delivery.
        /// The Driver identified by the Driver ID is to be removed from the Driver property of the Delivery identified by the Delivery Id.
        /// Delivery Id (Must be supplied in response to call to 'api/dds/delivery')
        /// Driver Id(Driver Id is supplied in the response to 'api/dds/driver' GetDriver)
        /// </summary>
        /// <returns>
        /// If Driver cannot be found then return an error to say that is not possible.
        /// If Delivery cannot be found then return an error to say that is not possible.
        /// </returns>
        public IPosResult DriverRemove(PosRequest request, DeliveryDriverParameter deliveryDriver, bool fromWeb = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/driver DriverRemove", request.UserId, null, request, deliveryDriver);

                //If Driver cannot be found then return an error to say that is not possible.
                var driver = dbContext.Drivers.Find(deliveryDriver.DriverId);
                if (driver == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "driver")
                };
                //If Delivery cannot be found then return an error to say that is not possible.
                var delivery = dbContext.Deliveries.Find(deliveryDriver.DeliveryId);
                if (delivery == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "delivery")
                };
                //if the Driver is not linked to the Delivery, return NotAcceptable response
                if (delivery.Driver?.Id != driver.Id) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = "The Driver is not linked to the Delivery."
                };
                //if the Delivery status is not New or Start, return NotAcceptable response
                if (delivery.Status != DeliveryStatus.New) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Status is not New or Start")
                };
                //Remove Driver from delivery
                driver.Status = DriverStatus.IsAvailable;
                driver.DeliveryStatus = DriverDeliveryStatus.NotSet;
                delivery.Driver = null;
                
                delivery.DriverArchived = null;
                //Change status delivery
                delivery.Status = DeliveryStatus.New;

                if (dbContext.Entry(delivery).State == EntityState.Detached)
                    dbContext.Deliveries.Attach(delivery);
                dbContext.Entry(delivery).State = EntityState.Modified;
                dbContext.SaveChanges();

                new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.DeliveryStatus, delivery.Id, request.UserId);

                var posResult = new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Message = ResourcesManager._L("SUCCESSFULLY"),
                    Status = request.Status
                };

                if (!fromWeb)
                    new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);

                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request, deliveryDriver);

                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Status = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        //============= API DDS Driver =======================
        /// <summary>
        /// API to provide a list of the Orders in the current queue.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="includeInDelivery"></param>
        /// <returns></returns>
        public PdsQueueModel OrderGet(PosRequest request, bool includeInDelivery)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/order OrderGet", request.UserId, null, request);

                var ddsDevice = dbContext.DdsDevice.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);
                if (ddsDevice == null)
                    return null;



                var posResult = GetOrders(ddsDevice.Location.Domain.Id, request.UserId, includeInDelivery);

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);


                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);

                return new PdsQueueModel();
            }
        }

        public PdsQueueModel GetOrders(int domainId, string userId, bool includeInDelivery = false)
        {
            var user = new UserRules(dbContext).GetById(userId);
            if (user == null)
                return null;

            var ddsQueueOrders = dbContext.QueueOrders.Where(e =>
                e.PrepQueue.Location.Domain.Id == domainId &&
                e.Classification == OrderTypeClassification.HomeDelivery
                && (e.Status == PrepQueueStatus.NotStarted || e.Status == PrepQueueStatus.Preparing || e.Status == PrepQueueStatus.Completing)).ToList();

            var model = new List<Order>();

            ddsQueueOrders.ForEach(queueOrder =>
            {
                // If IncludeInDelivery = false returns those QueueOrders that are NOT in any Delivery
                if (!includeInDelivery && queueOrder.Delivery != null)
                {
                    return;
                }
                Models.TraderApi.Customer customer = null;
                if (queueOrder.Customer != null)
                {
                    customer = new Models.TraderApi.Customer
                    {
                        TraderId = queueOrder.Customer.Id,
                        Name = queueOrder.Customer.CustomerName,
                        ContactRef = queueOrder.Customer.CustomerRef,
                        Email = queueOrder.Customer.Email,
                        Phone = queueOrder.Customer.PhoneNumber,
                        Address = new Address
                        {
                            AddressLine1 = queueOrder.Customer?.FullAddress?.AddressLine1,
                            AddressLine2 = queueOrder.Customer?.FullAddress?.AddressLine2,
                            City = queueOrder.Customer?.FullAddress?.City,
                            Country = queueOrder.Customer?.FullAddress?.Country?.CommonName,
                            Latitude = queueOrder.Customer?.FullAddress?.Latitude,
                            Longitude = queueOrder.Customer?.FullAddress?.Longitude,
                            Postcode = queueOrder.Customer?.FullAddress?.PostCode
                        }

                    };
                    var cAvatar = dbContext.TraderContacts.Find(queueOrder.Customer.Id)?.AvatarUri;
                    if (!string.IsNullOrEmpty(cAvatar))
                        customer.Avatar = cAvatar.ToUri();
                }

                Cashier cashier = null;
                if (queueOrder.Cashier != null)
                {
                    var cAvatar = queueOrder.Cashier.Avatars.FirstOrDefault(e => e.isDefault)?.URI ?? queueOrder.Cashier.ProfilePic;
                    cashier = new Cashier
                    {
                        TraderId = queueOrder.Cashier.Id,
                        Avatar = cAvatar.ToUri(),
                        Forename = queueOrder.Cashier.Forename,
                        Surname = queueOrder.Cashier.Surname,
                        Pin = dbContext.DeviceUsers.FirstOrDefault(e => e.User.Id == queueOrder.Cashier.Id)?.Pin
                    };
                }
                var order = new Order
                {
                    TraderId = queueOrder.Id,
                    LinkedTraderId = queueOrder.LinkedOrderId,
                    Status = new Status
                    {
                        OrderStatus = (int)queueOrder.Status,
                        CompletedDateTime = queueOrder.CompletedDate.ConvertTimeFromUtc(user.Timezone),
                        CreatedDateTime = queueOrder.CreatedDate.ConvertTimeFromUtc(user.Timezone),
                        DeliveredDateTime = queueOrder.DeliveredDate.ConvertTimeFromUtc(user.Timezone),
                        PaidDateTime = queueOrder.PaidDate.ConvertTimeFromUtc(user.Timezone),
                        PreparedDateTime = queueOrder.PreparedDate.ConvertTimeFromUtc(user.Timezone),
                        StartedDateTime = queueOrder.PrepStartedDate.ConvertTimeFromUtc(user.Timezone)
                    },
                    Delivery = (int)(queueOrder.Delivery?.Status ?? DeliveryStatus.New),
                    Classification = (int)queueOrder.Classification,
                    Type = queueOrder.Type,
                    Reference = queueOrder.OrderRef,
                    Notes = queueOrder.Notes,
                    Customer = customer,
                    Cashier = cashier,
                    AmountExclTax = queueOrder.AmountExclTax,
                    AmountInclTax = queueOrder.OrderTotal,
                    AmountTax = queueOrder.AmountTax,
                    Payments = new List<Payment>(),
                    Items = new List<Item>()

                };
                //Retrieve any payment information stored for the Order and return it in the Order\
                var payments = queueOrder.Payments.Select(p => new Payment
                {
                    TraderId = p.Id,
                    Method = p.MethodAccountXref.Id,
                    Reference = p.Reference,
                    AmountTendered = p.AmountTendered,
                    AmountAccepted = p.AmountAccepted
                });
                order.Payments.AddRange(payments);

                //oder items
                queueOrder.OrderItems.ForEach(item =>
            {
                if (item.IsInPrep) return;
                var extras = item.Extras.Select(e => new Models.TraderApi.Variant
                {
                    TraderId = e.Extra.Id,
                    Name = e.Extra.Name,
                    Discount = 0,
                    AmountInclTax = e.GrossPrice,
                    Taxes = e.OrderTaxes.Select(t => new Tax { TraderId = t.StaticTaxRate?.Id ?? 0, AmountTax = t.Value }).ToList()

                }).ToList();
                order.Items.Add(new Item
                {
                    TraderId = item.Variant?.CategoryItem?.Id ?? 0,
                    Name = item.Variant?.CategoryItem?.Name,
                    Quantity = item.Quantity,
                    Variant = new Models.TraderApi.Variant
                    {
                        TraderId = item.Variant.Id,
                        Name = item.Variant.Name,
                        Discount = item.Discount,
                        AmountInclTax = item.GrossPrice,
                        Taxes = item.OrderTaxes.Select(t => new Tax { TraderId = t.StaticTaxRate?.Id ?? 0, AmountTax = t.Value }).ToList()
                    },
                    Prepared = item.IsInPrep,
                    Extras = extras
                });
            });
                model.Add(order);
            });


            var posResult = new PdsQueueModel
            {
                Orders = model
            };


            return posResult;

        }
        //============= API DDS Delivery =======================
        /// <summary>
        /// The API to get information from delivery information from the Trader system
        /// Parameters: Device (from token) => DeliveryQueue => Collection of Deliveries
        /// </summary>
        /// <returns></returns>
        public DsDelivery DeliveryGet(PosRequest request, int traderId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/delivery DeliveryGet", request.UserId, null, request);

                var ddsDevice = dbContext.DdsDevice.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);
                if (ddsDevice == null)
                    return null;

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, request.Header, MethodBase.GetCurrentMethod().Name);

                return GetDsDelivery(traderId, request.UserId);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);

                return null;
            }
        }

        private DsDelivery GetDsDelivery(int deliveryId, string userId)
        {

            var user = dbContext.QbicleUser.AsNoTracking().FirstOrDefault(e => e.Id == userId);
            if (user == null)
                return null;


            var deliveryDb = dbContext.Deliveries.FirstOrDefault(e => e.Id == deliveryId && e.Status != DeliveryStatus.Deleted);

            var deliveryReturn = new DsDelivery
            {
                TraderId = deliveryDb.Id,
                Status = (int)deliveryDb.Status,
                TimeStarted = deliveryDb.TimeStarted.ConvertTimeFromUtc(user.Timezone),
                TimeFinished = deliveryDb.TimeFinished.ConvertTimeFromUtc(user.Timezone),
                EstimateTime = deliveryDb.EstimateTime,
                EstimateDistance = deliveryDb.EstimateDistance,
                DriverLatitude = deliveryDb.Driver?.Latitude,
                DriverLongitude = deliveryDb.Driver?.Longitude,
                Routes = deliveryDb.Routes,
                DriverId = deliveryDb.Driver?.Id ?? 0,
                DriverName = HelperClass.GetFullNameOfUser(deliveryDb.Driver?.User.User),
                Total = deliveryDb.Total,
                Orders = new List<Order>(),
                Token = deliveryDb.Timestamp.ToString().Encrypt(),
                Reference = deliveryDb.Reference.FullRef ?? $"#{deliveryDb.Id}"
            };

            if (deliveryDb.Driver?.DeliveryStatus != null)
                deliveryReturn.DriverDeliveryStatus = (int)deliveryDb.Driver.DeliveryStatus;

            int orderCompleted = 0;

            var orderReferences = new OrderProcessingHelper(dbContext).GetTradeOrderRef(deliveryDb.Orders);

            deliveryDb.Orders.OrderBy(s => s.DeliverySequence).ForEach(queueOrder =>
            {
                if (queueOrder.Status == PrepQueueStatus.Completed || queueOrder.Status == PrepQueueStatus.CompletedWithProblems)
                    orderCompleted++;

                Models.TraderApi.Customer customer = null;
                if (queueOrder.Customer != null)
                {
                    customer = new Models.TraderApi.Customer
                    {
                        TraderId = queueOrder.Customer.Id,
                        Name = queueOrder.Customer.CustomerName,
                        ContactRef = queueOrder.Customer.CustomerRef,
                        Avatar = dbContext.TraderContacts.Find(queueOrder.Customer.Id)?.AvatarUri.ToUri(),
                        Email = queueOrder.Customer.Email,
                        Phone = queueOrder.Customer.PhoneNumber,
                        Address = new Address
                        {
                            AddressLine1 = queueOrder.Customer?.FullAddress?.AddressLine1,
                            AddressLine2 = queueOrder.Customer?.FullAddress?.AddressLine2,
                            City = queueOrder.Customer?.FullAddress?.City,
                            Country = queueOrder.Customer?.FullAddress?.Country?.CommonName,
                            Latitude = queueOrder.Customer?.FullAddress?.Latitude,
                            Longitude = queueOrder.Customer?.FullAddress?.Longitude,
                            Postcode = queueOrder.Customer?.FullAddress?.PostCode
                        }

                    };

                }

                Cashier cashier = null;
                if (queueOrder.Cashier != null)
                {
                    var cAvatar = queueOrder.Cashier.Avatars.FirstOrDefault(e => e.isDefault)?.URI ?? queueOrder.Cashier.ProfilePic;
                    cashier = new Cashier
                    {
                        TraderId = queueOrder.Cashier.Id,
                        Avatar = cAvatar.ToUri(),
                        Forename = queueOrder.Cashier.Forename,
                        Surname = queueOrder.Cashier.Surname,
                        Pin = dbContext.DeviceUsers.FirstOrDefault(e => e.User.Id == queueOrder.Cashier.Id)?.Pin
                    };
                }
                var o = new Order
                {
                    TraderId = queueOrder.Id,
                    TradeOrderId = queueOrder.Id,
                    Status = new Status
                    {
                        OrderStatus = (int)queueOrder.Status,
                        CompletedDateTime = queueOrder.CompletedDate.ConvertTimeFromUtc(user.Timezone),
                        CreatedDateTime = queueOrder.CreatedDate.ConvertTimeFromUtc(user.Timezone),
                        DeliveredDateTime = queueOrder.DeliveredDate.ConvertTimeFromUtc(user.Timezone),
                        PaidDateTime = queueOrder.PaidDate.ConvertTimeFromUtc(user.Timezone),
                        PreparedDateTime = queueOrder.PreparedDate.ConvertTimeFromUtc(user.Timezone),
                        StartedDateTime = queueOrder.PrepStartedDate.ConvertTimeFromUtc(user.Timezone)
                    },
                    Delivery = (int)(queueOrder.Delivery?.Status ?? DeliveryStatus.New),
                    Classification = (int)queueOrder.Classification,
                    Type = queueOrder.Type,
                    Reference = queueOrder.OrderRef,
                    TradeOrderRef = orderReferences.FirstOrDefault(e => e.LinkedTraderId == queueOrder.LinkedOrderId)?.Reference ?? "",
                    Notes = queueOrder.Notes,
                    Customer = customer,
                    Cashier = cashier,
                    AmountExclTax = queueOrder.AmountExclTax,
                    AmountInclTax = queueOrder.OrderTotal,
                    AmountTax = queueOrder.AmountTax,
                    DeliverySequence = queueOrder.DeliverySequence,
                    Payments = new List<Payment>(),
                    Items = new List<Item>()
                };
                //Retrieve any payment information stored for the Order and return it in the Order
                var oPayments = dbContext.OrderPayments.Where(e => e.AssociatedOrder.Id == queueOrder.Id).ToList();

                oPayments.ForEach(p =>
                {
                    o.Payments.Add(new Payment
                    {
                        TraderId = p.Id,
                        Method = p.AssociatedOrder.Id,
                        Reference = p.Reference,
                        AmountTendered = p.AmountTendered,
                        AmountAccepted = p.AmountAccepted
                    });
                });
                //order items
                queueOrder.OrderItems.ForEach(item =>
            {
                if (item.IsInPrep) return;
                var extras = item.Extras.Select(e => new Models.TraderApi.Variant
                {
                    TraderId = e.Extra.Id,
                    Name = e.Extra.Name,
                    Discount = 0,
                    AmountInclTax = e.GrossPrice,
                    Taxes = e.OrderTaxes.Select(t => new Tax { TraderId = t.Id, AmountTax = t.Value }).ToList()

                }).ToList();
                o.Items.Add(new Item
                {
                    TraderId = item.Id,
                    Name = item.Variant?.TraderItem?.Name,
                    Quantity = item.Quantity,
                    Variant = new Models.TraderApi.Variant
                    {
                        TraderId = item.Variant.Id,
                        Name = item.Variant.Name,
                        Discount = item.Discount,
                        AmountInclTax = item.GrossPrice,
                        Taxes = item.OrderTaxes.Select(t => new Tax { TraderId = t.Id, AmountTax = t.Value }).ToList()
                    },
                    Prepared = item.IsInPrep,
                    Extras = extras
                });
            });

                deliveryReturn.Orders.Add(o);
            });

            deliveryReturn.OrderComplete = $"{orderCompleted}/{deliveryDb.Orders.Count} complete";

            return deliveryReturn;

        }

        /// <summary>
        /// The API to get information from delivery information from the Trader system
        /// Parameters: Device (from token) => DeliveryQueue => Collection of Deliveries
        /// </summary>
        /// <returns></returns>
        public DsDeliveriesRefs DeliveriesGet(PosRequest request, DeliveryStatus? status)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/delivery DeliveryGet", request.UserId, null, request);

                var user = dbContext.QbicleUser.AsNoTracking().FirstOrDefault(e => e.Id == request.UserId);
                if (user == null)
                    return null;

                var ddsDevice = dbContext.DdsDevice.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);
                if (ddsDevice == null)
                    return null;


                var deliveries = new List<DsDeliveries>();

                var drs = ddsDevice.Queue.Deliveries.Where(e => e.Status != DeliveryStatus.Deleted);
                if (status != null)
                    drs = drs.Where(s => s.Status == status).ToList();
                else
                    drs = drs.Where(s => s.Status == DeliveryStatus.New || s.Status == DeliveryStatus.Accepted || s.Status == DeliveryStatus.Started).ToList();
                drs.ForEach(d =>
                {
                    int orderCompleted = 0;

                    var delivery = new DsDeliveries
                    {
                        TraderId = d.Id,
                        Status = (int)d.Status,
                        TimeStarted = d.TimeStarted.ConvertTimeFromUtc(user.Timezone),
                        TimeFinished = d.TimeFinished.ConvertTimeFromUtc(user.Timezone),
                        EstimateTime = d.EstimateTime,
                        EstimateDistance = d.EstimateDistance,
                        Routes = d.Routes,
                        DriverId = d.Driver?.Id,
                        DriverName = HelperClass.GetFullNameOfUser(d.Driver?.User.User),
                        DriverLatitude = d.Driver?.Latitude,
                        DriverLongitude = d.Driver?.Longitude,
                        Total = d.Total,
                        Orders = new List<OrderDelivery>(),
                        Token = d.Timestamp.ToString().Encrypt(),
                        DriverDeliveryStatus = (int)(d.Driver?.DeliveryStatus ?? 0),
                        DriverStatus = d.Driver?.DeliveryStatus.GetDescription(),
                        Reference = d.Reference?.FullRef ?? $"#{d.Id}"
                    };

                    var orderReferences = new OrderProcessingHelper(dbContext).GetTradeOrderRef(d.Orders);

                    d.Orders.OrderBy(s => s.DeliverySequence).ForEach(queueOrder =>
                    {
                        // SendToPrep queue order not associate with the prepqueu until call send to prep
                        if (queueOrder.PrepQueue == null) return;

                        if (queueOrder.Status == PrepQueueStatus.Completed || queueOrder.Status == PrepQueueStatus.CompletedWithProblems)
                            orderCompleted++;

                        delivery.Orders.Add(new OrderDelivery
                        {
                            Id = queueOrder.Id,
                            Reference = queueOrder.OrderRef,
                            TradeOrderRef = orderReferences.FirstOrDefault(e => e.LinkedTraderId == queueOrder.LinkedOrderId)?.Reference ?? "",
                            Customer = queueOrder.Customer?.CustomerName,
                            Address = queueOrder.Customer?.FullAddress?.ToAddress(),
                            PhoneNumber = queueOrder.Customer?.PhoneNumber,
                            Latitude = queueOrder.Customer?.FullAddress?.Latitude ?? 0,
                            Longitude = queueOrder.Customer?.FullAddress?.Longitude ?? 0,
                            Status = (int)queueOrder.Status,
                            IsActive = (d.ActiveOrder?.Id ?? 0) == queueOrder.Id,
                            DeliverySequence = queueOrder.DeliverySequence ?? 0
                        });

                    });


                    delivery.OrderComplete = $"{orderCompleted}/{delivery.Orders.Count} complete";

                    deliveries.Add(delivery);

                });


                new PosRequestRules(dbContext).HangfirePosRequestLog(request, deliveries.ToJson(), MethodBase.GetCurrentMethod().Name);

                var response = new DsDeliveriesRefs();
                response.Deliveries.AddRange(deliveries);
                return response;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);

                return null;
            }


        }
        /// <summary>
        /// API call is required to be able to add a delivery to the system.
        /// Device serial number (from token) => Device => DeliveryQueue
        /// The UserId can be extracted from the token for the CreatedBy field
        /// </summary>
        /// <returns></returns>
        public IPosResult DeliveryAdd(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/delivery DeliveryAdd", request.UserId, null, request);


                var ddsDevice = dbContext.DdsDevice.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);
                if (ddsDevice == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device")
                };
                var deliveryQueue = ddsDevice.Queue;
                if (deliveryQueue == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "devivery queue")
                };

                var delivery = CreateDelivery(request.UserId, deliveryQueue.Location.Id);

                var posResult = new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Message = delivery.Timestamp.ToString().Encrypt(),
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
        /// add a delivery to the system
        /// </summary>
        /// <returns></returns>
        public Delivery CreateDelivery(string userId, int locationId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Create a delivery", userId);

            var user = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);

            var deliveryQueue = dbContext.DeliveryQueues.FirstOrDefault(l => l.Location.Id == locationId);

            var delivery = new Delivery
            {
                Status = DeliveryStatus.New,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = user,
                DeliveryQueue = deliveryQueue,
                Timestamp = DateTime.UtcNow,
                Reference = new TraderReferenceRules(dbContext).GetNewReference(deliveryQueue.Location.Domain.Id, Models.Trader.TraderReferenceType.Delivery),
                Routes = new DeliveryRoutes().ToJson()
            };

            dbContext.Deliveries.Add(delivery);
            dbContext.SaveChanges();

            new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.DeliveryStatus, delivery.Id, user.Id);

            return delivery;

        }

        /// <summary>
        /// An API is required to be able to delete a Delivery.
        /// The Delivery, identified by the Delivery Id, is added to the DeliveryQueueArchive that is associated with the current DeliveryQueue.
        /// The Delivery is then removed from the DeliveryQueue(Delivery.DeliveryQueue = NULL).
        /// The status of the Delivers is set to Deleted.
        /// Delivery Id (Must be supplied in response to call to 'api/dds/delivery')
        /// </summary>
        /// <returns>
        /// If Delivery cannot be found then return an error to say that is not possible.
        /// Set Status = Deleted
        /// Move it to the DeliveryQueueArchive
        /// break the connection with the DeliveryQueue
        /// </returns>
        public IPosResult DeliveryRemove(PosRequest request, DsDeliveryParameter deliveryParameter)
        {
            try
            {

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/delivery Delivery remove", request.UserId, null, request, deliveryParameter);

                //if the Delivery does not exist, return NotAcceptable response
                var delivery = dbContext.Deliveries.Find(deliveryParameter.Id);
                if (delivery == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "delivery")
                };
                //if the Delivery does not have a status of New or Start, return NotAcceptable response
                if (delivery.Status != DeliveryStatus.New && delivery.Status != DeliveryStatus.Started) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "delivery")
                };
                //Delete any relationship that links Orders to the Delivery.Do NOT delete the Orders


                delivery.Orders.Clear();
                delivery.Status = DeliveryStatus.Deleted;
                dbContext.SaveChanges();

                new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.DeliveryStatus, delivery.Id, request.UserId);

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
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request, deliveryParameter);

                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Status = HttpStatusCode.InternalServerError,
                    Message = ex.ToJson()
                };
            }
        }



        /// <summary>
        /// API that is used to set just the Route proterty of a Delivery.
        /// found and its Routes property is updated with whatever the string value of Routes is.  
        /// </summary>
        /// <param name="request"></param>
        /// <param name="routes"></param>
        /// <returns></returns>
        public IPosResult DeliveryRoute(PosRequest request, DsDeliveryRoutes deliveryRoutes)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/routes Delivery update", request.UserId, null, request, deliveryRoutes);

                //If Delivery cannot be found then return an error to say that is not possible.
                var delivery = dbContext.Deliveries.Find(deliveryRoutes.TraderId);
                if (delivery == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotFound,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "delivery")
                };

                //found and its Routes property is updated with whatever the string value of Routes is. 
                delivery.Routes = deliveryRoutes.Routes;


                if (dbContext.Entry(delivery).State == EntityState.Detached)
                    dbContext.Deliveries.Attach(delivery);
                dbContext.Entry(delivery).State = EntityState.Modified;
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
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request, deliveryRoutes);

                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Status = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        /// <summary>
        /// Delivery manager update the delivery (add/ remove order, update delivery status)
        /// </summary>
        /// <param name="request"></param>
        /// <param name="delivery"></param>
        /// <returns></returns>
        public ReturnJsonModel DeliveryUpdate(PosRequest request, DsDelivery deliveryParameter, bool fromWeb = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/dds/status Delivery update", request.UserId, null, request, deliveryParameter);

                //Check the Status to be set for the Delivery if the Delivery status is not Finish or Completed, return NotAcceptable response
                if (!fromWeb)
                {
                    if ((DeliveryStatus)deliveryParameter.Status != DeliveryStatus.New) return new ReturnJsonModel
                    {
                        result = false,
                        Object = HttpStatusCode.NotAcceptable,
                        msg = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "The delivery started, cannot update! Only update the Delivery when status is New!")
                    };
                }
                else
                {
                    if ((DeliveryStatus)deliveryParameter.Status != DeliveryStatus.New) return new ReturnJsonModel
                    {
                        result = false,
                        Object = HttpStatusCode.NotAcceptable,
                        msg = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "The delivery started, cannot update! Only update the Delivery when status is New!")
                    };
                }

                //If an invalid status is supplied then return an error to say that is not possible.
                if (!fromWeb)
                {
                    if (!deliveryParameter.Status.TryParseEnum<DriverStatus>() || deliveryParameter.Status > 5) return new ReturnJsonModel
                    {
                        result = false,
                        Object = HttpStatusCode.NotAcceptable,
                        msg = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Status")
                    };
                }
                else
                {
                    if (!deliveryParameter.Status.TryParseEnum<DeliveryStatus>() || deliveryParameter.Status > 3) return new ReturnJsonModel
                    {
                        result = false,
                        Object = HttpStatusCode.NotAcceptable,
                        msg = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Status")
                    };
                }


                //If Delivery cannot be found then return an error to say that is not possible.
                var delivery = dbContext.Deliveries.Find(deliveryParameter.TraderId);
                if (delivery == null) return new ReturnJsonModel
                {
                    result = false,
                    Object = HttpStatusCode.NotAcceptable,
                    msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "delivery")
                };
                if (!fromWeb)
                    if (delivery.Timestamp.ToString() != deliveryParameter.Token.Decrypt()) return new ReturnJsonModel
                    {
                        result = false,
                        Object = HttpStatusCode.NotAcceptable,
                        msg = ResourcesManager._L("ERROR_DELIVERY_UPDATE_OLDEST"),
                        actionVal = 3
                    };


                //Update Delivery with parameter
                Driver driver = null;

                if (deliveryParameter.DriverId > 0)
                {
                    //valid a driver only in one delivery
                    var isInDelivery = dbContext.Deliveries.Any(d => d.Driver.Id == deliveryParameter.DriverId && d.Id != deliveryParameter.TraderId);
                    if (isInDelivery) return new ReturnJsonModel
                    {
                        result = false,
                        Object = HttpStatusCode.NotAcceptable,
                        msg = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "The Driver is not available, he is doing another delivery, cannot update!")
                    };

                    driver = dbContext.Drivers.FirstOrDefault(d => d.Id == deliveryParameter.DriverId && d.Status == DriverStatus.IsAvailable);
                    if (driver == null)
                        return new ReturnJsonModel
                        {
                            result = false,
                            Object = HttpStatusCode.NotAcceptable,
                            msg = "Driver is not Available."
                        };
                    else
                        driver.DeliveryStatus = DriverDeliveryStatus.NotSet;
                }


                //the Driver.DeliveryStatus can be set to anything because when the Driver.Status=IsAvailable, the Driver is not on a Delivery

                delivery.Status = (DeliveryStatus)deliveryParameter.Status;
                delivery.Driver = driver;
                delivery.Total = 0;
                delivery.TimeStarted = deliveryParameter.TimeStarted;
                delivery.TimeFinished = deliveryParameter.TimeFinished;
                delivery.EstimateTime = deliveryParameter.EstimateTime;
                delivery.EstimateDistance = deliveryParameter.EstimateDistance;
                delivery.Routes = deliveryParameter.Routes;
                delivery.Timestamp = DateTime.UtcNow;


                //string messageOrderExitedDelivery = "";
                delivery.Orders.ForEach(o =>
                {
                    o.Delivery = null;
                    o.DeliverySequence = 0;
                });


                delivery.Orders.Clear();
                if (deliveryParameter.Orders != null && deliveryParameter.Orders.Any())
                {
                    int orderSequence = 1;
                    deliveryParameter.Orders.ForEach(order =>
                    {

                        var qOrder = dbContext.QueueOrders.Find(order.TraderId);
                        if (qOrder == null) return;

                        //if the order in another delivery (Status is NEW) then remove it from delivery and add it into current delivery

                        //var otherDelivery = qOrder.Delivery;
                        //if (otherDelivery != null && otherDelivery.Id != delivery.Id && otherDelivery.Status == DeliveryStatus.New)
                        //{
                        //    otherDelivery.Orders.Remove(qOrder);

                        //    otherDelivery.Total += otherDelivery.Orders.Sum(e => e.OrderTotal);

                        //    int otherSequence = 1;
                        //    otherDelivery.Orders.ForEach(otherOrder =>
                        //    {
                        //        qOrder.DeliverySequence = otherSequence;

                        //        if (otherSequence == 1)
                        //            otherDelivery.ActiveOrder = otherOrder;

                        //        otherSequence++;
                        //    });
                        //}

                        qOrder.Delivery = delivery;

                        //All Orders in the Delivery have a Status of Completing, and will remain at that status until the Customer gets the order.
                        qOrder.Status = PrepQueueStatus.Completing;

                        new OrderProcessingHelper(dbContext).QueueOrderApplyDate(qOrder, qOrder.Status, null, null, DateTime.UtcNow, null);

                        qOrder.DeliverySequence = orderSequence;

                        delivery.Orders.Add(qOrder);
                        //delivery.Total += qOrder.OrderTotal;

                        if (orderSequence == 1)
                            delivery.ActiveOrder = qOrder;

                        orderSequence++;
                    });
                    delivery.Total += delivery.Orders.Sum(e => e.OrderTotal);
                }




                if (dbContext.Entry(delivery).State == EntityState.Detached)
                    dbContext.Deliveries.Attach(delivery);
                dbContext.Entry(delivery).State = EntityState.Modified;
                dbContext.SaveChanges();


                new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.DeliveryStatus, delivery.Id, request.UserId);

                //if (!string.IsNullOrEmpty(messageOrderExitedDelivery))
                //    messageOrderExitedDelivery = $"The orders existed in the other Delivery: TraderId in: {messageOrderExitedDelivery}";

                var posResult = new ReturnJsonModel
                {
                    result = true,
                    Object = request.Status,
                    msg = delivery.Timestamp.ToString().Encrypt(),
                    msgName = ""
                };

                if (!fromWeb)
                    new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);
                if (fromWeb)
                    posResult.Object2 = delivery;

                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request, deliveryParameter);

                return new ReturnJsonModel
                {
                    result = false,
                    Object = HttpStatusCode.InternalServerError,
                    msg = ex.Message
                };
            }
        }

    }
}
