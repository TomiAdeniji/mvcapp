using DotnetDaddy.DocumentViewer;
using Microsoft.Ajax.Utilities;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Model;
using System;
using System.Data.Entity;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qbicles.Doc.Controllers
{
    public class RetrieverController : Controller
    {
        private ApplicationDbContext _dbContext = new ApplicationDbContext();

        #region Azure

        /// <summary>
        /// Get document file from storage
        /// </summary>
        /// <param name="file"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Authorize]
        [AcceptVerbs("GET")]
        public async Task<RedirectResult> GetDocument(string file, string size = "")
        {
            //Check for file size
            if (size == "")
            {
                var s3Url = AzureStorageHelper.SignedUrl(file);
                return Redirect(s3Url);
            }

            //Create object variable
            var objectUrl = string.Empty;

            //Fetch storage file details from DB
            var storageFileDetail = await _dbContext.StorageFileDetails.FirstOrDefaultAsync(e => e.StorageFile == file && e.Extension == size);

            //
            objectUrl = AzureStorageHelper.SignedUrl(storageFileDetail?.Path ?? file, HelperClass.QbicleLogoDefault);

            return Redirect(objectUrl);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [Authorize]
        [AcceptVerbs("GET")]
        public async Task<RedirectResult> GetVideoMediaScreenshot(string file)
        {
            var storage = await _dbContext.StorageFileDetails.FirstOrDefaultAsync(e => e.StorageFile == file && e.Extension == "png");
            if (storage == null) return null;

            var s3ObjectUrl = AzureStorageHelper.SignedUrl(storage.Path);
            return Redirect(s3ObjectUrl);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [Authorize]
        [AcceptVerbs("GET")]
        public async Task<RedirectResult> GetVideoMediaPlay(string file, string type)
        {
            var storage = await _dbContext.StorageFileDetails.FirstOrDefaultAsync(e => e.StorageFile == file && e.Extension == type);
            var s3ObjectUrl = AzureStorageHelper.SignedUrl(storage?.Path ?? file);
            return Redirect(s3ObjectUrl);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        public async Task<ActionResult> GetPublicDocument(string file)
        {
            var fileStorageInformation = await _dbContext.StorageFiles.FirstOrDefaultAsync(e => e.Id == file && e.IsPublic);
            if (fileStorageInformation == null)
                return null;
            var s3Object = await AzureStorageHelper.ReadObjectDataAsync(fileStorageInformation.Id);

            Response.Headers.Add("Content-Disposition", new ContentDisposition
            {
                FileName = s3Object.ObjectName,
                Inline = true
            }.ToString());

            return File(s3Object.ObjectStream, s3Object.ObjectContentType);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [Authorize]
        [AcceptVerbs("GET")]
        public async Task<ActionResult> GetDocumentBase64(string file)
        {
            var fileStorageInformation = _dbContext.StorageFiles.Find(file);
            if (fileStorageInformation == null)
                return Json("", JsonRequestBehavior.AllowGet);

            var s3Object = await AzureStorageHelper.ReadObjectDataAsync(fileStorageInformation.Id);

            var imageBase64 = HelperClass.GetBase64StringFromStream(s3Object.ObjectStream);
            return Json(imageBase64, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [Authorize]
        [AcceptVerbs("GET")]
        public async Task<Stream> GetDocumentStream(string file)
        {
            //TODO: test and update this
            var fileStorageInformation = _dbContext.StorageFiles.Find(file);
            if (fileStorageInformation == null)
                return null;
            var s3Object = await AzureStorageHelper.ReadObjectDataAsync(fileStorageInformation.Id);
            return s3Object.ObjectStream;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Authorize]
        public async Task<ActionResult> ViewFile(string file)
        {
            var viewer = new DocViewer { ID = "ctlDoc", IncludeJQuery = false, DebugMode = false, BasePath = "/", FitType = "width" };
            // if you want to use an older version of jQuery specify this above contructor:
            // OldJQuery = true

            // Get the required client side script and css

            ViewBag.ViewerScripts = viewer.ReferenceScripts();
            ViewBag.ViewerCSS = viewer.ReferenceCss();
            ViewBag.ViewerID = viewer.ClientID;
            ViewBag.ViewerObject = viewer.JsObject;

            // open  document
            var fileStorage = _dbContext.StorageFiles.Find(file);
            if (fileStorage != null)
            {
                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(fileStorage.Id);
                var fileBytes = HelperClass.Stream2Bytes(s3Object.ObjectStream);

                var extension = Path.GetExtension(fileStorage.Name);
                if (extension == null || extension == "") extension = "." + s3Object.Extension;
                var token = viewer.OpenDocument(fileBytes, extension);

                if (token.IsNullOrWhiteSpace())
                {
                    throw new Exception(viewer.InternalError);
                }

                // Get final Init arguments to render the viewer

                ViewBag.ViewerInit = viewer.GetAjaxInitArguments(token);

                return View();
            }
            return View();
        }

        #endregion Azure

        #region Sample Endpoints



        #endregion
    }
}