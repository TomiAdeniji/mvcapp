using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.ProfilePage;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.ProfilePages
{
    public class BusinessPageRules
    {
        ApplicationDbContext dbContext;

        public BusinessPageRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public BusinessPage GetBusinessPageById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);
                return dbContext.BusinessPages.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public List<BusinessPage> GetBusinessPages(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId);
                return dbContext.BusinessPages.Where(s => s.Domain.Id == domainId && s.Status == BusinessPageStatusEnum.IsActive)
                    .OrderBy(s => s.DisplayOrder).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }
        }
        public BusinessProfilePageModel GetBusinessPageForJsonById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);
                var page = dbContext.BusinessPages.Find(id);
                BusinessProfilePageModel model = new BusinessProfilePageModel();
                model.Id = page.Id;
                model.PageTitle = page.PageTitle;
                model.Status = page.Status;
                var blHeros = page.Blocks.Where(s => s.Type == BusinessPageBlockType.Hero).ToList();
                if (blHeros.Any())
                    model.BlockHeroes = blHeros.Cast<Hero>().ToList();
                var blFeatures = page.Blocks.Where(s => s.Type == BusinessPageBlockType.FeatureList).ToList();
                if (blFeatures.Any())
                    model.BlockFeatures = blFeatures.Cast<FeatureList>().ToList();
                var blGalleries = page.Blocks.Where(s => s.Type == BusinessPageBlockType.Gallery).ToList();
                if (blGalleries.Any())
                    model.BlockGalleries = blGalleries.Cast<GalleryList>().ToList();
                var blPromotions = page.Blocks.Where(s => s.Type == BusinessPageBlockType.Promotion).ToList();
                if (blPromotions.Any())
                    model.BlockPromotions = blPromotions.Cast<Promotion>().ToList();
                var blTestimonials = page.Blocks.Where(s => s.Type == BusinessPageBlockType.Testimonial).ToList();
                if (blTestimonials.Any())
                    model.BlockTestimonials = blTestimonials.Cast<TestimonialList>().ToList();
                var blTextImages = page.Blocks.Where(s => s.Type == BusinessPageBlockType.TextImage).ToList();
                if (blTextImages.Any())
                    model.BlockTextImages = blTextImages.Cast<TextImage>().ToList();
                return model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new BusinessProfilePageModel();
            }
        }
        public ReturnJsonModel SaveBusinessPageBuilder(BusinessProfilePageModel pageBuilder)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, pageBuilder);

            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var businessPage = dbContext.BusinessPages.Find(pageBuilder.Id);
                    if (businessPage == null)
                    {
                        var currentDisplayOrder = dbContext.BusinessPages.Where(s => s.Domain.Id == pageBuilder.Domain.Id).OrderByDescending(s => s.DisplayOrder).FirstOrDefault()?.DisplayOrder ?? 1;
                        businessPage = new BusinessPage();
                        businessPage.CreateBy = pageBuilder.CreateBy;
                        businessPage.CreateDate = DateTime.UtcNow;
                        businessPage.PageTitle = pageBuilder.PageTitle;
                        businessPage.Status = pageBuilder.Status;
                        businessPage.Domain = pageBuilder.Domain;
                        businessPage.LastUpdatedBy = pageBuilder.CreateBy;
                        businessPage.LastUpdatedDate = businessPage.CreateDate;
                        businessPage.DisplayOrder = currentDisplayOrder + 1;
                        #region add Blocks into BusinessPage
                        if (pageBuilder.BlockHeroes != null)
                            foreach (var item in pageBuilder.BlockHeroes)
                            {
                                if (item != null)
                                {
                                    item.ParentPage = businessPage;
                                    dbContext.BlockHeroTemplates.Add(item);
                                    businessPage.Blocks.Add(item);
                                }
                            }
                        if (pageBuilder.BlockFeatures != null)
                            foreach (var item in pageBuilder.BlockFeatures)
                            {
                                if (item != null)
                                {
                                    item.ParentPage = businessPage;
                                    dbContext.BlockFeatureTemplates.Add(item);
                                    businessPage.Blocks.Add(item);
                                }
                            }
                        if (pageBuilder.BlockGalleries != null)
                            foreach (var item in pageBuilder.BlockGalleries)
                            {
                                if (item != null)
                                {
                                    item.ParentPage = businessPage;
                                    dbContext.BlockGalleryTemplates.Add(item);
                                    businessPage.Blocks.Add(item);
                                }
                            }
                        if (pageBuilder.BlockPromotions != null)
                            foreach (var item in pageBuilder.BlockPromotions)
                            {
                                if (item != null)
                                {
                                    item.ParentPage = businessPage;
                                    dbContext.BlockPromotionTemplates.Add(item);
                                    businessPage.Blocks.Add(item);
                                }
                            }
                        if (pageBuilder.BlockTestimonials != null)
                            foreach (var item in pageBuilder.BlockTestimonials)
                            {
                                if (item != null)
                                {
                                    item.ParentPage = businessPage;
                                    dbContext.BlockTestimonialTemplates.Add(item);
                                    businessPage.Blocks.Add(item);
                                }
                            }
                        if (pageBuilder.BlockTextImages != null)
                            foreach (var item in pageBuilder.BlockTextImages)
                            {
                                if (item != null)
                                {
                                    item.ParentPage = businessPage;
                                    dbContext.BlockTextImageTemplates.Add(item);
                                    businessPage.Blocks.Add(item);
                                }
                            }
                        #endregion
                        dbContext.BusinessPages.Add(businessPage);
                    }
                    else
                    {
                        businessPage.LastUpdatedBy = pageBuilder.CreateBy;
                        businessPage.LastUpdatedDate = DateTime.UtcNow;
                        businessPage.PageTitle = pageBuilder.PageTitle;
                        businessPage.Status = pageBuilder.Status;
                        //dbContext.BlockTemplates.RemoveRange(businessPage.Blocks);
                        #region add Blocks into BusinessPage
                        var itemsBlock = new List<Block>();
                        if (pageBuilder.BlockHeroes != null)
                            foreach (var item in pageBuilder.BlockHeroes)
                            {
                                if (item != null)
                                {
                                    var hero = dbContext.BlockHeroTemplates.Find(item.Id);
                                    if (hero != null)
                                    {
                                        hero.HeroBackgroundColour = item.HeroBackgroundColour;
                                        hero.HeroBackGroundImage = item.HeroBackGroundImage;
                                        hero.HeroBackgroundType = item.HeroBackgroundType;
                                        hero.HeroFeaturedImage = item.HeroFeaturedImage;
                                        hero.HeroGradientColour1 = item.HeroGradientColour1;
                                        hero.HeroGradientColour2 = item.HeroGradientColour2;
                                        hero.HeroHeadingAccentColour = item.HeroHeadingAccentColour;
                                        hero.HeroHeadingColour = item.HeroHeadingColour;
                                        hero.HeroHeadingFontSize = item.HeroHeadingFontSize;
                                        hero.HeroHeadingText = item.HeroHeadingText;
                                        hero.HeroLogo = item.HeroLogo;
                                        hero.HeroSubHeadingColour = item.HeroSubHeadingColour;
                                        hero.HeroSubHeadingFontSize = item.HeroSubHeadingFontSize;
                                        hero.HeroSubHeadingText = item.HeroSubHeadingText;
                                        hero.DisplayOrder = item.DisplayOrder;
                                    }
                                    else
                                    {
                                        hero = item;
                                        hero.ParentPage = businessPage;
                                        dbContext.BlockHeroTemplates.Add(hero);
                                        businessPage.Blocks.Add(hero);
                                    }
                                    itemsBlock.Add(hero);
                                }
                            }
                        dbContext.SaveChanges();
                        if (pageBuilder.BlockFeatures != null)
                            foreach (var item in pageBuilder.BlockFeatures)
                            {
                                if (item != null)
                                {
                                    var feature = dbContext.BlockFeatureTemplates.Find(item.Id);
                                    if (feature != null)
                                    {
                                        feature.DisplayOrder = item.DisplayOrder;
                                        feature.HeadingColour = item.HeadingColour;
                                        feature.HeadingFontSize = item.HeadingFontSize;
                                        feature.HeadingText = item.HeadingText;
                                        feature.SubHeadingColour = item.SubHeadingColour;
                                        feature.SubHeadingFontSize = item.SubHeadingFontSize;
                                        feature.SubHeadingText = item.SubHeadingText;
                                        dbContext.BlockFeatureItems.RemoveRange(feature.FeatureItems);
                                        feature.FeatureItems = item.FeatureItems;
                                    }
                                    else
                                    {
                                        feature = item;
                                        feature.ParentPage = businessPage;
                                        dbContext.BlockFeatureTemplates.Add(feature);
                                        businessPage.Blocks.Add(feature);
                                    }
                                    itemsBlock.Add(feature);
                                }
                            }
                        dbContext.SaveChanges();
                        if (pageBuilder.BlockGalleries != null)
                            foreach (var item in pageBuilder.BlockGalleries)
                            {
                                if (item != null)
                                {
                                    var gallary = dbContext.BlockGalleryTemplates.Find(item.Id);
                                    if (gallary != null)
                                    {
                                        gallary.DisplayOrder = item.DisplayOrder;
                                        dbContext.BlockGalleryItems.RemoveRange(gallary.GalleryItems);
                                        gallary.GalleryItems = item.GalleryItems;
                                    }
                                    else
                                    {
                                        gallary = item;
                                        gallary.ParentPage = businessPage;
                                        dbContext.BlockGalleryTemplates.Add(gallary);
                                        businessPage.Blocks.Add(gallary);
                                    }
                                    itemsBlock.Add(gallary);
                                }
                            }
                        dbContext.SaveChanges();
                        if (pageBuilder.BlockPromotions != null)
                            foreach (var item in pageBuilder.BlockPromotions)
                            {
                                if (item != null)
                                {
                                    var promo = dbContext.BlockPromotionTemplates.Find(item.Id);
                                    if (promo != null)
                                    {
                                        promo.DisplayOrder = item.DisplayOrder;
                                        promo.ButtonColour = item.ButtonColour;
                                        promo.ButtonJumpTo = item.ButtonJumpTo;
                                        promo.ButtonText = item.ButtonText;
                                        promo.ExternalLink = item.ExternalLink;
                                        promo.FeaturedImage = item.FeaturedImage;
                                        promo.HeadingAccentColour = item.HeadingAccentColour;
                                        promo.HeadingColour = item.HeadingColour;
                                        promo.HeadingFontSize = item.HeadingFontSize;
                                        promo.HeadingText = item.HeadingText;
                                        promo.IsIncludeButton = item.IsIncludeButton;
                                        promo.SubHeadingColour = item.SubHeadingColour;
                                        promo.SubHeadingFontSize = item.SubHeadingFontSize;
                                        promo.SubHeadingText = item.SubHeadingText;
                                        dbContext.BlockPromotionItems.RemoveRange(promo.Items);
                                        promo.Items = item.Items;
                                    }
                                    else
                                    {
                                        promo = item;
                                        promo.ParentPage = businessPage;
                                        dbContext.BlockPromotionTemplates.Add(promo);
                                        businessPage.Blocks.Add(promo);
                                    }
                                    itemsBlock.Add(promo);
                                }
                            }
                        dbContext.SaveChanges();
                        if (pageBuilder.BlockTestimonials != null)
                            foreach (var item in pageBuilder.BlockTestimonials)
                            {
                                if (item != null)
                                {
                                    var testimonial = dbContext.BlockTestimonialTemplates.Find(item.Id);
                                    if (testimonial != null)
                                    {
                                        testimonial.DisplayOrder = item.DisplayOrder;
                                        dbContext.BlockTestimonialItems.RemoveRange(testimonial.TestimonialItems);
                                        testimonial.TestimonialItems = item.TestimonialItems;
                                    }
                                    else
                                    {
                                        testimonial = item;
                                        testimonial.ParentPage = businessPage;
                                        dbContext.BlockTestimonialTemplates.Add(testimonial);
                                        businessPage.Blocks.Add(testimonial);
                                    }
                                    itemsBlock.Add(testimonial);
                                }
                            }
                        dbContext.SaveChanges();
                        if (pageBuilder.BlockTextImages != null)
                            foreach (var item in pageBuilder.BlockTextImages)
                            {
                                if (item != null)
                                {
                                    var textImage = dbContext.BlockTextImageTemplates.Find(item.Id);
                                    if (textImage != null)
                                    {
                                        textImage.DisplayOrder = item.DisplayOrder;
                                        textImage.ButtonColour = item.ButtonColour;
                                        textImage.ButtonJumpTo = item.ButtonJumpTo;
                                        textImage.ButtonText = item.ButtonText;
                                        textImage.Content = item.Content;
                                        textImage.ContentColour = item.ContentColour;
                                        textImage.ContentFontSize = item.ContentFontSize;
                                        textImage.ExternalLink = item.ExternalLink;
                                        textImage.FeaturedImage = item.FeaturedImage;
                                        textImage.HeadingAccentColour = item.HeadingAccentColour;
                                        textImage.HeadingColour = item.HeadingColour;
                                        textImage.HeadingFontSize = item.HeadingFontSize;
                                        textImage.HeadingText = item.HeadingText;
                                        textImage.IsIncludeButton = item.IsIncludeButton;
                                    }
                                    else
                                    {
                                        textImage = item;
                                        textImage.ParentPage = businessPage;
                                        dbContext.BlockTextImageTemplates.Add(textImage);
                                        businessPage.Blocks.Add(textImage);
                                    }
                                    itemsBlock.Add(textImage);
                                }
                            }
                        dbContext.SaveChanges();
                        #endregion
                        #region removes blocks
                        var blocks = businessPage.Blocks.Where(s => !itemsBlock.Any(i => i.Id == s.Id)).ToList();
                        dbContext.BlockTemplates.RemoveRange(blocks);
                        #endregion
                        dbContext.Entry(businessPage).State = EntityState.Modified;
                    }
                    returnJson.result = true;
                    #region Use map for Button jumps to
                    if (pageBuilder.BlockTextImages != null)
                        foreach (var item in pageBuilder.BlockTextImages.Where(s => s.ButtonJumpTo != "external"))
                        {
                            if (!businessPage.Blocks.Any(s => ("block" + s.Id) == item.ButtonJumpTo?.Replace("#", "")))
                            {
                                var blockJumpto = businessPage.Blocks.FirstOrDefault(s => s.ElmId == item.ButtonJumpTo);
                                if (blockJumpto != null)
                                    item.ButtonJumpTo = $"#block{blockJumpto.Id}";
                            }
                        }
                    if (pageBuilder.BlockPromotions != null)
                        foreach (var item in pageBuilder.BlockPromotions.Where(s => s.ButtonJumpTo != "external"))
                        {
                            if (!businessPage.Blocks.Any(s => ("block" + s.Id) == item.ButtonJumpTo?.Replace("#", "")))
                            {
                                var blockJumpto = businessPage.Blocks.FirstOrDefault(s => s.ElmId == item.ButtonJumpTo);
                                if (blockJumpto != null)
                                    item.ButtonJumpTo = $"#block{blockJumpto.Id}";
                            }
                        }
                    #endregion
                    dbContext.SaveChanges();
                    returnJson.Object = businessPage.Id;
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    returnJson.result = false;
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pageBuilder);
                    transaction.Rollback();
                }
            }
            return returnJson;
        }
        public ReturnJsonModel SetBusinessPageStatus(int pageId, BusinessPageStatusEnum statusEnum)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, pageId, statusEnum);

            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var businessPage = dbContext.BusinessPages.Find(pageId);

                businessPage.Status = statusEnum;
                dbContext.Entry(businessPage).State = EntityState.Modified;

                returnJson.result = dbContext.SaveChanges() > 0;
                returnJson.Object = businessPage.Id;
                returnJson.Object2 = businessPage;

            }
            catch (Exception ex)
            {
                returnJson.result = true;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pageId, statusEnum);
            }
            return returnJson;
        }
        public DataTablesResponse GetBusinessProfilePages(IDataTablesRequest requestModel, int domainId, string keyword, string timezone, string dateformat, int status = -1)
        {
            try
            {
                int totalRecords = 0;
                #region Filters
                var query = dbContext.BusinessPages.Where(s => s.Type == ProfilePageType.Business && s.Domain.Id == domainId);
                if (status >= 0)
                {
                    query = query.Where(s => s.Status == (BusinessPageStatusEnum)status);
                }
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.PageTitle.Contains(keyword));
                }
                #endregion
                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "PageTitle":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "PageTitle" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Created":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreateDate" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "DisplayOrder":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "DisplayOrder" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "DisplayOrder asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "DisplayOrder asc" : orderByString);
                totalRecords = query.Count();
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Key,
                    PageTitle = q.PageTitle,
                    Created = q.CreateDate.ConvertTimeFromUtc(timezone).ToString(dateformat) + " by " + $"<a href=\"/Community/UserProfilePage?uId={q.CreateBy.Id}\" target=\"_blank\">{q.CreateBy.GetFullName().FixQuoteCode()}</a>",
                    DisplayOrder = q.Status == BusinessPageStatusEnum.IsActive ? $"{q.DisplayOrder}" : $"{q.DisplayOrder}(Hidden)",
                    Status = q.Status.ToString()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public ReturnJsonModel DeleteProfilePage(int pageId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, pageId);

            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var businessPage = dbContext.BusinessPages.Find(pageId);
                if (businessPage != null)
                {
                    dbContext.ProfilePages.Remove(businessPage);
                }
                returnJson.result = dbContext.SaveChanges() > 0;
                returnJson.Object = businessPage.Id;
            }
            catch (Exception ex)
            {
                returnJson.result = true;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pageId);
            }
            return returnJson;
        }
        public ReturnJsonModel UpdateDisplayOrder(List<ProfilePage> pages)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, pages);

            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (pages != null)
                {
                    foreach (var item in pages.Where(s => s.DisplayOrder == 0))
                    {
                        var pageOther = pages.FirstOrDefault(s => s.Id != item.Id);
                        var dbPageOther = dbContext.ProfilePages.Find(pageOther.Id);
                        item.DisplayOrder = dbPageOther.DisplayOrder;
                    }
                    foreach (var item in pages)
                    {
                        var page = dbContext.ProfilePages.Find(item.Id);
                        if (page != null)
                        {
                            page.DisplayOrder = item.DisplayOrder;
                        }
                    }
                }
                dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                returnJson.result = true;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pages);
            }
            return returnJson;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">ProfilePage key</param>
        /// <param name="direction">up=-1, down=1</param>
        /// <returns></returns>
        public ReturnJsonModel UpdateDisplayOrderPageBuilder(string key, int direction, int domainId)
        {
            if (direction > 0)
                ChangeOrder(true, key, direction, domainId);
            else
                ChangeOrder(false, key, direction, domainId);

            return new ReturnJsonModel() { result = true };
        }

        private void ChangeOrder(bool orderDown, string key, int direction, int domainId)
        {
            var id = key.Decrypt2Int();
            var currentPage = dbContext.ProfilePages.FirstOrDefault(e => e.Id == id);

            // Get page by current ProfilePages's order + 1
            ProfilePage tempPage;
            if (orderDown)
            {
                tempPage = dbContext.BusinessPages.Where(s => s.Type == ProfilePageType.Business && s.Domain.Id == domainId).OrderBy(o => o.DisplayOrder).FirstOrDefault(m => m.DisplayOrder > currentPage.DisplayOrder);
            }
            else
            {
                tempPage = dbContext.BusinessPages.Where(s => s.Type == ProfilePageType.Business && s.Domain.Id == domainId).OrderByDescending(m => m.DisplayOrder).FirstOrDefault(m => m.DisplayOrder < currentPage.DisplayOrder);

            }
            if (tempPage != null)
            {
                //update tempPage
                tempPage.DisplayOrder = currentPage.DisplayOrder;

                //update tempPage
                currentPage.DisplayOrder += direction;
                dbContext.SaveChanges();
            }
        }
    }
}
