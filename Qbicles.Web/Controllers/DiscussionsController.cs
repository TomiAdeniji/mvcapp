using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class DiscussionsController : BaseController
    {
        //ApplicationDbContext dbContext = new ApplicationDbContext();
        private ReturnJsonModel refModel;

        /*Add/ edit Discussion*/

        public ActionResult DuplicateDiscussionNameCheck(int cubeId, int discussionid, string discussionName)
        {
            refModel = new ReturnJsonModel();
            try
            {
                DiscussionsRules qb = new DiscussionsRules(dbContext);
                refModel.result = qb.DuplicateDiscussionNameCheck(cubeId, discussionid, discussionName);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                refModel.result = false;
                refModel.Object = ex;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Discussion for Idea
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        public ActionResult SaveDiscussion(DiscussionIdeaCustomeModel model)
        {
            return Json(new DiscussionsRules(dbContext).SaveDiscussionForIdea(model, CurrentDomainId(), CurrentUser()), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Discussion for Place
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        public ActionResult SaveDiscussionForPlace(DiscussionPlaceModel model)
        {
            return Json(new DiscussionsRules(dbContext).SaveDiscussionForPlace(model, CurrentDomainId(), CurrentUser()), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Discussion for Goal
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        public ActionResult SaveDiscussionForGoal(DiscussionGoalModel model)
        {
            return Json(new DiscussionsRules(dbContext).SaveDiscussionForGoal(model, CurrentDomainId(), CurrentUser()), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Discussion for Perfomance tracking
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        public ActionResult SaveDiscussionForPerformance(DiscussionPerfomanceModel model)
        {
            return Json(new DiscussionsRules(dbContext).SaveDiscussionForPerformance(model, CurrentDomainId(), CurrentUser()), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Discussion for Perfomance tracking
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        public ActionResult SaveDiscussionForComplianceTask(DiscussionComplianceTaskModel model)
        {
            return Json(new DiscussionsRules(dbContext).SaveDiscussionForComplianceTask(model, CurrentDomainId(), CurrentUser()), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Discussion for Qbicle stream
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<ActionResult> SaveDiscussionForQbicle(DiscussionQbicleModel model)
        {
            refModel = new ReturnJsonModel();
            DefaultMedia img = null;
            model.QbicleId = CurrentQbicleId();
            model.Id = string.IsNullOrEmpty(model.Key) ? 0 : int.Parse(model.Key.Decrypt());

            var isDiscussionExist = new DiscussionsRules(dbContext).DuplicateDiscussionNameCheck(model.QbicleId, model.Id, model.Title);
            if (isDiscussionExist)
            {
                refModel.result = false;
                refModel.Object = new { Title = ResourcesManager._L("ERROR_MSG_393") };
                return Json(refModel);
            }
            model.Media = new MediaModel { IsPublic = false };
            if (model.FeaturedOption == 2)
            {
                model.Media.UrlGuid = model.UploadKey;
            }
            else if (model.FeaturedOption == 1 && !model.MediaDiscussionUse.Equals("0"))
            {
                var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
                img = HelperClass.GetListDefaultMedia(domainLink).FirstOrDefault(i => i.Id == model.MediaDiscussionUse);

                var uriKey = await UploadMediaFromPath(img.FileName, img.FilePath);

                model.Media.UrlGuid = uriKey;
            }
            else if (model.MediaDiscussionUse.Equals("0") && model.Id == 0)
            {
                refModel.result = false;
                refModel.msg = string.Format(ResourcesManager._L("ERROR_MSG_403"));
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }

            refModel = new DiscussionsRules(dbContext).SaveDiscussionForQbicle(model, CurrentUser(), IsCreatorTheCustomer(), GetOriginatingConnectionIdFromCookies(), AppType.Web);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /*End add/ edit Discussion*/

        public ActionResult SetDiscussionSelected(string key, string goBack)
        {
            var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(EncryptionService.Decrypt(key));

            //Check for activity accessibility
            var checkResult = new QbicleRules(dbContext).CheckActivityAccibility(id, CurrentUser().Id);
            if (checkResult.result && (bool)checkResult.Object == true)
            {
                refModel = new ReturnJsonModel();
                this.SetCurrentDiscussionIdCookies(id);
                this.SetCookieGoBackPage(goBack);
                this.SetCookieGoBackSubActivity(goBack);

                //var dis = new DiscussionsRules(dbContext).GetDiscussionById(id);
                refModel.result = true;

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(checkResult, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult MyDeskLoadMoreDiscussion(int skip, int folderId)
        {
            try
            {
                string currUserId = CurrentUser().Id;
                List<QbicleDiscussion> discuss = new DiscussionsRules(dbContext).GetDiscussionByUserId(currUserId, folderId, skip).BusinessMapping(CurrentUser().Timezone);

                var querry = from pin in dbContext.MyPins
                             join desk in dbContext.MyDesks on pin.Desk.Id equals desk.Id
                             where desk.Owner.Id == currUserId && pin.PinnedActivity.ActivityType ==
                                QbicleActivity.ActivityTypeEnum.DiscussionActivity
                             select pin.PinnedActivity;

                var myPinnedDiscussions = querry.ToList();

                var result = discuss.Select(x => new
                {
                    IsPinned = myPinnedDiscussions.Any(y => y.Id == x.Id),
                    DisId = x.Id,
                    IsMember = (x.ActivityMembers.Any(m => m.Id == currUserId) || (x.StartedBy.Id == currUserId)) ? true : false,
                    QbicleName = x.Qbicle != null ? x.Qbicle.Name : "",
                    DomainName = x.Qbicle != null ? x.Qbicle.Domain.Name : "",
                    DisName = x.Name,
                    State = x.State == QbicleActivity.ActivityStateEnum.Open ? "In progress" : "Done",
                    NewReplies = x.Posts.Where(p => p.StartedDate.Date == DateTime.UtcNow.Date).Count(),
                    NewTask = x.SubActivities.Where(t => t.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity && t.StartedDate.Date == DateTime.UtcNow.Date & t.State == QbicleActivity.ActivityStateEnum.Open).Count()
                }).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        /// Create user for Domain
        /// </summary>
        /// <param name="userAccount"></param>
        /// <returns></returns>

        public ActionResult AddNewGuestToDiscussion(string email)
        {
            try
            {
                refModel = new ReturnJsonModel();
                var domain = CurrentDomain();
                var guest = new DiscussionsRules(dbContext).CreateNewGuestToDiscussion(CurrentDiscussionId(), email, CurrentUser());

                if (guest == null)
                {
                    refModel.msg = string.Format("The email invite '{0}' as user in the system. Please chose 'Add new Participant'!", email);
                    refModel.result = false;
                }
                else
                {
                    //sends the user the 'Token' email to tell them that they have been invited to join Qbicles.
                    //send here
                    var userSetting = CurrentUser();
                    string callbackUrl = GenerateUrlToken(guest.Id, domain.Id, QbicleActivity.ActivityTypeEnum.DiscussionActivity, userSetting.Email);
                    bool emailInvited = new EmailRules(dbContext).SendEmailInvitedGuest(userSetting.Id, guest.Email,
                                                                  callbackUrl, QbicleActivity.ActivityTypeEnum.Domain, domain.Name, "");
                }
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        /// Add new participant to the discussion member
        /// </summary>
        /// <param name="usersDomainAssign">string[] userid</param>
        /// <returns></returns>

        public ActionResult AddNewParticipantToDiscussion(string[] usersDomainAssign)
        {
            try
            {
                refModel = new ReturnJsonModel
                {
                    result = new DiscussionsRules(dbContext).
                    CreateNewParticipant(usersDomainAssign, CurrentDomain(), CurrentDiscussionId(), CurrentUser())
                };
                if (!refModel.result)
                    refModel.msg = ResourcesManager._L("ERROR_MSG_16");
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SaveB2BCatalogDiscussion(B2BCatalogDiscussionModel discussionModel)
        {
            discussionModel.QbicleId = CurrentQbicleId();
            var rs = new DiscussionsRules(dbContext).SaveB2BCatalogDiscussion(discussionModel, CurrentDomainId(), CurrentUser().Id);
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
    }
}