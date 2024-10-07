using Qbicles.BusinessRules;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models;
using System;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class QbicleFilesController : BaseController
    {

        public ActionResult SaveMedia(QbicleMedia media, S3ObjectUploadModel s3ObjectUpload, int mediaFolderId, int qbicleId, int isMediaOntab = 0)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                qbicleId = qbicleId > 0 ? qbicleId : CurrentQbicleId();
                if (media.Name.Length > 250)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_389", 250);
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                if (media.Description != null && media.Description.Length > 500)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_389", 500);
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }


                var extension = HelperClass.GetFileExtension(s3ObjectUpload.FileName);

                var fileType = new FileTypeRules(dbContext).GetFileTypeByExtension(extension);



                var versionFile = new VersionedFile
                {
                    Uri = s3ObjectUpload.FileKey,
                    FileSize = HelperClass.FileSize(int.Parse(s3ObjectUpload.FileSize)),
                    FileType = fileType
                };

                //Document Retrieval API

                media.FileType = fileType;
                media.Qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
                ReturnJsonModel result;
                if (CurrentJournalEntryId() > 0)
                {
                    var journalEntry = new JournalEntryRules(dbContext).GetById(CurrentJournalEntryId());
                    result = new MediasRules(dbContext).SaveMedia(media, IsCreatorTheCustomer(),
                        CurrentUser().Id,isMediaOntab == 1,
                        0, 0, 0, 0, 0, 0, 0, "", versionFile, mediaFolderId, null, null, journalEntry, null, null, GetOriginatingConnectionIdFromCookies(), AppType.Web);
                }
                else
                {
                    result = new MediasRules(dbContext).SaveMedia(media, IsCreatorTheCustomer(),
                        CurrentUser().Id, isMediaOntab == 1, CurrentDiscussionId(), CurrentTaskId(), CurrentEventId(),
                        CurrentAlertId(), CurrentApprovalId(), CurrentLinkId(), CurrentMediaId(), "", versionFile, mediaFolderId, null, null, null, null, null, GetOriginatingConnectionIdFromCookies(), AppType.Web);
                }

                refModel = result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                refModel.result = false;
                refModel.msg = ex.Message;
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request")
                    || ex.Message.Contains("The resource cannot be found"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
    }
}