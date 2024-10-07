using Newtonsoft.Json;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Operator;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorWorkgroupRules
    {
        ApplicationDbContext dbContext;
        public OperatorWorkgroupRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public OperatorWorkGroup getWorkgroupById(int Id)
        {
            try
            {
                return dbContext.OperatorWorkGroups.Find(Id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public List<OperatorWorkGroup> GetAllWorkgroupsHasTeamPerson(int domainId)
        {
            try
            {
                return dbContext.OperatorWorkGroups.Where(s =>
                s.Domain.Id == domainId && s.TeamMembers.SelectMany(t => t.Member.TeamPersons).Any() && !s.IsHide).OrderBy(n => n.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public List<OperatorWorkGroup> GetOperatorWorksAll(int domainId, WorkGroupTypeEnum type)
        {
            try
            {
                return dbContext.OperatorWorkGroups.Where(s => s.Domain.Id == domainId && !s.IsHide && s.Type == type).OrderBy(n => n.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<OperatorWorkGroup>();
            }
        }

        public DataTablesResponse SearchWorkgroups(IDataTablesRequest requestModel, int domainId, int type, string name, string currentUserId, string dateFormat)
        {
            try
            {
                var query = dbContext.OperatorWorkGroups.Where(t => t.Domain.Id == domainId && !t.IsHide).AsQueryable();
                if (type != -1)
                {
                    query = query.Where(t => t.Type == (WorkGroupTypeEnum)type);
                }
                if (!String.IsNullOrEmpty(name))
                {
                    query = query.Where(t => t.Name.Trim().ToLower().Contains(name.Trim().ToLower()));
                }
                int totalWorkgroup = query.Count();
                var newQuery = query.ToList().Select(q => new OperatorWorkgroupModel
                {
                    Id = q.Id,
                    Name = q.Name,
                    Type = q.Type.ToString(),
                    Qbicle = q.SourceQbicle.Name,
                    Members = q.TaskMembers.Count + q.TeamMembers.Count(),
                    Creator = HelperClass.GetFullNameOfUser(q.CreatedBy, currentUserId),
                    CreatorId = q.CreatedBy.Id,
                    Created = q.CreatedDate.ToString(dateFormat),
                });

                var sortedColumn = requestModel.Columns.GetSortedColumns().FirstOrDefault();
                if (sortedColumn.Data.Equals("Name"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Name);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Name);
                    }
                }
                else if (sortedColumn.Data.Equals("Creator"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Creator);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Creator);
                    }
                }
                else if (sortedColumn.Data.Equals("Created"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Created);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Created);
                    }
                }
                else if (sortedColumn.Data.Equals("Type"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Type);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Type);
                    }
                }
                else if (sortedColumn.Data.Equals("Qbicle"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Qbicle);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Qbicle);
                    }
                }
                else if (sortedColumn.Data.Equals("Members"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Members);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Members);
                    }
                }
                var list = newQuery.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                return new DataTablesResponse(requestModel.Draw, list, totalWorkgroup, totalWorkgroup);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        public ReturnJsonModel RemoveWorkgroup(int id)
        {
            ReturnJsonModel refModel = new ReturnJsonModel { result = false };
            try
            {
                var workgroup = dbContext.OperatorWorkGroups.Find(id);
                workgroup.IsHide = true;
                if (dbContext.Entry(workgroup).State == EntityState.Detached)
                    dbContext.OperatorWorkGroups.Attach(workgroup);
                dbContext.Entry(workgroup).State = EntityState.Modified;
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public ReturnJsonModel SaveWorkgroup(OperatorWorkgroupCustom workgroup, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userId, workgroup.Domain.Id);
                if (userRoleRights.All(r => r != RightPermissions.OperatorAccess))
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
                var location = dbContext.OperatorLocations.Find(workgroup.LocationId);
                if (workgroup.LocationId <= 0 || location == null)
                {
                    refModel.msg = "ERROR_MSG_802";
                    return refModel;
                }
                if (dbContext.OperatorWorkGroups.Any(s => s.Id != workgroup.Id && s.Name == workgroup.Name && s.Domain.Id == workgroup.Domain.Id))
                {
                    refModel.msg = "ERROR_MSG_257";
                    return refModel;
                }
                var currentUser = dbContext.QbicleUser.Find(userId);
                var dbWorkgroup = dbContext.OperatorWorkGroups.Find(workgroup.Id);
                if (dbWorkgroup != null)
                {
                    dbWorkgroup.Name = workgroup.Name;
                    dbWorkgroup.SourceQbicle = qbicle;
                    dbWorkgroup.DefaultTopic = topic;
                    dbWorkgroup.Type = workgroup.Type;
                    dbWorkgroup.LastUpdatedBy = currentUser;
                    dbWorkgroup.LastUpdateDate = DateTime.UtcNow;
                    dbWorkgroup.Location = location;

                    dbContext.OperatorWGTeamMembers.RemoveRange(dbWorkgroup.TeamMembers);

                    List<WorkGroupTeamMember> lstWGTeamMembers = new List<WorkGroupTeamMember>();
                    if (workgroup.TeamMembers != null)
                    {
                        var teamMembersList = JsonConvert.DeserializeObject<List<TeamMember>>(workgroup.TeamMembers);
                        if (teamMembersList.Any(m => String.IsNullOrEmpty(m.Role)))
                        {
                            refModel.msg = "ERROR_MSG_803";
                            return refModel;
                        }
                        else
                        {
                            foreach (var item in teamMembersList)
                            {
                                WorkGroupTeamMember member = new WorkGroupTeamMember();
                                var user = dbContext.QbicleUser.Find(item.Id);
                                if (user != null)
                                {
                                    member.Member = user;
                                    member.TeamPermission = (TeamPermissionTypeEnum)(Int32.Parse(item.Role));
                                    lstWGTeamMembers.Add(member);
                                }
                            }
                            dbWorkgroup.TeamMembers.AddRange(lstWGTeamMembers);
                        }

                    }


                    dbContext.OperatorWGTaskMembers.RemoveRange(dbWorkgroup.TaskMembers);

                    List<WorkGroupTaskMember> lstWGTaskMembers = new List<WorkGroupTaskMember>();

                    if (workgroup.TaskMembers != null)
                    {
                        var taskMembersList = JsonConvert.DeserializeObject<List<TaskMember>>(workgroup.TaskMembers);
                        foreach (var item in taskMembersList)
                        {
                            WorkGroupTaskMember member = new WorkGroupTaskMember();
                            var user = dbContext.QbicleUser.Find(item.Id);
                            if (user != null)
                            {
                                member.Member = user;
                                member.IsTaskCreator = item.IsTaskCreator;
                                lstWGTaskMembers.Add(member);
                            }
                        }
                        dbWorkgroup.TaskMembers.AddRange(lstWGTaskMembers);
                    }

                    if (dbContext.Entry(dbWorkgroup).State == EntityState.Detached)
                        dbContext.OperatorWorkGroups.Attach(dbWorkgroup);
                    dbContext.Entry(dbWorkgroup).State = EntityState.Modified;
                }
                else
                {
                    dbWorkgroup = new OperatorWorkGroup
                    {
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        Domain = workgroup.Domain,
                        SourceQbicle = qbicle,
                        DefaultTopic = topic,
                        Type = workgroup.Type,
                        Name = workgroup.Name,
                        Location = location,
                        LastUpdateDate = DateTime.UtcNow,
                        LastUpdatedBy = currentUser
                    };

                    List<WorkGroupTeamMember> lstTeamMembers = new List<WorkGroupTeamMember>();
                    if (workgroup.TeamMembers != null)
                    {
                        var teamMembersList = JsonConvert.DeserializeObject<List<TeamMember>>(workgroup.TeamMembers);
                        if (teamMembersList.Any(m => String.IsNullOrEmpty(m.Role)))
                        {
                            refModel.msg = "ERROR_MSG_803";
                            return refModel;
                        }
                        else
                        {
                            foreach (var item in teamMembersList)
                            {
                                WorkGroupTeamMember member = new WorkGroupTeamMember();
                                var user = dbContext.QbicleUser.Find(item.Id);
                                if (user != null)
                                {
                                    member.Member = user;
                                    member.TeamPermission = (TeamPermissionTypeEnum)(Int32.Parse(item.Role));
                                    lstTeamMembers.Add(member);
                                }
                            }
                            dbWorkgroup.TeamMembers.AddRange(lstTeamMembers);
                        }
                    }

                    List<WorkGroupTaskMember> lstTaskMembers = new List<WorkGroupTaskMember>();
                    if (workgroup.TaskMembers != null)
                    {
                        var taskMembersList = JsonConvert.DeserializeObject<List<TaskMember>>(workgroup.TaskMembers);
                        foreach (var item in taskMembersList)
                        {
                            WorkGroupTaskMember member = new WorkGroupTaskMember();
                            var user = dbContext.QbicleUser.Find(item.Id);
                            if (user != null)
                            {
                                member.Member = user;
                                member.IsTaskCreator = item.IsTaskCreator;
                                lstTaskMembers.Add(member);
                            }
                        }
                        dbWorkgroup.TaskMembers.AddRange(lstTaskMembers);
                    }

                    dbContext.OperatorWorkGroups.Add(dbWorkgroup);
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
        public List<OperatorTeamMembersModel> getTeamMembersByDomain(int domainId)
        {
            try
            {
                var _members = dbContext.OperatorTeamPersons.Where(s => s.Domain.Id == domainId && s.User.TeamMembers.Any(p => p.TeamPermission == TeamPermissionTypeEnum.Member)).ToList();
                return _members.Select(s => new OperatorTeamMembersModel
                {
                    UserId = s.User.Id,
                    TeamPersonId = s.Id,
                    Fullname = HelperClass.GetFullNameOfUser(s.User)
                }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<OperatorTeamMembersModel>();
            }
        }
        public bool checkIsManagerOrSupervisor(int domainId, string userId)
        {
            try
            {
                return dbContext.OperatorWGTeamMembers.Any(s => s.WorkGroup.Domain.Id == domainId && s.Member.Id == userId && (s.TeamPermission == TeamPermissionTypeEnum.Supervisor || s.TeamPermission == TeamPermissionTypeEnum.Manager));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return false;
            }
        }
        public OperatorWorkgroupPreviewModel getWorkgroupPreviewById(int Id)
        {
            try
            {
                var wg = dbContext.OperatorWorkGroups.Find(Id);
                OperatorWorkgroupPreviewModel model = new OperatorWorkgroupPreviewModel();
                model.localtion = wg.Location.Name;
                model.qbicle = wg.SourceQbicle.Name;
                model.countmember = wg.TeamMembers.Count;
                model.members = wg.TeamMembers.Where(s => s.TeamPermission == TeamPermissionTypeEnum.Member).ToList().Select(s => new OperatorTeamMembersModel { UserId = s.Member.Id, Fullname = HelperClass.GetFullNameOfUser(s.Member) }).ToList();
                return model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public OperatorWorkgroupPreviewModel getWorkgroupPreviewTaskById(int Id)
        {
            try
            {
                var wg = dbContext.OperatorWorkGroups.Find(Id);
                OperatorWorkgroupPreviewModel model = new OperatorWorkgroupPreviewModel();
                model.localtion = wg.Location.Name;
                model.qbicle = wg.SourceQbicle.Name;
                model.countmember = wg.TaskMembers.Count;
                model.members = wg.TaskMembers.ToList().Select(s => new OperatorTeamMembersModel { UserId = s.Member.Id, Fullname = HelperClass.GetFullNameOfUser(s.Member), AvatarUrl = s.Member.ProfilePic.ToUriString() }).ToList();
                return model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
    }
}
