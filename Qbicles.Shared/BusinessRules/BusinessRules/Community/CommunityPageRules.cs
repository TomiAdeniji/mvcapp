using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Community;

namespace Qbicles.BusinessRules.Community
{
    public class CommunityPageRules
    {
        private ApplicationDbContext _db;

        public CommunityPageRules(ApplicationDbContext context)
        {
            _db = context;
        }

        private ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            set => _db = value;
        }

        /// <summary>
        ///     Validation Delete page, If any follower, can not delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true - can not delete</returns>
        public bool ValidationDeletePage(int id)
        {
            return DbContext.CommunityPages.Any(e => e.Id == id && e.Followers.Any());
        }

        public List<int> Search(List<int> lstId)
        {
            var modelSearch = DbContext.CommunityPages.Where(d => d.IsSuspended == false)
                .Select(q => new {q.Id, tag = q.Tags.Select(x => x.Id).ToList()});
            if (!modelSearch.Any()) return new List<int>();
            {
                var communityPages = modelSearch.ToList()
                    .Where(q => q.tag.Any() && q.tag.Where(x => lstId.Contains(x)).Any()).ToList();
                return communityPages.Any() ? communityPages.Select(q => q.Id).ToList() : new List<int>();
            }
        }

        public List<CommunityPage> GetCommunityPages(int domainId)
        {
            return DbContext.CommunityPages
                .Where(d => d.Domain.Id == domainId && d.PageType == CommunityPageTypeEnum.CommunityPage)
                .OrderByDescending(q => q.CreatedDate).ToList();
        }

        public void UpdateOrderPages(List<int> lstId, List<int> lstOrder)
        {
            var lstPages = DbContext.CommunityPages.Where(q => lstId.Contains(q.Id)).ToList();
            for (var i = 0; i < lstId.Count; i++)
            {
                var item = lstPages.FirstOrDefault(q => q.Id == lstId[i]);
                if (item == null) continue;
                item.FeatureOrder = lstOrder[i];
                DbContext.Entry(item).State = EntityState.Modified;
            }

            DbContext.SaveChanges();
        }

        public void UpdateIsFeature(int id, bool value = false)
        {
            var page = DbContext.CommunityPages.FirstOrDefault(q => q.Id == id);
            if (page != null)
            {
                page.IsFeatured = value;
                DbContext.Entry(page).State = EntityState.Modified;
            }

            DbContext.SaveChanges();
        }

        public List<CommunityPage> GetCommunityPagesOrder(int domainId)
        {
            var pages = DbContext.CommunityPages.Where(d => d.Domain.Id == domainId).OrderBy(q => q.FeatureOrder);
            if (!pages.Any(q => q.FeatureOrder == 0)) return pages.ToList();
            var index = 1;
            foreach (var item in pages)
            {
                item.FeatureOrder = index;
                index++;
                DbContext.Entry(item).State = EntityState.Modified;
            }

            DbContext.SaveChanges();

            return pages.ToList();
        }

        public IQueryable<CommunityPage> GetCommunityPagesByListDomainId(List<int> lstDomainId)
        {
            var lstCommunity =
                DbContext.CommunityPages.Where(
                    d => lstDomainId.Contains(d.Domain.Id) && d.PageType == CommunityPageTypeEnum.CommunityPage);
            return lstCommunity.Any() ? lstCommunity.OrderByDescending(q => q.CreatedDate) : null;
        }

        public CommunityPage GetCommunityPageById(int id)
        {
            return DbContext.CommunityPages.FirstOrDefault(d =>
                d.PageType == CommunityPageTypeEnum.CommunityPage && d.Id == id);
        }

        public List<CommunityPage> GetCommunityPageByListId(List<int> lstId)
        {
            return DbContext.CommunityPages.Where(d =>
                    d.PageType == CommunityPageTypeEnum.CommunityPage && lstId.Contains(d.Id) && d.IsSuspended == false)
                .ToList();
        }

        public CommunityPage SaveCommunityPage(CommunityPage page)
        {
            if (page.Id > 0)
            {
                var comPage = DbContext.CommunityPages.FirstOrDefault(q => q.Id == page.Id);
                if (comPage != null)
                {
                    comPage.Title = page.Title;
                    comPage.BodyText = page.BodyText;
                    comPage.FeaturedImageCaption = page.FeaturedImageCaption;
                    comPage.FeaturedImage = page.FeaturedImage;
                    comPage.Qbicle = page.Qbicle;
                    comPage.AlertsDisplayStatus = page.AlertsDisplayStatus;
                    comPage.FilesDisplayStatus = page.FilesDisplayStatus;
                    comPage.EventsDisplayStatus = page.EventsDisplayStatus;
                    comPage.PostsDisplayStatus = page.PostsDisplayStatus;
                    comPage.ArticlesDisplayStatus = page.ArticlesDisplayStatus;
                    comPage.LastUpdated = DateTime.UtcNow;
                    comPage.PublicContactEmail = page.PublicContactEmail;
                    if (page.Tags.Count > 0)
                    {
                        var lstId = page.Tags.Select(q => q.Id).ToList();
                        var lstIdPage = comPage.Tags.Select(q => q.Id).ToList();

                        for (var i = 0; i < lstIdPage.Count; i++)
                            if (!lstId.Contains(lstIdPage[i]))
                            {
                                var item = comPage.Tags.FirstOrDefault(q => q.Id == lstIdPage[i]);
                                comPage.Tags.Remove(item);
                                if (item != null)
                                {
                                    item.Pages.Remove(comPage);
                                    if (item.Pages.Count == 0) item.IsCommunityPageTag = false;
                                    DbContext.Entry(item).State = EntityState.Modified;
                                }

                                DbContext.SaveChanges();
                            }
                            else
                            {
                                lstId.Remove(i);
                            }

                        if (lstId.Count > 0)
                        {
                            var tagsNew = DbContext.Tags.Where(q => lstId.Contains(q.Id));
                            foreach (var item in tagsNew) item.IsCommunityPageTag = true;
                            comPage.Tags.AddRange(tagsNew);
                        }
                    }

                    var lstIdNew = page.Articles.Where(q => q.Id == 0).Select(q => q.Id).ToList();
                    var lstIdIdUpdate = page.Articles.Where(q => q.Id > 0).Select(q => q.Id).ToList();
                    var lstItemDelete = comPage.Articles
                        .Where(q => !lstIdIdUpdate.Contains(q.Id) && !lstIdNew.Contains(q.Id))
                        .ToList();
                    if (lstItemDelete.Count > 0)
                        foreach (var item in lstItemDelete)
                        {
                            comPage.Articles.Remove(item);
                            DbContext.Entry(item).State = EntityState.Deleted;
                            DbContext.SaveChanges();
                        }

                    foreach (var item in page.Articles)
                        if (item.Id == 0)
                        {
                            item.AssociatedPage = comPage;
                            comPage.Articles.Add(item);
                        }
                        else
                        {
                            var articles = comPage.Articles.FirstOrDefault(q => q.Id == item.Id);
                            if (articles == null) continue;
                            articles.Image = item.Image;
                            articles.IsDisplayed = item.IsDisplayed;
                            articles.Source = item.Source;
                            articles.Title = item.Title;
                            articles.URL = item.URL;
                        }

                    if (page.Follower_1 != null && page.Follower_1.Id.Length > 0 &&
                        comPage.Followers.Any(q => q.Id == page.Follower_1.Id))
                        comPage.Follower_1 = comPage.Followers.FirstOrDefault(q => q.Id == page.Follower_1.Id);
                    if (page.Follower_2 != null && page.Follower_2.Id.Length > 0 &&
                        comPage.Followers.Any(q => q.Id == page.Follower_2.Id))
                        comPage.Follower_2 = comPage.Followers.FirstOrDefault(q => q.Id == page.Follower_2.Id);
                    if (page.Follower_3 != null && page.Follower_3.Id.Length > 0 &&
                        comPage.Followers.Any(q => q.Id == page.Follower_3.Id))
                        comPage.Follower_3 = comPage.Followers.FirstOrDefault(q => q.Id == page.Follower_3.Id);
                    if (page.Follower_4 != null && page.Follower_4.Id.Length > 0 &&
                        comPage.Followers.Any(q => q.Id == page.Follower_4.Id))
                        comPage.Follower_4 = comPage.Followers.FirstOrDefault(q => q.Id == page.Follower_4.Id);
                    if (page.Follower_5 != null && page.Follower_5.Id.Length > 0 &&
                        comPage.Followers.Any(q => q.Id == page.Follower_5.Id))
                        comPage.Follower_5 = comPage.Followers.FirstOrDefault(q => q.Id == page.Follower_5.Id);

                    if (DbContext.Entry(comPage).State == EntityState.Detached)
                        DbContext.CommunityPages.Attach(comPage);
                    DbContext.Entry(comPage).State = EntityState.Modified;
                }

                DbContext.SaveChanges();
            }
            else
            {
                if (page.Tags == null || page.Tags.Count <= 0) return page;
                var lstIdTags = page.Tags.Select(q => q.Id).ToList();
                page.Tags = DbContext.Tags.Where(q => lstIdTags.Contains(q.Id)).ToList();
                DbContext.CommunityPages.Add(page);
                DbContext.SaveChanges();
            }

            return page;
        }

        public CommunityPage GetCommunityPage(int pageId)
        {
            return DbContext.CommunityPages.FirstOrDefault(d =>
                d.Id == pageId && d.PageType == CommunityPageTypeEnum.CommunityPage);
        }

        public int SuspendReinstatePage(int id)
        {
            var page = DbContext.CommunityPages.Find(id);
            if (page == null) return 1;
            page.IsSuspended = !page.IsSuspended;
            DbContext.SaveChanges();
            return page.IsSuspended ? 1 : 0;
        }

        public bool DeletePage(int id)
        {
            var page = GetCommunityPage(id);
            DbContext.CommunityPages.Remove(page);
            DbContext.SaveChanges();
            return true;
        }

        public List<CommunityPage> GetByUser(string userId)
        {
            return DbContext.CommunityPages.Where(q => q.Followers.Any(x => x.Id == userId)).ToList();
        }

        public bool FollowPage(int id, string userId)
        {
            var page = DbContext.CommunityPages.Find(id);
            var applicationUser = DbContext.QbicleUser.Find(userId);
            if (page != null && page.Followers.Contains(applicationUser)) return true;
            if (page == null) return true;
            page.Followers.Add(applicationUser);
     
            DbContext.SaveChanges();
            return true;
        }

        public bool UnFollowPage(int id, string userId)
        {
            var commPages = DbContext.CommunityPages;
            var page = commPages.Find(id);

            var applicationUser = DbContext.QbicleUser.Find(userId);

            if (page.Followers.Contains(applicationUser))
            {
                page.Followers.Remove(applicationUser);
                DbContext.SaveChanges();
                if (!commPages.Any(p => p.Followers.Any(u => u.Id == applicationUser.Id)))
                {
                  
                }

                DbContext.Entry(page).State = EntityState.Modified;
                DbContext.SaveChanges();
            }

            return true;
        }

        public List<CommunityPage> GetCommunityPagesDisplayOnQbicle(int domainId)
        {
            return DbContext.CommunityPages.Where(d => d.Domain.Id == domainId
                                                       && d.PageType == CommunityPageTypeEnum.CommunityPage
                                                       && d.IsFeatured).OrderBy(q => q.FeatureOrder).ToList();
        }

        public IQueryable<CommunityPage> GetAllCommunityPages()
        {
            var lstCommunity =
                DbContext.CommunityPages.Where(p => p.PageType == CommunityPageTypeEnum.CommunityPage);
            if (lstCommunity.Any())
                return lstCommunity.OrderByDescending(q => q.CreatedDate);
            return null;
        }
    }
}