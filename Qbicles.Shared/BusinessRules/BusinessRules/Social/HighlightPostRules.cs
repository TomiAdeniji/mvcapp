using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.C2C;
using Qbicles.BusinessRules.Community;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Community;
using Qbicles.Models.Highlight;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.BusinessRules.Social
{
    public class HighlightPostRules
    {
        private ApplicationDbContext dbContext;

        public HighlightPostRules(ApplicationDbContext context)
        {
            this.dbContext = context;
        }

        public HighlightModel GetListHighlightPostCustomById(int postId, string userId, UserSetting user)
        {
            var lstConnectedBusinessIdWithCurrentUser = dbContext.B2CQbicles
                    .Where(p => p.Customer.Id == userId
                    && !p.RemovedForUsers.Any(x => x.Id == userId)
                    && p.Business.Status != QbicleDomain.DomainStatusEnum.Closed
                    && p.Status != CommsStatus.Blocked).Select(p => p.Business.Id).ToList();

            var postItem = dbContext.HighlightPosts.FirstOrDefault(p => p.Id == postId);
            if (postItem != null)
            {
                var postCustomModel = new HighlightModel();
                postCustomModel = new HighlightModel()
                {
                    Id = postItem.Id,
                    Type = postItem.Type,
                    Title = postItem.Title,
                    Content = postItem.Content,
                    Tags = postItem.Tags.Select(t => new BaseModel { Id = t.Id, Name = t.Name }).ToList(),
                    //CreatedBy = postItem.CreatedBy,
                    CreatedDate = postItem.CreatedDate,
                    LikedTimes = postItem.LikedTimes,
                    IsCreatedByCurrentUser = postItem.CreatedBy.Id == userId ? true : false,
                    ImageUri = postItem.ImgUri.ToDocumentUri(),
                    LogoUri = (postItem.Domain?.LogoUri ?? ConfigManager.DefaultProductPlaceholderImageUrl).ToDocumentUri(),

                    CreatedDateString = postItem.CreatedDate.GetTimeRelative(),
                    IsBookmarkedByCurrentUser = postItem.BookmarkedBy.Any(b => b.Id == userId) ? true : false,
                    IsLikedByCurrentUser = postItem.LikedBy.Any(l => l.Id == userId) ? true : false,
                    IsDomainFollowedByCurrentUser = lstConnectedBusinessIdWithCurrentUser.Any(x => x == postItem.Domain.Id),
                    BusinessName = postItem.Domain?.Name ?? "",
                    DomainId = postItem.Domain?.Id ?? 0,
                    DomainKey = postItem.Domain?.Key ?? ""
                };

                if (postItem is ListingHighlight)
                {
                    postCustomModel.HLListingType = (postItem as ListingHighlight).ListingHighlightType;
                    postCustomModel.IsFlaggedByCurrentUser = (postItem as ListingHighlight).FlaggedBy.Any(f => f.Id == userId) ? true : false;
                }

                // Event Listing Posts information
                if (postItem is EventListingHighlight)
                {
                    var eventListingModel = (postItem as EventListingHighlight);
                    postCustomModel.StartTimeString = eventListingModel.StartDate.ConvertTimeFromUtc(user.Timezone).ToString(user.DateTimeFormat);
                    postCustomModel.EndTimeString = eventListingModel.EndDate.ConvertTimeFromUtc(user.Timezone).ToString(user.DateTimeFormat);
                    postCustomModel.EventLocation = eventListingModel.EventLocation ?? "";
                }

                var currentTime = DateTime.UtcNow;
                var timeDifference = currentTime - postItem.CreatedDate;
                var hourDifference = timeDifference.TotalHours;
                postCustomModel.HoursAgo = (int)(hourDifference / 10) * 10;
                postCustomModel.MinutesAgo = (int)((hourDifference - postCustomModel.HoursAgo) * 60);

                return postCustomModel;
            }
            else
            {
                return null;
            }
        }

        public HighlightPostStreamModel GetListHlPostCustomPagination(UserSetting user, int createdDomainId, string keySearch,
            HighlightPostType hlTypeSearch, ListingType lsTypeSearch, List<string> tagsSearch, bool isBookmarked,
            bool isFlagged, int pageIndex, string countryName = "", string eventDateRange = "", string newsPublishedDate = "",
            List<int> rePropTypeIds = null, int reBedroomNumber = 0, int reBathroomNumber = 0, List<int> rePropertyIds = null, int areaId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, user.Id, keySearch, tagsSearch, pageIndex);

                var resultList = new List<HighlightModel>();
                var totalNumber = 0;
                var totalPosts = 0;
                var totalFollowers = 0;

                var lstBlockedAndRemovedDomaiIds = dbContext.B2CQbicles
                    .Where(p => p.Customer.Id == user.Id
                    && (p.RemovedForUsers.Any(x => x.Id == user.Id)
                    || p.Business.Status == QbicleDomain.DomainStatusEnum.Closed
                    || p.Status == CommsStatus.Blocked)).Select(p => p.Business.Id).ToList();

                var query = from post in dbContext.HighlightPosts
                            where post.Status == HighlightPostStatus.Active
                                && post.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
                                && !lstBlockedAndRemovedDomaiIds.Contains(post.Domain.Id)
                            select post;

                #region Filter

                if (!string.IsNullOrEmpty(keySearch))
                {
                    query = query.Where(p => p.Title.ToLower().Contains(keySearch.ToLower())
                                || p.Content.ToLower().Contains(keySearch.ToLower()));
                }

                if (createdDomainId > 0)
                {
                    query = query.Where(p => p.Domain.Id == createdDomainId);
                    totalPosts = query.Count();
                    var userConnectedWithHLDomainNumber = dbContext.B2CQbicles
                        .Where(p => p.Business.Id == createdDomainId && p.Status != CommsStatus.Blocked).Select(p => p.Customer.Id).Distinct().Count();
                    totalFollowers = userConnectedWithHLDomainNumber;
                }
                else
                {
                    query = query.Where(p => !p.Domain.HighlightPosterHiddenUser.Any(x => x.Id == user.Id));
                }

                if (isBookmarked && !isFlagged)
                {
                    query = query.Where(p => p.BookmarkedBy.Select(t => t.Id).ToList().Contains(user.Id));
                }
                if (isFlagged && !isBookmarked)
                {
                    query = query.Where(p => (p as ListingHighlight).FlaggedBy.Select(f => f.Id).ToList().Contains(user.Id));
                }

                if (tagsSearch != null && tagsSearch.Count > 0)
                {
                    query = query.Where(p => p.Tags.Any(t => tagsSearch.Contains(t.Name)));
                }

                if (hlTypeSearch != 0 && hlTypeSearch != HighlightPostType.News)
                {
                    query = query.Where(p => p.Type == hlTypeSearch);
                }
                else if (hlTypeSearch == HighlightPostType.News)
                {
                    query = query.Where(p => p.Type == HighlightPostType.News || p.Type == HighlightPostType.Article);
                }
                if (hlTypeSearch == HighlightPostType.Listings && lsTypeSearch > 0)
                {
                    query = query.Where(p => (p as ListingHighlight).ListingHighlightType == lsTypeSearch);
                }

                //Filter for specific type
                if (hlTypeSearch == HighlightPostType.News)
                {
                    if (!string.IsNullOrEmpty(newsPublishedDate))
                    {
                        var datesearch = newsPublishedDate.ConvertDateFormat(user.DateFormat).ConvertTimeToUtc(user.Timezone);
                        var dateAdd1Day = datesearch.AddDays(1);
                        query = query.Where(p => (p.CreatedDate >= datesearch && p.CreatedDate < dateAdd1Day));
                    }
                }

                if (hlTypeSearch == HighlightPostType.Knowledge)
                {
                    if (!string.IsNullOrEmpty(countryName))
                    {
                        query = query.Where(p => (p as KnowledgeHighlight).Country.CommonName == countryName || string.IsNullOrEmpty((p as KnowledgeHighlight).Country.CommonName));
                    }
                }

                if (hlTypeSearch == HighlightPostType.Listings)
                {
                    if (lsTypeSearch == ListingType.Event)
                    {
                        if (!String.IsNullOrEmpty(eventDateRange))
                        {
                            var startTime = DateTime.UtcNow;
                            var endTime = DateTime.UtcNow;

                            HelperClass.ConvertDaterangeFormat(eventDateRange, user.DateFormat, user.Timezone, out startTime, out endTime, HelperClass.endDateAddedType.minute);
                            startTime = startTime.Date;
                            endTime = endTime.Date.AddDays(1).AddMinutes(-1);
                            query = query.Where(p => (p as EventListingHighlight).StartDate >= startTime && (p as EventListingHighlight).EndDate <= endTime);
                        }

                        if (!string.IsNullOrEmpty(countryName))
                        {
                            query = query.Where(p => (p as EventListingHighlight).Country.CommonName == countryName || String.IsNullOrEmpty((p as EventListingHighlight).Country.CommonName));
                            if (areaId > 0)
                            {
                                query = query.Where(p => (p as EventListingHighlight).ListingLocation == null || (p as EventListingHighlight).ListingLocation.Id == areaId);
                            }
                        }
                    }

                    if (lsTypeSearch == ListingType.Job)
                    {
                        if (!string.IsNullOrEmpty(countryName.Trim()))
                        {
                            query = query.Where(p => (p as JobListingHighlight).Country.CommonName == countryName || string.IsNullOrEmpty((p as JobListingHighlight).Country.CommonName));
                            if (areaId > 0)
                            {
                                query = query.Where(p => (p as JobListingHighlight).ListingLocation == null || (p as JobListingHighlight).ListingLocation.Id == areaId);
                            }
                        }
                    }

                    if (lsTypeSearch == ListingType.RealEstate)
                    {
                        if (!string.IsNullOrEmpty(countryName))
                        {
                            query = query.Where(p => (p as RealEstateListingHighlight).Country.CommonName == countryName || string.IsNullOrEmpty((p as RealEstateListingHighlight).Country.CommonName));
                            if (areaId > 0)
                            {
                                query = query.Where(p => (p as RealEstateListingHighlight).ListingLocation == null || (p as RealEstateListingHighlight).ListingLocation.Id == areaId);
                            }
                        }

                        if (rePropTypeIds != null && rePropTypeIds.Count > 0 && !rePropTypeIds.Contains(0))
                        {
                            query = query.Where(p => rePropTypeIds.Contains((p as RealEstateListingHighlight).PropType.Id));
                        }

                        if (rePropertyIds != null && rePropertyIds.Count > 0 && !rePropertyIds.Contains(0))
                        {
                            query = query.Where(p => rePropertyIds.Any(x => (p as RealEstateListingHighlight).IncludedProperties.Any(propItem => propItem.Id == x)));
                        }

                        if (reBedroomNumber > 0)
                        {
                            query = query.Where(p => (p as RealEstateListingHighlight).BedRoomNum == reBedroomNumber
                                        || (reBedroomNumber == 5 && (p as RealEstateListingHighlight).BedRoomNum >= reBedroomNumber));
                        }

                        if (reBathroomNumber > 0)
                        {
                            query = query.Where(p => (p as RealEstateListingHighlight).BathRoomNum == reBathroomNumber
                                                                    || (reBathroomNumber == 3 && (p as RealEstateListingHighlight).BathRoomNum >= reBathroomNumber));
                        }
                    }
                }

                #endregion Filter

                totalNumber = query.Count();

                #region Ordering

                query = query.OrderByDescending(p => p.CreatedDate);

                #endregion Ordering

                #region Paging

                var lstPost = query.Skip(pageIndex * HelperClass.qbiclePageSize).Take(HelperClass.qbiclePageSize).ToList();
                //var lstPost = query.Take(pageIndex).ToList();

                #endregion Paging

                var currentUser = dbContext.QbicleUser.Find(user.Id);

                var lstConnectedBusinessIdWithCurrentUser = dbContext.B2CQbicles.AsNoTracking()
                    .Where(p => p.Customer.Id == user.Id
                    && !p.RemovedForUsers.Any(x => x.Id == user.Id)
                    && p.Business.Status != QbicleDomain.DomainStatusEnum.Closed
                    && p.Status != CommsStatus.Blocked).Select(p => p.Business.Id).ToList();

                foreach (var postItem in lstPost)
                {
                    var postCustomModel = MapHighlightPost2HighlightModel(postItem, currentUser.Id, currentUser.Timezone, user.DateFormat + ' ' + user.TimeFormat, lstConnectedBusinessIdWithCurrentUser);
                    resultList.Add(postCustomModel);
                }
                //model.TotalCount /= HelperClass.qbiclePageSize;
                return new HighlightPostStreamModel()
                {
                    HighlightPosts = resultList.OrderByDescending(d => d.CreatedDate).ToList(),
                    TotalPage = ((totalNumber % HelperClass.qbiclePageSize) == 0) ? (totalNumber / HelperClass.qbiclePageSize) : (totalNumber / HelperClass.qbiclePageSize) + 1,
                    BookmarkedNumber = currentUser.BookmarkedHighlightPosts.Distinct().Count(),
                    FlaggedNumber = currentUser.FlaggedListings.Distinct().Count(),
                    DomainPosts = totalPosts,
                    DomainFollowers = totalFollowers
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, user.Id, keySearch, tagsSearch, pageIndex);
                throw ex;
            }
        }

        public HighlightModel MapHighlightPost2HighlightModel(HighlightPost postItem, string userId, string timezone, string datetimeFormat, List<int> lstConnectedBusinessIdWithCurrentUser)
        {
            var image = postItem.ImgUri;
            if (string.IsNullOrEmpty(image))
                switch (postItem.Type)
                {
                    case HighlightPostType.Article:
                        image = ConfigManager.HighlightBannerEvent;
                        break;

                    case HighlightPostType.News:
                        image = ConfigManager.HighlightBannerNew;
                        break;

                    case HighlightPostType.Knowledge:
                        image = ConfigManager.HighlightBannerKnowledge;
                        break;

                    case HighlightPostType.Listings:
                        image = ConfigManager.HighlightBannerListing;
                        break;
                }
            var postCustomModel = new HighlightModel()
            {
                Id = postItem.Id,
                Type = postItem.Type,
                TypeName = postItem.Type.GetDescription(),
                Title = postItem.Title,
                ImageUri = image.ToDocumentUri(),
                Content = postItem.Content,
                Tags = postItem.Tags.Select(t => new BaseModel { Id = t.Id, Name = t.Name }).ToList(),
                CreatedDate = postItem.CreatedDate,
                CreatedDateString = postItem.CreatedDate.GetTimeRelative(),
                LikedTimes = postItem.LikedBy.Count(),
                IsCreatedByCurrentUser = postItem.CreatedBy.Id == userId ? true : false,
                IsBookmarkedByCurrentUser = postItem.BookmarkedBy.Any(b => b.Id == userId) ? true : false,
                IsLikedByCurrentUser = postItem.LikedBy.Any(l => l.Id == userId) ? true : false,
                IsDomainFollowedByCurrentUser = lstConnectedBusinessIdWithCurrentUser.Any(x => x == postItem.Domain.Id),
                LogoUri = (postItem.Domain?.LogoUri ?? ConfigManager.DefaultProductPlaceholderImageUrl).ToDocumentUri(),
                BusinessName = postItem.Domain?.Name ?? "",
                DomainId = postItem.Domain?.Id ?? 0,
                DomainKey = postItem.Domain?.Key ?? "",
                IsDomainHiddenByCurrentUser = postItem.Domain.HighlightPosterHiddenUser.Any(x => x.Id == userId)
            };

            var otherNumberLike = postCustomModel.LikedTimes;
            if (postCustomModel.IsLikedByCurrentUser)
            {
                otherNumberLike = postCustomModel.LikedTimes - 1;
                if (otherNumberLike > 0)
                {
                    postCustomModel.LikeStatusString = "You and " + otherNumberLike + " people love this";
                }
                else
                {
                    postCustomModel.LikeStatusString = "You love this";
                }
            }
            else
            {
                postCustomModel.LikeStatusString = otherNumberLike + " people love this";
            }

            //News information
            if (postItem.Type == HighlightPostType.News)
            {
                postCustomModel.Citation = (postItem as NewsHighlight).NewsCitation ?? "";
                postCustomModel.HyperLink = (postItem as NewsHighlight).NewsHyperLink ?? "";
            }

            //Article information

            //Knowledge information
            if (postItem.Type == HighlightPostType.Knowledge)
            {
                postCustomModel.CountryName = (postItem as KnowledgeHighlight).Country?.CommonName ?? "";
                postCustomModel.Citation = (postItem as KnowledgeHighlight).KnowledgeCitation ?? "";
                postCustomModel.HyperLink = (postItem as KnowledgeHighlight).KnowledgeHyperlink ?? "";
            }

            // Listing Posts information
            if (postItem.Type == HighlightPostType.Listings)
            {
                var listingModel = (postItem as ListingHighlight);
                postCustomModel.IsFlaggedByCurrentUser = listingModel.FlaggedBy.Any(f => f.Id == userId) ? true : false;
                postCustomModel.Reference = listingModel.Reference;
                postCustomModel.CountryName = listingModel.Country?.CommonName ?? "";
                postCustomModel.AreaName = listingModel.ListingLocation?.Name ?? "";
                postCustomModel.ListingTypeName = listingModel.ListingHighlightType.GetDescription();
                postCustomModel.HLListingType = listingModel.ListingHighlightType;

                // Event Listing Posts information
                if (listingModel.ListingHighlightType == ListingType.Event)
                {
                    var eventListingModel = (listingModel as EventListingHighlight);
                    postCustomModel.StartTimeString = eventListingModel.StartDate.ConvertTimeFromUtc(timezone).ToString(datetimeFormat);
                    postCustomModel.EndTimeString = eventListingModel.EndDate.ConvertTimeFromUtc(timezone).ToString(datetimeFormat);
                    postCustomModel.EventLocation = eventListingModel.EventLocation ?? "";
                }

                // Job Listing Posts information
                if (listingModel.ListingHighlightType == ListingType.Job)
                {
                    var jobListingModel = (listingModel as JobListingHighlight);
                    postCustomModel.Salary = jobListingModel.Salary ?? "";
                    postCustomModel.ClosingDateString = jobListingModel.ClosingDate.ConvertTimeFromUtc(timezone).ToString(datetimeFormat);
                    postCustomModel.SkillRequired = jobListingModel.SkillRequired ?? "";
                }

                // Real Estate Posts information
                if (listingModel.ListingHighlightType == ListingType.RealEstate)
                {
                    var realestateListingModel = (listingModel as RealEstateListingHighlight);
                    postCustomModel.Price = realestateListingModel.PricingInfo;
                    postCustomModel.SelectedPropertyTypeName = realestateListingModel.PropType == null ? "" : realestateListingModel.PropType.Name;
                    postCustomModel.SelectedProperties = new List<string>();
                    if (realestateListingModel.IncludedProperties != null && realestateListingModel.IncludedProperties.Count > 0)
                    {
                        realestateListingModel.IncludedProperties.ForEach(prop =>
                        {
                            postCustomModel.SelectedProperties.Add(prop.Name);
                        });
                    }
                    postCustomModel.BedroomNum = realestateListingModel.BedRoomNum;
                    postCustomModel.BathroomNum = realestateListingModel.BathRoomNum;
                }
            }

            var currentTime = DateTime.UtcNow;
            var timeDifference = currentTime - postItem.CreatedDate;
            var dayDifference = timeDifference.TotalDays;
            var hourDifference = timeDifference.TotalHours;
            var minuteDifference = timeDifference.TotalMinutes;

            postCustomModel.DaysAgo = (int)(dayDifference);
            postCustomModel.HoursAgo = (int)(hourDifference - postCustomModel.DaysAgo * 24);
            postCustomModel.MinutesAgo = (int)(minuteDifference - postCustomModel.DaysAgo * 24 * 60 - postCustomModel.HoursAgo * 60);

            if (postCustomModel.Type == HighlightPostType.Listings && postCustomModel.HLListingType == ListingType.RealEstate)
                postCustomModel.ShowPropertyInfo = true;

            if (postCustomModel.Type == HighlightPostType.Article)
                postCustomModel.ShowReadArticle = true;

            if (postCustomModel.Type == HighlightPostType.Knowledge)
                postCustomModel.ShowExternalSite = true;

            return postCustomModel;
        }

        public ReturnJsonModel AddEditHighlightPost(HighlightPost highlightPost, List<string> tags, string userId, S3ObjectUploadModel uploadModel)
        {
            var result = new ReturnJsonModel { actionVal = 1 };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, highlightPost, userId, uploadModel);
                var user = new UserRules(dbContext).GetById(userId);

                if (tags != null && tags.Count > 0)
                {
                    var tagRules = new TagRules(dbContext);
                    foreach (var tagItem in tags)
                    {
                        if (!String.IsNullOrEmpty(tagItem))
                        {
                            var tagInDb = tagRules.GetFirstTagByName(tagItem);
                            if (tagInDb == null)
                            {
                                Tag _tag = new Tag()
                                {
                                    Id = 0,
                                    Name = tagItem,
                                };
                                tagRules.SaveTag(_tag, null, userId);
                                tagInDb = tagRules.GetFirstTagByName(tagItem);
                            }

                            if (highlightPost.Tags == null)
                            {
                                highlightPost.Tags = new List<Tag>();
                            }
                            highlightPost.Tags.Add(tagInDb);
                        }
                    }
                }

                //Start: Processing with file uploaded
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);

                        s3Rules.ProcessingMediaS3(uploadModel.FileKey);
                    }
                }
                else
                {
                    // Processing with default image
                    uploadModel = new S3ObjectUploadModel
                    {
                        FileKey = ""
                    };
                }
                //End: processing with file uploaded

                highlightPost.CreatedBy = user;
                highlightPost.CreatedDate = DateTime.UtcNow;
                highlightPost.ImgUri = uploadModel.FileKey;

                if (highlightPost.Id <= 0)
                {
                    dbContext.HighlightPosts.Add(highlightPost);
                    dbContext.Entry(highlightPost).State = EntityState.Added;
                    dbContext.SaveChanges();

                    result.result = true;
                    return result;
                }
                else
                {
                    var postInDb = dbContext.HighlightPosts.FirstOrDefault(p => p.Id == highlightPost.Id);
                    if (postInDb != null)
                    {
                        postInDb.Type = highlightPost.Type;
                        postInDb.Title = highlightPost.Title;
                        postInDb.Content = highlightPost.Content;
                        postInDb.Tags = highlightPost.Tags;
                        if (!string.IsNullOrEmpty(highlightPost.ImgUri))
                        {
                            postInDb.ImgUri = highlightPost.ImgUri;
                        }

                        result.actionVal = 2;
                        dbContext.Entry(postInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        result.result = true;
                    }
                    else
                    {
                        result.actionVal = 2;
                        result.msg = "Highlight Post does not exist.";

                        result.result = false;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, highlightPost, userId, uploadModel);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel DeleteHighlightPost(int postId)
        {
            var result = new ReturnJsonModel() { actionVal = 3 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, postId);

                var hlPost = dbContext.HighlightPosts.FirstOrDefault(p => p.Id == postId);
                if (hlPost != null)
                {
                    if (hlPost.Tags != null)
                    {
                        hlPost.Tags = null;
                        dbContext.Entry(hlPost).State = EntityState.Modified;
                        var modifyResult = dbContext.SaveChanges();

                        if (modifyResult > 0)
                        {
                            dbContext.HighlightPosts.Remove(hlPost);
                            dbContext.Entry(hlPost).State = EntityState.Deleted;
                            dbContext.SaveChanges();
                        }
                    }
                    else
                    {
                        dbContext.HighlightPosts.Remove(hlPost);
                        dbContext.Entry(hlPost).State = EntityState.Deleted;
                        dbContext.SaveChanges();
                    }
                }
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, postId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel PostLikeProcess(int highlightPostId, string userId)
        {
            var result = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, highlightPostId, userId);

                var postInDb = dbContext.HighlightPosts.FirstOrDefault(p => p.Id == highlightPostId);
                if (postInDb == null)
                {
                    result.result = false;
                    result.msg = "Highlight Post does not exist.";
                    return result;
                }

                var userpostxref = dbContext.UserAndHighlightPostXrefs.FirstOrDefault(p => p.HighlightPostId == highlightPostId && p.UserId == userId);
                if (userpostxref == null)
                {
                    userpostxref = new UserAndHighlightPostXref()
                    {
                        HighlightPostId = highlightPostId,
                        IsLiked = false,
                        UserId = userId
                    };

                    dbContext.UserAndHighlightPostXrefs.Add(userpostxref);
                    dbContext.Entry(userpostxref).State = EntityState.Added;
                    dbContext.SaveChanges();
                }

                userpostxref.IsLiked = !userpostxref.IsLiked;
                dbContext.Entry(userpostxref).State = EntityState.Modified;
                dbContext.SaveChanges();

                if (userpostxref.IsLiked)
                {
                    postInDb.LikedTimes++;
                }
                else
                {
                    postInDb.LikedTimes--;
                }
                dbContext.Entry(postInDb).State = EntityState.Modified;
                dbContext.SaveChanges();

                result.Object = postInDb.LikedTimes;
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, highlightPostId, userId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public List<string> GetHighlightPostTagName()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);

                var listTagsName = new List<string>();
                var listPosts = dbContext.HighlightPosts.ToList();
                listPosts.ForEach(p => p.Tags.ForEach(t =>
                {
                    if (!listTagsName.Contains(t.Name))
                    {
                        listTagsName.Add(t.Name);
                    }
                }));
                return listTagsName;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null);
                return null;
            }
        }

        public List<Tag> GetHighlightPostTags()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);

                var listTags = new List<Tag>();
                var listPosts = dbContext.HighlightPosts.ToList();
                listPosts.ForEach(p => p.Tags.ForEach(t =>
                {
                    if (!listTags.Any(item => item.Name == t.Name))
                    {
                        listTags.Add(t);
                    }
                }));
                return listTags;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null);
                return null;
            }
        }

        public List<HighlightPostCustomModel> GetHighlightPostPagination(UserSetting user,
            QbicleDomain currentDomain, HighlightPostType searchType, string searchKey, ref int totalRecord, IDataTablesRequest requestModel, int domainId = 0,
            HighlightPostStatus searchStatus = 0, List<int> searchListingTypes = null, string searchRef = "", int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, user,
                        searchType, searchKey, totalRecord, requestModel, domainId, searchStatus, searchListingTypes);

                var query = from hlPost in dbContext.HighlightPosts where hlPost.Type == searchType && hlPost.Domain.Id == currentDomain.Id select hlPost;
                if (searchType == HighlightPostType.Listings)
                {
                    searchRef = searchRef.Trim();
                    if (searchListingTypes == null)
                    {
                        query = from listingHlPost in dbContext.ListingHighlightPosts
                                where listingHlPost.Domain.Id == currentDomain.Id
                                && (string.IsNullOrEmpty(searchRef) || listingHlPost.Reference == searchRef)
                                select listingHlPost;
                    }
                    else
                    {
                        query = from listingHlPost in dbContext.ListingHighlightPosts
                                where searchListingTypes.Contains((int)listingHlPost.ListingHighlightType)
                                && listingHlPost.Domain.Id == currentDomain.Id
                                && (string.IsNullOrEmpty(searchRef) || listingHlPost.Reference == searchRef)
                                select listingHlPost;
                    }
                }

                #region Filtering

                if (!string.IsNullOrEmpty(searchKey))
                {
                    query = query.Where(p => p.Title.ToLower().Contains(searchKey.ToLower()));
                }
                if (searchStatus != 0)
                {
                    query = query.Where(p => p.Status == (HighlightPostStatus)searchStatus);
                }

                #endregion Filtering

                totalRecord = query.Count();

                #region Ordering

                var columns = requestModel.Columns;
                var orderString = "";
                foreach (var columnItem in columns.GetSortedColumns())
                {
                    switch (columnItem.Data)
                    {
                        case "Title":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += columnItem.SortDirection == TB_Column.OrderDirection.Descendant ? "Title desc" : "Title asc";
                            break;

                        case "Added":
                        case "CreatedDate":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += columnItem.SortDirection == TB_Column.OrderDirection.Descendant ? "CreatedDate desc" : "CreatedDate asc";
                            break;

                        case "StatusLabel":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += columnItem.SortDirection == TB_Column.OrderDirection.Descendant ? "Status desc" : "Status asc";
                            break;
                    }
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderString) ? "CreatedDate desc" : orderString);

                #endregion Ordering

                #region Paging

                query = query.Skip(start).Take(length);
                var listHighlight = query.ToList();

                #endregion Paging

                var listHighlightCustom = new List<HighlightPostCustomModel>();
                listHighlight.ForEach(highlight =>
                {
                    var hlCustomItem = new HighlightPostCustomModel
                    {
                        Id = highlight.Id,
                        Title = highlight.Title,
                        CreatedDate = highlight.CreatedDate.ConvertTimeFromUtc(user.Timezone).ToString(user.DateTimeFormat),
                        Status = highlight.Status.GetDescription() ?? "",
                        StatusLabel = GetHighlightStatusLabelByStatus(highlight.Status),
                        Category = "",
                        Country = "",
                        IsPublished = highlight.Status != HighlightPostStatus.InActive,
                        HlType = highlight.Type,
                        PersonName = highlight.CreatedBy?.GetFullName() ?? "",
                        PersonImgUri = highlight.CreatedBy?.ProfilePic.ToUriString() ?? ""
                    };

                    if (searchType == HighlightPostType.Listings)
                    {
                        var listingPost = highlight as ListingHighlight;
                        hlCustomItem.LsType = listingPost.ListingHighlightType;
                        hlCustomItem.Reference = listingPost.Reference;
                        hlCustomItem.Category = listingPost.ListingHighlightType.GetDescription();
                        hlCustomItem.Country = (listingPost.Country == null || listingPost.Country.CommonName == null) ? "Available everywhere" : listingPost.Country.CommonName;
                    }

                    listHighlightCustom.Add(hlCustomItem);
                });
                return listHighlightCustom;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, user, searchType,
                    searchKey, totalRecord, requestModel, domainId, searchStatus, searchListingTypes);
                return new List<HighlightPostCustomModel>();
            }
        }

        public List<FlagCustomModel> GetFlagModelPagination(UserSetting user, string searchKey, string searchRef, int domainId, ListingType listingType, UserSetting currentUser, ref int totalRecord,
            IDataTablesRequest requestModel, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, user,
                        searchRef, searchKey, totalRecord, requestModel, domainId);

                var query = from hlPost in dbContext.ListingHighlightPosts
                            from u in hlPost.FlaggedBy
                            where hlPost.Domain.Id == domainId
                            && hlPost.ListingHighlightType == listingType
                            //&& hlPost.FlaggedBy.Any()
                            select new PostFlagCustomModel
                            {
                                Post = hlPost,
                                User = u
                            };

                #region Filtering

                if (!string.IsNullOrEmpty(searchKey))
                {
                    query = query.Where(p => p.Post.Title.Contains(searchKey)
                        || p.User.DisplayUserName.Contains(searchKey)
                        || p.User.Forename.Contains(searchKey)
                        || p.User.Email.Contains(searchKey)
                        || p.User.Surname.Contains(searchKey)
                        || p.Post.Reference.Contains(searchKey));
                }
                if (!string.IsNullOrEmpty(searchRef.Trim()))
                {
                    query = query.Where(p => p.Post.Reference == searchRef);
                }

                #endregion Filtering

                var columns = requestModel.Columns;
                var orderString = "";
                foreach (var columnItem in columns.GetSortedColumns())
                {
                    switch (columnItem.Data)
                    {
                        case "Title":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += columnItem.SortDirection == TB_Column.OrderDirection.Descendant ? "Post.Title desc" : "Post.Title asc";
                            break;

                        case "PersonName":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "User.Forename" + (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            orderString += orderString != string.Empty ? "," : "";
                            orderString += "User.Surname" + (columnItem.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "CreateDateString":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Post.CreatedDate" + (columnItem.SortDirection == TB_Column.OrderDirection.Descendant ? " desc" : " asc");
                            break;

                        case "PostReference":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Post.Reference" + (columnItem.SortDirection == TB_Column.OrderDirection.Descendant ? " desc" : " asc");
                            break;
                    }
                }
                query = query.OrderBy(string.IsNullOrEmpty(orderString) ? "Post.CreatedDate desc" : orderString);
                totalRecord = query.Count();
                query = query.Skip(start).Take(length);
                var lstPosts = query.ToList();
                var listModels = new List<FlagCustomModel>();
                lstPosts.ForEach(p =>
                {
                    #region Get detail info

                    var item = new FlagCustomModel()
                    {
                        FlaggedUserId = p.User.Id,
                        DomainId = p.Post.Domain.Id,
                        PostId = p.Post.Id,
                        Title = p.Post.Title,
                        PostReference = p.Post.Reference,
                        CreatedDate = p.Post.CreatedDate,
                        PersonName = p.User.GetFullName(),
                        PersonImageUri = p.User.ProfilePic.ToUriString(),
                        CreateDateString = p.Post.CreatedDate.ConvertTimeFromUtc(currentUser.Timezone).ToString(currentUser.DateTimeFormat)
                    };
                    var bProfile = dbContext.B2BProfiles.FirstOrDefault(bp => bp.Domain.Id == item.DomainId);
                    if (bProfile != null)
                    {
                        item.BusinessProfileId = bProfile.Id.ToString();
                        var b2cQBicle = dbContext.B2CQbicles.FirstOrDefault(b2c => b2c.Business.Id == item.DomainId && b2c.Customer.Id == item.FlaggedUserId);
                        if (b2cQBicle != null)
                            item.HasB2CChat = true;
                        else
                            item.HasB2CChat = false;
                    }
                    else
                    {
                        item.BusinessProfileId = "";
                        item.HasB2CChat = false;
                    }

                    #endregion Get detail info

                    listModels.Add(item);
                });
                return listModels;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, user, searchKey, totalRecord, requestModel, domainId, searchRef, listingType);
                return new List<FlagCustomModel>();
            }
        }

        public ReturnJsonModel AddEditNewsHighlightPost(NewsHighlight newsPost, List<string> tags, string userId, S3ObjectUploadModel uploadModel)
        {
            var result = new ReturnJsonModel { actionVal = 1 };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, newsPost, userId, uploadModel);
                var user = new UserRules(dbContext).GetById(userId);

                if (tags != null && tags.Count > 0)
                {
                    var tagRules = new TagRules(dbContext);
                    foreach (var tagItem in tags)
                    {
                        if (!String.IsNullOrEmpty(tagItem))
                        {
                            var tagInDb = tagRules.GetFirstTagByName(tagItem);
                            if (tagInDb == null)
                            {
                                Tag _tag = new Tag()
                                {
                                    Id = 0,
                                    Name = tagItem,
                                };
                                tagRules.SaveTag(_tag, null, userId);
                                tagInDb = tagRules.GetFirstTagByName(tagItem);
                            }

                            if (newsPost.Tags == null)
                            {
                                newsPost.Tags = new List<Tag>();
                            }
                            newsPost.Tags.Add(tagInDb);
                        }
                    }
                }

                //Start: Processing with file uploaded
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);

                        s3Rules.ProcessingMediaS3(uploadModel.FileKey);
                    }
                }
                else
                {
                    // Processing with default image
                    uploadModel = new S3ObjectUploadModel
                    {
                        FileKey = ""
                    };
                }
                //End: processing with file uploaded

                newsPost.CreatedBy = user;
                newsPost.CreatedDate = DateTime.UtcNow;
                newsPost.ImgUri = uploadModel.FileKey;
                newsPost.Type = HighlightPostType.News;

                if (newsPost.Id <= 0)
                {
                    newsPost.Status = HighlightPostStatus.Active;
                    dbContext.NewsHighlightPosts.Add(newsPost);
                    dbContext.Entry(newsPost).State = EntityState.Added;
                    dbContext.SaveChanges();

                    result.result = true;
                    return result;
                }
                else
                {
                    var postInDb = dbContext.NewsHighlightPosts.FirstOrDefault(p => p.Id == newsPost.Id);
                    if (postInDb != null)
                    {
                        postInDb.Title = newsPost.Title;
                        postInDb.Content = newsPost.Content;
                        postInDb.Tags = new List<Tag>();

                        if (tags != null && tags.Count > 0)
                        {
                            var tagRules = new TagRules(dbContext);
                            foreach (var tagItem in tags)
                            {
                                if (!String.IsNullOrEmpty(tagItem))
                                {
                                    var tagInDb = tagRules.GetFirstTagByName(tagItem);
                                    if (tagInDb == null)
                                    {
                                        Tag _tag = new Tag()
                                        {
                                            Id = 0,
                                            Name = tagItem,
                                        };
                                        tagRules.SaveTag(_tag, null, userId);
                                        tagInDb = tagRules.GetFirstTagByName(tagItem);
                                    }

                                    if (newsPost.Tags == null)
                                    {
                                        newsPost.Tags = new List<Tag>();
                                    }
                                    postInDb.Tags.Add(tagInDb);

                                    if (tagInDb.HLPosts == null)
                                    {
                                        tagInDb.HLPosts = new List<HighlightPost>();
                                    }
                                    tagInDb.HLPosts.Add(postInDb);
                                }
                            }
                        }

                        postInDb.NewsHyperLink = newsPost.NewsHyperLink;
                        if (!string.IsNullOrEmpty(newsPost.ImgUri))
                        {
                            postInDb.ImgUri = newsPost.ImgUri;
                        }
                        postInDb.NewsCitation = newsPost.NewsCitation;

                        result.actionVal = 2;
                        dbContext.Entry(postInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        result.result = true;
                    }
                    else
                    {
                        result.actionVal = 2;
                        result.msg = "News Highlight Post does not exist.";

                        result.result = false;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, newsPost, userId, uploadModel);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public ReturnJsonModel AddEditArticlesHighlightPost(ArticleHighlight articlePost, List<string> tags, string userId, S3ObjectUploadModel uploadModel)
        {
            var result = new ReturnJsonModel { actionVal = 1 };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, articlePost, userId, uploadModel);
                var user = new UserRules(dbContext).GetById(userId);

                if (tags != null && tags.Count > 0)
                {
                    var tagRules = new TagRules(dbContext);
                    foreach (var tagItem in tags)
                    {
                        if (!String.IsNullOrEmpty(tagItem))
                        {
                            var tagInDb = tagRules.GetFirstTagByName(tagItem);
                            if (tagInDb == null)
                            {
                                Tag _tag = new Tag()
                                {
                                    Id = 0,
                                    Name = tagItem,
                                };
                                tagRules.SaveTag(_tag, null, userId);
                                tagInDb = tagRules.GetFirstTagByName(tagItem);
                            }

                            if (articlePost.Tags == null)
                            {
                                articlePost.Tags = new List<Tag>();
                            }
                            articlePost.Tags.Add(tagInDb);
                        }
                    }
                }

                //Start: Processing with file uploaded
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);

                        s3Rules.ProcessingMediaS3(uploadModel.FileKey);
                    }
                }
                else
                {
                    // Processing with default image
                    uploadModel = new S3ObjectUploadModel
                    {
                        FileKey = ""
                    };
                }
                //End: processing with file uploaded

                articlePost.CreatedBy = user;
                articlePost.CreatedDate = DateTime.UtcNow;
                articlePost.ImgUri = uploadModel.FileKey;
                articlePost.Type = HighlightPostType.Article;

                if (articlePost.Id <= 0)
                {
                    articlePost.Status = HighlightPostStatus.Active;
                    dbContext.ArticleHighlightPosts.Add(articlePost);
                    dbContext.Entry(articlePost).State = EntityState.Added;
                    dbContext.SaveChanges();

                    result.result = true;
                    return result;
                }
                else
                {
                    var postInDb = dbContext.ArticleHighlightPosts.FirstOrDefault(p => p.Id == articlePost.Id);
                    if (postInDb != null)
                    {
                        postInDb.Title = articlePost.Title;
                        postInDb.Content = articlePost.Content;
                        postInDb.Tags.Clear();
                        postInDb.Tags = articlePost.Tags;
                        postInDb.ArticleBody = articlePost.ArticleBody;
                        if (!string.IsNullOrEmpty(articlePost.ImgUri))
                        {
                            postInDb.ImgUri = articlePost.ImgUri;
                        }

                        result.actionVal = 2;
                        dbContext.Entry(postInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        result.result = true;
                    }
                    else
                    {
                        result.actionVal = 2;
                        result.msg = "Article Highlight Post does not exist.";

                        result.result = false;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, articlePost, userId, uploadModel);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public ReturnJsonModel AddEditKnowledgeHighlightPost(KnowledgeHighlight knowledgePost, List<string> tags, string countryName, string userId, S3ObjectUploadModel uploadModel)
        {
            var result = new ReturnJsonModel { actionVal = 1 };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, knowledgePost, userId, uploadModel);
                var user = new UserRules(dbContext).GetById(userId);

                if (tags != null && tags.Count > 0)
                {
                    var tagRules = new TagRules(dbContext);
                    foreach (var tagItem in tags)
                    {
                        if (!String.IsNullOrEmpty(tagItem))
                        {
                            var tagInDb = tagRules.GetFirstTagByName(tagItem);
                            if (tagInDb == null)
                            {
                                Tag _tag = new Tag()
                                {
                                    Id = 0,
                                    Name = tagItem,
                                };
                                tagRules.SaveTag(_tag, null, userId);
                                tagInDb = tagRules.GetFirstTagByName(tagItem);
                            }

                            if (knowledgePost.Tags == null)
                            {
                                knowledgePost.Tags = new List<Tag>();
                            }
                            knowledgePost.Tags.Add(tagInDb);
                        }
                    }
                }

                //Start: Processing with file uploaded
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);

                        s3Rules.ProcessingMediaS3(uploadModel.FileKey);
                    }
                }
                else
                {
                    // Processing with default image
                    uploadModel = new S3ObjectUploadModel
                    {
                        FileKey = ""
                    };
                }
                //End: processing with file uploaded

                knowledgePost.CreatedBy = user;
                knowledgePost.CreatedDate = DateTime.UtcNow;
                knowledgePost.ImgUri = uploadModel.FileKey;
                knowledgePost.Type = HighlightPostType.Knowledge;
                if (!string.IsNullOrEmpty(countryName))
                {
                    knowledgePost.Country = new CountriesRules().GetCountryByName(countryName);
                }
                else
                {
                    knowledgePost.Country = new Models.Qbicles.Country();
                }

                if (knowledgePost.Id <= 0)
                {
                    knowledgePost.Status = HighlightPostStatus.Active;
                    dbContext.KnowledgeHighlightPosts.Add(knowledgePost);
                    dbContext.Entry(knowledgePost).State = EntityState.Added;
                    dbContext.SaveChanges();

                    result.result = true;
                    return result;
                }
                else
                {
                    var postInDb = dbContext.KnowledgeHighlightPosts.FirstOrDefault(p => p.Id == knowledgePost.Id);
                    if (postInDb != null)
                    {
                        postInDb.Title = knowledgePost.Title;
                        postInDb.Content = knowledgePost.Content;
                        postInDb.Tags.Clear();
                        postInDb.Tags = knowledgePost.Tags;
                        postInDb.KnowledgeHyperlink = knowledgePost.KnowledgeHyperlink;
                        postInDb.KnowledgeCitation = knowledgePost.KnowledgeCitation;
                        if (knowledgePost.Country != null && !string.IsNullOrEmpty(knowledgePost.Country.CommonName))
                        {
                            postInDb.Country = new CountriesRules().GetCountryByName(knowledgePost.Country.CommonName);
                        }
                        else
                        {
                            postInDb.Country = new Models.Qbicles.Country();
                        }
                        if (!string.IsNullOrEmpty(knowledgePost.ImgUri))
                        {
                            postInDb.ImgUri = knowledgePost.ImgUri;
                        }

                        result.actionVal = 2;
                        dbContext.Entry(postInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        result.result = true;
                    }
                    else
                    {
                        result.actionVal = 2;
                        result.msg = "Knowledge Highlight Post does not exist.";

                        result.result = false;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, knowledgePost, userId, uploadModel);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public ReturnJsonModel AddEditEventHighlightPost(EventListingHighlight eventListingPost, List<string> tags, string countryName, int locationId, string userId, S3ObjectUploadModel uploadModel)
        {
            var result = new ReturnJsonModel { actionVal = 1 };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, eventListingPost, locationId, userId, uploadModel);
                var user = new UserRules(dbContext).GetById(userId);
                if (tags != null && tags.Count > 0)
                {
                    var tagRules = new TagRules(dbContext);
                    foreach (var tagItem in tags)
                    {
                        if (!String.IsNullOrEmpty(tagItem))
                        {
                            var tagInDb = tagRules.GetFirstTagByName(tagItem);
                            if (tagInDb == null)
                            {
                                Tag _tag = new Tag()
                                {
                                    Id = 0,
                                    Name = tagItem,
                                };
                                tagRules.SaveTag(_tag, null, userId);
                                tagInDb = tagRules.GetFirstTagByName(tagItem);
                            }

                            if (eventListingPost.Tags == null)
                            {
                                eventListingPost.Tags = new List<Tag>();
                            }
                            eventListingPost.Tags.Add(tagInDb);
                        }
                    }
                }
                //Start: Processing with file uploaded
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);

                        s3Rules.ProcessingMediaS3(uploadModel.FileKey);
                    }
                }
                else
                {
                    // Processing with default image
                    uploadModel = new S3ObjectUploadModel
                    {
                        FileKey = ""
                    };
                }
                //End: processing with file uploaded

                eventListingPost.CreatedBy = user;
                eventListingPost.CreatedDate = DateTime.UtcNow;
                eventListingPost.ImgUri = uploadModel.FileKey;
                eventListingPost.Type = HighlightPostType.Listings;
                eventListingPost.ListingHighlightType = ListingType.Event;
                if (!string.IsNullOrEmpty(countryName))
                {
                    eventListingPost.Country = new CountriesRules().GetCountryByName(countryName);
                }
                else
                {
                    eventListingPost.Country = new Models.Qbicles.Country();
                }
                eventListingPost.ListingLocation = dbContext.HighlightLocations.Find(locationId);

                if (eventListingPost.Id <= 0)
                {
                    eventListingPost.Status = HighlightPostStatus.Active;
                    dbContext.EventHighlightPosts.Add(eventListingPost);
                    dbContext.Entry(eventListingPost).State = EntityState.Added;
                    dbContext.SaveChanges();

                    result.result = true;
                    return result;
                }
                else
                {
                    var postInDb = dbContext.EventHighlightPosts.FirstOrDefault(p => p.Id == eventListingPost.Id);
                    if (postInDb != null)
                    {
                        postInDb.Title = eventListingPost.Title;
                        postInDb.Tags.Clear();
                        postInDb.Tags = eventListingPost.Tags;
                        postInDb.Reference = eventListingPost.Reference;
                        postInDb.Content = eventListingPost.Content;
                        postInDb.EventLocation = eventListingPost.EventLocation;
                        postInDb.StartDate = eventListingPost.StartDate;
                        postInDb.EndDate = eventListingPost.EndDate;
                        postInDb.ListingLocation = eventListingPost.ListingLocation;
                        if (eventListingPost.Country != null && !string.IsNullOrEmpty(eventListingPost.Country.CommonName))
                        {
                            postInDb.Country = new CountriesRules().GetCountryByName(eventListingPost.Country.CommonName);
                        }
                        else
                        {
                            postInDb.Country = new Models.Qbicles.Country();
                        }

                        if (!string.IsNullOrEmpty(eventListingPost.ImgUri))
                        {
                            postInDb.ImgUri = eventListingPost.ImgUri;
                        }

                        result.actionVal = 2;
                        dbContext.Entry(postInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        result.result = true;
                    }
                    else
                    {
                        result.actionVal = 2;
                        result.msg = "Event Listing Highlight Post does not exist.";

                        result.result = false;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, eventListingPost, locationId, userId, uploadModel);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public ReturnJsonModel AddEditJobHighlightPost(JobListingHighlight jobListingPost, List<string> tags, string countryName, int locationId, string userId, S3ObjectUploadModel uploadModel)
        {
            var result = new ReturnJsonModel { actionVal = 1 };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, jobListingPost, locationId, userId, uploadModel);
                var user = new UserRules(dbContext).GetById(userId);

                if (tags != null && tags.Count > 0)
                {
                    var tagRules = new TagRules(dbContext);
                    foreach (var tagItem in tags)
                    {
                        if (!String.IsNullOrEmpty(tagItem))
                        {
                            var tagInDb = tagRules.GetFirstTagByName(tagItem);
                            if (tagInDb == null)
                            {
                                Tag _tag = new Tag()
                                {
                                    Id = 0,
                                    Name = tagItem,
                                };
                                tagRules.SaveTag(_tag, null, userId);
                                tagInDb = tagRules.GetFirstTagByName(tagItem);
                            }

                            if (jobListingPost.Tags == null)
                            {
                                jobListingPost.Tags = new List<Tag>();
                            }
                            jobListingPost.Tags.Add(tagInDb);
                        }
                    }
                }
                //Start: Processing with file uploaded
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);

                        s3Rules.ProcessingMediaS3(uploadModel.FileKey);
                    }
                }
                else
                {
                    // Processing with default image
                    uploadModel = new S3ObjectUploadModel
                    {
                        FileKey = ""
                    };
                }
                //End: processing with file uploaded

                jobListingPost.CreatedBy = user;
                jobListingPost.CreatedDate = DateTime.UtcNow;
                jobListingPost.ImgUri = uploadModel.FileKey;
                jobListingPost.Type = HighlightPostType.Listings;
                jobListingPost.ListingHighlightType = ListingType.Job;
                if (!string.IsNullOrEmpty(countryName))
                {
                    jobListingPost.Country = new CountriesRules().GetCountryByName(countryName);
                }
                else
                {
                    jobListingPost.Country = new Models.Qbicles.Country();
                }
                jobListingPost.ListingLocation = dbContext.HighlightLocations.Find(locationId);

                if (jobListingPost.Id <= 0)
                {
                    jobListingPost.Status = HighlightPostStatus.Active;
                    dbContext.JobHighlightPosts.Add(jobListingPost);
                    dbContext.Entry(jobListingPost).State = EntityState.Added;
                    dbContext.SaveChanges();

                    result.result = true;
                    return result;
                }
                else
                {
                    var postInDb = dbContext.JobHighlightPosts.FirstOrDefault(p => p.Id == jobListingPost.Id);
                    if (postInDb != null)
                    {
                        postInDb.Title = jobListingPost.Title;
                        postInDb.Tags.Clear();
                        postInDb.Tags = jobListingPost.Tags;
                        postInDb.Reference = jobListingPost.Reference;
                        postInDb.Salary = jobListingPost.Salary;
                        postInDb.ClosingDate = jobListingPost.ClosingDate;
                        postInDb.Content = jobListingPost.Content;
                        postInDb.SkillRequired = jobListingPost.SkillRequired;
                        postInDb.ListingLocation = jobListingPost.ListingLocation;
                        if (jobListingPost.Country != null && !string.IsNullOrEmpty(jobListingPost.Country.CommonName))
                        {
                            postInDb.Country = new CountriesRules().GetCountryByName(jobListingPost.Country.CommonName);
                        }
                        else
                        {
                            postInDb.Country = new Models.Qbicles.Country();
                        }

                        if (!string.IsNullOrEmpty(jobListingPost.ImgUri))
                        {
                            postInDb.ImgUri = jobListingPost.ImgUri;
                        }

                        result.actionVal = 2;
                        dbContext.Entry(postInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        result.result = true;
                    }
                    else
                    {
                        result.actionVal = 2;
                        result.msg = "Job Listing Highlight Post does not exist.";

                        result.result = false;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, jobListingPost, locationId, userId, uploadModel);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public ReturnJsonModel AddEditRealEstateHighlightPost(RealEstateListingHighlight realestateListingPost, List<string> tags, string countryName, int locationId, int propTypeId, List<int> propListids, string userId, List<MediaModel> postAttachments, List<RealEstateImage> existedAttachments, List<RealEstateImage> updatedAttachments)
        {
            var result = new ReturnJsonModel { actionVal = 1 };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, realestateListingPost, userId, postAttachments);
                var user = new UserRules(dbContext).GetById(userId);
                if (tags != null && tags.Count > 0)
                {
                    var tagRules = new TagRules(dbContext);
                    foreach (var tagItem in tags)
                    {
                        if (!String.IsNullOrEmpty(tagItem))
                        {
                            var tagInDb = tagRules.GetFirstTagByName(tagItem);
                            if (tagInDb == null)
                            {
                                Tag _tag = new Tag()
                                {
                                    Id = 0,
                                    Name = tagItem,
                                };
                                tagRules.SaveTag(_tag, null, userId);
                                tagInDb = tagRules.GetFirstTagByName(tagItem);
                            }

                            if (realestateListingPost.Tags == null)
                            {
                                realestateListingPost.Tags = new List<Tag>();
                            }
                            realestateListingPost.Tags.Add(tagInDb);
                        }
                    }
                }
                var orderOfAttachmentUpdated = updatedAttachments?.Select(p => p.Id).ToList() ?? new List<string>();

                //Start: Processing with file uploaded
                realestateListingPost.RealEstateListImgs = new List<RealEstateImage>();
                if (postAttachments == null)
                {
                    postAttachments = new List<MediaModel>();
                }

                var attachOrder = 1;
                foreach (var attachmentItem in postAttachments)
                {
                    if (!string.IsNullOrEmpty(attachmentItem.Id))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);

                        s3Rules.ProcessingMediaS3(attachmentItem.Id);

                        var reImg = new RealEstateImage
                        {
                            Id = attachmentItem.Id,
                            Name = attachmentItem.Name,
                            Order = attachOrder,
                            FileUri = attachmentItem.Id,
                            FileType = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(attachmentItem.Name))
                        };
                        if (!orderOfAttachmentUpdated.Contains(reImg.Id))
                        {
                            attachOrder++;
                            realestateListingPost.RealEstateListImgs.Add(reImg);
                        }
                        else
                        {
                            var currentUpdateAttachment = updatedAttachments.FirstOrDefault(x => x.Id == reImg.Id);
                            reImg.Order = currentUpdateAttachment.Order;
                            currentUpdateAttachment.Id = reImg.Id;
                            currentUpdateAttachment.Name = reImg.Name;
                            currentUpdateAttachment.FileUri = reImg.Id;
                            currentUpdateAttachment.FileType = reImg.FileType;
                        }
                    }
                }

                var uploadModel = new S3ObjectUploadModel();
                if (postAttachments != null && postAttachments.Count > 0)
                {
                    uploadModel = new S3ObjectUploadModel
                    {
                        FileKey = postAttachments.FirstOrDefault().Id,
                        FileName = postAttachments.FirstOrDefault().Name,
                        FileSize = postAttachments.FirstOrDefault().Size
                    };
                }
                else
                {
                    // Processing with default image
                    uploadModel = new S3ObjectUploadModel
                    {
                        FileKey = ""
                    };
                }
                //End: processing with file uploaded

                realestateListingPost.CreatedBy = user;
                realestateListingPost.CreatedDate = DateTime.UtcNow;
                realestateListingPost.ImgUri = uploadModel.FileKey;
                realestateListingPost.Type = HighlightPostType.Listings;
                realestateListingPost.ListingHighlightType = ListingType.RealEstate;
                if (!string.IsNullOrEmpty(countryName))
                {
                    realestateListingPost.Country = new CountriesRules().GetCountryByName(countryName);
                }
                else
                {
                    realestateListingPost.Country = new Models.Qbicles.Country();
                }

                realestateListingPost.ListingLocation = dbContext.HighlightLocations.Find(locationId);
                realestateListingPost.PropType = dbContext.PropertyTypes.Find(propTypeId);
                if (propListids != null)
                {
                    if (realestateListingPost.IncludedProperties == null)
                        realestateListingPost.IncludedProperties = new List<PropertyExtras>();
                    foreach (var propId in propListids)
                    {
                        realestateListingPost.IncludedProperties.Add(dbContext.PropertyExtras.Find(propId));
                    }
                }

                if (realestateListingPost.Id <= 0)
                {
                    realestateListingPost.Status = HighlightPostStatus.Active;
                    dbContext.RealEstateHighlightPosts.Add(realestateListingPost);
                    dbContext.Entry(realestateListingPost).State = EntityState.Added;
                    dbContext.SaveChanges();

                    result.result = true;
                    return result;
                }
                else
                {
                    var postInDb = dbContext.RealEstateHighlightPosts.FirstOrDefault(p => p.Id == realestateListingPost.Id);
                    if (postInDb != null)
                    {
                        postInDb.Title = realestateListingPost.Title;
                        postInDb.Tags.Clear();
                        postInDb.Tags = realestateListingPost.Tags;
                        postInDb.Reference = realestateListingPost.Reference;
                        postInDb.Content = realestateListingPost.Content;
                        postInDb.PropType = realestateListingPost.PropType;
                        postInDb.BedRoomNum = realestateListingPost.BedRoomNum;
                        postInDb.BathRoomNum = realestateListingPost.BathRoomNum;
                        postInDb.PricingInfo = realestateListingPost.PricingInfo;
                        postInDb.IncludedProperties.Clear();
                        postInDb.IncludedProperties = realestateListingPost.IncludedProperties;
                        if (realestateListingPost.Country != null && !string.IsNullOrEmpty(realestateListingPost.Country.CommonName))
                        {
                            postInDb.Country = new CountriesRules().GetCountryByName(realestateListingPost.Country.CommonName);
                        }
                        else
                        {
                            postInDb.Country = new Models.Qbicles.Country();
                        }
                        postInDb.ListingLocation = realestateListingPost.ListingLocation;

                        if (!string.IsNullOrEmpty(realestateListingPost.ImgUri))
                        {
                            postInDb.ImgUri = realestateListingPost.ImgUri;
                        }

                        if (existedAttachments != null && existedAttachments.Count > 0 && postInDb.RealEstateListImgs != null && postInDb.RealEstateListImgs.Count > 0)
                        {
                            foreach (var updateAttachment in existedAttachments)
                            {
                                if (!string.IsNullOrEmpty(updateAttachment.Name))
                                {
                                    var currentAttachment = postInDb.RealEstateListImgs.FirstOrDefault(p => p.Order == updateAttachment.Order);
                                    if (currentAttachment != null)
                                    {
                                        currentAttachment.Name = updateAttachment.Name;
                                    }
                                }
                            }
                        }
                        if (updatedAttachments != null && updatedAttachments.Count > 0 && postInDb.RealEstateListImgs != null && postInDb.RealEstateListImgs.Count > 0)
                        {
                            foreach (var updatedItem in updatedAttachments)
                            {
                                if (!string.IsNullOrEmpty(updatedItem.Id))
                                {
                                    var currentAttachment = postInDb.RealEstateListImgs.FirstOrDefault(p => p.Order == updatedItem.Order);
                                    postInDb.RealEstateListImgs.Remove(currentAttachment);
                                    postInDb.RealEstateListImgs.Add(updatedItem);
                                }
                            }
                        }
                        if (realestateListingPost.RealEstateListImgs != null && realestateListingPost.RealEstateListImgs.Count > 0)
                        {
                            var currentIndex = postInDb.RealEstateListImgs?.Count ?? 0;
                            realestateListingPost.RealEstateListImgs.ForEach(img =>
                            {
                                img.Order += currentIndex;
                            });

                            postInDb.RealEstateListImgs.AddRange(realestateListingPost.RealEstateListImgs);
                        }

                        if (postInDb.RealEstateListImgs != null && postInDb.RealEstateListImgs.Count > 0)
                        {
                            var firstImage = postInDb.RealEstateListImgs.FirstOrDefault(i => i.Order == 1);
                            if (firstImage != null)
                            {
                                postInDb.ImgUri = firstImage.FileUri;
                            }
                        }

                        result.actionVal = 2;
                        dbContext.Entry(postInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        result.result = true;
                    }
                    else
                    {
                        result.actionVal = 2;
                        result.msg = "RealEstate Listing Highlight Post does not exist.";

                        result.result = false;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, realestateListingPost, userId, postAttachments);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public ReturnJsonModel UnpublishHighlightPost(int postId, string userId)
        {
            var result = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, postId, userId);
                var hlPost = dbContext.HighlightPosts.Find(postId);
                if (hlPost == null)
                {
                    result.result = false;
                    result.msg = "Can not find Highlight post";
                    return result;
                }
                else
                {
                    hlPost.Status = HighlightPostStatus.InActive;
                    dbContext.Entry(hlPost).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    result.result = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, postId, userId);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public string GetHighlightStatusLabelByStatus(HighlightPostStatus status)
        {
            string statusString = "";
            switch (status)
            {
                case HighlightPostStatus.Active:
                    statusString = @"<span class='label label-lg label-success'>Live</span>";
                    break;

                case HighlightPostStatus.InActive:
                    statusString = @"<span class='label label-lg label-warning'>Inactive</span>";
                    break;

                default:
                    statusString = "";
                    break;
            }
            return statusString;
        }

        public HighlightPost GetHighlightPost(int highlightId, HighlightPostType type, ListingType _listingType)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, highlightId, type);
                if (type == HighlightPostType.Listings && _listingType != 0)
                {
                    return dbContext.HighlightPosts.FirstOrDefault(p => p.Id == highlightId && p.Type == type && (p as ListingHighlight).ListingHighlightType == _listingType);
                }
                return dbContext.HighlightPosts.FirstOrDefault(p => p.Id == highlightId && p.Type == type);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, highlightId, type);
                return null;
            }
        }

        public List<String> GetListTagsByTypes(HighlightPostType hlType, ListingType lsType = 0, int domainId = 0)
        {
            var listPosts = dbContext.HighlightPosts.Where(p => p.Type == hlType && (domainId == 0 || p.Domain.Id == domainId)).ToList();
            var tags = new List<String>();
            listPosts.ForEach(p =>
            {
                tags.AddRange(p.Tags.Select(t => t.Name).ToList() ?? new List<string>());
            });

            if (hlType == HighlightPostType.Listings && lsType > 0)
            {
                var listListingPosts = dbContext.ListingHighlightPosts.Where(p => p.ListingHighlightType == lsType && (domainId == 0 || p.Domain.Id == domainId)).ToList();
                tags = new List<String>();
                listListingPosts.ForEach(p =>
                {
                    tags.AddRange(p.Tags.Select(t => t.Name).ToList() ?? new List<string>());
                });
            }
            return tags.Distinct().ToList();
        }

        public List<QbicleDomain> GetListRandomDomainToFollow(string userId)
        {
            var listResult = new List<QbicleDomain>();
            var rnd = new Random();
            var lstConnectedBusinessIdWithCurrentUser = dbContext.B2CQbicles
                    .Where(p => p.Customer.Id == userId
                    && !p.RemovedForUsers.Any(x => x.Id == userId)
                    && p.Business.Status != QbicleDomain.DomainStatusEnum.Closed
                    && p.Status != CommsStatus.Blocked).Select(p => p.Business.Id).ToList();
            var lstDomain = dbContext.Domains
                .Where(p => !lstConnectedBusinessIdWithCurrentUser.Any(f => f == p.Id)
                            && dbContext.B2BProfiles.Any(profile => profile.Domain.Id == p.Id)
                            && p.Status != QbicleDomain.DomainStatusEnum.Closed)
                .ToList();
            // var lstDomain = dbContext.B2BProfiles.Select(b => b.Domain).Where(p => !p.HighlightPosterFollowers.Any(f => f.Id == userId));//.ToList();
            var totalDomain = lstDomain.Count();
            if (totalDomain <= 3)
            {
                return lstDomain.ToList();
            }

            var firstIndex = rnd.Next(totalDomain);
            var secondIndex = rnd.Next(totalDomain - 1);
            var thirdIndex = rnd.Next(totalDomain - 2);

            var firstRandomDomain = lstDomain.ElementAt(firstIndex);
            lstDomain.RemoveAt(firstIndex);
            var secondRandomDomain = lstDomain.ElementAt(secondIndex);
            lstDomain.RemoveAt(secondIndex);
            var thirdRandomDomain = lstDomain.ElementAt(thirdIndex);

            listResult.Add(firstRandomDomain);
            listResult.Add(secondRandomDomain);
            listResult.Add(thirdRandomDomain);

            return listResult;
        }

        public List<QbicleDomain> GetListDomainFollowed(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, userId);

                var query = from c in dbContext.CQbicles
                            join b2c in dbContext.B2CQbicles on c.Id equals b2c.Id into dept1
                            from b2c in dept1.DefaultIfEmpty()
                            join b2bprofile in dbContext.B2BProfiles on b2c.Business.Id equals b2bprofile.Domain.Id into dept2
                            from b2bprofile in dept2.DefaultIfEmpty()
                            where !c.IsHidden
                            && b2c.Status != CommsStatus.Blocked
                            && ((b2c != null && b2c.Customer.Id == userId))
                            && (!c.RemovedForUsers.Any(p => p.Id == userId))
                            && (b2c.Business.Status != QbicleDomain.DomainStatusEnum.Closed)
                            select b2c.Business;

                return query.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return new List<QbicleDomain>();
            }
        }

        public List<string> GetListReferenceByType(int domainId, ListingType lsType)
        {
            var lstRef = dbContext.ListingHighlightPosts.Where(p => p.ListingHighlightType == lsType && p.Domain.Id == domainId && p.FlaggedBy.Any()).Select(p => p.Reference).ToList();
            return lstRef;
        }

        /// <summary>
        /// Domain Follow
        /// </summary>
        /// <param name="domainId"></param>
        /// <param name="userId"></param>
        /// <param name="type">1: Follow, 2: Unfollow</param>
        /// <returns></returns>
        public ReturnJsonModel UpdateDomainFollowStatus(int domainId, string userId, int type)
        {
            var result = new ReturnJsonModel { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, userId, type);

                // type == 1 ---> Connect to a Business
                if (type == 1)
                {
                    var b2bProfileId = dbContext.B2BProfiles.FirstOrDefault(p => p.Domain.Id == domainId).Id;
                    result = new C2CRules(dbContext).ConnectC2C(userId, b2bProfileId.ToString(), 1);
                    return result;
                }
                // type == 2 ---> Block a business
                else if (type == 2)
                {
                    var b2cQbicleId = dbContext.B2CQbicles.FirstOrDefault(p => p.Customer.Id == userId && p.Business.Id == domainId);
                    return new B2CRules(dbContext).SetStatusByCustomer(b2cQbicleId.Id, userId, CommsStatus.Blocked);
                }
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, userId, type);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        /// <summary>
        /// Interested - Flag Highlight
        /// </summary>
        /// <param name="highlightId"></param>
        /// <param name="userId"></param>
        /// <param name="type">1: Flag, 2: UnFlag</param>
        /// <returns></returns>
        public ReturnJsonModel UpdateHLFlagStatus(int highlightId, string userId, int type, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, highlightId, userId, type);
                var hlPost = dbContext.ListingHighlightPosts.FirstOrDefault(p => p.Id == highlightId);
                var userInDb = dbContext.QbicleUser.FirstOrDefault(p => p.Id == userId);
                if (hlPost != null && userInDb != null)
                {
                    if (type == 1 && !hlPost.FlaggedBy.Any(p => p.Id == userId) && !userInDb.FlaggedListings.Any(p => p.Id == highlightId))
                    {
                        hlPost.FlaggedBy.Add(userInDb);
                        userInDb.FlaggedListings.Add(hlPost);
                    }
                    else if (type == 2)
                    {
                        hlPost.FlaggedBy.Remove(userInDb);
                        userInDb.FlaggedListings.Remove(hlPost);
                    }
                }
                dbContext.Entry(userInDb).State = EntityState.Modified;
                dbContext.Entry(hlPost).State = EntityState.Modified;
                dbContext.SaveChanges();

                //Send notification
                if (type == 1)
                {
                    var nRule = new NotificationRules(dbContext);
                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = hlPost.Id,
                        PostId = hlPost.Id,
                        EventNotify = NotificationEventEnum.ListingInterested,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        ReminderMinutes = 0,
                        CreatedById = userId,
                        CreatedByName = HelperClass.GetFullName(userInDb),
                        DomainId = hlPost.Domain.Id
                    };
                    nRule.NotifyOnFlagListing(activityNotification);
                }

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, highlightId, userId, type);
                result.result = false;
                result.msg = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Like status Highlight
        /// </summary>
        /// <param name="highlightId"></param>
        /// <param name="userId"></param>
        /// <param name="type">type: 1: Like, 2: Unlike</param>
        /// <returns></returns>
        public ReturnJsonModel UpdateHLLikeStatus(int highlightId, string userId, int type)
        {
            var result = new ReturnJsonModel { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, highlightId, userId, type);
                var userInDb = dbContext.QbicleUser.FirstOrDefault(p => p.Id == userId);
                var HLPst = dbContext.HighlightPosts.FirstOrDefault(p => p.Id == highlightId);
                if (userInDb != null && HLPst != null)
                {
                    if (type == 1 && !HLPst.LikedBy.Any(p => p.Id == userId) && !userInDb.LikedHighlightPosts.Any(p => p.Id == highlightId))
                    {
                        userInDb.LikedHighlightPosts.Add(HLPst);
                        HLPst.LikedBy.Add(userInDb);
                    }
                    else if (type == 2)
                    {
                        userInDb.LikedHighlightPosts.Remove(HLPst);
                        HLPst.LikedBy.Remove(userInDb);
                    }
                }
                dbContext.Entry(userInDb).State = EntityState.Modified;
                dbContext.Entry(HLPst).State = EntityState.Modified;
                dbContext.SaveChanges();

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, highlightId, userId, type);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        /// <summary>
        /// Bookmark Highlight
        /// </summary>
        /// <param name="highlightId"></param>
        /// <param name="userId"></param>
        /// <param name="type">1:Bookmark, 2:Remove bookmark</param>
        /// <returns></returns>
        public ReturnJsonModel UpdateHLBookmarkStatus(int highlightId, string userId, int type)
        {
            var result = new ReturnJsonModel { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, highlightId, userId, type);
                var userInDb = dbContext.QbicleUser.FirstOrDefault(p => p.Id == userId);
                var HLPst = dbContext.HighlightPosts.FirstOrDefault(p => p.Id == highlightId);
                if (userInDb != null && HLPst != null)
                {
                    if (type == 1 && !HLPst.BookmarkedBy.Any(p => p.Id == userId) && !userInDb.BookmarkedHighlightPosts.Any(p => p.Id == highlightId))
                    {
                        userInDb.BookmarkedHighlightPosts.Add(HLPst);
                        HLPst.BookmarkedBy.Add(userInDb);
                    }
                    else if (type == 2)
                    {
                        userInDb.BookmarkedHighlightPosts.Remove(HLPst);
                        HLPst.BookmarkedBy.Remove(userInDb);
                    }
                }
                dbContext.Entry(userInDb).State = EntityState.Modified;
                dbContext.Entry(HLPst).State = EntityState.Modified;
                dbContext.SaveChanges();

                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, highlightId, userId, type);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public List<PropertyType> GetAllPropertyType()
        {
            return dbContext.PropertyTypes.ToList();
        }

        public List<PropertyExtras> GetAllProperties()
        {
            return dbContext.PropertyExtras.ToList();
        }

        public ReturnJsonModel RemoveRealEstateImg(string imgKey, int postId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, imgKey, postId);

                var imgCount = 0;
                var rePost = dbContext.RealEstateHighlightPosts.Find(postId);
                if (rePost != null && rePost.RealEstateListImgs != null)
                {
                    var imgToRemove = rePost.RealEstateListImgs.FirstOrDefault(p => p.Id == imgKey);
                    if (imgToRemove != null)
                    {
                        rePost.RealEstateListImgs.Remove(imgToRemove);

                        //Rearrange and get new default img for post
                        rePost.RealEstateListImgs.OrderBy(p => p.Order);
                        int index = 1;
                        foreach (var imgItem in rePost.RealEstateListImgs)
                        {
                            imgItem.Order = index;
                            index++;
                        }
                        rePost.ImgUri = rePost.RealEstateListImgs?.FirstOrDefault(p => p.Order == 1)?.Id ?? "";
                        dbContext.Entry(imgToRemove).State = EntityState.Modified;
                        imgCount = rePost.RealEstateListImgs?.Count ?? 0;
                    }
                    dbContext.SaveChanges();
                }
                return new ReturnJsonModel() { actionVal = 2, result = true, Object = imgCount };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, imgKey, postId);
                return new ReturnJsonModel() { actionVal = 2, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public EventListingHighlight GetEventListingHighlight(int highlightId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, highlightId);

                return dbContext.EventHighlightPosts.Find(highlightId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, highlightId);
                return null;
            }
        }

        public JobListingHighlight GetJobListingHighlight(int highlightId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, highlightId);

                return dbContext.JobHighlightPosts.Find(highlightId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, highlightId);
                return null;
            }
        }

        public RealEstateListingHighlight GetRealEstateListingHighlight(int highlightId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, highlightId);

                return dbContext.RealEstateHighlightPosts.Find(highlightId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, highlightId);
                return null;
            }
        }

        #region HL posts Sharing

        public List<ApplicationUser> GetContactsForHLShare(string currentUserId)
        {
            var c2cQbicleQuery = dbContext.C2CQbicles.
                Where(s => !s.IsHidden && s.Customers.Any(u => u.Id == currentUserId)).
                SelectMany(p => p.Customers);//.ToList();
            return c2cQbicleQuery
                .Where(x => x.Id != currentUserId)
                .Distinct()
                .OrderBy(p => p.DisplayUserName).ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type">1-existed contact, 2-email</param>
        /// <param name="email"></param>
        /// <param name="postId"></param>
        /// <param name="sharedWithIds"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public async Task<ReturnJsonModel> ShareHLPost(int type, string email, int postId, string sharedWithIds, string currentUserId)
        {
            var result = new ReturnJsonModel() { actionVal = 1, result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, type, email, postId, sharedWithIds, currentUserId);
                var currentUser = await dbContext.QbicleUser.FindAsync(currentUserId);
                var hlPost = await dbContext.HighlightPosts.FirstOrDefaultAsync(p => p.Id == postId);
                if (hlPost == null)
                {
                    result.result = false;
                    result.msg = "Can not find Highlight post";
                    return result;
                }

                if (type == 1)
                {
                    var lstShareUserIds = JsonHelper.ParseAs<List<string>>(sharedWithIds);

                    var c2CQbicles = dbContext.C2CQbicles.Where(p => !p.IsHidden).ToList();
                    foreach (var userId in lstShareUserIds)
                    {
                        var qbicle = c2CQbicles.FirstOrDefault(p => p.Customers.Any(x => x.Id == userId) && p.Customers.Any(x => x.Id == currentUserId));
                        if (qbicle == null)
                            continue;

                        qbicle.LastUpdated = DateTime.UtcNow;

                        var sharedWithUser = await dbContext.QbicleUser.FindAsync(userId);
                        var shareObj = new HLSharedPost()
                        {
                            Topic = new TopicRules(dbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicle.Id),
                            SharedBy = currentUser,
                            SharedWith = sharedWithUser,
                            SharedPost = hlPost,
                            ActivityType = QbicleActivity.ActivityTypeEnum.SharedHLPost,
                            Qbicle = qbicle,
                            TimeLineDate = DateTime.UtcNow,
                            ShareDate = DateTime.UtcNow,
                            StartedBy = currentUser,
                            StartedDate = DateTime.UtcNow,
                            SharedWithEmail = sharedWithUser?.Email ?? ""
                        };
                        dbContext.HLSharedPosts.Add(shareObj);
                        dbContext.Entry(shareObj).State = EntityState.Added;
                        dbContext.SaveChanges();
                    }
                }
                else if (type == 2)
                {
                    var existedUserWithEmail = dbContext.QbicleUser.Where(p => p.Email == email);
                    if (existedUserWithEmail.Count() > 0)
                    {
                        result.result = false;
                        result.msg = "This email address is already associated with a Qbicles user. Please " +
                            "connect with the user in Community before sharing and interacting with them.";
                        return result;
                    }

                    //Send email
                    var shareObj = new HLSharedPost()
                    {
                        SharedBy = currentUser,
                        SharedWithEmail = email,
                        ActivityType = QbicleActivity.ActivityTypeEnum.SharedHLPost,
                        TimeLineDate = DateTime.UtcNow,
                        ShareDate = DateTime.UtcNow,
                        StartedDate = DateTime.UtcNow,
                        StartedBy = currentUser,
                        SharedPost = hlPost
                    };

                    await (new EmailRules(dbContext).SendEmailHLPostSharingAsync(shareObj));
                }
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, type, email, postId, sharedWithIds, currentUserId);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        #endregion HL posts Sharing

        /// <summary>
        ///
        /// </summary>
        /// <param name="domainId"></param>
        /// <param name="userId"></param>
        /// <param name="type">1 - Hide, 2 - Unhide</param>
        /// <returns></returns>
        public ReturnJsonModel ChangeHiddingHLDomainPostStatus(int domainId, string userId, int type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, userId);

                var domain = dbContext.Domains.FirstOrDefault(p => p.Id == domainId);
                var currentUser = dbContext.QbicleUser.Find(userId);

                if (type == 1)
                {
                    domain.HighlightPosterHiddenUser.Add(currentUser);
                    currentUser.HighlightDomainHidden.Add(domain);
                }
                else if (type == 2)
                {
                    domain.HighlightPosterHiddenUser.Remove(currentUser);
                    currentUser.HighlightDomainHidden.Remove(domain);
                }

                dbContext.Entry(domain).State = EntityState.Modified;
                dbContext.Entry(currentUser).State = EntityState.Modified;

                dbContext.SaveChanges();
                return new ReturnJsonModel()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, userId);
                return new ReturnJsonModel()
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_5")
                };
            }
        }
    }
}