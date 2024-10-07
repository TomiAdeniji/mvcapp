using Facebook;
using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using static Qbicles.Models.ApprovalReq;
using static Qbicles.Models.EmailLog;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class CampaignRules
    {
        private ApplicationDbContext dbContext;

        public CampaignRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public List<SocialCampaignModel> GetSocialCampaignByKeywordAndTargetNetwork(int domainId, string search, int[] targetNetworks, CampaignType campaigntype, int skip, int take)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get social campaign by keyword and target network", null, null, domainId, search, targetNetworks, campaigntype, skip, take);

                IQueryable<SocialCampaign> query = dbContext.SocialCampaigns;
                if (string.IsNullOrEmpty(search))
                {
                    query = query.Where(s => s.Domain.Id == domainId);
                }
                else
                {
                    query = query.Where(s => s.Domain.Id == domainId
                    && (s.Name.Contains(search) || s.Details.Contains(search)));
                }
                query = query.Where(s => s.Domain.Id == domainId && (campaigntype == CampaignType.Both || s.CampaignType == campaigntype) && (targetNetworks.Any(tg => tg == 0) || s.TargetNetworks.Any(tg => targetNetworks.Contains(tg.Id))));
                var lstSocialCampaign = query.OrderByDescending(s => s.CreatedDate).Skip(skip).Take(take).ToList();
                List<SocialCampaignModel> lstSocialCampaignModel = new List<SocialCampaignModel>();
                foreach (var campaign in lstSocialCampaign)
                {
                    SocialCampaignModel model = new SocialCampaignModel();
                    model.Campaign = campaign;
                    model.IsHalted = dbContext.SocialCampaignQueues.Where(s => s.Post.AssociatedCampaign.Id == campaign.Id && s.Status == CampaignPostQueueStatus.Scheduled).Any();
                    lstSocialCampaignModel.Add(model);
                }
                return lstSocialCampaignModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, search, targetNetworks, campaigntype, skip, take);
                return new List<SocialCampaignModel>();
            }
        }

        public int CountSocialCampaignByKeywordAndTargetNetwork(int domainId, string search, int[] targetNetworks, CampaignType campaigntype)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count social campaign by keyword and target network", null, null, domainId, search, targetNetworks, campaigntype);

                IQueryable<SocialCampaign> query = dbContext.SocialCampaigns;
                if (string.IsNullOrEmpty(search))
                {
                    query = query.Where(s => s.Domain.Id == domainId);
                }
                else
                {
                    query = query.Where(s => s.Domain.Id == domainId
                    && (s.Name.Contains(search) || s.Details.Contains(search)));
                }
                query = query.Where(s => s.Domain.Id == domainId && (campaigntype == CampaignType.Both || s.CampaignType == campaigntype) && (targetNetworks.Any(tg => tg == 0) || s.TargetNetworks.Any(tg => targetNetworks.Contains(tg.Id))));
                return query.Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, search, targetNetworks, campaigntype);
                return 0;
            }
        }

        public List<HaltedEmailCampaignModel> GetEmailCampaignByKeywordAndTargetNetwork(int domainId, string search, int[] targetsegments, int skip, int take)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get email campaign by keyword and target network", null, null, domainId, search, targetsegments, skip, take);

                IQueryable<EmailCampaign> query = dbContext.EmailCampaigns;
                if (string.IsNullOrEmpty(search))
                {
                    query = query.Where(s => s.Domain.Id == domainId);
                }
                else
                {
                    query = query.Where(s => s.Domain.Id == domainId
                    && (s.Name.Contains(search) || s.Summary.Contains(search)));
                }
                query = query.Where(t => t.Domain.Id == domainId && (targetsegments.Any(ts => ts == 0) || t.Segments.Any(s => targetsegments.Contains(s.Id))));
                var lstSocialCampaign = query.OrderByDescending(s => s.CreatedDate).Skip(skip).Take(take).ToList();
                List<HaltedEmailCampaignModel> lstEmailCampaignModel = new List<HaltedEmailCampaignModel>();
                foreach (var campaign in lstSocialCampaign)
                {
                    HaltedEmailCampaignModel model = new HaltedEmailCampaignModel();
                    model.Campaign = campaign;
                    model.IsHalted = dbContext.EmailCampaignQueues.Where(s => s.Email.Campaign.Id == campaign.Id && s.Status == CampaignEmailQueueStatus.Scheduled).Any();
                    lstEmailCampaignModel.Add(model);
                }
                return lstEmailCampaignModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, search, targetsegments, skip, take);
                return new List<HaltedEmailCampaignModel>();
            }
        }

        public int CountEmailCampaignByKeywordAndTargetNetwork(int domainId, string search, int[] targetsegments)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count email campaign by keyword and target network", null, null, domainId, search, targetsegments);

                IQueryable<EmailCampaign> query = dbContext.EmailCampaigns;
                if (string.IsNullOrEmpty(search))
                {
                    query = query.Where(s => s.Domain.Id == domainId);
                }
                else
                {
                    query = query.Where(s => s.Domain.Id == domainId
                    && (s.Name.Contains(search) || s.Summary.Contains(search)));
                }
                query = query.Where(t => t.Domain.Id == domainId && (targetsegments.Any(ts => ts == 0) || t.Segments.Any(s => targetsegments.Contains(s.Id))));
                return query.Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, search, targetsegments);
                return 0;
            }
        }

        public ReturnJsonModel SaveSocialCampaign(SocialCampaign campaign, int workgroup, int resourcesfolder, string newfoldername, int qbicleFolderId, MediaModel media, int topicId, int[] networktype,
            int? brandId, int[] attributes, int[] brandproducts, int[] valueprops, int ideaId, string userId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save social campaign", userId, null, campaign, workgroup, resourcesfolder,
                        newfoldername, qbicleFolderId, media, topicId, networktype, brandId, attributes, brandproducts, valueprops, ideaId);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }
                var user = dbContext.QbicleUser.Find(userId);
                var wg = dbContext.SalesMarketingWorkGroups.Find(workgroup);
                if (wg != null)
                {
                    var qbicle = dbContext.Qbicles.Find(qbicleFolderId);
                    if (qbicle == null)
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_188");
                        return refModel;
                    }
                    var medfolder = AddMediaFolder(resourcesfolder, newfoldername, qbicle, user);
                    if (medfolder == null)
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_189");
                        return refModel;
                    }
                    var topic = dbContext.Topics.Find(topicId);
                    if (topic == null)
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_190");
                        return refModel;
                    }
                    var mediaModel = AddMediaQbicle(media, user, qbicle, medfolder, campaign.Name, campaign.Details, topic);
                    if (mediaModel != null)
                    {
                        qbicle.Media.Add(mediaModel);
                        dbContext.Entry(mediaModel).State = EntityState.Added;
                    }
                    var cpmodel = dbContext.SocialCampaigns.Find(campaign.Id);
                    if (cpmodel != null)
                    {
                        cpmodel.Name = campaign.Name;
                        cpmodel.Details = campaign.Details;
                        cpmodel.ResourceFolder = medfolder;
                        if (!string.IsNullOrEmpty(media.UrlGuid))
                        {
                            cpmodel.FeaturedImageUri = media.UrlGuid;
                        }
                        cpmodel.WorkGroup = wg;
                        if (cpmodel.TargetNetworks.Count() > 0)
                        {
                            cpmodel.TargetNetworks.Clear();
                        }
                        foreach (var item in networktype)
                        {
                            var type = dbContext.NetworkTypes.Find(item);
                            if (type != null)
                            {
                                cpmodel.TargetNetworks.Add(type);
                            }
                        }

                        #region Brand

                        var dbbrand = dbContext.SMBrands.Find(brandId.HasValue ? brandId.Value : 0);
                        if (dbbrand != null)
                        {
                            cpmodel.Brand = dbbrand;
                            //Brand Products
                            cpmodel.BrandProducts.Clear();
                            if (brandproducts != null && brandproducts.Count() > 0)
                                foreach (var item in brandproducts)
                                {
                                    var product = dbContext.SmBrandProducts.Find(item);
                                    if (product != null)
                                    {
                                        cpmodel.BrandProducts.Add(product);
                                    }
                                }
                            //Attributes
                            cpmodel.Attributes.Clear();
                            if (attributes != null && attributes.Count() > 0)
                                foreach (var item in attributes)
                                {
                                    var attribute = dbContext.SMAttributes.Find(item);
                                    if (attribute != null)
                                    {
                                        cpmodel.Attributes.Add(attribute);
                                    }
                                }
                            //Value Propositions
                            cpmodel.ValuePropositons.Clear();
                            if (valueprops != null && valueprops.Count() > 0)
                                foreach (var item in valueprops)
                                {
                                    var value = dbContext.SMValuePropositions.Find(item);
                                    if (value != null)
                                    {
                                        cpmodel.ValuePropositons.Add(value);
                                    }
                                }
                        }
                        else
                        {
                            cpmodel.Brand = null;
                            cpmodel.BrandProducts.Clear();
                            cpmodel.Attributes.Clear();
                            cpmodel.ValuePropositons.Clear();
                        }

                        #endregion Brand

                        #region Idea Theme

                        var idea = dbContext.SMIdeaThemes.Find(ideaId);
                        if (idea != null)
                        {
                            cpmodel.IdeaTheme = idea;
                        }

                        #endregion Idea Theme

                        if (dbContext.Entry(cpmodel).State == EntityState.Detached)
                            dbContext.SocialCampaigns.Attach(cpmodel);
                        dbContext.Entry(cpmodel).State = EntityState.Modified;
                    }
                    else
                    {
                        campaign.CreatedBy = user;
                        campaign.CreatedDate = DateTime.UtcNow;
                        campaign.ResourceFolder = medfolder;
                        campaign.WorkGroup = wg;
                        if (!string.IsNullOrEmpty(media.UrlGuid))
                            campaign.FeaturedImageUri = media.UrlGuid;
                        foreach (var item in networktype)
                        {
                            var type = dbContext.NetworkTypes.Find(item);
                            if (type != null)
                            {
                                campaign.TargetNetworks.Add(type);
                            }
                        }

                        #region Brand

                        var dbbrand = dbContext.SMBrands.Find(brandId.HasValue ? brandId.Value : 0);
                        if (dbbrand != null)
                        {
                            campaign.Brand = dbbrand;
                            //Brand Products
                            if (brandproducts != null && brandproducts.Count() > 0)
                                foreach (var item in brandproducts)
                                {
                                    var product = dbContext.SmBrandProducts.Find(item);
                                    if (product != null)
                                    {
                                        campaign.BrandProducts.Add(product);
                                    }
                                }
                            //Attributes
                            if (attributes != null && attributes.Count() > 0)
                                foreach (var item in attributes)
                                {
                                    var attribute = dbContext.SMAttributes.Find(item);
                                    if (attribute != null)
                                    {
                                        campaign.Attributes.Add(attribute);
                                    }
                                }
                            //Value Propositions
                            if (valueprops != null && valueprops.Count() > 0)
                                foreach (var item in valueprops)
                                {
                                    var value = dbContext.SMValuePropositions.Find(item);
                                    if (value != null)
                                    {
                                        campaign.ValuePropositons.Add(value);
                                    }
                                }
                        }
                        else
                        {
                            campaign.Brand = null;
                        }

                        #endregion Brand

                        #region Idea Theme

                        var idea = dbContext.SMIdeaThemes.Find(ideaId);
                        if (idea != null)
                        {
                            campaign.IdeaTheme = idea;
                        }

                        #endregion Idea Theme

                        dbContext.SocialCampaigns.Add(campaign);
                        dbContext.Entry(campaign).State = EntityState.Added;
                    }

                    var result = dbContext.SaveChanges();
                    refModel.result = result > 0 ? true : false;
                    return refModel;
                }
                refModel.msg = ResourcesManager._L("ERROR_MSG_191");
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, campaign, workgroup, resourcesfolder, newfoldername, qbicleFolderId, media, topicId, networktype, brandId, attributes, brandproducts, valueprops, ideaId);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public ReturnJsonModel SaveEmailCampaign(EmailCampaign campaign, int workgroup, int resourcesfolder, string newfoldername, int qbicleFolderId, MediaModel media, int topicId, int[] lstSegments,
            int? brandId, int[] attributes, int[] brandproducts, int[] valueprops, int ideaId, string userId
            )
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Save email campaign", userId, null, campaign, workgroup, resourcesfolder, newfoldername, qbicleFolderId, media, topicId, lstSegments,
                                 brandId, attributes, brandproducts, valueprops, ideaId);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }

                var wg = dbContext.SalesMarketingWorkGroups.Find(workgroup);
                if (wg != null)
                {
                    var qbicle = dbContext.Qbicles.Find(qbicleFolderId);
                    if (qbicle == null)
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_188");
                        return refModel;
                    }
                    var user = dbContext.QbicleUser.Find(userId);
                    var medfolder = AddMediaFolder(resourcesfolder, newfoldername, qbicle, user);
                    if (medfolder == null)
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_189");
                        return refModel;
                    }
                    var topic = dbContext.Topics.Find(topicId);
                    if (topic == null)
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_190");
                        return refModel;
                    }
                    var mediaModel = AddMediaQbicle(media, user, qbicle, medfolder, campaign.Name, campaign.Summary, topic);
                    if (mediaModel != null)
                    {
                        qbicle.Media.Add(mediaModel);
                        dbContext.Entry(mediaModel).State = EntityState.Added;
                    }
                    var cpmodel = dbContext.EmailCampaigns.Find(campaign.Id);
                    if (cpmodel != null)
                    {
                        cpmodel.Name = campaign.Name;
                        cpmodel.Summary = campaign.Summary;
                        cpmodel.ResourceFolder = medfolder;
                        cpmodel.DefaultFromName = campaign.DefaultFromName;
                        cpmodel.DefaultFromEmail = campaign.DefaultFromEmail;
                        cpmodel.DefaultReplyEmail = campaign.DefaultReplyEmail;
                        if (!string.IsNullOrEmpty(media.UrlGuid))
                        {
                            cpmodel.FeaturedImageUri = media.UrlGuid;
                        }
                        cpmodel.WorkGroup = wg;
                        if (cpmodel.Segments.Count() > 0)
                        {
                            cpmodel.Segments.Clear();
                        }
                        foreach (var item in lstSegments)
                        {
                            var type = dbContext.SMSegments.Find(item);
                            if (type != null)
                            {
                                cpmodel.Segments.Add(type);
                            }
                        }

                        #region Brand

                        var dbbrand = dbContext.SMBrands.Find(brandId.HasValue ? brandId.Value : 0);
                        if (dbbrand != null)
                        {
                            cpmodel.Brand = dbbrand;
                            //Brand Products
                            cpmodel.BrandProducts.Clear();
                            if (brandproducts != null && brandproducts.Count() > 0)
                                foreach (var item in brandproducts)
                                {
                                    var product = dbContext.SmBrandProducts.Find(item);
                                    if (product != null)
                                    {
                                        cpmodel.BrandProducts.Add(product);
                                    }
                                }
                            //Attributes
                            cpmodel.Attributes.Clear();
                            if (attributes != null && attributes.Count() > 0)
                                foreach (var item in attributes)
                                {
                                    var attribute = dbContext.SMAttributes.Find(item);
                                    if (attribute != null)
                                    {
                                        cpmodel.Attributes.Add(attribute);
                                    }
                                }
                            //Value Propositions
                            cpmodel.ValuePropositons.Clear();
                            if (valueprops != null && valueprops.Count() > 0)
                                foreach (var item in valueprops)
                                {
                                    var value = dbContext.SMValuePropositions.Find(item);
                                    if (value != null)
                                    {
                                        cpmodel.ValuePropositons.Add(value);
                                    }
                                }
                        }
                        else
                        {
                            cpmodel.Brand = null;
                            cpmodel.BrandProducts.Clear();
                            cpmodel.Attributes.Clear();
                            cpmodel.ValuePropositons.Clear();
                        }

                        #endregion Brand

                        #region Idea Theme

                        var idea = dbContext.SMIdeaThemes.Find(ideaId);
                        if (idea != null)
                        {
                            cpmodel.IdeaTheme = idea;
                        }

                        #endregion Idea Theme

                        if (dbContext.Entry(cpmodel).State == EntityState.Detached)
                            dbContext.EmailCampaigns.Attach(cpmodel);
                        dbContext.Entry(cpmodel).State = EntityState.Modified;
                    }
                    else
                    {
                        campaign.CreatedDate = DateTime.UtcNow;
                        campaign.CreatedBy = user;
                        campaign.ResourceFolder = medfolder;
                        campaign.WorkGroup = wg;
                        if (!string.IsNullOrEmpty(media.UrlGuid))
                            campaign.FeaturedImageUri = media.UrlGuid;
                        foreach (var item in lstSegments)
                        {
                            var type = dbContext.SMSegments.Find(item);
                            if (type != null)
                            {
                                campaign.Segments.Add(type);
                            }
                        }

                        #region Brand

                        var dbbrand = dbContext.SMBrands.Find(brandId.HasValue ? brandId.Value : 0);
                        if (dbbrand != null)
                        {
                            campaign.Brand = dbbrand;
                            //Brand Products
                            if (brandproducts != null && brandproducts.Count() > 0)
                                foreach (var item in brandproducts)
                                {
                                    var product = dbContext.SmBrandProducts.Find(item);
                                    if (product != null)
                                    {
                                        campaign.BrandProducts.Add(product);
                                    }
                                }
                            //Attributes
                            if (attributes != null && attributes.Count() > 0)
                                foreach (var item in attributes)
                                {
                                    var attribute = dbContext.SMAttributes.Find(item);
                                    if (attribute != null)
                                    {
                                        campaign.Attributes.Add(attribute);
                                    }
                                }
                            //Value Propositions
                            if (valueprops != null && valueprops.Count() > 0)
                                foreach (var item in valueprops)
                                {
                                    var value = dbContext.SMValuePropositions.Find(item);
                                    if (value != null)
                                    {
                                        campaign.ValuePropositons.Add(value);
                                    }
                                }
                        }
                        else
                        {
                            campaign.Brand = null;
                        }

                        #endregion Brand

                        #region Idea Theme

                        var idea = dbContext.SMIdeaThemes.Find(ideaId);
                        if (idea != null)
                        {
                            campaign.IdeaTheme = idea;
                        }

                        #endregion Idea Theme

                        dbContext.EmailCampaigns.Add(campaign);
                        dbContext.Entry(campaign).State = EntityState.Added;
                    }

                    var result = dbContext.SaveChanges();
                    refModel.result = result > 0 ? true : false;
                    return refModel;
                }
                refModel.msg = ResourcesManager._L("ERROR_MSG_191");
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, campaign, workgroup, resourcesfolder, newfoldername, qbicleFolderId, media, topicId, lstSegments,
                                 brandId, attributes, brandproducts, valueprops, ideaId);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        private QbicleMedia AddMediaQbicle(MediaModel media, ApplicationUser user, Qbicle qbicle, MediaFolder folder, string name, string description, Topic topic)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add media of qbicle", user.Id, null, qbicle, folder, name, description, topic);

                if (string.IsNullOrEmpty(media.Name)) return null;
                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }
                //dbContext.Entry(media.Type).State = System.Data.Entity.EntityState.Modified;
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
                    Description = description,
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
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, qbicle, folder, name, description, topic);
                return null;
            }
        }

        private MediaFolder AddMediaFolder(int resourceFolder, string newFolderName, Qbicle qbicle, ApplicationUser user)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add media folder", user.Id, null, resourceFolder, newFolderName, qbicle, user);

                if (!string.IsNullOrEmpty(newFolderName))
                {
                    var media = dbContext.MediaFolders.FirstOrDefault(s => s.Name == newFolderName && s.Qbicle.Id == qbicle.Id);
                    if (media != null) return media;
                    media = new MediaFolder
                    {
                        Name = newFolderName,
                        Qbicle = qbicle,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = user
                    };
                    dbContext.MediaFolders.Add(media);
                    dbContext.Entry(media).State = EntityState.Added;
                    return media;
                }

                return dbContext.MediaFolders.Find(resourceFolder);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, resourceFolder, newFolderName, qbicle, user);
                return null;
            }
        }

        /// <summary>
        /// Automatically generated folder name social makerting
        /// App (SM) - Campaign Type (SOC) - Random number (001)
        /// </summary>
        /// <param name="qbicleId"></param>
        /// <returns></returns>
        public string AutoGenerateFolderName(int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Auto generate folder name", null, null, qbicleId);

                var random = new Random();
                var randomNumber = random.Next(1, 999);
                var sFolderName = "SM-SOC-" + randomNumber.ToString("000");
                for (var i = 0; i < 20; i++)
                {
                    var isExist = dbContext.MediaFolders.Any(m => m.Qbicle.Id == qbicleId && m.Name == sFolderName);
                    if (!isExist)
                    {
                        return sFolderName;
                    }
                    else
                    {
                        sFolderName = "SM-SOC-" + random.Next(1, 999).ToString("000");
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId);
                return null;
            }
        }

        public SocialCampaign GetSocialCampaignById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get social campaign by id", null, null, id);

                return dbContext.SocialCampaigns.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public EmailCampaign GetEmailCampaignById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get email campaign by id", null, null, id);

                return dbContext.EmailCampaigns.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public CampaignEmail GetCampaignEmailById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get campaign email by id", null, null, id);

                return dbContext.CampaignEmails.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public EmailPostApproval GetApprovalEmailById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approval email by id", null, null, id);

                return dbContext.EmailPostApproval.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public ReturnJsonModel SaveSocialCampaingnResource(MediaModel media, string userId, int qbicleId, int mediaFolderId, string name, string description, int topicId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save resource", userId, null, media, qbicleId, mediaFolderId, name, description, topicId);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }
                var topic = new TopicRules(dbContext).GetTopicById(topicId);
                if (topic == null)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_190");
                    return refModel;
                }
                var folder = new MediaFolderRules(dbContext).GetMediaFolderById(mediaFolderId, qbicleId);
                if (folder == null)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_808");
                    return refModel;
                }
                var qbicle = dbContext.Qbicles.Find(qbicleId);
                if (qbicle == null)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_188");
                    return refModel;
                }

                var dbMedia = AddMediaQbicle(media, dbContext.QbicleUser.Find(userId), qbicle, folder, name, description, topic);
                if (dbMedia == null) return refModel;
                qbicle.Media.Add(dbMedia);
                if (dbContext.SaveChanges() > 0)
                    refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, media, qbicleId, mediaFolderId, name, description, topicId);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public async Task<bool> SaveSocialPost(SocialCampaignPost campaignPost, MediaModel media, string userId, int domainId,
            int[] sharingAccount, int[] netWorkType, int socialCampaignId, bool? isReminder, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save social post", userId, null, campaignPost, media, domainId, sharingAccount, netWorkType, socialCampaignId, isReminder);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }
                var user = dbContext.QbicleUser.Find(userId);
                campaignPost.CreatedBy = user;
                campaignPost.CreatedDate = DateTime.UtcNow;
                campaignPost.LastUpdateDate = DateTime.UtcNow;
                campaignPost.LastUpdatedBy = user;
                if (isReminder != null && isReminder.Value)
                {
                    campaignPost.Reminder.Domain = new DomainRules(dbContext).GetDomainById(domainId);
                    campaignPost.Reminder.Status = ReminderQueueStatus.Scheduled;
                    campaignPost.Reminder.CreatedBy = user;
                    campaignPost.Reminder.CreatedDate = DateTime.UtcNow;
                    campaignPost.Reminder.SocialCampaignPost = campaignPost;
                }
                else
                {
                    campaignPost.Reminder = null;
                }

                if (socialCampaignId > 0)
                {
                    campaignPost.AssociatedCampaign = dbContext.SocialCampaigns.Find(socialCampaignId);
                }
                foreach (var item in sharingAccount)
                {
                    var ac = dbContext.SocialNetworkAccounts.Find(item);
                    if (ac != null)
                    {
                        campaignPost.SharingAccount.Add(ac);
                    }
                }
                if (netWorkType != null)
                {
                    foreach (var item in netWorkType)
                    {
                        var ac = dbContext.NetworkTypes.Find(item);
                        if (ac != null)
                        {
                            campaignPost.Networks.Add(ac);
                        }
                    }
                }

                var dbCampaign = dbContext.SocialCampaignPosts.Find(campaignPost.Id);
                ApprovalReq approval = null;
                var campaign = campaignPost.AssociatedCampaign;
                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(campaign?.Domain.Id ?? 0);
                if (dbCampaign != null)
                {
                    if (!string.IsNullOrEmpty(media.Name))
                    {
                        if (!string.IsNullOrEmpty(media.Name))
                        {
                            if (campaign != null)
                            {
                                var folder = campaign.ResourceFolder;
                                var md = AddMediaQbicle(media, user, setting.SourceQbicle, folder, campaignPost.Title, campaignPost.Content, setting.DefaultTopic);
                                campaignPost.ImageOrVideo = md;
                            }
                        }
                    }
                    else
                    {
                        if (campaignPost.ImageOrVideo != null)
                            dbCampaign.ImageOrVideo = campaignPost.ImageOrVideo;
                    }
                    dbCampaign.Title = campaignPost.Title;
                    dbCampaign.Content = campaignPost.Content;
                    dbCampaign.LastUpdateDate = campaignPost.LastUpdateDate;
                    dbCampaign.LastUpdatedBy = campaignPost.LastUpdatedBy;
                    dbCampaign.SharingAccount.Clear();
                    dbCampaign.SharingAccount = campaignPost.SharingAccount;
                    dbCampaign.Networks.Clear();
                    dbCampaign.Networks = campaignPost.Networks;
                    if (isReminder.Value)
                    {
                        if (dbCampaign.Reminder == null)
                        {
                            campaignPost.Reminder.SocialCampaignPost = dbCampaign;
                            dbContext.ReminderQueues.Add(campaignPost.Reminder);
                            var reminder = campaignPost.Reminder.ReminderDate - DateTime.UtcNow;
                            var job = new QbicleJobParameter
                            {
                                Id = campaignPost.Id,
                                EndPointName = "schedulecampaignpostreminder",
                                ReminderMinutes = reminder.TotalMinutes
                            };
                            var resultHangFire = await new QbiclesJob().HangFireExcecuteAsync(job);
                            if (resultHangFire != null && resultHangFire.Status == HttpStatusCode.OK)
                            {
                                dbCampaign.Reminder.JobId = resultHangFire.JobId;
                            }
                        }
                        else
                        {
                            if (dbCampaign.Reminder.ReminderDate != campaignPost.Reminder.ReminderDate)
                            {
                                var reminder = campaignPost.Reminder.ReminderDate - DateTime.UtcNow;
                                var job = new QbicleJobParameter
                                {
                                    Id = campaignPost.Id,
                                    EndPointName = "schedulecampaignpostreminder",
                                    ReminderMinutes = reminder.TotalMinutes
                                };
                                var resultHangFire = await new QbiclesJob().HangFireExcecuteAsync(job);
                                if (resultHangFire != null && resultHangFire.Status == HttpStatusCode.OK)
                                {
                                    dbCampaign.Reminder.JobId = resultHangFire.JobId;
                                }
                            }
                            dbCampaign.Reminder.ReminderDate = campaignPost.Reminder.ReminderDate;
                            dbCampaign.Reminder.Content = campaignPost.Reminder.Content;
                        }
                    }
                    else
                    {
                        if (dbCampaign.Reminder != null)
                        {
                            var job = new QbicleJobParameter
                            {
                                Id = dbCampaign.Id,
                                JobId = dbCampaign.Reminder.JobId,
                                EndPointName = "removecampaignpostreminder",
                                ReminderMinutes = 0
                            };
                            var resultJob = await new QbiclesJob().HangFireExcecuteAsync(job);
                            dbContext.ReminderQueues.Remove(dbCampaign.Reminder);
                        }
                    }
                    if (dbContext.Entry(dbCampaign).State == EntityState.Detached)
                        dbContext.SocialCampaignPosts.Attach(dbCampaign);
                    dbContext.Entry(dbCampaign).State = EntityState.Modified;
                }
                else
                {
                    if (!string.IsNullOrEmpty(media.Name))
                    {
                        if (campaign != null)
                        {
                            var folder = campaign.ResourceFolder;
                            var md = AddMediaQbicle(media, user, setting.SourceQbicle, folder, campaignPost.Title, campaignPost.Content, setting.DefaultTopic);
                            campaignPost.ImageOrVideo = md;
                        }
                    }
                    dbContext.SocialCampaignPosts.Add(campaignPost);
                    dbContext.Entry(campaignPost).State = EntityState.Added;
                    var campaignPostApproval = new CampaignPostApproval
                    {
                        ApprovalStatus = SalesMktApprovalStatusEnum.InReview,
                        CampaignPost = campaignPost
                    };

                    if (campaign != null)
                        campaignPostApproval.WorkGroup = campaign.WorkGroup;
                    campaignPostApproval.ApprovedDate = null;
                    var approvalRD = new ApprovalRequestDefinition
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Description = campaignPost.Title,
                        Title = "Social Media Post Approval",
                        ApprovalImage = !string.IsNullOrEmpty(media.UrlGuid) ? media.UrlGuid : "",
                        Group = null,
                        Type = ApprovalRequestDefinition.RequestTypeEnum.General
                    };
                    var wg = campaign.WorkGroup;
                    var userReviewers = wg.Members;
                    var userApprovers = wg.ReviewersApprovers;
                    if (userReviewers != null)
                    {
                        if (!userReviewers.Any(s => s.Id == user.Id))
                            userReviewers.Add(user);
                        approvalRD.Reviewers.AddRange(userReviewers);
                    }
                    else
                    {
                        approvalRD.Reviewers.Add(user);
                    }
                    if (userApprovers != null)
                    {
                        if (!userApprovers.Any(s => s.Id == user.Id))
                            userApprovers.Add(user);
                        approvalRD.Approvers.AddRange(userApprovers);
                    }
                    else
                    {
                        approvalRD.Approvers.Add(user);
                    }

                    approval = new ApprovalReq()
                    {
                        StartedBy = campaignPost.CreatedBy,
                        StartedDate = campaignPost.CreatedDate,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        Topic = setting.DefaultTopic,
                        Notes = campaignPost.Content,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Reviewed,
                        ApprovalRequestDefinition = approvalRD,
                        Qbicle = setting.SourceQbicle,
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequestApp,
                        TimeLineDate = DateTime.UtcNow,
                        Name = campaignPost.Title,
                        App = QbicleActivity.ActivityApp.SalesAndMarketing
                    };
                    campaignPostApproval.Activity = approval;
                    dbContext.SocialCampaignApprovals.Add(campaignPostApproval);
                    dbContext.Entry(campaignPostApproval).State = EntityState.Added;
                }
                var result = dbContext.SaveChanges() > 0;
                if (!result || approval == null) return result;
                if (isReminder.Value && campaignPost.Reminder != null)
                {
                    var reminder = campaignPost.Reminder.ReminderDate - DateTime.UtcNow;
                    var job = new QbicleJobParameter
                    {
                        Id = campaignPost.Id,
                        EndPointName = "schedulecampaignpostreminder",
                        ReminderMinutes = reminder.TotalMinutes
                    };
                    var resultHangFire = await new QbiclesJob().HangFireExcecuteAsync(job);
                    if (resultHangFire != null && resultHangFire.Status == HttpStatusCode.OK)
                    {
                        var dbCampaignReminder = dbContext.SocialCampaignPosts.Find(campaignPost.Id);
                        dbCampaignReminder.Reminder.JobId = resultHangFire.JobId;
                        dbContext.Entry(dbCampaignReminder).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }
                }

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = NotificationEventEnum.ApprovalReviewed,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = user.GetFullName(),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2Activity(activityNotification);

                return true;
            }
            catch (DbEntityValidationException Ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), Ex, userId, campaignPost, media, domainId, sharingAccount, netWorkType, socialCampaignId, isReminder);
                return false;
            }
        }

        public bool SaveEmailPost(CampaignEmail campaignPost,
            MediaModel mediaFeatured, MediaModel mediaPromotional, MediaModel mediaAd,
            int[] campaignsegments, string userId, int emailCampaignId, int templateId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save email post", userId, null, campaignPost, mediaFeatured, mediaPromotional, mediaAd, campaignsegments, emailCampaignId);

                var s3Rules = new Azure.AzureStorageRules(dbContext);
                if (!string.IsNullOrEmpty(mediaFeatured.UrlGuid))
                {
                    s3Rules.ProcessingMediaS3(mediaFeatured.UrlGuid, mediaFeatured.IsPublic);
                }
                if (!string.IsNullOrEmpty(mediaPromotional.UrlGuid))
                {
                    s3Rules.ProcessingMediaS3(mediaPromotional.UrlGuid, mediaPromotional.IsPublic);
                }
                if (!string.IsNullOrEmpty(mediaAd.UrlGuid))
                {
                    s3Rules.ProcessingMediaS3(mediaAd.UrlGuid, mediaAd.IsPublic);
                }
                var user = dbContext.QbicleUser.Find(userId);
                if (campaignPost.Id == 0)
                {
                    campaignPost.CreatedBy = user;
                    campaignPost.CreatedDate = DateTime.UtcNow;
                    campaignPost.LastUpdateDate = DateTime.UtcNow;
                    campaignPost.LastUpdatedBy = user;
                    campaignPost.Template = (templateId == 0 ? null : dbContext.EmailTemplates.Find(templateId));
                    ApprovalReq approval = null;
                    var campaign = dbContext.EmailCampaigns.FirstOrDefault(e => e.Id == emailCampaignId);
                    campaignPost.Campaign = campaign;
                    if (campaignsegments == null)
                    {
                        campaignPost.Segments.AddRange(campaign.Segments);
                    }
                    else
                    {
                        campaignPost.Segments.AddRange(campaign.Segments.Where(s => campaignsegments.Contains(s.Id)));
                    }
                    var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(campaign?.Domain.Id ?? 0);
                    if (!string.IsNullOrEmpty(mediaPromotional.Name))
                    {
                        var folder = campaign.ResourceFolder;
                        var md = AddMediaQbicle(mediaPromotional, campaign.CreatedBy, setting.SourceQbicle, folder, campaignPost.Title, campaignPost.Headline, setting.DefaultTopic);
                        campaignPost.PromotionalImage = md;
                    }

                    if (!string.IsNullOrEmpty(mediaAd.Name))
                    {
                        var folder = campaign.ResourceFolder;
                        var md = AddMediaQbicle(mediaAd, campaign.CreatedBy, setting.SourceQbicle, folder, campaignPost.Title, campaignPost.Headline, setting.DefaultTopic);
                        campaignPost.AdvertisementImage = md;
                    }

                    if (!string.IsNullOrEmpty(mediaFeatured.Name))
                    {
                        campaignPost.FeaturedImageUri = mediaFeatured.UrlGuid;
                    }

                    dbContext.CampaignEmails.Add(campaignPost);
                    dbContext.Entry(campaignPost).State = EntityState.Added;
                    dbContext.SaveChanges();
                    var emailPostApproval = new EmailPostApproval()
                    {
                        ApprovalStatus = SalesMktApprovalStatusEnum.InReview,
                        CampaignEmail = campaignPost
                    };

                    if (campaign != null)
                        emailPostApproval.WorkGroup = campaign.WorkGroup;

                    emailPostApproval.ApprovedDate = null;
                    var approvalRD = new ApprovalRequestDefinition
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        Description = campaignPost.Title,
                        Title = "Email Post Approval",
                        ApprovalImage = !string.IsNullOrEmpty(mediaFeatured.UrlGuid) ? mediaFeatured.UrlGuid : "",
                        Group = null,
                        Type = ApprovalRequestDefinition.RequestTypeEnum.General
                    };
                    var wg = campaign.WorkGroup;
                    var userReviewers = wg.Members;
                    var userApprovers = wg.ReviewersApprovers;
                    if (userReviewers != null)
                    {
                        if (!userReviewers.Any(s => s.Id == user.Id))
                            userReviewers.Add(user);
                        approvalRD.Reviewers.AddRange(userReviewers);
                    }
                    else
                    {
                        approvalRD.Reviewers.Add(user);
                    }
                    if (userApprovers != null)
                    {
                        if (!userApprovers.Any(s => s.Id == user.Id))
                            userApprovers.Add(user);
                        approvalRD.Approvers.AddRange(userApprovers);
                    }
                    else
                    {
                        approvalRD.Approvers.Add(user);
                    }

                    approval = new ApprovalReq()
                    {
                        StartedBy = campaignPost.CreatedBy,
                        StartedDate = campaignPost.CreatedDate,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        Topic = setting.DefaultTopic,
                        Notes = campaignPost.Headline,
                        RequestStatus = ApprovalReq.RequestStatusEnum.Reviewed,
                        ApprovalRequestDefinition = approvalRD,
                        Qbicle = setting.SourceQbicle,
                        ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequestApp,
                        TimeLineDate = DateTime.UtcNow,
                        Name = campaignPost.Title,
                        App = QbicleActivity.ActivityApp.SalesAndMarketing
                    };
                    emailPostApproval.Activity = approval;
                    dbContext.EmailPostApproval.Add(emailPostApproval);
                    dbContext.Entry(emailPostApproval).State = EntityState.Added;
                    var result = dbContext.SaveChanges() > 0;
                    if (!result || approval == null) return result;

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = approval.Id,
                        EventNotify = NotificationEventEnum.ApprovalReviewed,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userId,
                        CreatedByName = user.GetFullName(),
                        ReminderMinutes = 0
                    };
                    new NotificationRules(dbContext).Notification2Activity(activityNotification);
                }
                else
                {
                    var post = dbContext.CampaignEmails.FirstOrDefault(p => p.Id == campaignPost.Id);
                    post.LastUpdateDate = DateTime.UtcNow;
                    post.LastUpdatedBy = user;
                    post.Title = campaignPost.Title;
                    post.EmailSubject = campaignPost.EmailSubject;
                    post.FromEmail = campaignPost.FromEmail;
                    post.FromName = campaignPost.FromName;
                    post.ReplyEmail = campaignPost.ReplyEmail;
                    post.Headline = campaignPost.Headline;
                    post.BodyContent = campaignPost.BodyContent;
                    post.ButtonText = campaignPost.ButtonText;
                    post.ButtonLink = campaignPost.ButtonLink;
                    post.Template = (templateId == 0 ? null : dbContext.EmailTemplates.Find(templateId));
                    post.Segments.Clear();
                    if (campaignsegments == null)
                    {
                        post.Segments.AddRange(post.Campaign.Segments);
                    }
                    else
                    {
                        post.Segments.AddRange(post.Campaign.Segments.Where(s => campaignsegments.Contains(s.Id)));
                    }
                    var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(post.Campaign?.Domain.Id ?? 0);
                    if (!string.IsNullOrEmpty(mediaPromotional.Name))
                    {
                        var folder = post.Campaign.ResourceFolder;
                        var md = AddMediaQbicle(mediaPromotional, post.Campaign.CreatedBy, setting.SourceQbicle, folder, campaignPost.Title, campaignPost.Headline, setting.DefaultTopic);
                        post.PromotionalImage = md;
                    }
                    else
                    {
                        post.PromotionalImage = campaignPost.PromotionalImage;
                    }

                    if (!string.IsNullOrEmpty(mediaAd.Name))
                    {
                        var folder = post.Campaign.ResourceFolder;
                        var md = AddMediaQbicle(mediaAd, post.Campaign.CreatedBy, setting.SourceQbicle, folder, campaignPost.Title, campaignPost.Headline, setting.DefaultTopic);
                        post.AdvertisementImage = md;
                    }
                    else
                    {
                        post.AdvertisementImage = campaignPost.AdvertisementImage;
                    }

                    if (!string.IsNullOrEmpty(mediaFeatured.Name))
                    {
                        post.FeaturedImageUri = mediaFeatured.UrlGuid;
                    }

                    dbContext.SaveChanges();
                }

                return true;
            }
            catch (DbEntityValidationException Ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), Ex, userId, campaignPost,
                    mediaFeatured, mediaPromotional, mediaAd, campaignsegments, emailCampaignId);
                return false;
            }
        }

        public bool DeletePostApproval(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete post approval", null, null, id);

                var cap = dbContext.SocialCampaignApprovals.Find(id);
                if (cap == null) return dbContext.SaveChanges() > 0;
                dbContext.ApprovalReqs.Remove(cap.Activity);
                dbContext.SocialCampaignApprovals.Remove(cap);
                return dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public bool DeleteEmailPostApproval(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete email post approval", null, null, id);

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete post approval", null, null, id);
                var cap = dbContext.EmailPostApproval.Find(id);
                if (cap == null) return dbContext.SaveChanges() > 0;
                dbContext.ApprovalReqs.Remove(cap.Activity);
                dbContext.EmailPostApproval.Remove(cap);
                return dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public async Task<bool> DeletePostQueue(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete post queue", null, null, id);

                var queue = dbContext.SocialCampaignQueues.Find(id);
                if (queue == null) return false;

                var job = new QbicleJobParameter
                {
                    Id = queue.Id,
                    JobId = queue.JobId,
                    EndPointName = "removecampaignpost",
                    ReminderMinutes = 0
                };
                var result = await new QbiclesJob().HangFireExcecuteAsync(job);
                if (result.Status != HttpStatusCode.OK)
                    return false;

                dbContext.SocialCampaignQueues.Remove(queue);
                dbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public async Task<bool> DeleteEmailPostQueue(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete email post queue", null, null, id);

                var queue = dbContext.EmailCampaignQueues.Find(id);
                if (queue == null) return false;

                var job = new QbicleJobParameter
                {
                    Id = queue.Id,
                    JobId = queue.JobId,
                    EndPointName = "removecampaignpost",
                    ReminderMinutes = 0
                };
                var result = await new QbiclesJob().HangFireExcecuteAsync(job);
                if (result.Status != HttpStatusCode.OK)
                    return false;

                dbContext.EmailCampaignQueues.Remove(queue);
                dbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public bool PostSetDenied(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Set post denied", null, null, id);

                var cap = dbContext.SocialCampaignApprovals.Find(id);
                if (cap == null) return dbContext.SaveChanges() > 0;
                var approvalReq = cap.Activity;
                approvalReq.RequestStatus = RequestStatusEnum.Denied;
                if (dbContext.Entry(approvalReq).State == EntityState.Detached)
                    dbContext.ApprovalReqs.Attach(approvalReq);
                dbContext.Entry(approvalReq).State = EntityState.Modified;
                cap.ApprovalStatus = SalesMktApprovalStatusEnum.Denied;
                cap.ApprovedDate = DateTime.UtcNow;
                if (dbContext.Entry(cap).State == EntityState.Detached)
                    dbContext.SocialCampaignApprovals.Attach(cap);
                dbContext.Entry(cap).State = EntityState.Modified;
                return dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public bool DenyEmailPost(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Deny email post", null, null, id);

                var cap = dbContext.EmailPostApproval.Find(id);
                if (cap == null) return dbContext.SaveChanges() > 0;
                var approvalReq = cap.Activity;
                approvalReq.RequestStatus = RequestStatusEnum.Denied;
                if (dbContext.Entry(approvalReq).State == EntityState.Detached)
                    dbContext.ApprovalReqs.Attach(approvalReq);
                dbContext.Entry(approvalReq).State = EntityState.Modified;
                cap.ApprovalStatus = SalesMktApprovalStatusEnum.Denied;
                cap.ApprovedDate = DateTime.UtcNow;
                if (dbContext.Entry(cap).State == EntityState.Detached)
                    dbContext.EmailPostApproval.Attach(cap);
                dbContext.Entry(cap).State = EntityState.Modified;
                return dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public bool PostSetApproved(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Set post approved", null, null, id);

                var cap = dbContext.SocialCampaignApprovals.Find(id);
                if (cap == null) return dbContext.SaveChanges() > 0;
                var approvalReq = cap.Activity;
                approvalReq.RequestStatus = RequestStatusEnum.Approved;
                if (dbContext.Entry(approvalReq).State == EntityState.Detached)
                    dbContext.ApprovalReqs.Attach(approvalReq);
                dbContext.Entry(approvalReq).State = EntityState.Modified;
                cap.ApprovalStatus = SalesMktApprovalStatusEnum.Approved;
                cap.ApprovedDate = DateTime.UtcNow;
                if (dbContext.Entry(cap).State == EntityState.Detached)
                    dbContext.SocialCampaignApprovals.Attach(cap);
                dbContext.Entry(cap).State = EntityState.Modified;
                return dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public bool ApproveEmailPost(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Approve email post", null, null, id);

                var cap = dbContext.EmailPostApproval.Find(id);
                if (cap == null) return dbContext.SaveChanges() > 0;
                var approvalReq = cap.Activity;
                approvalReq.RequestStatus = RequestStatusEnum.Approved;
                if (dbContext.Entry(approvalReq).State == EntityState.Detached)
                    dbContext.ApprovalReqs.Attach(approvalReq);
                dbContext.Entry(approvalReq).State = EntityState.Modified;
                cap.ApprovalStatus = SalesMktApprovalStatusEnum.Approved;
                cap.ApprovedDate = DateTime.UtcNow;
                if (dbContext.Entry(cap).State == EntityState.Detached)
                    dbContext.EmailPostApproval.Attach(cap);
                dbContext.Entry(cap).State = EntityState.Modified;
                return dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public async Task<ReturnJsonModel> AddQueueSchedule(int aid, string sPostingDate, bool isNotifyWhenSent, string currentTimeZone, string dateTimeFormat)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add queue schedule", null, null, aid, sPostingDate, isNotifyWhenSent, currentTimeZone, dateTimeFormat);

                var datePost = DateTime.UtcNow;
                var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                if (!string.IsNullOrEmpty(sPostingDate))
                {
                    datePost = TimeZoneInfo.ConvertTimeToUtc(sPostingDate.ConvertDateFormat(dateTimeFormat), tz);
                }
                var cap = dbContext.SocialCampaignApprovals.Find(aid);
                if (cap == null)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_192");
                    return refModel;
                }

                cap.ApprovalStatus = SalesMktApprovalStatusEnum.Queued;
                if (dbContext.Entry((object)cap).State == EntityState.Detached)
                    dbContext.SocialCampaignApprovals.Attach(cap);
                dbContext.Entry((object)cap).State = EntityState.Modified;
                var queue = new SocialCampaignQueue
                {
                    Domain = cap.WorkGroup.Domain,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = cap.CampaignPost.CreatedBy,
                    Post = cap.CampaignPost,
                    PostingDate = datePost,
                    Status = CampaignPostQueueStatus.Scheduled,
                    CountErrors = 0,
                    isNotifyWhenSent = isNotifyWhenSent
                };
                dbContext.SocialCampaignQueues.Add(queue);
                dbContext.Entry(queue).State = EntityState.Added;
                if (dbContext.SaveChanges() > 0)
                {
                    var reminder = datePost - DateTime.UtcNow;

                    var job = new QbicleJobParameter
                    {
                        Id = queue.Id,
                        EndPointName = "schedulecampaignpost",
                        ReminderMinutes = reminder.TotalMinutes
                    };
                    var result = await new QbiclesJob().HangFireExcecuteAsync(job);
                    if (result != null && result.Status == HttpStatusCode.OK && UpdateQueueJobId(job.Id, result.JobId))
                        refModel.result = true;
                    else
                        refModel.msg = refModel.msg = ResourcesManager._L("ERROR_MSG_809");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, aid, sPostingDate, isNotifyWhenSent, currentTimeZone, dateTimeFormat);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public async Task<ReturnJsonModel> RemoveQueueSchedule(int campaignId)
        {
            var refModel = new ReturnJsonModel { result = true };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Remove queue schedule", null, null, campaignId);

                    SocialCampaign campaign = dbContext.SocialCampaigns.FirstOrDefault(s => s.Id == campaignId);
                    var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(campaign?.Domain.Id ?? 0);
                    List<SocialCampaignQueue> lstPostInQueue = dbContext.SocialCampaignQueues.Where(s => s.Post.AssociatedCampaign.Id == campaignId && s.Status == CampaignPostQueueStatus.Scheduled).ToList();
                    List<int> lstPostId = lstPostInQueue.Select(s => s.Post.Id).ToList();
                    List<string> lstJobId = lstPostInQueue.Select(s => s.JobId).ToList();
                    foreach (var id in lstJobId)
                    {
                        var job = new QbicleJobParameter
                        {
                            JobId = id,
                            EndPointName = "removecampaignpost"
                        };
                        var result = await new QbiclesJob().HangFireExcecuteAsync(job);
                        if (result == null || result.Status != HttpStatusCode.OK)
                        {
                            refModel.result = false;
                            break;
                        }
                    }

                    if (refModel.result)
                    {
                        dbContext.SocialCampaignQueues.RemoveRange(lstPostInQueue);
                        var lstCampaignApproval = dbContext.SocialCampaignApprovals.Where(s => lstPostId.Contains(s.CampaignPost.Id));
                        lstCampaignApproval.ForEach(a => a.ApprovalStatus = SalesMktApprovalStatusEnum.Approved);
                        dbContext.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignId);
                    transaction.Rollback();
                    refModel.result = false;
                    refModel.msg = ex.Message;
                }
            }
            return refModel;
        }

        public async Task<ReturnJsonModel> AddEmailQueueSchedule(int aid, string sPostingDate, bool isNotifyWhenSent, string currentTimeZone, string dateTimeFormat)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add email queue schedule", null, null, aid, sPostingDate, isNotifyWhenSent, currentTimeZone, dateTimeFormat);

                var datePost = DateTime.UtcNow;
                var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                if (!string.IsNullOrEmpty(sPostingDate))
                {
                    datePost = TimeZoneInfo.ConvertTimeToUtc(sPostingDate.ConvertDateFormat(dateTimeFormat), tz);
                }
                var cap = dbContext.EmailPostApproval.Find(aid);
                if (cap == null)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_810");
                    return refModel;
                }

                cap.ApprovalStatus = SalesMktApprovalStatusEnum.Queued;
                if (dbContext.Entry((object)cap).State == EntityState.Detached)
                    dbContext.EmailPostApproval.Attach(cap);
                dbContext.Entry((object)cap).State = EntityState.Modified;
                var queue = new EmailCampaignQueue
                {
                    Domain = cap.WorkGroup.Domain,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = cap.CampaignEmail.CreatedBy,
                    Email = cap.CampaignEmail,
                    PostingDate = datePost,
                    Status = CampaignEmailQueueStatus.Scheduled,
                    CountErrors = 0,
                    isNotifyWhenSent = isNotifyWhenSent
                };
                dbContext.EmailCampaignQueues.Add(queue);
                dbContext.Entry(queue).State = EntityState.Added;
                if (dbContext.SaveChanges() > 0)
                {
                    var reminder = datePost - DateTime.UtcNow;

                    var job = new QbicleJobParameter
                    {
                        Id = queue.Id,
                        EndPointName = "scheduleemailcampaignpost",
                        ReminderMinutes = reminder.TotalMinutes
                    };
                    var result = await new QbiclesJob().HangFireExcecuteAsync(job);
                    if (result != null && result.Status == HttpStatusCode.OK && UpdateEmailQueueJobId(job.Id, result.JobId))
                        refModel.result = true;
                    else
                        refModel.msg = ResourcesManager._L("ERROR_MSG_809");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, aid, sPostingDate, isNotifyWhenSent, currentTimeZone, dateTimeFormat);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public async Task<ReturnJsonModel> RemoveEmailQueueSchedule(int campaignId)
        {
            var refModel = new ReturnJsonModel { result = true };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Remove email queue schedule", null, null, campaignId);

                    List<EmailCampaignQueue> lstPostInQueue = dbContext.EmailCampaignQueues.Where(s => s.Email.Campaign.Id == campaignId && s.Status == CampaignEmailQueueStatus.Scheduled).ToList();
                    var campaign = dbContext.EmailCampaigns.FirstOrDefault(s => s.Id == campaignId);
                    var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(campaign?.Domain.Id ?? 0);
                    List<int> lstPostId = lstPostInQueue.Select(s => s.Email.Id).ToList();
                    List<string> lstJobId = lstPostInQueue.Select(s => s.JobId).ToList();
                    foreach (var id in lstJobId)
                    {
                        var job = new QbicleJobParameter
                        {
                            JobId = id,
                            EndPointName = "removecampaignpost"
                        };
                        var result = await new QbiclesJob().HangFireExcecuteAsync(job);
                        if (result == null || result.Status != HttpStatusCode.OK)
                        {
                            refModel.result = false;
                            break;
                        }
                    }

                    if (refModel.result)
                    {
                        dbContext.EmailCampaignQueues.RemoveRange(lstPostInQueue);
                        var lstEmailCampaignApproval = dbContext.EmailPostApproval.Where(s => lstPostId.Contains(s.CampaignEmail.Id));
                        lstEmailCampaignApproval.ForEach(a => a.ApprovalStatus = SalesMktApprovalStatusEnum.Approved);
                        dbContext.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignId);
                    transaction.Rollback();
                    refModel.result = false;
                    refModel.msg = ex.Message;
                }
            }
            return refModel;
        }

        public bool SentBackToReview(int aid)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send back to review", null, null, aid);

                var cap = dbContext.SocialCampaignApprovals.Find(aid);
                if (cap != null)
                {
                    var appreq = cap.Activity;
                    appreq.RequestStatus = RequestStatusEnum.Reviewed;
                    if (dbContext.Entry(appreq).State == EntityState.Detached)
                        dbContext.ApprovalReqs.Attach(appreq);
                    dbContext.Entry(appreq).State = EntityState.Modified;
                    cap.ApprovalStatus = SalesMktApprovalStatusEnum.InReview;
                    if (dbContext.Entry(cap).State == EntityState.Detached)
                        dbContext.SocialCampaignApprovals.Attach(cap);
                    dbContext.Entry(cap).State = EntityState.Modified;
                }
                return dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, aid);
                return false;
            }
        }

        public bool SentBackEmailPostToReview(int aid)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send back email post to review", null, null, aid);

                var cap = dbContext.EmailPostApproval.Find(aid);
                if (cap != null)
                {
                    var appreq = cap.Activity;
                    appreq.RequestStatus = RequestStatusEnum.Reviewed;
                    if (dbContext.Entry(appreq).State == EntityState.Detached)
                        dbContext.ApprovalReqs.Attach(appreq);
                    dbContext.Entry(appreq).State = EntityState.Modified;
                    cap.ApprovalStatus = SalesMktApprovalStatusEnum.InReview;
                    if (dbContext.Entry(cap).State == EntityState.Detached)
                        dbContext.EmailPostApproval.Attach(cap);
                    dbContext.Entry(cap).State = EntityState.Modified;
                }
                return dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, aid);
                return false;
            }
        }

        public async Task<ReturnJsonModel> SentBackFromQueueToReview(int queueId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send back from queue to review", null, null, queueId);

                var queue = dbContext.SocialCampaignQueues.Find(queueId);
                if (queue != null)
                {
                    var campaignApproval = dbContext.SocialCampaignApprovals.SingleOrDefault(s => s.CampaignPost.Id == queue.Post.Id);
                    if (campaignApproval != null)
                    {
                        var appreq = campaignApproval.Activity;
                        appreq.RequestStatus = RequestStatusEnum.Reviewed;
                        if (dbContext.Entry(appreq).State == EntityState.Detached)
                            dbContext.ApprovalReqs.Attach(appreq);
                        dbContext.Entry(appreq).State = EntityState.Modified;
                        campaignApproval.ApprovalStatus = SalesMktApprovalStatusEnum.InReview;
                        if (dbContext.Entry(campaignApproval).State == EntityState.Detached)
                            dbContext.SocialCampaignApprovals.Attach(campaignApproval);
                        dbContext.Entry(campaignApproval).State = EntityState.Modified;
                        string _jobId = queue.JobId;
                        dbContext.SocialCampaignQueues.Remove(queue);
                        refModel.result = dbContext.SaveChanges() > 0;

                        #region remove Job Hangfire

                        if (!string.IsNullOrEmpty(_jobId))
                        {
                            var job = new QbicleJobParameter
                            {
                                JobId = _jobId,
                                EndPointName = "removecampaignpost",
                                ReminderMinutes = 0
                            };
                            var result = await new QbiclesJob().HangFireExcecuteAsync(job);
                            if (result != null && result.Status == HttpStatusCode.OK && UpdateQueueJobId(job.Id, result.JobId))
                                refModel.result = true;
                            else
                                refModel.msg = ResourcesManager._L("ERROR_MSG_811");
                        }

                        #endregion remove Job Hangfire
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public async Task<ReturnJsonModel> SentBackFromEmailQueueToReview(int queueId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Send back from email queue to review", null, null, queueId);

                var queue = dbContext.EmailCampaignQueues.Find(queueId);
                if (queue != null)
                {
                    var campaignApproval = dbContext.EmailPostApproval.SingleOrDefault(s => s.CampaignEmail.Id == queue.Email.Id);
                    if (campaignApproval != null)
                    {
                        var appreq = campaignApproval.Activity;
                        appreq.RequestStatus = RequestStatusEnum.Reviewed;
                        if (dbContext.Entry(appreq).State == EntityState.Detached)
                            dbContext.ApprovalReqs.Attach(appreq);
                        dbContext.Entry(appreq).State = EntityState.Modified;
                        campaignApproval.ApprovalStatus = SalesMktApprovalStatusEnum.InReview;
                        if (dbContext.Entry(campaignApproval).State == EntityState.Detached)
                            dbContext.EmailPostApproval.Attach(campaignApproval);
                        dbContext.Entry(campaignApproval).State = EntityState.Modified;
                        string _jobId = queue.JobId;
                        dbContext.EmailCampaignQueues.Remove(queue);
                        refModel.result = dbContext.SaveChanges() > 0;

                        #region remove Job Hangfire

                        if (!string.IsNullOrEmpty(_jobId))
                        {
                            var job = new QbicleJobParameter
                            {
                                JobId = _jobId,
                                EndPointName = "removeemailcampaignpost",
                                ReminderMinutes = 0
                            };
                            var result = await new QbiclesJob().HangFireExcecuteAsync(job);
                            if (result != null && result.Status == HttpStatusCode.OK && UpdateEmailQueueJobId(job.Id, result.JobId))
                                refModel.result = true;
                            else
                                refModel.msg = ResourcesManager._L("ERROR_MSG_811");
                        }

                        #endregion remove Job Hangfire
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public bool UpdatePostQueueImmediately(int queueId, int countErrors)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update post queue immediately", null, null, queueId, countErrors);

                var queue = dbContext.SocialCampaignQueues.Find(queueId);
                if (queue == null) return dbContext.SaveChanges() > 0;
                queue.PostingDate = DateTime.UtcNow;
                queue.CountErrors = countErrors;
                queue.Status = countErrors == 0 ? CampaignPostQueueStatus.Sent : CampaignPostQueueStatus.Error;
                //queue.JobId = string.Empty;
                if (dbContext.Entry(queue).State == EntityState.Detached)
                    dbContext.SocialCampaignQueues.Attach(queue);
                dbContext.Entry(queue).State = EntityState.Modified;
                return dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId, countErrors);
                return false;
            }
        }

        public bool UpdateEmailPostQueueImmediately(int queueId, int countErrors)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update email post queue immediately", null, null, queueId, countErrors);

                var queue = dbContext.EmailCampaignQueues.Find(queueId);
                if (queue == null) return dbContext.SaveChanges() > 0;
                queue.PostingDate = DateTime.UtcNow;
                queue.CountErrors = countErrors;
                queue.Status = countErrors == 0 ? CampaignEmailQueueStatus.Sent : CampaignEmailQueueStatus.Error;
                //queue.JobId = string.Empty;
                if (dbContext.Entry(queue).State == EntityState.Detached)
                    dbContext.EmailCampaignQueues.Attach(queue);
                dbContext.Entry(queue).State = EntityState.Modified;
                return dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId, countErrors);
                return false;
            }
        }

        public SocialCampaignQueue GetSocialCampaignQueue(int queueId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get social campaign queue", null, null, queueId);

                return dbContext.SocialCampaignQueues.Find(queueId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId);
                return null;
            }
        }

        public int CountQueue(int campaignById)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count queue", null, null, campaignById);

                return dbContext.SocialCampaignQueues.Count(s => s.Post.AssociatedCampaign.Id == campaignById && s.Status == CampaignPostQueueStatus.Scheduled);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById);
                return 0;
            }
        }

        public int CountApproved(int campaignById)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count Approved", null, null, campaignById);

                return dbContext.SocialCampaignApprovals.Count(s => s.CampaignPost.AssociatedCampaign.Id == campaignById && (s.ApprovalStatus == SalesMktApprovalStatusEnum.Approved || s.ApprovalStatus == SalesMktApprovalStatusEnum.Denied));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById);
                return 0;
            }
        }

        public int CountApproval(int campaignById)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count Approval", null, null, campaignById);

                return dbContext.SocialCampaignApprovals.Count(s => s.CampaignPost.AssociatedCampaign.Id == campaignById && (s.ApprovalStatus == SalesMktApprovalStatusEnum.InReview || s.ApprovalStatus == SalesMktApprovalStatusEnum.Denied));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById);
                return 0;
            }
        }

        public int CountSent(int campaignById)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count sent", null, null, campaignById);

                return dbContext.SocialCampaignQueues.Count(s => s.Post.AssociatedCampaign.Id == campaignById && (s.Status == CampaignPostQueueStatus.Sent || s.Status == CampaignPostQueueStatus.Error));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById);
                return 0;
            }
        }

        public int CountEmailCampaignQueue(int campaignById)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count email campaign queue", null, null, campaignById);

                return dbContext.EmailCampaignQueues.Count(s => s.Email.Campaign.Id == campaignById && s.Status == CampaignEmailQueueStatus.Scheduled);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById);
                return 0;
            }
        }

        public int CountEmailCampaignApproved(int campaignById)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count email campaign approved", null, null, campaignById);

                return dbContext.EmailPostApproval.Count(s => s.CampaignEmail.Campaign.Id == campaignById && (s.ApprovalStatus == SalesMktApprovalStatusEnum.Approved || s.ApprovalStatus == SalesMktApprovalStatusEnum.Denied));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById);
                return 0;
            }
        }

        public int CountEmailCampaignApproval(int campaignById)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count email campaign approval", null, null, campaignById);

                return dbContext.EmailPostApproval.Count(s => s.CampaignEmail.Campaign.Id == campaignById && s.ApprovalStatus == SalesMktApprovalStatusEnum.InReview);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById);
                return 0;
            }
        }

        public int CountEmailCampaignSent(int campaignById)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count email campaign sent", null, null, campaignById);

                return dbContext.EmailCampaignQueues.Count(s => s.Email.Campaign.Id == campaignById && (s.Status == CampaignEmailQueueStatus.Sent || s.Status == CampaignEmailQueueStatus.Error));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById);
                return 0;
            }
        }

        public List<SocialCampaignQueue> QueuePostsByCampaignId(int campaignById, bool isSent)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Queue post by campaign id", null, null, campaignById, isSent);

                if (!isSent)
                    return dbContext.SocialCampaignQueues.Where(s => s.Post.AssociatedCampaign.Id == campaignById && s.Status == CampaignPostQueueStatus.Scheduled).ToList();
                else
                    return dbContext.SocialCampaignQueues.Where(s => s.Post.AssociatedCampaign.Id == campaignById && (s.Status == CampaignPostQueueStatus.Sent || s.Status == CampaignPostQueueStatus.Error)).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById, isSent);
                return new List<SocialCampaignQueue>();
            }
        }

        public List<EmailCampaignQueue> QueueEmailPostsByCampaignId(int campaignById, bool isSent)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Queue email post by campaign id", null, null, campaignById, isSent);

                if (!isSent)
                    return dbContext.EmailCampaignQueues.Where(s => s.Email.Campaign.Id == campaignById && s.Status == CampaignEmailQueueStatus.Scheduled).ToList();
                else
                    return dbContext.EmailCampaignQueues.Where(s => s.Email.Campaign.Id == campaignById && (s.Status == CampaignEmailQueueStatus.Sent || s.Status == CampaignEmailQueueStatus.Error)).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById, isSent);
                return new List<EmailCampaignQueue>();
            }
        }

        public List<CampaignPostApproval> ApprovedPostsByCampaignId(int campaignById, bool isApproved)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Approved posts by campaign id", null, null, campaignById, isApproved);

                if (isApproved)
                    return dbContext.SocialCampaignApprovals.Where(s => s.CampaignPost.AssociatedCampaign.Id == campaignById && (s.ApprovalStatus == SalesMktApprovalStatusEnum.Approved || s.ApprovalStatus == SalesMktApprovalStatusEnum.Denied)).ToList();
                else
                    return dbContext.SocialCampaignApprovals.Where(s => s.CampaignPost.AssociatedCampaign.Id == campaignById && s.ApprovalStatus == SalesMktApprovalStatusEnum.InReview).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById, isApproved);
                return new List<CampaignPostApproval>();
            }
        }

        public List<EmailPostApproval> ApprovedEmailPostsByCampaignId(int campaignById, bool isApproved)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Approved email posts by campaign id", null, null, campaignById, isApproved);

                if (isApproved)
                    return dbContext.EmailPostApproval.Where(s => s.CampaignEmail.Campaign.Id == campaignById && (s.ApprovalStatus == SalesMktApprovalStatusEnum.Approved || s.ApprovalStatus == SalesMktApprovalStatusEnum.Denied)).ToList();
                else
                    return dbContext.EmailPostApproval.Where(s => s.CampaignEmail.Campaign.Id == campaignById && s.ApprovalStatus == SalesMktApprovalStatusEnum.InReview).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignById, isApproved);
                return new List<EmailPostApproval>();
            }
        }

        public CampaignPostApproval CampaignPostApprovalById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Campaign post approval by id", null, null, id);

                var data = dbContext.SocialCampaignApprovals.Find(id);
                return dbContext.SocialCampaignApprovals.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public SocialCampaignPost CampaignPostById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Campaign post by id", null, null, id);

                return dbContext.SocialCampaignPosts.FirstOrDefault(p => p.Id == id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        private bool UpdateQueueJobId(int queueId, string jobJd)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update queue job id", null, null, queueId, jobJd);

                var queue = dbContext.SocialCampaignQueues.Find(queueId);
                if (queue == null) return false;
                queue.JobId = jobJd;
                if (dbContext.Entry(queue).State == EntityState.Detached)
                    dbContext.SocialCampaignQueues.Attach(queue);
                dbContext.Entry(queue).State = EntityState.Modified;
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId, jobJd);
                return false;
            }
        }

        private bool UpdateEmailQueueJobId(int queueId, string jobJd)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update email queue job id", null, null, queueId, jobJd);

                var queue = dbContext.EmailCampaignQueues.Find(queueId);
                if (queue == null) return false;
                queue.JobId = jobJd;
                if (dbContext.Entry(queue).State == EntityState.Detached)
                    dbContext.EmailCampaignQueues.Attach(queue);
                dbContext.Entry(queue).State = EntityState.Modified;
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId, jobJd);
                return false;
            }
        }

        /// <summary>
        /// Post Immediately Campaign Post IN APPROVED
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public async Task<bool> PostImmediately(int aid)
        {
            using (dbContext)
            {
                using (var dbContextTransaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (ConfigManager.LoggingDebugSet)
                            LogManager.Debug(MethodBase.GetCurrentMethod(), "Post immediately", null, null, aid);

                        var campaign = dbContext.SocialCampaignApprovals.Find(aid);
                        if (campaign == null) return true;
                        campaign.ApprovalStatus = SalesMktApprovalStatusEnum.Queued;
                        if (dbContext.Entry(campaign).State == EntityState.Detached)
                            dbContext.SocialCampaignApprovals.Attach(campaign);
                        dbContext.Entry(campaign).State = EntityState.Modified;
                        var queue = new SocialCampaignQueue
                        {
                            Domain = campaign.WorkGroup.Domain,
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = campaign.CampaignPost.CreatedBy,
                            Post = campaign.CampaignPost,
                            PostingDate = DateTime.UtcNow,
                            Status = CampaignPostQueueStatus.Sent,
                            CountErrors = 0,
                            isNotifyWhenSent = true
                        };
                        dbContext.SocialCampaignQueues.Add(queue);
                        dbContext.Entry(queue).State = EntityState.Added;
                        var result = await dbContext.SaveChangesAsync();
                        if (result > 0)
                        {
                            //await PostSocialNetworkAccounts(queue.Id);

                            var job = new QbicleJobParameter
                            {
                                Id = queue.Id,
                                EndPointName = "executecampaignpost",
                                ReminderMinutes = 0
                            };
                            var resultApi = await new QbiclesJob().HangFireExcecuteAsync(job);
                            if (resultApi.Status != HttpStatusCode.OK)
                            {
                                dbContextTransaction.Rollback();
                                return false;
                            }
                        }
                        dbContextTransaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, aid);
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Post Immediately Campaign Post IN APPROVED
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public async Task<bool> PostEmailImmediately(int aid)
        {
            using (var dbContextTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Post email immediately", null, null, aid);

                    var campaign = dbContext.EmailPostApproval.Find(aid);
                    if (campaign == null) return true;
                    campaign.ApprovalStatus = SalesMktApprovalStatusEnum.Queued;
                    if (dbContext.Entry(campaign).State == EntityState.Detached)
                        dbContext.EmailPostApproval.Attach(campaign);
                    dbContext.Entry(campaign).State = EntityState.Modified;
                    var queue = new EmailCampaignQueue
                    {
                        Domain = campaign.WorkGroup.Domain,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = campaign.CampaignEmail.CreatedBy,
                        Email = campaign.CampaignEmail,
                        PostingDate = DateTime.UtcNow,
                        Status = CampaignEmailQueueStatus.Sent,
                        CountErrors = 0,
                        isNotifyWhenSent = true
                    };
                    dbContext.EmailCampaignQueues.Add(queue);
                    dbContext.Entry(queue).State = EntityState.Added;
                    var result = dbContext.SaveChanges();
                    if (result > 0)
                    {
                        //PostEmailCampaign(queue.Id);

                        var job = new QbicleJobParameter
                        {
                            Id = queue.Id,
                            EndPointName = "executeemailcampaignpost",
                            ReminderMinutes = 0
                        };
                        var resultApi = await new QbiclesJob().HangFireExcecuteAsync(job);
                        if (resultApi.Status != HttpStatusCode.OK)
                        {
                            dbContextTransaction.Rollback();
                            return false;
                        }
                    }
                    dbContextTransaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, aid);
                    return false;
                }
            }
        }

        /// <summary>
        /// Change post in approved to sent
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public bool ChangePostInApprovedToSent(int aid)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Change post in approved to sent", null, null, aid);

                var campaign = dbContext.SocialCampaignApprovals.Find(aid);
                if (campaign == null) return true;
                campaign.ApprovalStatus = SalesMktApprovalStatusEnum.Queued;
                if (dbContext.Entry(campaign).State == EntityState.Detached)
                    dbContext.SocialCampaignApprovals.Attach(campaign);
                dbContext.Entry(campaign).State = EntityState.Modified;
                var queue = new SocialCampaignQueue
                {
                    Domain = campaign.WorkGroup.Domain,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = campaign.CampaignPost.CreatedBy,
                    Post = campaign.CampaignPost,
                    PostingDate = DateTime.UtcNow,
                    Status = CampaignPostQueueStatus.Sent,
                    CountErrors = 0,
                    isNotifyWhenSent = true
                };
                dbContext.SocialCampaignQueues.Add(queue);
                dbContext.Entry(queue).State = EntityState.Added;
                var result = dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, aid);
                return false;
            }
        }

        /// <summary>
        /// Post Immediately Campaign Post IN QUEUE
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public async Task<bool> PostQueueImmediately(int queueId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Post queue immediately", null, null, queueId);

                //await PostSocialNetworkAccounts(queueId);
                //input to hangfire
                var queue = dbContext.SocialCampaignQueues.Find(queueId);
                if (queue == null) return true;

                var job = new QbicleJobParameter
                {
                    Id = queue.Id,
                    JobId = queue.JobId,
                    EndPointName = "executecampaignpost",
                };
                var result = await new QbiclesJob().HangFireExcecuteAsync(job);
                return result.Status == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId);
                return false;
            }
        }

        /// <summary>
        /// Post Immediately Campaign Post IN QUEUE
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public async Task<bool> PostEmailQueueImmediately(int queueId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Post email queue immediately", null, null, queueId);

                //await PostSocialNetworkAccounts(queueId);
                //input to hangfire
                var queue = dbContext.EmailCampaignQueues.Find(queueId);
                queue.Status = CampaignEmailQueueStatus.Sent;
                dbContext.EmailCampaignQueues.Attach(queue);
                dbContext.Entry(queue).State = EntityState.Modified;
                dbContext.SaveChanges();

                var job = new QbicleJobParameter
                {
                    Id = queue.Id,
                    JobId = queue.JobId,
                    EndPointName = "executeemailcampaignpost",
                };
                var result = await new QbiclesJob().HangFireExcecuteAsync(job);
                return result.Status == HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId);
                return false;
            }
        }

        /// <summary>
        /// Change post in queue to sent
        /// </summary>
        /// <param name="queueId"></param>
        /// <returns></returns>
        public bool ChangePostInQueueToSent(int queueId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Change post in queue to sent", null, null, queueId);

                var queue = dbContext.SocialCampaignQueues.Find(queueId);
                queue.Status = CampaignPostQueueStatus.Sent;
                dbContext.SocialCampaignQueues.Attach(queue);
                dbContext.Entry(queue).State = EntityState.Modified;
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId);
                return false;
            }
        }

        /// <summary>
        /// Call from HangFire
        /// Post SocialCampaignQueue SocialNetworkAccounts
        /// </summary>
        public async Task<ReturnJsonModel> PostSocialNetworkAccounts(int queueId)
        {
            var refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Post social network accounts", null, null, queueId);

                int countErrors = 0;
                var socialCampaignQueue = GetSocialCampaignQueue(queueId);
                var socialNetworkSystemSetting = dbContext.SocialNetworkSystemSettings.FirstOrDefault();
                if (socialNetworkSystemSetting != null)
                {
                    var campaignPost = socialCampaignQueue.Post;
                    var sharingAccounts = campaignPost.SharingAccount.Where(s => !s.IsDisabled).ToList();

                    var file = campaignPost.ImageOrVideo?.VersionedFiles.Where(s => !s.IsDeleted).OrderByDescending(o => o.UploadedDate).FirstOrDefault();
                    var filename = file != null ? Path.GetFileName(file.Uri + "." + file.FileType.Extension) : "";
                    byte[] imageBytes = null;
                    if (campaignPost.ImageOrVideo != null)
                    {
                        imageBytes = GetMedia(file.Uri);
                    }
                    //End
                    if (sharingAccounts.Count > 0)
                    {
                        foreach (var item in sharingAccounts)
                        {
                            if (item.Type != null && item.Type.Name.ToUpper() == "Facebook".ToUpper())
                            {
                                try
                                {
                                    var fb = (FaceBookAccount)item;
                                    var facebookClient = new FacebookClient(fb.Token);
                                    var parametersPost = new Dictionary<string, object>();
                                    if (campaignPost.ImageOrVideo != null)
                                    {
                                        if (file != null && file.FileType.Type == "Video File")
                                        {
                                            //Post Video
                                            dynamic parameters = new ExpandoObject();
                                            parameters.source = new FacebookMediaObject
                                            {
                                                ContentType = "multipart/form-data",
                                                FileName = Path.GetFileName(filename)
                                            }.SetValue(imageBytes);
                                            parameters.title = campaignPost.Content;
                                            parameters.description = campaignPost.Content;
                                            var url = "https://graph-video.facebook.com" + $"/{fb.FaceBookId}/videos";
                                            //var dresult = await facebookClient.PostTaskAsync(url, parameters);
                                            await facebookClient.PostTaskAsync(url, parameters);
                                        }
                                        else
                                        {
                                            //post Image
                                            var imgBatch = new FacebookBatchParameter(
                                        Facebook.HttpMethod.Post, $"/{fb.FaceBookId}/photos",
                                        new Dictionary<string, object> {
                                        { "published", false },
                                        { "pic" + fb.Id,
                                            new FacebookMediaObject
                                        {
                                            ContentType = MimeMapping.GetMimeMapping(filename),
                                            FileName = Path.GetFileName(filename)
                                        }.SetValue(imageBytes) }
                                        });
                                            dynamic imgList = facebookClient.Batch(
                                                    imgBatch
                                                );
                                            var ls = new List<dynamic>();
                                            foreach (var media in imgList)
                                            {
                                                ls.Add(new { media_fbid = media.id });
                                            }
                                            parametersPost["attached_media"] = ls.ToArray();
                                            parametersPost["message"] = campaignPost.Content;
                                            //await facebookClient.PostTaskAsync($"/{fb.FaceBookId}/feed", parametersPost);
                                            var fbPostTaskResult = await facebookClient.PostTaskAsync($"/{fb.FaceBookId}/feed", parametersPost);
                                            var result = (IDictionary<string, object>)fbPostTaskResult;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    countErrors = countErrors + 1;
                                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId);
                                }
                            }
                            else
                            {
                                try
                                {
                                    // post twitter
                                    var tw = (TwitterAccount)item;
                                    var appCredentials = new TwitterCredentials(
                                         socialNetworkSystemSetting.TwitterConsumerKey,
                                         socialNetworkSystemSetting.TwitterConsumerSecret,
                                         tw.Token,
                                         tw.TokenSecret
                                        );
                                    var tweet = Auth.ExecuteOperationWithCredentials(appCredentials, () =>
                                    {
                                        IMedia media = file != null && file.FileType.Type == "Video File"
                                            ? Upload.UploadVideo(imageBytes)
                                            : Upload.UploadBinary(imageBytes);

                                        return Tweet.PublishTweet(campaignPost.Content, new PublishTweetOptionalParameters
                                        {
                                            Medias = { media }
                                        });
                                    });
                                }
                                catch (Exception ex)
                                {
                                    countErrors = countErrors + 1;
                                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId);
                                }
                            }
                        }
                        refModel.result = true;
                        UpdatePostQueueImmediately(queueId, countErrors);
                    }
                }
                else
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_812");
                    refModel.result = false;
                }
            }
            catch (Exception ex)
            {
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId);
            }
            return refModel;
        }

        /// <summary>
        /// Call from HangFire
        /// Post SocialCampaignQueue SocialNetworkAccounts
        /// </summary>
        public async Task<ReturnJsonModel> PostEmailCampaign(int queueId)
        {
            var refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Post email campaign", null, null, queueId);

                EmailRules rule = new EmailRules(dbContext);
                int countErrors = 0;
                EmailCampaignQueue queue = dbContext.EmailCampaignQueues.FirstOrDefault(e => e.Id == queueId);

                string from = queue.Email.FromEmail;
                string replyto = queue.Email.ReplyEmail;

                List<SMContact> lstContacts = queue.Email.Segments.SelectMany(s => s.Contacts.Where(c => c.IsSubscribed)).Distinct().ToList();

                List<MailMessage> lstMail = new List<MailMessage>();
                var campaignpost = queue.Email;
                var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
                List<LinkedResource> linkedResources = new List<LinkedResource>();
                string body = rule.CreateEmailBody2Campaign(queue.Email, ref linkedResources);

                #region Linked Resources Email

                var webClient = new WebClient();
                LinkedResource promotionalResource = null;
                if (campaignpost.PromotionalImage != null)
                {
                    var vsFilePromotionalImage = queue.Email.PromotionalImage != null && queue.Email.PromotionalImage.VersionedFiles != null ? queue.Email.PromotionalImage.VersionedFiles.OrderByDescending(s => s.UploadedDate).FirstOrDefault() : null;
                    //var awsS3Bucket = AzureStorageHelper.AwsS3Client();
                    var s3Object = await AzureStorageHelper.ReadObjectDataAsync(vsFilePromotionalImage?.Uri);
                    var filePath = Path.Combine(ConfigManager.TempPathRepository, s3Object.ObjectKey);
                    using (FileStream outputFileStream = new FileStream($"{filePath}", FileMode.Create))
                    {
                        await s3Object.ObjectStream.CopyToAsync(outputFileStream);
                    }
                    promotionalResource = new LinkedResource(filePath)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };
                    var promoImageExtension = queue.Email.PromotionalImage != null ? vsFilePromotionalImage?.FileType.Extension : "jpg";
                    promotionalResource.ContentType.Name = "promotional." + promoImageExtension;
                    body = body.Replace("{PromotionalImgPath}", "cid:" + promotionalResource.ContentId);
                }
                else
                {
                    body = body.Replace("{PromotionalImgPath}", "");
                }
                LinkedResource advImgResource = null;
                if ((campaignpost.Template == null && campaignpost.AdvertisementImage != null) || (campaignpost.Template != null && campaignpost.Template.AdvertImgiIsHidden && campaignpost.AdvertisementImage != null))
                {
                    var vsFileAdvertisementImage = queue.Email.AdvertisementImage != null && queue.Email.AdvertisementImage.VersionedFiles != null ? queue.Email.AdvertisementImage.VersionedFiles.OrderByDescending(s => s.UploadedDate).FirstOrDefault() : null;
                    var s3Object = await AzureStorageHelper.ReadObjectDataAsync(vsFileAdvertisementImage?.Uri);
                    byte[] imageBytesProm = null;
                    var memoryStream = new MemoryStream();
                    await s3Object.ObjectStream.CopyToAsync(memoryStream);
                    imageBytesProm = memoryStream.ToArray();
                    var stream = new MemoryStream(imageBytesProm);
                    advImgResource = new LinkedResource(stream)
                    {
                        ContentId = Guid.NewGuid().ToString()
                    };
                    var extention = queue.Email.AdvertisementImage != null ? vsFileAdvertisementImage?.FileType.Extension : "jpg";
                    advImgResource.ContentType.Name = "adv." + extention;
                    body = body.Replace("{AdImgPath}", "cid:" + advImgResource.ContentId);
                }
                else
                {
                    body = body.Replace("{AdImgPath}", "");
                }

                #endregion Linked Resources Email

                foreach (var contact in lstContacts)
                {
                    var mail = new MailMessage();
                    mail.ReplyToList.Add(replyto);
                    mail.From = new MailAddress(from, queue.Email.FromName);
                    mail.Subject = queue.Email.EmailSubject;
                    mail.IsBodyHtml = true;
                    mail.BodyEncoding = Encoding.UTF8;
                    mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    var bodyemail = body.Replace("{Unsubscribe}", ConfigurationManager.AppSettings["BaseURL"] + "/Manage/Unsubscribe?token=" + HelperClass.Encrypt(campaignpost.Campaign.Id + "@" + contact.Id));
                    //AlternateView body = rule.CreateEmailBody2Campaign(queue.Email, msPromotionalImage, promoImageExtension, AdvertisementImage, advImageExtension, contact.Id);
                    var bodyview = AlternateView.CreateAlternateViewFromString(bodyemail, null, MediaTypeNames.Text.Html);
                    if (promotionalResource != null)
                        bodyview.LinkedResources.Add(promotionalResource);
                    if (advImgResource != null)
                        bodyview.LinkedResources.Add(advImgResource);
                    foreach (var item in linkedResources)
                    {
                        bodyview.LinkedResources.Add(item);
                    }
                    mail.AlternateViews.Add(bodyview);
                    mail.To.Add(contact.Email);
                    lstMail.Add(mail);
                }
                var emailHelper = new EmailHelperRules(dbContext);
                countErrors = await emailHelper.SendEmailCampaignAsync(lstMail);

                foreach (var mail in lstMail)
                {
                    var mess = new IdentityMessage
                    {
                        Destination = mail.To.FirstOrDefault().Address,
                        Subject = queue.Email.EmailSubject, //enum get name from value
                        Body = mail.Subject
                    };
                    //save log send email
                    emailHelper.SaveEmailLogNotification(mess, queue.Email.EmailSubject, ReasonSent.EmailCampaignPost);
                }

                refModel.result = true;
                UpdateEmailPostQueueImmediately(queueId, countErrors);
            }
            catch (Exception ex)
            {
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, queueId);
            }
            return refModel;
        }

        public bool AddPostToSocialPostApproval(string socialPostKey, QbiclePost post, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add post to social post approval", null, null, socialPostKey, post);
                var socialPostId = int.Parse(socialPostKey.Decrypt());
                var socialPostReq = dbContext.Activities.Find(socialPostId);
                socialPostReq.TimeLineDate = DateTime.UtcNow;
                socialPostReq.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                socialPostReq.Posts.Add(post);
                socialPostReq.Qbicle.LastUpdated = DateTime.UtcNow;
                dbContext.Activities.Attach(socialPostReq);
                dbContext.Entry(socialPostReq).State = EntityState.Modified;
                dbContext.SaveChanges();

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = socialPostReq.Id,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.DiscussionCreation,
                    AppendToPageName = ApplicationPageName.Approval,
                    AppendToPageId = socialPostId,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).NotificationComment2Activity(activityNotification);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, socialPostKey, post);
                return false;
            }
        }

        public List<QbicleMedia> GetMediasCampaign(int fid, int qid, int fbrandid, int fideaid, string currentTimeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get media campaign", null, null, fid, qid, fbrandid, fideaid, currentTimeZone);

                var mediaRule = new MediaFolderRules(dbContext);
                var listMedia = mediaRule.GetMediaItemByFolderId(fid, qid, currentTimeZone);
                if (fbrandid > 0)
                {
                    var mediasbrand = mediaRule.GetMediaItemByFolderId(fbrandid, qid, currentTimeZone);
                    if (mediasbrand != null)
                        listMedia.AddRange(mediasbrand);
                }
                if (fideaid > 0)
                {
                    var mediasidea = mediaRule.GetMediaItemByFolderId(fideaid, qid, currentTimeZone);
                    if (mediasidea != null)
                        listMedia.AddRange(mediasidea);
                }
                return listMedia;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, fid, qid, fbrandid, fideaid, currentTimeZone);
                return new List<QbicleMedia>();
            }
        }

        public async Task<ReturnJsonModel> PostSocialCampaignNotification(int campaignPostId)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Post social campaign notification", null, null, campaignPostId);

                var dbCampaignPostApproval = dbContext.SocialCampaignApprovals.FirstOrDefault(s => s.CampaignPost.Id == campaignPostId);
                if (dbCampaignPostApproval != null)
                {
                    var appReq = dbCampaignPostApproval.Activity;
                    await Task.Run(() =>
                    {
                        var notifyUsers = new List<ApplicationUser> { appReq.StartedBy };
                        new NotificationRules(dbContext).NotifyReminderCampaignPost(appReq, notifyUsers, appReq.StartedBy, Notification.NotificationEventEnum.ReminderCampaignPost);
                    });

                    returnJson.result = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, campaignPostId);
                returnJson.msg = ex.Message;
            }

            return returnJson;
        }

        private byte[] GetMedia(string file)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get media", null, null, file);

                //TODO: get file from S3
                //var image = dbContext.StorageFiles.Find(file);
                //if (image == null) return null;

                //var root = ConfigManager.QbiclesRepository;
                //var fullFileName = Path.Combine(root, image.Path);

                //var stream = File.OpenRead(fullFileName);
                //var fileBytes = new byte[stream.Length];

                //stream.Read(fileBytes, 0, fileBytes.Length);
                //stream.Close();
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, file);
                return null;
            }
        }

        public void SMUiSetting(string page, string userId, string tab)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SMUiSetting", userId, null, page, tab);

                List<UiSetting> uis = new List<UiSetting>();
                uis.Add(new UiSetting()
                {
                    CurrentPage = page,
                    CurrentUser = dbContext.QbicleUser.Find(userId),
                    Key = SaleMarketingStoreUiSettings.tabActive,
                    Value = tab
                });
                new QbicleRules(dbContext).StoredUiSettings(uis);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, page, tab);
            }
        }

        public SegmentContactModel CountContacts(int[] lstSegments)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count contacts", null, null, lstSegments);

                SegmentContactModel model = new SegmentContactModel();
                model.lstSegments = String.Join(", ", dbContext.SMSegments.Where(s => lstSegments.Contains(s.Id)).Select(s => s.Name).ToArray());
                model.totalContacts = dbContext.SMSegments.Where(s => lstSegments.Contains(s.Id)).SelectMany(s => s.Contacts.Where(c => c.IsSubscribed)).Distinct().Count();
                return model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, lstSegments);
                return null;
            }
        }

        public int CountListPipeline(string name, bool isLoadingHide, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count list pipeline", null, null, name, isLoadingHide, domainId);

                return dbContext.Pipelines.Where(p => p.Domain.Id == domainId && (isLoadingHide || (!isLoadingHide && !p.IsHidden)) && (name.Equals("") || p.Name.ToLower().Contains(name.Trim().ToLower()))).Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name, isLoadingHide, domainId);
                return 0;
            }
        }

        public List<Pipeline> GetListPipeline(string name, bool isLoadingHide, int domainId, int skip, int take)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get list pipeline", null, null, name, isLoadingHide, domainId, skip, take);

                return dbContext.Pipelines.Where(p => p.Domain.Id == domainId && (isLoadingHide || (!isLoadingHide && !p.IsHidden)) && (name.Equals("") || p.Name.ToLower().Contains(name.Trim().ToLower()))).OrderByDescending(p => p.Id).Skip(skip).Take(take).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, name, isLoadingHide, domainId, skip, take);
                return new List<Pipeline>();
            }
        }

        public ReturnJsonModel ShowOrHidePipeline(int id)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show or hide pipeline", null, null, id);

                var pipeline = dbContext.Pipelines.FirstOrDefault(s => s.Id == id);
                pipeline.IsHidden = !pipeline.IsHidden;
                if (dbContext.Entry(pipeline).State == EntityState.Detached)
                    dbContext.Pipelines.Attach(pipeline);
                dbContext.Entry(pipeline).State = EntityState.Modified;
                returnModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnModel;
        }

        public List<PipelineContact> LoadExistPipelineContacts(int id, string name)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load exist pipeline contacts", null, null, id, name);

                return dbContext.PipelineContacts.Where(p => p.Pipeline.Id == id && (name.Equals("") || p.Contact.Name.ToLower().Contains(name.Trim().ToLower()))).OrderBy(p => p.Contact.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, name);
                return new List<PipelineContact>();
            }
        }

        public List<SMContact> LoadNewPipelineContacts(int domainId, string name, int pipelineId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load new pipeline contacts", null, null, domainId, name, pipelineId);

                var lstContactInPipeline = dbContext.PipelineContacts.Where(p => p.Pipeline.Id == pipelineId).Select(p => p.Contact.Id).ToList();
                return dbContext.SMContacts.Where(p => p.Domain.Id == domainId && !lstContactInPipeline.Contains(p.Id) && (name.Equals("") || p.Name.ToLower().Contains(name.Trim().ToLower()))).OrderBy(p => p.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, name, pipelineId);
                return new List<SMContact>();
            }
        }

        public Pipeline GetPipelineById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get pipeline by id", null, null, id);

                return dbContext.Pipelines.FirstOrDefault(p => p.Id == id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public PipelineContact GetExistPipelineContactById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get exist pipeline contact by id", null, null, id);

                return dbContext.PipelineContacts.FirstOrDefault(p => p.Id == id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new PipelineContact();
            }
        }

        public ReturnJsonModel SavePipeline(Pipeline pipeline, string[] steps, int[] idSteps, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save pipeline", userId, null, pipeline,
                        steps, idSteps);

                if (!string.IsNullOrEmpty(pipeline.FeaturedImageUri))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(pipeline.FeaturedImageUri);
                }
                var user = dbContext.QbicleUser.Find(userId);
                Pipeline mdPipeline = dbContext.Pipelines.Find(pipeline.Id);
                if (mdPipeline != null)
                {
                    mdPipeline.Name = pipeline.Name;
                    mdPipeline.Summary = pipeline.Summary;
                    mdPipeline.FeaturedImageUri = string.IsNullOrEmpty(pipeline.FeaturedImageUri) ? mdPipeline.FeaturedImageUri : pipeline.FeaturedImageUri;
                    mdPipeline.LastUpdatedBy = user;
                    mdPipeline.LastUpdateDate = DateTime.UtcNow;

                    var lstStepIdsToRemove = dbContext.PipelineSteps.Where(s => s.Pipeline.Id == pipeline.Id && !idSteps.Contains(s.Id)).Select(s => s.Id).ToArray();
                    var lstPipelineContactToMove = dbContext.PipelineContacts.Where(p => p.Pipeline.Id == pipeline.Id && !idSteps.Contains(p.Step.Id));

                    int idx = 1;
                    for (var i = 0; i < steps.Count(); i++)
                    {
                        if (idSteps[i] == 0)
                        {
                            Step step = new Step();
                            step.Pipeline = mdPipeline;
                            step.Name = steps[i];
                            step.Order = idx;
                            step.CreatedBy = user;
                            step.CreatedDate = DateTime.UtcNow;
                            dbContext.PipelineSteps.Add(step);
                            dbContext.Entry(step).State = EntityState.Added;
                        }
                        else
                        {
                            int stepId = idSteps[i];
                            Step step = dbContext.PipelineSteps.FirstOrDefault(p => p.Id == stepId);
                            step.Name = steps[i];
                            step.Order = idx;
                            if (dbContext.Entry(step).State == EntityState.Detached)
                                dbContext.PipelineSteps.Attach(step);
                            dbContext.Entry(step).State = EntityState.Modified;
                        }
                        idx++;
                    }
                    dbContext.SaveChanges();

                    int min = dbContext.PipelineSteps.Where(s => s.Pipeline.Id == pipeline.Id && !lstStepIdsToRemove.Contains(s.Id)).Min(i => i.Order);
                    var firstStep = dbContext.PipelineSteps.Where(s => s.Pipeline.Id == pipeline.Id && !lstStepIdsToRemove.Contains(s.Id)).First(x => x.Order == min);
                    if (firstStep.Contacts == null)
                    {
                        firstStep.Contacts = new List<PipelineContact>();
                    }
                    firstStep.Contacts.AddRange(lstPipelineContactToMove);
                    if (dbContext.Entry(firstStep).State == EntityState.Detached)
                        dbContext.PipelineSteps.Attach(firstStep);
                    dbContext.Entry(firstStep).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    dbContext.PipelineSteps.RemoveRange(dbContext.PipelineSteps.Where(s => lstStepIdsToRemove.Contains(s.Id)));
                    if (dbContext.Entry(mdPipeline).State == EntityState.Detached)
                        dbContext.Pipelines.Attach(mdPipeline);
                    dbContext.Entry(mdPipeline).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                else
                {
                    pipeline.CreatedDate = DateTime.UtcNow;
                    pipeline.LastUpdatedBy = user;
                    pipeline.LastUpdateDate = pipeline.CreatedDate;
                    pipeline.CreatedBy = user;

                    int idx = 1;
                    for (var i = 0; i < steps.Count(); i++)
                    {
                        Step step = new Step();
                        step.Pipeline = pipeline;
                        step.Name = steps[i];
                        step.Order = idx;
                        step.CreatedBy = user;
                        step.CreatedDate = DateTime.UtcNow;
                        pipeline.Steps.Add(step);
                        idx++;
                    }

                    dbContext.Pipelines.Add(pipeline);
                    dbContext.Entry(pipeline).State = EntityState.Added;
                    dbContext.SaveChanges();
                }

                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, pipeline, steps, idSteps);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public ReturnJsonModel SavePipelineContact(PipelineContact pipelineContact, int pipelineId, int contactId, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save pipeline contact", userId,
                        null, pipelineContact, pipelineId, contactId);

                var user = dbContext.QbicleUser.Find(userId);
                var mdPipelineContact = dbContext.PipelineContacts.FirstOrDefault(p => p.Id == pipelineContact.Id);
                if (mdPipelineContact != null)
                {
                    mdPipelineContact.PotentialValue = pipelineContact.PotentialValue;
                    mdPipelineContact.Rating = pipelineContact.Rating;
                    mdPipelineContact.LastUpdatedBy = user;
                    mdPipelineContact.LastUpdateDate = DateTime.UtcNow;
                    if (dbContext.Entry(mdPipelineContact).State == EntityState.Detached)
                        dbContext.PipelineContacts.Attach(mdPipelineContact);
                    dbContext.Entry(mdPipelineContact).State = EntityState.Modified;
                }
                else
                {
                    var pipeline = dbContext.Pipelines.FirstOrDefault(p => p.Id == pipelineId);
                    if (pipeline != null)
                    {
                        pipelineContact.Pipeline = pipeline;
                        pipelineContact.Step = pipeline.Steps.FirstOrDefault(s => s.Order == pipeline.Steps.Min(p => p.Order));
                    }
                    pipelineContact.Contact = dbContext.SMContacts.FirstOrDefault(c => c.Id == contactId);
                    pipelineContact.CreatedBy = user;
                    pipelineContact.LastUpdatedBy = user;
                    pipelineContact.CreatedDate = DateTime.UtcNow;
                    pipelineContact.LastUpdateDate = DateTime.UtcNow;
                    dbContext.PipelineContacts.Add(pipelineContact);
                    dbContext.Entry(pipelineContact).State = EntityState.Added;
                }

                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, pipelineContact,
                    pipelineId, contactId);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public ReturnJsonModel ChangePipelineContactToStep(int[] pipelineContactId, int stepId)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Change pipeline contact to step", null, null, pipelineContactId, stepId);

                Step step = dbContext.PipelineSteps.FirstOrDefault(s => s.Id == stepId);
                if (step != null)
                {
                    foreach (var id in pipelineContactId)
                    {
                        PipelineContact pipelineContact = dbContext.PipelineContacts.FirstOrDefault(p => p.Id == id);
                        if (pipelineContact != null)
                        {
                            pipelineContact.Step = step;
                            if (dbContext.Entry(step).State == EntityState.Detached)
                                dbContext.PipelineSteps.Attach(step);
                            dbContext.Entry(step).State = EntityState.Modified;
                            dbContext.SaveChanges();
                        }
                    }
                }

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pipelineContactId, stepId);
                refModel.msg = ex.Message;
                return new ReturnJsonModel { result = false };
            }
        }

        public ReturnJsonModel RemovePipelineContact(int[] pipelineContactId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Remove pipeline contact", null, null, pipelineContactId);

                foreach (var id in pipelineContactId)
                {
                    var pipelineContact = dbContext.PipelineContacts.FirstOrDefault(p => p.Id == id);
                    if (pipelineContact != null)
                    {
                        dbContext.PipelineContacts.Remove(pipelineContact);
                        dbContext.SaveChanges();
                    }
                }
                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pipelineContactId);
                return new ReturnJsonModel { result = true, msg = ex.Message };
            }
        }

        public List<PipelineContact> GetListPipelineContact(int pipelineId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get pipeline contact", null, null, pipelineId);

                return dbContext.PipelineContacts.Where(p => p.Pipeline.Id == pipelineId).OrderBy(p => p.Pipeline.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pipelineId);
                return new List<PipelineContact>();
            }
        }

        public List<EmailCampaignModel> GetCampaignsOfContact(int contactId, int start, int length, ref int totalRecord)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get campaign of contact", null, null, contactId, start, length, totalRecord);

                var segments = dbContext.SMContacts.FirstOrDefault(c => c.Id == contactId)?.Segments.Select(s => s.Id).ToList();
                if (segments != null)
                {
                    var query = dbContext.EmailCampaigns.Where(e => e.Segments.Any(s => segments.Contains(s.Id)));
                    totalRecord = query.Count();
                    List<EmailCampaign> lst = query.OrderByDescending(p => p.Id).Skip(start).Take(length).ToList();
                    List<EmailCampaignModel> lstMd = new List<EmailCampaignModel>();
                    foreach (var item in lst)
                    {
                        EmailCampaignModel e = new EmailCampaignModel();
                        e.Id = item.Id;
                        e.Name = item.Name;
                        e.Type = "Email Marketing";
                        lstMd.Add(e);
                    }
                    return lstMd;
                }
                else
                {
                    totalRecord = 0;
                    return new List<EmailCampaignModel>();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactId, start, length, totalRecord);
                totalRecord = 0;
                return new List<EmailCampaignModel>();
            }
        }

        public bool SaveSMQbicleTask(QbicleTask task, string assignee,
           MediaModel media, string[] watchers, int cubeId,
           string userId, int topicId, int[] activitiesRelate, List<QbicleStep> stepLst,
           QbicleRecurrance qbicleRecurrance, List<CustomDateModel> lstDate, long pipelineContactId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save task", userId, null, task, assignee,
                                    media, watchers, cubeId, topicId, activitiesRelate, stepLst, qbicleRecurrance, lstDate, pipelineContactId);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }
                var topic = new TopicRules(dbContext).GetTopicById(topicId);
                task.Topic = topic;
                var currentUser = dbContext.QbicleUser.Find(userId);
                task.StartedBy = currentUser;
                task.StartedDate = DateTime.UtcNow;
                task.State = QbicleActivity.ActivityStateEnum.Open;

                var currentQbicle = new QbicleRules(dbContext).GetQbicleById(cubeId);

                if (task.Id == 0)
                {
                    task.Qbicle = currentQbicle;
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
                        Qbicle = currentQbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = task.Topic,

                        MediaFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, cubeId),
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

                #endregion Steps

                QbicleSet set;
                if (task.Id == 0)
                {
                    set = new QbicleSet();
                    task.AssociatedSet = set;
                    task.App = QbicleActivity.ActivityApp.SalesAndMarketing;
                    dbContext.QbicleTasks.Add(task);
                    dbContext.Entry(task).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
                else
                {
                    var dbtask = dbContext.QbicleTasks.Find(task.Id);
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

                    if (dbContext.Entry(dbtask).State == EntityState.Detached)
                        dbContext.QbicleTasks.Attach(dbtask);
                    dbContext.Entry(dbtask).State = EntityState.Modified;
                    //Update LastUpdated currentDomain
                    currentQbicle.LastUpdated = DateTime.UtcNow;
                    if (dbContext.Entry(currentQbicle).State == EntityState.Detached)
                        dbContext.Qbicles.Attach(currentQbicle);
                    dbContext.Entry(currentQbicle).State = EntityState.Modified;

                    //end
                }

                //link
                if (set.Id == 0)
                {
                    dbContext.Sets.Add(set);
                    dbContext.Entry(set).State = EntityState.Added;
                }

                #region Peoples

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

                #endregion Peoples

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

                #endregion Related

                #region recurrance

                //dbContext.SaveChanges();
                var recurrance = dbContext.Recurrances.Where(s => s.AssociatedSet.Id == set.Id).ToList();
                if (task.isRecurs)
                {
                    qbicleRecurrance.Id = set.Id;
                    if (recurrance.Count > 0) dbContext.Recurrances.RemoveRange(recurrance);
                    dbContext.Recurrances.Add(qbicleRecurrance);
                    dbContext.Entry(qbicleRecurrance).State = EntityState.Added;
                    foreach (var item in lstDate)
                        if (task.ProgrammedStart != item.StartDate)
                        {
                            var task2 = new QbicleTask
                            {
                                ActualEnd = task.ActualEnd,
                                ActualStart = task.ActualStart,
                                ClosedBy = task.ClosedBy,
                                ClosedDate = task.ClosedDate,
                                Description = task.Description,
                                Duration = task.Duration,
                                DurationUnit = task.DurationUnit,
                                isComplete = task.isComplete,
                                isRecurs = task.isRecurs,
                                Name = task.Name,
                                StartedBy = task.StartedBy,
                                StartedDate = item.StartDate,
                                Topic = topic,
                                isSteps = task.isSteps,
                                Priority = task.Priority,
                                TimeLineDate = task.TimeLineDate,
                                App = QbicleActivity.ActivityApp.SalesAndMarketing,
                                IsVisibleInQbicleDashboard = false
                            };

                            task2.Qbicle = currentQbicle;
                            task2.ActivityType = QbicleActivity.ActivityTypeEnum.TaskActivity;
                            //task2.TimeLineDate = DateTime.UtcNow;
                            task2.ProgrammedStart = item.StartDate;
                            if (task2.isSteps)
                                foreach (var it in stepLst)
                                {
                                    var step = new QbicleStep
                                    {
                                        Name = it.Name,
                                        Order = it.Order,
                                        Description = it.Description,
                                        Weight = it.Weight
                                    };
                                    dbContext.Steps.Add(step);
                                    dbContext.Entry(step).State = EntityState.Added;
                                    task2.QSteps.Add(step);
                                }

                            if (!task2.ProgrammedStart.HasValue) task2.ProgrammedStart = DateTime.UtcNow;
                            if (task2.DurationUnit == QbicleTask.TaskDurationUnitEnum.Days)
                                task2.ProgrammedEnd = task2.ProgrammedStart.HasValue
                                    ? task2.ProgrammedStart.Value.AddDays(task.Duration)
                                    : DateTime.UtcNow;
                            else if (task2.DurationUnit == QbicleTask.TaskDurationUnitEnum.Hours)
                                task2.ProgrammedEnd = task2.ProgrammedStart.HasValue
                                    ? task2.ProgrammedStart.Value.AddHours(task.Duration)
                                    : DateTime.UtcNow;
                            else
                                task2.ProgrammedEnd = task2.ProgrammedStart.HasValue
                                    ? task2.ProgrammedStart.Value.AddDays(task.Duration * 7)
                                    : DateTime.UtcNow;

                            if (!string.IsNullOrEmpty(media.Name))
                            {
                                //Media attach
                                m = new QbicleMedia
                                {
                                    StartedBy = currentUser,
                                    StartedDate = DateTime.UtcNow,
                                    Name = task.Name,
                                    FileType = media.Type,
                                    Qbicle = currentQbicle,
                                    TimeLineDate = DateTime.UtcNow,
                                    Topic = task.Topic,

                                    MediaFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, cubeId),
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

                                task2.SubActivities.Add(m);
                            }

                            task2.AssociatedSet = set;
                            dbContext.QbicleTasks.Add(task2);
                            dbContext.Entry(task2).State = EntityState.Added;
                        }
                }

                #endregion recurrance

                dbContext.SaveChanges();
                PipelineContact pipelineContact = dbContext.PipelineContacts.Find(pipelineContactId);
                if (pipelineContact != null)
                {
                    pipelineContact.Tasks.Add(task);
                }
                dbContext.SaveChanges();

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
                new NotificationRules(dbContext).Notification2Activity(activityNotification);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, task, assignee,
                                    media, watchers, cubeId, topicId, activitiesRelate, stepLst, qbicleRecurrance, lstDate, pipelineContactId);
                return false;
            }
        }

        public ReturnJsonModel SaveEvent(QbicleEvent qEvent, string eventStart, int cubeId,
            string[] sendInvitesTo, int[] activitiesRelate, MediaModel media, string userId, int topicId, QbicleRecurrance qbicleRecurance, List<CustomDateModel> lstDate, int pipelineContactId, string originatingConnectionId = "")
        {
            if (eventStart == null) throw new ArgumentNullException(nameof(eventStart));

            var result = new ReturnJsonModel();
            var currentInvite = new QbiclePeople();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save event", userId, null, qEvent, eventStart, cubeId,
                    sendInvitesTo, activitiesRelate, media, topicId, qbicleRecurance, lstDate, pipelineContactId);

                var currentQbicle = new QbicleRules(dbContext).GetQbicleById(cubeId);
                var topic = new TopicRules(dbContext).GetTopicById(topicId);
                var currentUser = dbContext.QbicleUser.Find(userId);
                qEvent.Topic = topic;
                qEvent.StartedBy = currentUser;
                qEvent.StartedDate = DateTime.UtcNow;
                qEvent.State = QbicleActivity.ActivityStateEnum.Open;
                if (qEvent.Id == 0)
                {
                    currentQbicle.LastUpdated = DateTime.UtcNow;
                    qEvent.Qbicle = currentQbicle;
                }
                qEvent.ActivityType = QbicleActivity.ActivityTypeEnum.EventActivity;
                var dateStart = qEvent.Start;
                switch (qEvent.DurationUnit)
                {
                    case QbicleEvent.EventDurationUnitEnum.Days:
                        qEvent.End = dateStart.AddDays(qEvent.Duration);
                        break;

                    case QbicleEvent.EventDurationUnitEnum.Hours:
                        qEvent.End = dateStart.AddHours(qEvent.Duration);
                        break;

                    default:
                        qEvent.End = dateStart.AddDays(qEvent.Duration * 7);
                        break;
                }

                qEvent.TimeLineDate = DateTime.UtcNow;
                qEvent.ProgrammedStart = qEvent.Start;
                qEvent.ProgrammedEnd = qEvent.End;
                QbicleMedia m = null;
                if (!string.IsNullOrEmpty(media.Name))
                {
                    if (!string.IsNullOrEmpty(media.UrlGuid))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);
                        s3Rules.ProcessingMediaS3(media.UrlGuid);
                    }
                    //Media attach
                    m = new QbicleMedia
                    {
                        StartedBy = currentUser,
                        StartedDate = DateTime.UtcNow,
                        Name = qEvent.Name,
                        Description = qEvent.Description,
                        FileType = media.Type,
                        Qbicle = currentQbicle,
                        TimeLineDate = DateTime.UtcNow,
                        MediaFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, cubeId),
                        Topic = qEvent.Topic,
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
                    qEvent.SubActivities.Add(m);
                }

                QbicleSet qbicleSet = null;
                if (qEvent.Id == 0)
                {
                    qbicleSet = new QbicleSet();
                    qEvent.AssociatedSet = qbicleSet;
                    dbContext.Events.Add(qEvent);
                    dbContext.Entry(qEvent).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
                else
                {
                    var dbEvent = dbContext.Events.Find(qEvent.Id);
                    if (dbEvent != null)
                    {
                        dbEvent.Name = qEvent.Name;
                        dbEvent.Description = qEvent.Description;
                        dbEvent.Topic = qEvent.Topic;
                        dbEvent.EventType = qEvent.EventType;
                        dbEvent.Location = qEvent.Location;
                        dbEvent.Start = qEvent.Start;
                        dbEvent.End = qEvent.End;
                        dbEvent.Duration = qEvent.Duration;
                        dbEvent.DurationUnit = qEvent.DurationUnit;
                        dbEvent.ProgrammedStart = qEvent.Start;
                        dbEvent.ProgrammedEnd = qEvent.End;
                        dbEvent.isRecurs = qEvent.isRecurs;
                        qEvent.TimeLineDate = DateTime.UtcNow;
                        qEvent.IsVisibleInQbicleDashboard = true;
                        if (m != null)
                            dbEvent.SubActivities.Add(m);
                        if (dbEvent.AssociatedSet != null)
                        {
                            qbicleSet = dbEvent.AssociatedSet;
                        }
                        else
                        {
                            qbicleSet = new QbicleSet();
                            dbContext.Sets.Add(qbicleSet);
                            dbContext.Entry(qbicleSet).State = EntityState.Added;
                            dbContext.SaveChanges();
                            dbEvent.AssociatedSet = qbicleSet;
                        }

                        if (dbContext.Entry(dbEvent).State == EntityState.Detached)
                            dbContext.Events.Attach(dbEvent);
                        dbContext.Entry(dbEvent).State = EntityState.Modified;
                    }

                    currentQbicle.LastUpdated = DateTime.UtcNow;
                    if (dbContext.Entry(currentQbicle).State == EntityState.Detached)
                        dbContext.Qbicles.Attach(currentQbicle);
                    dbContext.Entry(currentQbicle).State = EntityState.Modified;
                    //end
                }

                #region People

                //Remove Watchers Old
                var peoplesInvite = dbContext.People.Where(s => s.AssociatedSet.Id == qbicleSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Invitee).ToList();
                if (peoplesInvite.Count > 0)
                {
                    dbContext.People.RemoveRange(peoplesInvite);
                }
                //Add current User
                currentInvite.isPresent = true;
                currentInvite.Type = QbiclePeople.PeopleTypeEnum.Invitee;
                currentInvite.User = currentUser;
                currentInvite.AssociatedSet = qbicleSet;
                dbContext.People.Add(currentInvite);
                dbContext.Entry(currentInvite).State = EntityState.Added;
                //end
                if (sendInvitesTo != null && sendInvitesTo.Any())
                {
                    foreach (var item in sendInvitesTo.Where(s => s != currentUser.Id))
                    {
                        var peopleInvite = new QbiclePeople
                        {
                            isPresent = true,
                            Type = QbiclePeople.PeopleTypeEnum.Invitee
                        };
                        var user = dbContext.QbicleUser.Find(item);
                        if (user == null) continue;
                        peopleInvite.User = user;
                        peopleInvite.AssociatedSet = qbicleSet;
                        dbContext.People.Add(peopleInvite);
                        dbContext.Entry(peopleInvite).State = EntityState.Added;
                    }
                }

                #endregion People

                #region Related

                var relates = dbContext.Relateds.Where(s => s.AssociatedSet.Id == qbicleSet.Id).ToList();
                if (relates.Count > 0)
                {
                    dbContext.Relateds.RemoveRange(relates);
                }
                if (activitiesRelate != null && activitiesRelate.Length > 0)
                {
                    foreach (var item in activitiesRelate)
                    {
                        var activity = dbContext.Activities.Find(item);
                        if (activity == null) continue;
                        var rl = new QbicleRelated { AssociatedSet = qbicleSet, Activity = activity };
                        dbContext.Relateds.Add(rl);
                        dbContext.Entry(rl).State = EntityState.Added;
                    }
                }

                #endregion Related

                #region recurrance

                if (qEvent.isRecurs)
                {
                    if (qbicleSet != null)
                    {
                        qbicleRecurance.Id = qbicleSet.Id;
                        var recurrance = dbContext.Recurrances.Where(s => s.AssociatedSet.Id == qbicleSet.Id).ToList();
                        if (recurrance.Count > 0)
                        {
                            dbContext.Recurrances.RemoveRange(recurrance);
                        }

                        dbContext.Recurrances.Add(qbicleRecurance);
                        dbContext.Entry(qbicleRecurance).State = EntityState.Added;
                        foreach (var item in lstDate)
                        {
                            if (qEvent.ProgrammedStart == item.StartDate) continue;
                            var qEvent2 = new QbicleEvent
                            {
                                End = qEvent.End,
                                ActualEnd = qEvent.ActualEnd,
                                ActualStart = qEvent.ActualStart,
                                Description = qEvent.Description,
                                Duration = qEvent.Duration,
                                DurationUnit = qEvent.DurationUnit,
                                EventType = qEvent.EventType,
                                isRecurs = qEvent.isRecurs,
                                Location = qEvent.Location,
                                Name = qEvent.Name,
                                Start = item.StartDate,
                                StartedBy = qEvent.StartedBy,
                                StartedDate = item.StartDate,
                                State = qEvent.State,
                                Topic = topic,
                                Qbicle = qEvent.Qbicle,
                                TimeLineDate = qEvent.TimeLineDate,
                                ActivityType = QbicleActivity.ActivityTypeEnum.EventActivity,
                                IsVisibleInQbicleDashboard = false
                            };

                            dateStart = item.StartDate;
                            switch (qEvent2.DurationUnit)
                            {
                                case QbicleEvent.EventDurationUnitEnum.Days:
                                    qEvent2.End = dateStart.AddDays(qEvent.Duration);
                                    break;

                                case QbicleEvent.EventDurationUnitEnum.Hours:
                                    qEvent2.End = dateStart.AddHours(qEvent.Duration);
                                    break;

                                default:
                                    qEvent2.End = dateStart.AddDays(qEvent.Duration * 7);
                                    break;
                            }

                            qEvent2.ProgrammedStart = qEvent2.Start;
                            qEvent2.ProgrammedEnd = qEvent2.End;
                            if (!string.IsNullOrEmpty(media.Name))
                            {
                                //Media attach
                                m = new QbicleMedia
                                {
                                    StartedBy = currentUser,
                                    StartedDate = DateTime.UtcNow,
                                    Name = qEvent.Name,
                                    Description = qEvent.Description,
                                    FileType = media.Type,
                                    Qbicle = currentQbicle,
                                    TimeLineDate = DateTime.UtcNow,
                                    MediaFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, cubeId),
                                    Topic = qEvent.Topic,
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
                                qEvent2.SubActivities.Add(m);
                            }

                            qEvent2.AssociatedSet = qbicleSet;
                            dbContext.Events.Add(qEvent2);
                            dbContext.Entry(qEvent2).State = EntityState.Added;
                        }
                    }
                }

                #endregion recurrance

                dbContext.SaveChanges();
                PipelineContact pipelineContact = dbContext.PipelineContacts.Find(pipelineContactId);
                if (pipelineContact != null)
                {
                    pipelineContact.Events.Add(qEvent);
                }
                dbContext.SaveChanges();
                result.result = true;
                result.Object = new { topic = new { topic.Id, topic.Name } };

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = qEvent.Id,
                    EventNotify = NotificationEventEnum.EventCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = currentUser.GetFullName(),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2Activity(activityNotification);

                return result;
            }
            catch (Exception ex)
            {
                result.result = false;
                result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, qEvent, eventStart, cubeId,
                    sendInvitesTo, activitiesRelate, media, topicId, qbicleRecurance, lstDate, pipelineContactId);
                return result;
            }
        }

        public List<PipelineTasksModel> GetPipelineTasks(int pipelineContactId, int start, int length, ref int totalRecord, string dateFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get pipeline task", null, null, pipelineContactId, start, length, totalRecord, dateFormat);

                PipelineContact pipelineContact = dbContext.PipelineContacts.Find(pipelineContactId);
                totalRecord = pipelineContact.Tasks.Count();
                List<PipelineTasksModel> lstModel = new List<PipelineTasksModel>();
                List<QbicleTask> lstQbicleTask = pipelineContact.Tasks.OrderByDescending(t => t.Id).Skip(start).Take(length).ToList();

                foreach (QbicleTask task in lstQbicleTask)
                {
                    PipelineTasksModel model = new PipelineTasksModel();
                    model.Id = task.Id;
                    model.Title = task.Name;
                    model.Summary = task.Description;
                    if (!task.isComplete && task.ActualStart == null && task.ProgrammedEnd >= DateTime.UtcNow)
                    { model.Status = "Pending"; }
                    else if (!task.isComplete && task.ActualStart != null && task.ProgrammedEnd >= DateTime.UtcNow)
                    { model.Status = "In progress"; }
                    else if (!task.isComplete && task.ProgrammedEnd < DateTime.UtcNow)
                    { model.Status = "Overdue"; }
                    else if (task.isComplete)
                    { model.Status = "Complete"; }

                    if (task.ProgrammedStart != null)
                    {
                        model.Deadline = task.ProgrammedStart?.ToString(dateFormat) + " " + task.ProgrammedStart?.ToString("hh:mmtt").ToLower();
                    }

                    lstModel.Add(model);
                }

                return lstModel.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pipelineContactId, start, length, totalRecord, dateFormat);
                totalRecord = 0;
                return new List<PipelineTasksModel>();
            }
        }

        public List<PipelineEventsModel> GetPipelineEvents(int pipelineContactId, int start, int length, ref int totalRecord, string dateFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get pipeline event", null, null, pipelineContactId, start, length, totalRecord, dateFormat);

                PipelineContact pipelineContact = dbContext.PipelineContacts.Find(pipelineContactId);
                totalRecord = pipelineContact.Events.Count();
                List<PipelineEventsModel> lstModel = new List<PipelineEventsModel>();
                List<QbicleEvent> lstQbicleEvent = pipelineContact.Events.OrderByDescending(t => t.Id).Skip(start).Take(length).ToList();

                foreach (QbicleEvent evn in lstQbicleEvent)
                {
                    PipelineEventsModel model = new PipelineEventsModel();
                    model.Id = evn.Id;
                    model.Title = evn.Name;
                    if (evn.StartedDate != null)
                    {
                        model.StartDate = evn.ProgrammedStart?.ToString(dateFormat) + " " + evn.ProgrammedStart?.ToString("hh:mmtt").ToLower();
                    }
                    lstModel.Add(model);
                }

                return lstModel.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pipelineContactId, start, length, totalRecord, dateFormat);
                totalRecord = 0;
                return new List<PipelineEventsModel>();
            }
        }

        public async Task<ReturnJsonModel> GetZipFileAsync(int id, string type)
        {
            try
            {
                var campaignRule = new CampaignRules(dbContext);
                var post = new SocialCampaignPost();
                if (type.Equals("approved"))
                {
                    var approvedPost = campaignRule.CampaignPostApprovalById(id);
                    post = approvedPost.CampaignPost;
                }
                else
                {
                    var queuePost = campaignRule.GetSocialCampaignQueue(id);
                    post = queuePost.Post;
                }

                string fileId = post.ImageOrVideo.VersionedFiles.FirstOrDefault().Uri;
                var zipName = post.Title.Replace(" ", "-") + post.AssociatedCampaign.Domain.Name.Replace(" ", "-");

                var downloadFolder = Path.Combine(ConfigManager.TempPathRepository, fileId + "\\" + zipName);// + ".zip") ;

                if (!Directory.Exists(downloadFolder))
                    Directory.CreateDirectory(downloadFolder);

                var fileStorageInformation = dbContext.StorageFiles.Find(fileId);

                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(fileId);

                var fullPathMedia = Path.Combine(downloadFolder, fileStorageInformation.Name);
                using (FileStream outputFileStream = new FileStream(fullPathMedia, FileMode.Create))
                {
                    s3Object.ObjectStream.CopyTo(outputFileStream);
                }

                string contentFile = Path.Combine(downloadFolder, "content.txt");
                using (StreamWriter outputFile = new StreamWriter(contentFile))
                {
                    outputFile.WriteLine(post.Content);
                }

                string zipFile = $"{downloadFolder}.zip";

                if (File.Exists(zipFile))
                {
                    File.Delete(zipFile);
                }
                ZipFile.CreateFromDirectory(downloadFolder, zipFile);

                //var zipKey = Guid.NewGuid().ToString();
                var mediaProcess = new MediaProcess
                {
                    FileName = $"{zipName}.zip",
                    ObjectKey = zipName,
                    FilePath = zipFile
                };

                await (new AzureStorageRules(dbContext).UploadMediaFromPathByHangfireAsync(mediaProcess));

                //device.ProductFile = mediaProcess.ObjectKey;

                File.Delete(zipFile);
                Directory.Delete(downloadFolder, true);
                var refModel = new ReturnJsonModel() { result = true, Object = zipName };
                //refModel.Object = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + ConfigurationManager.AppSettings["CampaignDownload"].Replace("\\", "/") + "/" + userSetting.UserName + "/" + post.Title + ".zip";
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                var refModel = new ReturnJsonModel() { result = false };
                return refModel;
            }
        }
    }
}