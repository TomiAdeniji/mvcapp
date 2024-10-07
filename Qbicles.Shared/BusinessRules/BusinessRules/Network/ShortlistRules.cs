using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using Qbicles.BusinessRules.Azure;

namespace Qbicles.BusinessRules.BusinessRules.Network
{
    public class ShortlistRules
    {
        ApplicationDbContext dbContext;
        public ShortlistRules(ApplicationDbContext context)
        {
            this.dbContext = context;
        }

        #region Shortlist Group
        public List<ShortListGroup> getUserSlGroups(string userId, int domainId, string keySearch)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, domainId);

                var query = from slGroup in dbContext.ShortListGroups
                            where slGroup.AssociatedUser.Id == userId
&& slGroup.AssociatedDomain.Id == domainId
                            select slGroup;

                keySearch = keySearch.Trim().ToLower();
                if (!string.IsNullOrEmpty(keySearch))
                {
                    query = query.Where(p => p.Title.ToLower().Contains(keySearch) || p.Summary.ToLower().Contains(keySearch));
                }
                return query.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, domainId);
                return new List<ShortListGroup>();
            }
        }

        public ShortListGroup getSlGroupById(int slGroupId)
        {
            return dbContext.ShortListGroups.Find(slGroupId);
        }

        public ReturnJsonModel SaveShortlistGroup(ShortListGroup slGroup, S3ObjectUploadModel uploadModel, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, slGroup, uploadModel, currentUserId);

                //Process with upload model
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var awsRules = new AzureStorageRules(dbContext);
                        awsRules.ProcessingMediaS3(uploadModel.FileKey);
                    }
                    slGroup.IconUri = uploadModel.FileKey;
                }
                var currentUser = dbContext.QbicleUser.Find(currentUserId);

                if (slGroup.Id <= 0)
                {
                    slGroup.AssociatedUser = currentUser;
                    slGroup.CreatedDate = DateTime.UtcNow;

                    if (slGroup.AssociatedDomain == null)
                    {
                        return new ReturnJsonModel() { actionVal = 1, result = false, msg = "Shortlist Group Domain is required." };
                    }

                    dbContext.ShortListGroups.Add(slGroup);
                    dbContext.Entry(slGroup).State = System.Data.Entity.EntityState.Added;
                    dbContext.SaveChanges();
                    return new ReturnJsonModel() { actionVal = 1, result = true };
                }
                else
                {
                    var slGroupInDb = dbContext.ShortListGroups.Find(slGroup.Id);
                    if (slGroupInDb == null)
                    {
                        return new ReturnJsonModel() { actionVal = 2, result = false, msg = "Can not find Shortlist Group." };
                    }
                    if (!string.IsNullOrEmpty(slGroup.IconUri))
                    {
                        slGroupInDb.IconUri = slGroup.IconUri;
                    }
                    slGroupInDb.Title = slGroup.Title;
                    slGroupInDb.Summary = slGroup.Summary;

                    dbContext.Entry(slGroupInDb).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();
                    return new ReturnJsonModel() { actionVal = 2, result = true };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, slGroup, uploadModel, currentUserId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
            #endregion
        }

        public ReturnJsonModel DeleteShortListGroup(int slGroupId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, slGroupId);
                var shortlistGroup = dbContext.ShortListGroups.Find(slGroupId);
                if (shortlistGroup != null)
                {
                    dbContext.ShortListGroups.Remove(shortlistGroup);
                    dbContext.Entry(shortlistGroup).State = System.Data.Entity.EntityState.Deleted;
                    dbContext.SaveChanges();
                }
                return new ReturnJsonModel() { actionVal = 3, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, slGroupId);
                return new ReturnJsonModel() { actionVal = 3, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public ReturnJsonModel addCandidate(string userId, int slGroupId, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, slGroupId, currentUserId);

                var slGroup = dbContext.ShortListGroups.Find(slGroupId);
                var user = dbContext.QbicleUser.Find(userId);
                if (slGroup == null)
                    return new ReturnJsonModel() { result = false, actionVal = 2, msg = "Can not find Shortlist Group." };
                if (user == null)
                    return new ReturnJsonModel() { result = false, actionVal = 2, msg = "Can not find User." };

                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var associatedSlGroups = dbContext.ShortListGroups.Where(p => p.AssociatedUser.Id == currentUserId).ToList();
                foreach (var gritem in associatedSlGroups)
                {
                    gritem.Candidates.Remove(user);
                }

                slGroup.Candidates.Add(user);
                user.AssociatedShortListGroups.Add(slGroup);
                dbContext.Entry(slGroup).State = System.Data.Entity.EntityState.Modified;
                dbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                return new ReturnJsonModel() { actionVal = 2, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, slGroupId, currentUserId);
                return new ReturnJsonModel() { actionVal = 2, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public ReturnJsonModel removeCandidate(string userId, int slGroupId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, slGroupId);

                var slGroup = dbContext.ShortListGroups.Find(slGroupId);
                var user = dbContext.QbicleUser.Find(userId);
                if (slGroup == null)
                    return new ReturnJsonModel() { result = false, actionVal = 2, msg = "Can not find Shortlist Group." };
                if (user == null)
                    return new ReturnJsonModel() { result = false, actionVal = 2, msg = "Can not find User." };
                slGroup.Candidates.Remove(user);
                user.AssociatedShortListGroups.Remove(slGroup);
                dbContext.Entry(slGroup).State = System.Data.Entity.EntityState.Modified;
                dbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                return new ReturnJsonModel() { actionVal = 2, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, slGroupId);
                return new ReturnJsonModel() { actionVal = 2, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public List<ShortlistGroupCandidateCustomModel> GetSlGroupCandidatesPagination(int slGroupId, string searchKey, ref int totalRecord,
            IDataTablesRequest requestModel, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, slGroupId, searchKey, totalRecord, requestModel, start, length);

                var slGroup = dbContext.ShortListGroups.Find(slGroupId);
                if (slGroup == null)
                    return new List<ShortlistGroupCandidateCustomModel>();
                var groupOwner = slGroup.AssociatedUser;
                var query = from candidate in slGroup.Candidates select candidate;

                #region Filtering
                if (!string.IsNullOrEmpty(searchKey))
                {
                    searchKey = searchKey.Trim().ToLower();
                    query = query.Where(p => p.Forename.ToLower().Contains(searchKey) || p.Surname.ToLower().Contains(searchKey)
                                || (p.Tell != null && p.Tell.ToLower().Contains(searchKey)) || (p.JobTitle != null && p.JobTitle.ToLower().Contains(searchKey))
                                || (p.Email != null && p.Email.ToLower().Contains(searchKey)));
                }
                #endregion

                totalRecord = query.Count();

                #region Ordering
                var columns = requestModel.Columns;
                var orderString = string.Empty;
                foreach (var columnItem in columns.GetSortedColumns())
                {
                    switch (columnItem.Name)
                    {
                        case "userFullName":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Forename" + (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Surname" + (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Email":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Email" + (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Tel":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Tell" + (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Job":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "JobTitle" + (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                    }
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderString) ? "Forename asc, Surname asc" : orderString);
                #endregion

                #region Paging
                query = query.Skip(start).Take(length);
                #endregion

                var lstCandidate = query.ToList();
                var lstCandidateCustom = new List<ShortlistGroupCandidateCustomModel>();
                lstCandidate.ForEach(p =>
                {
                    var customItem = new ShortlistGroupCandidateCustomModel()
                    {
                        userId = p.Id,
                        userFullName = p.GetFullName(),
                        Email = p.Email,
                        Tel = p.Tell,
                        Job = p.JobTitle,
                        LogoUri = p.ProfilePic.ToUriString()
                    };
                    customItem.isConnectedC2C = dbContext.C2CQbicles.Any(x => x.Customers.Any(c => c.Id == groupOwner.Id) && x.Customers.Any(c => c.Id == p.Id));
                    customItem.isConnectLabel = customItem.isConnectedC2C ? @"<span class='label label-success label-lg'>Yes</span>" : @"<span class='label label-warning label-lg'>No</span>";
                    lstCandidateCustom.Add(customItem);
                });
                return lstCandidateCustom;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, slGroupId, searchKey, totalRecord, requestModel, start, length);
                return new List<ShortlistGroupCandidateCustomModel>();
            }
        }
    }
}
