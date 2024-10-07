using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.ProfilePage;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.ProfilePages;
using Qbicles.Models;
using Qbicles.Models.ProfilePage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class ProfilePageController : BaseController
    {
        /// <summary>
        /// Business Profile Page Builder
        /// </summary>
        /// <returns></returns>
        public ActionResult BusinessPageBuilder(string pageKey)
        {
            if (!CanAccessBusiness())
                return View("ErrorAccessPage");
            var pageId = 0;
            if (!string.IsNullOrEmpty(pageKey?.Trim()))
            {
                pageId = Int32.Parse(EncryptionService.Decrypt(pageKey));
            }
            var imgListFileType = new FileTypeRules(dbContext).GetExtensionsByType("Image File");
            ViewBag.ImageAcceptedExtensions = imgListFileType.Count() > 0 ? ("." + string.Join(",.", imgListFileType)) : "";
            return View(new BusinessPageRules(dbContext).GetBusinessPageById(pageId));
        }
        public ActionResult UserPageBuilder(string pageKey)
        {
            var pageId = 0;
            if (!string.IsNullOrEmpty(pageKey?.Trim()))
            {
                pageId = Int32.Parse(EncryptionService.Decrypt(pageKey));
            }
            var imgListFileType = new FileTypeRules(dbContext).GetExtensionsByType("Image File");
            ViewBag.ImageAcceptedExtensions = imgListFileType.Count() > 0 ? ("." + string.Join(",.", imgListFileType)) : "";
            return View(new UserProfileRules(dbContext).GetUserPageById(pageId));
        }
        public ActionResult BusinessPagePreview(string pageKey)
        {
            if (!CanAccessBusiness() || string.IsNullOrEmpty(pageKey))
                return View("ErrorAccessPage");

            ViewBag.IsPreview = true;
            return View(new BusinessPageRules(dbContext).GetBusinessPageById(pageKey.Decrypt2Int()));
        }
        public ActionResult UserPagePreview(string pageKey)
        {
            ViewBag.IsPreview = true;
            return View(new UserProfileRules(dbContext).GetUserPageById(pageKey.Decrypt2Int()));
        }
        public ActionResult GetBusinessProfilePages([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int status = -1)
        {
            var currentUser = CurrentUser();
            return Json(new BusinessPageRules(dbContext).GetBusinessProfilePages(requestModel, CurrentDomainId(), keyword, currentUser.Timezone, currentUser.DateFormat, status), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveBusinessPageBuilder(BusinessProfilePageModel pageBuilder)
        {
            if (!CanAccessBusiness())
                return Json(new ReturnJsonModel() { result = false, msg = "ERROR_MSG_28" });
            var currentUser = CurrentUser();
            pageBuilder.Domain = CurrentDomain();
            pageBuilder.CreateBy = new UserRules(dbContext).GetById(currentUser.Id);
            return Json(new BusinessPageRules(dbContext).SaveBusinessPageBuilder(pageBuilder));
        }
        public ActionResult GetJsonBusinessPageById(int id)
        {
            var configs = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            configs.Converters.Add(new StringEnumConverter());
            var list = JsonConvert.SerializeObject(
            new BusinessPageRules(dbContext).GetBusinessPageForJsonById(id),
            Formatting.None,
            configs
            );

            return Content(list, "application/json");
            //return Json(new BusinessPageRules(dbContext).GetBusinessPageForJsonById(id),JsonRequestBehavior.AllowGet);
        }
        public ActionResult SetBusinessPageStatus(string pageKey, BusinessPageStatusEnum status)
        {
            if (!CanAccessBusiness() || string.IsNullOrEmpty(pageKey))
                return Json(new ReturnJsonModel() { result = false, msg = "ERROR_MSG_28" });

            var result = new BusinessPageRules(dbContext).SetBusinessPageStatus(pageKey.Decrypt2Int(), status);
            //var page = new UserProfileRules(dbContext).GetUserPageById(pageKey.Decrypt2Int());
            //var viewPage = RenderViewToString("~/Views/ProfilePage/_BlockTemplate.cshtml", result.Object2);
            result.Object2 = null;
            return Json(result);
        }
        public ActionResult DeleteProfilePage(string pageKey)
        {
            if (!CanAccessBusiness())
                return Json(new ReturnJsonModel() { result = false, msg = "ERROR_MSG_28" });
            var pageId = 0;
            if (!string.IsNullOrEmpty(pageKey?.Trim()))
            {
                pageId = Int32.Parse(EncryptionService.Decrypt(pageKey));
            }
            return Json(new BusinessPageRules(dbContext).DeleteProfilePage(pageId));
        }
        public ActionResult GetUserProfilePages([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int status = -1)
        {
            var currentUser = CurrentUser();
            return Json(new UserProfileRules(dbContext).GetUserProfilePages(requestModel, currentUser.Id, keyword, currentUser.Timezone, currentUser.DateFormat, status), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveUserPageBuilder(UserProfilePageModel pageBuilder)
        {
            var currentUser = CurrentUser();
            pageBuilder.user = new UserRules(dbContext).GetById(currentUser.Id);
            pageBuilder.CreateBy = pageBuilder.user;
            return Json(new UserProfileRules(dbContext).SaveUserPageBuilder(pageBuilder));
        }
        public ActionResult GetJsonUserPageById(int id)
        {
            var configs = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            configs.Converters.Add(new StringEnumConverter());
            var list = JsonConvert.SerializeObject(
            new UserProfileRules(dbContext).GetUserPageForJsonById(id),
            Formatting.None,
            configs
            );

            return Content(list, "application/json");
        }
        public ActionResult SetUserPageStatus(string pageKey, UserPageStatusEnum status)
        {
            var pageId = 0;
            if (!string.IsNullOrEmpty(pageKey?.Trim()))
            {
                pageId = Int32.Parse(EncryptionService.Decrypt(pageKey));
            }
            return Json(new UserProfileRules(dbContext).SetUserPageStatus(pageId, status));
        }
        public ActionResult DeleteUserProfilePage(string pageKey)
        {
            var pageId = 0;
            if (!string.IsNullOrEmpty(pageKey?.Trim()))
            {
                pageId = Int32.Parse(EncryptionService.Decrypt(pageKey));
            }
            return Json(new UserProfileRules(dbContext).DeleteProfilePage(pageId, CurrentUser().Id));
        }
        public ActionResult UpdateDisplayOrder(List<ProfilePage> pages)
        {
            if (pages != null)
                foreach (var item in pages)
                {
                    item.Id = item.Key.Decrypt2Int();
                }
            return Json(new BusinessPageRules(dbContext).UpdateDisplayOrder(pages));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">ProfilePage key</param>
        /// <param name="direction">up=-1, down=1</param>
        /// <returns></returns>
        public ActionResult UpdateDisplayOrderPageBuilder(string key, int direction)
        {
            //page.Id = page.Key.Decrypt2Int();
            return Json(new BusinessPageRules(dbContext).UpdateDisplayOrderPageBuilder(key, direction, CurrentDomainId()));
        }

        //[AllowAnonymous]
        //[HttpPost]
        //public ActionResult BusinessPublicPages(BusinessPageParameter parameter)
        //{
        //    var domainId = parameter.DomainKey.Decrypt2Int();
        //    var pages = dbContext.BusinessPages.Where(s => s.Type == ProfilePageType.Business && s.Status == BusinessPageStatusEnum.IsActive && s.Domain.Id == domainId).OrderBy(o => o.DisplayOrder).ToList();

        //    var refModel = new List<BusinessPageModel>();

        //    pages.ForEach(p =>
        //    {
        //        var page = new BusinessPageModel
        //        {
        //            PageTitle = p.PageTitle,
        //            PageContent = RenderViewToString("~/Views/ProfilePage/_UserBlockTemplate.cshtml", p)
        //        };
        //        refModel.Add(page);
        //    });


        //    return Json(refModel, JsonRequestBehavior.AllowGet);
        //}

        private string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new System.IO.StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }
    }
}