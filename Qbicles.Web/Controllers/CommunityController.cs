using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.ProfilePage;
using Qbicles.BusinessRules.BusinessRules.UserInformation;
using Qbicles.BusinessRules.Community;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Community;
using Qbicles.Models.UserInformation;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class CommunityController : BaseController
    {
        public ActionResult CommunityApps()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                ViewBag.UserRoleRights = userRoleRights;

                ClearAllCurrentActivities();
                ViewBag.CurrentPage = "Community";SetCurrentPage("Community");

                //get domain current
                var domainProfile = new DomainProfileRules(dbContext).GetDomainProfile(CurrentDomainId());
                if (domainProfile == null)
                    return View("Error");
                if (domainProfile.Id == 0) domainProfile.Domain = CurrentDomain();
                ViewBag.DomainProfile = domainProfile;

                var countPages = 0;

                var pages = new List<ModelPage>();
                // get list communityPage  
                var communities = new CommunityPageRules(dbContext).GetAllCommunityPages();

                if (communities != null)
                {
                    countPages += communities.Count();
                    pages.AddRange(communities.Take(4).Select(q => new ModelPage
                    {
                        Id = q.Id,
                        CreatedDate = q.CreatedDate,
                        BodyText = q.BodyText,
                        DomainName = q.Domain.Name,
                        PageType = q.PageType,
                        StrapLine = string.Empty,
                        Title = q.Title,
                        Domain = q.Domain,
                        FeaturedImage = q.FeaturedImage,
                        FeaturedImageCaption = q.FeaturedImageCaption,
                        Tags = q.Tags
                    }).ToList());
                }

                // get list domainProfile
                var lstDomain = new DomainProfileRules(dbContext).GetAllDomainProfiles();
                if (lstDomain != null)
                {
                    countPages += lstDomain.Count();
                    pages.AddRange(lstDomain.Take(4).Select(q => new ModelPage
                    {
                        Id = q.Id,
                        CreatedDate = q.CreatedDate,
                        BodyText = string.Empty,
                        DomainName = q.Domain.Name,
                        PageType = q.PageType,
                        StrapLine = q.StrapLine,
                        Title = string.Empty,
                        Domain = q.Domain,
                        StoredLogoName = q.StoredLogoName,
                        StoredFeaturedImageName = q.StoredFeaturedImageName,
                        Tags = q.Tags
                    }).ToList());
                }


                if (pages.Count > 0)
                    pages = pages.OrderByDescending(q => q.CreatedDate).ToList();
                else
                    pages.Add(new ModelPage { PageType = CommunityPageTypeEnum.CommunityPage, Domain = CurrentDomain() });
                ViewBag.CountPage = countPages;
                ViewBag.Pages = pages;
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult DomainProfile(int domainid = 0)
        {
            try
            {
                if (domainid == 0) domainid = CurrentDomainId();
                ViewBag.ThisDomain = domainid == CurrentDomainId();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainid);

                ViewBag.UserRoleRights = userRoleRights;
                var domainProfile = new DomainProfileRules(dbContext).GetDomainProfile(domainid);
                if (domainProfile.Id == 0)
                    return RedirectToAction("DomainProfileSetup");
                ClearAllCurrentActivities();
                ViewBag.CurrentPage = "Community";SetCurrentPage("Community");
                ViewBag.CommunityPages =
                    new CommunityPageRules(dbContext).GetCommunityPages(domainid)
                        .Where(d => d.IsDisplayedOnDomainProfile).OrderBy(o => o.DisplayOrderOnDomainProfile).ToList();
                ViewBag.Locations = new LocationRules(dbContext).GetLocations(domainProfile.Id);
                return View(domainProfile);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult UserProfilePage(int id = 0, string uId = "")
        {
            try
            {
                if (string.IsNullOrEmpty(uId))
                {
                    uId = CurrentUser().Id;
                }

                var currentUserId = CurrentUser().Id;
                ViewBag.lstShortlistGroups = dbContext.ShortListGroups.Where(p => p.AssociatedUser.Id == currentUserId).ToList();
                var c2cQbicleConnected = dbContext.C2CQbicles.Any(p => p.Customers.Any(x => x.Id == uId) && p.Customers.Any(x => x.Id == currentUserId));
                var isC2CConnectionAccepted = dbContext.C2CQbicles.Any(p => p.Customers.Any(x => x.Id == uId) && p.Customers.Any(x => x.Id == currentUserId) && p.Status == CommsStatus.Approved);
                ViewBag.isC2CConnected = c2cQbicleConnected;
                ViewBag.isC2CConnectionAccepted = isC2CConnectionAccepted;

                var userSettings = CurrentUser();
                var user = dbContext.QbicleUser.Find(uId);
                ViewBag.User = user;
                var showcaseTotal = 0;
                var lstShowcases = new UserInformationRules(dbContext).GetListShowCase(uId, string.Empty, ref showcaseTotal);
                ViewBag.Showcases = lstShowcases;
                var lstSkills = new UserInformationRules(dbContext).GetSkillsByUser(uId);
                ViewBag.Skills = lstSkills;
                var lstWorkExps = new UserInformationRules(dbContext).GetListUserExp(uId, ExperienceType.WorkExperience, userSettings.Timezone);
                ViewBag.WorkExp = lstWorkExps;
                var lstEduExps = new UserInformationRules(dbContext).GetListUserExp(uId, ExperienceType.EducationExperience, userSettings.Timezone);
                ViewBag.EduExp = lstEduExps;
                var lstUserFiles = new UserInformationRules(dbContext).GetUserPublicFiles(uId, string.Empty);
                ViewBag.PublicFiles = lstUserFiles;
                //var lstSharedQbicles = new QbicleRules(dbContext).GetQbiclesForProfileByUserId(currentUserId, uId);
                //ViewBag.SharedQbicles = lstSharedQbicles ?? new List<Qbicle>();
                ViewBag.CurrentUserId = currentUserId;
                ViewBag.IsPreview = true;//Enable Preview BlockTemplate
                ViewBag.UserPages = new UserProfileRules(dbContext).GetUserPages(uId);
                return View("~/Views/UserInformation/PublicProfileView.cshtml");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult UserProfileSetupPage()
        {
            try
            {
                var userProfilePageRules = new UserProfilePageRules(dbContext);
                var tagRules = new TagRules(dbContext);
                ClearAllCurrentActivities();
                var communities = userProfilePageRules.GetUserProfilePage(CurrentUser().Id) ?? userProfilePageRules.CreateProfilePage(
                                      CurrentDomain().Users.FirstOrDefault(q => q.Id == CurrentUser().Id));
                ViewBag.CurrentPage = "Community";SetCurrentPage("Community");
                ViewBag.SkillTags = tagRules.GetTags();
                ViewBag.UserProfilePageTag = tagRules.GetTags();
                return View(communities);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult DomainProfileSetup()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.CMAddEditDomainProfile))
                    return View("ErrorAccessPage");
                ViewBag.UserRoleRights = userRoleRights;

                var domainProfileRules = new DomainProfileRules(dbContext);
                ClearAllCurrentActivities();
                ViewBag.CurrentPage = "Community";SetCurrentPage("Community");
                var domainProfile = domainProfileRules.GetDomainProfile(CurrentDomainId());
                domainProfile.Domain = CurrentDomain();
                ViewBag.CommunityPages =
                    new CommunityPageRules(dbContext).GetCommunityPages(CurrentDomainId())
                        .OrderBy(o => o.DisplayOrderOnDomainProfile).ToList();
                ViewBag.Tags = new TagRules(dbContext).GetTags();
                ViewBag.Locations = new LocationRules(dbContext).GetLocations(domainProfile.Id);
                return View(domainProfile);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult CommunityList(string name = "businesses", string lstBusId = "", string lstPeoId = "",
            string lstPageId = "")
        {
            ViewBag.Name = name;
            //businesses - domain
            if (name == "businesses")
                ViewBag.Tags = new TagRules(dbContext).GetTagByDomain();
            else if (name == "people") //people - userprofile
                ViewBag.Tags = new TagRules(dbContext).GetTagByUserProfile();
            else if (name == "pages") //page - page
                ViewBag.Tags = new TagRules(dbContext).GetTagByPage();
            ViewBag.StringBusId = lstBusId;
            ViewBag.StringPeoId = lstPeoId;
            ViewBag.StringPageId = lstPageId;
            return View();
        }

        public ActionResult BusinessesTable(string lstId = "", bool callback = false, string tagIds = null,
            string key = "")
        {
            ViewBag.CallBack = callback;

            if (string.IsNullOrEmpty(lstId))
            {
                ViewBag.Domains = new List<DomainProfile>();
            }
            else
            {
                var lstIdDomain = lstId.Split(',').Select(int.Parse).ToList();
                var domains = new DomainProfileRules(dbContext).GetDomainProfileByListId(lstIdDomain);

                if (!string.IsNullOrEmpty(tagIds))
                {
                    var lstTagId = tagIds.Split(',').Select(int.Parse).ToList();
                    domains = domains
                        .Where(q => q.Tags.Select(x => x.Id).ToList().Any(v => lstTagId.Contains(v))).ToList();
                }

                if (!string.IsNullOrEmpty(key))
                    domains = domains.Where(q => q.Domain.Name.ToLower().Contains(key.ToLower())).ToList();
                ViewBag.Domains = domains;
            }

            return PartialView("_BusinessesTable");
        }

        public ActionResult PeopleTable(string lstId = "", bool callback = false, string tagIds = null, string key = "")
        {
            ViewBag.CallBack = callback;
            if (string.IsNullOrEmpty(lstId))
            {
                ViewBag.UserProfiles = new List<UserProfile>();
            }
            else
            {
                var lstIdUser = lstId.Split(',').Select(int.Parse).ToList();
                var userProfiles = new UserProfilePageRules(dbContext).GetUserProfilePageByListId(lstIdUser);
                if (!string.IsNullOrEmpty(tagIds))
                {
                    var lstTagId = tagIds.Split(',').Select(int.Parse).ToList();
                    userProfiles = userProfiles
                        .Where(q => q.Tags.Select(x => x.Id).ToList().Any(v => lstTagId.Contains(v))).ToList();
                }

                if (!string.IsNullOrEmpty(key))
                    userProfiles = userProfiles.Where(q =>
                            (q.AssociatedUser.Forename + " " + q.AssociatedUser.Surname).ToLower()
                            .Contains(key.ToLower()))
                        .ToList();
                ViewBag.UserProfiles = userProfiles;
            }

            return PartialView("_PeopleTable");
        }

        public ActionResult PagesTable(string lstId = "", bool callback = false, string tagIds = null, string key = "")
        {
            ViewBag.CallBack = callback;
            ViewBag.UserId = CurrentUser().Id;
            if (string.IsNullOrEmpty(lstId))
            {
                ViewBag.Pages = new List<CommunityPage>();
            }
            else
            {
                var lstIdPages = lstId.Split(',').Select(int.Parse).ToList();
                var pages = new CommunityPageRules(dbContext).GetCommunityPageByListId(lstIdPages);
                if (!string.IsNullOrEmpty(tagIds))
                {
                    var lstTagId = tagIds.Split(',').Select(int.Parse).ToList();
                    pages = pages.Where(q => q.Tags.Select(x => x.Id).ToList().Any(v => lstTagId.Contains(v)))
                        .ToList();
                }

                if (!string.IsNullOrEmpty(key))
                    pages = pages.Where(q => q.Title.ToLower().Contains(key.ToLower())).ToList();
                ViewBag.Pages = pages;
            }

            return PartialView("_PagesTable");
        }

        public ActionResult SystemCommunityPages()
        {
            var Pages = new CommunityPageRules(dbContext).GetCommunityPagesOrder(CurrentDomainId());
            return PartialView("_CommunityPages", Pages);
        }

        [HttpPost]
        public ActionResult UpdateIsFeature(int Id, bool value = false)
        {
            var result = new ReturnJsonModel();
            try
            {
                result.actionVal = 1;
                new CommunityPageRules(dbContext).UpdateIsFeature(Id, value);
            }
            catch (Exception ex)
            {
                result.actionVal = 2;
                result.msg = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateOrderpages(List<int> lstId, List<int> lstOrder)
        {
            var result = new ReturnJsonModel();
            try
            {
                result.actionVal = 1;
                new CommunityPageRules(dbContext).UpdateOrderPages(lstId, lstOrder);
            }
            catch (Exception ex)
            {
                result.actionVal = 2;
                result.msg = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SystemCommunityTags()
        {
            var Tags = new TagRules(dbContext).GetTags();
            return PartialView("_CommunityTags", Tags);
        }

        public ActionResult CommunityPageTable()
        {
            // get list communityPage  
            var communities = new CommunityPageRules(dbContext).GetCommunityPages(CurrentDomainId());
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            ViewBag.UserRoleRights = userRoleRights;
            return PartialView("_CommunityPageTable", communities);
        }

        public ActionResult CommunityFollowerTable()
        {
            var currentUserId = CurrentUser().Id;
            var modelPages = new List<ModelPage>();
            var domainProfileUser = new DomainProfileRules(dbContext).GetByUser(currentUserId);
            var communityPageByUser = new CommunityPageRules(dbContext).GetByUser(currentUserId);
            if (domainProfileUser.Any())
                modelPages.AddRange(domainProfileUser.Select(q => new ModelPage
                {
                    Id = q.Id,
                    BodyText = string.Empty,
                    CreatedDate = q.CreatedDate,
                    DomainName = q.Domain.Name,
                    Followers = q.Followers.Count,
                    PageType = q.PageType,
                    StrapLine = q.StrapLine,
                    Title = string.Empty
                }));
            if (communityPageByUser.Any())
                modelPages.AddRange(communityPageByUser.Select(q => new ModelPage
                {
                    Id = q.Id,
                    BodyText = q.BodyText,
                    CreatedDate = q.CreatedDate,
                    DomainName = q.Domain.Name,
                    Followers = q.Followers.Count,
                    PageType = q.PageType,
                    StrapLine = string.Empty,
                    Title = q.Title
                }));
            return PartialView("_CommunityFollowers", modelPages);
        }

        public ActionResult SystemCommunityProfiles()
        {
            return PartialView("_CommunityProfiles");
        }

        public ActionResult CreatePage()
        {
            ViewBag.CurrentPage = "Community";SetCurrentPage("Community");
            var tags = new TagRules(dbContext).GetTagForAdminCreate();
            ViewBag.Qbicles = CurrentDomain().Qbicles;

            ViewBag.Tags = tags;
            var jsonTags = new JavaScriptSerializer().Serialize(tags.Select(q => new
            {
                q.Id,
                q.Name,
                ListKeyWord = q.AssociatedKeyWords.Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToList()
            }).ToList());
            ViewBag.TagJson = jsonTags;
            return View();
        }

        public ActionResult EditPage(int edit)
        {
            ClearAllCurrentActivities();
            ViewBag.CurrentPage = "Community";SetCurrentPage("Community");
            var communityPage = new CommunityPageRules(dbContext).GetCommunityPageById(edit);
            if (communityPage == null)
                return View("Error");
            var tags = new TagRules(dbContext).GetTagForAdminCreate();

            SetCCommunityPageIdCookies(edit);
            ViewBag.Tags = tags;
            var jsonTags = new JavaScriptSerializer().Serialize(tags.Select(q => new
            {
                q.Id,
                q.Name,
                ListKeyWord = q.AssociatedKeyWords.Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToList()
            }).ToList());
            ViewBag.TagJson = jsonTags;
            return View(communityPage);
        }

        public ActionResult CommunityPage(int id)
        {
            ClearAllCurrentActivities();
            ViewBag.UserCommunitySubscription = new UserRules(dbContext).GetById(CurrentUser().Id)?.CommunitySubscription;
            ViewBag.CurrentPage = "Community";SetCurrentPage("Community");
            var communityPage = new CommunityPageRules(dbContext).GetCommunityPageById(id);
            if (communityPage == null)
                return View("Error");
            SetCCommunityPageIdCookies(id);
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, communityPage.Domain.Id);
            ViewBag.UserRoleRights = userRoleRights;
            SetUserProfilePageId(communityPage.CreatedBy.Id);
            return View(communityPage);
        }

        public ActionResult CommunityActivities(Qbicle cube, string hidenCss, CommunityPage page = null)
        {
            ViewBag.hidenCss = hidenCss;
            ViewBag.page = page;
            return PartialView("_CommunityActivities", cube);
        }


        public ActionResult LoadMoreAlerts(int id, int size)
        {
            try
            {
                var cube = new QbicleRules(dbContext).GetQbicleById(id);
                var alerts = cube.Activities.Where(a => a.ActivityType == QbicleActivity.ActivityTypeEnum.AlertActivity)
                    .OrderByDescending(d => d.TimeLineDate).Skip(size).Take(5).ToList();
                if (alerts.Count > 0)
                    return PartialView("_Alerts", alerts);
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return null;
            }
        }

        public ActionResult LoadMoreMedias(int id, int size)
        {
            try
            {
                var cube = new QbicleRules(dbContext).GetQbicleById(id);
                var medias = cube.Activities.Where(a => a.ActivityType == QbicleActivity.ActivityTypeEnum.MediaActivity)
                    .OrderByDescending(d => d.TimeLineDate).Skip(size).Take(5).ToList();
                if (medias.Count > 0)
                    return PartialView("_Medias", medias);
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return null;
            }
        }

        public ActionResult LoadMoreEvents(int id, int size)
        {
            try
            {
                var cube = new QbicleRules(dbContext).GetQbicleById(id);
                var events = cube.Activities.Where(a => a.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity)
                    .OrderByDescending(d => d.TimeLineDate).Skip(size).Take(5).ToList();
                if (events.Count > 0)
                    return PartialView("_Events", events);
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return null;
            }
        }

        public ActionResult LoadMorePosts(string key, int size)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
                var posts = new PostsRules(dbContext).GetPosts(id);
                posts = posts.OrderByDescending(d => d.TimeLineDate).Skip(size).Take(5).ToList();
                if (posts.Count > 0)
                    return PartialView("_Posts", posts);
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return null;
            }
        }

        public ActionResult FollowPage(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new CommunityPageRules(dbContext).FollowPage(id, CurrentUser().Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UnFollowPage(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new CommunityPageRules(dbContext).UnFollowPage(id, CurrentUser().Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult UnFollower(int id, CommunityPageTypeEnum type)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (type == CommunityPageTypeEnum.CommunityPage)
                    refModel.result = new CommunityPageRules(dbContext).UnFollowPage(id, CurrentUser().Id);
                else if (type == CommunityPageTypeEnum.DomainProfile)
                    refModel.result = new DomainProfileRules(dbContext).UnFollowDomain(id, CurrentUser().Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Follower(int id, CommunityPageTypeEnum type)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (type == CommunityPageTypeEnum.CommunityPage)
                    refModel.result = new CommunityPageRules(dbContext).FollowPage(id, CurrentUser().Id);
                else if (type == CommunityPageTypeEnum.DomainProfile)
                    refModel.result = new DomainProfileRules(dbContext).FollowDomain(id, CurrentUser().Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Search(List<string> lstKeyWord)
        {
            var result = new ReturnJsonModel();
            try
            {
                lstKeyWord = lstKeyWord.Where(q => q.Trim().Length > 0).Select(x => x.Trim().ToLower()).ToList();

                var lstTagId = new TagRules(dbContext).GetListTagIdByListKeyWord(lstKeyWord);
                var lstTagSkillId = new TagRules(dbContext).GetListTagIdByListKeyWordSkill(lstKeyWord);
                var resultModel = new List<object>();
                if (lstTagId != null)
                {
                    var userProfiles = new UserProfilePageRules(dbContext).Search(lstTagId, lstTagSkillId);
                    resultModel.Add(new { PageType = CommunityPageTypeEnum.UserProfile, ListId = userProfiles });
                    var domainProfiles = new DomainProfileRules(dbContext).Search(lstTagId);
                    resultModel.Add(new { PageType = CommunityPageTypeEnum.DomainProfile, ListId = domainProfiles });
                    var communityPages = new CommunityPageRules(dbContext).Search(lstTagId);
                    resultModel.Add(new { PageType = CommunityPageTypeEnum.CommunityPage, ListId = communityPages });
                }

                result.actionVal = 1;
                result.Object = resultModel;
            }
            catch (Exception ex)
            {
                result.actionVal = 2;
                result.msg = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UserProfilePreview()
        {
            try
            {
                var userId = CurrentUser().Id;
                var domains = dbContext.Domains.Where(q => q.Users.Any(x => x.Id == userId)).ToList();
                var lstDomains = new List<QbicleDomain>();
                lstDomains.AddRange(domains);
                foreach (var item in lstDomains)
                {
                    var profile = new DomainProfileRules(dbContext).GetDomainProfile(item.Id);
                    if (profile.Id == 0) domains.Remove(item);
                }

                var userProfilePageRules = new UserProfilePageRules(dbContext);
                ClearAllCurrentActivities();
                ViewBag.CurrentPage = "Community";SetCurrentPage("Community");

                ViewBag.Domains = domains;
                var userProfile = userProfilePageRules.GetUserProfilePage(UserProfilePageId());

                return View(userProfile);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
    }
}