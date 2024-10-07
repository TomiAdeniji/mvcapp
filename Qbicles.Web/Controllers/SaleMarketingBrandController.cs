using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.SalesMkt;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Enums;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class SalesMarketingBrandController : BaseController
    {
        // GET: SaleMarketingBrand
        public ActionResult Detail(int brandId)
        {
            var brandRule = new SocialBrandRules(dbContext);
            var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId());
            var brand = brandRule.GetBrandById(brandId);
            ViewBag.Setting = setting;
            ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(setting?.SourceQbicle.Id ?? 0);
            ViewBag.Products = brandRule.LoadBrandProductsAll(brandId);
            ViewBag.Segments = brandRule.CustomerSegmentsAll(CurrentDomain().Id);
            if (brand != null)
                return View(brand);
            else
                return View("Error");
        }
        public ActionResult SaveSaleMarketingBrand(BrandCustomModel model, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            var refModel = new ReturnJsonModel { result = false };
            var domain = CurrentDomain();
            var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domain.Id);
            if (setting == null)
            {
                refModel.msg = ResourcesManager._L("ERROR_MSG_153");
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            var media = new MediaModel
            {
                UrlGuid = mediaObjectKey,
                Name = mediaObjectName,
                Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
            };

            model.CurrentDomain = domain;
            refModel = new SocialBrandRules(dbContext).SaveBrand(model, media, setting, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AutoGenerateFolderName()
        {
            try
            {
                string folderName = new SocialBrandRules(dbContext).AutoGenerateFolderName(CurrentDomainId());
                return Json(folderName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GenerateModalBrandAddEdit(int brandId)
        {
            var brandRule = new SocialBrandRules(dbContext);
            ViewBag.MediaFolders = brandRule.GetMediaFoldersByQbicleId(CurrentDomainId());
            return PartialView("~/Views/SalesMarketing/_BrandAdd.cshtml", brandRule.GetBrandById(brandId));
        }

        [HttpGet]
        public ActionResult LoadBrands(int skip, int take, string keyword, bool isLoadingHide)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                int totalRecord = 0;
                var brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(CurrentDomainId(), isLoadingHide, skip, take, keyword, ref totalRecord);
                var partialView = "";
                if (take != 0)
                {
                    partialView = RenderViewToString("~/Views/SalesMarketingBrand/_BrandsContent.cshtml", brands);
                }

                refModel.Object = new
                {
                    strResult = partialView,
                    totalRecord = totalRecord
                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult LoadModalBrandById(int id)
        {
            var brandRule = new SocialBrandRules(dbContext);
            ViewBag.MediaFolders = brandRule.GetMediaFoldersByQbicleId(CurrentDomainId());
            return PartialView("~/Views/SalesMarketing/_BrandAdd.cshtml", brandRule.GetBrandById(id));
        }
        [HttpPost]
        public ActionResult ShowOrHideBrand(int id)
        {
            var brandRule = new SocialBrandRules(dbContext);
            var refModel = brandRule.ShowOrHideBrand(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadBrandProducts(int brandId, int size, int pageSize, string keyword, bool isLoadingHide)
        {
            int totalCount = 0;
            var brandProducts = new SocialBrandRules(dbContext).LoadBrandProductsByBrandId(brandId, size, pageSize, keyword, isLoadingHide, ref totalCount);
            ViewBag.totalCount = totalCount;
            ViewBag.pageIndex = size;
            return PartialView("~/Views/SalesMarketingBrand/_BrandProductContent.cshtml", brandProducts);
        }
        [HttpPost]
        public ActionResult ShowOrHideBrandProduct(int id)
        {
            var brandRule = new SocialBrandRules(dbContext);
            var refModel = brandRule.ShowOrHideBrandProduct(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveBrandProduct(BrandProductCustomModel model, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            var media = new MediaModel
            {
                UrlGuid = mediaObjectKey,
                Name = mediaObjectName,
                Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
            };


            var refModel = new SocialBrandRules(dbContext).SaveBrandProduct(model, media, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadBrandProductById(int id)
        {
            return Json(new SocialBrandRules(dbContext).GetBrandProductById(id), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="qid"></param>
        /// <param name="fileType">Video File,Image File</param>
        /// <returns></returns>
        public ActionResult LoadMediasByBrand(int fid, int qid, string fileType)
        {
            try
            {
                var listMedia = new MediaFolderRules(dbContext).GetMediaItemByFolderId(fid, qid, fileType, CurrentUser().Timezone);
                return PartialView("~/Views/SalesMarketing/_CampaignResources.cshtml", listMedia);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }
        public ActionResult SaveBrandValuePropotion(BrandValuePropositionCustomModel model)
        {            
            return Json(new SocialBrandRules(dbContext).SaveValuePropositon(model, CurrentUser().Id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadValuePropositionByBrand(int brandId, int productId, bool isLoadingHide)
        {
            return PartialView("_ValueProContent", new SocialBrandRules(dbContext).LoadBrandValuePropositonByPId(brandId, productId, isLoadingHide));
        }
        [HttpPost]
        public ActionResult ShowOrHideValueProp(int id)
        {
            var brandRule = new SocialBrandRules(dbContext);
            var refModel = brandRule.ShowOrHideValueProp(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadBrandValueProposionById(int id)
        {
            return Json(new SocialBrandRules(dbContext).GetBrandValuePropositionById(id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadOptionAttributeByBrandId(int brandId)
        {
            return PartialView("_OptionAttributes", new SocialBrandRules(dbContext).LoadBrandAttrGroupsByBrandId(brandId, ""));
        }
        public ActionResult LoadBrandAttrGroupsByBrandId(int brandId, string keyword)
        {
            return PartialView("_AttributeGroupContent", new SocialBrandRules(dbContext).LoadBrandAttrGroupsByBrandId(brandId, keyword));
        }
        public ActionResult SaveBrandAttrGroup(BrandAttributeGroupCustomModel model, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {

            var media = new MediaModel
            {
                UrlGuid = mediaObjectKey,
                Name = mediaObjectName,
                Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
            };

            var refModel = new SocialBrandRules(dbContext).SaveBrandAttrGroup(model, media, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetBrandAttributegroupById(int id)
        {
            return Json(new SocialBrandRules(dbContext).GetBrandAttributegroupById(id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveBrandAttribute(BrandAttributeCustomModel model, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            var refModel = new ReturnJsonModel { result = false };

            var media = new MediaModel
            {
                UrlGuid = mediaObjectKey,
                Name = mediaObjectName,
                Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
            };

            refModel = new SocialBrandRules(dbContext).SaveBrandAttribute(model, media, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadBrandAttributeByBrandId(int brandId, string keyword, bool isLoadingHide)
        {
            return PartialView("_AttributeContent", new SocialBrandRules(dbContext).LoadBrandAttributeByBrandId(brandId, keyword, isLoadingHide));
        }
        [HttpPost]
        public ActionResult ShowOrHideBrandAttribute(int id)
        {
            var brandRule = new SocialBrandRules(dbContext);
            var refModel = brandRule.ShowOrHideBrandAttribute(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadAttributeGroupByBrandId(int brandId)
        {
            return Json(new SocialBrandRules(dbContext).GetAttributeGroupByBrandId(brandId), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetBrandAttributeById(int id)
        {
            return Json(new SocialBrandRules(dbContext).GetBrandAttributeById(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadCampaignsInBrand(int brandId, int[] types, string search, int draw, int start, int length)
        {
            var totalRecord = 0;
            var lstResult = new SocialBrandRules(dbContext).GetListCampaignsInBrand(brandId, types, search, start, length, ref totalRecord);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadBrandElement(int id, string type)
        {
            var rule = new CampaignRules(dbContext);
            if (type.Equals("Email"))
            {
                EmailCampaign campaignEmail = rule.GetEmailCampaignById(id);
                ViewBag.Name = campaignEmail.Name;
                ViewBag.Attributes = campaignEmail.Attributes;
                ViewBag.Propositions = campaignEmail.ValuePropositons;
                ViewBag.Products = campaignEmail.BrandProducts;
            }
            else
            {
                SocialCampaign socialCampaign = rule.GetSocialCampaignById(id);
                ViewBag.Name = socialCampaign.Name;
                ViewBag.Attributes = socialCampaign.Attributes;
                ViewBag.Propositions = socialCampaign.ValuePropositons;
                ViewBag.Products = socialCampaign.BrandProducts;
            }

            return PartialView("_BrandElement");
        }

        public string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
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