using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.B2B;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.Commerce
{
    public class B2BTradingItemRules
    {
        private ApplicationDbContext dbContext;
        public B2BTradingItemRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public B2BTradingItem GetTradingItemById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);
                return dbContext.B2BTradingItems.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public ReturnJsonModel UpdateTradingName(int id, string tradingName)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id, tradingName);
                if(string.IsNullOrEmpty(tradingName))
                {
                    returnJson.msg =ResourcesManager._L("ERROR_VALUE_REQUIRED","Trading name");
                    return returnJson;
                }
                //var tradingItem = dbContext.B2BTradingItems.Find(id);
                //if (tradingItem != null)
                //{
                //    tradingItem.TradingName = tradingName;
                //}
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, tradingName);
            }
            return returnJson;
        }
        public ReturnJsonModel UpdateTradingItemStatus(int id,bool isShown)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id, isShown);
                //var tradingItem = dbContext.B2BTradingItems.Find(id);
                //if (tradingItem != null)
                //{
                //    tradingItem.IsShown = isShown;
                //}
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, isShown);
            }
            return returnJson;
        }
        public DataTablesResponse GetTradingItems(IDataTablesRequest requestModel, int relationshipId, string keyword,List<int> groupIds,int status,int currentDomainId)
        {
            try
            {
                int totalcount = 0;
                //#region Filters
                //var query = dbContext.B2BTradingItems.Where(s => s.Relationship.Id == relationshipId && s.CatalogItem.Domain.Id == currentDomainId);
                //if (groupIds!=null&& groupIds.Any())
                //    query = query.Where(s => groupIds.Contains(s.CatalogItem.Item.Group.Id));
                //if (!string.IsNullOrEmpty(keyword))
                //{
                //    query = query.Where(s=>s.TradingName.Contains(keyword)||s.CatalogItem.Item.Name.Contains(keyword)||s.CatalogItem.Item.SKU.Contains(keyword)||s.CatalogItem.Item.Barcode.Contains(keyword));
                //}
                //if(status>0)
                //{
                //    var st = status == 1 ? false : true;
                //    query = query.Where(s=>s.IsShown==st);
                //}
                //totalcount = query.Count();
                //#endregion
                //#region Sorting

                //var sortedColumns = requestModel.Columns.GetSortedColumns();
                //var orderByString = string.Empty;
                //foreach (var column in sortedColumns)
                //{
                //    switch (column.Data)
                //    {
                //        case "Item":
                //            orderByString += orderByString != string.Empty ? "," : "";
                //            orderByString += "CatalogItem.Item.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                //            break;
                //        case "TradingName":
                //            orderByString += orderByString != string.Empty ? "," : "";
                //            orderByString += "TradingName" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                //            break;
                //        case "SKU":
                //            orderByString += orderByString != string.Empty ? "," : "";
                //            orderByString += "CatalogItem.Item.SKU" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                //            break;
                //        case "ProductGroup":
                //            orderByString += orderByString != string.Empty ? "," : "";
                //            orderByString += "CatalogItem.Item.Group.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                //            break;
                //        case "Unit":
                //            orderByString += orderByString != string.Empty ? "," : "";
                //            orderByString += "CatalogItem.ProviderUnit.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                //            break;
                //        default:
                //            orderByString = "Id asc";
                //            break;
                //    }
                //}

                //query = query.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                //#endregion
                //#region Paging
                //var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                //#endregion
                //var dataJson = list.Select(q => new
                //{
                //    q.Id,
                //    Item=q.CatalogItem.Item.Name,
                //    q.TradingName,
                //    q.CatalogItem.Item.SKU,
                //    ProductGroup=q.CatalogItem.Item.Group.Name,
                //    Unit=q.CatalogItem.ProviderUnit.Name,
                //    Status=q.IsShown,
                //    Locations= q.CatalogItem.ProviderLocations.Any()?string.Join(", ", q.CatalogItem.ProviderLocations.Select(s=>s.Name)):""
                //}).ToList();
                //return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);

                return new DataTablesResponse(requestModel.Draw, null, totalcount, totalcount);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public List<B2BTradingItem> GetTradingItemsPartnership(int relationshipId, string keyword, List<int> groupIds, int domainParnershipId,string orderByString)
        {
            try
            {
                //#region Filters
                //var query = dbContext.B2BTradingItems.Where(s => s.Relationship.Id == relationshipId&&s.CatalogItem.Domain.Id==domainParnershipId&&s.IsShown);
                //if (groupIds != null && groupIds.Any())
                //    query = query.Where(s => groupIds.Contains(s.CatalogItem.Item.Group.Id));
                //if (!string.IsNullOrEmpty(keyword))
                //{
                //    query = query.Where(s => s.TradingName.Contains(keyword) || s.CatalogItem.Item.Name.Contains(keyword) || s.CatalogItem.Item.SKU.Contains(keyword) || s.CatalogItem.Item.Barcode.Contains(keyword));
                //}
                //#endregion
                //#region Sorting
                //query = query.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                //#endregion
                //return query.ToList();
                return null;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<B2BTradingItem>();
            }
        }
        public ReturnJsonModel SaveLinkConsumerItem(B2bLinkConsumerItem linkConsumerItem)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, linkConsumerItem);
                var tradingItem = dbContext.B2BTradingItems.Find(linkConsumerItem.TradingItemId);
                if (tradingItem != null)
                {
                    //tradingItem.IsLinked = true;
                    tradingItem.ConsumerDomainItem = dbContext.TraderItems.Find(linkConsumerItem.ConsumerItemId);
                    tradingItem.ConsumerUnit = dbContext.ProductUnits.Find(linkConsumerItem.ConsumerUnitId);
                    //tradingItem.TradingName = linkConsumerItem.TradingName;
                    //tradingItem.ConsumerLocations.Clear();
                    //foreach (var lid in linkConsumerItem.ConsumerLocations)
                    //{
                    //    var location = dbContext.TraderLocations.Find(lid);
                    //    if(location!=null)
                    //    {
                    //        tradingItem.ConsumerLocations.Add(location);
                    //    }
                    //}
                }
                dbContext.SaveChanges();
                returnJson.result =true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, linkConsumerItem);
            }
            return returnJson;
        }
        public DataTablesResponse GetTradingItemsOfRelationship(IDataTablesRequest requestModel, int relationshipId, string keyword, int isLinked)
        {
            try
            {
                int totalcount = 0;
                //#region Filters
                //var query = dbContext.B2BTradingItems.Where(s => s.Relationship.Id == relationshipId);
                //if (!string.IsNullOrEmpty(keyword))
                //{
                //    query = query.Where(s => s.TradingName.Contains(keyword) || s.CatalogItem.Item.Name.Contains(keyword) || s.CatalogItem.Item.SKU.Contains(keyword) || s.CatalogItem.Item.Barcode.Contains(keyword));
                //}
                //if (isLinked > 0)
                //{
                //    var st = isLinked == 1 ? true : false;
                //    query = query.Where(s => s.IsLinked == st);
                //}
                //totalcount = query.Count();
                //#endregion
                //#region Sorting

                //var sortedColumns = requestModel.Columns.GetSortedColumns();
                //var orderByString = string.Empty;
                //foreach (var column in sortedColumns)
                //{
                //    switch (column.Data)
                //    {
                //        case "TradingName":
                //            orderByString += orderByString != string.Empty ? "," : "";
                //            orderByString += "TradingName" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                //            break;
                //        case "SKU":
                //            orderByString += orderByString != string.Empty ? "," : "";
                //            orderByString += "CatalogItem.Item.SKU" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                //            break;
                //        case "Unit":
                //            orderByString += orderByString != string.Empty ? "," : "";
                //            orderByString += "CatalogItem.ProviderUnit.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                //            break;
                //        case "IsLinked":
                //            orderByString += orderByString != string.Empty ? "," : "";
                //            orderByString += "IsLinked" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                //            break;
                //        default:
                //            orderByString = "Id asc";
                //            break;
                //    }
                //}

                //query = query.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                //#endregion
                //#region Paging
                //var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                //#endregion
                //var dataJson = list.Select(q => new
                //{
                //    q.Id,
                //    q.TradingName,
                //    q.CatalogItem.Item.SKU,
                //    Unit = q.CatalogItem.ProviderUnit.Name,
                //    q.IsLinked,
                //    DomainId=q.CatalogItem.Domain.Id
                //}).ToList();
                //return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);
                return new DataTablesResponse(requestModel.Draw, null, totalcount, totalcount);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        /// <summary>
        /// Checking and auto-create trading item if catalog item exist when has a relationship is accepted
        /// </summary>
        /// <param name="relationship"></param>
        public void CheckingTradingItemsWhenHasNewRelationship(B2BRelationship relationship)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, relationship);

                //var catalogueItems1 = dbContext.B2BCatalogItems.Where(s=>s.Domain.Id==relationship.Domain1.Id).ToList();
                //foreach (var item in catalogueItems1)
                //{
                //    var b2bTradingItem = dbContext.B2BTradingItems.FirstOrDefault(s=>s.Relationship.Id==relationship.Id&&s.CatalogItem.Id==item.Id);
                //    if(b2bTradingItem==null)
                //    {
                //        b2bTradingItem = new B2BTradingItem();
                //        b2bTradingItem.Relationship = relationship;
                //        b2bTradingItem.CatalogItem = item;
                //        b2bTradingItem.CreatedBy = item.CreatedBy;
                //        b2bTradingItem.CreatedDate = DateTime.UtcNow;
                //        b2bTradingItem.TradingName = item.Item.Name;
                //        b2bTradingItem.IsShown = false;
                //        b2bTradingItem.IsLinked = false;
                //        dbContext.B2BTradingItems.Add(b2bTradingItem);
                //    }
                //}
                //var catalogueItems2 = dbContext.B2BCatalogItems.Where(s => s.Domain.Id == relationship.Domain2.Id).ToList();
                //foreach (var item in catalogueItems2)
                //{
                //    var b2bTradingItem = dbContext.B2BTradingItems.FirstOrDefault(s => s.Relationship.Id == relationship.Id && s.CatalogItem.Id == item.Id);
                //    if (b2bTradingItem == null)
                //    {
                //        b2bTradingItem = new B2BTradingItem();
                //        b2bTradingItem.Relationship = relationship;
                //        b2bTradingItem.CatalogItem = item;
                //        b2bTradingItem.CreatedBy = item.CreatedBy;
                //        b2bTradingItem.CreatedDate = DateTime.UtcNow;
                //        b2bTradingItem.TradingName = item.Item.Name;
                //        b2bTradingItem.IsShown = false;
                //        b2bTradingItem.IsLinked = false;
                //        dbContext.B2BTradingItems.Add(b2bTradingItem);
                //    }
                //}
                //dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, relationship);
            }
        }
        public bool CheckingSellToOther(int relationshipId, int currentDomainId)
        {
            try
            {
                //return dbContext.B2BTradingItems.Any(s => s.Relationship.Id == relationshipId && s.CatalogItem.Domain.Id == currentDomainId);
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return false;
            }
        }
    }
}
