using Qbicles.BusinessRules.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Qbicles.Models;
using System.Data.Entity;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class MediaFolderRules
    {
        ApplicationDbContext _db;
        ReturnJsonModel refModel;

        public MediaFolderRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }


        public bool IsDuplicateFolderName(int mediaFolderId, string mediaFolderName, int qbicleId)
        {
            var isDuplicate = true;
            try
            {
                if (mediaFolderId > 0)
                {
                    isDuplicate = DbContext.MediaFolders.Any(x => x.Id != mediaFolderId && x.Name == mediaFolderName && x.Qbicle.Id == qbicleId);
                }
                else
                {
                    isDuplicate = DbContext.MediaFolders.Any(x => x.Name == mediaFolderName && x.Qbicle.Id == qbicleId);
                }

            }
            catch (Exception)
            {
            }
            return isDuplicate;
        }

        public MediaFolder InsertMediaFolder(string mediaFolderName, string userId, int qbicleId)
        {
            try
            {

                var qbicle = new QbicleRules(DbContext).GetQbicleById(qbicleId);
                var folder = new MediaFolder
                {
                    Name = mediaFolderName,
                    CreatedBy = new UserRules(DbContext).GetUser(userId, 0),
                    CreatedDate = DateTime.UtcNow,
                    Qbicle = qbicle
                };
                qbicle.LastUpdated = DateTime.UtcNow;

                DbContext.MediaFolders.Add(folder);
                DbContext.Entry(folder).State = EntityState.Added;
                DbContext.SaveChanges();

                return folder;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, mediaFolderName, userId, qbicleId);
                return null;

            }
        }

        public MediaFolder GetMediaFolderById(int mediaFolerId, int qbicleId)
        {
            return DbContext.MediaFolders.FirstOrDefault(x => x.Id == mediaFolerId && x.Qbicle.Id == qbicleId);
        }
        /// <summary>
        /// Get or create new folder if does not existed
        /// </summary>
        /// <param name="mediaFolerName"></param>
        /// <param name="qbicleId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MediaFolder GetMediaFolderByName(string mediaFolerName, int qbicleId, string userId = "")
        {
            var folder = DbContext.MediaFolders.FirstOrDefault(x => x.Qbicle.Id == qbicleId && x.Name == mediaFolerName);
            if (folder != null)
                return folder;

            return InsertMediaFolder(HelperClass.GeneralName, userId, qbicleId);
        }


        public MediaFolder UpdateMediaFolder(int mediaFolderId, string mediaFolderName, int qbicleId)
        {

            try
            {
                var mf = GetMediaFolderById(mediaFolderId, qbicleId);

                if (mf != null)
                {
                    mf.Name = mediaFolderName;
                    //mf.CreatedBy = new UserRules(DbContext).GetUser(userId, 0);
                    //mf.CreatedDate = DateTime.UtcNow;
                    //mf.Qbicle = new QbicleRules(DbContext).GetQbicleById(qbicleId); ;
                    mf.Qbicle.LastUpdated = DateTime.UtcNow;
                    if (DbContext.Entry(mf).State == EntityState.Detached)
                    {
                        DbContext.MediaFolders.Attach(mf);
                    }
                    DbContext.Entry(mf).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }

                return mf;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, mediaFolderId);
                return null;
            }

        }

        public List<QbicleMedia> GetMediaItemByFolderId(int mediaFolderId, int qbicleId, string fileType, string currentTimeZone)
        {
            var listResult = new List<QbicleMedia>();
            var mediaFolder = DbContext.MediaFolders.FirstOrDefault(x => x.Id == mediaFolderId && x.Qbicle.Id == qbicleId);
            if (mediaFolder != null)
            {
                if (!string.IsNullOrEmpty(fileType))
                    if (fileType.Equals("Documents"))
                    {
                        listResult = mediaFolder.Media.Where(x => x.FileType != null && x.FileType.Type != "Image File" && x.FileType.Type != "Video File").OrderBy(x => x.Id).ToList();
                    }
                    else
                    {
                        listResult = mediaFolder.Media.Where(x => x.FileType != null && x.FileType.Type == fileType).OrderBy(x => x.Id).ToList();
                    }

                else
                    listResult = mediaFolder.Media.OrderBy(x => x.Id).ToList();
            }

            return listResult.BusinessMapping(currentTimeZone);
        }

        public List<QbicleMedia> GetMediaItemByFolderIdWithName(int mediaFolderId, int qbicleId, string fileType, string name, string CurrentTimeZone)
        {
            var listResult = new List<QbicleMedia>();
            var mediaFolder = DbContext.MediaFolders.FirstOrDefault(x => x.Id == mediaFolderId && x.Qbicle.Id == qbicleId);
            if (mediaFolder != null)
            {
                var query = mediaFolder.Media.AsQueryable();
                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(x => x.Name.ToLower().Contains(name.Trim().ToLower()));
                }
                if (!string.IsNullOrEmpty(fileType))
                    if (fileType.Equals("Documents"))
                    {
                        listResult = query.Where(x => x.FileType.Type != "Image File" && x.FileType.Type != "Video File").OrderBy(x => x.Id).ToList();
                    }
                    else
                    {
                        listResult = query.Where(x => x.FileType.Type == fileType).OrderBy(x => x.Id).ToList();
                    }

                else
                    listResult = query.OrderBy(x => x.Id).ToList();
            }

            return listResult.BusinessMapping(CurrentTimeZone);
        }


        public List<QbicleMedia> GetMediaItemByFolderId(int mediaFolderId, int qbicleId, string currentTimeZone)
        {
            var listMedia = new List<QbicleMedia>();
            try
            {
                var listResult = new List<QbicleMedia>();
                var mediaFolder = DbContext.MediaFolders.FirstOrDefault(x => x.Id == mediaFolderId && x.Qbicle.Id == qbicleId);
                if (mediaFolder != null)
                {
                    listResult = mediaFolder.Media.OrderBy(x => x.Id).ToList();
                }
                listMedia = listResult.BusinessMapping(currentTimeZone);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

            return listMedia;
        }

        public List<QbicleMedia> MediaFilter(int mediaFolderId, int qbicleId, string name, string fileType, string currentTimeZone)
        {
            try
            {
                var medias = from m in DbContext.Medias
                             where m.MediaFolder.Id == mediaFolderId && m.Qbicle.Id == qbicleId
                             select m;

                if (!string.IsNullOrEmpty(name))
                    medias = medias.Where(e => e.Name.ToLower().Contains(name.ToLower()));
                if (!string.IsNullOrEmpty(fileType) && fileType != "all")
                    medias = medias.Where(e => e.FileType.Extension == fileType);


                medias = medias.OrderByDescending(x => x.StartedDate);

                return medias.ToList().BusinessMapping(currentTimeZone);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

            return new List<QbicleMedia>();
        }

        public List<MediaFolder> GetMediaFoldersByQbicleIdAndUserId(int qbicleId, string userId)
        {
            var excludedMediaFolder = DbContext.OperatorTeamPersons.Where(t => t.User.Id != userId && !t.ResourceFolder.Media.Any(m => m.StartedBy.Id == userId) && t.ResourceFolder.Qbicle.Id == qbicleId)
                                      .Select(t => t.ResourceFolder);
            return DbContext.MediaFolders.Where(x => x.Qbicle.Id == qbicleId && !excludedMediaFolder.Any(e => e.Id == x.Id)).ToList();
        }

        public List<MediaFolder> GetMediaFoldersByQbicleId(int qbicleId, string search)
        {
            if (qbicleId == 0)
                return new List<MediaFolder>();

            if (!string.IsNullOrEmpty(search))
                return DbContext.MediaFolders.Where(x => x.Qbicle.Id == qbicleId && x.Name.Contains(search)).AsNoTracking().ToList();
            else
            {
                var folders = DbContext.MediaFolders.Where(x => x.Qbicle.Id == qbicleId).AsNoTracking().ToList();
                if (folders == null || !folders.Any())
                {
                    var qbicle = DbContext.Qbicles.Find(qbicleId);
                    var folder = new MediaFolder
                    {
                        Name = HelperClass.GeneralName,
                        CreatedBy = qbicle.StartedBy,
                        CreatedDate = DateTime.UtcNow,
                        Qbicle = qbicle
                    };
                    DbContext.MediaFolders.Add(folder);
                    DbContext.Entry(folder).State = EntityState.Added;
                    DbContext.SaveChanges();
                    folders.Add(folder);
                }
                return folders;
            }

        }
        public List<MediaFolder> GetMediaMoveFoldersByQbicleId(int qbicleId, int cFolderId)
        {
            return DbContext.MediaFolders.Where(f => f.Qbicle.Id == qbicleId && f.Id != cFolderId).ToList();
        }
        public ReturnJsonModel DeleteMediaFolderById(int mFolderId, int qbicleId)
        {
            refModel = new ReturnJsonModel { result = false };
            try
            {
                var mFolder = GetMediaFolderById(mFolderId, qbicleId);
                if (mFolder != null)
                {
                    if (mFolder != null && mFolder.Name == HelperClass.GeneralName)
                    {
                        refModel.msg = "This is the General folder, so delete it !";
                    }
                    else
                    {
                        var generalFolderByQbicle = GetMediaFolderByName(HelperClass.GeneralName, qbicleId);

                        foreach (var media in mFolder.Media)
                        {
                            media.MediaFolder = generalFolderByQbicle;
                        }
                        if (mFolder.Media.Count > 0)
                        {
                            DbContext.SaveChanges();
                        }
                        mFolder.Qbicle.LastUpdated = DateTime.UtcNow;
                        DbContext.MediaFolders.Remove(mFolder);
                        DbContext.SaveChanges();
                        refModel.result = true;
                        refModel.Object = new { DeletedId = mFolderId };
                    }
                }
                else
                {
                    refModel.msg = "Media Folder not found !";
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.msg = "An error occurred while deleting the folder";
            }


            return refModel;
        }

        public ReturnJsonModel SaveMoveMediasToOtherFolder(int toFolder, int qbicleId, List<int> listMedias)
        {
            refModel = new ReturnJsonModel { result = false };
            try
            {
                var mediaFolder = new MediaFolderRules(DbContext).GetMediaFolderById(toFolder, qbicleId);
                var mediaItemsByListId = new MediasRules(DbContext).GetMediaByListId(listMedias);
                mediaItemsByListId.ForEach(x => x.MediaFolder = mediaFolder);
                DbContext.SaveChanges();
                refModel.result = true;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;

        }
        public List<QbicleMedia> GetQbicleMediasByDefault(int qbicleId)
        {
            try
            {
                var folder = DbContext.MediaFolders.FirstOrDefault(f => f.Qbicle.Id == qbicleId && f.Name.ToLower() == HelperClass.GeneralName);
                if (folder != null)
                    return folder.Media.ToList();
                else
                    return new List<QbicleMedia>();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<QbicleMedia>();
            }
        }
    }
}
