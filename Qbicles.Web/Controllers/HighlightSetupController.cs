using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.AdminListing;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Highlight;
using Qbicles.Models.Qbicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class HighlightSetupController : BaseController
    {
        #region showing Views
        //Property TYPE
        public ActionResult ListPropertyTypePartialShow()
        {
            var creatorList = new AdminListingRules(dbContext).GetPropTypeCreatorList();
            ViewBag.creatorLst = creatorList;
            return PartialView("_PropertyTypeList");
        }
        public ActionResult AddEditPropertyTypeModalShow(int propertyTypeId)
        {
            var propertyType = new AdminListingRules(dbContext).GetPropertyTypeById(propertyTypeId);
            return PartialView("_AddEditPropertyType", propertyType);
        }

        //Property EXTRA
        public ActionResult ListPropertyExtraPartialShow()
        {
            var creatorList = new AdminListingRules(dbContext).GetPropExtraCreatorList();
            ViewBag.creatorLst = creatorList;
            return PartialView("_PropertyExtraList");
        }

        public ActionResult AddEditPropertyExtraModalShow(int propertyExtraId)
        {
            var propertyType = new AdminListingRules(dbContext).GetPropertyExtrasById(propertyExtraId);
            return PartialView("_AddEditPropertyExtra", propertyType);
        }

        //Countries List
        public ActionResult ListCountriesPartial()
        {
            return PartialView("_CountriesList");
        }

        //public ActionResult AddEditLocationGroupModalShow(int groupId)
        //{
        //    var locationGroup = new AdminListingRules(dbContext).GetLocationGroupById(groupId);
        //    return PartialView("_AddEditLocationGroup", locationGroup);
        //}

        //Location LOCATIONS
        public ActionResult ListHLLocationPartialShow()
        {
            var countries = new CountriesRules().GetAllCountries();
            ViewBag.lstCountries = countries;
            return PartialView("_HLLocationsList");
        }

        public ActionResult AddEditLocationsModalShow(int locationId)
        {
            var locationModel = new AdminListingRules(dbContext).GetListingLocationById(locationId);
            var listLocationCountries = new CountriesRules().GetAllCountries();
            ViewBag.lstLocationCountries = listLocationCountries;
            return PartialView("_AddEditHLLocation", locationModel);
        }
        public ActionResult AddEditBusinessCategoryModalShow(int id)
        {
            return PartialView("_AddEditBusinessCategory", new AdminListingRules(dbContext).GetBusinessCategoryById(id));
        }
        #endregion

        [HttpPost]
        public ActionResult LoadPropertyType([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string propertyNameSearch, string creatorId, int start, int length, int draw)
        {
            try
            {
                var totalRecord = 0;
                List<ListingPropertyCustom> lstResult = new AdminListingRules(dbContext).GetPropertyTypePagination(propertyNameSearch, creatorId, ref totalRecord, requestModel, start, length);
                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = lstResult,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };
                return Json(dataTableData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<PropertyType>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadPropertyExtra([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string propertyNameSearch, string creatorId, int start, int length, int draw)
        {
            try
            {
                var totalRecord = 0;
                List<ListingPropertyCustom> lstResult = new AdminListingRules(dbContext).GetPropertyExtraPagination(propertyNameSearch, creatorId, ref totalRecord, requestModel, start, length);
                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = lstResult,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };
                return Json(dataTableData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<PropertyType>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadHLLocations([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string locationNameString, string countryName, int start, int length, int draw)
        {
            try
            {
                var totalRecord = 0;
                List<ListingLocationCustom> lstResult = new AdminListingRules(dbContext).GetHLLocationPagination(locationNameString, countryName, requestModel, ref totalRecord, start, length);
                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = lstResult,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };
                return Json(dataTableData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<ListingLocationCustom>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadCountries([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keySearch, int start, int length, int draw)
        {
            try
            {
                var totalRecord = 0;
                List<Country> lstResult = new AdminListingRules(dbContext).GetCountryPagination(keySearch, requestModel, ref totalRecord, start, length);
                var dataTableData = new DataTableModel
                {
                    draw = draw,
                    data = lstResult,
                    recordsFiltered = totalRecord,
                    recordsTotal = totalRecord
                };
                return Json(dataTableData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new DataTableModel() { draw = draw, data = new List<ListingLocationCustom>(), recordsFiltered = 0, recordsTotal = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult LoadBusinessCategories([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword)
        {
           return Json(new AdminListingRules(dbContext).GetBusinessCategories(requestModel, keyword), JsonRequestBehavior.AllowGet);
        }

        #region Business Rules
        //Property TYPE
        public ActionResult AddEditPropertyType(PropertyType propertyType)
        {
            var result = new AdminListingRules(dbContext).SaveProppertyType(propertyType, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletePropertyType(int propertyTypeId)
        {
            var result = new AdminListingRules(dbContext).DeletePropertyType(propertyTypeId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //Property EXTRAS
        public ActionResult AddEditPropertyExtra(PropertyExtras propertyExtra)
        {
            var result = new AdminListingRules(dbContext).SaveProppertyExtra(propertyExtra, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeletePropertyExtra(int propertyExtraId)
        {
            var result = new AdminListingRules(dbContext).DeletePropertyExtra(propertyExtraId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //Listing Location
        public ActionResult AddEditHLLocation(HighlightLocation location, string countryName)
        {
            var result = new AdminListingRules(dbContext).SaveHLLocation(location, countryName, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteHLLocation(int locationId)
        {
            var result = new AdminListingRules(dbContext).DeleteHLLocation(locationId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateLocationCountry(int locationId, string countryName)
        {
            var updateResult = new AdminListingRules(dbContext).UpdateHLLocationCountry(locationId, countryName);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddEditBusinessCategory(BusinessCategory category)
        {
            var result = new AdminListingRules(dbContext).SaveBusinessCategory(category, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteBusinessCategory(int id)
        {
            var result = new AdminListingRules(dbContext).DeleteBusinessCategory(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddUserInterest(List<int> interestIds)
        {
            var addResult = new AdminListingRules(dbContext).AddInterestToUser(interestIds, CurrentUser().Id);
            return Json(addResult, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}