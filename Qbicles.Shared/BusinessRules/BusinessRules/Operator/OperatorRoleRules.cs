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
    public class OperatorRoleRules
    {
        ApplicationDbContext dbContext;
        public OperatorRoleRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public OperatorRole getRoleById(int id)
        {
            try
            {
                return dbContext.OperatorRoles.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public ReturnJsonModel SaveRole(OperatorRole role, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var dbRole = dbContext.OperatorRoles.Find(role.Id);
                if (dbRole != null)
                {
                    dbRole.Name = role.Name;
                    dbRole.Summary = role.Summary;
                    if (dbContext.Entry(dbRole).State == EntityState.Detached)
                        dbContext.OperatorRoles.Attach(dbRole);
                    dbContext.Entry(dbRole).State = EntityState.Modified;
                }
                else
                {
                    dbRole = new OperatorRole
                    {
                        Domain = role.Domain,
                        Name = role.Name,
                        Summary = role.Summary,
                        Status = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = dbContext.QbicleUser.Find(userId)
                    };
                    dbContext.OperatorRoles.Add(dbRole);
                    dbContext.Entry(dbRole).State = EntityState.Added;
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }
        
        public DataTablesResponse SearchRoles([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string roleName, int domainId, string dateFormat)
        {
            try
            {
                var query = dbContext.OperatorRoles.Where(t => t.Domain.Id == domainId && !t.IsHide).AsQueryable();
                if (!String.IsNullOrEmpty(roleName))
                {
                    query = query.Where(t => t.Name.Trim().ToLower().Contains(roleName.Trim().ToLower()));
                }
                int totalRole = query.Count();
                var sortedColumn = requestModel.Columns.GetSortedColumns().FirstOrDefault();
                if (sortedColumn.Data.Equals("Name"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        query = query.OrderBy(s => s.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.Name);
                    }
                    
                }
                else if(sortedColumn.Data.Equals("Summary"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        query = query.OrderBy(s => s.Summary);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.Summary);
                    }
                }
                else if (sortedColumn.Data.Equals("Status"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        query = query.OrderBy(s => !s.Status);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => !s.Status);
                    }
                }

                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    q.Name,
                    q.Summary,
                    q.Status
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRole, totalRole);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        public ReturnJsonModel SetOperatorStatus(int id, bool status)
        {
            ReturnJsonModel refModel = new ReturnJsonModel { result = false };
            try
            {
                var role = dbContext.OperatorRoles.Find(id);
                role.Status = status;
                if (dbContext.Entry(role).State == EntityState.Detached)
                        dbContext.OperatorRoles.Attach(role);
                dbContext.Entry(role).State = EntityState.Modified;
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public ReturnJsonModel RemoveOperatorRole(int id)
        {
            ReturnJsonModel refModel = new ReturnJsonModel { result = false };
            try
            {
                var role = dbContext.OperatorRoles.Find(id);
                if (role.Teams.Any())
                {
                    role.IsHide = true;
                    if (dbContext.Entry(role).State == EntityState.Detached)
                        dbContext.OperatorRoles.Attach(role);
                    dbContext.Entry(role).State = EntityState.Modified;
                }
                else
                {
                    dbContext.OperatorRoles.Remove(role);
                }
                
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public List<OperatorRole> getAlRolesByDomainId(int id)
        {
            try
            {
                return dbContext.OperatorRoles.Where(o => o.Domain.Id == id && !o.IsHide && o.Status).OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<OperatorRole>();
            }
        }
    }
}
