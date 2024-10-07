using Qbicles.BusinessRules.Model;
using Qbicles.Models.Operator.Goals;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorMeasureRules
    {
        ApplicationDbContext dbContext;
        public OperatorMeasureRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public Measure getMeasureById(int id)
        {
            try
            {
                return dbContext.OperatorMeasures.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public List<Measure> GetMeasuresAll(int domainId)
        {
            try
            {
                return dbContext.OperatorMeasures.Where(s => s.Domain.Id == domainId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public ReturnJsonModel SaveMeasure(Measure measure, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbMeasure = dbContext.OperatorMeasures.Find(measure.Id);
                if (dbMeasure != null)
                {
                    dbMeasure.Name = measure.Name;
                    dbMeasure.Summary = measure.Summary;
                    if (dbContext.Entry(dbMeasure).State == EntityState.Detached)
                        dbContext.OperatorMeasures.Attach(dbMeasure);
                    dbContext.Entry(dbMeasure).State = EntityState.Modified;
                }
                else
                {
                    dbMeasure = new Measure
                    {
                        Domain = measure.Domain,
                        Name = measure.Name,
                        Summary = measure.Summary,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = dbContext.QbicleUser.Find(userId)
                    };
                    dbContext.OperatorMeasures.Add(dbMeasure);
                    dbContext.Entry(dbMeasure).State = EntityState.Added;
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }
        public ReturnJsonModel DeleteMeasure(int id)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbMeasure = dbContext.OperatorMeasures.Find(id);
                if (dbMeasure != null)
                {
                    if (dbMeasure.FormElements.Any() || dbMeasure.GoalMeasures.Any() || dbMeasure.TrackingMeasures.Any())
                    {
                        refModel.msg = "ERROR_MSG_702";
                        return refModel;
                    }
                    dbContext.OperatorMeasures.Remove(dbMeasure);
                    refModel.result = dbContext.SaveChanges() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }
        public ReturnJsonModel DeleteGoalMeasure(int id)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbGoalMeasure = dbContext.OperatorGoalMeasures.Find(id);
                if (dbGoalMeasure != null)
                {
                    dbContext.OperatorGoalMeasures.Remove(dbGoalMeasure);
                    refModel.result = dbContext.SaveChanges() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }
        public DataTablesResponse SearchMeasures([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string measureName, int domainId, string dateFormat)
        {
            try
            {
                var query = dbContext.OperatorMeasures.Where(t => t.Domain.Id == domainId).AsQueryable();
                int totalMeasure = 0;
                #region Filter
                var keyword = requestModel.Search != null ? requestModel.Search.Value.Trim() : "";
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q =>
                        q.Name.Contains(keyword)
                        || q.Summary.Contains(keyword)
                    );
                totalMeasure = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Measure":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Summary":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Summary" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Name asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Measure = q.Name,
                    q.Summary,
                    AllowDelete = !(q.TrackingMeasures.Any() || q.GoalMeasures.Any() || q.FormElements.Any())
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalMeasure, totalMeasure);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public bool CheckMeasuresExist(int domainId)
        {
            try
            {
                return dbContext.OperatorMeasures.Any(s=>s.Domain.Id==domainId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return false;
            }
        }
    }
}
