using Newtonsoft.Json;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Qbicles.BusinessRules
{
    public class TaskFormDefinitionRefRules
    {
        ApplicationDbContext _db;
        public TaskFormDefinitionRefRules()
        {
        }
        public TaskFormDefinitionRefRules(ApplicationDbContext context)
        {
            _db = context;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }

        public bool UpdateFormBuilder(TaskFormDefinitionRefCustom form, string userId, int taskId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update form builder", userId, null, form, userId, taskId);

                var taskFormDefinitionRef = GetTaskFormDefinitionRefByTaskId(taskId, form.Id);
                if (taskFormDefinitionRef.Task.ClosedBy == null)
                {
                    taskFormDefinitionRef.FormBuilder = form.FormBuilder;
                    taskFormDefinitionRef.FormData = form.FormData;
                    taskFormDefinitionRef.LastUpdateDate = DateTime.UtcNow;
                    taskFormDefinitionRef.LastUpdatedBy = DbContext.QbicleUser.Find(userId);

                    var defi = taskFormDefinitionRef.FormDefinition;
                    if (DbContext.Entry(taskFormDefinitionRef).State == EntityState.Detached)
                        DbContext.TaskFormDefinitionRef.Attach(taskFormDefinitionRef);
                    DbContext.Entry(taskFormDefinitionRef).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, form, userId, taskId);
                return false;
            }
        }

        //public List<TaskFormDefinitionRef> GetAllTaskFormDefinitionRefByDomain(int domainId)
        //{
        //    try
        //    {
        //        return DbContext.TaskFormDefinitionRef.Where(x => x.FormDefinition.Domain.Id == domainId).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(MethodBase.GetCurrentMethod(),ex);
        //        return new List<TaskFormDefinitionRef>();
        //    }
        //}

        private TaskFormDefinitionRef GetTaskFormDefinitionRefByTaskId(int taskId, int formDefinitionId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetTaskFormDefinitionRefByTaskId", null, null, taskId, formDefinitionId);

                return DbContext.TaskFormDefinitionRef.FirstOrDefault(x => x.Task.Id == taskId && x.FormDefinition.Id == formDefinitionId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taskId, formDefinitionId);
                return new TaskFormDefinitionRef();

            }
        }
        public List<TaskFormDefinitionRef> GetTaskFormDefinitionRefsByTaskId(int taskId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetTaskFormDefinitionRefsByTaskId", null, null, taskId);

                return DbContext.TaskFormDefinitionRef.Where(x => x.Task.Id == taskId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taskId);
                return new List<TaskFormDefinitionRef>();
            }
        }
        public ReturnJsonModel ExecuteQueryToTbodyTable(TaskFormParameter taskFormParameter, TaskParameter taskParameter,
            bool filterTask, bool filterTaskForm, string timeZone
            )
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if(ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetTaskFormDefinitionRefsByTaskId", null, null, taskFormParameter, taskParameter, filterTask, filterTaskForm, timeZone);

                StringBuilder sqlQuery = new StringBuilder();
                StringBuilder sqlQuerySelect = new StringBuilder();
                #region !filterTask && filterTaskForm
                if (!filterTask && filterTaskForm)
                {
                    //init query base

                    sqlQuery.Append("SELECT distinct ats.Id, ats.Name,t.Description, tfd.FormData,ats.StartedBy_Id FROM qb_taskformdefinitionref as tf" +
                        " join qb_qbicletask as t on tf.Task_Id = t.Id" +
                        " join qb_taskformdefinitionref as tfd on t.Id = tfd.Task_Id" +
                        " join qb_taskformdefinitionxref tx on t.Id = tx.Task_Id" +
                        " join qb_qbicleactivities as ats on tf.Task_Id = ats.Id" +
                        " join qb_qbicles as q on ats.Qbicle_Id = q.Id");
                    sqlQuery.AppendFormat(" WHERE tx.FormDefinition_Id = {0} and q.Domain_Id = {1}", taskFormParameter.formDefinitionId,
                        taskFormParameter.domainId);
                    // init parameters

                    var listOfParameters = JsonConvert.DeserializeObject<IEnumerable<ExecuteQueryModel>>(taskFormParameter.formQuery);
                    var dateParameters = listOfParameters.Where(d => d.Name.StartsWith("date"));
                    var checkgroupParameters = listOfParameters.Where(d => d.Name.StartsWith("checkbox"));
                    var anotherParameters = listOfParameters.Except(dateParameters).Except(checkgroupParameters).Where(v => v.Value != "");
                    var listDateFilter = JsonConvert.DeserializeObject<IEnumerable<ExecuteQueryModel>>(taskFormParameter.arrDateFields);

                    //render parameters

                    foreach (var date in listDateFilter)
                    {
                        var dateRange = date.Value.Split('-');

                        sqlQuery.AppendFormat(" AND ((JSON_EXTRACT(tfd.FormData,'$.{0}') >= '{1}'", date.Name.Replace("-", "").Replace("filter", ""),
                                    dateRange[0].Trim().ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", ""));
                        sqlQuery.AppendFormat(" AND JSON_EXTRACT(tfd.FormData,'$.{0}') <= '{1}'))", date.Name.Replace("-", "").Replace("filter", ""),
                            dateRange[1].Trim().ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", ""));
                    }
                    foreach (var check in checkgroupParameters)
                    {
                        sqlQuery.AppendFormat(" AND JSON_EXTRACT(tfd.FormData,'$.{0}') = '{1}'", check.Name, check.Value);
                    }

                    foreach (var other in anotherParameters)
                    {
                        sqlQuery.AppendFormat(" AND upper(JSON_EXTRACT(tfd.FormData,'$.{0}')) = upper('\"{1}\"')",
                            other.Name.Replace("-", ""), other.Value);
                    }

                    sqlQuery.Append(";");

                    var resultList = DbContext.Database.SqlQuery<ExecuteQueryResultModel>(sqlQuery.ToString());
                    refModel.Object = resultList;
                    sqlQuery = new StringBuilder();
                    var rowsTask_found = new List<object>();
                    foreach (var task in resultList.Distinct())
                    {
                        sqlQuery.Clear();
                        sqlQuery.AppendFormat("<div class=\"btn-group options\"><button type = \"button\" class=\"btn btn-success dropdown - toggle\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\"><i class=\"fa fa-cog\"></i> &nbsp; Actions </button>");
                        sqlQuery.AppendFormat("<ul class=\"dropdown-menu dropdown-menu-right\" style=\"right:0;\"><li><a href = \"javascript:ShowTaskPage({0},false)\" class=\"query_to_result\">View</a></li><li><a href = \"javascript:GenerateReport({0})\" class=\"query_to_result\">Generate report</a></li></ul></div>", task.Id);
                        rowsTask_found.Add(new { Id = task.Id, Name = task.Name.Decrypt(), Description = task.Description.Decrypt(), Options = sqlQuery.ToString() });
                    }
                    refModel.Object = rowsTask_found;

                }
                #endregion
                #region filterTask && !filterTaskForm
                else if (filterTask && !filterTaskForm)
                {
                    //init query base
                    if (!string.IsNullOrWhiteSpace(taskParameter.asssignTo))
                    {

                        sqlQuerySelect.Append("SELECT distinct ats.Id, ats.Name,t.Description, tfd.FormData,ats.StartedBy_Id FROM qb_ActivitiesUsersXref ax " +
                           " join qb_qbicleactivities as ats on ax.Activity_Id = ats.Id" +
                           " join qb_qbicletask as t on t.Id = ats.Id" +
                           " join qb_taskformdefinitionref as tfd on t.Id = tfd.Task_Id" +
                           " join qb_qbicles as q on ats.Qbicle_Id = q.Id WHERE 1=1");

                        var created = taskParameter.asssignTo.Split(',').ToList();
                        var createdFormat = created.Select(x => "'" + x + "'").ToList();
                        sqlQuery.AppendFormat(" AND ax.User_Id IN ({0})", string.Join(",", createdFormat));
                    }
                    else
                        sqlQuerySelect.Append("SELECT distinct ats.Id, ats.Name,t.Description,  tfd.FormData,ats.StartedBy_Id FROM qb_qbicletask as t" +
                            " join qb_taskformdefinitionref as tfd on t.Id = tfd.Task_Id" +
                            " join qb_qbicleactivities as ats on t.Id = ats.Id" +
                            " join qb_qbicles as q on ats.Qbicle_Id = q.Id WHERE 1=1");
                    if (taskParameter.qbicleId != 0)
                        sqlQuery.AppendFormat(" AND q.Id={0}", taskParameter.qbicleId);
                    if (taskParameter.priority != 0)
                        sqlQuery.AppendFormat(" AND t.Priority={0}", (int)taskParameter.priority);
                    if (taskParameter.recurring != 0)
                        sqlQuery.AppendFormat(" AND t.Repeat={0}", (int)taskParameter.recurring);

                    if (!string.IsNullOrWhiteSpace(taskParameter.description))
                        sqlQuery.AppendFormat(" AND t.Description='{0}'", taskParameter.description.Encrypt());
                    if (!string.IsNullOrWhiteSpace(taskParameter.taskName))
                        sqlQuery.AppendFormat(" AND ats.Name='{0}'", taskParameter.taskName.Encrypt());

                    if (!string.IsNullOrWhiteSpace(taskParameter.createdDateStart))
                        sqlQuery.AppendFormat(" AND (ats.StartedDate >= '{0}' and ats.StartedDate <= '{1}')",
                            taskParameter.createdDateStart.ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", timeZone),
                            taskParameter.createdDateEnd.ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", timeZone));

                    if (!string.IsNullOrWhiteSpace(taskParameter.deadlineDateStart))
                        sqlQuery.AppendFormat(" AND (t.DueDate >='{0}' and t.DueDate <='{1}')",
                             taskParameter.deadlineDateStart.ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", timeZone),
                             taskParameter.deadlineDateEnd.ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", timeZone));

                    if (!string.IsNullOrWhiteSpace(taskParameter.createdBy))
                    {
                        var created = taskParameter.createdBy.Split(',').ToList();
                        var createdFormat = created.Select(x => "'" + x + "'").ToList();
                        sqlQuery.AppendFormat(" AND ats.StartedBy_Id IN ({0})", string.Join(",", createdFormat));
                    }
                    sqlQuery.Append(";");
                    var resultList2 = DbContext.Database.SqlQuery<ExecuteQueryResultModel>(sqlQuerySelect.ToString() + sqlQuery.ToString());
                    sqlQuery = new StringBuilder();
                    var rowsTask_found = new List<object>();
                    foreach (var task in resultList2.Distinct())
                    {
                        sqlQuery.Clear();
                        sqlQuery.AppendFormat("<div class=\"btn-group options\"><button type = \"button\" class=\"btn btn-success dropdown - toggle\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\"><i class=\"fa fa-cog\"></i> &nbsp; Actions </button>");
                        sqlQuery.AppendFormat("<ul class=\"dropdown-menu dropdown-menu-right\" style=\"right:0;\"><li><a href = \"javascript:ShowTaskPage({0},false)\" class=\"query_to_result\">View</a></li><li><a href = \"javascript:GenerateReport({0})\" class=\"query_to_result\">Generate report</a></li></ul></div>", task.Id);
                        rowsTask_found.Add(new { Id = task.Id, Name = task.Name.Decrypt(), Description = task.Description.Decrypt(), Options = sqlQuery.ToString() });
                    }

                    refModel.Object = rowsTask_found;
                }
                #endregion
                #region filterTask && filterTaskForm
                else if (filterTask && filterTaskForm)
                {
                    StringBuilder sqlQueryTask1 = new StringBuilder();
                    StringBuilder sqlQueryTask2 = new StringBuilder();
                    StringBuilder sqlQueryTaskForm = new StringBuilder();
                    //build query task
                    if (!string.IsNullOrWhiteSpace(taskParameter.asssignTo))
                    {

                        sqlQueryTask1.Append("(SELECT distinct ats.Id, ats.Name,t.Description, tfd.FormData,ats.StartedBy_Id FROM qb_ActivitiesUsersXref ax " +
                           " join qb_qbicleactivities as ats on ax.Activity_Id = ats.Id" +
                           " join qb_qbicletask as t on t.Id = ats.Id" +
                           " join qb_taskformdefinitionref as tfd on t.Id = tfd.Task_Id" +
                           " join qb_qbicles as q on ats.Qbicle_Id = q.Id WHERE 1=1");

                        var created = taskParameter.asssignTo.Split(',').ToList();
                        var createdFormat = created.Select(x => "'" + x + "'").ToList();
                        sqlQueryTask2.AppendFormat(" AND ax.User_Id IN ({0})", string.Join(",", createdFormat));
                    }
                    else
                        sqlQueryTask1.Append("(SELECT distinct ats.Id, ats.Name,t.Description,ats.StartedBy_Id FROM qb_qbicletask as t" +
                            " join qb_qbicleactivities as ats on t.Id = ats.Id" +
                            " join qb_qbicles as q on ats.Qbicle_Id = q.Id WHERE 1=1");
                    if (taskParameter.qbicleId != 0)
                        sqlQueryTask2.AppendFormat(" AND q.Id={0}", taskParameter.qbicleId);
                    if (taskParameter.priority != 0)
                        sqlQueryTask2.AppendFormat(" AND t.Priority={0}", (int)taskParameter.priority);
                    if (taskParameter.recurring != 0)
                        sqlQueryTask2.AppendFormat(" AND t.Repeat={0}", (int)taskParameter.recurring);
                    if (!string.IsNullOrWhiteSpace(taskParameter.description))
                        sqlQueryTask2.AppendFormat(" AND t.Description='{0}'", taskParameter.description.Encrypt());
                    if (!string.IsNullOrWhiteSpace(taskParameter.taskName))
                        sqlQueryTask2.AppendFormat(" AND ats.Name='{0}'", taskParameter.taskName.Encrypt());

                    if (!string.IsNullOrWhiteSpace(taskParameter.createdDateStart))
                        sqlQueryTask2.AppendFormat(" AND (ats.StartedDate >= '{0}' and ats.StartedDate <= '{1}')",
                            taskParameter.createdDateStart.ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", timeZone),
                            taskParameter.createdDateEnd.ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", timeZone));

                    if (!string.IsNullOrWhiteSpace(taskParameter.deadlineDateStart))
                        sqlQueryTask2.AppendFormat(" AND (t.DueDate >='{0}' and t.DueDate <='{1}')",
                             taskParameter.deadlineDateStart.ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", timeZone),
                             taskParameter.deadlineDateEnd.ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", timeZone));

                    if (!string.IsNullOrWhiteSpace(taskParameter.createdBy))
                    {
                        var created = taskParameter.createdBy.Split(',').ToList();
                        var createdFormat = created.Select(x => "'" + x + "'").ToList();
                        sqlQueryTask2.AppendFormat(" AND ats.StartedBy_Id IN ({0})", string.Join(",", createdFormat));
                    }
                    sqlQueryTask2.Append(")");

                    //build query taskform
                    sqlQueryTaskForm.Append("SELECT distinct ats.Id, ats.Name,t.Description,tf.FormBuilder, tf.FormData,ats.StartedBy_Id FROM qb_taskformdefinitionref as tf");
                    sqlQueryTaskForm.AppendFormat(" join {0} as t on tf.Task_Id = t.Id", sqlQueryTask1.ToString() + sqlQueryTask2.ToString());
                    sqlQueryTaskForm.Append(" join qb_taskformdefinitionxref tx on t.Id = tx.Task_Id" +
                    " join qb_qbicleactivities as ats on tf.Task_Id = ats.Id" +
                    " join qb_qbicles as q on ats.Qbicle_Id = q.Id");
                    sqlQueryTaskForm.AppendFormat(" WHERE tx.FormDefinition_Id = {0} and q.Domain_Id = {1}",
                        taskFormParameter.formDefinitionId, taskFormParameter.domainId);
                    // init parameters
                    var listOfParameters = JsonConvert.DeserializeObject<IEnumerable<ExecuteQueryModel>>(taskFormParameter.formQuery);
                    var dateParameters = listOfParameters.Where(d => d.Name.StartsWith("date"));
                    var checkgroupParameters = listOfParameters.Where(d => d.Name.StartsWith("checkbox"));
                    var anotherParameters = listOfParameters.Except(dateParameters).Except(checkgroupParameters).Where(v => v.Value != "");
                    //render parameters
                    var listDateFilter = JsonConvert.DeserializeObject<IEnumerable<ExecuteQueryModel>>(taskFormParameter.arrDateFields);
                    foreach (var date in listDateFilter)
                    {
                        var dateRange = date.Value.Split('-');

                        sqlQuery.AppendFormat(" AND ((JSON_EXTRACT(tf.FormData,'$.{0}') >= '{1}'", date.Name.Replace("-", "").Replace("filter", ""),
                                    dateRange[0].Trim().ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", ""));
                        sqlQuery.AppendFormat(" AND JSON_EXTRACT(tf.FormData,'$.{0}') <= '{1}'))", date.Name.Replace("-", "").Replace("filter", ""),
                            dateRange[1].Trim().ConvertDateFormat("dd/mm/yyyy", "yyyy-mm-dd", ""));
                    }
                    foreach (var check in checkgroupParameters)
                    {
                        sqlQueryTaskForm.AppendFormat(" AND JSON_EXTRACT(tf.FormData,'$.{0}') = '{1}'", check.Name, check.Value);
                    }

                    foreach (var other in anotherParameters)
                    {
                        sqlQueryTaskForm.AppendFormat(" AND upper(JSON_EXTRACT(tf.FormData,'$.{0}')) = upper('\"{1}\"')",
                            other.Name.Replace("-", ""), other.Value);
                    }

                    sqlQueryTaskForm.Append(";");


                    //sumary query

                    var resultList = DbContext.Database.SqlQuery<ExecuteQueryResultModel>(sqlQueryTaskForm.ToString());
                    refModel.Object = resultList;
                    sqlQuery = new StringBuilder();
                    var rowsTask_found = new List<object>();
                    foreach (var task in resultList.Distinct())
                    {
                        sqlQuery.Clear();
                        sqlQuery.AppendFormat("<div class=\"btn-group options\"><button type = \"button\" class=\"btn btn-success dropdown - toggle\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\"><i class=\"fa fa-cog\"></i> &nbsp; Actions </button>");
                        sqlQuery.AppendFormat("<ul class=\"dropdown-menu dropdown-menu-right\" style=\"right:0;\"><li><a href = \"javascript:ShowTaskPage({0},false)\" class=\"query_to_result\">View</a></li><li><a href = \"javascript:GenerateReport({0})\" class=\"query_to_result\">Generate report</a></li></ul></div>", task.Id);
                        rowsTask_found.Add(new { Id = task.Id, Name = task.Name.Decrypt(), Description = task.Description.Decrypt(), Options = sqlQuery.ToString() });
                    }
                    refModel.Object = rowsTask_found;
                }
                #endregion
                refModel.result = true;

                refModel.msg = sqlQuery.ToString();
               
            }
            catch(Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, taskFormParameter, taskParameter, filterTask, filterTaskForm, timeZone);
            }
            return refModel;

        }

        public bool CheckFormDefinitionIsExistsInTaskFormDefinitionRef(int formId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CheckFormDefinitionIsExistsInTaskFormDefinitionRef", null, null, formId);

                return DbContext.TaskFormDefinitionRef.Any(x => x.FormDefinition.Id == formId);
            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, formId);
                return false;
            }
        }
    }
}
