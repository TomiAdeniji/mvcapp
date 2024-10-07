using MySql.Data.MySqlClient;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderLocationRules
    {
        private ApplicationDbContext _db;

        public TraderLocationRules(ApplicationDbContext context)
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

        public ReturnJsonModel SaveOnly(TraderLocation item, string userId)
        {
            var result = new ReturnJsonModel();
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, item);
            if (CheckExistName(item))
            {
                result.actionVal = 3;
                result.msg = item.Name + " already exists.";
                return result;
            }

            try
            {
                if (item.Id == 0)
                {
                    item.CreatedBy = DbContext.QbicleUser.Find(userId);
                    item.CreatedDate = DateTime.UtcNow;
                    DbContext.Entry(item).State = EntityState.Added;
                    DbContext.TraderLocations.Add(item);
                    DbContext.SaveChanges();
                    new PosSettingRules(DbContext).CreatePosSettingDefault(item.Id, userId);
                    result.actionVal = 1;
                }
                else
                {
                    var itemOld = DbContext.TraderLocations.Find(item.Id);
                    itemOld.Name = item.Name;
                    DbContext.Entry(itemOld).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    result.actionVal = 2;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, item);
                result.actionVal = 3;
                result.msg = ex.Message;
            }

            return result;
        }

        public List<TraderLocation> GetTraderLocation(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.TraderLocations.Where(d => d.Domain.Id == domainId).OrderBy(n => n.Name).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<TraderLocation>();
            }
        }

        /// <summary>
        /// Fetch domain trader address with geo loaction 
        /// </summary>
        /// <param name="domainId"></param>
        /// <returns></returns>
		public List<TraderLocation> GetTraderGeoLocation(int domainId)
		{
			try
			{
				if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
				return DbContext.TraderLocations.Where(d => d.Domain.Id == domainId && (d.Address.Latitude != 0 || d.Address.Longitude != 0)).OrderBy(n => n.Name).ToList();
			}
			catch (Exception e)
			{
				LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
				return new List<TraderLocation>();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requestModel"></param>
		/// <param name="keySearch"></param>
		/// <param name="isGeoLocated"></param>
		/// <param name="domainId"></param>
		/// <returns></returns>
		public DataTablesResponse GetLocationDataTableData(IDataTablesRequest requestModel, string keySearch, bool? isGeoLocated, int domainId)
        {
            try
            {
                int totalcount = 0;
                #region Filters
                var query = DbContext.TraderLocations.Where(d => d.Domain.Id == domainId).ToList();

                var dataJson = query.Select(s => new
                {
                    Name = s.Name,
                    Address = s.Address.ToAddress().Replace(",", "<br />"),
                    Lat = s.Address?.Latitude == 0 ? "Not provided" : $"{s.Address?.Latitude}°",
                    Long = s.Address?.Longitude == 0 ? "Not provided" : $"{s.Address?.Longitude}°",
                    GeoText = (s.Address?.Latitude == 0 || s.Address?.Longitude == 0) ? "none" : "check",
                    GeoClass = (s.Address?.Latitude == 0 || s.Address?.Longitude == 0) ? "fa fa-remove red" : "fa fa-check green",
                    Creator = s.CreatedBy.GetFullName(),
                    IsDefaultAddress = s.IsDefaultAddress,
                    DefaultAddressClass = s.IsDefaultAddress ? "checked" : "",
                    Id = s.Id,
                    CanDelText = s.Items.Any() || s.Inventory.Any() ? "disabled" : ""
                }).ToList();


                if (!string.IsNullOrEmpty(keySearch))
                {
                    dataJson = dataJson.Where(s =>
                    s.Name.ToLower().Contains(keySearch.ToLower())
                    || s.Address.ToLower().Contains(keySearch.ToLower())
                    || s.Creator.ToLower().Contains(keySearch.ToLower())
                    || s.Lat.Contains(keySearch.ToLower())
                    || s.Long.Contains(keySearch.ToLower())
                    ).ToList();
                }
                if (isGeoLocated != null)
                {
                    if (isGeoLocated == true)
                    {
                        dataJson = dataJson.Where(s => s.GeoText == "check").ToList();
                    }
                    else
                    {
                        dataJson = dataJson.Where(s => s.GeoText == "none").ToList();
                    }
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
                            if (column.SortDirection == OrderDirection.Ascendant)
                                dataJson = dataJson.OrderBy(e => e.Name).ToList();
                            else
                                dataJson = dataJson.OrderByDescending(e => e.Name).ToList();
                            break;
                        case "Creator":
                            if (column.SortDirection == OrderDirection.Ascendant)
                                dataJson = dataJson.OrderBy(e => e.Creator).ToList();
                            else
                                dataJson = dataJson.OrderByDescending(e => e.Creator).ToList();
                            break;
                        case "Lat":
                            if (column.SortDirection == OrderDirection.Ascendant)
                                dataJson = dataJson.OrderBy(e => e.Lat).ToList();
                            else
                                dataJson = dataJson.OrderByDescending(e => e.Lat).ToList();
                            break;
                        case "Long":
                            if (column.SortDirection == OrderDirection.Ascendant)
                                dataJson = dataJson.OrderBy(e => e.Long).ToList();
                            else
                                dataJson = dataJson.OrderByDescending(e => e.Long).ToList();
                            break;
                        case "GeoText":
                            if (column.SortDirection == OrderDirection.Ascendant)
                                dataJson = dataJson.OrderBy(e => e.GeoText).ToList();
                            else
                                dataJson = dataJson.OrderByDescending(e => e.GeoText).ToList();
                            break;
                        default:
                            orderByString = "Id asc";
                            break;
                    }
                }

                #endregion
               
                var list = dataJson.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                             

                return new DataTablesResponse(requestModel.Draw, list, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        public TraderLocation GetTraderLocationDefault(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.TraderLocations.FirstOrDefault(d => d.Domain.Id == domainId);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new TraderLocation();
            }
        }

        public bool CheckExistName(TraderLocation location)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, location);
            if (location.Id > 0)
                return DbContext.TraderLocations.Any(x =>
                    x.Id != location.Id && x.Domain.Id == location.Domain.Id && x.Name == location.Name);
            return DbContext.TraderLocations.Any(x => x.Name == location.Name && x.Domain.Id == location.Domain.Id);
        }

        public object GetOnlyById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var location = DbContext.TraderLocations.Find(id);
                return new BaseItemModel
                {
                    Id = location.Id,
                    Name = location.Name,
                    CreatedBy = HelperClass.GetFullNameOfUser(location.CreatedBy),
                    CreatedDate = location.CreatedDate.ToString("dd/MM/yyyy")
                };
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return null;
            }
        }


        public ReturnJsonModel SaveLocation(TraderLocation location, string userId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, location);
                if (location != null && !string.IsNullOrEmpty(location.Name))
                {
                    if (location.Id > 0)
                    {
                        var updateLocation = DbContext.TraderLocations.FirstOrDefault(q => q.Id == location.Id);
                        updateLocation.Name = location.Name;
                        if (updateLocation.Address == null) updateLocation.Address = new TraderAddress();
                        updateLocation.Address.AddressLine1 = location.Address.AddressLine1;
                        updateLocation.Address.AddressLine2 = location.Address.AddressLine2;
                        updateLocation.Address.Country = location.Address.Country;
                        updateLocation.Address.PostCode = location.Address.PostCode;
                        updateLocation.Address.State = location.Address.State;
                        updateLocation.Address.City = location.Address.City;
                        updateLocation.Address.Email = location.Address.Email;
                        updateLocation.Address.Phone = location.Address.Phone;
                        updateLocation.Address.Latitude = location.Address.Latitude;
                        updateLocation.Address.Longitude = location.Address.Longitude;

                        if (DbContext.Entry(updateLocation).State == EntityState.Detached)
                            DbContext.TraderLocations.Attach(updateLocation);
                        DbContext.Entry(updateLocation).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        new TraderItemRules(DbContext).UpdateTraderItemLocations(updateLocation.Domain.Id);
                        refModel.actionVal = 2;
                        refModel.msg = updateLocation.Name;
                        refModel.msgId = updateLocation.Id.ToString();
                        refModel.msgName = updateLocation.Name;
                    }
                    else
                    {
                        location.CreatedBy = DbContext.QbicleUser.Find(userId);
                        location.CreatedDate = DateTime.UtcNow;
                        DbContext.TraderLocations.Add(location);
                        DbContext.Entry(location).State = EntityState.Added;
                        DbContext.SaveChanges();
                        new PosSettingRules(DbContext).CreatePosSettingDefault(location.Id, userId);
                        refModel.actionVal = 1;
                        //append to select group
                        refModel.msgId = location.Id.ToString();
                        refModel.msgName = location.Name;
                        // update TraderItems
                        new TraderItemRules(DbContext).UpdateTraderItemLocations(location.Domain.Id);
                        //If a new TraderLocation is created, and there are items that have an InventoryDetail at the other TraderLocations in the Domain, then an InventoryDetail must be created for each of those items at the newly created TraderLocation.
                        CheckingCreateInvDetailForDomain(location.Domain.Id);
                        //All will be “On” by default (meaning they will appear in the Profile)
                        var profile = DbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == location.Domain.Id);
                        if (profile != null && !profile.BusinessLocations.Any(s => s.Id == location.Id))
                        {
                            profile.BusinessLocations.Add(location);
                        }
                    }

                    refModel.result = true;
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, location);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = e.Message;
            }

            return refModel;
        }

        public TraderAddress SaveAddress(TraderAddress address)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, address);

                if (address.Id > 0)
                {
                    var _address = DbContext.TraderAddress.FirstOrDefault(p => p.Id == address.Id);
                    if (_address == null)
                    {
                        return null;
                    }
                    else
                    {
                        _address.AddressLine1 = address.AddressLine1;
                        _address.AddressLine2 = address.AddressLine2;
                        _address.City = address.City;
                        _address.State = address.State;
                        _address.Country = address.Country;
                        _address.PostCode = address.PostCode;
                        _address.IsDefault = address.IsDefault;
                        _address.Latitude = address.Latitude;
                        _address.Longitude = address.Longitude;
                        DbContext.Entry(_address).State = EntityState.Modified;
                        DbContext.SaveChanges();
                        return _address;
                    }
                }
                else
                {
                    DbContext.TraderAddress.Add(address);
                    DbContext.Entry(address).State = EntityState.Added;

                    DbContext.SaveChanges();
                    return address;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, address);
                return null;
            }
        }
        public ReturnJsonModel SaveAddressForUser(TraderAddress address, string userId)
        {
            var responsJson = new ReturnJsonModel();

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, address);
                var user = DbContext.QbicleUser.Find(userId);
                if (user != null)
                {
                    DbContext.TraderAddress.Add(address);
                    DbContext.Entry(address).State = EntityState.Added;
                    user.TraderAddresses.Add(address);
                    responsJson.result = DbContext.SaveChanges() > 0 ? true : false;
                    if (responsJson.result)
                    {
                        responsJson.Object = new { Id = address.Id, Address = address.ToAddress() };
                    }
                }

                return responsJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, address);
                responsJson.result = false;
                responsJson.msg = "Add address failed. Please contact the administrator.";
                return responsJson;
            }
        }

        public bool DeleteLocation(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                var location = GetById(id);
                var domainId = location.Domain.Id;
                DbContext.TraderLocations.Remove(location);
                DbContext.SaveChanges();
                if (DbContext.TraderLocations.Any(d => d.Domain.Id == domainId))
                    return true;
                var traderSeting = DbContext.TraderSettings.FirstOrDefault(d => d.Domain.Id == domainId);
                if (traderSeting == null) return true;
                traderSeting.IsSetupCompleted = TraderSetupCurrent.Location;
                DbContext.Entry(traderSeting).State = EntityState.Modified;
                DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }
        }

        public TraderLocation GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.TraderLocations.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new TraderLocation();
            }

        }

        public ReturnJsonModel GetPosMenusByLocationIds(int[] locationIds, int itemId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationIds, itemId);
                if (locationIds != null && locationIds.Count() > 0)
                {
                    refModel.Object = DbContext.PosMenus.Where(m => locationIds.Contains(m.Location.Id)
                         && m.Categories.Any(c => c.PosCategoryItems.Any(p => p.PosVariants.Any(i => i.TraderItem.Id == itemId)))).Select(n => n.Name).ToList();
                }
                else
                {
                    refModel.Object = new List<string>();
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, locationIds, itemId);
                refModel.result = false;
                refModel.msg = e.Message;
            }

            return refModel;
        }

        public ReturnJsonModel setIsActiveByLocations(int itemId, bool isActive, int[] locationIds)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, itemId, isActive, locationIds);

                var item = DbContext.TraderItems.Find(itemId);
                if (isActive)
                {
                    var locations = DbContext.TraderLocations.Where(l => locationIds.Contains(l.Id));
                    item.Locations.AddRange(locations);
                }
                else
                {
                    item.Locations.RemoveAll(l => locationIds.Contains(l.Id));
                    item.IsActiveInAllLocations = false;
                }
                DbContext.Entry(item).State = EntityState.Modified;
                refModel.result = DbContext.SaveChanges() > 0;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, itemId, isActive, locationIds);
                refModel.result = false;
                refModel.msg = e.Message;
            }

            return refModel;
        }
        /// <summary>
        /// This is the method used ExecuteSqlCommand to let performance optimization
        /// If a new TraderLocation is created, and there are items that have an InventoryDetail at the other TraderLocations in the Domain, then an InventoryDetail must be created for each of those items at the newly created TraderLocation.
        /// </summary>
        /// <param name="domainId"></param>
        public void CheckingCreateInvDetailForDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                string script = @"INSERT INTO  trad_inventorydetail(
	                                MinInventorylLevel,
	                                MaxInventoryLevel,
	                                CurrentInventoryLevel,
	                                CreatedDate,
	                                LastUpdatedDate,
	                                AverageCost,
	                                LatestCost,
	                                StorageLocationRef,
	                                CreatedBy_Id,
	                                CurrentRecipe_Id,
	                                Item_Id,
	                                LastUpdatedBy_Id,
	                                Location_Id
                                )
                                select 
	                                0,/*MinInventorylLevel*/
                                    0,/*MaxInventoryLevel*/
                                    0,/*CurrentInventoryLevel*/
                                    view_temp.CreatedDate,/*CreatedDate*/
                                    view_temp.CreatedDate ,/*LastUpdatedDate*/
                                    0,/*AverageCost*/
                                    0,/*LatestCost*/
                                    null,/*StorageLocationRef*/
                                    view_temp.CreatedBy_Id,/*CreatedBy_Id*/
                                    null,/*CurrentRecipe_Id*/
                                    view_temp.Id,/*Item_Id*/
                                    view_temp.CreatedBy_Id,/*LastUpdatedBy_Id*/
                                    view_temp.Location_Id/*Location_Id*/
                                from
	                                (SELECT  ti.Id,ti.Name,ti.CreatedDate as CreatedDate,ti.CreatedBy_Id as CreatedBy_Id,tl.Id as Location_Id,ivd.Id as item_detail_id,tl.Domain_Id as Domain_Id FROM trad_item as ti
	                                inner join trad_location as tl on (tl.Domain_Id=ti.Domain_Id)
	                                left join trad_inventorydetail as ivd on (ti.Id=ivd.Item_Id and ivd.Location_Id=tl.Id)) as view_temp
                                where view_temp.item_detail_id is null and view_temp.Domain_Id = @Domain_Id and EXISTS (SELECT tivd.Id FROM trad_inventorydetail as tivd WHERE  tivd.Item_Id = view_temp.Id);";
                DbContext.Database.ExecuteSqlCommand(script, new MySqlParameter("@Domain_Id", domainId));
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
            }
        }
        public DataTablesResponse GetBusinessLocations(IDataTablesRequest requestModel, int domainId)
        {
            try
            {
                int totalcount = 0;
                #region Filters
                var businessProfile = DbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == domainId);
                var locationIds = businessProfile != null ? businessProfile.BusinessLocations.Select(s => s.Id).ToArray() : Array.Empty<int>();
                var query = from tradl in DbContext.TraderLocations
                            where tradl.Domain.Id == domainId
                            select tradl;
                if (!string.IsNullOrEmpty(requestModel.Search?.Value ?? ""))
                {
                    query = query.Where(s => s.Name.Contains(requestModel.Search.Value) || (s.Address != null && s.Address.AddressLine1.Contains(requestModel.Search.Value)));
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
                        case "Address":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Address.AddressLine1" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Geolocated":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "(Address.Longitude!=0&&Address.Latitude!=0?true:false)" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "Id asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion                

                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Name = q.Name,
                    Address = (q.Address == null ? "--" : q.Address.ToAddress()),
                    Geolocated = (q.Address != null && q.Address.Longitude != 0 && q.Address.Latitude != 0 ? true : false),
                    IsIncludeInProfile = locationIds.Contains(q.Id)
                }).ToList();

                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "IsIncludeInProfile":
                            if (column.SortDirection == OrderDirection.Ascendant)
                                dataJson = dataJson.OrderBy(o => o.IsIncludeInProfile).ToList();
                            else
                                dataJson = dataJson.OrderByDescending(o => o.IsIncludeInProfile).ToList();
                            break;
                    }
                }


                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public ReturnJsonModel SetLocationIncludeInProfile(int domainId, int locationId, bool isIncludeInProfile)
        {
            var refModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, locationId, isIncludeInProfile);

                var businessProfile = DbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == domainId);
                if (businessProfile == null)
                {
                    refModel.msg = ResourcesManager._L("WARNING_MSG_SAVEBUSINESSPROFILE");
                    return refModel;
                }
                var location = DbContext.TraderLocations.Find(locationId);
                if (isIncludeInProfile && !businessProfile.BusinessLocations.Any(s => s.Id == locationId))
                    businessProfile.BusinessLocations.Add(location);
                else if (!isIncludeInProfile && businessProfile.BusinessLocations.Any(s => s.Id == locationId))
                    businessProfile.BusinessLocations.Remove(location);

                refModel.result = DbContext.SaveChanges() > 0;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, locationId, isIncludeInProfile);
                refModel.result = false;
                refModel.msg = e.Message;
            }

            return refModel;
        }

        public ReturnJsonModel DeleteUserLocation(int addressId)
        {
            var result = new ReturnJsonModel() { actionVal = 3 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, addressId);

                var addressToRemove = DbContext.TraderAddress.FirstOrDefault(p => p.Id == addressId);
                DbContext.TraderAddress.Remove(addressToRemove);
                DbContext.Entry(addressToRemove).State = EntityState.Deleted;
                DbContext.SaveChanges();
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, addressId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }
        public ReturnJsonModel SetUserDefaultLocation(int addressId, string userId)
        {
            var result = new ReturnJsonModel()
            {
                actionVal = 2
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, addressId, userId);

                var user = DbContext.QbicleUser.Find(userId);
                if (user == null)
                {
                    result.result = false;
                    result.msg = "User does not exist.";
                    return result;
                }
                var address = DbContext.TraderAddress.FirstOrDefault(p => p.Id == addressId);
                if (address == null)
                {
                    result.result = false;
                    result.msg = "Address does not exist.";
                    return result;
                }
                else
                {
                    foreach (var addressItem in user.TraderAddresses)
                    {
                        addressItem.IsDefault = false;
                    }

                    address.IsDefault = true;
                    DbContext.Entry(address).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    result.result = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, addressId, userId);
                result.result = false;
                result.msg = "Something went wrong. Please contact the administrator.";
                return result;
            }
        }

        public List<BaseModel> GetTraderLocationBase(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.TraderLocations.Where(d => d.Domain.Id == domainId).OrderBy(n => n.Name).Select(l => new BaseModel { Id = l.Id, Name = l.Name }).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<BaseModel>();
            }
        }
    }
}