using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Qbicles;
using Qbicles.Models.WaitList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Model.TB_Column;
using static Qbicles.Models.EmailLog;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class WaitListRules : ISignalRNotification
    {
        ApplicationDbContext dbContext;

        public WaitListRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        public ReturnJsonModel CheckPendingWaitlist()
        {
            try
            {
                var waitList = dbContext.WaitListRequests.AsNoTracking().Where(e => !e.IsApprovedForCustomDomain && !e.IsApprovedForSubsDomain && !e.IsRejected).Count();
                return new ReturnJsonModel { actionVal = waitList };
            }
            catch
            {
                return new ReturnJsonModel { actionVal = 0 };
            }
            finally
            {
            }
        }

        public WaitlistRequestRights CheckApprovalWaitlist(string userId)
        {
            try
            {
                var domainCreationRights = new WaitListRules(dbContext).GetDomainCreationRights(userId) ?? new DomainCreationRights();
                var waitingRequest = GetWaitRequest(userId);
                var isNewRequest = waitingRequest.Id > 0 && waitingRequest.IsApprovedForSubsDomain == false && waitingRequest.IsApprovedForCustomDomain == false && waitingRequest.IsRejected == false;

                var waitRequest = "none";
                var waitPending = "none";
                var allDomainCustom = "none";
                var domainWithoutCustom = "none";
                var waitJoinCustom = "none";


                if (domainCreationRights.IsApprovedForSubsDomain && domainCreationRights.IsApprovedForCustomDomain)
                {
                    allDomainCustom = "block";
                }
                else if (domainCreationRights.IsApprovedForSubsDomain && !domainCreationRights.IsApprovedForCustomDomain && isNewRequest == false)
                {
                    domainWithoutCustom = "block";
                }
                else if (domainCreationRights.IsApprovedForSubsDomain && !domainCreationRights.IsApprovedForCustomDomain && isNewRequest == true)
                {
                    waitJoinCustom = "block";
                }
                else if (!domainCreationRights.IsApprovedForSubsDomain && !domainCreationRights.IsApprovedForCustomDomain && isNewRequest == true)
                {
                    waitPending = "block";
                }
                else if (!domainCreationRights.IsApprovedForSubsDomain && !domainCreationRights.IsApprovedForCustomDomain)
                {
                    waitRequest = "block";
                }

                return new WaitlistRequestRights
                {
                    waitRequest = waitRequest,
                    waitPending = waitPending,
                    allDomainCustom = allDomainCustom,
                    domainWithoutCustom = domainWithoutCustom,
                    waitJoinCustom = waitJoinCustom,
                };
            }
            catch
            {
                return new WaitlistRequestRights();
            }
            finally
            {
            }
        }
        /// <summary>
        /// Get user's waitlist, return waitlist empty if null
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public WaitListRequest GetWaitRequest(string userId)
        {
            return dbContext.WaitListRequests.Where(e => e.User.Id == userId && e.ReviewedBy == null).OrderByDescending(d => d.CreatedDate).FirstOrDefault() ?? new WaitListRequest();
        }
        /// <summary>
        /// Get DomainCreationRights, return waitlist empty if null
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DomainCreationRights GetDomainCreationRights(string userId)
        {
            return dbContext.DomainCreationRights.FirstOrDefault(e => e.AssociatedUser.Id == userId);
        }

        public DataTablesResponse GetWaitListRequests([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string dateFormat, string timezone,
            string keyword = "", string daterange = "", List<CountryCode> countries = null, List<NumberOfEmployees> employees = null, List<DiscoveredVia> discoveredVia = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, daterange, keyword);

                int totalrecords = 0;
                #region Filters
                IQueryable<WaitListRequest> query = dbContext.WaitListRequests.Where(e => !e.IsApprovedForCustomDomain && !e.IsApprovedForSubsDomain && !e.IsRejected);

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(
                        s => s.User.Email.Contains(keyword)
                        || s.User.DisplayUserName.Contains(keyword)
                        || s.User.Surname.Contains(keyword)
                        || s.User.Forename.Contains(keyword)
                        || s.CountryName.Contains(keyword));
                }
                if (employees != null)
                {
                    var getEmployEmpty = false;

                    for (int i = 0; i < employees.Count; i++)
                    {
                        if (!Enum.IsDefined(typeof(NumberOfEmployees), employees[i]))
                        {
                            getEmployEmpty = true;
                            break;
                        }
                    }
                    if (getEmployEmpty)
                        query = query.Where(s => employees.Contains((NumberOfEmployees)s.NumberOfEmployees) || s.NumberOfEmployees == null);
                    else
                        query = query.Where(s => employees.Contains((NumberOfEmployees)s.NumberOfEmployees) && s.NumberOfEmployees != null);
                }
                if (discoveredVia != null)
                {
                    var getDiscoveredVia = false;
                    for (int i = 0; i < discoveredVia.Count; i++)
                    {
                        if (!Enum.IsDefined(typeof(DiscoveredVia), discoveredVia[i]))
                        {
                            getDiscoveredVia = true;
                            break;
                        }
                    }
                    if (getDiscoveredVia)
                        query = query.Where(s => discoveredVia.Contains((DiscoveredVia)s.DiscoveredVia) || s.DiscoveredVia == null);
                    else
                        query = query.Where(s => discoveredVia.Contains((DiscoveredVia)s.DiscoveredVia) && s.DiscoveredVia != null);
                }

                if (countries != null)
                {
                    query = query.Where(s => countries.Contains(s.CountryCode));
                }
                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    daterange.ConvertDaterangeFormat(dateFormat, timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }
                totalrecords = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = "";

                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "User":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "User.Forename" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ",User.Surname" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "DateTime":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "LastRequesstdDate" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Country":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CountryName" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Business":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "NumberOfEmployees" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "DiscoveredVia" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "LastRequesstdDate desc";
                            break;
                    }
                }
                query = query.OrderBy(orderByString == "" ? "LastRequesstdDate asc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    DateTime = q.LastRequesstdDate?.ConvertTimeFromUtc(timezone).ToString(dateFormat + " hh:mmtt") ?? q.CreatedDate.ConvertTimeFromUtc(timezone).ToString(dateFormat + " hh:mmtt"),
                    User = q.User.GetFullName(),
                    UserKey = q.User.Id,
                    CreatorUri = q.User.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T"),
                    Country = q.CountryName,
                    Categories = string.Join(", ", q.BusinessCategories.Select(c => c.Name)),
                    Business = q.NumberOfEmployees?.GetDescription(),
                    Discovered = q.DiscoveredVia?.GetDescription(),
                    q.IsApprovedForSubsDomain,
                    q.IsApprovedForCustomDomain,
                    q.IsRejected
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalrecords, totalrecords);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="dateFormat"></param>
        /// <param name="timezone"></param>
        /// <param name="keyword"></param>
        /// <param name="daterange"></param>
        /// <param name="employees"></param>
        /// <param name="discoveredVia"></param>
        /// <param name="rights">0-IsApprovedForSubsDomain, 1 - IsApprovedForCustomDomain, 2 - IsRejected</param>
        /// <returns></returns>
        public DataTablesResponse GetDomainCreationRightsLog([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string dateFormat, string timezone,
            string keyword = "", string daterange = "", List<NumberOfEmployees> employees = null, List<DiscoveredVia> discoveredVia = null, List<int> rights = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, daterange, keyword);

                int totalrecords = 0;
                #region Filters
                IQueryable<DomainCreationRightsLog> query = dbContext.DomainCreationRightsLogs;

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(
                        s => s.AssociatedWaitListRequest.User.Email.Contains(keyword)
                        || s.AssociatedWaitListRequest.User.DisplayUserName.Contains(keyword)
                        || s.AssociatedWaitListRequest.User.Surname.Contains(keyword)
                        || s.AssociatedWaitListRequest.User.Forename.Contains(keyword)
                        || s.AssociatedWaitListRequest.CountryName.Contains(keyword));
                }
                if (employees != null)
                {
                    var getEmployEmpty = false;

                    for (int i = 0; i < employees.Count; i++)
                    {
                        if (!Enum.IsDefined(typeof(NumberOfEmployees), employees[i]))
                        {
                            getEmployEmpty = true;
                            break;
                        }
                    }
                    if (getEmployEmpty)
                        query = query.Where(s => employees.Contains((NumberOfEmployees)s.AssociatedWaitListRequest.NumberOfEmployees) || s.AssociatedWaitListRequest.NumberOfEmployees == null);
                    else
                        query = query.Where(s => employees.Contains((NumberOfEmployees)s.AssociatedWaitListRequest.NumberOfEmployees) && s.AssociatedWaitListRequest.NumberOfEmployees != null);
                }
                if (discoveredVia != null)
                {
                    var getDiscoveredVia = false;
                    for (int i = 0; i < discoveredVia.Count; i++)
                    {
                        if (!Enum.IsDefined(typeof(DiscoveredVia), discoveredVia[i]))
                        {
                            getDiscoveredVia = true;
                            break;
                        }
                    }
                    if (getDiscoveredVia)
                        query = query.Where(s => discoveredVia.Contains((DiscoveredVia)s.AssociatedWaitListRequest.DiscoveredVia) || s.AssociatedWaitListRequest.DiscoveredVia == null);
                    else
                        query = query.Where(s => discoveredVia.Contains((DiscoveredVia)s.AssociatedWaitListRequest.DiscoveredVia) && s.AssociatedWaitListRequest.DiscoveredVia != null);
                }
                if (rights != null)
                {
                    if (rights.Count == 1)
                    {
                        if (rights[0] == 1)
                            query = query.Where(s => s.IsApprovedForSubsDomain);

                        else if (rights[0] == 2)
                            query = query.Where(s => s.IsApprovedForCustomDomain);

                        else if (rights[0] == 3)
                            query = query.Where(s => s.IsRejected);
                    }
                    else if (rights.Count == 2)
                    {
                        if (rights.Contains(1) && rights.Contains(2))
                            query = query.Where(s => s.IsApprovedForSubsDomain || s.IsApprovedForCustomDomain);

                        else if (rights.Contains(1) && rights.Contains(3))
                            query = query.Where(s => s.IsApprovedForSubsDomain || s.IsRejected);

                        else if (rights.Contains(2) && rights.Contains(3))
                            query = query.Where(s => s.IsApprovedForCustomDomain || s.IsRejected);
                    }
                }
                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    daterange.ConvertDaterangeFormat(dateFormat, timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }
                totalrecords = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = "";

                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "User":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "AssociatedWaitListRequest.User.Forename" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ",AssociatedWaitListRequest.User.Surname" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "DateTime":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Country":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "AssociatedWaitListRequest.CountryName" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Business":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "AssociatedWaitListRequest.NumberOfEmployees" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "AssociatedWaitListRequest.DiscoveredVia" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "CreatedDate asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == "" ? "CreatedDate asc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion

                var dataJson = list.Select(q => new
                {
                    q.Id,
                    DateTime = q.CreatedDate.ConvertTimeFromUtc(timezone).ToString(dateFormat + " hh:mmtt"),
                    User = q.AssociatedWaitListRequest?.User.GetFullName(),
                    CreatorUri = q.AssociatedWaitListRequest?.User.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T"),
                    Country = q.AssociatedWaitListRequest?.CountryName,
                    Categories = q.AssociatedWaitListRequest == null ? "" : string.Join(", ", q.AssociatedWaitListRequest.BusinessCategories.Select(c => c.Name)),
                    Business = q.AssociatedWaitListRequest?.NumberOfEmployees?.GetDescription(),
                    Discovered = q.AssociatedWaitListRequest?.DiscoveredVia?.GetDescription(),
                    q.IsApprovedForSubsDomain,
                    q.IsApprovedForCustomDomain,
                    q.IsRejected
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalrecords, totalrecords);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="dateFormat"></param>
        /// <param name="timezone"></param>
        /// <param name="keyword"></param>
        /// <param name="daterange"></param>
        /// <param name="rights">0-IsApprovedForSubsDomain, 1 - IsApprovedForCustomDomain, 2 - IsRejected</param>
        /// <returns></returns>
        public DataTablesResponse GetDomainCreationRightsToRevoke([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string dateFormat, string timezone,
            string keyword = "", string daterange = "", List<int> rights = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, daterange, keyword);

                int totalrecords = 0;
                #region Filters
                IQueryable<DomainCreationRights> query = dbContext.DomainCreationRights.Where(e => e.IsApprovedForCustomDomain || e.IsApprovedForSubsDomain);

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(
                        s => s.AssociatedUser.Email.Contains(keyword)
                        || s.AssociatedUser.DisplayUserName.Contains(keyword)
                        || s.AssociatedUser.Surname.Contains(keyword)
                        || s.AssociatedUser.Forename.Contains(keyword)
                        );
                }

                if (rights != null)
                {
                    if (rights.Count == 1)
                    {
                        if (rights[0] == 1)
                            query = query.Where(s => s.IsApprovedForSubsDomain);

                        else if (rights[0] == 2)
                            query = query.Where(s => s.IsApprovedForCustomDomain);
                    }
                    else if (rights.Count == 2)
                    {
                        query = query.Where(s => s.IsApprovedForSubsDomain || s.IsApprovedForCustomDomain);
                    }
                }
                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    daterange.ConvertDaterangeFormat(dateFormat, timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    query = query.Where(s => s.LastModifiedDate >= startDate && s.LastModifiedDate < endDate);
                }
                totalrecords = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = "";

                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "User":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "AssociatedUser.Forename" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ",AssociatedUser.Surname" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "DateTime":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "LastModifiedDate" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "LastModifiedDate asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == "" ? "CreatedDate asc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,//rights Id
                    DateTime = q.LastModifiedDate?.ConvertTimeFromUtc(timezone).ToString(dateFormat + " hh:mmtt"),
                    User = q.AssociatedUser.GetFullName(),
                    CreatorUri = q.AssociatedUser.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T"),
                    q.IsApprovedForSubsDomain,
                    q.IsApprovedForCustomDomain
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalrecords, totalrecords);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keyword);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        /// <summary>
        /// Create a new waitlist on BOTH Join the waitlist and Join to Custom
        /// </summary>
        /// <param name="waitList"></param>
        /// <param name="countryCode"></param>
        /// <param name="originatingConnectionId"></param>
        /// <returns></returns>
        public ReturnJsonModel JoinTheWaitlist(WaitListRequest waitList, CountryCode countryCode, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Join TheWait List", null, null, waitList);

                var userRequest = dbContext.QbicleUser.FirstOrDefault(e => e.Id == waitList.User.Id);

                //if (waitList.BusinessCategories != null && waitList.BusinessCategories.Count > 1)
                //{

                //}

                var bCategoriesId = waitList.BusinessCategories?.Select(e => e.Id).ToList() ?? new List<int>();
                var businessCategories = dbContext.BusinessCategories.Where(e => bCategoriesId.Contains(e.Id)).ToList();

                waitList.BusinessCategories.Clear();
                waitList.BusinessCategories.AddRange(businessCategories);


                waitList.User = userRequest;
                waitList.CountryCode = countryCode;
                waitList.CountryName = new CountriesRules().GetCountryByCode(countryCode).CommonName;
                waitList.CreatedDate = DateTime.UtcNow;
                waitList.LastRequesstdDate = DateTime.UtcNow;

                dbContext.WaitListRequests.Add(waitList);
                dbContext.Entry(waitList).State = EntityState.Added;

                dbContext.SaveChanges();

                var job = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = waitList.Id,
                    AppendToPageName = ApplicationPageName.All,
                    EventNotify = NotificationEventEnum.JoinToWaitlist,
                    Notify2SysAdmin = true
                };
                NotificationToAdminUserJoinToWaitlist(job);

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, waitList);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
            finally
            {
            }
        }

        /// <summary>
        /// activityNotification.Id is waitlist Id
        /// </summary>
        /// <param name="activityNotification"></param>
        private void NotificationToAdminUserJoinToWaitlist(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "jountowaitlist",
                ActivityNotification = activityNotification
            };
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        public void SignalRNotificationToAdminUserJoinToWaitlist(QbicleJobParameter job)
        {
            try
            {
                var waitListRequests = dbContext.WaitListRequests.FirstOrDefault(e => e.Id == job.ActivityNotification.Id);
                List<ApplicationUser> lstReceivers;
                if (job.ActivityNotification.Notify2SysAdmin)
                {
                    var sysAdminRole = dbContext.Roles.FirstOrDefault(x => x.Name == SystemRoles.SystemAdministrator).Id;
                    lstReceivers = dbContext.QbicleUser.Where(p => p.Roles.Any(r => r.RoleId == sysAdminRole)).ToList();
                }
                else
                    lstReceivers = dbContext.QbicleUser.Where(p => p.Id == waitListRequests.User.Id).ToList();

                lstReceivers.ForEach(user =>
                {
                    var notify = new Notification
                    {
                        CreatedBy = waitListRequests.User,
                        CreatedDate = DateTime.UtcNow,
                        SentDate = DateTime.UtcNow,
                        Event = job.ActivityNotification.EventNotify,
                        NotifiedUser = user,
                        AssociatedUser = user,
                        SentMethod = user.ChosenNotificationMethod,
                        IsRead = false,
                        AppendToPageName = job.ActivityNotification.AppendToPageName,
                        AssociatedWaitList = waitListRequests,
                        IsCreatorTheCustomer = false,
                        IsAlertDisplay = true
                    };
                    dbContext.Notifications.Add(notify);
                    dbContext.Entry(notify).State = EntityState.Added;
                    dbContext.SaveChanges();

                    var reason = ReasonSent.WaitlistJoin;
                    var subject = "Your Qbicles waitlist application has been updated";
                    switch (job.ActivityNotification.EventNotify)
                    {
                        case NotificationEventEnum.JoinToWaitlist:
                            reason = ReasonSent.WaitlistJoin;
                            subject = "A new user has joined the Qbicles waitlist";
                            break;
                        case NotificationEventEnum.ApprovalSubscriptionAndCustomWaitlist:
                        case NotificationEventEnum.ApprovalSubscriptionWaitlist:
                        case NotificationEventEnum.ApprovalCustomWaitlist:
                            reason = ReasonSent.WaitlistApproval;
                            break;
                        case NotificationEventEnum.RejectWaitlist:
                            reason = ReasonSent.WaitlistReject;
                            break;
                    }

                    switch (notify.SentMethod)
                    {
                        case NotificationSendMethodEnum.Both:
                        case NotificationSendMethodEnum.Email:
                            WaitlistEmail(reason, user, waitListRequests.User.GetFullName(), subject);
                            SendBroadcastNotification(notify);
                            break;
                        case NotificationSendMethodEnum.Broadcast:
                            SendBroadcastNotification(notify);
                            break;
                    }
                });

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="status">WaitlistJoin, WaitlistApproval, Waitlistreject </param>
        /// <param name="emailTo">WaitlistJoin-sys admin, WaitlistApproval/reject- user</param>
        private void WaitlistEmail(ReasonSent status, ApplicationUser emailTo, string userRequest, string subject)
        {
            var body = CreateWaitlistEmailBody(status, emailTo, userRequest);

            var emailHelper = new EmailHelperRules(dbContext);
            emailHelper.SendEmail("", subject, emailTo.Email, body);

            var mess = new IdentityMessage
            {
                Destination = emailTo.Email,
                Subject = subject, //enum get name from value 
                Body = body.ToString()
            };
            emailHelper.SaveEmailLogNotification(mess, subject, status);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailStatus"></param>
        /// <param name="emailTo"></param>
        /// <returns></returns>
        private AlternateView CreateWaitlistEmailBody(ReasonSent emailStatus, ApplicationUser emailTo, string userRequest)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create email body for invited guest", null, null, emailTo);

                string body;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;

                var emailTemplate = ""; var emailImage = "";
                switch (emailStatus)
                {
                    case ReasonSent.WaitlistJoin:
                        emailTemplate = "_EmailWaitlist.html";
                        emailImage = "waitlistApproval.jpg";
                        break;
                    case ReasonSent.WaitlistApproval:
                        emailTemplate = "_EmailWaitlistApproval.html";
                        emailImage = "waitlistApproval.jpg";
                        break;
                    case ReasonSent.WaitlistReject:
                        emailTemplate = "_EmailWaitlistRejected.html";
                        emailImage = "waitlistReject.jpg";
                        break;
                }


                var pathTemplate = Path.Combine(startupPath, "Templates", emailTemplate);
                var logoPath = Path.Combine(startupPath, "Templates", "logo_200x75.png");
                var headlinePath = Path.Combine(startupPath, "Templates", emailImage);

                using (var reader = new StreamReader(pathTemplate))
                {
                    body = reader.ReadToEnd();
                }

                var logoResource = new LinkedResource(logoPath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                logoResource.ContentType.Name = "Logo.png";
                body = body.Replace("{logo}", "cid:" + logoResource.ContentId);

                var headlineResource = new LinkedResource(headlinePath)
                {
                    ContentId = Guid.NewGuid().ToString()
                };
                headlineResource.ContentType.Name = "hello.png";
                body = body.Replace("{headlineimage}", "cid:" + headlineResource.ContentId);

                body = body.Replace("{BASE_URL}", ConfigManager.QbiclesUrl);

                if (emailStatus == ReasonSent.WaitlistJoin)
                    body = body.Replace("{user_name}", userRequest.ToUpper());

                AlternateView alternateView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(logoResource);
                alternateView.LinkedResources.Add(headlineResource);

                return alternateView;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, emailTo);
                return null;
            }
        }

        public ReturnJsonModel ApprovalSubscriptionCustomRejectDomain(List<int> waitlistIds, NotificationEventEnum action, string userId, string originatingConnectionId = "")
        {
            try
            {
                var waitLists = dbContext.WaitListRequests.Where(e => waitlistIds.Contains(e.Id) && e.IsApprovedForSubsDomain == false && e.IsApprovedForCustomDomain == false && e.IsRejected == false).ToList();
                waitLists.ForEach(waitlist =>
                {
                    //waitlist.IsNewRequest = false;
                    var domainCreationRight = dbContext.DomainCreationRights.FirstOrDefault(e => e.AssociatedUser.Id == waitlist.User.Id);
                    if (domainCreationRight == null)
                    {
                        domainCreationRight = new DomainCreationRights
                        {
                            //AssociatedWaitlistRequest = waitList,
                            CreatedDate = DateTime.UtcNow,
                            IsApprovedForCustomDomain = false,
                            IsApprovedForSubsDomain = false,
                            LastModifiedDate = null,
                            LastModifiedBy = null,
                            AssociatedUser = waitlist.User,
                        };

                        dbContext.DomainCreationRights.Add(domainCreationRight);
                        dbContext.Entry(domainCreationRight).State = EntityState.Added;

                        dbContext.SaveChanges();
                    }
                    else
                    {
                        domainCreationRight.LastModifiedBy = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);
                        domainCreationRight.LastModifiedDate = DateTime.UtcNow;
                    }

                    var domainCreationRightsLog = new DomainCreationRightsLog
                    {
                        AssociatedDomainCreationRightsId = domainCreationRight.Id,
                        AssociatedUserId = waitlist.User.Id,
                        AssociatedWaitListRequest = waitlist,
                        CreatedDate = DateTime.UtcNow,
                        IsApprovedForSubsDomain = false,
                        IsApprovedForSubsDomainLog = false,
                        IsApprovedForCustomDomain = false,
                        IsApprovedForCustomDomainLog = false,
                        IsRejected = waitlist.IsRejected,
                        IsRejectedLog = waitlist.IsRejected,
                        LastModifiedBy = domainCreationRight.LastModifiedBy,
                        LastModifiedDate = DateTime.UtcNow
                    };


                    switch (action)
                    {
                        case NotificationEventEnum.ApprovalSubscriptionAndCustomWaitlist:
                            waitlist.IsApprovedForSubsDomain = true;
                            waitlist.IsApprovedForCustomDomain = true;

                            domainCreationRight.IsApprovedForSubsDomain = true;
                            domainCreationRight.IsApprovedForCustomDomain = true;

                            domainCreationRightsLog.IsApprovedForSubsDomain = true;
                            domainCreationRightsLog.IsApprovedForCustomDomain = true;
                            break;
                        case NotificationEventEnum.ApprovalSubscriptionWaitlist:
                            waitlist.IsApprovedForSubsDomain = true;
                            domainCreationRight.IsApprovedForSubsDomain = true;
                            domainCreationRightsLog.IsApprovedForSubsDomain = true;
                            break;
                        case NotificationEventEnum.ApprovalCustomWaitlist:
                            waitlist.IsApprovedForCustomDomain = true;
                            domainCreationRight.IsApprovedForCustomDomain = true;
                            domainCreationRightsLog.IsApprovedForCustomDomain = true;
                            break;
                        case NotificationEventEnum.RejectWaitlist:
                            waitlist.IsRejected = true;
                            domainCreationRightsLog.IsRejected = true;
                            break;
                    }

                    waitlist.ReviewedBy = domainCreationRight.LastModifiedBy;
                    waitlist.ReviewedDate = DateTime.UtcNow;

                    dbContext.DomainCreationRightsLogs.Add(domainCreationRightsLog);
                    dbContext.Entry(domainCreationRightsLog).State = EntityState.Added;


                    dbContext.SaveChanges();

                    var job = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = waitlist.Id,
                        AppendToPageName = ApplicationPageName.Domain,
                        EventNotify = action,
                        Notify2SysAdmin = false,
                    };
                    NotificationToAdminUserJoinToWaitlist(job);
                });

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, waitlistIds, action);
                return new ReturnJsonModel { result = false };
            }
            finally
            {
            }
        }

        public ReturnJsonModel RevokeWaitlist(int domainCreationRightId, string adminUserId)
        {
            try
            {
                var domainCreationRight = dbContext.DomainCreationRights.FirstOrDefault(e => e.Id == domainCreationRightId);

                var waitlist = dbContext.WaitListRequests.Where(e =>
                e.User.Id == domainCreationRight.AssociatedUser.Id && e.IsRejected == false
                && (e.IsApprovedForCustomDomain || e.IsApprovedForSubsDomain)).OrderByDescending(d => d.ReviewedDate).FirstOrDefault();

                domainCreationRight.IsApprovedForCustomDomain = false;
                domainCreationRight.IsApprovedForSubsDomain = false;

                domainCreationRight.LastModifiedBy = dbContext.QbicleUser.FirstOrDefault(e => e.Id == adminUserId);
                domainCreationRight.LastModifiedDate = DateTime.UtcNow;

                var domainCreationRightsLog = new DomainCreationRightsLog
                {
                    AssociatedDomainCreationRightsId = domainCreationRight.Id,
                    AssociatedUserId = domainCreationRight.AssociatedUser.Id,
                    AssociatedWaitListRequest = waitlist,
                    CreatedDate = DateTime.UtcNow,
                    IsApprovedForSubsDomain = false,
                    IsApprovedForSubsDomainLog = false,
                    IsApprovedForCustomDomain = false,
                    IsApprovedForCustomDomainLog = false,
                    IsRejected = true,
                    LastModifiedBy = domainCreationRight.LastModifiedBy,
                    LastModifiedDate = DateTime.UtcNow
                };

                dbContext.DomainCreationRightsLogs.Add(domainCreationRightsLog);
                dbContext.Entry(domainCreationRightsLog).State = EntityState.Added;


                dbContext.SaveChanges();

                var job = new ActivityNotification
                {
                    OriginatingConnectionId = "",
                    Id = waitlist.Id,
                    AppendToPageName = ApplicationPageName.Domain,
                    EventNotify = NotificationEventEnum.RejectWaitlist,
                    Notify2SysAdmin = false,
                };
                NotificationToAdminUserJoinToWaitlist(job);

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, adminUserId);
                return new ReturnJsonModel { result = false };
            }
            finally
            {
            }
        }

        public List<WaitListRequest> FilterWaitlist(string country, int categoryId, int type)
        {
            if (type == 2)
                return dbContext.WaitListRequests.Where(e => e.IsApprovedForCustomDomain == false && e.IsApprovedForSubsDomain == false && e.IsRejected == false && e.CountryName == country).ToList();

            return dbContext.WaitListRequests.Where(e => e.IsApprovedForCustomDomain == false && e.IsApprovedForSubsDomain == false && e.IsRejected == false && e.BusinessCategories.Any(c => c.Id == categoryId)).ToList();

        }


        public List<WaitListRequest> FilterWaitlist(string ids)
        {
            var idList = ids.Split(',').Select(int.Parse);
            return dbContext.WaitListRequests.Where(e => idList.Contains(e.Id)).ToList();
        }
    }
}
