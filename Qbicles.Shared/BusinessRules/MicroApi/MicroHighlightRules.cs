using Qbicles.BusinessRules.BusinessRules.Social;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.Models;
using Qbicles.Models.Highlight;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroHighlightRules : MicroRulesBase
    {
        public MicroHighlightRules(MicroContext microContext) : base(microContext)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="highlight"></param>
        /// <param name="isFilter">false- get and return filter object</param>
        /// <returns></returns>
        public ReturnJsonModel GetHighlight(HighlightParameter highlight)
        {
            try
            {
                var rules = new HighlightPostRules(dbContext);

                var userSetting = new UserSetting
                {
                    DateFormat = CurrentUser.DateFormat,
                    TimeFormat = CurrentUser.TimeFormat,
                    Id = CurrentUser.Id,
                    DisplayName = CurrentUser.DisplayUserName,
                    UserName = CurrentUser.UserName,
                    Timezone = CurrentUser.Timezone,
                    Email = CurrentUser.Email,
                    ProfilePic = CurrentUser.ProfilePic
                };

                var highlights = rules.GetListHlPostCustomPagination(
                    userSetting, highlight.DomainId, highlight.Key, highlight.TypeShowed, highlight.TypeSearch, highlight.Tags, highlight.IsBookmarked,
                    highlight.IsFlagged, highlight.PageIndex, highlight.CountryName, highlight.EventDateRange, highlight.NewsPublishedDate, highlight.PropertyFacilities,
                    highlight.BedroomNumber, highlight.BathroomNumber, highlight.PropertyTypes, highlight.AreaId);

                return new ReturnJsonModel { result = true, Object = highlights };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, highlight);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        public MicroHighlightFilterOption GetFilterOption()
        {
            return new MicroHighlightFilterOption
            {
                Tags2Filter = new HighlightPostRules(dbContext).GetHighlightPostTagName().ToList(),
                Categories2Filter = new List<HighlightCategoryFilter>
                 {
                    new HighlightCategoryFilter { Name = "Event", TypeShowed = HighlightPostType.Listings, TypeSearch= ListingType.Event },
                    new HighlightCategoryFilter { Name = "Job", TypeShowed = HighlightPostType.Listings, TypeSearch= ListingType.Job },
                    new HighlightCategoryFilter { Name = "Knowledge", TypeShowed = HighlightPostType.Knowledge, TypeSearch= ListingType.None },
                    new HighlightCategoryFilter { Name = "News", TypeShowed = HighlightPostType.News, TypeSearch= ListingType.None },
                    new HighlightCategoryFilter { Name = "Real estate", TypeShowed = HighlightPostType.Listings, TypeSearch= ListingType.RealEstate },
                },
                BathRoom2Filter = new List<BaseModel>
                {
                    new BaseModel{Id=0,Name="Any"},
                    new BaseModel{Id=1,Name="1"},
                    new BaseModel{Id=2,Name="2"},
                    new BaseModel{Id=3,Name="3+"}
                },
                BedRoom2Filter = new List<BaseModel>
                {
                    new BaseModel {Id=0,Name="Any"},
                    new BaseModel {Id=1,Name="1"},
                    new BaseModel {Id=2,Name="2"},
                    new BaseModel{Id=3,Name="3"},
                    new BaseModel{Id=4,Name="4"},
                    new BaseModel{Id=5,Name="5+"}
                },
                PropertyType2Filter = dbContext.PropertyTypes.Select(e => new BaseModel { Id = e.Id, Name = e.Name }).ToList(),
                PropertyFacilities2Filter = dbContext.PropertyExtras.Select(e => new BaseModel { Id = e.Id, Name = e.Name }).ToList()
            };
        }

        public List<HighlightDomainFollows> GetDomainFollowings()
        {
            var domains = new HighlightPostRules(dbContext).GetListDomainFollowed(CurrentUser.Id);
            return MapHighlightDomainFollows(domains);
        }

        public List<HighlightDomainFollows> GetDomainRecommends()
        {
            var domains = new HighlightPostRules(dbContext).GetListRandomDomainToFollow(CurrentUser.Id);
            return MapHighlightDomainFollows(domains);
        }

        public List<HighlightDomainFollows> MapHighlightDomainFollows(List<QbicleDomain> domains)
        {
            var highlightDomainFollows = new List<HighlightDomainFollows>();
            domains.ForEach(d =>
            {
                highlightDomainFollows.Add(new HighlightDomainFollows
                {
                    DomainId = d.Key,
                    Name = d.Name,
                    ImageUri = new Uri(d.LogoUri.ToDocumentUri().ToString()),
                    Tags = d.Id.BusinesProfile()?.Tags.Select(e => e.TagName).ToList() ?? new List<string>()
                });
            });
            return highlightDomainFollows;
        }

        /// <summary>
        /// Interested - Flag Highlight
        /// </summary>
        /// <param name="highlightId"></param>
        /// <param name="userId"></param>
        /// <param name="type">1: Flag, 2: UnFlag</param>
        /// <returns></returns>
        public ReturnJsonModel MicroInterestedHighlight(int id, int type)
        {
            return new HighlightPostRules(dbContext).UpdateHLFlagStatus(id, CurrentUser.Id, type);
        }

        /// <summary>
        /// Bookmark highlight
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">1:Bookmark, 2:Remove bookmark</param>
        /// <returns></returns>
        public ReturnJsonModel MicroBookmarkHighlight(int id, int type)
        {
            return new HighlightPostRules(dbContext).UpdateHLBookmarkStatus(id, CurrentUser.Id, type);
        }

        /// <summary>
        /// Like Highlight post
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">1 - like, 2 - unlike</param>
        /// <returns></returns>
        public ReturnJsonModel MicroLikeHighlight(int id, int type)
        {
            return new HighlightPostRules(dbContext).UpdateHLLikeStatus(id, CurrentUser.Id, type);
        }

        /// <summary>
        /// Domain Follow Status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">1 - like, 2 - unlike</param>
        /// <returns></returns>
        public ReturnJsonModel MicroDomainFollowStatusHighlight(int domainId, int type)
        {
            return new HighlightPostRules(dbContext).UpdateDomainFollowStatus(domainId, CurrentUser.Id, type);
        }

        public ReturnJsonModel MicroHighlightPropertyInfo(int id)
        {
            try
            {
                var property = dbContext.RealEstateHighlightPosts.Find(id);

                var microPropertyInfo = new HighlightPropertyInfo
                {
                    Id = property.Id,
                    Title = property.Title,
                    Content = property.Content,
                    Bathrooms = property.BathRoomNum,
                    Bedrooms = property.BedRoomNum,
                    Location = property.Country?.CommonName ?? "Available everywhere",
                    Price = property?.PricingInfo,
                    PropertyType = property.PropType?.Name,
                    IncludedProperties = property.IncludedProperties.Select(i => new BaseModel { Id = i.Id, Name = i.Name }).ToList(),
                    Images = property.RealEstateListImgs.OrderBy(p => p.Order).Select(p => new BaseModelImage { Name = p.Name, ImageUri = p.FileUri.ToUri() }).ToList()
                };

                if (microPropertyInfo.Images == null || microPropertyInfo.Images.Count <= 0)
                    microPropertyInfo.Images.Add(new BaseModelImage { Id = 0, Name = "", ImageUri = ConfigManager.HighlightBannerListing.ToUri() });

                return new ReturnJsonModel { result = true, Object = microPropertyInfo };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        public ReturnJsonModel MicroHighlightReadArticle(int id)
        {
            try
            {
                var property = dbContext.ArticleHighlightPosts.Find(id);

                var microPropertyInfo = new HighlightArticle
                {
                    Id = property.Id,
                    Title = property.Title,
                    Content = property.ArticleBody,
                    Tags = property.Tags.Select(t => t.Name).ToList(),
                    CreatedDate = property.CreatedDate.ConvertTimeFromUtc(CurrentUser.Timezone).ToString(CurrentUser.DateFormat + " " + CurrentUser.TimeFormat),
                    ImageUri = string.IsNullOrEmpty(property.ImgUri) ? ConfigManager.HighlightBannerNew.ToUri() : property.ImgUri.ToUri()
                };
                return new ReturnJsonModel { result = true, Object = microPropertyInfo };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ex.Message
                };
            }
        }

        public List<MicroContact> GetContactsForHLShare()
        {
            var c2cContactUsers = new HighlightPostRules(dbContext).GetContactsForHLShare(CurrentUser.Id);
            return c2cContactUsers.ToMicroUser();
        }

        public async Task<ReturnJsonModel> ShareHL2Contacts(HighlightPromotionShareParameter share)
        {
            return await (new HighlightPostRules(dbContext).ShareHLPost(1, "", share.Id, share.ContactIds.ToJson(), CurrentUser.Id));
        }

        public async Task<ReturnJsonModel> ShareHL2Email(HighlightPromotionShareParameter share)
        {
            return await (new HighlightPostRules(dbContext).ShareHLPost(2, share.Email, share.Id, "", CurrentUser.Id));
        }
    }
}