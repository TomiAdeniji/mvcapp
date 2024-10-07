using Qbicles.BusinessRules.Model;
using Qbicles.Models.Spannered;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.BusinessRules.Spannered
{
    public class SpanneredAssetTagRules
    {
        ApplicationDbContext dbContext;
        public SpanneredAssetTagRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public AssetTag getTagById(int id)
        {
            try
            {
                return dbContext.SpanneredTags.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public ReturnJsonModel SaveTag(AssetTag tag, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbTag = dbContext.SpanneredTags.Find(tag.Id);
                if (dbTag != null)
                {
                    dbTag.Name = tag.Name;
                    dbTag.Summary = tag.Summary;
                    dbTag.LastUpdatedBy = tag.CreatedBy;
                    dbTag.LastUpdateDate = DateTime.UtcNow;
                    if (dbContext.Entry(dbTag).State == EntityState.Detached)
                        dbContext.SpanneredTags.Attach(dbTag);
                    dbContext.Entry(dbTag).State = EntityState.Modified;
                }
                else
                {
                    dbTag = new AssetTag
                    {
                         Domain = tag.Domain,
                        Name = tag.Name,
                        Summary = tag.Summary,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = dbContext.QbicleUser.Find(userId)
                    };
                    dbContext.SpanneredTags.Add(dbTag);
                    dbContext.Entry(dbTag).State = EntityState.Added;
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }
        public ReturnJsonModel DeleteTag(int id)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbTag = dbContext.SpanneredTags.Find(id);
                if (dbTag != null && dbTag.Assets.Any())
                {
                    refModel.msg = "ERROR_MSG_408";
                    return refModel;
                }
                if (dbTag != null)
                {
                    dbContext.SpanneredTags.Remove(dbTag);
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }
        public DataTablesResponse GetTagAll(IDataTablesRequest requestModel, int domainId, string dateFormat)
        {
            try
            {
                var query = dbContext.SpanneredTags.Where(s => s.Domain.Id == domainId).AsQueryable();
                int totalRole = query.Count();
                var list = query.OrderBy(s => s.Name).ToList();
                var dataJson = list.Select(q => new
                {
                    Id = q.Id,
                    Name = q.Name,
                    Summary = q.Summary,
                    Creator = HelperClass.GetFullNameOfUser(q.CreatedBy),
                    Created = q.CreatedDate.ToString(dateFormat),
                    Instances = q.Assets.Count()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRole, totalRole);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public List<AssetTag> GetTags(int domainId)
        {
            try
            {
                return dbContext.SpanneredTags.Where(s => s.Domain.Id == domainId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<AssetTag>();
            }
        }

        public List<AssetTag> GetTagsByAssetId(int assetId)
        {
            try
            {
                return dbContext.SpanneredTags.Where(s => s.Assets.Any(a => a.Id == assetId)).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<AssetTag>();
            }
        }
    }
}
