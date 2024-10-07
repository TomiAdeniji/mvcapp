using Newtonsoft.Json;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Spannered;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.BusinessRules.Spannered
{
    public class SpanneredWorkgroupRules
    {
        ApplicationDbContext dbContext;
        public SpanneredWorkgroupRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public SpanneredWorkgroup getWorkgroupById(int Id)
        {
            try
            {
                return dbContext.SpanneredWorkgroups.Find(Id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public List<SpanneredProcess> GetProcesses()
        {
            try
            {
                return dbContext.SpanneredProcesses.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<SpanneredProcess>();
            }
        }
        public ReturnJsonModel SaveWorkgroup(SpanneredWorkgroupCustom workgroup, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userId, workgroup.Domain.Id);
                if (userRoleRights.All(r => r != RightPermissions.SpanneredAccess))
                {
                    refModel.msg = "ERROR_MSG_28";
                    return refModel;
                }
                workgroup.Name = workgroup.Name.Trim();
                var qbicle = dbContext.Qbicles.Find(workgroup.QbicleId);
                if (workgroup.QbicleId <= 0 || qbicle == null)
                {
                    refModel.msg = "ERROR_MSG_406";
                    return refModel;
                }
                var topic = dbContext.Topics.Find(workgroup.TopicId);
                if (workgroup.TopicId <= 0 || topic == null)
                {
                    refModel.msg = "ERROR_MSG_407";
                    return refModel;
                }
                if (dbContext.SpanneredWorkgroups.Any(s => s.Id != workgroup.Id && s.Name == workgroup.Name && s.Domain.Id == workgroup.Domain.Id))
                {
                    refModel.msg = "ERROR_MSG_257";
                    return refModel;
                }
                var location = dbContext.TraderLocations.Find(workgroup.LocationId);
                if(location==null)
                {
                    refModel.msg = "ERROR_MSG_802";
                    return refModel;
                }
                var user = dbContext.QbicleUser.Find(userId);
                var dbWorkgroup = dbContext.SpanneredWorkgroups.Find(workgroup.Id);
                if (dbWorkgroup != null)
                {
                    dbWorkgroup.LastUpdatedBy = user;
                    dbWorkgroup.LastUpdateDate = DateTime.UtcNow;
                    dbWorkgroup.SourceQbicle = qbicle;
                    dbWorkgroup.DefaultTopic = topic;
                    if (dbWorkgroup.Location == null)
                        dbWorkgroup.Location = location;
                    dbWorkgroup.Members.Clear();
                    if (workgroup.Members != null)
                        foreach (var item in workgroup.Members)
                        {
                            var userM = dbContext.QbicleUser.Find(item);
                            if (userM != null)
                            {
                                dbWorkgroup.Members.Add(userM);
                            }
                        }
                    dbWorkgroup.ReviewersApprovers.Clear();
                    if (workgroup.Approvers != null)
                        foreach (var item in workgroup.Approvers)
                        {
                            var userA = dbContext.QbicleUser.Find(item);
                            if (userA != null)
                            {
                                dbWorkgroup.ReviewersApprovers.Add(userA);
                            }
                        }
                    dbWorkgroup.Processes.Clear();
                    if (workgroup.Processes != null)
                        foreach (var item in workgroup.Processes)
                        {
                            var process = dbContext.SpanneredProcesses.Find(item);
                            if (process != null)
                            {
                                dbWorkgroup.Processes.Add(process);
                            }
                        }
                    dbWorkgroup.ProductGroups.Clear();
                    if (workgroup.Groups != null)
                        foreach (var item in workgroup.Groups)
                        {
                            var tradergroup = dbContext.TraderGroups.Find(item);
                            if (tradergroup != null)
                            {
                                dbWorkgroup.ProductGroups.Add(tradergroup);
                            }
                        }
                    dbWorkgroup.Name = workgroup.Name;
                }
                else
                {
                    dbWorkgroup = new SpanneredWorkgroup();
                    dbWorkgroup.Location = location;
                    dbWorkgroup.CreatedBy = user;
                    dbWorkgroup.CreatedDate = DateTime.UtcNow;
                    dbWorkgroup.Domain = workgroup.Domain;
                    dbWorkgroup.SourceQbicle = qbicle;
                    dbWorkgroup.DefaultTopic = topic;
                    dbWorkgroup.Name = workgroup.Name;
                    if (workgroup.Members != null)
                        foreach (var item in workgroup.Members)
                        {
                            var userM = dbContext.QbicleUser.Find(item);
                            if (userM != null)
                            {
                                dbWorkgroup.Members.Add(userM);
                            }
                        }
                    if (workgroup.Approvers != null)
                        foreach (var item in workgroup.Approvers)
                        {
                            var userA = dbContext.QbicleUser.Find(item);
                            if (userA != null)
                            {
                                dbWorkgroup.ReviewersApprovers.Add(userA);
                            }
                        }
                    if (workgroup.Processes != null)
                        foreach (var item in workgroup.Processes)
                        {
                            var process = dbContext.SpanneredProcesses.Find(item);
                            if (process != null)
                            {
                                dbWorkgroup.Processes.Add(process);
                            }
                        }
                    if (workgroup.Groups != null)
                        foreach (var item in workgroup.Groups)
                        {
                            var tradergroup = dbContext.TraderGroups.Find(item);
                            if (tradergroup != null)
                            {
                                dbWorkgroup.ProductGroups.Add(tradergroup);
                            }
                        }
                }
                var processConsumeReport= dbWorkgroup.Processes.FirstOrDefault(s => s.Name == ProcessesConst.ConsumptionReports);
                if(processConsumeReport!=null)
                {
                    var appDef =dbContext.ConsumeReportApprovalDefinitions.FirstOrDefault(w => w.SpanneredWorkgroup.Id == dbWorkgroup.Id);
                    if(appDef==null)
                    {
                        var rule = new ApprovalAppsRules(dbContext);
                        ApprovalGroup appGroup = rule.GetApprovalAppsGroupByName("Consume Report Approvals", dbWorkgroup.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, "Consume Report Approvals", user, dbWorkgroup.Domain).Object;
                        var consumeAppDef = new ConsumeReportApprovalDefinition
                        {
                            Type = ApprovalRequestDefinition.RequestTypeEnum.Trader,
                            Title = $"{dbWorkgroup.Name}/ {processConsumeReport.Name}",
                            ApprovalImage = "",
                            Description = $"Spannered WorkGroup: {dbWorkgroup.Name} {processConsumeReport.Name} process",
                            Initiators = dbWorkgroup.Members,
                            Approvers = dbWorkgroup.ReviewersApprovers,
                            Reviewers = dbWorkgroup.ReviewersApprovers,
                            IsViewOnly = true,
                            Group = appGroup,
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            SpanneredWorkgroup = dbWorkgroup,
                            SpanneredProcessType = processConsumeReport
                        };
                        dbWorkgroup.ApprovalDefs.Add(consumeAppDef);
                    }
                }
                if(dbWorkgroup.Id>0)
                {
                    if (dbContext.Entry(dbWorkgroup).State == EntityState.Detached)
                        dbContext.SpanneredWorkgroups.Attach(dbWorkgroup);
                    dbContext.Entry(dbWorkgroup).State = EntityState.Modified;
                }else
                {
                    dbContext.SpanneredWorkgroups.Add(dbWorkgroup);
                    dbContext.Entry(dbWorkgroup).State = EntityState.Added;
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.msg = "ERROR_MSG_EXCEPTION_SYSTEM";
            }
            return refModel;
        }
        /// <summary>
        /// return workgroups all for Datatable
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="domainId"></param>
        /// <param name="dateFormat"></param>
        /// <returns></returns>
        public DataTablesResponse GetWorkgroupAll(IDataTablesRequest requestModel, int domainId,int locationId, string dateFormat)
        {
            try
            {
                var query = dbContext.SpanneredWorkgroups.Where(s => s.Domain.Id == domainId&&s.Location.Id==locationId).AsQueryable();
                int totalRole = query.Count();
                var list = query.OrderBy(s => s.CreatedDate).ToList();
                var dataJson = list.Select(q => new
                {
                    Id = q.Id,
                    Name = q.Name,
                    Creator = HelperClass.GetFullNameOfUser(q.CreatedBy),
                    Created = q.CreatedDate.ToString(dateFormat),
                    Process = q.Processes != null && q.Processes.Any() ? string.Join(", ", q.Processes.Select(s => s.Name)) : "",
                    Qbicle = q.SourceQbicle?.Name,
                    QbicleId = q.SourceQbicle?.Key??"0",
                    Members = q.Members.Count(),
                    ProductGroups=q.ProductGroups.Count()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRole, totalRole);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public List<SpanneredWorkgroupsInfo> GetallWorkgroupsHasProcess(string process, string userId, int domainId,int locationId,bool isloadProductGroups=false) {
            try
            {
                var query = dbContext.SpanneredWorkgroups
                    .Where(s => s.Domain.Id == domainId&&s.Location.Id== locationId && (s.CreatedBy.Id.Equals(userId) || s.Members.Any(m => m.Id == userId) || s.ReviewersApprovers.Any(a => a.Id == userId)) && s.Processes.Any(p => p.Name.Equals(process)))
                    .ToList();
                return query.Select(s =>
                    new SpanneredWorkgroupsInfo
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Qbicle = s.SourceQbicle.Name.Replace(";", ""),
                        QbicleId = s.SourceQbicle?.Id ?? 0,
                        TopicId = s.DefaultTopic?.Id ?? 0,
                        Members = s.Members.Count(),
                        Process = s.Processes != null ? string.Join(", ", s.Processes.Select(q => q.Name)) : "",
                        JsonTraderGroups= (isloadProductGroups? JsonConvert.SerializeObject(s.ProductGroups.Select(p=>p.Name).ToList()):"")
                    }).OrderBy(n => n.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<SpanneredWorkgroupsInfo>();
            }
        }
        public ReturnJsonModel CheckPermissionAddEdit(string process, int workgroupId, int domainId, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var processes = process.Split(',');
                var permissions = dbContext.SpanneredWorkgroups
                     .Where(s => (workgroupId == 0 || s.Id == workgroupId) && s.Domain.Id == domainId && (s.CreatedBy.Id.Equals(userId) || s.Members.Any(m => m.Id == userId) || s.ReviewersApprovers.Any(a => a.Id == userId)) && s.Processes.Any(p => processes.Contains(p.Name)));
                refModel.result = permissions.Any();
                refModel.Object = permissions.SelectMany(s => s.Processes).Distinct().Select(s => s.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }
        public ReturnJsonModel DeleteWorkgroup(int id)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var wg = dbContext.SpanneredWorkgroups.Find(id);
                if (wg != null && wg.Assets.Any())
                {
                    refModel.msg = "ERROR_MSG_409";
                    return refModel;
                }
                if (wg != null)
                {
                    dbContext.SpanneredWorkgroups.Remove(wg);
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }
        public List<SpanneredWorkgroupsInfo> getWorkgroups(int domainId)
        {
            try
            {
                var query = dbContext.SpanneredWorkgroups.Where(s => s.Domain.Id == domainId).ToList();
                return query.Select(s =>
                    new SpanneredWorkgroupsInfo
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Qbicle = s.SourceQbicle.Name.Replace(";", ""),
                        Members = s.Members.Count(),
                        Process = s.Processes != null? string.Join(", ", s.Processes.Select(q => q.Name)) : ""
                    }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<SpanneredWorkgroupsInfo>();
            }
        }
        public ReturnJsonModel GetMembersQbicleByWorkgroupId(int wgId, string search)
        {
            var returnJson = new ReturnJsonModel() { result = true };
            try
            {
                var workgroup = dbContext.SpanneredWorkgroups.Find(wgId);
                var members = workgroup != null ? workgroup.SourceQbicle.Members : new List<ApplicationUser>();
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    members = members.Where(q => (q.Forename + q.Surname).ToLower().Contains(search) || q.Forename.ToLower().Contains(search) || q.Surname.ToLower().Contains(search)).ToList();
                }

                returnJson.Object = members.Select(s => new {
                    id = s.Id,
                    text = HelperClass.GetFullNameOfUser(s),
                }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                returnJson.result = false;
            }
            return returnJson;

        }
    }
}
