using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.Broadcast;
using Qbicles.BusinessRules.Helper;
using Attribute = Qbicles.Models.SalesMkt.Attribute;
using System.Threading;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class SocialBrandRules
    {
        private ApplicationDbContext _db;

        public SocialBrandRules()
        {
        }

        public SocialBrandRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }
        public Brand GetBrandById(int brandId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Brand by id", null, null, brandId);

                return DbContext.SMBrands.FirstOrDefault(b => b.Id == brandId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, brandId);
                return null;
            }
        }
        public ReturnJsonModel SaveBrand(BrandCustomModel model, MediaModel media, Settings setting, string userId)
        {
            var returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save brand", userId, null, model, media, setting);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(DbContext);

                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }

                var user = DbContext.QbicleUser.Find(userId);
                var qbicle = DbContext.Qbicles.Find(setting.SourceQbicle.Id);
                if (qbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_188");
                    return returnModel;
                }
                var medfolder = AddMediaFolder(model.FeaturedImageUri, model.FolderName, qbicle, user);
                if (medfolder == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_189");
                    return returnModel;
                }
                var topic = DbContext.Topics.Find(setting.DefaultTopic.Id);
                if (topic == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_190");
                    return returnModel;
                }
                var mediaModel = AddMediaQbicle(media, user, qbicle, medfolder, model.Name, model.Description, topic);
                if (mediaModel != null)
                {
                    qbicle.Media.Add(mediaModel);
                    DbContext.Entry(mediaModel).State = EntityState.Added;
                }
                var dbBrand = DbContext.SMBrands.Find(model.Id);
                if (dbBrand != null)
                {
                    dbBrand.ResourceFolder = medfolder;
                    if (!string.IsNullOrEmpty(media.UrlGuid))
                    {
                        dbBrand.FeaturedImageUri = media.UrlGuid;
                    }
                    dbBrand.Name = model.Name;
                    dbBrand.Description = model.Description;
                    dbBrand.LastUpdatedBy = user;
                    dbBrand.LastUpdateDate = DateTime.UtcNow;
                    if (DbContext.Entry(dbBrand).State == EntityState.Detached)
                        DbContext.SMBrands.Attach(dbBrand);
                    DbContext.Entry(dbBrand).State = EntityState.Modified;
                }
                else
                {
                    Brand brand = new Brand
                    {
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedBy = user,
                        CreatedBy = user,
                        Name = model.Name,
                        Description = model.Description,
                        Domain = model.CurrentDomain,
                        FeaturedImageUri = media.UrlGuid,
                        ResourceFolder = medfolder
                    };
                    brand.LastUpdateDate = brand.CreatedDate;
                    DbContext.SMBrands.Add(brand);
                    DbContext.Entry(brand).State = EntityState.Added;
                }
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model, media, setting);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }
        private MediaFolder AddMediaFolder(int resourcesfolder, string newfoldername, Qbicle qbicle, ApplicationUser user)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add media folder", user.Id, null, resourcesfolder, newfoldername, user);

                if (!string.IsNullOrEmpty(newfoldername))
                {
                    var media = DbContext.MediaFolders.FirstOrDefault(s => s.Name == newfoldername && s.Qbicle.Id == qbicle.Id);
                    if (media == null)
                    {
                        media = new MediaFolder();
                        media.Name = newfoldername;
                        media.Qbicle = qbicle;
                        media.CreatedDate = DateTime.Now;
                        media.CreatedBy = user;
                        DbContext.MediaFolders.Add(media);
                        DbContext.Entry(media).State = EntityState.Added;
                    }
                    return media;
                }
                else
                {
                    return DbContext.MediaFolders.Find(resourcesfolder);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, resourcesfolder, newfoldername, user);
                return null;
            }
        }
        private QbicleMedia AddMediaQbicle(MediaModel media, ApplicationUser user, Qbicle qbicle, MediaFolder folder, string name, string descript, Topic topic)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add media qbicle", user.Id, null, media, user, qbicle, folder, name, descript, topic);

                if (!string.IsNullOrEmpty(media.Name))
                {
                    //DbContext.Entry(media.Type).State = System.Data.Entity.EntityState.Modified;
                    //Media attach
                    var m = new QbicleMedia
                    {
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        Name = name,
                        FileType = media.Type,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic,

                        MediaFolder = folder,
                        Description = descript,
                        IsVisibleInQbicleDashboard = false
                    };
                    var versionFile = new VersionedFile
                    {
                        IsDeleted = false,
                        FileSize = media.Size,
                        UploadedBy = user,
                        UploadedDate = DateTime.UtcNow,
                        Uri = media.UrlGuid,
                        FileType = media.Type
                    };
                    m.VersionedFiles.Add(versionFile);

                    DbContext.Medias.Add(m);
                    DbContext.Entry(m).State = EntityState.Added;
                    return m;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, media, user, qbicle, folder, name, descript, topic);
                return null;
            }
        }
        /// <summary>
        /// Automatically generated folder name social maketing
        /// App (SM) - Campaign Type (SOC) - Random number (001)
        /// </summary>
        /// <param name="qbicleId"></param>
        /// <returns></returns>
        public string AutoGenerateFolderName(int CurrentDomainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Auto generate folder name", null, null, CurrentDomainId);

                var setting = new SocialWorkgroupRules(DbContext).getSettingByDomainId(CurrentDomainId);
                var qbicle = DbContext.Qbicles.Find(setting.SourceQbicle.Id);
                if (qbicle != null)
                {
                    var random = new Random();
                    var randomNumber = random.Next(1, 999);
                    var sFolderName = "SM-BRAND-" + randomNumber.ToString("000");
                    for (int i = 0; i < 20; i++)
                    {
                        var isExist = DbContext.MediaFolders.Any(m => m.Qbicle.Id == qbicle.Id && m.Name == sFolderName);
                        if (!isExist)
                        {
                            return sFolderName;
                        }
                        else
                        {
                            sFolderName = "SM-BRAND-" + random.Next(1, 999).ToString("000");
                            continue;
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, CurrentDomainId);
                return "";
            }
        }
        public List<MediaFolder> GetMediaFoldersByQbicleId(int currentDomainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get media folder by qbicle id", null, null, currentDomainId);


                var setting = new SocialWorkgroupRules(DbContext).getSettingByDomainId(currentDomainId);
                if (setting != null && setting.SourceQbicle != null)
                    return DbContext.MediaFolders.Where(x => x.Qbicle.Id == setting.SourceQbicle.Id).ToList();
                else
                    return new List<MediaFolder>();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentDomainId);
                return new List<MediaFolder>();
            }
        }
        public List<Brand> LoadBrandsByDomainId(int currentDomainId, bool isLoadingHide, int skip, int take, string keyword, ref int totalRecord)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load brands by domain id", null, null, currentDomainId, isLoadingHide, skip, take, keyword, totalRecord);

                var query = DbContext.SMBrands.Where(s => s.Domain.Id == currentDomainId);
                if (!isLoadingHide)
                {
                    query = query.Where(s => !s.IsHidden);
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Name.Contains(keyword) || s.Description.Contains(keyword));
                }

                if (take != 0)
                {
                    totalRecord = 0;
                    return query.OrderByDescending(s => s.CreatedDate).Skip(skip).Take(take).ToList();
                }
                else
                {
                    totalRecord = query.Count();
                    return new List<Brand>();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentDomainId, isLoadingHide, skip, take, keyword, totalRecord);
                return new List<Brand>();
            }
        }
        public ReturnJsonModel ShowOrHideBrand(int id)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show or hide brand", null, null, id);

                var brand = DbContext.SMBrands.FirstOrDefault(s => s.Id == id);
                brand.IsHidden = !brand.IsHidden;
                if (DbContext.Entry(brand).State == EntityState.Detached)
                    DbContext.SMBrands.Attach(brand);
                DbContext.Entry(brand).State = EntityState.Modified;
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnModel;
        }
        public List<Brand> LoadBrandsByDomainId(int currentDomainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load brands by domain id", null, null, currentDomainId);

                var query = DbContext.SMBrands.Where(s => s.Domain.Id == currentDomainId);
                return query.OrderByDescending(s => s.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentDomainId);
                return new List<Brand>();
            }
        }
        public List<BrandProduct> LoadBrandProductsAll(int brandId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load brand products all", null, null, brandId);

                return DbContext.SmBrandProducts.Where(s => s.Brand.Id == brandId).OrderBy(s => s.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, brandId);
                return new List<BrandProduct>();
            }
        }
        public List<BrandProduct> LoadBrandProductsByBrandId(int brandId, int size, int pageSize, string keyword, bool isLoadingHide, ref int totalCount)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load brand products by brand id", null, null, brandId, size, pageSize, keyword, isLoadingHide, totalCount);

                var query = DbContext.SmBrandProducts.Where(s => s.Brand.Id == brandId);
                if (!isLoadingHide)
                {
                    query = query.Where(s => !s.IsHidden);
                }
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(s => s.Name.Contains(keyword) || s.Summary.Contains(keyword));
                totalCount = query.Count();
                return query.OrderByDescending(s => s.CreatedDate).Skip(size).Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, brandId, size, pageSize, keyword, isLoadingHide, totalCount);
                return new List<BrandProduct>();
            }
        }
        public ReturnJsonModel ShowOrHideBrandProduct(int id)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show or hide brand product", null, null, id);

                var brandProduct = DbContext.SmBrandProducts.FirstOrDefault(s => s.Id == id);
                brandProduct.IsHidden = !brandProduct.IsHidden;
                if (DbContext.Entry(brandProduct).State == EntityState.Detached)
                    DbContext.SmBrandProducts.Attach(brandProduct);
                DbContext.Entry(brandProduct).State = EntityState.Modified;
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnModel;
        }
        public ReturnJsonModel SaveBrandProduct(BrandProductCustomModel model, MediaModel media, string userId)
        {
            var returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save brand product", userId, null, model, media);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(DbContext);

                    s3Rules.ProcessingMediaS3(media.UrlGuid);

                }



                var brand = DbContext.SMBrands.Find(model.BrandId);
                if (brand == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_194");
                    return returnModel;
                }
                var setting = new SocialWorkgroupRules(DbContext).getSettingByDomainId(brand.Domain.Id);

                if (setting.SourceQbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_188");
                    return returnModel;
                }


                if (setting.DefaultTopic == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_190");
                    return returnModel;
                }
                var user = DbContext.QbicleUser.Find(userId);
                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var mediaModel = AddMediaQbicle(media, user, setting.SourceQbicle, brand.ResourceFolder, model.Name, model.Summary, setting.DefaultTopic);
                    if (mediaModel != null)
                    {
                        setting.SourceQbicle.Media.Add(mediaModel);
                        DbContext.Entry(mediaModel).State = EntityState.Added;
                    }
                }

                var dbBrandProduct = DbContext.SmBrandProducts.Find(model.Id);
                if (dbBrandProduct != null)
                {
                    if (!string.IsNullOrEmpty(media.UrlGuid))
                    {
                        dbBrandProduct.FeaturedImageUri = media.UrlGuid;
                    }
                    dbBrandProduct.Name = model.Name;
                    dbBrandProduct.Summary = model.Summary;
                    dbBrandProduct.LastUpdatedBy = user;
                    dbBrandProduct.LastUpdateDate = DateTime.UtcNow;
                    if (DbContext.Entry(dbBrandProduct).State == EntityState.Detached)
                        DbContext.SmBrandProducts.Attach(dbBrandProduct);
                    DbContext.Entry(dbBrandProduct).State = EntityState.Modified;
                }
                else
                {
                    dbBrandProduct = new BrandProduct
                    {
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedBy = user,
                        LastUpdateDate = brand.CreatedDate,
                        CreatedBy = user,
                        Name = model.Name,
                        Summary = model.Summary,
                        FeaturedImageUri = media.UrlGuid,
                        Brand = brand
                    };
                    DbContext.SmBrandProducts.Add(dbBrandProduct);
                    DbContext.Entry(dbBrandProduct).State = EntityState.Added;
                }
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
                var select = new Select2CustomeModel();
                if (dbBrandProduct != null)
                {
                    select.id = dbBrandProduct.Id;
                    select.text = dbBrandProduct.Name;
                }
                returnModel.Object = select;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model, media);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }
        public dynamic GetBrandProductById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get brand product by id", null, null, id);

                var brandpro = DbContext.SmBrandProducts.Find(id);
                if (brandpro != null)
                {
                    return new { brandpro.Id, brandpro.Name, brandpro.Summary, featuredimg = brandpro.FeaturedImageUri.ToUriString() };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public List<Attribute> LoadBrandAttrGroupsByBrandId(int brandId, string keyword)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load brand attribute group by brand id", null, null, brandId, keyword);

                var query = DbContext.SMAttributes.Where(s => s.Brand.Id == brandId);
                if (!string.IsNullOrEmpty(keyword))
                    query = DbContext.SMAttributes.Where(s => s.Brand.Id == brandId && (s.Name.Contains(keyword) || s.Summary.Contains(keyword)));
                return query.OrderByDescending(s => s.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, brandId, keyword);
                return new List<Attribute>();
            }
        }
        public ReturnJsonModel SaveBrandAttrGroup(BrandAttributeGroupCustomModel model, MediaModel media, string userId)
        {
            var returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save brand attribute group", userId, null, model, media);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(DbContext);

                    s3Rules.ProcessingMediaS3(media.UrlGuid);

                }

                var brand = DbContext.SMBrands.Find(model.BrandId);
                if (brand == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_194");
                    return returnModel;
                }
                var setting = new SocialWorkgroupRules(DbContext).getSettingByDomainId(brand.Domain.Id);

                if (setting.SourceQbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_188");
                    return returnModel;
                }


                if (setting.DefaultTopic == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_190");
                    return returnModel;
                }

                var user = DbContext.QbicleUser.Find(userId);
                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var mediaModel = AddMediaQbicle(media, user, setting.SourceQbicle, brand.ResourceFolder, model.Name, model.Summary, setting.DefaultTopic);
                    if (mediaModel != null)
                    {
                        setting.SourceQbicle.Media.Add(mediaModel);
                        DbContext.Entry(mediaModel).State = EntityState.Added;
                    }
                }
                var dbBrandAttributeGroup = DbContext.SMAttributeGroups.Find(model.Id);
                if (dbBrandAttributeGroup != null)
                {
                    if (!string.IsNullOrEmpty(media.UrlGuid))
                    {
                        dbBrandAttributeGroup.Icon = media.UrlGuid;
                    }
                    dbBrandAttributeGroup.Name = model.Name;
                    dbBrandAttributeGroup.Summary = model.Summary;
                    dbBrandAttributeGroup.LastUpdatedBy = user;
                    dbBrandAttributeGroup.LastUpdateDate = DateTime.UtcNow;
                    dbBrandAttributeGroup.Attributes.Clear();
                    if (model.Attributes != null && model.Attributes.Count > 0)
                    {
                        foreach (var item in model.Attributes)
                        {
                            var attr = DbContext.SMAttributes.Find(item);
                            if (attr != null)
                            {
                                dbBrandAttributeGroup.Attributes.Add(attr);
                            }
                        }
                    }
                    if (DbContext.Entry(dbBrandAttributeGroup).State == EntityState.Detached)
                        DbContext.SMAttributeGroups.Attach(dbBrandAttributeGroup);
                    DbContext.Entry(dbBrandAttributeGroup).State = EntityState.Modified;
                }
                else
                {
                    AttributeGroup brandAttrGroup = new AttributeGroup();
                    brandAttrGroup.CreatedDate = DateTime.UtcNow;
                    brandAttrGroup.LastUpdatedBy = user;
                    brandAttrGroup.LastUpdateDate = brand.CreatedDate;
                    brandAttrGroup.CreatedBy = user;
                    brandAttrGroup.Name = model.Name;
                    brandAttrGroup.Summary = model.Summary;
                    brandAttrGroup.Icon = media.UrlGuid;
                    brandAttrGroup.Brand = brand;
                    if (model.Attributes != null && model.Attributes.Count > 0)
                    {
                        foreach (var item in model.Attributes)
                        {
                            var attr = DbContext.SMAttributes.Find(item);
                            if (attr != null)
                            {
                                brandAttrGroup.Attributes.Add(attr);
                            }
                        }
                    }
                    DbContext.SMAttributeGroups.Add(brandAttrGroup);
                    DbContext.Entry(brandAttrGroup).State = EntityState.Added;
                }
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model, media);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }
        public dynamic GetBrandAttributegroupById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get brand attribute group by id", null, null, id);

                var brandAttrGroup = DbContext.SMAttributeGroups.Find(id);
                if (brandAttrGroup != null)
                {
                    return new { brandAttrGroup.Id, brandAttrGroup.Name, brandAttrGroup.Summary, IconUrl = brandAttrGroup.Icon.ToUriString(), Attributes = brandAttrGroup.Attributes.Select(s => s.Id).ToArray() };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public List<Models.SalesMkt.Attribute> LoadBrandAttributeByBrandId(int brandId, string keyword, bool isLoadingHide)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load brand attribute by brand id", null, null, brandId, keyword, isLoadingHide);

                var query = DbContext.SMAttributes.Where(s => s.Brand.Id == brandId);
                if (!isLoadingHide)
                    query = query.Where(s => !s.IsHidden);
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(s => s.Name.Contains(keyword) || s.Summary.Contains(keyword));
                return query.OrderByDescending(s => s.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, brandId, keyword, isLoadingHide);
                return new List<Models.SalesMkt.Attribute>();
            }
        }
        public ReturnJsonModel ShowOrHideBrandAttribute(int id)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show or hide brand attribute", null, null, id);

                var brandAttr = DbContext.SMAttributes.FirstOrDefault(s => s.Id == id);
                brandAttr.IsHidden = !brandAttr.IsHidden;
                if (DbContext.Entry(brandAttr).State == EntityState.Detached)
                    DbContext.SMAttributes.Attach(brandAttr);
                DbContext.Entry(brandAttr).State = EntityState.Modified;
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnModel;
        }
        public dynamic GetBrandAttributeById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get brand attribute by id", null, null, id);

                var brandAttr = DbContext.SMAttributes.Find(id);
                if (brandAttr != null)
                {
                    return new { brandAttr.Id, brandAttr.Name, brandAttr.Summary, IconUrl = brandAttr.Icon.ToUriString() };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public ReturnJsonModel SaveBrandAttribute(BrandAttributeCustomModel model, MediaModel media, string userId)
        {
            var returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save brand attribute", userId, null, model, media);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(DbContext);

                    s3Rules.ProcessingMediaS3(media.UrlGuid);

                }

                var brand = DbContext.SMBrands.Find(model.BrandId);
                if (brand == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_194");
                    return returnModel;
                }
                var setting = new SocialWorkgroupRules(DbContext).getSettingByDomainId(brand.Domain.Id);


                if (setting.SourceQbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_188");
                    return returnModel;
                }


                if (setting.DefaultTopic == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_190");
                    return returnModel;
                }
                var user = DbContext.QbicleUser.Find(userId);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var mediaModel = AddMediaQbicle(media, user, setting.SourceQbicle, brand.ResourceFolder, model.Name, model.Summary, setting.DefaultTopic);
                    if (mediaModel != null)
                    {
                        setting.SourceQbicle.Media.Add(mediaModel);
                        DbContext.Entry(mediaModel).State = EntityState.Added;
                    }
                }


                var dbBrandAttribute = DbContext.SMAttributes.Find(model.Id);
                if (dbBrandAttribute != null)
                {
                    if (!string.IsNullOrEmpty(media.UrlGuid))
                    {
                        dbBrandAttribute.Icon = media.UrlGuid;
                    }
                    dbBrandAttribute.Name = model.Name;
                    dbBrandAttribute.Summary = model.Summary;
                    dbBrandAttribute.LastUpdatedBy = user;
                    dbBrandAttribute.LastUpdateDate = DateTime.UtcNow;
                    if (DbContext.Entry(dbBrandAttribute).State == EntityState.Detached)
                        DbContext.SMAttributes.Attach(dbBrandAttribute);
                    DbContext.Entry(dbBrandAttribute).State = EntityState.Modified;
                }
                else
                {
                    Models.SalesMkt.Attribute brandAttr = new Models.SalesMkt.Attribute();
                    brandAttr.CreatedDate = DateTime.UtcNow;
                    brandAttr.LastUpdatedBy = user;
                    brandAttr.LastUpdateDate = brand.CreatedDate;
                    brandAttr.CreatedBy = user;
                    brandAttr.Name = model.Name;
                    brandAttr.Summary = model.Summary;
                    brandAttr.Icon = media.UrlGuid;
                    brandAttr.Brand = brand;
                    DbContext.SMAttributes.Add(brandAttr);
                    DbContext.Entry(brandAttr).State = EntityState.Added;
                }
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model, media);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }
        public ReturnJsonModel SaveValuePropositon(BrandValuePropositionCustomModel model, string userId)
        {

            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save value proposition", userId, null, model);

                var brand = DbContext.SMBrands.Find(model.BrandId);
                if (brand == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_194");
                    return returnModel;
                }
                var user = DbContext.QbicleUser.Find(userId);
                var dbValuePropositon = DbContext.SMValuePropositions.Find(model.Id);
                if (dbValuePropositon != null)
                {
                    dbValuePropositon.BrandProduct = DbContext.SmBrandProducts.Find(model.ProductId);
                    dbValuePropositon.LastUpdateDate = DateTime.UtcNow;
                    dbValuePropositon.LastUpdatedBy = user;
                    dbValuePropositon.WhoWantTo = model.WhoWantTo;
                    dbValuePropositon.By = model.By;
                    dbValuePropositon.Segments.Clear();
                    if (model.CustomerSegment != null)
                    {
                        foreach (var item in model.CustomerSegment)
                        {
                            var segment = DbContext.SMSegments.Find(item);
                            if (segment != null)
                                dbValuePropositon.Segments.Add(segment);
                        }
                    }
                    if (DbContext.Entry(dbValuePropositon).State == EntityState.Detached)
                        DbContext.SMValuePropositions.Attach(dbValuePropositon);
                    DbContext.Entry(dbValuePropositon).State = EntityState.Modified;
                }
                else
                {
                    dbValuePropositon = new ValueProposition();
                    dbValuePropositon.BrandProduct = DbContext.SmBrandProducts.Find(model.ProductId);
                    dbValuePropositon.LastUpdateDate = DateTime.UtcNow;
                    dbValuePropositon.LastUpdatedBy = user;
                    dbValuePropositon.WhoWantTo = model.WhoWantTo;
                    dbValuePropositon.By = model.By;
                    dbValuePropositon.Brand = brand;
                    dbValuePropositon.CreatedBy = user;
                    dbValuePropositon.CreatedDate = dbValuePropositon.LastUpdateDate;
                    dbValuePropositon.Segments.Clear();
                    if (model.CustomerSegment != null)
                    {
                        foreach (var item in model.CustomerSegment)
                        {
                            var segment = DbContext.SMSegments.Find(item);
                            if (segment != null)
                                dbValuePropositon.Segments.Add(segment);
                        }
                    }
                    DbContext.SMValuePropositions.Add(dbValuePropositon);
                    DbContext.Entry(dbValuePropositon).State = EntityState.Added;
                }
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }
        public List<ValueProposition> LoadBrandValuePropositonByPId(int brandId, int productId, bool isLoadingHide)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load brand value proposition by product id", null, null, brandId, productId, isLoadingHide);

                var query = DbContext.SMValuePropositions.Where(s => s.Brand.Id == brandId);
                if (!isLoadingHide)
                    query = query.Where(s => !s.IsHidden);
                if (productId > 0)
                    return query.Where(s => s.BrandProduct.Id == productId).OrderByDescending(o => o.CreatedDate).ToList();
                else
                    return query.OrderByDescending(o => o.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, brandId, productId, isLoadingHide);
                return new List<ValueProposition>();
            }
        }
        public ReturnJsonModel ShowOrHideValueProp(int id)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show or hide value prop", null, null, id);

                var valueProp = DbContext.SMValuePropositions.FirstOrDefault(s => s.Id == id);
                valueProp.IsHidden = !valueProp.IsHidden;
                if (DbContext.Entry(valueProp).State == EntityState.Detached)
                    DbContext.SMValuePropositions.Attach(valueProp);
                DbContext.Entry(valueProp).State = EntityState.Modified;
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnModel;
        }
        public List<ValueProposition> LoadBrandValuePropositonByAll(int brandId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load brand value propsition by brand id", null, null, brandId);

                return DbContext.SMValuePropositions.Where(s => s.Brand.Id == brandId).OrderByDescending(o => o.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, brandId);
                return new List<ValueProposition>();
            }
        }
        public dynamic GetBrandValuePropositionById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get brand value proposition by id", null, null, id);

                var brandpro = DbContext.SMValuePropositions.Find(id);
                if (brandpro != null)
                {
                    return new { brandpro.Id, brandpro.WhoWantTo, brandpro.By, productId = brandpro.BrandProduct.Id, segments = brandpro.Segments.Select(s => s.Id).ToArray() };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public List<Segment> CustomerSegmentsAll(int? domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get all segments by domain id", null, null, domainId);

                if (domainId == null)
                    return DbContext.SMSegments.OrderBy(s => s.Name).ToList();
                return DbContext.SMSegments.Where(s => s.Domain.Id == domainId).OrderBy(s => s.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<Segment>();
            }
        }
        public List<Select2CustomeModel> GetAttributeGroupByBrandId(int brandId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get attribute group by brand id", null, null, brandId);

                return DbContext.SMAttributeGroups.Where(s => s.Brand.Id == brandId).OrderByDescending(s => s.CreatedDate).Select(s => new Select2CustomeModel { id = s.Id, text = s.Name }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, brandId);
                return new List<Select2CustomeModel>();
            }
        }

        public List<BrandCampaignModel> GetListCampaignsInBrand(int brandId, int[] types, string search, int start, int length, ref int totalRecord)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get list campaigns in brand", null, null, brandId, types, search, start, length, totalRecord);

                List<BrandCampaignModel> lstModel = new List<BrandCampaignModel>();
                if (types != null && !types.Contains(0))
                {
                    if (types.Contains(1))
                    {
                        lstModel.AddRange(DbContext.SocialCampaigns.Where(s => s.Brand.Id == brandId && s.Name.ToLower().Contains(search.Trim().ToLower()) && s.CampaignType == CampaignType.Automated).
                            Select(c => new BrandCampaignModel
                            {
                                Id = c.Id,
                                Name = c.Name,
                                NumberOfQueue = DbContext.SocialCampaignQueues.Where(s => s.Post.AssociatedCampaign.Id == c.Id && s.Status == CampaignPostQueueStatus.Scheduled).Count(),
                                NumberOfCompletedPost = DbContext.SocialCampaignQueues.Where(s => s.Post.AssociatedCampaign.Id == c.Id && s.Status == CampaignPostQueueStatus.Sent).Count(),
                                Type = "Automated Social"
                            }));
                    };

                    if (types.Contains(2))
                    {
                        lstModel.AddRange(DbContext.SocialCampaigns.Where(s => s.Brand.Id == brandId && s.Name.ToLower().Contains(search.Trim().ToLower()) && s.CampaignType == CampaignType.Manual).Select(c => new BrandCampaignModel
                        {
                            Id = c.Id,
                            Name = c.Name,
                            NumberOfQueue = DbContext.SocialCampaignQueues.Where(s => s.Post.AssociatedCampaign.Id == c.Id && s.Status == CampaignPostQueueStatus.Scheduled).Count(),
                            NumberOfCompletedPost = DbContext.SocialCampaignQueues.Where(s => s.Post.AssociatedCampaign.Id == c.Id && s.Status == CampaignPostQueueStatus.Sent).Count(),
                            Type = "Manual Social"
                        }));
                    };

                    if (types.Contains(3))
                    {
                        lstModel.AddRange(DbContext.EmailCampaigns.Where(e => e.Brand.Id == brandId && e.Name.ToLower().Contains(search.Trim().ToLower())).
                                            Select(c => new BrandCampaignModel
                                            {
                                                Id = c.Id,
                                                Name = c.Name,
                                                NumberOfQueue = DbContext.EmailCampaignQueues.Where(s => s.Email.Campaign.Id == c.Id && s.Status == CampaignEmailQueueStatus.Scheduled).Count(),
                                                NumberOfCompletedPost = DbContext.EmailCampaignQueues.Where(s => s.Email.Campaign.Id == c.Id && s.Status == CampaignEmailQueueStatus.Sent).Count(),
                                                Type = "Email"
                                            }));
                    };
                }

                totalRecord = lstModel.Count();

                return lstModel.OrderBy(l => l.Name).Skip(start).Take(length).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, brandId, types, search, start, length, totalRecord);
                totalRecord = 0;
                return new List<BrandCampaignModel>();
            }

        }
    }
}
