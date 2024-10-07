using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.Firebase;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Model.Firebase;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Ods
{
    public class OdsApiRules
    {
        ApplicationDbContext dbContext;

        public OdsApiRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        /// <summary>
        /// The call sets the SerialNumber for the PrepDisplayDevice identified by the supplied DeviceID.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IPosResult TabletForDevice(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pds/device sets the SerialNumber for the PrepDisplayDevice", request.UserId, null, request);

                //Does the Device, identified by the DeviceId, exist?
                var prepDisplayDevice = dbContext.PrepDisplayDevices.Find(request.DeviceId);
                if (prepDisplayDevice == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device")
                };
                //Is the user, identified by UserID, an Administrator of the device?
                if (prepDisplayDevice.Administrators.All(u => u.Id != request.UserId))
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Status = HttpStatusCode.NotAcceptable,
                        Message = ResourcesManager._L("ERROR_USER_NOT_ADMIN_DEVICE")
                    };

                //Is the SerialNumber used in any device OTHER than the Device identified in 1.
                //If it is then that Device's serial number MUST be set to null.
                var prepDisplayDeviceSerialNumbers = dbContext.PrepDisplayDevices.Where(e => e.SerialNumber == request.SerialNumber).ToList();
                if (prepDisplayDeviceSerialNumbers.Any())
                {
                    prepDisplayDeviceSerialNumbers.ForEach(device =>
                    {
                        device.SerialNumber = "";
                    });
                    dbContext.SaveChanges();
                }

                prepDisplayDevice.SerialNumber = request.SerialNumber;
                if (dbContext.Entry(prepDisplayDevice).State == EntityState.Detached)
                    dbContext.PrepDisplayDevices.Attach(prepDisplayDevice);
                dbContext.Entry(prepDisplayDevice).State = EntityState.Modified;
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
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, null, request);
                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Status = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }


        }

        /// <summary>
        /// The call returns the list of PDS devices for which the user, identified in the token, is an administrator.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public DsDeviceForUser GetDeviceForUser(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pds/device returns the list of PDS devices for which the user", request.UserId, null, request);

                var user = new UserRules(dbContext).GetById(request.UserId);
                //check if not found user
                if (user == null)
                    return null;


                var prepDisplayDevice = dbContext.PrepDisplayDevices.Where(e => e.Administrators.Any(u => u.Id == request.UserId)).ToList();


                //All devices, no matter which Domain or  Location they are associated with, if the user is an Admin dor that device the Device must be included in the list.
                //var posDevices = dbContext.PosDevices.Where(e => e.Administrators.Any(u => u.User.Id == request.UserId)).ToList();

                var devices = new List<OdsDeviceResult>();
                prepDisplayDevice.ForEach(d =>
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
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, null, request);
                return null;
            }


        }

        /// <summary>
        /// API to provide a list of the Orders in the current queue.
        /// This list of orders will be displayed in the Preparation Display System(PDS)
        /// </summary>
        /// <param name="numberOfHours">GetOrderTimeSpan enum
        /// Hours_24 = 1,
        /// Hours_48 = 2,
        /// Hours_72 = 3
        /// </param>
        /// <param name="request"></param>
        /// <returns></returns>
        public PdsQueueModel PrepQueue(PosRequest request, bool showIsPrepared = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pds/queue list of orders will be displayed in the Preparation Display System(PDS)", request.UserId, null, request);

                var pdsDevice = new PDSRules(dbContext).GetPrepDisplayDeviceBySerialNumber(request.SerialNumber);
                if (pdsDevice == null)
                    return null;

                var user = new UserRules(dbContext).GetById(request.UserId);

                if (user == null)
                    return null;

                var model = new List<Order>();


                var queueOrders = pdsDevice.Queue.QueueOrders;

                //If no Device Type is associated with the identified PrepDisplayDevice, 
                //no filtering is carried out on the QueueOrders in the Preparation Queue, ALL QueueOrders in the Preparation queue are returned.
                if (pdsDevice.Type != null)
                {
                    /*
                     The filter for Orders will now take into account the ODS/PDS Display Type associated with the PrepDisplayDevice that is identified in the API as making the API call.
                     The QueueOrders in the Preparation Queue will now be filtered by the Order Types and Order Statuses to be returned to the calling tablet.
                     */

                    var orderTypes = pdsDevice.Type.AssociatedOrderTypes.Select(e => e.Classification);
                    var statuses = pdsDevice.Type.OrderStatus.Select(e => e.Status);

                    if (statuses.Any())
                        queueOrders = queueOrders.Where(e => statuses.Contains(e.Status)).ToList();

                    if (orderTypes.Any())
                        queueOrders = queueOrders.Where(e => orderTypes.Contains(e.Classification)).ToList();

                }

                if (queueOrders != null)
                {
                    queueOrders.OrderBy(e => e.CreatedDate).ForEach(queueOrder =>
                    {
                        var order = InitQueueOrder(queueOrder, user.Timezone, showIsPrepared);
                        model.Add(order);
                    });
                }

                var posResult = new PdsQueueModel
                {
                    Orders = model
                };

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);

                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);

                return new PdsQueueModel();
            }
        }

        private Order InitQueueOrder(QueueOrder queueOrder, string timezone, bool showIsPrepared)
        {
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
                var cAvatar = dbContext.TraderContacts.AsNoTracking().FirstOrDefault(e => e.Id == queueOrder.Customer.Id)?.AvatarUri;
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
                    Pin = dbContext.DeviceUsers.AsNoTracking().FirstOrDefault(e => e.User.Id == queueOrder.Cashier.Id)?.Pin
                };
            }

            var order = new Order
            {
                TraderId = queueOrder.Id,
                LinkedTraderId = queueOrder.LinkedOrderId,
                Status = new Status
                {
                    OrderStatus = (int)queueOrder.Status,
                    CompletedDateTime = queueOrder.CompletedDate.ConvertTimeFromUtc(timezone),
                    CreatedDateTime = queueOrder.QueuedDate.ConvertTimeFromUtc(timezone),
                    DeliveredDateTime = queueOrder.DeliveredDate.ConvertTimeFromUtc(timezone),
                    PaidDateTime = queueOrder.PaidDate.ConvertTimeFromUtc(timezone),
                    PreparedDateTime = queueOrder.PreparedDate.ConvertTimeFromUtc(timezone),
                    StartedDateTime = queueOrder.PrepStartedDate.ConvertTimeFromUtc(timezone)
                },
                Delivery = (int)(queueOrder.Delivery?.Status ?? DeliveryStatus.New),
                Classification = (int)queueOrder.Classification,
                Type = queueOrder.Type,
                Reference = $"{queueOrder.OrderRef} <{queueOrder.SalesChannel}>",
                Notes = queueOrder.Notes,
                Customer = customer,
                Cashier = cashier,
                AmountExclTax = queueOrder.AmountExclTax,
                AmountInclTax = queueOrder.OrderTotal,
                AmountTax = queueOrder.AmountTax,
                Payments = new List<Payment>(),
                Items = new List<Item>(),
                Table = queueOrder.Table,
                NumberAtTable = queueOrder.NumberAtTable,
                SplitTimes = queueOrder.SplitTimes,
                SplitType = queueOrder.SplitType,
                SplitAmounts = queueOrder.SplitAmounts.Select(e => new OrderSplitAmount { SplitNo = e.SplitNo, Amount = e.Amount }).ToList(),
                IsCancelled = queueOrder.IsCancelled,
                CancelledItems = new List<Item>(),
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

            queueOrder.OrderItems.ForEach(item =>
            {
                /// In the previous interaction between the PDS and PrepQueue this was correct
                /// but now we will need to see items that have been prepared but may have been cancelled
                /// we will have to remove the following and get the PDS to display properly
                if (showIsPrepared == false)
                {
                    if (item.IsInPrep == true || item.IsNotForPrep == true)
                        return;
                }
                var extras = item.Extras.Select(e => new Models.TraderApi.Variant
                {
                    TraderId = e.Extra.Id,
                    Name = e.Extra?.Name,
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
                    Extras = extras,
                    SplitNo = item.SplitNo,
                    Note = item.Note,
                    IsCancelled = item.IsCancelled,
                    IsCancelledByLaterPrep = item.IsCancelledByLaterPrep,
                    IsNotForPrep = item.IsNotForPrep,
                    LinkedItemId = item.LinkedItemId,
                });
            });
            queueOrder.CancelledOrderItems.ForEach(item =>
            {
                /// In the previous interaction between the PDS and PrepQueue this was correct
                /// but now we will need to see items that have been prepared but may have been cancelled
                /// we will have to remove the following and get the PDS to display properly
                if (showIsPrepared == false)
                {
                    if (item.IsInPrep == true || item.IsNotForPrep == true)
                        return;
                }
                var extras = item.Extras.Select(e => new Models.TraderApi.Variant
                {
                    TraderId = e.Extra.Id,
                    Name = e.Extra?.Name,
                    Discount = 0,
                    AmountInclTax = e.GrossPrice,
                    Taxes = e.OrderTaxes.Select(t => new Tax { TraderId = t.StaticTaxRate?.Id ?? 0, AmountTax = t.Value }).ToList()

                }).ToList();
                order.CancelledItems.Add(new Item
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
                    Extras = extras,
                    SplitNo = item.SplitNo,
                    Note = item.Note,
                    IsCancelled = item.IsCancelled,
                    IsCancelledByLaterPrep = item.IsCancelledByLaterPrep,
                    IsNotForPrep = item.IsNotForPrep,
                    LinkedItemId = item.LinkedItemId,
                });
            });

            return order;
        }


        /// <summary>
        /// API to update the status of an order in a Queue
        /// </summary>
        /// <param name="order"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public IPosResult PrepQueueStatusUpdate(PdsOrderUpdate order, PosRequest request, ref Status qStatus)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pds/status update the status of an order in a Queue", request.UserId, null, order, request);

                //validation parameter

                if (order.Id <= 0 || order.StatusChangeDateTime == DateTime.MinValue || !order.Status.TryParseEnum<PrepQueueStatus>() || order.Status == PrepQueueStatus.NotStarted) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Status")
                };


                //Based on the Serial Number, identify the PrepDisplayDevice (PrepDisplayDevice.SerialNumber)
                var pdsDevice = new PDSRules(dbContext).GetPrepDisplayDeviceBySerialNumber(request.SerialNumber);
                if (pdsDevice == null || pdsDevice.Queue == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device")
                };

                //Based on the PrepDisplayDevice, identify the relevant PrepQueue(PrepDisplayDevice.Queue)
                var user = dbContext.QbicleUser.AsNoTracking().FirstOrDefault(e => e.Id == request.UserId);

                var queueOrder = pdsDevice.Queue.QueueOrders.FirstOrDefault(e => e.Id == order.Id);
                if (queueOrder == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Order")
                };


                var statusChangeDateTime = order.StatusChangeDateTime.ConvertTimeToUtc(user.Timezone);

                if (order.Status == PrepQueueStatus.NotStarted)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Status = HttpStatusCode.NotAcceptable,
                        Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Status")
                    };

                queueOrder.Status = order.Status;

                new OrderProcessingHelper(dbContext).QueueOrderApplyDate(queueOrder, queueOrder.Status,
                    queueOrder.QueuedDate, queueOrder.PrepStartedDate, queueOrder.PreparedDate, queueOrder.CompletedDate);

                var queueOrderUpdated = new QueueOrder
                {
                    Id = queueOrder.Id,
                    OrderRef = queueOrder.OrderRef,
                    Table = queueOrder.Table,
                    PrepQueue = new PrepQueue { Id = queueOrder.PrepQueue?.Id ?? 0 }
                };


                //When the Order Status is set to Completing
                if (order.Status == PrepQueueStatus.Completing)
                {   //If the Order is associated with a Delivery
                    if (queueOrder.Delivery != null)//orders on Delivery are Status = Completing and there is a Driver associated with Delivery
                        if (queueOrder.Delivery.Orders.Count > 0 && queueOrder.Delivery.Orders.TrueForAll(s => s.Status == PrepQueueStatus.Completing) && queueOrder.Delivery.Driver != null)
                        {
                            queueOrder.Delivery.Status = DeliveryStatus.Started;
                            queueOrder.Delivery.TimeStarted = null;

                            new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.DeliveryStatus, queueOrder.Delivery.Id, request.UserId);
                        }
                }
                                
                if (order.Status == PrepQueueStatus.Completed || order.Status == PrepQueueStatus.CompletedWithProblems)
                {

                    //(1) get the PrepQueue from the QueueOrder)
                    var prepQueue = queueOrder.PrepQueue;

                    //(2) Get the PrepQueueArchive with the PrepQueue as ParentPrepQueue (var archivePrepQueue)
                    var archivePrepQueue = dbContext.PrepQueueArchives.First(a => a.ParentPrepQueue.Id == prepQueue.Id);

                    //(3) Then, to move the QueueOrder to archive
                    queueOrder.PrepQueue = null;
                    queueOrder.PrepQueueArchive = archivePrepQueue;
                    queueOrder.ArchivedDate = DateTime.UtcNow;
                }


                if (dbContext.Entry(queueOrder).State == EntityState.Detached)
                    dbContext.QueueOrders.Attach(queueOrder);
                dbContext.Entry(queueOrder).State = EntityState.Modified;
                dbContext.SaveChanges();

               new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.Preparation, 0, user.Id, new List<int> { queueOrder.Id });


                var posResult = new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.OK,
                    Message = "OK."
                };

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);

                qStatus = new Status
                {
                    OrderStatus = (int)queueOrder.Status,
                    CompletedDateTime = queueOrder.CompletedDate.ConvertTimeFromUtc(user.Timezone),
                    CreatedDateTime = queueOrder.QueuedDate.ConvertTimeFromUtc(user.Timezone),
                    DeliveredDateTime = queueOrder.DeliveredDate.ConvertTimeFromUtc(user.Timezone),
                    PaidDateTime = queueOrder.PaidDate.ConvertTimeFromUtc(user.Timezone),
                    PreparedDateTime = queueOrder.PreparedDate.ConvertTimeFromUtc(user.Timezone),
                    StartedDateTime = queueOrder.PrepStartedDate.ConvertTimeFromUtc(user.Timezone)
                };

                //push notification to PDS(s)
                queueOrderUpdated.PushPdsStatusUpdate(new List<string> { request.SerialNumber });
               
                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, order, request);

                return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.Forbidden,
                    Message = ex.Message
                };
            }
        }

        public IPosResult SendToPrep(Order saleOrder, PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/sendtoprep SendToPrep", request.UserId, null, saleOrder, request);

                if (!saleOrder.Classification.TryParseEnum<OrderTypeClassification>())
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Order type"),
                        Status = HttpStatusCode.NotAcceptable
                    };

                var user = dbContext.QbicleUser.AsNoTracking().Any(e => e.Id == request.UserId);
                if (!user)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "user"),
                        Status = HttpStatusCode.NotAcceptable
                    };

                var deviceLocationId = dbContext.PosDevices.AsNoTracking().FirstOrDefault(e => e.SerialNumber == request.SerialNumber && !e.Archived)?.Location?.Id;
                if (deviceLocationId == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device"),
                        Status = HttpStatusCode.NotAcceptable
                    };


                var posSetting = dbContext.PosSettings.AsNoTracking().Any(e => e.Location.Id == deviceLocationId && e.DefaultWorkGroup != null && e.DefaultWalkinCustomer != null);
                if (!posSetting)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "POS Setting configuration"),
                        Status = HttpStatusCode.NotAcceptable
                    };
                saleOrder.SalesChannel = SalesChannelEnum.POS;
                var linkedOrderId = string.IsNullOrEmpty(saleOrder.LinkedTraderId) ? Guid.NewGuid().ToString() : saleOrder.LinkedTraderId;

                var job = new PosSendToPrepParameter
                {
                    SaleOrder = saleOrder,
                    Request = request,
                    LinkedOrderId = linkedOrderId,
                    EndPointName = "sendtoprep"
                };

                Task tskHangfire = new Task(async () =>
                {
                    await new QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, saleOrder.ToJson(), MethodBase.GetCurrentMethod().Name);

                return new IPosResult
                {
                    IsTokenValid = true,
                    Message = ResourcesManager._L("SUCCESSFULLY"),
                    Status = HttpStatusCode.OK,
                    TraderId = job.LinkedOrderId
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, saleOrder, request);

                return new IPosResult
                {
                    IsTokenValid = true,
                    Message = ResourcesManager._L("ERROR_MSG_98"),
                    Status = HttpStatusCode.Forbidden
                };
            }
        }


        public void SenToPrepHangfieExecute(PosSendToPrepParameter job)
        {
            var saleOrder = job.SaleOrder; var request = job.Request;


            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/sendtoprep SendToPrep", request.UserId, null, saleOrder, request);


            var user = new UserRules(dbContext).GetById(saleOrder.Cashier.TraderId);


            var device = new PosDeviceRules(dbContext).GetBySerialNumber(request.SerialNumber);


            var posSetting = dbContext.PosSettings.FirstOrDefault(e => e.Location.Id == device.Location.Id);


            var walkinCustomerId = posSetting.DefaultWalkinCustomer.Id;

            #region Get TraderContact and  an OrderCustomer for the QueueOrder

            var contact = dbContext.TraderContacts.FirstOrDefault(e => e.Id == walkinCustomerId);
            var customer = new OrderCustomer();
            var contactId = walkinCustomerId;

            // Does the SaleOrder have a customer
            if (saleOrder.Customer != null && !string.IsNullOrEmpty(saleOrder.Customer.Email))
            {
                // Need to add a Trader Contact here if possible
                contact = new OrderProcessingHelper(dbContext).GetCreateTraderContactFromCustomer(saleOrder.Customer, device.Location.Domain, user, SalesChannelEnum.POS);

                if (contact != null) // Get the OrderCustomer from the saleOrder customer and updated the data where necessary from the contact
                    customer = new OrderProcessingHelper(dbContext).MapCustomer2OrderCustomer(saleOrder.Customer, contact);
                else
                    customer = new OrderProcessingHelper(dbContext).MapCustomer2OrderCustomer(saleOrder.Customer);
            }
            else
                customer = new OrderProcessingHelper(dbContext).MapTraderContact2OrderCustomer(contact); //In this case the contact can only be the walkin in contact


            if (customer.FullAddress.Country == null)
                customer.FullAddress = null;


            /*
                 If there is a QbicleUser (ApplicationUser), in the Qbicles System, 
               with the same email address as the TraderContact link the QbicleUser to the new TraderContact
            */
            new TraderContactRules(dbContext).LinkUserToContact(contact);


            #endregion

            var prepQueueStatus = dbContext.PosSettings.FirstOrDefault(e => e.Location.Id == device.Location.Id)?.OrderStatusWhenAddedToQueue ?? PrepQueueStatus.Completed;

            var queueOrder = CreateQueueOrderForPrep(saleOrder, customer, user, device.PreparationQueue.Id, prepQueueStatus, job.LinkedOrderId);
            //push notification to PDS(s)
            queueOrder.PushPdsNewOrder();
        }

        /// <summary>
        /// CreateQueueOrderForPrep
        /// </summary>
        /// <param name="saleOrder"></param>
        /// <param name="customer"></param>
        /// <param name="userId"></param>
        /// <param name="prepQueueId"></param>
        /// <param name="prepStatus"></param>
        /// <returns>queueOrder.LinkedOrderId</returns>
        private QueueOrder CreateQueueOrderForPrep(Order saleOrder, OrderCustomer customer, ApplicationUser user, int prepQueueId, PrepQueueStatus prepStatus, string linkedOrderId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, saleOrder, customer, user.Id, prepQueueId, prepStatus);

                // Get the prep queue
                var prepQueue = dbContext.PrepQueues.FirstOrDefault(p => p.Id == prepQueueId);
                if (prepQueue == null)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "PrepQueue is not found.", null, null,
                        saleOrder, customer, user.Id, prepQueueId, prepStatus);
                    return new QueueOrder();
                }

                // Create the Queue Order
                var queueOrder = new OrderProcessingHelper(dbContext).CreateQueueOrder(saleOrder, customer, user, prepStatus, false, null, prepQueue, linkedOrderId);

                //return queueOrder.LinkedOrderId;
                return queueOrder;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, saleOrder, customer, user.Id, prepQueueId, prepStatus);
                return new QueueOrder();
            }
        }

        public PdsQueueModel GetQueueOrderById(int id, PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pds/queue by id of orders will be displayed in the Preparation Display System(PDS)", request.UserId, null, request);

                var pdsDevice = new PDSRules(dbContext).GetPrepDisplayDeviceBySerialNumber(request.SerialNumber);
                if (pdsDevice == null)
                    return null;

                var user = new UserRules(dbContext).GetById(request.UserId);

                if (user == null)
                    return null;

                var model = new List<Order>();


                var queueOrder = dbContext.QueueOrders.FirstOrDefault(e => e.Id == id);
                if (queueOrder == null)
                    return null;

                var order = InitQueueOrder(queueOrder, user.Timezone, true);
                model.Add(order);

                var posResult = new PdsQueueModel
                {
                    Orders = model
                };

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, posResult.ToJson(), MethodBase.GetCurrentMethod().Name);

                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);

                return new PdsQueueModel();
            }
        }

        public void TestFirebase(DevicesToken registrationTokens, PosRequest request)
        {
            //var title = "New order on the queue";
            //var body = "OrderRef:POS1-868686, Table:868686";

            //var data = new Dictionary<string, string>()
            //{
            //    {"CallbackMethod" , "GET"},
            //    {"CallbackApi" , "api/pds/queue/order" },
            //    {"Parameter" , "id" },
            //    {"ParameterValue" , "868686"}
            //};
            //var message = new
            //{
            //    //to, // Recipient device token
            //    registration_ids = registrationTokens.Tokens,
            //    notification = new { title, body },
            //    data = data
            //};


            //var notificationContent = JsonConvert.SerializeObject(message);

            //Task firebase = new Task(async () =>
            //{
            //    await FirebasePushNotification.PushAsync(notificationContent);
            //});
            //firebase.Start();

        }
    }
}
