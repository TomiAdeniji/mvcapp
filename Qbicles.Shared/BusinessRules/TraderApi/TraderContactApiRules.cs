using Qbicles.BusinessRules.BusinessRules.Loyalty;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Qbicles;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Qbicles.BusinessRules.TraderApi
{
    public class TraderContactApiRules
    {
        ApplicationDbContext dbContext;

        public TraderContactApiRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public ReturnJsonModel ContactCreate(Models.TraderApi.Customer customer, PosRequest request)
        {
            var user = new UserRules(dbContext).GetById(request.UserId);

            if (string.IsNullOrEmpty(customer.Email) || string.IsNullOrEmpty(customer.Name) || string.IsNullOrEmpty(customer.Phone))
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("WARNING_MSG_CUSTOMER_MISSING_INFO"),
                    Object = HttpStatusCode.NotAcceptable.GetDescription()
                };

            if (user == null)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "user"),
                    Object = HttpStatusCode.NotAcceptable
                };

            var device = new PosDeviceRules(dbContext).GetBySerialNumber(request.SerialNumber);
            if (device == null)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device"),
                    Object = HttpStatusCode.NotAcceptable
                };
            if (device.PreparationQueue == null)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Queue"),
                    Object = HttpStatusCode.NotAcceptable
                };


            // Need to add a Trader Contact here if possible
            var contact = new OrderProcessingHelper(dbContext).GetCreateTraderContactFromCustomer(customer, device.Location.Domain, user, SalesChannelEnum.POS, true);

            /*
                If there is a QbicleUser (ApplicationUser), in the Qbicles System, 
                with the same email address as the TraderContact link the QbicleUser to the new TraderContact
             */
            var contactUser = new TraderContactRules(dbContext).LinkUserToContact(contact);

            if (contactUser != null)
            {
                var requestUri = new Uri($"{ConfigManager.QbiclesUrl}/qbicles/ReSendInvited?tokenToUserId={contactUser.Id}" +
                    $"&tokenToEmail={contactUser.Email}&activityId={contact.ContactGroup.Domain.Id}" +
                    $"&type={QbicleActivity.ActivityTypeEnum.Domain}" +
                    $"&sendByEmail={user.Email}");
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);
                client.GetAsync(requestUri);
            }
            return new ReturnJsonModel
            {
                result = true,
                msgId = contact.Id.ToString()
            };

        }

        public ReturnJsonModel ContactAddressCreate(TraderAddress address, CountryCode countryCode, int contactId)
        {
            address.Country = new CountriesRules().GetCountryByCode(countryCode);
            var addressSave = new TraderLocationRules(dbContext).SaveAddress(address);
            if (addressSave == null)
                return new ReturnJsonModel { result = false, msg = "Address does not exist." };

            var contact = dbContext.TraderContacts.FirstOrDefault(e => e.Id == contactId);
            contact.Address = addressSave;
            dbContext.SaveChanges();
            var contactUser = contact.QbicleUser;

            if (contactUser == null)
                contactUser = new TraderContactRules(dbContext).LinkUserToContact(contact);
            if (contactUser != null)
                return new MyDesksRules(dbContext).AddUserAddress(contactUser.Id, addressSave);
            return new ReturnJsonModel { result = true };
        }



        public ReturnJsonModel ContactPINVerify(PinVerify verify, PosRequest request)
        {
            var contact = dbContext.TraderContacts.FirstOrDefault(c => c.Id == verify.Id);
            if (contact == null)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_815", "Contact"),
                    Object = HttpStatusCode.NotFound
                };
            var device = new PosDeviceRules(dbContext).GetBySerialNumber(request.SerialNumber);
            if (device == null)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device"),
                    Object = HttpStatusCode.NotAcceptable
                };
            if (device.PreparationQueue == null)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Queue"),
                    Object = HttpStatusCode.NotAcceptable
                };


            //valid PIN
            if (contact.QbicleUser.StoreCreditPINs.FirstOrDefault(p => !p.IsArchieved)?.PIN != verify.PIN)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_CONTACT_PIN_INVALID"),
                    Object = HttpStatusCode.NotAcceptable
                };

            var generated = new StoreCreditRules(dbContext).GenerateStoreCreditPIN(contact.QbicleUser.Id, StoreCreditTransactionReason.UsedInPayment, contact.Key);

            if (!generated.result)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_5"),
                    Object = HttpStatusCode.InternalServerError
                };

            return new ReturnJsonModel
            {
                result = true
            };
        }

        public List<Models.TraderApi.Customer> ContactFilter(Models.TraderApi.Customer filter, PosRequest request, int pageIndex, int pageSize, ref int totalRecord, ref int totalCustomer)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, filter);

                var contacts = new List<Models.TraderApi.Customer>();
                var device = new PosDeviceRules(dbContext).GetBySerialNumber(request.SerialNumber);



                var defaultWalkinCustomer = new PosSettingRules(dbContext).GetByLocation(device.Location.Id, request.UserId).DefaultWalkinCustomer?.Id;

                var query = dbContext.TraderContacts.Where(d => d.ContactGroup.Domain.Id == device.Location.Domain.Id);






                if (!string.IsNullOrEmpty(filter.Name.Trim()))
                {
                    query = query.Where(d => (d.Name.Trim().ToLower().Contains(filter.Name.Trim().ToLower())));
                }
                if (!string.IsNullOrEmpty(filter.Email.Trim()))
                {
                    query = query.Where(d => (d.Email.Trim().ToLower().Contains(filter.Email.Trim().ToLower())));
                }
                if (!string.IsNullOrEmpty(filter.Phone.Trim()))
                {
                    query = query.Where(d => (d.PhoneNumber.Trim().ToLower().Contains(filter.Phone.Trim().ToLower())));
                }
                if (!string.IsNullOrEmpty(filter.ContactRef.Trim()))
                {
                    query = query.Where(d => (d.ContactRef.Reference != null));
                    query = query.Where(d => (d.ContactRef.Reference.Trim().ToLower().Contains(filter.ContactRef.Trim().ToLower())));
                }

                query = query.OrderBy(d => d.Name);
                totalCustomer = query.Count();
                totalRecord = totalCustomer / pageSize;
                List<TraderContact> lstTraderContact = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();

                foreach (var contact in lstTraderContact)
                {

                    var storeCreditAccount = new StoreCreditRules(dbContext).GetOrCreateStoreCreditAccount(contact);

                    var addresses = new List<Address>();
                    if (contact.QbicleUser != null)
                    {
                        addresses = contact.QbicleUser?.TraderAddresses.Select(c => new Address
                        {
                            AddressLine1 = c.AddressLine1,
                            AddressLine2 = c.AddressLine2,
                            City = c.City,
                            Country = c.Country?.CommonName,
                            Latitude = c.Latitude,
                            Longitude = c.Longitude,
                            Postcode = c.PostCode,
                            IsDefault = c.IsDefault
                        }).ToList();
                    }
                    else if (contact.Address != null)
                        addresses = new List<Address> { new Address
                        {
                            AddressLine1 = contact.Address.AddressLine1,
                            AddressLine2 = contact.Address.AddressLine2,
                            City = contact.Address.City,
                            Country = contact.Address.Country?.CommonName,
                            Latitude = contact.Address.Latitude,
                            Longitude = contact.Address.Longitude,
                            Postcode = contact.Address.PostCode,
                            IsDefault = contact.Address.IsDefault
                        } };

                    var model = new Models.TraderApi.Customer
                    {
                        TraderId = contact.Id,
                        Name = contact.Name,
                        Avatar = contact.AvatarUri == "" ? ConfigManager.DefaultUserUrlGuid.ToDocumentUri() : contact.AvatarUri.ToDocumentUri(),
                        ContactRef = contact.ContactRef?.Reference,
                        Phone = contact.PhoneNumber,
                        Email = contact.Email,
                        Addresses = addresses,
                        IsWalkinCustomer = contact.Id == defaultWalkinCustomer,
                        //PIN = contact.QbicleUser?.StoreCreditPINs.FirstOrDefault(p => !p.IsArchieved)?.PIN,
                        StoreCredit = storeCreditAccount?.CurrentBalance ?? 0
                    };

                    contacts.Add(model);
                }
                return contacts;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, filter);
                return new List<Models.TraderApi.Customer>();
            }
        }


        public ReturnJsonModel ContactGetVoucher(int id, PosRequest request)
        {
            var contact = dbContext.TraderContacts.FirstOrDefault(c => c.Id == id);
            if (contact == null)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_815", "Contact"),
                    Object = HttpStatusCode.NotFound
                };

            var device = new PosDeviceRules(dbContext).GetBySerialNumber(request.SerialNumber);
            if (device == null)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "device"),
                    Object = HttpStatusCode.NotAcceptable
                };
            if (device.PreparationQueue == null)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Queue"),
                    Object = HttpStatusCode.NotAcceptable
                };


            var contactUserId = contact.QbicleUser?.Id ?? "";

            var voucheres = GetContactVouchers(request.UserId, contactUserId, device.Location.Domain.Id, device.Location.Id);

            //var user = dbContext.QbicleUser.FirstOrDefault(u => u.Id == request.UserId);

            //var timeZone = user.Timezone;
            //var dateFormat = $"{user.DateFormat} {user.TimeFormat}";

            //var currentDateTime = DateTime.UtcNow;

            //var queryVouchers = dbContext.Vouchers.Where(s => !s.IsRedeemed
            //            && s.ClaimedBy.Id == contactUserId
            //            && s.Promotion.Domain.Id == device.Location.Domain.Id
            //            && !s.Promotion.IsArchived && !s.IsRedeemed
            //            && s.Promotion.VoucherInfo.Locations.Select(l => l.Id).Contains(device.Location.Id)
            //            && s.Promotion.StartDate <= currentDateTime
            //            && s.VoucherExpiryDate >= currentDateTime
            //            ).OrderByDescending(s => s.CreatedDate).ToList();

            //var voucheres = new List<ContactVoucher>();
            //queryVouchers.ForEach(v =>
            //{
            //    if (!VerifySelectedVoucher(timeZone, 0, v).result) return;
            //    var voucher = new ContactVoucher
            //    {
            //        Id = v.Id,
            //        Code = v.Code,
            //        Name = v.Promotion.Name,
            //        Description = v.Promotion.Description,
            //        FeaturedImageUri = v.Promotion.FeaturedImageUri.ToUri(),
            //        DaysAllowed = v.Promotion.VoucherInfo.DaysAllowed.Select(d => new Models.MicroQbicleStream.BaseModel { Id = d.Id, Name = d.Day }).ToList(),
            //        StartDate = v.Promotion.StartDate.ConvertTimeFromUtc(timeZone),
            //        EndDate = v.VoucherExpiryDate?.ConvertTimeFromUtc(timeZone) ?? v.Promotion.EndDate.ConvertTimeFromUtc(timeZone),
            //        EndDateString = (v.VoucherExpiryDate?.ConvertTimeFromUtc(timeZone) ?? v.Promotion.EndDate.ConvertTimeFromUtc(timeZone)).ToString(dateFormat),
            //        StartDateString = v.Promotion.StartDate.ConvertTimeFromUtc(timeZone).ToString(dateFormat),
            //        Type = v.Promotion.VoucherInfo.Type.GetId(),
            //        TypeName = v.Promotion.VoucherInfo.Type.GetDescription(),
            //        TermsAndConditions = v.Promotion.VoucherInfo.TermsAndConditions,
            //        EndTime = v.Promotion.VoucherInfo.EndTime,
            //        StartTime = v.Promotion.VoucherInfo.StartTime,
            //    };
            //    switch (v.Promotion.VoucherInfo.Type)
            //    {
            //        case VoucherType.ItemDiscount:
            //            var itemDiscount = v.Promotion.VoucherInfo as ItemDiscountVoucherInfo;
            //            voucher.ItemSKU = itemDiscount.ItemSKU;
            //            voucher.ItemDiscount = itemDiscount.ItemDiscount;
            //            voucher.MaxNumberOfItemsPerOrder = itemDiscount.MaxNumberOfItemsPerOrder;
            //            break;
            //        case VoucherType.OrderDiscount:
            //            var orderDiscount = v.Promotion.VoucherInfo as OrderDiscountVoucherInfo;
            //            voucher.OrderDiscount = orderDiscount.OrderDiscount;
            //            voucher.MaxDiscountValue = orderDiscount.MaxDiscountValue;
            //            break;
            //        default:
            //            break;
            //    }
            //    voucheres.Add(voucher);
            //});


            return new ReturnJsonModel
            {
                result = true,
                Object = voucheres
            };
        }

        /// <summary>
        /// GetContactVouchers
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="customerId"></param>
        /// <param name="domainId"></param>
        /// <param name="locationId">>0 - call from POS, = 0 - call from B2C</param>
        /// <returns></returns>
        public List<ContactVoucher> GetContactVouchers(string currentUserId, string customerId, int domainId, int locationId = 0)
        {
            var user = dbContext.QbicleUser.FirstOrDefault(u => u.Id == currentUserId);

            var timeZone = user.Timezone;
            var dateFormat = $"{user.DateFormat} {user.TimeFormat}";
            var currentDateTime = DateTime.UtcNow;

            //var queryVouchers = dbContext.Vouchers.Where(s => !s.IsRedeemed
            //            && s.ClaimedBy.Id == customerId
            //            && s.Promotion.Domain.Id == domainId
            //            && !s.Promotion.IsArchived && !s.IsRedeemed
            //            && s.Promotion.VoucherInfo.Locations.Select(l => l.Id).Contains(locationId)
            //            && s.Promotion.StartDate <= currentDateTime
            //            && s.VoucherExpiryDate >= currentDateTime
            //            ).OrderByDescending(s => s.CreatedDate).ToList();


            var queryVouchers = dbContext.Vouchers.Where(s => !s.IsRedeemed
                        && s.ClaimedBy.Id == customerId
                        && s.Promotion.Domain.Id == domainId
                        && !s.Promotion.IsArchived && !s.IsRedeemed
                        && s.Promotion.StartDate <= currentDateTime
                        && s.VoucherExpiryDate >= currentDateTime
                        );
            if (locationId > 0)
                queryVouchers = queryVouchers.Where(s => s.Promotion.VoucherInfo.Locations.Select(l => l.Id).Contains(locationId));

            var vouchersDb = queryVouchers.OrderByDescending(s => s.CreatedDate).ToList();

            var vouchers = new List<ContactVoucher>();
            vouchersDb.ForEach(v =>
            {
                if (!VerifySelectedVoucher(timeZone, 0, v).result) return;
                var voucher = new ContactVoucher
                {
                    Id = v.Id,
                    Code = v.Code,
                    Name = v.Promotion.Name,
                    Description = v.Promotion.Description,
                    FeaturedImageUri = v.Promotion.FeaturedImageUri.ToUri(),
                    DaysAllowed = v.Promotion.VoucherInfo.DaysAllowed.Select(d => new Models.MicroQbicleStream.BaseModel { Id = d.Id, Name = d.Day }).ToList(),
                    StartDate = v.Promotion.StartDate.ConvertTimeFromUtc(timeZone),
                    EndDate = v.VoucherExpiryDate?.ConvertTimeFromUtc(timeZone) ?? v.Promotion.EndDate.ConvertTimeFromUtc(timeZone),
                    EndDateString = (v.VoucherExpiryDate?.ConvertTimeFromUtc(timeZone) ?? v.Promotion.EndDate.ConvertTimeFromUtc(timeZone)).ToString(dateFormat),
                    StartDateString = v.Promotion.StartDate.ConvertTimeFromUtc(timeZone).ToString(dateFormat),
                    Type = v.Promotion.VoucherInfo.Type.GetId(),
                    TypeName = v.Promotion.VoucherInfo.Type.GetDescription(),
                    TermsAndConditions = v.Promotion.VoucherInfo.TermsAndConditions,
                    EndTime = v.Promotion.VoucherInfo.EndTime,
                    StartTime = v.Promotion.VoucherInfo.StartTime,
                };
                switch (v.Promotion.VoucherInfo.Type)
                {
                    case VoucherType.ItemDiscount:
                        var itemDiscount = v.Promotion.VoucherInfo as ItemDiscountVoucherInfo;
                        voucher.ItemSKU = itemDiscount.ItemSKU;
                        voucher.ItemDiscount = itemDiscount.ItemDiscount;
                        voucher.MaxNumberOfItemsPerOrder = itemDiscount.MaxNumberOfItemsPerOrder;
                        break;
                    case VoucherType.OrderDiscount:
                        var orderDiscount = v.Promotion.VoucherInfo as OrderDiscountVoucherInfo;
                        voucher.OrderDiscount = orderDiscount.OrderDiscount;
                        voucher.MaxDiscountValue = orderDiscount.MaxDiscountValue;
                        break;
                }
                vouchers.Add(voucher);
            });

            return vouchers;
        }


        public ReturnJsonModel VerifySelectedVoucher(string timeZone, int voucherId = 0, Voucher voucher = null)
        {
            if (voucherId > 0)
                voucher = dbContext.Vouchers.FirstOrDefault(v => v.Id == voucherId);

            if (voucher == null)
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_CUSTOMER_VOUCHER_INVALID"),
                    Object = HttpStatusCode.NotAcceptable
                };


            var msg = "";
            var daysAllowed = voucher.Promotion.VoucherInfo.DaysAllowed.Select(d => new Models.MicroQbicleStream.BaseModel { Id = d.Id, Name = d.Day }).ToList();

            var currentDateTime = DateTime.UtcNow;

            var vStartDate = voucher.Promotion.StartDate;
            var vEndDate = voucher.VoucherExpiryDate ?? voucher.Promotion.EndDate;

            if (currentDateTime < vStartDate || currentDateTime > vEndDate)
            {
                var sDate = vStartDate.ConvertTimeFromUtc(timeZone).ToString("dd/MM/yyyy");
                var eDate = vEndDate.ConvertTimeFromUtc(timeZone).ToString("dd/MM/yyyy");
                msg = ResourcesManager._L("ERROR_VERIFY_VOUCHER_DATE_TIME", new string[] { sDate, eDate });
            }
            else
            {
                if (daysAllowed.Count > 0 && !daysAllowed.Select(d => d.Name).Contains(currentDateTime.DayOfWeek.ToString()))
                    msg = ResourcesManager._L("ERROR_VERIFY_VOUCHER_DAY", string.Join(", ", daysAllowed.Select(d => d.Name)));

                if (voucher.Promotion.VoucherInfo.StartTime != TimeSpan.Zero && voucher.Promotion.VoucherInfo.EndTime != TimeSpan.Zero)
                {
                    var currentTime = currentDateTime.TimeOfDay;
                    if (currentTime < voucher.Promotion.VoucherInfo.StartTime || currentTime > voucher.Promotion.VoucherInfo.EndTime)
                        msg = ResourcesManager._L("ERROR_VERIFY_VOUCHER_DATE_TIME", new string[] { voucher.Promotion.VoucherInfo.StartTime.ToString(), voucher.Promotion.VoucherInfo.EndTime.ToString() });
                }
            }

            return new ReturnJsonModel
            {
                result = msg == "",
                msg = msg
            };
        }
    }
}
