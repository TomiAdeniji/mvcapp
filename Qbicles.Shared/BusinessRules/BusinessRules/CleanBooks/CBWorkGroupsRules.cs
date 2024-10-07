using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;
using CleanBooksData;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Tweetinvi.Logic.Model;

namespace Qbicles.BusinessRules
{
    public class CBWorkGroupsRules
    {
        private readonly ApplicationDbContext _db;

        public CBWorkGroupsRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext DbContext => _db ?? new ApplicationDbContext();

        public List<CBWorkGroup> GetAllCbWorkGroup(int domainId)
        {
            return DbContext.CBWorkGroups.Where(d => d.Domain.Id == domainId).ToList();
        }

        public WorkgroupUser GetWorkgroupUser(int id)
        {
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
        public bool WorkGroupNameCheck(CBWorkGroup wg, int domainId)
        {
            if (wg.Id > 0)
                return DbContext.CBWorkGroups.Any(x =>
                    x.Id != wg.Id && x.Domain.Id == domainId && x.Name == wg.Name);
            return DbContext.CBWorkGroups.Any(x =>
                x.Name == wg.Name && x.Domain.Id == domainId);
        }
        public bool SaveWorkgroup(CBWorkGroup wg, string userId)
        {
            var qbicle = new QbicleRules(DbContext).GetQbicleById(wg.Qbicle.Id);
            if (wg.Id == 0)
            {
                var workGroup = new CBWorkGroup
                {
                    Name = wg.Name,
                    Domain = wg.Domain,
                    Processes = new List<CBProcess>(),
                    Qbicle = qbicle,
                    Topic = new TopicRules(DbContext).GetTopicById(wg.Topic.Id),
                    CreatedBy = DbContext.QbicleUser.Find(userId),
                    CreatedDate = DateTime.UtcNow
                };
                foreach (var process in wg.Processes)
                {
                    var proc = new CBProcessRules(DbContext).GetById(process.Id);
                    workGroup.Processes.Add(proc);
                }

                if (wg.Members != null)
                    foreach (var user in wg.Members)
                    {
                        var member = new UserRules(DbContext).GetUser(user.Id, 0);
                        workGroup.Members.Add(member);
                    }
                // create ApprovalRequestDefinition

                DbContext.CBWorkGroups.Add(workGroup);
                DbContext.Entry(workGroup).State = EntityState.Added;
            }
            else
            {
                if (wg.Members != null)
                    foreach (var user in wg.Members)
                    {
                        var member = new UserRules(DbContext).GetUser(user.Id, 0);
                        if (qbicle.Members.All(u => u.Id != user.Id))
                            qbicle.Members.Add(member);
                    }
                for (int i = 0; i < wg.Processes.Count; i++)
                {
                    wg.Processes[i] = DbContext.CBProcesses.Find(wg.Processes[i].Id);
                }

                var workGroupDb = GetById(wg.Id);
                workGroupDb.Members.Clear();
                workGroupDb.Processes.Clear();
                DbContext.SaveChanges();
                if (wg.Members != null)
                    foreach (var user in wg.Members)
                    {
                        var member = new UserRules(DbContext).GetUser(user.Id, 0);
                        workGroupDb.Members.Add(member);
                    }
                //update workgroup processes
                workGroupDb.Processes = wg.Processes;
                //var lazyLoad = wkg.ItemCategories;
                workGroupDb.Name = wg.Name;
                workGroupDb.Qbicle = qbicle;
                workGroupDb.Topic = new TopicRules(DbContext).GetTopicById(wg.Topic.Id);
                if (DbContext.Entry(workGroupDb).State == EntityState.Detached)
                    DbContext.CBWorkGroups.Attach(workGroupDb);
                DbContext.Entry(workGroupDb).State = EntityState.Modified;

            }

            DbContext.SaveChanges();
            return true;
        }
        public bool Delete(int id)
        {
            var wg = DbContext.CBWorkGroups.FirstOrDefault(e => e.Id == id);
            if (wg == null) return false;

            DbContext.CBWorkGroups.Remove(wg);
            DbContext.SaveChanges();
            return true;
        }
        public CBWorkGroup GetById(int id)
        {
            return DbContext.CBWorkGroups.Find(id);
        }
        public string ReInitUsersEdit(int id, QbicleDomain domain)
        {
            var tr = new StringBuilder();
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

            return tr.ToString();
        }
    }
}