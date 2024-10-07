using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Payments;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderContactRules
    {
        private readonly ApplicationDbContext _db;

        public TraderContactRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext dbContext => _db ?? new ApplicationDbContext();

        //contact group
        public List<TraderContactGroup> GetTraderContactsGroupByDomain(int domainId, SalesChannelContactGroup type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return dbContext.TraderContactGroups.Where(d => d.Domain.Id == domainId && d.saleChannelGroup == type).OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderContactGroup>();
            }

        }
        public List<TraderContactGroup> GetTraderContactsGroupFilter(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                var contactGroups = dbContext.TraderContactGroups.Where(q => q.Domain.Id == domainId).OrderBy(n => n.Name);
                return contactGroups.Any() ? contactGroups.ToList() : new List<TraderContactGroup>();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderContactGroup>();
            }
        }
        public TraderContactGroup GetTraderContactsGroupById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.TraderContactGroups.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderContactGroup();
            }
        }

        public List<TraderSale> GetTraderSaleByContact(int contactId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactId);
                var sales = dbContext.TraderSales.Where(q => q.Purchaser != null && q.Purchaser.Id == contactId).ToList();
                return sales;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, contactId);
                return new List<TraderSale>();
            }
        }

        public List<TraderPurchase> GetTraderPurchasesByContact(int contactId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactId);
                var purchases = dbContext.TraderPurchases.Where(q => q.Vendor != null && q.Vendor.Id == contactId).ToList();
                return purchases;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, contactId);
                return new List<TraderPurchase>();
            }
        }

        private TraderContactGroup GetTraderContactGroupById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.TraderContactGroups.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderContactGroup();
            }
        }

        private TraderContactGroup UpdateContactGroup(TraderContactGroup contactGroup)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactGroup);
                var contactG = dbContext.TraderContactGroups.Find(contactGroup.Id);
                if (contactG == null) return null;
                if (contactG.CreatedDate == DateTime.MinValue)
                {
                    contactG.CreatedDate = DateTime.UtcNow;
                    contactG.Creator = contactGroup.Creator;
                }
                contactG.Name = contactGroup.Name;
                dbContext.SaveChanges();
                return contactG;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, contactGroup);
                return new TraderContactGroup();
            }
        }

        public bool DeleteContactGroup(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                bool del = true;
                if (id > 0)
                {
                    TraderContactGroup contactGroup = dbContext.TraderContactGroups.Find(id);
                    if (contactGroup != null && !contactGroup.Contacts.Any())
                    {
                        dbContext.TraderContactGroups.Remove(contactGroup);
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        del = false;
                    }
                }
                return del;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return false;
            }
        }
        public bool TraderContactGroupNameCheck(TraderContactGroup contactGroup)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactGroup);
                if (contactGroup.Id > 0)
                {
                    return dbContext.TraderContactGroups.Any(x =>
                        x.Id != contactGroup.Id && x.Domain.Id == contactGroup.Domain.Id && x.Name == contactGroup.Name);
                }

                return dbContext.TraderContactGroups.Any(x =>
                    x.Name == contactGroup.Name && x.Domain.Id == contactGroup.Domain.Id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, contactGroup);
                return false;
            }
        }

        public bool CheckExistReferenceNumber(int id, int number, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, number, domainId);
                var refs = dbContext.TraderContactRefs.Where(q => q.Id != id && q.ReferenceNumber == number && q.Domain.Id == domainId);
                if (refs.Any())
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, number, domainId);
                return false;
            }
        }

        public TraderContactGroup SaveTraderContactGroup(TraderContactGroup group, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, group);
                if (group == null)
                {
                    return null;
                }

                if (group.Id > 0)
                {
                    return UpdateContactGroup(group);
                }
                else
                {
                    group.Creator = dbContext.QbicleUser.Find(userId);
                    group.CreatedDate = DateTime.UtcNow;
                    dbContext.TraderContactGroups.Add(group);
                    dbContext.Entry(group).State = EntityState.Added;
                    dbContext.SaveChanges();
                    return group;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, group);
                return new TraderContactGroup();
            }
        }
        public List<WorkGroup> GetContactWorkGroups(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                var contactWg = dbContext.WorkGroups.Where(q =>
                    q.Domain.Id == domainId &&
                    q.Processes.Any(p => p.Name == TraderProcessName.TraderContactProcessName));

                return contactWg.ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<WorkGroup>();
            }
        }

        //contact
        public List<TraderContact> GetTraderContactsByDomain(int domainId, SalesChannelContactGroup type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, type);
                return dbContext.TraderContacts.Where(d => d.ContactGroup.Domain.Id == domainId && d.ContactGroup.saleChannelGroup == type).OrderBy(n => n.Name)
                    .ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, type);
                return new List<TraderContact>();
            }
        }
        public List<TraderContact> GetTraderContactsByDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return dbContext.TraderContacts.Where(d => d.ContactGroup.Domain.Id == domainId).OrderBy(n => n.Name)
                    .ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderContact>();
            }
        }

        public TraderContact GetTraderContactByKey(string key)
        {
            var contactId = int.Parse(key.Decrypt());
            return dbContext.TraderContacts.Find(contactId);
        }

        public List<TraderContactModel> GetListTraderContacts(int column, string orderby, int locationId, int[] lstGroupId, int contactGroupId,
            string search, QbicleDomain domain, string currentUserId, int start, int length, ref int totalRecord)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, currentUserId, null, column, orderby, locationId,
                        lstGroupId, contactGroupId, search, domain, currentUserId, start, length, totalRecord);
                List<TraderContactModel> lstTraderContactModel = new List<TraderContactModel>();
                var query = dbContext.TraderContacts.Where(d => d.ContactGroup.Domain.Id == domain.Id);

                if (!string.IsNullOrEmpty(search.Trim()))
                {
                    query = query.Where(d => (d.Name.Trim().ToLower().Contains(search.Trim().ToLower())));
                }

                var wgIdFilter = lstGroupId.FirstOrDefault();



                if (wgIdFilter > 0)
                    query = query.Where(d => d.Workgroup.Id == wgIdFilter);
                else if (wgIdFilter < 0)
                    query = query.Where(d => d.Workgroup == null);

                if (contactGroupId > 0)
                {
                    query = query.Where(d => (d.ContactGroup.Id == contactGroupId));
                }
                #region Sorting
                if (column == 0)
                {
                    if (orderby.Equals("asc"))
                    {
                        query = query.OrderBy(d => d.ContactRef.Reference);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.ContactRef.Reference);
                    }
                }
                else if (column == 2)
                {
                    if (orderby.Equals("asc"))
                    {
                        query = query.OrderBy(d => d.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.Name);
                    }
                }
                else if (column == 3)
                {
                    if (orderby.Equals("asc"))
                    {
                        query = query.OrderBy(d => d.ContactGroup.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.ContactGroup.Name);
                    }
                }
                else if (column == 4)
                {
                    if (orderby.Equals("asc"))
                    {
                        query = query.OrderBy(d => d.Workgroup.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.Workgroup.Name);
                    }
                }
                else if (column == 8)
                {
                    if (orderby.Equals("asc"))
                    {
                        query = query.OrderBy(d => d.Status);
                    }
                    else
                    {
                        query = query.OrderByDescending(p => p.Status);
                    }
                }
                else
                {
                    query = query.OrderBy(d => d.Name);
                }
                #endregion
                totalRecord = query.Count();
                List<TraderContact> lstTraderContact = query.Skip(start).Take(length).ToList();

                foreach (var item in lstTraderContact)
                {
                    var model = new TraderContactModel
                    {
                        Action = "",
                        Id = item.Id,
                        Name = item.Name,
                        Avatar = item.AvatarUri,
                        ContactGroup = item.ContactGroup.Name,
                        Workgroup = item.Workgroup?.Name,
                        Key = HttpUtility.UrlEncode(item.Key),
                        Reference = item.ContactRef?.Reference ?? ""
                    };
                    switch (item.Status)
                    {
                        case TraderContactStatusEnum.Draft: model.Status = "Draft"; break;
                        case TraderContactStatusEnum.ContactApproved: model.Status = "ContactApproved"; break;
                        case TraderContactStatusEnum.ContactDenied: model.Status = "ContactDenied"; break;
                        case TraderContactStatusEnum.PendingApproval: model.Status = "PendingApproval"; break;
                        case TraderContactStatusEnum.PendingReview: model.Status = "PendingReview"; break;
                        case TraderContactStatusEnum.ContactDiscarded: model.Status = "ContactDiscarded"; break;
                    }

                    //If the current user is in the WorkGroup that is associated with the contact
                    if (item.Workgroup != null && item.Workgroup.Members.Any(u => u.Id == currentUserId))
                        model.Action = ContactAction(item.Status);

                    //If the current user is in the Business User role => the user is a manager
                    var lstUserRoles = dbContext.DomainRole.Where(p => p.Domain.Id == domain.Id && p.Users.Any(i => i.Id == currentUserId) && p.Name == FixedRoles.QbiclesBusinessRole);
                    if (lstUserRoles.Any())
                    {
                        model.Action = ContactAction(item.Status);
                    }

                    model.IsDisabled = !checkTraderContactCanBeDeleted(item);
                    var rules = new TraderContactRules(dbContext);
                    var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domain.Id);

                    model.AccountBalance = currencySettings.CurrencySymbol + rules.GetBalanceContact(item.Id).ToDecimalPlace(currencySettings);
                    model.BillBalance = currencySettings.CurrencySymbol + rules.GetPurchaseInvoiceBalanceByTraderContact(item.Id).ToDecimalPlace(currencySettings);
                    model.InvoiceBalance = currencySettings.CurrencySymbol + rules.GetSaleInvoiceBalanceByTraderContact(item.Id).ToDecimalPlace(currencySettings);
                    lstTraderContactModel.Add(model);
                }
                return lstTraderContactModel;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, column, orderby, locationId, lstGroupId,
                    contactGroupId, search, domain, currentUserId, start, length, totalRecord);
                return new List<TraderContactModel>();
            }
        }

        private string ContactAction(TraderContactStatusEnum status)
        {
            string action;
            if (status == TraderContactStatusEnum.Draft)
                action = "1";
            else
                action = "2";
            action += ",3";
            action.TrimStart(',');
            return action;
        }

        public TraderContact GetTraderContactById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                TraderContact contact = dbContext.TraderContacts.Find(id);
                if (contact == null)
                {
                    return null;
                }
                var address = contact.Address;
                int groupId = contact.ContactGroup.Id;
                int? accountId = contact.CustomerAccount?.Id;
                string accountName = contact.CustomerAccount?.Name;
                int? workgroupId = contact.Workgroup?.Id;

                if (contact.ContactRef == null)
                {
                    contact.ContactRef = CreateNewTraderContactRef(contact.ContactGroup.Domain.Id);
                    dbContext.SaveChanges();
                }
                                                   
                var contactRef = contact.ContactRef;
                dbContext.Entry(contact).State = EntityState.Detached;
                contact.ContactGroup = new TraderContactGroup
                {
                    Id = groupId
                };
                contact.CustomerAccount = new BKAccount
                {
                    Id = accountId ?? 0,
                    Name = accountName
                };
                contact.Workgroup = new WorkGroup
                {
                    Id = workgroupId ?? 0
                };
                
                contact.ContactRef = new TraderContactRef
                {
                    Id = contactRef.Id,
                    ReferenceNumber = contactRef.ReferenceNumber,
                    Reference = contactRef.Reference
                };
                contact.Address = address;
                return contact;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderContact();
            }
        }

        private readonly object referenceLock = new object();
        public void Update(TraderContactRef refe)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, refe);
                dbContext.Entry(refe).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, refe);
            }

        }
        public TraderContactRef SaveReference(TraderContactRef refe)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, refe);
                dbContext.Entry(refe).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, refe);
            }
            return refe;
        }
        public bool TraderContactNameCheck(TraderContact contact, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contact, domainId);
                //if(contact.Id<=0)
                return contact.Id <= 0
                    ? dbContext.TraderContacts.Any(m =>
                        m.Name == contact.Name &&
                        m.Workgroup.Domain.Id == domainId)
                    : dbContext.TraderContacts.Any(m =>
                        m.Name == contact.Name &&
                        m.Workgroup.Domain.Id == domainId && m.Id != contact.Id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, contact, domainId);
                return false;
            }

        }

        public TraderContactRef CreateNewTraderContactRef(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);

                {
                    var contactRef = new TraderContactRef
                    {
                        Domain = dbContext.Domains.Find(domainId),
                        ReferenceNumber = 0,
                        Reference = ""
                    };
                    dbContext.TraderContactRefs.Add(contactRef);
                    dbContext.SaveChanges();//save data to database and get ReferenceNumber                    
                    dbContext.Entry(contactRef).Reload();
                    return contactRef;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return null;
            }
        }

        public TraderContact SaveTraderContact(TraderContact contact, int groupId, int accountId,
            int addressId, string countryName, TraderAddress address, string userId,
            int workgroupId, TraderContactStatusEnum contactStatus, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, contact, groupId, accountId, addressId,
                        countryName, address, workgroupId, contactStatus);
                if (contact == null)
                {
                    return null;
                }

                if (!string.IsNullOrEmpty(contact.AvatarUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(contact.AvatarUri);
                }

                contact.ContactGroup = GetTraderContactGroupById(groupId);

                contact.Workgroup = new TraderWorkGroupsRules(dbContext).GetById(workgroupId);
                contact.Status = contactStatus;
                TraderAddress contactAddress = new TraderContactRules(dbContext).TraderAddressGetById(addressId);
                if (contactAddress == null)
                {
                    contactAddress = new TraderAddress
                    {
                        AddressLine1 = address.AddressLine1,
                        AddressLine2 = address.AddressLine2,
                        City = address.City,
                        PostCode = address.PostCode,
                        State = address.State,
                        Longitude = address.Longitude,
                        Latitude = address.Latitude,
                    };
                }
                else
                {
                    contactAddress.AddressLine1 = address.AddressLine1;
                    contactAddress.AddressLine2 = address.AddressLine2;
                    contactAddress.City = address.City;
                    contactAddress.PostCode = address.PostCode;
                    contactAddress.State = address.State;
                    contactAddress.Longitude = address.Longitude;
                    contactAddress.Latitude = address.Latitude;
                }

                Models.Qbicles.Country country = new CountriesRules().GetCountryByName(countryName);
                contact.Address = contactAddress;
                contact.Address.Country = country;
                contact.CustomerAccount = new BKCoANodesRule(dbContext).GetAccountById(accountId);
                var user = dbContext.QbicleUser.Find(userId);

                if (contact.Id > 0)
                {
                    TraderContact ct = GetTraderContactId(contact.Id);
                    if (!string.IsNullOrEmpty(contact.AvatarUri))
                    {
                        ct.AvatarUri = contact.AvatarUri;
                    }

                    if (ct.CreatedBy == null)
                    {
                        ct.CreatedBy = contact.CreatedBy;
                    }

                    if (ct.CreatedDate == DateTime.MinValue)
                    {
                        ct.CreatedDate = DateTime.UtcNow;
                    }
                    ct.ContactGroup = contact.ContactGroup;
                    if (contact.Workgroup != null)//Ignore if value Workgroup is Not Assigned
                        ct.Workgroup = contact.Workgroup;
                    if (contact.ContactRef == null && contact.ContactRef.Id > 0)
                        contact.ContactRef = dbContext.TraderContactRefs.Find(contact.ContactRef.Id);
                    ct.Status = contact.Status;
                    ct.Name = contact.Name;
                    ct.Address = contact.Address;
                    ct.CompanyName = contact.CompanyName;
                    ct.CustomerAccount = contact.CustomerAccount;
                    ct.Email = contact.Email;
                    ct.JobTitle = contact.JobTitle;
                    ct.PhoneNumber = contact.PhoneNumber;
                    ct.QbicleUser = new UserRules(dbContext).GetUserByEmail(contact.Email);
                    if (dbContext.Entry(ct).State == EntityState.Detached)
                    {
                        dbContext.TraderContacts.Attach(ct);
                    }

                    dbContext.Entry(ct).State = EntityState.Modified;
                }
                else
                {
                    if (contact.ContactGroup != null && contact.ContactGroup.saleChannelGroup == SalesChannelContactGroup.B2C)
                        contact.Status = TraderContactStatusEnum.ContactApproved;

                    contact.CreatedBy = user;
                    contact.ContactRef = dbContext.TraderContactRefs.Find(contact.ContactRef.Id);
                    contact.CreatedDate = DateTime.UtcNow;
                    contact.QbicleUser = new UserRules(dbContext).GetUserByEmail(contact.Email);
                    dbContext.TraderContacts.Add(contact);
                    dbContext.Entry(contact).State = EntityState.Added;
                }
                if (contact.Workgroup != null)
                    contact.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                dbContext.SaveChanges();


                TraderContact traderContact = GetById(contact.Id);
                if (traderContact == null || traderContact.Status != TraderContactStatusEnum.PendingReview
                )
                {
                    return contact;
                }

                if (traderContact.ContactApprovalProcess != null)
                {
                    return contact;
                }


                if (contact.ContactGroup != null && contact.ContactGroup.saleChannelGroup != SalesChannelContactGroup.B2C)
                {
                    var contactApprovalDef = dbContext.ContactApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == traderContact.Workgroup.Id);
                    var nameOfUserApproval = HelperClass.GetFullNameOfUser(traderContact.QbicleUser);
                    var approval = new ApprovalReq
                    {
                        ApprovalRequestDefinition = contactApprovalDef,
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                        Priority = ApprovalReq.ApprovalPriorityEnum.High,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                        Qbicle = traderContact.Workgroup.Qbicle,
                        Topic = traderContact.Workgroup.Topic,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Notes = "",
                        IsVisibleInQbicleDashboard = true,
                        App = QbicleActivity.ActivityApp.Trader,
                        Name = $"Trader Contact Approval for {(!String.IsNullOrEmpty(nameOfUserApproval) ? nameOfUserApproval : traderContact.Name)}",
                        TraderContact = new List<TraderContact> { traderContact }
                    };
                    traderContact.ContactApprovalProcess = approval;
                    approval.ActivityMembers.AddRange(traderContact.Workgroup.Members);

                    dbContext.SaveChanges();

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
                    new NotificationRules(dbContext).Notification2Activity(activityNotification);
                }

                return contact;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, contact, groupId, accountId, addressId, countryName, address,
                    workgroupId, contactStatus);
                return new TraderContact();
            }
        }

        private TraderContact GetTraderContactId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.TraderContacts.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderContact();
            }
        }

        public bool DeleteContact(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                TraderContact contact = GetTraderContactId(id);
                if (contact != null && contact.Status != TraderContactStatusEnum.ContactApproved)
                {
                    if (contact.ContactApprovalProcess != null)
                    {
                        var notifications =
                            dbContext.Notifications.Where(q => q.AssociatedAcitvity.Id == contact.ContactApprovalProcess.Id).ToList();
                        dbContext.Notifications.RemoveRange(notifications);
                        dbContext.ApprovalReqs.Remove(contact.ContactApprovalProcess);
                    }
                    if (contact.Address != null)
                    {
                        dbContext.TraderAddress.Remove(contact.Address);
                    }
                    dbContext.TraderContacts.Remove(contact);
                    dbContext.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
            }
            return false;
        }
        public BalanceAllocation GetAllocationById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.BalanceAllocations.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new BalanceAllocation();
            }
        }
        public List<ApprovalStatusTimeline> CreditNoteApprovalStatusTimeline(int id, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);
                var timeline = new List<ApprovalStatusTimeline>();
                var creditNote = dbContext.CreditNotes.Find(id);
                string status = "", icon = "";

                foreach (var log in creditNote.ApprovalProcess.ApprovalReqHistories)
                {
                    switch (log.RequestStatus)
                    {
                        case ApprovalReq.RequestStatusEnum.Pending:
                            status = "Pending Review";
                            icon = "fa fa-info bg-aqua";
                            break;
                        case ApprovalReq.RequestStatusEnum.Reviewed:
                            status = "Pending Approval";
                            icon = "fa fa-truck bg-yellow";
                            break;
                        case ApprovalReq.RequestStatusEnum.Approved:
                            status = "Approved";
                            icon = "fa fa-check bg-green";
                            break;
                        case ApprovalReq.RequestStatusEnum.Denied:
                            status = "Denied";
                            icon = "fa fa-warning bg-red";
                            break;
                        case ApprovalReq.RequestStatusEnum.Discarded:
                            status = "Discarded";
                            icon = "fa fa-trash bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timeZone).ToShortTimeString(),
                            Status = status,
                            Icon = icon
                        }
                    );
                }

                return timeline;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, timeZone);
                return new List<ApprovalStatusTimeline>();
            }
        }

        public bool DeleteAllocation(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                BalanceAllocation allocation = dbContext.BalanceAllocations.Find(id);
                if (allocation == null) return false;
                dbContext.BalanceAllocations.Remove(allocation);
                dbContext.Entry(allocation).State = EntityState.Deleted;
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return false;
            }

        }


        public List<TraderContact> GetByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                //IQueryable<TraderContact> contacts = dbContext.TraderContacts.Where(q => q.ContactGroup != null && q.ContactGroup.Domain.Id == domainId).OrderBy(n => n.Name);
                return dbContext.TraderContacts.Where(q => q.ContactGroup != null && q.ContactGroup.Domain.Id == domainId).OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderContact>();
            }
        }
        public TraderContact GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                TraderContact traderContact = dbContext.TraderContacts.Find(id);
                return traderContact;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderContact();
            }
        }
        public decimal GetBalanceContact(int contactid)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactid);
                decimal sumCreditNote, sumDebitNote, paymentsIn, paymentsOut, allocations = 0;

                //SumCreditNote calculation
                var asumCreditNote = dbContext.CreditNotes.Where(q => q.Contact.Id == contactid
                                        && (q.Reason == CreditNoteReason.CreditNote
                                        || q.Reason == CreditNoteReason.Discount
                                        || q.Reason == CreditNoteReason.PriceDecrease
                                        || q.Reason == CreditNoteReason.Voucher)
                                        && q.Status == CreditNoteStatus.Approved);
                if (asumCreditNote.Any())
                {
                    sumCreditNote = asumCreditNote.Sum(q => q.Value);
                }
                else sumCreditNote = 0;

                //SumDebitNote calculation
                var asumDebitNote = dbContext.CreditNotes.Where(q => q.Contact.Id == contactid
                                        && (q.Reason == CreditNoteReason.DebitNote
                                        || q.Reason == CreditNoteReason.PriceIncrease)
                                        && q.Status == CreditNoteStatus.Approved);
                if (asumDebitNote.Any())
                {
                    sumDebitNote = asumDebitNote.Sum(q => q.Value);
                }
                else sumDebitNote = 0;

                //Payment on Account
                var apaymentsIn = dbContext.CashAccountTransactions.Where(q => q.Contact.Id == contactid
                                                            && q.OriginatingAccount == null
                                                            && q.Status == TraderPaymentStatusEnum.PaymentApproved
                                                            && q.AssociatedInvoice == null);
                if (apaymentsIn.Any())
                {
                    paymentsIn = apaymentsIn.Sum(q => q.Amount);
                }
                else paymentsIn = 0;

                var apaymentsOut = dbContext.CashAccountTransactions.Where(q => q.Contact.Id == contactid
                                                            && q.DestinationAccount == null
                                                            && q.Status == TraderPaymentStatusEnum.PaymentApproved
                                                            && q.AssociatedInvoice == null);
                if (apaymentsOut.Any())
                {
                    paymentsOut = apaymentsOut.Sum(q => q.Amount);
                }
                else paymentsOut = 0;

                //SummAllocation calculation
                var aallocations = dbContext.BalanceAllocations.Where(q => q.Contact.Id == contactid
                                                            && q.Invoice != null);
                if (aallocations.Any())
                {
                    allocations = aallocations.Sum(q => q.Value);
                }
                else allocations = 0;

                var balance = (sumCreditNote + paymentsIn) - (sumDebitNote + paymentsOut + allocations);
                return balance;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, contactid);
                return 0;
            }
        }

        public decimal GetSaleInvoiceBalanceByTraderContact(int contactId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactId);
                decimal invoiceBalance = 0;
                decimal paymensIn = 0;
                decimal sumInvoices = 0;

                //PaymentsIn Calculation
                var lstPaymentsIn = dbContext.CashAccountTransactions.Where(p => p.Contact.Id == contactId
                                                                        && p.OriginatingAccount == null
                                                                        && p.Status == TraderPaymentStatusEnum.PaymentApproved
                                                                        && p.AssociatedInvoice != null);
                if (lstPaymentsIn.Any())
                {
                    paymensIn = lstPaymentsIn.Sum(p => p.Amount);
                }

                //SumInvoices Calculation
                var lstInvoices = dbContext.Invoices.Where(p => p.Sale.Purchaser.Id == contactId
                                                                && (p.Status == TraderInvoiceStatusEnum.InvoiceApproved || p.Status == TraderInvoiceStatusEnum.InvoiceIssued));
                if (lstInvoices.Any())
                {
                    sumInvoices = lstInvoices.Sum(p => p.TotalInvoiceAmount);
                }

                invoiceBalance = paymensIn - sumInvoices;
                return invoiceBalance;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactId);
                return 0;
            }
        }

        public decimal GetPurchaseInvoiceBalanceByTraderContact(int contactId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactId);

                decimal billBalance, sumBills = 0;
                decimal paymentsOut = 0;

                //PaymentsOut calculation
                var lstPaymentsOut = dbContext.CashAccountTransactions.Where(p => p.Contact.Id == contactId
                                                                        && p.DestinationAccount == null
                                                                        && p.Status == TraderPaymentStatusEnum.PaymentApproved
                                                                        && p.AssociatedInvoice != null);
                if (lstPaymentsOut.Any())
                {
                    paymentsOut = lstPaymentsOut.Sum(p => p.Amount);
                }


                //SumBills Calculation
                var lstBills = dbContext.Invoices.Where(p => p.Purchase.Vendor.Id == contactId
                                                    && (p.Status == TraderInvoiceStatusEnum.InvoiceApproved || p.Status == TraderInvoiceStatusEnum.InvoiceIssued));
                if (lstBills.Any())
                {
                    sumBills = lstBills.Sum(p => p.TotalInvoiceAmount);
                }

                billBalance = sumBills - paymentsOut;
                return billBalance;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactId);
                return 0;
            }
        }

        public decimal GetBalanceInvoice(int invoiceid)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, invoiceid);
                decimal payments, allocation = 0;
                var invoice = dbContext.Invoices.Find(invoiceid);
                var lstpaymentsIn = dbContext.CashAccountTransactions.Where(q => q.AssociatedInvoice.Id == invoiceid
                                                                                 && q.Status == TraderPaymentStatusEnum.PaymentApproved);
                if (lstpaymentsIn.Any())
                {
                    payments = lstpaymentsIn.Sum(q => q.Amount);
                }
                else payments = 0;
                var lstallocation = dbContext.BalanceAllocations.Where(q => q.Invoice.Id == invoiceid);
                if (lstallocation.Any())
                {
                    allocation = lstallocation.Sum(q => q.Value);
                }
                else allocation = 0;
                return invoice.TotalInvoiceAmount - (payments + allocation);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, invoiceid);
                return 0;
            }
        }
        public decimal GetAmountPaid(int invoiceid)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, invoiceid);
                decimal payments, allocation = 0;
                var lstpaymentsIn = dbContext.CashAccountTransactions.Where(q => q.AssociatedInvoice.Id == invoiceid
                                                                                 && q.Status == TraderPaymentStatusEnum.PaymentApproved);
                if (lstpaymentsIn.Any())
                {
                    payments = lstpaymentsIn.Sum(q => q.Amount);
                }
                else payments = 0;
                var lstallocation = dbContext.BalanceAllocations.Where(q => q.Invoice.Id == invoiceid);
                if (lstallocation.Any())
                {
                    allocation = lstallocation.Sum(q => q.Value);
                }
                else allocation = 0;
                return payments + allocation;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, invoiceid);
                return 0;
            }
        }
        public List<InvoiceContact> InvoicesByContact(int contactId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactId);
                var invoices = new List<InvoiceContact>();
                var saleInvoices = dbContext.TraderSales.Where(p => p.Purchaser.Id == contactId).ToList();
                var purchaseInvoice = dbContext.TraderPurchases.Where(c => c.Vendor.Id == contactId).ToList();

                foreach (var item in saleInvoices)
                {
                    if (item.Invoices != null && item.Invoices.Any())
                    {
                        invoices.AddRange(item.Invoices.Select(q => new InvoiceContact()
                        {
                            Id = q.Id,
                            Amount = q.TotalInvoiceAmount.ToString(),
                            Date = q.CreatedDate.ToString("dd.MM.yyyy"),
                            Date_Sort = q.CreatedDate.ToString("yyyy.MM.dd"),
                            Ref = q.Reference?.FullRef,
                            BalanceInvoice = GetBalanceInvoice(q.Id).ToString(),
                            AmountPaid = GetAmountPaid(q.Id).ToString(),
                            Status = "sale"
                        }).OrderBy(q => q.Id).ToList());
                    }
                }
                foreach (var item in purchaseInvoice)
                {
                    if (item.Invoices != null && item.Invoices.Any())
                    {
                        invoices.AddRange(item.Invoices.Select(q => new InvoiceContact()
                        {
                            Id = q.Id,
                            Amount = q.TotalInvoiceAmount.ToString(),
                            Date = q.CreatedDate.ToString("dd.MM.yyyy"),
                            Date_Sort = q.CreatedDate.ToString("yyyy.MM.dd"),
                            Ref = q.Reference?.FullRef,
                            BalanceInvoice = GetBalanceInvoice(q.Id).ToString(),
                            AmountPaid = GetAmountPaid(q.Id).ToString(),
                            Status = "purchase"
                        }).OrderBy(q => q.Id).ToList());
                    }
                }

                return invoices;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, contactId);
                return new List<InvoiceContact>();
            }
        }
        public ReturnJsonModel SaveAllocation(BalanceAllocation allocation, string userId)
        {
            var refModel = new ReturnJsonModel() { result = true, actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, allocation);
                if (allocation.Invoice != null && allocation.Invoice.Id > 0)
                {
                    allocation.Invoice = dbContext.Invoices.Find(allocation.Invoice.Id);
                }
                if (allocation.Reference != null)
                {
                    allocation.Reference = new TraderReferenceRules(dbContext).GetById(allocation.Reference.Id);
                }
                if (allocation.Contact != null && allocation.Contact.Id > 0)
                {
                    allocation.Contact = dbContext.TraderContacts.Find(allocation.Contact.Id);
                }

                // save update
                if (allocation.Id == 0)
                {

                    allocation.CreatedBy = dbContext.QbicleUser.Find(userId);
                    allocation.CreatedDate = DateTime.UtcNow;
                    allocation.Reference.Type = TraderReferenceType.Allocation;

                    dbContext.BalanceAllocations.Add(allocation);
                    dbContext.Entry(allocation).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
                else
                {
                    var sallocation = dbContext.BalanceAllocations.Find(allocation.Id);
                    sallocation.Invoice = allocation.Invoice;
                    sallocation.Contact = allocation.Contact;
                    sallocation.Reference = allocation.Reference;
                    sallocation.Value = allocation.Value;
                    sallocation.LastUpdateDate = DateTime.UtcNow;
                    sallocation.LastUpdatedBy = allocation.LastUpdatedBy;
                    sallocation.ContactBalanceBefore = allocation.ContactBalanceBefore;
                    sallocation.Description = allocation.Description;
                    sallocation.AllocatedDate = allocation.AllocatedDate;
                    dbContext.Entry(sallocation).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModel.actionVal = 2;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, allocation);
                refModel.result = false;
                refModel.msg = e.Message;
                refModel.actionVal = 3;
            }
            return refModel;
        }
        public ReturnJsonModel SaveCreditDebit(CreditNote creditnote, string userId, string originatingConnectionId = "")
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = true, actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, creditnote);

                var type = TraderReferenceType.CreditNote;
                if (creditnote.Reason == CreditNoteReason.DebitNote
                    || creditnote.Reason == CreditNoteReason.PriceIncrease)
                {
                    type = TraderReferenceType.DebitNote;
                }
                if (creditnote.Invoice != null && creditnote.Invoice.Id > 0)
                {
                    creditnote.Invoice = dbContext.Invoices.Find(creditnote.Invoice.Id);
                }
                if (creditnote.Reference != null)
                {
                    creditnote.Reference.Type = type;
                    creditnote.Reference = new TraderReferenceRules(dbContext).GetById(creditnote.Reference.Id);

                }
                if (creditnote.Contact != null && creditnote.Contact.Id > 0)
                {
                    creditnote.Contact = dbContext.TraderContacts.Find(creditnote.Contact.Id);
                }
                if (creditnote.WorkGroup != null && creditnote.WorkGroup.Id > 0)
                {
                    creditnote.WorkGroup = dbContext.WorkGroups.Find(creditnote.WorkGroup.Id);
                }
                if (creditnote.Sale != null && creditnote.Sale.Id > 0)
                {
                    creditnote.Sale = dbContext.TraderSales.Find(creditnote.Sale.Id);
                }
                if (creditnote.Purchase != null && creditnote.Purchase.Id > 0)
                {
                    creditnote.Purchase = dbContext.TraderPurchases.Find(creditnote.Purchase.Id);
                }


                var user = dbContext.QbicleUser.Find(userId);
                // save update
                if (creditnote.Id == 0)
                {
                    creditnote.CreatedBy = user;
                    creditnote.CreatedDate = DateTime.UtcNow;
                    dbContext.CreditNotes.Add(creditnote);
                    dbContext.Entry(creditnote).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
                else
                {
                    CreditNote screditnote = dbContext.CreditNotes.Find(creditnote.Id);
                    screditnote.Invoice = creditnote.Invoice;
                    screditnote.Contact = creditnote.Contact;
                    screditnote.Reference = creditnote.Reference;
                    screditnote.Value = creditnote.Value;
                    screditnote.LastUpdateDate = DateTime.UtcNow;
                    screditnote.LastUpdatedBy = creditnote.LastUpdatedBy;
                    screditnote.Notes = creditnote.Notes;
                    screditnote.FinalisedDate = creditnote.FinalisedDate;
                    screditnote.Sale = creditnote.Sale;
                    screditnote.Purchase = creditnote.Purchase;
                    screditnote.Status = creditnote.Status;
                    screditnote.Reason = creditnote.Reason;
                    screditnote.WorkGroup = creditnote.WorkGroup;
                    creditnote.LastUpdateDate = DateTime.UtcNow;
                    creditnote.LastUpdatedBy = user;
                    dbContext.SaveChanges();
                    refModel.actionVal = 2;
                }

                CreditNote creditnoteApproval = dbContext.CreditNotes.Find(creditnote.Id);
                if (creditnoteApproval == null || creditnoteApproval.Status != CreditNoteStatus.PendingReview)
                {
                    return refModel;
                }

                if (creditnoteApproval.ApprovalProcess != null)
                {
                    return refModel;
                }

                creditnoteApproval.WorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = dbContext.CreditNoteApprovalDefinitions.FirstOrDefault(w => w.CreditNoteWorkGroup.Id == creditnoteApproval.WorkGroup.Id),
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Qbicle = creditnoteApproval.WorkGroup.Qbicle,
                    Topic = creditnoteApproval.WorkGroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = user,
                    ApprovalReqHistories = new List<ApprovalReqHistory>(),
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader,
                    Name = $"Credit Note Request for " + creditnoteApproval.Contact.Name,
                    CreditNotes = new List<CreditNote> { creditnoteApproval }
                };
                approval.ApprovalReqHistories.Add(new ApprovalReqHistory()
                {
                    ApprovalReq = approval,
                    CreatedDate = DateTime.UtcNow,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    UpdatedBy = user
                });
                if (creditnoteApproval.Reason == CreditNoteReason.DebitNote || creditnoteApproval.Reason == CreditNoteReason.PriceIncrease)
                {
                    approval.Name = $"Debit Note Request for " + creditnoteApproval.Contact.Name;
                }
                creditnoteApproval.ApprovalProcess = approval;
                approval.ActivityMembers.AddRange(creditnoteApproval.WorkGroup.Members);

                dbContext.SaveChanges();

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
                new NotificationRules(dbContext).Notification2Activity(activityNotification);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, creditnote);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = e.Message;
            }
            return refModel;
        }


        public DataTablesResponse GetBalanceAllocations(int contactId, IDataTablesRequest requestModel, string currentTimezone,
            string dateTimeFormat, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactId, requestModel, currentTimezone, dateTimeFormat, domainId);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var query = dbContext.BalanceAllocations.Where(l => l.Contact.Id == contactId);
                int totalSpot = 0;
                #region Filter
                var keyword = requestModel.Search != null ? requestModel.Search.Value : "";
                if (!string.IsNullOrEmpty(keyword))
                {
                    //Check keyword is date and convert keyword date to DateTime.UTC
                    DateTime date = DateTime.UtcNow;
                    bool isDate = false;
                    Regex rx = new Regex("^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[0-9]|[12][0-9]|[3][01])[./-]([0-9]{4}|[0-9]{2})$");
                    var match = rx.Matches(keyword);
                    if (match.Count > 0)
                    {
                        var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimezone);
                        try
                        {
                            isDate = true;
                            date = TimeZoneInfo.ConvertTimeToUtc(keyword.ConvertDateFormat(dateTimeFormat), tz);
                        }
                        catch
                        {
                            date = DateTime.UtcNow;
                        }

                    }
                    //End
                    var _id = 0;
                    var endate = date.AddDays(1);
                    int.TryParse(keyword.TrimStart('0'), out _id);
                    query = query.Where(q =>
                        q.Id == _id
                        || (isDate && q.CreatedDate >= date && q.CreatedDate < endate)
                        || q.Description.Contains(keyword)
                        || q.Invoice.Reference.FullRef.Contains(keyword)
                    );
                }
                totalSpot = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Ref":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Id" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Value":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Value" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Description":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Description" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    Id = q.Id,
                    Ref = q.Reference != null ? q.Reference.FullRef : "",
                    Date = q.CreatedDate.ConvertTimeFromUtc(currentTimezone).ToString(dateTimeFormat),
                    InvoiceRef = (q.Invoice != null && q.Invoice.Reference != null) ? q.Invoice.Reference.FullRef : "",
                    Value = q.Value.ToDecimalPlace(currencySettings),
                    Description = q.Description
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalSpot, totalSpot);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactId, requestModel, currentTimezone, dateTimeFormat, domainId);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }
        public DataTablesResponse GetCreditNotes(int contactId, string type, IDataTablesRequest requestModel, string currentTimezone,
            string dateTimeFormat, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactId, type, requestModel,
                        currentTimezone, dateTimeFormat, domainId);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                IQueryable<CreditNote> query = null;
                if (type == "Credit")
                {
                    query = dbContext.CreditNotes.Where(q => q.Contact.Id == contactId
                                                    && (q.Reason == CreditNoteReason.CreditNote
                                                        || q.Reason == CreditNoteReason.Discount
                                                        || q.Reason == CreditNoteReason.PriceDecrease
                                                        || q.Reason == CreditNoteReason.Voucher));
                }
                else if (type == "Debit")
                {
                    query = dbContext.CreditNotes.Where(q => q.Contact.Id == contactId
                                                    && (q.Reason == CreditNoteReason.DebitNote
                                                        || q.Reason == CreditNoteReason.PriceIncrease));
                }
                else
                { query = dbContext.CreditNotes.Where(q => q.Contact.Id == contactId); }
                int totalSpot = 0;
                #region Filter
                var keyword = requestModel.Search != null ? requestModel.Search.Value : "";
                if (!string.IsNullOrEmpty(keyword))
                {
                    //Check keyword is date and convert keyword date to DateTime.UTC
                    DateTime date = DateTime.UtcNow;
                    bool isDate = false;
                    Regex rx = new Regex("^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[0-9]|[12][0-9]|[3][01])[./-]([0-9]{4}|[0-9]{2})$");
                    var match = rx.Matches(keyword);
                    if (match.Count > 0)
                    {
                        var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimezone);
                        try
                        {
                            isDate = true;
                            date = TimeZoneInfo.ConvertTimeToUtc(keyword.ConvertDateFormat(dateTimeFormat), tz);
                        }
                        catch
                        {
                            date = DateTime.UtcNow;
                        }

                    }
                    //End
                    var _id = 0;
                    var endate = date.AddDays(1);
                    int.TryParse(keyword.TrimStart('0'), out _id);
                    query = query.Where(q =>
                        q.Id == _id
                        || (isDate && q.FinalisedDate >= date && q.FinalisedDate < endate)
                        || q.Notes.Contains(keyword)
                        || q.Invoice.Id == _id
                        || q.Sale.Id == _id
                    );
                }
                totalSpot = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Ref":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Reason":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reason" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Value":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Value" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Finalised":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "FinalisedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Notes":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Notes" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "FinalisedDate asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "FinalisedDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    Id = q.Id,
                    Ref = q.Reference?.FullRef,
                    Reason = q.Reason.ToString(),
                    Finalised = q.FinalisedDate.ConvertTimeFromUtc(currentTimezone).ToString(dateTimeFormat),
                    InvoiceRef = (q.Invoice != null && q.Invoice.Reference != null) ? q.Invoice.Reference.FullRef : "",
                    SaleRef = (q.Sale != null && q.Sale.Reference != null) ? q.Sale.Reference.FullRef : "",
                    Value = q.Value.ToDecimalPlace(currencySettings),
                    Notes = q.Notes?.Replace(Environment.NewLine, "<br/>").Replace("\n", "<br/>"),
                    Status = q.Status,
                    Date = q.CreatedDate.ConvertTimeFromUtc(currentTimezone).ToString(dateTimeFormat),
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalSpot, totalSpot);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactId, type, requestModel, currentTimezone, dateTimeFormat, domainId);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        private TraderAddress TraderAddressGetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.TraderAddress.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderAddress();
            }
        }

        public void UpdateContactAvatar(int id, string uri)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, uri);

                var s3Rules = new Azure.AzureStorageRules(dbContext);
                s3Rules.ProcessingMediaS3(uri);

                var ct = GetTraderContactId(id);
                if (!string.IsNullOrEmpty(uri))
                {
                    ct.AvatarUri = uri;
                }

                if (dbContext.Entry(ct).State == EntityState.Detached)
                {
                    dbContext.TraderContacts.Attach(ct);
                }

                dbContext.Entry(ct).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, uri);
            }
        }


        public void ContactApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                TraderContact contact = approval.TraderContact.FirstOrDefault();
                if (contact == null)
                {
                    return;
                }

                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        contact.Status = TraderContactStatusEnum.PendingReview;
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        contact.Status = TraderContactStatusEnum.PendingApproval;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        contact.Status = TraderContactStatusEnum.ContactApproved;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        contact.Status = TraderContactStatusEnum.ContactDenied;
                        break;
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        contact.Status = TraderContactStatusEnum.ContactDiscarded;
                        break;
                }
                dbContext.Entry(contact).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }
        public void CreditNoteApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                CreditNote creditNote = approval.CreditNotes.FirstOrDefault();
                if (creditNote == null)
                {
                    return;
                }

                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        creditNote.Status = CreditNoteStatus.PendingReview;
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        creditNote.Status = CreditNoteStatus.Reviewed;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        creditNote.Status = CreditNoteStatus.Approved;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        creditNote.Status = CreditNoteStatus.Denied;
                        break;
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        creditNote.Status = CreditNoteStatus.Discarded;
                        break;
                }
                dbContext.Entry(creditNote).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }
        public CreditNote GetCreditNoteById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var creditnote = new CreditNote();
                if (id > 0)
                {
                    creditnote = dbContext.CreditNotes.Find(id) ?? (new CreditNote());
                }
                return creditnote;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new CreditNote();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="domainId"></param>
        /// <param name="action">1-Add or Edit, 2-Approve</param>
        /// <returns></returns>
        public ReturnJsonModel ValidateOnApproveContact(TraderContact contact, int domainId)
        {
            var rs = new ReturnJsonModel() { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contact, domainId);

                var isApprovedContactExisted = dbContext.TraderContacts.Any(p => p.Email == contact.Email && p.Id != contact.Id && p.ContactGroup.Domain.Id == domainId);
                if (isApprovedContactExisted)
                {
                    rs.result = false;
                    rs.msg = "A Trader Contact with the same email already exists in the domain.";
                }

                return rs;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contact, domainId);
                rs.result = false;
                rs.msg = "Failed to check existing approved Contact in the Domain.";
                return rs;
            }
        }

        public List<TraderContact> GetSearchTop10TraderContact(int domainId, string keyword)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, keyword);

                return dbContext.TraderContacts.Where(s => s.ContactGroup.Domain.Id == domainId && s.Status == TraderContactStatusEnum.ContactApproved
                && s.Name.Contains(keyword) || s.Email.Contains(keyword)).Take(10).OrderBy(s => s.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, keyword);
                return new List<TraderContact>();
            }
        }

        public TraderContact GetOrCreateTraderContact(ApplicationUser user, QbicleDomain domain)
        {
            // Find the TraderContact or create one
            // Create
            var helper = new Helper.OrderProcessingHelper(dbContext);
            return helper.GetCreateTraderContactFromUserInfo(user, domain, Models.Trader.SalesChannel.SalesChannelEnum.Trader);
        }

        /// <summary>
        /// If there is a QbicleUser (ApplicationUser), in the Qbicles System, 
        /// with the same email address as the TraderContact link the QbicleUser to the new TraderContact
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="domain"></param>
        public ApplicationUser LinkUserToContact(TraderContact contact)
        {
            try
            {
                var userContact = dbContext.QbicleUser.FirstOrDefault(e => e.Email == contact.Email);
                if (userContact != null)
                {
                    contact.QbicleUser = userContact;
                    dbContext.SaveChanges();
                }
                return userContact;
            }

            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contact.Id);
                return null;
            }
        }

        public void LinkContactsToUser(string email)
        {
            try
            {
                var user = dbContext.QbicleUser.FirstOrDefault(e => e.Email == email);
                var contacts = dbContext.TraderContacts.Where(e => e.Email == email).ToList();
                if (contacts.Count > 0)
                {
                    contacts.ForEach(c =>
                    {
                        c.QbicleUser = user;
                    });
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, email);
            }
        }

        public List<ContactGroupModel> GetTraderContactsGroupConfig(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return (from d in dbContext.TraderContactGroups
                        where d.Domain.Id == domainId
                        select new ContactGroupModel
                        {
                            Id = d.Id,
                            Name = d.Name,
                            CreatedDate = d.CreatedDate,
                            CreatedBy = d.Creator,
                            CanDelete = d.Contacts.Count == 0,
                            Members = d.Contacts.Count
                        }).OrderBy(n => n.Name).ToList();

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<ContactGroupModel>();
            }
        }

        private bool checkTraderContactCanBeDeleted(TraderContact contact)
        {
            if (contact.Status == TraderContactStatusEnum.ContactApproved)
            {
                return false;
            }
            if (dbContext.B2BRelationships.Any(p => p.Domain1TraderContactForDomain2.Id == contact.Id
             || p.Domain2TraderContactForDomain1.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.B2BTradeOrders.Any(p => p.VendorTraderContact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.TradeOrders.Any(p => p.TraderContact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.StoreCreditAccounts.Any(p => p.Contact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.StoreCreditPINs.Any(p => p.CreatedByContact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.StorePointAccounts.Any(p => p.Contact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.SMContacts.Any(p => p.TraderContact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.CashAccountTransactions.Any(p => p.Contact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.CashAccountTransactionLogs.Any(p => p.Contact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.Shipments.Any(p => p.Driver.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.TraderTransfers.Any(p => p.Contact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.TraderTransferLogs.Any(p => p.Contact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.BalanceAllocations.Any(p => p.Contact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.CreditNotes.Any(p => p.Contact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.TraderPurchaseLogs.Any(p => p.Vendor.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.ReorderItems.Any(p => p.PrimaryContact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.ReorderItemGroups.Any(p => p.PrimaryContact.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.TraderSaleLogs.Any(p => p.Purchaser.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.PosSettings.Any(p => p.DefaultWalkinCustomer.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.TraderItemVendors.Any(p => p.Vendor.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.TraderPurchases.Any(p => p.Vendor.Id == contact.Id))
            {
                return false;
            }
            if (dbContext.TraderSales.Any(p => p.Purchaser.Id == contact.Id))
            {
                return false;
            }
            return true;
        }

        public ReturnJsonModel GetTraderContactsDataForSelect2(int domainId, string keySearch = "")
        {
            var result = new ReturnJsonModel() { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet == true)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, keySearch);

                var contact_query = dbContext.TraderContacts
                    .Where(q => q.ContactGroup != null && q.ContactGroup.Domain.Id == domainId);
                if (!string.IsNullOrEmpty(keySearch))
                {
                    contact_query = contact_query.Where(p => p.Name.ToLower().Contains(keySearch.ToLower()));
                }
                var lst_contacts = contact_query.OrderBy(p => p.Name).Take(10).Select(p => new
                {
                    id = p.Id,
                    text = p.Name
                }).ToList();
                result.Object = lst_contacts;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, keySearch);
                result.result = false;
            }
            return result;
        }

        public object FindSaleCreditServerSide(IDataTablesRequest requestModel, string keyword, int contactId, string dateRange, UserSetting dateTimeFormat, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, requestModel, keyword);

                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                var query = dbContext.TraderSales.Where(q => q.Purchaser != null && q.Purchaser.Id == contactId);

                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q =>
                        q.Purchaser != null && q.Purchaser.Name.ToLower().Contains(keyword)
                        || (q.Reference != null && q.Reference.FullRef.ToLower().Contains(keyword)));

                if (!string.IsNullOrEmpty(dateRange))
                {
                    if (!dateRange.Contains('-'))
                    {
                        dateRange += "-";
                    }
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    dateRange.ConvertDaterangeFormat(dateTimeFormat.DateFormat, "", out startDate, out endDate);
                    startDate = startDate.AddTicks(1);
                    endDate = endDate.AddDays(1).AddTicks(-1);
                    query = query.Where(q => q.CreatedDate >= startDate && q.CreatedDate <= endDate);
                }




                var total = query.Count();

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
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "SaleTotal":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SaleTotal" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.Reference?.FullRef,
                    ReferenceId = q.Reference?.Id ?? 0,
                    SaleTotal = q.SaleTotal.ToDecimalPlace(currencySettings),
                    CreatedDate = q.CreatedDate.ConvertTimeFromUtc(dateTimeFormat.Timezone).ToString(dateTimeFormat.DateFormat + " " + dateTimeFormat.TimeFormat),
                    ReportingFilters = q.SaleItems.SelectMany(s => s.Dimensions).Select(d => d.Name)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, total, total);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
                return null;
            }
        }

        public object FindPurchaseCreditServerSide(IDataTablesRequest requestModel, string keyword, int contactId, string dateRange, UserSetting dateTimeFormat, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, requestModel, keyword);

                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                var query = dbContext.TraderPurchases.Where(q => q.Vendor != null && q.Vendor.Id == contactId);


                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q =>
                        q.Vendor != null && q.Vendor.Name.ToLower().Contains(keyword)
                        || (q.Reference != null && q.Reference.FullRef.ToLower().Contains(keyword)));

                if (!string.IsNullOrEmpty(dateRange))
                {
                    if (!dateRange.Contains('-'))
                    {
                        dateRange += "-";
                    }
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    dateRange.ConvertDaterangeFormat(dateTimeFormat.DateFormat, "", out startDate, out endDate);
                    startDate = startDate.AddTicks(1);
                    endDate = endDate.AddDays(1).AddTicks(-1);
                    query = query.Where(q => q.CreatedDate >= startDate && q.CreatedDate <= endDate);
                }




                var total = query.Count();

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
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "PurchaseTotal":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "PurchaseTotal" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.Reference?.FullRef,
                    ReferenceId = q.Reference?.Id ?? 0,
                    PurchaseTotal = q.PurchaseTotal.ToDecimalPlace(currencySettings),
                    CreatedDate = q.CreatedDate.ConvertTimeFromUtc(dateTimeFormat.Timezone).ToString(dateTimeFormat.DateFormat + " " + dateTimeFormat.TimeFormat),
                    ReportingFilters = q.PurchaseItems.SelectMany(s => s.Dimensions).Select(d => d.Name)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, total, total);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
                return null;
            }
        }


        public object FindInvoiceServerSide(IDataTablesRequest requestModel, string keyword, int contactId, string dateRange, string type, UserSetting dateTimeFormat, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, requestModel, keyword);

                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);


                IQueryable<Invoice> query;
                if (type == "CreditNote")
                    query = dbContext.TraderSales.Where(p => p.Purchaser.Id == contactId).SelectMany(s => s.Invoices);
                else
                    query = dbContext.TraderPurchases.Where(p => p.Vendor.Id == contactId).SelectMany(s => s.Invoices);

                //var query = saleInvoiceQuery.Union(purchaseInvoiceQuery);

                //var query = dbContext.TraderPurchases.Where(q => q.Vendor != null && q.Vendor.Id == contactId);


                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q =>
                        q.Sale != null && q.Sale.Purchaser.Name.ToLower().Contains(keyword)
                        || q.Purchase != null && q.Purchase.Vendor.Name.ToLower().Contains(keyword)
                        || (q.Reference != null && q.Reference.FullRef.ToLower().Contains(keyword)));

                if (!string.IsNullOrEmpty(dateRange))
                {
                    if (!dateRange.Contains('-'))
                    {
                        dateRange += "-";
                    }
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    dateRange.ConvertDaterangeFormat(dateTimeFormat.DateFormat, "", out startDate, out endDate);
                    startDate = startDate.AddTicks(1);
                    endDate = endDate.AddDays(1).AddTicks(-1);
                    query = query.Where(q => q.CreatedDate >= startDate && q.CreatedDate <= endDate);
                }




                var total = query.Count();

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
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        //case "BalanceInvoice":
                        //    orderByString += orderByString != string.Empty ? "," : "";
                        //    orderByString += "TotalInvoiceAmount" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                        //    break;

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
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.Reference?.FullRef,
                    ReferenceId = q.Reference?.Id ?? 0,
                    BalanceInvoice = GetBalanceInvoice(q).ToDecimalPlace(currencySettings),
                    CreatedDate = q.CreatedDate.ConvertTimeFromUtc(dateTimeFormat.Timezone).ToString(dateTimeFormat.DateFormat + " " + dateTimeFormat.TimeFormat),
                    AmountPaid = GetAmountPaid(q.Id).ToString(),
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, total, total);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
                return null;
            }
        }

        private decimal GetBalanceInvoice(Invoice invoice)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, invoice.Id);
                decimal payments, allocation = 0;

                var lstpaymentsIn = dbContext.CashAccountTransactions.Where(q => q.AssociatedInvoice.Id == invoice.Id
                                                                                 && q.Status == TraderPaymentStatusEnum.PaymentApproved);
                if (lstpaymentsIn.Any())
                {
                    payments = lstpaymentsIn.Sum(q => q.Amount);
                }
                else payments = 0;
                var lstallocation = dbContext.BalanceAllocations.Where(q => q.Invoice.Id == invoice.Id);
                if (lstallocation.Any())
                {
                    allocation = lstallocation.Sum(q => q.Value);
                }
                else allocation = 0;
                return invoice.TotalInvoiceAmount - (payments + allocation);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, invoice.Id);
                return 0;
            }
        }

        public decimal GetAmountPaid(Invoice invoice)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, invoice.Id);
                decimal payments, allocation = 0;
                var lstpaymentsIn = dbContext.CashAccountTransactions.Where(q => q.AssociatedInvoice.Id == invoice.Id
                                                                                 && q.Status == TraderPaymentStatusEnum.PaymentApproved);
                if (lstpaymentsIn.Any())
                {
                    payments = lstpaymentsIn.Sum(q => q.Amount);
                }
                else payments = 0;
                var lstallocation = dbContext.BalanceAllocations.Where(q => q.Invoice.Id == invoice.Id);
                if (lstallocation.Any())
                {
                    allocation = lstallocation.Sum(q => q.Value);
                }
                else allocation = 0;
                return payments + allocation;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, invoice.Id);
                return 0;
            }
        }
    }
}