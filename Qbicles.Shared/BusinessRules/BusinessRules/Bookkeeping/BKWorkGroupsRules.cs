using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;

namespace Qbicles.BusinessRules
{
    public class BKWorkGroupsRules
    {
        ApplicationDbContext dbContext;

        public BKWorkGroupsRules(ApplicationDbContext context)
        {
            dbContext = context;
        }



        public List<BookkeepingProcess> GetKBWorkGroupProcesss()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "List<BookkeepingProcess> GetKBWorkGroupProcesss", null, null);

                return dbContext.BookkeepingProcesses.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<BookkeepingProcess>();
            }
        }

        public List<BKWorkGroup> GetKBWorkGroups(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "List<BKWorkGroup> GetKBWorkGroups", null, null, domainId);

                return dbContext.BKWorkGroups.Where(d => d.Domain.Id == domainId).ToList();


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }
        }

        public BKWorkGroup GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "BKWorkGroup GetById", null, null, id);

                return dbContext.BKWorkGroups.Find(id);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public bool WorkGroupNameCheck(BKWorkGroup wg, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "WorkGroupNameCheck", null, null, wg, domainId);

                if (wg.Id > 0)
                    return dbContext.BKWorkGroups.Any(x =>
                        x.Id != wg.Id && x.Domain.Id == domainId && x.Name == wg.Name);
                return dbContext.BKWorkGroups.Any(x =>
                    x.Name == wg.Name && x.Domain.Id == domainId);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, wg, domainId);
                return true;
            }

        }

        public ReturnJsonModel CreateBKWorkGroup(BKWorkGroup wg, string userId, string appImage)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateBKWorkGroup", userId, null, wg);
                var currentUser = dbContext.QbicleUser.Find(userId);

                var qbicle = new QbicleRules(dbContext).GetQbicleById(wg.Qbicle.Id);
                var bkWorkGroup = new BKWorkGroup
                {
                    Name = wg.Name,
                    Processes = new List<BookkeepingProcess>(),
                    Qbicle = qbicle,
                    Topic = new TopicRules(dbContext).GetTopicById(wg.Topic.Id),
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    Domain = wg.Domain,
                    ApprovalDefs = new List<ApprovalRequestDefinition>()
                };
                foreach (var process in wg.Processes)
                {
                    var proc = new BookkeepingProcessRules(dbContext).GetById(process.Id);
                    bkWorkGroup.Processes.Add(proc);
                }

                if (wg.Members != null)
                    foreach (var user in wg.Members)
                    {
                        var member = new UserRules(dbContext).GetUser(user.Id, 0);
                        bkWorkGroup.Members.Add(member);
                    }

                if (wg.Reviewers != null)
                    foreach (var user in wg.Reviewers)
                    {
                        var member = new UserRules(dbContext).GetUser(user.Id, 0);
                        bkWorkGroup.Reviewers.Add(member);
                    }

                if (wg.Approvers != null)
                    foreach (var user in wg.Approvers)
                    {
                        var member = new UserRules(dbContext).GetUser(user.Id, 0);
                        bkWorkGroup.Approvers.Add(member);
                    }
                // create ApprovalRequestDefinition

                foreach (var process in bkWorkGroup.Processes)
                {
                    string groupName = process.Name;
                    var rule = new ApprovalAppsRules(dbContext);
                    //ApprovalGroup appGroup;

                    var appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                               (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                    var accountAppDef = new ApprovalRequestDefinition()
                    {
                        Type = ApprovalRequestDefinition.RequestTypeEnum.Bookkeeping,
                        Title = $"{bkWorkGroup.Name}/ {process.Name}",
                        ApprovalImage = appImage,
                        Description = $"Bookkeeping Process: {process.Name}",
                        Initiators = bkWorkGroup.Members,
                        Approvers = bkWorkGroup.Approvers,
                        Reviewers = bkWorkGroup.Reviewers,
                        IsViewOnly = true,
                        Group = appGroup,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow
                    };
                    bkWorkGroup.ApprovalDefs.Add(accountAppDef);

                }
                dbContext.BKWorkGroups.Add(bkWorkGroup);
                dbContext.Entry(bkWorkGroup).State = EntityState.Added;
                dbContext.SaveChanges();
                refModel.actionVal = 1;
                return refModel;


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, wg);
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
                return refModel;
            }

        }

        public ReturnJsonModel UpdateBKWorkGroup(BKWorkGroup wg, string userId, string appImage)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateBKWorkGroup", userId, null, wg);

                var qbicle = new QbicleRules(dbContext).GetQbicleById(wg.Qbicle.Id);
                foreach (var user in wg.Members)
                {
                    var member = new UserRules(dbContext).GetUser(user.Id, 0);
                    if (qbicle.Members.All(u => u.Id != user.Id))
                        qbicle.Members.Add(member);
                }

                var workGroupDb = GetById(wg.Id);
                //workGroupDb.ItemCategories.Clear();
                workGroupDb.Members.Clear();
                workGroupDb.Approvers.Clear();
                workGroupDb.Reviewers.Clear();
                if (wg.Members != null)
                    foreach (var user in wg.Members)
                    {
                        var member = new UserRules(dbContext).GetUser(user.Id, 0);
                        workGroupDb.Members.Add(member);
                    }

                if (wg.Reviewers != null)
                    foreach (var user in wg.Reviewers)
                    {
                        var member = new UserRules(dbContext).GetUser(user.Id, 0);
                        workGroupDb.Reviewers.Add(member);
                    }

                if (wg.Approvers != null)
                    foreach (var user in wg.Approvers)
                    {
                        var member = new UserRules(dbContext).GetUser(user.Id, 0);
                        workGroupDb.Approvers.Add(member);
                    }
                //var lazyLoad = wkg.ItemCategories;
                workGroupDb.Name = wg.Name;

                foreach (var process in wg.Processes)
                {
                    if (workGroupDb.Processes.Any(p => p.Id == process.Id)) continue;
                    var proc = new BookkeepingProcessRules(dbContext).GetById(process.Id);
                    workGroupDb.Processes.Add(proc);

                    string groupName = proc.Name;
                    var rule = new ApprovalAppsRules(dbContext);
                    //ApprovalGroup appGroup;
                    var currentUser = dbContext.QbicleUser.Find(userId);
                    //groupName = "Bookkeeping Accounts";
                    var appGroup = rule.GetApprovalAppsGroupByName(groupName, wg.Domain) ??
                                   (ApprovalGroup)rule.SaveApprovalAppGroup(0, groupName, currentUser, wg.Domain).Object;
                    var accountAppDef = new ApprovalRequestDefinition()
                    {
                        Type = ApprovalRequestDefinition.RequestTypeEnum.Bookkeeping,
                        Title = $"{workGroupDb.Name}/ {proc.Name}",
                        ApprovalImage = appImage,
                        Description = $"Bookkeeping Process: {proc.Name}",
                        Initiators = workGroupDb.Members,
                        Approvers = workGroupDb.Approvers,
                        Reviewers = workGroupDb.Reviewers,
                        IsViewOnly = true,
                        Group = appGroup,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow
                    };
                    workGroupDb.ApprovalDefs.Add(accountAppDef);
                }

                workGroupDb.Qbicle = qbicle;
                workGroupDb.Topic = new TopicRules(dbContext).GetTopicById(wg.Topic.Id);
                //workGroupDb.ItemCategories =new TraderGroupRules(DbContext).GetByIds(wg.ItemCategories.Select(e => e.Id));
                //workGroupDb.Location = new TraderLocationRules(DbContext).GetById(wg.Location.Id);
                //update workgroup processes
                var removeProcesses = new List<BookkeepingProcess>();
                foreach (var p in workGroupDb.Processes)
                {
                    if (wg.Processes.Any(e => e.Id == p.Id))
                        continue;
                    var pRemove = workGroupDb.Processes.FirstOrDefault(e => e.Id == p.Id);
                    removeProcesses.Add(pRemove);
                }




                removeProcesses.ForEach(process =>
                {
                    var approvalDefs = workGroupDb.ApprovalDefs.Where(e =>
                        e.Group.Name == process.Name && e.Title == $"{workGroupDb.Name}/ {process.Name}").ToList();
                    dbContext.ApprovalAppsRequestDefinition.RemoveRange(approvalDefs);

                    workGroupDb.Processes.Remove(process);
                });



                foreach (var approvalDef in workGroupDb.ApprovalDefs)
                {
                    if (approvalDef.Id == 0) continue;
                    approvalDef.Initiators.Clear();
                    approvalDef.Initiators = workGroupDb.Members;
                    approvalDef.Reviewers.Clear();
                    approvalDef.Reviewers = workGroupDb.Reviewers;
                    approvalDef.Approvers.Clear();
                    approvalDef.Approvers = workGroupDb.Approvers;
                }
                if (dbContext.Entry(workGroupDb).State == EntityState.Detached)
                    dbContext.BKWorkGroups.Attach(workGroupDb);
                dbContext.Entry(workGroupDb).State = EntityState.Modified;
                dbContext.SaveChanges();
                return refModel;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, wg);
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
                return refModel;
            }
            
        }

        public ReturnJsonModel DeleteBKWorkGroup(int id)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeleteBKWorkGroup", null, null, id);


                var wg = dbContext.BKWorkGroups.FirstOrDefault(e => e.Id == id);
                
                if (wg.ApprovalDefs != null && wg.ApprovalDefs.Count > 0)
                {
                    foreach (var approvalDef in wg.ApprovalDefs.ToList())
                    {
                        var approvalReqs =
                            dbContext.ApprovalReqs.Any(e => e.ApprovalRequestDefinition.Id == approvalDef.Id);
                        if (!approvalReqs)
                            dbContext.ApprovalAppsRequestDefinition.Remove(approvalDef);
                    }
                }
                dbContext.BKWorkGroups.Remove(wg);
                dbContext.SaveChanges();
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_MSG_BK_WG_DEL");
                return refModel;
            }
            
        }

        public WorkgroupUser GetWorkgroupUser(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetWorkgroupUser", null, null, id);

                var wgU = new WorkgroupUser();
                var wg = GetById(id);
                wgU.Members = wg.Members.Select(u => new WorkgroupMember
                {
                    Id = u.Id,
                    Name = HelperClass.GetFullNameOfUser(u),
                    Pic = u.ProfilePic.ToUriString()
                }).ToList();
                wgU.Approvers = wg.Approvers.Select(u => new WorkgroupMemberId { Id = u.Id }).ToList();
                wgU.Reviewers = wg.Reviewers.Select(u => new WorkgroupMemberId { Id = u.Id }).ToList();
                return wgU;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new WorkgroupUser();
            }
            
        }

        public string ReInitUsersEdit(int id, QbicleDomain domain)
        {
            var tr = new StringBuilder();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "ReInitUsersEdit", null, null, id, domain.Users);


                var wgMembers = GetById(id)?.Members;
                foreach (var u in domain.Users.OrderBy(u => u.UserName))
                {
                    var fullName = HelperClass.GetFullNameOfUser(u);
                    var isMember = "";
                    if (wgMembers != null && wgMembers.Any(user => user.Id == u.Id))
                        isMember = "checked";
                    tr.Append("<tr id=\"tr_edit_user_" + u.Id + "\">");
                    tr.Append("<td>");
                    tr.Append(
                        $"<div class=\"table-avatar mini\" style=\"background-image: url('{u.ProfilePic.ToUri()}');\">&nbsp;</div>");
                    tr.Append("</td>");
                    tr.Append($"<td>{fullName}</td>");
                    tr.Append("<td>");
                    var roleAll = "";
                    foreach (var r in u.DomainRoles.Where(d => d.Domain.Id == domain.Id))
                    {
                        roleAll += r.Name + ",";
                        tr.Append($"<span class=\"label label-lg label-info\">{r.Name}</span> ");
                    }

                    tr.Append($"<span class=\"hidden\">{roleAll}</span>");
                    tr.Append("</td>");
                    tr.Append("<td>");
                    tr.Append("<div class=\"checkbox toggle\">");
                    tr.Append(
                        $"<input {isMember} data-fullname=\"{fullName}\" onchange=\"AddUsersToMembers(this.checked,'{u.Id}', $(this).data('fullname'),'{u.ProfilePic.ToUri()}')\" class=\"check-right\" data-toggle=\"toggle\" data-onstyle=\"success\" data-on=\"<i class='fa fa-check'></i>\" data-off=\" \" type=\"checkbox\">");

                    tr.Append("</div>");
                    tr.Append("</td>");
                    tr.Append("</tr>");
                }


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, domain.Users);                
            }
            
            return tr.ToString();
        }

        public List<BKWorkGroup> GetKBWorkGroupsOfUser(int domainId, string userId, string bkProcess)
        {
            try
            {
                
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetKBWorkGroupsOfUser", userId, null, domainId,  userId,  bkProcess);

                if (bkProcess != "")
                    return dbContext.BKWorkGroups.Where(q => q.Domain.Id == domainId
                        && q.Processes.Any(p => p.Name.Equals(bkProcess))
                        && q.Members.Select(u => u.Id).Contains(userId)).ToList();

                return dbContext.BKWorkGroups.Where(q => q.Domain.Id == domainId && q.Members.Select(u => u.Id).Contains(userId)).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, domainId, userId, bkProcess);
                return null;
            }
            
        }
    }
}