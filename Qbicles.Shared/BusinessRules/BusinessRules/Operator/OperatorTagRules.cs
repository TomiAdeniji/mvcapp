using Qbicles.BusinessRules.Model;
using Qbicles.Models.Operator;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorTagRules
    {
        ApplicationDbContext dbContext;
        public OperatorTagRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public OperatorTag getTagById(int id)
        {
            try
            {
                return dbContext.OperatorTags.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public List<OperatorTag> getTagsAll(int domainId)
        {
            try
            {
                return dbContext.OperatorTags.Where(s=>s.Domain.Id== domainId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public ReturnJsonModel SaveTag(OperatorTag tag, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbTag = dbContext.OperatorTags.Find(tag.Id);
                if (dbTag != null)
                {
                    dbTag.Name = tag.Name;
                    dbTag.Summary = tag.Summary;
                    if (dbContext.Entry(dbTag).State == EntityState.Detached)
                        dbContext.OperatorTags.Attach(dbTag);
                    dbContext.Entry(dbTag).State = EntityState.Modified;
                }
                else
                {
                    dbTag = new OperatorTag
                    {
                        Domain = tag.Domain,
                        Name = tag.Name,
                        Summary = tag.Summary,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = dbContext.QbicleUser.Find(userId)
                    };
                    dbContext.OperatorTags.Add(dbTag);
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
        
        public DataTablesResponse SearchTags([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string tagName, int domainId, string currentUserId, string dateFormat)
        {
            try
            {
                var query = dbContext.OperatorTags.Where(t => t.Domain.Id == domainId).AsQueryable();
                if (!String.IsNullOrEmpty(tagName))
                {
                    query = query.Where(t => t.Name.Trim().ToLower().Contains(tagName.Trim().ToLower()));
                }
                int totalTag = query.Count();
                var newQuery = query.ToList().Select(q => new OperatorTagModel
                {
                    Id = q.Id,
                    Name = q.Name,
                    Summary = q.Summary,
                    Creator = HelperClass.GetFullNameOfUser(q.CreatedBy, currentUserId),
                    CreatorId = q.CreatedBy.Id,
                    Created = q.CreatedDate.ToString(dateFormat),
                    Instances = q.FormDefinitions.Count() + q.Goals.Count()
                });
                
                var sortedColumn = requestModel.Columns.GetSortedColumns().FirstOrDefault();
                if (sortedColumn.Data.Equals("Name"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Name);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Name);
                    }
                }
                else if (sortedColumn.Data.Equals("Summary"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Summary);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Summary);
                    }
                }
                else if (sortedColumn.Data.Equals("Created"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Created);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Created);
                    }
                }
                else if (sortedColumn.Data.Equals("Creator"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Creator);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Creator);
                    }
                }
                else if (sortedColumn.Data.Equals("Instances"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Instances);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Instances);
                    }
                }
                var list = newQuery.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                return new DataTablesResponse(requestModel.Draw, list, totalTag, totalTag);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
       
    }
}
