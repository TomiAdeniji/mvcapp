using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroMediasRules : MicroRulesBase
    {
        public MicroMediasRules(MicroContext microContext) : base(microContext)
        {
        }

        public MicroActivityMedias GetActivityMedias(int activityId, int pageSize)
        {
            string timezone = CurrentUser.Timezone; string dateFormat = CurrentUser.DateFormat; string timeFormat = CurrentUser.TimeFormat;
            var dateTimeFormat = $"{dateFormat} {timeFormat}";
            var medias = dbContext.Activities.Find(activityId)?.SubActivities;
            pageSize *= HelperClass.activitiesPageSize;

            var microMedias = new MicroActivityMedias
            {
                Total = (medias.Count / HelperClass.activitiesPageSize) + (medias.Count % HelperClass.activitiesPageSize == 0 ? 0 : 1)
            };

            medias = medias.OrderByDescending(d => d.TimeLineDate).Skip(pageSize).Take(HelperClass.activitiesPageSize).ToList();

            medias.ForEach(m =>
            {
                var media = (QbicleMedia)m;
                var createdDate = media.StartedDate.Date == DateTime.UtcNow.Date ? "Today, " + media.StartedDate.ConvertTimeFromUtc(timezone).ToString(timeFormat) : media.StartedDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat);

                var mediaLastupdate = media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).First();
                var lastUpdateFile = mediaLastupdate != null ? (mediaLastupdate.UploadedDate.Date == DateTime.UtcNow.Date ? "Today, " + mediaLastupdate.UploadedDate.ConvertTimeFromUtc(timezone).ToString(timeFormat) : mediaLastupdate.UploadedDate.ConvertTimeFromUtc(timezone).ToString(dateTimeFormat)) : createdDate;

                microMedias.Medias.Add(new MicroActivityMedia
                {
                    Id = media.Id,
                    Key = media.Key,
                    Name = media.Name,
                    Description = media.Description,
                    CreatedBy = media.StartedBy.GetFullName(),
                    CreatedDate = createdDate,
                    CreatedByImage = media.StartedBy.ProfilePic.ToUri(),
                    LastUpdate = $"{media.FileType?.Type} | Update {lastUpdateFile}",
                    FileType = new MicroFileType
                    {
                        Extension = media.FileType?.Extension,
                        Type = media.FileType?.Type
                    },
                    MediaUri = mediaLastupdate?.Uri.ToUri((media.FileType?.Type ?? "").GetFileTypeEnum()),
                    Topic = media.Topic?.Name,
                    Folder = media.MediaFolder?.Name
                });

            });
            return microMedias;
        }

        public ReturnJsonModel AddActivityMedia(MicroMediaUpload microMedia, bool isCreatorTheCustomer)
        {
            try
            {
                var rules = new MediasRules(dbContext);
                if (microMedia.Name.Length > 250)
                    return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_389", 250) };
                if (microMedia.Description != null && microMedia.Description.Length > 500)
                    return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_389", 500) };
                if (rules.DuplicateMediaNameCheck(0, microMedia.Name, microMedia.QbicleId))
                    return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MEDIANAME_EXISTS") };

                var extension = HelperClass.GetFileExtension(microMedia.Detail.FileName);

                var fileType = new FileTypeRules(dbContext).GetFileTypeByExtension(extension == "" ? microMedia.Detail.FileType : extension);
                var versionFile = new VersionedFile
                {
                    Uri = microMedia.Detail.FileKey,
                    FileSize = HelperClass.FileSize(int.Parse(microMedia.Detail.FileSize)),
                    FileType = fileType
                };

                var media = new QbicleMedia
                {
                    Id = microMedia.Id,
                    Name = microMedia.Name,
                    Description = microMedia.Description,
                    FileType = fileType,
                    Qbicle = new QbicleRules(dbContext).GetQbicleById(microMedia.QbicleId)
                };


                switch (microMedia.ActivityType)
                {
                    case StreamType.Discussion:
                        return rules.SaveMedia(media, isCreatorTheCustomer, CurrentUser.Id, false,
                            microMedia.ActivityId, 0, 0, 0, 0, 0, 0, microMedia.TopicName, versionFile, microMedia.FolderId, null, null, null, null, null, microMedia.OriginatingConnectionId);
                    case StreamType.Event:
                        return rules.SaveMedia(media, isCreatorTheCustomer, CurrentUser.Id, false,
                            0, 0, microMedia.ActivityId, 0, 0, 0, 0, microMedia.TopicName, versionFile, microMedia.FolderId, null, null, null, null, null, microMedia.OriginatingConnectionId);
                    case StreamType.Task:
                        return rules.SaveMedia(media, isCreatorTheCustomer, CurrentUser.Id, false,
                            0, microMedia.ActivityId, 0, 0, 0, 0, 0, microMedia.TopicName, versionFile, microMedia.FolderId, null, null, null, null, null, microMedia.OriginatingConnectionId);
                    case StreamType.Approval:
                        return rules.SaveMedia(media, isCreatorTheCustomer, CurrentUser.Id, false,
                            0, 0, 0, 0, microMedia.ActivityId, 0, 0, microMedia.TopicName, versionFile, microMedia.FolderId, null, null, null, null, null, microMedia.OriginatingConnectionId);
                    case StreamType.Link:
                        return rules.SaveMedia(media, isCreatorTheCustomer, CurrentUser.Id, false,
                            0, 0, 0, 0, 0, microMedia.ActivityId, 0, microMedia.TopicName, versionFile, microMedia.FolderId, null, null, null, null, null, microMedia.OriginatingConnectionId);
                    case StreamType.Medias:
                        return rules.SaveMedia(media, isCreatorTheCustomer, CurrentUser.Id, false,
                            0, 0, 0, 0, 0, 0, microMedia.ActivityId, microMedia.TopicName, versionFile, microMedia.FolderId, null, null, null, null, null, microMedia.OriginatingConnectionId);
                    default://media
                        return rules.SaveMedia(media, isCreatorTheCustomer, CurrentUser.Id, false,
                            0, 0, 0, 0, 0, 0, 0, microMedia.TopicName, versionFile, microMedia.FolderId, null, null, null, null, null, microMedia.OriginatingConnectionId);
                }
                //return new ReturnJsonModel { result = false };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, microMedia);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }

        }

        public ReturnJsonModel UpdateActivityMedia(MicroMediaUpload microMedia, bool isCreatorTheCustomer)
        {
            try
            {
                var rules = new MediasRules(dbContext);

                if (microMedia.Name.Length > 250)
                    return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_389", 250) };
                if (microMedia.Description != null && microMedia.Description.Length > 500)
                    return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_389", 500) };
                if (rules.DuplicateMediaNameCheck(microMedia.Id, microMedia.Name, microMedia.QbicleId))
                    return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MEDIANAME_EXISTS") };

                var media = new QbicleMedia
                {
                    Id = microMedia.Id,
                    Name = microMedia.Name,
                    Description = microMedia.Description,
                    Topic = new TopicRules(dbContext).GetTopicByName(microMedia.TopicName, microMedia.QbicleId)
                };
                return new MediasRules(dbContext).UpdateMedia(media, isCreatorTheCustomer, new UserSetting { Id = CurrentUser.Id, DisplayName = CurrentUser.GetFullName() });

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, microMedia);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }


        public MicroMediaView GetMedia(int id)
        {
            var media = new MediasRules(dbContext).GetMediaById(id).BusinessMapping(CurrentUser.Timezone);

            var versions = media.VersionedFiles.Where(e => !e.IsDeleted).BusinessMapping(CurrentUser.Timezone).OrderByDescending(x => x.UploadedDate).ToList();

            var createdDate = media.StartedDate.Date == DateTime.UtcNow.Date ? "Today, " + media.StartedDate.ToString(CurrentUser.TimeFormat)
                : media.StartedDate.ToString($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}");

            var mediaLastupdate = media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).First();
            var lastUpdateFile = mediaLastupdate != null ?
                (mediaLastupdate.UploadedDate.Date == DateTime.UtcNow.Date ? "Today, " + mediaLastupdate.UploadedDate.ToString($"{CurrentUser.TimeFormat}")
                : mediaLastupdate.UploadedDate.ToString($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}")) : createdDate;

            var microMedia = new MicroMediaView
            {
                Id = media.Id,
                Key = media.Key,
                Name = media.Name,
                Description = media.Description,
                CreatedBy = media.StartedBy.GetFullName(),
                CreatedDate = createdDate,
                CreatedByImage = media.StartedBy.ProfilePic.ToUri(),
                LastUpdate = $"{media.FileType?.Type} | Update {lastUpdateFile}",
                FileType = new MicroFileType
                {
                    Extension = media.FileType?.Extension,
                    Type = mediaLastupdate.FileType?.Type ?? "",
                    Size = mediaLastupdate.FileSize
                },
                MediaUri = mediaLastupdate?.Uri.ToUri(),
                Topic = media.Topic?.Name,
                Folder = media.MediaFolder?.Name,
                Foldedrs = new MediaFolderRules(dbContext).GetMediaFoldersByQbicleId(media.Qbicle.Id, "").Select(f => new BaseModel { Id = f.Id, Name = f.Name }).ToList(),
                Topics = new TopicRules(dbContext).GetTopicByQbicle(media.Qbicle.Id).Select(t => new BaseModel { Id = t.Id, Name = t.Name }).ToList(),
                VersionedFiles = versions.Select(version => new MicroVersionedFile
                {
                    Id = version.Id,
                    FileSize = version.FileSize,
                    FileType = new MicroFileType { Extension = version.FileType?.Extension, Type = version.FileType?.Type, Size = version?.FileSize },
                    UploadedBy = version.UploadedBy.GetFullName(),
                    UploadedDate = version.UploadedDate.ConvertTimeFromUtc(CurrentUser.Timezone),
                    Uri = version.Uri.ToUri((version.FileType?.Type ?? "").GetFileTypeEnum()),
                    FileKey = version.Uri
                }).ToList(),
                VersionsOption = versions.Select(v => new BaseModel
                {
                    Id = v.Id,
                    Name = v.UploadedDate.ToString("MMMM dd,yyyy HH:mm")
                }).ToList()
            };



            return microMedia;
        }

        public ReturnJsonModel MediaDeleteVersion(int id, bool isCreatorTheCustomer)
        {
            return new MediasRules(dbContext).DeleteVersionFile(isCreatorTheCustomer, id, CurrentUser.Id);
        }

        public ReturnJsonModel MediaVersionDownload(string uri)
        {
            try
            {
                var fileinfo = new MediasRules(dbContext).GetFileInfoByURI(uri);
                var fileString = AzureStorageHelper.SignedUrl(uri, fileinfo?.Name ?? "");
                return new ReturnJsonModel { result = true, msg = fileString };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }

        }

        public ReturnJsonModel MediaVersionAdd(MicroMediaUpload microMedia, bool isCreatorTheCustomer)
        {
            var media = new MediaModel
            {
                UrlGuid = microMedia.Detail.FileKey,
                Name = microMedia.Detail.FileName,
                Size = HelperClass.FileSize(int.Parse(microMedia.Detail.FileSize == "" ? "0" : microMedia.Detail.FileSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(System.IO.Path.GetExtension(microMedia.Detail.FileName))
            };


            var version = new MediasRules(dbContext).SaveVersionFile(isCreatorTheCustomer,
                microMedia.Id, CurrentUser.Id, CurrentUser.Timezone, media);
            if (version != null)
                return new ReturnJsonModel { result = true, actionVal = version.Id };
            return new ReturnJsonModel { result = false };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type">sale/purchse/transfer</param>
        /// <returns></returns>
        public object GetTopicFolders(string key, string type)
        {
            var id = key.Decrypt2Int();
            var qbicleId = 0;
            switch (type)
            {
                case "sale":
                    qbicleId = dbContext.TraderSales.FirstOrDefault(e => e.Id == id).Workgroup.Qbicle.Id;
                    break;

                case "purchse":
                    qbicleId = dbContext.TraderPurchases.FirstOrDefault(e => e.Id == id).Workgroup.Qbicle.Id;
                    break;

                case "transfer":
                    qbicleId = dbContext.TraderTransfers.FirstOrDefault(e => e.Id == id).Workgroup.Qbicle.Id;
                    break;
            }

            var topics = dbContext.Topics.AsNoTracking().Where(e => e.Qbicle.Id == qbicleId).Select(t => new SelectOption { text = t.Name, id = t.Name }).ToList();
            var folders = dbContext.MediaFolders.AsNoTracking().Where(e => e.Qbicle.Id == qbicleId).Select(t => new Select2CustomModel { text = t.Name, id = t.Id }).ToList();
            return new
            {
                qbicleId,
                topics,
                folders
            };
        }
    }
}
