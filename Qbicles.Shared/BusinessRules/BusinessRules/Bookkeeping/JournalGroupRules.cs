using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class JournalGroupRules
    {
        ApplicationDbContext dbContext;

        public JournalGroupRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        private bool CheckExistName(JournalGroup group)
        {
            var groups = dbContext.JournalGroups.Where(q => q.Name == group.Name && q.Domain.Id == group.Domain.Id);
            if (group.Id > 0 && groups.Any())
                groups = groups.Where(q => q.Id != group.Id);
            if (groups.Any()) return true;
            else return false;
        }
        public IQueryable<JournalGroup> GetByDomainId(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetByDomainId", null, null, domainId);

                return  dbContext.JournalGroups.Where(q => q.Domain.Id == domainId).OrderBy(n => n.Name);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }

        }
        public JournalGroup GetById(int journalGroupId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetById", null, null, journalGroupId);


                var journalGroup = dbContext.JournalGroups.Find(journalGroupId);
                return journalGroup;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, journalGroupId);
                return null;
            }

        }
        public JournalGroupItem GetJournalGroupOnlyById(int journalGroupId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetJournalGroupOnlyById", null, null, journalGroupId);

                var journalGroups = dbContext.JournalGroups.Find(journalGroupId);
                return new JournalGroupItem
                {
                    Id = journalGroups.Id,
                    Name = journalGroups.Name,
                    CreatedBy = HelperClass.GetFullNameOfUser(journalGroups.CreatedBy),
                    CreatedDate = journalGroups.CreatedDate
                };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, journalGroupId);
                return null;
            }
            
        }
        public ReturnJsonModel SaveJournalGroup(JournalGroup journalGroup, string userId, QbicleDomain domain)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveJournalGroup", userId, null, journalGroup);


                if (journalGroup != null && !string.IsNullOrEmpty(journalGroup.Name))
                {

                    if (journalGroup.CreatedBy == null)
                        journalGroup.CreatedBy = dbContext.QbicleUser.Find(userId);
                    journalGroup.Domain = domain;
                    if (CheckExistName(journalGroup))
                    {
                        refModel.actionVal = 3;
                        refModel.msg = ResourcesManager._L("ERROR_MSG_632",  journalGroup.Name);
                        refModel.msgId = journalGroup.Id.ToString();
                        refModel.msgName = journalGroup.Name;
                        refModel.result = true;
                        return refModel;
                    }
                    if (journalGroup.Id > 0)
                    {
                        
                        var jlGroupUpdate = dbContext.JournalGroups.Where(q => q.Id == journalGroup.Id).FirstOrDefault();
                        if (jlGroupUpdate.CreatedBy == null) jlGroupUpdate.CreatedBy = journalGroup.CreatedBy;
                        if (jlGroupUpdate.CreatedDate == DateTime.MinValue) jlGroupUpdate.CreatedDate = DateTime.UtcNow;
                        jlGroupUpdate.Name = journalGroup.Name;
                        if (dbContext.Entry(jlGroupUpdate).State == EntityState.Detached)
                            dbContext.JournalGroups.Attach(jlGroupUpdate);
                        dbContext.Entry(jlGroupUpdate).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        refModel.actionVal = 2;
                        //refModel.msg = _journalGroup.Name;
                        refModel.msgId = jlGroupUpdate.Id.ToString();
                        refModel.msgName = jlGroupUpdate.Name;
                    }
                    else
                    {
                        journalGroup.CreatedDate = DateTime.UtcNow;
                        dbContext.JournalGroups.Add(journalGroup);
                        dbContext.Entry(journalGroup).State = EntityState.Added;
                        dbContext.SaveChanges();
                        refModel.actionVal = 1;
                        //append to select group
                        refModel.msgId = journalGroup.Id.ToString();
                        refModel.msgName = journalGroup.Name;
                    }
                    refModel.result = true;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, journalGroup);
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
            }
            
            return refModel;
        }
        public bool DeleteJournalGroup(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeleteJournalGroup", null, null, id);
                var journalGroup = dbContext.JournalGroups.Find(id);
                dbContext.JournalGroups.Remove(journalGroup);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }
    }
}
