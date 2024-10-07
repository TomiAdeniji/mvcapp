using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Community;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class UserProfileController : BaseController
    {
        //ProfileTex - avatar - cover
        [HttpPost]
        public ActionResult CommunityChangeUserAvatar()
        {
            var resultModel = new ReturnJsonModel { actionVal = 2, msg = "No file update." };
            try
            {
                if (Request.Files.Count <= 0) return Json(resultModel, JsonRequestBehavior.AllowGet);
                var file = Request.Files[0];
                CommunityUpdateUserAvatar(file);
                resultModel.msg = "";
                resultModel.actionVal = 1;
                return Json(resultModel, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                resultModel.actionVal = 3;
                resultModel.msg = ex.Message;
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CommunityChangeFeatureUserImage()
        {
            var resultModel = new ReturnJsonModel { actionVal = 2, msg = "No file update." };
            try
            {
                if (Request.Files.Count <= 0) return Json(resultModel, JsonRequestBehavior.AllowGet);
                var file = Request.Files[0];
                CommunityUpdateFeaturedUserImage(file);
                resultModel.msg = "";
                resultModel.actionVal = 1;
                return Json(resultModel, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                resultModel.actionVal = 3;
                resultModel.msg = ex.Message;
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult UpdateProfileText(string profileText)
        {
            var resultModel = new ReturnJsonModel();
            try
            {
                var profileRule = new UserProfilePageRules(dbContext);
                profileRule.UpdateProfileText(profileText, CurrentUser().Id);
                resultModel.actionVal = 1;
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                resultModel.actionVal = 2;
                resultModel.msg = ex.Message;
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
        }

        private MediaModel CommunityUpdateUserAvatar(HttpPostedFileBase file)
        {
            try
            {
                //var userProfileRules = new UserProfilePageRules(dbContext); update when open community
                //var mediaModel = new MediaModel();
                //if (string.IsNullOrEmpty(file.FileName)) return mediaModel;
                //mediaModel = StoreFile(file, Enums.RepositoryItemType.Community);
                //userProfileRules.UpdateAvatarImage(mediaModel.UrlGuid, UserSettings().Id);

                return new MediaModel();
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public MediaModel CommunityUpdateFeaturedUserImage(HttpPostedFileBase file)
        {
            try
            {
                var userProfileRules = new UserProfilePageRules(dbContext);
                var mediaModel = new MediaModel();
                //if (string.IsNullOrEmpty(file.FileName)) return mediaModel; update when open community
                //mediaModel = StoreFile(file, Enums.RepositoryItemType.Community);
                //userProfileRules.CommunityUpdateFeaturedUserImage(mediaModel.UrlGuid, UserSettings().Id);

                return mediaModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        // ProfileFiles - Skill - File
        [HttpPost]
        public ActionResult CheckProfileFiles()
        {
            var refModel = new ReturnJsonModel();
            var mediaModel = new List<MediaModel>();
            try
            {
                if (Request.Files.Count > 0)
                {
                    var fileRequests = Request.Files;
                    for (var i = 0; i < fileRequests.Count; i++)
                    {
                        var media = new MediaModel();
                        var file = fileRequests[i];

                        if (file != null && HelperClass.CheckFileUpload(file.FileName) != true) continue;
                        media.Name = fileRequests.AllKeys[i];
                        if (file != null)
                            media.Type =
                                new FileTypeRules(dbContext).GetFileTypeById(Path.GetExtension(file.FileName)
                                    ?.ToLower()
                                    .Replace(".", ""));
                        mediaModel.Add(media);
                    }

                    refModel.actionVal = 1;
                    refModel.Object = mediaModel;
                }
            }
            catch (Exception ex)
            {
                refModel.actionVal = 2;
                refModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteFiles(List<int> lstId)
        {
            var profiler = new UserProfilePageRules(dbContext);
            var refModel = profiler.DeleteFiles(lstId, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CommunityUserAddNewFiles()
        {
            var refModel = new ReturnJsonModel();
            try
            {
                //if (Request.Files.Count > 0) Update when open Community
                //{
                //    var _userProfileRule = new UserProfilePageRules(dbContext);
                //    var lstProfileFiles = new List<ProfileFile>();
                //    for (var i = 0; i < Request.Files.Count; i++)
                //    {
                //        var file = Request.Files[i];
                //        var mediaModel = StoreFile(file, Enums.RepositoryItemType.Community);
                //        lstProfileFiles.Add(new ProfileFile
                //        {
                //            CreatedBy = CurrentUser(CurrentQbicleId()),
                //            CreatedDate = DateTime.UtcNow,
                //            Name = Request.Files.AllKeys[i],
                //            StoredFileName = mediaModel.UrlGuid,
                //            FileType = mediaModel.Type
                //        });
                //    }

                //    _userProfileRule.AddNewFile(lstProfileFiles, UserSettings().Id);
                //    refModel.actionVal = 1;
                //}
            }
            catch (Exception ex)
            {
                refModel.actionVal = 2;
                refModel.msg = ex.Message;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateInterests(List<int> interests)
        {
            string userId = CurrentUser().Id;
            return Json(new UserRules(dbContext).UpdateInterests(interests, userId));
        }

        public ActionResult UpdateUserNavBarOpenStatus(bool isClosed)
        {
            var userId = CurrentUser().Id;
            return Json(new UserRules(dbContext).ToggleUserNavBarOpenStt(userId, isClosed), JsonRequestBehavior.AllowGet);
        }
    }
}