using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Operator.Goals;
using Qbicles.Models.Operator.TimeAttendance;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Linq.Dynamic;
using static Qbicles.BusinessRules.Model.TB_Column;
using Qbicles.BusinessRules.Helper;
using static Qbicles.Models.Notification;
using System.Reflection;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorAttendanceRules
    {
        ApplicationDbContext dbContext;
        public OperatorAttendanceRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public Attendance GetAttendanceById(int id)
        {
            try
            {
                return dbContext.OperatorAttendances.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new Attendance();
            }
        }
        public ReturnJsonModel SaveClock(OperatorClockIn operatorClock, UserSetting userSetting, string originatingConnectionId = "")
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var dbworkgroup = dbContext.OperatorWorkGroups.Find(operatorClock.Workgroup);
                if (dbworkgroup == null)
                {
                    returnJson.msg = "ERROR_MSG_168";
                    return returnJson;
                }
                if (!new OperatorWorkgroupRules(dbContext).checkIsManagerOrSupervisor(dbworkgroup.Domain.Id, userSetting.Id))
                {
                    returnJson.msg = "ERROR_MSG_28";
                    return returnJson;
                }


                var currentUser = dbContext.QbicleUser.Find(userSetting.Id);


                var lstApprovals = new List<ApprovalReq>();
                foreach (var item in operatorClock.Peoples)
                {
                    Attendance attendance = new Attendance
                    {
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow
                    };
                    if (!string.IsNullOrEmpty(operatorClock.Date))
                    {
                        attendance.Date = operatorClock.Date.ConvertDateFormat(userSetting.DateFormat).ConvertTimeToUtc(userSetting.Timezone);
                    }
                    else
                        attendance.Date = DateTime.UtcNow;
                    attendance.WorkGroup = dbworkgroup;
                    if (!string.IsNullOrEmpty(operatorClock.Date) && !string.IsNullOrEmpty(operatorClock.Time))
                    {
                        attendance.TimeIn = (operatorClock.Date + " " + operatorClock.Time).ConvertDateFormat(userSetting.DateFormat + " HH:mm").ConvertTimeToUtc(userSetting.Timezone);
                    }
                    else
                        attendance.TimeIn = DateTime.UtcNow;
                    attendance.Notes = operatorClock.Notes;
                    attendance.TimeOut = null;
                    attendance.People = dbContext.QbicleUser.Find(item);
                    #region add AprovalReq
                    var approvalRD = new ApprovalRequestDefinition
                    {
                        CreatedBy = attendance.People,
                        CreatedDate = DateTime.UtcNow,
                        Description = attendance.Notes,
                        Title = $"{HelperClass.GetFullNameOfUser(attendance.People)} clocked in",
                        ApprovalImage = "",
                        Group = null,
                        Type = ApprovalRequestDefinition.RequestTypeEnum.General
                    };
                    ApprovalReq approval = new ApprovalReq()
                    {
                        StartedBy = attendance.People,
                        StartedDate = attendance.CreatedDate,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        Topic = dbworkgroup.DefaultTopic,
                        Notes = operatorClock.Notes,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Reviewed,
                        ApprovalRequestDefinition = approvalRD,
                        Qbicle = dbworkgroup.SourceQbicle,
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequestApp,
                        TimeLineDate = DateTime.UtcNow,
                        Name = approvalRD.Title,
                        App = QbicleActivity.ActivityApp.Operator
                    };
                    dbContext.Entry(attendance).State = EntityState.Added;
                    attendance.ApprovalTimeIn = approval;
                    dbContext.OperatorAttendances.Add(attendance);
                    lstApprovals.Add(approval);
                    #endregion
                }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                var nRule = new NotificationRules(dbContext);
                foreach (var item in lstApprovals)
                {
                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = item.Id,
                        EventNotify = Notification.NotificationEventEnum.ApprovalCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userSetting.Id,
                        CreatedByName = userSetting.DisplayName,
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return returnJson;
        }
        public ReturnJsonModel UpdateClockOut(int id, string clockIn, string clockOut, string timezone, string dateformat, int domainId, string currentId, string originatingConnectionId = "")
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var attendance = dbContext.OperatorAttendances.Find(id);
                if (attendance != null)
                {
                    if (!new OperatorWorkgroupRules(dbContext).checkIsManagerOrSupervisor(domainId, currentId))
                    {
                        returnJson.msg = "ERROR_MSG_28";
                        return returnJson;
                    }
                    attendance.TimeIn = (attendance.Date.ConvertTimeFromUtc(timezone).ToString(dateformat) + " " + clockIn).ConvertDateFormat(dateformat + " HH:mm").ConvertTimeToUtc(timezone);
                    attendance.TimeOut = (attendance.Date.ConvertTimeFromUtc(timezone).ToString(dateformat) + " " + clockOut).ConvertDateFormat(dateformat + " HH:mm").ConvertTimeToUtc(timezone);
                    ApprovalReq approval = null;
                    if (attendance.ApprovalTimeOut == null)
                    {
                        #region add AprovalReq
                        var approvalRD = new ApprovalRequestDefinition
                        {
                            CreatedBy = attendance.People,
                            CreatedDate = DateTime.UtcNow,
                            Description = attendance.Notes,
                            Title = $"{HelperClass.GetFullNameOfUser(attendance.People)} clocked out",
                            ApprovalImage = "",
                            Group = null,
                            Type = ApprovalRequestDefinition.RequestTypeEnum.General
                        };
                        approval = new ApprovalReq()
                        {
                            StartedBy = attendance.People,
                            StartedDate = attendance.CreatedDate,
                            State = QbicleActivity.ActivityStateEnum.Open,
                            Topic = attendance.WorkGroup.DefaultTopic,
                            Notes = attendance.Notes,
                            RequestStatus = ApprovalReq.RequestStatusEnum.Reviewed,
                            ApprovalRequestDefinition = approvalRD,
                            Qbicle = attendance.WorkGroup.SourceQbicle,
                            ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequestApp,
                            TimeLineDate = DateTime.UtcNow,
                            Name = approvalRD.Title,
                            App = QbicleActivity.ActivityApp.Operator
                        };
                        dbContext.Entry(attendance).State = EntityState.Modified;
                        attendance.ApprovalTimeOut = approval;
                        #endregion
                    }

                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    if (approval != null)
                    {
                        var nRule = new NotificationRules(dbContext);
                        var activityNotification = new ActivityNotification
                        {
                            OriginatingConnectionId = originatingConnectionId,
                            Id = approval.Id,
                            EventNotify = NotificationEventEnum.ApprovalCreation,
                            AppendToPageName = ApplicationPageName.Activities,
                            AppendToPageId = 0,
                            CreatedById = currentId,
                            CreatedByName = HelperClass.GetFullNameOfUser(approval.StartedBy),
                            ReminderMinutes = 0
                        };
                        nRule.Notification2Activity(activityNotification);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return returnJson;
        }
        public DataTablesResponse SearchAttendances([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string search, string filterdate, List<string> peoples, int domainId, string dateFormat, string timezone)
        {
            try
            {
                var query = dbContext.OperatorAttendances.Where(t => t.WorkGroup.Domain.Id == domainId).AsQueryable();
                int totalAttendance = 0;
                #region Filter
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(q => (q.People.Forename + " " + q.People.Surname).Contains(search));
                if (!string.IsNullOrEmpty(filterdate))
                {
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    filterdate.ConvertDaterangeFormat(dateFormat, timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    query = query.Where(s => s.TimeIn >= startDate && s.TimeIn < endDate);
                }
                if (peoples != null && peoples.Any())
                {
                    query = query.Where(q => peoples.Any(s => s == q.People.Id));
                }
                totalAttendance = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Person":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "People.Forename" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += ",People.Surname" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Date" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Location":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "WorkGroup.Location.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "TimeOut":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TimeOut" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "TimeIn":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TimeIn" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Date desc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "Date desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Person = HelperClass.GetFullNameOfUser(q.People),
                    PersonId = q.People.Id,
                    PersonUrl = q.People.ProfilePic.ToUriString(),
                    Date = q.Date.ConvertTimeFromUtc(timezone).ToString(dateFormat),
                    Location = q.WorkGroup.Location.Name,
                    TimeIn = q.TimeIn.ConvertTimeFromUtc(timezone).ToString("HH:mm"),
                    TimeOut = q.TimeOut == null ? "" : q.TimeOut.Value.ConvertTimeFromUtc(timezone).ToString("HH:mm")
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalAttendance, totalAttendance);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
    }
}
