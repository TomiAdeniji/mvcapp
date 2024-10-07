using Newtonsoft.Json;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Spannered;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.ODS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Qbicles.BusinessRules.Model.TB_Column;
using static Qbicles.Models.Notification;
using static Qbicles.Models.QbiclePeople;
using static Qbicles.Models.Spannered.Asset;

namespace Qbicles.BusinessRules.BusinessRules.Spannered
{
    public class SpanneredAssetRules
    {
        ApplicationDbContext dbContext;
        public SpanneredAssetRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public Asset GetAssetById(int id)
        {
            try
            {
                return dbContext.SpanneredAssets.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public AssetInventory GetAssetInventoryById(int id)
        {
            try
            {
                return dbContext.SpanneredAssetInventories.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public List<Asset> GetRelatedAssets(int assetId)
        {
            try
            {
                var asset = dbContext.SpanneredAssets.FirstOrDefault(a => a.Id == assetId);
                var tempAssets = dbContext.SpanneredAssets.Where(a => a.OtherAssets.Any(s => s.Id == assetId)).ToList();
                return tempAssets.Union(asset.OtherAssets).Distinct().ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<Asset>();
            }
        }
        public ReturnJsonModel UpdateOptionAsset(int id, OptionsEnum Option, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var asset = dbContext.SpanneredAssets.Find(id);
                if (asset != null)
                {
                    asset.Options = Option;
                    asset.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                    asset.LastUpdateDate = DateTime.UtcNow;
                    if (dbContext.Entry(asset).State == EntityState.Detached)
                        dbContext.SpanneredAssets.Attach(asset);
                    dbContext.Entry(asset).State = EntityState.Modified;
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
        public List<Asset> GetOthersAssetById(int id, int domainId, int lid)
        {
            try
            {
                return dbContext.SpanneredAssets.Where(s => s.Id != id && s.Domain.Id == domainId && s.Location.Id == lid).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<Asset>();
            }
        }
        public List<Asset> LoadAssetsByDomainId(int currentDomainId, int locationId, OptionsEnum Options, int skip, int take, string keyword, List<int> tags, ref int totalRecord)
        {
            try
            {
                var query = dbContext.SpanneredAssets.Where(s => s.Domain.Id == currentDomainId && s.Location.Id == locationId && (s.Options == OptionsEnum.Show || s.Options == Options));

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Title.Contains(keyword) || s.Description.Contains(keyword));
                }
                if (tags != null)
                {
                    query = query.Where(s => s.Tags.Any(t => tags.Any(g => g == t.Id)));
                }
                totalRecord = query.Count();
                return query.OrderByDescending(s => s.CreatedDate).Skip(skip).Take(take).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<Asset>();
            }
        }
        public ReturnJsonModel SpanneredFreeSaveAsset(SpanneredAssetCustom asset, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userId, asset.Domain.Id);
                if (userRoleRights.All(r => r != RightPermissions.SpanneredAccess))
                {
                    refModel.msg = "ERROR_MSG_28";
                    return refModel;
                }
                asset.Title = asset.Title.Trim();
                if (dbContext.SpanneredAssets.Any(s => s.Id != asset.Id && s.Title == asset.Title && s.Domain.Id == asset.Domain.Id))
                {
                    refModel.msg = "ERROR_MSG_412";
                    return refModel;
                }
                var wg = dbContext.SpanneredWorkgroups.Find(asset.WorkgroupId);
                if (asset.Id == 0 && wg == null)
                {
                    refModel.msg = "ERROR_MSG_168";
                    return refModel;
                }
                var location = dbContext.TraderLocations.Find(asset.LocationId);
                if (asset.LocationId == 0 && location == null)
                {
                    refModel.msg = "ERROR_MSG_802";
                    return refModel;
                }

                if (!string.IsNullOrEmpty(asset.FeaturedImageUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(asset.FeaturedImageUri);
                }
                var user = dbContext.QbicleUser.Find(userId);
                var dbAsset = dbContext.SpanneredAssets.Find(asset.Id);
                if (dbAsset != null)
                {
                    dbAsset.Title = asset.Title;
                    dbAsset.Identification = asset.Identification;
                    dbAsset.Description = asset.Description;
                    dbAsset.LastUpdatedBy = user;
                    dbAsset.LastUpdateDate = DateTime.UtcNow;
                    dbAsset.AssociatedTraderItem = dbContext.TraderItems.Find(asset.LinkTraderItemId);
                    if (!string.IsNullOrEmpty(asset.FeaturedImageUri))
                    {
                        AddSpanneredAssetMediaQbicle(asset.MediaResponse, user, wg.SourceQbicle, dbAsset.ResourceFolder, asset.Title, asset.Description, wg.DefaultTopic);
                        dbAsset.FeaturedImageUri = asset.FeaturedImageUri;
                    }
                    dbAsset.Tags.Clear();
                    if (asset.Tags != null)
                        foreach (var item in asset.Tags)
                        {
                            var tag = dbContext.SpanneredTags.Find(item);
                            if (tag != null)
                            {
                                dbAsset.Tags.Add(tag);
                            }
                        }
                    dbAsset.OtherAssets.Clear();
                    if (asset.OtherAssets != null)
                        foreach (var item in asset.OtherAssets)
                        {
                            var assetother = dbContext.SpanneredAssets.Find(item);
                            if (assetother != null)
                            {
                                dbAsset.OtherAssets.Add(assetother);
                            }
                        }
                    #region Add Asset Inventory List
                    var lstAssetInventoryRemove = new List<AssetInventory>();
                    foreach (var item in dbAsset.AssetInventories)
                    {
                        var assetInventory = asset.AssetInventories != null ? asset.AssetInventories.FirstOrDefault(s => s.Id == item.Id) : null;
                        if (assetInventory == null)
                        {
                            lstAssetInventoryRemove.Add(item);
                        }
                        else
                        {
                            item.Item = dbContext.TraderItems.Find(assetInventory.ItemId);
                            item.Unit = dbContext.ProductUnits.Find(assetInventory.UnitId);
                            item.Purpose = assetInventory.Purpose;
                            if (dbContext.Entry(dbAsset).State == EntityState.Detached)
                                dbContext.SpanneredAssetInventories.Attach(item);
                            dbContext.Entry(item).State = EntityState.Modified;

                        }
                    }
                    dbContext.SpanneredAssetInventories.RemoveRange(lstAssetInventoryRemove);
                    //Create meters and add to Asset
                    if (asset.AssetInventories != null)
                        foreach (var item in asset.AssetInventories.Where(s => s.Id == 0))
                        {
                            AssetInventory assetInventory = new AssetInventory();
                            assetInventory.Item = dbContext.TraderItems.Find(item.ItemId);
                            assetInventory.ParentAsset = dbAsset;
                            assetInventory.Purpose = item.Purpose;
                            assetInventory.Unit = dbContext.ProductUnits.Find(item.UnitId);
                            dbAsset.AssetInventories.Add(assetInventory);
                        }
                    #endregion

                    #region Add Meters List
                    //Check Meters if it does not exist then delete it, if it exists then update
                    var lstMetersRemove = new List<Meter>();
                    foreach (var item in dbAsset.Meters)
                    {
                        var meter = asset.Meters != null ? asset.Meters.FirstOrDefault(s => s.Id == item.Id) : null;
                        if (meter == null)
                        {
                            lstMetersRemove.Add(item);
                        }
                        else
                        {
                            item.Name = meter.Name;
                            item.Unit = meter.Unit;
                            item.Description = meter.Description;
                            if (dbContext.Entry(dbAsset).State == EntityState.Detached)
                                dbContext.SpanneredMeters.Attach(item);
                            dbContext.Entry(item).State = EntityState.Modified;

                        }
                    }
                    dbContext.SpanneredMeters.RemoveRange(lstMetersRemove);
                    //Create meters and add to Asset
                    if (asset.Meters != null)
                        foreach (var item in asset.Meters.Where(s => s.Id == 0))
                        {
                            item.CreatedBy = user;
                            item.CreatedDate = dbAsset.CreatedDate;
                            dbAsset.Meters.Add(item);
                        }
                    #endregion
                    if (dbContext.Entry(dbAsset).State == EntityState.Detached)
                        dbContext.SpanneredAssets.Attach(dbAsset);
                    dbContext.Entry(dbAsset).State = EntityState.Modified;
                }
                else
                {
                    dbAsset = new Asset
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Domain = asset.Domain,
                        Workgroup = wg,
                        Title = asset.Title,
                        Description = asset.Description,
                        Identification = asset.Identification,
                        Options = OptionsEnum.Show,
                        AssociatedTraderItem = dbContext.TraderItems.Find(asset.LinkTraderItemId),
                        Location = location
                    };
                    #region Add Resource folder
                    var folderName = HelperClass.GeneralName;//Set default folder of QBICLE
                    var folder = dbContext.MediaFolders.FirstOrDefault(s => s.Name == folderName && s.Qbicle.Id == wg.SourceQbicle.Id);
                    if (folder == null)
                    {
                        folder = new MediaFolder();
                        folder.Name = folderName;
                        folder.Qbicle = wg.SourceQbicle;
                        folder.CreatedDate = DateTime.UtcNow;
                        folder.CreatedBy = user;
                        dbContext.MediaFolders.Add(folder);
                        dbContext.Entry(folder).State = EntityState.Added;
                    }
                    dbAsset.ResourceFolder = folder;
                    #endregion
                    if (!string.IsNullOrEmpty(asset.FeaturedImageUri))
                    {
                        AddSpanneredAssetMediaQbicle(asset.MediaResponse, user, wg.SourceQbicle, folder, asset.Title, asset.Description, wg.DefaultTopic);
                        dbAsset.FeaturedImageUri = asset.FeaturedImageUri;
                    }
                    if (asset.Tags != null)
                        foreach (var item in asset.Tags)
                        {
                            var tag = dbContext.SpanneredTags.Find(item);
                            if (tag != null)
                            {
                                dbAsset.Tags.Add(tag);
                            }
                        }
                    dbAsset.OtherAssets.Clear();
                    if (asset.OtherAssets != null)
                        foreach (var item in asset.OtherAssets)
                        {
                            var assetother = dbContext.SpanneredAssets.Find(item);
                            if (assetother != null)
                            {
                                dbAsset.OtherAssets.Add(assetother);
                            }
                        }
                    #region Add Asset Inventory List
                    //Create meters and add to Asset
                    if (asset.AssetInventories != null)
                        foreach (var item in asset.AssetInventories)
                        {
                            AssetInventory assetInventory = new AssetInventory();
                            assetInventory.Item = dbContext.TraderItems.Find(item.ItemId);
                            assetInventory.ParentAsset = dbAsset;
                            assetInventory.Purpose = item.Purpose;
                            assetInventory.Unit = dbContext.ProductUnits.Find(item.UnitId);
                            dbAsset.AssetInventories.Add(assetInventory);
                        }
                    #endregion
                    #region Add Meters List to Asset
                    if (asset.Meters != null)
                        foreach (var item in asset.Meters)
                        {
                            item.CreatedBy = user;
                            item.CreatedDate = dbAsset.CreatedDate;
                            dbAsset.Meters.Add(item);
                        }
                    #endregion
                    dbContext.SpanneredAssets.Add(dbAsset);
                    dbContext.Entry(dbAsset).State = EntityState.Added;
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
        /// Automatically generated folder name spannered
        /// App (SP) - Asset (ASSET) - Random number (001)
        /// </summary>
        /// <param name="qbicleId"></param>
        /// <returns></returns>
        public string AutoGenerateFolderName(Models.Qbicle qbicle)
        {
            try
            {
                if (qbicle != null)
                {
                    var random = new Random();
                    var randomNumber = random.Next(1, 999);
                    var sFolderName = "SP-ASSET-" + randomNumber.ToString("000");
                    for (int i = 0; i < 20; i++)
                    {
                        var isExist = dbContext.MediaFolders.Any(m => m.Qbicle.Id == qbicle.Id && m.Name == sFolderName);
                        if (!isExist)
                        {
                            return sFolderName;
                        }
                        else
                        {
                            sFolderName = "SP-ASSET-" + random.Next(1, 999).ToString("000");
                            continue;
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return "";
            }
        }
        private QbicleMedia AddSpanneredAssetMediaQbicle(MediaModel media, ApplicationUser user, Qbicle qbicle, MediaFolder folder, string name, string descript, Topic topic)
        {
            try
            {
                if (!string.IsNullOrEmpty(media.Name))
                {
                    //DbContext.Entry(media.Type).State = System.Data.Entity.EntityState.Modified;
                    //Media attach
                    var m = new QbicleMedia
                    {
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        Name = name,
                        FileType = media.Type,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic,
                        MediaFolder = folder,
                        Description = descript,
                        IsVisibleInQbicleDashboard = false
                    };
                    var versionFile = new VersionedFile
                    {
                        IsDeleted = false,
                        FileSize = media.Size,
                        UploadedBy = user,
                        UploadedDate = DateTime.UtcNow,
                        Uri = media.UrlGuid,
                        FileType = media.Type
                    };
                    m.VersionedFiles.Add(versionFile);

                    dbContext.Medias.Add(m);
                    dbContext.Entry(m).State = EntityState.Added;
                    return m;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public ReturnJsonModel SpanneredFreeSaveResource(MediaModel media, string userId, int qbicleId, int mediaFolderId, string name, string description, int topicId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);

                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }

                var topic = new TopicRules(dbContext).GetTopicById(topicId);
                if (topic == null)
                {
                    refModel.msg = "Error not finding the current Topic!";
                    return refModel;
                }
                var folder = new MediaFolderRules(dbContext).GetMediaFolderById(mediaFolderId, qbicleId);
                if (folder == null)
                {
                    refModel.msg = "Error not finding the current Folder!";
                    return refModel;
                }
                var qbicle = dbContext.Qbicles.Find(qbicleId);
                if (qbicle == null)
                {
                    refModel.msg = "Error not finding the current Qbicle!";
                    return refModel;
                }
                var dbMedia = AddSpanneredAssetMediaQbicle(media, dbContext.QbicleUser.Find(userId), qbicle, folder, name, description, topic);
                if (dbMedia == null) return refModel;
                qbicle.Media.Add(dbMedia);
                if (dbContext.SaveChanges() > 0)
                    refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public bool SaveTask(QbicleTask task, string assignee, MediaModel media, string[] watchers, int qbicleId,
           string userId, int topicId, int[] activitiesRelate, List<QbicleStep> stepLst, long assetId, int workgroupId, string inventoriescps, string originatingConnectionId = "")
        {
            try
            {

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);

                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }
                var currentUser = dbContext.QbicleUser.Find(userId);

                var qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
                var topic = new TopicRules(dbContext).GetTopicById(topicId);
                task.Topic = topic;
                //var uRules = new UserRules(dbContext);
                task.StartedBy = currentUser;
                task.StartedDate = DateTime.UtcNow;
                task.State = QbicleActivity.ActivityStateEnum.Open;
                if (task.Id == 0)
                {
                    qbicle.LastUpdated = DateTime.UtcNow;
                    task.Qbicle = qbicle;
                }

                task.ActivityType = QbicleActivity.ActivityTypeEnum.TaskActivity;
                if (!task.ProgrammedStart.HasValue)
                    task.ProgrammedStart = DateTime.UtcNow;
                task.TimeLineDate = DateTime.UtcNow;
                if (task.DurationUnit == QbicleTask.TaskDurationUnitEnum.Days)
                    task.ProgrammedEnd = task.ProgrammedStart.Value.AddDays(task.Duration);
                else if (task.DurationUnit == QbicleTask.TaskDurationUnitEnum.Hours)
                    task.ProgrammedEnd = task.ProgrammedStart.Value.AddHours(task.Duration);
                else
                    task.ProgrammedEnd = task.ProgrammedStart.Value.AddDays(task.Duration * 7);
                QbicleMedia m = null;
                if (!string.IsNullOrEmpty(media.Name))
                {
                    //Media attach
                    m = new QbicleMedia
                    {
                        StartedBy = currentUser,
                        StartedDate = DateTime.UtcNow,
                        Name = task.Name,
                        FileType = media.Type,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = task.Topic,

                        MediaFolder =
                            new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, qbicleId),
                        Description = task.Description,
                        IsVisibleInQbicleDashboard = false
                    };
                    var versionFile = new VersionedFile
                    {
                        IsDeleted = false,
                        FileSize = media.Size,
                        UploadedBy = currentUser,
                        UploadedDate = DateTime.UtcNow,
                        Uri = media.UrlGuid,
                        FileType = media.Type
                    };
                    m.VersionedFiles.Add(versionFile);
                    m.ActivityMembers.Add(currentUser);

                    dbContext.Medias.Add(m);
                    dbContext.Entry(m).State = EntityState.Added;

                    task.SubActivities.Add(m);
                }

                #region Steps

                var steps = dbContext.Steps.Where(s => s.ActivityId == task.Id).ToList();

                if (steps.Count > 0)
                    foreach (var item in steps)
                    {
                        var sti = item.StepInstance.FirstOrDefault(s => s.Step.Id == item.Id && s.Task.Id == task.Id);
                        if (sti != null)
                            dbContext.Stepinstances.Remove(sti);
                        dbContext.Steps.Remove(item);
                    }

                if (task.isSteps)
                    foreach (var item in stepLst)
                    {
                        if (task.Id > 0)
                            item.ActivityId = task.Id;
                        dbContext.Steps.Add(item);
                        dbContext.Entry(item).State = EntityState.Added;
                        task.QSteps.Add(item);
                    }

                #endregion
                SpanneredWorkgroup workgroup = dbContext.SpanneredWorkgroups.FirstOrDefault(w => w.Id == workgroupId);
                Asset asset = dbContext.SpanneredAssets.FirstOrDefault(s => s.Id == assetId);

                var dbtask = dbContext.QbicleTasks.Find(task.Id);
                QbicleSet set;
                if (dbtask == null)
                {
                    dbtask = task;
                    if (workgroup != null)
                    {
                        dbtask.Workgroup = workgroup;
                    }
                    if (asset != null)
                    {
                        asset.Tasks.Add(dbtask);
                    }
                    set = new QbicleSet();
                    dbtask.AssociatedSet = set;
                    dbtask.App = QbicleActivity.ActivityApp.Spannered;
                }
                else
                {
                    dbtask.Topic = task.Topic;
                    dbtask.Name = task.Name;
                    dbtask.Description = task.Description;
                    dbtask.Duration = task.Duration;
                    dbtask.DurationUnit = task.DurationUnit;
                    dbtask.Priority = task.Priority;
                    dbtask.ProgrammedStart = task.ProgrammedStart;
                    dbtask.ProgrammedEnd = task.ProgrammedEnd;
                    dbtask.isSteps = task.isSteps;
                    dbtask.isRecurs = task.isRecurs;
                    dbtask.QSteps = task.QSteps;
                    dbtask.MeterThreshold = task.MeterThreshold;
                    if (workgroup != null)
                    {
                        dbtask.Workgroup = workgroup;
                    }
                    if (m != null)
                        dbtask.SubActivities.Add(m);
                    if (dbtask != null && dbtask.AssociatedSet != null)
                    {
                        set = dbtask.AssociatedSet;
                        //var lstTaskOldRemove = dbContext.QbicleTasks.Where(s => s.AssociatedSet.Id == Set.Id).ToList();
                        //if (lstTaskOldRemove.Count > 0)
                        //{
                        //    dbContext.QbicleTasks.RemoveRange(lstTaskOldRemove.Where(p => p.Id != dbtask.Id));
                        //}
                    }
                    else
                    {
                        set = new QbicleSet();
                        dbContext.Sets.Add(set);
                        dbContext.Entry(set).State = EntityState.Added;
                        dbtask.AssociatedSet = set;
                    }

                    //Update LastUpdated currentDomain
                    qbicle.LastUpdated = DateTime.UtcNow;
                    if (dbContext.Entry(qbicle).State == EntityState.Detached)
                        dbContext.Qbicles.Attach(qbicle);
                    dbContext.Entry(qbicle).State = EntityState.Modified;


                    //end
                }

                //link
                if (set.Id == 0)
                {
                    dbContext.Sets.Add(set);
                    dbContext.Entry(set).State = EntityState.Added;
                }
                #region Peobles

                //Task People (People->Type 0 = Assignee);(People->Type 1 = Watcher)
                if (!string.IsNullOrEmpty(assignee))
                {
                    var peopleAssignee = dbContext.People.Where(s =>
                            s.AssociatedSet.Id == set.Id && s.Type == QbiclePeople.PeopleTypeEnum.Assignee)
                        .FirstOrDefault();
                    if (peopleAssignee == null)
                    {
                        peopleAssignee = new QbiclePeople();
                        peopleAssignee.isPresent = true;
                        peopleAssignee.Type = QbiclePeople.PeopleTypeEnum.Assignee;
                        var user = dbContext.QbicleUser.Find(assignee);
                        if (user != null)
                        {
                            peopleAssignee.User = user;
                            peopleAssignee.AssociatedSet = set;
                            dbContext.People.Add(peopleAssignee);
                            dbContext.Entry(peopleAssignee).State = EntityState.Added;
                        }
                    }
                    else if (peopleAssignee.User.Id != assignee)
                    {
                        var user = dbContext.QbicleUser.Find(assignee);
                        if (user != null)
                        {
                            peopleAssignee.User = user;
                            //dbContext.People.Add(peopleAssignee);
                            if (dbContext.Entry(peopleAssignee).State == EntityState.Detached)
                                dbContext.People.Attach(peopleAssignee);
                            dbContext.Entry(peopleAssignee).State = EntityState.Modified;
                        }
                    }
                }

                //Remove Watchers Old
                var peoplesWatch = dbContext.People.Where(s =>
                    s.AssociatedSet.Id == set.Id && s.Type == QbiclePeople.PeopleTypeEnum.Watcher).ToList();
                if (peoplesWatch.Count > 0) dbContext.People.RemoveRange(peoplesWatch);
                if (watchers != null && watchers.Any())
                    foreach (var item in watchers)
                        if (item != assignee)
                        {
                            var peopleWatcher = new QbiclePeople
                            {
                                isPresent = true,
                                Type = QbiclePeople.PeopleTypeEnum.Watcher
                            };
                            var user = dbContext.QbicleUser.Find(item);
                            if (user != null)
                            {
                                peopleWatcher.User = user;
                                peopleWatcher.AssociatedSet = set;
                                dbContext.People.Add(peopleWatcher);
                                dbContext.Entry(peopleWatcher).State = EntityState.Added;
                            }
                        }

                #endregion

                #region Related

                var relates = dbContext.Relateds.Where(s => s.AssociatedSet.Id == set.Id).ToList();
                if (relates.Count > 0) dbContext.Relateds.RemoveRange(relates);
                if (activitiesRelate != null && activitiesRelate.Length > 0)
                    foreach (var item in activitiesRelate)
                    {
                        var activity = dbContext.Activities.Find(item);
                        if (activity != null)
                        {
                            var rl = new QbicleRelated { AssociatedSet = set, Activity = activity };
                            dbContext.Relateds.Add(rl);
                            dbContext.Entry(rl).State = EntityState.Added;
                        }
                    }

                #endregion
                if (!string.IsNullOrEmpty(inventoriescps))
                {
                    var assetTaskCPSItem = JsonConvert.DeserializeObject<List<AssetTaskCPSItem>>(inventoriescps);
                    foreach (var item in assetTaskCPSItem)
                    {
                        ConsumablesPartServiceItem ai = null;
                        if (item.Id > 0)
                        {
                            ai = dbContext.SpanneredTaskConsumablesPartServiceItems.Find(item.Id);
                            ai.Allocated = item.Allocated;
                            if (dbContext.Entry(ai).State == EntityState.Detached)
                                dbContext.SpanneredTaskConsumablesPartServiceItems.Attach(ai);
                            dbContext.Entry(ai).State = EntityState.Modified;
                        }
                        else
                        {
                            ai = new ConsumablesPartServiceItem();
                            ai.QbicleTask = dbtask;
                            ai.AssetInventory = dbContext.SpanneredAssetInventories.Find(item.AssetInventoryId);
                            ai.Allocated = item.Allocated;
                            ai.CreatedBy = currentUser;
                            ai.CreatedDate = DateTime.UtcNow;
                            dbContext.SpanneredTaskConsumablesPartServiceItems.Add(ai);
                            dbContext.Entry(ai).State = EntityState.Added;
                        }
                    }
                }
                if (dbtask.Id > 0)
                {
                    if (dbContext.Entry(dbtask).State == EntityState.Detached)
                        dbContext.QbicleTasks.Attach(dbtask);
                    dbContext.Entry(dbtask).State = EntityState.Modified;
                }
                else
                {
                    dbContext.QbicleTasks.Add(dbtask);
                    dbContext.Entry(dbtask).State = EntityState.Added;
                }

                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = task.Id,
                    EventNotify = NotificationEventEnum.TaskCreation,
                    AppendToPageName = ApplicationPageName.Link,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = currentUser.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.Notification2Activity(activityNotification);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex, userId);
                return false;
            }
        }

        public List<AssetTasksModel> GetAssetTasks(int assetId, string search, string status, string assigneeId, string currentUserId, int column, string orderBy, int start, int length, ref int totalRecord, string dateFormat)
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                Asset asset = dbContext.SpanneredAssets.Find(assetId);
                List<AssetTasksModel> lstModel = new List<AssetTasksModel>();
                var query = asset.Tasks.AsQueryable();
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(t => t.Name.Trim().ToLower().Contains(search.Trim().ToLower()));
                }

                if (!string.IsNullOrEmpty(assigneeId))
                {
                    query = query.Where(t => t.AssociatedSet.Peoples.Any(a => (a.User.Id == assigneeId || assigneeId == " ") && a.Type == PeopleTypeEnum.Assignee));
                }

                switch (status)
                {
                    case "Pending":
                        query = query.Where(t => !t.isComplete && t.ActualStart == null && t.ProgrammedEnd >= currentDate); break;
                    case "In progress":
                        query = query.Where(t => !t.isComplete && t.ActualStart != null && t.ProgrammedEnd >= currentDate); break;
                    case "Overdue":
                        query = query.Where(t => !t.isComplete && t.ProgrammedEnd < currentDate); break;
                    case "Complete":
                        query = query.Where(t => t.isComplete); break;
                    default:
                        query = query.Where(t => !t.isComplete); break;
                }

                List<QbicleTask> lstQbicleTask = query.Skip(start).Take(length).ToList();
                totalRecord = query.Count();

                foreach (QbicleTask task in lstQbicleTask)
                {
                    AssetTasksModel model = new AssetTasksModel();
                    model.Id = task.Id;
                    model.ActivityKey = task.Key;
                    model.Name = task.Name;
                    model.MeterThreshold = task.MeterThreshold;
                    if (!task.isComplete && task.ActualStart == null && task.ProgrammedEnd >= currentDate)
                    { model.Status = "Pending"; }
                    else if (!task.isComplete && task.ActualStart != null && task.ProgrammedEnd >= currentDate)
                    { model.Status = "In progress"; }
                    else if (!task.isComplete && task.ProgrammedEnd < currentDate)
                    { model.Status = "Overdue"; }
                    else if (task.isComplete)
                    { model.Status = "Complete"; }
                    model.Created = task.StartedDate.ToString(dateFormat) + " " + task.StartedDate.ToString("hh:mmtt").ToLower();
                    model.Due = task.StartedDate.ToString(dateFormat) + " " + task.StartedDate.ToString("hh:mmtt").ToLower();
                    if (task.ProgrammedStart != null)
                    {
                        model.Due = task.ProgrammedStart?.ToString(dateFormat) + " " + task.ProgrammedStart?.ToString("hh:mmtt").ToLower();
                    }

                    model.Assignee = HelperClass.GetFullNameOfUser(task.AssociatedSet.Peoples.Where(p => p.Type == PeopleTypeEnum.Assignee).FirstOrDefault()?.User, currentUserId);
                    model.IsAllowEdit = task.Workgroup.Processes.Any(p => p.Name.Equals("Asset Tasks")) && (task.Workgroup.CreatedBy.Id.Equals(currentUserId) || task.Workgroup.Members.Any(m => m.Id == currentUserId));
                    lstModel.Add(model);
                }

                var queryModel = lstModel.AsQueryable();
                if (column == 0)
                {
                    if (orderBy.Equals("asc"))
                    {
                        queryModel = queryModel.OrderBy(p => p.Id);
                    }
                    else
                    {
                        queryModel = queryModel.OrderByDescending(p => p.Id);
                    }
                }
                if (column == 1)
                {
                    if (orderBy.Equals("asc"))
                    {
                        queryModel = queryModel.OrderBy(p => p.Created);
                    }
                    else
                    {
                        queryModel = queryModel.OrderByDescending(p => p.Created);
                    }
                }
                if (column == 2)
                {
                    if (orderBy.Equals("asc"))
                    {
                        queryModel = queryModel.OrderBy(p => p.Assignee);
                    }
                    else
                    {
                        queryModel = queryModel.OrderByDescending(p => p.Assignee);
                    }
                }
                if (column == 3)
                {
                    if (orderBy.Equals("asc"))
                    {
                        queryModel = queryModel.OrderBy(p => p.Due);
                    }
                    else
                    {
                        queryModel = queryModel.OrderByDescending(p => p.Due);
                    }
                }
                if (column == 4)
                {
                    if (orderBy.Equals("asc"))
                    {
                        queryModel = queryModel.OrderBy(p => p.MeterThreshold);
                    }
                    else
                    {
                        queryModel = queryModel.OrderByDescending(p => p.MeterThreshold);
                    }
                }
                if (column == 5)
                {
                    if (orderBy.Equals("asc"))
                    {
                        queryModel = queryModel.OrderBy(p => p.Status);
                    }
                    else
                    {
                        queryModel = queryModel.OrderByDescending(p => p.Status);
                    }
                }

                return queryModel.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                totalRecord = 0;
                return new List<AssetTasksModel>();
            }
        }
        public DataTablesResponse GetListAssociatedTraderItem(IDataTablesRequest requestModel, int domainId, int locationId, int spwgid, string keyword, int groupId, int itemLink)
        {
            try
            {
                var spanneredwg = dbContext.SpanneredWorkgroups.Find(spwgid);
                var wgs = (spanneredwg != null && spanneredwg.ProductGroups.Any() ? spanneredwg.ProductGroups.Select(s => s.Id).ToList() : new List<int>());
                int totalcount = 0;
                var query = from it in dbContext.TraderItems
                            join dt in dbContext.InventoryDetails on it.Id equals dt.Item.Id
                            where it.Domain.Id == domainId
                            && dt.Location.Id == locationId
                            && wgs.Contains(it.Group.Id)
                            select it;
                #region Filter
                if (itemLink > 0)
                    query = query.Where(s => s.Id == itemLink);
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q => q.Name.Contains(keyword) || q.Barcode.Contains(keyword) || q.SKU.Contains(keyword));
                if (groupId > 0)
                    query = query.Where(q => q.Group.Id == groupId);
                totalcount = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Item":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Barcode":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Barcode" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "SKU":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "SKU" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Group":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Group.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Name asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "Name asc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Item = q.Name,
                    q.Barcode,
                    q.SKU,
                    Group = q.Group.Name,
                    Islink = (q.Id == itemLink ? true : false)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public ReturnJsonModel GetListTraderItem(int domainId, int locationId, int spwgid, string search, int itemId)
        {
            var returnJson = new ReturnJsonModel() { result = true };
            try
            {
                if (itemId == 0)
                {
                    var spanneredwg = dbContext.SpanneredWorkgroups.Find(spwgid);
                    var wgs = (spanneredwg != null && spanneredwg.ProductGroups.Any() ? spanneredwg.ProductGroups.Select(s => s.Id).ToList() : new List<int>());
                    var query = from it in dbContext.TraderItems
                                join dt in dbContext.InventoryDetails on it.Id equals dt.Item.Id
                                where it.Domain.Id == domainId
                                && dt.Location.Id == locationId
                                && wgs.Contains(it.Group.Id)
                                select it;
                    if (!string.IsNullOrEmpty(search))
                    {
                        search = search.ToLower();
                        query = query.Where(q => q.Name.ToLower().Contains(search) || q.Barcode.ToLower().Contains(search) || q.SKU.ToLower().Contains(search));
                    }

                    returnJson.Object = query.ToList().Select(s => new
                    {
                        id = s.Id,
                        text = $"{s.SKU} - {s.Name}",
                        units = s.Units.Select(u => new { id = u.Id, text = u.Name }).ToList(),
                        itemname = s.Name,
                        barcode = s.Barcode,
                        sku = s.SKU
                    }).ToList();
                }
                else
                {
                    var item = dbContext.TraderItems.Find(itemId);
                    returnJson.Object = new
                    {
                        id = item.Id,
                        text = $"{item.SKU} - {item.Name}",
                        units = item.Units.Select(u => new { id = u.Id, text = u.Name }).ToList(),
                        itemname = item.Name,
                        barcode = item.Barcode,
                        sku = item.SKU
                    };
                }


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                returnJson.result = false;
            }
            return returnJson;

        }
        public ReturnJsonModel SaveTransferAsset(int domainId, int assetId, TraderTransfer transfer, string userId, string originatingConnectionId = "")
        {
            var returnJson = new ReturnJsonModel() { result = false };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, transfer);
                    var transferItemRule = new TraderItemRules(dbContext);

                    var user = dbContext.QbicleUser.Find(userId);
                    transfer.CreatedBy = user;
                    int transferId;
                    if (transfer.Reference == null)
                    {
                        transfer.Reference = new TraderReferenceRules(dbContext).GetNewReference(domainId, Models.Trader.TraderReferenceType.Transfer);
                    }
                    var dbAsset = dbContext.SpanneredAssets.Find(assetId);
                    #region Link TransferItem from Asset
                    if ((transfer.TransferItems == null || !transfer.TransferItems.Any()) && dbAsset != null)
                    {
                        var transferItem = new TraderTransferItem();
                        transferItem.TraderItem = dbAsset.AssociatedTraderItem;
                        transferItem.QuantityAtPickup = 1;
                        transferItem.Unit = dbAsset.AssociatedTraderItem.Units.FirstOrDefault(s => s.IsBase);
                        transfer.TransferItems.Add(transferItem);
                    }
                    #endregion

                    var locationRule = new TraderLocationRules(dbContext);
                    var transferType = TransferType.P2P;

                    if (transfer.Id > 0)
                    {
                        transferId = transfer.Id;
                        var trDb = dbContext.TraderTransfers.Find(transfer.Id);
                        trDb.Status = transfer.Status;
                        while (trDb.TransferItems.Count > 0)
                        {
                            dbContext.TraderTransferItems.Remove(trDb.TransferItems[0]);
                            dbContext.SaveChanges();
                        }


                        if (transfer.Sale != null || transfer.Purchase != null)
                        {
                            if (transfer.Sale?.Id > 0)
                            {
                                transferType = TransferType.Sale;
                                var sale = new TraderSaleRules(dbContext).GetById(transfer.Sale.Id);
                                transfer.Sale = sale;
                                transfer.DestinationLocation = null;
                                transfer.OriginatingLocation = sale.Location;
                                transfer.Address = sale.DeliveryAddress;
                                transfer.Contact = sale.Purchaser;
                                transfer.Reason = TransferReasonEnum.Sale;
                            }
                            else if (transfer.Purchase?.Id > 0)
                            {
                                transferType = TransferType.Purchase;
                                var purchase = new TraderPurchaseRules(dbContext).GetById(transfer.Purchase.Id);
                                transfer.Purchase = purchase;
                                transfer.DestinationLocation = purchase.Location;
                                transfer.OriginatingLocation = null;
                                transfer.Address = purchase.Vendor.Address;
                                transfer.Contact = purchase.Vendor;
                                transfer.Reason = TransferReasonEnum.Purchase;
                            }
                        }
                        else
                        {
                            transfer.Reason = TransferReasonEnum.PointToPoint;
                            transferType = TransferType.P2P;
                            trDb.DestinationLocation = locationRule.GetById(transfer.DestinationLocation?.Id ?? 0);
                            trDb.OriginatingLocation = locationRule.GetById(transfer.OriginatingLocation?.Id ?? 0);
                            trDb.Workgroup = new TraderWorkGroupsRules(dbContext).GetById(transfer.Workgroup.Id);
                        }
                        foreach (var item in transfer.TransferItems)
                        {
                            var transactionItem = transferType == TransferType.P2P ? null : dbContext.TraderSaleItems.Find(item.TransactionItem?.Id);
                            trDb.TransferItems.Add(new TraderTransferItem
                            {
                                Unit = dbContext.ProductUnits.Find(item.Unit?.Id),
                                QuantityAtPickup = item.QuantityAtPickup,//By default the Quantity at Delivery is to be set to the Quantity at Pickup
                                                                         //It will be up to the person indicating that delivery has occurred to indicate that the quantities were not the same.
                                QuantityAtDelivery = item.QuantityAtPickup,
                                TransactionItem = transactionItem,
                                TraderItem = transferItemRule.GetById(item.TraderItem?.Id ?? 0)
                            });
                        }

                        trDb.Reference = transfer.Reference;
                        if (dbContext.Entry(trDb).State == EntityState.Detached)
                            dbContext.TraderTransfers.Attach(trDb);
                        dbContext.Entry(trDb).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        var traderTransfer = new TraderTransfer
                        {
                            CreatedBy = transfer.CreatedBy,
                            CreatedDate = DateTime.UtcNow,
                            DestinationLocation = locationRule.GetById(transfer.DestinationLocation?.Id ?? 0),
                            OriginatingLocation = locationRule.GetById(transfer.OriginatingLocation?.Id ?? 0),
                            Workgroup = new TraderWorkGroupsRules(dbContext).GetById(transfer.Workgroup.Id),
                            Status = transfer.Status,
                            Reference = transfer.Reference,
                            Reason = TransferReasonEnum.PointToPoint
                        };

                        if (transfer.Sale?.Id > 0)
                        {
                            transferType = TransferType.Sale;
                            var sale = new TraderSaleRules(dbContext).GetById(transfer.Sale.Id);
                            traderTransfer.Sale = sale;
                            traderTransfer.DestinationLocation = null;
                            traderTransfer.OriginatingLocation = sale.Location;
                            traderTransfer.Address = sale.DeliveryAddress;
                            traderTransfer.Contact = sale.Purchaser;
                            traderTransfer.Reason = TransferReasonEnum.Sale;
                        }
                        else if (transfer.Purchase?.Id > 0)
                        {
                            transferType = TransferType.Purchase;
                            var purchase = new TraderPurchaseRules(dbContext).GetById(transfer.Purchase.Id);
                            traderTransfer.Purchase = purchase;
                            traderTransfer.OriginatingLocation = null;
                            traderTransfer.DestinationLocation = purchase.Location;
                            traderTransfer.Address = purchase.Vendor.Address;
                            traderTransfer.Contact = purchase.Vendor;
                            traderTransfer.Reason = TransferReasonEnum.Purchase;
                        }

                        var transferItems = new List<TraderTransferItem>();
                        foreach (var item in transfer.TransferItems)
                        {
                            var transactionItem = transferType == TransferType.P2P ? null : dbContext.TraderSaleItems.Find(item.TransactionItem?.Id);
                            transferItems.Add(new TraderTransferItem
                            {
                                Unit = dbContext.ProductUnits.Find(item.Unit?.Id),
                                QuantityAtPickup = item.QuantityAtPickup,//By default the Quantity at Delivery is to be set to the Quantity at Pickup
                                                                         //It will be up to the person indicating that delivery has occurred to indicate that the quantities were not the same.
                                QuantityAtDelivery = item.QuantityAtPickup,
                                TransactionItem = transactionItem,
                                TraderItem = transferItemRule.GetById(item.TraderItem?.Id ?? 0)
                            });
                        }
                        traderTransfer.TransferItems.AddRange(transferItems);


                        dbContext.TraderTransfers.Add(traderTransfer);
                        dbContext.Entry(traderTransfer).State = EntityState.Added;
                        dbContext.SaveChanges();
                        transferId = traderTransfer.Id;
                    }

                    var tradTransfer = dbContext.TraderTransfers.Find(transferId);

                    tradTransfer.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                    var appDef =
                        dbContext.TransferApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == tradTransfer.Workgroup.Id);
                    var refFull = tradTransfer.Reference == null ? "" : tradTransfer.Reference.FullRef;
                    var approval = new ApprovalReq
                    {
                        ApprovalRequestDefinition = appDef,
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                        Priority = ApprovalReq.ApprovalPriorityEnum.High,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                        Qbicle = tradTransfer.Workgroup.Qbicle,
                        Topic = tradTransfer.Workgroup.Topic,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Notes = "",
                        IsVisibleInQbicleDashboard = true,
                        App = QbicleActivity.ActivityApp.Trader,
                        Name = $"Trader Transfer Process #{refFull}",
                        Transfer = new List<TraderTransfer> { tradTransfer },
                        //Sale = new List<TraderSale> { appSale },
                        //Purchase = new List<TraderPurchase> { appPurchase }
                    };
                    tradTransfer.TransferApprovalProcess = approval;
                    approval.ActivityMembers.AddRange(tradTransfer.Workgroup.Members);
                    dbContext.Entry(tradTransfer).State = EntityState.Modified;
                    //Add associcated the Transfer for the Asset
                    if (dbAsset != null)
                        dbAsset.Transfers.Add(tradTransfer);
                    //end
                    var transferLog = new TransferLog
                    {
                        Address = tradTransfer.Address,
                        AssociatedTransfer = tradTransfer,
                        Contact = tradTransfer.Contact,
                        CreatedDate = DateTime.UtcNow,
                        Purchase = tradTransfer.Purchase,
                        Sale = tradTransfer.Sale,
                        TransferApprovalProcess = approval,
                        Status = tradTransfer.Status,
                        UpdatedBy = user,
                        AssociatedShipment = tradTransfer.AssociatedShipment,
                        DestinationLocation = tradTransfer.DestinationLocation,
                        OriginatingLocation = tradTransfer.OriginatingLocation,
                        TransferItems = tradTransfer.TransferItems,
                        Workgroup = tradTransfer.Workgroup
                    };
                    var transferProcessLog = new TransferProcessLog
                    {
                        AssociatedTransfer = tradTransfer,
                        AssociatedTransferLog = transferLog,
                        TransferStatus = tradTransfer.Status,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        ApprovalReqHistory = new ApprovalReqHistory
                        {
                            ApprovalReq = approval,
                            UpdatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            RequestStatus = approval.RequestStatus
                        }
                    };
                    dbContext.TraderTransferProcessLogs.Add(transferProcessLog);
                    dbContext.Entry(transferProcessLog).State = EntityState.Added;
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbTransaction.Commit();
                    var nRule = new NotificationRules(dbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = approval.Id,
                        EventNotify = NotificationEventEnum.ApprovalCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userId,
                        CreatedByName = user.GetFullName(),
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);

                }
                catch (Exception e)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, transfer);
                    dbTransaction.Rollback();
                }
            }

            return returnJson;
        }
        public ReturnJsonModel SaveTraderPurchaseForAsset(int assetId, TraderPurchase traderPurchase, string userId, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, traderPurchase);

                    traderPurchase.CreatedDate = DateTime.UtcNow;
                    if (traderPurchase.Reference != null)
                        traderPurchase.Reference = new TraderReferenceRules(dbContext).GetById(traderPurchase.Reference.Id);

                    var user = dbContext.QbicleUser.Find(userId);
                    //traderPurchase.CreatedBy = CurrentUser(CurrentQbicleId());
                    int purchaseId;
                    if (traderPurchase.PurchaseItems.Count > 0)
                    {
                        foreach (var item in traderPurchase.PurchaseItems)
                        {
                            if (item.Id == 0)
                            {
                                item.CreatedDate = DateTime.UtcNow;
                                item.CreatedBy = user;
                            }
                            else
                            {
                                item.LastUpdatedBy = user;
                                item.LastUpdatedDate = DateTime.UtcNow;
                            }
                            item.TraderItem = dbContext.TraderItems.Find(item.TraderItem.Id);
                            if (item.TraderItem != null && traderPurchase.Location != null && item.PriceBookPrice == null)
                            {
                                var traderId = item.TraderItem.Id;
                                item.PriceBookPrice = dbContext.TraderPrices.FirstOrDefault(q => q.Location.Id == traderPurchase.Location.Id && q.Item.Id == traderId && q.SalesChannel == Models.Trader.SalesChannel.SalesChannelEnum.Trader);
                            }
                            else if (item.PriceBookPrice != null)
                            {
                                item.PriceBookPrice = dbContext.TraderPrices.Find(item.PriceBookPrice.Id);
                            }
                            if (item.PriceBookPrice != null)
                            {
                                item.PriceBookPriceValue = item.PriceBookPrice.NetPrice;
                            }
                            if (item.Dimensions.Count > 0)
                            {
                                for (var j = 0; j < item.Dimensions.Count; j++)
                                {
                                    item.Dimensions[j] =
                                        dbContext.TransactionDimensions.Find(item.Dimensions[j].Id);
                                }
                            }
                            if (item.Unit != null)
                            {
                                item.Unit =
                                    dbContext.ProductUnits.Find(item.Unit.Id);
                            }
                            //Update Taxes TraderTransactionItem
                            if (item.TraderItem != null)
                            {
                                foreach (var tax in item.TraderItem.TaxRates.Where(s => s.IsPurchaseTax).ToList())
                                {
                                    var staticTaxRate = new TaxRateRules(dbContext).CloneStaticTaxRateById(tax.Id);
                                    OrderTax orderTax = new OrderTax
                                    {
                                        StaticTaxRate= staticTaxRate,
                                        TaxRate = tax,
                                        Value = item.CostPerUnit * item.Quantity * (1 - (item.Discount / 100)) * (tax.Rate / 100)
                                    };
                                    item.Taxes.Add(orderTax);
                                }
                            }
                        }
                    }


                    if (traderPurchase.Vendor.Id != 0)
                    {
                        traderPurchase.Vendor = dbContext.TraderContacts.Find(traderPurchase.Vendor.Id);
                        if (traderPurchase.Vendor != null)
                            traderPurchase.Vendor.InUsed = true;
                    }
                    if (traderPurchase.Workgroup != null && traderPurchase.Workgroup.Id > 0)
                    {
                        traderPurchase.Workgroup = dbContext.WorkGroups.Find(traderPurchase.Workgroup.Id);

                    }

                    if (traderPurchase.Id == 0)
                    {
                        traderPurchase.CreatedBy = user;
                        dbContext.TraderPurchases.Add(traderPurchase);
                        dbContext.Entry(traderPurchase).State = EntityState.Added;
                        dbContext.SaveChanges();
                        result.actionVal = 1;
                        purchaseId = traderPurchase.Id;
                        //Whenever a TraderTransactionItem is saved to the database a copy of the TraderTransactionItem is saved to the TransactionItemLog table.
                        foreach (var item in traderPurchase.PurchaseItems)
                        {
                            var transItemLog = new TransactionItemLog
                            {
                                Unit = item.Unit,
                                AssociatedTransactionItem = item,
                                Cost = item.Cost,
                                CostPerUnit = item.CostPerUnit,
                                Dimensions = item.Dimensions,
                                Discount = item.Discount,
                                Price = item.Price,
                                PriceBookPrice = item.PriceBookPrice,
                                PriceBookPriceValue = item.PriceBookPriceValue,
                                Quantity = item.Quantity,
                                SalePricePerUnit = item.SalePricePerUnit,
                                TraderItem = item.TraderItem,
                                TransferItems = item.TransferItems
                            };
                            dbContext.Entry(transItemLog).State = EntityState.Added;
                            dbContext.TraderTransactionItemLogs.Add(transItemLog);
                        }
                        //Add related purchases for Asset
                        if (assetId > 0)
                        {
                            var dbAsset = dbContext.SpanneredAssets.Find(assetId);
                            dbAsset.AssetTraderPurchases.Add(traderPurchase);
                        }
                        //end
                        dbContext.SaveChanges();
                    }
                    else
                    {


                        var purchaseDb = dbContext.TraderPurchases.Find(traderPurchase.Id);
                        if (traderPurchase.Reference != null)
                        {
                            purchaseDb.Reference = traderPurchase.Reference;
                        }
                        purchaseDb.Status = traderPurchase.Status;
                        purchaseDb.DeliveryMethod = traderPurchase.DeliveryMethod;
                        purchaseDb.Vendor = traderPurchase.Vendor;
                        purchaseDb.PurchaseTotal = traderPurchase.PurchaseTotal;
                        purchaseDb.Workgroup = traderPurchase.Workgroup;
                        dbContext.Entry(purchaseDb).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        result.actionVal = 2;
                        purchaseId = purchaseDb.Id;

                        //Update Transaction Items

                        var itemsUi = traderPurchase.PurchaseItems;
                        var itemsDb = purchaseDb.PurchaseItems;

                        var itemsNew = itemsUi.Where(c => !itemsDb.Any(d => c.Id == d.Id)).ToList();

                        var itemsDelete = itemsDb.Where(c => itemsUi.All(d => c.Id != d.Id)).ToList();

                        var itemsUpdate = itemsUi.Where(c => itemsDb.Any(d => c.Id == d.Id)).ToList();


                        foreach (var itemDel in itemsDelete)
                        {
                            //remove Order Tax
                            if (itemDel.Taxes != null && itemDel.Taxes.Any())
                                dbContext.OrderTaxs.RemoveRange(itemDel.Taxes);
                            if (itemDel.Logs.Any())
                            {
                                dbContext.TraderTransactionItemLogs.RemoveRange(itemDel.Logs);
                            }
                            purchaseDb.PurchaseItems.Remove(itemDel);
                            dbContext.Entry(purchaseDb).State = EntityState.Modified;
                            dbContext.SaveChanges();
                        }

                        foreach (var iDb in purchaseDb.PurchaseItems)
                        {
                            var iUpdate = itemsUpdate.FirstOrDefault(e => e.Id == iDb.Id);
                            iDb.Dimensions.Clear();
                            if (iDb.Logs.Any())
                            {
                                dbContext.TraderTransactionItemLogs.RemoveRange(iDb.Logs);
                            }
                            if (iUpdate == null) continue;
                            iDb.Unit = iUpdate.Unit;
                            iDb.Cost = iUpdate.Cost;
                            //it.Id = iUi.Id;
                            //it.CreatedDate = DateTime.UtcNow;
                            iDb.Price = iUpdate.Price;
                            //it.CreatedBy = user;
                            iDb.Discount = iUpdate.Discount;
                            iDb.Quantity = iUpdate.Quantity;
                            iDb.TraderItem = iUpdate.TraderItem;
                            iDb.PriceBookPrice = iUpdate.PriceBookPrice;
                            iDb.LastUpdatedBy = user;
                            iDb.TransferItems = iUpdate.TransferItems;
                            iDb.Dimensions = iUpdate.Dimensions;
                            iDb.LastUpdatedDate = DateTime.UtcNow;
                            //iDb.Logs = iUpdate.Logs;
                            iDb.CostPerUnit = iUpdate.CostPerUnit;
                            iDb.PriceBookPriceValue = iUpdate.PriceBookPriceValue;
                            iDb.SalePricePerUnit = iUpdate.SalePricePerUnit;
                            //remove Order Tax
                            if (iDb.Taxes != null && iDb.Taxes.Any())
                                dbContext.OrderTaxs.RemoveRange(iDb.Taxes);
                            iDb.Taxes = iUpdate.Taxes;


                            dbContext.Entry(iDb).State = EntityState.Modified;
                            dbContext.SaveChanges();

                            var transItemLog = new TransactionItemLog
                            {
                                Unit = iDb.Unit,
                                AssociatedTransactionItem = iDb,
                                Cost = iDb.Cost,
                                CostPerUnit = iDb.CostPerUnit,
                                Dimensions = iDb.Dimensions,
                                Discount = iDb.Discount,
                                Price = iDb.Price,
                                PriceBookPrice = iDb.PriceBookPrice,
                                PriceBookPriceValue = iDb.PriceBookPriceValue,
                                Quantity = iDb.Quantity,
                                SalePricePerUnit = iDb.SalePricePerUnit,
                                TraderItem = iDb.TraderItem,
                                TransferItems = iDb.TransferItems
                            };
                            dbContext.Entry(transItemLog).State = EntityState.Added;
                            dbContext.TraderTransactionItemLogs.Add(transItemLog);
                            dbContext.SaveChanges();
                        }
                        if (itemsNew.Count > 0)
                        {
                            purchaseDb.PurchaseItems.AddRange(itemsNew);
                            dbContext.Entry(purchaseDb).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            foreach (var item in itemsNew)
                            {
                                //Whenever a TraderTransactionItem is saved to the database a copy of the TraderTransactionItem is saved to the TransactionItemLog table.
                                var transItemLog = new TransactionItemLog
                                {
                                    Unit = item.Unit,
                                    AssociatedTransactionItem = item,
                                    Cost = item.Cost,
                                    CostPerUnit = item.CostPerUnit,
                                    Dimensions = item.Dimensions,
                                    Discount = item.Discount,
                                    Price = item.Price,
                                    PriceBookPrice = item.PriceBookPrice,
                                    PriceBookPriceValue = item.PriceBookPriceValue,
                                    Quantity = item.Quantity,
                                    SalePricePerUnit = item.SalePricePerUnit,
                                    TraderItem = item.TraderItem,
                                    TransferItems = item.TransferItems
                                };
                                dbContext.Entry(transItemLog).State = EntityState.Added;
                                dbContext.TraderTransactionItemLogs.Add(transItemLog);
                            }

                            dbContext.SaveChanges();


                        }
                    }



                    if (traderPurchase.Status != TraderPurchaseStatusEnum.PendingReview)
                    {
                        dbTransaction.Commit();
                        return result;
                    }


                    var tradPurchaseDb = dbContext.TraderPurchases.Find(purchaseId);

                    if (tradPurchaseDb?.PurchaseApprovalProcess != null)
                        return result;

                    tradPurchaseDb.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                    var refFull = tradPurchaseDb.Reference == null ? "" : tradPurchaseDb.Reference.FullRef;
                    var approval = new ApprovalReq
                    {
                        ApprovalRequestDefinition = dbContext.PurchaseApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == tradPurchaseDb.Workgroup.Id),
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                        Priority = ApprovalReq.ApprovalPriorityEnum.High,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                        Purchase = new List<TraderPurchase> { tradPurchaseDb },
                        Name = $"Trader Approval for Purchase #{refFull}",
                        Qbicle = tradPurchaseDb.Workgroup.Qbicle,
                        Topic = tradPurchaseDb.Workgroup.Topic,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        StartedBy = tradPurchaseDb.CreatedBy,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Notes = "",
                        App = QbicleActivity.ActivityApp.Trader
                    };

                    approval.ActivityMembers.AddRange(tradPurchaseDb.Workgroup.Members);
                    dbContext.ApprovalReqs.Add(approval);
                    dbContext.Entry(approval).State = EntityState.Added;
                    tradPurchaseDb.PurchaseApprovalProcess = approval;

                    dbContext.Entry(tradPurchaseDb).State = EntityState.Modified;


                    dbContext.SaveChanges();


                    var purchaseLog = new PurchaseLog
                    {
                        AssociatedPurchase = tradPurchaseDb,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        DeliveryMethod = tradPurchaseDb.DeliveryMethod,
                        Invoices = tradPurchaseDb.Invoices,
                        IsInHouse = false,
                        Location = tradPurchaseDb.Location,
                        PurchaseApprovalProcess = approval,
                        PurchaseItems = tradPurchaseDb.PurchaseItems,
                        PurchaseOrder = tradPurchaseDb.PurchaseOrder,
                        PurchaseTotal = tradPurchaseDb.PurchaseTotal,
                        Status = tradPurchaseDb.Status,
                        Transfer = null,
                        Vendor = tradPurchaseDb.Vendor,
                        Workgroup = tradPurchaseDb.Workgroup
                    };

                    var purchaseProcessLog = new PurchaseProcessLog
                    {
                        AssociatedPurchase = tradPurchaseDb,
                        AssociatedPurchaseLog = purchaseLog,
                        PurchaseStatus = tradPurchaseDb.Status,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = tradPurchaseDb.CreatedBy,
                        ApprovalReqHistory = new ApprovalReqHistory
                        {
                            ApprovalReq = approval,
                            UpdatedBy = user,
                            CreatedDate = DateTime.UtcNow,
                            RequestStatus = approval.RequestStatus
                        }

                    };

                    dbContext.TraderPurchaseProcessLogs.Add(purchaseProcessLog);
                    dbContext.Entry(purchaseProcessLog).State = EntityState.Added;
                    dbContext.SaveChanges();
                    dbTransaction.Commit();

                    var nRule = new NotificationRules(dbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = approval.Id,
                        EventNotify = NotificationEventEnum.ApprovalCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = user.Id,
                        CreatedByName = user.GetFullName(),
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);

                }
                catch (Exception e)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, traderPurchase);
                    result.result = false;
                    result.actionVal = 3;
                    result.msg = e.Message;
                    dbTransaction.Rollback();
                }
            }


            return result;

        }
        public DataTablesResponse GetSpanneredInventoryItems(IDataTablesRequest requestModel, int domainId, int locationId, string keyword, int groupId, int linkTaskId = 0, int itemId = 0, int unitId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId, locationId, keyword, groupId);
                int totalcount = 0;
                var query = dbContext.SpanneredAssetInventories.Where(s => s.ParentAsset.Location.Id == locationId)
                    .GroupBy(s => new { ItemId = s.Item.Id, UnitId = s.Unit.Id })
                    .Select(g => g.FirstOrDefault());
                #region Filter
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q => q.Item.Name.Contains(keyword) || q.Item.Barcode.Contains(keyword) || q.Item.SKU.Contains(keyword));
                if (groupId > 0)
                    query = query.Where(q => q.Item.Group.Id == groupId);
                if (itemId > 0)
                    query = query.Where(q => q.Item.Id == itemId);
                totalcount = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Item":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Item.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Unit":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Unit.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Barcode":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Item.Barcode" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "SKU":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Item.SKU" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Group":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Item.Group.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Item.Name asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "Item.Name asc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                //Get itemlinks
                List<ConsumeReportItemCustome> taskConsumeItems = null;
                if (linkTaskId > 0)
                {
                    var dbtask = dbContext.QbicleTasks.Find(linkTaskId);
                    taskConsumeItems = dbtask?.ConsumableItems.Select(s => new ConsumeReportItemCustome
                    {
                        ItemId = s.AssetInventory.Item.Id,
                        Allocated = s.Allocated
                    }).ToList() ?? null;
                }
                ProductUnit unit = null;
                if (unitId > 0)
                {
                    unit = dbContext.ProductUnits.Find(unitId);
                }

                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Item = q.Item.Name,
                    ItemId = q.Item.Id,
                    UnitId = unit == null ? q.Unit.Id : unit.Id,
                    Unit = q.Unit.Name,
                    q.Item.Barcode,
                    q.Item.SKU,
                    Group = q.Item.Group.Name,
                    CurrentStock = q.Item.GetInStockByItem(locationId, unit == null ? q.Unit : unit),
                    Allocated = (taskConsumeItems != null ? (taskConsumeItems.FirstOrDefault(s => s.ItemId == q.Item.Id)?.Allocated.ToString("F2") ?? "") : ""),
                    Used = "",
                    Units = (itemId > 0 ? q.Item.Units.Select(s => new { id = s.Id, text = s.Name }).ToList() : null)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public ReturnJsonModel GetItemProductByWorkgroupIsBought(QbicleDomain domain, int workGroupId, int locationId, string userId, int assetId)
        {
            var result = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, domain, workGroupId, locationId, assetId);
                result.result = true;
                var workgroups = new TraderWorkGroupsRules(dbContext).GetWorkGroups(locationId, domain, userId, TraderProcessName.TraderPurchaseProcessName);
                var productGroupIds = workgroups.Where(s => s.Id == workGroupId).SelectMany(q => q.ItemCategories.Select(i => i.Id)).Distinct().ToList();
                var query = dbContext.SpanneredAssetInventories.Where(s => s.ParentAsset.Location.Id == locationId && productGroupIds.Contains(s.Item.Group.Id) && (assetId == 0 || (assetId > 0 && s.ParentAsset.Id == assetId)))
                        .GroupBy(s => new { ItemId = s.Item.Id, UnitId = s.Unit.Id })
                        .Select(g => g.FirstOrDefault().Item);
                var traderItems = query.ToList().Select(i => new TraderItemModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    ImageUri = i.ImageUri,
                    CostUnit = 0,
                    TaxRateName = i.StringItemTaxRates(false),
                    TaxRateValue = i.SumTaxRatesPercent(false)
                    //WgIds = i.Group.WorkGroupCategories.Select(g => g.Id).ToList()
                }).ToList();
                result.Object = traderItems;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return result;
        }
        public ReturnJsonModel RemoveItemSpanneredByAssetInventoryId(int aiId)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, aiId);

                var dbassetinventory = dbContext.SpanneredAssetInventories.Find(aiId);
                if (dbassetinventory != null)
                {
                    dbContext.SpanneredAssetInventories.Remove(dbassetinventory);
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return returnJson;
        }
    }
}
