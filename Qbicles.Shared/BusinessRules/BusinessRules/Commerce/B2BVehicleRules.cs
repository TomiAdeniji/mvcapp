using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.B2B;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.Commerce
{
    public class B2BVehicleRules
    {
        ApplicationDbContext dbContext;
        public B2BVehicleRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public Vehicle VehicleById(int verhicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, verhicleId);

                var vehicle = dbContext.B2BVehicles.Find(verhicleId);

                return vehicle != null ? vehicle : new Vehicle();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex,null, verhicleId);
                return new Vehicle();
            }
        }
        public bool CheckVehicleName(int verhicleId, string name,int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, verhicleId, name);

                return dbContext.B2BVehicles.Any(s => s.Id != verhicleId && s.Name.ToLower() == name.ToLower()&&s.Domain.Id==domainId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, verhicleId, name);
                return false;
            }
        }
        public ReturnJsonModel SaveVehicle(Vehicle model, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, model);
                var vehicle = dbContext.B2BVehicles.Find(model.Id);
                if(vehicle != null)
                {
                    vehicle.Name = model.Name;
                    vehicle.RefOrRegistration = model.RefOrRegistration;
                    vehicle.Type = model.Type;
                    vehicle.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                    vehicle.LastUpdatedDate = DateTime.UtcNow;
                    dbContext.Entry(vehicle).State = EntityState.Modified;
                }else
                {
                    var createdBy = dbContext.QbicleUser.Find(userId);
                    vehicle = new Vehicle
                    {
                        Domain = model.Domain,
                        Name = model.Name,
                        RefOrRegistration = model.RefOrRegistration,
                        Type = model.Type,
                        CreatedBy = createdBy,
                        LastUpdatedBy = createdBy,
                        CreatedDate = DateTime.UtcNow
                    };
                    vehicle.LastUpdatedDate = vehicle.CreatedDate;
                    dbContext.B2BVehicles.Add(vehicle);
                    dbContext.Entry(vehicle).State = EntityState.Added;
                }
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
            }
            return returnJson;
        }
        public DataTablesResponse SearchVehicles(IDataTablesRequest requestModel, int domainId, string keyword)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, domainId);
                int totalcount = 0;
                var query = dbContext.B2BVehicles.Where(s => s.Domain.Id == domainId);
                #region Filter
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q => q.Name.Contains(keyword) || q.RefOrRegistration.Contains(keyword));
                totalcount = query.Count();
                #endregion
                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "VehicleType":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Type" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "RefOrRegistration":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "RefOrRegistration" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Type asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "Type asc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                //Get itemlinks
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    VehicleType = q.Type.GetDescription(),
                    q.Name,
                    q.RefOrRegistration
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public ReturnJsonModel DeleteVehicle(int id)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);

                var vehicle = dbContext.B2BVehicles.Find(id);
                if (vehicle != null)
                {
                    if (dbContext.Drivers.Any(s=>s.Vehicle.Id==vehicle.Id))
                    {
                        returnJson.msg =ResourcesManager._L("ERROR_DATA_IN_USED_CAN_NOT_DELETE", vehicle.Name);
                        return returnJson;
                    }
                    dbContext.B2BVehicles.Remove(vehicle);
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return returnJson;
        }
        public List<Select2GroupItems> GetVehiclesForSelect2(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null,domainId);
                var query = dbContext.B2BVehicles.Where(s => s.Domain.Id == domainId);
                var items = query.GroupBy(s => s.Type).Select(g => new Select2GroupItems {
                    GroupName = g.Key.ToString(),
                    Items = g.Select(s => new Select2Model {
                        Id = s.Id,
                        Text = (s.Name + "," + s.RefOrRegistration)
                    }).ToList()
                }).ToList();
                return items;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<Select2GroupItems>();
            }
        }
    }
}
