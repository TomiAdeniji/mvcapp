using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.BusinessRules.MarketingSocial;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.SalesMkt;
using Qbicles.Web.Models;
using static Qbicles.BusinessRules.HelperClass;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class SalesMarketingController : BaseController
    {
        private ReturnJsonModel refModel;

        [HttpGet]
        public ActionResult CheckSMSetup()
        {
            return Json(CheckSMIsSetupComplete(), JsonRequestBehavior.AllowGet);
        }

        private bool CheckSMIsSetupComplete()
        {
            return new SalesMaketingSettingRules(dbContext).CheckSMIsSetupComplete(CurrentDomainId());
        }

        public ActionResult SMSetup()
        {
            var domainId = CurrentDomainId();
            var checkQbicle = false;
            ViewBag.DomainRoles = new DomainRolesRules(dbContext).GetDomainRolesDomainId(domainId);
            var smSetting = new SalesMaketingSettingRules(dbContext).GetSMSettingByDomain(domainId);
            var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId());
            if (setting != null)
            {
                var topics = new TopicRules(dbContext).GetTopicByQbicle(setting.SourceQbicle != null ? setting.SourceQbicle.Id : 0);
                if (topics != null && topics.Count > 0)
                {
                    foreach (var item in topics)
                    {
                        checkQbicle = setting.DefaultTopic != null && setting.DefaultTopic.Id == item.Id ? true : false;
                        if (checkQbicle)
                            break;
                    }
                }
            }

            var smSetupInit = new SMSetupInit
            {
                ContactsClass = new SocialWorkgroupRules(dbContext).GetCustomOptions(CurrentDomainId(), CurrentDomain()).Any() ? "complete" : "incomplete",
                ContactsCompleted = new SocialWorkgroupRules(dbContext).GetCustomOptions(CurrentDomainId(), CurrentDomain()).Any(),

                TraderContactsClass = new SocialWorkgroupRules(dbContext).GetSMContacts(domainId).Any() ? "complete" : "incomplete",
                TraderContactsCompleted = new SocialWorkgroupRules(dbContext).GetSMContacts(domainId).Any(),

                QbileClass = checkQbicle ? "complete" : "incomplete",
                QbileCompleted = checkQbicle,

                WorkgroupClass = new SocialWorkgroupRules(dbContext).GetMarketingWorkGroupsByDomainId(domainId).Any() ? "complete" : "incomplete",
                WorkgroupCompleted = new SocialWorkgroupRules(dbContext).GetMarketingWorkGroupsByDomainId(domainId).Any(),
            };
            if (smSetting != null)
            {
                switch (smSetting.IsSetupCompleted)
                {
                    case SMSetupCurrent.Contacts:
                        smSetupInit.ContactsClass = "incomplete active";
                        break;

                    case SMSetupCurrent.TraderContacts:
                        smSetupInit.TraderContactsClass = "incomplete active";
                        break;

                    case SMSetupCurrent.Qbicle:
                        smSetupInit.QbileClass = "incomplete active";
                        break;

                    case SMSetupCurrent.Workgroup:
                        smSetupInit.WorkgroupClass = "incomplete active";
                        break;

                    case SMSetupCurrent.Complete:
                        smSetupInit.SetupCompleteClass = "incomplete active";
                        break;

                    case SMSetupCurrent.SMApp:
                        smSetupInit.SetupCompleteClass = "complete active";
                        break;

                    default:
                        smSetupInit.ContactsClass = "incomplete active";
                        break;
                }
            }
            else
            {
                smSetting = new Settings();
                smSetupInit.ContactsClass = "incomplete active";
            }
            ViewBag.SMSetupInit = smSetupInit;
            return View(smSetting);
        }

        // GET: SalesMarketing
        public ActionResult SMApps()
        {
            int totalRecord = 0;
            ViewBag.ListAccountSocialNetwork = new SocialNetworkAccountRules(dbContext).ListAccountSocialNetwork(CurrentDomainId());
            ViewBag.NetworkType = new SocialNetworkRules(dbContext).GetNetworkTypes();
            ViewBag.ListAreas = new SocialLocationRule(dbContext).GetListArea("", CurrentDomainId(), 0, 100000);
            ViewBag.Segments = new SocialSegmentRule(dbContext).LoadSegmentsByDomainId(CurrentDomainId(), 0, 100000, null, null, true, ref totalRecord);
            var pl = TempData["PageList"];
            if (pl != null)
            {
                ViewBag.PageList = TempData["PageList"];
                ViewBag.GroupList = TempData["GroupList"];
                ViewBag.AccessToken = TempData["AccessToken"];
                ViewBag.NetworkType = TempData["NetworkType"];
                ViewBag.ListAccountSocialNetwork = TempData["ListAccountSocialNetwork"];
                ViewBag.TabConfig = true;
                ViewBag.FacebookPage = true;
                ViewBag.FacebookType = TempData["FacebookType"];
                ViewBag.ListAreas = TempData["ListAreas"];
            }

            #region Stored UiSettings

            var uiSettings = new QbicleRules(dbContext).LoadUiSettings(QbiclePages.pageSalesMarketing, CurrentUser().Id);
            ViewBag.UiSetting = uiSettings;

            #endregion Stored UiSettings

            ViewBag.CurrentPage = "SalesMarketing"; SetCurrentPage("SalesMarketing");
            return View();
        }

        public ActionResult SMSocial(int id)
        {
            var domainId = CurrentDomainId();
            var socialNetworkRule = new SocialNetworkRules(dbContext);
            var campaignRule = new CampaignRules(dbContext);
            ViewBag.Setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domainId);
            ViewBag.LstFilesAccept = Utility.SocialFilesAccept();
            ViewBag.NetworkType = socialNetworkRule.GetNetworkTypes();
            var socialCampaign = campaignRule.GetSocialCampaignById(id);
            if (socialCampaign != null)
            {
                var networkAccounts = socialNetworkRule.GetSocialNetworkAccountsByNWType(socialCampaign.TargetNetworks.Select(s => s.Id).ToArray(), domainId);
                ViewBag.NetworkAccounts = networkAccounts;
                ViewBag.CountQueue = campaignRule.CountQueue(id);
                ViewBag.CountApproved = campaignRule.CountApproved(id);
                ViewBag.CountApproval = campaignRule.CountApproval(id);
                ViewBag.CountSent = campaignRule.CountSent(id);
                ViewBag.Brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId);
                ViewBag.IdeaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2);
                return View(socialCampaign);
            }
            else
                return View("Error");
        }

        public ActionResult SMManualSocial(int id)
        {
            var domainId = CurrentDomainId();
            var socialNetworkRule = new SocialNetworkRules(dbContext);
            var campaignRule = new CampaignRules(dbContext);
            ViewBag.Setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domainId);
            ViewBag.LstFilesAccept = Utility.SocialFilesAccept();
            ViewBag.NetworkType = socialNetworkRule.GetNetworkTypes();
            var socialCampaign = campaignRule.GetSocialCampaignById(id);
            if (socialCampaign != null)
            {
                var networkAccounts = socialNetworkRule.GetSocialNetworkAccountsByNWType(socialCampaign.TargetNetworks.Select(s => s.Id).ToArray(), domainId);
                ViewBag.NetworkAccounts = networkAccounts;
                ViewBag.CountQueue = campaignRule.CountQueue(id);
                ViewBag.CountApproved = campaignRule.CountApproved(id);
                ViewBag.CountApproval = campaignRule.CountApproval(id);
                ViewBag.CountSent = campaignRule.CountSent(id);
                ViewBag.Brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId);
                ViewBag.IdeaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2);
                return View(socialCampaign);
            }
            else
                return View("Error");
        }

        public ActionResult SMEmail(int id)
        {
            var totalRecord = 0;
            var domainId = CurrentDomainId();
            var campaignRule = new CampaignRules(dbContext);
            ViewBag.Setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domainId);
            ViewBag.LstFilesAccept = Utility.SocialFilesAccept();
            ViewBag.Segments = new SocialSegmentRule(dbContext).LoadSegmentsByDomainId(CurrentDomainId(), 0, 100000, null, null, true, ref totalRecord);
            ViewBag.EmailTemplates = new EmailTemplateRules(dbContext).GetEmailTemplates(domainId);
            ViewBag.Qbicle = new QbicleRules(dbContext).GetQbicleById(CurrentQbicleId());
            ViewBag.VerifiedEmails = new SocialContactRule(dbContext).GetListVerifiedSESEmailByDomain(domainId);
            var emailCampaign = campaignRule.GetEmailCampaignById(id);
            if (emailCampaign != null)
            {
                //ViewBag.CountQueue = campaignRule.CountEmailCampaignQueue(id);
                //ViewBag.CountApproved = campaignRule.CountEmailCampaignApproved(id);
                //ViewBag.CountApproval = campaignRule.CountEmailCampaignApproval(id);
                //ViewBag.CountSent = campaignRule.CountEmailCampaignSent(id);
                ViewBag.Brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId);
                ViewBag.IdeaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2);
                return View(emailCampaign);
            }
            else
                return View("Error");
        }

        public ActionResult RenderPreviewEmail(EmailPreviewModel model)
        {
            var previewFolder = ConfigurationManager.AppSettings["PreviewEmail"].Replace("\\", "\\\\") + "\\" + CurrentUser().UserName;
            if (!Directory.Exists(previewFolder))
                Directory.CreateDirectory(Server.MapPath(previewFolder));
            String previewUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + ConfigurationManager.AppSettings["PreviewEmail"].Replace("\\", "/") + "/" + CurrentUser().UserName + "/";

            DirectoryInfo di = new DirectoryInfo(Server.MapPath(previewFolder));
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            if (model.PromotionalImgFile != null)
            {
                string path = Path.Combine(Server.MapPath(previewFolder), model.PromotionalImgFile.FileName);
                model.PromotionalImgFile.SaveAs(path);
                model.PromotionalImgPath = previewUrl + model.PromotionalImgFile.FileName;
            }
            else if (model.PromotionalImg != 0)
            {
                QbicleMedia media = new MediasRules(dbContext).GetMediaById(model.PromotionalImg);
                model.PromotionalImgPath = ViewBag.DocRetrievalUrl + media.VersionedFiles.FirstOrDefault()?.Uri;
            }

            if (model.AdImgFile != null)
            {
                string path = Path.Combine(Server.MapPath(previewFolder), model.AdImgFile.FileName);
                model.AdImgFile.SaveAs(path);
                model.AdImgPath = previewUrl + model.AdImgFile.FileName;
            }
            else if (model.AdImg != 0)
            {
                QbicleMedia media = new MediasRules(dbContext).GetMediaById(model.AdImg);
                model.AdImgPath = ViewBag.DocRetrievalUrl + media.VersionedFiles.FirstOrDefault()?.Uri;
            }

            model.PromotionalImgFile = null;
            model.AdImgFile = null;

            //SetPreviewEmailCookie(JsonConvert.SerializeObject(model));

            var refModel = new ReturnJsonModel
            {
                result = true,
                Object = model.ToJson()
            };

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PreviewEmail(int id, string preview = "")
        {
            if (id == 0 && preview != "")
            {
                var emailPreview = preview.ParseAs<EmailPreviewModel>();
                ViewBag.EmailTemplate = null;
                return View(emailPreview);
            }
            else if (id > 0)
            {
                var post = new CampaignRules(dbContext).GetCampaignEmailById(id);
                var model = new EmailPreviewModel
                {
                    Headline = post.Headline,
                    BodyContent = post.BodyContent,
                    ButtonLink = post.ButtonLink,
                    ButtonText = post.ButtonText,
                    PromotionalImgPath = ViewBag.DocRetrievalUrl + post.PromotionalImage.VersionedFiles.FirstOrDefault()?.Uri,
                    AdImgPath = post.AdvertisementImage != null ? ViewBag.DocRetrievalUrl + post.AdvertisementImage.VersionedFiles.FirstOrDefault()?.Uri : ""
                };
                ViewBag.EmailTemplate = post.Template;
                return View(model);
            }
            return View("Error");
        }

        public ActionResult LoadCountQueue(int CampaignById)
        {
            var campaignRule = new CampaignRules(dbContext);
            var countQueue = campaignRule.CountQueue(CampaignById);
            var countApproved = campaignRule.CountApproved(CampaignById);
            var countApproval = campaignRule.CountApproval(CampaignById);
            var countSent = campaignRule.CountSent(CampaignById);
            return Json(new { CountQueue = countQueue, CountApproved = countApproved, CountApproval = countApproval, CountSent = countSent }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadCountEmailQueue(int CampaignById)
        {
            var campaignRule = new CampaignRules(dbContext);
            var countQueue = campaignRule.CountEmailCampaignQueue(CampaignById);
            var countApproved = campaignRule.CountEmailCampaignApproved(CampaignById);
            var countApproval = campaignRule.CountEmailCampaignApproval(CampaignById);
            var countSent = campaignRule.CountEmailCampaignSent(CampaignById);
            return Json(new { CountQueue = countQueue, CountApproved = countApproved, CountApproval = countApproval, CountSent = countSent }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SocialPostInApp(int id)
        {
            var campaignApproval = new CampaignRules(dbContext).CampaignPostApprovalById(id);
            if (campaignApproval != null)
            {
                var currentDomainId = campaignApproval.WorkGroup?.Domain.Id ?? 0;
                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(currentDomainId);
                ValidateCurrentDomain(campaignApproval.WorkGroup?.Domain, setting?.SourceQbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.SalesAndMarketingAccess))
                    return View("ErrorAccessPage");
                ViewBag.Setting = setting;
                ViewBag.CampaignAproval = campaignApproval;
                var postModel = dbContext.SocialCampaignPosts.Find(campaignApproval.CampaignPost.Id);
                if (postModel == null) return View((SocialCampaignPost)null);
                var campaign = postModel.AssociatedCampaign;
                var networkAccounts = new SocialNetworkRules(dbContext).GetSocialNetworkAccountsByNWType(campaign.TargetNetworks.Select(s => s.Id).ToArray(), currentDomainId);
                ViewBag.NetworkAccounts = networkAccounts;
                var fbrandId = campaign.Brand != null && campaign.Brand.ResourceFolder != null ? campaign.Brand.ResourceFolder.Id : 0;
                var fideaId = campaign.IdeaTheme != null && campaign.IdeaTheme.ResourceFolder != null ? campaign.IdeaTheme.ResourceFolder.Id : 0;
                ViewBag.ListMedia = new CampaignRules(dbContext).GetMediasCampaign(campaign.ResourceFolder.Id, setting.SourceQbicle.Id, fbrandId, fideaId, CurrentUser().Timezone);
                ViewBag.CurrentPage = "SocialPostApproval"; SetCurrentPage("SocialPostApproval");
                SetCurrentApprovalIdCookies(campaignApproval.Activity?.Id ?? 0);

                return View(postModel);
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult ManualSocialPostInApp(int id)
        {
            var campaignApproval = new CampaignRules(dbContext).CampaignPostApprovalById(id);
            if (campaignApproval != null)
            {
                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId());
                ViewBag.Setting = setting;
                ViewBag.CampaignAproval = campaignApproval;
                var postModel = dbContext.SocialCampaignPosts.Find(campaignApproval.CampaignPost.Id);
                if (postModel == null) return View((SocialCampaignPost)null);
                var campaign = postModel.AssociatedCampaign;
                var networkAccounts = new SocialNetworkRules(dbContext).GetSocialNetworkAccountsByNWType(campaign.TargetNetworks.Select(s => s.Id).ToArray(), CurrentDomainId());
                ViewBag.NetworkAccounts = networkAccounts;
                var fbrandId = campaign.Brand != null && campaign.Brand.ResourceFolder != null ? campaign.Brand.ResourceFolder.Id : 0;
                var fideaId = campaign.IdeaTheme != null && campaign.IdeaTheme.ResourceFolder != null ? campaign.IdeaTheme.ResourceFolder.Id : 0;
                ViewBag.ListMedia = new CampaignRules(dbContext).GetMediasCampaign(campaign.ResourceFolder.Id, setting.SourceQbicle.Id, fbrandId, fideaId, CurrentUser().Timezone);
                ViewBag.CurrentPage = "SocialPostApproval"; SetCurrentPage("SocialPostApproval");
                SetCurrentApprovalIdCookies(campaignApproval.Activity?.Id ?? 0);

                return View(postModel);
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult EmailPostInApp(int id)
        {
            var emailApproval = new CampaignRules(dbContext).GetApprovalEmailById(id);
            if (emailApproval != null)
            {
                var currentDomainId = emailApproval?.WorkGroup.Domain.Id ?? 0;
                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(currentDomainId);
                ValidateCurrentDomain(emailApproval?.WorkGroup.Domain, setting?.SourceQbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.SalesAndMarketingAccess))
                    return View("ErrorAccessPage");
                ViewBag.Setting = setting;
                ViewBag.EmailAproval = emailApproval;
                var postModel = dbContext.CampaignEmails.Find(emailApproval.CampaignEmail.Id);
                if (postModel == null) return View((CampaignEmail)null);
                var campaign = postModel.Campaign;

                var fbrandId = campaign.Brand != null && campaign.Brand.ResourceFolder != null ? campaign.Brand.ResourceFolder.Id : 0;
                var fideaId = campaign.IdeaTheme != null && campaign.IdeaTheme.ResourceFolder != null ? campaign.IdeaTheme.ResourceFolder.Id : 0;
                ViewBag.ListMedia = new CampaignRules(dbContext).GetMediasCampaign(campaign.ResourceFolder.Id, setting.SourceQbicle.Id, fbrandId, fideaId, CurrentUser().Timezone);
                dynamic loadresourceparms = new System.Dynamic.ExpandoObject();
                loadresourceparms.fbrandId = fbrandId;
                loadresourceparms.fideaId = fideaId;
                loadresourceparms.resourceFolderId = campaign.ResourceFolder.Id;
                loadresourceparms.qbicleId = setting.SourceQbicle.Id;
                ViewBag.LoadMediaParamaters = loadresourceparms;
                ViewBag.EmailTemplates = new EmailTemplateRules(dbContext).GetEmailTemplates(currentDomainId);
                ViewBag.CurrentPage = "SocialPostApproval"; SetCurrentPage("SocialPostApproval");

                SetCurrentApprovalIdCookies(emailApproval.Activity?.Id ?? 0);
                ViewBag.VerifiedEmails = new SocialContactRule(dbContext).GetListVerifiedSESEmailByDomain(currentDomainId);

                return View(postModel);
            }
            else
            {
                return View("Error");
            }
        }

        public ActionResult SaveCampaignResource(string name, string description, int qbicleId, int topicId, int mediaFolderId,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            refModel = new ReturnJsonModel { result = false };
            if (!string.IsNullOrEmpty(mediaObjectKey))
            {
                var media = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
                };
                refModel = new CampaignRules(dbContext).SaveSocialCampaingnResource(media, CurrentUser().Id, qbicleId, mediaFolderId, name, description, topicId);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            else
            {
                refModel.msg = _L("ERROR_MSG_154");
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SocialPost()
        {
            return View();
        }

        public ActionResult LoadSocialCampains(string search, int[] targnetworks, CampaignType campaigntype, int skip, int take)
        {
            var lstCampaigns = new CampaignRules(dbContext).GetSocialCampaignByKeywordAndTargetNetwork(CurrentDomainId(), search, targnetworks, campaigntype, skip, take);
            return PartialView("_CampaignsSocialContent", lstCampaigns);
        }

        public ActionResult CountSocialCampains(string search, int[] targnetworks, CampaignType campaigntype)
        {
            try
            {
                var refModel = new ReturnJsonModel { result = true };
                var numberOfCamp = new CampaignRules(dbContext).CountSocialCampaignByKeywordAndTargetNetwork(CurrentDomainId(), search, targnetworks, campaigntype);
                return Json(numberOfCamp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadEmailCampaigns(string search, int[] targetsegments, int skip, int take)
        {
            var lstCampaigns = new CampaignRules(dbContext).GetEmailCampaignByKeywordAndTargetNetwork(CurrentDomainId(), search, targetsegments, skip, take);
            return PartialView("_EmailCampaignsContent", lstCampaigns);
        }

        public ActionResult CountEmailCampaigns(string search, int[] targetsegments)
        {
            try
            {
                var refModel = new ReturnJsonModel { result = true };
                var numberOfCamp = new CampaignRules(dbContext).CountEmailCampaignByKeywordAndTargetNetwork(CurrentDomainId(), search, targetsegments);
                return Json(numberOfCamp, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetDependencyCheckManual()
        {
            try
            {
                var domainId = CurrentDomainId();
                var netwokType = new SocialNetworkAccountRules(dbContext).ListAccountSocialNetwork(domainId).Count();
                var brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId).Count();
                var ideaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2).Count();
                var segments = new SocialSegmentRule(dbContext).CountSegment(domainId);
                return Json(new { status = true, netwokType = netwokType, brands = brands, ideaThemes = ideaThemes, segments = segments }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return Json(new { status = false, ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult DependencyCheckPartial(int type)
        {
            var domainId = CurrentDomainId();
            ViewBag.Type = type;
            ViewBag.NetwokType = new SocialNetworkAccountRules(dbContext).ListAccountSocialNetwork(domainId).Count();
            ViewBag.Brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId).Count();
            ViewBag.IdeaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2).Count();
            ViewBag.Segments = new SocialSegmentRule(dbContext).CountSegment(domainId);
            return PartialView("_SocialAutoDependencies");
        }

        public PartialViewResult SocialCampaignAdd()
        {
            var domainId = CurrentDomainId();
            ViewBag.NetworkType = new SocialNetworkRules(dbContext).GetNetworkTypes();
            ViewBag.Setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domainId);
            ViewBag.Brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId);
            ViewBag.IdeaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2);
            return PartialView("_SocialCampaignAdd");
        }

        public PartialViewResult SocialCampaignEdit(int id)
        {
            var domainId = CurrentDomainId();
            ViewBag.NetworkType = new SocialNetworkRules(dbContext).GetNetworkTypes();
            ViewBag.Setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domainId);
            ViewBag.Brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId);
            ViewBag.IdeaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2);
            return PartialView("_SocialCampaignEdit", new CampaignRules(dbContext).GetSocialCampaignById(id));
        }

        public PartialViewResult ManualSocialCampaignAdd()
        {
            var domainId = CurrentDomainId();
            ViewBag.NetworkType = new SocialNetworkRules(dbContext).GetNetworkTypes();
            ViewBag.Setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domainId);
            ViewBag.Brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId);
            ViewBag.IdeaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2);
            return PartialView("_ManualSocialCampaignAdd");
        }

        public PartialViewResult ManualSocialCampaignEdit(int id)
        {
            var domainId = CurrentDomainId();
            ViewBag.NetworkType = new SocialNetworkRules(dbContext).GetNetworkTypes();
            ViewBag.Setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domainId);
            ViewBag.Brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId);
            ViewBag.IdeaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2);
            return PartialView("_ManualSocialCampaignEdit", new CampaignRules(dbContext).GetSocialCampaignById(id));
        }

        public PartialViewResult EmailCampaignAdd()
        {
            int totalRecord = 0;
            var domainId = CurrentDomainId();
            ViewBag.Setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domainId);
            ViewBag.Brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId);
            ViewBag.IdeaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2);
            ViewBag.Segments = new SocialSegmentRule(dbContext).LoadSegmentsByDomainId(domainId, 0, 100000, null, null, true, ref totalRecord);
            ViewBag.VerifiedEmails = new SocialContactRule(dbContext).GetListVerifiedSESEmailByDomain(domainId);
            return PartialView("_EmailCampaignAdd");
        }

        public PartialViewResult EmailCampaignEdit(int id)
        {
            int totalRecord = 0;
            var domainId = CurrentDomainId();
            ViewBag.Setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(domainId);
            ViewBag.Brands = new SocialBrandRules(dbContext).LoadBrandsByDomainId(domainId);
            ViewBag.IdeaThemes = new SocialIdeasThemeRule(dbContext).GetIdeaThemesByDomainId(domainId, "", 2);
            ViewBag.Segments = new SocialSegmentRule(dbContext).LoadSegmentsByDomainId(CurrentDomainId(), 0, 100000, null, null, true, ref totalRecord);
            ViewBag.VerifiedEmails = new SocialContactRule(dbContext).GetListVerifiedSESEmailByDomain(domainId);
            return PartialView("_EmailCampaignEdit", new CampaignRules(dbContext).GetEmailCampaignById(id));
        }

        public PartialViewResult SocialBrandOptions(int brandId)
        {
            var brandRule = new SocialBrandRules(dbContext);
            ViewBag.BrandProducts = brandRule.LoadBrandProductsAll(brandId);
            ViewBag.ValuePropositions = brandRule.LoadBrandValuePropositonByAll(brandId);
            ViewBag.Attributes = brandRule.LoadBrandAttrGroupsByBrandId(brandId, "");
            return PartialView("_BrandOptionsContent");
        }

        public PartialViewResult EmailBrandOptions(int brandId)
        {
            var brandRule = new SocialBrandRules(dbContext);
            ViewBag.ValuePropositions = brandRule.LoadBrandValuePropositonByAll(brandId);
            ViewBag.Attributes = brandRule.LoadBrandAttrGroupsByBrandId(brandId, "");
            return PartialView("_EmailBrandOptionsContent");
        }

        public ActionResult LoadFoldersByQbicle(int qbicleId)
        {
            try
            {
                var listMediaFolderBy =
                    new MediaFolderRules(dbContext).GetMediaFoldersByQbicleId(qbicleId, "");
                return Json(listMediaFolderBy.Select(s => new { id = s.Id, text = s.Name }).ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AutoGenerateFolderName(int qbicleId)
        {
            try
            {
                string foldername = new CampaignRules(dbContext).AutoGenerateFolderName(qbicleId);
                return Json(foldername, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveSocialCampaign(SocialCampaign socialCampaign, int workgroup, int resourcesfolder, string newfoldername, int qbicleFolderId, int topicId,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize,
            int[] networktype, int? brandId, int[] attributes, int[] brandproducts, int[] valueprops, int ideaId)
        {
            refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (networktype == null)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_391");
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                var media = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
                };

                socialCampaign.Domain = CurrentDomain();

                refModel = new CampaignRules(dbContext).SaveSocialCampaign(
                    socialCampaign, workgroup, resourcesfolder, newfoldername, qbicleFolderId,
                    media, topicId, networktype, brandId, attributes, brandproducts, valueprops, ideaId, CurrentUser().Id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                refModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveEmailCampaign(EmailCampaign emailCampaign, int workgroup, int resourcesfolder, string newfoldername, int qbicleFolderId, int topicId, int[] lstSegments,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize, int? brandId, int[] attributes, int[] brandproducts, int[] valueprops, int ideaId)
        {
            refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (lstSegments == null)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_233");
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }

                var media = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
                };

                emailCampaign.Domain = CurrentDomain();

                refModel = new CampaignRules(dbContext).SaveEmailCampaign(emailCampaign, workgroup, resourcesfolder,
                    newfoldername, qbicleFolderId, media, topicId, lstSegments, brandId, attributes, brandproducts,
                    valueprops, ideaId, CurrentUser().Id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                refModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadMediasByFolderId(int fid, int qid, int fbrandid, int fideaid)
        {
            try
            {
                return PartialView("_CampaignResources", new CampaignRules(dbContext).GetMediasCampaign(fid, qid, fbrandid, fideaid, CurrentUser().Timezone));
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult LoadPostContent(string type, int campaignId)
        {
            try
            {
                var campaignRule = new CampaignRules(dbContext);
                ViewBag.Type = type;
                if (type == "queue")
                {
                    ViewBag.LstQueuePosts = campaignRule.QueuePostsByCampaignId(campaignId, false);
                }
                else if (type == "approved")
                {
                    ViewBag.LstApprovedPosts = campaignRule.ApprovedPostsByCampaignId(campaignId, true);
                }
                else if (type == "approvals")
                {
                    ViewBag.LstApprovedPosts = campaignRule.ApprovedPostsByCampaignId(campaignId, false);
                }
                else
                {
                    ViewBag.LstQueuePosts = campaignRule.QueuePostsByCampaignId(campaignId, true);
                }
                return PartialView("_CampaignPostContent");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadManualPostContent(string type, int campaignId)
        {
            try
            {
                var campaignRule = new CampaignRules(dbContext);
                ViewBag.Type = type;
                if (type == "queue")
                {
                    ViewBag.LstQueuePosts = campaignRule.QueuePostsByCampaignId(campaignId, false);
                }
                else if (type == "approved")
                {
                    ViewBag.LstApprovedPosts = campaignRule.ApprovedPostsByCampaignId(campaignId, true);
                }
                else if (type == "approvals")
                {
                    ViewBag.LstApprovedPosts = campaignRule.ApprovedPostsByCampaignId(campaignId, false);
                }
                else
                {
                    ViewBag.LstQueuePosts = campaignRule.QueuePostsByCampaignId(campaignId, true);
                }
                return PartialView("_ManualCampaignPostContent");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadEmailPostContent(string type, int campaignId)
        {
            try
            {
                var campaignRule = new CampaignRules(dbContext);
                ViewBag.Type = type;
                if (type == "queue")
                {
                    ViewBag.LstQueuePosts = campaignRule.QueueEmailPostsByCampaignId(campaignId, false);
                }
                else if (type == "approved")
                {
                    ViewBag.LstApprovedPosts = campaignRule.ApprovedEmailPostsByCampaignId(campaignId, true);
                }
                else if (type == "approvals")
                {
                    ViewBag.LstApprovedPosts = campaignRule.ApprovedEmailPostsByCampaignId(campaignId, false);
                }
                else
                {
                    ViewBag.LstQueuePosts = campaignRule.QueueEmailPostsByCampaignId(campaignId, true);
                }
                return PartialView("_EmailCampaignPostContent");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadDownloadModal(int id, string type)
        {
            var campaignRule = new CampaignRules(dbContext);
            var post = new SocialCampaignPost();
            if (type.Equals("approved"))
            {
                var approvedPost = campaignRule.CampaignPostApprovalById(id);
                ViewBag.Id = approvedPost.Id;
                post = approvedPost.CampaignPost;
            }
            else
            {
                var queuePost = campaignRule.GetSocialCampaignQueue(id);
                ViewBag.Id = queuePost.Id;
                post = queuePost.Post;
            }
            ViewBag.Type = type;
            return PartialView("_ManualSocialDownload", post);
        }

        public async Task<ActionResult> GetZipFile(int id, string type)
        {
            var refModel = await (new CampaignRules(dbContext).GetZipFileAsync(id, type));
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConfirmDownload(int postId)
        {
            var campaignRule = new CampaignRules(dbContext);
            campaignRule.UpdatePostQueueImmediately(postId, 0);
            refModel = new ReturnJsonModel() { result = true };
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadCampaignResourcesContent(int fid, int qid, int fbrandid, int fideaid)
        {
            try
            {
                return PartialView("_CampaignResourcesPost", new CampaignRules(dbContext).GetMediasCampaign(fid, qid, fbrandid, fideaid, CurrentUser().Timezone));
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadImageResources(string refdivid)
        {
            try
            {
                ViewBag.refdivid = refdivid;
                return PartialView("_ImageResources", new SalesMaketingSettingRules(dbContext).GetMediasSMByDomain(CurrentDomainId(), CurrentUser().Timezone));
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadPromotionResourcesContent(int fid, int qid, int fbrandid, int fideaid, string featuredimage)
        {
            try
            {
                ViewBag.featuredimage = featuredimage;
                return PartialView("_PromotionResourcesPost", new CampaignRules(dbContext).GetMediasCampaign(fid, qid, fbrandid, fideaid, CurrentUser().Timezone));
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadAdResourcesContent(int fid, int qid, int fbrandid, int fideaid, string advimage)
        {
            try
            {
                ViewBag.advimage = advimage;
                return PartialView("_AdResourcesPost", new CampaignRules(dbContext).GetMediasCampaign(fid, qid, fbrandid, fideaid, CurrentUser().Timezone));
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadPostEdit(int campaignPId)
        {
            try
            {
                var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId());
                ViewBag.Setting = setting;
                var postModel = dbContext.SocialCampaignPosts.Find(campaignPId);
                if (postModel != null)
                {
                    var campaign = postModel.AssociatedCampaign;
                    var networkAccounts = new SocialNetworkRules(dbContext).GetSocialNetworkAccountsByNWType(campaign.TargetNetworks.Select(s => s.Id).ToArray(), CurrentDomainId());
                    ViewBag.NetworkAccounts = networkAccounts;
                    var listMedia = new MediaFolderRules(dbContext).GetMediaItemByFolderId(campaign.ResourceFolder.Id,
                        setting.SourceQbicle.Id, CurrentUser().Timezone);
                    ViewBag.ListMedia = listMedia;
                }

                return PartialView("_SocialPostEdit", postModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public async Task<ActionResult> SaveSocialPost(SocialCampaignPost campaignPost, int ImageOrVideo, int[] SharingAccount, int[] networktype,

             string mediaSocialPostFeatureObjectKey, string mediaSocialPostFeatureObjectName, string mediaSocialPostFeatureObjectSize,
            int SocialCampaignId, bool? isReminder, string reminderDate)
        {
            refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (string.IsNullOrEmpty(campaignPost.Title) || string.IsNullOrEmpty(campaignPost.Content))
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_181");
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                if (SharingAccount == null)
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_182");
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                if (isReminder != null && isReminder.Value)
                {
                    if (campaignPost.Reminder.ReminderDate == null)
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_183");
                        return Json(refModel, JsonRequestBehavior.AllowGet);
                    }
                    if (campaignPost.Reminder.Content == null)
                    {
                        refModel.msg = ResourcesManager._L("ERROR_MSG_184");
                        return Json(refModel, JsonRequestBehavior.AllowGet);
                    }
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
                    try
                    {
                        campaignPost.Reminder.ReminderDate = TimeZoneInfo.ConvertTimeToUtc(reminderDate.ConvertDateFormat(CurrentUser().DateTimeFormat), tz);
                    }
                    catch
                    {
                        campaignPost.Reminder.ReminderDate = DateTime.UtcNow;
                    }
                }

                var media = new MediaModel();
                if (ImageOrVideo > 0)
                {
                    campaignPost.ImageOrVideo = new MediasRules(dbContext).GetMediaById(ImageOrVideo);
                }
                else
                {
                    media = new MediaModel
                    {
                        UrlGuid = mediaSocialPostFeatureObjectKey,
                        Name = mediaSocialPostFeatureObjectName,
                        Size = HelperClass.FileSize(int.Parse(mediaSocialPostFeatureObjectSize == "" ? "0" : mediaSocialPostFeatureObjectSize)),
                        Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaSocialPostFeatureObjectName))
                    };
                }

                refModel.result = await new CampaignRules(dbContext).SaveSocialPost(
                    campaignPost, media, CurrentUser().Id, CurrentDomainId(), SharingAccount, networktype, SocialCampaignId, isReminder);

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                refModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SaveEmailPost(CampaignEmail campaignPost, int promotionalImg, int adImg,
             string mediaEmailPostFeatureObjectKey, string mediaEmailPostFeatureObjectName, string mediaEmailPostFeatureObjectSize,
             string mediaEmailPostPromotionalObjectKey, string mediaEmailPostPromotionalObjectName, string mediaEmailPostPromotionalObjectSize,
             string mediaEmailPostAdObjectKey, string mediaEmailPostAdObjectName, string mediaEmailPostAdObjectSize,
            int[] campaignsegments, int emailCampaignId, int templateId = 0)
        {
            refModel = new ReturnJsonModel() { result = false };
            try
            {
                var mediaFeatured = new MediaModel
                {
                    UrlGuid = mediaEmailPostFeatureObjectKey,
                    Name = mediaEmailPostFeatureObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaEmailPostFeatureObjectSize == "" ? "0" : mediaEmailPostFeatureObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaEmailPostFeatureObjectName)),
                    IsPublic = false
                };
                var mediaPromotional = new MediaModel();
                var mediaAd = new MediaModel();
                var mediaRule = new MediasRules(dbContext);

                if (promotionalImg > 0)
                {
                    var qbMedia = mediaRule.GetMediaById(promotionalImg);
                    mediaRule.SetMediaIsPublish(qbMedia);
                    campaignPost.PromotionalImage = qbMedia;
                }
                else
                {
                    mediaPromotional = new MediaModel
                    {
                        UrlGuid = mediaEmailPostPromotionalObjectKey,
                        Name = mediaEmailPostPromotionalObjectName,
                        Size = HelperClass.FileSize(int.Parse(mediaEmailPostPromotionalObjectSize == "" ? "0" : mediaEmailPostPromotionalObjectSize)),
                        Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaEmailPostPromotionalObjectName)),
                        IsPublic = true
                    };
                }

                if (adImg > 0)
                {
                    var qbMedia = mediaRule.GetMediaById(adImg);
                    mediaRule.SetMediaIsPublish(qbMedia);
                    campaignPost.AdvertisementImage = qbMedia;
                }
                else
                {
                    mediaAd = new MediaModel
                    {
                        UrlGuid = mediaEmailPostAdObjectKey,
                        Name = mediaEmailPostAdObjectName,
                        Size = HelperClass.FileSize(int.Parse(mediaEmailPostAdObjectSize == "" ? "0" : mediaEmailPostAdObjectSize)),
                        Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaEmailPostAdObjectName)),
                        IsPublic = true
                    };
                }

                refModel.result = new CampaignRules(dbContext).SaveEmailPost(campaignPost, mediaFeatured, mediaPromotional, mediaAd, campaignsegments, CurrentUser().Id, emailCampaignId, templateId);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                refModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeletePostApproval(int id)
        {
            var result = new CampaignRules(dbContext).DeletePostApproval(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteEmailPostApproval(int id)
        {
            var result = new CampaignRules(dbContext).DeleteEmailPostApproval(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DeletePostQueue(int id)
        {
            var result = await new CampaignRules(dbContext).DeletePostQueue(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DeleteEmailPostQueue(int id)
        {
            var result = await new CampaignRules(dbContext).DeleteEmailPostQueue(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeniedPostApproval(int id)
        {
            var result = new CampaignRules(dbContext).PostSetDenied(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DenyEmailPost(int id)
        {
            var result = new CampaignRules(dbContext).DenyEmailPost(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PostSetApproved(int id)
        {
            var result = new CampaignRules(dbContext).PostSetApproved(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApproveEmailPost(int id)
        {
            var result = new CampaignRules(dbContext).ApproveEmailPost(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AddQueueSchedule(int aid, bool isNotifyWhenSent, string sPostingDate)
        {
            var result = await new CampaignRules(dbContext).AddQueueSchedule(aid, sPostingDate, isNotifyWhenSent, CurrentUser().Timezone, CurrentUser().DateTimeFormat);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AddEmailQueueSchedule(int aid, bool isNotifyWhenSent, string sPostingDate)
        {
            var result = await new CampaignRules(dbContext).AddEmailQueueSchedule(aid, sPostingDate, isNotifyWhenSent, CurrentUser().Timezone, CurrentUser().DateTimeFormat);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SentBackToReview(int aid)
        {
            return Json(new CampaignRules(dbContext).SentBackToReview(aid), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SentBackEmailPostToReview(int aid)
        {
            return Json(new CampaignRules(dbContext).SentBackEmailPostToReview(aid), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SentBackFromQueueToReview(int queueId)
        {
            return Json(await new CampaignRules(dbContext).SentBackFromQueueToReview(queueId), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SentBackFromEmailQueueToReview(int queueId)
        {
            return Json(await new CampaignRules(dbContext).SentBackFromEmailQueueToReview(queueId), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AddPostImmediately(int aid)
        {
            var result = await new CampaignRules(dbContext).PostImmediately(aid);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AddEmailPostImmediately(int aid)
        {
            var result = await new CampaignRules(dbContext).PostEmailImmediately(aid);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangePostInApprovedToSent(int aid)
        {
            var result = new CampaignRules(dbContext).ChangePostInApprovedToSent(aid);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AddPostQueueImmediately(int queueId)
        {
            var result = await new CampaignRules(dbContext).PostQueueImmediately(queueId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> AddEmailPostQueueImmediately(int queueId)
        {
            var result = await new CampaignRules(dbContext).PostEmailQueueImmediately(queueId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangePostInQueueToSent(int queueId)
        {
            var result = new CampaignRules(dbContext).ChangePostInQueueToSent(queueId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> RemoveQueueSchedule(int campaignId)
        {
            var result = await new CampaignRules(dbContext).RemoveQueueSchedule(campaignId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> RemoveEmailQueueSchedule(int campaignId)
        {
            var result = await new CampaignRules(dbContext).RemoveEmailQueueSchedule(campaignId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadSocicalConfigs()
        {
            var domain = CurrentDomain();
            ViewBag.qbicles = domain.Qbicles;
            var setting = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId());
            if (setting != null)
            {
                ViewBag.Setting = setting;
                ViewBag.topics = new TopicRules(dbContext).GetTopicByQbicle(setting.SourceQbicle != null ? setting.SourceQbicle.Id : 0);
                ViewBag.ListAccountSocialNetwork = new SocialNetworkAccountRules(dbContext).ListAccountSocialNetwork(CurrentDomainId());
            }

            return PartialView("_ConfigsContent");
        }

        public ActionResult LoadTopicsByQbicleId(int qid)
        {
            try
            {
                var topics = new TopicRules(dbContext).GetTopicByQbicle(qid);
                if (topics != null && topics.Count > 0)
                    return Json(topics.Select(s => new { id = s.Id, text = s.Name }).ToList(), JsonRequestBehavior.AllowGet);
                else
                    return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadModalWorkgroup(int id)
        {
            var socialWorkgroupRule = new SocialWorkgroupRules(dbContext);
            ViewBag.Setting = socialWorkgroupRule.getSettingByDomainId(CurrentDomainId());
            ViewBag.Process = socialWorkgroupRule.MarketingProcess();
            return PartialView("_WorkgroupAddEdit", socialWorkgroupRule.getWorkgroupById(id));
        }

        public ActionResult LoadTableWorkgroup()
        {
            var rule = new SocialWorkgroupRules(dbContext);
            var domainId = CurrentDomainId();
            ViewBag.Setting = rule.getSettingByDomainId(domainId);
            ViewBag.Process = rule.MarketingProcess();
            ViewBag.ListAccountSocialNetwork = new SocialNetworkAccountRules(dbContext).ListAccountSocialNetwork(CurrentDomainId());
            var workGroups = rule.GetMarketingWorkGroupsByDomainId(domainId);
            return PartialView("_WorkgroupsTable", workGroups);
        }

        [HttpPost]
        public ActionResult SaveWorkgroup(workgroupCustomeModel model)
        {
            refModel = new ReturnJsonModel { result = false };
            try
            {
                model.DomainId = CurrentDomainId();
                refModel.result = new SocialWorkgroupRules(dbContext).SaveWorkgroup(model, CurrentUser().Id);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.msg = ex.Message;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteWorkgroupById(int id)
        {
            return Json(new SocialWorkgroupRules(dbContext).deleteWorkgroupById(id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteSocialNetworkById(int id)
        {
            return Json(new SocialNetworkAccountRules(dbContext).deleteSocialNetworkById(id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetDisableSocialNetworkById(int id)
        {
            return Json(new SocialNetworkAccountRules(dbContext).SetDisableSocialNetwork(id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateSetting(int id, int qId, int tId)
        {
            return Json(new SocialWorkgroupRules(dbContext).UpdateSetting(id, CurrentDomain(), tId, qId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TwitterAuth(AuthTwitter model)
        {
            try
            {
                var context = new SocialNetworkAccountRules(dbContext);
                var redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/SalesMarketing/ValidateTwitterAuth";
                var authenticationContext = context.GetTwitterUriAuthentication(redirectUri);
                new CampaignRules(dbContext).SMUiSetting(QbiclePages.pageSalesMarketing, CurrentUser().Id, "Configs");
                return Json(new { AuthorizationURL = authenticationContext }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ValidateTwitterAuth()
        {
            try
            {
                var verifierCode = Request.Params.Get("oauth_verifier");
                var authorizationId = Request.Params.Get("authorization_id");
                var settings = new SocialWorkgroupRules(dbContext).getSettingByDomainId(CurrentDomainId());
                new SocialNetworkAccountRules(dbContext).ValidateTwitterAuth(verifierCode, authorizationId, CurrentUser().Id, settings, CurrentDomainId());
                ViewBag.NetworkType = new SocialNetworkRules(dbContext).GetNetworkTypes();
                ViewBag.TabConfig = true;
                new CampaignRules(dbContext).SMUiSetting(QbiclePages.pageSalesMarketing, CurrentUser().Id, "Configs");
                ViewBag.ListAreas = new SocialLocationRule(dbContext).GetListArea("", CurrentDomainId(), 0, 100000);
                return View("SMApps");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                ViewBag.NetworkType = dbContext.NetworkTypes.ToList();
                ViewBag.TabConfig = true;
                new CampaignRules(dbContext).SMUiSetting(QbiclePages.pageSalesMarketing, CurrentUser().Id, "Configs");
                ViewBag.ListAreas = new SocialLocationRule(dbContext).GetListArea("", CurrentDomainId(), 0, 100000);
                return View("SMApps");
            }
        }

        [HttpPost]
        public ActionResult FacebookAuth(AuthFacebook model)
        {
            try
            {
                Session["FacebookType"] = model.FacebookType;
                var context = new SocialNetworkAccountRules(dbContext);
                var redirectUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/SalesMarketing/ValidateFacebookAuth";
                var authenticationContext = context.GetFacebookUriAuthentication(redirectUrl);
                return Json(new { AuthorizationURL = authenticationContext }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ValidateFacebookAuth(string code)
        {
            var facebookType = Session["FacebookType"] == null ? 0 : (int)Session["FacebookType"];
            try
            {
                var redirectUri = Request.Url.Scheme + "://" + Request.Url.Authority + "/SalesMarketing/ValidateFacebookAuth";
                var context = new SocialNetworkAccountRules(dbContext);
                var fbResult = context.ValidateFacebookAuth(code, redirectUri);
                //ViewBag.PageList = fbResult.pages;
                TempData["PageList"] = fbResult.pages;
                TempData["GroupList"] = fbResult.groups;
                TempData["AccessToken"] = fbResult.access_token;
                TempData["NetworkType"] = dbContext.NetworkTypes.ToList();
                TempData["ListAccountSocialNetwork"] = context.ListAccountSocialNetwork(CurrentDomainId());
                TempData["TabConfig"] = true;
                new CampaignRules(dbContext).SMUiSetting(QbiclePages.pageSalesMarketing, CurrentUser().Id, "Configs");
                TempData["FacebookPage"] = true;
                TempData["FacebookType"] = facebookType;
                TempData["ListAreas"] = new SocialLocationRule(dbContext).GetListArea("", CurrentDomainId(), 0, 100000);
                return RedirectToAction("SMApps", "SalesMarketing");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                TempData["NetworkType"] = dbContext.NetworkTypes.ToList();
                TempData["TabConfig"] = true;
                new CampaignRules(dbContext).SMUiSetting(QbiclePages.pageSalesMarketing, CurrentUser().Id, "Configs");
                //  ViewBag.FacebookPage = true;
                TempData["FacebookType"] = facebookType;
                TempData["ListAreas"] = new SocialLocationRule(dbContext).GetListArea("", CurrentDomainId(), 0, 100000);
                return RedirectToAction("SMApps", "SalesMarketing");
            }
        }

        [HttpPost]
        public JsonResult AddProfileFacebookPage(FacebookPageGroupModel model)
        {
            var dataResult = new SocialNetworkAccountRules(dbContext).AddFacebookAccount(model, CurrentDomainId(), CurrentUser().Id, FaceBookAccount.FacebookTypeEnum.Page);
            return dataResult.result ? Json(new { status = true }, JsonRequestBehavior.AllowGet) : Json(new { status = false, message = dataResult.msg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddProfileFacebookGroup(FacebookPageGroupModel model)
        {
            var dataResult = new SocialNetworkAccountRules(dbContext).AddFacebookAccount(model, CurrentDomainId(), CurrentUser().Id, FaceBookAccount.FacebookTypeEnum.Group);
            return dataResult.result ? Json(new { status = true }, JsonRequestBehavior.AllowGet) : Json(new { status = false, message = dataResult.msg }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSocialNetworkById(int id, string provider)
        {
            try
            {
                //var context = new SocialNetworkAccountRules(dbContext);
                //if (provider.ToUpper() == "Facebook".ToUpper())
                //{
                //    var result = context.GetFacebookAccount(id);
                //    return Json(new { result.ClientId, result.ClientSecret }, JsonRequestBehavior.AllowGet);
                //} else if(provider.ToUpper() == "Twitter".ToUpper())
                //{
                //    var result = context.GetTwitterAccount(id);
                //    return Json(new { result.ConsumerKey, result.ConsumerSecret, result.UserAccessToken, result.UserAccessSecret }, JsonRequestBehavior.AllowGet);
                //} else
                //{
                //    var result = context.GetInstagramAccount(id);
                //    return Json(new { result.ClientId, result.ClientSecret }, JsonRequestBehavior.AllowGet);
                //}
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CampaignPostPreview(bool isVideo, string link, string thumb)
        {
            try
            {
                if (isVideo && !link.EndsWith("mediaVideo", StringComparison.CurrentCultureIgnoreCase))
                    link += "&type=mediaVideo";
                ViewBag.link = link;
                ViewBag.isVideo = isVideo;
                ViewBag.thumb = thumb;
                return PartialView("_CampaignPostPreview");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadSyncTrader()
        {
            try
            {
                var workgroupRules = new SocialWorkgroupRules(dbContext);
                var domainId = CurrentDomainId();
                ViewBag.DocRetrievalUrl = ConfigManager.ApiGetDocumentUri;
                var syncTrader = workgroupRules.SyncTrader(domainId, CurrentUser().Id);
                return PartialView("_SyncTraderTable", syncTrader);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadSyncTraderSetup()
        {
            try
            {
                var workgroupRules = new SocialWorkgroupRules(dbContext);
                var domainId = CurrentDomainId();
                ViewBag.DocRetrievalUrl = ConfigManager.ApiGetDocumentUri;
                var syncTrader = workgroupRules.SyncTrader(domainId, CurrentUser().Id);
                return PartialView("_SyncTraderTableSetup", syncTrader);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadContact()
        {
            try
            {
                var workgroupRules = new SocialWorkgroupRules(dbContext);
                var domainId = CurrentDomainId();
                var contact = workgroupRules.GetCustomOptions(domainId, CurrentDomain());
                return PartialView("_ContactTable", contact);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SaveAgeRange(int id, int idCus, int start, int end)
        {
            return Json(new SocialWorkgroupRules(dbContext).SaveAgeRanges(id, idCus, start, end, CurrentUser().Id, CurrentDomain()), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteAgeRange(int id)
        {
            return Json(new SocialWorkgroupRules(dbContext).DeleteAgeRanges(id, CurrentDomain().Id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveSMPlaceArea(Area area, int[] AssociatePlaces, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            ReturnJsonModel refModel = new ReturnJsonModel();
            try
            {
                area.FeaturedImageUri = mediaObjectKey;
                area.Domain = CurrentDomain();
                refModel = new SocialLocationRule(dbContext).SaveSMPlaceArea(area, CurrentUser().Id, AssociatePlaces);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SavePlace(Place place, int[] AssociateAreas, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            ReturnJsonModel refModel = new ReturnJsonModel();
            try
            {
                place.FeaturedImageUri = mediaObjectKey;
                place.Domain = CurrentDomain();
                refModel = new SocialLocationRule(dbContext).SavePlace(place, CurrentUser().Id, AssociateAreas);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadModalArea(int id)
        {
            var rule = new SocialLocationRule(dbContext);
            ViewBag.ListPlaces = rule.GetListPlace("", CurrentDomainId(), 0, 0, 100000);
            return PartialView("_AreaAddEdit", rule.GetAreaById(id));
        }

        [HttpPost]
        public ActionResult ShowOrHideArea(int id)
        {
            var rule = new SocialLocationRule(dbContext);
            var refModel = rule.ShowOrHideArea(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadModalPlace(int id)
        {
            var rule = new SocialLocationRule(dbContext);
            ViewBag.ListAreas = rule.GetListArea("", CurrentDomainId(), 0, 100000, true);
            return PartialView("_PlaceAddEdit", rule.GetPlaceById(id));
        }

        [HttpPost]
        public ActionResult ShowOrHidePlace(int id)
        {
            var rule = new SocialLocationRule(dbContext);
            var refModel = rule.ShowOrHidePlace(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadArea(string name, int skip, int take, bool isLoadingHide)
        {
            var rule = new SocialLocationRule(dbContext);
            var domainId = CurrentDomainId();
            return PartialView("_AreaContent", rule.GetListArea(name, domainId, skip, take, isLoadingHide));
        }

        public int CountListArea(string name, bool isLoadingHide)
        {
            var rule = new SocialLocationRule(dbContext);
            var domainId = CurrentDomainId();
            return rule.CountListArea(name, domainId, isLoadingHide);
        }

        public PartialViewResult LoadSocialPostAdd(int campId)
        {
            var socialNetworkRule = new SocialNetworkRules(dbContext);
            var campaignRule = new CampaignRules(dbContext);
            var socialCampaign = campaignRule.GetSocialCampaignById(campId);
            if (socialCampaign != null)
            {
                var networkAccounts = socialNetworkRule.GetSocialNetworkAccountsByNWType(socialCampaign.TargetNetworks.Select(s => s.Id).ToArray(), CurrentDomainId());
                ViewBag.NetworkAccounts = networkAccounts;
            }
            return PartialView("_SocialPostAdd", socialCampaign);
        }

        public PartialViewResult LoadManualPostAdd(int campId)
        {
            var campaignRule = new CampaignRules(dbContext);
            var socialCampaign = campaignRule.GetSocialCampaignById(campId);
            return PartialView("_ManualSocialPostAdd", socialCampaign);
        }

        public PartialViewResult LoadPlace(string name, int areaId, int skip, int take, bool isLoadingHide)
        {
            var rule = new SocialLocationRule(dbContext);
            var domainId = CurrentDomainId();
            return PartialView("_PlaceContent", rule.GetListPlace(name, domainId, areaId, skip, take, isLoadingHide));
        }

        public int CountListPlace(string name, int areaId, bool isLoadingHide)
        {
            var rule = new SocialLocationRule(dbContext);
            var domainId = CurrentDomainId();
            return rule.CountListPlace(name, domainId, areaId, isLoadingHide);
        }

        public int CountListPipeline(string name, bool isLoadingHide)
        {
            var rule = new CampaignRules(dbContext);
            var domainId = CurrentDomainId();
            return rule.CountListPipeline(name, isLoadingHide, domainId);
        }

        public PartialViewResult LoadPipeline(string name, int skip, int take, bool isLoadingHide)
        {
            var rule = new CampaignRules(dbContext);
            var domainId = CurrentDomainId();
            return PartialView("_PipelineContent", rule.GetListPipeline(name, isLoadingHide, domainId, skip, take));
        }

        [HttpPost]
        public ActionResult ShowOrHidePipeline(int id)
        {
            var rule = new CampaignRules(dbContext);
            var refModel = rule.ShowOrHidePipeline(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadSteps(int pipelineId)
        {
            var rule = new CampaignRules(dbContext);
            var domainId = CurrentDomainId();
            return PartialView("_StepContent", rule.GetPipelineById(pipelineId));
        }

        public PartialViewResult LoadExistPipelineContacts(string name, int id)
        {
            var rule = new CampaignRules(dbContext);
            ViewBag.PipelineContacts = rule.LoadExistPipelineContacts(id, name);
            return PartialView("_LoadExistPipelineContacts");
        }

        public PartialViewResult LoadNewPipelineContacts(string name, int pipelineId)
        {
            var rule = new CampaignRules(dbContext);
            var domainId = CurrentDomainId();
            ViewBag.Contacts = rule.LoadNewPipelineContacts(domainId, name, pipelineId);
            return PartialView("_LoadNewPipelineContacts");
        }

        public PartialViewResult ShowExistPipelineContact(int id)
        {
            var rule = new CampaignRules(dbContext);
            return PartialView("_ShowExistPipelineContact", rule.GetExistPipelineContactById(id));
        }

        public PartialViewResult ShowNewPipelineContact(int id)
        {
            var rule = new SocialContactRule(dbContext);
            return PartialView("_ShowNewPipelineContact", rule.GetSMContractById(id));
        }

        public PartialViewResult LoadModalPipeline(int id)
        {
            ViewBag.Pipeline = new CampaignRules(dbContext).GetPipelineById(id);
            return PartialView("_PipelineAddEdit");
        }

        public PartialViewResult LoadModalPipelineContact(int id)
        {
            ViewBag.Contacts = new SocialContactRule(dbContext).GetListContact("", CurrentDomainId());
            ViewBag.PipelineContacts = new CampaignRules(dbContext).GetListPipelineContact(id);
            return PartialView("_PipelineContact", (id == 0 ? new Pipeline() : new CampaignRules(dbContext).GetPipelineById(id)));
        }

        public PartialViewResult LoadModalPipelineContactDetail(int id)
        {
            var rule = new CampaignRules(dbContext);
            return PartialView("_PipelineContactDetail", rule.GetExistPipelineContactById(id));
        }

        public ActionResult SavePipeline(Pipeline pipeline, string steps, string idSteps,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            ReturnJsonModel refModel = new ReturnJsonModel();
            try
            {
                pipeline.FeaturedImageUri = mediaObjectKey;

                pipeline.Domain = CurrentDomain();
                refModel = new CampaignRules(dbContext).SavePipeline(pipeline,
                    JsonConvert.DeserializeObject<string[]>(steps), JsonConvert.DeserializeObject<int[]>(idSteps),
                    CurrentUser().Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SMPipeline(int id)
        {
            return View(new CampaignRules(dbContext).GetPipelineById(id));
        }

        public ActionResult SavePipelineContact(PipelineContact pipelineContact, int pipelineId, int contactId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel();
            try
            {
                var domain = CurrentDomain();
                refModel = new CampaignRules(dbContext).SavePipelineContact(pipelineContact, pipelineId, contactId, CurrentUser().Id);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangePipelineContactToStep(string pipelineContactId, int stepId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel();
            try
            {
                var domain = CurrentDomain();
                refModel = new CampaignRules(dbContext).ChangePipelineContactToStep(JsonConvert.DeserializeObject<int[]>(pipelineContactId), stepId);
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemovePipelineContact(string pipelineContactId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel();
            try
            {
                var domain = CurrentDomain();
                refModel = new CampaignRules(dbContext).RemovePipelineContact(JsonConvert.DeserializeObject<int[]>(pipelineContactId));
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerateModalTask(string taskKey, int pipelineContactId, int pipelineId)
        {
            try
            {
                var taskId = string.IsNullOrEmpty(taskKey) ? 0 : int.Parse(taskKey.Decrypt());

                var pipeline = new CampaignRules(dbContext).GetPipelineById(pipelineId);
                var mktSettings = dbContext.SalesMarketingSettings.FirstOrDefault(p => p.Domain.Id == pipeline.Domain.Id);
                ViewBag.MarketingSettings = mktSettings;

                var recurrance = new RecurranceRules(dbContext).GetRecurranceById(0);
                ViewBag.lstMonth = Utility.GetListMonth(DateTime.UtcNow);
                ViewBag.Recurrance = recurrance;
                ViewBag.taskId = taskId;
                ViewBag.taskKey = taskKey;
                ViewBag.pipelineContactId = pipelineContactId;
                return PartialView("_ModalTask");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveSMQbicleTask(QbicleTask task, string Assignee, string ProgrammedStart, string[] Watchers,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize,
            int qbicleId, int TopicId, int[] ActivitiesRelate, List<QbicleStep> Steplst, int? Type, string LastOccurrence, string DayOrMonth, int? pattern, int? customDate, string dayofweek, List<string> listDate, short? monthdates, long pipelineContactId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                string currentDatetimeFormat = CurrentUser().DateTimeFormat;
                DateTime dtLastOccurrence = DateTime.UtcNow;
                var lstDate = new List<CustomDateModel>();
                if (string.IsNullOrEmpty(task.Name) || string.IsNullOrEmpty(task.Description))
                {
                    refModel.result = false;
                    refModel.msg = "Request to enter information!";
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                if (ActivitiesRelate != null && ActivitiesRelate.Count() > 31)
                {
                    refModel.result = false;
                    refModel.msg = "Data associate activities cannot be greater than 31 records!";
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }

                var media = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaObjectName))
                };

                var tz = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
                if (!string.IsNullOrEmpty(ProgrammedStart))
                {
                    try
                    {
                        task.ProgrammedStart = TimeZoneInfo.ConvertTimeToUtc(ProgrammedStart.ConvertDateFormat(currentDatetimeFormat), tz);
                    }
                    catch
                    {
                        task.ProgrammedStart = DateTime.UtcNow;
                    }
                }
                try
                {
                    if (!string.IsNullOrEmpty(LastOccurrence))
                        dtLastOccurrence = TimeZoneInfo.ConvertTimeToUtc(LastOccurrence.ConvertDateFormat(CurrentUser().DateFormat), tz);
                    if (listDate != null && listDate.Any())
                    {
                        var arrDate = listDate[0].Split(',');
                        if (arrDate != null)
                        {
                            CustomDateModel cDate;
                            foreach (var item in arrDate)
                            {
                                cDate = new CustomDateModel
                                {
                                    StartDate = TimeZoneInfo.ConvertTimeToUtc(item.ConvertDateFormat(currentDatetimeFormat), tz)
                                };
                                lstDate.Add(cDate);
                            }
                        }
                    }
                }
                catch
                {
                    dtLastOccurrence = DateTime.UtcNow;
                }
                var _recurrance = new QbicleRecurrance
                {
                    Days = Type == 0 || Type == 1 ? DayOrMonth : "",
                    Months = Type == 2 ? DayOrMonth : "",
                    FirstOccurrence = task.ProgrammedStart ?? DateTime.UtcNow,
                    LastOccurrence = dtLastOccurrence,
                    MonthDate = monthdates.HasValue ? monthdates.Value : (short)0
                };
                if (Type != null)
                    _recurrance.Type = (QbicleRecurrance.RecurranceTypeEnum)Type;
                if (Type == 2)
                    _recurrance.Pattern = (short)pattern;

                refModel.result = new CampaignRules(dbContext).
                    SaveSMQbicleTask(task, Assignee, media, Watchers, CurrentQbicleId(),
                    CurrentUser().Id, TopicId, ActivitiesRelate, Steplst, _recurrance, lstDate, pipelineContactId);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
            if (refModel.result)
                return Json(refModel, JsonRequestBehavior.AllowGet);
            else
                return null;
        }

        public ActionResult GenerateModalEvent(int eventId, int pipelineContactId, int pipelineId)
        {
            try
            {
                // get data to modal add new event
                var pipeline = new CampaignRules(dbContext).GetPipelineById(pipelineId);
                var mktSettings = dbContext.SalesMarketingSettings.FirstOrDefault(p => p.Domain.Id == pipeline.Domain.Id);
                ViewBag.MarketingSettings = mktSettings;

                var currentCube = mktSettings?.SourceQbicle ?? new Qbicle();

                var recurrance = new RecurranceRules(dbContext).GetRecurranceById(0);

                ViewBag.lstMonth = Utility.GetListMonth(DateTime.UtcNow);
                ViewBag.UserCurrentQbicleAssing = currentCube?.Members?.ToList() ?? new List<ApplicationUser>();

                ViewBag.eventId = eventId;
                ViewBag.EventRecurrance = recurrance;
                ViewBag.PipelineContactId = pipelineContactId;
                return PartialView("_ModalEvent");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }

        [HttpPost]
        public ActionResult SaveEvent(QbicleEvent qEvent, string[] sendInvitesTo, int[] ActivitiesRelate,
            string mediaObjectKey, string mediaObjectName, string mediaObjectSize,
            string eventStart, int qbicleId, HttpPostedFileBase eventAttachment, int TopicId, int? Type, string LastOccurrence, string DayOrMonth, int? pattern, int? customDate, string dayofweek, List<string> listDate, short? monthdates, int pipelineContactId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var currentDatetimeFormat = CurrentUser().DateTimeFormat;
                var currentDateFormat = CurrentUser().DateFormat;
                DateTime dtLastOccurrence = DateTime.UtcNow;
                var lstDate = new List<CustomDateModel>();
                if (string.IsNullOrEmpty(qEvent.Name))
                {
                    refModel.result = false;
                    refModel.msg = "Request to enter information!";
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                if (ActivitiesRelate != null && ActivitiesRelate.Count() > 31)
                {
                    refModel.result = false;
                    refModel.msg = "Data associate activities cannot be greater than 31 records!";
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
                //valid event dates
                var tz = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
                try
                {
                    qEvent.Start = TimeZoneInfo.ConvertTimeToUtc(eventStart.ConvertDateFormat(currentDatetimeFormat), tz);
                }
                catch
                {
                    qEvent.Start = DateTime.UtcNow;
                }
                try
                {
                    if (!string.IsNullOrEmpty(LastOccurrence))
                        dtLastOccurrence = TimeZoneInfo.ConvertTimeToUtc(LastOccurrence.ConvertDateFormat(currentDateFormat), tz);
                    if (listDate != null && listDate.Any())
                    {
                        var arrDate = listDate[0].Split(',');
                        if (arrDate != null)
                        {
                            CustomDateModel cDate;
                            foreach (var item in arrDate)
                            {
                                cDate = new CustomDateModel
                                {
                                    StartDate = TimeZoneInfo.ConvertTimeToUtc(item.ConvertDateFormat(currentDatetimeFormat), tz)
                                };
                                lstDate.Add(cDate);
                            }
                        }
                    }
                }
                catch
                {
                    dtLastOccurrence = DateTime.UtcNow;
                }
                var media = new MediaModel
                {
                    UrlGuid = mediaObjectKey,
                    Name = mediaObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaObjectSize == "" ? "0" : mediaObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(System.IO.Path.GetExtension(mediaObjectName))
                };

                var _recurrance = new QbicleRecurrance
                {
                    Days = Type == 0 || Type == 1 ? DayOrMonth : "",
                    Months = Type == 2 ? DayOrMonth : "",
                    FirstOccurrence = qEvent.Start,
                    LastOccurrence = dtLastOccurrence,
                    MonthDate = monthdates.HasValue ? monthdates.Value : (short)0
                };
                if (Type != null)
                    _recurrance.Type = (QbicleRecurrance.RecurranceTypeEnum)Type;
                if (Type == 2)
                    _recurrance.Pattern = (short)pattern;

                refModel = new CampaignRules(dbContext).SaveEvent(qEvent,
                    eventStart, CurrentQbicleId(), sendInvitesTo, ActivitiesRelate, media, CurrentUser().Id, TopicId, _recurrance, lstDate, pipelineContactId);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.result = false;
                refModel.msg = ex.Message;
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadPipelineTasks([Bind(Prefix = "search[value]")] string search, int draw, int pipelineContactId, int start, int length)
        {
            var totalRecord = 0;
            List<PipelineTasksModel> lstResult = new CampaignRules(dbContext).GetPipelineTasks(pipelineContactId, start, length, ref totalRecord, CurrentUser().DateFormat);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadPipelineEvents([Bind(Prefix = "search[value]")] string search, int draw, int pipelineContactId, int start, int length)
        {
            var totalRecord = 0;
            List<PipelineEventsModel> lstResult = new CampaignRules(dbContext).GetPipelineEvents(pipelineContactId, start, length, ref totalRecord, CurrentUser().DateFormat);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadSMContact([Bind(Prefix = "order[0][column]")] int column, [Bind(Prefix = "order[0][dir]")] string orderby, [Bind(Prefix = "order[2][dir]")] string emailOrder, [Bind(Prefix = "order[3][dir]")] string phoneOrder, [Bind(Prefix = "order[4][dir]")] string sourceOrder, string search, int draw, int start, int length)
        {
            var totalRecord = 0;
            var lstResult = new SocialContactRule(dbContext).GetListSMContact(column, orderby, search, CurrentDomainId(), start, length, ref totalRecord);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadCampaignsOfContact(int contactId, string search, int draw, int start, int length)
        {
            var totalRecord = 0;
            var lstResult = new CampaignRules(dbContext).GetCampaignsOfContact(contactId, start, length, ref totalRecord);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult LoadModalAddSMContact()
        {
            //ViewBag.AgeRanges = ;
            ViewBag.Areas = new SocialLocationRule(dbContext).GetListArea("", CurrentDomainId(), 0, 10000);
            ViewBag.Criterias = new SocialContactRule(dbContext).GetCriteriaDefinitions(CurrentDomainId());
            ViewBag.AgeRange = new SocialWorkgroupRules(dbContext).GetCustomOptions(CurrentDomainId(), CurrentDomain());
            DateTime now = DateTime.UtcNow;
            ViewBag.MinDate = now.AddYears(-100).ToString("dd/MM/yyyy");
            ViewBag.MaxDate = now.AddYears(-18).ToString("dd/MM/yyyy");
            return PartialView("_SMContactAdd");
        }

        public ActionResult ShowEditSMContract(long contactId)
        {
            ViewBag.Areas = new SocialLocationRule(dbContext).GetListArea("", CurrentDomainId(), 0, 10000);
            ViewBag.Criterias = new SocialContactRule(dbContext).GetCriteriaDefinitions(CurrentDomainId());
            ViewBag.AgeRange = new SocialWorkgroupRules(dbContext).GetCustomOptions(CurrentDomainId(), CurrentDomain());
            DateTime now = DateTime.UtcNow;
            ViewBag.MaxDate = now.AddYears(-18).ToString("dd/MM/yyyy");
            return View("_SMContactEdit", new SocialContactRule(dbContext).GetSMContractById(contactId));
        }

        public ActionResult SaveSMContact(SMContactModel model, string mediaObjectKey, string mediaObjectName, string mediaObjectSize)
        {
            ReturnJsonModel refModel = new ReturnJsonModel();
            try
            {
                model.AvatarUri = mediaObjectKey;
                var tz = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
                if (!string.IsNullOrEmpty(model.DateOfBirth))
                {
                    model.BirthDay = TimeZoneInfo.ConvertTimeToUtc(model.DateOfBirth.ConvertDateFormat("dd-MM-yyyy"), tz);
                }
                refModel = new SocialContactRule(dbContext).SaveSMContact(model, CurrentUser().Id, CurrentDomain());
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.Object = ex;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteSMContact(int id)
        {
            return Json(new SocialContactRule(dbContext).DeleteSMContact(id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveTabActive(string tab)
        {
            new CampaignRules(dbContext).SMUiSetting(QbiclePages.pageSalesMarketing, CurrentUser().Id, tab);
            return null;
        }

        public ActionResult CountContacts(int[] lstSegments)
        {
            ReturnJsonModel refModel = new ReturnJsonModel();
            SegmentContactModel model = new CampaignRules(dbContext).CountContacts(lstSegments);
            refModel.Object = model;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EmailBuilder(int id = 0)
        {
            var currentDomainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.SalesAndMarketingAccess))
                return View("ErrorAccessPage");
            var listMedia = new List<QbicleMedia>();
            if (id > 0)
                listMedia = new SalesMaketingSettingRules(dbContext).GetMediasSMByDomain(currentDomainId, CurrentUser().Timezone);
            ViewBag.listMedia = listMedia;
            var emailtemp = new EmailTemplateRules(dbContext).GetEmailTemplateById(id);
            return View(emailtemp != null ? emailtemp : new EmailTemplate());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveEmailBuilder(EmailTemplate email,
            //HttpPostedFileBase FileFeaturedImg, HttpPostedFileBase FileAdvImg,
            string mediaFeatureObjectKey, string mediaFeatureObjectName, string mediaFeatureObjectSize,
            string mediaAdvObjectKey, string mediaAdvObjectName, string mediaAdvObjectSize
            )
        {
            new CampaignRules(dbContext).SMUiSetting(QbiclePages.pageSalesMarketing, CurrentUser().Id, "Configs.EmailTemplates");
            email.Domain = CurrentDomain();

            #region Upload files to DOC API

            MediaModel mediaModelFeatured = null;
            MediaModel mediaModelAdv = null;

            if (!string.IsNullOrEmpty(mediaFeatureObjectKey))
            {
                mediaModelFeatured = new MediaModel
                {
                    UrlGuid = mediaFeatureObjectKey,
                    Name = mediaFeatureObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaFeatureObjectSize == "" ? "0" : mediaFeatureObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaFeatureObjectName))
                };
            }

            if (!string.IsNullOrEmpty(mediaAdvObjectKey))
            {
                mediaModelAdv = new MediaModel
                {
                    UrlGuid = mediaAdvObjectKey,
                    Name = mediaAdvObjectName,
                    Size = HelperClass.FileSize(int.Parse(mediaAdvObjectSize == "" ? "0" : mediaAdvObjectSize)),
                    Type = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(mediaAdvObjectName))
                };
            }

            #endregion Upload files to DOC API

            var refModel = new EmailTemplateRules(dbContext).SaveEmailTemplate(email, mediaModelFeatured, mediaModelAdv, CurrentUser().Id);
            if (refModel.result)
                refModel.Object = Url.Action("SMApps", "SalesMarketing");
            return Json(refModel);
        }

        public ActionResult GetEmailTemplates([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            return Json(new EmailTemplateRules(dbContext).GetEmailTemplates(requestModel, CurrentDomainId(), CurrentUser().Timezone, CurrentUser().DateFormat), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteEmailTemplateById(int id)
        {
            return Json(new EmailTemplateRules(dbContext).DeleteEmailTemplateById(id));
        }

        public ActionResult GetEmailTemplate(int id)
        {
            return Json(new EmailTemplateRules(dbContext).GetEmailTemplateJsonById(id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendVerifyEmailSES(string email)
        {
            var result = new SocialContactRule(dbContext).VerifyIdentity(email, CurrentDomainId(), CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateSESVerifyEmailTemplate()
        {
            var result = new SocialContactRule(dbContext).CreateSESEmailTemplate();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteSESVerifyEmailTemplate(string templateName)
        {
            var result = new SocialContactRule(dbContext).DeleteSESVerificationEmailTemplate(templateName);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadSESIdentities([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string key, SESIdentityStatus status, int start, int length, int draw)
        {
            try
            {
                var totalRecord = 0;
                var currentUser = CurrentUser();
                List<SESIdentityCustomModel> lstResult = new SocialContactRule(dbContext).GetListSESIdentities(CurrentDomainId(), key, status, currentUser.Timezone, currentUser.DateTimeFormat, ref totalRecord, requestModel, start, length);
                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = lstResult,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };
                return Json(dataTableData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<SESIdentity>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteSESIdentity(int identityId)
        {
            var result = new SocialContactRule(dbContext).DeleteSESIdentity(identityId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateSESIdentityStatus()
        {
            var domain = CurrentDomain();
            var userId = CurrentUser().Id;
            var result = new SocialContactRule(dbContext).UpdateIdentitiesStatus(domain, userId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult SESVerifySuccessPage()
        {
            return View("SESVerifySuccess");
        }

        [AllowAnonymous]
        public ActionResult SESVerifyFailPage()
        {
            return View("SESVerifyFail");
        }
    }
}