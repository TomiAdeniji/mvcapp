using Qbicles.BusinessRules;
using Qbicles.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Qbicles.Models.Notification;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class LinksController : BaseController
    {
        private ReturnJsonModel refModel;

        /// <summary>
        /// Save Activity Link
        /// </summary>
        /// <param name="link">Model Link Paramater</param>
        /// <param name="topicId">Topic Id</param>
        /// <param name="fileFeaturedImage">HttpPostedFileBase upload</param>
        /// <param name="featuredOption">1:Select from a list of defaults;2:Upload my own image</param>
        /// <param name="mediaLinkUse"></param>
        /// <returns></returns>
        public async Task<ActionResult> SaveLink(QbicleLink link, int topicId, int featuredOption, string mediaLinkUse,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            refModel = new ReturnJsonModel();

            var media = new MediaModel();
            if (featuredOption == 2)
            {
                media.UrlGuid = mediaObjectKey;
                media.Name = mediaObjectName;
                media.Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize));
                media.Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName));
            }
            else if (featuredOption == 1 && !mediaLinkUse.Equals("0"))
            {
                var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
                var img = HelperClass.GetListDefaultMedia(domainLink).FirstOrDefault(i => i.Id == mediaLinkUse);

                var uriKey = await UploadMediaFromPath(img.FileName, img.FilePath);

                media.UrlGuid = uriKey;
                media.Name = img.FileName;
                media.Size = HelperClass.FileSize((int)new FileInfo(img.FilePath).Length);
                media.Type = new FileTypeRules(dbContext).GetFileTypeByExtension(img.Extension);
            }
            else if (mediaLinkUse.Equals("0") && link.Id == 0)
            {
                refModel.result = false;
                refModel.msg = string.Format(_L("ERROR_MSG_403"));
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }

            if (!Utility.ValidateUrl(link.URL))
            {
                refModel.result = false;
                refModel.Object = new { Url = _L("ERROR_MSG_101") };
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }

            refModel = new LinksRules(dbContext).SaveLink(link, CurrentQbicleId(), topicId, media, CurrentUser().Id, GetOriginatingConnectionIdFromCookies(), AppType.Web);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetLinkSelected(string key, string goBack)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());

                //Check for activity accessibility
                var checkResult = new QbicleRules(dbContext).CheckActivityAccibility(id, CurrentUser().Id);
                if (checkResult.result && (bool)checkResult.Object == true)
                {
                    refModel = new ReturnJsonModel();

                    var lk = new LinksRules(dbContext).GetLinkById(id);
                    refModel.msgId = lk.Qbicle?.Domain?.Id.ToString();
                    refModel.result = true;
                    SetCurrentQbicleIdCookies(lk.Qbicle?.Id ?? 0);
                    SetCurrentLinkIdCookies(id);
                    SetCookieGoBackPage(goBack);

                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(checkResult, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }
    }
}