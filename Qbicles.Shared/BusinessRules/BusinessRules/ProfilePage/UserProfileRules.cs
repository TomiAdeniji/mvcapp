using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.ProfilePage;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.BusinessRules.ProfilePage
{
    public class UserProfileRules
    {
        ApplicationDbContext dbContext;

        public UserProfileRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public UserPage GetUserPageById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);
                return dbContext.UserPages.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public List<UserPage> GetUserPages(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, userId);
                return dbContext.UserPages.Where(s => s.User.Id == userId && s.Status == UserPageStatusEnum.IsActive).OrderBy(s => s.DisplayOrder).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return null;
            }
        }
        public UserProfilePageModel GetUserPageForJsonById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);
                var page = dbContext.UserPages.Find(id);
                UserProfilePageModel model = new UserProfilePageModel();
                model.Id = page.Id;
                model.PageTitle = page.PageTitle;
                model.Status = page.Status;
                var blHeros = page.Blocks.Where(s => s.Type == BusinessPageBlockType.HeroPersonal).ToList();
                if (blHeros.Any())
                    model.BlockHeroes = blHeros.Cast<HeroPersonal>().ToList();
                var blGalleries = page.Blocks.Where(s => s.Type == BusinessPageBlockType.Gallery).ToList();
                if (blGalleries.Any())
                    model.BlockGalleries = blGalleries.Cast<GalleryList>().ToList();
                var blMasonryGalleries = page.Blocks.Where(s => s.Type == BusinessPageBlockType.MasonryGallery).ToList();
                if (blMasonryGalleries.Any())
                    model.BlockMasonryGalleries = blMasonryGalleries.Cast<MasonryGallery>().ToList();
                var blTextImages = page.Blocks.Where(s => s.Type == BusinessPageBlockType.TextImage).ToList();
                if (blTextImages.Any())
                    model.BlockTextImages = blTextImages.Cast<TextImage>().ToList();
                return model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new UserProfilePageModel();
            }
        }
        public ReturnJsonModel SaveUserPageBuilder(UserProfilePageModel pageBuilder)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, pageBuilder);

            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var userPage = dbContext.UserPages.Find(pageBuilder.Id);
                    if (userPage == null)
                    {
                        var currentDisplayOrder = dbContext.UserPages.Where(s => s.User.Id == pageBuilder.user.Id).OrderByDescending(s => s.DisplayOrder).FirstOrDefault()?.DisplayOrder ?? 1;
                        userPage = new UserPage();
                        userPage.CreateBy = pageBuilder.CreateBy;
                        userPage.CreateDate = DateTime.UtcNow;
                        userPage.PageTitle = pageBuilder.PageTitle;
                        userPage.Status = pageBuilder.Status;
                        userPage.User = pageBuilder.user;
                        userPage.LastUpdatedBy = pageBuilder.CreateBy;
                        userPage.LastUpdatedDate = userPage.CreateDate;
                        userPage.DisplayOrder = currentDisplayOrder + 1;
                        #region add Blocks into BusinessPage
                        if (pageBuilder.BlockHeroes != null)
                            foreach (var item in pageBuilder.BlockHeroes)
                            {
                                if (item != null)
                                {
                                    item.ParentPage = userPage;
                                    dbContext.BlockHeroPersonalTemplates.Add(item);
                                    userPage.Blocks.Add(item);
                                }
                            }
                        if (pageBuilder.BlockGalleries != null)
                            foreach (var item in pageBuilder.BlockGalleries)
                            {
                                if (item != null)
                                {
                                    item.ParentPage = userPage;
                                    dbContext.BlockGalleryTemplates.Add(item);
                                    userPage.Blocks.Add(item);
                                }
                            }
                       
                        if (pageBuilder.BlockTextImages != null)
                            foreach (var item in pageBuilder.BlockTextImages)
                            {
                                if (item != null)
                                {
                                    item.ParentPage = userPage;
                                    dbContext.BlockTextImageTemplates.Add(item);
                                    userPage.Blocks.Add(item);
                                }
                            }
                        if (pageBuilder.BlockMasonryGalleries != null)
                            foreach (var item in pageBuilder.BlockMasonryGalleries)
                            {
                                if (item != null)
                                {
                                    item.ParentPage = userPage;
                                    dbContext.BlockMasonryGalleryTemplates.Add(item);
                                    userPage.Blocks.Add(item);
                                }
                            }
                        #endregion
                        dbContext.UserPages.Add(userPage);
                    }
                    else
                    {
                        userPage.LastUpdatedBy = pageBuilder.CreateBy;
                        userPage.LastUpdatedDate = DateTime.UtcNow;
                        userPage.PageTitle = pageBuilder.PageTitle;
                        userPage.Status = pageBuilder.Status;
                        //dbContext.BlockTemplates.RemoveRange(businessPage.Blocks);
                        #region add Blocks into BusinessPage
                        var itemsBlock = new List<Block>();
                        if (pageBuilder.BlockHeroes != null)
                            foreach (var item in pageBuilder.BlockHeroes)
                            {
                                if (item != null)
                                {
                                    var hero = dbContext.BlockHeroPersonalTemplates.Find(item.Id);
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
                                        hero.HeroIsIncludeButton = item.HeroIsIncludeButton;
                                        hero.HeroExternalLink = item.HeroExternalLink;
                                        hero.HeroButtonJumpTo = item.HeroButtonJumpTo;
                                        hero.HeroButtonText = item.HeroButtonText;
                                        hero.HeroButtonColour = item.HeroButtonColour;
                                        hero.DisplayOrder = item.DisplayOrder;
                                    }
                                    else
                                    {
                                        hero = item;
                                        hero.ParentPage = userPage;
                                        dbContext.BlockHeroPersonalTemplates.Add(hero);
                                        userPage.Blocks.Add(hero);
                                    }
                                    itemsBlock.Add(hero);
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
                                        gallary.ParentPage = userPage;
                                        dbContext.BlockGalleryTemplates.Add(gallary);
                                        userPage.Blocks.Add(gallary);
                                    }
                                    itemsBlock.Add(gallary);
                                }
                            }
                        dbContext.SaveChanges();
                        if (pageBuilder.BlockMasonryGalleries != null)
                            foreach (var item in pageBuilder.BlockMasonryGalleries)
                            {
                                if (item != null)
                                {
                                    var masonrygallary = dbContext.BlockMasonryGalleryTemplates.Find(item.Id);
                                    if (masonrygallary != null)
                                    {
                                        masonrygallary.DisplayOrder = item.DisplayOrder;
                                        masonrygallary.HeadingColour = item.HeadingColour;
                                        masonrygallary.HeadingFontSize = item.HeadingFontSize;
                                        masonrygallary.HeadingText = item.HeadingText;
                                        masonrygallary.HeadingAccentColour = item.HeadingAccentColour;
                                        masonrygallary.SubHeadingColour = item.SubHeadingColour;
                                        masonrygallary.SubHeadingFontSize = item.SubHeadingFontSize;
                                        masonrygallary.SubHeadingText = item.SubHeadingText;
                                        dbContext.BlockMasonryGalleryItems.RemoveRange(masonrygallary.GalleryItems);
                                        masonrygallary.GalleryItems = item.GalleryItems;
                                    }
                                    else
                                    {
                                        masonrygallary = item;
                                        masonrygallary.ParentPage = userPage;
                                        dbContext.BlockMasonryGalleryTemplates.Add(masonrygallary);
                                        userPage.Blocks.Add(masonrygallary);
                                    }
                                    itemsBlock.Add(masonrygallary);
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
                                        textImage.ParentPage = userPage;
                                        dbContext.BlockTextImageTemplates.Add(textImage);
                                        userPage.Blocks.Add(textImage);
                                    }
                                    itemsBlock.Add(textImage);
                                }
                            }
                        dbContext.SaveChanges();
                        #endregion
                        #region removes blocks
                        var blocks = userPage.Blocks.Where(s => !itemsBlock.Any(i => i.Id == s.Id)).ToList();
                        dbContext.BlockTemplates.RemoveRange(blocks);
                        #endregion
                        dbContext.Entry(userPage).State = EntityState.Modified;
                    }
                    returnJson.result = true;
                    #region Use map for Button jumps to
                    if (pageBuilder.BlockTextImages != null)
                        foreach (var item in pageBuilder.BlockTextImages.Where(s => s.ButtonJumpTo != "external"))
                        {
                            if (!userPage.Blocks.Any(s => ("block" + s.Id) == item.ButtonJumpTo?.Replace("#", "")))
                            {
                                var blockJumpto = userPage.Blocks.FirstOrDefault(s => s.ElmId == item.ButtonJumpTo);
                                if (blockJumpto != null)
                                    item.ButtonJumpTo = $"#block{blockJumpto.Id}";
                            }
                        }
                    #endregion
                    dbContext.SaveChanges();
                    returnJson.Object = userPage.Id;
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
        public ReturnJsonModel SetUserPageStatus(int pageId, UserPageStatusEnum statusEnum)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, pageId, statusEnum);

            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var businessPage = dbContext.UserPages.Find(pageId);
                if (businessPage != null)
                {
                    businessPage.Status = statusEnum;
                    dbContext.Entry(businessPage).State = EntityState.Modified;
                }
                returnJson.result = dbContext.SaveChanges() > 0;
                returnJson.Object = businessPage.Id;
            }
            catch (Exception ex)
            {
                returnJson.result = true;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pageId, statusEnum);
            }
            return returnJson;
        }
        public DataTablesResponse GetUserProfilePages(IDataTablesRequest requestModel, string userId, string keyword, string timezone, string dateformat, int status = -1)
        {
            try
            {
                int totalRecords = 0;
                #region Filters
                var query = dbContext.UserPages.Where(s => s.Type == ProfilePageType.User && s.User.Id == userId);
                if (status >= 0)
                {
                    query = query.Where(s => s.Status == (UserPageStatusEnum)status);
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
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Key,
                    PageTitle = q.PageTitle,
                    Created = q.CreateDate.ConvertTimeFromUtc(timezone).ToString(dateformat) + " by " + $"<a href=\"/Community/UserProfilePage?uId={q.CreateBy.Id}\" target=\"_blank\">{q.CreateBy.GetFullName().FixQuoteCode()}</a>",
                    DisplayOrder = q.Status == UserPageStatusEnum.IsActive ? q.DisplayOrder.ToString() : "Hidden",
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
        public ReturnJsonModel DeleteProfilePage(int pageId,string currentUserID)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, pageId);

            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var userPage = dbContext.UserPages.Find(pageId);
                if (userPage != null)
                {
                    if(userPage.User.Id!= currentUserID)
                    {
                        returnJson.msg = "ERROR_MSG_28";
                        return returnJson;
                    }
                        
                    dbContext.ProfilePages.Remove(userPage);
                }
                returnJson.result = dbContext.SaveChanges() > 0;
                returnJson.Object = userPage.Id;
            }
            catch (Exception ex)
            {
                returnJson.result = true;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pageId);
            }
            return returnJson;
        }
    }
}
