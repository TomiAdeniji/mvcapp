using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class DomainExtensionRules
    {
        private ApplicationDbContext dbContext;
        public DomainExtensionRules(ApplicationDbContext context)
        {
            this.dbContext = context;
        }

        public DomainExtensionRequest GetExtensionRequestByDomainAndType(int domainId, ExtensionRequestType type)
        {
            var request = dbContext.DomainExtensionRequests.Where(p => p.Domain.Id == domainId && p.Type == type).OrderByDescending(p => p.CreatedDate).FirstOrDefault();
            return request;
        }

        public DomainExtensionRequest GetRequestById(int requestId)
        {
            return dbContext.DomainExtensionRequests.Find(requestId);
        }

        public ReturnJsonModel UpdateExtensionRequestStatus(int requestId, ExtensionRequestStatus rqNewStatus,
            int domainId, string rqAssociatedUserId, string currentUserId, ExtensionRequestType type, string note, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestId, rqNewStatus, domainId, rqAssociatedUserId, currentUserId, type, note);

                var extensionRequest = dbContext.DomainExtensionRequests.Find(requestId);
                var domain = dbContext.Domains.Find(domainId);
                var rqAssociatedUser = dbContext.QbicleUser.Find(rqAssociatedUserId);
                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                if (extensionRequest == null)
                {
                    if (rqNewStatus != ExtensionRequestStatus.Pending || (rqNewStatus == ExtensionRequestStatus.Pending && domain == null))
                    {
                        return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "extension request") };
                    }
                    else
                    {
                        extensionRequest = new DomainExtensionRequest
                        {
                            CreatedBy = rqAssociatedUser,
                            CreatedDate = DateTime.UtcNow,
                            Domain = domain,
                            Note = "",
                            Status = ExtensionRequestStatus.None,
                            Type = type
                        };
                        dbContext.DomainExtensionRequests.Add(extensionRequest);
                        dbContext.Entry(extensionRequest).State = EntityState.Added;

                        domain.ExtensionRequests.Add(extensionRequest);
                        dbContext.Entry(domain).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }
                }

                // Can not update approved or rejected request - unless unsubscribe
                if ((extensionRequest.Status == ExtensionRequestStatus.Approved || extensionRequest.Status == ExtensionRequestStatus.Rejected) && (rqNewStatus != ExtensionRequestStatus.None))
                {
                    var statusStr = extensionRequest.Status == ExtensionRequestStatus.Approved ? "approved" : "rejected";
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "The request has been " + statusStr + " . Update failed!"
                    };
                }

                // Update Extension Request Status
                // If user unsubscribe, do not update the status of the extension request
                if (rqNewStatus != ExtensionRequestStatus.None)
                {
                    extensionRequest.Status = rqNewStatus;
                    extensionRequest.Note = note;
                }

                // If user raise request (status = pending), update createdDate and CreatedBy
                if (rqNewStatus == ExtensionRequestStatus.Pending)
                {
                    extensionRequest.CreatedDate = DateTime.UtcNow;
                    extensionRequest.CreatedBy = currentUser;
                }

                // If extension request is going to be approved/rejected, update when and who approve/reject
                if (extensionRequest.Status == ExtensionRequestStatus.Approved || extensionRequest.Status == ExtensionRequestStatus.Rejected)
                {
                    extensionRequest.ApprovedOrRejectedBy = currentUser;
                    extensionRequest.ApprovedOrRejectedDate = DateTime.UtcNow;
                }

                //If Extension Request is rejected or Extension Subscription is cancelled, create a new Request
                var newRequest = new DomainExtensionRequest();
                if (rqNewStatus == ExtensionRequestStatus.Rejected || rqNewStatus == ExtensionRequestStatus.None)
                {
                    newRequest = new DomainExtensionRequest
                    {
                        Domain = domain,
                        Note = "",
                        Status = ExtensionRequestStatus.None,
                        Type = type,
                        CreatedDate = DateTime.UtcNow
                    };
                    dbContext.DomainExtensionRequests.Add(newRequest);
                    dbContext.Entry(newRequest).State = EntityState.Added;
                }
                dbContext.Entry(extensionRequest).State = EntityState.Modified;
                dbContext.SaveChanges();

                //Send notifications
                if (rqNewStatus == ExtensionRequestStatus.Rejected || rqNewStatus == ExtensionRequestStatus.Approved)
                {
                    var notificationObj = new ActivityNotification()
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        CreatedById = currentUser.Id,
                        HasActionToHandle = false,
                        ReminderMinutes = 0,
                        EventNotify = Notification.NotificationEventEnum.ProcessExtensionRequest,
                        AppendToPageName = Notification.ApplicationPageName.Domain,
                        CreatedByName = currentUser.GetFullName(),
                        Id = requestId
                    };
                    new NotificationRules(dbContext).RaiseExtensionRequestProcessNotification(notificationObj);
                }
                else if (rqNewStatus == ExtensionRequestStatus.Pending)
                {
                    var job = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = extensionRequest.Id
                    };
                    new NotificationRules(dbContext).RaiseNotificationOnExtensionRequestCreated(job);
                }

                return new ReturnJsonModel() { result = true, Object = newRequest.Id };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestId, rqNewStatus, domainId, rqAssociatedUserId, currentUserId, type, note);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public List<DomainExtensionRequestCustomModel> GetListExtensionRequestPagination(IDataTablesRequest requestModel, string keySearch, string dateRange,
            string createdUserIdSearch, ExtensionRequestType requestTypeSearch, List<ExtensionRequestStatus> lstRequestStatusSearch, int domainId, string dateTimeFormat,
            string dateFormat, string timeZone, ref int total, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, keySearch, dateRange, createdUserIdSearch, requestTypeSearch,
                        lstRequestStatusSearch, dateTimeFormat, dateFormat, timeZone);

                var startDate = new DateTime();
                var endDate = new DateTime();
                if (!string.IsNullOrEmpty(dateRange))
                {
                    dateRange.ConvertDaterangeFormat(dateFormat, timeZone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                }
                else
                {
                    startDate = DateTime.UtcNow.Date;
                    endDate = DateTime.UtcNow.Date.AddDays(1);
                }

                var query = from request in dbContext.DomainExtensionRequests select request;

                #region Filter
                if (domainId > 0)
                {
                    query = query.Where(p => p.Domain.Id == domainId);
                }

                if (!string.IsNullOrEmpty(createdUserIdSearch.Trim()))
                {
                    query = query.Where(p => p.CreatedBy != null && p.CreatedBy.Id == createdUserIdSearch);
                }

                if (lstRequestStatusSearch != null && lstRequestStatusSearch.Count > 0)
                {
                    query = query.Where(p => lstRequestStatusSearch.Contains(p.Status));
                }

                if (requestTypeSearch > 0)
                {
                    query = query.Where(p => p.Type == requestTypeSearch);
                }

                query = query.Where(p => p.CreatedDate >= startDate && p.CreatedDate < endDate);

                if (!string.IsNullOrEmpty(keySearch))
                {
                    keySearch = keySearch.ToLower();
                    query = query.Where(p => p.Note.ToLower().Contains(keySearch) ||
                                (p.CreatedBy != null && (p.CreatedBy.Forename.ToLower().Contains(keySearch) || p.CreatedBy.Surname.ToLower().Contains(keySearch))));
                }
                #endregion

                #region Ordering
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var sortedString = "";
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "RequestedDate":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "CreatedDate asc" : "CreatedDate desc";
                            break;
                        case "RequestedByName":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "CreatedBy.SurName asc, CreatedBy.ForeName asc" : "CreatedBy.SurName desc, CreatedBy.ForeName desc";
                            break;
                        case "DomainName":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ",";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "Domain.Name asc" : "Domain.Name desc";
                            break;
                        case "TypeName":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "Type asc" : "Type desc";
                            break;
                        case "Status":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ", ";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "Status asc" : "Status desc";
                            break;
                        case "Note":
                            sortedString += string.IsNullOrEmpty(sortedString) ? "" : ",";
                            sortedString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "Note asc" : "Note desc";
                            break;
                    }
                }
                #endregion
                query = query.OrderBy(string.IsNullOrEmpty(sortedString) ? "CreatedDate desc" : sortedString);
                total = query.Count();

                #region Paging
                query = query.Skip(start).Take(length);
                #endregion

                var lstExtensionRequest = query.ToList();
                var resultList = new List<DomainExtensionRequestCustomModel>();

                lstExtensionRequest.ForEach(p =>
                {
                    var customModel = new DomainExtensionRequestCustomModel()
                    {
                        RequestId = p.Id,
                        DomainId = p.Domain?.Id ?? 0,
                        DomainKey = p.Domain?.Key ?? "",
                        DomainLogoUri = p.Domain?.LogoUri.ToDocumentUri().ToString() ?? "",
                        DomainName = p.Domain?.Name ?? "",
                        ReqeustedById = p.CreatedBy?.Id ?? "",
                        RequestedByName = p.CreatedBy?.GetFullName() ?? "",
                        RequestedByLogoUri = p.CreatedBy?.ProfilePic.ToDocumentUri().ToString() ?? "",
                        RequestedDate = p.CreatedDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                        Status = p.Status,
                        StatusLabel = GetExtensionStatusLabel(p.Status),
                        Type = p.Type,
                        TypeName = p.Type.ToString(),
                        Note = p.Status == ExtensionRequestStatus.Rejected ? p.Note : "--"
                    };
                    resultList.Add(customModel);
                });
                return resultList;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, keySearch, dateRange, createdUserIdSearch, requestTypeSearch,
                        lstRequestStatusSearch, dateTimeFormat, dateFormat, timeZone);
                return new List<DomainExtensionRequestCustomModel>();
            }
        }

        public string GetExtensionStatusLabel(ExtensionRequestStatus status)
        {
            var lblStr = "";
            switch (status)
            {
                case ExtensionRequestStatus.Rejected:
                    lblStr = "<label class=\"label label-lg label-danger\">Rejected</label>";
                    break;
                case ExtensionRequestStatus.Approved:
                    lblStr = "<label class=\"label label-lg label-success\">Approved</label>";
                    break;
                default:
                    lblStr = "";
                    break;
            }

            return lblStr;
        }

        public List<ApplicationUser> GetListExtensionRequestCreators(int domainId)
        {
            var lstCreator = dbContext.DomainExtensionRequests
                                .Where(p => (domainId == 0 || (domainId != 0 && p.Domain != null && p.Domain.Id == domainId)) && p.CreatedBy != null)
                                .Select(p => p.CreatedBy).Distinct().ToList();

            return lstCreator;
        }
    }
}
