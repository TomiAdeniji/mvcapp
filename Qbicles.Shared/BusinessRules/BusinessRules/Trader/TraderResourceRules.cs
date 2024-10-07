using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader.Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderResourceRules
    {
        private ApplicationDbContext _db;

        public TraderResourceRules(ApplicationDbContext context)
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

        public ResourceImage GetResourceImageById(int id = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.ResourceImages.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new ResourceImage();
            }
        }
        public List<ResourceImage> GetListResourceImages(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.ResourceImages.Where(q => q.Domain.Id == domainId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<ResourceImage>();
            }
        }

        // document resource
        public ResourceDocument GetResourceDocumentById(int id = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.ResourceDocuments.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new ResourceDocument();
            }
        }
        public List<ResourceDocument> GetListResourceDocuments(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId);
                return DbContext.ResourceDocuments.Where(q => q.Domain.Id == domainId).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId);
                return new List<ResourceDocument>();
            }
        }
        public ReturnJsonModel SaveDocument(ResourceDocument document, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, document);
                var checkDocument = DbContext.ResourceDocuments.Where(q =>
                    q.Domain.Id == document.Domain.Id && q.Id != document.Id && q.Name.ToLower() == document.Name.ToLower());
                if (checkDocument.Any())
                {
                    return new ReturnJsonModel { actionVal = 3, result = false, msg = ResourcesManager._L("ERROR_MSG_632") };
                }

                var s3Rules = new Azure.AzureStorageRules(DbContext);
                if (document.Id == 0)
                {
                    document.CreatedDate = DateTime.UtcNow;
                    document.CreatedBy = DbContext.QbicleUser.Find(userId);
                    s3Rules.ProcessingMediaS3(document.FileUri);
                }
                else
                {
                    var mediaValid = DbContext.ResourceDocuments.Find(document.Id);
                    if (mediaValid.FileUri != document.FileUri)
                        s3Rules.ProcessingMediaS3(document.FileUri);
                }

                document.Category = DbContext.ResourceCategorys.Find(document.Category.Id);
                document.Type = DbContext.QbicleFileTypes.FirstOrDefault(q => q.Extension == document.Type.Extension);
                // add new
                if (document.Id == 0)
                {
                    DbContext.Entry(document).State = EntityState.Added;
                    DbContext.ResourceDocuments.Add(document);
                    DbContext.SaveChanges();
                }
                else // edit
                {
                    var uDocument = DbContext.ResourceDocuments.Find(document.Id);
                    uDocument.FileUri = document.FileUri;
                    uDocument.Category = document.Category;
                    uDocument.Type = document.Type;
                    uDocument.Description = document.Description;
                    uDocument.Name = document.Name;

                    DbContext.Entry(uDocument).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    result.actionVal = 2;
                }

                result.msgId = document.Id.ToString();
            }
            catch (Exception ex)
            {
                result.actionVal = 3;
                //result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, document);
            }

            return result;
        }
        public ReturnJsonModel DeleteDocument(int id)
        {
            var result = new ReturnJsonModel { actionVal = 1, result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                if (id > 0)
                {
                    var document = DbContext.ResourceDocuments.Find(id);
                    DbContext.Entry(document).State = EntityState.Deleted;
                    DbContext.ResourceDocuments.Remove(document);
                    DbContext.SaveChanges();
                }
                else
                {
                    result.actionVal = 3;
                    result.result = false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                result.msg = ex.Message;
                result.result = false;
                result.actionVal = 3;
            }

            return result;
        }
        // end document resource

        // additionalInfo
        public List<AdditionalInfo> GetListAdditionalInfos(int domainId, AdditionalInfoType type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, type);
                switch (type)
                {
                    case AdditionalInfoType.Need:
                    case AdditionalInfoType.QualityRating:
                        return DbContext.AdditionalInfos.Where(q => q.Type == type & q.Domain.Id == domainId).ToList();
                    case AdditionalInfoType.Brand:
                    case AdditionalInfoType.ProductTag:
                        return DbContext.AdditionalInfos.Where(q => q.Type == type).ToList();
                }
                return DbContext.AdditionalInfos.Where(q => q.Type == type).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, type);
                return new List<AdditionalInfo>();
            }
        }

        public List<AdditionalInfo> GetListAdditionalInfos(int domainId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);
                if (domainId == 0)
                    return DbContext.AdditionalInfos.AsNoTracking().OrderBy(n => n.Name).ToList();

                var needsRates = DbContext.AdditionalInfos.AsNoTracking().Where(e => e.Domain.Id == domainId && (e.Type == AdditionalInfoType.Need || e.Type == AdditionalInfoType.QualityRating)).ToList();
                var tagsBrands = DbContext.AdditionalInfos.AsNoTracking().Where(e => e.Type == AdditionalInfoType.ProductTag || e.Type == AdditionalInfoType.Brand).ToList();

                return needsRates.Concat(tagsBrands).OrderBy(n => n.Name).ToList();

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                return new List<AdditionalInfo>();
            }
        }
        public AdditionalInfo GetAdditionalInfoById(int id = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.AdditionalInfos.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new AdditionalInfo();
            }
        }
        public ReturnJsonModel SaveAdditionalInfo(AdditionalInfo additional, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, additional);

                var domainId = additional.Domain.Id;

                var isExisted = false;

                if (additional.Id == 0)
                {
                    if (additional.Type == AdditionalInfoType.ProductTag || additional.Type == AdditionalInfoType.Brand)
                        isExisted = DbContext.AdditionalInfos.Any(q => q.Name.ToLower() == additional.Name.ToLower() && q.Type == additional.Type);
                    else
                        isExisted = DbContext.AdditionalInfos.Any(q => q.Name.ToLower() == additional.Name.ToLower() && q.Type == additional.Type && q.Domain.Id == domainId);
                    if (isExisted)
                        return new ReturnJsonModel
                        {
                            actionVal = 3,
                            result = false,
                            msg = ResourcesManager._L("ERROR_MSG_632", "The name field")
                        };
                }
                else
                {
                    if (additional.Type == AdditionalInfoType.ProductTag || additional.Type == AdditionalInfoType.Brand)
                        isExisted = DbContext.AdditionalInfos.Any(q => q.Id != additional.Id && q.Name.ToLower() == additional.Name.ToLower() && q.Type == additional.Type);
                    else
                        isExisted = DbContext.AdditionalInfos.Any(q => q.Id != additional.Id && q.Name.ToLower() == additional.Name.ToLower() && q.Type == additional.Type && q.Domain.Id == domainId);
                    if (isExisted)
                        return new ReturnJsonModel
                        {
                            actionVal = 3,
                            result = false,
                            msg = ResourcesManager._L("ERROR_MSG_632", "The name field")
                        };
                }


                // add new
                if (additional.Id == 0)
                {
                    additional.CreatedDate = DateTime.UtcNow;
                    additional.CreatedBy = DbContext.QbicleUser.Find(userId);

                    DbContext.Entry(additional).State = EntityState.Added;
                    DbContext.AdditionalInfos.Add(additional);
                    DbContext.SaveChanges();
                }
                else // edit
                {
                    var uAdditional = DbContext.AdditionalInfos.Find(additional.Id);
                    uAdditional.Name = additional.Name;
                    uAdditional.Domain = additional.Domain;

                    DbContext.Entry(uAdditional).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    result.actionVal = 2;
                }

                result.msgId = additional.Id.ToString();
            }
            catch (Exception ex)
            {
                result.result = false;
                result.actionVal = 3;
                //result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, additional);
            }

            return result;
        }
        public ReturnJsonModel DeleteAdditionalInfo(int id)
        {
            var result = new ReturnJsonModel { actionVal = 1, result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                if (id > 0)
                {
                    var additional = DbContext.AdditionalInfos.Find(id);
                    DbContext.Entry(additional).State = EntityState.Deleted;
                    DbContext.AdditionalInfos.Remove(additional);
                    DbContext.SaveChanges();
                }
                else
                {
                    result.actionVal = 3;
                    result.result = false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                result.msg = ex.Message;
                result.result = false;
                result.actionVal = 3;
            }

            return result;
        }

        // end additionalInfo

        public List<ResourceCategory> GetListResourceCategory(int domainId, ResourceCategoryType type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, type);
                return DbContext.ResourceCategorys.Where(q => q.Domain.Id == domainId && q.Type == type).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, type);
                return new List<ResourceCategory>();
            }
        }

        public ReturnJsonModel SaveImage(ResourceImage image, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, image);
                var checkCategory = DbContext.ResourceImages.Where(q =>
                    q.Domain.Id == image.Domain.Id && q.Id != image.Id && q.Name.ToLower() == image.Name.ToLower());
                if (checkCategory.Any())
                {
                    return new ReturnJsonModel { actionVal = 3, result = false, msg = ResourcesManager._L("ERROR_MSG_632") };
                }


                var s3Rules = new Azure.AzureStorageRules(DbContext);
                if (image.Id == 0)
                {
                    s3Rules.ProcessingMediaS3(image.FileUri);
                }
                else
                {
                    var mediaValid = DbContext.ResourceImages.Find(image.Id);
                    if (mediaValid.FileUri != image.FileUri)
                        s3Rules.ProcessingMediaS3(image.FileUri);
                }

                image.Category = DbContext.ResourceCategorys.Find(image.Category.Id);
                image.Type = DbContext.QbicleFileTypes.FirstOrDefault(q => q.Extension == image.Type.Extension);
                // add new
                if (image.Id == 0)
                {
                    image.CreatedDate = DateTime.UtcNow;
                    image.CreatedBy = DbContext.QbicleUser.Find(userId);
                    image.Category.AssociatedImages.Add(image);
                    DbContext.Entry(image).State = EntityState.Added;
                    DbContext.ResourceImages.Add(image);
                    DbContext.SaveChanges();
                }
                else // edit
                {
                    var uImage = DbContext.ResourceImages.Find(image.Id);
                    uImage.FileUri = image.FileUri;
                    uImage.Category = image.Category;
                    uImage.Category.AssociatedImages.Add(uImage);
                    uImage.Type = image.Type;
                    uImage.Description = image.Description;
                    uImage.Name = image.Name;

                    DbContext.Entry(uImage).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    result.actionVal = 2;
                }

                result.msgId = image.Id.ToString();
            }
            catch (Exception ex)
            {
                result.actionVal = 3;
                //result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, image);
            }

            return result;
        }
        public ReturnJsonModel DeleteImage(int id)
        {
            var result = new ReturnJsonModel { actionVal = 1, result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                if (id > 0)
                {
                    var image = DbContext.ResourceImages.Find(id);
                    DbContext.Entry(image).State = EntityState.Deleted;
                    DbContext.ResourceImages.Remove(image);
                    DbContext.SaveChanges();
                }
                else
                {
                    result.actionVal = 3;
                    result.result = false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                result.msg = ex.Message;
                result.result = false;
                result.actionVal = 3;
            }

            return result;
        }

        // access 
        public AccessArea GetAccessAreaById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return DbContext.AccessAreas.Find(id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new AccessArea();
            }
        }
        public List<AccessArea> GetListAccessArea(int domainId, string key = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, domainId, key);
                key = key.ToLower();
                return DbContext.AccessAreas.Where(q => q.Domain.Id == domainId && (q.AreaName.ToLower().Contains(key) || q.Description.ToLower().Contains(key))).ToList();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, domainId, key);
                return new List<AccessArea>();
            }
        }
        public ReturnJsonModel SaveAccessArea(AccessArea access, string userId)
        {
            var result = new ReturnJsonModel { actionVal = 1 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, access);

                var checkAdditional = DbContext.AccessAreas.Where(q =>
                    q.Domain.Id == access.Domain.Id && q.Id != access.Id && q.AreaName.ToLower() == access.AreaName.ToLower());
                if (checkAdditional.Any())
                {
                    return new ReturnJsonModel { actionVal = 3, result = false, msg = ResourcesManager._L("ERROR_MSG_632") };
                }
                var s3Rules = new Azure.AzureStorageRules(DbContext);
                if (access.Id == 0)
                {
                    s3Rules.ProcessingMediaS3(access.ImageUri);
                }
                else
                {
                    var mediaValid = DbContext.AccessAreas.Find(access.Id);
                    if (mediaValid.ImageUri != access.ImageUri)
                        s3Rules.ProcessingMediaS3(access.ImageUri);
                }
                if (access.Location != null && access.Location.Id > 0)
                {
                    access.Location = DbContext.TraderLocations.Find(access.Location.Id);
                }

                // add new
                if (access.Id == 0)
                {
                    access.CreatedDate = DateTime.UtcNow;
                    access.CreatedBy = DbContext.QbicleUser.Find(userId);
                    DbContext.Entry(access).State = EntityState.Added;
                    DbContext.AccessAreas.Add(access);
                    DbContext.SaveChanges();
                }
                else // edit
                {
                    var uAccess = DbContext.AccessAreas.Find(access.Id);
                    uAccess.AreaName = access.AreaName;
                    uAccess.Description = access.Description;
                    uAccess.Location = access.Location;
                    uAccess.ImageUri = access.ImageUri;
                    uAccess.Type = access.Type;
                    DbContext.Entry(uAccess).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    result.actionVal = 2;
                }

                result.msgId = access.Id.ToString();
            }
            catch (Exception ex)
            {
                result.actionVal = 3;
                //result.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, access);
            }

            return result;
        }
        // end access


        // Business Trading Profile page
        public DataTablesResponse GetAdditionalInforDTData(IDataTablesRequest requestModel, string keySearch, int domainId, AdditionalInfoType type)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, keySearch, domainId, type);

                int totalcount = 0;
                #region Filters
                var query = DbContext.AdditionalInfos
                    .Where(q => q.Type == type);
                if (!string.IsNullOrEmpty(keySearch))
                {
                    query = query.Where(s => s.Name.ToLower().Contains(keySearch.Trim().ToLower()));
                }
                if (domainId > 0)
                {
                    query = query.Where(s => s.Domain.Id == domainId);
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
                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "DomainName":
                            orderByString += string.IsNullOrEmpty(orderByString) ? "" : ",";
                            orderByString += column.SortDirection == TB_Column.OrderDirection.Ascendant ? "Domain.Name asc" : "Domain.Name desc";
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
                var dataJson = list.Select(s => new
                {
                    Id = s.Id,
                    Name = s.Name,
                    AssociatedNumber = s.TraderItems?.Count ?? 0,
                    DomainName = s.Domain?.Name ?? "",
                    DomainId = s.Domain?.Id ?? 0,
                    CreatedDateStr = s.CreatedDate.ToString("dd/MM/yyyy hh:mm"),
                    CreatedByName = s.CreatedBy.GetFullName(),
                    UserId = s.CreatedBy.Id
                });
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, type);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        // End Business Trading Profile page
    }
}