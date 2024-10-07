using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.Community;
using Qbicles.BusinessRules.Community;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models.Community;

namespace Qbicles.Web.Controllers
{
    public class CommunityAddEditController : BaseController
    {
        [HttpPost]
        public ActionResult SaveProceed(CommunityPage page)
        {
            var result = new ReturnJsonModel();
            result.actionVal = 1;
            if (page.Id > 0) {
                result.actionVal = 2;
                result.msgId = page.Id.ToString();
            }
            try
            {
                var createdBy = new UserRules(dbContext).GetById(CurrentUser().Id);
                page.CreatedDate = DateTime.UtcNow;
                page.LastUpdated = DateTime.UtcNow;
                page.CreatedBy = createdBy;
                page.PageType = CommunityPageTypeEnum.CommunityPage;
                page.Domain = CurrentDomain();
                page.Qbicle = CurrentDomain().Qbicles.FirstOrDefault(q => q.Id == page.Qbicle.Id);
               
                if (page.Articles != null && page.Articles.Count > 0)
                {
                    var pageOrigin = new CommunityPageRules(dbContext).GetCommunityPageById(page.Id);
                    for (int i = 0; i < page.Articles.Count; i++)
                    {
                        if (page.Articles[i].Id > 0)
                        {
                            var artic = page.Articles[i];
                            page.Articles[i] = new ArticlesRules(dbContext).GetArticlesById(artic.Id);
                            page.Articles[i].Title = artic.Title;
                            page.Articles[i].DisplayOrder = artic.DisplayOrder;
                            page.Articles[i].Image = artic.Image;
                            page.Articles[i].IsDisplayed = artic.IsDisplayed;
                            page.Articles[i].Source = artic.Source;
                            page.Articles[i].URL = artic.URL;
                        }
                        else
                        {
                            page.Articles[i].AssociatedPage = pageOrigin;
                            page.Articles[i].CreatedBy = createdBy;
                            page.Articles[i].CreatedDate = DateTime.UtcNow;
                        }
                    }
                }
                var _communityPage = new CommunityPageRules(dbContext);
                _communityPage.SaveCommunityPage(page);
                result.Object = page.Id;
            }
            catch (Exception ex)
            {
                result.actionVal = 3;
                result.msg = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UpdateFeature()
        {
            var resultModel = new ReturnJsonModel();
            resultModel.actionVal = 2;
            resultModel.msg = ResourcesManager._L("ERROR_MSG_487");
            try
            {

                if (Request.Files.Count > 0)
                {
                    HttpPostedFileBase file = Request.Files[0];
                    var mediaModel = new MediaModel();
                    if (!string.IsNullOrEmpty(file.FileName))
                    {
                        //mediaModel = StoreFile(file, Enums.RepositoryItemType.Community);
                    }
                    resultModel.Object = mediaModel.UrlGuid;
                    resultModel.msg = "";
                    resultModel.actionVal = 1;
                    return Json(resultModel, JsonRequestBehavior.AllowGet);
                }
                else
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
        public ActionResult UpdateImage()
        {
            var resultModel = new ReturnJsonModel();
            resultModel.actionVal = 1;
            resultModel.msg = "";
            try
            {

                if (Request.Files.Count > 0)
                {
                    var lstItem = new List<object>();
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        HttpPostedFileBase file = Request.Files[i];

                        var mediaModel = new MediaModel();
                        if (!string.IsNullOrEmpty(file.FileName))
                        {
                            //mediaModel = StoreFile(file, Enums.RepositoryItemType.Community);
                            var item = new { Id = Request.Files.AllKeys[i], Image = mediaModel.UrlGuid }; 
                            lstItem.Add(item);
                        }
                    }
                    resultModel.Object = lstItem;
                    resultModel.msg = "";
                    resultModel.actionVal = 1;
                    return Json(resultModel, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                resultModel.actionVal = 3;
                resultModel.msg = ex.Message;
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
        }

    }
}