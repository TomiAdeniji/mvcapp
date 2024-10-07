using Qbicles.BusinessRules;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.BusinessRules.AdminListing;
using Qbicles.BusinessRules.BusinessRules.Social;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Highlight;
using Qbicles.Models.Qbicles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class HighlightPostController : BaseController
    {
        #region Showing Views
        public ActionResult HighlightAddView(int highlightPostId)
        {
            var postCustomModel = new HighlightPostRules(dbContext).GetListHighlightPostCustomById(highlightPostId, CurrentUser().Id, CurrentUser());

            return PartialView("_HighlightAddView", postCustomModel);
        }

        public ActionResult HighlightPostDetail(int hlPostId)
        {
            var hlpost = new HighlightPostRules(dbContext).GetListHighlightPostCustomById(hlPostId, CurrentUser().Id, CurrentUser());
            ViewBag.CurrentPage = "highlight";
            return View(hlpost);
        }

        public ActionResult ShowSharePostPartialView(int sharedPostId)
        {
            ViewBag.SharedPostId = sharedPostId;
            var c2cContactUsers = new HighlightPostRules(dbContext).GetContactsForHLShare(CurrentUser().Id);
            return PartialView("_HLPostSharingPartialView", c2cContactUsers);
        }

        public ActionResult BusinessHighlight()
        {
            var domainId = CurrentDomainId();
            var profile = new CommerceRules(dbContext).GetB2bProfileByDomainId(domainId);
            if (profile == null)
                return Redirect("~/Commerce/BusinessProfile");
            if (!CanAccessBusiness())
                return View("ErrorAccessPage");

            var highlightRules = new HighlightPostRules(dbContext);
            var eventRefs = highlightRules.GetListReferenceByType(domainId, ListingType.Event);
            var jobRefs = highlightRules.GetListReferenceByType(domainId, ListingType.Job);
            var realestateRefs = highlightRules.GetListReferenceByType(domainId, ListingType.RealEstate);

            ViewBag.EventRefs = eventRefs;
            ViewBag.JobRefs = jobRefs;
            ViewBag.RealEstate = realestateRefs;
            ViewBag.CurrentPage = "BusinessProfile";

            var extensionRules = new DomainExtensionRules(dbContext);
            var newsExtensionRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.News);
            var articleExtensionRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.Articles);
            var knowledgeExtensionRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.Knowledge);
            var eventExtensionRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.Events);
            var jobExtensionRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.Jobs);
            var realestateExtensionRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.RealEstate);

            ViewBag.IsNewRequestApproved = newsExtensionRequest != null && newsExtensionRequest.Status == ExtensionRequestStatus.Approved;
            ViewBag.IsArticleRequestApproved = articleExtensionRequest != null && articleExtensionRequest.Status == ExtensionRequestStatus.Approved;
            ViewBag.IsKnowledgeRequestApproved = knowledgeExtensionRequest != null && knowledgeExtensionRequest.Status == ExtensionRequestStatus.Approved;
            ViewBag.IsEventRequestApproved = eventExtensionRequest != null && eventExtensionRequest.Status == ExtensionRequestStatus.Approved;
            ViewBag.IsJobRequestApproved = jobExtensionRequest != null && jobExtensionRequest.Status == ExtensionRequestStatus.Approved;
            ViewBag.IsRealEstateRequestApproved = realestateExtensionRequest != null && realestateExtensionRequest.Status == ExtensionRequestStatus.Approved;
            return View(profile);
        }

        public ActionResult NewsHighlightAddView(int newsPostId)
        {
            var post = new HighlightPostRules(dbContext).GetHighlightPost(newsPostId, HighlightPostType.News, 0);
            var lstTags = new HighlightPostRules(dbContext).GetListTagsByTypes(HighlightPostType.News);
            ViewBag.ListTags = lstTags;
            return PartialView("_HLNewsAddView", post);
        }

        public ActionResult ArticleHighlightAddView(int articlePostId)
        {
            var post = new HighlightPostRules(dbContext).GetHighlightPost(articlePostId, HighlightPostType.Article, 0);
            var lstTags = new HighlightPostRules(dbContext).GetListTagsByTypes(HighlightPostType.Article);
            ViewBag.ListTags = lstTags;
            return PartialView("_HLArticleAddView", post);
        }

        public ActionResult KnowledgeHighlightAddView(int knowledgePostId)
        {
            var post = new HighlightPostRules(dbContext).GetHighlightPost(knowledgePostId, HighlightPostType.Knowledge, 0);
            var lstTags = new HighlightPostRules(dbContext).GetListTagsByTypes(HighlightPostType.Knowledge);
            var lstCountries = new CountriesRules().GetAllCountries();
            ViewBag.ListTags = lstTags;
            ViewBag.ListCountries = lstCountries;
            return PartialView("_HLKnowledgeAddView", post);
        }
        
        public ActionResult ListingHighlightAddView(int listingPostId, ListingType listingType)
        {
            var post = new HighlightPostRules(dbContext).GetHighlightPost(listingPostId, HighlightPostType.Listings, listingType);
            var lstCountries = new CountriesRules().GetAllCountries();
            var lstLocations = new List<Select2Option>();
            lstLocations.Add(new Select2Option { id = "0", text = "All areas" });
            if(post != null)
            {
                var listingpost = post as ListingHighlight;
                var countryname = listingpost.Country?.CommonName ?? "";
                var lstHLLocationByCountry = new AdminListingRules(dbContext).GetListLocationByCountry(countryname);
                lstLocations.AddRange(lstHLLocationByCountry.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }));
            }
            ViewBag.ListLocations = lstLocations;
            ViewBag.ListCountries = lstCountries;
            ViewBag.PropTypeList = dbContext.PropertyTypes.ToList();
            ViewBag.PropertiesList = dbContext.PropertyExtras.ToList();

            var userSetting = CurrentUser();
            ViewBag.TimeZone = userSetting.Timezone;
            ViewBag.DateTimeFormat = userSetting.DateTimeFormat;

            var domainId = CurrentDomainId();
            if(post != null && post.Domain != null)
            {
                domainId = post.Domain.Id;
            }
            var extensionRules = new DomainExtensionRules(dbContext);
            var eventExtensionRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.Events);
            var jobExtensionRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.Jobs);
            var realestateExtensionRequest = extensionRules.GetExtensionRequestByDomainAndType(domainId, ExtensionRequestType.RealEstate);

            ViewBag.IsEventRequestApproved = (eventExtensionRequest != null && eventExtensionRequest.Status == ExtensionRequestStatus.Approved) 
                                                || (post != null && (post as ListingHighlight).ListingHighlightType == ListingType.Event);
            ViewBag.IsJobRequestApproved = (jobExtensionRequest != null && jobExtensionRequest.Status == ExtensionRequestStatus.Approved) 
                                                || (post != null && (post as ListingHighlight).ListingHighlightType == ListingType.Job);
            ViewBag.IsRealEstateRequestApproved = (realestateExtensionRequest != null && realestateExtensionRequest.Status == ExtensionRequestStatus.Approved)
                                                || (post != null && (post as ListingHighlight).ListingHighlightType == ListingType.RealEstate);
            return PartialView("_HLListingAddView", post);
        }
        public ActionResult EventHighlightAddView(int listingPostId)
        {
            var post = new HighlightPostRules(dbContext).GetEventListingHighlight(listingPostId);
            var lstCountries = new CountriesRules().GetAllCountries();
            var lstLocations = new List<Select2Option>();
            lstLocations.Add(new Select2Option { id = "0", text = "All areas" });
            if (post != null)
            {
                var listingpost = post as ListingHighlight;
                var countryname = listingpost.Country?.CommonName ?? "";
                var lstHLLocationByCountry = new AdminListingRules(dbContext).GetListLocationByCountry(countryname);
                lstLocations.AddRange(lstHLLocationByCountry.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }));
            }
            ViewBag.ListLocations = lstLocations;
            ViewBag.ListCountries = lstCountries;
            ViewBag.ListTags = new HighlightPostRules(dbContext).GetListTagsByTypes(HighlightPostType.Listings,ListingType.Event);
            return PartialView("_HLEventAddView", post);
        }
        public ActionResult JobHighlightAddView(int listingPostId)
        {
            var post = new HighlightPostRules(dbContext).GetJobListingHighlight(listingPostId);
            var lstCountries = new CountriesRules().GetAllCountries();
            var lstLocations = new List<Select2Option>();
            lstLocations.Add(new Select2Option { id = "0", text = "All areas" });
            if (post != null)
            {
                var listingpost = post as ListingHighlight;
                var countryname = listingpost.Country?.CommonName ?? "";
                var lstHLLocationByCountry = new AdminListingRules(dbContext).GetListLocationByCountry(countryname);
                lstLocations.AddRange(lstHLLocationByCountry.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }));
            }
            ViewBag.ListLocations = lstLocations;
            ViewBag.ListCountries = lstCountries;
            ViewBag.ListTags = new HighlightPostRules(dbContext).GetListTagsByTypes(HighlightPostType.Listings, ListingType.Job);
            return PartialView("_HLJobAddView", post);
        }
        public ActionResult RealEstateHighlightAddView(int listingPostId)
        {
            var post = new HighlightPostRules(dbContext).GetRealEstateListingHighlight(listingPostId);
            var lstCountries = new CountriesRules().GetAllCountries();
            var lstLocations = new List<Select2Option>();
            lstLocations.Add(new Select2Option { id = "0", text = "All areas" });
            if (post != null)
            {
                var listingpost = post as ListingHighlight;
                var countryname = listingpost.Country?.CommonName ?? "";
                var lstHLLocationByCountry = new AdminListingRules(dbContext).GetListLocationByCountry(countryname);
                lstLocations.AddRange(lstHLLocationByCountry.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }));
            }
            ViewBag.ListLocations = lstLocations;
            ViewBag.ListCountries = lstCountries;
            ViewBag.PropTypeList = dbContext.PropertyTypes.ToList();
            ViewBag.PropertiesList = dbContext.PropertyExtras.ToList();
            ViewBag.ListTags = new HighlightPostRules(dbContext).GetListTagsByTypes(HighlightPostType.Listings, ListingType.RealEstate);
            return PartialView("_HLRealEstateAddView", post);
        }
        public ActionResult AllTagsModalShow(List<string> chosenTags)
        {
            ViewBag.chosenTags = chosenTags;
            var allTags = new HighlightPostRules(dbContext).GetHighlightPostTags();
            return PartialView("_HLEventAddView", allTags);
        }

        public ActionResult ArticleContentModal(int articleId)
        {
            ViewBag.TimeZone = CurrentUser().Timezone;
            ViewBag.DateTimeFormat = CurrentUser().DateTimeFormat;
            var articleItem = dbContext.ArticleHighlightPosts.Find(articleId);
            return PartialView("_HLArticleContent", articleItem);
        }

        public ActionResult RealEstateInfoModal(int rePostId)
        {
            ViewBag.UserId = CurrentUser().Id;
            var rePost = dbContext.RealEstateHighlightPosts.Find(rePostId);
            return PartialView("_HLRealEstateInfo", rePost);
        }
        #endregion

        #region Processing Business Rules
        public ActionResult AddEditHighlightPost(HighlightPost highlightPost, List<string> tags, S3ObjectUploadModel uploadModel = null)
        {
            var result = new ReturnJsonModel();
            result.result = false;
            result.msg = "You do not have permissions to create Highlight Post.";

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteHighlightPost(int highlightPostId)
        {
            var result = new HighlightPostRules(dbContext).DeleteHighlightPost(highlightPostId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LoadMoreHighlightPost(string keySearch, List<string> tagsSearch, int pageIndex, HighlightPostType typeShowed, ListingType lsTypeSearch,
            string domainKey, bool isBookmarked, bool isFlagged, string countryName, string eventDateRange, string newsPublishedDate, List<int> rePropTypeIds, 
            int reBedroomNumber, int reBathroomNumber, List<int> rePropertyIds, int areaId)
        {
            try
            {
                var domainId = 0;
                if (!string.IsNullOrEmpty(domainKey.Trim()))
                {
                    domainId = Int32.Parse(EncryptionService.Decrypt(domainKey));
                }

                var rules = new HighlightPostRules(dbContext);
                var dateTimeFormat = CurrentUser().DateFormat;
                var timeZone = CurrentUser().Timezone;
                
                var userSetting = CurrentUser();
                var highlightPostStreamModel = rules.GetListHlPostCustomPagination(userSetting, domainId, keySearch, typeShowed, lsTypeSearch, tagsSearch, isBookmarked, 
                    isFlagged, pageIndex, countryName, eventDateRange, newsPublishedDate, rePropTypeIds,
                    reBedroomNumber, reBathroomNumber, rePropertyIds, areaId);
                

                var modelString = RenderLoadNextViewToString("_HighlightPostList", highlightPostStreamModel.HighlightPosts);
                var result = Json(new { ModelString = modelString, TotalPage = highlightPostStreamModel.TotalPage }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return null;
            }
        }

        public string RenderLoadNextViewToString(string viewName, object model)
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

        public ActionResult LikePostProcess(int highlightPostId)
        {
            var result = new HighlightPostRules(dbContext).PostLikeProcess(highlightPostId, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //type: 1: Follow, 2: UnFollow
        public ActionResult UpdateHLDomainFollowStatus(string domainKey, int type)
        {
            var domainId = 0;
            if (!string.IsNullOrEmpty(domainKey.Trim()))
            {
                domainId = domainKey.Decrypt2Int();
            }

            var resultJson = new HighlightPostRules(dbContext).UpdateDomainFollowStatus(domainId, CurrentUser().Id, type);
            return Json(resultJson, JsonRequestBehavior.AllowGet);
        }

        //type: 1:Like, 2:Unlike
        public ActionResult UpdateHLLikeStatus(int hlPostId, int type)
        {
            var resultJson = new HighlightPostRules(dbContext).UpdateHLLikeStatus(hlPostId, CurrentUser().Id, type);
            return Json(resultJson, JsonRequestBehavior.AllowGet);
        }

        //type: 1:Flag, 2:Unflag
        //Interested - Flag Highlight
        public ActionResult UpdateHLFlagStatus(int hlPostId, int type)
        {
            var resultJson = new HighlightPostRules(dbContext).UpdateHLFlagStatus(hlPostId, CurrentUser().Id, type);
            return Json(resultJson, JsonRequestBehavior.AllowGet);
        }

        //type: 1:Bookmark, 2:Remove bookmark
        public ActionResult UpdateHLBookmarkStatus(int hlPostId, int type)
        {
            var resultJson = new HighlightPostRules(dbContext).UpdateHLBookmarkStatus(hlPostId, CurrentUser().Id, type);
            return Json(resultJson, JsonRequestBehavior.AllowGet);
        }

        #region Add Edit Posts
        public ActionResult SaveNewsPost(NewsHighlight newsPost, List<string> tags, S3ObjectUploadModel uploadModel = null)
        {
            newsPost.Domain = CurrentDomain();
            var saveResult = new HighlightPostRules(dbContext).AddEditNewsHighlightPost(newsPost, tags, CurrentUser().Id, uploadModel);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveArticlePost(ArticleHighlight articlePost, List<string> tags, S3ObjectUploadModel uploadModel = null)
        {
            articlePost.Domain = CurrentDomain();
            var saveResult = new HighlightPostRules(dbContext).AddEditArticlesHighlightPost(articlePost, tags, CurrentUser().Id, uploadModel);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveKnowledgePost(KnowledgeHighlight knowledgePost, List<string> tags, string countryName, S3ObjectUploadModel uploadModel = null)
        {
            knowledgePost.Domain = CurrentDomain();
            var saveResult = new HighlightPostRules(dbContext).AddEditKnowledgeHighlightPost(knowledgePost, tags, countryName, CurrentUser().Id, uploadModel);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveListingEventPost(EventListingHighlight eventPost, 
            List<string> tags, string countryName, int locationId, string startDate, string endDate,
            S3ObjectUploadModel uploadModel = null)
        {
            if(CheckReferenceDuplication(eventPost.Reference, eventPost.Id))
            {
                return Json(new ReturnJsonModel() { result = false, msg = "Event Post reference existed in the Domain." }) ;
            }

            eventPost.Domain = CurrentDomain();
            var timezone = CurrentUser().Timezone;
            var datetimeFormat = CurrentUser().DateTimeFormat;
            var tz = TimeZoneInfo.FindSystemTimeZoneById(CurrentUser().Timezone);
            try
            {
                eventPost.StartDate = TimeZoneInfo.ConvertTimeToUtc(startDate.ConvertDateFormat(datetimeFormat), tz);
            }
            catch
            {
                eventPost.StartDate = DateTime.UtcNow;
            }
            try
            {
                eventPost.EndDate = TimeZoneInfo.ConvertTimeToUtc(endDate.ConvertDateFormat(datetimeFormat), tz);
            }
            catch
            {
                eventPost.EndDate = DateTime.UtcNow;
            }

            var country = new Country();
            eventPost.Country = country;
            var saveResult = new HighlightPostRules(dbContext).AddEditEventHighlightPost(eventPost, tags, countryName, locationId, CurrentUser().Id, uploadModel);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveListingJobPost(JobListingHighlight jobPost, List<string> tags, string countryName, int locationId, S3ObjectUploadModel uploadModel = null)
        {
            if (CheckReferenceDuplication(jobPost.Reference, jobPost.Id))
            {
                return Json(new ReturnJsonModel() { result = false, msg = "Job Post reference existed in the Domain." });
            }
            jobPost.Domain = CurrentDomain();
            jobPost.ClosingDate = jobPost.ClosingDate.ConvertTimeToUtc(CurrentUser().Timezone);
            var saveResult = new HighlightPostRules(dbContext).AddEditJobHighlightPost(jobPost,tags, countryName, locationId, CurrentUser().Id, uploadModel);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveListingRealEstatePost(RealEstateListingHighlight realestatePost, List<string> tags, string countryName, int locationId, int propTypeId, List<int> propListIds, List<MediaModel> postAttachments = null, List<RealEstateImage> existedAttachments = null, List<RealEstateImage> updatedAttachments = null)
        {
            if (CheckReferenceDuplication(realestatePost.Reference, realestatePost.Id))
            {
                return Json(new ReturnJsonModel() { result = false, msg = "Real Estate Post reference existed in the Domain." });
            }
            realestatePost.Domain = CurrentDomain();
            var saveResult = new HighlightPostRules(dbContext).AddEditRealEstateHighlightPost(realestatePost, tags, countryName, locationId, propTypeId, propListIds, CurrentUser().Id, postAttachments, existedAttachments, updatedAttachments);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UnpublishHLPost(int postId)
        {
            var rs = new HighlightPostRules(dbContext).UnpublishHighlightPost(postId, CurrentUser().Id);
            return Json(rs, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// Getting Highlight Posts
        /// </summary>
        /// <param name="searchKey"></param>
        /// <param name="searchStatus"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LoadingHighlightPosts([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string searchKey, HighlightPostStatus searchStatus, HighlightPostType searchType, int start, int length, int draw, string searchListingTypesStr, string searchListingRefKey = "")
        {
            try
            {
                var totalRecord = 0;
                List<int> searchListingTypes = new List<int>();
                if (searchListingTypesStr == null)
                    searchListingTypes = null;
                else
                {
                    List<string> lstSearchTypes = JsonHelper.ParseAs<List<string>>(searchListingTypesStr);
                    lstSearchTypes.ForEach(t =>
                    {
                        var tp = Convert.ToInt32(t);
                        searchListingTypes.Add(tp);
                    });
                }
                List<HighlightPostCustomModel> lstResult = new HighlightPostRules(dbContext).GetHighlightPostPagination(CurrentUser(), CurrentDomain(), searchType, searchKey, ref totalRecord, requestModel, CurrentDomainId(), searchStatus, searchListingTypes, searchListingRefKey, start, length); ;
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
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<HighlightPostCustomModel>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }
        //[HttpGet]
        //public ActionResult ListingHighlightPosts([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string searchKey, HighlightPostStatus searchStatus, ListingType listingType, int start, int length, int draw)
        //{
        //    try
        //    {
        //        var totalRecord = 0;
        //        List<int> searchListingTypes = new List<int>();
        //        searchListingTypes.Add((int)listingType);
        //        List<HighlightPostCustomModel> lstResult = new HighlightPostRules(dbContext).GetHighlightPostPagination(CurrentUser(), CurrentDomain(), HighlightPostType.Listings, searchKey, ref totalRecord, requestModel, CurrentDomainId(), searchStatus, searchListingTypes, "", start, length); ;
        //        var dataTableData = new DataTableModel
        //        {
        //            draw = draw,
        //            data = lstResult,
        //            recordsFiltered = totalRecord,
        //            recordsTotal = totalRecord
        //        };
        //        return Json(dataTableData, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
        //        return Json(new DataTableModel() { draw = draw, data = new List<HighlightPostCustomModel>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public ActionResult ListingPostDataTableContent([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string searchKey, string searchRef, ListingType searchType, int start, int length, int draw)
        {
            try
            {
                var totalRecord = 0;
                List<FlagCustomModel> lstResult = new HighlightPostRules(dbContext).GetFlagModelPagination(CurrentUser(), searchKey, searchRef, CurrentDomainId(), searchType, CurrentUser(), ref totalRecord, requestModel, start, length);
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
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, this.CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<FlagCustomModel>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        public bool CheckReferenceDuplication(string reference, int postId)
        {
            var currentDomainId = CurrentDomainId();
            return dbContext.ListingHighlightPosts.Any(p => p.Domain.Id == currentDomainId && p.Id != postId && p.Reference == reference);
        }

        public ActionResult GetListHLLocationByCountry(string countryName)
        {
            var lstHLLocationByCountry = new AdminListingRules(dbContext).GetListLocationByCountry(countryName);
            var lstLocations = new List<Select2Option>();
            lstLocations.Add(new Select2Option { id = "0", text = "All areas" });
            lstLocations.AddRange(lstHLLocationByCountry.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }));
            return Json(lstLocations, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveRestateImage(string imgKey, int postId)
        {
            var result = new HighlightPostRules(dbContext).RemoveRealEstateImg(imgKey, postId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region Sharing Highlight Posts
        public ActionResult ShareHLPost(int type, string email, string sharedUserIds, int postId)
        {
            var currentUserId = CurrentUser().Id;
            var result = new HighlightPostRules(dbContext).ShareHLPost(type, email, postId, sharedUserIds, currentUserId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="domainKey"></param>
        /// <param name="type"> 1 - Hide, 2 - Unhide </param>
        /// <returns></returns>
        public ActionResult HideDomainHLPost(string domainKey, int type)
        {
            var domainId = string.IsNullOrEmpty(domainKey) ? 0 : domainKey.Decrypt2Int();
            var currentUserId = CurrentUser().Id;

            var updateResult = new HighlightPostRules(dbContext).ChangeHiddingHLDomainPostStatus(domainId, currentUserId, type);

            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }
    }
}