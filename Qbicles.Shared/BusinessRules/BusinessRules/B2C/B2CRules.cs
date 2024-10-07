using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Catalogs;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.Pricing;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Qbicles.Models.Notification;
using Variant = Qbicles.Models.Catalogs.Variant;

namespace Qbicles.BusinessRules.B2C
{
    public class B2CRules
    {
        private readonly ApplicationDbContext _dbContext;

        public B2CRules(ApplicationDbContext context)
        {
            _dbContext = context;
        }
        public bool CheckHasAccessB2C(int domainId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId, userId);
                //only be visible to Domain Administrators and those users in the role Business User...
                //var isB2CConnectionAllowed = false;
                var b2cQbicleMemIds = _dbContext.B2CQbicles.Where(p => p.Business.Id == domainId && !p.IsHidden).SelectMany(x => x.Members).Select(x => x.Id).Distinct().ToList();
                //A user who is a Relationship Manager in a B2C Qbicle should also have access to the B2B menu item.
                return b2cQbicleMemIds.Contains(userId);
                //if (b2cQbicleMemIds.Contains(currentUserId))
                //{
                //    isB2CConnectionAllowed = true;
                //}

                //return isB2CConnectionAllowed;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, userId);
                return false;
            }
        }

        public bool CheckHasAccessB2B(int domainId, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId, currentUserId);
                //only be visible to Domain Administrators and those users in the role Business User...
                var isB2BConnectionAllowed = false;

                var b2bQbicleMemIds = _dbContext.B2BQbicles.Where(p => p.Domains.Any(x => x.Id == domainId)).SelectMany(x => x.Members).Select(x => x.Id).Distinct().ToList();
                //A user who is a Relationship Manager in a B2B Qbicle should also have access to the B2B menu item.
                if (b2bQbicleMemIds.Contains(currentUserId))
                {
                    isB2BConnectionAllowed = true;
                }

                return isB2BConnectionAllowed;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, currentUserId);
                return false;
            }
        }
        public B2CQbicle Get2CQbicleByQId(int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qbicleId);
                return _dbContext.B2CQbicles.Find(qbicleId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId);
                return new B2CQbicle();
            }
        }
        public B2CQbicle Get2CQbicleByBusinessIdAndCustomerId(int businessId, string customerId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, businessId, customerId);
                return _dbContext.B2CQbicles.FirstOrDefault(s => !s.IsHidden && s.Business.Id == businessId && s.Customer.Id == customerId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, businessId, customerId);
                return new B2CQbicle();
            }
        }
        public B2CQbicle CreateB2CQbicleForChannel(int businessId, string customerId, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, businessId, customerId);
                var b2Cqbicle = _dbContext.B2CQbicles.FirstOrDefault(s => !s.IsHidden && s.Business.Id == businessId && s.Customer.Id == customerId);
                if (b2Cqbicle == null)
                {
                    var currentUser = _dbContext.QbicleUser.Find(currentUserId);
                    var b2BProfile = _dbContext.B2BProfiles.Find(businessId);
                    var business = _dbContext.Domains.Find(businessId);
                    b2BProfile = new Models.B2B.B2BProfile
                    {
                        BusinessName = business.Name,
                        BusinessSummary = "",
                        Domain = business,
                        IsDisplayedInB2CListings = true,
                        LogoUri = business.LogoUri,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedBy = currentUser,
                        LastUpdatedDate = DateTime.UtcNow,
                    };

                    b2BProfile.DefaultB2CRelationshipManagers.AddRange(business.Administrators);
                    _dbContext.B2BProfiles.Add(b2BProfile);
                    _dbContext.Entry(b2BProfile).State = EntityState.Added;

                    // Need to add a Trader Contact here if possible
                    new OrderProcessingHelper(_dbContext).GetCreateTraderContactFromUserInfo(currentUser, b2BProfile.Domain, SalesChannelEnum.B2C);
                    b2Cqbicle = new B2CQbicle
                    {
                        Status = CommsStatus.Approved,
                        Domain = _dbContext.SystemDomains.FirstOrDefault(s => s.Name == SystemDomainConst.BUSINESS2CUSTOMER && s.Type == SystemDomainType.B2C),
                        Business = b2BProfile.Domain,
                        Customer = currentUser,
                        Name = $"{b2BProfile.BusinessName} & {HelperClass.GetFullNameOfUser(currentUser)}",
                        Description = $"{SystemDomainConst.BUSINESS2CUSTOMER} - {b2BProfile.BusinessName} & {HelperClass.GetFullNameOfUser(currentUser)}",
                        LogoUri = HelperClass.QbicleLogoDefault,
                        IsHidden = false,
                        OwnedBy = currentUser,
                        StartedBy = currentUser,
                        Manager = currentUser,
                        StartedDate = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };

                    if (b2BProfile.DefaultB2CRelationshipManagers.Any())
                        b2Cqbicle.Members.AddRange(b2BProfile.DefaultB2CRelationshipManagers);
                    if (!b2Cqbicle.Members.Any(s => s.Id == currentUser.Id))
                        b2Cqbicle.Members.Add(currentUser);

                    _dbContext.B2CQbicles.Add(b2Cqbicle);
                    _dbContext.Entry(b2Cqbicle).State = EntityState.Added;
                    _dbContext.SaveChanges();

                    new TopicRules(_dbContext).GetCreateTopicByName(HelperClass.GeneralName, b2Cqbicle.Id);

                }
                return b2Cqbicle;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, businessId, customerId);
                return new B2CQbicle();
            }
        }

        /// <summary>
        /// Get all b2c qbicle by current domain
        /// </summary>
        /// <param name="currentDomainId">current domain Id</param>
        /// <param name="currentUserId"></param>
        /// <param name="keyword">Search contacts...</param>
        /// <param name="orderby">
        /// 0: Order by latest activity(Default)
        /// 1: Order by forename A-Z
        /// 2: Order by forename Z-A
        /// 3: Order by surname A-Z
        /// 4: Order by surname Z-A
        /// </param>
        /// <param name="typeShown">1 - show all, exclude blocked connections, 2 - show new connections, 3 - show blocked connections</param>
        /// <returns>List C2CQbicle</returns>
        public List<B2CQbicle> GetB2CQbicles(int currentDomainId, string currentUserId, string keyword, int orderby, int typeShown = 1)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, currentDomainId, keyword, orderby, typeShown);

                var qbicles = GetB2CQbiclesData(currentDomainId, currentUserId, keyword, orderby, typeShown).ToList();


                var b2cQbicles = new List<B2CQbicle>();
                qbicles.ForEach(q =>
                {
                    if (q.RemovedForUsers.Count == 0)
                        b2cQbicles.Add(q);
                    else if (q.RemovedForUsers.Any(r => r.Id != currentUserId))
                        b2cQbicles.Add(q);
                });

                return b2cQbicles;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentDomainId, keyword, orderby, typeShown);
                return new List<B2CQbicle>();
            }
        }

        public IQueryable<B2CQbicle> GetB2CQbiclesData(int currentDomainId, string currentUserId, string keyword, int orderby, int typeShown = 1)
        {
            var domain = _dbContext.Domains.Find(currentDomainId);
            var isDomainAdmin = domain?.Administrators.Any(p => p.Id == currentUserId) ?? false;
            var query = from b2c in _dbContext.B2CQbicles
                        where !b2c.IsHidden
                        && b2c.Business.Id == currentDomainId
                        && (isDomainAdmin || b2c.Members.Any(s => s.Id == currentUserId))
                        && (!b2c.RemovedForUsers.Any(p => p.Id == currentUserId))
                        select b2c;
            #region Filters
            if (typeShown == 1)
            {
                query = query.Where(s => s.Status != CommsStatus.Blocked);
            }
            else if (typeShown == 2)
            {
                query = query.Where(s => s.IsNewContact == true);
            }
            else if (typeShown == 3)
            {
                query = query.Where(s => s.Status == CommsStatus.Blocked);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s => (s.Customer.Forename + " " + s.Customer.Surname).Contains(keyword));
            }
            #endregion
            #region Order by
            switch (orderby)
            {
                case 1://Order by forename A-Z
                    query = query.OrderBy(s => s.Customer.Forename);
                    break;
                case 2://Order by forename Z-A
                    query = query.OrderByDescending(s => s.Customer.Forename);
                    break;
                case 3://Order by surname A-Z
                    query = query.OrderBy(s => s.Customer.Surname);
                    break;
                case 4://Order by surname Z-A
                    query = query.OrderByDescending(s => s.Customer.Surname);
                    break;
                default://Order by latest activity
                    query = query.OrderByDescending(s => s.LastUpdated);
                    break;
            }
            #endregion

            return query;
        }

        /// <summary>
        /// This is the function update status the QBicle
        /// </summary>
        /// <param name="qId">This is qbicleid</param>
        /// <param name="currentDomainId">This is current DomainId</param>
        /// <param name="currentUserId">This is current UserId</param>
        /// <param name="status">This indicates the status of the communications between the parties in the Qbicle</param>
        /// <param name="type">1: Businesses, 2: Individual</param>
        /// <returns></returns>
        public ReturnJsonModel SetStatusByBusiness(int qId, int currentDomainId, string currentUserId, CommsStatus status)
        {
            var returnJson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId, status);
                var b2cqbicle = _dbContext.B2CQbicles.Find(qId);
                if (b2cqbicle != null)
                {
                    if (b2cqbicle.Business.Id != currentDomainId)
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                        return returnJson;
                    }
                    if (status == CommsStatus.Approved)
                    {
                        b2cqbicle.BlockerDomain = null;
                        b2cqbicle.Status = status;
                    }
                    else if (status == CommsStatus.Blocked)
                    {
                        b2cqbicle.Status = status;
                        b2cqbicle.BlockerDomain = _dbContext.Domains.Find(currentDomainId);
                    }
                    var relationshipLog = new CustomerRelationshipLog
                    {
                        QbicleId = b2cqbicle.Id,
                        Status = status,
                        UserId = currentUserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    if (b2cqbicle.Status == CommsStatus.Blocked)
                    {
                        relationshipLog.BlockedByDomainId = currentDomainId;
                    }

                    _dbContext.CustomerRelationshipLogs.Add(relationshipLog);
                    _dbContext.Entry(relationshipLog).State = EntityState.Added;
                }
                returnJson.result = _dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                returnJson.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId, status);
            }
            return returnJson;
        }
        /// <summary>
        /// This is the function update status the QBicle
        /// </summary>
        /// <param name="qId">This is qbicleid</param>
        /// <param name="currentUserId">This is current UserId</param>
        /// <param name="status">This indicates the status of the communications between the parties in the Qbicle</param>
        /// <param name="type">1: Businesses, 2: Individual</param>
        /// <returns></returns>
        public ReturnJsonModel SetStatusByCustomer(int qId, string currentUserId, CommsStatus status)
        {
            var returnJson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId, status);
                var b2cqbicle = _dbContext.B2CQbicles.Find(qId);
                if (b2cqbicle != null)
                {
                    if (b2cqbicle.Customer.Id != currentUserId)
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                        return returnJson;
                    }
                    if (status == CommsStatus.Approved)
                    {
                        b2cqbicle.Blocker = null;
                        b2cqbicle.Status = status;
                    }
                    else if (status == CommsStatus.Blocked)
                    {
                        b2cqbicle.Status = status;
                        b2cqbicle.Blocker = _dbContext.QbicleUser.Find(currentUserId);
                    }
                    var relationshipLog = new CustomerRelationshipLog
                    {
                        QbicleId = b2cqbicle.Id,
                        Status = status,
                        UserId = currentUserId,
                        CreatedDate = DateTime.UtcNow
                    };
                    if (b2cqbicle.Status == CommsStatus.Blocked)
                    {
                        relationshipLog.BlockedByUserId = currentUserId;
                    }
                    _dbContext.CustomerRelationshipLogs.Add(relationshipLog);
                    _dbContext.Entry(relationshipLog).State = EntityState.Added;
                }
                _dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId, status);
            }
            return returnJson;
        }
        public ReturnJsonModel SetViewedConnection(int qId, bool isViewed = true)
        {
            var returnJson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId);
                var b2cqbicle = _dbContext.B2CQbicles.Find(qId);
                if (b2cqbicle != null)
                {
                    b2cqbicle.BusinessViewed = isViewed;
                    if (b2cqbicle.IsNewContact == true)
                    {
                        b2cqbicle.IsNewContact = false;
                    }
                }
                returnJson.result = _dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId);
            }
            return returnJson;
        }
        public B2CQbicle GetB2CQbicleById(int qId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId);
                return _dbContext.B2CQbicles.Find(qId); ;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId);
                return new B2CQbicle();
            }
        }
        public ReturnJsonModel UpdateRelationshipManagers(int qId, string currentUserId, List<string> UserIds)
        {
            var returnJson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId, currentUserId, UserIds);
                var b2cqbicle = _dbContext.B2CQbicles.Find(qId);
                if (b2cqbicle != null)
                {
                    var isDomainAdmin = b2cqbicle.Business.Administrators.Any(s => s.Id == currentUserId);
                    if (!isDomainAdmin && !b2cqbicle.Members.Any(s => s.Id == currentUserId && s.Id != b2cqbicle.Customer.Id))
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                        return returnJson;
                    }
                    var removeMembers = b2cqbicle.Members.Where(s => s.Id != currentUserId && s.Id != b2cqbicle.Customer.Id).ToList();
                    if (removeMembers != null && removeMembers.Any())
                        foreach (var item in removeMembers)
                        {
                            b2cqbicle.Members.Remove(item);
                        }
                    if (UserIds != null && UserIds.Any())
                        foreach (var userid in UserIds)
                        {
                            var user = _dbContext.QbicleUser.Find(userid);
                            if (user != null && !b2cqbicle.Members.Any(s => s.Id == userid))
                                b2cqbicle.Members.Add(user);
                        }
                }
                returnJson.result = _dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId, currentUserId, UserIds);
            }
            return returnJson;
        }
        public ReturnJsonModel RemoveB2CQbicleById(int qId, string currentUserId)
        {
            var returnJson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId, currentUserId);
                var b2cqbicle = _dbContext.B2CQbicles.Find(qId);
                if (b2cqbicle != null)
                {
                    if (!b2cqbicle.Members.Any(u => u.Id == currentUserId))
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                        return returnJson;
                    }
                    b2cqbicle.IsHidden = true;
                }
                _dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                returnJson.result = false;
                returnJson.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId, currentUserId);
            }
            return returnJson;
        }

        public List<Catalog> GetPosMenusByB2CQbicleId(int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, qbicleId);
                return _dbContext.B2CProductMenuDiscussions.Where(s => s.Qbicle.Id == qbicleId).Select(s => s.ProductMenu).Distinct().ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId);
                return new List<Catalog>();
            }
        }


        #region Get data for Bookkeeping
        public DataTablesResponse GetBkManuDatas(IDataTablesRequest requestModel, int domainId, string keyword, string daterange, int locationId, string dateTimeFormat, string dateFormat, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);

                var query = _dbContext.ManufacturingBookkeepingLogs.Where(p => p.Domain.Id == domainId);
                #region Filter
                if (!string.IsNullOrEmpty(keyword.Trim()))
                {
                    keyword = keyword.Trim().ToLower();
                    query = query.Where(p => p.JournalEntry.Number.ToString().Contains(keyword)
                                || (p.ManufacturingJob.Reference.FullRef).ToLower().Contains(keyword)
                                || (p.TraderSale.Reference.FullRef).ToLower().Contains(keyword)
                                || (p.TransferIn.Reference.FullRef).ToLower().Contains(keyword)
                                || (p.TransferOut.Reference.FullRef).ToLower().Contains(keyword));
                }

                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    daterange.ConvertDaterangeFormat(dateTimeFormat, timeZone, out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }

                if (locationId > 0)
                {
                    query = query.Where(p => p.ManufacturingJob.Location.Id == locationId || p.TraderSale.Location.Id == locationId);
                }

                var totalRecord = query.Count();
                #endregion

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreatedBy":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.ForeName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.SurName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "JournalEntry":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "JournalEntry.Number" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Job":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "ManufacturingJob.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Sale":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Sale.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "TransferIn":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TransferIn.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "TransferOut":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TransferOut.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                var dataJson = list.Select(s => new
                {
                    CreatedDate = s.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                    CreatedBy = s.CreatedBy.GetFullName(),
                    CreatedById = s.CreatedBy.Id,
                    JournalEntryId = s.JournalEntry?.Id ?? 0,
                    JournalEntryNumber = s.JournalEntry?.Number ?? 0,
                    JobId = s.ManufacturingJob?.Id ?? 0,
                    JobRef = s.ManufacturingJob == null ? "" : (s.ManufacturingJob?.Reference?.FullRef ?? "#" + s.ManufacturingJob.Id),
                    SaleId = s.TraderSale?.Id ?? 0,
                    SaleKey = s.TraderSale?.Key ?? "",
                    SaleRef = s.TraderSale == null ? "" : (s.TraderSale?.Reference?.FullRef ?? ("#" + s.TraderSale.Id)),
                    TransferInId = s.TransferIn?.Id ?? 0,
                    TransferInKey = s.TransferIn?.Key ?? "",
                    TransferInRef = s.TransferIn == null ? "" : (s.TransferIn?.Reference?.FullRef ?? "#" + s.TransferIn.Id),
                    TransferOutId = s.TransferOut?.Id ?? 0,
                    TransferOutKey = s.TransferOut?.Key ?? "",
                    TransferOutRef = s.TransferOut == null ? "" : (s.TransferOut?.Reference?.FullRef ?? ("#" + s.TransferOut.Id))
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecord, totalRecord);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }

        }

        public DataTablesResponse GetBkPaymentDatas(IDataTablesRequest requestModel, int domainId, string keyword, string daterange, int locationId, string dateTimeFormat, string dateFormat, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);

                var query = _dbContext.PaymentBookkeepingLogs.Where(p => p.Domain.Id == domainId);
                #region Filter
                if (!string.IsNullOrEmpty(keyword.Trim()))
                {
                    keyword = keyword.Trim().ToLower();
                    query = query.Where(p => p.JournalEntry.Number.ToString().Contains(keyword)
                                || (p.Payment.Reference).ToLower().Contains(keyword));
                }

                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    daterange.ConvertDaterangeFormat(dateTimeFormat, timeZone, out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }

                if (locationId > 0)
                {
                    query = query.Where(p => p.Payment.Workgroup.Location.Id == locationId);
                }

                var totalRecord = query.Count();
                #endregion

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreatedBy":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.ForeName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.SurName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "JournalEntry":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "JournalEntry.Number" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Payment":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Payment.Reference" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                var dataJson = list.Select(s => new
                {
                    CreatedDate = s.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                    CreatedBy = s.CreatedBy.GetFullName(),
                    CreatedById = s.CreatedBy.Id,
                    JournalEntryId = s.JournalEntry?.Id ?? 0,
                    JournalEntryNumber = s.JournalEntry?.Number ?? 0,
                    PaymentId = s.Payment?.Id ?? 0,
                    PaymentRef = s.Payment == null ? "" : (s.Payment?.Reference ?? "#" + s.Payment.Id)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecord, totalRecord);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }

        }

        public DataTablesResponse GetBkInvPurchaseDatas(IDataTablesRequest requestModel, int domainId, string keyword, string daterange, int locationId, string dateTimeFormat, string dateFormat, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);

                var query = _dbContext.PurchaseInventoryBookkeepingLogs.Where(p => p.Domain.Id == domainId);
                #region Filter
                if (!string.IsNullOrEmpty(keyword.Trim()))
                {
                    keyword = keyword.Trim().ToLower();
                    query = query.Where(p => p.JournalEntry.Number.ToString().Contains(keyword)
                                || (p.PurchaseTransfer.Purchase.Reference.FullRef).ToLower().Contains(keyword)
                                || (p.PurchaseTransfer.TransferApprovalProcess.Transfer.Any(x => x.Reference.FullRef.ToLower().Contains(keyword))));
                }

                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    daterange.ConvertDaterangeFormat(dateTimeFormat, timeZone, out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }

                if (locationId > 0)
                {
                    query = query.Where(p => p.PurchaseTransfer.Purchase.Location.Id == locationId);
                }
                var totalRecord = query.Count();
                #endregion

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreatedBy":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.ForeName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.SurName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "JournalEntry":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "JournalEntry.Number" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Purchase":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "PurchaseTransfer.Purchase.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                var dataJson = list.Select(s => new
                {
                    CreatedDate = s.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                    CreatedBy = s.CreatedBy.GetFullName(),
                    CreatedById = s.CreatedBy.Id,
                    JournalEntryId = s.JournalEntry?.Id ?? 0,
                    JournalEntryNumber = s.JournalEntry?.Number ?? 0,
                    PurchaseId = s.PurchaseTransfer?.Purchase?.Id ?? 0,
                    PurchaseRef = s.PurchaseTransfer?.Purchase == null ? "" : (s.PurchaseTransfer?.Purchase?.Reference?.FullRef ?? ("#" + s.PurchaseTransfer.Purchase.Id)),
                    TransferId = s.PurchaseTransfer?.TransferApprovalProcess?.Transfer?.FirstOrDefault()?.Id ?? 0,
                    TransferKey = s.PurchaseTransfer?.TransferApprovalProcess?.Transfer?.FirstOrDefault()?.Key ?? "",
                    TransferRef = s.PurchaseTransfer?.TransferApprovalProcess?.Transfer?.FirstOrDefault() == null
                                    ? "" : s.PurchaseTransfer?.TransferApprovalProcess?.Transfer?.FirstOrDefault()?.Reference?.FullRef
                                    ?? ("#" + s.PurchaseTransfer?.TransferApprovalProcess?.Transfer?.FirstOrDefault()?.Id)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecord, totalRecord);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }

        }

        public DataTablesResponse GetBkNonInvPurchaseDatas(IDataTablesRequest requestModel, int domainId, string keyword, string daterange, int locationId, string dateTimeFormat, string dateFormat, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);

                var query = _dbContext.PurchaseNoninvBookkeepingLogs.Where(p => p.Domain.Id == domainId);
                #region Filter
                if (!string.IsNullOrEmpty(keyword.Trim()))
                {
                    keyword = keyword.Trim().ToLower();
                    query = query.Where(p => p.JournalEntry.Number.ToString().Contains(keyword)
                                || (p.Bill.Reference.FullRef).ToLower().Contains(keyword)
                                || (p.Bill.Purchase.Reference.FullRef.ToLower().Contains(keyword)));
                }

                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    daterange.ConvertDaterangeFormat(dateTimeFormat, timeZone, out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }
                if (locationId > 0)
                {
                    query = query.Where(p => p.Bill.Purchase.Location.Id == locationId);
                }
                var totalRecord = query.Count();
                #endregion

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreatedBy":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.ForeName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.SurName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "JournalEntry":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "JournalEntry.Number" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Purchase":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Bill.Purchase.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Bill":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Bill.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                var dataJson = list.Select(s => new
                {
                    CreatedDate = s.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                    CreatedBy = s.CreatedBy.GetFullName(),
                    CreatedById = s.CreatedBy.Id,
                    JournalEntryId = s.JournalEntry?.Id ?? 0,
                    JournalEntryNumber = s.JournalEntry?.Number ?? 0,
                    PurchaseId = s.Bill?.Purchase?.Id ?? 0,
                    PurchaseRef = s.Bill?.Purchase == null ? "" : (s.Bill?.Purchase?.Reference?.FullRef ?? ("#" + s.Bill.Purchase.Id)),
                    BillId = s.Bill?.Id ?? 0,
                    BillRef = s.Bill == null ? "" : (s.Bill?.Reference?.FullRef ?? ("#" + s.Bill.Id))
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecord, totalRecord);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }

        }

        public DataTablesResponse GetBkSaleInvoiceDatas(IDataTablesRequest requestModel, int domainId, string keyword, string daterange, int locationId, string dateTimeFormat, string dateFormat, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);

                var query = _dbContext.SaleInvoiceBookkeepingLogs.Where(p => p.Domain.Id == domainId);
                #region Filter
                if (!string.IsNullOrEmpty(keyword.Trim()))
                {
                    keyword = keyword.Trim().ToLower();
                    query = query.Where(p => p.JournalEntry.Number.ToString().Contains(keyword)
                                || (p.Invoice.Sale.Reference.FullRef).ToLower().Contains(keyword)
                                || (p.Invoice.Reference.FullRef.ToLower().Contains(keyword)));
                }

                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    daterange.ConvertDaterangeFormat(dateTimeFormat, timeZone, out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }

                if (locationId > 0)
                {
                    query = query.Where(p => p.Invoice.Sale.Location.Id == locationId);
                }
                var totalRecord = query.Count();
                #endregion

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreatedBy":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.ForeName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.SurName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "JournalEntry":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "JournalEntry.Number" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Sale":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Invoice.Sale.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Invoice":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Invoice.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                var dataJson = list.Select(s => new
                {
                    CreatedDate = s.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                    CreatedBy = s.CreatedBy.GetFullName(),
                    CreatedById = s.CreatedBy.Id,
                    JournalEntryId = s.JournalEntry?.Id ?? 0,
                    JournalEntryNumber = s.JournalEntry?.Number ?? 0,
                    SaleId = s.Invoice?.Sale?.Id ?? 0,
                    SaleKey = s.Invoice?.Sale?.Key ?? "",
                    SaleRef = s.Invoice?.Sale == null ? "" : (s.Invoice?.Sale?.Reference?.FullRef ?? ("#" + s.Invoice.Sale.Id)),
                    InvoiceId = s.Invoice?.Id ?? 0,
                    InvoiceKey = s.Invoice?.Key ?? "",
                    InvoiceRef = s.Invoice == null ? "" : (s.Invoice?.Reference?.FullRef ?? "#" + s.Invoice.Id)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecord, totalRecord);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }

        }

        public DataTablesResponse GetBkSaleTransferDatas(IDataTablesRequest requestModel, int domainId, string keyword, string daterange, int locationId, string dateTimeFormat, string dateFormat, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);

                var query = _dbContext.SaleTransferBookkeepingLogs.Where(p => p.Domain.Id == domainId);
                #region Filter
                if (!string.IsNullOrEmpty(keyword.Trim()))
                {
                    keyword = keyword.Trim().ToLower();
                    query = query.Where(p => p.JournalEntry.Number.ToString().Contains(keyword)
                                || (p.SaleTransfer.TransferApprovalProcess.Transfer.Any(x => x.Reference.FullRef.ToLower().Contains(keyword))));
                }

                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    daterange.ConvertDaterangeFormat(dateTimeFormat, timeZone, out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }

                if (locationId > 0)
                {
                    query = query.Where(p => p.SaleTransfer.DestinationLocation.Id == locationId
                                || p.SaleTransfer.OriginatingLocation.Id == locationId);
                }

                var totalRecord = query.Count();
                #endregion

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreatedBy":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.ForeName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.SurName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "JournalEntry":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "JournalEntry.Number" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Invoice":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SaleTransferType" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
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

                var dataJson = list.Select(s => new
                {
                    CreatedDate = s.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                    CreatedBy = s.CreatedBy.GetFullName(),
                    CreatedById = s.CreatedBy.Id,
                    JournalEntryId = s.JournalEntry?.Id ?? 0,
                    JournalEntryNumber = s.JournalEntry?.Number ?? 0,
                    TransferId = s.SaleTransfer?.Id ?? 0,
                    TransferKey = s.SaleTransfer?.Key ?? "",
                    TransferRef = s.SaleTransfer?.Reference == null ? ""
                                    : s.SaleTransfer?.Reference?.FullRef,
                    Type = s.SaleTransferType ?? ""
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecord, totalRecord);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, keyword, daterange, dateTimeFormat, timeZone);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }

        }

        #endregion

        public void B2CUiSetting(string page, string userId, B2CSearchQbicleModel searchModel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "B2CUiSetting", userId, null, page, searchModel);

                var uis = new List<UiSetting>();
                #region Filters stored
                var user = _dbContext.QbicleUser.Find(userId);
                uis.Add(new UiSetting() { CurrentPage = page, CurrentUser = user, Key = B2CStoreUiSettingsConst.ORDERBY, Value = searchModel.Orderby.ToString().ToLower() });
                #endregion
                new QbicleRules(_dbContext).StoredUiSettings(uis);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, page, searchModel);
            }
        }

        public PaginationResponse GetBusinessStores(FindBusinessStoresRequest request)
        {
            var response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, request);

                var query = _dbContext.B2BProfiles.Where(s => s.IsDisplayedInB2CListings && s.Domain.Status != QbicleDomain.DomainStatusEnum.Closed);

                if (!string.IsNullOrEmpty(request.keyword))
                {
                    query = query.Where(s => s.BusinessName.ToLower().Contains(request.keyword));
                }
                if (request.limitMyConnections)
                {
                    query = from prf in query
                            join b2c in _dbContext.B2CQbicles on prf.Domain.Id equals b2c.Business.Id
                            into b2c
                            from qb in b2c.DefaultIfEmpty()
                            where qb != null
                            && !qb.IsHidden
                            && qb.Customer.Id == request.currentUserId
                            select prf;
                }
                if (request.AreaOfOperation != "0")
                {
                    query = query.Where(s => s.AreasOperation.Any(a => a.Name == request.AreaOfOperation));
                }
                if (!string.IsNullOrEmpty(request.categoryIds))
                {
                    var catIds = request.categoryIds.Split(',').Select(int.Parse).ToList();
                    query = query.Where(e => e.BusinessCategories.Any(c => catIds.Contains(c.Id)));
                }

                query = query.Distinct();

                var businessLists = query.OrderBy(s => s.BusinessName).ToList();

                var microShops = new List<MicroShop>();
                businessLists.ForEach(s =>
                    {
                        var isPublished = _dbContext.PosMenus.AsNoTracking().Any(e => !e.IsDeleted
                                                                && e.Location.Domain.Id == s.Domain.Id
                                                                && e.SalesChannel == SalesChannelEnum.B2C
                                                                && e.Type == CatalogType.Sales && e.IsPublished);
                        if (isPublished)
                            microShops.Add(new MicroShop
                            {
                                LogoUri = s.LogoUri.ToUriString(),
                                DomainId = s.Domain.Id,
                                DomainKey = s.Domain.Key,
                                BusinessName = s.BusinessName,
                                BusinessSummary = s.BusinessSummary,
                                Categories = s.BusinessCategories.Select(c => c.Name).OrderBy(e => e).ToArray(),
                                Locations = s.AreasOperation.Select(c => c.Name).ToArray(),
                                Tags = s.Tags.Select(c => c.TagName).OrderBy(o => o).ToArray(),
                            });
                    });


                response.totalNumber = microShops.Count;
                response.totalPage = response.totalNumber / request.pageSize + 1;

                response.items = microShops.Skip((request.pageNumber - 1) * request.pageSize).Take(request.pageSize);

                return response;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                return response;
            }
        }

        public PaginationResponse GetFeaturedBusinessStores(FindBusinessStoresRequest request)
        {
            var response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, request);

                var listStore = from featuredStore in _dbContext.FeaturedStores
                                join b2bProfile in _dbContext.B2BProfiles on featuredStore.Domain.Id equals b2bProfile.Domain.Id
                                where b2bProfile.IsDisplayedInB2CListings
                                && b2bProfile.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
                                && b2bProfile.DefaultB2CRelationshipManagers.Any()
                                select b2bProfile;

                // Searching feature will be done in the full public Stores initially
                if (request.IsAllPublicStoreShown || !string.IsNullOrEmpty(request.keyword))
                {
                    listStore = from b2bProfile in _dbContext.B2BProfiles
                                where b2bProfile.IsDisplayedInB2CListings
                                        && b2bProfile.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
                                        && b2bProfile.DefaultB2CRelationshipManagers.Any()
                                select b2bProfile;
                }

                var listLocationPublishCatalog = _dbContext.PosMenus.Where(e => e.IsPublished).Select(t => t.Location.Id).ToList();
                var query = listStore.Where(e => e.Domain.TraderLocations.Any(t => listLocationPublishCatalog.Contains(t.Id)));

                if (!string.IsNullOrEmpty(request.keyword))
                {
                    query = query.Where(s => s.BusinessName.ToLower().Contains(request.keyword));
                }
                if (request.limitMyConnections)
                {
                    query = from prf in query
                            join b2c in _dbContext.B2CQbicles on prf.Domain.Id equals b2c.Business.Id
                            into b2c
                            from qb in b2c.DefaultIfEmpty()
                            where qb != null
                            && !qb.IsHidden
                            && qb.Customer.Id == request.currentUserId
                            select prf;
                }
                if (request.AreaOfOperation != "0")
                {
                    query = query.Where(s => s.AreasOperation.Any(a => a.Name == request.AreaOfOperation));
                }
                if(request.categoryIds != null)
                {
                var catIds = request.categoryIds.Split(',').Select(int.Parse).ToList();
                query = query.Where(e => e.BusinessCategories.Any(c => catIds.Contains(c.Id)));
                }

                query = query.Distinct();

                response.totalNumber = query.Count();

                var businessList = query.OrderBy(s => s.BusinessName)
                    .Skip((request.pageNumber - 1) * request.pageSize)
                    .Take(request.pageSize).ToList();

                var shops = businessList.Select(s => new MicroShop
                {
                    LogoUri = s.LogoUri.ToUriString(),
                    DomainId = s.Domain.Id,
                    DomainKey = s.Domain.Key,
                    BusinessName = s.BusinessName,
                    BusinessSummary = s.BusinessSummary,
                    Categories = s.BusinessCategories.Select(c => c.Name).OrderBy(e => e).ToArray(),
                    Locations = s.AreasOperation.Select(c => c.Name).ToArray(),
                    Tags = s.Tags.Select(c => c.TagName).OrderBy(e => e).ToArray(),
                });

                response.items = shops;
                return response;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                return response;
            }
        }


        public List<Catalog> GetCatalogsByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return _dbContext.PosMenus.Where(s => !s.IsDeleted && s.Location.Domain.Id == domainId && s.SalesChannel == SalesChannelEnum.B2C && s.Type == CatalogType.Sales).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<Catalog>();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainId"></param>
        /// <param name="isLoadAll">true get all, false get where IsPublished = true</param>
        /// <returns></returns>
        public List<CatalogListCustomModel> GetListCatalogViewModelByDomainId(int domainId, bool isLoadAll)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                var lstCatalogs = _dbContext.PosMenus.AsNoTracking().Where(s => !s.IsDeleted
                                                                && s.Location.Domain.Id == domainId
                                                                && s.SalesChannel == SalesChannelEnum.B2C
                                                                && s.Type == CatalogType.Sales);

                if (!isLoadAll)
                    lstCatalogs = lstCatalogs.Where(e => e.IsPublished);

                var lstCataCustomizedCatalogs = new List<CatalogListCustomModel>();
                lstCatalogs.ToList().ForEach(cataItem =>
                {
                    //var lstIds = cataItem.Categories.Select(x => x.Id).ToList();
                    ////TODO: QBIC-3927 Customers now can see the non-inventory items (additional service items)            
                    //var query = from citem in _dbContext.PosCategoryItems
                    //            where lstIds.Contains(citem.Category.Id)
                    //            && citem.PosVariants.Any()
                    //            select citem;
                    var categoryItems = _dbContext.PosMenus.Where(p => p.Id == cataItem.Id).SelectMany(x => x.Categories).SelectMany(x => x.PosCategoryItems).Distinct().Count();

                    var catalogItemImage = string.IsNullOrEmpty(cataItem.Image) ? ConfigManager.CatalogDefaultImage : cataItem.Image;
                    lstCataCustomizedCatalogs.Add(new CatalogListCustomModel()
                    {
                        Id = cataItem.Id,
                        Key = cataItem.Key,
                        Description = cataItem.Description,
                        Name = cataItem.Name,
                        ImageUri = catalogItemImage.ToUriString(),
                        ItemNum = categoryItems,//query.Count(),
                        Location = cataItem.Location,
                        IsPublished = cataItem.IsPublished
                    });
                });
                return lstCataCustomizedCatalogs;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<CatalogListCustomModel>();
            }
        }

        /// <summary>
        /// Create a new comment to B2C Dicussion Order, and send notification if the order begin process
        /// </summary>
        /// <param name="isCreatorTheCustomer">connection id, using for case a user log in to multi app</param>
        /// <param name="message"></param>
        /// <param name="disId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="qbicleId"></param>
        /// <param name="originatingCreationId"></param>
        public void B2CDicussionOrderSendMessage(bool isCreatorTheCustomer, string message, int disId, string currentUserId, int qbicleId, string originatingCreationId, bool orderBeingProcessed = false)
        {
            var post = new PostsRules(_dbContext).SavePost(isCreatorTheCustomer, message, currentUserId, qbicleId);
            if (post != null)
                new DiscussionsRules(_dbContext).AddComment(isCreatorTheCustomer, disId.Encrypt(), post, originatingCreationId, Notification.ApplicationPageName.DiscussionOrder, true);

            if (!orderBeingProcessed)
                return;

            var discussionOrder = new DiscussionsRules(_dbContext).GetB2CDiscussionOrderByDiscussionId(disId);
            new NotificationRules(_dbContext).SignalRB2COrderProcess(discussionOrder.TradeOrder, currentUserId, Notification.NotificationEventEnum.B2COrderBeginProcess);

        }
        /// <summary>
        /// Get discussion order
        /// </summary>
        /// <param name="discussionId"></param>
        /// <param name="currentUser"></param>
        /// <param name="creatorTheCustomer"></param>
        /// <param name="discussion"></param>
        /// <returns></returns>
        public DiscusionOrder DiscussionOrderBy(int discussionId, UserSetting currentUser, string creatorTheCustomer, B2COrderCreation discussion = null)
        {
            if (discussion == null)
                discussion = new DiscussionsRules(_dbContext).GetB2CDiscussionOrderByDiscussionId(discussionId);
            var b2cqbicle = discussion.Qbicle as B2CQbicle;

            var isDomainAdmin = b2cqbicle.Business.Administrators.Any(p => p.Id == currentUser.Id);
            var isMemberOfDomain = b2cqbicle.Business.Users.Any(p => p.Id == currentUser.Id);
            var isMemberOfQbicle = b2cqbicle.Members.Any(p => p.Id == currentUser.Id);
            var isCustomerOfBusiness = b2cqbicle.Customer.Id == currentUser.Id;

            // If the current user is the customer of the discussion,
            // and the gobackpage must not be B2C, as if user can access the order via B2C page, the user is considered as Business's member
            if (isCustomerOfBusiness && creatorTheCustomer != SystemPageConst.B2C)
            {
                return new DiscusionOrder { Type = DiscusionOrderBy.Customer, DisplayName = currentUser.DisplayName };
            }
            // else, only if the current user must be the member/admin of the discussion
            // or, currnet user is the order customer, and current gobackpage is B2C page (the user is considered as Business's member then)
            // and, the information of the business will be returned instead of the individual
            else if ((isDomainAdmin || (isMemberOfDomain && isMemberOfQbicle)) &&
                (!isCustomerOfBusiness || (creatorTheCustomer == SystemPageConst.B2C && isCustomerOfBusiness)))
            {
                return new DiscusionOrder { Type = DiscusionOrderBy.Business, DisplayName = discussion.TradeOrder.SellingDomain.Id.BusinesProfile().BusinessName };
            }

            return new DiscusionOrder { Type = DiscusionOrderBy.Customer, DisplayName = currentUser.DisplayName };
        }

        public B2CConnectionTypeNumber GetB2CConnectionTypeNum(string currentUserId, int currentDomainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, currentUserId, currentDomainId);
                var domain = _dbContext.Domains.Find(currentDomainId);
                var isDomainAdmin = domain?.Administrators.Any(p => p.Id == currentUserId) ?? false;
                var query = from b2c in _dbContext.B2CQbicles
                            where !b2c.IsHidden
                            && b2c.Business.Id == currentDomainId
                            && (isDomainAdmin || b2c.Members.Any(s => s.Id == currentUserId))
                            select b2c;
                var ExcludeConnectionNum = query.Where(p => p.Status != CommsStatus.Blocked).Count();
                var NewConnectionNum = query.Where(p => p.IsNewContact == true).Count();
                var BlockedConnectionNum = query.Where(p => p.Status == CommsStatus.Blocked).Count();

                return new B2CConnectionTypeNumber()
                {
                    NonBlockedConnectionNumber = ExcludeConnectionNum,
                    BlockedConnectionNumber = BlockedConnectionNum,
                    NewConnectionNumber = NewConnectionNum
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId, currentDomainId);
                return new B2CConnectionTypeNumber()
                {
                    BlockedConnectionNumber = 0,
                    NewConnectionNumber = 0,
                    NonBlockedConnectionNumber = 0
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tradeOrderId"></param>
        /// <param name="payment"></param>
        /// <param name="isCutomer"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReturnJsonModel CreateB2CPayment(string tradeOrderId, CashAccountTransaction payment, bool isCutomer, string userId)
        {
            try
            {
                var tId = tradeOrderId.Decrypt2Int();
                var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(e => e.Id == tId);

                //var iId = 0;
                //if (invoiceId != "")
                //    iId = invoiceId.Decrypt2Int();
                //else
                //    iId = tradeOrder.Invoice.Id;
                var invoice = tradeOrder.Invoice;// _dbContext.Invoices.FirstOrDefault(e => e.Id == iId);
                
                
                //validate Payment before executing 
                var leftAmout = (invoice?.TotalInvoiceAmount ?? 0) - tradeOrder.Payments.Sum(e => e.Amount);
                var isPaymentApproval = false;
                var statusPayment = TraderPaymentStatusEnum.PaymentDiscarded;
                var requestStatusPayment = ApprovalReq.RequestStatusEnum.Discarded;

                if (payment.Amount < 0 || leftAmout <=0 || leftAmout < payment.Amount) 
                {
                    isPaymentApproval = false;
                    statusPayment = TraderPaymentStatusEnum.PaymentDiscarded;
                    requestStatusPayment = ApprovalReq.RequestStatusEnum.Discarded;
                }
                else
                {
                    isPaymentApproval = true;
                    statusPayment = TraderPaymentStatusEnum.PaymentApproved;
                    requestStatusPayment = ApprovalReq.RequestStatusEnum.Approved;
                }

                var user = _dbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);
                var paymentB2C = new CashAccountTransaction
                {
                    CreatedDate = DateTime.UtcNow,
                    Amount = payment.Amount,
                    AssociatedInvoice = invoice,
                    CreatedBy = user,
                    Status = statusPayment,
                    Description = $"Payment for B2C Order {tradeOrder.SalesChannel.GetDescription()} #{invoice.Reference?.FullRef}",
                    Workgroup = tradeOrder.PaymentWorkGroup,
                    Type = CashAccountTransactionTypeEnum.PaymentIn,
                    DestinationAccount = tradeOrder.PaymentAccount,
                    Reference = $"{PaymentReferenceConst.B2CCashPaymentReferenceString} {payment.Reference} {tradeOrder.OrderReference.FullRef}",
                    Contact = invoice.Sale?.Purchaser,
                    Charges = 0,
                    AssociatedSale = invoice.Sale,
                    PaymentMethod = _dbContext.PaymentMethods.FirstOrDefault(e => e.Name == payment.PaymentMethod.Name)
                };

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), $"B2C Order CashAccountTransaction Payment creating ", user.Id, paymentB2C);

                tradeOrder.PaymentWorkGroup.Qbicle.LastUpdated = DateTime.UtcNow;

                var approverId = new ProcessOrderRules(_dbContext).GetOrderFromTradeOrder(tradeOrder).Cashier.TraderId;
                var approver = _dbContext.QbicleUser.FirstOrDefault(e => e.Id == approverId);

                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = _dbContext.PaymentApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == tradeOrder.PaymentWorkGroup.Id),
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = requestStatusPayment,
                    Qbicle = tradeOrder.PaymentWorkGroup.Qbicle,
                    Topic = tradeOrder.PaymentWorkGroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    ApprovedOrDeniedAppBy = approver,
                    StartedBy = approver,
                    StartedDate = paymentB2C.CreatedDate,
                    TimeLineDate = paymentB2C.CreatedDate,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader,
                    Name = $"Approval Payment for B2C Order #{paymentB2C.Reference}",
                    Payments = new List<CashAccountTransaction> { paymentB2C }
                };

                paymentB2C.PaymentApprovalProcess = approval;
                approval.ActivityMembers.AddRange(tradeOrder.PaymentWorkGroup.Members);


                _dbContext.CashAccountTransactions.Add(paymentB2C);
                _dbContext.Entry(paymentB2C).State = EntityState.Added;

                var paymentB2CLog = new CashAccountTransactionLog
                {
                    CreatedDate = DateTime.UtcNow,
                    Id = 0,
                    CreatedBy = user,
                    Status = paymentB2C.Status,
                    Description = paymentB2C.Description,
                    Type = paymentB2C.Type,
                    AssociatedInvoice = paymentB2C.AssociatedInvoice,
                    Workgroup = tradeOrder.PaymentWorkGroup,
                    DestinationAccount = paymentB2C.DestinationAccount,
                    OriginatingAccount = paymentB2C.OriginatingAccount,
                    AssociatedFiles = paymentB2C.AssociatedFiles,
                    Amount = paymentB2C.Amount,
                    AssociatedSale = paymentB2C.AssociatedSale,
                    AssociatedPurchase = paymentB2C.AssociatedPurchase,
                    PaymentApprovalProcess = paymentB2C.PaymentApprovalProcess,
                    Contact = paymentB2C.Contact,
                    AssociatedTransaction = paymentB2C,
                    AssociatedBKTransaction = paymentB2C.AssociatedBKTransaction,
                    Charges = paymentB2C.Charges,
                    Reference = paymentB2C.Reference,
                    PaymentMethod = paymentB2C.PaymentMethod
                };

                var wasteProcessB2CLog = new PaymentProcessLog
                {
                    AssociatedTransaction = paymentB2C,
                    AssociatedCashAccountTransactionLog = paymentB2CLog,
                    PaymentStatus = paymentB2C.Status,
                    CreatedBy = user,
                    CreatedDate = DateTime.UtcNow
                };

                _dbContext.PaymentProcessLogs.Add(wasteProcessB2CLog);
                _dbContext.Entry(wasteProcessB2CLog).State = EntityState.Added;

                tradeOrder.Payments.Add(paymentB2C);

                _dbContext.SaveChanges();


                var loggingRules = new TradeOrderLoggingRules(_dbContext);
                if (!isCutomer)
                    loggingRules.TradeOrderLogging(TradeOrderLoggingType.PaymentAdd, paymentB2C.Id);
                else
                    loggingRules.TradeOrderLogging2PaymentAdd(TradeOrderLoggingType.PaymentAdd, paymentB2C.Id, isCutomer);

                loggingRules.TradeOrderLogging(TradeOrderLoggingType.PaymentApproval, paymentB2C.Id, approval.GetCreatedBy().Id);

                
                if (isPaymentApproval)
                {
                //Get AssociatedPrepOrders and set them paid
                var associatedPrepOrders = _dbContext.QueueOrders.Where(q => q.LinkedOrderId == tradeOrder.LinkedOrderId).ToList();
                associatedPrepOrders.ForEach(qOrder => { qOrder.IsPaid = true; });
                _dbContext.SaveChanges();

                new BookkeepingIntegrationRules(_dbContext).AddPaymentJournalEntry(user, paymentB2C);

                new NotificationRules(_dbContext).SignalRB2COrderProcess(tradeOrder, "", NotificationEventEnum.B2COrderPaymentApproved);

                //If the payment uses the Store Credit payment method then decrease the StoreCredit
                if (paymentB2C.PaymentMethod.Name == PaymentMethodNameConst.StoreCredit)
                    new TraderEventRules(_dbContext).DecreaseStoreCreditFromPaymentApproved(paymentB2C.Id);

                //Hangfire to generate store points from the payment approved
                if (paymentB2C.Status == TraderPaymentStatusEnum.PaymentApproved)
                    new TraderEventRules(_dbContext).GenerateStorePoinFromPaymentApproved(paymentB2C.Id);
                
                }
                if (payment.Amount < 0)
                {
                    return new ReturnJsonModel { result = false, msg = "Amount to pay must be positive", Object = paymentB2C };
                }
                if (leftAmout <= 0)
                {
                    return new ReturnJsonModel { result = false, msg = "Paid out, can not create a new one", Object = paymentB2C };
                }
                if (leftAmout < payment.Amount)
                {
                    return new ReturnJsonModel { result = false, msg = "Amount to pay is too much", Object = paymentB2C };
                }

                return new ReturnJsonModel { result = true, Object = paymentB2C };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrderId);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        #region B2COrder processing on Order

        public TradeOrder GetTradeOrderById(int orderId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, orderId);
                return _dbContext.TradeOrders.Find(orderId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, orderId);
                return new TradeOrder();
            }
        }
        public void B2BDicussionOrderSendMessage(bool isCreatorTheCustomer, string message, int disId, string currentUserId, int qbicleId, string originatingCreationId, bool orderBeingProcessed = false)
        {
            var post = new PostsRules(_dbContext).SavePost(isCreatorTheCustomer, message, currentUserId, qbicleId);
            if (post != null)
                new DiscussionsRules(_dbContext).AddComment(isCreatorTheCustomer, disId.Encrypt(), post, originatingCreationId, Notification.ApplicationPageName.DiscussionOrder, true);

            if (!orderBeingProcessed)
                return;

            var discussionOrder = new DiscussionsRules(_dbContext).GetDiscussionByB2BOrderById(disId);
            new NotificationRules(_dbContext).SignalRB2COrderProcess(discussionOrder.TradeOrder, currentUserId, Notification.NotificationEventEnum.B2COrderBeginProcess);

        }

        public ReturnJsonModel SetOrderAcceptedByCustomer(B2CCustomerAcceptedInfo acceptedInfo)
        {
            var resultJson = new ReturnJsonModel() { actionVal = 2, result = false };
            try
            {
                var discussion = _dbContext.B2COrderCreations.Find(acceptedInfo.disId);
                var tradeOrder = discussion.TradeOrder;
                if (tradeOrder != null)
                {
                    if (!string.IsNullOrEmpty(acceptedInfo.note) && acceptedInfo.note.Length > 500)
                    {
                        resultJson.msg = ResourcesManager._L("ERROR_MSG_MAXNUMBERCHARACTORS", 500);
                        return resultJson;
                    }

                    //var _order = JsonHelper.ParseAs<Order>(tradeOrder.OrderJson);
                    var orderJson = tradeOrder.OrderJson;

                    var order = orderJson.ParseAs<Order>();
                    order.Notes = acceptedInfo.note;


                    if (acceptedInfo.method == 0 || acceptedInfo.method == 1)
                    {
                        if (acceptedInfo.method == 0)
                        {
                            tradeOrder.IsDeliveriedToMe = true;
                        }
                        else
                        {
                            tradeOrder.IsDeliveriedToMe = false;
                        }

                        tradeOrder.DeliveryMethod = DeliveryMethodEnum.Delivery;
                        order.Classification = OrderTypeClassification.HomeDelivery.GetId();
                        order.Type = "Delivery";

                        if (acceptedInfo.delivery > 0)
                        {
                            if (order.Customer == null)
                                order.Customer = new Models.TraderApi.Customer();
                            var userAddress = _dbContext.TraderAddress.Find(acceptedInfo.delivery);
                            if (userAddress != null)
                            {
                                order.Customer.Address = new Address
                                {
                                    AddressLine1 = userAddress.AddressLine1,
                                    AddressLine2 = userAddress.AddressLine2,
                                    City = userAddress.City,
                                    Country = userAddress.Country.ToString(),
                                    Latitude = userAddress.Latitude,
                                    Longitude = userAddress.Longitude,
                                    Postcode = userAddress.PostCode,
                                    AssociatedAddressId = userAddress.Id
                                };
                                if (!string.IsNullOrEmpty(userAddress.Phone))
                                    order.Customer.Phone = userAddress.Phone;
                                if (!string.IsNullOrEmpty(userAddress.Email))
                                    order.Customer.Email = userAddress.Email;
                            }

                            if (acceptedInfo.method == 1)
                            {
                                if (!string.IsNullOrEmpty(acceptedInfo.someoneName))
                                    order.Customer.Name = acceptedInfo.someoneName;
                                else
                                {
                                    resultJson.msg = "ERROR_MSG_181";
                                    return resultJson;
                                }
                            }
                            else
                            {
                                order.Customer.Name = tradeOrder.Customer.GetFullName();
                            }

                        }
                        else
                        {
                            resultJson.msg = "ERROR_MSG_181";
                            return resultJson;
                        }
                    }
                    else if (acceptedInfo.method == 2)
                    {
                        tradeOrder.DeliveryMethod = DeliveryMethodEnum.CustomerPickup;
                        order.Type = DeliveryMethodEnum.CustomerPickup.GetDescription();
                        order.Classification = OrderTypeClassification.EatIn.GetId();
                    }

                    order.Delivery = tradeOrder.DeliveryMethod.GetId();
                    tradeOrder.IsAgreedByCustomer = true;
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.AwaitingProcessing;
                    tradeOrder.OrderJson = order.ToJson();

                    var orderOrg = tradeOrder.OrderJsonOrig.ParseAs<Order>();
                    orderOrg.Customer = order.Customer;
                    orderOrg.Notes = order.Notes;
                    tradeOrder.OrderJsonOrig = orderOrg.ToJson();
                    //CalculationOrderJson(tradeOrder, order);
                }
                _dbContext.Entry(discussion).State = EntityState.Modified;
                _dbContext.SaveChanges();
                resultJson.result = true;
                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, acceptedInfo);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }

        public ReturnJsonModel SetOrderAcceptedByBusiness(int discussionId)
        {
            var resultJson = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                var discussion = _dbContext.B2COrderCreations.Find(discussionId);
                discussion.TradeOrder.IsAgreedByBusiness = true;
                _dbContext.Entry(discussion).State = EntityState.Modified;
                _dbContext.SaveChanges();
                resultJson.result = true;
                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussionId);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }

        public List<B2COrderItemModel> GetB2COrderItemsPagination(int discussionId, string keySearch, IDataTablesRequest requestModel, ref int totalRecord, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, discussionId, keySearch, requestModel, totalRecord, start, length);

                var resultList = new List<B2COrderItemModel>();
                var b2cOrderDiscussion = _dbContext.B2COrderCreations.FirstOrDefault(s => s.Id == discussionId);
                var b2cqbicle = b2cOrderDiscussion.Qbicle as B2CQbicle;
                var currencySetting = new CurrencySettingRules(_dbContext).GetCurrencySettingByDomain(b2cqbicle.Business.Id);

                var order = (b2cOrderDiscussion.TradeOrder?.OrderJson ?? "").ParseAs<Order>();
                if (order == null || order.Items == null || order.Items.Count <= 0)
                    return new List<B2COrderItemModel>();

                var query = from item in order.Items select item;

                #region filter
                if (!string.IsNullOrEmpty(keySearch))
                {
                    query = query.Where(p => p.Name.ToLower().Contains(keySearch.ToLower()));
                }
                #endregion

                totalRecord = query.Skip(start).Take(length).Count();

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "ItemName":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            query = query.OrderBy(string.IsNullOrEmpty(orderString) ? "Name asc" : orderString);
                            break;
                        case "Quantity":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Quantity" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            query = query.OrderBy(string.IsNullOrEmpty(orderString) ? "Name asc" : orderString);
                            break;
                        case "CategoryName":
                            if (column.SortDirection == TB_Column.OrderDirection.Ascendant)
                            {
                                query = from orderItem in query
                                        join posItem in _dbContext.PosCategoryItems on orderItem.TraderId equals posItem.Id
                                        orderby posItem.Category.Name ascending
                                        select orderItem;
                            }
                            else
                            {
                                query = from orderItem in query
                                        join posItem in _dbContext.PosCategoryItems on orderItem.TraderId equals posItem.Id
                                        orderby posItem.Category.Name descending
                                        select orderItem;
                            }
                            break;
                    }
                }

                #endregion

                #region Paging
                var orderItemList = query.ToList();
                #endregion
                var tradeOrder = b2cOrderDiscussion.TradeOrder;
                var _isAllowEdit = true;
                if (tradeOrder != null && tradeOrder.IsAgreedByBusiness && tradeOrder.IsAgreedByCustomer)
                {
                    _isAllowEdit = false;
                }
                var disabledPrice = order.VoucherId == 0 ? "" : "disabled";

                var voucher = new Models.Loyalty.Voucher();
                if (order.VoucherId > 0)
                    voucher = _dbContext.Vouchers.FirstOrDefault(v => v.Id == order.VoucherId);
                orderItemList.ForEach(p =>
                {
                    var orderItemModel = new B2COrderItemModel()
                    {
                        ItemId = p.Id,
                        ItemName = p.Name,
                        Quantity = p.Quantity,
                        TotalPrice = order.AmountInclTax,
                        //TotalPrice = totalPrice,
                        IsAllowEdit = _isAllowEdit,
                        //Variant = new Models.TraderApi.Variant(),
                        //Extras = new List<Models.TraderApi.Variant>()
                        Variant = p.Variant,
                        Extras = p.Extras,
                        VoucherId = order.VoucherId,
                        VoucherName = voucher?.Promotion?.Name ?? "",
                        VoucherCode = voucher?.Code ?? ""
                    };


                    var posItem = _dbContext.PosCategoryItems.Find(p.TraderId);
                    orderItemModel.CategoryName = posItem?.Category?.Name ?? "";
                    orderItemModel.CatItemHasMultipleVariants = posItem?.PosVariants != null && posItem?.PosVariants.Count > 1;

                    orderItemModel.Discount = p.Variant.Discount;

                    // For setting tooltip text
                    var defaultPosVariant = posItem.PosVariants.FirstOrDefault(x => x.IsDefault);
                    var associatedTraderItem = defaultPosVariant?.TraderItem ?? null;
                    orderItemModel.SourceName = associatedTraderItem?.Name == p.Name ? "" : (associatedTraderItem?.Name ?? "");

                    orderItemModel.Price += p.Variant.TotalAmount + p.Extras.Sum(e => e.TotalAmount);

                    orderItemModel.PriceWithoutDiscount = p.Variant.TotalDiscount + p.Extras.Sum(e => e.TotalDiscount);

                    //Taxes List Info
                    orderItemModel.Taxes = new List<Tax>();
                    p.Variant.Taxes.ForEach(t =>
                    {
                        t.AmountTax *= p.Quantity;
                        if (orderItemModel.Taxes.Any(x => x.TraderId == t.TraderId))
                        {
                            var taxitem = orderItemModel.Taxes.FirstOrDefault(x => x.TraderId == t.TraderId);
                            taxitem.AmountTax += t.AmountTax;
                        }
                        else
                        {
                            orderItemModel.Taxes.Add(t);
                        }
                    });

                    p.Extras.ForEach(extraItem =>
                    {
                        extraItem.Taxes.ForEach(t =>
                        {
                            t.AmountTax *= p.Quantity;
                            if (orderItemModel.Taxes.Any(x => x.TraderId == t.TraderId))
                            {
                                var taxitem = orderItemModel.Taxes.FirstOrDefault(x => x.TraderId == t.TraderId);
                                taxitem.AmountTax += t.AmountTax;
                            }
                            else
                            {
                                orderItemModel.Taxes.Add(t);
                            }
                        });
                    });

                    // InitialPrice is the Net Price of the item when the item is added to the order
                    // It should not include discount
                    decimal initialTaxAmount = 0;
                    if (p.Variant.Discount != 100)
                    {
                        initialTaxAmount = (orderItemModel.Taxes?.Sum(x => x.AmountTax) ?? 0)
                                             * (1 / (1 - p.Variant.Discount / 100));
                    }
                    orderItemModel.ItemInitialPrice = (orderItemModel.Price +
                        orderItemModel.PriceWithoutDiscount - initialTaxAmount) / p.Quantity;
                    // END: Initial Price calculation

                    orderItemModel.TaxInfo = "";
                    if (orderItemModel.Taxes == null || orderItemModel.Taxes.Count <= 0)
                    {
                        orderItemModel.TaxInfo = "--";
                    }
                    else
                    {
                        var htmlString = "";
                        htmlString += "<ul id='taxes" + p.Id + "' class='unstyled'>";
                        foreach (var taxitem in orderItemModel.Taxes)
                        {

                            htmlString += "<li>";
                            htmlString += currencySetting.CurrencySymbol + taxitem.AmountTax.ToDecimalPlace(currencySetting);
                            htmlString += "<small><i>(";
                            htmlString += taxitem.TaxName;
                            htmlString += ")</i></small></li>";

                        }
                        htmlString += "</ul>";
                        orderItemModel.TaxInfo = htmlString;
                    }

                    //Price html String
                    var priceString = $"<input {disabledPrice} itemId='" + p.Id + "' " + (_isAllowEdit ? "" : "disabled") + " type='number' id='itemprice" + p.Id + "' class='form-control itemprice" + p.Id + "' value=\'" + orderItemModel.Price.ToDecimalPlace(currencySetting).Replace(",", "") + "\'/>";
                    priceString += "<input type='hidden' value='" + orderItemModel.PriceWithoutDiscount + "' id='pureprice" + p.Id + "'/>";
                    priceString += "<input type='hidden' value='" + (orderItemModel.PriceWithoutDiscount + orderItemModel.Price) + "' id='totalprice" + p.Id + "'/>";
                    orderItemModel.PriceString = priceString;

                    resultList.Add(orderItemModel);
                });
                return resultList.OrderBy(p => p.ItemName).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussionId, keySearch, requestModel, totalRecord, start, length);
                return new List<B2COrderItemModel>();
            }
        }

        public DataTablesResponse GetB2COrders(
            IDataTablesRequest requestModel, string keyword, int locationId, int contactId, string datetime, int channel,
            B2CFilterInvoiceType filterInvoice, B2CFilterPaymentType filterPayment, B2CFilterDeliveryType filterDelivery,
            UserSetting dateTimeFormat, int domainId, bool isSaleOrderShow)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, keyword, contactId, locationId, datetime, filterInvoice, filterPayment, filterDelivery, dateTimeFormat, domainId);
                var currencySettings = new CurrencySettingRules(_dbContext).GetCurrencySettingByDomain(domainId);
                var query = _dbContext.TradeOrders.Where(s => s is TradeOrderB2B && s.BuyingDomain.Id == domainId);
                if (isSaleOrderShow)
                    query = _dbContext.TradeOrders.Where(s => s.SellingDomain.Id == domainId);
                int totalRecords;

                #region Filter
                //B2C orders
                if (isSaleOrderShow)
                {
                    if (channel > 0)
                    {
                        query = query.Where(s => s.SalesChannel == (SalesChannelEnum)channel);
                    }
                    if (locationId > 0)
                    {
                        query = query.Where(s => s.Location.Id == locationId);
                    }
                    if (contactId > 0)
                    {
                        query = query.Where(s => s.TraderContact.Id == contactId);
                    }
                    if (!string.IsNullOrEmpty(keyword))
                        query = query.Where(q => q.OrderReference.FullRef.Contains(keyword)
                        || q.Location.Name.Contains(keyword)
                        || (q.Customer.Forename + " " + q.Customer.Surname).Contains(keyword)
                        || (q.Sale != null && q.Sale.Reference.FullRef.Contains(keyword))
                        || (q.Invoice != null && q.Invoice.Reference.FullRef.Contains(keyword))
                        || (q.Transfer != null && q.Transfer.Reference.FullRef.Contains(keyword))
                        || (q.Payments.Any(s => s.Reference.Contains(keyword)))
                        );

                    if (!string.IsNullOrEmpty(datetime))
                    {
                        var startDate = new DateTime();
                        var endDate = new DateTime();
                        var tz = TimeZoneInfo.FindSystemTimeZoneById(dateTimeFormat.Timezone);
                        datetime.ConvertDaterangeFormat(dateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                        startDate = startDate.ConvertTimeToUtc(tz);
                        endDate = endDate.ConvertTimeToUtc(tz);
                        query = query.Where(s => (s.CreateDate >= startDate && s.CreateDate < endDate) || (s.Sale.CreatedDate >= startDate && s.Sale.CreatedDate < endDate));
                    }
                    if (filterInvoice > 0)
                    {
                        if (filterInvoice == B2CFilterInvoiceType.NotProvided)
                            query = query.Where(s => s.Invoice == null);
                        else if (filterInvoice == B2CFilterInvoiceType.Provided)
                            query = query.Where(s => s.Invoice != null);
                    }
                    if (filterPayment > 0)
                    {
                        //need confirm business rule
                        if (filterPayment == B2CFilterPaymentType.NotPaid)
                            query = query.Where(s => !s.Payments.Any());
                        else if (filterPayment == B2CFilterPaymentType.PaidInPart)
                            query = query.Where(s => s.Payments.Any(p => p.Status == TraderPaymentStatusEnum.PaymentApproved)
                            && (s.Sale.SaleTotal > s.Payments.Where(p => p.Type == CashAccountTransactionTypeEnum.PaymentIn && p.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(p => p.Amount)));
                        else if (filterPayment == B2CFilterPaymentType.PaidInFull)
                            query = query.Where(s => s.Sale.SaleTotal <= s.Payments.Where(p => p.Type == CashAccountTransactionTypeEnum.PaymentIn && p.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(p => p.Amount));
                    }
                    if (filterDelivery > 0)
                    {
                        if (filterDelivery == B2CFilterDeliveryType.NotAssigned)
                            query = query.Where(s => s.Transfer == null);
                        else if (filterDelivery == B2CFilterDeliveryType.InProgress)
                            query = query.Where(s => s.Transfer != null
                            && (s.Transfer.Status != TransferStatus.Delivered
                            || s.Transfer.Status != TransferStatus.Denied
                            || s.Transfer.Status != TransferStatus.Discarded));
                        else if (filterDelivery == B2CFilterDeliveryType.Delivered)
                            query = query.Where(s => s.Transfer != null && s.Transfer.Status == TransferStatus.Delivered);
                    }
                }
                //B2B orders
                else
                {
                    query = GetB2BOrders(keyword, locationId, contactId, datetime, filterInvoice, filterPayment, filterDelivery, dateTimeFormat, domainId);
                }
                totalRecords = query.Count();
                #endregion

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Ref":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "OrderReference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Location":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Location.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Channel":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SalesChannel" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Contact":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Customer.Forename" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Customer.Surname" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Total":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Sale.SaleTotal" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "OrderStatus" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Sale":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Sale.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreateDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Invoice":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Invoice.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Transfer":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Transfer.Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Id desc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "Id desc" : orderByString);
                #endregion

                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion

                if (isSaleOrderShow)
                {
                    var dataJson = list.Select(s => new BusinessOrderModel
                    {
                        Id = s.Id,
                        Ref = s.OrderReference.FullRef,
                        Location = s.Location.Name,
                        Channel = s.SalesChannel.ToString(),
                        Contact = s.TraderContact?.Name ?? "",
                        ContactId = s.TraderContact?.Id ?? 0,
                        Total = s.Sale?.SaleTotal.ToDecimalPlace(currencySettings) ?? ((decimal)0).ToDecimalPlace(currencySettings),
                        Status = s.OrderStatus.ToString(),
                        OrderProblem = s.OrderProblem.ToString(),
                        Sale = s.Sale?.Reference?.FullRef ?? "",
                        SaleId = s.Sale?.Id ?? 0,
                        SaleKey = s.Sale?.Key ?? "",
                        SaleStatus = s.Sale?.Status.ToString() ?? "",
                        Invoice = s.Invoice?.Reference?.FullRef ?? "",
                        InvoiceId = s.Invoice?.Id ?? 0,
                        InvoiceKey = s.Invoice?.Key ?? "",
                        InvoiceStatus = s.Invoice?.Status.ToString() ?? "",
                        Payments = s.Payments.Select(p =>
                               new PaymentReportModel
                               {
                                   Ref = (!string.IsNullOrEmpty(p.Reference) ? p.Reference : p.Id.ToString()),
                                   Id = p.Id,
                                   Status = p.Status.ToString()
                               }).ToList(),
                        Transfer = s.Transfer?.Reference?.FullRef ?? "",
                        TransferId = s.Transfer?.Id ?? 0,
                        TransferKey = s.Transfer?.Key ?? "",
                        TransferStatus = s.Transfer?.Status.ToString() ?? "",
                        CreatedDate = s.CreateDate.ConvertTimeFromUtc(dateTimeFormat.Timezone).ToString(dateTimeFormat.DateTimeFormat)
                    }).ToList();
                    return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
                }
                else
                {

                    var purchaseJson = new List<BusinessOrderModel>();
                    list.ForEach(s =>
                    {
                        var tradeOrderB2B = s as TradeOrderB2B;
                        purchaseJson.Add(new BusinessOrderModel
                        {
                            Id = s.Id,
                            Ref = s.OrderReference.FullRef,
                            Location = s.Location.Name,
                            DestinationLocation = tradeOrderB2B.DestinationLocation?.Name ?? "",
                            Channel = s.SalesChannel.ToString(),
                            Contact = tradeOrderB2B.VendorTraderContact?.Name ?? "",
                            ContactId = tradeOrderB2B.VendorTraderContact?.Id ?? 0,
                            Total = s.Sale?.SaleTotal.ToDecimalPlace(currencySettings) ?? ((decimal)0).ToDecimalPlace(currencySettings),
                            Status = s.OrderStatus.ToString(),
                            OrderProblem = s.OrderProblem.ToString(),
                            //Sale = s.Sale?.Reference.FullRef,
                            //SaleId = s.Sale?.Id,
                            //SaleStatus = s.Sale?.Status.ToString(),
                            Purchase = tradeOrderB2B.Purchase?.Reference?.FullRef ?? "",
                            PurchaseId = tradeOrderB2B.Purchase?.Id ?? 0,
                            PurchaseStatus = tradeOrderB2B.Purchase?.Status.ToString() ?? "",
                            Bill = tradeOrderB2B.Bill?.Reference?.FullRef ?? "",
                            BillId = tradeOrderB2B.Bill?.Id ?? 0,
                            BillStatus = tradeOrderB2B.Bill?.Status.ToString() ?? "",
                            Payments = tradeOrderB2B.PurchasePayments.Select(p =>
                                 new PaymentReportModel
                                 {
                                     Ref = (!string.IsNullOrEmpty(p.Reference) ? p.Reference : p.Id.ToString()),
                                     Id = p.Id,
                                     Status = p.Status.ToString()
                                 }).ToList(),
                            Transfer = tradeOrderB2B.PurchaseTransfer?.Reference?.FullRef ?? "",
                            TransferId = tradeOrderB2B.PurchaseTransfer?.Id ?? 0,
                            TransferKey = tradeOrderB2B.PurchaseTransfer?.Key ?? "",
                            TransferStatus = tradeOrderB2B.PurchaseTransfer?.Status.ToString() ?? "",
                            CreatedDate = s.CreateDate.ConvertTimeFromUtc(dateTimeFormat.Timezone).ToString(dateTimeFormat.DateTimeFormat)
                        });
                    });

                    return new DataTablesResponse(requestModel.Draw, purchaseJson, totalRecords, totalRecords);
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword, contactId, locationId, datetime, filterInvoice, filterPayment, filterDelivery, dateTimeFormat, domainId);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }

        }


        public IQueryable<TradeOrderB2B> GetB2BOrders(string keyword, int locationId, int contactId, string datetime, B2CFilterInvoiceType filterBill, B2CFilterPaymentType filterPayment, B2CFilterDeliveryType filterDelivery,
        UserSetting dateTimeFormat, int domainId)
        {
            try
            {

                var query = _dbContext.B2BTradeOrders.Where(s => s.BuyingDomain.Id == domainId);

                query = query.Where(s => s.SalesChannel == SalesChannelEnum.B2B);

                #region filter
                if (locationId > 0)
                {
                    query = query.Where(s => s.DestinationLocation.Id == locationId);
                }
                if (contactId > 0)
                {
                    query = query.Where(s => s.VendorTraderContact.Id == contactId);
                }
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q => q.OrderReference.FullRef.Contains(keyword)
                    || q.DestinationLocation.Name.Contains(keyword)
                    || (q.Customer.Forename + " " + q.Customer.Surname).Contains(keyword)
                    || (q.Purchase != null && q.Purchase.Reference.FullRef.Contains(keyword))
                    || (q.Bill != null && q.Bill.Reference.FullRef.Contains(keyword))
                    || (q.PurchaseTransfer != null && q.PurchaseTransfer.Reference.FullRef.Contains(keyword))
                    || (q.PurchasePayments.Any(s => s.Reference.Contains(keyword)))
                    );
                if (!string.IsNullOrEmpty(datetime))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(dateTimeFormat.Timezone);
                    datetime.ConvertDaterangeFormat(dateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    startDate = startDate.ConvertTimeToUtc(tz);
                    endDate = endDate.ConvertTimeToUtc(tz);
                    query = query.Where(s => (s.CreateDate >= startDate && s.CreateDate < endDate) || (s.Sale.CreatedDate >= startDate && s.Sale.CreatedDate < endDate));
                }

                if (filterBill > 0)
                {
                    if (filterBill == B2CFilterInvoiceType.NotProvided)
                        query = query.Where(s => s.Bill == null);
                    else if (filterBill == B2CFilterInvoiceType.Provided)
                        query = query.Where(s => s.Bill != null);
                }
                if (filterPayment > 0)
                {
                    if (filterPayment == B2CFilterPaymentType.NotPaid)
                        query = query.Where(s => !s.PurchasePayments.Any());
                    else if (filterPayment == B2CFilterPaymentType.PaidInPart)
                        //currently B2B don't have payment
                        query = query.Where(s => s.Sale.SaleTotal <= s.Payments.Where(p => p.Type == CashAccountTransactionTypeEnum.PaymentIn && p.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(p => p.Amount));
                    //query = query.Where(s => s.PurchasePayments.Any(p => p.Status == TraderPaymentStatusEnum.PaymentApproved)
                    //&& (s.Sale.SaleTotal > s.PurchasePayments.Where(p => p.Type == CashAccountTransactionTypeEnum.PaymentIn && p.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(p => p.Amount)));
                    else if (filterPayment == B2CFilterPaymentType.PaidInFull)
                        //currently B2B don't have payment
                        query = query.Where(s => s.Sale.SaleTotal <= s.Payments.Where(p => p.Type == CashAccountTransactionTypeEnum.PaymentIn && p.Status == TraderPaymentStatusEnum.PaymentApproved).Sum(p => p.Amount));
                }
                if (filterDelivery > 0)
                {
                    if (filterDelivery == B2CFilterDeliveryType.NotAssigned)
                        query = query.Where(s => s.PurchaseTransfer == null);
                    else if (filterDelivery == B2CFilterDeliveryType.InProgress)
                        query = query.Where(s => s.PurchaseTransfer != null
                        && !(s.PurchaseTransfer.Status == TransferStatus.Delivered
                        || s.PurchaseTransfer.Status == TransferStatus.Denied
                        || s.PurchaseTransfer.Status == TransferStatus.Discarded));
                    else if (filterDelivery == B2CFilterDeliveryType.Delivered)
                        query = query.Where(s => s.PurchaseTransfer != null && s.PurchaseTransfer.Status == TransferStatus.Delivered);
                }
                return query;
            }
            #endregion
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, keyword, contactId, locationId, datetime, filterBill, filterPayment, filterDelivery, dateTimeFormat, domainId);
                return null;
            }
        }
        
        
        
        /// <summary>
        /// Re-calculation Item Taxes while business update change discount
        /// </summary>
        /// <param name="tradeOrderId"></param>
        /// <param name="discount"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ReturnJsonModel ReCalculateTax(int tradeOrderId, decimal discount, int itemId)
        {
            var resultJson = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOrderId, discount, itemId);

                var tradeOrder = _dbContext.TradeOrders.Find(tradeOrderId);

                var orderJson = tradeOrder.OrderJson;
                var order = orderJson.ParseAs<Order>();
                if (!string.IsNullOrEmpty(tradeOrder.OrderJsonOrig))
                {
                    orderJson = tradeOrder.OrderJsonOrig;
                    order = orderJson.ParseAs<Order>();
                    ValidateB2COrder(order);
                }

                var orderItem = order.Items.FirstOrDefault(p => p.Id == itemId);

                var lstTax = new List<Tax>();
                var oldDiscount = orderItem.Variant.Discount == 100 ? 0 : 1 / (1 - orderItem.Variant.Discount / 100);

                orderItem.Variant.Taxes.ForEach(t =>
                {
                    t.AmountTax = t.AmountTax * oldDiscount * (100 - discount) / 100 * orderItem.Quantity;
                    if (lstTax.Any(x => x.TraderId == t.TraderId))
                    {
                        var taxitem = lstTax.FirstOrDefault(x => x.TraderId == t.TraderId);
                        taxitem.AmountTax += t.AmountTax;
                    }
                    else
                    {
                        lstTax.Add(t);
                    }
                });

                orderItem.Extras.ForEach(extraItem =>
                {
                    extraItem.Taxes.ForEach(t =>
                    {
                        t.AmountTax = t.AmountTax * oldDiscount * (100 - discount) / 100 * orderItem.Quantity;
                        if (lstTax.Any(x => x.TraderId == t.TraderId))
                        {
                            var taxitem = lstTax.FirstOrDefault(x => x.TraderId == t.TraderId);
                            taxitem.AmountTax += t.AmountTax;
                        }
                        else
                        {
                            lstTax.Add(t);
                        }
                    });
                });

                resultJson.result = true;
                resultJson.Object = lstTax;
                resultJson.actionVal = order.VoucherId;
                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrderId, discount, itemId);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }

        /// <summary>
        /// update item when business change discount or price
        /// NOTE: Mark to the Item as IsBusinessDiscount = true
        /// </summary>
        /// <param name="tradeOrderId"></param>
        /// <param name="updatedItem"></param>
        /// <returns></returns>
        public ReturnJsonModel UpdateItemInfor(int tradeOrderId, Item updatedItem)
        {
            var resultJson = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOrderId, updatedItem);

                var tradeOrder = _dbContext.TradeOrders.Find(tradeOrderId);


                var orderJson = tradeOrder.OrderJson.ParseAs<Order>();
                var orderJsonOrig = tradeOrder.OrderJsonOrig.ParseAs<Order>();

                //update change quantity from Business
                var itemOrg = orderJsonOrig.Items.FirstOrDefault(e => e.Id == updatedItem.Id);
                itemOrg.Variant.Discount = updatedItem.Variant.Discount;
                itemOrg.Variant.IsBusinessDiscount = true;
                itemOrg.Extras.ForEach(extra =>
                {
                    extra.IsBusinessDiscount = true;
                    extra.Discount = updatedItem.Variant.Discount;
                });
                var rules = new PosRules(_dbContext);
                var itemB2C = orderJson.Items.FirstOrDefault(e => e.Id == updatedItem.Id);
                orderJson.Discount -= itemB2C.Variant.TotalDiscount + itemB2C.Extras.Sum(e => e.TotalDiscount);
                orderJson.AmountInclTax -= itemB2C.Variant.TotalAmount + itemB2C.Extras.Sum(e => e.TotalAmount);
                orderJson.AmountExclTax -= itemB2C.Variant.AmountExclTax * itemB2C.Variant.Quantity + itemB2C.Extras.Sum(e => e.AmountExclTax * e.Quantity);

                orderJson.Items.Remove(itemB2C);
                orderJson.Items.Add(itemOrg);
                //re-calculation for item updating from Busniness
                rules.ItemDiscountCalculation(orderJson, itemOrg.Variant, itemOrg.Variant.Discount, itemOrg.Quantity);
                itemOrg.Extras.ForEach(extra =>
                {
                    rules.ItemDiscountCalculation(orderJson, extra, extra.Discount, itemOrg.Quantity);
                });
                orderJson.AmountTax = orderJson.AmountInclTax - orderJson.AmountExclTax;


                tradeOrder.OrderJson = orderJson.ToJson();
                tradeOrder.IsAgreedByBusiness = false;
                tradeOrder.IsAgreedByCustomer = false;
                tradeOrder.OrderStatus = TradeOrderStatusEnum.Draft;
                _dbContext.Entry(tradeOrder).State = EntityState.Modified;
                _dbContext.SaveChanges();
                resultJson.msgName = tradeOrder.SellingDomain.Id.BusinesProfile().BusinessName;
                resultJson.result = true;
                resultJson.Object = orderJson.AmountInclTax;

                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrderId, updatedItem);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }
        /// <summary>
        /// Add an item into the Order by Customer or Business
        /// NOTE: this will ignore all change from Business and re-Calculation Order Json and set IsBusinessDiscount = false
        /// </summary>
        /// <param name="tradeOrderId"></param>
        /// <param name="variant"></param>
        /// <param name="extras"></param>
        /// <param name="quantity"></param>
        /// <param name="includedTaxAmount"></param>
        /// <param name="userId"></param>
        /// <param name="categoryItemId"></param>
        /// <param name="voucherId"></param>
        /// <returns></returns>
        public ReturnJsonModel AddItemToB2COrder(int tradeOrderId, Models.Catalogs.Variant variant, List<Extra> extras, int quantity,
            decimal includedTaxAmount, string userId, int categoryItemId, int voucherId)
        {
            var result = new ReturnJsonModel { actionVal = 2 };

            try
            {
                var tradeOrder = GetTradeOrderById(tradeOrderId);

                var currencySetting = new CurrencySettingRules(_dbContext).GetCurrencySettingByDomain(tradeOrder.SellingDomain.Id);

                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, tradeOrder, variant, extras, quantity, includedTaxAmount);
                }

                var b2cOrder = new Order();

                var orderJson = tradeOrder.OrderJson;
                b2cOrder = orderJson.ParseAs<Order>();
                
                if (!string.IsNullOrEmpty(tradeOrder.OrderJsonOrig))
                {
                    orderJson = tradeOrder.OrderJsonOrig;
                    b2cOrder = orderJson.ParseAs<Order>();
                    ValidateB2COrder(b2cOrder);
                }
                UpdateB2COrderIsBusinessDiscount(b2cOrder, false);

                var itemNew = new Item
                {
                    Prepared = false,
                    TraderId = categoryItemId,
                    Quantity = quantity,
                    ImageUri = variant.ImageUri
                };

                var categoryItem = _dbContext.PosCategoryItems.AsNoTracking().FirstOrDefault(p => p.Id == categoryItemId);
                if (!string.IsNullOrEmpty(variant.ImageUri))
                    itemNew.ImageUri = categoryItem?.ImageUri;

                if (b2cOrder.Items == null || b2cOrder.Items.Count() == 0)
                    itemNew.Id = 1;
                else
                {
                    var maxItemId = b2cOrder.Items.Max(p => p.Id);
                    itemNew.Id = maxItemId + 1;
                }

                //Calculate variant price
                itemNew.Name = categoryItem?.Name ?? "";
                itemNew.Variant = new Models.TraderApi.Variant();

                //Get Variant
                if (variant != null && variant.Id > 0)
                {
                    variant = _dbContext.PosVariants.FirstOrDefault(p => p.Id == variant.Id);
                    itemNew.Variant.TraderId = variant.Id;
                    itemNew.Variant.Name = variant.Name;

                    itemNew.Variant.Discount = 0;
                    itemNew.Variant.SKU = variant.TraderItem?.SKU;

                    itemNew.Variant.AmountExclTax = variant.Price?.NetPrice ?? 0;

                    itemNew.Variant.AmountInclTax += variant.Price?.GrossPrice ?? 0;

                    var taxRateList = variant.Price?.Taxes ?? new List<PriceTax>();
                    if (itemNew.Variant.Taxes == null)
                        itemNew.Variant.Taxes = new List<Tax>();

                    foreach (var taxItem in taxRateList)
                    {
                        itemNew.Variant.Taxes.Add(new Tax
                        {
                            TraderId = taxItem.Id,
                            AmountTax = taxItem.Amount,
                            TaxName = taxItem.TaxName
                        });
                    }

                    itemNew.Variant.GrossValue = variant.Price?.GrossPrice ?? 0;
                    itemNew.Variant.GrossValueText = itemNew.Variant.GrossValue.ToCurrencySymbol(currencySetting);

                    itemNew.Variant.NetValue = variant.Price?.NetPrice ?? 0;
                    itemNew.Variant.NetValueText = itemNew.Variant.NetValue.ToCurrencySymbol(currencySetting);

                    itemNew.Variant.TaxAmount = variant.Price?.TotalTaxAmount ?? 0;
                    itemNew.Variant.TaxAmountText = itemNew.Variant.TaxAmount.ToCurrencySymbol(currencySetting);
                }

                b2cOrder.AmountTax = (variant.Price?.TotalTaxAmount ?? 0) * quantity;

                //Insert extra info
                itemNew.Extras = new List<Models.TraderApi.Variant>();
                if (extras != null)
                {
                    foreach (var extraItem in extras)
                    {
                        var extraNew = new Models.TraderApi.Variant();

                        if (extraItem.Id > 0)
                        {
                            var extraItemInDb = _dbContext.PosExtras.FirstOrDefault(p => p.Id == extraItem.Id);
                            extraNew.TraderId = extraItem.Id;
                            extraNew.Name = extraItemInDb.Name;
                            extraNew.SKU = extraItemInDb.TraderItem.SKU;

                            extraNew.AmountInclTax += extraItemInDb.Price?.GrossPrice ?? 0;
                            extraNew.Discount = 0;
                            extraNew.AmountExclTax = extraItemInDb.Price?.NetPrice ?? 0;
                            b2cOrder.AmountTax += (extraItemInDb.Price?.TotalTaxAmount ?? 0) * quantity;

                            var taxRateList = extraItemInDb.Price?.Taxes ?? new List<PriceTax>();
                            if (extraNew.Taxes == null)
                            {
                                extraNew.Taxes = new List<Tax>();
                            }
                            foreach (var taxItem in taxRateList)
                            {
                                extraNew.Taxes.Add(new Tax()
                                {
                                    TraderId = taxItem.Id,
                                    AmountTax = taxItem.Amount,
                                    TaxName = taxItem.TaxName
                                });
                            }

                            extraNew.GrossValue = extraItemInDb.Price?.GrossPrice ?? 0;
                            extraNew.GrossValueText = extraNew.GrossValue.ToCurrencySymbol(currencySetting);

                            extraNew.NetValue = extraItemInDb.Price?.NetPrice ?? 0;
                            extraNew.NetValueText = extraNew.NetValue.ToCurrencySymbol(currencySetting);

                            extraNew.TaxAmount = extraItemInDb.Price?.TotalTaxAmount ?? 0;
                            extraNew.TaxAmountText = extraNew.TaxAmount.ToCurrencySymbol(currencySetting);

                            itemNew.Extras.Add(extraNew);
                        }
                    }
                }

                if (b2cOrder.Items == null)
                {
                    b2cOrder.Items = new List<Item>();
                }
                b2cOrder.Items.Add(itemNew);
                b2cOrder.AmountInclTax += includedTaxAmount;
                b2cOrder.AmountExclTax = b2cOrder.AmountInclTax - b2cOrder.AmountTax;

                b2cOrder.VoucherId = voucherId;

                CalculationB2COrderJson(tradeOrder, b2cOrder, true);

                //Remove customer and business accepted flag
                tradeOrder.IsAgreedByBusiness = false;
                tradeOrder.IsAgreedByCustomer = false;
                tradeOrder.OrderStatus = TradeOrderStatusEnum.Draft;
                _dbContext.Entry(tradeOrder).State = EntityState.Modified;
                _dbContext.SaveChanges();

                result.msgName = tradeOrder.SellingDomain.Id.BusinesProfile().BusinessName;
                result.result = true;
                result.Object = tradeOrder.Id;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, tradeOrderId, variant, extras, quantity, includedTaxAmount);
                result.result = false;
                result.msg = ex.Message;
                return result;
            }
        }
        //OrderOrig - recheck AmountInclTax and AmountExclTax
        public void ValidateB2COrder(Order b2COrder)
        {
            //fix image of items 
            string ApiGetDocumentUri = "getdocument?file=";
            b2COrder.Items.Where(e => e?.ImageUri?.IndexOf(ApiGetDocumentUri) > 0).ForEach(e =>
            {
                e.ImageUri = e.ImageUri.Remove(0, e.ImageUri.IndexOf(ApiGetDocumentUri) + ApiGetDocumentUri.Length);
            });

            //Caculator OrderOrig

            {
                var lstItems = b2COrder.Items;
                b2COrder.AmountInclTax = 0;
                b2COrder.AmountExclTax = 0;
                foreach (var item in lstItems)
                {
                    decimal itemInclTax = 0;
                    decimal itemExclTax = 0;

                    itemInclTax += (item?.Variant?.AmountInclTax ?? 0) * (item?.Quantity ?? 0);
                    itemExclTax += (item?.Variant?.AmountExclTax ?? 0) * (item?.Quantity ?? 0);
                    if (item.Extras != null)
                    {
                        foreach (var extraItem in item.Extras)
                        {
                            itemInclTax += extraItem.AmountInclTax * item.Quantity;
                            itemExclTax += extraItem.AmountExclTax * item.Quantity;
                        }
                    }

                    b2COrder.AmountInclTax += itemInclTax;
                    b2COrder.AmountExclTax += itemExclTax;
                }
                b2COrder.AmountTax = b2COrder.AmountInclTax - b2COrder.AmountExclTax;
            }
        }

        /// <summary>
        /// Remove an item from the Order by Customer
        /// NOTE: this will ignore all change from Business and re-Calculation Order Json
        /// </summary>
        /// <param name="disId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ReturnJsonModel RemoveItemFromOrder(int disId, int itemId)
        {
            var result = new ReturnJsonModel { actionVal = 2, Object = 0 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, disId, itemId);

                var discussion = new DiscussionsRules(_dbContext).GetB2CDiscussionOrderByDiscussionId(disId);

                var tradeOrder = _dbContext.TradeOrders.FirstOrDefault(p => p.Id == discussion.TradeOrder.Id);
                if (tradeOrder.IsAgreedByBusiness && tradeOrder.IsAgreedByCustomer)
                {
                    result.result = true;
                    result.actionVal = -1;
                    return result;
                }
                var orderJson = tradeOrder.OrderJson;
                var order = orderJson.ParseAs<Order>();
                if (!string.IsNullOrEmpty(tradeOrder.OrderJsonOrig))
                {
                    orderJson = tradeOrder.OrderJsonOrig;
                    order = orderJson.ParseAs<Order>();
                    ValidateB2COrder(order);
                }

                UpdateB2COrderIsBusinessDiscount(order, false);
                //Calculate price of item to remove
                var itemToRemove = order.Items.FirstOrDefault(p => p.Id == itemId);
                decimal itemAmountInclTax = itemToRemove.Variant.AmountInclTax;
                decimal itemAmountExclTax = itemToRemove.Variant.AmountExclTax;

                itemToRemove.Extras.ForEach(extraItem =>
                {
                    itemAmountInclTax += extraItem.AmountInclTax;
                    itemAmountExclTax += extraItem.AmountExclTax;
                });
                order.AmountInclTax -= itemAmountInclTax * itemToRemove.Quantity;
                order.AmountExclTax -= itemAmountExclTax * itemToRemove.Quantity;
                order.AmountTax = order.AmountInclTax - order.AmountExclTax;

                order.Items.Remove(order.Items.FirstOrDefault(p => p.Id == itemId));

                CalculationB2COrderJson(tradeOrder, order, true);

                tradeOrder.IsAgreedByBusiness = false;
                tradeOrder.IsAgreedByCustomer = false;
                tradeOrder.OrderStatus = TradeOrderStatusEnum.Draft;
                _dbContext.Entry(tradeOrder).State = EntityState.Modified;
                _dbContext.SaveChanges();
                result.result = true;
                result.Object = order.AmountInclTax;
                result.msgName = tradeOrder.SellingDomain.Id.BusinesProfile().BusinessName;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, disId, itemId);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }
        /// <summary>
        /// Update quantity to an item from the Order by Customer
        /// NOTE: this will ignore all change from Business and re-Calculation Order Json
        /// </summary>
        /// <param name="b2cOrder"></param>
        /// <param name="itemId"></param>
        /// <param name="newQuantity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReturnJsonModel UpdateB2COrderItemQuantity(TradeOrder b2cOrder, int itemId, int newQuantity, string userId)
        {
            var result = new ReturnJsonModel()
            {
                actionVal = 2
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, b2cOrder, itemId, newQuantity);

                if (b2cOrder != null && b2cOrder.Id > 0)
                {
                    b2cOrder = _dbContext.TradeOrders.FirstOrDefault(p => p.Id == b2cOrder.Id);
                }
                if (b2cOrder.IsAgreedByBusiness && b2cOrder.IsAgreedByCustomer)
                {
                    result.result = true;
                    result.actionVal = -1;
                    return result;
                }

                var orderJson = b2cOrder.OrderJson;
                var order = orderJson.ParseAs<Order>();
                if (!string.IsNullOrEmpty(b2cOrder.OrderJsonOrig))
                {
                    orderJson = b2cOrder.OrderJsonOrig;
                    order = orderJson.ParseAs<Order>();
                    ValidateB2COrder(order);
                }

                UpdateB2COrderIsBusinessDiscount(order, false);

                var lstItems = order.Items;
                foreach (var item in lstItems)
                {
                    if (item.Id == itemId)
                    {
                        decimal itemInclTax = 0;
                        decimal itemExclTax = 0;
                        decimal itemAmountTax = 0;

                        itemInclTax += (item?.Variant?.AmountInclTax ?? 0) * (item?.Quantity ?? 0);
                        itemExclTax += (item?.Variant?.AmountExclTax ?? 0) * (item?.Quantity ?? 0);
                        if (item.Extras != null)
                        {
                            foreach (var extraItem in item.Extras)
                            {
                                itemInclTax += extraItem.AmountInclTax * item.Quantity;
                                itemExclTax += extraItem.AmountExclTax * item.Quantity;
                            }
                        }
                        itemAmountTax = itemInclTax - itemExclTax;

                        order.AmountInclTax = order.AmountInclTax - itemInclTax + (itemInclTax / item.Quantity) * newQuantity;
                        order.AmountExclTax = order.AmountExclTax - itemExclTax + (itemExclTax / item.Quantity) * newQuantity;
                        order.AmountTax = order.AmountInclTax - order.AmountExclTax;

                        item.Quantity = newQuantity;


                        CalculationB2COrderJson(b2cOrder, order, true);

                        b2cOrder.IsAgreedByCustomer = false;
                        b2cOrder.IsAgreedByBusiness = false;
                        b2cOrder.OrderStatus = TradeOrderStatusEnum.Draft;
                        _dbContext.Entry(b2cOrder).State = EntityState.Modified;
                        _dbContext.SaveChanges();

                        break;
                    }
                }
                result.result = true;
                result.msgName = b2cOrder.SellingDomain.Id.BusinesProfile().BusinessName;
                return result;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, b2cOrder, itemId, newQuantity);
                result.result = false;
                result.msg = "Something went wrong. Please contact to the administrator.";
                return result;
            }
        }


        /// <summary>
        /// Apply or remove voucher from B2C Order - by customer
        /// </summary>
        /// <param name="discussionKey"></param>
        /// <param name="tradeOrderId"></param>
        /// <param name="voucherId">=0 - remove voucher</param>
        /// <returns></returns>
        public ReturnJsonModel ApplyVoucherOrderB2C(string discussionKey, string tradeOrderId, int voucherId, UserSetting user, string currentGoBackPage, int qbicleId, string originatingCreationId)
        {
            var tradeOrder = GetTradeOrderById(int.Parse(tradeOrderId.Decrypt()));

            if (string.IsNullOrEmpty(tradeOrder.OrderJson))
                return new ReturnJsonModel { result = true };
            var orderJson = tradeOrder.OrderJson;
            var b2COrder = orderJson.ParseAs<Order>();
            if (!string.IsNullOrEmpty(tradeOrder.OrderJsonOrig))
            {
                orderJson = tradeOrder.OrderJsonOrig;
                b2COrder = orderJson.ParseAs<Order>();
                ValidateB2COrder(b2COrder);
            }

            UpdateB2COrderIsBusinessDiscount(b2COrder, false);
            b2COrder.VoucherId = voucherId;

            CalculationB2COrderJson(tradeOrder, b2COrder, true);

            _dbContext.Entry(tradeOrder).State = EntityState.Modified;
            _dbContext.SaveChanges();

            var dicussionId = int.Parse(discussionKey.Decrypt());
            var discussion = new B2CRules(_dbContext).DiscussionOrderBy(dicussionId, user, currentGoBackPage);
            new B2CRules(_dbContext).B2CDicussionOrderSendMessage(true, ResourcesManager._L("B2C_UPDATED_ORDER", discussion.DisplayName), dicussionId, user.Id, qbicleId, originatingCreationId);

            return new ReturnJsonModel { result = true };
        }

        /// <summary>
        /// Update OrderJsonOrig = order
        /// Update OrderJson = OrderVoucherCalculation2Pos(order)
        /// </summary>
        /// <param name="tradeOrder"></param>
        /// <param name="orderOrigRequest">order orig request from app to api</param>   
        /// <param name="resetDiscount">If change quantity, remove item, apply voucher then reset discount = 0</param>
        public void CalculationB2COrderJson(TradeOrder tradeOrder, Order orderOrigRequest, bool resetDiscount)
        {
            tradeOrder.OrderJson = "";
            tradeOrder.OrderJsonOrig = orderOrigRequest.ToJson();
            if (orderOrigRequest.VoucherId > 0)
                tradeOrder.OrderJson = new PosRules(_dbContext).OrderVoucherCalculation2Pos(orderOrigRequest).ToJson();
            else
            {
                orderOrigRequest.VoucherId = 0;
                orderOrigRequest.VoucherName = "";
                orderOrigRequest.Discount = 0;
                orderOrigRequest.AmountInclTax = 0;
                orderOrigRequest.AmountExclTax = 0;
                orderOrigRequest.AmountTax = 0;
                var rules = new PosRules(_dbContext);
                orderOrigRequest.Items.ForEach(item =>
                {
                    var discount = resetDiscount ? 0 : item.Variant.Discount;
                    rules.ItemDiscountCalculation(orderOrigRequest, item.Variant, discount, item.Quantity);

                    item.Extras.ForEach(extra =>
                    {
                        discount = resetDiscount ? 0 : extra.Discount;
                        rules.ItemDiscountCalculation(orderOrigRequest, extra, discount, item.Quantity);
                    });
                });
                orderOrigRequest.AmountTax = orderOrigRequest.AmountInclTax - orderOrigRequest.AmountExclTax;

                tradeOrder.OrderJson = orderOrigRequest.ToJson();
            }
        }

        /// <summary>
        /// Update B2C Order Is Business Discount
        /// </summary>
        /// <param name="orderOrigRequest">order orig from app request to api</param>
        /// <param name="isBusinessDiscount"></param>
        public void UpdateB2COrderIsBusinessDiscount(Order orderOrigRequest, bool isBusinessDiscount)
        {
            orderOrigRequest.Items.ForEach(item =>
            {
                //item.Variant.IsBusinessDiscount = isBusinessDiscount;
                item.Extras.ForEach(extra =>
                {
                    extra.IsBusinessDiscount = isBusinessDiscount;
                });
            });
        }

        public ReturnJsonModel ProcessB2COrder(int tradeOrderId, int paymentAccId, int saleWgId, int invoiceWgId, int paymentWgId, int transferWgId,
            string customerId, string currentUserId, bool isProcessOrder = true)
        {
            var resultJson = new ReturnJsonModel();
            try
            {
                // Log the information for the CURRENT USER
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOrderId, currentUserId);

                var linkedOrderId = Guid.NewGuid().ToString();

                var user = _dbContext.QbicleUser.Find(currentUserId);
                var tradeOrder = _dbContext.TradeOrders.Find(tradeOrderId);

                var order = tradeOrder.OrderJson.ParseAs<Order>();
                var orderOrg = tradeOrder.OrderJsonOrig.ParseAs<Order>();

                tradeOrder.LinkedOrderId = linkedOrderId;
                order.LinkedTraderId = linkedOrderId;
                orderOrg.LinkedTraderId = linkedOrderId;
                orderOrg.Classification = order.Classification;

                var b2COrder = new PosRules(_dbContext).OrderVoucherCalculation2B2C(order, orderOrg);
                b2COrder.LinkedTraderId = linkedOrderId;

                var customerUser = _dbContext.QbicleUser.Find(customerId);

                var traderContact = new OrderProcessingHelper(_dbContext).GetCreateTraderContactFromUserInfo(customerUser, tradeOrder.Location.Domain, SalesChannelEnum.B2C);
                if (traderContact == null || traderContact.Id <= 0)
                {
                    resultJson.result = false;
                    resultJson.msg = "Created Trader contact failed";
                    return resultJson;
                }

                var cashier = new Cashier
                {
                    TraderId = user.Id,
                    Avatar = user.ProfilePic.ToUri(),
                    Forename = user.Forename,
                    Surname = user.Surname

                };
                b2COrder.Cashier = cashier;
                b2COrder.Reference = tradeOrder.OrderReference.FullRef;
                b2COrder.TradeOrderId = tradeOrderId;

                tradeOrder.OrderJsonOrig = b2COrder.ToJson();
                tradeOrder.OrderJsonOrig = order.ToJson();

                tradeOrder.IsAgreedByBusiness = true;
                tradeOrder.TraderContact = traderContact;
                tradeOrder.ProvisionalOrder = b2COrder;

                tradeOrder.Customer = traderContact.QbicleUser;
                tradeOrder.SaleWorkGroup = _dbContext.WorkGroups.Find(saleWgId);
                tradeOrder.InvoiceWorkGroup = _dbContext.WorkGroups.Find(invoiceWgId);
                tradeOrder.PaymentWorkGroup = _dbContext.WorkGroups.Find(paymentWgId);
                tradeOrder.TransferWorkGroup = _dbContext.WorkGroups.Find(transferWgId);
                tradeOrder.PaymentAccount = _dbContext.TraderCashAccounts.Find(paymentAccId);

                switch (tradeOrder.DeliveryMethod)
                {
                    case DeliveryMethodEnum.Delivery:
                        tradeOrder.OrderCustomer = new OrderProcessingHelper(_dbContext).MapCustomer2OrderCustomer(b2COrder.Customer);
                        break;
                    case DeliveryMethodEnum.CustomerPickup:
                        tradeOrder.OrderCustomer = new OrderProcessingHelper(_dbContext).MapTraderContact2OrderCustomer(traderContact);
                        break;
                }

                if (tradeOrder.IsAgreedByBusiness && tradeOrder.IsAgreedByCustomer)
                {
                    tradeOrder.OrderStatus = TradeOrderStatusEnum.AwaitingProcessing;
                }
                _dbContext.Entry(tradeOrder).State = EntityState.Modified;
                _dbContext.SaveChanges();

                resultJson.result = true;
                if (isProcessOrder)
                    B2CProcessOrder(tradeOrder.Id);

                tradeOrder.OrderStatus = TradeOrderStatusEnum.InProcessing;
                _dbContext.SaveChanges();
                return resultJson;
            }
            catch (Exception ex)
            {
                // Log the information for the CURRENT USER
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrderId, currentUserId);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }

        public ReturnJsonModel ProcessB2COrderMicro(int tradeOrderId, int paymentAccId, int saleWgId, int invoiceWgId, int paymentWgId, int transferWgId,
            string customerId, string currentUserId, int discussionId, int qbicleId, string userNotificationId)
        {
            var resultJson = ProcessB2COrder(tradeOrderId, paymentAccId, saleWgId, invoiceWgId, paymentWgId, transferWgId, customerId, currentUserId, false);

            var job = new OrderJobParameter
            {
                Id = tradeOrderId,
                EndPointName = "processorder",
                Address = "",
                InvoiceDetail = "",
                ActivityNotification = new ActivityNotification
                {
                    QbicleId = qbicleId,
                    DiscussionId = discussionId,//process order and CreateB2cDicussionOrderSendMessage
                    CreatedById = userNotificationId
                },
            };

            var tskHangfire = new Task(async () =>
            {
                await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();

            return resultJson;

        }
        private void B2CProcessOrder(int tradeOrderId)
        {
            var job = new OrderJobParameter
            {
                Id = tradeOrderId,

                EndPointName = "processorder",
                Address = "",
                InvoiceDetail = ""
            };
            var tskHangfire = new Task(async () =>
            {
                await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }
        #endregion

        public ReturnJsonModel GetVariantBySelectedOptions(List<int> listVariantOptionIds, int categoryItemId, int quantity)
        {
            var returnResult = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, listVariantOptionIds, categoryItemId, quantity);

                var categoryItem = _dbContext.PosCategoryItems.FirstOrDefault(p => p.Id == categoryItemId);
                var domain = categoryItem.Category?.Menu?.Location?.Domain == null
                    ? categoryItem.Category.Menu.Domain : categoryItem.Category.Menu?.Location?.Domain;
                var currcencySetting = _dbContext.CurrencySettings.FirstOrDefault(p => p.Domain.Id == domain.Id);

                if (!(categoryItem.PosVariants.Count > 0 && categoryItem.VariantProperties.Count == 0))
                {
                    // Get variant
                    var variantItem = categoryItem.PosVariants
                        .Where(p => !p.VariantOptions.Any(x => !listVariantOptionIds.Contains(x.Id))
                                && p.VariantOptions.Count() == listVariantOptionIds.Count()).FirstOrDefault();

                    if (variantItem == null)
                    {
                        returnResult.Object = new SelectedVariantModel()
                        {
                            ImageUri = categoryItem?.ImageUri?.ToUriString() ?? "",
                            Price = 0,
                            PriceStr = null,
                            CategoryName = "",
                            Description = "",
                            GrossValue = 0,
                            GrossValueStr = "",
                            NetValue = 0,
                            NetValueStr = "",
                            SKU = "",
                            TaxAmount = 0,
                            TaxAmountStr = "",
                            Taxes = new List<Tax>(),
                            VariantName = "",
                            CategoryId = categoryItem.Category?.Id ?? 0,
                            VariantId = 0
                        };
                        returnResult.result = true;
                        return returnResult;
                    }

                    var grossPrice = (variantItem.Price?.GrossPrice ?? 0) * quantity;
                    var netPrice = (variantItem.Price?.NetPrice ?? 0) * quantity;
                    var amountTax = (variantItem.Price?.TotalTaxAmount ?? 0) * quantity;

                    var selectedVariant = new SelectedVariantModel()
                    {
                        ImageUri = variantItem?.ImageUri?.ToUriString() ?? "",
                        Price = grossPrice,
                        PriceStr = currcencySetting.CurrencySymbol + grossPrice.ToDecimalPlace(currcencySetting),

                        GrossValue = grossPrice,
                        GrossValueStr = currcencySetting.CurrencySymbol + grossPrice.ToDecimalPlace(currcencySetting),
                        NetValue = netPrice,
                        NetValueStr = currcencySetting.CurrencySymbol + netPrice.ToDecimalPlace(currcencySetting),
                        TaxAmount = amountTax,
                        TaxAmountStr = currcencySetting.CurrencySymbol + amountTax.ToDecimalPlace(currcencySetting),
                        CategoryName = categoryItem?.Category?.Name ?? "",
                        CategoryId = categoryItem?.Category?.Id ?? 0,
                        Description = categoryItem?.Description ?? "",
                        SKU = variantItem.TraderItem?.SKU ?? "",
                        Taxes = new List<Tax>(),
                        VariantName = variantItem.Name,
                        VariantId = variantItem.Id
                    };

                    if (variantItem.Price?.Taxes != null)
                    {
                        selectedVariant.Taxes = variantItem.Price?.Taxes.Select(p => new Tax
                        {
                            AmountTax = p.Amount,
                            TaxName = p.TaxName,
                            TaxRate = p.Rate,
                            TraderId = p.Id
                        }).ToList();
                    }
                    returnResult.Object = selectedVariant;
                    returnResult.result = true;
                    return returnResult;
                }

                var variantDefault = categoryItem.PosVariants.FirstOrDefault(v => v.IsDefault) ?? new Variant();
                var defaultGrossPrice = (variantDefault.Price?.GrossPrice ?? 0) * quantity;
                var defaultNetPrice = (variantDefault.Price?.NetPrice ?? 0) * quantity;
                var defaultAmountTax = (variantDefault.Price?.TotalTaxAmount ?? 0) * quantity;

                var selectedItem = new SelectedVariantModel()
                {
                    ImageUri = categoryItem?.ImageUri?.ToUriString() ?? "",
                    Price = defaultGrossPrice,
                    PriceStr = currcencySetting.CurrencySymbol + defaultGrossPrice.ToDecimalPlace(currcencySetting),
                    CategoryName = categoryItem.Category?.Name ?? "",
                    Description = categoryItem.Description ?? "",
                    GrossValue = defaultGrossPrice,
                    GrossValueStr = currcencySetting.CurrencySymbol + defaultGrossPrice.ToDecimalPlace(currcencySetting),
                    NetValue = defaultNetPrice,
                    NetValueStr = currcencySetting.CurrencySymbol + defaultNetPrice.ToDecimalPlace(currcencySetting),
                    SKU = variantDefault.TraderItem.SKU,
                    TaxAmount = defaultAmountTax,
                    TaxAmountStr = currcencySetting.CurrencySymbol + defaultAmountTax.ToDecimalPlace(currcencySetting),
                    Taxes = new List<Tax>(),
                    CategoryId = categoryItem.Category?.Id ?? 0,
                    VariantName = variantDefault.Name,
                    VariantId = variantDefault.Id,
                };

                if (variantDefault.Price?.Taxes != null)
                {
                    selectedItem.Taxes = variantDefault.Price?.Taxes.Select(p => new Tax()
                    {
                        AmountTax = p.Amount,
                        TaxName = p.TaxName,
                        TaxRate = p.Rate,
                        TraderId = p.Id
                    }).ToList();
                }

                returnResult.Object = selectedItem;
                returnResult.result = true;
                return returnResult;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, listVariantOptionIds, categoryItemId, quantity);
                return returnResult;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="qbicleKey"></param>
        /// <param name="type">b2c - c2c</param>
        /// <param name="dateFormat"></param>
        /// <param name="timezone"></param>
        /// <param name="keyword"></param>
        /// <param name="daterange"></param>
        /// <param name="status"></param>
        /// <param name="orderBy">
        /// 0>Latest activity, 1> Date(recent first) 2>Date(oldest first)
        /// </param>
        /// <returns></returns>
        public DataTablesResponse GetOrderContextFlyout([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string qbicleKey, string type, string dateFormat, string timezone,
            string keyword = "", string daterange = "", List<int> status = null, int orderBy = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, daterange, keyword);

                int totalrecords = 0;
                #region Filters
                var qbicleId = qbicleKey.Decrypt2Int();
                IQueryable<B2COrderCreation> query = _dbContext.B2COrderCreations.Where(e => e.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2COrder && e.Qbicle.Id == qbicleId && e.TradeOrder != null);

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.TradeOrder.OrderReference.FullRef.Contains(keyword));
                }

                if (status != null)
                {
                    List<TradeOrderStatusEnum> enumList = status.Select(x => (TradeOrderStatusEnum)Enum.Parse(typeof(TradeOrderStatusEnum), x.ToString())).ToList();
                    query = query.Where(s => enumList.Contains(s.TradeOrder.OrderStatus));
                }
                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    daterange.ConvertDaterangeFormat(dateFormat, timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    query = query.Where(s => s.TimeLineDate >= startDate && s.TimeLineDate < endDate);
                }
                totalrecords = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();

                switch (orderBy)
                {
                    case 1:
                        query = query.OrderByDescending(e => e.TradeOrder.CreateDate);
                        break;
                    case 2:
                        query = query.OrderBy(e => e.TradeOrder.CreateDate);
                        break;
                    default:
                        query = query.OrderByDescending(e => e.TimeLineDate);
                        break;
                }
                #endregion
                #region Paging
                var orderCreations = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                var tableContents = new List<string>();

                orderCreations.ForEach(o =>
                {
                    var order = o.TradeOrder.OrderJson.ParseAs<Order>();

                    var oPost = o.Posts.OrderByDescending(e => e.TimeLineDate).FirstOrDefault();

                    var lastUpdate = oPost?.TimeLineDate;
                    if (lastUpdate == null)
                        lastUpdate = o.TimeLineDate;


                    type = type == "b2c" ? "B2C" : "C2C";

                    var tableContent = $"<a href='/B2C/DiscussionOrder?disKey={o.Key}&discussionType={type}' target='_blank'>";
                    tableContent += $"<div class='order-summary'>";
                    tableContent += $"<div class='flexit'>";
                    tableContent += $"<div class='order--0'>";
                    tableContent += $"<h1>{o.TradeOrder?.OrderReference?.FullRef}</h1>";
                        tableContent += $"<label id='order-context-flyout-status-{o.Id}' class='label label-lg label-{o.TradeOrder.GetClass()}'>{o.TradeOrder.GetDescription()}</label>";
                    tableContent += $"<small>{lastUpdate?.ConvertTimeFromUtc(timezone, dateFormat + " hh:mmtt").ToLower()}</small>";
                    tableContent += $"</div>";
                    tableContent += $"<div class='order--1'>";
                    tableContent += $"<div class='flexitems'>";

                    var img = "/Content/DesignStyle/img/item-placeholder.png";
                    if (!order.Items.Any())
                        tableContent += $"<div class='pimg' style=\"background-image: url('{img}');\">&nbsp;</div>";
                    else
                    {
                        var itemIndex = 1;
                        order.Items.ForEach(item =>
                        {
                            if (itemIndex > 2) return;
                            var itemImg = item.ImageUri.ToUriString(Enums.FileTypeEnum.Image, "S");
                            if (item.ImageUri.Contains("retriever/getdocument") && item.ImageUri.Contains("="))
                                itemImg = item.ImageUri.Split('=')[1].ToUriString(Enums.FileTypeEnum.Image, "S");
                            tableContent += $"<div class='pimg' style=\"background-image: url('{itemImg}');\">&nbsp;</div>";
                            itemIndex++;
                        });
                        if (order.Items.Count > 2)
                        {
                            tableContent += $"<div class='pimg andmore'>+{order.Items.Count - 2}</div>";
                        }
                    }
                    var message = oPost?.Message;
                    if (string.IsNullOrEmpty(message))
                        message = o.StartedBy.GetFullName() + " has created order";
                    tableContent += $"</div>";
                    tableContent += $"</div>";
                    tableContent += $"</div>";
                    tableContent += $"<div class='order--detes'>";
                    tableContent += $"<div class='well custom rounded' style='margin: 0; padding: 18px 20px 12px 20px;'>";
                    tableContent += $"<p style='margin: 0; padding: 0;'><strong><label>Last update:</label> </strong>{message}</p>";
                    tableContent += $"</div>";
                    tableContent += $"</div>";
                    tableContent += $"</div>";
                    tableContent += $"</a>";

                    tableContents.Add(tableContent);
                });


                #endregion
                var dataJson = tableContents.Select(q => new
                {
                    tableContent = q
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalrecords, totalrecords);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        public bool CheckExistB2cOrders(string qbicleKey)
        {
            var qbicleId = qbicleKey.Decrypt2Int();
            return _dbContext.B2COrderCreations.Any(e => e.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2COrder && e.Qbicle.Id == qbicleId && e.TradeOrder != null);

        }
    }
}
