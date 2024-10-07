using Qbicles.BusinessRules.BusinessRules.Social;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.ProductSearch;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader.Resources;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Model.TB_Column;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.C2C
{
    public class C2CRules
    {
        private readonly ApplicationDbContext dbContext;

        public C2CRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public C2CRules()
        {
            dbContext = new ApplicationDbContext();
        }
        public PaginationResponse FindPeople(FindPeopleRequest request)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, request);
                #region query filter
                IQueryable<PeopleInfoModel> query = null;

                //filter all the contacts that form the relationship
                var queryUsersInRelationShip = dbContext.C2CQbicles
                    .Where(s => !s.IsHidden && s.Customers.Any(u => u.Id == request.CurrentUserId))
                    .SelectMany(s => s.Customers.Select(u => new PeopleInfoModel
                    {
                        DomainKey = s.Id.ToString(),
                        UserId = u.Id,
                        AvatarUri = u.ProfilePic,
                        FullName = (u.DisplayUserName == null ? u.Forename + " " + u.Surname : u.DisplayUserName),
                        HasRemoved = s.RemovedForUsers.Any(r => r.Id == request.CurrentUserId),
                        Type = 1,
                        HasConnected = true,
                        HasDefaultB2CRelationshipManager = true
                    }));

                #region Build IQueryable Individual
                // 0: Show all types,2: Only individuals
                if (request.ContactType == FindContactType.All || request.ContactType == FindContactType.Individuals)
                {
                    if (request.PeopleType == FindPeopleType.Affiliates)//Limit to my affiliates
                    {
                        query = dbContext.Domains.Where(s => s.Users.Any(u => u.Id == request.CurrentUserId))
                            .SelectMany(s => s.Users.Select(u => new PeopleInfoModel
                            {
                                DomainKey = s.Id.ToString(),
                                UserId = u.Id,
                                AvatarUri = u.ProfilePic,
                                FullName = (u.DisplayUserName == null ? u.Forename + " " + u.Surname : u.DisplayUserName),
                                HasRemoved = false,
                                Type = 2,
                                HasConnected = queryUsersInRelationShip.Any(x => !x.HasRemoved && x.UserId != request.CurrentUserId && x.UserId == u.Id),
                                HasDefaultB2CRelationshipManager = true
                            }));
                        query = query.Where(p => p.UserId != request.CurrentUserId).Distinct();
                    }
                    else//Show full public list
                    {
                        query = dbContext.QbicleUser.Select(u => new PeopleInfoModel
                        {
                            DomainKey = "",
                            UserId = u.Id,
                            AvatarUri = u.ProfilePic,
                            FullName = (u.DisplayUserName == null ? u.Forename + " " + u.Surname : u.DisplayUserName),
                            HasRemoved = false,
                            Type = 2,
                            HasConnected = queryUsersInRelationShip.Any(x => !x.HasRemoved && x.UserId != request.CurrentUserId && x.UserId == u.Id),
                            HasDefaultB2CRelationshipManager = true
                        });
                        query = query.Where(p => p.UserId != request.CurrentUserId).Distinct();
                    }
                }
                #endregion
                #region Build IQueryable Business 
                // 0: Show all types
                if (request.ContactType == FindContactType.All)
                {
                    var b2cQbicles = new List<int>();
                    var qbicles = dbContext.B2CQbicles
                        .Where(s => !s.IsHidden
                        && s.Customer.Id == request.CurrentUserId
                        && s.Domain.Status != QbicleDomain.DomainStatusEnum.Closed).ToList();
                    qbicles.ForEach(q =>
                    {
                        if (q.RemovedForUsers.Count == 0)
                            b2cQbicles.Add(q.Business.Id);
                        else if (q.RemovedForUsers.Any(r => r.Id != request.CurrentUserId))
                            b2cQbicles.Add(q.Business.Id);
                    });
                    query = query.Union(from d in dbContext.B2BProfiles
                                        where d.IsDisplayedInB2CListings
                                        //&& d.DefaultB2CRelationshipManagers.Any()
                                        && d.Domain.Id != request.CurrentBusinessId
                                        && d.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
                                        select new PeopleInfoModel
                                        {
                                            DomainKey = d.Domain.Id.ToString(),
                                            UserId = d.Id.ToString(),
                                            AvatarUri = d.LogoUri,
                                            FullName = d.BusinessName,
                                            HasRemoved = false,
                                            Type = 1,
                                            HasConnected = b2cQbicles.Contains(d.Domain.Id),
                                            HasDefaultB2CRelationshipManager = d.DefaultB2CRelationshipManagers.Any()
                                        }
                                        );
                }
                else if (request.ContactType == FindContactType.Businesses)//1: Only businesses
                {
                    var b2cQbicles = new List<int>();
                    var qbicles = dbContext.B2CQbicles.Where(s => !s.IsHidden && s.Customer.Id == request.CurrentUserId).ToList();
                    qbicles.ForEach(q =>
                    {
                        if (q.RemovedForUsers.Count == 0)
                            b2cQbicles.Add(q.Business.Id);
                        else if (q.RemovedForUsers.Any(r => r.Id != request.CurrentUserId))
                            b2cQbicles.Add(q.Business.Id);
                    });

                    query = from d in dbContext.B2BProfiles
                            where d.IsDisplayedInB2CListings
                            //&& d.DefaultB2CRelationshipManagers.Any()
                            && d.Domain.Id != request.CurrentBusinessId
                            && d.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
                            select new PeopleInfoModel
                            {
                                DomainKey = d.Domain.Id.ToString(),
                                UserId = d.Id.ToString(),
                                AvatarUri = d.LogoUri,
                                FullName = d.BusinessName,
                                HasRemoved = false,
                                Type = 1,
                                HasConnected = b2cQbicles.Contains(d.Domain.Id),
                                HasDefaultB2CRelationshipManager = d.DefaultB2CRelationshipManagers.Any()
                            };
                }
                #endregion

                if (!string.IsNullOrEmpty(request.keyword))
                    query = query.Where(s => s.FullName.Contains(request.keyword));
                #endregion
                response.totalNumber = query.Count();
                response.items = query.OrderBy(s => s.FullName).Skip((request.pageNumber - 1) * request.pageSize).Take(request.pageSize).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
            }
            return response;
        }

        public DataTablesResponse FindProducts([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequestModel, string keySearch, string countryName, List<int> listBrandIds, List<string> listProductTag, ref int totalRecord, int start = 0, int length = 10)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, dtRequestModel, keySearch, countryName, listBrandIds, listProductTag);

                var apiBaseUrl = ConfigManager.ApiGetDocumentUri;

                bool isIncludeProductWithoutBrand = false;
                bool isIncludeProductWithoutTagName = false;
                bool isShowAllProductsWithouAdditionalInfos = false;
                //some traderitems don't have AdditionalInfos => If no filter => show them
                if (listBrandIds.Contains(0) && listProductTag.FirstOrDefault().Equals(""))
                {
                    isShowAllProductsWithouAdditionalInfos = true;
                }

                //Filter include non-tagname
                if (listProductTag.FirstOrDefault().Equals(""))
                {
                    isIncludeProductWithoutTagName = true;
                }

                //Filter include non-brand
                if (listBrandIds.Contains(0))
                {
                    listBrandIds.Remove(0);
                    isIncludeProductWithoutBrand = true;
                }

                // Query
                #region Filtering
                // Filter
                var query = from variant in dbContext.PosVariants
                            .Where(x =>
                                        x.IsActive
                                       &&
                                       x.IsDefault
                                       && !x.CategoryItem.Category.Menu.IsDeleted
                                       && x.CategoryItem.Category.IsVisible
                                       && x.CategoryItem.Category.Menu.IsPublished
                                       &&
                                            (
                                            //Case 1 : Nobrand && no tag
                                            isShowAllProductsWithouAdditionalInfos 
                                            ||
                                            //Case 2 : Has both brands && tags
                                                (
                                                       x.TraderItem.AdditionalInfos.Any(t => t.Type == AdditionalInfoType.Brand && listBrandIds.Contains(t.Id)) 
                                                    && x.TraderItem.AdditionalInfos.Any(y => y.Type == AdditionalInfoType.ProductTag && listProductTag.Contains(y.Name))
                                                )
                                            ||
                                            //Case 3 : No tag
                                            isIncludeProductWithoutTagName &&
                                                (
                                                    x.TraderItem.AdditionalInfos.Any(t => t.Type == AdditionalInfoType.Brand && listBrandIds.Contains(t.Id))
                                                )
                                            ||
                                            //Case 4 : No brand
                                            isIncludeProductWithoutBrand &&
                                                (
                                                    x.TraderItem.AdditionalInfos.Any(y => y.Type == AdditionalInfoType.ProductTag && listProductTag.Contains(y.Name))
                                                )
                                            )
                                    )
                                       
                                // Hide products don't meet b2bProfile requirement
                            join b2bProfile in dbContext.B2BProfiles
                            .Where(b => b.IsDisplayedInB2CListings
                                        && b.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
                                        && b.DefaultB2CRelationshipManagers.Any())
                                    on variant.TraderItem.Domain.Id equals b2bProfile.Domain.Id
                            join currencySetting in dbContext.CurrencySettings
                            on  variant.TraderItem.Domain.Id equals currencySetting.Domain.Id
                            select new FeaturedProductModel()
                            {
                                B2BProfileItem = b2bProfile,
                                CatItemId = variant.CategoryItem.Id,
                                CatItemImageUri = apiBaseUrl + variant.CategoryItem.ImageUri,
                                CatItemName = variant.CategoryItem.Name,
                                Price = variant.Price.GrossPrice,
                                SKU = variant.TraderItem.SKU,
                                AssociatedTraderItem = variant.TraderItem,
                                AssociatedCatalog = variant.CategoryItem.Category.Menu,
                                AssociatedCurrencySetting = currencySetting,
                                DomainId = b2bProfile.Domain.Id,
                                AdditionalInfos = variant.TraderItem.AdditionalInfos
                            };

                if (!string.IsNullOrEmpty(keySearch))
                {
                    query = query.Where(p => p.CatItemName.ToLower().Contains(keySearch.ToLower()));
                }

                if (!string.IsNullOrEmpty(countryName) && countryName != "0")
                {
                    query = query
                        .Where(e => e.B2BProfileItem.AreasOperation.Any(x => x.Name == countryName));
                }
                #endregion

                #region Sorting
                query = query.OrderBy(p => p.CatItemName);
                #endregion

                totalRecord = query.Count();

                #region Pagination
                var lstFeaturedProducts = query.Skip(start).Take(length).ToList();
                #endregion

                var dataJson = lstFeaturedProducts.Select(p => new
                {
                    CategoryItemId = p.CatItemId,
                    CategoryItemName = p.CatItemName,
                    CatItemImageUri = p.CatItemImageUri,
                    SKU = p.SKU,
                    B2BProfileBusinessName = p.B2BProfileItem.BusinessName,
                    B2BProfileBusinessImg = p.B2BProfileItem.LogoUri.ToDocumentUri(),
                    BusinessId = p.B2BProfileItem.Id,
                    Price = p.Price.ToCurrencySymbol(p.AssociatedCurrencySetting),
                    CatalogId = p.AssociatedCatalog.Id,
                    CatalogKey = p.AssociatedCatalog.Key,
                    CatalogName = p.AssociatedCatalog.Name,
                    DomainKey = p.DomainId.Encrypt(),
                    DomainId = p.DomainId
                });

                return new DataTablesResponse(dtRequestModel.Draw, dataJson, totalRecord, totalRecord);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, dtRequestModel, keySearch, countryName, listBrandIds);
                return new DataTablesResponse(dtRequestModel.Draw, null, 0, 0);
            }
        }

        public object FindProductsData(string keySearch, string countryName, List<int> listBrandIds, List<string> listProductTag, ref int totalRecord, int start = 0, int length = 10)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, keySearch, countryName, listBrandIds, listProductTag);

                //this logic lone from DataTablesResponse FindProducts( because it's in release 2.0 and not sure completed)
                var apiBaseUrl = ConfigManager.ApiGetDocumentUri;

                //update from FindProducts
                //Filter include non-tagname
                bool isIncludeProductWithoutBrand = false;
                bool isIncludeProductWithoutTagName = false;
                bool isShowAllProductsWithouAdditionalInfos = false;
                //some traderitems don't have AdditionalInfos => If no filter => show them
                if (listBrandIds.Contains(0) && listProductTag.IsNullOrEmpty())
                {
                    isShowAllProductsWithouAdditionalInfos = true;
                }

                //Filter include non-tagname
                if (listProductTag.IsNullOrEmpty())
                {
                    isIncludeProductWithoutTagName = true;
                }

                //Filter include non-brand
                if (listBrandIds.Contains(0))
                {
                    isIncludeProductWithoutBrand = true;
                    listBrandIds.Remove(0);
                }

                // Query
                #region Filtering
                // Filter
                var query = from variant in dbContext.PosVariants
                            .Where(x =>
                                        x.IsActive
                                       &&
                                       x.IsDefault
                                       && !x.CategoryItem.Category.Menu.IsDeleted
                                       && x.CategoryItem.Category.IsVisible
                                       && x.CategoryItem.Category.Menu.IsPublished
                                       &&
                                            (
                                            //Case 1 : Nobrand && no tag
                                            isShowAllProductsWithouAdditionalInfos
                                            ||
                                                //Case 2 : Has both brands && tags
                                                (
                                                       x.TraderItem.AdditionalInfos.Any(t => t.Type == AdditionalInfoType.Brand && listBrandIds.Contains(t.Id))
                                                    && x.TraderItem.AdditionalInfos.Any(y => y.Type == AdditionalInfoType.ProductTag && listProductTag.Contains(y.Name))
                                                )
                                            ||
                                            //Case 3 : No tag
                                            isIncludeProductWithoutTagName &&
                                                (
                                                    x.TraderItem.AdditionalInfos.Any(t => t.Type == AdditionalInfoType.Brand && listBrandIds.Contains(t.Id))
                                                )
                                            ||
                                            //Case 4 : No brand
                                            isIncludeProductWithoutBrand &&
                                                (
                                                    x.TraderItem.AdditionalInfos.Any(y => y.Type == AdditionalInfoType.ProductTag && listProductTag.Contains(y.Name))
                                                )
                                            )
                                    )

                                // Hide products don't meet b2bProfile requirement
                            join b2bProfile in dbContext.B2BProfiles
                            .Where(b => b.IsDisplayedInB2CListings
                                        && b.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
                                        && b.DefaultB2CRelationshipManagers.Any())
                                    on variant.TraderItem.Domain.Id equals b2bProfile.Domain.Id
                            join currencySetting in dbContext.CurrencySettings
                            on variant.TraderItem.Domain.Id equals currencySetting.Domain.Id
                            select new FeaturedProductModel()
                            {
                                B2BProfileItem = b2bProfile,
                                CatItemId = variant.CategoryItem.Id,
                                CatItemImageUri = apiBaseUrl + b2bProfile.LogoUri,
                                ProductImageUri = apiBaseUrl + variant.CategoryItem.ImageUri,
                                CatItemName = variant.CategoryItem.Name,
                                Price = variant.Price.GrossPrice,
                                SKU = variant.TraderItem.SKU,
                                AssociatedTraderItem = variant.TraderItem,
                                AssociatedCurrencySetting = currencySetting,
                                AssociatedCatalog = variant.CategoryItem.Category.Menu,
                                //DisplayOrder is not use anymore
                                DisplayOrder = 1,
                                DomainId = variant.TraderItem.Domain.Id,
                                AdditionalInfos = variant.TraderItem.AdditionalInfos,
                                //Country = fpProductItem.B2BProfileItem.a
                            };

                if (!string.IsNullOrEmpty(keySearch))
                {
                    query = query.Where(p => p.CatItemName.ToLower().Contains(keySearch.ToLower()));
                }

                if (!string.IsNullOrEmpty(countryName) && countryName != "0")
                {
                    query = query
                        .Where(e => e.B2BProfileItem.AreasOperation.Any(x => x.Name == countryName));
                }
                #endregion

                #region Sorting
                query = query.OrderBy(p => p.CatItemName);
                #endregion

                totalRecord = query.Count();

                #region Pagination
                var lstFeaturedProducts = query.Skip(start * length).Take(length).ToList();
                #endregion

                var dataJson = lstFeaturedProducts.Select(p => new
                {
                    CategoryItemId = p.CatItemId,
                    CategoryItemName = p.CatItemName,
                    CatItemImageUri = p.CatItemImageUri,
                    ProductImageUri = p.ProductImageUri,
                    SKU = p.SKU,
                    B2BProfileBusinessName = p.B2BProfileItem.BusinessName,
                    BusinessId = p.B2BProfileItem.Id,
                    Price = p.Price.ToCurrencySymbol(p.AssociatedCurrencySetting),
                    CatalogId = p.AssociatedCatalog.Id,
                    CatalogKey = p.AssociatedCatalog.Key,
                    CatalogName = p.AssociatedCatalog.Name,
                    DomainKey = p.DomainId.Encrypt(),
                    DomainId = p.DomainId,
                    Country = p.B2BProfileItem.AreasOperation.IsNullOrEmpty() ? null : p.B2BProfileItem.AreasOperation.FirstOrDefault().Name,
                    Brand = p.AdditionalInfos.FirstOrDefault(e => e.Type == AdditionalInfoType.Brand)?.Name,
                    Tags = p.AdditionalInfos.IsNullOrEmpty() ? new List<string>() : p.AdditionalInfos.Where(e => e.Type == AdditionalInfoType.ProductTag).Select(n => n.Name).ToList()
                }).ToList();

                return dataJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, keySearch, countryName, listBrandIds);
                return null;
            }
        }

        /// <summary>
        /// This is function create a associate of  B2C or C2C
        /// </summary>
        /// <param name="currentUserId">This is current UserId</param>
        /// <param name="linkId">- Type=1 => linkId is a the b2bProfileId, Type=2 => linkId is a the UserId </param>
        /// <param name="Type">1: Businesses, 2: Individual</param>
        /// <returns></returns>
        public ReturnJsonModel ConnectC2C(string currentUserId, string linkId, int Type, string originatingConnectionId = "")
        {
            var returnJson = new ReturnJsonModel() { result = false };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, currentUserId, linkId);
                    var currentUser = dbContext.QbicleUser.FirstOrDefault(e => e.Id == currentUserId);
                    //Create a connection Customer and Customer
                    if (Type == 2)
                    {
                        var linkUser = dbContext.QbicleUser.FirstOrDefault(e => e.Id == linkId);
                        var existedC2CQbicle = dbContext.C2CQbicles.FirstOrDefault(s => !s.IsHidden && s.Customers.Any(u => u.Id == currentUserId) && s.Customers.Any(u => u.Id == linkId));
                        if (existedC2CQbicle != null)
                        {
                            if (existedC2CQbicle.RemovedForUsers != null && existedC2CQbicle.RemovedForUsers.Any(p => p.Id == currentUserId))
                            {
                                existedC2CQbicle.RemovedForUsers.Remove(currentUser);
                                currentUser.RemovedQbicle.Remove(existedC2CQbicle);
                                dbContext.Entry(existedC2CQbicle).State = EntityState.Modified;
                                dbContext.Entry(currentUser).State = EntityState.Modified;
                                dbContext.SaveChanges();
                                dbTransaction.Commit();
                            }

                            returnJson.result = true;
                            returnJson.Object = existedC2CQbicle.Id;
                            returnJson.Object2 = existedC2CQbicle.Status.ToString();
                            //returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                            return returnJson;
                        }
                        var qbicle = new C2CQbicle
                        {
                            Status = CommsStatus.Pending,
                            Domain = dbContext.SystemDomains.FirstOrDefault(s => s.Name == SystemDomainConst.CUSTOMER2CUSTOMER && s.Type == SystemDomainType.C2C),
                            Source = currentUser,
                            Name = $"{HelperClass.GetFullNameOfUser(currentUser)} & {HelperClass.GetFullNameOfUser(linkUser)}",
                            Description = $"{SystemDomainConst.CUSTOMER2CUSTOMER} - {HelperClass.GetFullNameOfUser(currentUser)} & {HelperClass.GetFullNameOfUser(linkUser)}",
                            LogoUri = HelperClass.QbicleLogoDefault,
                            IsHidden = false,
                            OwnedBy = currentUser,
                            StartedBy = currentUser,
                            Manager = currentUser,
                            StartedDate = DateTime.UtcNow
                        };
                        qbicle.LastUpdated = qbicle.StartedDate;
                        qbicle.Members = qbicle.Customers;
                        qbicle.Customers.Add(currentUser);
                        qbicle.Customers.Add(linkUser);
                        dbContext.C2CQbicles.Add(qbicle);
                        dbContext.Entry(qbicle).State = EntityState.Added;
                        returnJson.result = dbContext.SaveChanges() > 0;
                        returnJson.Object = qbicle.Id;
                        returnJson.Object2 = qbicle.Status.ToString();
                        var relationshipLog = new CustomerRelationshipLog
                        {
                            QbicleId = qbicle.Id,
                            Status = CommsStatus.Pending,
                            UserId = currentUser.Id,
                            CreatedDate = DateTime.UtcNow
                        };
                        dbContext.CustomerRelationshipLogs.Add(relationshipLog);
                        dbContext.Entry(relationshipLog).State = EntityState.Added;
                        dbContext.SaveChanges();

                        // Create Notification
                        var notification = new ActivityNotification
                        {
                            OriginatingConnectionId = originatingConnectionId,
                            DomainId = qbicle.Domain.Id,
                            QbicleId = qbicle.Id,
                            AppendToPageName = ApplicationPageName.Qbicle,
                            EventNotify = NotificationEventEnum.C2CConnectionIssued,
                            CreatedByName = HelperClass.GetFullName(currentUser),
                            ObjectById = linkId,
                            ReminderMinutes = 0,
                            CreatedById = currentUser.Id,
                            Id = qbicle.Id
                        };
                        new NotificationRules(dbContext).NotificationOnC2CConnectionIssued(notification);
                        dbTransaction.Commit();

                    }
                    else//Create a connection Business and Customer
                    {
                        var b2bProfileId = int.Parse(linkId);
                        var b2bProfile = dbContext.B2BProfiles.FirstOrDefault(e => e.Id == b2bProfileId);
                        var qbicleInDb = dbContext.B2CQbicles.FirstOrDefault(s => !s.IsHidden && s.Business.Id == b2bProfile.Domain.Id && s.Customer.Id == currentUser.Id);
                        if (qbicleInDb != null)
                        {
                            if (qbicleInDb.RemovedForUsers != null && qbicleInDb.RemovedForUsers.Any(p => p.Id == currentUserId))
                            {
                                qbicleInDb.RemovedForUsers.Remove(currentUser);
                                currentUser.RemovedQbicle.Remove(qbicleInDb);
                                dbContext.Entry(currentUser).State = EntityState.Modified;
                                dbContext.Entry(qbicleInDb).State = EntityState.Modified;
                            }

                            qbicleInDb.Status = CommsStatus.Approved;
                            dbContext.SaveChanges();
                            dbTransaction.Commit();
                            returnJson.result = true;
                            returnJson.Object = qbicleInDb.Id;
                            return returnJson;
                        }

                        //if (!b2bProfile.DefaultB2CRelationshipManagers.Any())
                        //{
                        //    returnJson.msg = ResourcesManager._L("ERROR_NOT_FOUND_RELATIONSHIPMANAGER", "B2C");
                        //    returnJson.result = false;
                        //    return returnJson;
                        //}


                        var b2CQbicleManager = currentUser;

                        //if the user who is connecting is one of the relationship managers
                        if (b2bProfile.DefaultB2CRelationshipManagers.Any(e => e.Id == currentUserId))
                        {
                            //if there is more than one relationship manager
                            if (b2bProfile.DefaultB2CRelationshipManagers.Count > 1)
                            {
                                //use the relationship managers who is not the connecting manager
                                b2CQbicleManager = b2bProfile.DefaultB2CRelationshipManagers.FirstOrDefault();
                            }
                            //else//if there is only one relationship manager i.e. the connecting manager
                            //{
                            //    returnJson.msg = ResourcesManager._L("ERROR_CONNECT_B2C_MANAGER", "B2C");
                            //    returnJson.result = false;
                            //    return returnJson;
                            //}
                        }

                        var qbicle = new B2CQbicle
                        {
                            Status = CommsStatus.Approved,
                            Domain = dbContext.SystemDomains.FirstOrDefault(s => s.Name == SystemDomainConst.BUSINESS2CUSTOMER && s.Type == SystemDomainType.B2C),
                            Business = b2bProfile.Domain,
                            Customer = currentUser,
                            Name = $"{b2bProfile.BusinessName} & {currentUser.GetFullName()}",
                            Description = $"{SystemDomainConst.BUSINESS2CUSTOMER} - {b2bProfile.BusinessName} & {currentUser.GetFullName()}",
                            LogoUri = HelperClass.QbicleLogoDefault,
                            IsHidden = false,
                            OwnedBy = currentUser,
                            StartedBy = currentUser,
                            Manager = b2CQbicleManager,
                            StartedDate = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow
                        };

                        if (b2bProfile.DefaultB2CRelationshipManagers.Any())
                            qbicle.Members.AddRange(b2bProfile.DefaultB2CRelationshipManagers);

                        if (!qbicle.Members.Any(s => s.Id == currentUser.Id))
                            qbicle.Members.Add(currentUser);


                        dbContext.B2CQbicles.Add(qbicle);
                        dbContext.Entry(qbicle).State = EntityState.Added;
                        dbContext.SaveChanges();

                        new TopicRules(dbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicle.Id);

                        returnJson.result = true;
                        returnJson.Object = qbicle.Id;

                        var relationshipLog = new CustomerRelationshipLog
                        {
                            QbicleId = qbicle.Id,
                            Status = CommsStatus.Pending,
                            UserId = currentUser.Id,
                            CreatedDate = DateTime.UtcNow
                        };
                        dbContext.CustomerRelationshipLogs.Add(relationshipLog);
                        dbContext.Entry(relationshipLog).State = EntityState.Added;
                        dbContext.SaveChanges();

                        //Send notification - B2C Connection is created
                        var notification = new ActivityNotification()
                        {
                            OriginatingConnectionId = originatingConnectionId,
                            DomainId = qbicle.Domain.Id,
                            QbicleId = qbicle.Id,
                            AppendToPageName = ApplicationPageName.Qbicle,
                            EventNotify = NotificationEventEnum.B2CConnectionCreated,
                            CreatedByName = HelperClass.GetFullName(currentUser),
                            ObjectById = linkId,
                            ReminderMinutes = 0,
                            CreatedById = currentUser.Id,
                            Id = b2bProfileId
                        };
                        new NotificationRules(dbContext).NotificationOnB2CConnectionAccepted(notification);
                        dbTransaction.Commit();
                        // Need to add a Trader Contact here if possible
                        new OrderProcessingHelper(dbContext).GetCreateTraderContactFromUserInfo(currentUser, b2bProfile.Domain, SalesChannelEnum.B2C);
                        returnJson.result = true;


                        new HighlightPostRules(dbContext).ChangeHiddingHLDomainPostStatus(b2bProfile.Domain.Id, currentUserId, 2);
                    }


                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId, linkId);
                    returnJson.result = false;
                    returnJson.msg = ex.Message;
                    dbTransaction.Rollback();
                }
            }
            return returnJson;
        }

        /// <summary>
        /// This is the function of C2C who like (The loveheart in the UI) the QBicle
        /// </summary>
        /// <param name="qId"></param>
        /// <param name="currentUserId">This is current UserId</param>
        /// <param name="linkId">- Type=1 => linkId is a the b2bProfileId, Type=2 => linkId is a the UserId </param>
        /// <param name="type">1: Businesses, 2: Individual</param>
        /// <param name="isFavorite">is Favorite True/False</param>
        /// <returns></returns>
        public ReturnJsonModel SetLikeBy(int qId, string currentUserId, string linkId, int type, bool isFavorite,string originatingCreationId="", string originatingConnectionId = "")
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId, currentUserId, linkId, type, isFavorite);
                if (type == 1)
                {
                    var b2bProfileId = Convert.ToInt32(linkId);
                    var b2bprofile = dbContext.B2BProfiles.FirstOrDefault(e => e.Id == b2bProfileId);
                    var b2cqbicle = dbContext.B2CQbicles.FirstOrDefault(e => e.Id == qId);
                    if (b2cqbicle != null)
                    {
                        if (b2cqbicle.Customer.Id != currentUserId || b2cqbicle.Business.Id != b2bprofile.Domain.Id)
                        {
                            returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                            return returnJson;
                        }
                        var user = dbContext.QbicleUser.FirstOrDefault(e => e.Id == currentUserId);
                        if (isFavorite && !b2cqbicle.LikedBy.Any(u => u.Id == currentUserId) && user != null)
                        {
                            b2cqbicle.LikedBy.Add(user);
                        }
                        else
                        {
                            b2cqbicle.LikedBy.Remove(user);
                        }
                    }
                }
                else
                {
                    var c2cqbicle = dbContext.C2CQbicles.FirstOrDefault(e => e.Id == qId);
                    if (c2cqbicle != null)
                    {
                        if (!c2cqbicle.Customers.Any(u => u.Id == currentUserId))
                        {
                            returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                            return returnJson;
                        }
                        var user = dbContext.QbicleUser.FirstOrDefault(e => e.Id == linkId);
                        if (isFavorite && !c2cqbicle.LikedBy.Any(u => u.Id == linkId) && user != null)
                        {
                            c2cqbicle.LikedBy.Add(user);
                        }
                        else
                        {
                            c2cqbicle.LikedBy.Remove(user);
                        }

                    }
                }
                dbContext.SaveChanges();
                returnJson.result = true;

                //TODO send notification here
                //var currentUser = dbContext.QbicleUser.FirstOrDefault(e => e.Id == currentUserId);
                //var notificationReceiver = c2cqbicle.Customers.FirstOrDefault(p => p.Id != currentUserId);

                //var notification = new ActivityNotification()
                //{
                //    OriginatingConnectionId = originatingConnectionId,
                //    OriginatingCreationId = originatingCreationId,
                //    DomainId = c2cqbicle.Domain.Id,
                //    QbicleId = c2cqbicle.Id,
                //    AppendToPageName = ApplicationPageName.Qbicle,
                //    EventNotify = NotificationEventEnum.C2CConnectionAccepted,
                //    CreatedByName = HelperClass.GetFullName(currentUser),
                //    ObjectById = notificationReceiver.Id,
                //    ReminderMinutes = 0,
                //    CreatedById = currentUser.Id,
                //    Id = c2cqbicle.Id
                //};
                //new NotificationRules(dbContext).NotificationOnC2CConnectionAccepted(notification);
            }
            catch (Exception ex)
            {
                returnJson.result = false;
                returnJson.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId, currentUserId, linkId, type, isFavorite);
            }
            return returnJson;
        }
        /// <summary>
        /// This is the function update status the QBicle
        /// </summary>
        /// <param name="qId">This is qbicleid</param>
        /// <param name="currentUserId">This is current UserId</param>
        /// <param name="status">This indicates the status of the communications between the parties in the Qbicle</param>
        /// <param name="type">1: Businesses, 2: Individual</param>
        /// <returns></returns>
        public ReturnJsonModel SetStatusBy(int qId, string currentUserId, CommsStatus status, string originatingConnectionId = "")
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId, status);
                var c2cqbicle = dbContext.C2CQbicles.FirstOrDefault(e => e.Id == qId);
                if (c2cqbicle != null)
                {
                    if (!c2cqbicle.Customers.Any(u => u.Id == currentUserId))
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                        return returnJson;
                    }


                    var relationshipLog = new CustomerRelationshipLog
                    {
                        QbicleId = c2cqbicle.Id,
                        Status = status,
                        UserId = currentUserId,
                        CreatedDate = DateTime.UtcNow
                    };

                    c2cqbicle.Status = status;
                    switch (status)
                    {
                        case CommsStatus.Pending:
                            break;
                        case CommsStatus.Approved:
                            c2cqbicle.Blocker = null;
                            break;
                        case CommsStatus.Blocked:
                            c2cqbicle.Blocker = dbContext.QbicleUser.FirstOrDefault(e => e.Id == currentUserId);
                            relationshipLog.BlockedByUserId = c2cqbicle.Blocker.Id;
                            break;
                        case CommsStatus.Cancel:
                            c2cqbicle.IsHidden = true;
                            break;
                    }

                    dbContext.CustomerRelationshipLogs.Add(relationshipLog);
                    dbContext.Entry(relationshipLog).State = EntityState.Added;
                }
                var saveResult = dbContext.SaveChanges();

                //Send notification on accepting connection
                if (saveResult > 0 && status == CommsStatus.Approved)
                {
                    var currentUser = dbContext.QbicleUser.FirstOrDefault(e => e.Id == currentUserId);
                    var notificationReceiver = c2cqbicle.Customers.FirstOrDefault(p => p.Id != currentUserId);

                    var notification = new ActivityNotification()
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        DomainId = c2cqbicle.Domain.Id,
                        QbicleId = c2cqbicle.Id,
                        AppendToPageName = ApplicationPageName.Qbicle,
                        EventNotify = NotificationEventEnum.C2CConnectionAccepted,
                        CreatedByName = HelperClass.GetFullName(currentUser),
                        ObjectById = notificationReceiver.Id,
                        ReminderMinutes = 0,
                        CreatedById = currentUser.Id,
                        Id = c2cqbicle.Id
                    };
                    new NotificationRules(dbContext).NotificationOnC2CConnectionAccepted(notification);
                }

                returnJson.result = true;
            }
            catch (Exception ex)
            {
                returnJson.result = false;
                returnJson.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId, status);
            }
            return returnJson;
        }
        public C2CQbicle GetC2CQbicleById(int qId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId);
                return dbContext.C2CQbicles.FirstOrDefault(e => e.Id == qId); ;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId);
                return new C2CQbicle();
            }
        }
        public ReturnJsonModel RemoveC2CQbicleById(int qId, string currentUserId)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId, currentUserId);
                var c2cqbicle = dbContext.C2CQbicles.FirstOrDefault(e => e.Id == qId);
                if (c2cqbicle != null)
                {
                    if (!c2cqbicle.Customers.Any(u => u.Id == currentUserId))
                    {
                        returnJson.msg = ResourcesManager._L("ERROR_MSG_28");
                        return returnJson;
                    }
                    c2cqbicle.IsHidden = true;
                }
                dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                returnJson.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId, currentUserId);
            }
            return returnJson;
        }

        public void C2CUiSetting(string page, string userId, C2CSearchQbicleModel searchModel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "C2CUiSetting", userId, null, page, userId, searchModel);

                var currentUser = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);

                List<UiSetting> uis = new List<UiSetting>
                {
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = C2CStoreUiSettingsConst.ORDERBY, Value = searchModel.Orderby.ToString() },
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = C2CStoreUiSettingsConst.INCLUDEBLOCKED, Value = searchModel.IncludeBlocked.ToString().ToLower() },
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = C2CStoreUiSettingsConst.CONTACTTYPE, Value = searchModel.ContactType.ToString() },
                    new UiSetting() { CurrentPage = page, CurrentUser = currentUser, Key = C2CStoreUiSettingsConst.ONLYSHOWFAVOURITES, Value = searchModel.OnlyShowFavourites.ToString().ToLower() }
                };

                new QbicleRules(dbContext).StoredUiSettings(uis);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, page, userId, searchModel);
            }

        }

        public DataTablesResponse GetMyOrders(IDataTablesRequest requestModel, string currentUserId, string keyword, string daterange, int salesChannel = 0, bool isHideCompleted = true)
        {
            try
            {
                var currentUser = dbContext.QbicleUser.FirstOrDefault(e => e.Id == currentUserId);
                int totalcount = 0;
                #region Filters
                var query = from od in dbContext.TradeOrders.Where(s => s.Customer.Id == currentUserId
                            && s.SellingDomain != null && s.BuyingDomain == null)
                            join pr in dbContext.B2BProfiles on od.SellingDomain.Id equals pr.Domain.Id
                            select new MyOrder
                            {
                                OrderId = od.Id,
                                FullRef = od.OrderReference.FullRef,
                                Placed = od.CreateDate,
                                Status = od.OrderStatus,
                                BusinessId = od.SellingDomain.Id,
                                BusinessLogoUri = pr.LogoUri,
                                BusinessName = pr.BusinessName,
                                Channel = od.SalesChannel,
                                Invoice = od.Invoice,
                                Payments = od.Payments
                            };
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.FullRef.Contains(keyword) || s.BusinessName.Contains(keyword));
                }
                if (!string.IsNullOrEmpty(daterange))
                {
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    daterange.ConvertDaterangeFormat(currentUser.DateFormat, currentUser.Timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    query = query.Where(s => s.Placed >= startDate && s.Placed < endDate);
                }
                if (salesChannel > 0)
                {
                    query = query.Where(s => s.Channel == (SalesChannelEnum)salesChannel);
                }
                if (isHideCompleted)
                {
                    var completeOrder = query.Where(s => (s.Status == TradeOrderStatusEnum.Processed || s.Status == TradeOrderStatusEnum.ProcessedWithProblems) && ((s.Invoice.TotalInvoiceAmount <= s.Payments.Sum(t => t.Amount)) || s.Invoice.TotalInvoiceAmount == 0));
                    query = from mo in query
                            where !(from ob in completeOrder select ob.OrderId).Contains(mo.OrderId) select mo;
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
                        case "FullRef":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "FullRef" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "BusinessName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "BusinessName" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Placed":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Placed" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Channel":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Channel" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        default:
                            orderByString = "OrderId asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "OrderId asc" : orderByString);
                #endregion
                #region Paging
                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                #endregion
                var dataJson = list.Select(s => new
                {
                    s.OrderId,
                    s.FullRef,
                    Placed = s.Placed.ConvertTimeFromUtc(currentUser.Timezone).ToString("ddnn MMMM yyyy, hh:mmtt", true),
                    Status = s.GetDescription(),
                    s.BusinessId,
                    BusinessLogoUri = s.BusinessLogoUri.ToDocumentUri(Enums.FileTypeEnum.Image, "T"),
                    s.BusinessName,
                    Channel = s.Channel.ToString()
                });
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="keyword"></param>
        /// <param name="contactType">3 pending, 2 business, 1 user</param>
        /// <param name="allNum"></param>
        /// <param name="favouriteNum"></param>
        /// <param name="requestNum"></param>
        /// <param name="sentNum"></param>
        /// <param name="blockedNum"></param>
        public void GetCommunityTalkNumByType(string currentUserId, string keyword, int contactType, ref int allNum,
            ref int favouriteNum, ref int requestNum, ref int sentNum, ref int blockedNum)
        {

            var query = BuildQueryB2CC2CModel(new CommunityParameter
            {
                UserId = currentUserId,
                ContactType = contactType,
                Keyword = keyword,
            });




            allNum = query.Where(s => s.Status != CommsStatus.Blocked).Count();

            favouriteNum = query.Where(s => s.Status != CommsStatus.Blocked && s.LikedBy.Any(u => (s.Type == 2 && u.Id != currentUserId) || (s.Type == 1 && u.Id == currentUserId))).Count();

            requestNum = query.Where(p => p.Status == CommsStatus.Pending && (p.Type == 2 && p.SourceUser.Id != currentUserId)).Count();

            sentNum = query.Where(p => p.Status == CommsStatus.Pending && p.Type == 2 && p.SourceUser.Id == currentUserId).Count();

            blockedNum = query.Where(p => p.Status == CommsStatus.Blocked).Count();

        }


        /// <summary>
        /// Get all c2c qbicle by current user
        /// </summary>
        /// <param name="currentUserId">current user Id</param>
        /// <param name="keyword">Search contacts...</param>
        /// <param name="orderby">
        /// 0: Order by latest activity(Default)
        /// 1: Order by forename A-Z
        /// 2: Order by forename Z-A
        /// 3: Order by surname A-Z
        /// 4: Order by surname Z-A
        /// 5: Order by date added
        /// </param>
        /// <param name="contactType">
        /// 0: Show all contact types(Default)
        /// 1: Only individuals
        /// 2: Only pending
        /// </param>
        /// <param name="onlyShowFavourites">Only show favourites</param>
        /// <param name="includeBlocked">Include blocked users</param>
        /// <returns>List C2CQbicle</returns>
        public List<B2CC2CModel> GetC2CQbicles(out int totalPage, CommunityParameter parameter)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, parameter.UserId);

                var query = BuildQueryB2CC2CModel(parameter);

                #region Filters                

                if (parameter.ShownAll)
                {
                    query = query.Where(s => s.Status != CommsStatus.Blocked);
                }
                else if (parameter.ShownFavourite)
                {
                    query = query.Where(s => s.Status != CommsStatus.Blocked && s.LikedBy.Any(u => (s.Type == 2 && u.Id != parameter.UserId) || (s.Type == 1 && u.Id == parameter.UserId)));
                }
                else if (parameter.ShownRequest)
                {
                    query = query.Where(p => p.Status == CommsStatus.Pending && (p.Type == 2 && p.SourceUser.Id != parameter.UserId));
                }
                else if (parameter.ShownSent)
                {
                    query = query.Where(p => p.Status == CommsStatus.Pending && p.Type == 2 && p.SourceUser.Id == parameter.UserId);
                }
                else if (parameter.ShownBlocked)
                {
                    query = query.Where(p => p.Status == CommsStatus.Blocked);
                }


                var list = new List<B2CC2CModel>();
                var totalNumber = query.Count();
                int pageSize = 10;
                totalPage = ((totalNumber % pageSize) == 0) ? (totalNumber / pageSize) : (totalNumber / pageSize) + 1;


                switch (parameter.OrderBy)
                {
                    case 1://Order by forename A-Z
                        query = query.OrderBy(s => s.Type == 2 ? s.LinkUsers.Where(u => u.Id != parameter.UserId).FirstOrDefault().Forename : s.LinkBusiness.BusinessName);
                        break;
                    case 2://Order by forename Z-A
                        query = query.OrderByDescending(s => s.Type == 2 ? s.LinkUsers.Where(u => u.Id != parameter.UserId).FirstOrDefault().Forename : s.LinkBusiness.BusinessName);
                        break;
                    case 3://Order by surname A-Z
                        query = query.OrderBy(s => s.Type == 2 ? s.LinkUsers.Where(u => u.Id != parameter.UserId).FirstOrDefault().Surname : s.LinkBusiness.BusinessName);
                        break;
                    case 4://Order by surname Z-A
                        query = query.OrderByDescending(s => s.Type == 2 ? s.LinkUsers.Where(u => u.Id != parameter.UserId).FirstOrDefault().Surname : s.LinkBusiness.BusinessName);
                        break;
                    case 5://Order by date added
                        query = query.OrderByDescending(s => s.Type == 2 ? s.LinkUsers.Where(u => u.Id != parameter.UserId).FirstOrDefault().DateBecomesMember : s.LinkBusiness.CreatedDate);
                        break;
                    default://Order by latest activity
                        query = query.OrderByDescending(s => s.LastUpdated);
                        break;
                }

                list = query.Skip(parameter.PageIndex * pageSize).Take(pageSize).ToList();
                #endregion

                return list;
            }
            catch (Exception ex)
            {
                totalPage = 0;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, parameter.UserId);
                return new List<B2CC2CModel>();
            }
        }

        private IQueryable<B2CC2CModel> BuildQueryB2CC2CModel(CommunityParameter parameter)
        {
            var query = from c in dbContext.CQbicles
                        join c2c in dbContext.C2CQbicles on c.Id equals c2c.Id into dept
                        from c2c in dept.DefaultIfEmpty()
                        join b2c in dbContext.B2CQbicles on c.Id equals b2c.Id into dept1
                        from b2c in dept1.DefaultIfEmpty()
                        join b2bprofile in dbContext.B2BProfiles on b2c.Business.Id equals b2bprofile.Domain.Id into dept2
                        from b2bprofile in dept2.DefaultIfEmpty()
                        where !c.IsHidden
                        && ((c2c != null && c2c.Customers.Any(u => u.Id == parameter.UserId)) || (b2c != null && b2c.Customer.Id == parameter.UserId))
                        && (!c.RemovedForUsers.Any(p => p.Id == parameter.UserId))
                        && (b2c.Business.Status != QbicleDomain.DomainStatusEnum.Closed)
                        select new B2CC2CModel
                        {
                            Id = c.Id,
                            QbicleId = c.Id,
                            LikedBy = c.LikedBy,
                            LinkBusiness = b2bprofile,
                            LinkUsers = c2c.Customers,
                            Status = c.Status,
                            Type = (c2c == null ? 1 : 2),//1:businesses, 2:individuals
                            LastUpdated = c.LastUpdated,
                            SourceUser = c2c.Source,
                            NotViewed = (c2c == null ? !(b2c.CustomerViewed != null && b2c.CustomerViewed == true) : c2c.NotViewedBy.Any(x => x.Id == parameter.UserId))
                        };

            #region Filters
            if (parameter.ContactType == 3)//2: Only pending
            {
                query = query.Where(s => s.Status == CommsStatus.Pending);
            }
            else if (parameter.ContactType == 2)
            {
                query = query.Where(s => s.LinkUsers.Any());
            }
            else if (parameter.ContactType == 1)
            {
                query = query.Where(s => s.LinkBusiness != null);
            }

            if (!string.IsNullOrEmpty(parameter.Keyword))
            {
                query = query.Where(s =>
                s.LinkUsers.Any(u => u.Forename.Contains(parameter.Keyword) || u.Surname.Contains(parameter.Keyword) || u.DisplayUserName.Contains(parameter.Keyword))
                || (s.LinkBusiness != null && s.LinkBusiness.BusinessName.Contains(parameter.Keyword)));
            }
            #endregion

            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qId"></param>
        /// <param name="type">1-Business, 2-Individual</param>
        /// <param name="isViewed"></param>
        /// <returns></returns>
        public ReturnJsonModel UpdateC2CCommunityTalkViewedStatus(int qId, int type, string currentUserId, bool isViewed = true)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, qId);
                // Business
                // We need to return the Shopping ability - Have at least one public catalog
                if (type == 1)
                {
                    var b2cQbicle = dbContext.B2CQbicles.Find(qId);
                    if (b2cQbicle != null)
                    {
                        b2cQbicle.CustomerViewed = isViewed;
                        dbContext.Entry(b2cQbicle).State = EntityState.Modified;

                        var lstTemp = dbContext.PosMenus.FirstOrDefault(p => (p.Location.Domain.Id == b2cQbicle.Business.Id || (p.Domain != null && p.Domain.Id == b2cQbicle.Business.Id)));

                        var lstCatalog = dbContext.PosMenus
                            .Where(p => (p.Location.Domain.Id == b2cQbicle.Business.Id || (p.Domain != null && p.Domain.Id == b2cQbicle.Business.Id))
                                        && p.SalesChannel == SalesChannelEnum.B2C
                                        && p.IsPublished == true);

                        var b2bProfile = dbContext.B2BProfiles.FirstOrDefault(p => p.Domain.Id == b2cQbicle.Business.Id);
                        var isB2CActive = false;
                        if (b2bProfile != null)
                        {
                            isB2CActive = b2bProfile.IsDisplayedInB2CListings && b2bProfile.DefaultB2CRelationshipManagers.Any();
                        }

                        returnJson.Object = new
                        {
                            HasPublicCatalog = lstCatalog.Any(),
                            IsB2CActive = isB2CActive
                        };
                    }
                    else
                    {
                        returnJson.Object = new
                        {
                            HasPublicCatalog = false,
                            IsB2CActive = false
                        };
                    }
                }
                // Individual
                else if (type == 2)
                {
                    var c2cQbicle = dbContext.C2CQbicles.Find(qId);
                    if (c2cQbicle != null)
                    {
                        if (c2cQbicle.NotViewedBy == null)
                            c2cQbicle.NotViewedBy = new List<ApplicationUser>();
                        var currentUser = dbContext.QbicleUser.Find(currentUserId);
                        if (currentUser.NotViewedQbicle == null)
                            currentUser.NotViewedQbicle = new List<C2CQbicle>();

                        if (currentUser != null)
                        {
                            if (isViewed)
                            {
                                c2cQbicle.NotViewedBy.Remove(currentUser);
                                currentUser.NotViewedQbicle.Remove(c2cQbicle);
                            }
                            else
                            {
                                c2cQbicle.NotViewedBy.Add(currentUser);
                                currentUser.NotViewedQbicle.Add(c2cQbicle);
                            }
                        }
                        dbContext.Entry(c2cQbicle).State = EntityState.Modified;
                    }
                }
                dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qId);
            }
            return returnJson;
        }

        public ReturnJsonModel RemoveQbicleForUser(string currentUserId, int qId)
        {
            ReturnJsonModel rsObj = new ReturnJsonModel() { actionVal = 2, result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, currentUserId, qId);

                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var qbicle = dbContext.Qbicles.Find(qId);

                if (currentUser != null && qbicle != null)
                {
                    currentUser.RemovedQbicle.Add(qbicle);
                    qbicle.RemovedForUsers.Add(currentUser);

                    dbContext.Entry(currentUser).State = EntityState.Modified;
                    dbContext.Entry(qbicle).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                rsObj.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId, qId);
                rsObj.msg = ResourcesManager._L("ERROR_MSG_5");
            }
            return rsObj;
        }

        /// <summary>
        /// Get list of BusinessCategory where BusinessCategory.Profiles.Count > 0
        /// I.e.show only those BusinessCategory records that are used by profiles
        /// </summary>
        /// <returns></returns>
        public List<BaseModel> GetBusinessCategoriesProfile()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);

                return dbContext.BusinessCategories.Where(e => e.Profiles.Any()).Select(n => new BaseModel { Id = n.Id, Name = n.Name }).OrderBy(p => p.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }
        /// <summary>
        /// Area of country
        /// </summary>
        /// <returns></returns>
        public List<string> GetAreaOfOperations()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);

                return dbContext.B2BAreasOfOperation.Where(e => e.Profile.IsDisplayedInB2CListings).Select(n => n.Name).Distinct().OrderBy(p => p).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }
        /// products list in Feature products Commuity
        /// 
        public List<FeaturedProduct> GetPositionFeatureProductsList()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);
                return dbContext.FeaturedProducts.OrderBy(p => p.DisplayOrder).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }
        public List<Product> GetProductsList()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);
                return dbContext.FPProducts.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }

        public List<FeaturedProductImage> GetProductsImageList()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);
                return dbContext.FPImages.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }

        public List<FeaturedProduct> CustomObject()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);

                var listProduct = dbContext.FPProducts.ToList();
                var listImage = dbContext.FPImages.ToList();
                var totallist = listProduct.Cast<FeaturedProduct>().Concat(listImage.Cast<FeaturedProduct>()).ToList();
                return totallist;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }



        public List<FeaturedStore> GetFeaturedStores()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);
                return dbContext.FeaturedStores.Include(p => p.Domain).OrderBy(e => e.DisplayOrder).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }


        public List<AdditionalInfo> GetBrandsMaster()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null);
                //Some traderItems don't belong to any brand;
                var noBrand = new AdditionalInfo { Id = 0, Name = "No brand" };

                var lstBrands = dbContext.TraderItems
                    .SelectMany(p => p.AdditionalInfos)
                    .Where(p => p.Type == AdditionalInfoType.Brand)
                    .Distinct()
                    .OrderBy(p => p.Name).ToList();
                lstBrands.Insert(0,noBrand);
                return lstBrands;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }

        public List<CommunityFeturedProductModel> GetCustomizedCommunityFeaturedProduct()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod());

                var apiBaseUrl = ConfigManager.ApiGetDocumentUri;

                var productQuery = (from fProduct in dbContext.FPProducts
                                    from variant in dbContext.PosVariants
                                         .Where(x => x.CategoryItem.Category.Menu.Id == fProduct.Catalog.Id
                                                     && x.IsDefault
                                                     && x.IsActive
                                                     && x.TraderItem.Id == fProduct.TraderItem.Id)
                                    join business in dbContext.B2BProfiles on fProduct.Domain.Id equals business.Domain.Id
                                    join currencySetting in dbContext.CurrencySettings on fProduct.Domain.Id equals currencySetting.Domain.Id
                                    where !fProduct.Catalog.IsDeleted
                                            && variant.Price != null
                                            && variant.Price.GrossPrice != null
                                    select new CommunityFeturedProductModel()
                                    {
                                        BusinessLogo = apiBaseUrl + business.LogoUri,
                                        BusinessName = business.BusinessName,
                                        CategoryItemName = variant.CategoryItem.Name,
                                        Id = fProduct.Id,
                                        LogoImageUri = apiBaseUrl + variant.CategoryItem.ImageUri,
                                        Price = variant.Price.GrossPrice,
                                        PriceStr = "",
                                        Type = FeaturedType.Product,
                                        TypeName = FeaturedType.Product.ToString(),
                                        AssociatedCurrencySetting = currencySetting,
                                        AssociatedCatalog = fProduct.Catalog,
                                        AssociatedCatalogKey = "",
                                        FeaturedImageURL = "",
                                        DisplayOrder = fProduct.DisplayOrder
                                    }).ToList();

                productQuery.ForEach(p =>
                {
                    p.PriceStr = p.Price.ToCurrencySymbol(p.AssociatedCurrencySetting);

                    p.AssociatedCurrencySetting = null;

                    p.AssociatedCatalogKey = p.AssociatedCatalog?.Key ?? "";
                    p.AssociatedCatalog = null;
                });

                var imageQuery = (from fImage in dbContext.FPImages
                                  select new CommunityFeturedProductModel()
                                  {
                                      BusinessLogo = "",
                                      BusinessName = "",
                                      CategoryItemName = "",
                                      Id = fImage.Id,
                                      LogoImageUri = apiBaseUrl + fImage.FeaturedImageUri,
                                      Price = 0,
                                      PriceStr = "",
                                      Type = FeaturedType.Image,
                                      TypeName = FeaturedType.Image.ToString(),
                                      AssociatedCurrencySetting = null,
                                      AssociatedCatalog = null,
                                      AssociatedCatalogKey = "",
                                      FeaturedImageURL = fImage.URL,
                                      DisplayOrder = fImage.DisplayOrder
                                  }).ToList();

                var result = (productQuery.Union(imageQuery)).OrderBy(x => x.DisplayOrder).ToList();
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new List<CommunityFeturedProductModel>();
            }
        }

        public List<CommunityFeturedProductMicroModel> GetCustomizedCommunityFeaturedProductMicro()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod());

                var apiBaseUrl = ConfigManager.ApiGetDocumentUri;

                var productQuery = (from fProduct in dbContext.FPProducts
                                    from variant in dbContext.PosVariants
                                         .Where(x => x.CategoryItem.Category.Menu.Id == fProduct.Catalog.Id
                                                     && x.IsDefault
                                                     && x.IsActive
                                                     && x.TraderItem.Id == fProduct.TraderItem.Id)
                                    join business in dbContext.B2BProfiles on fProduct.Domain.Id equals business.Domain.Id
                                    join currencySetting in dbContext.CurrencySettings on fProduct.Domain.Id equals currencySetting.Domain.Id
                                    where !fProduct.Catalog.IsDeleted
                                            && variant.Price != null
                                            && variant.Price.GrossPrice != null
                                    select new CommunityFeturedProductMicroModel()
                                    {
                                        B2BProfileItem = business,
                                        AdditionalInfos = variant.TraderItem.AdditionalInfos,
                                        BusinessLogo = apiBaseUrl + business.LogoUri,
                                        BusinessName = business.BusinessName,
                                        CategoryItemName = variant.CategoryItem.Name,
                                        Id = fProduct.Id,
                                        //DomainKey = business.Domain.Key,
                                        LogoImageUri = apiBaseUrl + variant.CategoryItem.ImageUri,
                                        Price = variant.Price.GrossPrice,
                                        PriceStr = "",
                                        Type = FeaturedType.Product,
                                        TypeName = FeaturedType.Product.ToString(),
                                        AssociatedCurrencySetting = currencySetting,
                                        Catalog = fProduct.Catalog,
                                        AssociatedCatalogKey = "",
                                        FeaturedImageURL = "",
                                        DisplayOrder = fProduct.DisplayOrder
                                    }).ToList();

                productQuery.ForEach(p =>
                {
                    p.PriceStr = p.Price.ToCurrencySymbol(p.AssociatedCurrencySetting);

                    p.AssociatedCurrencySetting = null;

                    p.AssociatedCatalogKey = p.Catalog?.Key ?? "";
                    p.CatalogName = p.Catalog.Name;
                    p.CatalogId = p.Catalog.Id;
                    p.Catalog = null;

                    p.DomainKey = p.B2BProfileItem.Domain.Key;
                });

                var imageQuery = (from fImage in dbContext.FPImages
                                  select new CommunityFeturedProductModel()
                                  {
                                      //DomainKey= fImage.Domain.Key,
                                      BusinessLogo = "",
                                      BusinessName = "",
                                      CategoryItemName = "",
                                      Id = fImage.Id,
                                      LogoImageUri = apiBaseUrl + fImage.FeaturedImageUri,
                                      Price = 0,
                                      PriceStr = "",
                                      Type = FeaturedType.Image,
                                      TypeName = FeaturedType.Image.ToString(),
                                      AssociatedCurrencySetting = null,
                                      AssociatedCatalog = null,
                                      CatalogName = "",
                                      CatalogId = 0,
                                      DomainKey = "",
                                      AssociatedCatalogKey = "",
                                      FeaturedImageURL = fImage.URL,
                                      DisplayOrder = fImage.DisplayOrder
                                  }).ToList();


                var products = productQuery.Select(e => new CommunityFeturedProductMicroModel
                {
                    DomainKey = e.DomainKey,
                    BusinessLogo = e.BusinessLogo,
                    BusinessName = e.BusinessName,
                    CategoryItemName = e.CategoryItemName,
                    Id = e.Id,
                    LogoImageUri = e.LogoImageUri,
                    Price = e.Price,
                    PriceStr = e.PriceStr,
                    Type = e.Type,
                    TypeName = e.TypeName,
                    AssociatedCurrencySetting = e.AssociatedCurrencySetting,
                    Catalog = e.Catalog,
                    CatalogName = e.CatalogName,
                    CatalogId = e.CatalogId,
                    AssociatedCatalogKey = e.AssociatedCatalogKey,
                    FeaturedImageURL = e.FeaturedImageURL,
                    DisplayOrder = e.DisplayOrder,
                    Country = e.B2BProfileItem.AreasOperation.FirstOrDefault().Name,
                    Brand = e.AdditionalInfos?.FirstOrDefault(a => a.Type == AdditionalInfoType.Brand)?.Name ?? "",
                    Tags = e.AdditionalInfos?.Where(a => a.Type == AdditionalInfoType.ProductTag).Select(n => n.Name).ToList() ?? new List<string>()
                });
                var images = imageQuery.Select(e => new CommunityFeturedProductMicroModel
                {
                    DomainKey = e.DomainKey,
                    BusinessLogo = "",
                    BusinessName = "",
                    CategoryItemName = "",
                    Id = e.Id,
                    LogoImageUri = e.LogoImageUri,
                    Price = 0,
                    PriceStr = "",
                    Type = e.Type,
                    TypeName = e.TypeName,
                    AssociatedCurrencySetting = e.AssociatedCurrencySetting,
                    Catalog = e.AssociatedCatalog,
                    CatalogName = e.CatalogName,
                    CatalogId = e.CatalogId,
                    AssociatedCatalogKey = "",
                    FeaturedImageURL = e.FeaturedImageURL,
                    DisplayOrder = e.DisplayOrder,
                    Country = "",
                    Brand = "",
                    Tags = new List<string>()
                });

                var result = products.Union(images).OrderBy(x => x.DisplayOrder).ToList();
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new List<CommunityFeturedProductMicroModel>();
            }
        }
        public List<AdditionalInfo> GetProductTagTagify()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod());
                var lstProductTag = dbContext.TraderItems
                    .SelectMany(p => p.AdditionalInfos)
                    .Where(e => e.Type == AdditionalInfoType.ProductTag).Distinct()
                    .OrderBy(p => p.Name).ToList();
                return lstProductTag;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
    }
}
