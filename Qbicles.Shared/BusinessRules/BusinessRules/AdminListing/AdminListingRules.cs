using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Highlight;
using Qbicles.Models.Qbicles;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.BusinessRules.AdminListing
{
    public class AdminListingRules
    {
        private readonly ApplicationDbContext dbContext;

        public AdminListingRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        #region Property Type

        public List<ListingPropertyCustom> GetPropertyTypePagination(string propertyNameSearch, string creatorId, ref int totalRecord, IDataTablesRequest requestModel, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, propertyNameSearch, creatorId, start, length, requestModel);

                var propertyTypeList = new List<PropertyType>();
                var query = from prop in dbContext.PropertyTypes select prop;

                #region Filter
                if (!string.IsNullOrEmpty(propertyNameSearch))
                {
                    query = query.Where(p => p.Name.ToLower().Contains(propertyNameSearch.ToLower()));
                }

                if (!string.IsNullOrEmpty(creatorId))
                {
                    query = query.Where(p => p.CreatedBy.Id == creatorId);
                }
                #endregion

                totalRecord = query.Count();

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreatorName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.Forename" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc," : " desc,");
                            orderByString += "CreatedBy.Surname" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                    }
                }

                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "Name ASC, CreatedBy.Forename asc, CreatedBy.Surname asc" : orderByString);

                #endregion

                #region Paging
                propertyTypeList = query.ToList().Skip(start).Take(length).ToList();
                #endregion

                var resultList = new List<ListingPropertyCustom>();
                propertyTypeList.ForEach(p =>
                {
                    resultList.Add(new ListingPropertyCustom()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CreatorName = (p.CreatedBy?.Forename ?? "") + " " + (p.CreatedBy?.Surname ?? "")
                    });
                });
                return resultList;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, propertyNameSearch, creatorId, start, length, requestModel);
                return new List<ListingPropertyCustom>();
            }
        }

        public PropertyType GetPropertyTypeById(int propTypeId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, propTypeId);

                var propertyType = dbContext.PropertyTypes.FirstOrDefault(p => p.Id == propTypeId);
                return propertyType;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, propTypeId);
                return null;
            }
        }

        public PropertyType GetPropertyTypeByName(string propTypeName)
        {
            return dbContext.PropertyTypes.FirstOrDefault(p => p.Name.ToLower() == propTypeName.ToLower());
        }

        public ReturnJsonModel SaveProppertyType(PropertyType propType, string userId)
        {
            var result = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, propType, userId);

                var user = dbContext.QbicleUser.Find(userId);
                var propertyInDbWithName = GetPropertyTypeByName(propType.Name);
                if (propertyInDbWithName != null && propertyInDbWithName.Id != propType.Id)
                {
                    result.result = false;
                    result.msg = "Property Type already exists.";
                    return result;
                }

                if (propType.Id > 0)
                {
                    result.actionVal = 2;
                    var propTypeInDb = dbContext.PropertyTypes.FirstOrDefault(p => p.Id == propType.Id);
                    if (propTypeInDb == null)
                    {
                        result.result = false;
                        result.msg = "Property Type does not exist. Update Property Type failed.";
                        return result;
                    }

                    propTypeInDb.Name = propType.Name;
                    dbContext.Entry(propTypeInDb).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();
                    result.result = true;
                    return result;
                }
                else
                {
                    result.actionVal = 1;
                    propType.CreatedBy = user;
                    propType.CreatedDate = DateTime.UtcNow;
                    dbContext.PropertyTypes.Add(propType);
                    dbContext.Entry(propType).State = System.Data.Entity.EntityState.Added;
                    dbContext.SaveChanges();
                    result.result = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, propType, userId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel DeletePropertyType(int propId)
        {
            var result = new ReturnJsonModel() { actionVal = 3 };
            var propertyType = dbContext.PropertyTypes.FirstOrDefault(p => p.Id == propId);
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, propId);

                if (propertyType == null)
                {
                    result.result = true;
                    return result;
                }
                dbContext.Entry(propertyType).State = System.Data.Entity.EntityState.Deleted;
                dbContext.PropertyTypes.Remove(propertyType);
                dbContext.SaveChanges();
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, propId);
                result.result = false;
                result.msg = "The property type " + propertyType.Name + " is in use and cannot be deleted.";
                return result;
            }
        }

        public List<ApplicationUser> GetPropTypeCreatorList()
        {
            var resultList = dbContext.PropertyTypes.Select(p => p.CreatedBy).Distinct().ToList();
            return resultList;
        }

        public List<PropertyType> GetAllPropertyType()
        {
            return dbContext.PropertyTypes.ToList();
        }
        #endregion

        #region Property Extras
        public List<ApplicationUser> GetPropExtraCreatorList()
        {
            var resultList = dbContext.PropertyExtras.Select(p => p.CreatedBy).Distinct().ToList();
            return resultList;
        }

        public List<ListingPropertyCustom> GetPropertyExtraPagination(string propertyNameSearch, string creatorId, ref int totalRecord, IDataTablesRequest requestModel, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, propertyNameSearch, creatorId, start, length, requestModel);

                var propertyExtrasList = new List<PropertyExtras>();
                var query = from prop in dbContext.PropertyExtras select prop;
                #region filter
                if (!string.IsNullOrEmpty(propertyNameSearch))
                {
                    query = query.Where(p => p.Name.ToLower().Contains(propertyNameSearch.ToLower()));
                }

                if (!string.IsNullOrEmpty(creatorId))
                {
                    query = query.Where(p => p.CreatedBy.Id == creatorId);
                }
                #endregion

                totalRecord = query.Count();

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "CreatorName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedBy.Forename" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc," : " desc,");
                            orderByString += "CreatedBy.Surname" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                    }
                }

                query = query.OrderBy(string.IsNullOrEmpty(orderByString) ? "Name ASC, CreatedBy.Forename asc, CreatedBy.Surname asc" : orderByString);

                #endregion

                #region Paging
                propertyExtrasList = query.ToList().Skip(start).Take(length).ToList();
                #endregion

                var resultList = new List<ListingPropertyCustom>();
                propertyExtrasList.ForEach(p =>
                {
                    resultList.Add(new ListingPropertyCustom()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        CreatorName = (p.CreatedBy?.Forename ?? "") + " " + (p.CreatedBy?.Surname ?? "")
                    });
                });
                return resultList;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, propertyNameSearch, creatorId, start, length, requestModel);
                return new List<ListingPropertyCustom>();
            }
        }

        public PropertyExtras GetPropertyExtrasById(int propExtraId)
        {
            return dbContext.PropertyExtras.FirstOrDefault(p => p.Id == propExtraId);
        }

        public PropertyExtras GetPropertyExtrasByName(string propExtraName)
        {
            return dbContext.PropertyExtras.FirstOrDefault(p => p.Name.ToLower() == propExtraName.ToLower());
        }

        public ReturnJsonModel SaveProppertyExtra(PropertyExtras propExtra, string userId)
        {
            var result = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, propExtra, userId);

                var user = dbContext.QbicleUser.Find(userId);
                var propExtraInDbWithName = GetPropertyExtrasByName(propExtra.Name);
                if (propExtraInDbWithName != null && propExtraInDbWithName.Id != propExtra.Id)
                {
                    result.result = false;
                    result.msg = "Property Extra already exists";
                    return result;
                }

                if (propExtra.Id > 0)
                {
                    result.actionVal = 2;
                    var propExtraInDb = dbContext.PropertyExtras.FirstOrDefault(p => p.Id == propExtra.Id);
                    if (propExtraInDb == null)
                    {
                        result.result = false;
                        result.msg = "Property Extra does not exist. Update Property Extra failed.";
                        return result;
                    }

                    propExtraInDb.Name = propExtra.Name;
                    dbContext.Entry(propExtraInDb).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();
                    result.result = true;
                    return result;
                }
                else
                {
                    result.actionVal = 1;
                    propExtra.CreatedBy = user;
                    propExtra.CreatedDate = DateTime.UtcNow;
                    dbContext.PropertyExtras.Add(propExtra);
                    dbContext.Entry(propExtra).State = System.Data.Entity.EntityState.Added;
                    dbContext.SaveChanges();
                    result.result = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, propExtra, userId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel DeletePropertyExtra(int propId)
        {
            var result = new ReturnJsonModel() { actionVal = 3 };
            var propertyExtra = dbContext.PropertyExtras.FirstOrDefault(p => p.Id == propId);
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, propId);


                if (propertyExtra == null)
                {
                    result.result = true;
                    return result;
                }
                dbContext.Entry(propertyExtra).State = System.Data.Entity.EntityState.Deleted;
                dbContext.PropertyExtras.Remove(propertyExtra);
                dbContext.SaveChanges();
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, propId);
                result.result = false;
                result.msg = "The property extra " + propertyExtra.Name + " is in use and cannot be deleted.";
                return result;
            }
        }

        public List<PropertyExtras> GetAllPropertyExtras()
        {
            return dbContext.PropertyExtras.ToList();
        }
        #endregion

        #region Countries 
        public List<Country> GetCountryPagination(string keySearch, IDataTablesRequest requestModel, ref int totalRecord, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, keySearch, requestModel, totalRecord, start, length);

                var resultList = new List<Country>();
                var listCountries = new CountriesRules().GetAllCountries();
                #region filter
                if (!string.IsNullOrEmpty(keySearch))
                {
                    listCountries = listCountries.Where(p => p.CommonName.ToLower().Contains(keySearch.ToLower())).ToList();
                }
                #endregion

                totalRecord = listCountries.Count();

                #region Sorting
                var sortedColumn = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;

                foreach (var column in sortedColumn)
                {
                    switch (column.Data)
                    {
                        case "CommonName":
                            orderByString += string.IsNullOrEmpty(orderByString) ? "" : ",";
                            orderByString += "CommonName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                    }
                }
                listCountries = listCountries.OrderBy(string.IsNullOrEmpty(orderByString) ? "CommonName asc" : orderByString).ToList();
                #endregion

                #region Paging
                resultList = listCountries.Skip(start).Take(length).ToList();
                #endregion

                return resultList;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, keySearch, requestModel, totalRecord, start, length);
                return new List<Country>();
            }
        }
        #endregion

        //public ListingLocationGroup GetLocationGroupById(int groupId)
        //{
        //    try
        //    {
        //        if (ConfigManager.LoggingDebugSet)
        //            LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, groupId);

        //        return dbContext.ListingLocationGroups.FirstOrDefault(p => p.Id == groupId);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, groupId);
        //        return null;
        //    }
        //}

        //public ListingLocationGroup GetLocationGroupByName(string groupName)
        //{
        //    return dbContext.ListingLocationGroups.FirstOrDefault(p => p.Name.ToLower() == groupName.ToLower());
        //}

        //public ReturnJsonModel SaveLocationGroup(ListingLocationGroup locationGroup, ApplicationUser user)
        //{
        //    var result = new ReturnJsonModel();
        //    var locationGroupByName = GetLocationGroupByName(locationGroup.Name);
        //    if (locationGroupByName != null && locationGroupByName.Id != locationGroup.Id)
        //    {
        //        result.result = false;
        //        result.msg = "Location Group already exists";
        //        return result;
        //    }

        //    try
        //    {
        //        if (locationGroup.Id > 0)
        //        {
        //            result.actionVal = 2;
        //            var locationGroupInDb = dbContext.ListingLocationGroups.FirstOrDefault(p => p.Id == locationGroup.Id);
        //            if (locationGroupInDb == null)
        //            {
        //                result.result = false;
        //                result.msg = "The Location Group does not exist. Updating failed.";
        //                return result;
        //            }

        //            locationGroupInDb.Name = locationGroup.Name;
        //            dbContext.Entry(locationGroupInDb).State = System.Data.Entity.EntityState.Modified;
        //            dbContext.SaveChanges();
        //            result.result = true;
        //            return result;
        //        }
        //        else
        //        {
        //            result.actionVal = 1;
        //            locationGroup.CreatedBy = user;
        //            locationGroup.CreatedDate = DateTime.UtcNow;

        //            dbContext.ListingLocationGroups.Add(locationGroup);
        //            dbContext.Entry(locationGroup).State = System.Data.Entity.EntityState.Added;
        //            dbContext.SaveChanges();
        //            result.result = true;
        //            return result;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationGroup, user);
        //        result.result = false;
        //        result.msg = "Something went wrong. Please contact administrator.";
        //        return result;
        //    }
        //}

        //public ReturnJsonModel DeleteLocationGroup(int groupId)
        //{
        //    var result = new ReturnJsonModel() { actionVal = 3 };
        //    var locationGroup = dbContext.ListingLocationGroups.FirstOrDefault(p => p.Id == groupId);

        //    try
        //    {
        //        if (ConfigManager.LoggingDebugSet)
        //            LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, groupId);

        //        if (locationGroup == null)
        //        {
        //            result.result = true;
        //            return result;
        //        }

        //        dbContext.Entry(locationGroup).State = System.Data.Entity.EntityState.Deleted;
        //        dbContext.ListingLocationGroups.Remove(locationGroup);
        //        dbContext.SaveChanges();
        //        result.result = true;
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, groupId);
        //        result.result = false;
        //        result.msg = "The Location Group " + locationGroup.Name + " is in use and cannot be deleted.";
        //        return result;
        //    }
        //}
        //#endregion

        #region Highlight Locations
        public List<ListingLocationCustom> GetHLLocationPagination(string locationNameSearch, string countryCommonName, IDataTablesRequest requestModel, ref int totalRecord, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationNameSearch, countryCommonName, requestModel, totalRecord, start, length);

                var resultList = new List<ListingLocationCustom>();
                var query = from location in dbContext.HighlightLocations select location;

                #region filter
                if (!string.IsNullOrEmpty(locationNameSearch))
                {
                    query = query.Where(p => p.Name.ToLower().Contains(locationNameSearch.ToLower()));
                }

                if (!string.IsNullOrEmpty(countryCommonName))
                {
                    query = query.Where(p => p.Country.CommonName == countryCommonName);
                }
                #endregion

                totalRecord = query.Count();

                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Name":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Group":
                            orderString += string.IsNullOrEmpty(orderString) ? "" : ",";
                            orderString += "Group.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                    }
                    query = query.OrderBy(string.IsNullOrEmpty(orderString) ? "Name asc" : orderString);
                }
                #endregion

                #region Paging
                var listingLocationList = query.Skip(start).Take(length).ToList();
                #endregion

                var listCountries = new CountriesRules().GetAllCountries();
                listingLocationList.ForEach(p =>
                {
                    var locationCustom = new ListingLocationCustom()
                    {
                        Id = p.Id,
                        Name = p.Name
                    };

                    var htmlString = new StringBuilder();
                    htmlString.Append($"<select name=\'locationgroup\' class=\'locationselect2 form-control select2\' onchange=\'UpdateLocationCountry(this)\' locationIdAttr=\'{p.Id}\' style=\'width: 100%;\' tabindex=\'-1\' aria-hidden=\'true\'>");
                    htmlString.Append($"<option value=\'\'></option>");

                    listCountries.ForEach(country =>
                    {
                        var selectedString = "";
                        if (p.Country.CommonName == country.CommonName)
                            selectedString = " selected ";
                        htmlString.Append($"<option value=\'{country.CommonName}\'{selectedString}>{country.CommonName}</option>");
                    });
                    htmlString.Append($"</select>");
                    locationCustom.GroupListRenderString = htmlString.ToString();

                    resultList.Add(locationCustom);
                });
                return resultList;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationNameSearch, countryCommonName, requestModel, totalRecord, start, length);
                return new List<ListingLocationCustom>();
            }
        }

        public HighlightLocation GetHLLocationByName(string locationName)
        {
            return dbContext.HighlightLocations.FirstOrDefault(p => p.Name.ToLower() == locationName.ToLower());
        }

        public HighlightLocation GetListingLocationById(int locationId)
        {
            return dbContext.HighlightLocations.FirstOrDefault(p => p.Id == locationId);
        }

        public List<HighlightLocation> GetAllListingLocation()
        {
            return dbContext.HighlightLocations.ToList();
        }

        public ReturnJsonModel SaveHLLocation(HighlightLocation location, string countryName, string userId)
        {
            var result = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, location, countryName, userId);
                var locationModelByName = GetHLLocationByName(location.Name);
                if (locationModelByName != null && locationModelByName.Id != location.Id)
                {
                    result.result = false;
                    result.msg = "Location already exists";
                    return result;
                }

                var user = dbContext.QbicleUser.Find(userId);

                var country = new CountriesRules().GetCountryByName(countryName);
                if (country == null)
                {
                    result.result = false;
                    result.msg = "Country does not exist.";
                    return result;
                }

                if (location.Id > 0)
                {
                    result.actionVal = 2;

                    var locationInDb = dbContext.HighlightLocations.FirstOrDefault(p => p.Id == location.Id);
                    if (locationInDb == null)
                    {
                        result.result = false;
                        result.msg = "Location does not exist.";
                        return result;
                    }
                    locationInDb.Name = location.Name;
                    locationInDb.Country = country;
                    dbContext.Entry(locationInDb).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();
                    result.result = true;
                    return result;
                }
                else
                {
                    result.actionVal = 1;
                    location.Country = country;
                    location.CreatedBy = user;
                    location.CreatedDate = DateTime.UtcNow;
                    dbContext.HighlightLocations.Add(location);
                    dbContext.Entry(location).State = System.Data.Entity.EntityState.Added;
                    dbContext.SaveChanges();
                    result.result = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, location, countryName, userId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public ReturnJsonModel DeleteHLLocation(int locationId)
        {
            var result = new ReturnJsonModel() { actionVal = 3 };
            var locationInDb = dbContext.HighlightLocations.FirstOrDefault(p => p.Id == locationId);
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);

                if (locationInDb == null)
                {
                    result.result = true;
                    return result;
                }

                locationInDb.Country = null;
                dbContext.HighlightLocations.Remove(locationInDb);
                dbContext.Entry(locationInDb).State = System.Data.Entity.EntityState.Deleted;
                dbContext.SaveChanges();
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
                result.result = false;
                result.msg = "The Location " + locationInDb.Name + " is in use and cannot be deleted.";
                return result;
            }
        }

        public ReturnJsonModel UpdateHLLocationCountry(int locationId, string countryName)
        {
            var resultJson = new ReturnJsonModel() { actionVal = 2 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId, countryName);

                var country = new CountriesRules().GetCountryByName(countryName);
                var location = dbContext.HighlightLocations.Find(locationId);
                if (location == null)
                {
                    resultJson.result = false;
                    resultJson.msg = "Location does not exist.";
                    return resultJson;
                }
                location.Country = country;
                dbContext.Entry(location).State = System.Data.Entity.EntityState.Modified;
                dbContext.SaveChanges();
                resultJson.result = true;
                return resultJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, countryName);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }
        /// <summary>
        /// System listing locations(from listing location table)
        /// A distinct list of the locations from the new location property in the listing class.
        /// </summary>
        /// <returns></returns>
        public List<string> GetLocationNameList()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null);

                var locationNameList = new List<string>();
                locationNameList.AddRange(dbContext.HighlightLocations.Where(p => p.Name.ToLower() != "available everywhere").Select(p => p.Name).Distinct());

                locationNameList = locationNameList.Distinct().OrderBy(p => p).ToList();
                if (!locationNameList.Any(p => p.ToLower() == "available everywhere"))
                {
                    locationNameList.Insert(0, "Available everywhere");
                }
                return locationNameList;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null);
                return new List<string>();
            }
        }

        public List<HighlightLocation> GetListLocationByCountry(string countryName)
        {
            return dbContext.HighlightLocations.Where(p => p.Country.CommonName == countryName).ToList();
        }
        #endregion

        public List<BusinessCategory> GetAllBusinessCategories()
        {
            try
            {

                return dbContext.BusinessCategories.OrderBy(s => s.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<BusinessCategory>();
            }
        }
        public DataTablesResponse GetBusinessCategories(IDataTablesRequest requestModel, string keyword)
        {
            try
            {
                int totalcount = 0;
                #region Filters
                var query = dbContext.BusinessCategories.AsQueryable();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Name.Contains(keyword));
                }

                totalcount = query.Count();
                #endregion
                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Id asc";
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
                    Name = q.Name
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        public ReturnJsonModel SaveBusinessCategory(BusinessCategory category, string userId)
        {
            var result = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, category, userId);

                var user = dbContext.QbicleUser.Find(userId);
                var isExistCategory = dbContext.BusinessCategories.Any(s => s.Name == category.Name && s.Id != category.Id);
                if (isExistCategory)
                {
                    result.result = false;
                    result.msg = ResourcesManager._L("ERROR_DATA_EXISTED", category.Name);
                    return result;
                }

                if (category.Id > 0)
                {
                    result.actionVal = 2;
                    var categoryDb = dbContext.BusinessCategories.Find(category.Id);
                    if (categoryDb == null)
                    {
                        result.result = false;
                        result.msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "data");
                        return result;
                    }

                    categoryDb.Name = category.Name;
                    dbContext.Entry(categoryDb).State = System.Data.Entity.EntityState.Modified;
                    dbContext.SaveChanges();
                    result.result = true;
                    return result;
                }
                else
                {
                    result.actionVal = 1;
                    category.CreatedBy = user;
                    category.CreatedDate = DateTime.UtcNow;
                    dbContext.BusinessCategories.Add(category);
                    dbContext.Entry(category).State = System.Data.Entity.EntityState.Added;
                    dbContext.SaveChanges();
                    result.result = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, category, userId);
                result.result = false;
                return result;
            }
        }

        public ReturnJsonModel DeleteBusinessCategory(int id)
        {
            var result = new ReturnJsonModel() { actionVal = 3 };
            var businessCategory = dbContext.BusinessCategories.Find(id);
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);

                if (businessCategory == null)
                {
                    result.result = true;
                    return result;
                }
                dbContext.Entry(businessCategory).State = System.Data.Entity.EntityState.Deleted;
                dbContext.BusinessCategories.Remove(businessCategory);
                dbContext.SaveChanges();
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_DATA_IN_USED_CAN_NOT_DELETE", businessCategory.Name);
                return result;
            }
        }
        public BusinessCategory GetBusinessCategoryById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);

                return dbContext.BusinessCategories.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public ReturnJsonModel AddInterestToUser(List<int> interestIds, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, interestIds, userId);

                if (interestIds == null || interestIds.Count <= 0)
                {
                    return new ReturnJsonModel() { result = false, msg = "You must choose at least one category to continue." };
                }
                var user = dbContext.QbicleUser.Find(userId);
                user.Interests.Clear();
                //clear user intereset relationships
                foreach (var interestItem in user.Interests)
                {
                    interestItem.Users.Remove(user);
                }

                foreach (var interestId in interestIds)
                {
                    var interest = dbContext.BusinessCategories.Find(interestId);
                    if (interest != null)
                    {
                        user.Interests.Add(interest);
                        interest.Users.Add(user);
                    }
                }
                dbContext.Entry(user).State = EntityState.Modified;
                dbContext.SaveChanges();

                return new ReturnJsonModel() { actionVal = 2, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, interestIds, userId);
                return new ReturnJsonModel() { actionVal = 2, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public List<BusinessProfileAndInterest> GetB2BProfilesByInterests(List<int> lstInterestIds, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, lstInterestIds);

                var listBusinessProfileCustomModel = new List<BusinessProfileAndInterest>();

                foreach (var interestId in lstInterestIds)
                {
                    var interest = dbContext.BusinessCategories.Find(interestId);
                    if (interest != null)
                    {
                        var lstBusinessProfile = interest.Profiles.Where(s => s.IsDisplayedInB2CListings && s.DefaultB2CRelationshipManagers.Any()).ToList();
                        foreach (var businessItem in lstBusinessProfile)
                        {
                            var existedProfile = listBusinessProfileCustomModel.FirstOrDefault(i => i.BusinessId == businessItem.Id);
                            if (existedProfile != null)
                            {
                                existedProfile.ListInterests += ", " + interest.Name;
                            }
                            else
                            {
                                var newCustomObj = new BusinessProfileAndInterest()
                                {
                                    BusinessId = businessItem.Id,
                                    BusinessName = businessItem.BusinessName,
                                    BusinessSummary = businessItem.BusinessSummary,
                                    ListInterests = interest.Name,
                                    LogoUri = businessItem.LogoUri
                                };

                                if (dbContext.B2CQbicles.Any(s => !s.IsHidden && s.Business.Id == businessItem.Domain.Id && s.Customer.Id == currentUserId))
                                    newCustomObj.IsConnected = true;

                                listBusinessProfileCustomModel.Add(newCustomObj);
                            }
                        }
                    }
                }

                return listBusinessProfileCustomModel.Distinct().ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, lstInterestIds);
                return new List<BusinessProfileAndInterest>();
            }
        }

    }
}
