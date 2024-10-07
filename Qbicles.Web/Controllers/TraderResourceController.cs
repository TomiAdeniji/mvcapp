using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader.Resources;

using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderResourceController : BaseController
    {
        public ActionResult ListResource()
        {
            return PartialView("_ItemProductResourceTab");
        }
        // Image Resource
        public ActionResult ResourceImagesTab()
        {
            ViewBag.ResourceCategorys = new TraderResourceRules(dbContext).GetListResourceCategory(CurrentDomainId(), ResourceCategoryType.Image);
            return PartialView("_ResourceImagesTab");
        }

        public ActionResult ResourceImageData(int locationId = 0)
        {
            var images = new TraderResourceRules(dbContext).GetListResourceImages(CurrentDomainId());
            return PartialView("_ResourceImageData", images);
        }

        public ActionResult ResourceImageAddEdit(int id = 0)
        {
            ResourceImage image = new ResourceImage();
            if (id > 0)
            {
                image = new TraderResourceRules(dbContext).GetResourceImageById(id);
            }
            ViewBag.ResourceCategorys = new TraderResourceRules(dbContext).GetListResourceCategory(CurrentDomainId(), ResourceCategoryType.Image);
            return PartialView("_ResourceImageAddEdit", image);
        }
        [HttpPost]
        public ActionResult SaveImage(ResourceImage reImage)
        {          
   
            reImage.Domain = CurrentDomain();
            
            var result = new TraderResourceRules(dbContext).SaveImage(reImage, CurrentUser().Id);
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteReImage(int id)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = true, actionVal = 1 };
            refModel = new TraderResourceRules(dbContext).DeleteImage(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        // Document resource
        public ActionResult ResourceDocumentsTab(int locationId = 0)
        {
            ViewBag.ResourceCategorys = new TraderResourceRules(dbContext).GetListResourceCategory(CurrentDomainId(), ResourceCategoryType.Document);
            return PartialView("_ResourceDocumentsTab");
        }
        public ActionResult ResourceDocumentData(int locationId = 0)
        {
            var documents = new TraderResourceRules(dbContext).GetListResourceDocuments(CurrentDomainId());
            return PartialView("_ResourceDocumentData", documents);
        }
        public ActionResult ResourceDocumentAddEdit(int id = 0)
        {
            ResourceDocument document = new ResourceDocument();
            if (id > 0)
            {
                document = new TraderResourceRules(dbContext).GetResourceDocumentById(id);
            }
            ViewBag.ResourceCategorys = new TraderResourceRules(dbContext).GetListResourceCategory(CurrentDomainId(), ResourceCategoryType.Document);
            return PartialView("_ResourceDocumentAddEdit", document);
        }
        [HttpPost]
        public ActionResult SaveDocument(ResourceDocument reDocument)
        {
            reDocument.Domain = CurrentDomain();

           var result = new TraderResourceRules(dbContext).SaveDocument(reDocument, CurrentUser().Id);
            

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteReDocument(int id)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = true, actionVal = 1 };
            refModel = new TraderResourceRules(dbContext).DeleteDocument(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        // Additional info Resource
        public ActionResult ResourceBrandsTab(int locationId = 0, AdditionalInfoType type = AdditionalInfoType.Brand)
        {
            return PartialView("_ResourceBrandsTab", type);
        }

        public ActionResult ResourceBrandData(AdditionalInfoType type = AdditionalInfoType.Brand)
        {
            var additionalInfos = new TraderResourceRules(dbContext).GetListAdditionalInfos(CurrentDomainId(), type);
            ViewBag.Type = type;
            return PartialView("_ResourceBrandData", additionalInfos);
        }
        public ActionResult ResourceBrandAddEdit(int id = 0, AdditionalInfoType type = AdditionalInfoType.Brand)
        {
            // Get list suggested brands
            var lstSuggestedBrands = dbContext.AdditionalInfos.AsNoTracking()
                .Where(p => p.Type == type);//.OrderBy(p => p.Name).ToList();
            switch (type)
            {
                case AdditionalInfoType.Need:
                case AdditionalInfoType.QualityRating:
                    var domainId = CurrentDomainId();
                    lstSuggestedBrands = lstSuggestedBrands.Where(e=>e.Domain.Id == domainId);
                    break;
                default:
                    break;
            }


            ViewBag.ListSuggestedBrands = lstSuggestedBrands.OrderBy(p => p.Name).ToList();

            AdditionalInfo brand = new AdditionalInfo();
            brand.Type = type;
            if (id > 0)
            {
                brand = new TraderResourceRules(dbContext).GetAdditionalInfoById(id);
            }
            return PartialView("_ResourceBrandAddEdit", brand);
        }
        [HttpPost]
        public ActionResult SaveBrand(AdditionalInfo additional)
        {
            additional.Domain = CurrentDomain();
            
           var result = new TraderResourceRules(dbContext).SaveAdditionalInfo(additional, CurrentUser().Id);
            

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteAdditionalInfo(int id)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = true, actionVal = 1 };

            refModel = new TraderResourceRules(dbContext).DeleteAdditionalInfo(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        // Access
        public ActionResult ResourceAccessTab(int locationId = 0)
        {
            return PartialView("_ResourceAccessTab");
        }
        public ActionResult ResourceAccessData(int locationId = 0, string key = "")
        {
            var access = new TraderResourceRules(dbContext).GetListAccessArea(CurrentDomainId(), key);
            return PartialView("_ResourceAccessData", access);
        }
        public ActionResult AccessAddEdit(int id = 0)
        {
            AccessArea access = new AccessArea();
            ViewBag.TraderLocations = CurrentDomain().TraderLocations;
            if (id > 0)
            {
                access = new TraderResourceRules(dbContext).GetAccessAreaById(id);
            }
            return PartialView("_ResourceAccessAddEdit", access);
        }
        [HttpPost]
        public ActionResult SaveAccess(AccessArea access)
        {
            access.Domain = CurrentDomain();
            var result = new TraderResourceRules(dbContext).SaveAccessArea(access, CurrentUser().Id);
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}