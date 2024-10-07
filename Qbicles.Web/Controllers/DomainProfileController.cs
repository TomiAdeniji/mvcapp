using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Community;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Community;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class DomainProfileController : BaseController
    {
        [HttpPost]
        public ActionResult SaveAndFinish(DomainProfile domainProfile, List<Location> locations, List<CommunityPage> pages)
        {
            var result = new ReturnJsonModel()
            {
                actionVal = 1
            };
            try
            {
                var _domainProfileRule = new DomainProfileRules(dbContext);
                _domainProfileRule.SaveAndFinish(domainProfile, locations, pages,
                    CurrentDomainId(), CurrentUser().Id);
            }
            catch (Exception ex)
            {
                result.actionVal = 2;
                result.msg = ex.Message;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //ProfileTex - logo - cover
        [HttpPost]
        public ActionResult ChangeLogo()
        {
            var resultModel = new ReturnJsonModel();
            resultModel.actionVal = 2;
            resultModel.msg = ResourcesManager._L("ERROR_MSG_20");
            try
            {
                if (Request.Files.Count > 0)
                {
                    HttpPostedFileBase file = Request.Files[0];
                    var message = "";
                    var maxFileSize = 0;
                    var ext = System.IO.Path.GetExtension(file.FileName)?.Replace(".", "").ToLower();
                    if (file != null && !Utility.ValidateFileSize(file.ContentLength, ext, ref message, ref maxFileSize))
                    {
                        resultModel.result = false;
                        resultModel.msg = string.Format(ResourcesManager._L(message), ext.ToUpper(), Utility.BytesToSize(maxFileSize));
                        return Json(resultModel, JsonRequestBehavior.AllowGet);
                    }
                    var mediaModel = CommunityUpdateLogo(file);
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
        public ActionResult ComunityChangeFeatureDomainImage()
        {
            var resultModel = new ReturnJsonModel();
            resultModel.actionVal = 2;
            resultModel.msg = ResourcesManager._L("ERROR_MSG_20");
            try
            {

                if (Request.Files.Count > 0)
                {
                    HttpPostedFileBase file = Request.Files[0];
                    var message = "";
                    var maxFileSize = 0;
                    var ext = System.IO.Path.GetExtension(file.FileName)?.Replace(".", "").ToLower();
                    if (file != null && !Utility.ValidateFileSize(file.ContentLength, ext, ref message, ref maxFileSize))
                    {
                        resultModel.result = false;
                        resultModel.msg = string.Format(ResourcesManager._L(message), ext.ToUpper(), Utility.BytesToSize(maxFileSize));
                        return Json(resultModel, JsonRequestBehavior.AllowGet);
                    }
                    var mediaModel = CommunityChangeFeatureDomainImage(file);
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
        public ActionResult UpdateProfileText(string profileText)
        {
            var resultModel = new ReturnJsonModel();
            try
            {
                var fp = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(profileText);
                var page = new DomainProfileRules(dbContext).GetDomainProfile(CurrentDomainId());
                page.ProfileText = fp.Trim();
                if (dbContext.Entry(page).State == EntityState.Detached)
                    dbContext.DomainProfiles.Attach(page);
                dbContext.Entry(page).State = EntityState.Modified;
                dbContext.SaveChanges();
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
        public MediaModel CommunityUpdateLogo(HttpPostedFileBase file)
        {
            var mediaModel = new MediaModel();
            try
            {
                if (!string.IsNullOrEmpty(file.FileName))
                {

                    //mediaModel = StoreFile(file, Enums.RepositoryItemType.Community);
                    new DomainProfileRules(dbContext).CommunityUpdateLogo(mediaModel, CurrentDomainId());
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
            }

            return mediaModel;
        }
        public MediaModel CommunityChangeFeatureDomainImage(HttpPostedFileBase file)
        {
            var mediaModel = new MediaModel();
            //try Update when open Community
            //{
            //    if (!string.IsNullOrEmpty(file.FileName))
            //    {

            //        mediaModel = StoreFile(file, Enums.RepositoryItemType.Community);                    
            //        new DomainProfileRules(dbContext).ComunityChangeFeatureDomainImage(mediaModel, CurrentDomainId());
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
            //}
            return mediaModel;
        }

        public ActionResult BuildLocation(Location location)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                var locationId = Guid.NewGuid();
                var html = new StringBuilder();
                html.Append($"<div id='location-{locationId}' class='office-location'>");
                html.Append($"<div class='edit_entry editProfile'>");
                html.Append($"<a class='btn btn-warning' " +
                    $"onclick = \"editLocation('{locationId}')\">");
                html.Append($"<i class='fa fa-pencil'></i></a>  ");
                html.Append($"<a onclick = \"deleteLocation('{locationId}')\" class='btn btn-danger'>");
                html.Append($"<i class='fa fa-trash'></i></a></div>");
                html.Append($"<i class='fa fa-map-marker'></i>");
                html.Append($"<input class='location_id' value='{locationId}' hidden/>");
                html.Append($"<p>");
                html.Append($"<strong id='name-{locationId}' class='location_name' >{location.Name}</strong>");
                html.Append("</p><br><br>");

                html.Append($"<p id='address-{locationId}' class='location_address'  style='margin: 0 0 0 10px;'>");
                html.Append($"{location.Address.Replace("\n", "<br/>")}");
                html.Append("</p>");

                html.Append($"</div><div class='clearfix'></div><br>");
                refModel.msg = html.ToString();
                refModel.result = true;

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult FollowDomain(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new DomainProfileRules(dbContext).FollowDomain(id, this.CurrentUser().Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            refModel.result = true;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UnFollowDomain(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new DomainProfileRules(dbContext).UnFollowDomain(id, this.CurrentUser().Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            refModel.result = true;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FinishBusinessProfileWizard(string domainKey)
        {
            return Json(new DomainProfileRules(dbContext).FinishBusinessProfileWizard(domainKey, true));
        }

        public ActionResult BusinessProfileWizardGeneral(B2BProfileWizardModel profile)
        {
            return Json(new DomainProfileRules(dbContext).BusinessProfileWizardGeneral(profile, CurrentUser().Id));
        }


        public ActionResult InvitationUser(string email, int domainId)
        {
            try
            {
                return Json(new UserRules(dbContext).InviteUserJoinQbicles(email, "", domainId, GenerateCallbackUrl(),
                    CurrentUser().Id, true), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult GetDomainUsersInvitation([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int domainId)
        {
            var invited = new DomainProfileRules(dbContext).GetDomainUsersInvitation(requestModel, domainId);
            if (invited != null)
                return Json(invited, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }


        public ActionResult ValidationBusinessName(string name, string id)
        {
            var valid = new DomainProfileRules(dbContext).ValidationBusinessName(name, id);
            return Json(new ReturnJsonModel { result = valid }, JsonRequestBehavior.AllowGet);
        }
    }
}