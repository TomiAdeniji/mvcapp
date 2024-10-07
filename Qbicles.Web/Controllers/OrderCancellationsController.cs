using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class OrderCancellationController : BaseController
    {
        public ActionResult TraderPosOrderCancellations([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string[] cashiers, string[] managers, string datetime)
        {
            keyword = keyword.Trim().ToLower();
            var result = new OrderCancellationRules(dbContext).GetTraderPosOrderCancellations(requestModel, keyword.ToLower(), cashiers, managers, datetime, CurrentLocationManage(), CurrentUser());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<PosOrderTypeCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult OrderCancellationItemDetail(string cancelKey)
        {
            ViewBag.CurrencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
            var order = new OrderCancellationRules(dbContext).OrderCancellationItemDetail(cancelKey);
            ViewBag.Voucher = dbContext.Vouchers.AsNoTracking().FirstOrDefault(e => e.Id == order.VoucherId) ?? new Qbicles.Models.Loyalty.Voucher();
            return PartialView("_OrderCancellationItemDetail", order);
        }

        public ActionResult AddOrderCancellationDiscussion(string cancelKey)
        {
            var id = int.Parse(cancelKey.Decrypt());
            var cancel = dbContext.PosOrderCancels.FirstOrDefault(e => e.Id == id);

            var qbicle = dbContext.PosSettings.AsNoTracking().FirstOrDefault(e => e.Location.Id == cancel.Location.Id)?.DefaultWorkGroup.Qbicle;

            var discussion = new QbicleDiscussion();
            ViewBag.CurrentQbicle = qbicle;
            ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(qbicle.Id);

            var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
            ViewBag.DefaultMedia = HelperClass.GetListDefaultMedia(domainLink);
            ViewBag.FeaturedImageId = (dbContext.StorageFiles.FirstOrDefault(s => s.Id == discussion.FeaturedImageUri)?.Name ?? "0").Replace(".jpg", "");
            return PartialView("_ModalOrderCancellationDiscussion", discussion);
        }

        [HttpPost]
        public async Task<ActionResult> SaveOrderCancellationDiscussion(string cancelKey, DiscussionQbicleModel model)
        {
            model.Media = new MediaModel { IsPublic = false };
            if (model.FeaturedOption == 2)
            {
                model.Media.UrlGuid = model.UploadKey;
            }
            else if (model.FeaturedOption == 1 && !model.MediaDiscussionUse.Equals("0"))
            {
                var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
                var img = HelperClass.GetListDefaultMedia(domainLink).FirstOrDefault(i => i.Id == model.MediaDiscussionUse);

                var uriKey = await UploadMediaFromPath(img.FileName, img.FilePath);
                model.Media.UrlGuid = uriKey;
            }
            else if (model.MediaDiscussionUse.Equals("0") && model.Id == 0)
                return Json(new ReturnJsonModel { result = false, msg = string.Format(ResourcesManager._L("ERROR_MSG_403")) }, JsonRequestBehavior.AllowGet);

            var save = new OrderCancellationRules(dbContext).SaveOrderCancellationDiscussion(model, cancelKey, CurrentUser());
            return Json(save, JsonRequestBehavior.AllowGet);
        }
    }
}