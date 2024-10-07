using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Broadcast;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class SocialIdeasThemeRule
    {
        #region init class
        private ApplicationDbContext dbContext;

        public SocialIdeasThemeRule(ApplicationDbContext context)
        {
            dbContext = context;
        }
        #endregion
        public IdeaTheme GetIdeaThemeById(int brandId)
        {
            return dbContext.SMIdeaThemes.Find(brandId);
        }
        public IdeaTheme GetIdeaThemeByActivityId(int activityId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Idea theme by activity id", null, null, activityId);

                var idea = dbContext.SMIdeaThemes.FirstOrDefault(s => s.Discussion.Id == activityId);
                if (idea.IsActive && idea.Discussion != null && idea.Discussion.ExpiryDate < DateTime.UtcNow)
                {
                    idea.IsActive = false;
                    if (dbContext.Entry(idea).State == EntityState.Detached)
                        dbContext.SMIdeaThemes.Attach(idea);
                    dbContext.Entry(idea).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                return idea;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activityId);
                return null;
            }
        }
        public List<MediaFolder> GetMediaFoldersByQbicleId(int currentDomainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get media folder by qbicle id", null, null, currentDomainId);

                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(currentDomainId);
                if (setting != null && setting.SourceQbicle != null)
                    return dbContext.MediaFolders.Where(x => x.Qbicle.Id == setting.SourceQbicle.Id).ToList();
                else
                    return new List<MediaFolder>();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentDomainId);
                return new List<MediaFolder>();
            }
        }
        public List<IdeaThemeType> GetIdeaThemeTypes()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get idea theme types", null, null);

                return dbContext.SMIdeaThemeTypes.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<IdeaThemeType>();
            }
        }
        public List<IdeaTheme> GetIdeaThemesByDomainId(int domainId, string keyword, int isActive)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get idea theme by domain id", null, null, domainId, keyword, isActive);

                var query = dbContext.SMIdeaThemes.Where(s => s.Domain.Id == domainId);
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = dbContext.SMIdeaThemes.Where(s => s.Domain.Id == domainId && (s.Name.Contains(keyword) || s.Explanation.Contains(keyword)));
                }
                if (isActive > 0)
                {
                    query = query.Where(s => s.IsActive == (isActive == 1 ? false : true));
                }
                return query.OrderByDescending(s => s.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, keyword, isActive);
                return new List<IdeaTheme>();
            }
        }

        public List<IdeaTheme> GetIdeasByDomainId(int domainId, string keyword, int isActive, int skip, int take, bool isLoadingHide, ref int totalRecord)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get ideas by domain id", null, null, domainId, keyword, isActive, skip, take, isLoadingHide, totalRecord);

                var query = dbContext.SMIdeaThemes.Where(s => s.Domain.Id == domainId);
                if (!isLoadingHide)
                {
                    query = query.Where(s => !s.IsHidden);
                }
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Name.Contains(keyword) || s.Explanation.Contains(keyword));
                }
                if (isActive > 0)
                {
                    query = query.Where(s => s.IsActive == (isActive == 1 ? false : true));
                }
                if (take == 0)
                {
                    totalRecord = query.Count();
                    return new List<IdeaTheme>();
                }
                else
                {
                    totalRecord = 0;
                    return query.OrderByDescending(s => s.CreatedDate).Skip(skip).Take(take).ToList();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, keyword, isActive, skip, take, isLoadingHide, totalRecord);
                return new List<IdeaTheme>();
            }
        }

        public ReturnJsonModel ShowOrHideIdea(int id)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show or hide idea", null, null, id);

                var idea = dbContext.SMIdeaThemes.FirstOrDefault(s => s.Id == id);
                idea.IsHidden = !idea.IsHidden;
                if (dbContext.Entry(idea).State == EntityState.Detached)
                    dbContext.SMIdeaThemes.Attach(idea);
                dbContext.Entry(idea).State = EntityState.Modified;
                returnModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnModel;
        }
        public ReturnJsonModel SaveIdea(IdeaThemeCustomeModel model, MediaModel media, Settings setting, string userId)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save idea", userId, null, model, media, setting);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);

                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }
                var qbicle = dbContext.Qbicles.Find(setting.SourceQbicle.Id);
                if (qbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_188");
                    return returnModel;
                }
                var user = dbContext.QbicleUser.Find(userId);
                var medfolder = AddMediaFolder(model.ResourcesFolder, model.FolderName, qbicle, user);
                if (medfolder == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_189");
                    return returnModel;
                }
                var topic = dbContext.Topics.Find(setting.DefaultTopic.Id);
                if (topic == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_190");
                    return returnModel;
                }
                var mediaModel = AddMediaQbicle(media, user, qbicle, medfolder, model.Name, model.Explanation, topic);
                if (mediaModel != null)
                {
                    qbicle.Media.Add(mediaModel);
                    dbContext.Entry(mediaModel).State = EntityState.Added;
                }
                var dbIdea = dbContext.SMIdeaThemes.Find(model.Id);
                if (dbIdea != null)
                {
                    dbIdea.ResourceFolder = medfolder;
                    if (!string.IsNullOrEmpty(media.UrlGuid))
                    {
                        dbIdea.FeaturedImageUri = media.UrlGuid;
                    }
                    dbIdea.Name = model.Name;
                    dbIdea.Explanation = model.Explanation;
                    dbIdea.LastUpdatedBy = user;
                    dbIdea.LastUpdateDate = DateTime.UtcNow;
                    dbIdea.IsActive = model.Active.HasValue && model.Active.Value ? true : false;
                    var dbideatype = dbContext.SMIdeaThemeTypes.Find(model.Type);
                    if (dbideatype != null)
                        dbIdea.Type = dbideatype;
                    dbIdea.Links.Clear();
                    if (model.Links != null && model.Links.Count > 0)
                    {
                        IdeaThemeLink link = null;
                        foreach (var item in model.Links)
                        {
                            link = new IdeaThemeLink();
                            link.CreatedBy = user;
                            link.CreatedDate = dbIdea.LastUpdateDate;
                            link.URL = item;
                            dbIdea.Links.Add(link);
                            dbContext.SMIdeaThemeLinks.Add(link);
                            dbContext.Entry(link).State = EntityState.Added;
                        }
                    }
                    if (dbContext.Entry(dbIdea).State == EntityState.Detached)
                        dbContext.SMIdeaThemes.Attach(dbIdea);
                    dbContext.Entry(dbIdea).State = EntityState.Modified;
                }
                else
                {
                    dbIdea = new IdeaTheme();
                    dbIdea.CreatedDate = DateTime.UtcNow;
                    dbIdea.LastUpdatedBy = user;
                    dbIdea.LastUpdateDate = dbIdea.CreatedDate;
                    dbIdea.CreatedBy = user;
                    dbIdea.Name = model.Name;
                    dbIdea.Explanation = model.Explanation;
                    dbIdea.Domain = model.CurrentDomain;
                    dbIdea.FeaturedImageUri = media.UrlGuid;
                    dbIdea.ResourceFolder = medfolder;
                    dbIdea.IsActive = true;
                    var dbideatype = dbContext.SMIdeaThemeTypes.Find(model.Type);
                    if (dbideatype != null)
                        dbIdea.Type = dbideatype;
                    if (model.Links != null && model.Links.Count > 0)
                    {
                        IdeaThemeLink link = null;
                        foreach (var item in model.Links)
                        {
                            link = new IdeaThemeLink();
                            link.CreatedBy = user;
                            link.CreatedDate = dbIdea.LastUpdateDate;
                            link.URL = item;
                            dbIdea.Links.Add(link);
                            dbContext.SMIdeaThemeLinks.Add(link);
                            dbContext.Entry(link).State = EntityState.Added;
                        }
                    }
                    dbContext.SMIdeaThemes.Add(dbIdea);
                    dbContext.Entry(dbIdea).State = EntityState.Added;
                }
                returnModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model, media, setting);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }
        public string AutoGenerateFolderName(int CurrentDomainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Auto generate folder name", null, null, CurrentDomainId);

                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId);
                var qbicle = dbContext.Qbicles.Find(setting.SourceQbicle.Id);
                if (qbicle != null)
                {
                    var random = new Random();
                    var randomNumber = random.Next(1, 999);
                    var sFolderName = "SM-IDEA-" + randomNumber.ToString("000");
                    for (int i = 0; i < 20; i++)
                    {
                        var isExist = dbContext.MediaFolders.Any(m => m.Qbicle.Id == qbicle.Id && m.Name == sFolderName);
                        if (!isExist)
                        {
                            return sFolderName;
                        }
                        else
                        {
                            sFolderName = "SM-IDEA-" + random.Next(1, 999).ToString("000");
                            continue;
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, CurrentDomainId);
                return "";
            }
        }
        private MediaFolder AddMediaFolder(int resourcesfolder, string newfoldername, Qbicle qbicle, ApplicationUser user)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add media folder", user.Id, null, resourcesfolder, newfoldername, qbicle, user);
                
                if (!string.IsNullOrEmpty(newfoldername))
                {
                    var media = dbContext.MediaFolders.FirstOrDefault(s => s.Name == newfoldername && s.Qbicle.Id == qbicle.Id);
                    if (media == null)
                    {
                        media = new MediaFolder();
                        media.Name = newfoldername;
                        media.Qbicle = qbicle;
                        media.CreatedDate = DateTime.Now;
                        media.CreatedBy = user;
                        dbContext.MediaFolders.Add(media);
                        dbContext.Entry(media).State = EntityState.Added;
                    }
                    return media;
                }
                else
                {
                    return dbContext.MediaFolders.Find(resourcesfolder);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, resourcesfolder, newfoldername, qbicle, user);
                return null;
            }
        }
        private QbicleMedia AddMediaQbicle(MediaModel media, ApplicationUser user, Qbicle qbicle, MediaFolder folder, string name, string descript, Topic topic)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add media qbicle", user.Id, null, media, user, qbicle, folder, name, descript, topic);
                
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
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, media, user, qbicle, folder, name, descript, topic);
                return null;
            }
        }

        public List<ThemeCampaignModel> GetListCampaignsInTheme(int themeId, int[] types, string search, int start, int length, ref int totalRecord, string dateFormat)
        {
            List<ThemeCampaignModel> lstModel = new List<ThemeCampaignModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get list campaigns in theme", null, null, themeId, types, search, start, length, totalRecord, dateFormat);
                
                if (types != null && !types.Contains(0))
                {
                    if (types.Contains(1))
                    {
                        lstModel.AddRange(dbContext.SocialCampaignQueues.Where(s => s.Post.AssociatedCampaign.IdeaTheme != null && s.Post.AssociatedCampaign.IdeaTheme.Id == themeId && (search.Equals("") || s.Post.Title.ToLower().Contains(search.Trim().ToLower()) || s.Post.AssociatedCampaign.Name.ToLower().Contains(search.Trim().ToLower())) && s.Post.AssociatedCampaign.CampaignType == CampaignType.Automated).
                            Select(c => new ThemeCampaignModel
                            {
                                PostId = c.Post.Id,
                                PostTitle = c.Post.Title,
                                CampaignName = c.Post.AssociatedCampaign.Name,
                                DateOfIssue = c.PostingDate,
                                Status = c.Status == CampaignPostQueueStatus.Scheduled ? "Queued" : "Complete",
                                Type = "Automated Social"
                            }));
                    };

                    if (types.Contains(2))
                    {
                        lstModel.AddRange(dbContext.SocialCampaignQueues.Where(s => s.Post.AssociatedCampaign.IdeaTheme != null && s.Post.AssociatedCampaign.IdeaTheme.Id == themeId && (search.Equals("") || s.Post.Title.ToLower().Contains(search.Trim().ToLower()) || s.Post.AssociatedCampaign.Name.ToLower().Contains(search.Trim().ToLower())) && s.Post.AssociatedCampaign.CampaignType == CampaignType.Manual).
                            Select(c => new ThemeCampaignModel
                            {
                                PostId = c.Post.Id,
                                PostTitle = c.Post.Title,
                                CampaignName = c.Post.AssociatedCampaign.Name,
                                DateOfIssue = c.PostingDate,
                                Status = c.Status == CampaignPostQueueStatus.Scheduled ? "Queued" : "Complete",
                                Type = "Manual Social"
                            }));
                    };

                    if (types.Contains(3))
                    {
                        lstModel.AddRange(dbContext.EmailCampaignQueues.Where(e => e.Email.Campaign.IdeaTheme != null && e.Email.Campaign.IdeaTheme.Id == themeId && (search.Equals("") || e.Email.Title.ToLower().Contains(search.Trim().ToLower()) || e.Email.Campaign.Name.ToLower().Contains(search.Trim().ToLower()))).
                            Select(c => new ThemeCampaignModel
                            {
                                PostId = c.Email.Id,
                                PostTitle = c.Email.Title,
                                CampaignName = c.Email.Campaign.Name,
                                DateOfIssue = c.PostingDate,
                                Status = c.Status == CampaignEmailQueueStatus.Scheduled ? "Queued" : "Complete",
                                Type = "Email"
                            }));
                    };
                };

                totalRecord = lstModel.Count();
                lstModel = lstModel.OrderBy(l => l.PostId).Skip(start).Take(length).ToList();
                foreach (var model in lstModel)
                {
                    model.StrDateOfIssue = model.DateOfIssue.ToString(dateFormat + ", hh:mmtt");
                }
                return lstModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, themeId, types, search, start, length, totalRecord, dateFormat);
                totalRecord = 0;
                return lstModel;
            }
        }
    }
}
