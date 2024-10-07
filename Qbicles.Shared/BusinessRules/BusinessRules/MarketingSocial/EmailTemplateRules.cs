using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.BusinessRules.MarketingSocial
{
    public class EmailTemplateRules
    {
        ApplicationDbContext dbContext;
        public EmailTemplateRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public EmailTemplate GetEmailTemplateById(int id)
        {
            return dbContext.EmailTemplates.Find(id);
        }
        public EmailTemplateModel GetEmailTemplateJsonById(int id)
        {
            var model = dbContext.EmailTemplates.Find(id);
            var emailTemplate = new EmailTemplateModel();
            if (model != null)
            {
                #region Map model
                emailTemplate.Id = model.Id;
                emailTemplate.TemplateName = model.TemplateName;
                emailTemplate.TemplateDescription = model.TemplateDescription;
                emailTemplate.HeadingBg = model.HeadingBg;
                emailTemplate.HeadlineText = model.HeadlineText;
                emailTemplate.HeadlineColour = model.HeadlineColour;
                emailTemplate.HeadlineFont = model.HeadlineFont;
                emailTemplate.HeadlineFontSize = model.HeadlineFontSize;
                emailTemplate.BodyBg = model.BodyBg;
                emailTemplate.BodyTextColour = model.BodyTextColour;
                emailTemplate.BodyContent = model.BodyContent;
                emailTemplate.BodyFont = model.BodyFont;
                emailTemplate.BodyFontSize = model.BodyFontSize;
                emailTemplate.ButtonIsHidden = model.ButtonIsHidden;
                emailTemplate.ButtonText = model.ButtonText;
                emailTemplate.ButtonTextColour = model.ButtonTextColour;
                emailTemplate.ButtonLink = model.ButtonLink;
                emailTemplate.ButtonBg = model.ButtonBg;
                emailTemplate.ButtonFont = model.ButtonFont;
                emailTemplate.ButtonFontSize = model.ButtonFontSize;
                emailTemplate.AdvertImgiIsHidden = model.AdvertImgiIsHidden;
                emailTemplate.AdvertLink = model.AdvertLink;
                emailTemplate.FacebookLink = model.FacebookLink;
                emailTemplate.InstagramLink = model.InstagramLink;
                emailTemplate.LinkedInLink = model.LinkedInLink;
                emailTemplate.PinterestLink = model.PinterestLink;
                emailTemplate.TwitterLink = model.TwitterLink;
                emailTemplate.YoutubeLink = model.YoutubeLink;
                emailTemplate.IsHiddenFacebook = model.IsHiddenFacebook;
                emailTemplate.IsHiddenInstagram = model.IsHiddenInstagram;
                emailTemplate.IsHiddenLinkedIn = model.IsHiddenLinkedIn;
                emailTemplate.IsHiddenPinterest = model.IsHiddenPinterest;
                emailTemplate.IsHiddenTwitter = model.IsHiddenTwitter;
                emailTemplate.IsHiddenYoutube = model.IsHiddenYoutube;
                emailTemplate.FeaturedImage = model.FeaturedImage;
                emailTemplate.AdvertImage = model.AdvertImage;
                #endregion
            }
            return emailTemplate;
        }
        public ReturnJsonModel DeleteEmailTemplateById(int id)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save Email Template", null, null, id);
                var model = dbContext.EmailTemplates.Find(id);
                if (model != null)
                {
                    if (dbContext.CampaignEmails.Any(s => s.Template.Id == model.Id))
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_DATA_IN_USED_CAN_NOT_DELETE", model.TemplateName);
                        return returnJson;
                    }
                    dbContext.EmailTemplates.Remove(model);
                    returnJson.result = dbContext.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnJson;
        }
        public ReturnJsonModel SaveEmailTemplate(EmailTemplate email, MediaModel mediaModelFeatured, MediaModel mediaModelAdv, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save Email Template", null, null, email, mediaModelFeatured, mediaModelAdv);
                var setting = dbContext.SalesMarketingSettings.FirstOrDefault(s => s.Domain.Id == email.Domain.Id);
                if (setting == null)
                {
                    returnJson.msg = "ERROR_MSG_153";
                    return returnJson;
                }


                if (!string.IsNullOrEmpty(mediaModelFeatured?.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(mediaModelFeatured.UrlGuid);
                }

                if (!string.IsNullOrEmpty(mediaModelAdv?.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);
                    s3Rules.ProcessingMediaS3(mediaModelAdv.UrlGuid);
                }

                var user = dbContext.QbicleUser.Find(userId);

                var folder = dbContext.MediaFolders.FirstOrDefault(s => s.Name == HelperClass.GeneralName && s.Qbicle.Id == setting.SourceQbicle.Id);
                if (folder == null)
                {
                    folder = new MediaFolder
                    {
                        Name = HelperClass.GeneralName,
                        Qbicle = setting.SourceQbicle,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = user
                    };
                    dbContext.MediaFolders.Add(folder);
                    dbContext.Entry(folder).State = EntityState.Added;
                }
                var emailTemplate = dbContext.EmailTemplates.Find(email.Id);
                if (emailTemplate != null)
                {
                    #region Map model
                    emailTemplate.TemplateName = email.TemplateName;
                    emailTemplate.TemplateDescription = email.TemplateDescription;
                    emailTemplate.HeadingBg = email.HeadingBg;
                    emailTemplate.HeadlineText = email.HeadlineText;
                    emailTemplate.HeadlineColour = email.HeadlineColour;
                    emailTemplate.HeadlineFont = email.HeadlineFont?.Replace("+", " ");
                    emailTemplate.HeadlineFontSize = email.HeadlineFontSize;
                    emailTemplate.BodyBg = email.BodyBg;
                    emailTemplate.BodyTextColour = email.BodyTextColour;
                    emailTemplate.BodyContent = email.BodyContent;
                    emailTemplate.BodyFont = email.BodyFont?.Replace("+", " ");
                    emailTemplate.BodyFontSize = email.BodyFontSize;
                    emailTemplate.ButtonIsHidden = email.ButtonIsHidden;
                    emailTemplate.ButtonText = email.ButtonText;
                    emailTemplate.ButtonTextColour = email.ButtonTextColour;
                    emailTemplate.ButtonLink = email.ButtonLink;
                    emailTemplate.ButtonBg = email.ButtonBg;
                    emailTemplate.ButtonFont = email.ButtonFont?.Replace("+", " ");
                    emailTemplate.ButtonFontSize = email.ButtonFontSize;
                    emailTemplate.AdvertImgiIsHidden = email.AdvertImgiIsHidden;
                    emailTemplate.AdvertLink = email.AdvertLink;
                    emailTemplate.FacebookLink = email.FacebookLink;
                    emailTemplate.InstagramLink = email.InstagramLink;
                    emailTemplate.LinkedInLink = email.LinkedInLink;
                    emailTemplate.PinterestLink = email.PinterestLink;
                    emailTemplate.TwitterLink = email.TwitterLink;
                    emailTemplate.YoutubeLink = email.YoutubeLink;
                    emailTemplate.IsHiddenFacebook = email.IsHiddenFacebook;
                    emailTemplate.IsHiddenInstagram = email.IsHiddenInstagram;
                    emailTemplate.IsHiddenLinkedIn = email.IsHiddenLinkedIn;
                    emailTemplate.IsHiddenPinterest = email.IsHiddenPinterest;
                    emailTemplate.IsHiddenTwitter = email.IsHiddenTwitter;
                    emailTemplate.IsHiddenYoutube = email.IsHiddenYoutube;
                    if (mediaModelFeatured != null)
                    {
                        AddMediaQbicle(mediaModelFeatured, user, setting.SourceQbicle, folder, email.TemplateName, email.TemplateDescription, setting.DefaultTopic);
                        emailTemplate.FeaturedImage = mediaModelFeatured.UrlGuid;
                    }
                    else
                        emailTemplate.FeaturedImage = email.FeaturedImage;
                    if (mediaModelAdv != null)
                    {
                        AddMediaQbicle(mediaModelAdv, user, setting.SourceQbicle, folder, email.TemplateName, email.TemplateDescription, setting.DefaultTopic);
                        emailTemplate.AdvertImage = mediaModelAdv.UrlGuid;
                    }
                    else
                        emailTemplate.AdvertImage = email.AdvertImage;
                    #endregion
                    if (dbContext.Entry(emailTemplate).State == EntityState.Detached)
                        dbContext.EmailTemplates.Attach(emailTemplate);
                    dbContext.Entry(emailTemplate).State = EntityState.Modified;
                    returnJson.result = dbContext.SaveChanges() > 0;
                }
                else
                {
                    emailTemplate = email;

                    emailTemplate.CreateBy = user;
                    emailTemplate.CreateDate = DateTime.UtcNow;
                    emailTemplate.HeadlineFont = emailTemplate.HeadlineFont?.Replace("+", " ");
                    emailTemplate.BodyFont = emailTemplate.BodyFont?.Replace("+", " ");
                    emailTemplate.ButtonFont = emailTemplate.ButtonFont?.Replace("+", " ");
                    if (mediaModelFeatured != null)
                    {
                        AddMediaQbicle(mediaModelFeatured, user, setting.SourceQbicle, folder, email.TemplateName, email.TemplateDescription, setting.DefaultTopic);
                        emailTemplate.FeaturedImage = mediaModelFeatured.UrlGuid;
                    }
                    if (mediaModelAdv != null)
                    {
                        AddMediaQbicle(mediaModelAdv, user, setting.SourceQbicle, folder, email.TemplateName, email.TemplateDescription, setting.DefaultTopic);
                        emailTemplate.AdvertImage = mediaModelAdv.UrlGuid;
                    }

                    dbContext.EmailTemplates.Add(emailTemplate);
                    returnJson.result = dbContext.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, email, mediaModelFeatured, mediaModelAdv);
            }
            return returnJson;
        }
        public List<EmailTemplate> GetEmailTemplates(int domainId)
        {
            try
            {
                return dbContext.EmailTemplates.Where(s => s.Domain.Id == domainId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<EmailTemplate>();
            }
        }
        public DataTablesResponse GetEmailTemplates([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int domainId, string timezone, string dateformat)
        {
            try
            {
                var query = dbContext.EmailTemplates.Where(t => t.Domain.Id == domainId).AsQueryable();
                int totalcount = 0;
                #region Filter
                totalcount = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "TemplateName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TemplateName" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "TemplateDescription":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TemplateDescription" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreateDate" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreateDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.TemplateName,
                    q.TemplateDescription,
                    CreateBy = HelperClass.GetFullNameOfUser(q.CreateBy),
                    CreateById = q.CreateBy.Id,
                    CreateDate = q.CreateDate.ConvertTimeFromUtc(timezone).ToString(dateformat)
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        private QbicleMedia AddMediaQbicle(MediaModel media, ApplicationUser user, Qbicle qbicle, MediaFolder folder, string name, string descript, Topic topic)
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
    }
}
