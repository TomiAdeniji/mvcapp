using Qbicles.BusinessRules.BusinessRules.PayStack;
using Qbicles.BusinessRules.C2C;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Loyalty
{
    public class PromotionRules
    {
        private ApplicationDbContext dbContext;

        public PromotionRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="promotionKey"></param>
        /// <returns></returns>
        public LoyaltyPromotionAndTypeModel GetPromotionByKey(string promotionKey)
        {
            //Init model
            var model = new LoyaltyPromotionAndTypeModel();

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, promotionKey);

                //Get all promotion types types
                model.LoyaltyPromotionTypes = dbContext.PromotionTypes
                    .Where(t => t.IsActive)
                    .OrderBy(x => x.Rank).ToList();

                if (!string.IsNullOrEmpty(promotionKey))
                {
                    var promotionId = int.Parse(promotionKey.Decrypt());
                    model.LoyaltyPromotion = dbContext.Promotions.Include(x => x.PlanType).FirstOrDefault(x => x.Id == promotionId);
                }

                return model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, promotionKey);
                return model;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="promotionKey"></param>
        /// <returns></returns>
        public LoyaltyBulkDealPromotionAndTypeModel GetBulkDealPromotionByKey(string promotionKey)
        {
            //Init model
            var model = new LoyaltyBulkDealPromotionAndTypeModel();

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, promotionKey);

                if (!string.IsNullOrEmpty(promotionKey))
                {
                    var promotionId = int.Parse(promotionKey.Decrypt());
                    model.LoyaltyBulkDealPromotion = dbContext.BulkDealPromotions.Find(promotionId);
                }

                return model;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, promotionKey);
                return model;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="promotionKey"></param>
        /// <returns></returns>
        public List<PromotionModel> GetBulkDealPromotionsSlim(PromotionFilterModel filterModel)
        {
            try
            {
                return dbContext.BulkDealPromotions.OrderBy(x => x.Name).ToList().Select(s => new PromotionModel
                {
                    PromotionKey = s.Key,
                    Name = s.Name,
                    Description = s.Description,
                    DisplayDate = s.DisplayDate,
                    VoucherExpiryDate = s.BulkDealVoucherInfo.VoucherExpiryDate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    IsHalted = s.IsHalted,
                    IsDraft = s.IsDraft,
                    IsArchived = s.IsArchived,
                    FeaturedImageUri = s.FeaturedImageUri.ToDocumentUri(),
                    TotalClaimed = s.OptInVouchers.Count(e => e.OptInBusiness != null && !e.IsOptIn)
                }).ToList();
            }
            catch (Exception ex)
            {
                return new List<PromotionModel>();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="promotionKey"></param>
        /// <returns></returns>
        public List<PromotionModel> GetBulkDealPromotions(BulkDealSearchParameter filterModel)
        {
            var currentDate = DateTime.UtcNow;
            var startDate = DateTime.MinValue;
            var endDate = DateTime.UtcNow;
            try
            {
                var queryBulkDealPromotions = dbContext.BulkDealPromotions.Include(p => p.BulkDealVoucherInfo);

                if (!string.IsNullOrEmpty(filterModel.Name))
                {
                    queryBulkDealPromotions = queryBulkDealPromotions.Where(s => s.Name.Contains(filterModel.Name) || s.Description.Contains(filterModel.Name));
                }

                if (!string.IsNullOrEmpty(filterModel.DateRange))
                {
                    if (!filterModel.DateRange.Contains('-'))
                        filterModel.DateRange += "-";
                    filterModel.DateRange.ConvertDaterangeFormat(filterModel.DateFormat, filterModel.Timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    queryBulkDealPromotions = queryBulkDealPromotions.Where(s => s.StartDate >= startDate && s.StartDate < endDate);
                }

                if (filterModel.Type != 0)
                {
                    switch (filterModel.Type)
                    {
                        case 1:
                            queryBulkDealPromotions = queryBulkDealPromotions.Where(s => s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived);
                            break;

                        case 2:
                            queryBulkDealPromotions = queryBulkDealPromotions.Where(s => s.StartDate > currentDate && s.EndDate < currentDate && s.IsHalted && s.IsArchived);
                            break;
                    }
                }
                return queryBulkDealPromotions.OrderBy(x => x.Name).ToList().Select(s => new PromotionModel
                {
                    PromotionKey = s.Key,
                    Name = s.Name,
                    Description = s.Description,
                    DisplayDate = s.DisplayDate,
                    VoucherExpiryDate = s.BulkDealVoucherInfo.VoucherExpiryDate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    IsHalted = s.IsHalted,
                    IsDraft = s.IsDraft,
                    IsArchived = s.IsArchived,
                    FeaturedImageUri = s.FeaturedImageUri.ToDocumentUri(),
                    TotalClaimed = s.OptInVouchers.Count(e => e.OptInBusiness != null && !e.IsOptIn)
                }).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="promotionKey"></param>
        /// <returns></returns>
        public LoyaltyPromotionType GetPromotionTypeByKey(string promotionKey)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, promotionKey);

                if (string.IsNullOrEmpty(promotionKey)) return new LoyaltyPromotionType();

                var promotionId = int.Parse(promotionKey.Decrypt());

                return dbContext.PromotionTypes.Find(promotionId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, promotionKey);
                return new LoyaltyPromotionType();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="promotionKey"></param>
        /// <returns></returns>
        public DataTablesResponse GetParticipatingDomainsInPromotionType(IDataTablesRequest requestModel, string promotionKey)
        {
            try
            {
                //Init total records
                int totalRecords = 0;

                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, promotionKey);

                if (string.IsNullOrEmpty(promotionKey))
                    return new DataTablesResponse(requestModel.Draw, "", totalRecords, totalRecords);

                var promotionId = int.Parse(promotionKey.Decrypt());

                var queryParticipatingDomain = dbContext.Promotions.Include(x => x.PlanType)
                                                                    .Include(x => x.Domain)
                                                                    .Where(x => x.PlanType.Id == promotionId)
                                                                    .Select(x => new
                                                                    {
                                                                        BusinessName = x.Domain.Name,
                                                                        DateStarted = x.StartDate,
                                                                        ElapseDate = x.EndDate,
                                                                    }).AsQueryable();

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "BusinessName":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "BusinessName" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "DateStarted":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "DateStarted" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "ElapseDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "ElapseDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "BusinessName asc";
                            break;
                    }
                }

                queryParticipatingDomain = queryParticipatingDomain.OrderBy(orderByString == string.Empty ? "Name asc" : orderByString);

                #endregion Sorting

                #region Paging

                var dataJson = queryParticipatingDomain.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                //Calculate total records
                totalRecords = queryParticipatingDomain.Count();

                //Return response
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, promotionKey);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public List<Select2Option> getAllBusinessHasPromotion()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null);
                var query = dbContext.Promotions.Where(p => p.Domain.Status != QbicleDomain.DomainStatusEnum.Closed).Select(s => s.Domain).Distinct();
                return query.ToList().Select(s => new Select2Option { id = s.Id.Encrypt(), text = s.Id.BusinesProfile()?.BusinessName ?? s.Name }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<Select2Option>();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public List<LoyaltyBulkDealPromotion> GetBulkDealCreation()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null);
                var query = dbContext.BulkDealPromotions.Include(x => x.BulkDealVoucherInfo).Where(p => !p.IsHalted && !p.IsArchived).Distinct();
                return query.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<LoyaltyBulkDealPromotion>();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public PaginationResponse GetPublishPromotions(PromotionPublishFilterModel filterModel)
        {
            var response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, filterModel);

                var currentDomainId = string.IsNullOrEmpty(filterModel.businessKey) ? 0 : int.Parse(filterModel.businessKey.Decrypt());
                var currentDate = DateTime.UtcNow;
                var queryPublishPromotions = dbContext.Promotions.Where(s => !s.IsArchived
                                                                        && !s.IsDraft
                                                                        && !s.IsHalted
                                                                        && s.DisplayDate <= currentDate
                                                                        && s.Domain.Status != QbicleDomain.DomainStatusEnum.Closed)
                                                                 .Include(s => s.PlanType)
                                                                 .Include(s => s.Audience);
                if (currentDomainId > 0)
                {
                    queryPublishPromotions = queryPublishPromotions.Where(s => s.Domain.Id == currentDomainId);
                }
                if (currentDomainId > 0 && filterModel.locationIds != null)
                {
                    queryPublishPromotions = queryPublishPromotions.Where(s => s.VoucherInfo.Locations.Any(x => filterModel.locationIds.Contains(x.Id)));
                }
                if (!string.IsNullOrEmpty(filterModel.keyword))
                {
                    queryPublishPromotions = queryPublishPromotions.Where(s => s.Name.Contains(filterModel.keyword) || s.Description.Contains(filterModel.keyword));
                }
                if (filterModel.isMyFavourites)
                {
                    queryPublishPromotions = queryPublishPromotions.Where(s => s.LikingUsers.Any(u => u.Id == filterModel.currentUserId));
                }

                //Save query changes
                var filteredPublishPromotions = queryPublishPromotions.ToList();

                //Get total count of records
                response.totalNumber = filterModel.isLoadTotalRecord ? filteredPublishPromotions.Count() : 0;

                //Set pageSize to zero for skipping purposes
                if (response.totalNumber < filterModel.pageSize)
                    filterModel.pageSize = 0;

                //Prepare promotion items
                response.items = filteredPublishPromotions.OrderBy(s => s.PlanType.Rank)
                        .Skip(filterModel.pageSize).Take(filterModel.pageNumber)
                        .Select(s => new PromotionModel
                        {
                            PromotionKey = s.Key,
                            Name = s.Name,
                            Description = s.Description,
                            DisplayDate = s.DisplayDate.ConvertTimeFromUtc(filterModel.timezone),
                            VoucherExpiryDate = s.VoucherInfo?.VoucherExpiryDate?.ConvertTimeFromUtc(filterModel.timezone) ?? s.EndDate.ConvertTimeFromUtc(filterModel.timezone),
                            StartDate = s.StartDate.ConvertTimeFromUtc(filterModel.timezone),
                            EndDate = s.EndDate.ConvertTimeFromUtc(filterModel.timezone),
                            IsHalted = s.IsHalted,
                            PlanType = s.PlanType,
                            IsArchived = s.IsArchived,
                            Audience = s.Audience,
                            FeaturedImageUri = s.FeaturedImageUri.ToDocumentUri(),
                            PaymentTransaction = s.PaymentTransactions.FirstOrDefault(),
                            BusinessName = s.Domain.Id.BusinesProfile()?.BusinessName ?? s.Domain.Name,
                            BusinessKey = s.Domain.Id.BusinesProfile()?.Key ?? s.Domain.Key,
                            BusinessProfileId = s.Domain.Id.BusinesProfile()?.Id ?? s.Domain.Id,
                            IsLiked = s.LikingUsers.Any(l => l.Id == filterModel.currentUserId),
                            AllowClaimNow = s.CheckAllowClaimNow(filterModel.currentUserId, currentDate),
                            RemainHtmlInfo = s.CalRemainPromotionInfo(filterModel.timezone, filterModel.dateformat, currentDate),
                            RemainInfo = s.CalRemainInfo(filterModel.timezone, filterModel.dateformat, currentDate),
                            DomainLogo = s.Domain.Id.BusinesProfile().LogoUri.ToDocumentUri().ToString(),
                            IsMarkedLiked = s.LikedBy.Any(l => l.Id == filterModel.currentUserId),
                            MarkedLikedCount = s.LikedBy?.Count ?? 0,
                            TotalClaimed = s.Vouchers.Count(e => e.ClaimedBy != null && !e.IsRedeemed)
                        }).ToList();

                return response;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, filterModel);
                return response;
            }
        }

        public PaginationResponse GetNearbyPublishPromotions(PromotionPublishFilterModel filterModel)
        {
            var response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, filterModel);

                var currentDomainId = string.IsNullOrEmpty(filterModel.businessKey) ? 0 : int.Parse(filterModel.businessKey.Decrypt());
                var currentDate = DateTime.UtcNow;
                var queryPublishPromotions = dbContext.Promotions.Where(s => !s.IsArchived
                                                                        && !s.IsDraft
                                                                        && !s.IsHalted
                                                                        && s.DisplayDate <= currentDate
                                                                        && s.Domain.Status != QbicleDomain.DomainStatusEnum.Closed)
                                                                 .Include(s => s.PlanType);
                if (currentDomainId > 0)
                {
                    queryPublishPromotions = queryPublishPromotions.Where(s => s.Domain.Id == currentDomainId);
                }
                if (currentDomainId > 0 && filterModel.locationIds != null)
                {
                    queryPublishPromotions = queryPublishPromotions.Where(s => s.VoucherInfo.Locations.Any(x => filterModel.locationIds.Contains(x.Id)));
                }
                if (!string.IsNullOrEmpty(filterModel.keyword))
                {
                    queryPublishPromotions = queryPublishPromotions.Where(s => s.Name.Contains(filterModel.keyword) || s.Description.Contains(filterModel.keyword));
                }
                if (filterModel.isMyFavourites)
                {
                    queryPublishPromotions = queryPublishPromotions.Where(s => s.LikingUsers.Any(u => u.Id == filterModel.currentUserId));
                }

                response.totalNumber = filterModel.isLoadTotalRecord ? queryPublishPromotions.Count() : 0;
                response.items = queryPublishPromotions.OrderBy(s => s.PlanType.Rank).Skip(filterModel.pageSize).Take(filterModel.pageNumber).ToList().Select(s => new PromotionModel
                {
                    PromotionKey = s.Key,
                    Name = s.Name,
                    Description = s.Description,
                    DisplayDate = s.DisplayDate.ConvertTimeFromUtc(filterModel.timezone),
                    VoucherExpiryDate = s.VoucherInfo.VoucherExpiryDate.ConvertTimeFromUtc(filterModel.timezone) ?? s.EndDate.ConvertTimeFromUtc(filterModel.timezone),
                    StartDate = s.StartDate.ConvertTimeFromUtc(filterModel.timezone),
                    EndDate = s.EndDate.ConvertTimeFromUtc(filterModel.timezone),
                    IsHalted = s.IsHalted,
                    PlanType = s.PlanType,
                    IsArchived = s.IsArchived,
                    FeaturedImageUri = s.FeaturedImageUri.ToDocumentUri(),
                    BusinessName = s.Domain.Id.BusinesProfile()?.BusinessName ?? s.Domain.Name,
                    BusinessKey = s.Domain.Id.BusinesProfile()?.Key ?? s.Domain.Key,
                    BusinessProfileId = s.Domain.Id.BusinesProfile()?.Id ?? s.Domain.Id,
                    IsLiked = s.LikingUsers.Any(l => l.Id == filterModel.currentUserId),
                    AllowClaimNow = s.CheckAllowClaimNow(filterModel.currentUserId, currentDate),
                    RemainHtmlInfo = s.CalRemainPromotionInfo(filterModel.timezone, filterModel.dateformat, currentDate),
                    RemainInfo = s.CalRemainInfo(filterModel.timezone, filterModel.dateformat, currentDate),
                    DomainLogo = s.Domain.Id.BusinesProfile().LogoUri.ToDocumentUri().ToString(),
                    IsMarkedLiked = s.LikedBy.Any(l => l.Id == filterModel.currentUserId),
                    MarkedLikedCount = s.LikedBy?.Count ?? 0,
                    TotalClaimed = s.Vouchers.Count(e => e.ClaimedBy != null && !e.IsRedeemed)
                }).ToList();
                return response;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, filterModel);
                return response;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public List<PromotionModel> GetActivePromotions(PromotionFilterModel filterModel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, filterModel);

                var queryActivePromotions = dbContext.Promotions
                    .Where(s => s.Domain.Id == filterModel.currentDomainId)
                    .Include(s => s.PlanType)
                    .Include(s => s.PaymentTransactions);

                if (!string.IsNullOrEmpty(filterModel.keyword))
                {
                    queryActivePromotions = queryActivePromotions.Where(s => s.Name.Contains(filterModel.keyword) || s.Description.Contains(filterModel.keyword));
                }

                if (!string.IsNullOrEmpty(filterModel.daterange))
                {
                    if (!filterModel.daterange.Contains('-'))
                        filterModel.daterange += "-";

                    var startDate = DateTime.MinValue;
                    var endDate = DateTime.UtcNow;
                    filterModel.daterange.ConvertDaterangeFormat(filterModel.dateformat, filterModel.timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    queryActivePromotions = queryActivePromotions.Where(s => s.StartDate >= startDate && s.StartDate < endDate);
                }

                if (filterModel.type != 0)
                    queryActivePromotions = queryActivePromotions.Where(s => s.VoucherInfo.Type == (VoucherType)filterModel.type);
                var currentDate = DateTime.UtcNow;

                if (filterModel.status.Length < 5)
                {
                    switch (filterModel.status.Length)
                    {
                        case 1:
                            if (filterModel.status.Contains("1"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived);
                            if (filterModel.status.Contains("2"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate > currentDate);
                            if (filterModel.status.Contains("3"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.EndDate < currentDate);
                            if (filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.IsArchived);
                            if (filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.IsHalted);
                            if (filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.IsDraft);
                            break;

                        case 2:
                            //if active & pending
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived) || s.StartDate > currentDate);

                            // if active & end
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("3"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived) || (s.EndDate < currentDate || s.IsHalted));

                            // if active & archive
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived) || s.IsArchived);

                            // if active & stop
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived) || s.IsHalted);

                            //if active & draft
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsDraft) || s.StartDate > currentDate);

                            //if pending & end
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("3"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate > currentDate || (s.EndDate < currentDate || s.IsHalted));

                            //if pending & archive
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate > currentDate || s.IsArchived);

                            //if pending & stop
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate > currentDate || s.IsHalted);

                            //if pending & draft
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate > currentDate || s.IsDraft);

                            // if end & archive
                            if (filterModel.status.Contains("3") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.EndDate < currentDate || s.IsArchived) && !s.IsHalted);

                            // if end & stop
                            if (filterModel.status.Contains("3") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.EndDate < currentDate || s.IsHalted) && !s.IsArchived);

                            // if end & draft
                            if (filterModel.status.Contains("3") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.EndDate < currentDate || s.IsDraft) && !s.IsHalted);

                            //if draft & archive
                            if (filterModel.status.Contains("6") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.IsDraft || s.IsArchived);

                            //if draft & stop
                            if (filterModel.status.Contains("6") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.IsDraft || s.IsHalted);

                            break;

                        case 3:
                            //if active & pending & Expired
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("3"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived)
                                || s.StartDate > currentDate || (s.EndDate < currentDate || s.IsHalted));
                            //if active & pending & Archived
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived)
                                || s.StartDate > currentDate || s.IsArchived);
                            //if active & pending & Stop
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && s.IsHalted && !s.IsArchived)
                                || s.StartDate > currentDate || s.IsArchived);
                            //if active & pending & Draft
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived && !s.IsDraft)
                                || s.StartDate > currentDate || s.IsDraft);

                            //if active & Expired & Archived
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("3") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                            (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived)
                            || (s.EndDate < currentDate || s.IsHalted) || s.IsArchived);

                            //if pending & Expired & Archived
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                            s.StartDate > currentDate || (s.EndDate < currentDate || !s.IsHalted) || s.IsArchived);
                            //if pending & Expired & Stop
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                            s.StartDate > currentDate || (s.EndDate < currentDate || s.IsHalted) || !s.IsArchived);
                            break;

                        case 4:
                            //if active & pending & Expired & Draft
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived && !s.IsDraft)
                                || s.StartDate > currentDate || (s.EndDate < currentDate || s.IsHalted) || s.IsDraft);
                            //if active & pending & Archived & Draft
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("4") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived && !s.IsDraft)
                                || s.StartDate > currentDate || s.IsArchived || s.IsDraft);
                            //if active & pending & Stop & Draft
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("5") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && s.IsHalted && !s.IsArchived && !s.IsDraft)
                                || s.StartDate > currentDate || s.IsArchived || s.IsDraft);

                            //if active & Expired & Archived & Draft
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("3") && filterModel.status.Contains("4") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                            (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived && !s.IsDraft)
                            || (s.EndDate < currentDate || s.IsHalted) || s.IsArchived || s.IsDraft);

                            //if pending & Expired & Archived & Draft
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("4") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                            s.StartDate > currentDate || (s.EndDate < currentDate || !s.IsHalted) || s.IsArchived || s.IsDraft);
                            //if pending & Expired & Stop & Draft
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("5") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                            s.StartDate > currentDate || (s.EndDate < currentDate || s.IsHalted) || !s.IsArchived || s.IsDraft);
                            break;

                        case 5:
                            //if active & pending & Expired & Archived & Draft
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("4") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && s.IsArchived && s.IsDraft)
                                || s.StartDate > currentDate || (s.EndDate < currentDate || s.IsHalted));
                            //if active & pending & Expired & Stop & Draft
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("5") && filterModel.status.Contains("6"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && s.IsHalted && !s.IsArchived && !s.IsDraft)
                                || s.StartDate > currentDate || (s.EndDate < currentDate || !s.IsHalted) || s.IsDraft);
                            break;
                    }
                }

                return queryActivePromotions.OrderBy(s => s.PlanType.Rank).ToList().Select(s => new PromotionModel
                {
                    PromotionKey = s.Key,
                    Name = s.Name,
                    Description = s.Description,
                    DisplayDate = s.DisplayDate,
                    VoucherExpiryDate = s.VoucherInfo?.VoucherExpiryDate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    PlanType = s.PlanType,
                    IsHalted = s.IsHalted,
                    IsDraft = s.IsDraft,
                    IsArchived = s.IsArchived,
                    FeaturedImageUri = s.FeaturedImageUri.ToDocumentUri(),
                    DomainLogo = s.Domain.LogoUri.ToDocumentUri().ToString(),
                    MarkedLikedCount = s.LikedBy?.Count ?? 0,
                    TotalClaimed = s.Vouchers.Count(e => e.ClaimedBy != null && !e.IsRedeemed),
                    PaymentTransaction = s.PaymentTransactions.OrderByDescending(x => x.Id).FirstOrDefault()
                }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, filterModel);
                return new List<PromotionModel>();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public List<PromotionModel> GetBulkDealPromotions(PromotionFilterModel filterModel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, filterModel);

                var queryActivePromotions = dbContext.BulkDealPromotions.AsQueryable();

                if (!string.IsNullOrEmpty(filterModel.keyword))
                {
                    queryActivePromotions = queryActivePromotions.Where(s => s.Name.Contains(filterModel.keyword) || s.Description.Contains(filterModel.keyword));
                }

                if (!string.IsNullOrEmpty(filterModel.daterange))
                {
                    if (!filterModel.daterange.Contains('-'))
                        filterModel.daterange += "-";

                    var startDate = DateTime.MinValue;
                    var endDate = DateTime.UtcNow;
                    filterModel.daterange.ConvertDaterangeFormat(filterModel.dateformat, filterModel.timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    queryActivePromotions = queryActivePromotions.Where(s => s.StartDate >= startDate && s.StartDate < endDate);
                }

                if (filterModel.type != 0)
                    queryActivePromotions = queryActivePromotions.Where(s => s.BulkDealVoucherInfo.Type == (VoucherType)filterModel.type);
                var currentDate = DateTime.UtcNow;

                if (filterModel.status.Length < 4)
                {
                    switch (filterModel.status.Length)
                    {
                        case 1:
                            if (filterModel.status.Contains("1"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived);
                            if (filterModel.status.Contains("2"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate > currentDate);
                            if (filterModel.status.Contains("3"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.EndDate < currentDate);
                            if (filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.IsArchived);
                            if (filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.IsHalted);
                            break;

                        case 2:
                            //if active & pending
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived) || s.StartDate > currentDate);

                            // if active & end
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("3"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived) || (s.EndDate < currentDate || s.IsHalted));

                            // if active & archive
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived) || s.IsArchived);

                            // if active & stop
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived) || s.IsHalted);

                            //if pending & end
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("3"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate > currentDate || (s.EndDate < currentDate || s.IsHalted));

                            //if pending & archive
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate > currentDate || s.IsArchived);

                            //if pending & stop
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s => s.StartDate > currentDate || s.IsHalted);

                            // if end & archive
                            if (filterModel.status.Contains("3") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.EndDate < currentDate || s.IsArchived) && !s.IsHalted);

                            // if end & stop
                            if (filterModel.status.Contains("3") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s => (s.EndDate < currentDate || s.IsHalted) && !s.IsArchived);

                            break;

                        case 3:
                            //if active & pending & Expired
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("3"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived)
                                || s.StartDate > currentDate || (s.EndDate < currentDate || s.IsHalted));
                            //if active & pending & Archived
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived)
                                || s.StartDate > currentDate || s.IsArchived);
                            //if active & pending & Stop
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && s.IsHalted && !s.IsArchived)
                                || s.StartDate > currentDate || s.IsArchived);

                            //if active & Expired & Archived
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("3") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                            (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && !s.IsArchived)
                            || (s.EndDate < currentDate || s.IsHalted) || s.IsArchived);
                            //if pending & Expired & Archived
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                            s.StartDate > currentDate || (s.EndDate < currentDate || !s.IsHalted) || s.IsArchived);
                            //if pending & Expired & Stop
                            if (filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                            s.StartDate > currentDate || (s.EndDate < currentDate || s.IsHalted) || !s.IsArchived);
                            break;

                        case 4:
                            //if active & pending & Expired & Archived
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("4"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && !s.IsHalted && s.IsArchived)
                                || s.StartDate > currentDate || (s.EndDate < currentDate || s.IsHalted));
                            //if active & pending & Expired & Stop
                            if (filterModel.status.Contains("1") && filterModel.status.Contains("2") && filterModel.status.Contains("3") && filterModel.status.Contains("5"))
                                queryActivePromotions = queryActivePromotions.Where(s =>
                                (s.StartDate <= currentDate && s.EndDate >= currentDate && s.IsHalted && !s.IsArchived)
                                || s.StartDate > currentDate || (s.EndDate < currentDate || !s.IsHalted));
                            break;
                    }
                }

                return queryActivePromotions.OrderBy(s => s.Name).ToList().Select(s => new PromotionModel
                {
                    PromotionKey = s.Key,
                    Name = s.Name,
                    Description = s.Description,
                    DisplayDate = s.DisplayDate,
                    VoucherExpiryDate = s.BulkDealVoucherInfo?.VoucherExpiryDate,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    IsDraft = s.IsDraft,
                    IsHalted = s.IsHalted,
                    IsArchived = s.IsArchived,
                    FeaturedImageUri = s.FeaturedImageUri.ToDocumentUri(),
                    TotalClaimed = s.OptInVouchers.Count(e => e.OptInBusiness != null && !e.IsOptIn)
                }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, filterModel);
                return new List<PromotionModel>();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public DataTablesResponse GetArchivedPromotions(IDataTablesRequest requestModel, PromotionFilterModel filterModel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, filterModel);

                var queryActivePromotions = dbContext.Promotions.Where(s => s.IsArchived && s.Domain.Id == filterModel.currentDomainId).Include(s => s.PlanType);
                int totalRecords;

                #region Filters

                if (!string.IsNullOrEmpty(filterModel.keyword))
                {
                    queryActivePromotions = queryActivePromotions.Where(s => s.Name.Contains(filterModel.keyword) || s.Description.Contains(filterModel.keyword));
                }
                if (!string.IsNullOrEmpty(filterModel.daterange))
                {
                    if (!filterModel.daterange.Contains('-'))
                        filterModel.daterange += "-";
                    var startDate = DateTime.MinValue;
                    var endDate = DateTime.UtcNow;
                    filterModel.daterange.ConvertDaterangeFormat(filterModel.dateformat, filterModel.timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    queryActivePromotions = queryActivePromotions.Where(s => s.StartDate >= startDate && s.StartDate < endDate);
                }
                if (filterModel.type != 0)
                {
                    queryActivePromotions = queryActivePromotions.Where(s => s.VoucherInfo.Type == (VoucherType)filterModel.type);
                }
                totalRecords = queryActivePromotions.Count();

                #endregion Filters

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Type":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Type" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "StartDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "StartDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "EndDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "EndDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "Id asc";
                            break;
                    }
                }

                queryActivePromotions = queryActivePromotions.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = queryActivePromotions.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(s => new
                {
                    PromotionKey = s.Key,
                    Name = s.Name,
                    Description = s.Description,
                    PlanType = s.PlanType,
                    StartDate = s.StartDate.ConvertTimeFromUtc(filterModel.timezone).ToString(filterModel.dateformat + " hh:mmtt"),
                    EndDate = s.EndDate.ConvertTimeFromUtc(filterModel.timezone).ToString(filterModel.dateformat + " hh:mmtt"),
                    IsHalted = s.IsHalted,
                    Type = s.VoucherInfo.Type.GetDescription()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, filterModel);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public DataTablesResponse GetPromotionTypes(IDataTablesRequest requestModel, PromotionFilterModel filterModel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, filterModel);

                var queryActivePromotions = dbContext.PromotionTypes.Where(s => !s.IsDeleted);

                int totalRecords;

                #region Filters

                if (!string.IsNullOrEmpty(filterModel.keyword))
                {
                    queryActivePromotions = queryActivePromotions.Where(s => s.Name.Contains(filterModel.keyword));
                }
                if (!string.IsNullOrEmpty(filterModel.daterange))
                {
                    if (!filterModel.daterange.Contains('-')) filterModel.daterange += "-";
                    var startDate = DateTime.MinValue;
                    var endDate = DateTime.UtcNow;
                    filterModel.daterange.ConvertDaterangeFormat(filterModel.dateformat, filterModel.timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    queryActivePromotions = queryActivePromotions.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }

                //todo: optimise this to filter by IsActive
                if (filterModel.type != 0)
                {
                    if (filterModel.type == 1)
                    {
                        queryActivePromotions = queryActivePromotions.Where(s => s.IsActive);
                    }
                    if (filterModel.type == 2)
                    {
                        queryActivePromotions = queryActivePromotions.Where(s => !s.IsActive);
                    }
                }

                totalRecords = queryActivePromotions.Count();

                #endregion Filters

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Name)
                    {
                        case "Name":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "CreatedDate":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "IsActive":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "IsActive" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "Name asc";
                            break;
                    }
                }

                queryActivePromotions = queryActivePromotions.OrderBy(orderByString == string.Empty ? "Name asc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = queryActivePromotions.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.OrderBy(x => x.LastModifiedDate).Select(s => new
                {
                    Key = s.Key,
                    Icon = s.Icon,
                    Name = s.Name,
                    Type = HelperClass.EnumModel.GetDescriptionFromEnumValue((Enums.PromotionType)s.Type),
                    Duration = s.Duration,
                    Amount = s.Price,
                    Rank = s.Rank,
                    CreatedDate = s.CreatedDate.ConvertTimeFromUtc(filterModel.timezone).ToString(filterModel.dateformat + " hh:mmtt"),
                    IsActive = s.IsActive
                }).ToList();

                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, filterModel);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="filterModel"></param>
        /// <returns></returns>
        public DataTablesResponse GetRankPromotionTypes(IDataTablesRequest requestModel, PromotionFilterModel filterModel)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, filterModel);

                var queryActivePromotions = dbContext.PromotionTypes.Where(s => !s.IsDeleted && s.IsActive);

                int totalRecords;

                #region Filters

                if (!string.IsNullOrEmpty(filterModel.keyword))
                {
                    queryActivePromotions = queryActivePromotions.Where(s => s.Name.Contains(filterModel.keyword) || s.Description.Contains(filterModel.keyword));
                }
                if (!string.IsNullOrEmpty(filterModel.daterange))
                {
                    if (!filterModel.daterange.Contains('-'))
                        filterModel.daterange += "-";
                    var startDate = DateTime.MinValue;
                    var endDate = DateTime.UtcNow;
                    filterModel.daterange.ConvertDaterangeFormat(filterModel.dateformat, filterModel.timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    queryActivePromotions = queryActivePromotions.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }

                //todo: optimise this to filter by IsActive
                if (filterModel.type != 0)
                {
                    queryActivePromotions = queryActivePromotions.Where(s => s.Type == filterModel.type);
                }

                totalRecords = queryActivePromotions.Count();

                #endregion Filters

                #region Paging

                queryActivePromotions = queryActivePromotions.OrderBy(x => x.Rank);
                var list = queryActivePromotions.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(s => new
                {
                    Key = s.Key,
                    Promotion = s.Name,
                    Type = HelperClass.EnumModel.GetDescriptionFromEnumValue((Enums.PromotionType)s.Type),
                    Duration = s.Duration,
                    Amount = s.Price,
                    Rank = s.Rank,
                }).ToList();

                return new DataTablesResponse(requestModel.Draw, dataJson, totalRecords, totalRecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, filterModel);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        public int CountVouchersValidByUserAndShop(string currentUserId, int currentDomainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, currentUserId, currentDomainId);
                var currentDate = DateTime.UtcNow;
                var queryVouchers = dbContext.Vouchers.Where(s =>
                    s.ClaimedBy.Id == currentUserId &&
                    s.Promotion.Domain.Id == currentDomainId &&
                    !s.IsRedeemed &&
                    s.VoucherExpiryDate > currentDate
                );
                return queryVouchers.Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return 0;
            }
        }

        public PaginationResponse GetVouchersByUserAndShop(VoucherByUserAndShopModel filterModel)
        {
            var response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, filterModel);
                var currentDate = DateTime.UtcNow;
                var currentDomainId = string.IsNullOrEmpty(filterModel.domainkey) ? 0 : int.Parse(filterModel.domainkey.Decrypt());
                var queryVouchers = dbContext.Vouchers.Where(s =>
                    s.ClaimedBy.Id == filterModel.currentUserId &&
                    s.Promotion.Domain.Id == currentDomainId
                    );

                #region Filters

                if (!string.IsNullOrEmpty(filterModel.keyword))
                {
                    queryVouchers = queryVouchers.Where(s => s.Promotion.Name.Contains(filterModel.keyword));
                }
                if (!filterModel.isRedeemed && !filterModel.isExpired)//Only display Vouchers is Valid
                    queryVouchers = queryVouchers.Where(s => !s.IsRedeemed && s.VoucherExpiryDate >= currentDate);
                else if (filterModel.isRedeemed && !filterModel.isExpired)//Display Valid Vouchers and Redeemed Vouchers
                    queryVouchers = queryVouchers.Where(s => (!s.IsRedeemed && s.VoucherExpiryDate >= currentDate) || s.IsRedeemed);
                else if (!filterModel.isRedeemed && filterModel.isExpired)//Display Valid Vouchers and Expired Vouchers
                    queryVouchers = queryVouchers.Where(s => (!s.IsRedeemed && s.VoucherExpiryDate <= currentDate) || (!s.IsRedeemed && s.VoucherExpiryDate >= currentDate));

                if (filterModel.isCountRecords)
                    response.totalNumber = queryVouchers.Count();

                #endregion Filters

                #region Sorting

                queryVouchers = queryVouchers.OrderByDescending(s => s.CreatedDate);

                #endregion Sorting

                #region Paging

                var list = queryVouchers.Skip(filterModel.pageSize).Take(filterModel.pageNumber).ToList();

                #endregion Paging

                response.items = list.Select(s => new VoucherOfUserModel
                {
                    PromotionKey = s.Promotion.Key,
                    VourcherKey = s.Id.Encrypt(),
                    Name = s.Promotion.Name,
                    StartDate = s.Promotion.StartDate.ConvertTimeFromUtc(filterModel.timezone),
                    EndDate = s.VoucherExpiryDate?.ConvertTimeFromUtc(filterModel.timezone) ?? s.Promotion.EndDate.ConvertTimeFromUtc(filterModel.timezone),
                    RedeemedDate = s.RedeemedDate.ConvertTimeFromUtc(filterModel.timezone),
                    Type = CalVoucherType(s, currentDate),
                }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
            }
            return response;
        }

        public async Task<ReturnJsonModel> SavePromotion(PromotionModel model, ItemDiscountVoucherInfo itemDiscountVoucherInfo, OrderDiscountVoucherInfo orderDiscountVoucherInfo, string featuredImageUri)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };

            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, model);

                    //Initialise data
                    var currentUser = dbContext.QbicleUser.Find(model.CurrentUserId);
                    var currentPromotionType = await dbContext.PromotionTypes.FindAsync(model.PlanType.Id);
                    var promotionId = int.Parse(model.PromotionKey.Decrypt());
                    var dbpromotion = dbContext.Promotions.Find(promotionId);
                    var transactionAmount = currentPromotionType.Price;

                    //Update plantype
                    model.PlanType = dbContext.PromotionTypes.Find(model.PlanType.Id);

                    //Check for modifying active promotions before processing
                    if (dbpromotion != null)
                    {
                        if (dbpromotion.StartDate < DateTime.UtcNow || dbpromotion.IsArchived)
                        {
                            returnJson.msg = ResourcesManager._L("ERROR_MSG_PROMOTIONSTARTED");
                            return returnJson;
                        }
                    }

                    //SCENARIO 1: Save Promotion and pend activation
                    //save promotion to DB and do not activate promotion
                    if (ProcessSavePromotionAsync(dbTransaction, model, itemDiscountVoucherInfo, orderDiscountVoucherInfo, featuredImageUri, currentUser, ref dbpromotion))
                    {
                        // Fetch existing transaction or return null
                        var paymentTranx = await dbContext.PromotionPaymentTransactions
                            .OrderByDescending(x => x.Id)
                            .FirstOrDefaultAsync(x => x.LoyaltyPromotion.Id == dbpromotion.Id);

                        //SCENARIO 2: Process Promotion payment transaction
                        //Skip payment if promotion is draft, a free promotion where id is 1,
                        //and not payment history is found

                        //Case: Draft promotion
                        if (model.IsDraft)
                        {
                            //Update payment for draft promotion
                            //set amount to zero and transaction reference to empty
                            var transaction = new LoyaltyPromotionPaymentTransaction()
                            {
                                LoyaltyPromotion = dbpromotion,
                                Amount = transactionAmount,
                                Status = PaymentStatus.OnHold,
                                TransactionReference = string.Empty,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.Now,
                                LastModifiedBy = currentUser,
                                LastModifiedDate = DateTime.Now,
                            };

                            //Add to database
                            dbContext.PromotionPaymentTransactions.Add(transaction);

                            //Save changes to DB
                            await dbContext.SaveChangesAsync();

                            returnJson.result = true;
                            //returnJson.msg = "Transaction initalization to get authorization data fails";
                        }

                        //Case: Newly created premium promotion without payment history
                        else if (!model.IsDraft && model.PlanType.Id > 1 && paymentTranx == null)
                        {
                            string[] transactionChannels = { };

                            //// Fetch existing transaction or return null
                            //var transaction = await dbContext.PromotionPaymentTransactions.FirstOrDefaultAsync(x => x.LoyaltyPromotion.Id == dbpromotion.Id);

                            //if (transaction != null)
                            //{
                            //    //update payment status
                            //    transaction.Status = PaymentStatus.Pending;
                            //    transaction.LastModifiedDate = DateTime.Now;

                            //    //Update database changes
                            //    dbContext.Entry(transaction).State = EntityState.Modified;
                            //}
                            //else
                            //{
                            //Init payment transaction model and save to database
                            // before initialising payment
                            var transaction = new LoyaltyPromotionPaymentTransaction()
                            {
                                LoyaltyPromotion = dbpromotion,
                                Amount = transactionAmount,
                                Status = PaymentStatus.Pending,
                                TransactionReference = string.Empty,
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.Now,
                                LastModifiedBy = currentUser,
                                LastModifiedDate = DateTime.Now,
                            };

                            //Add to database
                            dbContext.PromotionPaymentTransactions.Add(transaction);
                            //}

                            //Save changes to DB
                            await dbContext.SaveChangesAsync();

                            //Init paystack transaction
                            var transactionResponse = await new PayStackRules(dbContext).InitGettingAuthorizationTransaction(transactionAmount, "", "", currentUser.Email, transactionChannels);

                            // Check transaction response status
                            if (transactionResponse.status)
                            {
                                //update reference number
                                transaction.TransactionReference = transactionResponse.data.reference;

                                //Update database changes
                                dbContext.Entry(transaction).State = EntityState.Modified;

                                //Save changes to DB
                                await dbContext.SaveChangesAsync();

                                // Return result
                                returnJson.result = true;
                                //returnJson.msg = "";
                                returnJson.Object = transactionResponse.data;
                            }
                            else
                            {
                                returnJson.result = false;
                                returnJson.msg = "Transaction initalization to get authorization data fails";
                            }
                        }

                        //Case: Newly created freemuim promotion
                        else if (!model.IsDraft && model.PlanType.Id == 1)
                        {
                            //Update payment for freemium promotion
                            //set amount to zero and transaction reference to QBICLESFREE
                            var transaction = new LoyaltyPromotionPaymentTransaction()
                            {
                                LoyaltyPromotion = dbpromotion,
                                Amount = transactionAmount,
                                Status = PaymentStatus.Completed,
                                TransactionReference = "QBICLESFREEPROMOTION",
                                CreatedBy = currentUser,
                                CreatedDate = DateTime.Now,
                                LastModifiedBy = currentUser,
                                LastModifiedDate = DateTime.Now,
                            };

                            //Add to database
                            dbContext.PromotionPaymentTransactions.Add(transaction);

                            //Save changes to DB
                            await dbContext.SaveChangesAsync();

                            returnJson.result = true;
                            //returnJson.msg = "Transaction initalization to get authorization data fails";
                        }

                        //Case: Existing premium promotion with payment history
                        else if (!model.IsDraft && model.PlanType.Id > 1 && paymentTranx != null)
                        {
                            returnJson.result = true;
                            returnJson.msg = "Promotion updated successfully!";
                        }

                        dbTransaction.Commit();

                        //SCENARIO 3: Confirm/Verify Promotion payment transaction (TODO)
                    }
                    else
                    {
                        //TODO: change error type
                        returnJson.msg = ResourcesManager._L("ERROR_MSG_PROMOTIONSTARTED");
                        return returnJson;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                    dbTransaction.Rollback();
                }
            }

            return returnJson;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <param name="itemDiscountVoucherInfo"></param>
        /// <param name="orderDiscountVoucherInfo"></param>
        /// <param name="featuredImageUri"></param>
        /// <param name="currentUser"></param>
        /// <param name="dbpromotion"></param>
        /// <returns></returns>
        private bool ProcessSavePromotionAsync(DbContextTransaction dbTransaction, PromotionModel model, ItemDiscountVoucherInfo itemDiscountVoucherInfo, OrderDiscountVoucherInfo orderDiscountVoucherInfo, string featuredImageUri, ApplicationUser currentUser, ref LoyaltyPromotion dbpromotion)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, model);

                TimeSpan fromTime = string.IsNullOrEmpty(model.FromTime) ? new TimeSpan() : TimeSpan.Parse(model.FromTime);
                TimeSpan toTime = string.IsNullOrEmpty(model.ToTime) ? new TimeSpan() : TimeSpan.Parse(model.ToTime);

                var startDate = DateTime.MinValue;
                var endDate = DateTime.UtcNow;
                var dateRange = $"{model.StartDateString}-{model.EndDateString}";
                dateRange.ConvertDaterangeFormat(model.DateFormat, model.Timezone, out startDate, out endDate, HelperClass.endDateAddedType.none);

                model.StartDate = startDate;
                model.EndDate = endDate;
                model.DisplayDate = model.DisplayDateString.ConvertDateFormat(model.DateFormat).ConvertTimeToUtc(model.Timezone);

                if (string.IsNullOrEmpty(model.VoucherExpiryDateString))
                    model.VoucherExpiryDate = null;
                else
                    model.VoucherExpiryDate = model.VoucherExpiryDateString.ConvertDateFormat(model.DateFormat).ConvertTimeToUtc(model.Timezone);

                var expirydDate = model.VoucherExpiryDate?.ConvertTimeToUtc(model.Timezone);

                if (dbpromotion != null)
                {
                    dbpromotion.Name = model.Name;
                    dbpromotion.PlanType = model.PlanType;
                    dbpromotion.Description = model.Description;
                    dbpromotion.IsDraft = model.IsDraft;
                    dbpromotion.StartDate = model.StartDate.ConvertTimeToUtc(model.Timezone);
                    dbpromotion.EndDate = model.EndDate.ConvertTimeToUtc(model.Timezone);

                    if (!string.IsNullOrEmpty(featuredImageUri)) dbpromotion.FeaturedImageUri = featuredImageUri;

                    dbpromotion.DisplayDate = model.DisplayDate.ConvertTimeToUtc(model.Timezone);

                    if (dbpromotion.VoucherInfo.Type == VoucherType.ItemDiscount)
                    {
                        var itemdiscount = dbContext.ItemDiscountVoucherInfos.Find(dbpromotion.VoucherInfo.Id);
                        if (itemdiscount != null)
                        {
                            if (itemdiscount.Locations != null)
                                itemdiscount.Locations.Clear();
                            if (itemdiscount.DaysAllowed != null)
                                dbContext.LoyaltyWeekDays.RemoveRange(itemdiscount.DaysAllowed);
                            dbContext.ItemDiscountVoucherInfos.Remove(itemdiscount);
                            dbContext.SaveChanges();
                        }
                    }
                    else if (dbpromotion.VoucherInfo.Type == VoucherType.OrderDiscount)
                    {
                        var itemdiscount = dbContext.OrderDiscountVoucherInfos.Find(dbpromotion.VoucherInfo.Id);
                        if (itemdiscount != null)
                        {
                            if (itemdiscount.Locations != null)
                                itemdiscount.Locations.Clear();
                            if (itemdiscount.DaysAllowed != null)
                                dbContext.LoyaltyWeekDays.RemoveRange(itemdiscount.DaysAllowed);
                            dbContext.OrderDiscountVoucherInfos.Remove(itemdiscount);
                            dbContext.SaveChanges();
                        }
                    }

                    if (model.Type == VoucherType.ItemDiscount)
                    {
                        List<TraderLocation> locations = new List<TraderLocation>();
                        if (itemDiscountVoucherInfo.Locations != null)
                            foreach (var item in itemDiscountVoucherInfo.Locations)
                            {
                                var location = dbContext.TraderLocations.Find(item.Id);
                                if (location != null)
                                    locations.Add(location);
                            }
                        itemDiscountVoucherInfo.Locations = locations;
                        itemDiscountVoucherInfo.Promotion = dbpromotion;
                        itemDiscountVoucherInfo.CreatedDate = DateTime.UtcNow;
                        itemDiscountVoucherInfo.CreatedBy = currentUser;
                        itemDiscountVoucherInfo.StartTime = fromTime;
                        itemDiscountVoucherInfo.EndTime = toTime;
                        itemDiscountVoucherInfo.DaysAllowed = calWeekDays(itemDiscountVoucherInfo, model.DaysOfweek);
                        itemDiscountVoucherInfo.VoucherExpiryDate = model.VoucherExpiryDate;
                        dbContext.ItemDiscountVoucherInfos.Add(itemDiscountVoucherInfo);
                        dbpromotion.VoucherInfo = itemDiscountVoucherInfo;
                    }
                    else if (model.Type == VoucherType.OrderDiscount)
                    {
                        List<TraderLocation> locations = new List<TraderLocation>();
                        if (itemDiscountVoucherInfo.Locations != null)
                            foreach (var item in orderDiscountVoucherInfo.Locations)
                            {
                                var location = dbContext.TraderLocations.Find(item.Id);
                                if (location != null)
                                    locations.Add(location);
                            }
                        orderDiscountVoucherInfo.Locations = locations;
                        orderDiscountVoucherInfo.Promotion = dbpromotion;
                        orderDiscountVoucherInfo.CreatedDate = DateTime.UtcNow;
                        orderDiscountVoucherInfo.CreatedBy = currentUser;
                        orderDiscountVoucherInfo.StartTime = fromTime;
                        orderDiscountVoucherInfo.EndTime = toTime;
                        orderDiscountVoucherInfo.DaysAllowed = calWeekDays(orderDiscountVoucherInfo, model.DaysOfweek);
                        orderDiscountVoucherInfo.VoucherExpiryDate = model.VoucherExpiryDate;
                        dbContext.OrderDiscountVoucherInfos.Add(orderDiscountVoucherInfo);
                        dbpromotion.VoucherInfo = orderDiscountVoucherInfo;
                    }

                    //Get audience data
                    //var promotionId = dbpromotion.Id;
                    //var promotionAudience = dbContext.PromotionAudiences.FirstOrDefault(x => x.LoyaltyPromotionId == promotionId);

                    ////update promotion audience
                    model.Audience.LastModifiedDate = DateTime.UtcNow;
                    model.Audience.LastModifiedBy = currentUser;

                    //promotionAudience.LocationVisibility = model.Audience.LocationVisibility;
                    //promotionAudience.Distance = model.Audience.Distance;
                    //promotionAudience.DistanceFactor = model.Audience.DistanceFactor;
                    //promotionAudience.BusinessLocation = model.Audience.BusinessLocation;
                    //promotionAudience.LocationVisibility = model.Audience.LocationVisibility;

                    //Update entries in DB
                    dbContext.Entry(dbpromotion).State = EntityState.Modified;
                    //dbContext.Entry(model.Audience).State = EntityState.Modified;
                }
                else
                {
                    //SCENARIO
                    //By default, all paid promotions should be marked as draft
                    //and not started (isHalted = true) until payment is made
                    bool isHalted = false;
                    bool isDraft = model.IsDraft;
                    if (model.PlanType.Id > 1)
                    {
                        isDraft = true;
                        isHalted = true;
                    }

                    //TODO:
                    //When payment fails, halt the promotion and save as draft
                    //Users can return to edit mode if the want to retry payment

                    dbpromotion = new LoyaltyPromotion
                    {
                        Domain = dbContext.Domains.Find(model.CurrentDomainId),
                        Name = model.Name,
                        PlanType = model.PlanType,
                        Description = model.Description,
                        StartDate = model.StartDate,//.ConvertTimeToUtc(model.Timezone),
                        EndDate = model.EndDate,//.ConvertTimeToUtc(model.Timezone),
                        FeaturedImageUri = featuredImageUri,
                        IsArchived = false,
                        IsHalted = isHalted,
                        IsDraft = isDraft,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        DisplayDate = model.DisplayDate.ConvertTimeToUtc(model.Timezone)
                    };

                    //Initialise audience model
                    var promotionAudience = new LoyaltyPromotionAudience()
                    {
                        LoyaltyPromotion = dbpromotion,
                        LocationVisibility = model.Audience.LocationVisibility,
                        Distance = model.Audience.Distance,
                        DistanceFactor = model.Audience.DistanceFactor,
                        BusinessLocation = model.Audience.BusinessLocation,
                        IsDeleted = false,
                        CreatedBy = currentUser,
                        CreatedDate = DateTime.UtcNow,
                        LastModifiedBy = currentUser,
                        LastModifiedDate = DateTime.UtcNow,
                    };

                    //Add to DB
                    dbContext.PromotionAudiences.Add(promotionAudience);
                    dbpromotion.Audience = promotionAudience;

                    if (model.Type == VoucherType.ItemDiscount)
                    {
                        List<TraderLocation> locations = new List<TraderLocation>();
                        if (itemDiscountVoucherInfo.Locations != null)
                            foreach (var item in itemDiscountVoucherInfo.Locations)
                            {
                                var location = dbContext.TraderLocations.Find(item.Id);
                                if (location != null)
                                    locations.Add(location);
                            }
                        itemDiscountVoucherInfo.Locations = locations;
                        itemDiscountVoucherInfo.Promotion = dbpromotion;
                        itemDiscountVoucherInfo.CreatedDate = DateTime.UtcNow;
                        itemDiscountVoucherInfo.CreatedBy = currentUser;
                        itemDiscountVoucherInfo.StartTime = fromTime;
                        itemDiscountVoucherInfo.EndTime = toTime;
                        itemDiscountVoucherInfo.DaysAllowed = calWeekDays(itemDiscountVoucherInfo, model.DaysOfweek);
                        itemDiscountVoucherInfo.VoucherExpiryDate = model.VoucherExpiryDate;
                        dbContext.ItemDiscountVoucherInfos.Add(itemDiscountVoucherInfo);
                        dbpromotion.VoucherInfo = itemDiscountVoucherInfo;
                    }
                    else if (model.Type == VoucherType.OrderDiscount)
                    {
                        List<TraderLocation> locations = new List<TraderLocation>();
                        if (itemDiscountVoucherInfo.Locations != null)
                            foreach (var item in orderDiscountVoucherInfo.Locations)
                            {
                                var location = dbContext.TraderLocations.Find(item.Id);
                                if (location != null)
                                    locations.Add(location);
                            }
                        orderDiscountVoucherInfo.Locations = locations;
                        orderDiscountVoucherInfo.Promotion = dbpromotion;
                        orderDiscountVoucherInfo.CreatedDate = DateTime.UtcNow;
                        orderDiscountVoucherInfo.CreatedBy = currentUser;
                        orderDiscountVoucherInfo.StartTime = fromTime;
                        orderDiscountVoucherInfo.EndTime = toTime;
                        orderDiscountVoucherInfo.DaysAllowed = calWeekDays(orderDiscountVoucherInfo, model.DaysOfweek);
                        orderDiscountVoucherInfo.VoucherExpiryDate = model.VoucherExpiryDate;
                        dbContext.OrderDiscountVoucherInfos.Add(orderDiscountVoucherInfo);
                        dbpromotion.VoucherInfo = orderDiscountVoucherInfo;
                    }

                    //Add to DB
                    dbContext.Promotions.Add(dbpromotion);
                }

                return (dbContext.SaveChanges() > 0);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                dbTransaction.Rollback();
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public async Task<ReturnJsonModel> UpdatePromotionPayment(string reference, string status)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, reference);

                    if (!string.IsNullOrEmpty(status))
                    {
                        var paymentReference = await dbContext
                                                        .PromotionPaymentTransactions
                                                        .FirstOrDefaultAsync(x => x.TransactionReference == reference);

                        if (paymentReference != null)
                        {
                            //Get the promotion
                            var dbpromotion = dbContext.Promotions.Find(paymentReference.LoyaltyPromotion.Id);

                            //Update payment status
                            if (status.ToLower() == "success")
                            {
                                paymentReference.Status = PaymentStatus.Completed;

                                //update promotion
                                dbpromotion.IsDraft = false;
                                dbpromotion.IsHalted = false;
                            }
                            else if (status.ToLower() == "error")
                            {
                                paymentReference.Status = PaymentStatus.Failed;
                            }
                            else
                            {
                                //paymentReference.Status = PaymentStatus.;
                                returnJson.msg = "Error processing promotion payment!";
                                return returnJson;
                            }

                            //update modification
                            dbContext.Entry(dbpromotion).State = EntityState.Modified;
                            paymentReference.LastModifiedDate = DateTime.Now;
                            dbContext.Entry(paymentReference).State = EntityState.Modified;

                            //Save changes to DB
                            returnJson.result = await dbContext.SaveChangesAsync() > 0 ? true : false;
                            returnJson.msg = "Error processing promotion payment!";
                            dbTransaction.Commit();
                        }
                        else
                        {
                            returnJson.msg = "Error processing promotion payment!";
                        }
                    }
                    else
                    {
                        returnJson.msg = "Error processing promotion payment!";
                    }
                }
                catch (Exception ex)
                {
                    returnJson.result = false;
                    returnJson.msg = ex.Message;
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, reference);
                    dbTransaction.Rollback();
                }
            }

            return returnJson;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="promotionKey"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public async Task<ReturnJsonModel> RetryPromotionPayment(string promotionKey, string currentUserId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, promotionKey);

                    string[] transactionChannels = { };
                    var promotionId = int.Parse(promotionKey.Decrypt());
                    var dbpromotion = dbContext.Promotions.Where(p => p.Id == promotionId).Include(p => p.PlanType).FirstOrDefault();

                    if (dbpromotion != null)
                    {
                        var paymentReference = await dbContext
                                                        .PromotionPaymentTransactions
                                                        .FirstOrDefaultAsync(x => x.LoyaltyPromotion.Id == dbpromotion.Id);

                        if (paymentReference != null)
                        {
                            //Get the existing promotion payment and change to cancelled
                            paymentReference.Status = PaymentStatus.Cancelled;
                            paymentReference.LastModifiedDate = DateTime.Now;

                            //update modification
                            dbContext.Entry(paymentReference).State = EntityState.Modified;

                            //Save changes to DB
                            returnJson.result = await dbContext.SaveChangesAsync() > 0 ? true : false;
                            returnJson.msg = "Error processing promotion payment!";
                            dbTransaction.Commit();
                        }

                        //Initialise data
                        var currentUser = dbContext.QbicleUser.Find(currentUserId);
                        var currentPromotionType = await dbContext.PromotionTypes.FindAsync(dbpromotion.PlanType.Id);
                        var transactionAmount = currentPromotionType.Price;

                        //Init payment transaction model and save to database
                        // before initialising payment
                        var transaction = new LoyaltyPromotionPaymentTransaction()
                        {
                            LoyaltyPromotion = dbpromotion,
                            Amount = transactionAmount,
                            Status = PaymentStatus.Pending,
                            TransactionReference = string.Empty,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.Now,
                            LastModifiedBy = currentUser,
                            LastModifiedDate = DateTime.Now,
                        };

                        //Add to database
                        dbContext.PromotionPaymentTransactions.Add(transaction);

                        //Save changes to DB
                        await dbContext.SaveChangesAsync();

                        //Init paystack transaction
                        var transactionResponse = await new PayStackRules(dbContext).InitGettingAuthorizationTransaction(transactionAmount, "", "", currentUser.Email, transactionChannels);

                        // Check transaction response status
                        if (transactionResponse.status)
                        {
                            //update reference number
                            transaction.TransactionReference = transactionResponse.data.reference;

                            //Update database changes
                            dbContext.Entry(transaction).State = EntityState.Modified;

                            //Save changes to DB
                            await dbContext.SaveChangesAsync();

                            // Return result
                            returnJson.result = true;
                            //returnJson.msg = "";
                            returnJson.Object = transactionResponse.data;
                        }
                        else
                        {
                            returnJson.result = false;
                            returnJson.msg = "Transaction initalization to get authorization data fails";
                        }
                    }
                    else
                    {
                        returnJson.msg = "Error processing promotion payment!";
                    }
                }
                catch (Exception ex)
                {
                    returnJson.result = false;
                    returnJson.msg = ex.Message;
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, promotionKey);
                    dbTransaction.Rollback();
                }
            }

            return returnJson;
        }

        public ReturnJsonModel SaveBulkDealPromotion(PromotionModel model, ItemDiscountBulkDealVoucherInfo itemDiscountVoucherInfo, string featuredImageUri)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, model);

                    var currentUser = dbContext.QbicleUser.Find(model.CurrentUserId);
                    TimeSpan fromTime = string.IsNullOrEmpty(model.FromTime) ? new TimeSpan() : TimeSpan.Parse(model.FromTime);
                    TimeSpan toTime = string.IsNullOrEmpty(model.ToTime) ? new TimeSpan() : TimeSpan.Parse(model.ToTime);

                    var startDate = DateTime.MinValue;
                    var endDate = DateTime.UtcNow;
                    var dateRange = $"{model.StartDateString}-{model.EndDateString}";
                    dateRange.ConvertDaterangeFormat(model.DateFormat, model.Timezone, out startDate, out endDate, HelperClass.endDateAddedType.none);

                    model.StartDate = startDate;
                    model.EndDate = endDate;
                    model.DisplayDate = model.DisplayDateString.ConvertDateFormat(model.DateFormat).ConvertTimeToUtc(model.Timezone);

                    if (string.IsNullOrEmpty(model.VoucherExpiryDateString))
                        model.VoucherExpiryDate = null;
                    else
                        model.VoucherExpiryDate = model.VoucherExpiryDateString.ConvertDateFormat(model.DateFormat).ConvertTimeToUtc(model.Timezone);

                    var promotionId = int.Parse(model.PromotionKey.Decrypt());
                    var dbpromotion = dbContext.BulkDealPromotions.Find(promotionId);

                    if (dbpromotion != null)
                    {
                        if (dbpromotion.StartDate < DateTime.UtcNow || dbpromotion.IsArchived)
                        {
                            returnJson.msg = ResourcesManager._L("ERROR_MSG_PROMOTIONSTARTED");
                            return returnJson;
                        }

                        dbpromotion.Name = model.Name;
                        dbpromotion.Description = model.Description;
                        dbpromotion.StartDate = model.StartDate.ConvertTimeToUtc(model.Timezone);
                        dbpromotion.EndDate = model.EndDate.ConvertTimeToUtc(model.Timezone);

                        if (!string.IsNullOrEmpty(featuredImageUri)) dbpromotion.FeaturedImageUri = featuredImageUri;

                        dbpromotion.DisplayDate = model.DisplayDate.ConvertTimeToUtc(model.Timezone);

                        if (dbpromotion.BulkDealVoucherInfo.Type == VoucherType.ItemDiscount)
                        {
                            var itemdiscount = dbContext.ItemDiscountVoucherInfos.Find(dbpromotion.BulkDealVoucherInfo.Id);
                            if (itemdiscount != null)
                            {
                                if (itemdiscount.Locations != null)
                                    itemdiscount.Locations.Clear();
                                if (itemdiscount.DaysAllowed != null)
                                    dbContext.LoyaltyWeekDays.RemoveRange(itemdiscount.DaysAllowed);
                                dbContext.ItemDiscountVoucherInfos.Remove(itemdiscount);
                            }
                        }
                        else if (dbpromotion.BulkDealVoucherInfo.Type == VoucherType.OrderDiscount)
                        {
                            var itemdiscount = dbContext.OrderDiscountVoucherInfos.Find(dbpromotion.BulkDealVoucherInfo.Id);
                            if (itemdiscount != null)
                            {
                                if (itemdiscount.Locations != null)
                                    itemdiscount.Locations.Clear();
                                if (itemdiscount.DaysAllowed != null)
                                    dbContext.LoyaltyWeekDays.RemoveRange(itemdiscount.DaysAllowed);
                                dbContext.OrderDiscountVoucherInfos.Remove(itemdiscount);
                            }
                        }

                        if (model.Type == VoucherType.ItemDiscount)
                        {
                            //List<TraderLocation> locations = new List<TraderLocation>();
                            //if (itemDiscountVoucherInfo.Locations != null)
                            //	foreach (var item in itemDiscountVoucherInfo.Locations)
                            //	{
                            //		var location = dbContext.TraderLocations.Find(item.Id);
                            //		if (location != null)
                            //			locations.Add(location);
                            //	}
                            //itemDiscountVoucherInfo.Locations = locations;
                            itemDiscountVoucherInfo.BulkDealPromotion = dbpromotion;
                            itemDiscountVoucherInfo.CreatedDate = DateTime.UtcNow;
                            itemDiscountVoucherInfo.CreatedBy = currentUser;
                            itemDiscountVoucherInfo.StartTime = fromTime;
                            itemDiscountVoucherInfo.EndTime = toTime;
                            itemDiscountVoucherInfo.DaysAllowed = calBulkDealWeekDays(itemDiscountVoucherInfo, model.DaysOfweek);
                            itemDiscountVoucherInfo.VoucherExpiryDate = model.VoucherExpiryDate;
                            dbContext.ItemDiscountBulkDealVoucherInfos.Add(itemDiscountVoucherInfo);
                            dbpromotion.BulkDealVoucherInfo = itemDiscountVoucherInfo;
                        }
                        else if (model.Type == VoucherType.OrderDiscount)
                        {
                        }
                    }
                    else
                    {
                        dbpromotion = new LoyaltyBulkDealPromotion
                        {
                            IsHalted = model.IsHalted,
                            Name = model.Name,
                            Description = model.Description,
                            StartDate = model.StartDate,//.ConvertTimeToUtc(model.Timezone),
                            EndDate = model.EndDate,//.ConvertTimeToUtc(model.Timezone),
                            FeaturedImageUri = featuredImageUri,
                            IsArchived = false,
                            IsDraft = true,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.UtcNow,
                            DisplayDate = model.DisplayDate.ConvertTimeToUtc(model.Timezone)
                        };

                        if (model.Type == VoucherType.ItemDiscount)
                        {
                            itemDiscountVoucherInfo.BulkDealPromotion = dbpromotion;
                            itemDiscountVoucherInfo.CreatedDate = DateTime.UtcNow;
                            itemDiscountVoucherInfo.CreatedBy = currentUser;
                            itemDiscountVoucherInfo.StartTime = fromTime;
                            itemDiscountVoucherInfo.EndTime = toTime;
                            itemDiscountVoucherInfo.DaysAllowed = calBulkDealWeekDays(itemDiscountVoucherInfo, model.DaysOfweek);
                            itemDiscountVoucherInfo.VoucherExpiryDate = model.VoucherExpiryDate;
                            dbContext.ItemDiscountBulkDealVoucherInfos.Add(itemDiscountVoucherInfo);
                            dbpromotion.BulkDealVoucherInfo = itemDiscountVoucherInfo;
                        }
                        else if (model.Type == VoucherType.OrderDiscount)
                        {
                        }
                    }

                    //Add to DB
                    dbContext.BulkDealPromotions.Add(dbpromotion);

                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                    dbTransaction.Rollback();
                }
            }
            return returnJson;
        }

        public List<BulkDealPromotionModelItemOverview> GetItemOverviewItemProductForSearchAndAddBulkDeal(string keysearch, int start = 0, int take = 10)
        {
            var totalRecords = 0;
            var response = new List<BulkDealPromotionModelItemOverview>();
            if (string.IsNullOrEmpty(keysearch)) return response;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, keysearch);
                var traderItems = dbContext.TraderItems.Where(x => x.SKU != null);

                if (traderItems.Any())
                {
                    keysearch = keysearch.ToLower().Trim();
                    traderItems = traderItems.Where(q => q.Name.ToLower().Contains(keysearch) || q.SKU.ToLower().Contains(keysearch)
                                                                                                || q.Domain.Name.ToLower().Contains(keysearch)
                                                                                                || q.Barcode.ToLower().Contains(keysearch) || q.Description.ToLower().Contains(keysearch)
                                                                                                || (q.Group != null && q.Group.Name.ToLower().Contains(keysearch)));

                    if (traderItems.Any())
                    {
                        totalRecords = traderItems.Count();
                        var orderByString = string.Empty;

                        traderItems = traderItems.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                        var lstItems = traderItems.Skip(start).Take(take).ToList();

                        var returnListItem = lstItems.Select(q => new BulkDealPromotionModelItemOverview()
                        {
                            Id = q.Id,
                            ItemId = q.Id,
                            ImageUri = q.ImageUri,
                            ItemName = q.Name,
                            SKU = q.SKU,
                            Barcode = q.Barcode,
                            DomainId = q.Domain.Id,
                            Business = q.Domain.Name,
                            Location = (q.Locations.FirstOrDefault())?.Name ?? "",
                        }).ToList();

                        return returnListItem;
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, keysearch);
            }

            return response;
        }

        public DataTableResult<BulkDealPromotionModelItemOverview> GetItemOverviewItemProductForSearchAndAddBulkDeal(int draw, int start, int length, string searchValue)
        {
            var response = new DataTableResult<BulkDealPromotionModelItemOverview>
            {
                draw = draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new List<BulkDealPromotionModelItemOverview>()
            };
            if (string.IsNullOrEmpty(searchValue)) return response;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, searchValue);
                var traderItems = dbContext.TraderItems.Where(x => x.SKU != null);

                // Apply search filter
                var filteredItems = traderItems.Where(q => q.Name.ToLower().Contains(searchValue) || q.SKU.ToLower().Contains(searchValue)
                                                                                                || q.Domain.Name.ToLower().Contains(searchValue)
                                                                                                || q.Barcode.ToLower().Contains(searchValue) || q.Description.ToLower().Contains(searchValue)
                                                                                                || (q.Group != null && q.Group.Name.ToLower().Contains(searchValue)));
                // Apply pagination
                var orderByString = string.Empty;

                filteredItems = filteredItems.OrderBy(orderByString == string.Empty ? "Id asc" : orderByString);
                var pagedItems = filteredItems.Skip(start).Take(length).ToList();

                response = new DataTableResult<BulkDealPromotionModelItemOverview>
                {
                    draw = draw,
                    recordsTotal = filteredItems.Count(),
                    recordsFiltered = filteredItems.Count(),
                    data = pagedItems.Select(q => new BulkDealPromotionModelItemOverview()
                    {
                        Id = q.Id,
                        ImageUri = q.ImageUri,
                        ItemName = q.Name,
                        SKU = q.SKU,
                        Barcode = q.Barcode,
                        Business = q.Domain.Name,
                        Location = (q.Locations.FirstOrDefault())?.Name ?? "",
                        ItemId = q.Id,
                        DomainId = q.Domain.Id,
                    }).ToList()
                };

                return response;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, searchValue);
            }

            return response;
        }

        public ReturnJsonModel StopStartPromotion(string promotionKey, bool isStop, string message, string currentUserId, int currentDomainId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, promotionKey);
                var promotionId = int.Parse(promotionKey.Decrypt());
                var dbpromotion = dbContext.Promotions.Find(promotionId);
                if (dbpromotion != null)
                {
                    if (isStop)
                        dbpromotion.IsHalted = true;
                    else if (!isStop)
                    {
                        if (DateTime.UtcNow > dbpromotion.EndDate)
                        {
                            returnJson.msg = "ERROR_MSG_CANTNOTSTART";
                            return returnJson;
                        }
                        dbpromotion.IsHalted = false;
                    }
                }
                dbContext.SaveChanges();

                //post the message content to the B2C Qbicle they share with this business. (voucher has not been redeemed)
                new PostsRules(dbContext).SavePostPromotionStartEnd(message, dbpromotion.Vouchers.Where(e => !e.IsRedeemed).Select(u => u.ClaimedBy.Id).ToList(), currentUserId, currentDomainId);

                returnJson.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, promotionKey);
            }
            return returnJson;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="promotionKey"></param>
        /// <returns></returns>
        public ReturnJsonModel ArchivePromotion(string promotionKey)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, promotionKey);
                var promotionId = int.Parse(promotionKey.Decrypt());
                var dbpromotion = dbContext.Promotions.Where(p => p.Id == promotionId).Include(p => p.PlanType).FirstOrDefault();
                if (dbpromotion != null && !dbpromotion.IsArchived && (dbpromotion.EndDate < DateTime.UtcNow || dbpromotion.IsHalted))
                {
                    dbpromotion.IsArchived = true;
                    dbpromotion.ArchivedDate = DateTime.UtcNow;
                }

                dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, promotionKey);
            }

            return returnJson;
        }

        public ReturnJsonModel SetLikingUser(string currentUserId, string promotionKey, bool IsLiked)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, currentUserId, promotionKey);
                var promotionId = int.Parse(promotionKey.Decrypt());
                var user = dbContext.QbicleUser.Find(currentUserId);
                var dbpromotion = dbContext.Promotions.Find(promotionId);
                if (dbpromotion != null && user != null)
                {
                    if (IsLiked)
                    {
                        if (!dbpromotion.LikingUsers.Any(s => s.Id == currentUserId))
                        {
                            dbpromotion.LikingUsers.Add(user);
                        }
                    }
                    else
                    {
                        dbpromotion.LikingUsers.Remove(user);
                    }
                }
                dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                returnJson.result = false;
                returnJson.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId, promotionKey);
            }
            return returnJson;
        }

        public ReturnJsonModel MarkLikePromotion(string currentUserId, string promotionKey, bool IsLiked)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, currentUserId, promotionKey);
                var promotionId = int.Parse(promotionKey.Decrypt());
                var user = dbContext.QbicleUser.Find(currentUserId);
                var dbpromotion = dbContext.Promotions.Find(promotionId);
                if (dbpromotion != null && user != null)
                {
                    if (IsLiked)
                    {
                        if (!dbpromotion.LikedBy.Any(s => s.Id == currentUserId))
                        {
                            dbpromotion.LikedBy.Add(user);
                        }
                    }
                    else
                    {
                        dbpromotion.LikedBy.Remove(user);
                    }
                }
                dbContext.SaveChanges();
                returnJson.result = true;
            }
            catch (Exception ex)
            {
                returnJson.result = false;
                returnJson.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId, promotionKey);
            }
            return returnJson;
        }

        public ReturnJsonModel ClaimPromotion(string currentUserId, string promotionKey, string businessKey)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, currentUserId, promotionKey);
                var user = dbContext.QbicleUser.Find(currentUserId);
                var promotionId = int.Parse(promotionKey.Decrypt());
                var dbpromotion = dbContext.Promotions.Find(promotionId);
                //Check conditions allow this account to be allowed to claim
                if (dbpromotion != null && user != null && !dbpromotion.IsArchived && dbpromotion.CheckAllowClaimNow(currentUserId, DateTime.UtcNow))
                {
                    //If the user is NOT already a TraderContact of the business from which they have claimed the voucher, then the user should be made a TraderContact of the business
                    var promotionDomain = dbpromotion.Domain;

                    var contact = new TraderContactRules(dbContext).GetOrCreateTraderContact(user, promotionDomain);
                    if (contact == null)
                    {
                        return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", "Trader contact of user") };
                    }

                    var voucher = new Voucher
                    {
                        Promotion = dbpromotion,
                        CreatedDate = DateTime.UtcNow,
                        ClaimedBy = user,
                        Code = GeneratePromotionCode(dbpromotion.Domain.Id),
                        VoucherExpiryDate = dbpromotion.VoucherInfo.VoucherExpiryDate ?? dbpromotion.EndDate
                    };
                    dbpromotion.Vouchers.Add(voucher);
                    dbContext.SaveChanges();
                    returnJson.Object = new
                    {
                        code = voucher.Code,
                        canClaim = dbpromotion.Vouchers.Count(s => s.ClaimedBy.Id == currentUserId) < dbpromotion.VoucherInfo.MaxVoucherCountPerCustomer && dbpromotion.VoucherInfo.MaxVoucherCount > dbpromotion.Vouchers.Count(),
                        promoname = dbpromotion.Name,
                        promolimitvoucherpercus = dbpromotion.VoucherInfo.MaxVoucherCountPerCustomer,
                        promoreceived = voucher.CreatedDate.ConvertTimeFromUtc(user.Timezone).ToString((string.IsNullOrEmpty(user.DateFormat) ? "dd/MM/yyyy" : user.DateFormat) + " HH:mm"),
                        promolocations = string.Join(", ", dbpromotion.VoucherInfo.Locations.Select(s => s.Name))
                    };
                    returnJson.result = true;

                    var businessProfileId = int.Parse(businessKey.Decrypt());

                    //The user should be connected to the Business automatically if they are not already

                    var b2cqbicles = dbContext.B2CQbicles.Where(s => !s.IsHidden && s.Customer.Id == currentUserId).Select(s => s.Business.Id).ToList();
                    var b2bProfileConnected = dbContext.B2BProfiles.Any(e => e.Id == businessProfileId && b2cqbicles.Contains(e.Domain.Id));
                    if (!b2bProfileConnected)
                        new C2CRules(dbContext).ConnectC2C(currentUserId, businessProfileId.ToString(), 1);
                }
                else
                {
                    returnJson.result = false;
                    returnJson.msg = ResourcesManager._L("WARNING_MSG_CANNOTCLAIMPROMOTION");
                }
            }
            catch (Exception ex)
            {
                returnJson.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId, promotionKey);
                returnJson.Object = null;
                returnJson.msg = ex.Message;
            }
            return returnJson;
        }

        public ReturnJsonModel RemoveVoucher(string voucherKey)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, voucherKey);
                var voucherId = int.Parse(voucherKey.Decrypt());
                var dbvoucher = dbContext.Vouchers.Find(voucherId);
                if (dbvoucher != null)
                    dbContext.Vouchers.Remove(dbvoucher);
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, voucherKey);
                returnJson.Object = null;
            }
            return returnJson;
        }

        public Voucher GetVoucherByKey(string voucherKey)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, voucherKey);
                var voucherId = int.Parse(voucherKey.Decrypt());
                return dbContext.Vouchers.Find(voucherId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, voucherKey);
                return new Voucher();
            }
        }

        private List<LoyaltyWeekDay> calWeekDays(VoucherInfo voucher, string daysOfweek)
        {
            var weekDays = new List<LoyaltyWeekDay>();
            if (!string.IsNullOrEmpty(daysOfweek))
            {
                var lstDay = daysOfweek.Split(',');
                foreach (var item in lstDay)
                {
                    LoyaltyWeekDay day = new LoyaltyWeekDay();
                    day.Day = item;
                    day.VoucherInfo = voucher;
                    weekDays.Add(day);
                }
            }
            return weekDays;
        }

        private List<LoyaltyBulkDealWeekDay> calBulkDealWeekDays(BulkDealVoucherInfo voucher, string daysOfweek)
        {
            var weekDays = new List<LoyaltyBulkDealWeekDay>();
            if (!string.IsNullOrEmpty(daysOfweek))
            {
                var lstDay = daysOfweek.Split(',');
                foreach (var item in lstDay)
                {
                    LoyaltyBulkDealWeekDay day = new LoyaltyBulkDealWeekDay();
                    day.Day = item;
                    day.BulkDealVoucherInfo = voucher;
                    weekDays.Add(day);
                }
            }
            return weekDays;
        }

        private string GeneratePromotionCode(int currentDomainId)
        {
            RandomGenerator random = new RandomGenerator();
            string code = $"{random.RandomString(4)}{random.RandomNumber(100000, 999999)}";
            int countTimeCheck = 0;
            //Limit maximum 30 time of checks
            while (countTimeCheck < 30)
            {
                if (dbContext.Vouchers.Any(s => s.Promotion.Domain.Id == currentDomainId && s.Code == code))
                    code = $"{random.RandomString(4)}{random.RandomNumber(100000, 999999)}";
                else
                    break;
                countTimeCheck++;
            }
            return code;
        }

        private VoucherOfUserType CalVoucherType(Voucher voucher, DateTime currentDate)
        {
            if (!voucher.IsRedeemed && voucher.VoucherExpiryDate < currentDate)
                return VoucherOfUserType.IsExpired;
            else if (voucher.IsRedeemed)
                return VoucherOfUserType.IsRedeemed;
            else
                return VoucherOfUserType.IsValid;
        }

        #region Promotion Sharing

        public List<ApplicationUser> GetContactsForPromotionShare(string currentUserId)
        {
            var c2cQbicleQuery = dbContext.C2CQbicles.
                Where(s => !s.IsHidden && s.Customers.Any(u => u.Id == currentUserId)).
                Select(p => p.Customers).ToList();
            return c2cQbicleQuery
                .Select(p => p.FirstOrDefault(x => x.Id != currentUserId))
                .OrderBy(p => p.GetFullName())
                .Distinct().ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="type">1-existed contact, 2-email</param>
        /// <param name="email"></param>
        /// <param name="promotionId"></param>
        /// <param name="sharedWithIds"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public async Task<ReturnJsonModel> SharePromotion(int type, string email, string promotionKey, string sharedWithIds, string currentUserId)
        {
            var result = new ReturnJsonModel() { actionVal = 1, result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, type, email, promotionKey, sharedWithIds, currentUserId);

                var promotionId = string.IsNullOrEmpty(promotionKey) ? 0 : int.Parse(promotionKey.Decrypt());

                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var loyaltyPromotion = dbContext.Promotions.FirstOrDefault(p => p.Id == promotionId);
                if (loyaltyPromotion == null)
                {
                    result.result = false;
                    result.msg = "Can not find the loyalty promotion";
                    return result;
                }

                if (type == 1)
                {
                    var query = dbContext.C2CQbicles.Where(p => !p.IsHidden).ToList();
                    var lstShareUserIds = JsonHelper.ParseAs<List<string>>(sharedWithIds);
                    foreach (var userId in lstShareUserIds)
                    {
                        var qbicle = query.Where(p => p.Customers.Any(x => x.Id == userId) && p.Customers.Any(x => x.Id == currentUserId)).FirstOrDefault();
                        if (qbicle == null)
                            continue;
                        qbicle.LastUpdated = DateTime.UtcNow;

                        var sharedWithUser = dbContext.QbicleUser.Find(userId);
                        var shareObj = new LoyaltySharedPromotion()
                        {
                            Topic = new TopicRules(dbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicle.Id),
                            SharedBy = currentUser,
                            SharedWith = sharedWithUser,
                            SharedPromotion = loyaltyPromotion,
                            ActivityType = QbicleActivity.ActivityTypeEnum.SharedPromotion,
                            Qbicle = qbicle,
                            TimeLineDate = DateTime.UtcNow,
                            ShareDate = DateTime.UtcNow,
                            StartedBy = currentUser,
                            StartedDate = DateTime.UtcNow,
                            SharedWithEmail = sharedWithUser?.Email ?? ""
                        };
                        dbContext.SharedPromotions.Add(shareObj);
                        dbContext.Entry(shareObj).State = EntityState.Added;
                        dbContext.SaveChanges();
                    }
                }
                else if (type == 2)
                {
                    var existedUserWithEmail = dbContext.QbicleUser.Where(p => p.Email == email);
                    if (existedUserWithEmail.Count() > 0)
                    {
                        result.result = false;
                        result.msg = "This email address is already associated with a Qbicles user. Please " +
                            "connect with the user in Community before sharing and interacting with them.";
                        return result;
                    }

                    //Send email
                    var shareObj = new LoyaltySharedPromotion()
                    {
                        SharedBy = currentUser,
                        SharedWithEmail = email,
                        ActivityType = QbicleActivity.ActivityTypeEnum.SharedHLPost,
                        TimeLineDate = DateTime.UtcNow,
                        ShareDate = DateTime.UtcNow,
                        StartedDate = DateTime.UtcNow,
                        StartedBy = currentUser,
                        SharedPromotion = loyaltyPromotion
                    };

                    await new EmailRules(dbContext).SendEmailPromotionAsync(shareObj);
                }
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, type, email, promotionKey, sharedWithIds, currentUserId);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        #endregion Promotion Sharing

        public ReturnJsonModel AddToMyStores(string currentUserId, string businessKey)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = true };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, currentUserId);

                var businessProfileId = int.Parse(businessKey.Decrypt());
                //The user should be connected to the Business automatically if they are not already

                var b2cqbicles = dbContext.B2CQbicles.AsNoTracking().Where(s => !s.IsHidden && s.Customer.Id == currentUserId).Select(s => s.Business.Id).ToList();
                var b2bProfileConnected = dbContext.B2BProfiles.Any(e => e.Id == businessProfileId && b2cqbicles.Contains(e.Domain.Id));
                if (!b2bProfileConnected)
                    new C2CRules(dbContext).ConnectC2C(currentUserId, businessProfileId.ToString(), 1);
                else
                    return new ReturnJsonModel() { result = false, msg = "The business has been added to your store" };
            }
            catch (Exception ex)
            {
                returnJson.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId);
                returnJson.msg = ex.Message;
            }
            return returnJson;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ReturnJsonModel SavePromotionType(PromotionTypeModel model)
        {
            //Initialise the json model
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };

            //Init database transactions
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, model);

                    //Get current user details
                    var currentUser = dbContext.QbicleUser.Find(model.LastModifiedBy);

                    //Init model
                    var count = dbContext.PromotionTypes.Count();

                    //Scenario: Restrict the submission of pinned promotion to one
                    //Check if any pinned promotion exist
                    var checkPinnedTypeExist = dbContext.PromotionTypes.Any(p => p.Type == (int)PromotionTypeEnums.Pinned && p.IsActive);
                    if (checkPinnedTypeExist && model.Type == (int)PromotionTypeEnums.Pinned)
                    {
                        returnJson.result = false;
                        returnJson.actionVal = 4;
                        returnJson.msg = "Pinned promotion type already exist!";
                        return returnJson;
                    }

                    //Decrypt the promotion type key
                    var promotionId = int.Parse(model.PromotionTypeKey.Decrypt());

                    //Check if the item already exist
                    var dbpromotion = dbContext.PromotionTypes.Find(promotionId) ?? new LoyaltyPromotionType();

                    //Modify the model property if found
                    //else create new
                    if (dbpromotion.Id > 0)
                    {
                        dbpromotion.Name = model.Name;
                        dbpromotion.Description = model.Description;
                        dbpromotion.Icon = model.Icon;
                        dbpromotion.Type = model.Type;
                        dbpromotion.Duration = model.Duration;
                        dbpromotion.Price = model.Price;
                        dbpromotion.Rank = model.Rank;
                        dbpromotion.LastModifiedBy = currentUser;
                        dbpromotion.LastModifiedDate = DateTime.Now;

                        //Update DB
                        dbContext.Entry(dbpromotion).State = EntityState.Modified;
                    }
                    else
                    {
                        //Create new data with the model
                        dbpromotion = new LoyaltyPromotionType
                        {
                            Name = model.Name,
                            Description = model.Description,
                            Icon = model.Icon,
                            Type = model.Type,
                            Rank = count + 1,
                            Duration = model.Duration,
                            Price = model.Price,
                            IsActive = model.IsActive,
                            CreatedBy = currentUser,
                            CreatedDate = DateTime.Now,
                            LastModifiedBy = currentUser,
                            LastModifiedDate = DateTime.Now,
                        };

                        //Add to DB
                        dbContext.PromotionTypes.Add(dbpromotion);
                    }

                    //Return DB changes
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
                    dbTransaction.Rollback();
                }
            }

            return returnJson;
        }

        /// <summary>
        /// Change the status of the promotion type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReturnJsonModel ManagePromotionTypeStatusById(int id, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = false,
                msg = "An error"
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "", null, null, id, userId);

                var currentUser = dbContext.QbicleUser.Find(userId);

                if (currentUser.IsSystemAdmin)
                {
                    var promotionType = dbContext.PromotionTypes.Find(id);

                    if (promotionType != null)
                    {
                        //
                        var promotionTypes = dbContext.PromotionTypes.Where(x => !x.IsDeleted).AsQueryable();

                        //Update the model
                        promotionType.IsActive = !promotionType.IsActive;
                        promotionType.LastModifiedBy = currentUser;
                        promotionType.LastModifiedDate = DateTime.Now;

                        //When a promotion is deactivated, the promotion ranked below the just deactivated promotion assumes it rank.
                        //When user attempts to activate a deactivated pinned promotion, the checks identified below should be executed;
                        //If there is an active pinned promotion, they are notified; “Unable to activate, check active pinned promotion”
                        //If no pinned promotion is active, they are they are able to successfully activate.

                        if (!promotionType.IsActive)
                        {
                            int totalCount = promotionTypes.Count();
                            var selectedPromotionTypes = promotionTypes
                                                    .Where(x => x.Rank > promotionType.Rank)
                                                    .OrderBy(x => x.Rank)
                                                    .ToList();

                            for (int i = 0; i < selectedPromotionTypes.Count; i++)
                            {
                                selectedPromotionTypes[i].Rank = selectedPromotionTypes[i].Rank - 1;
                                selectedPromotionTypes[i].LastModifiedBy = currentUser;
                                selectedPromotionTypes[i].LastModifiedDate = DateTime.Now;

                                //Save changes to DB
                                dbContext.Entry(selectedPromotionTypes[i]).State = EntityState.Modified;
                            }

                            //USE CASE:
                            //When a deactivated promotion is reactivated, it assumes the last rank position.
                            //Assign last rank position
                            promotionType.Rank = totalCount;

                            //Update DB
                            //dbContext.Entry(selectedPromotionTypes).State = EntityState.Modified;
                        }
                        else
                        {
                            if (promotionType.Type == (int)PromotionTypeEnums.Pinned && promotionTypes.Any(x => x.Type == (int)PromotionTypeEnums.Pinned && x.IsActive))
                            {
                                refModel.msg = "Unable to activate, check active pinned promotion";
                                return refModel;
                            }
                        }

                        if (dbContext.Entry(promotionType).State == EntityState.Detached)
                            dbContext.PromotionTypes.Attach(promotionType);

                        //Save changes to DB
                        dbContext.Entry(promotionType).State = EntityState.Modified;
                        dbContext.SaveChanges();

                        //Update  response
                        refModel.result = true;
                        refModel.msg = promotionType.IsActive ? "Activated" : "Deactivated";
                        refModel.Object = new
                        { Label = promotionType.IsActive ? "Activated" : "Deactivated" };
                    }
                    else
                    {
                        refModel.msg = "Item not found! Please try again";
                    }
                }
                else
                {
                    refModel.msg = ResourcesManager._L("ERROR_MSG_1");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, userId);
            }

            return refModel;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="models"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ReturnJsonModel SaveRankPromotionOrder(List<PromotionTypeModel> models, string userId)
        {
            //Initialise the json model
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };

            //Init database transactions
            using (var dbTransaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet) LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, models);

                    //Get current user
                    var currentUser = dbContext.QbicleUser.Find(userId);

                    //Get current user details
                    foreach (var model in models)
                    {
                        //Decrypt the promotion type key
                        var promotionId = int.Parse(model.PromotionTypeKey.Decrypt());

                        //Check if the item already exist
                        var dbpromotion = dbContext.PromotionTypes.Find(promotionId) ?? new LoyaltyPromotionType();

                        //update rank status
                        dbpromotion.Rank = model.Rank;
                        dbpromotion.LastModifiedBy = currentUser;
                        dbpromotion.LastModifiedDate = DateTime.Now;

                        //Update DB
                        dbContext.Entry(dbpromotion).State = EntityState.Modified;

                        //Return DB changes
                        returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                    }

                    dbTransaction.Commit();
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, models);
                    dbTransaction.Rollback();
                }
            }

            return returnJson;
        }
    }

    public class DataTableResult<T>
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<T> data { get; set; }
    }
}