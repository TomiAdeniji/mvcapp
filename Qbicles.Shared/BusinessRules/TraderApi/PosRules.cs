using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.CMs;
using Qbicles.BusinessRules.Firebase;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Model.Firebase;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.PoS
{
    public class PosRules
    {

        ApplicationDbContext dbContext;

        public PosRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public UserInformation GetUserInformation(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/user: Get User Information", null, null, userId);

                var user = dbContext.QbicleUser.AsNoTracking().Where(e => e.Id == userId).Select(m => new UserInformation
                {
                    TraderId = m.Id,
                    Email = m.Email,
                    Timezone = m.Timezone,
                    Avatar = m.ProfilePic,
                    PhoneNumber = m.PhoneNumber,
                    UserName = m.UserName,
                    Forename = m.Forename,
                    DisplayUserName = m.DisplayUserName,
                    Profile = m.Profile,
                    Surname = m.Surname
                }).FirstOrDefault();
                user.Avatar = user.Avatar.ToUriString();

                return user;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return null;
            }

        }

        public List<PosUserInfomation> PosUsersInfomation(string serialNumber)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/users", null, null, serialNumber);

                var device = dbContext.PosDevices.FirstOrDefault(e => e.SerialNumber == serialNumber);
                if (device == null) return new List<PosUserInfomation>();

                var users = device.Users.Select(m => new PosUserInfomation
                {
                    TraderId = m.User.Id,
                    Email = m.User.Email,
                    Timezone = m.User.Timezone,
                    Avatar = m.User.ProfilePic.ToUriString(),
                    PhoneNumber = m.User.PhoneNumber,
                    UserName = m.User.UserName,
                    Forename = m.User.Forename,
                    DisplayUserName = m.User.DisplayUserName,
                    Profile = m.User.Profile,
                    Surname = m.User.Surname,
                    PIN = m.Pin,
                    TillCashier = device.DeviceCashiers.Any(u => u.User.Id == m.User.Id),
                    TillSupervisor = device.DeviceSupervisors.Any(u => u.User.Id == m.User.Id),
                    TillManager = device.DeviceManagers.Any(u => u.User.Id == m.User.Id),
                    TillUser = device.Users.Any(u => u.User.Id == m.User.Id)
                });

                
                var cashiers = device.DeviceCashiers.Select(m => new PosUserInfomation
                {
                    TraderId = m.User.Id,
                    Email = m.User.Email,
                    Timezone = m.User.Timezone,
                    Avatar = m.User.ProfilePic.ToUriString(),
                    PhoneNumber = m.User.PhoneNumber,
                    UserName = m.User.UserName,
                    Forename = m.User.Forename,
                    DisplayUserName = m.User.DisplayUserName,
                    Profile = m.User.Profile,
                    Surname = m.User.Surname,
                    PIN = m.Pin,
                    TillCashier = device.DeviceCashiers.Any(u => u.User.Id == m.User.Id),
                    TillSupervisor = device.DeviceSupervisors.Any(u => u.User.Id == m.User.Id),
                    TillManager = device.DeviceManagers.Any(u => u.User.Id == m.User.Id),
                    TillUser = device.Users.Any(u => u.User.Id == m.User.Id)
                });

                
                var supervisors = device.DeviceSupervisors.Select(m => new PosUserInfomation
                {
                    TraderId = m.User.Id,
                    Email = m.User.Email,
                    Timezone = m.User.Timezone,
                    Avatar = m.User.ProfilePic.ToUriString(),
                    PhoneNumber = m.User.PhoneNumber,
                    UserName = m.User.UserName,
                    Forename = m.User.Forename,
                    DisplayUserName = m.User.DisplayUserName,
                    Profile = m.User.Profile,
                    Surname = m.User.Surname,
                    PIN = m.Pin,
                    TillCashier = device.DeviceCashiers.Any(u => u.User.Id == m.User.Id),
                    TillSupervisor = device.DeviceSupervisors.Any(u => u.User.Id == m.User.Id),
                    TillManager = device.DeviceManagers.Any(u => u.User.Id == m.User.Id),
                    TillUser = device.Users.Any(u => u.User.Id == m.User.Id)
                });

                
                var managers = device.DeviceManagers.Select(m => new PosUserInfomation
                {
                    TraderId = m.User.Id,
                    Email = m.User.Email,
                    Timezone = m.User.Timezone,
                    Avatar = m.User.ProfilePic.ToUriString(),
                    PhoneNumber = m.User.PhoneNumber,
                    UserName = m.User.UserName,
                    Forename = m.User.Forename,
                    DisplayUserName = m.User.DisplayUserName,
                    Profile = m.User.Profile,
                    Surname = m.User.Surname,
                    PIN = m.Pin,
                    TillCashier = device.DeviceCashiers.Any(u => u.User.Id == m.User.Id),
                    TillSupervisor = device.DeviceSupervisors.Any(u => u.User.Id == m.User.Id),
                    TillManager = device.DeviceManagers.Any(u => u.User.Id == m.User.Id),
                    TillUser = device.Users.Any(u => u.User.Id == m.User.Id)
                });


                var allPosUsers = new[] { users, cashiers, supervisors, managers }.SelectMany(u => u).GroupBy(i => i.TraderId).Select(g => g.First());
                return allPosUsers.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, serialNumber);
                return new List<PosUserInfomation>();
            }

        }

        public PosDeviceForUser GetDeviceForUser(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/device: Get Device For User", request.UserId, request);


                var user = dbContext.QbicleUser.AsNoTracking().Any(u => u.Id == request.UserId);
                if (!user)
                    return null;

                //All devices, no matter which Domain or  Location they are associated with, if the user is an Admin dor that device the Device must be included in the list.
                var posDevices = dbContext.PosDevices.Where(e =>
                e.Location != null && e.Status == PosDeviceStatus.Active
                && e.Menu != null && e.Administrators.Any(u => u.User.Id == request.UserId))
                    .Select(d => new PosDeviceResult
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Location = d.Location.Name,
                        Menu = d.Menu.Name,
                        SerialNumber = d.SerialNumber,
                        Domain = d.Location.Domain.Name
                    }).ToList();

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, posDevices.ToJson(), MethodBase.GetCurrentMethod().Name);

                var posResult = new PosDeviceForUser
                {
                    Devices = posDevices
                };
                return posResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);
                return null;
            }
        }

        public IPosResult TabletForDevice(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), $"api/pos/device: Update pos device serial number by {request.SerialNumber}", request.UserId, request);

                //Does the Device, identified by the DeviceId, exist?
                var device = new PosDeviceRules(dbContext).GetById(request.DeviceId);

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), $"Select POS Device where id = {request.DeviceId}", request.UserId, device);

                if (device == null) return new IPosResult
                {
                    IsTokenValid = true,
                    Status = HttpStatusCode.NotAcceptable,
                    Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device")
                };


                //Is the user, identified by UserID, an Administrator of the device?
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), $"Is the user, identified by UserID, an Administrator of the device: {!device.Administrators.All(u => u.User.Id != request.UserId)}", request.UserId, device.Administrators);


                if (device.Administrators.All(u => u.User.Id != request.UserId))
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Status = HttpStatusCode.NotAcceptable,
                        Message = ResourcesManager._L("ERROR_USER_NOT_ADMIN_DEVICE")
                    };
                //Is the SerialNumber used in any device OTHER than the Device identified in 1.
                //If it is then that Device's serial number MUST be set to null.
                var deviceSerialNumber = new PosDeviceRules(dbContext).GetBySerialNumber(request.SerialNumber);

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), $"Select POS Device where serialNumber = {request.SerialNumber}", request.UserId, deviceSerialNumber);


                if (deviceSerialNumber != null)
                {
                    deviceSerialNumber.SerialNumber = string.Empty;
                    if (dbContext.Entry(deviceSerialNumber).State == EntityState.Detached)
                        dbContext.PosDevices.Attach(deviceSerialNumber);
                    dbContext.Entry(deviceSerialNumber).State = EntityState.Modified;
                }

                device.SerialNumber = request.SerialNumber;
                if (dbContext.Entry(device).State == EntityState.Detached)
                    dbContext.PosDevices.Attach(device);
                dbContext.Entry(device).State = EntityState.Modified;
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
                    Message = ResourcesManager._L("ERROR_DETAIL", ex.Message)
                };
            }


        }

        public PosContacts PosContact(PosTraderContact contact, PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/contact: pos contact filter", request.UserId, contact, request);


                var device = new PosDeviceRules(dbContext).GetBySerialNumber(request.SerialNumber);

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), $"api/pos/contact: filter pos device where serialNumber = {request.SerialNumber}", request.UserId, device, request.SerialNumber);


                if (device == null)
                    return new PosContacts { Contacts = new List<PosTraderContact> { new PosTraderContact { Id = -1 } } };

                var maxContactResult = new PosSettingRules(dbContext).GetByLocation(device.Location.Id, request.UserId)?.MaxContactResult ?? 0;
                //find Contacts by Domain
                var contacts = dbContext.TraderContacts.Where(e => e.ContactGroup.Domain.Id == device.Location.Domain.Id);

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), $"api/pos/contact: filter contact where domain id = {device.Location.Domain.Id}", request.UserId, contacts);


                if (!string.IsNullOrEmpty(contact.ContactRef))
                    contacts = contacts.Where(e => contact.ContactRef == e.ContactRef.Reference);

                if (contacts.Count() > 1)
                {
                    if (!string.IsNullOrEmpty(contact.Name))
                        contacts = contacts.Where(e => e.Name.Contains(contact.Name));

                    if (!string.IsNullOrEmpty(contact.Email))
                        contacts = contacts.Where(e => e.Email.Contains(contact.Email));

                    if (!string.IsNullOrEmpty(contact.Phone))
                        contacts = contacts.Where(e => e.PhoneNumber.Contains(contact.Phone));
                    if (contact.Address != null)
                    {
                        if (!string.IsNullOrEmpty(contact.Address.Street))
                        {
                            contacts = contacts.Where(e =>
                                e.Address != null && e.Address.AddressLine1.Contains(contact.Address.Street));
                            contacts = contacts.Where(e =>
                                e.Address != null && e.Address.AddressLine2.Contains(contact.Address.Street));
                        }

                        if (!string.IsNullOrEmpty(contact.Address.City))
                            contacts = contacts.Where(e => e.Address != null && e.Address.City == contact.Address.City);

                        if (!string.IsNullOrEmpty(contact.Address.Country))
                            contacts = contacts.Where(e =>
                                e.Address != null && e.Address.Country.CommonName == contact.Address.Country);

                        if (!string.IsNullOrEmpty(contact.Address.Postcode))
                            contacts = contacts.Where(e => e.Address != null && e.Address.PostCode == contact.Address.Postcode);
                    }
                }

                if (contacts.Count() > maxContactResult)
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), $"api/pos/contact: filter contacts found {contacts.Count()} > {maxContactResult}", request.UserId, contacts.Count());

                    return new PosContacts { Contacts = new List<PosTraderContact> { new PosTraderContact { Id = -2 } } };

                }
                var contactsRef = new PosContacts
                {
                    Contacts = new List<PosTraderContact>()
                };
                contacts.ToList().ForEach(c =>
                {
                    var contactRef = c.ContactRef == null ? "" : c.ContactRef?.Reference;

                    contactsRef.Contacts.Add(new PosTraderContact
                    {
                        Id = c.Id,
                        Name = c.Name,
                        AvatarUri = c.AvatarUri.ToUriString(),
                        ContactRef = contactRef,
                        Email = c.Email,
                        Phone = c.PhoneNumber,
                        Address = new PosContactAddress
                        {
                            AddressLine1 = c.Address?.AddressLine1,
                            AddressLine2 = c.Address?.AddressLine2,
                            Country = c.Address?.Country.CommonName,
                            City = c.Address?.City,
                            Postcode = c.Address?.PostCode,
                            Latitude = c.Address?.Latitude,
                            Longitude = c.Address?.Longitude
                        }
                    });
                });
                return contactsRef;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, contact, request);
                return new PosContacts { Contacts = new List<PosTraderContact> { new PosTraderContact { Id = -2 } } };
            }

        }

        private readonly object orderReferenceLock = new object();
        public IPosResult PosOrderReference(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/orderreference", request.UserId, request);


                var posDeviceLocation = dbContext.PosDevices.FirstOrDefault(e => e.SerialNumber == request.SerialNumber && !e.Archived).Location;

                if (posDeviceLocation == null)
                    return new IPosResult
                    {
                        IsTokenValid = request.IsTokenValid,
                        Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "device serial"),
                        Status = HttpStatusCode.NotAcceptable
                    };
                var user = new UserRules(dbContext).GetById(request.UserId);
                if (user == null) return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "user"),
                    Status = HttpStatusCode.NotAcceptable
                };
                var locationId = posDeviceLocation.Id;

                var posSetting = dbContext.PosSettings.AsNoTracking().FirstOrDefault(e => e.Location.Id == locationId);

                if (posSetting?.RolloverTime == null) return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Rollover Time"),
                    Status = HttpStatusCode.NotAcceptable
                };


                var orderRollover = posSetting.RolloverTime;

                var orderRef = dbContext.PosOrderReferences.OrderByDescending(r => r.CreatedDate).AsNoTracking().FirstOrDefault(e => e.Location.Id == locationId);
                long referenceNumber = 1;

                var currentDate = DateTime.UtcNow;

                lock (orderReferenceLock)
                {
                    int countLimitGoto = 0;
                    if (orderRef != null)
                    {
                        var latestRolloverTimeDate = currentDate.Date + orderRollover;
                        var latestPosOrderReference = orderRef.CreatedDate;

                        if (currentDate.Date > latestPosOrderReference.Date)
                        {
                            referenceNumber = 1;
                        }
                        else
                        {
                            if (orderRollover > new TimeSpan(12, 00, 00))
                            {
                                if (currentDate < latestRolloverTimeDate)
                                    referenceNumber = orderRef.ReferenceNumber + 1;
                            }
                            else if (orderRollover < new TimeSpan(12, 00, 00))
                            {
                                if (currentDate > latestRolloverTimeDate)
                                    referenceNumber = orderRef.ReferenceNumber + 1;
                            }
                        }
                    }

                    if (CheckOrderReferenceNumber(referenceNumber, locationId, currentDate.Date))
                    {
                        ifinity_Loop:
                        referenceNumber += 1;
                        countLimitGoto += 1;
                        if (CheckOrderReferenceNumber(referenceNumber, locationId, currentDate.Date) && countLimitGoto < int.MaxValue)
                        {
                            goto ifinity_Loop;
                        }
                    }

                    var orderRefNew = new PosOrderReference
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Location = posDeviceLocation,
                        ReferenceNumber = referenceNumber
                    };
                    dbContext.PosOrderReferences.Add(orderRefNew);
                    dbContext.Entry(orderRefNew).State = EntityState.Added;
                    dbContext.SaveChanges();
                }

                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Message = referenceNumber.ToString(),
                    Status = HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, request);
                return new IPosResult
                {
                    IsTokenValid = request.IsTokenValid,
                    Message = ResourcesManager._L("ERROR_DETAIL", ex.Message),
                    Status = HttpStatusCode.BadRequest
                };
            }
        }



        private bool CheckOrderReferenceNumber(long referenceNumber, int locationId, DateTime orderDate)
        {
            return dbContext.PosOrderReferences.OrderByDescending(r => r.CreatedDate).Any(q => q.Location.Id == locationId && q.ReferenceNumber >= referenceNumber
                                                                                    && q.CreatedDate.Year == orderDate.Year
                                                                                    && q.CreatedDate.Month == orderDate.Month
                                                                                    && q.CreatedDate.Day == orderDate.Day
                                                                                    );
        }

        public IPosResult PosOrder(Order saleOrder, PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/order", request.UserId, null, saleOrder, request);

                //check existed Trade Order
                var tradeOrderDb = dbContext.TradeOrders.Any(e => e.LinkedOrderId == saleOrder.LinkedTraderId);
                if (tradeOrderDb)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_POS_ORDER_DULICATE", saleOrder.Reference),
                        Status = HttpStatusCode.NotAcceptable
                    };

                #region Check And Prepare information for processing
                //Get the cashier on the POS
                var cashier = new UserRules(dbContext).GetById(saleOrder.Cashier.TraderId);
                if (cashier == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "user"),
                        Status = HttpStatusCode.NotAcceptable
                    };

                if (saleOrder.VoucherId > 0)
                {
                    var verifyVoucher = new TraderApi.TraderContactApiRules(dbContext).VerifySelectedVoucher(cashier.Timezone, saleOrder.VoucherId);
                    if (!verifyVoucher.result)
                    {
                        return new IPosResult
                        {
                            IsTokenValid = true,
                            Message = verifyVoucher.msg,
                            Status = HttpStatusCode.NotAcceptable
                        };
                    }

                    saleOrder = OrderVoucherCalculation2Web(saleOrder);
                }
                else
                    saleOrder = OrderCalculation(saleOrder);
                saleOrder.SalesChannel = SalesChannelEnum.POS;
                // Check that we can get a device based on the supplied serial number
                var device = dbContext.PosDevices.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);

                if (device == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device"),
                        Status = HttpStatusCode.NotAcceptable
                    };
                var location = device.Location;
                var menu = device.Menu;

                // Check that we can get a Till associated with the device
                var associatedTill = new CMsRules(dbContext).GetTillByPosDevice(device.Id);
                if (associatedTill == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = $"POS Device {device.Name}-{device.SerialNumber} required link to Till default.",
                        Status = HttpStatusCode.NotAcceptable
                    };

                // Check that we can get the settings associated with the location of the device
                var posSetting = dbContext.PosSettings.FirstOrDefault(e => e.Location.Id == device.Location.Id);

                if (posSetting == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = "POS required config first.",
                        Status = HttpStatusCode.NotAcceptable
                    };
                if (posSetting.DefaultWalkinCustomer == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Default Walkin customer"),
                        Status = HttpStatusCode.NotAcceptable
                    };
                var walkinCustomerId = posSetting.DefaultWalkinCustomer.Id;
                if (posSetting.DefaultWorkGroup == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Default Work Group"),
                        Status = HttpStatusCode.NotAcceptable
                    };
                var workGroup = posSetting.DefaultWorkGroup;

                // Get the delivery method
                if (!saleOrder.Delivery.TryParseEnum<DeliveryMethodEnum>())
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_DATA_NOT_ACEPTABLE", "Delivery"),
                        Status = HttpStatusCode.NotAcceptable
                    };

                var deliveryMethod = (DeliveryMethodEnum)saleOrder.Delivery;

                #endregion

                #region Get TraderContact for the TraderSale and an OrderCustomer for the QueueOrder
                var contactId = saleOrder.Customer?.TraderId ?? walkinCustomerId;

                var contact = dbContext.TraderContacts.FirstOrDefault(e => e.Id == contactId);

                if (saleOrder.Customer == null && saleOrder.VoucherId > 0)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_VOUCHER_INVALID", "device"),
                        Status = HttpStatusCode.NotAcceptable
                    };
                else if (saleOrder.Customer != null && saleOrder.VoucherId > 0)
                {
                    var voucher = dbContext.Vouchers.Where(s => !s.IsRedeemed && s.ClaimedBy.Id == contact.QbicleUser.Id && s.Id == saleOrder.VoucherId);
                    if (voucher == null)
                        return new IPosResult
                        {
                            IsTokenValid = true,
                            Message = ResourcesManager._L("ERROR_VOUCHER_INVALID", "device"),
                            Status = HttpStatusCode.NotAcceptable
                        };
                }

                var customer = new OrderCustomer();

                // Does the SaleOrder have a customer
                if (saleOrder.Customer != null)
                {
                    // Need to add a Trader Contact here if possible
                    contact = new OrderProcessingHelper(dbContext).GetCreateTraderContactFromCustomer(saleOrder.Customer, location.Domain, cashier, SalesChannelEnum.POS);

                    if (contact != null) // Get the OrderCustomer from the saleOrder customer and updated the data where necessary from the contact                       
                        customer = new OrderProcessingHelper(dbContext).MapCustomer2OrderCustomer(saleOrder.Customer, contact);
                    else
                        customer = new OrderProcessingHelper(dbContext).MapCustomer2OrderCustomer(saleOrder.Customer);
                }
                else
                    customer = new OrderProcessingHelper(dbContext).MapTraderContact2OrderCustomer(contact); //In this case the contact can only be the walkin in contact                               

                if (customer.FullAddress.Country == null)
                    customer.FullAddress = null;

                if (contact == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "customer 'Walkin customer'"),
                        Status = HttpStatusCode.NotAcceptable
                    };
                /*
                    If there is a QbicleUser (ApplicationUser), in the Qbicles System, 
                    with the same email address as the TraderContact link the QbicleUser to the new TraderContact
                */
                new TraderContactRules(dbContext).LinkUserToContact(contact);
                #endregion

                customer.IsDefaultWalkinCustomer = saleOrder.Customer == null;
                var tradeOrder = new TradeOrder
                {
                    OrderStatus = TradeOrderStatusEnum.AwaitingProcessing,
                    SalesChannel = saleOrder.SalesChannel ?? SalesChannelEnum.POS,
                    ProductMenu = menu,
                    PosDevice = device,
                    SellingDomain = location.Domain,
                    OrderCustomer = customer,
                    TraderContact = contact,
                    DeliveryMethod = deliveryMethod,
                    IsAgreedByBusiness = true,
                    IsAgreedByCustomer = true,
                    OrderJson = saleOrder.ToJson(),
                    ProvisionalOrder = saleOrder,
                    CreatedBy = cashier,
                    CreateDate = DateTime.UtcNow,
                    Location = location,
                    PrepDeliveryOrder = null,
                    ProcessedProblems = null,
                    Customer = contact?.QbicleUser,
                    BuyingDomain = null,
                    Invoice = null,
                    InvoiceWorkGroup = workGroup,
                    Sale = null,
                    SaleWorkGroup = workGroup,
                    Payments = new List<CashAccountTransaction>(),
                    PaymentWorkGroup = workGroup,
                    Transfer = null,
                    TransferWorkGroup = workGroup,
                    OrderProblem = TradeOrderProblemEnum.Non,
                    PaymentAccount = null,
                    OrderReference = new TraderReferenceRules(dbContext).GetNewReference(location.Domain.Id, TraderReferenceType.Order),
                    LinkedOrderId = saleOrder.LinkedTraderId
                };

                dbContext.Entry(tradeOrder).State = EntityState.Added;
                dbContext.TradeOrders.Add(tradeOrder);
                dbContext.SaveChanges();
                //recaculation order
                saleOrder.TradeOrderId = tradeOrder.Id;
                tradeOrder.OrderJson = saleOrder.ToJson();
                dbContext.SaveChanges();

                //call to hangfire process order
                var job = new OrderJobParameter
                {
                    Id = tradeOrder.Id,
                    EndPointName = "processorder",
                    InvoiceDetail = "",
                    Address = ""
                };
                var tskHangfire = new Task(async () =>
                {
                    await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();

                tradeOrder.OrderStatus = TradeOrderStatusEnum.InProcessing;
                dbContext.SaveChanges();



                new PosRequestRules(dbContext).HangfirePosRequestLog(request, tradeOrder.OrderJson, MethodBase.GetCurrentMethod().Name);
                return new IPosResult
                {
                    IsTokenValid = true,
                    Message = ResourcesManager._L("SUCCESSFULLY"),
                    Status = HttpStatusCode.OK,
                    TraderId = saleOrder.LinkedTraderId//tradeOrder.Id// queueOrder.Id
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, saleOrder, request);
                return new IPosResult
                {
                    IsTokenValid = false,
                    Message = ResourcesManager._L("ERROR_DETAIL", ex.Message),
                    Status = HttpStatusCode.Forbidden
                };
            }
        }

        #region Apply voucher

        /// <summary>
        /// calculation for variant/extra total amount, total discount
        /// un-used them for calculation howerver use them for display on B2C Order
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Order OrderCalculation(Order order)
        {
            try
            {
                order.Items.ForEach(oItem =>
                {
                    var quantity = oItem.Quantity;
                    oItem.Variant.TotalAmount = oItem.Variant.AmountInclTax * quantity;
                    oItem.Variant.TotalDiscount = oItem.Variant.DiscountAmount * quantity;

                    oItem.Extras.ForEach(e =>
                    {
                        e.TotalAmount = e.AmountInclTax * quantity;
                        e.TotalDiscount = e.DiscountAmount * quantity;

                    });
                });

                return order;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, order);
                return order;
            }
        }



        /// <summary>
        /// The issue is converting the JSON to a TraderSale 
        /// when an Item within the JSON could have 11 Quantity but only 10 are discounted.
        /// we were able to split the item into 2 TraderTRansactionItems
        /// 1. This has the 10 items at a discount
        /// 2. This has the 1 item that is not at a discount
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Order OrderVoucherCalculation2Web(Order order)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, order);

                var voucher = dbContext.Vouchers.FirstOrDefault(e => e.Id == order.VoucherId);
                if (voucher == null)
                    return order;

                switch (voucher.Promotion.VoucherInfo.Type)
                {
                    case VoucherType.OrderDiscount:
                        var oVoucher = voucher.Promotion.VoucherInfo as OrderDiscountVoucherInfo;

                        //%
                        decimal oMaxDiscountValue = oVoucher.MaxDiscountValue;
                        decimal oDiscountOrder = oVoucher.OrderDiscount / 100;
                        decimal subTotal = order.AmountInclTax;

                        order.Discount = 0;
                        if (order.AmountInclTax * oDiscountOrder <= oMaxDiscountValue)
                            order.Discount = order.AmountInclTax * oDiscountOrder;
                        else
                            order.Discount = oMaxDiscountValue;

                        order.AmountInclTax = 0;
                        order.AmountExclTax = 0;
                        order.AmountTax = 0;

                        decimal preItemDiscount = 0;
                        if (oMaxDiscountValue >= subTotal * oDiscountOrder)
                            preItemDiscount = oDiscountOrder;
                        else
                            preItemDiscount = oMaxDiscountValue / subTotal;

                        order.Items.ForEach(item =>
                        {
                            OrderDiscountCalculation(order, item.Variant, preItemDiscount, item.Quantity);
                            item.Extras.ForEach(extra =>
                            {
                                OrderDiscountCalculation(order, extra, preItemDiscount, item.Quantity);
                            });
                        });

                        break;
                    case VoucherType.ItemDiscount:

                        var orderItems = new List<Item>();

                        var iVoucher = voucher.Promotion.VoucherInfo as ItemDiscountVoucherInfo;

                        decimal maxNumberOfItemsPerOrder = iVoucher.MaxNumberOfItemsPerOrder;
                        decimal discountQantity = 0;
                        decimal remainQuantity = iVoucher.MaxNumberOfItemsPerOrder;
                        order.Discount = 0;
                        order.AmountInclTax = 0;
                        order.AmountExclTax = 0;

                        decimal itemRemainQuantity = 0;

                        order.Items.ForEach(item =>
                        {
                            var orderItem = new Item();

                            var variantItems = new List<Variant>();

                            itemRemainQuantity = item.Quantity;

                            if (item.Variant.SKU == iVoucher.ItemSKU)
                            {
                                if (item.Quantity <= maxNumberOfItemsPerOrder && remainQuantity > 0)
                                {
                                    if (item.Quantity > remainQuantity)
                                        discountQantity = remainQuantity;
                                    else
                                        discountQantity = item.Quantity;
                                    //if (remainQuantity > 0)
                                    //itemRemainQuantity = maxNumberOfItemsPerOrder - discountQantity;
                                }
                                else if (item.Quantity > maxNumberOfItemsPerOrder && remainQuantity > 0)
                                {
                                    discountQantity = remainQuantity;
                                    //itemRemainQuantity = item.Quantity - discountQantity;
                                }

                                var variantOrigin = item.Variant.Clone();
                                //discount
                                if (remainQuantity > 0)
                                {
                                    ItemDiscountCalculation(order, item.Variant, iVoucher.ItemDiscount, discountQantity);
                                    remainQuantity -= discountQantity;
                                    itemRemainQuantity = item.Quantity - discountQantity;
                                    variantItems.Add(item.Variant);
                                }
                                if (remainQuantity == 0 && itemRemainQuantity > 0)
                                {
                                    //without discount
                                    ItemDiscountCalculation(order, variantOrigin, 0, itemRemainQuantity);
                                    // add amount without discount
                                    item.Variant.TotalAmount += variantOrigin.AmountInclTax * itemRemainQuantity;
                                    item.Variant.TotalDiscount += variantOrigin.DiscountAmount * itemRemainQuantity;
                                    variantItems.Add(variantOrigin);
                                }
                            }
                            else
                            {
                                //without discount
                                ItemDiscountCalculation(order, item.Variant, 0, item.Quantity);
                                variantItems.Add(item.Variant);
                            }

                            var extraItems = new List<Variant>();
                            item.Extras.ForEach(extra =>
                            {

                                itemRemainQuantity = item.Quantity;

                                if (extra.SKU == iVoucher.ItemSKU)
                                {
                                    if (item.Quantity <= maxNumberOfItemsPerOrder && remainQuantity > 0)
                                    {
                                        if (item.Quantity > remainQuantity)
                                            discountQantity = remainQuantity;
                                        else
                                            discountQantity = item.Quantity;
                                        //if (remainQuantity > 0)
                                        //itemRemainQuantity = maxNumberOfItemsPerOrder - discountQantity;
                                    }
                                    else if (item.Quantity > maxNumberOfItemsPerOrder && remainQuantity > 0)
                                    {
                                        discountQantity = remainQuantity;
                                        //itemRemainQuantity = item.Quantity - discountQantity;
                                    }

                                    var extraOrigin = extra.Clone();
                                    //discount
                                    if (remainQuantity > 0)
                                    {
                                        ItemDiscountCalculation(order, extra, iVoucher.ItemDiscount, discountQantity);
                                        remainQuantity -= discountQantity;
                                        itemRemainQuantity = item.Quantity - discountQantity;
                                        extraItems.Add(extra);
                                    }
                                    if (remainQuantity == 0 && itemRemainQuantity > 0)
                                    {
                                        //without discount
                                        ItemDiscountCalculation(order, extraOrigin, 0, itemRemainQuantity);
                                        // add amount without discount
                                        extra.TotalAmount += extraOrigin.AmountInclTax * itemRemainQuantity;
                                        extra.TotalDiscount += extraOrigin.DiscountAmount * itemRemainQuantity;

                                        extraItems.Add(extraOrigin);
                                    }
                                }
                                else
                                {
                                    //without discount
                                    ItemDiscountCalculation(order, extra, 0, item.Quantity);
                                    extraItems.Add(extra);
                                }

                            });

                            var index = 0;
                            variantItems.ForEach(v =>
                            {
                                var oItem = item.Clone();
                                oItem.Extras = new List<Variant>();
                                oItem.Variant = v;
                                if (index == 0)
                                    oItem.Extras = extraItems;
                                orderItems.Add(oItem);
                                index++;
                            });

                        });

                        order.Items = orderItems;

                        break;
                }


                order.AmountTax = order.AmountInclTax - order.AmountExclTax;
                return order;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, order);
                return order;
            }
        }
        /// <summary>
        /// The issue is converting the JSON to a TraderSale 
        /// when an Item within the JSON could have 11 Quantity but only 10 are discounted.
        /// we were able to split the item into 2 TraderTRansactionItems
        /// 1. This has the 10 items at a discount
        /// 2. This has the 1 item that is not at a discount
        /// </summary>
        /// <param name="order"></param>
        /// <param name="orderOrg"></param>
        /// <returns></returns>
        public Order OrderVoucherCalculation2B2C(Order order, Order orderOrg)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, order);

                var orderItems = new List<Item>();

                decimal itemQuantity = 0;

                order.Items.ForEach(item =>
                {
                    var orderItem = new Item();

                    var variantItems = new List<Variant>();

                    itemQuantity = item.Quantity;

                    var variant = item.Variant.Clone();
                    variantItems.Add(variant);
                    if (itemQuantity > variant.Quantity)
                    {
                        var itemOrg = orderOrg.Items.FirstOrDefault(e => e.Id == item.Id && e.Variant.TraderId == variant.TraderId).Variant.Clone();
                        itemOrg.Quantity = itemQuantity - variant.Quantity;
                        //variant = itemOrg.Clone();
                        variantItems.Add(itemOrg);
                    }
                    var extras = new List<Variant>();
                    item.Extras.ForEach(extra =>
                    {
                        var iExtra = extra.Clone();
                        extras.Add(iExtra);
                        if (itemQuantity > iExtra.Quantity)
                        {
                            var extraOrg = item.Extras.FirstOrDefault(e => e.TraderId == iExtra.TraderId).Clone();
                            extraOrg.Quantity = itemQuantity - iExtra.Quantity;
                            //iExtra = extraOrg.Clone();
                            extras.Add(extraOrg);
                        }
                        //extras.Add(iExtra);
                    });


                    var index = 0;
                    variantItems.ForEach(v =>
                    {
                        var oItem = item.Clone();
                        oItem.Extras = new List<Variant>();
                        oItem.Variant = v;
                        if (index == 0)
                            oItem.Extras = extras;
                        orderItems.Add(oItem);
                        index++;
                    });

                });

                order.Items = orderItems;
                return order;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, order);
                return order;
            }
        }

        /// <summary>
        ///  Calculation response to POS
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Order OrderVoucherCalculation2Pos(Order order)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, order);

                var voucher = dbContext.Vouchers.FirstOrDefault(e => e.Id == order.VoucherId);
                if (voucher == null)
                    return order;

                order.VoucherName = voucher.Promotion.Name;

                switch (voucher.Promotion.VoucherInfo.Type)
                {
                    case VoucherType.OrderDiscount:
                        var oVoucher = voucher.Promotion.VoucherInfo as OrderDiscountVoucherInfo;

                        //%
                        decimal oMaxDiscountValue = oVoucher.MaxDiscountValue;
                        decimal oDiscountOrder = oVoucher.OrderDiscount / 100;
                        decimal subTotal = order.AmountInclTax;

                        order.Discount = 0;
                        if (order.AmountInclTax * oDiscountOrder <= oMaxDiscountValue)
                            order.Discount = order.AmountInclTax * oDiscountOrder;
                        else
                            order.Discount = oMaxDiscountValue;

                        order.AmountInclTax = 0;
                        order.AmountExclTax = 0;
                        order.AmountTax = 0;

                        decimal preItemDiscount = 0;
                        if (oMaxDiscountValue >= subTotal * oDiscountOrder)
                            preItemDiscount = oDiscountOrder;
                        else
                            preItemDiscount = oMaxDiscountValue / subTotal;

                        order.Items.ForEach(item =>
                        {
                            OrderDiscountCalculation(order, item.Variant, preItemDiscount, item.Quantity);
                            item.Extras.ForEach(extra =>
                            {
                                OrderDiscountCalculation(order, extra, preItemDiscount, item.Quantity);
                            });
                        });

                        break;
                    case VoucherType.ItemDiscount:

                        var iVoucher = voucher.Promotion.VoucherInfo as ItemDiscountVoucherInfo;

                        decimal maxNumberOfItemsPerOrder = iVoucher.MaxNumberOfItemsPerOrder;
                        decimal discountQantity = 0;
                        decimal remainQuantity = iVoucher.MaxNumberOfItemsPerOrder;
                        order.Discount = 0;
                        order.AmountInclTax = 0;
                        order.AmountExclTax = 0;
                        decimal itemRemainQuantity = 0;

                        order.Items.ForEach(item =>
                        {
                            itemRemainQuantity = item.Quantity;

                            if (item.Variant.SKU == iVoucher.ItemSKU)
                            {
                                if (item.Quantity <= maxNumberOfItemsPerOrder && remainQuantity > 0)
                                {
                                    if (item.Quantity > remainQuantity)
                                        discountQantity = remainQuantity;
                                    else
                                        discountQantity = item.Quantity;
                                    //if (remainQuantity > 0)
                                    //itemRemainQuantity = maxNumberOfItemsPerOrder - discountQantity;
                                }
                                else if (item.Quantity > maxNumberOfItemsPerOrder && remainQuantity > 0)
                                {
                                    discountQantity = remainQuantity;
                                    //itemRemainQuantity = item.Quantity - discountQantity;
                                }

                                var variantOrigin = item.Variant.Clone();
                                //discount
                                if (remainQuantity > 0)
                                {
                                    ItemDiscountCalculation(order, item.Variant, iVoucher.ItemDiscount, discountQantity);
                                    remainQuantity -= discountQantity;
                                    itemRemainQuantity = item.Quantity - discountQantity;
                                }
                                if (remainQuantity == 0 && itemRemainQuantity > 0)
                                {
                                    //without discount
                                    ItemDiscountCalculation(order, variantOrigin, 0, itemRemainQuantity);
                                    // add amount without discount
                                    item.Variant.TotalAmount += variantOrigin.AmountInclTax * itemRemainQuantity;
                                    item.Variant.TotalDiscount += variantOrigin.DiscountAmount * itemRemainQuantity;
                                }
                            }
                            else
                            {
                                //without discount
                                ItemDiscountCalculation(order, item.Variant, 0, item.Quantity);
                            }

                            item.Extras.ForEach(extra =>
                            {
                                //VoucherCalculation(order, extra, iVoucher, remainQuantity, item.Quantity);
                                itemRemainQuantity = item.Quantity;

                                if (extra.SKU == iVoucher.ItemSKU)
                                {
                                    if (item.Quantity <= maxNumberOfItemsPerOrder && remainQuantity > 0)
                                    {
                                        if (item.Quantity > remainQuantity)
                                            discountQantity = remainQuantity;
                                        else
                                            discountQantity = item.Quantity;
                                        //if (remainQuantity > 0)
                                        //itemRemainQuantity = maxNumberOfItemsPerOrder - discountQantity;
                                    }
                                    else if (item.Quantity > maxNumberOfItemsPerOrder && remainQuantity > 0)
                                    {
                                        discountQantity = remainQuantity;
                                        //itemRemainQuantity = item.Quantity - discountQantity;
                                    }

                                    var extraOrigin = extra.Clone();
                                    //discount
                                    if (remainQuantity > 0)
                                    {
                                        ItemDiscountCalculation(order, extra, iVoucher.ItemDiscount, discountQantity);
                                        remainQuantity -= discountQantity;
                                        itemRemainQuantity = item.Quantity - discountQantity;
                                    }
                                    if (remainQuantity == 0 && itemRemainQuantity > 0)
                                    {
                                        //without discount
                                        ItemDiscountCalculation(order, extraOrigin, 0, itemRemainQuantity);
                                        // add amount without discount
                                        extra.TotalAmount += extraOrigin.AmountInclTax * itemRemainQuantity;
                                        extra.TotalDiscount += extraOrigin.DiscountAmount * itemRemainQuantity;
                                    }
                                }
                                else
                                {
                                    //without discount
                                    ItemDiscountCalculation(order, extra, 0, item.Quantity);
                                }

                            });
                        });


                        break;
                }
                order.AmountTax = order.AmountInclTax - order.AmountExclTax;

                return order;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, order);
                return order;
            }
        }

        private void OrderDiscountCalculation(Order order, Variant item, decimal perItemDiscount, decimal quantity)
        {
            decimal amountInclTaxOld = item.AmountInclTax;
            decimal amountInclTaxNew = 0;


            item.Quantity = quantity;
            item.Discount = perItemDiscount * 100;
            item.DiscountAmount = perItemDiscount * item.AmountInclTax;

            item.AmountInclTax = item.AmountInclTax - item.DiscountAmount;
            amountInclTaxNew = item.AmountInclTax;

            item.Taxes.ForEach(tax =>
            {
                tax.AmountTax = tax.AmountTax * amountInclTaxNew / amountInclTaxOld;
            });

            //AmountExclTax must be calculated and included in the Response. AmountExclTax = AmountInclTax - TaxAmount
            item.AmountExclTax = item.AmountInclTax - item.Taxes.Sum(e => e.AmountTax);

            item.TotalDiscount = item.DiscountAmount * quantity;
            item.TotalAmount = item.AmountInclTax * quantity;


            ////Update order value
            order.AmountInclTax += item.TotalAmount;
            order.AmountExclTax += item.AmountExclTax * quantity;

        }

        /// <summary>
        /// In the Variant/Extra object, the values are based on ONE Variant/Extra . This value is then multiplied by the Items.Quantity
        /// </summary>
        /// <param name="order">order</param>
        /// <param name="item">variant item</param>
        /// <param name="itemDiscount">percent discount (%)
        ///  If the Variant/Extra have discount then itemDiscount = iVoucher.ItemDiscount else itemDiscount = 0
        /// </param>
        /// <param name="discountQantity">discount quantity
        ///  If the Variant/Extra have discount then discountQantity = discountQantity else discountQantity = item.Quantity
        /// </param>
        public void ItemDiscountCalculation(Order order, Variant item, decimal itemDiscount, decimal discountQantity)
        {
            decimal amountInclTaxOld = item.AmountInclTax;
            decimal amountInclTaxNew = 0;
            item.TotalDiscount = 0;
            item.TotalAmount = 0;

            if (item.AmountInclTax == 0 || amountInclTaxOld == 0)
                return;

            var itemDiscountAmount = item.AmountInclTax * itemDiscount / 100;
            var itemTotalAmount = item.AmountInclTax - itemDiscountAmount;
            item.Discount = (item.AmountInclTax - itemTotalAmount) * 100 / item.AmountInclTax;
            item.DiscountAmount = itemDiscountAmount;
            //the AmountInclTax for 1 Variant after the discount has been applied.
            item.AmountInclTax -= itemDiscountAmount;

            amountInclTaxNew = item.AmountInclTax;

            //If the AmountInclTax has been reduced because of a Discount, then the AmountTax must also have been reduced. 
            //AmountTaxNew = AmountTaxOld * AmountInclTaxNew / AmountInclTaxOld
            item.Taxes.ForEach(tax =>
            {
                tax.AmountTax = tax.AmountTax * amountInclTaxNew / amountInclTaxOld;
            });

            //AmountExclTax must be calculated and included in the Response. AmountExclTax = AmountInclTax - TaxAmount
            item.AmountExclTax = item.AmountInclTax - item.Taxes.Sum(e => e.AmountTax);
            item.Quantity = discountQantity;

            item.TotalDiscount += item.DiscountAmount * discountQantity;
            item.TotalAmount += item.AmountInclTax * discountQantity;

            //Update order value
            order.Discount += item.TotalDiscount;// item.DiscountAmount * discountQantity;
            //This is the sum of the Variant.AmountInclTax * Item.Quantity
            order.AmountInclTax += item.TotalAmount;// item.AmountInclTax * discountQantity;
            //This is the sum of the Variant.AmountExclTax * Item.Quantity
            order.AmountExclTax += item.AmountExclTax * discountQantity;
        }

        #endregion

        public async Task<IPosResult> PosOrderCancel(OrderCancelOrPrintCheckModel posOrderCancel, PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/ordercancel", request.UserId, null, posOrderCancel, request);

                var device = dbContext.PosDevices.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);
                if (device == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device"),
                        Status = HttpStatusCode.NotAcceptable
                    };

                if (posOrderCancel.TillUser == null)
                {
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("POS_ORDER_CANCEL_USER_ERROR"),
                        Status = HttpStatusCode.NotAcceptable
                    };
                }
                if (posOrderCancel.TillManager == null || string.IsNullOrEmpty(posOrderCancel.TillManager.TraderId))
                {
                    var isTillManager = device.DeviceManagers.Any(u => u.User.Id == posOrderCancel.TillManager.TraderId);
                    if (!isTillManager)
                        return new IPosResult
                        {
                            IsTokenValid = true,
                            Message = ResourcesManager._L("POS_ORDER_CANCEL_MANAGER_ERROR"),
                            Status = HttpStatusCode.NotAcceptable
                        };
                }
                if (string.IsNullOrEmpty(posOrderCancel.Description))
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("POS_ORDER_CANCEL_DESCRIPTION_ERROR"),
                        Status = HttpStatusCode.NotAcceptable
                    };
                if (posOrderCancel.Order == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_4"),
                        Status = HttpStatusCode.NotAcceptable
                    };

                var userRules = new UserRules(dbContext);
                var tillManager = userRules.GetById(posOrderCancel.TillManager.TraderId);

                var tillUser = userRules.GetById(posOrderCancel.TillUser.TraderId);
                if (tillManager == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Till Manager"),
                        Status = HttpStatusCode.NotAcceptable
                    };


                var saleOrder = OrderCalculation(posOrderCancel.Order);
                saleOrder.SalesChannel = SalesChannelEnum.POS;

                var orderCancel = new PosOrderCancel
                {
                    SalesChannel = SalesChannelEnum.POS,
                    Location = device.Location,
                    Domain = device.Location.Domain,
                    CreatedBy = tillManager,
                    TillUser = tillUser,
                    CreatedDate = DateTime.UtcNow,
                    Description = posOrderCancel.Description,
                    OrderJson = saleOrder.ToJson()
                };

                dbContext.PosOrderCancels.Add(orderCancel);

                await dbContext.SaveChangesAsync();

                saleOrder.IsCancelled = true;
                if (saleOrder.CancelledItems != null && saleOrder.CancelledItems.Any())
                {
                    saleOrder.CancelledItems.ForEach(e =>
                    {
                        e.IsCancelled = true;
                    });
                }
                //update orders on the queue
                if (!string.IsNullOrEmpty(saleOrder.LinkedTraderId))
                {
                    var queueOrders = dbContext.QueueOrders.Where(e => e.LinkedOrderId == saleOrder.LinkedTraderId).ToList();
                    if (queueOrders.Any())
                    {
                        queueOrders.ForEach(queueOrder =>
                        {
                            queueOrder.OrderItems.ForEach(e =>
                            {
                                e.IsCancelled = true;
                            });
                            dbContext.SaveChangesAsync();
                            //push notification
                            queueOrder.PushCancelOrderNotification(MessageType.ChangeOrderStatus, "Order cancelled");
                        });
                    }
                }
                //Create new Cancel order and push notification
                CreateCancelOrder(device, saleOrder, tillManager);


                return new IPosResult
                {
                    IsTokenValid = true,
                    Message = ResourcesManager._L("SUCCESSFULLY"),
                    Status = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, posOrderCancel, request);
                return new IPosResult
                {
                    IsTokenValid = false,
                    Message = ResourcesManager._L("ERROR_DETAIL", ex.Message),
                    Status = HttpStatusCode.Forbidden
                };
            }
        }

        private void CreateCancelOrder(PosDevice device, Order saleOrder, ApplicationUser tillManager)
        {
            var posSetting = dbContext.PosSettings.FirstOrDefault(e => e.Location.Id == device.Location.Id);

            var walkinCustomerId = posSetting.DefaultWalkinCustomer.Id;
            var contact = dbContext.TraderContacts.FirstOrDefault(e => e.Id == walkinCustomerId);
            var customer = new OrderCustomer();
            var contactId = walkinCustomerId;
            // Does the SaleOrder have a customer
            if (saleOrder.Customer != null && !string.IsNullOrEmpty(saleOrder.Customer.Email))
            {
                // Need to add a Trader Contact here if possible
                contact = new OrderProcessingHelper(dbContext).GetCreateTraderContactFromCustomer(saleOrder.Customer, device.Location.Domain, tillManager, SalesChannelEnum.POS);

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
                 If there is a QbicleUser (ApplicationUser), in the Qbicles System, with the same email address as the TraderContact link the QbicleUser to the new TraderContact
            */
            new TraderContactRules(dbContext).LinkUserToContact(contact);

            var queueOrder = new OrderProcessingHelper(dbContext).CreateQueueOrder(saleOrder, customer, tillManager, PrepQueueStatus.Completing, false, null, device.PreparationQueue, saleOrder.LinkedTraderId);

            queueOrder.PushCancelOrderNotification(MessageType.NewOrder, "New order on the queue");
        }

        public bool CheckSeialLinkToDevice(string serial)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/device/check: check the serial number has linked to a device");


            return dbContext.PosDevices.AsNoTracking().Any(e => e.SerialNumber.ToLower() == serial.ToLower());
        }

        public void OrderComment(string userId, OrderMessageModel orderMessage)
        {
            new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.DriverSendMessage, orderMessage.OrderId, userId, null, orderMessage.Message);
        }

        public async Task<IPosResult> PosOrderPrintCheck(OrderCancelOrPrintCheckModel posOrderPrintCheck, PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/order/printcheck", request.UserId, null, posOrderPrintCheck, request);


                if (posOrderPrintCheck.Order == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_4"),
                        Status = HttpStatusCode.NotAcceptable
                    };

                if (posOrderPrintCheck.TillUser == null)
                {
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("POS_ORDER_CANCEL_USER_ERROR"),
                        Status = HttpStatusCode.NotAcceptable
                    };
                }

                var device = dbContext.PosDevices.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);
                if (device == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device"),
                        Status = HttpStatusCode.NotAcceptable
                    };

                if (posOrderPrintCheck.TillManager == null || string.IsNullOrEmpty(posOrderPrintCheck.TillManager.TraderId))
                {
                    var isTillManager = device.DeviceManagers.Any(u => u.User.Id == posOrderPrintCheck.TillManager.TraderId);
                    if (!isTillManager)
                        return new IPosResult
                        {
                            IsTokenValid = true,
                            Message = ResourcesManager._L("POS_ORDER_CANCEL_MANAGER_ERROR"),
                            Status = HttpStatusCode.NotAcceptable
                        };
                }

                var userRules = new UserRules(dbContext);
                var tillManager = userRules.GetById(posOrderPrintCheck.TillManager.TraderId);

                if (tillManager == null)
                    return new IPosResult
                    {
                        IsTokenValid = true,
                        Message = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Till Manager"),
                        Status = HttpStatusCode.NotAcceptable
                    };

                var tillUser = userRules.GetById(posOrderPrintCheck.TillUser.TraderId);

                var saleOrder = OrderCalculation(posOrderPrintCheck.Order);
                saleOrder.SalesChannel = SalesChannelEnum.POS;

                var orderPrintCheck = new PosOrderPrintCheck
                {
                    SalesChannel = SalesChannelEnum.POS,
                    Location = device.Location,
                    Domain = device.Location.Domain,
                    CreatedBy = tillManager,
                    TillUser = tillUser,
                    CreatedDate = DateTime.UtcNow,
                    OrderJson = saleOrder.ToJson(),
                    POS = device
                };

                dbContext.PosOrderPrintChecks.Add(orderPrintCheck);
                await dbContext.SaveChangesAsync();

                return new IPosResult
                {
                    IsTokenValid = true,
                    Message = ResourcesManager._L("SUCCESSFULLY"),
                    Status = HttpStatusCode.OK
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, request.UserId, posOrderPrintCheck, request);
                return new IPosResult
                {
                    IsTokenValid = false,
                    Message = ResourcesManager._L("ERROR_DETAIL", ex.Message),
                    Status = HttpStatusCode.Forbidden
                };
            }
        }
    }
}
