using Qbicles.BusinessRules;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class MediasController : BaseController
    {
        ReturnJsonModel refModel;


        public ActionResult DuplicateMediaNameCheck(int qbicleId, int mediaId, string mediaName)
        {
            refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new MediasRules(dbContext).DuplicateMediaNameCheck(mediaId, mediaName, qbicleId);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                refModel.result = false;
                refModel.Object = ex;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateMedia(QbicleMedia media, int TopicId)
        {
            media.Topic = new TopicRules(dbContext).GetTopicById(TopicId);
            var result = new MediasRules(dbContext).UpdateMedia(media, IsCreatorTheCustomer(), CurrentUser(), GetOriginatingConnectionIdFromCookies());

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetMediaSelected(string key, string goBack)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
                var checkResult = new QbicleRules(dbContext).CheckActivityAccibility(id, CurrentUser().Id);
                if (checkResult.result && (bool)checkResult.Object == true)
                {
                    refModel = new ReturnJsonModel();

                    var mda = new MediasRules(dbContext).GetMediaById(id);
                    refModel.msgId = mda.Qbicle.Domain.Id.ToString();
                    SetCurrentQbicleIdCookies(mda.Qbicle?.Id ?? 0);
                    SetCurrentMediaIdCookies(id);
                    SetCookieGoBackPage(goBack);
                    refModel.result = true;

                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(checkResult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }

        public ActionResult SaveVersionFile(string mediaKey, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            var mediaId = string.IsNullOrEmpty(mediaKey) ? 0 : int.Parse(mediaKey.Decrypt());
            refModel = new ReturnJsonModel { result = false };
            try
            {

                var media = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(System.IO.Path.GetExtension(mediaObjectName))
                };

                var user = CurrentUser();
                refModel.Object = new MediasRules(dbContext).SaveVersionFile(IsCreatorTheCustomer(),
                    mediaId, user.Id,
                    user.Timezone, media);
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                refModel.Object = ex;
            }

            if (refModel.result)
                return Json(refModel, JsonRequestBehavior.AllowGet);
            return null;
        }

        public ActionResult DeleteVersionFile(int versionFileId)
        {
            refModel = new MediasRules(dbContext).DeleteVersionFile(IsCreatorTheCustomer(), versionFileId, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }        

        public bool UpdateFolderForMedia(string mediaKey, int folderId)
        {
            try
            {
                //var mediaId = string.IsNullOrEmpty(mediaKey) ? 0 : int.Parse(mediaKey.Decrypt());
                //var result = false;
                //var media = new MediasRules(dbContext).GetMediaById(mediaId);
                //if (media != null)
                //{
                //    var folder =
                //        new MediaFolderRules(dbContext).GetMediaFolderById(folderId, CurrentQbicleId());
                //    if (folder != null)
                //    {
                //        media.MediaFolder = folder;
                //        dbContext.SaveChanges();
                //        result = true;
                //    }
                //}

                //return result;
                var mediaId = mediaKey.Decrypt2Int();
                var result = new MediasRules(dbContext).MediaMoveFolderById(mediaId, folderId, CurrentUser().Id, IsCreatorTheCustomer());
                return result.result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return false;
            }
        }

        public ActionResult DownloadFile(FileModel file)
        {
            try
            {
                var fileName = new MediasRules(dbContext).GetFileInfoByURI(file.Uri)?.Name ?? "";
                var fileString = AzureStorageHelper.SignedUrl(file.Uri, fileName);
                return Json(fileString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return null;
            }
        }


        public ActionResult GetDocumentUrl(FileModel file)
        {
            try
            {
                var imageUrl = GetDocumentRetrievalUrl(file.Uri);
                return Json(imageUrl, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(HelperClass.ImageNotFoundUrl, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadFoldersByQbicle(string search)
        {
            try
            {
                search = !string.IsNullOrEmpty(search) ? search.Trim() : "";
                var listMediaFolderBy =
                    new MediaFolderRules(dbContext).GetMediaFoldersByQbicleId(CurrentQbicleId(), search);
                return Json(listMediaFolderBy.Select(s => new { id = s.Id, text = s.Name }).ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LoadMoveFoldersByQbicle(int cFolderId)
        {
            try
            {
                var listMediaFolderBy =
                    new MediaFolderRules(dbContext).GetMediaMoveFoldersByQbicleId(CurrentQbicleId(), cFolderId);
                return Json(listMediaFolderBy.Select(s => new { id = s.Id, text = s.Name }).ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult MediaMoveFolderById(string mediaKey, int nFolderId)
        {
            var mediaId = string.IsNullOrEmpty(mediaKey) ? 0 : int.Parse(mediaKey.Decrypt());
            var result = new MediasRules(dbContext).MediaMoveFolderById(mediaId, nFolderId, CurrentUser().Id, IsCreatorTheCustomer());

            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}