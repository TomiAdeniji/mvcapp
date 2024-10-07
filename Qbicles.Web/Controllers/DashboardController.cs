using Qbicles.BusinessRules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class DashboardController : BaseController
    {
        ReturnJsonModel refModel;


        [HttpGet]
        public ActionResult DashboardLoadUserContact(string searchName)
        {
            refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                var lstUser = new List<UserCustom>();
                var partialView = "";
                var cubeId = CurrentQbicleId();
                var lUser = new QbicleRules(dbContext).GetUsersByQbicleId(cubeId);
                var TMP = (from c in lUser
                           select new UserCustom
                           {
                               Forename = c.Forename,
                               Surname = c.Surname,
                               UserName = c.UserName,
                               Id = c.Id,
                               GroupId = 1,
                               ProfilePic = c.ProfilePic
                           }).ToList();
                if ((TMP.Any()))
                    lstUser.AddRange(TMP);
                if (!string.IsNullOrEmpty(searchName))
                    lstUser = lstUser.Where(p => (p.Surname ?? "").ToLower().Contains(searchName.ToLower())
                        || (p.Forename ?? "").ToLower().Contains(searchName.ToLower())
                        ).ToList();
                partialView = RenderViewToString("~/Views/Qbicles/_ModaDashBoardContact.cshtml", lstUser.Distinct().ToList());

                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult RemoveQbicMember(string userId)
        {
            refModel = new QbicleRules(dbContext).RemoveQbicMember(CurrentDomainId(), CurrentUser().Id, userId, CurrentQbicleId());
            return Json(refModel, JsonRequestBehavior.AllowGet);

        }
        public ActionResult AddMemberToQbicle(string userId)
        {
            refModel = new QbicleRules(dbContext).AddMemberToQbicle(CurrentDomainId(), CurrentUser().Id, userId, CurrentQbicleId());
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }



        public ActionResult InviteLoadUserContact(string searchName)
        {
            refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                var lstUser = new List<UserCustom>();
                var partialView = "";
                var cubeId = CurrentQbicleId();
                //var lQbicMember = new QbicleRules(dbContext).GetUsersByQbicleId(cubeId);
                var lUser = (from c in CurrentDomain().Users
                             select new UserCustom
                             {
                                 Forename = c.Forename,
                                 Surname = c.Surname,
                                 UserName = c.UserName,
                                 Id = c.Id,
                                 ProfilePic = c.ProfilePic
                             }).ToList();
                if ((lUser.Any()))
                {
                    // lUser = lUser.Where(p => !lQbicMember.Any(a => a.Id == p.Id)).ToList();
                    lstUser.AddRange(lUser);
                }

                if (!string.IsNullOrEmpty(searchName))
                    lstUser = lstUser.Where(p => (p.Surname ?? "").ToLower().Contains(searchName.ToLower())
                        || (p.Forename ?? "").ToLower().Contains(searchName.ToLower())
                        ).ToList();
                partialView = RenderViewToString("~/Views/Qbicles/_ModaDashBoardContact.cshtml", lstUser.Distinct().ToList());

                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadUserProfile(string UserId)
        {
            refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            try
            {
                var partialView = "";
                var profile = new UserRules(dbContext).GetProfileByUserId(UserId);
                partialView = RenderViewToString("~/Views/Qbicles/_ModaContactProfile.cshtml", profile);
                var cubeId = CurrentQbicleId();
                var lQbicMember = new QbicleRules(dbContext).GetUsersByQbicleId(cubeId);
                refModel.Object = new
                {
                    strResult = partialView

                };
                refModel.Object2 = lQbicMember.Any(a => a.Id == UserId);
                refModel.result = true;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
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