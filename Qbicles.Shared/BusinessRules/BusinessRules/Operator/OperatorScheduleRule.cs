using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Operator;
using Qbicles.Models.Operator.Team;
using Qbicles.Models.Operator.TimeAttendance;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorScheduleRule
    {
        ApplicationDbContext dbContext;
        public OperatorScheduleRule(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public ReturnJsonModel SaveSchedule(OperatorScheduleModel scheduleModel, UserSetting userSetting)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var currentDate = DateTime.UtcNow;

                if (scheduleModel.Days != null)
                    foreach (var day in scheduleModel.Days)
                    {
                        foreach (var empId in scheduleModel.Employees)
                        {
                            var _employee = dbContext.QbicleUser.Find(empId);
                            if (_employee != null)
                            {
                                Schedule schedule = new Schedule();
                                schedule.WorkGroup = dbContext.OperatorWorkGroups.Find(scheduleModel.Workgroup);
                                schedule.Domain = scheduleModel.Domain;
                                schedule.Employee = _employee;
                                schedule.CreatedBy = dbContext.QbicleUser.Find(userSetting.Id);
                                schedule.CreatedDate = currentDate;
                                if (!string.IsNullOrEmpty(scheduleModel.ShiftStart))
                                {
                                    schedule.StartDate = (day + " " + scheduleModel.ShiftStart).ConvertDateFormat(userSetting.DateFormat + " HH:mm").ConvertTimeToUtc(userSetting.Timezone);
                                }
                                if (!string.IsNullOrEmpty(scheduleModel.ShiftEnd))
                                {
                                    schedule.EndDate = (day + " " + scheduleModel.ShiftEnd).ConvertDateFormat(userSetting.DateFormat + " HH:mm").ConvertTimeToUtc(userSetting.Timezone);
                                }
                                dbContext.Entry(schedule).State = EntityState.Added;
                                dbContext.OperatorSchedules.Add(schedule);
                            }
                        }
                    }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return returnJson;
        }
        public ReturnJsonModel UpdateSchedule(OperatorScheduleUpdateModel scheduleUpdateModel)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var schedule = dbContext.OperatorSchedules.Find(scheduleUpdateModel.Id);
                if (schedule != null)
                {
                    var _tempDay = schedule.StartDate.ConvertTimeFromUtc(scheduleUpdateModel.Timezone);
                    schedule.StartDate = (_tempDay.ToString(scheduleUpdateModel.Dateformat) + " " + scheduleUpdateModel.ShiftStart).ConvertDateFormat(scheduleUpdateModel.Dateformat + " HH:mm").ConvertTimeToUtc(scheduleUpdateModel.Timezone);
                    schedule.EndDate = (_tempDay.ToString(scheduleUpdateModel.Dateformat) + " " + scheduleUpdateModel.ShiftEnd).ConvertDateFormat(scheduleUpdateModel.Dateformat + " HH:mm").ConvertTimeToUtc(scheduleUpdateModel.Timezone);
                }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return returnJson;
        }
        public Schedule GetScheduleById(int id)
        {
            try
            {
                return dbContext.OperatorSchedules.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new Schedule();
            }
        }
        public DataTablesResponse SearchTimesheetsDaily(OperatorSearchScheduleModel operatorSearch)
        {
            try
            {
                var _startDate = operatorSearch.Day.ConvertDateFormat(operatorSearch.Dateformat).ConvertTimeToUtc(operatorSearch.Timezone);
                var _endDate = _startDate.AddDays(1);
                int totalcount = 0;
                #region Filter
                var _querymembers = dbContext.OperatorTeamPersons.Where(s => s.Domain.Id == operatorSearch.DomainId && s.User.TeamMembers.Any(p => p.TeamPermission == TeamPermissionTypeEnum.Member));
                if (operatorSearch.Peoples != null)
                {
                    _querymembers = _querymembers.Where(s => operatorSearch.Peoples.Contains(s.User.Id));
                }
                if (operatorSearch.Roles != null)
                {
                    _querymembers = _querymembers.Where(s => s.Roles.Any(r => operatorSearch.Roles.Contains(r.Id)));
                }
                if (operatorSearch.Locations != null)
                {
                    _querymembers = _querymembers.Where(s => s.Locations.Any(r => operatorSearch.Locations.Contains(r.Id)));
                }
                var members = _querymembers.ToList().Select(s => new OperatorEmployeeModel() { Id = s.User.Id, Location = string.Join(", ", s.Locations.Select(l => l.Name)) }).ToList();
                var schedules = dbContext.OperatorAttendances.Where(t => t.WorkGroup.Domain.Id == operatorSearch.DomainId && t.TimeIn >= _startDate && t.TimeIn < _endDate && t.TimeOut != null).ToList();
                List<Attendance> lstSchedules;
                if (operatorSearch.Peoples != null || operatorSearch.Roles != null || operatorSearch.Locations != null)
                    lstSchedules = (from q in schedules
                                    join m in members on q.People.Id equals m.Id
                                    select q).ToList();
                else
                    lstSchedules = schedules;

                #endregion
                List<OperatorScheduleDailyModel> lstDaily = new List<OperatorScheduleDailyModel>();
                foreach (var item in lstSchedules)
                {
                    var _slocation = members.FirstOrDefault(s => s.Id == item.People.Id);
                    OperatorScheduleDailyModel dailyModel = new OperatorScheduleDailyModel();
                    dailyModel.Id = item.Id;
                    dailyModel.PersonId = item.People.Id;
                    dailyModel.PersonName = HelperClass.GetFullNameOfUser(item.People);
                    dailyModel.PersonUrl = item.People.ProfilePic.ToUriString();
                    dailyModel.PersonJobtile = item.People.JobTitle;
                    dailyModel.Shift = $"{item.TimeIn.ConvertTimeFromUtc(operatorSearch.Timezone).ToString("hh:mmtt")} - {item.TimeOut.Value.ConvertTimeFromUtc(operatorSearch.Timezone).ToString("hh:mmtt")} ({(item.TimeOut.Value - item.TimeIn).TotalHours} hours)";
                    dailyModel.Location = _slocation.Location;
                    lstDaily.Add(dailyModel);
                }
                totalcount = lstDaily.Count();
                return new DataTablesResponse(0, lstDaily, totalcount, totalcount);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(0, string.Empty, 0, 0);
            }
        }
        public DataTable SearchTimesheetsWeekly(OperatorSearchScheduleModel operatorSearch)
        {
            try
            {
                var _startDate = DateTime.UtcNow;
                var _endDate = DateTime.UtcNow;
                operatorSearch.Week.ConvertDaterangeFormat(operatorSearch.Dateformat, operatorSearch.Timezone, out _startDate, out _endDate, HelperClass.endDateAddedType.day);
                #region Filter
                var _querymembers = dbContext.OperatorTeamPersons.Where(s => s.Domain.Id == operatorSearch.DomainId && s.User.TeamMembers.Any(p => p.TeamPermission == TeamPermissionTypeEnum.Member));
                if (operatorSearch.Peoples != null)
                {
                    _querymembers = _querymembers.Where(s => operatorSearch.Peoples.Contains(s.User.Id));
                }
                if (operatorSearch.Roles != null)
                {
                    _querymembers = _querymembers.Where(s => s.Roles.Any(r => operatorSearch.Roles.Contains(r.Id)));
                }
                if (operatorSearch.Locations != null)
                {
                    _querymembers = _querymembers.Where(s => s.Locations.Any(r => operatorSearch.Locations.Contains(r.Id)));
                }
                var members = _querymembers.ToList().Select(s => new OperatorEmployeeModel() { Id = s.User.Id, Location = string.Join(", ", s.Locations.Select(l => l.Name)) }).ToList();
                var schedules = dbContext.OperatorAttendances.Where(t => t.WorkGroup.Domain.Id == operatorSearch.DomainId && t.TimeIn >= _startDate && t.TimeIn < _endDate && t.TimeOut != null).ToList();
                IEnumerable<OperatorScheduleWeekModel> lstSchedules;
                if (operatorSearch.Peoples != null || operatorSearch.Roles != null || operatorSearch.Locations != null)
                    lstSchedules = (from q in schedules
                                    join m in members on q.People.Id equals m.Id
                                    select q).Select(s => new OperatorScheduleWeekModel
                                    {
                                        PersonId = s.People.Id,
                                        PersonName = HelperClass.GetFullNameOfUser(s.People),
                                        PersonUrl = s.People.ProfilePic.ToUriString(),
                                        PersonJobtile = s.People.JobTitle,
                                        LabelColumn = $"{s.TimeIn.ConvertTimeFromUtc(operatorSearch.Timezone).ToString("ddd dd")}{HelperClass.Converter.OrdinalSuffix(s.TimeIn.ConvertTimeFromUtc(operatorSearch.Timezone).Day)}",
                                        Value = (s.TimeOut.Value - s.TimeIn).TotalHours,
                                    });
                else
                    lstSchedules = (from q in schedules
                                    select q).Select(s => new OperatorScheduleWeekModel
                                    {
                                        PersonId = s.People.Id,
                                        PersonName = HelperClass.GetFullNameOfUser(s.People),
                                        PersonUrl = s.People.ProfilePic.ToUriString(),
                                        PersonJobtile = s.People.JobTitle,
                                        LabelColumn = $"{s.TimeIn.ConvertTimeFromUtc(operatorSearch.Timezone).ToString("ddd dd")}{HelperClass.Converter.OrdinalSuffix(s.TimeIn.ConvertTimeFromUtc(operatorSearch.Timezone).Day)}",
                                        Value = (s.TimeOut.Value - s.TimeIn).TotalHours,
                                    });
                lstSchedules = lstSchedules.GroupBy(g => new
                {
                    g.PersonId,
                    g.PersonName,
                    g.PersonJobtile,
                    g.PersonUrl,
                    g.LabelColumn
                }).Select(
                s => new OperatorScheduleWeekModel
                {
                    PersonId = s.Key.PersonId,
                    PersonName = s.Key.PersonName,
                    PersonUrl = s.Key.PersonUrl,
                    PersonJobtile = s.Key.PersonJobtile,
                    LabelColumn = s.Key.LabelColumn,
                    Value = s.Sum(p => p.Value)
                });
                #endregion

                #region Datatable
                DataTable table = new DataTable();
                table.Columns.Add("PersonId");
                table.Columns.Add("PersonName");
                table.Columns.Add("PersonUrl");
                table.Columns.Add("PersonJobtile");
                var _tzstartDate = _startDate.ConvertTimeFromUtc(operatorSearch.Timezone);
                var _tzendDate = _endDate.ConvertTimeFromUtc(operatorSearch.Timezone);
                var _columnDynamic = new List<string>();
                while (_tzstartDate < _tzendDate)
                {
                    string columname = $"{_tzstartDate.ToString("ddd dd")}{HelperClass.Converter.OrdinalSuffix(_tzstartDate.Day)}";
                    table.Columns.Add(columname);
                    _columnDynamic.Add(columname);
                    _tzstartDate = _tzstartDate.AddDays(1);
                }
                var rows = lstSchedules.GroupBy(s => new { s.PersonId, s.PersonName, s.PersonUrl, s.PersonJobtile }).Select(x => x.First());
                foreach (var sched in rows)
                {
                    var dataRow = table.NewRow();
                    dataRow["PersonId"] = sched.PersonId;
                    dataRow["PersonName"] = sched.PersonName;
                    dataRow["PersonUrl"] = sched.PersonUrl;
                    dataRow["PersonJobtile"] = sched.PersonJobtile;
                    foreach (var clname in _columnDynamic)
                    {
                        var sc = lstSchedules.FirstOrDefault(s => s.PersonId == sched.PersonId && s.LabelColumn == clname);
                        dataRow[clname] = sc?.Value ?? 0;
                    }
                    table.Rows.Add(dataRow);
                }
                #endregion
                return table;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public DataTable SearchTimesheetsMonthly(OperatorSearchScheduleModel operatorSearch)
        {
            try
            {
                var _startDate = DateTime.UtcNow;
                var _endDate = DateTime.UtcNow;
                operatorSearch.Week.ConvertDaterangeFormat(operatorSearch.Dateformat, operatorSearch.Timezone, out _startDate, out _endDate);
                var _tzstartDate = _startDate.ConvertTimeFromUtc(operatorSearch.Timezone);
                var _tzendDate = _endDate.ConvertTimeFromUtc(operatorSearch.Timezone);
                #region Filter
                var _querymembers = dbContext.OperatorTeamPersons.Where(s => s.Domain.Id == operatorSearch.DomainId && s.User.TeamMembers.Any(p => p.TeamPermission == TeamPermissionTypeEnum.Member));
                if (operatorSearch.Peoples != null)
                {
                    _querymembers = _querymembers.Where(s => operatorSearch.Peoples.Contains(s.User.Id));
                }
                if (operatorSearch.Roles != null)
                {
                    _querymembers = _querymembers.Where(s => s.Roles.Any(r => operatorSearch.Roles.Contains(r.Id)));
                }
                if (operatorSearch.Locations != null)
                {
                    _querymembers = _querymembers.Where(s => s.Locations.Any(r => operatorSearch.Locations.Contains(r.Id)));
                }
                var members = _querymembers.ToList().Select(s => new OperatorEmployeeModel() { Id = s.User.Id, Location = string.Join(", ", s.Locations.Select(l => l.Name)) }).ToList();
                var schedules = dbContext.OperatorAttendances.Where(t => t.WorkGroup.Domain.Id == operatorSearch.DomainId && t.TimeIn >= _startDate && t.TimeIn < _endDate && t.TimeOut != null).ToList();
                IEnumerable<OperatorScheduleWeekModel> lstSchedules;
                if (operatorSearch.Peoples != null || operatorSearch.Roles != null || operatorSearch.Locations != null)
                    lstSchedules = (from q in schedules
                                    join m in members on q.People.Id equals m.Id
                                    select q).Select(s => new OperatorScheduleWeekModel
                                    {
                                        PersonId = s.People.Id,
                                        PersonName = HelperClass.GetFullNameOfUser(s.People),
                                        PersonUrl = s.People.ProfilePic.ToUriString(),
                                        PersonJobtile = s.People.JobTitle,
                                        LabelColumn = GetLabelColumnGroupWeekByMonth(s.TimeIn.ConvertTimeFromUtc(operatorSearch.Timezone), _tzstartDate, _tzendDate),
                                        Value = (s.TimeOut.Value - s.TimeIn).TotalHours,
                                    });
                else
                    lstSchedules = (from q in schedules
                                    select q).Select(s => new OperatorScheduleWeekModel
                                    {
                                        PersonId = s.People.Id,
                                        PersonName = HelperClass.GetFullNameOfUser(s.People),
                                        PersonUrl = s.People.ProfilePic.ToUriString(),
                                        PersonJobtile = s.People.JobTitle,
                                        LabelColumn = GetLabelColumnGroupWeekByMonth(s.TimeIn.ConvertTimeFromUtc(operatorSearch.Timezone), _tzstartDate, _tzendDate),
                                        Value = (s.TimeOut.Value - s.TimeIn).TotalHours,
                                    });
                lstSchedules = lstSchedules.GroupBy(g => new
                {
                    g.PersonId,
                    g.PersonName,
                    g.PersonJobtile,
                    g.PersonUrl,
                    g.LabelColumn
                }).Select(
                s => new OperatorScheduleWeekModel
                {
                    PersonId = s.Key.PersonId,
                    PersonName = s.Key.PersonName,
                    PersonUrl = s.Key.PersonUrl,
                    PersonJobtile = s.Key.PersonJobtile,
                    LabelColumn = s.Key.LabelColumn,
                    Value = s.Sum(p => p.Value)
                });
                #endregion

                #region Datatable
                DataTable table = new DataTable();
                table.Columns.Add("PersonId");
                table.Columns.Add("PersonName");
                table.Columns.Add("PersonUrl");
                table.Columns.Add("PersonJobtile");

                var _fisrtweek = _tzstartDate.AddDays(DayOfWeek.Monday - _tzstartDate.DayOfWeek).ConvertTimeFromUtc(operatorSearch.Timezone);
                var _columnDynamic = new List<string>();
                while (_fisrtweek < _tzendDate)
                {
                    string columname = GetLabelColumnGroupWeekByMonth(_fisrtweek, _tzstartDate, _tzendDate);
                    table.Columns.Add(columname);
                    _columnDynamic.Add(columname);
                    _fisrtweek = _fisrtweek.AddDays(7); ;
                }
                var rows = lstSchedules.GroupBy(s => new { s.PersonId, s.PersonName, s.PersonUrl, s.PersonJobtile }).Select(x => x.First());
                foreach (var sched in rows)
                {
                    var dataRow = table.NewRow();
                    dataRow["PersonId"] = sched.PersonId;
                    dataRow["PersonName"] = sched.PersonName;
                    dataRow["PersonUrl"] = sched.PersonUrl;
                    dataRow["PersonJobtile"] = sched.PersonJobtile;
                    foreach (var clname in _columnDynamic)
                    {
                        var sc = lstSchedules.FirstOrDefault(s => s.PersonId == sched.PersonId && s.LabelColumn == clname);
                        dataRow[clname] = sc?.Value ?? 0;
                    }
                    table.Rows.Add(dataRow);
                }
                #endregion
                return table;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public DataTablesResponse SearchSchedulesDaily(OperatorSearchScheduleModel operatorSearch)
        {
            try
            {
                var _startDate = operatorSearch.Day.ConvertDateFormat(operatorSearch.Dateformat).ConvertTimeToUtc(operatorSearch.Timezone);
                var _endDate = _startDate.AddDays(1);
                int totalcount = 0;
                #region Filter
                var _querymembers = dbContext.OperatorTeamPersons.Where(s => s.Domain.Id == operatorSearch.DomainId && s.User.TeamMembers.Any(p => p.TeamPermission == TeamPermissionTypeEnum.Member));
                if (operatorSearch.Peoples != null)
                {
                    _querymembers = _querymembers.Where(s => operatorSearch.Peoples.Contains(s.User.Id));
                }
                if (operatorSearch.Roles != null)
                {
                    _querymembers = _querymembers.Where(s => s.Roles.Any(r => operatorSearch.Roles.Contains(r.Id)));
                }
                if (operatorSearch.Locations != null)
                {
                    _querymembers = _querymembers.Where(s => s.Locations.Any(r => operatorSearch.Locations.Contains(r.Id)));
                }
                var members = _querymembers.ToList().Select(s => new OperatorEmployeeModel() { Id = s.User.Id, Location = string.Join(", ", s.Locations.Select(l => l.Name)) }).ToList();
                var schedules = dbContext.OperatorSchedules.Where(t => t.Domain.Id == operatorSearch.DomainId && t.StartDate >= _startDate && t.StartDate < _endDate).ToList();
                List<Schedule> lstSchedules;
                if (operatorSearch.Peoples != null || operatorSearch.Roles != null || operatorSearch.Locations != null)
                    lstSchedules = (from q in schedules
                                    join m in members on q.Employee.Id equals m.Id
                                    select q).ToList();
                else
                    lstSchedules = schedules;

                #endregion
                List<OperatorScheduleDailyModel> lstDaily = new List<OperatorScheduleDailyModel>();
                foreach (var item in lstSchedules)
                {
                    var _slocation = members.FirstOrDefault(s => s.Id == item.Employee.Id);
                    OperatorScheduleDailyModel dailyModel = new OperatorScheduleDailyModel();
                    dailyModel.Id = item.Id;
                    dailyModel.PersonId = item.Employee.Id;
                    dailyModel.PersonName = HelperClass.GetFullNameOfUser(item.Employee);
                    dailyModel.PersonUrl = item.Employee.ProfilePic.ToUriString();
                    dailyModel.PersonJobtile = item.Employee.JobTitle;
                    dailyModel.Shift = $"{item.StartDate.ConvertTimeFromUtc(operatorSearch.Timezone).ToString("hh:mmtt")} - {item.EndDate.ConvertTimeFromUtc(operatorSearch.Timezone).ToString("hh:mmtt")} ({(item.EndDate - item.StartDate).TotalHours} hours)";
                    dailyModel.Location = _slocation.Location;
                    lstDaily.Add(dailyModel);
                }
                totalcount = lstDaily.Count();
                return new DataTablesResponse(0, lstDaily, totalcount, totalcount);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(0, string.Empty, 0, 0);
            }
        }
        public DataTable SearchSchedulesWeekly(OperatorSearchScheduleModel operatorSearch)
        {
            try
            {
                var _startDate = DateTime.UtcNow;
                var _endDate = DateTime.UtcNow;
                operatorSearch.Week.ConvertDaterangeFormat(operatorSearch.Dateformat, operatorSearch.Timezone, out _startDate, out _endDate, HelperClass.endDateAddedType.day);
                #region Filter
                var _querymembers = dbContext.OperatorTeamPersons.Where(s => s.Domain.Id == operatorSearch.DomainId && s.User.TeamMembers.Any(p => p.TeamPermission == TeamPermissionTypeEnum.Member));
                if (operatorSearch.Peoples != null)
                {
                    _querymembers = _querymembers.Where(s => operatorSearch.Peoples.Contains(s.User.Id));
                }
                if (operatorSearch.Roles != null)
                {
                    _querymembers = _querymembers.Where(s => s.Roles.Any(r => operatorSearch.Roles.Contains(r.Id)));
                }
                if (operatorSearch.Locations != null)
                {
                    _querymembers = _querymembers.Where(s => s.Locations.Any(r => operatorSearch.Locations.Contains(r.Id)));
                }
                var members = _querymembers.ToList().Select(s => new OperatorEmployeeModel() { Id = s.User.Id, Location = string.Join(", ", s.Locations.Select(l => l.Name)) }).ToList();
                var schedules = dbContext.OperatorSchedules.Where(t => t.Domain.Id == operatorSearch.DomainId && t.StartDate >= _startDate && t.StartDate < _endDate).ToList();
                IEnumerable<OperatorScheduleWeekModel> lstSchedules;
                if (operatorSearch.Peoples != null || operatorSearch.Roles != null || operatorSearch.Locations != null)
                    lstSchedules = (from q in schedules
                                    join m in members on q.Employee.Id equals m.Id
                                    select q).Select(s => new OperatorScheduleWeekModel
                                    {
                                        PersonId = s.Employee.Id,
                                        PersonName = HelperClass.GetFullNameOfUser(s.Employee),
                                        PersonUrl = s.Employee.ProfilePic.ToUriString(),
                                        PersonJobtile = s.Employee.JobTitle,
                                        LabelColumn = $"{s.StartDate.ConvertTimeFromUtc(operatorSearch.Timezone).ToString("ddd dd")}{HelperClass.Converter.OrdinalSuffix(s.StartDate.ConvertTimeFromUtc(operatorSearch.Timezone).Day)}",
                                        Value = (s.EndDate - s.StartDate).TotalHours,
                                    });
                else
                    lstSchedules = (from q in schedules
                                    select q).Select(s => new OperatorScheduleWeekModel
                                    {
                                        PersonId = s.Employee.Id,
                                        PersonName = HelperClass.GetFullNameOfUser(s.Employee),
                                        PersonUrl = s.Employee.ProfilePic.ToUriString(),
                                        PersonJobtile = s.Employee.JobTitle,
                                        LabelColumn = $"{s.StartDate.ConvertTimeFromUtc(operatorSearch.Timezone).ToString("ddd dd")}{HelperClass.Converter.OrdinalSuffix(s.StartDate.ConvertTimeFromUtc(operatorSearch.Timezone).Day)}",
                                        Value = (s.EndDate - s.StartDate).TotalHours,
                                    });
                lstSchedules = lstSchedules.GroupBy(g => new
                {
                    g.PersonId,
                    g.PersonName,
                    g.PersonJobtile,
                    g.PersonUrl,
                    g.LabelColumn
                }).Select(
                s => new OperatorScheduleWeekModel
                {
                    PersonId = s.Key.PersonId,
                    PersonName = s.Key.PersonName,
                    PersonUrl = s.Key.PersonUrl,
                    PersonJobtile = s.Key.PersonJobtile,
                    LabelColumn = s.Key.LabelColumn,
                    Value = s.Sum(p => p.Value)
                });
                #endregion

                #region Datatable
                DataTable table = new DataTable();
                table.Columns.Add("PersonId");
                table.Columns.Add("PersonName");
                table.Columns.Add("PersonUrl");
                table.Columns.Add("PersonJobtile");
                var _tzstartDate = _startDate.ConvertTimeFromUtc(operatorSearch.Timezone);
                var _tzendDate = _endDate.ConvertTimeFromUtc(operatorSearch.Timezone);
                var _columnDynamic = new List<string>();
                while (_tzstartDate < _tzendDate)
                {
                    string columname = $"{_tzstartDate.ToString("ddd dd")}{HelperClass.Converter.OrdinalSuffix(_tzstartDate.Day)}";
                    table.Columns.Add(columname);
                    _columnDynamic.Add(columname);
                    _tzstartDate = _tzstartDate.AddDays(1);
                }
                var rows = lstSchedules.GroupBy(s => new { s.PersonId, s.PersonName, s.PersonUrl, s.PersonJobtile }).Select(x => x.First());
                foreach (var sched in rows)
                {
                    var dataRow = table.NewRow();
                    dataRow["PersonId"] = sched.PersonId;
                    dataRow["PersonName"] = sched.PersonName;
                    dataRow["PersonUrl"] = sched.PersonUrl;
                    dataRow["PersonJobtile"] = sched.PersonJobtile;
                    foreach (var clname in _columnDynamic)
                    {
                        var sc = lstSchedules.FirstOrDefault(s => s.PersonId == sched.PersonId && s.LabelColumn == clname);
                        dataRow[clname] = sc?.Value ?? 0;
                    }
                    table.Rows.Add(dataRow);
                }
                #endregion
                return table;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public DataTable SearchSchedulesMonthly(OperatorSearchScheduleModel operatorSearch)
        {
            try
            {
                var _startDate = DateTime.UtcNow;
                var _endDate = DateTime.UtcNow;
                operatorSearch.Week.ConvertDaterangeFormat(operatorSearch.Dateformat, operatorSearch.Timezone, out _startDate, out _endDate);
                var _tzstartDate = _startDate.ConvertTimeFromUtc(operatorSearch.Timezone);
                var _tzendDate = _endDate.ConvertTimeFromUtc(operatorSearch.Timezone);
                #region Filter
                var _querymembers = dbContext.OperatorTeamPersons.Where(s => s.Domain.Id == operatorSearch.DomainId && s.User.TeamMembers.Any(p => p.TeamPermission == TeamPermissionTypeEnum.Member));
                if (operatorSearch.Peoples != null)
                {
                    _querymembers = _querymembers.Where(s => operatorSearch.Peoples.Contains(s.User.Id));
                }
                if (operatorSearch.Roles != null)
                {
                    _querymembers = _querymembers.Where(s => s.Roles.Any(r => operatorSearch.Roles.Contains(r.Id)));
                }
                if (operatorSearch.Locations != null)
                {
                    _querymembers = _querymembers.Where(s => s.Locations.Any(r => operatorSearch.Locations.Contains(r.Id)));
                }
                var members = _querymembers.ToList().Select(s => new OperatorEmployeeModel() { Id = s.User.Id, Location = string.Join(", ", s.Locations.Select(l => l.Name)) }).ToList();
                var schedules = dbContext.OperatorSchedules.Where(t => t.Domain.Id == operatorSearch.DomainId && t.StartDate >= _startDate && t.StartDate < _endDate).ToList();
                IEnumerable<OperatorScheduleWeekModel> lstSchedules;
                if (operatorSearch.Peoples != null || operatorSearch.Roles != null || operatorSearch.Locations != null)
                    lstSchedules = (from q in schedules
                                    join m in members on q.Employee.Id equals m.Id
                                    select q).Select(s => new OperatorScheduleWeekModel
                                    {
                                        PersonId = s.Employee.Id,
                                        PersonName = HelperClass.GetFullNameOfUser(s.Employee),
                                        PersonUrl = s.Employee.ProfilePic.ToUriString(),
                                        PersonJobtile = s.Employee.JobTitle,
                                        LabelColumn = GetLabelColumnGroupWeekByMonth(s.StartDate.ConvertTimeFromUtc(operatorSearch.Timezone), _tzstartDate, _tzendDate),
                                        Value = (s.EndDate - s.StartDate).TotalHours,
                                    });
                else
                    lstSchedules = (from q in schedules
                                    select q).Select(s => new OperatorScheduleWeekModel
                                    {
                                        PersonId = s.Employee.Id,
                                        PersonName = HelperClass.GetFullNameOfUser(s.Employee),
                                        PersonUrl = s.Employee.ProfilePic.ToUriString(),
                                        PersonJobtile = s.Employee.JobTitle,
                                        LabelColumn = GetLabelColumnGroupWeekByMonth(s.StartDate.ConvertTimeFromUtc(operatorSearch.Timezone), _tzstartDate, _tzendDate),
                                        Value = (s.EndDate - s.StartDate).TotalHours,
                                    });
                lstSchedules = lstSchedules.GroupBy(g => new
                {
                    g.PersonId,
                    g.PersonName,
                    g.PersonJobtile,
                    g.PersonUrl,
                    g.LabelColumn
                }).Select(
                s => new OperatorScheduleWeekModel
                {
                    PersonId = s.Key.PersonId,
                    PersonName = s.Key.PersonName,
                    PersonUrl = s.Key.PersonUrl,
                    PersonJobtile = s.Key.PersonJobtile,
                    LabelColumn = s.Key.LabelColumn,
                    Value = s.Sum(p => p.Value)
                });
                #endregion

                #region Datatable
                DataTable table = new DataTable();
                table.Columns.Add("PersonId");
                table.Columns.Add("PersonName");
                table.Columns.Add("PersonUrl");
                table.Columns.Add("PersonJobtile");

                var _fisrtweek = _tzstartDate.AddDays(DayOfWeek.Monday - _tzstartDate.DayOfWeek).ConvertTimeFromUtc(operatorSearch.Timezone);
                var _columnDynamic = new List<string>();
                while (_fisrtweek < _tzendDate)
                {
                    string columname = GetLabelColumnGroupWeekByMonth(_fisrtweek, _tzstartDate, _tzendDate);
                    table.Columns.Add(columname);
                    _columnDynamic.Add(columname);
                    _fisrtweek = _fisrtweek.AddDays(7); ;
                }
                var rows = lstSchedules.GroupBy(s => new { s.PersonId, s.PersonName, s.PersonUrl, s.PersonJobtile }).Select(x => x.First());
                foreach (var sched in rows)
                {
                    var dataRow = table.NewRow();
                    dataRow["PersonId"] = sched.PersonId;
                    dataRow["PersonName"] = sched.PersonName;
                    dataRow["PersonUrl"] = sched.PersonUrl;
                    dataRow["PersonJobtile"] = sched.PersonJobtile;
                    foreach (var clname in _columnDynamic)
                    {
                        var sc = lstSchedules.FirstOrDefault(s => s.PersonId == sched.PersonId && s.LabelColumn == clname);
                        dataRow[clname] = sc?.Value ?? 0;
                    }
                    table.Rows.Add(dataRow);
                }
                #endregion
                return table;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        private string GetLabelColumnGroupWeekByMonth(DateTime day, DateTime minday, DateTime maxday)
        {
            DateTime monday = day.FirstDayOfWeek();
            DateTime sunday = day.LastDayOfWeek();
            if (monday < minday)
                monday = minday;
            if (sunday > maxday)
                sunday = maxday.AddDays(-1);
            if (monday != sunday)
                return $"{monday.ToString("dd")}{HelperClass.Converter.OrdinalSuffix(monday.Day)} - {sunday.ToString("dd")}{HelperClass.Converter.OrdinalSuffix(sunday.Day)}";//01st - 07th
            else
                return $"{monday.ToString("dd")}{HelperClass.Converter.OrdinalSuffix(monday.Day)}";//01st - 07th
        }
    }
}
