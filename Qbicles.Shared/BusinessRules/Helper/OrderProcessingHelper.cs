using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;

namespace Qbicles.BusinessRules.Helper
{
    public class OrderProcessingHelper
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderProcessingHelper(ApplicationDbContext context)
        {
            _dbContext = context;
        }


        public TraderContact GetCreateTraderContactFromCustomer(Models.TraderApi.Customer customer, QbicleDomain domain,
            ApplicationUser user, SalesChannelEnum salesChannel, bool alwaysCreate = false)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, user, domain);


                TraderContact contactModel = null;

                // Use the Customer Trader Id to get the TraderContact
                if (customer.TraderId != 0)
                    contactModel = _dbContext.TraderContacts.FirstOrDefault(e => e.Id == customer.TraderId && e.ContactGroup.Domain.Id == domain.Id);


                var qbicleUser = new UserRules(_dbContext).GetUserByEmail(customer.Email);
                // Use the Customer Trader email to get the TraderContact
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    contactModel = _dbContext.TraderContacts.FirstOrDefault(e => e.Email.ToLower() == customer.Email.ToLower()
                        && e.ContactGroup.Domain.Id == domain.Id);

                    if (contactModel != null)
                    {

                        if (contactModel.QbicleUser == null)
                        {
                            contactModel.QbicleUser = qbicleUser;
                            _dbContext.SaveChanges();
                        }

                        return contactModel;

                    }
                }
                // Check if we have a user in the system with the same email as the customer
                if (qbicleUser != null && !alwaysCreate)
                {
                    contactModel = GetCreateTraderContactFromUserInfo(qbicleUser, domain, salesChannel);
                    if (contactModel != null)
                        return contactModel;
                }

                // Create a TraderContact
                // Create the contact  address
                var contactAddress = new TraderAddress
                {
                    AddressLine1 = customer.Address?.AddressLine1 ?? "",
                    AddressLine2 = customer.Address?.AddressLine2 ?? "",
                    City = customer.Address?.City ?? "",
                    State = customer.Address?.City ?? "",
                    Email = customer.Email ?? "",
                    Phone = customer.Phone ?? "",
                    Country = new CountriesRules().GetCountryByName(customer.Address?.Country ?? "Nigeria") ?? null
                };

                if (contactAddress.Country == null)
                    contactAddress.Country = new CountriesRules().GetCountryByName("Nigeria");
                if (customer.Address.Longitude != null)
                    contactAddress.Longitude = (decimal)customer.Address.Longitude;
                if (customer.Address.Latitude != null)
                    contactAddress.Latitude = (decimal)customer.Address.Latitude;

                contactModel = new TraderContact
                {
                    Name = customer.Name ?? "",
                    AvatarUri = ConfigManager.DefaultUserUrlGuid,
                    CompanyName = "",
                    JobTitle = "",
                    PhoneNumber = customer.Phone ?? "",
                    Email = customer.Email ?? "",
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    Address = contactAddress,
                    ContactGroup = GetTraderContactGroup(domain, GetAutomaticContactGroup(salesChannel), user),
                    Status = TraderContactStatusEnum.ContactApproved,
                    ContactRef = new TraderContactRules(_dbContext).CreateNewTraderContactRef(domain.Id),
                    QbicleUser = qbicleUser
                };

                _dbContext.TraderContacts.Add(contactModel);
                _dbContext.Entry(contactModel).State = EntityState.Added;
                _dbContext.SaveChanges();

                return contactModel;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, user, domain);
                throw ex;
            }
        }

        public TraderContact GetCreateTraderContactFromUserInfo(ApplicationUser user, QbicleDomain domain, SalesChannelEnum salesChannel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, user.Id, domain);
                //var user = _dbContext.QbicleUser.Find(userId);

                var contactModel = _dbContext.TraderContacts.FirstOrDefault(p => p.QbicleUser.Id == user.Id &&
                    p.ContactGroup != null
                    && p.ContactGroup.Domain.Id == domain.Id) ?? _dbContext.TraderContacts.FirstOrDefault(p => p.Email == user.Email &&
                    p.ContactGroup != null
                    && p.ContactGroup.Domain.Id == domain.Id);

                if (contactModel != null)
                    return contactModel;

                contactModel = new TraderContact
                {
                    Name = user.GetFullName(),
                    AvatarUri = user.ProfilePic ?? "",
                    CompanyName = user.Company ?? "",
                    JobTitle = user.JobTitle ?? "",
                    PhoneNumber = user.Tell ?? "",
                    Email = user.Email ?? "",
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow,
                    QbicleUser = user,
                    Address = user.TraderAddresses.FirstOrDefault(p => p.IsDefault),
                    Status = TraderContactStatusEnum.ContactApproved,
                    ContactRef = new TraderContactRules(_dbContext).CreateNewTraderContactRef(domain.Id),
                    //if (salesChannel == SalesChannelEnum.POS)
                    ContactGroup = GetTraderContactGroup(domain, GetAutomaticContactGroup(salesChannel), user)
                };

                _dbContext.TraderContacts.Add(contactModel);
                _dbContext.Entry(contactModel).State = EntityState.Added;
                _dbContext.SaveChanges();


                return contactModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, user.Id, domain);
                return null;
            }
        }

        public TraderContactGroup GetTraderContactGroup(QbicleDomain domain, SalesChannelContactGroup saleContactGroup,
            ApplicationUser user)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domain, saleContactGroup, user);


                var contactGroupResult = _dbContext.TraderContactGroups.FirstOrDefault(p =>
                    p.Domain.Id == domain.Id && p.saleChannelGroup == saleContactGroup);
                if (contactGroupResult == null)
                {
                    contactGroupResult = new TraderContactGroup
                    {
                        Name = domain.Name + " " + saleContactGroup.GetDescription() + " Contact Group",
                        Contacts = new List<TraderContact>(),
                        Domain = domain,
                        saleChannelGroup = saleContactGroup,
                        CreatedDate = DateTime.UtcNow,
                        Creator = user
                    };

                    _dbContext.TraderContactGroups.Add(contactGroupResult);
                    _dbContext.Entry(contactGroupResult).State = EntityState.Added;
                    _dbContext.SaveChanges();
                }

                return contactGroupResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domain, saleContactGroup, user);
                return null;
            }
        }

        public SalesChannelContactGroup GetAutomaticContactGroup(SalesChannelEnum salesChannel)
        {
            switch (salesChannel)
            {
                case SalesChannelEnum.POS:
                    return SalesChannelContactGroup.POS;
                case SalesChannelEnum.B2C:
                    return SalesChannelContactGroup.B2C;
                default:
                    return SalesChannelContactGroup.Unassigned;
            }
        }

        public OrderCustomer MapTraderContact2OrderCustomer(TraderContact contact)
        {
            return new OrderCustomer
            {
                CustomerId = contact.Id,
                Address = contact.Address?.ToAddress(),
                CustomerName = contact.Name,
                CustomerRef = contact.ContactRef?.Reference,
                Email = contact.Email,
                PhoneNumber = contact.PhoneNumber,
                FullAddress = contact.Address
            };
        }

        public OrderCustomer MapCustomer2OrderCustomer(Models.TraderApi.Customer customer, TraderContact contact = null)
        {
            return new OrderCustomer
            {
                CustomerId = contact?.Id ?? 0,
                Address = customer.Address?.ToAddress(),
                CustomerName = customer.Name,
                CustomerRef = customer.ContactRef,
                Email = contact?.Email ?? customer.Email,
                PhoneNumber = contact?.PhoneNumber ?? customer.Phone,
                FullAddress = new TraderAddress
                {
                    AddressLine1 = customer.Address?.AddressLine1 ?? contact?.Address?.AddressLine1,
                    AddressLine2 = customer.Address?.AddressLine2 ?? contact?.Address?.AddressLine2,
                    City = customer.Address?.City ?? contact?.Address?.City,
                    Country = new CountriesRules().GetCountryByName(customer.Address?.Country ??
                                                                    contact?.Address?.Country.CommonName),
                    Latitude = customer.Address?.Latitude ?? contact?.Address?.Latitude ?? 0,
                    Longitude = customer.Address?.Longitude ?? contact?.Address?.Longitude ?? 0,
                    PostCode = customer.Address?.Postcode ?? contact?.Address?.PostCode
                }
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="saleOrder"></param>
        /// <param name="customer"></param>
        /// <param name="userId"></param>
        /// <param name="prepStatus">
        ///     All QueueOrders created from the ProcessOrder for POS must have a status of
        ///     PrepQueueStatus.Completed.
        /// </param>
        /// <param name="isPaid"></param>
        /// <param name="sale">null when call from SendToPrep</param>
        /// <param name="prepQueue"></param>
        /// <returns></returns>
        public QueueOrder CreateQueueOrder(Order saleOrder, OrderCustomer customer, ApplicationUser user, PrepQueueStatus prepStatus, bool isPaid, TraderSale sale, PrepQueue prepQueue, string linkedOrderId = "")
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, saleOrder, customer, user.Id, prepStatus);

            if (!string.IsNullOrEmpty(saleOrder.LinkedTraderId))
                linkedOrderId = saleOrder.LinkedTraderId;

            var orderClassification = (OrderTypeClassification)saleOrder.Classification;
            if (orderClassification == OrderTypeClassification.Unknown)
            {
                if (sale.DeliveryMethod != DeliveryMethodEnum.None)
                {
                    orderClassification = OrderTypeClassification.HomeDelivery;
                }
            }


            var queueOrder = new QueueOrder
            {
                Classification = orderClassification,
                Type = saleOrder.Type,
                Status = prepStatus,
                Cashier = user,
                OrderItems = new List<QueueOrderItem>(),
                OrderRef = saleOrder.Reference,
                Notes = saleOrder.Notes,
                OrderTotal = saleOrder.AmountInclTax,
                AmountExclTax = saleOrder.AmountExclTax,
                AmountTax = saleOrder.AmountTax,
                PaidDate = saleOrder.Status?.PaidDateTime?.ConvertTimeToUtc(user.Timezone) ?? null,

                //QueuedDate = DateTime.UtcNow,
                //PrepStartedDate = saleOrder.Status?.StartedDateTime?.ConvertTimeToUtc(user.Timezone) ?? null,
                //PreparedDate = saleOrder.Status?.PreparedDateTime?.ConvertTimeToUtc(user.Timezone) ?? null,
                //CompletedDate = saleOrder.Status?.CompletedDateTime?.ConvertTimeToUtc(user.Timezone) ?? null,

                CreatedDate = saleOrder.Status?.CreatedDateTime?.ConvertTimeToUtc(user.Timezone) ?? DateTime.UtcNow,
                DeliveredDate = saleOrder.Status?.DeliveredDateTime?.ConvertTimeToUtc(user.Timezone) ?? null,
                LinkedOrderId = linkedOrderId,
                SalesChannel = sale?.SalesChannel ?? SalesChannelEnum.POS,
                Customer = customer,
                IsPaid = isPaid,
                Sale = sale,
                PrepQueue = prepQueue,
                SplitTimes = saleOrder.SplitTimes,
                SplitType = saleOrder.SplitType,
                Table = saleOrder.Table,
                NumberAtTable = saleOrder.NumberAtTable,
                SplitAmounts = new List<SplitAmount>(),
                IsCancelled = saleOrder.IsCancelled == true,
                CancelledOrderItems = new List<QueueOrderItem>()
            };





            QueueOrderApplyDate(queueOrder,
                prepStatus,
                DateTime.UtcNow, saleOrder.Status?.StartedDateTime?.ConvertTimeToUtc(user.Timezone),
                saleOrder.Status?.PreparedDateTime?.ConvertTimeToUtc(user.Timezone),
                saleOrder.Status?.CompletedDateTime?.ConvertTimeToUtc(user.Timezone)
            );

            //Create an OrderPayment object for each payment entry in the Order JSON and link the OrderPayment to the associated Order
            if (saleOrder.Payments != null)
                saleOrder.Payments.ForEach(oPayment =>
                {
                    var methodAccountXref = _dbContext.PosPaymentMethodAccountXrefs.Find(oPayment.Method);

                    if (methodAccountXref == null)
                        return;

                    if (oPayment.TraderId != null)
                    {
                        var orderPaymentUpdate = _dbContext.OrderPayments.Find(oPayment.TraderId);
                        if (orderPaymentUpdate == null) return;
                        orderPaymentUpdate.AmountTendered = oPayment.AmountTendered;
                        orderPaymentUpdate.Reference = oPayment.Reference;
                        orderPaymentUpdate.MethodAccountXref = methodAccountXref;
                    }
                    else
                    {
                        var orderPayment = new OrderPayment
                        {
                            AmountTendered = oPayment.AmountTendered,
                            AssociatedOrder = queueOrder,
                            MethodAccountXref = methodAccountXref,
                            Reference = oPayment.Reference
                        };
                        queueOrder.Payments.Add(orderPayment);
                    }
                });


            if (saleOrder.Items != null)
            {
                /*
                 find in the order any item  in the Items collection that has the setting IsCancelled = true
                find the LinkedOrderId for the order
                for that item find its LinkedItemId with the LinkedItemId find all items with the same LinkedItemId in orders with the LinkedOrderId
                for each item found
                add in (the item) the property IsCancelledByLaterPrep with a value of True
                for each order in which the item update has been made
                issue a push notification to update the status of the order
                 */
                var itemsIsCancelled = saleOrder.Items.Where(e => e.IsCancelled).Select(i => i.LinkedItemId).ToList();
                var queueOrderItems = _dbContext.QueueOrders
                                    .Where(e => e.LinkedOrderId == linkedOrderId).SelectMany(o => o.OrderItems)
                                    .Where(i => itemsIsCancelled.Contains(i.LinkedItemId)).ToList();
                
                queueOrderItems.ForEach(item =>
                {
                    item.IsCancelledByLaterPrep = true;
                });
                _dbContext.SaveChanges();

                foreach (var orderItem in saleOrder.Items)
                {
                    var variant = _dbContext.PosVariants.Find(orderItem.Variant.TraderId);
                    if (variant == null) continue;

                    var queueOrderItem = new QueueOrderItem
                    {
                        Extras = new List<QueueExtra>(),
                        GrossPrice = orderItem.Variant.AmountInclTax * orderItem.Quantity,
                        IsInPrep = orderItem.Prepared == true,
                        OrderTaxes = new List<OrderTax>(),
                        ParentOrder = queueOrder,
                        Quantity = orderItem.Quantity,
                        Discount = orderItem.Variant.Discount,
                        Variant = variant,
                        SplitNo = orderItem.SplitNo,
                        Note = orderItem.Note,
                        IsCancelled = orderItem.IsCancelled,
                        IsNotForPrep = orderItem.IsNotForPrep,
                        LinkedItemId = orderItem.LinkedItemId,
                    };


                    if (orderItem.Variant.Taxes != null)
                    {
                        var taxesId = orderItem.Variant.Taxes.Select(t => t.TraderId);
                        var itemTaxes = _dbContext.TraderPriceTaxes.Where(e => taxesId.Contains(e.Id)).ToList();
                        itemTaxes.ForEach(tax =>
                        {
                            var staticTaxRate = tax.TaxRate;
                            queueOrderItem.OrderTaxes.Add(new OrderTax
                            {
                                StaticTaxRate = staticTaxRate,
                                TaxRate = staticTaxRate,
                                Value = orderItem.Variant.Taxes.FirstOrDefault(e => e.TraderId == tax.Id)?.AmountTax ?? 0
                            });
                        });
                    }


                    orderItem.Extras?.ForEach(extra =>
                    {
                        var extraItem = _dbContext.PosExtras.Find(extra.TraderId);

                        var queueExtra = new QueueExtra
                        {
                            Quantity = orderItem.Quantity,
                            GrossPrice = extra.AmountInclTax * orderItem.Quantity,
                            OrderTaxes = new List<OrderTax>(),
                            Extra = extraItem,
                            ParentOrderItem = queueOrderItem,
                            SplitNo = orderItem.SplitNo,
                        };


                        if (extra.Taxes != null)
                        {
                            var taxesId = extra.Taxes.Select(t => t.TraderId);
                            var extraTaxes = _dbContext.TraderPriceTaxes.Where(e => taxesId.Contains(e.Id)).ToList();
                            extraTaxes.ForEach(tax =>
                            {
                                var staticTaxRate = tax.TaxRate;
                                queueExtra.OrderTaxes.Add(new OrderTax
                                {
                                    StaticTaxRate = staticTaxRate,
                                    TaxRate = staticTaxRate,
                                    Value = extra.Taxes.FirstOrDefault(e => e.TraderId == tax.Id)?.AmountTax ?? 0
                                });
                            });
                        }

                        queueOrderItem.Extras.Add(queueExtra);
                    });

                    queueOrder.OrderItems.Add(queueOrderItem);
                }
            }



            if (saleOrder.SplitAmounts != null)
            {
                var splitAmounts = saleOrder.SplitAmounts.Select(e => new SplitAmount { Amount = e.Amount, SplitNo = e.SplitNo, QueueOrder = queueOrder });
                queueOrder.SplitAmounts.AddRange(splitAmounts);
            }


            if (saleOrder.CancelledItems != null && saleOrder.CancelledItems.Any())
                foreach (var orderItem in saleOrder.CancelledItems)
                {
                    var variant = _dbContext.PosVariants.Find(orderItem.Variant.TraderId);
                    if (variant == null) continue;

                    var queueOrderItem = new QueueOrderItem
                    {
                        Extras = new List<QueueExtra>(),
                        GrossPrice = orderItem.Variant.AmountInclTax * orderItem.Quantity,
                        IsInPrep = orderItem.Prepared == true,
                        OrderTaxes = new List<OrderTax>(),
                        ParentOrder = queueOrder,
                        Quantity = orderItem.Quantity,
                        Discount = orderItem.Variant.Discount,
                        Variant = variant,
                        SplitNo = orderItem.SplitNo,
                        Note = orderItem.Note,
                        IsCancelled = orderItem.IsCancelled,
                        IsNotForPrep = orderItem.IsNotForPrep,
                        LinkedItemId = orderItem.LinkedItemId,
                    };


                    if (orderItem.Variant.Taxes != null)
                    {
                        var taxesId = orderItem.Variant.Taxes.Select(t => t.TraderId);
                        var itemTaxes = _dbContext.TraderPriceTaxes.Where(e => taxesId.Contains(e.Id)).ToList();
                        itemTaxes.ForEach(tax =>
                        {
                            var staticTaxRate = tax.TaxRate;
                            queueOrderItem.OrderTaxes.Add(new OrderTax
                            {
                                StaticTaxRate = staticTaxRate,
                                TaxRate = staticTaxRate,
                                Value = orderItem.Variant.Taxes.FirstOrDefault(e => e.TraderId == tax.Id)?.AmountTax ?? 0
                            });
                        });
                    }


                    orderItem.Extras?.ForEach(extra =>
                    {
                        var extraItem = _dbContext.PosExtras.Find(extra.TraderId);

                        var queueExtra = new QueueExtra
                        {
                            Quantity = orderItem.Quantity,
                            GrossPrice = extra.AmountInclTax * orderItem.Quantity,
                            OrderTaxes = new List<OrderTax>(),
                            Extra = extraItem,
                            ParentOrderItem = queueOrderItem
                        };


                        if (extra.Taxes != null)
                        {
                            var taxesId = extra.Taxes.Select(t => t.TraderId);
                            var extraTaxes = _dbContext.TraderPriceTaxes.Where(e => taxesId.Contains(e.Id)).ToList();
                            extraTaxes.ForEach(tax =>
                            {
                                var staticTaxRate = tax.TaxRate;
                                queueExtra.OrderTaxes.Add(new OrderTax
                                {
                                    StaticTaxRate = staticTaxRate,
                                    TaxRate = staticTaxRate,
                                    Value = extra.Taxes.FirstOrDefault(e => e.TraderId == tax.Id)?.AmountTax ?? 0
                                });
                            });
                        }

                        queueOrderItem.Extras.Add(queueExtra);
                    });

                    queueOrder.CancelledOrderItems.Add(queueOrderItem);
                }

            if (saleOrder.SplitAmounts != null && saleOrder.SplitAmounts.Count > 0)
            {
                var splitAmounts = saleOrder.SplitAmounts.Select(e => new SplitAmount { Amount = e.Amount, SplitNo = e.SplitNo, QueueOrder = queueOrder });
                queueOrder.SplitAmounts.AddRange(splitAmounts);
            }

            //Saving

            _dbContext.Entry(queueOrder).State = EntityState.Added;
            _dbContext.QueueOrders.Add(queueOrder);
            _dbContext.SaveChanges();

            return queueOrder;
        }

        /// <summary>
        ///     The processes must be updated to set the QueueOrder.***Dates based on the initial status
        /// </summary>
        /// <param name="queueOrder"></param>
        /// <param name="prepStatus"></param>
        /// <param name="queuedDate"></param>
        /// <param name="prepStartedDate"></param>
        /// <param name="preparedDate"></param>
        /// <param name="completedDate"></param>
        public void QueueOrderApplyDate(QueueOrder queueOrder, PrepQueueStatus prepStatus,
            DateTime? queuedDate, DateTime? prepStartedDate, DateTime? preparedDate, DateTime? completedDate)
        {
            queueOrder.QueuedDate = queuedDate ?? DateTime.UtcNow;
            switch (prepStatus)
            {
                case PrepQueueStatus.NotStarted:
                    queueOrder.PrepStartedDate = null;
                    queueOrder.PreparedDate = null;
                    queueOrder.CompletedDate = null;
                    break;
                case PrepQueueStatus.Preparing:
                    queueOrder.PrepStartedDate = prepStartedDate ?? DateTime.UtcNow;
                    queueOrder.PreparedDate = null;
                    queueOrder.CompletedDate = null;
                    break;
                case PrepQueueStatus.Completing:
                    queueOrder.PrepStartedDate = prepStartedDate ?? DateTime.UtcNow;
                    queueOrder.PreparedDate = preparedDate ?? DateTime.UtcNow;
                    queueOrder.CompletedDate = null;
                    break;
                case PrepQueueStatus.Completed:
                case PrepQueueStatus.CompletedWithProblems:
                    queueOrder.PrepStartedDate = prepStartedDate ?? DateTime.UtcNow;
                    queueOrder.PreparedDate = preparedDate ?? DateTime.UtcNow;
                    queueOrder.CompletedDate = completedDate ?? DateTime.UtcNow;
                    break;
            }
        }

        public List<Order> GetTradeOrderRef(List<QueueOrder> queueOrder)
        {
            var linkedIds = queueOrder.Select(e => e.LinkedOrderId);
            return _dbContext.TradeOrders.Where(e => linkedIds.Contains(e.LinkedOrderId)).Select(o => new Order { LinkedTraderId = o.LinkedOrderId, Reference = o.OrderReference.FullRef }).ToList();
        }
    }
}