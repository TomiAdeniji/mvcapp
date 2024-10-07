using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class MyTagsRules
    {
        private ApplicationDbContext _db;

        public MyTagsRules()
        {
        }

        public MyTagsRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }
        public List<Select2Model> GetMoveTags(int myDeskId, string search, int currentTagId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get move tags", null, null, myDeskId, search, currentTagId);

                IQueryable<MyTag> query = DbContext.MyFolders;
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(s => s.Name.Contains(search) && s.Desk.Id == myDeskId && s.Id != currentTagId);
                else
                    query = query.Where(s => s.Desk.Id == myDeskId && s.Id != currentTagId);
                var lstTags = query.Select(s => new Select2Model { Id = s.Id, Text = s.Name }).ToList();
                return lstTags;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, myDeskId, search, currentTagId);
                return null;
            }
        }
        public int CountAssociatedActivities(int currentTagId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count associated activities", null, null, currentTagId);
                
                IQueryable<MyTag> query = DbContext.MyFolders;
                var mytag = query.FirstOrDefault(s => s.Id == currentTagId);
                if (mytag != null)
                {
                    return mytag.Posts.Count() + mytag.Activities.Count();
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentTagId);
                return 0;
            }
        }
        public bool DuplicateTagNameCheck(int myDeskId, int tagId, string tagName)
        {
            bool exists;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate tag name", null, null, myDeskId, tagId, tagName);

                if (tagId == 0)
                    exists = DbContext.MyFolders.Any(x => x.Desk.Id == myDeskId && x.Name == tagName);
                else
                    exists = DbContext.MyFolders.Any(x => x.Desk.Id == myDeskId && x.Name == tagName && x.Id != tagId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, myDeskId, tagId, tagName);
                exists = false;
            }

            return exists;
        }
        public dynamic SaveTag(int deskId, MyTag tag, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate tag name", null, null, deskId, tag);
                
                DbContext.Configuration.AutoDetectChangesEnabled = false;
                DbContext.Configuration.ValidateOnSaveEnabled = false;
                DbContext.Configuration.ProxyCreationEnabled = false;
                var Tp = DbContext.MyFolders.Find(tag.Id);
                MyTag t = null;
                if (Tp != null)
                {
                    Tp.Name = tag.Name;
                    Tp.CreatedDate = Tp.CreatedDate == null ? tag.CreatedDate : Tp.CreatedDate;
                    Tp.Creator = Tp.Creator == null ? DbContext.QbicleUser.Find(userId) : Tp.Creator;
                    if (DbContext.Entry(Tp).State == EntityState.Detached)
                        DbContext.MyFolders.Attach(Tp);
                    DbContext.Entry(Tp).State = EntityState.Modified;
                    t = tag;
                }
                else
                {
                    var cube = DbContext.MyDesks.Find(deskId);
                    t = new MyTag
                    {
                        Name = tag.Name,
                        CreatedDate = DateTime.UtcNow,
                        Creator = DbContext.QbicleUser.Find(userId),
                        Desk = cube
                    };
                    DbContext.MyFolders.Add(t);
                    DbContext.Entry(t).State = EntityState.Added;
                }
                ;
                if (DbContext.SaveChanges() > 0)
                {
                    dynamic tpic = new
                    {
                        t.Id,
                        t.Name,
                        CreatedDate = t.CreatedDate.HasValue ? t.CreatedDate.Value.ToString("dd-MM-yyyy") : "",
                        Creator = t.Creator != null ? HelperClass.GetFullNameOfUser(t.Creator) : "",
                        Instances = t.Activities.Count() + t.Posts.Count()
                    };
                    return tpic;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, deskId, tag);
                return null;
            }
            finally
            {
                DbContext.Configuration.AutoDetectChangesEnabled = true;
                DbContext.Configuration.ValidateOnSaveEnabled = true;
                DbContext.Configuration.ProxyCreationEnabled = true;
            }
        }
        public bool DeleteTag(int tagId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete tag", null, null, tagId);

                var tag = DbContext.MyFolders.Find(tagId);
                if (tag != null)
                {
                    DbContext.MyFolders.Remove(tag);
                    return DbContext.SaveChanges() > 0 ? true : false;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tagId);
                return false;
            }
        }
        public List<string> GetTagNameForTypeahead(int deskId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get tag name for type ahead", null, null, deskId);

                return DbContext.MyFolders.Where(q => q.Desk.Id == deskId).Select(n => n.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, deskId);
                return new List<string>();
            }
        }
    }
}
