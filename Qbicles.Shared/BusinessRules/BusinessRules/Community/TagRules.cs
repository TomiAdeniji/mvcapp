using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Community;

namespace Qbicles.BusinessRules.Community
{
    public class TagRules
    {
        private ApplicationDbContext _db;

        public TagRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }


        public List<Tag> GetTags()
        {
            var tags = DbContext.Tags.ToList();
            return tags;
        }

        public List<Tag> GetTagByDomain()
        {
            var tags = DbContext.Tags.Where(q => q.IsDomainProfileTag).ToList();
            return tags;
        }

        public List<Tag> GetTagByUserProfile()
        {
            var tags = DbContext.Tags.Where(q => q.IsUserProfileTag).ToList();
            return tags;
        }

        public List<Tag> GetTagByPage()
        {
            var tags = DbContext.Tags.Where(q => q.IsCommunityPageTag).ToList();
            return tags;
        }

        public Tag GetFirstTagByName(string tagName)
        {
            var tag = DbContext.Tags.Where(p => p.Name == tagName).FirstOrDefault();
            return tag;
        }

        public List<int> GetListTagIdByListKeyWord(List<string> lstKey)
        {
            var modelTags = DbContext.Tags
                .Select(q => new {q.Id, key = q.AssociatedKeyWords.Select(x => x.Name.ToLower()).ToList()}).ToList();
            var tags = modelTags.Where(q => q.key.Any() && q.key.Where(x => HelperClass.ContainsKey(lstKey, x)).Any())
                .ToList();
            if (tags.Any())
                return tags.Select(q => q.Id).ToList();
            return new List<int>();
        }

        public List<int> GetListTagIdByListKeyWordSkill(List<string> lstKey)
        {
            var modelTags = DbContext.Tags.Where(d => d.IsSkillTag).Select(q =>
                new {q.Id, key = q.AssociatedKeyWords.Select(x => x.Name.ToLower()).ToList()}).ToList();
            var tags = modelTags.Where(q => q.key.Any() && q.key.Where(x => HelperClass.ContainsKey(lstKey, x)).Any())
                .ToList();
            if (tags.Any())
                return tags.Select(q => q.Id).ToList();
            return new List<int>();
        }

        public bool SaveTag(Tag tag, string[] keywords, string userId)
        {
            Tag _tag = null;
            var user = DbContext.QbicleUser.Find(userId);
            if (tag.Id == 0)
            {
                tag.CreatedBy = user;
                tag.CreatedDate = DateTime.UtcNow;
                tag.EditedDate = DateTime.UtcNow;
                if (keywords != null)
                    foreach (var item in keywords)
                        tag.AssociatedKeyWords.Add(new KeyWord
                        {
                            Name = item,
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow
                        });
                DbContext.Tags.Add(tag);
                DbContext.Entry(tag).State = EntityState.Added;
            }
            else
            {
                _tag = DbContext.Tags.Find(tag.Id);
                _tag.EditedDate = DateTime.UtcNow;
                _tag.Name = tag.Name;
                _tag.AssociatedKeyWords.Clear();
                if (keywords != null)
                    foreach (var item in keywords)
                        _tag.AssociatedKeyWords.Add(new KeyWord
                        {
                            Name = item,
                            CreatedBy = user,
                            CreatedDate = DateTime.UtcNow
                        });
                if (DbContext.Entry(_tag).State == EntityState.Detached)
                    DbContext.Tags.Attach(_tag);
                DbContext.Entry(_tag).State = EntityState.Modified;
            }

            DbContext.SaveChanges();
            return true;
        }

        public bool DeleteTag(int id)
        {
            var tag = DbContext.Tags.Find(id);
            DbContext.Tags.Remove(tag);
            DbContext.SaveChanges();
            return true;
        }

        /// <summary>
        ///     Validation tag. if is used can not delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true - can not delete</returns>
        public bool ValidationDeleteTag(int id)
        {
            return DbContext.Tags.Any(e => e.Id == id &&
                                           (e.IsCommunityPageTag || e.IsDomainProfileTag || e.IsSkillTag ||
                                            e.IsUserProfileTag));
        }

        public bool DuplicateTagName(int id, string Name)
        {
            bool exist;
            if (id > 0)
                exist = DbContext.Tags.Any(x => x.Id != id && x.Name == Name);
            else
                exist = DbContext.Tags.Any(x => x.Name == Name);
            return exist;
        }

        public TagModel GetTagToEditView(int Id)
        {
            var tag = DbContext.Tags.Find(Id);

            var keywords = "";
            foreach (var k in tag.AssociatedKeyWords) keywords += k.Name + ",";
            keywords = keywords.TrimEnd(',');

            var tagRef = new TagModel
            {
                Id = tag.Id,
                Name = tag.Name,
                CreatedBy = tag.CreatedBy.Id,
                CreatedDate = tag.CreatedDate,
                EditedDate = tag.EditedDate,
                Keywords = keywords
            };
            return tagRef;
        }

        public List<Tag> GetTagForAdminCreate()
        {
            var tags = DbContext.Tags.Where(q => q.IsCommunityPageTag).ToList();
            return tags;
        }
    }
}