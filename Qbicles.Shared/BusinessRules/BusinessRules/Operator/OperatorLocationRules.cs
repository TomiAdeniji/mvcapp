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
    public class OperatorLocationRules
    {
        ApplicationDbContext dbContext;
        public OperatorLocationRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public OperatorLocation getLocationById(int id)
        {
            try
            {
                return dbContext.OperatorLocations.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public List<OperatorLocation> getAlLocationsByDomainId(int id)
        {
            try
            {
                return dbContext.OperatorLocations.Where(o => o.Domain.Id == id && !o.IsHide).OrderBy(o => o.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<OperatorLocation>();
            }
        }
        public ReturnJsonModel SaveLocation(OperatorLocation location, string countryName, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            var country = new CountriesRules().GetCountryByName(countryName);
            try
            {
                var dbLocation = dbContext.OperatorLocations.Find(location.Id);
                if (dbLocation != null)
                {
                    dbLocation.Name = location.Name;
                    dbLocation.AddressLine1 = location.AddressLine1;
                    dbLocation.AddressLine2 = location.AddressLine2;
                    dbLocation.City = location.City;
                    dbLocation.State = location.State;
                    dbLocation.Postcode = location.Postcode;
                    dbLocation.Country = country;
                    dbLocation.Telephone = location.Telephone;
                    dbLocation.Email = location.Email;
                    if (dbContext.Entry(dbLocation).State == EntityState.Detached)
                        dbContext.OperatorLocations.Attach(dbLocation);
                    dbContext.Entry(dbLocation).State = EntityState.Modified;
                }
                else
                {
                    dbLocation = new OperatorLocation
                    {
                        Domain = location.Domain,
                        Name = location.Name,
                        AddressLine1 = location.AddressLine1,
                        AddressLine2 = location.AddressLine2,
                        City = location.City,
                        State = location.State,
                        Postcode = location.Postcode,
                        Country = country,
                        Telephone = location.Telephone,
                        Email = location.Email,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = dbContext.QbicleUser.Find(userId)
                    };
                    dbContext.OperatorLocations.Add(dbLocation);
                    dbContext.Entry(dbLocation).State = EntityState.Added;
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }

        public DataTablesResponse SearchLocations([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string locationSearch, int domainId, string currentUserId, string dateFormat)
        {
            try
            {
                var query = dbContext.OperatorLocations.Where(t => t.Domain.Id == domainId && !t.IsHide).AsQueryable();
                if (!String.IsNullOrEmpty(locationSearch))
                {
                    locationSearch = locationSearch.Trim().ToLower();
                    query = query.Where(t => t.Name.Trim().ToLower().Contains(locationSearch) ||
                                              t.AddressLine1.Trim().ToLower().Contains(locationSearch) ||
                                              t.AddressLine2.Trim().ToLower().Contains(locationSearch) ||
                                              t.City.Trim().ToLower().Contains(locationSearch) ||
                                              t.State.Trim().ToLower().Contains(locationSearch) ||
                                              t.Postcode.Trim().ToLower().Contains(locationSearch) ||
                                              t.Country.CommonName.Trim().ToLower().Contains(locationSearch)
                                       );
                }
                int totalLocation = query.Count();
                var newQuery = query.ToList().Select(q => new OperatorLocationModel
                {
                    Id = q.Id,
                    Name = q.Name,
                    Address = q.Name + ", " + q.AddressLine1 + ", " +
                                       (!String.IsNullOrEmpty(q.AddressLine2) ? q.AddressLine2 + ", " : "") + q.City + ", " +
                                       (!String.IsNullOrEmpty(q.State) ? q.State + ", " : "") +
                                       (!String.IsNullOrEmpty(q.Postcode) ? q.Postcode + ", " : "") +
                                       q.Country.CommonName,
                    Creator = HelperClass.GetFullNameOfUser(q.CreatedBy, currentUserId),
                    CreatorId = q.CreatedBy.Id,
                    Created = q.CreatedDate.ToString(dateFormat)
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
                else if (sortedColumn.Data.Equals("Address"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Address);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Address);
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

                var list = newQuery.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                return new DataTablesResponse(requestModel.Draw, list, totalLocation, totalLocation);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }


        public ReturnJsonModel RemoveOperatorLocation(int id)
        {
            ReturnJsonModel refModel = new ReturnJsonModel { result = false };
            try
            {
                var location = dbContext.OperatorLocations.Find(id);
                if (location.Teams.Any() || location.Workgroups.Any())
                {
                    location.IsHide = true;
                    if (dbContext.Entry(location).State == EntityState.Detached)
                        dbContext.OperatorLocations.Attach(location);
                    dbContext.Entry(location).State = EntityState.Modified;
                }
                else
                {
                    dbContext.OperatorLocations.Remove(location);
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
    }
}
