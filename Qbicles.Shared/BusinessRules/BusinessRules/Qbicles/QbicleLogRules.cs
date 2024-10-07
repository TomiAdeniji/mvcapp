using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class QbicleLogRules
    {
        ApplicationDbContext dbContext;
        public QbicleLogRules()
        {

        }
        public QbicleLogRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public void SaveQbicleLog(QbicleLog log)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save qbicle log", null, null, log);

                log.SessionId = HelperClass.GetCurrentSessionID();
                log.IPAddress = HelperClass.GetIPAddress();
                dbContext.QbicleLogs.Add(log);
                dbContext.Entry(log).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, log);
            }
        }

        public void SaveDomainAccessLog(DomainAccessLog log)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save domain access log", null, null, log);

                log.SessionId = HelperClass.GetCurrentSessionID();
                log.IPAddress = HelperClass.GetIPAddress();
                dbContext.DomainAccessLogs.Add(log);
                dbContext.Entry(log).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, log);
            }
        }

        public void SaveQbicleAccessLog(QbicleAccessLog log)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save qbicle access log", null, null, log);

                log.SessionId = HelperClass.GetCurrentSessionID();
                log.IPAddress = HelperClass.GetIPAddress();
                dbContext.QbicleAccessLogs.Add(log);
                dbContext.Entry(log).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, log);
            }
        }

        public void SaveAppAccessLog(AppAccessLog log)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save app access log", null, null, log);

                log.SessionId = HelperClass.GetCurrentSessionID();
                log.IPAddress = HelperClass.GetIPAddress();
                dbContext.AppAccessLogs.Add(log);
                dbContext.Entry(log).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, log);
            }
        }

        public List<LogModel> GetLogs(string searchAll, string dateRangeLog, string sessionId, int domainId, int appType, int action, int column, string orderBy, int start, int length, ref int totalRecord, string currentDateFormat, string currentDatetimeFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get logs", null, null, searchAll, dateRangeLog, sessionId, domainId, appType, action, column, orderBy, start, length, totalRecord, currentDateFormat, currentDatetimeFormat);
               
                var queryCommon = from log in dbContext.QbicleLogs
                                  where log.LogType != QbicleLogType.AppAccess && log.LogType != QbicleLogType.DomainAccess && log.LogType != QbicleLogType.QbicleAccess
                                  select new TempLogModel
                                  {
                                      Id = log.Id,
                                      UserId = log.UserId,
                                      CreatedDate = log.CreatedDate,
                                      SessionId = log.SessionId,
                                      IPAddress = log.IPAddress,
                                      Action = log.LogType,
                                      AppType = AppType.Core,
                                      DomainId = 0,
                                      QbicleId = 0
                                  };

                var queryDomainLog = from log in dbContext.DomainAccessLogs
                                     select new TempLogModel
                                     {
                                         Id = log.Id,
                                         UserId = log.UserId,
                                         CreatedDate = log.CreatedDate,
                                         SessionId = log.SessionId,
                                         IPAddress = log.IPAddress,
                                         Action = log.LogType,
                                         AppType = AppType.Core,
                                         DomainId = log.DomainId,
                                         QbicleId = 0
                                     };
                var queryQbicleLog = from log in dbContext.QbicleAccessLogs
                                     select new TempLogModel
                                     {
                                         Id = log.Id,
                                         UserId = log.UserId,
                                         CreatedDate = log.CreatedDate,
                                         SessionId = log.SessionId,
                                         IPAddress = log.IPAddress,
                                         Action = log.LogType,
                                         AppType = AppType.Core,
                                         DomainId = log.DomainId ?? 0,
                                         QbicleId = log.QbicleId
                                     };
                var queryAppLog = from log in dbContext.AppAccessLogs
                                  select new TempLogModel
                                  {
                                      Id = log.Id,
                                      UserId = log.UserId,
                                      CreatedDate = log.CreatedDate,
                                      SessionId = log.SessionId,
                                      IPAddress = log.IPAddress,
                                      Action = log.LogType,
                                      AppType = log.Type,
                                      DomainId = log.DomainId,
                                      QbicleId = 0
                                  };

                var query = queryCommon.Union(queryDomainLog).Union(queryQbicleLog).Union(queryAppLog);

                if (!String.IsNullOrEmpty(dateRangeLog))
                {
                    string[] splitDate = dateRangeLog.Split('-');
                    var startDate = DateTime.ParseExact(splitDate[0].Trim(), currentDateFormat, new CultureInfo("en-US"));
                    var endDate = DateTime.ParseExact(splitDate[1].Trim(), currentDateFormat, new CultureInfo("en-US")).AddDays(1);
                    query = query.Where(q => q.CreatedDate >= startDate && q.CreatedDate < endDate);
                }

                if (!string.IsNullOrEmpty(sessionId))
                {
                    query = query.Where(q => q.SessionId.Contains(sessionId.Trim()));
                }

                if (domainId != 0)
                {
                    query = query.Where(q => q.DomainId == domainId);
                }

                if (appType != 0)
                {
                    query = query.Where(q => q.AppType == (AppType)appType);
                }

                if (action != 0)
                {
                    query = query.Where(q => q.Action == (QbicleLogType)action);
                }

                totalRecord = query.Count();

                var tempQuery = from q in query
                                join u in dbContext.QbicleUser on q.UserId equals u.Id into gu
                                from su in gu.DefaultIfEmpty()
                                join d in dbContext.Domains on q.DomainId equals d.Id into gd
                                from sd in gd.DefaultIfEmpty()
                                join qb in dbContext.Qbicles on q.QbicleId equals qb.Id into gqb
                                from sqb in gqb.DefaultIfEmpty()
                                select new LogModel
                                {
                                    Id = q.Id,
                                    CreatedDate = q.CreatedDate,
                                    SessionID = q.SessionId,
                                    IPAddress = q.IPAddress,
                                    User = su.UserName,
                                    Domain = sd != null ? sd.Name : "",
                                    Qbicle = sqb != null ? sqb.Name : "",
                                    App =
                                    (
                                      q.AppType == AppType.Bookkeeping ? "Bookkeeping" :
                                      q.AppType == AppType.Cleanbooks ? "Cleanbooks" :
                                      q.AppType == AppType.SalesMarketing ? "Sales & Marketing" :
                                      q.AppType == AppType.Trader ? "Trader" :
                                      q.AppType == AppType.Spannered ? "Spannered" :
                                      "Core"
                                    ),
                                    Action =
                                    (
                                      q.Action == QbicleLogType.Login ? "Log in" :
                                      q.Action == QbicleLogType.Logout ? "Log out" :
                                      q.Action == QbicleLogType.PasswordReset ? "Password reset" :
                                      q.Action == QbicleLogType.DomainAccess ? "Domain access" :
                                      q.Action == QbicleLogType.QbicleAccess ? "Qbicle access" :
                                      q.Action == QbicleLogType.MyDeskAccess ? "MyDesk access" :
                                      q.Action == QbicleLogType.AppAccess ? "App access" :
                                      ""
                                    )
                                };

                if (!string.IsNullOrEmpty(searchAll))
                {
                    tempQuery = tempQuery.Where(t => t.SessionID.ToLower().Contains(searchAll.Trim().ToLower()) ||
                                                    t.IPAddress.ToLower().Contains(searchAll.Trim().ToLower()) ||
                                                    t.User.ToLower().Contains(searchAll.Trim().ToLower()) ||
                                                    t.Domain.ToLower().Contains(searchAll.Trim().ToLower()) ||
                                                    t.Qbicle.ToLower().Contains(searchAll.Trim().ToLower()) ||
                                                    t.App.ToLower().Contains(searchAll.Trim().ToLower()) ||
                                                    t.Action.ToLower().Contains(searchAll.Trim().ToLower())
                     );
                }

                if (column == 0)
                {
                    if (orderBy.Equals("asc"))
                    {
                        tempQuery = tempQuery.OrderBy(p => p.Id);
                    }
                    else
                    {
                        tempQuery = tempQuery.OrderByDescending(p => p.Id);
                    }
                }

                if (column == 1)
                {
                    if (orderBy.Equals("asc"))
                    {
                        tempQuery = tempQuery.OrderBy(p => p.SessionID);
                    }
                    else
                    {
                        tempQuery = tempQuery.OrderByDescending(p => p.SessionID);
                    }
                }

                if (column == 2)
                {
                    if (orderBy.Equals("asc"))
                    {
                        tempQuery = tempQuery.OrderBy(p => p.Domain);
                    }
                    else
                    {
                        tempQuery = tempQuery.OrderByDescending(p => p.Domain);
                    }
                }

                if (column == 3)
                {
                    if (orderBy.Equals("asc"))
                    {
                        tempQuery = tempQuery.OrderBy(p => p.Qbicle);
                    }
                    else
                    {
                        tempQuery = tempQuery.OrderByDescending(p => p.Qbicle);
                    }
                }

                if (column == 4)
                {
                    if (orderBy.Equals("asc"))
                    {
                        tempQuery = tempQuery.OrderBy(p => p.User);
                    }
                    else
                    {
                        tempQuery = tempQuery.OrderByDescending(p => p.User);
                    }
                }

                if (column == 5)
                {
                    if (orderBy.Equals("asc"))
                    {
                        tempQuery = tempQuery.OrderBy(p => p.IPAddress);
                    }
                    else
                    {
                        tempQuery = tempQuery.OrderByDescending(p => p.IPAddress);
                    }
                }

                if (column == 6)
                {
                    if (orderBy.Equals("asc"))
                    {
                        tempQuery = tempQuery.OrderBy(p => p.App);
                    }
                    else
                    {
                        tempQuery = tempQuery.OrderByDescending(p => p.App);
                    }
                }

                if (column == 7)
                {
                    if (orderBy.Equals("asc"))
                    {
                        tempQuery = tempQuery.OrderBy(p => p.Action);
                    }
                    else
                    {
                        tempQuery = tempQuery.OrderByDescending(p => p.Action);
                    }
                }

                var result = new List<LogModel>();
                if (length != 0)
                {
                    result = tempQuery.Skip(start).Take(length).ToList();
                }
                else
                {
                    result = tempQuery.ToList();
                }

                if (result != null)
                {
                    result.ForEach(x => { x.StrCreatedDate = x.CreatedDate.ToString(currentDatetimeFormat); });
                }

                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, searchAll, dateRangeLog, sessionId, domainId, appType, action, column, orderBy, start, length, totalRecord, currentDateFormat, currentDatetimeFormat);
                return new List<LogModel>();
            }
        }

    }
}
