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
    public class B2BCatalogueItemRules
    {
        private ApplicationDbContext dbContext;
        public B2BCatalogueItemRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public ReturnJsonModel AddCatalogueItem(B2BCatalogItem model,string tradingName,bool IsShown, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, model.CreatedBy.Id, model);
                    model.Item = dbContext.TraderItems.Find(model.Item.Id);
                    if (dbContext.B2BCatalogItems.Any(s => s.Domain.Id == model.Domain.Id && s.Item.Id == model.Item.Id))
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_DATA_EXISTED", model.Item.Name);
                        return returnJson;
                    }
                    if (model.ProviderLocations.Any())
                    {
                        List<TraderLocation> _dblocations = new List<TraderLocation>();
                        foreach (var item in model.ProviderLocations)
                        {
                            var location = dbContext.TraderLocations.Find(item.Id);
                            if (location != null)
                                _dblocations.Add(location);
                        }
                        model.ProviderLocations = _dblocations;
                    }
                    model.ProviderUnit = dbContext.ProductUnits.Find(model.ProviderUnit.Id);
                    model.CreatedDate = DateTime.UtcNow;
                    model.CreatedBy = dbContext.QbicleUser.Find(userId);

                    dbContext.B2BCatalogItems.Add(model);
                    var relationships = dbContext.B2BRelationships.Where(s=>s.Domain1.Id== model.Domain.Id|| s.Domain2.Id==model.Domain.Id).ToList();
                    if(relationships.Any())
                    {
                        foreach (var relationship in relationships)
                        {
                            B2BTradingItem b2bTradingItem = new B2BTradingItem
                            {
                                //b2bTradingItem.Relationship = relationship;
                                //b2bTradingItem.CatalogItem = model;
                                CreatedBy = model.CreatedBy,
                                CreatedDate = model.CreatedDate
                            };
                            //b2bTradingItem.TradingName = tradingName;
                            //b2bTradingItem.IsShown = IsShown;
                            //b2bTradingItem.IsLinked = false;
                            dbContext.B2BTradingItems.Add(b2bTradingItem);
                        }
                    }
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, model.CreatedBy.Id, model);
                }
            }
            
            return returnJson;
        }
        //public ReturnJsonModel RemoveCatalogueItemById(int id)
        //{
        //    ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
        //    using (var dbTransaction = dbContext.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            if (ConfigManager.LoggingDebugSet)
        //                LogManager.Debug(MethodBase.GetCurrentMethod(), null,null, id);
        //            var catalogItem = dbContext.PosVariants.Find(id);
        //            if (catalogItem != null)
        //            {
        //                var b2btraderitems = dbContext.B2BTradingItems.Where(s => s.Variant.Id == catalogItem.Id).ToList();
        //                if(b2btraderitems!=null)
        //                    dbContext.B2BTradingItems.RemoveRange(b2btraderitems);
        //                //dbContext.B2BCatalogItems.Remove(catalogItem);
        //            }
        //            returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
        //            dbTransaction.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            if (ex.Message.Contains("foreign key"))
        //                returnJson.msg = ResourcesManager._L("ERROR_DATA_IN_USED_CAN_NOT_DELETE", "This catalogue item");
        //            LogManager.Error(MethodBase.GetCurrentMethod(), ex,null, id);
        //        }
        //    }
        //    return returnJson;
        //}
        public DataTablesResponse GetCatalogueItems(IDataTablesRequest requestModel, int domainId, string keyword, int itemgroupId)
        {
            try
            {
                int totalcount = 0;
                #region Filters
                var query = dbContext.B2BCatalogItems.Where(s=>s.Domain.Id==domainId);
                if (itemgroupId > 0)
                    query = query.Where(s =>s.Item.Group.Id == itemgroupId);
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s=>s.Item.Name.Contains(keyword)||s.Item.SKU.Contains(keyword)||s.Item.Barcode.Contains(keyword));
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
                        case "Item":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Item.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "SKU":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Item.SKU" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "ProductGroup":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Item.Group.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Unit":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "ProviderUnit.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
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
                    Item=q.Item.Name,
                    q.Item.SKU,
                    ProductGroup=q.Item.Group.Name,
                    Unit=q.ProviderUnit.Name,
                    Locations= q.ProviderLocations.Any()?string.Join(", ", q.ProviderLocations.Select(s=>s.Name)):""
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
    }
}
