using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Community;

namespace Qbicles.BusinessRules.Community
{
    public class UserProfilePageRules
    {
        private ApplicationDbContext _db;

        public UserProfilePageRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }

        public UserProfilePage GetUserProfilePage(string userId)
        {
            var profilePage = DbContext.UserProfilePages.FirstOrDefault(d =>
                d.AssociatedUser != null && d.AssociatedUser.Id == userId &&
                d.PageType == CommunityPageTypeEnum.UserProfile);
            return profilePage;
        }

        public UserProfilePage GetUserProfilePageById(int id)
        {
            var profilePage =
                DbContext.UserProfilePages.FirstOrDefault(d =>
                    d.Id == id && d.PageType == CommunityPageTypeEnum.UserProfile);
            return profilePage;
        }

        /// <summary>
        ///     Search User profile
        /// </summary>
        /// <param name="lstId">list tags id</param>
        /// <param name="lstIdSkill"> list skill tags id</param>
        /// <returns></returns>
        public List<int> Search(List<int> lstId, List<int> lstIdSkill)
        {
            var searchModel = DbContext.UserProfilePages.Where(d => d.IsSuspended == false).Select(q =>
                new
                {
                    q.Id,
                    tag1 = q.Tags.Where(t => lstId.Contains(t.Id)).Select(x => x.Id).ToList(),
                    //tag2 = q.Skills.Select(u =>
                    //    new
                    //    {
                    //        tag = u.Tags.Where(st => lstIdSkill.Contains(st.Id)).Select(v => v.Id).ToList()
                    //    }).ToList()
                });

            if (searchModel.Any())
            {
                var resultSearchModel = searchModel.ToList()
                    .Where(q => q.tag1.Any()).ToList();
                return resultSearchModel.Select(q => q.Id).ToList();
            }

            return new List<int>();
        }

        public List<UserProfilePage> GetUserProfilePageByListId(List<int> lstId)
        {
            return DbContext.UserProfilePages.Where(d =>
                    d.PageType == CommunityPageTypeEnum.UserProfile && lstId.Contains(d.Id) && d.IsSuspended == false)
                .ToList();
        }

        public UserProfilePage CreateProfilePage(ApplicationUser user)
        {
            var profilePage = new UserProfilePage();
            profilePage.PageType = CommunityPageTypeEnum.UserProfile;
            profilePage.AssociatedUser = user;
            profilePage.CreatedDate = DateTime.UtcNow;
            profilePage.CreatedBy = user;
            profilePage.LastUpdated = DateTime.UtcNow;
            profilePage.StrapLine = "strap is empty";
            profilePage.StoredLogoName = "noImage";
            profilePage.ProfileText = "Empty";
            profilePage.StoredFeaturedImageName = "noImage";
            DbContext.UserProfilePages.Add(profilePage);
            DbContext.SaveChanges();
            return profilePage;
        }

        //public UserProfilePage SaveAndFinish(UserProfilePage userProfile, List<EmploymentModel> employments, string currentUserId)
        //{
        //    if (userProfile != null)
        //    {
        //        var page = GetUserProfilePage(currentUserId);
        //        var user = DbContext.QbicleUser.Find(currentUserId);
        //        page.StrapLine = userProfile.StrapLine;
        //        if (userProfile.Skills != null)
        //        {
        //            if (page.Skills == null) page.Skills = new List<Skill>();
        //            // delete skill
        //            var lstSkillNew = userProfile.Skills.Select(q => q.Id).ToList();
        //            var lstSkillDeletes = page.Skills.Where(q => !lstSkillNew.Contains(q.Id)).ToList();
        //            if (lstSkillDeletes.Count > 0)
        //                foreach (var item in lstSkillDeletes)
        //                {
        //                    var tagsDeleteSkill = item.Tags;
        //                    foreach (var itemTagDelete in tagsDeleteSkill)
        //                        if (!DbContext.Skills.Where(q =>
        //                            q.Tags.Any(x => x.Id == itemTagDelete.Id) && q.Id != item.Id).Any())
        //                        {
        //                            itemTagDelete.IsSkillTag = false;
        //                            DbContext.Entry(itemTagDelete).State = EntityState.Modified;
        //                        }

        //                    page.Skills.Remove(item);
        //                    DbContext.Entry(item).State = EntityState.Deleted;
        //                    DbContext.SaveChanges();
        //                }

        //            // update skill
        //            for (var i = 0; i < page.Skills.Count; i++)
        //            {
        //                var skill = userProfile.Skills.FirstOrDefault(q => q.Id == page.Skills[i].Id);
        //                if (skill != null)
        //                {
        //                    var lstTagId = skill.Tags.Select(q => q.Id).ToList();
        //                    var tags = DbContext.Tags.Where(q => lstTagId.Contains(q.Id)).ToList();
        //                    var tagIdRemoves = page.Skills[i].Tags.Select(q => q.Id).ToList().Except(lstTagId).ToList();
        //                    var lstTagsRemove = DbContext.Tags.Where(q => tagIdRemoves.Contains(q.Id)).ToList();
        //                    // remove tags
        //                    foreach (var itemTagsRemove in lstTagsRemove)
        //                    {
        //                        itemTagsRemove.IsSkillTag = false;
        //                        DbContext.Entry(itemTagsRemove).State = EntityState.Modified;
        //                        DbContext.SaveChanges();
        //                    }

        //                    // update tags
        //                    foreach (var item in tags)
        //                    {
        //                        item.IsSkillTag = true;
        //                        DbContext.Entry(item).State = EntityState.Modified;
        //                        DbContext.SaveChanges();
        //                    }

        //                    page.Skills[i].Name = skill.Name;
        //                    page.Skills[i].Level = skill.Level;
        //                    page.Skills[i].Description = skill.Description;
        //                    page.Skills[i].Tags.Clear();
        //                    page.Skills[i].Tags = tags;
        //                }
        //            }

        //            // add and update skill
        //            foreach (var item in userProfile.Skills)
        //            {
        //                if (item.Tags != null && item.Tags.Count > 0)
        //                {
        //                    var lstIdtags = item.Tags.Select(q => q.Id).ToList();
        //                    item.Tags = DbContext.Tags.Where(q => lstIdtags.Contains(q.Id)).ToList();
        //                    foreach (var itemTag in item.Tags)
        //                    {
        //                        itemTag.IsSkillTag = true;
        //                        DbContext.Entry(itemTag).State = EntityState.Modified;
        //                        DbContext.SaveChanges();
        //                    }
        //                }
        //                else
        //                {
        //                    item.Tags = new List<Tag>();
        //                }

        //                if (item.Id == 0)
        //                {
        //                    page.Skills.Add(new Skill
        //                    {
        //                        EditedDate = DateTime.UtcNow,
        //                        CreatedBy = user,
        //                        Tags = item.Tags,
        //                        CreatedDate = DateTime.UtcNow,
        //                        Level = item.Level,
        //                        Description = item.Description,
        //                        AssociatedProfile = page,
        //                        Name = item.Name
        //                    });
        //                }
        //                else if (item.Id > 0)
        //                {
        //                    var skill = page.Skills.FirstOrDefault(q => q.Id == item.Id);
        //                    if (skill != null)
        //                    {
        //                        skill.Name = item.Name;
        //                        skill.Level = item.Level;
        //                        skill.Tags = item.Tags;
        //                        skill.EditedDate = DateTime.UtcNow;
        //                        skill.Description = item.Description;
        //                    }
        //                }
        //            }
        //        }

        //        // profile tags
        //        if (userProfile.Tags != null)
        //        {
        //            if (page.Tags == null) page.Tags = new List<Tag>();
        //            var lstId = userProfile.Tags.Select(q => q.Id).ToList();
        //            var tagsRemove = page.Tags.Where(q => !lstId.Contains(q.Id)).ToList();
        //            foreach (var tag in tagsRemove)
        //            {
        //                var domainsProfileAll = DbContext.DomainProfiles.ToList();
        //                domainsProfileAll.ForEach(e => e.Tags = e.Tags.Where(t => t.Id == tag.Id).ToList());
        //                if (domainsProfileAll.Count == 0)
        //                    tag.IsUserProfileTag = false;
        //                page.Tags.Remove(tag);
        //            }

        //            // add new tags
        //            var lstIdPageTags = page.Tags.Select(q => q.Id).ToList();
        //            var itemAdd = userProfile.Tags.Where(q => !lstIdPageTags.Contains(q.Id)).Select(d => d.Id).ToList();
        //            if (itemAdd.Count > 0)
        //            {
        //                var tags = DbContext.Tags.Where(q => itemAdd.Contains(q.Id)).ToList();
        //                foreach (var tag in tags)
        //                {
        //                    tag.IsUserProfileTag = true;
        //                    page.Tags.Add(tag);
        //                }
        //            }
        //        }

        //        // employer
        //        if (employments != null)
        //        {
        //            // delete employer
        //            var lstEmployerNewId = employments.Select(q => q.Id).ToList();
        //            var lstEmployerDeletes = page.Employments.Where(q => !lstEmployerNewId.Contains(q.Id)).ToList();
        //            if (lstEmployerDeletes.Count > 0)
        //                foreach (var item in lstEmployerDeletes)
        //                {
        //                    page.Employments.Remove(item);
        //                    DbContext.Entry(item).State = EntityState.Deleted;
        //                    DbContext.SaveChanges();
        //                }

        //            // update and add new Employ
        //            foreach (var item in employments)
        //                if (item.Id == 0)
        //                {
        //                    page.Employments.Add(new Employment
        //                    {
        //                        Id = 0,
        //                        CreatedDate = DateTime.UtcNow,
        //                        CreatedBy = user,
        //                        Employer = item.Employer,
        //                        StartDate = DateTime.Parse(item.StartDate.ConvertDateFormat("dd/MM/yyyy", "yyyy/MM/dd")),
        //                        Role = item.Role,
        //                        AssociatedProfile = page,
        //                        EndDate = DateTime.Parse(item.EndDate.ConvertDateFormat("dd/MM/yyyy", "yyyy/MM/dd"))
        //                });
        //                }
        //                else
        //                {
        //                    var employ = page.Employments.FirstOrDefault(q => q.Id == item.Id);
        //                    if (employ == null) continue;
        //                    employ.StartDate =
        //                        DateTime.Parse(item.StartDate.ConvertDateFormat("dd/MM/yyyy",
        //                    "yyyy/MM/dd"));
        //                    employ.EndDate =
        //                        DateTime.Parse(item.EndDate.ConvertDateFormat("dd/MM/yyyy",
        //                      "yyyy/MM/dd"));
        //                    employ.Employer = item.Employer;
        //                    employ.Role = item.Role;
        //                }
        //        }

        //        if (DbContext.Entry(page).State == EntityState.Detached)
        //            DbContext.UserProfilePages.Attach(page);
        //        DbContext.Entry(page).State = EntityState.Modified;
        //        DbContext.SaveChanges();
        //    }

        //    return userProfile;
        //}

        // user profile image
        public void CommunityUpdateFeaturedUserImage(string strFeatureImage, string userId)
        {
            var page = GetUserProfilePage(userId);
            page.StoredFeaturedImageName = strFeatureImage;
            if (DbContext.Entry(page).State == EntityState.Detached)
                DbContext.UserProfilePages.Attach(page);
            DbContext.Entry(page).State = EntityState.Modified;
            DbContext.SaveChanges();
        }

        public void UpdateAvatarImage(string strAvatarImage, string userId)
        {
            var page = GetUserProfilePage(userId);
            page.StoredLogoName = strAvatarImage;
            if (DbContext.Entry(page).State == EntityState.Detached)
                DbContext.UserProfilePages.Attach(page);
            DbContext.Entry(page).State = EntityState.Modified;
            DbContext.SaveChanges();
        }

        public void UpdateProfileText(string strProfileText, string userId)
        {
            var page = GetUserProfilePage(userId);
            page.ProfileText = strProfileText;
            if (DbContext.Entry(page).State == EntityState.Detached)
                DbContext.UserProfilePages.Attach(page);
            DbContext.Entry(page).State = EntityState.Modified;
            DbContext.SaveChanges();
        }

        // profileFiles
        public void AddNewFile(List<ProfileFile> lstProfile, string userId)
        {
            var page = GetUserProfilePage(userId);
            if (lstProfile.Count > 0)
                foreach (var item in lstProfile)
                {
                    item.AssociatedProfile = page;
                    page.ProfileFiles.Add(item);
                }

            if (DbContext.Entry(page).State == EntityState.Detached)
                DbContext.UserProfilePages.Attach(page);
            DbContext.Entry(page).State = EntityState.Modified;
            DbContext.SaveChanges();
        }

        public ReturnJsonModel DeleteFiles(List<int> lstId, string userId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (lstId != null && lstId.Count > 0)
                {
                    var page = GetUserProfilePage(userId);
                    var itemRemoves = page.ProfileFiles.Where(q => lstId.Contains(q.Id)).ToList();
                    foreach (var item in itemRemoves) DbContext.Entry(item).State = EntityState.Deleted;
                    DbContext.Entry(page).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }

                refModel.actionVal = 1;
            }
            catch (Exception ex)
            {
                refModel.actionVal = 2;
                refModel.msg = ex.Message;
            }

            return refModel;
        }
    }
}