using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.C2C;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.BusinessRules.HelperClass;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroCommunityRules : MicroRulesBase
    {
        public MicroCommunityRules(MicroContext microContext) : base(microContext)
        {
        }
        public object GetCommunityOption()
        {

            var uiSettings = new QbicleRules(dbContext).LoadUiSettings(SystemPageConst.C2C, CurrentUser.Id);

            var orderby = uiSettings.FirstOrDefault(s => s.Key == C2CStoreUiSettingsConst.ORDERBY)?.Value ?? "0";
            var contactType = uiSettings.FirstOrDefault(s => s.Key == C2CStoreUiSettingsConst.CONTACTTYPE)?.Value ?? "0";
            var includeBlocked = uiSettings.FirstOrDefault(s => s.Key == C2CStoreUiSettingsConst.INCLUDEBLOCKED)?.Value ?? "true";
            var onlyShowFavourites = uiSettings.FirstOrDefault(s => s.Key == C2CStoreUiSettingsConst.ONLYSHOWFAVOURITES)?.Value ?? "false";

            var orders = new List<SelectFilter>
            {
                new SelectFilter{Id=0, Name="Order by latest activity",Selected= orderby=="0"},
                new SelectFilter{Id=1, Name="Order by forename A-Z",Selected= orderby=="1"},
                new SelectFilter{Id=2, Name="Order by forename Z-A",Selected= orderby=="2"},
                new SelectFilter{Id=3, Name="Order by surname A-Z",Selected= orderby=="3"},
                new SelectFilter{Id=4, Name="Order by surname Z-A",Selected= orderby=="4"},
                new SelectFilter{Id=5, Name="Order by date added",Selected= orderby=="5"}
            };
            var contactTypes = new List<SelectFilter>
            {
                new SelectFilter{Id=0, Name="Show all contact types",Selected= contactType=="0"},
                new SelectFilter{Id=1, Name="Only businesses",Selected= contactType=="2"},
                new SelectFilter{Id=2, Name="Only individuals",Selected= contactType=="1"},
                new SelectFilter{Id=3, Name ="Only pending",Selected= contactType=="3"}
            };


            var refModel = new
            {
                Orders = orders,
                ContacTypes = contactTypes,
                OnlyShowFavourites = bool.Parse(onlyShowFavourites),
                IncludeBlockedUsers = bool.Parse(includeBlocked),
                ContactCount = GetContactCount()
            };

            return refModel;
        }


        private int GetContactCount()
        {
            var query = from c in dbContext.CQbicles
                        join c2c in dbContext.C2CQbicles on c.Id equals c2c.Id into dept
                        from c2c in dept.DefaultIfEmpty()
                        join b2c in dbContext.B2CQbicles on c.Id equals b2c.Id into dept1
                        from b2c in dept1.DefaultIfEmpty()
                        join b2bprofile in dbContext.B2BProfiles on b2c.Business.Id equals b2bprofile.Domain.Id into dept2
                        from b2bprofile in dept2.DefaultIfEmpty()
                        where !c.IsHidden
                        && ((c2c != null && c2c.Customers.Any(u => u.Id == CurrentUser.Id)) || (b2c != null && b2c.Customer.Id == CurrentUser.Id))
                        && (!c.RemovedForUsers.Any(p => p.Id == CurrentUser.Id))
                        select new
                        {
                            Type = c2c == null ? 1 : 2,//1:businesses, 2:individuals                            
                        };

            return query.Where(e => e.Type == 2).Count();
        }









        public object GetCommunityConnectOption()
        {
            var refModel = new
            {
                ContactTypes = EnumModel.ConvertEnumToList<FindContactType>(),
                PeopleTypes = EnumModel.ConvertEnumToList<FindPeopleType>(),
            };

            return refModel;
        }


        public object GetMyOrderOption()
        {
            var refModel = new
            {
                SalesChannels = new object[]{
                new {key=0,Value="Show all"},
                new {key=SalesChannelEnum.B2C,Value=SalesChannelEnum.B2C.GetDescription()},
                new {key=SalesChannelEnum.POS,Value=SalesChannelEnum.POS.GetDescription()},
                },
                OrderStatuses = new object[]{
                new {key=0,Value="Show all"},
                new {key=TradeOrderStatusEnum.Draft,Value=TradeOrderStatusEnum.Draft.GetDescription()},
                new {key=TradeOrderStatusEnum.AwaitingProcessing,Value=TradeOrderStatusEnum.AwaitingProcessing.GetDescription()},
                new {key=TradeOrderStatusEnum.InProcessing,Value=TradeOrderStatusEnum.InProcessing.GetDescription()},
                new {key=TradeOrderStatusEnum.Processed,Value=TradeOrderStatusEnum.Processed.GetDescription()},
                new {key=TradeOrderStatusEnum.ProcessedWithProblems,Value=TradeOrderStatusEnum.ProcessedWithProblems.GetDescription()},
                }
            };

            return refModel;
        }


        public ReturnJsonModel LikeUnLikeCommunity(MicroCommunity like, bool isLike)
        {
            return new C2CRules(dbContext).SetLikeBy(like.QbicleId, CurrentUser.Id, like.LinkId, like.Type, isLike);
        }
        /// <summary>
        /// C2C Accept/Unblock, Decline/Block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ReturnJsonModel AcceptDeclineC2CCommunity(MicroCommunity block, CommsStatus status, string ogrConnection = "")
        {
            return new C2CRules(dbContext).SetStatusBy(block.QbicleId, CurrentUser.Id, status, ogrConnection);
        }

        /// <summary>
        /// B2C Accept/Unblock, Decline/Block
        /// </summary>
        /// <param name="block"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ReturnJsonModel AcceptDeclineB2CCommunity(MicroCommunity block, CommsStatus status)
        {
            return new B2CRules(dbContext).SetStatusByCustomer(block.QbicleId, CurrentUser.Id, status);
        }

        public ReturnJsonModel RemoveB2CQbicleById(int qId)
        {
            return new B2CRules(dbContext).RemoveB2CQbicleById(qId, CurrentUser.Id);
        }

        public ReturnJsonModel RemoveC2CQbicleById(int qId)
        {
            return new C2CRules(dbContext).RemoveC2CQbicleById(qId, CurrentUser.Id);
        }

        public object CommunityShoppingCategories()
        {
            return dbContext.BusinessCategories.Select(c => new { c.Id, c.Name });
        }

        public object CommunityShopping(ShoppingFilter filter)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, filter);

            var request = new FindBusinessStoresRequest
            {
                AreaOfOperation = filter.LocationId,
                currentUserId = CurrentUser.Id,
                keyword = filter.Keyword,
                limitMyConnections = filter.LimitMyConnections,
                pageNumber = filter.PageIndex + 1,
                pageSize = filter.PageSize == 0 ? HelperClass.activitiesPageSize : filter.PageSize,
                categoryIds = string.Join(",", filter.CategoryIds)
            };

            var sh = new B2CRules(dbContext).GetBusinessStores(request);

            return new
            {
                TotalPage = sh.totalPage,
                Shops = sh
            };
        }

        public MicroConnect MicroGetConnectes(FindPeopleRequest request)
        {
            var response = new MicroConnect();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, request);

                request.CurrentUserId = CurrentUser.Id;
                request.CurrentBusinessId = 0;
                request.pageSize = HelperClass.activitiesPageSize;
                request.pageNumber++;
                var result = new C2CRules(dbContext).FindPeople(request);
                response.TotalPage = result.totalNumber / request.pageSize;

                foreach (var item in result.items)
                {
                    var c = (PeopleInfoModel)item;
                    var connected = new MicroCommunityConnect
                    {
                        DomainKey = c.DomainKey.Encrypt(),
                        Id = c.UserId,
                        Name = c.FullName,
                        Type = c.Type
                    };
                    if (c.Type == 1)
                        connected.TypeName = "Business";
                    else
                        connected.TypeName = "Individual";
                    connected.AvatarUri = c.AvatarUri;

                    if (string.IsNullOrEmpty(c.AvatarUri))
                        connected.AvatarUri = ConfigManager.DefaultUserUrlGuid;
                    else if (c.AvatarUri.Contains("avatar.jpg"))
                        connected.AvatarUri = ConfigManager.DefaultUserUrlGuid;
                    connected.Image = connected.AvatarUri.ToDocumentUri();

                    response.Connectes.Add(connected);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
            }
            return response;
        }

        public ReturnJsonModel Connect2Community(MicroCommunityConnect connect)
        {
            return new C2CRules(dbContext).ConnectC2C(CurrentUser.Id, connect.Id, connect.Type);
        }

        public ReturnJsonModel GetMyOrders(MyOrderFilter filter)
        {
            try
            {

                int totalcount = 0;
                #region Filters
                var query = from od in dbContext.TradeOrders.Where(s => s.Customer.Id == CurrentUser.Id
                            && s.SellingDomain != null && s.BuyingDomain == null)
                            join pr in dbContext.B2BProfiles on od.SellingDomain.Id equals pr.Domain.Id
                            select new MyOrder
                            {
                                OrderId = od.Id,
                                FullRef = od.OrderReference.FullRef,
                                Placed = od.CreateDate,
                                Status = od.OrderStatus,
                                BusinessId = od.SellingDomain.Id,
                                DomainId = pr.Domain.Id,
                                BusinessLogoUri = pr.LogoUri,
                                BusinessName = pr.BusinessName,
                                Channel = od.SalesChannel
                            };
                if (!string.IsNullOrEmpty(filter.Keyword))
                    query = query.Where(s => s.FullRef.Contains(filter.Keyword) || s.BusinessName.Contains(filter.Keyword));

                if (!string.IsNullOrEmpty(filter.Daterange))
                {
                    var startDate = DateTime.UtcNow;
                    var endDate = DateTime.UtcNow;
                    filter.Daterange.ConvertDaterangeFormat(CurrentUser.DateFormat, CurrentUser.Timezone, out startDate, out endDate, HelperClass.endDateAddedType.day);
                    query = query.Where(s => s.Placed >= startDate && s.Placed < endDate);
                }
                if (filter.SaleChannel > 0)
                    query = query.Where(s => s.Channel == (SalesChannelEnum)filter.SaleChannel);

                if (filter.OrderStatus >= 0)
                    query = query.Where(s => s.Status == (TradeOrderStatusEnum)filter.OrderStatus);



                totalcount = query.Count() / HelperClass.activitiesPageSize;
                #endregion
                #region Sorting


                query = query.OrderBy("Placed desc");
                #endregion
                #region Paging
                var list = query.Skip(filter.PageIndex * HelperClass.activitiesPageSize).Take(HelperClass.activitiesPageSize).ToList();
                #endregion

                var dataJson = list.Select(s => new
                {
                    s.OrderId,
                    s.FullRef,
                    Placed = s.Placed.ConvertTimeFromUtc(CurrentUser.Timezone).ToString("ddnn MMMM yyyy, hh:mmtt", true),
                    Status = s.Status.GetDescription(),
                    s.BusinessId,
                    BusinessKey = s.BusinessId.Encrypt(),
                    s.DomainId,
                    DomainKey = s.DomainId.Encrypt(),
                    BusinessLogoUri = s.BusinessLogoUri.ToDocumentUri(Enums.FileTypeEnum.Image, "T"),
                    s.BusinessName,
                    Channel = s.Channel.ToString(),
                    QbicleId = new B2CRules(dbContext).Get2CQbicleByBusinessIdAndCustomerId(s.BusinessId, CurrentUser.Id)?.Id ?? 0
                });
                return new ReturnJsonModel
                {
                    result = true,
                    Object = new
                    {
                        Orders = dataJson,
                        TotalPage = totalcount
                    }
                };


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        public object GetMyOrderDetail(int id)
        {
            var tradeOrder = new B2CRules(dbContext).GetTradeOrderById(id);

            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(tradeOrder.SellingDomain.Id);
            var businessProfile = tradeOrder.SellingDomain?.Id.BusinesProfile();

            var _order = JsonHelper.ParseAs<Models.TraderApi.Order>(tradeOrder.OrderJson);
            if (_order == null)
            {
                _order = new Models.TraderApi.Order();
            }
            decimal cartTotal = 0;
            _order.Items.ForEach(it =>
            {
                cartTotal += it.Variant.AmountInclTax * it.Quantity;
                it.Extras.ForEach(e =>
                {
                    cartTotal += e.AmountInclTax * it.Quantity;
                });
            });

            var myOrder = new MicroMyOrderDetail
            {
                Ref = $"Order #{tradeOrder.OrderReference.FullRef}",
                BusinessLogo = businessProfile.BannerUri.ToDocumentUri(Enums.FileTypeEnum.Image, "T"),
                BusinessName = businessProfile.BusinessName,
                Placed = tradeOrder.CreateDate.ConvertTimeFromUtc(CurrentUser.Timezone).ToString("ddnn MMMM yyyy, hh:mmtt", true),
                StatusName = tradeOrder.OrderStatus.GetDescription(),
                Status = (int)tradeOrder.OrderStatus,
                Total = cartTotal.ToCurrencySymbol(currencySetting),
                Items = new List<MicroMyOrderItem>(),
                ShippingAddress = _order.Customer?.Address?.ToAddress(),
                ShippingNote = _order.Notes
            };

            _order.Items.ForEach(orderItem =>
            {
                decimal discount = 0;
                var taxes = new List<Tax>();

                decimal subTotal = orderItem.Variant.AmountInclTax * orderItem.Quantity;
                decimal subTotalWithoutDiscount = 0;

                decimal variantPrice = orderItem.Variant.AmountInclTax;
                discount += orderItem.Variant.Discount;
                taxes.AddRange(orderItem.Variant.Taxes);
                foreach (var itemExtras in orderItem.Extras)
                {
                    subTotal += itemExtras.AmountInclTax * orderItem.Quantity;
                    discount += itemExtras.Discount;
                    taxes.AddRange(itemExtras.Taxes);
                }

                if (orderItem.Variant.Discount < 100)
                {
                    subTotalWithoutDiscount = (subTotal / (1 - orderItem.Variant.Discount / 100));

                }


                if (orderItem.Variant.Discount < 100)
                {
                    variantPrice = variantPrice / (1 - orderItem.Variant.Discount / 100);
                    foreach (var extraItem in orderItem.Extras)
                    {
                        extraItem.AmountInclTax = extraItem.AmountInclTax / (1 - orderItem.Variant.Discount / 100);
                    }
                }

                var taxInfo = "";
                if (taxes == null || taxes.Count <= 0)
                {
                    taxInfo = "--";
                }
                else
                {
                    taxInfo = taxes.Sum(s => s.AmountTax).ToDecimalPlace(currencySetting); ;
                }

                var item = new MicroMyOrderItem
                {
                    Name = orderItem.Name,
                    Quantity = orderItem.Quantity.ToDecimalPlace(currencySetting),
                    Discount = discount.ToDecimalPlace(currencySetting),
                    Total = subTotal.ToDecimalPlace(currencySetting),
                    TotalWithoutDiscount = currencySetting.CurrencySymbol + subTotalWithoutDiscount.ToDecimalPlace(currencySetting),
                    Variant = new MicroMyOrderItemVariantExtra
                    {
                        Name = orderItem.Variant.Name,
                        Price = variantPrice.ToCurrencySymbol(currencySetting)
                    },
                    Extras = new List<MicroMyOrderItemVariantExtra>(),
                    Taxes = taxInfo
                };
                orderItem.Extras.ForEach(ext =>
                {
                    item.Extras.Add(
                        new MicroMyOrderItemVariantExtra
                        {
                            Name = $"+ {ext.Name}",
                            Price = ext.AmountInclTax.ToCurrencySymbol(currencySetting)
                        });
                });
                myOrder.Items.Add(item);
            });
            return myOrder;
        }


        /// <summary>
        /// Mapping contact from Phone and contact in Qbicles
        /// </summary>
        /// <param name="phoneContacts"></param>
        /// <returns></returns>
        public MicroContactImport ImportPhoneContacts(List<MicroContact> phoneContacts)
        {
            var currentUserId = CurrentUser.Id;

            var import = new MicroContactImport
            {
                InviteUrl = $"{ConfigManager.QbiclesUrl}/registration?connectcode={currentUserId.Encrypt()}"
            };
            var dbUsers = dbContext.QbicleUser.ToList();

            var contactPotentials = new List<MicroContact>();

            phoneContacts.ForEach(contact =>
            {
                if (dbUsers.FirstOrDefault(u => u.Email == contact.Email || u.PhoneNumber == contact.Phone) == null)
                    import.InviteAsNews.Add(contact);
                else
                    contactPotentials.Add(contact);
            });

            var phones = contactPotentials.Select(e => e.Phone).ToList();
            var emails = contactPotentials.Select(e => e.Email).ToList();

            var queryUsersInRelationShip = dbContext.C2CQbicles.Where(s => !s.IsHidden && s.Customers.Any(u => u.Id == currentUserId))
                .SelectMany(s => s.Customers.Select(u => new PeopleInfoModel
                {
                    UserId = u.Id,
                    AvatarUri = u.ProfilePic,
                    FullName = (u.Forename != null ? u.Forename + " " + u.Surname : u.DisplayUserName),
                    Email = u.Email,
                    Phone = u.PhoneNumber,
                    HasConnected = true,
                    HasDefaultB2CRelationshipManager = true
                }));


            var query = dbUsers
                .Select(u => new PeopleInfoModel
                {
                    UserId = u.Id,
                    AvatarUri = u.ProfilePic,
                    FullName = (u.Forename != null ? u.Forename + " " + u.Surname : u.DisplayUserName),
                    Email = u.Email,
                    Phone = u.PhoneNumber,
                    HasConnected = queryUsersInRelationShip.Any(x => x.UserId != currentUserId && x.UserId == u.Id),
                    HasDefaultB2CRelationshipManager = true
                });
            query = query.Where(p => p.UserId != currentUserId).Distinct();


            //get not connected
            query = query.Where(e => !e.HasConnected);
            query = query.Where(e => emails.Contains(e.Email) || phones.Contains(e.Phone));

            query = query.OrderBy(s => s.FullName);

            import.Potentials = query.Select(connect => new MicroContact
            {
                Id = connect.UserId,
                Name = connect.FullName,
                AvatarUri = connect.AvatarUri.ToUri(),
                Email = connect.Email,
                Phone = connect.Phone
            }).ToList();

            return import;
        }

        /// <summary>
        /// Connect contact to C2C
        /// </summary>
        /// <param name="contacts"></param>
        public void Connect2Contacts(List<MicroContact> contacts)
        {
            var rules = new C2CRules(dbContext);
            contacts.ForEach(contact =>
            {
                rules.ConnectC2C(CurrentUser.Id, contact.Id, 2);
            });
        }

        public void SendEmailImportInvite(List<MicroContact> contacts)
        {
            new EmailRules(dbContext).SendEmailImportInvite(CurrentUser, contacts, true);
        }

        /// <summary>
        /// for business
        /// </summary>
        /// <param name="domainKey"></param>
        /// <param name="qbicleKey"></param>
        /// <returns></returns>
        public object GetInitB2CCommAddOrder(string domainKey, string qbicleKey)
        {
            var id = int.Parse(domainKey.Decrypt());
            var qbicleId = int.Parse(qbicleKey.Decrypt());
            var reference = new TraderReferenceRules(dbContext).GetNewReference(id, TraderReferenceType.Order);

            List<int> lids = dbContext.TraderLocations.Where(d => d.Domain.Id == id).Select(s => s.Id).ToList();
            var products = new PosMenuRules(dbContext).FiltersCatalog(lids, "", true, (int)SalesChannelEnum.B2C);

            var orderInit = new
            {
                Reference = reference.FullRef,
                ReferenceKey = reference.Key,
                Catalogues = products.Select(c => new
                {
                    c.Key,
                    c.Name,
                    c.Image
                }),
                Customer = dbContext.B2CQbicles.FirstOrDefault(e => e.Id == qbicleId)?.Customer?.GetFullName()
            };


            return orderInit;
        }

        /// <summary>
        /// Create new order from B2C Manager
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReturnJsonModel CreateB2CCommAddOrder(B2OrderParameter parameter, string userId, bool isCreatorTheCustomer)
        {
            var order = new B2OrderCreationDiscussionModel
            {
                QbicleId = parameter.Key.Decrypt2Int(),
                MenuId = parameter.MenuKey.Decrypt2Int(),
                OpeningComment = parameter.OpeningComment,
                OrderReferenceId = parameter.ReferenceKey.Decrypt2Int(),
            };
            return new DiscussionsRules(dbContext).SaveDiscussionForOrderCreation(order, userId, isCreatorTheCustomer, parameter.OriginatingConnectionId, AppType.Micro);
        }

        /// <summary>
        /// promotecatalogue - creating: get location by domain for display on UI dialog
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        public object B2CPromoteCatalogueGetLocation(string domainKey)
        {
            var id = int.Parse(domainKey.Decrypt());
            return dbContext.TraderLocations.Where(d => d.Domain.Id == id).OrderBy(n => n.Name).ToList().Select(l => new { LocationKey = l.Key, l.Name });
        }

        /// <summary>
        /// Get catalog menu from Domain - display on UI modal
        /// </summary>
        /// <param name="locationKey"></param>
        /// <returns></returns>
        public object B2CPromoteGetCatalogues(string domainKey)
        {
            var domainId = int.Parse(domainKey.Decrypt());
            var query = dbContext.PosMenus.Where(e => e.Location.Domain.Id == domainId && !e.IsDeleted && e.SalesChannel == SalesChannelEnum.B2C);

            query.ForEach(catalog =>
            {
                var catalogItemImage = string.IsNullOrEmpty(catalog.Image) ? ConfigManager.CatalogDefaultImage : catalog.Image;
                catalog.Image = catalogItemImage.ToUriString();
            });
            return query.OrderBy(d => d.Name).ToList().Select(c => new { MenuKey = c.Key, c.Name, c.Image });

        }

        /// <summary>
        /// Get catalog menu from location - display on UI modal
        /// </summary>
        /// <param name="locationKey"></param>
        /// <returns></returns>
        public object B2CPromoteGetLocationCatalogues(string locationKey)
        {
            var locationId = int.Parse(locationKey.Decrypt());
            var query = dbContext.PosMenus.Where(e => e.Location.Id == locationId && !e.IsDeleted && e.SalesChannel == SalesChannelEnum.B2C);

            query.ForEach(catalog =>
            {
                var catalogItemImage = string.IsNullOrEmpty(catalog.Image) ? ConfigManager.CatalogDefaultImage : catalog.Image;
                catalog.Image = catalogItemImage.ToUriString();
            });
            return query.OrderBy(d => d.Name).ToList().Select(c => new { MenuKey = c.Key, c.Name, c.Image });

        }
        /// <summary>
        ///  Create new promotecatalogue
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReturnJsonModel B2CCreatePromoteCatalogue(B2OrderParameter parameter, string userId)
        {
            var model = new B2CProductMenuDiscussionModel
            {
                OriginatingConnectionId = parameter.OriginatingConnectionId,
                QbicleId = parameter.Key.Decrypt2Int(),
                MenuId = parameter.MenuKey.Decrypt2Int(),
                OpeningComment = parameter.OpeningComment
            };
            return new DiscussionsRules(dbContext).SaveDiscussionForProductMenu(model, userId, parameter.OriginatingConnectionId);
        }
        /// <summary>
        /// Browse promote catalog
        /// </summary>
        /// <param name="discussionKey"></param>
        /// <returns></returns>
        public object B2CPromoteCatalogueBrowse(string discussionKey)
        {
            var disId = int.Parse(discussionKey.Decrypt());
            var discussion = new DiscussionsRules(dbContext).GetDiscussionProductMenuById(disId);

            return new
            {
                discussion.Key,
                discussion.Name,
                Location = discussion.ProductMenu.Location.Name,
                discussion.Summary,
                Categories = discussion.ProductMenu.Categories.Select(c => new { c.Id, c.Name })
            };
        }
        /// <summary>
        /// Browse promote catalog - get items from catalogues
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PaginationResponse B2CPromoteCatalogueItems(B2CMenuItemsRequestModel request)
        {
            request.bdomainId = int.Parse(request.DomainKey.Decrypt());
            return new PosMenuRules(dbContext).LoadMenuItems(request, true);
        }





        /// <summary>
        /// Get detail response for api/micro/b2c/comms/communicate/detail and api/micro/community/detail
        /// Will be delete until Micro app implemented new api
        /// </summary>
        /// <param name="fillterModel"></param>
        /// <returns></returns>
        public object GetCommunityDetail(QbicleFillterModel fillterModel)
        {
            var streamsResponse = new List<MicroActivitesStream>();

            var streams = new MicroQbicleStream();

            var communityDefault = new MicroCommunityDefault();
            var businessName = "";

            var qbRule = new QbicleRules(dbContext);
            fillterModel.UserId = CurrentUser.Id;

            var isHidden = false;
            C2CQbicle c2cqbicle = null;
            B2CQbicle b2cqbicle = null;
            if (fillterModel.Type == 2)
            {
                c2cqbicle = new C2CRules(dbContext).GetC2CQbicleById(fillterModel.QbicleId);
                if (c2cqbicle == null)
                    return new { communityDefault, streamsResponse, totalPage = 0 };
            }
            else if (fillterModel.Type == 1)
            {
                b2cqbicle = new B2CRules(dbContext).GetB2CQbicleById(fillterModel.QbicleId);
                if (b2cqbicle == null)
                    return new { communityDefault, streamsResponse, totalPage = 0 };

                b2cqbicle.BusinessViewed = true;
                if (b2cqbicle.IsNewContact == true)
                {
                    b2cqbicle.IsNewContact = false;
                }
                dbContext.SaveChanges();
            }

            Models.ApplicationUser linkUser = null;
            string forename = "";
            if (c2cqbicle != null && c2cqbicle.Status == CommsStatus.Pending)
            {
                linkUser = c2cqbicle.Customers.Where(s => s.Id != CurrentUser.Id).FirstOrDefault();
                forename = !string.IsNullOrEmpty(linkUser.Forename) ? linkUser.Forename : linkUser.DisplayUserName;
                if (c2cqbicle.Source.Id == CurrentUser.Id)
                {
                    communityDefault.Image = ConfigManager.CommunityPendingadd.ToDocumentUri();
                    communityDefault.Title = ResourcesManager._L("COM_C2C_PENDING_C_TITLE", forename);
                    communityDefault.Description = ResourcesManager._L("COM_C2C_PENDING_C_DESCRIPTION", forename);
                    communityDefault.Action = new List<string> { "CancelRequest" };
                }
                else
                {
                    communityDefault.Image = ConfigManager.CommunityLestTalk.ToDocumentUri();
                    communityDefault.Title = ResourcesManager._L("COM_C2C_PENDING_B_TITLE", forename);
                    communityDefault.Description = ResourcesManager._L("COM_C2C_PENDING_B_DESCRIPTION");
                    communityDefault.Action = new List<string> { "Accept", "Decline" };
                }
                communityDefault.CreatedDate = c2cqbicle.StartedDate.ConvertTimeFromUtc(CurrentUser.Timezone).ToString($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}");
                return new { communityDefault, streamsResponse, totalPage = 0 };
            }


            fillterModel.Size *= HelperClass.qbiclePageSize;
            var model = new QbicleRules(dbContext).GetQbicleStreams(fillterModel, CurrentUser.Timezone, CurrentUser.DateFormat);

            if (model == null)
                return new { communityDefault, streamsResponse, totalPage = 0 };

            model.TotalCount /= HelperClass.qbiclePageSize;

            if (model.Dates != null)
                streams = model.ToMicro(CurrentUser.Timezone, CurrentUser.DateFormat, CurrentUser.Id);
            else
                streams = new MicroQbicleStream
                {
                    MicroActivities = new List<MicroDatesQbicleStream>(),
                    TotalCount = model.TotalCount
                };
            if (c2cqbicle != null)
            {
                isHidden = c2cqbicle.IsHidden;

                linkUser = c2cqbicle.Customers.Where(s => s.Id != CurrentUser.Id).FirstOrDefault();
                forename = !string.IsNullOrEmpty(linkUser.Forename) ? linkUser.Forename : linkUser.DisplayUserName;

                communityDefault.CreatedDate = c2cqbicle.StartedDate.ConvertTimeFromUtc(CurrentUser.Timezone).ToString($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}");

                if (c2cqbicle.Status == CommsStatus.Blocked && fillterModel.Size == 0)
                {
                    if (c2cqbicle.Blocker.Id == CurrentUser.Id)
                    {
                        communityDefault.Image = ConfigManager.CommunityPendingadd.ToDocumentUri();
                        communityDefault.Title = ResourcesManager._L("COM_C2C_BLOCK_B_TITLE");
                        communityDefault.Description = ResourcesManager._L("COM_C2C_BLOCK_B_DESCRIPTION");
                        communityDefault.Action = new List<string> { "Unblock" };
                    }
                    else
                    {
                        communityDefault.Image = ConfigManager.CommunityPendingadd.ToDocumentUri();
                        communityDefault.Title = ResourcesManager._L("COM_C2C_BLOCK_C_TITLE", forename);
                        communityDefault.Description = ResourcesManager._L("COM_C2C_BLOCK_C_DESCRIPTION");
                        communityDefault.Action = new List<string> { "RemoveContact" };
                    }
                }
            }
            else if (b2cqbicle != null)
            {
                isHidden = b2cqbicle.IsHidden;
                communityDefault.CreatedDate = b2cqbicle.StartedDate.ConvertTimeFromUtc(CurrentUser.Timezone).ToString($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}");
                if (b2cqbicle.Status == CommsStatus.Blocked && fillterModel.Size == 0)
                {
                    communityDefault.Image = ConfigManager.CommunityBlocked.ToDocumentUri();

                    if (b2cqbicle.Blocker != null)
                    {
                        communityDefault.Title = ResourcesManager._L("COM_B2C_BLOCK_B_TITLE");
                        communityDefault.Description = ResourcesManager._L("COM_B2C_BLOCK_B_DESCRIPTION");
                        communityDefault.Action = new List<string> { "Accept" };
                    }
                    else
                    {
                        businessName = new CommerceRules(dbContext).GetB2bBusinessNameById(b2cqbicle.Business?.Id ?? 0);

                        communityDefault.Title = ResourcesManager._L("COM_B2C_BLOCK_C_TITLE");
                        communityDefault.Description = ResourcesManager._L("COM_B2C_BLOCK_C_DESCRIPTION", businessName);
                        communityDefault.Action = new List<string> { "RemoveContact" };
                    }
                }
                else if (model.TotalCount == 0 && fillterModel.Size == 0)
                {
                    businessName = new CommerceRules(dbContext).GetB2bBusinessNameById(b2cqbicle.Business?.Id ?? 0);

                    communityDefault.Title = ResourcesManager._L("COM_B2B_LETTALK_TITLE"); ;
                    communityDefault.Image = ConfigManager.CommunityLestTalk.ToDocumentUri();
                    communityDefault.Description = ResourcesManager._L("COM_B2B_LETTALK_DESCRIPTION", businessName);
                    communityDefault.Action = new List<string>();
                };
            }


            streamsResponse = streams.MicroActivities.SelectMany(a => a.Activities).Where(e => e.SalesChannel != SalesChannelEnum.POS).ToList();

            return new { communityDefault, streams = streamsResponse, totalPage = streams.TotalCount };
        }

        public object GetCommunityDetailPagging(CommunityPaggingModel fillterModel)
        {
            var communityDefault = new MicroCommunityDefault();
            var businessName = "";

            var streams = new List<MicroActivitesStream>();
            var currentUser = CurrentUser;

            fillterModel.UserId = currentUser.Id;
            C2CQbicle c2cqbicle = null;
            B2CQbicle b2cqbicle = null;

            if (fillterModel.Type == 2)
            {
                c2cqbicle = new C2CRules(dbContext).GetC2CQbicleById(fillterModel.QbicleId);
                if (c2cqbicle == null)
                    return new { communityDefault, streams, totalPage = 0 };
            }
            else if (fillterModel.Type == 1)
            {
                b2cqbicle = new B2CRules(dbContext).GetB2CQbicleById(fillterModel.QbicleId);
                if (b2cqbicle == null)
                    return new { communityDefault, streams, totalPage = 0 };
            }
            ApplicationUser linkUser = null;
            string forename = "";
            if (c2cqbicle != null && c2cqbicle.Status == CommsStatus.Pending)
            {
                if (c2cqbicle.Source.Id == currentUser.Id)
                {
                    communityDefault.Image = ConfigManager.CommunityPendingadd.ToDocumentUri();
                    communityDefault.Title = ResourcesManager._L("COM_C2C_PENDING_C_TITLE", forename);
                    communityDefault.Description = ResourcesManager._L("COM_C2C_PENDING_C_DESCRIPTION", forename);
                    communityDefault.Action = new List<string> { "CancelRequest" };
                }
                else
                {
                    linkUser = c2cqbicle.Customers.Where(s => s.Id != currentUser.Id).FirstOrDefault();
                    forename = !string.IsNullOrEmpty(linkUser.Forename) ? linkUser.Forename : linkUser.DisplayUserName;
                    communityDefault.Image = ConfigManager.CommunityLestTalk.ToDocumentUri();
                    communityDefault.Title = ResourcesManager._L("COM_C2C_PENDING_B_TITLE", forename);
                    communityDefault.Description = ResourcesManager._L("COM_C2C_PENDING_B_DESCRIPTION");
                    communityDefault.Action = new List<string> { "Accept", "Decline" };
                }
                communityDefault.CreatedDate = c2cqbicle.StartedDate.ConvertTimeFromUtc(currentUser.Timezone).ToString($"{currentUser.DateFormat} {currentUser.TimeFormat}");
                return new { communityDefault, streams, totalPage = 0 };
            }

            var totalPage = 0;

            var model = GetQbicleStreamsPagging(fillterModel, out totalPage);
            streams = model.ToMicro(currentUser.Timezone, currentUser.DateFormat, currentUser.Id);


            if (c2cqbicle != null)
            {
                if (c2cqbicle.Status == CommsStatus.Blocked && fillterModel.PageIndex == 0)
                {
                    communityDefault.CreatedDate = c2cqbicle.StartedDate.ConvertTimeFromUtc(currentUser.Timezone).ToString($"{currentUser.DateFormat} {currentUser.TimeFormat}");

                    if (c2cqbicle.Blocker.Id == currentUser.Id)
                    {
                        communityDefault.Image = ConfigManager.CommunityPendingadd.ToDocumentUri();
                        communityDefault.Title = ResourcesManager._L("COM_C2C_BLOCK_B_TITLE");
                        communityDefault.Description = ResourcesManager._L("COM_C2C_BLOCK_B_DESCRIPTION");
                        communityDefault.Action = new List<string> { "Unblock" };
                    }
                    else
                    {
                        linkUser = c2cqbicle.Customers.Where(s => s.Id != currentUser.Id).FirstOrDefault();
                        forename = !string.IsNullOrEmpty(linkUser.Forename) ? linkUser.Forename : linkUser.DisplayUserName;

                        communityDefault.Image = ConfigManager.CommunityPendingadd.ToDocumentUri();
                        communityDefault.Title = ResourcesManager._L("COM_C2C_BLOCK_C_TITLE", forename);
                        communityDefault.Description = ResourcesManager._L("COM_C2C_BLOCK_C_DESCRIPTION");
                        communityDefault.Action = new List<string> { "RemoveContact" };
                    }
                }
            }
            else if (b2cqbicle != null)
            {
                communityDefault.CreatedDate = b2cqbicle.StartedDate.ConvertTimeFromUtc(currentUser.Timezone).ToString($"{currentUser.DateFormat} {currentUser.TimeFormat}");
                if (b2cqbicle.Status == CommsStatus.Blocked && fillterModel.PageIndex == 0)
                {
                    communityDefault.Image = ConfigManager.CommunityBlocked.ToDocumentUri();

                    if (b2cqbicle.Blocker != null)
                    {
                        communityDefault.Title = ResourcesManager._L("COM_B2C_BLOCK_B_TITLE");
                        communityDefault.Description = ResourcesManager._L("COM_B2C_BLOCK_B_DESCRIPTION");
                        communityDefault.Action = new List<string> { "Accept" };
                    }
                    else
                    {
                        businessName = new CommerceRules(dbContext).GetB2bBusinessNameById(b2cqbicle.Business?.Id ?? 0);

                        communityDefault.Title = ResourcesManager._L("COM_B2C_BLOCK_C_TITLE");
                        communityDefault.Description = ResourcesManager._L("COM_B2C_BLOCK_C_DESCRIPTION", businessName);
                        communityDefault.Action = new List<string> { "RemoveContact" };
                    }
                }
                else if (model.Count == 0 && fillterModel.PageIndex == 0)
                {
                    businessName = new CommerceRules(dbContext).GetB2bBusinessNameById(b2cqbicle.Business?.Id ?? 0);

                    communityDefault.Title = ResourcesManager._L("COM_B2B_LETTALK_TITLE"); ;
                    communityDefault.Image = ConfigManager.CommunityLestTalk.ToDocumentUri();
                    communityDefault.Description = ResourcesManager._L("COM_B2B_LETTALK_DESCRIPTION", businessName);
                    communityDefault.Action = new List<string>();
                };
            }

            return new { communityDefault, streams, totalPage };

        }


        private List<QbicleActivity> GetQbicleStreamsPagging(CommunityPaggingModel fillterModel, out int totalPage)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get qbicle streams", null, null, fillterModel);

                var query = dbContext.Activities.Where(s => s.Qbicle.Id == fillterModel.QbicleId && s.IsVisibleInQbicleDashboard);
                var totalNumber = query.Count();
                query = query.OrderByDescending(p => p.TimeLineDate);

                var activities = query.Skip(fillterModel.PageIndex * HelperClass.activitiesPageSize).Take(HelperClass.activitiesPageSize).AsNoTracking().ToList();

                totalPage = ((totalNumber % HelperClass.activitiesPageSize) == 0) ? (totalNumber / HelperClass.activitiesPageSize) : (totalNumber / HelperClass.activitiesPageSize) + 1;

                return activities;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, fillterModel);
                totalPage = 0;
                return new List<QbicleActivity>();
            }
        }


        public object GetCommunitySubNavigation(int contactType, string textFilter)
        {
            var userId = CurrentUser.Id;

            int allNum = 0;
            int favouriteNum = 0;
            int requestNum = 0;
            int sentNum = 0;
            int blockedNum = 0;
            new C2CRules(dbContext).GetCommunityTalkNumByType(userId, textFilter, contactType, ref allNum, ref favouriteNum, ref requestNum, ref sentNum, ref blockedNum);

            return new
            {
                All = allNum,
                Favourites = favouriteNum,
                Requests = requestNum,
                Sent = sentNum,
                Blocked = blockedNum
            };
            //var query = from c in dbContext.CQbicles
            //            join c2c in dbContext.C2CQbicles on c.Id equals c2c.Id into dept
            //            from c2c in dept.DefaultIfEmpty()
            //            join b2c in dbContext.B2CQbicles on c.Id equals b2c.Id into dept1
            //            from b2c in dept1.DefaultIfEmpty()
            //            join b2bprofile in dbContext.B2BProfiles on b2c.Business.Id equals b2bprofile.Domain.Id into dept2
            //            from b2bprofile in dept2.DefaultIfEmpty()
            //            where !c.IsHidden
            //            && ((c2c != null && c2c.Customers.Any(u => u.Id == userId)) || (b2c != null && b2c.Customer.Id == userId))
            //            && (!c.RemovedForUsers.Any(p => p.Id == userId))
            //            select new
            //            {
            //                c.LikedBy,
            //                c.Status,
            //                Type = (c2c == null ? 1 : 2),//1:businesses, 2:individuals
            //                SourceUserId = c2c.Source.Id
            //            };

            //var allNum = query.Where(s => s.Status != CommsStatus.Blocked).Count();

            //var favouriteNum = query.Where(s => s.Status != CommsStatus.Blocked && s.LikedBy.Any(u => (s.Type == 2 && u.Id != userId) || (s.Type == 1 && u.Id == userId))).Count();

            //var requestNum = query.Where(p => p.Status == CommsStatus.Pending && (p.Type == 2 && p.SourceUserId != userId)).Count();

            //var sentNum = query.Where(p => p.Status == CommsStatus.Pending && p.Type == 2 && p.SourceUserId == userId).Count();

            //var blockedNum = query.Where(p => p.Status == CommsStatus.Blocked).Count();
            //return new
            //{
            //    All = allNum,
            //    Favourites = favouriteNum,
            //    Requests = requestNum,
            //    Sent = sentNum,
            //    Blocked = blockedNum
            //};
        }

        public MicroCommunities GetCommunities(CommunityParameter filter)
        {
            var userid = CurrentUser.Id;
            filter.UserId = userid;

            var c2cRule = new C2CRules(dbContext);

            var totalPage = 0;
            var communities = c2cRule.GetC2CQbicles(out totalPage, filter).ToCommunity(userid, CurrentUser.Timezone);

            return new MicroCommunities { Communities = communities, TotalPage = totalPage };
        }

    }
}
