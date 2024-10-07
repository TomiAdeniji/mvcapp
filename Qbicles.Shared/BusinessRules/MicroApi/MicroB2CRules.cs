using DocumentFormat.OpenXml.ExtendedProperties;
using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
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

namespace Qbicles.BusinessRules.Micro
{
    public class MicroB2CRules : MicroRulesBase
    {
        public MicroB2CRules(MicroContext microContext) : base(microContext)
        {
        }
        /// <summary>
        /// Call this api to get changed when recieved a notifiation update the Order fro Business
        /// </summary>
        /// <param name="tradeId">Get B2COrderCreation (Discussion)</param>
        /// <returns>B2COrder object</returns>
        public object GetB2COrder(int tradeId)
        {
            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId(tradeId);

            var tradeOrder = discussion.TradeOrder;

            var b2cqbicle = discussion.Qbicle as B2CQbicle;
            var businessProfile = b2cqbicle.Business.Id.BusinesProfile();
            var catalog = discussion.TradeOrder?.ProductMenu ?? new Models.Catalogs.Catalog();

            var oJson = string.IsNullOrEmpty(tradeOrder.OrderJson)
                ? JsonHelper.ToJson(new Order { TraderId = discussion.Id, TradeOrderId = tradeOrder.Id, SalesChannel = SalesChannelEnum.B2C, Items = new List<Item>() })
                : tradeOrder.OrderJson;

            var oJsonOrg = string.IsNullOrEmpty(tradeOrder.OrderJsonOrig) ? oJson : tradeOrder.OrderJsonOrig;

            var customer = new
            {
                Avatar = tradeOrder.Customer.ProfilePic.ToUri(),
                Name = tradeOrder.Customer.GetFullName(),
                Phone = tradeOrder.Customer.Tell,
                Email = tradeOrder.Customer.Email,
                DeliveryType = "",
                DeliveryAddress = "",
                DeliveryNote = ""
            };

            var jOrder = oJson.ParseAs<Order>();
            if (tradeOrder.OrderStatus.GetId() > 3)
            {
                customer = new
                {
                    Avatar = tradeOrder.Customer.ProfilePic.ToUri(),
                    Name = tradeOrder.Customer.GetFullName(),
                    Phone = tradeOrder.Customer.Tell,
                    Email = tradeOrder.Customer.Email,
                    DeliveryType = jOrder.Type,
                    DeliveryAddress = jOrder.Customer.Address.ToAddress(),
                    DeliveryNote = jOrder.Notes
                };
            }


            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(b2cqbicle.Business.Id);

            jOrder.Items.ForEach(item =>
            {
                item.Variant.Taxes.ForEach(tax =>
                {
                    tax.AmountTax *= item.Quantity;
                    tax.AmountTax = Decimal.Round(tax.AmountTax, (int)currencySetting.DecimalPlace);
                });

                item.Variant.Discount = Decimal.Round(item.Variant.Discount, (int)currencySetting.DecimalPlace);
                item.Variant.DiscountAmount = Decimal.Round(item.Variant.DiscountAmount, (int)currencySetting.DecimalPlace);
                item.Variant.AmountInclTax = Decimal.Round(item.Variant.AmountInclTax, (int)currencySetting.DecimalPlace);
                item.Variant.AmountExclTax = Decimal.Round(item.Variant.AmountExclTax, (int)currencySetting.DecimalPlace);
                item.Variant.TotalAmount = Decimal.Round(item.Variant.TotalAmount, (int)currencySetting.DecimalPlace);
                item.Variant.TotalDiscount = Decimal.Round(item.Variant.TotalDiscount, (int)currencySetting.DecimalPlace);
                item.Variant.TotalAmountWithoutDiscount = Decimal.Round(
                                                (item.Variant.TotalAmount + item.Variant.TotalDiscount), 
                                                (int)currencySetting.DecimalPlace);


                //item.Variant.GrossValue = item.Variant.AmountInclTax;
                item.Variant.GrossValueText = item.Variant.GrossValue.ToCurrencySymbol(currencySetting);

                //item.Variant.NetValue = item.Variant.AmountExclTax;
                item.Variant.NetValueText = item.Variant.NetValue.ToCurrencySymbol(currencySetting);

                //item.Variant.TaxAmount = item.Variant.AmountInclTax - item.Variant.AmountExclTax;
                item.Variant.TaxAmountText = item.Variant.TaxAmount.ToCurrencySymbol(currencySetting);

                item.Extras.ForEach(extra =>
                {
                    extra.Taxes.ForEach(tax =>
                    {
                        tax.AmountTax *= item.Quantity;
                        tax.AmountTax = Decimal.Round(tax.AmountTax, (int)currencySetting.DecimalPlace);
                    });

                    extra.Discount = Decimal.Round(extra.Discount, (int)currencySetting.DecimalPlace);
                    extra.DiscountAmount = Decimal.Round(extra.DiscountAmount, (int)currencySetting.DecimalPlace);
                    extra.AmountInclTax = Decimal.Round(extra.AmountInclTax, (int)currencySetting.DecimalPlace);
                    extra.AmountExclTax = Decimal.Round(extra.AmountExclTax, (int)currencySetting.DecimalPlace);
                    extra.TotalAmount = Decimal.Round(extra.TotalAmount, (int)currencySetting.DecimalPlace);
                    extra.TotalDiscount = Decimal.Round(extra.TotalDiscount, (int)currencySetting.DecimalPlace);
                    extra.TotalAmountWithoutDiscount = Decimal.Round(
                                                                        (extra.TotalAmount + extra.TotalDiscount), 
                                                                        (int)currencySetting.DecimalPlace
                                                                     );

                    //extra.GrossValue = extra.AmountInclTax;
                    extra.GrossValueText = extra.GrossValue.ToCurrencySymbol(currencySetting);

                    //extra.NetValue = extra.AmountExclTax;
                    extra.NetValueText = extra.NetValue.ToCurrencySymbol(currencySetting);

                    //extra.TaxAmount = extra.AmountInclTax - extra.AmountExclTax;
                    extra.TaxAmountText = extra.TaxAmount.ToCurrencySymbol(currencySetting);
                });
            });           


            // Rounding
            jOrder.AmountExclTax = Decimal.Round(jOrder.AmountExclTax, (int)currencySetting.DecimalPlace);
            jOrder.AmountInclTax = Decimal.Round(jOrder.AmountInclTax, (int)currencySetting.DecimalPlace);
            jOrder.AmountTax = Decimal.Round(jOrder.AmountTax, (int)currencySetting.DecimalPlace);
            jOrder.Discount = Decimal.Round(jOrder.Discount, (int)currencySetting.DecimalPlace);



            var newOrder = new
            {
                DomainKey = businessProfile.Domain.Key,
                BusinessKey = b2cqbicle.Business.Key,
                BusinessLogoUri = businessProfile.LogoUri.ToUri(Enums.FileTypeEnum.Image, "T"),
                BusinessName = businessProfile.BusinessName,
                CatalogName = catalog.Name,
                OrderName = discussion.Name,
                IsAgreedByBusiness = tradeOrder.IsAgreedByBusiness,
                IsAgreedByCustomer = tradeOrder.IsAgreedByCustomer,
                OrderStatus = tradeOrder.OrderStatus.GetId(),
                OrderStatusLabel = tradeOrder.GetDescription(),
                Order = jOrder,
                OrderOrig = oJsonOrg.ParseAs<Order>(),
                Customer = customer,
                Currency = new { Symbol = currencySetting.CurrencySymbol, Name = currencySetting.CurrencyName , DecimalPlace = currencySetting.DecimalPlace}
            };

            return newOrder;
        }

        /// <summary>
        /// Get catalogues from a business
        /// </summary>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        public B2CProfileCatalogues GetB2CCatalogues(string domainKey)
        {
            var domainId = 0;
            if (!string.IsNullOrEmpty(domainKey.Trim()))
            {
                domainId = int.Parse(domainKey.Decrypt());
            }
            var businessProfile = new CommerceRules(dbContext).GetB2bProfileByDomainId(domainId);
            var catalogues = new B2CRules(dbContext).GetListCatalogViewModelByDomainId(domainId, false);

            var profileCatalogues = new B2CProfileCatalogues
            {
                DomainKey = businessProfile.Domain.Key,
                Name = businessProfile.BusinessName,
                ImageUri = businessProfile.LogoUri.ToUri(Enums.FileTypeEnum.Image, "T"),
            };
            catalogues.OrderBy(s => s.Name).ToList().ForEach(c =>
            {
                profileCatalogues.Catalogues.Add(new B2CCatalogues
                {
                    CatalogId = c.Id,
                    Key = c.Key,
                    Name = c.Name,
                    Description = c.Description
                });
            });
            return profileCatalogues;
        }

        /// <summary>
        /// Create b2c order when click to a catalog
        /// </summary>
        /// <param name="domainKey"></param>
        /// <param name="catalogKey"></param>
        /// <param name="isCreatorTheCustomer">Default = false. If the creator of the Activity or Post is the Customer in a B2CQBicle or B2COrder then this value must be set to true at the time the Activity/post is created</param>
        /// <returns>
        /// order infomation
        /// category's items 
        /// Categories list( for search)
        /// B2COrder object
        /// </returns>
        public object CreateB2COrder(string domainKey, int catalogId, bool isCreatorTheCustomer)
        {
            var domainId = domainKey.Decrypt2Int();
            var createTradeOrder = new DiscussionsRules(dbContext).SaveB2CDiscussionForStore(domainId, catalogId, CurrentUser.Id, isCreatorTheCustomer);
            if (createTradeOrder.Object2 == null)
                return null;
            var discussion = createTradeOrder.Object2 as B2COrderCreation;

            var b2cqbicle = discussion.Qbicle as B2CQbicle;
            var businessProfile = b2cqbicle.Business.Id.BusinesProfile();
            var catalog = discussion.TradeOrder?.ProductMenu ?? new Models.Catalogs.Catalog();

            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

            var newOrder = new
            {
                DomainKey = businessProfile.Domain.Key,
                BusinessKey = b2cqbicle.Business.Key,
                BusinessLogoUri = businessProfile.LogoUri.ToUri(Enums.FileTypeEnum.Image, "T"),
                BusinessName = businessProfile.BusinessName,
                CatalogName = catalog.Name,
                OrderName = discussion.Name,
                OrderStatus = discussion.TradeOrder.OrderStatus.GetId(),
                IsAgreedByBusiness = false,
                IsAgreedByCustomer = false,
                Order = new Order
                {
                    TradeOrderId = discussion.TradeOrder.Id,
                    Reference = discussion.TradeOrder?.OrderReference.FullRef,
                    TraderId = discussion.Id,
                    Items = new List<Item>()
                },
                OrderOrig = new Order
                {
                    TradeOrderId = discussion.TradeOrder.Id,
                    Reference = discussion.TradeOrder?.OrderReference.FullRef,
                    TraderId = discussion.Id,
                    Items = new List<Item>()
                },
                Currency = new { Symbol = currencySetting.CurrencySymbol, Name = currencySetting.CurrencyName }
            };

            return newOrder;
        }

        /// <summary>
        /// get first catalogues and items after create Trade Order
        /// response items in ALL caregoty
        /// </summary>
        /// <param name="traderId"></param>
        /// <returns></returns>
        public object GetInitCatalogAndItems(int tradeId, string domainKey, int page)
        {
            var tradeOrder = dbContext.TradeOrders.FirstOrDefault(e => e.Id == tradeId);

            var categories = tradeOrder.ProductMenu.Categories.Select(e => new BaseModel { Id = e.Id, Name = e.Name });
            //search category items
            var search = new B2COrderItemsRequestModel
            {
                bdomainId = int.Parse(domainKey.Decrypt()),
                CatIds = categories.Select(e => e.Id).ToList(),
                pageSize = HelperClass.activitiesPageSize,
                pageNumber = page + 1
            };

            var items = new PosMenuRules(dbContext).LoadOrderMenuItem(search);

            return new { categories, items };
        }
        /// <summary>
        /// get catalogue and items
        /// response items in a caregoty
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public object GetCategoryItems(int categoryId, string domainKey, int page)
        {
            var search = new B2COrderItemsRequestModel
            {
                bdomainId = int.Parse(domainKey.Decrypt()),
                CatIds = new List<int> { categoryId },
                pageSize = 10,
                pageNumber = page
            };

            var items = new PosMenuRules(dbContext).GetCategoryItems(search);

            return new { items };
        }

        public PaginationResponse SearchB2bOrderItems(B2COrderItemsRequestModel filter)
        {
            filter.bdomainId = int.Parse(filter.bdomainKey.Decrypt());
            filter.pageSize = HelperClass.activitiesPageSize;

            return new PosMenuRules(dbContext).LoadOrderMenuItem(filter);
        }

        public B2COrderItem GetItemDetail(int itemId, string domainKey)
        {
            var menuRules = new PosMenuRules(dbContext);
            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(int.Parse(domainKey.Decrypt()));
            var categoryItem = menuRules.GetPosCategoryItemWithTaxesById(itemId);
            var itemDetail = new B2COrderItem();


            // Common information:
            //      Category Name
            //      Item Name
            //      Min price of Variant
            //      Description - Html
            itemDetail.CategoryName = categoryItem.Category?.Name ?? "";
            itemDetail.Name = categoryItem.Name;
            itemDetail.MinPrice = categoryItem.PosVariants.Count == 0 ?
                0 : categoryItem.PosVariants.Min(p => p.Price?.GrossPrice ?? 0);
            itemDetail.Description = categoryItem.Description;


            // Galaries
            var variantDefault = categoryItem.PosVariants.FirstOrDefault(v => v.IsDefault);
            var galleryItems = variantDefault?.TraderItem.GalleryItems.OrderBy(e => e.Order).ToList() ?? new List<Models.Trader.Product.ProductGalleryItem>();
            itemDetail.ItemsGaleries = galleryItems.Select(e => new ItemGalery
            {
                FileUri = e.FileUri,
                Order = e.Order,
                Small = e.FileUri.ToUri(Enums.FileTypeEnum.Image, "T"),
                Large = e.FileUri.ToUri()
            }).ToList();


            //  Variants:
            //      Id
            //      Name
            //      The list of Options (Id and Name)
            var lstVariants = new List<CategoryVariant>();
            var variantProperties = categoryItem.VariantProperties;
            variantProperties.ForEach(variantItem =>
            {
                var variantCustomizedObject = new CategoryVariant
                {
                    Id = variantItem.Id,
                    Name = variantItem.Name,
                    VariantOptions = new List<CategoryVariantOption>()
                };

                variantItem.VariantOptions.ForEach(optionItem =>
                {
                    variantCustomizedObject.VariantOptions.Add(new CategoryVariantOption()
                    {
                        Name = optionItem.Name,
                        Id = optionItem.Id
                    });
                });

                lstVariants.Add(variantCustomizedObject);
            });
            itemDetail.Variants = lstVariants;


            //  Extras:
            //      Id
            //      Name
            //      Price string
            //      Price as decimal
            var lstExtras = new List<CategoryExtras>();
            var extras = categoryItem.PosExtras ?? new List<Models.Catalogs.Extra>();
            extras.ForEach(extraItem =>
            {
                var extraCustomizedObject = new CategoryExtras()
                {
                    Id = extraItem.Id,
                    Price = extraItem.Price?.GrossPrice ?? 0,
                    PriceStr = (extraItem.Price?.GrossPrice ?? 0).ToCurrencySymbol(currencySetting),
                    Name = extraItem.Name ?? "",
                    CategoryId = extraItem.CategoryItem.Category.Id,
                    CategoryName = extraItem.CategoryItem.Category.Name,
                    Description = extraItem.CategoryItem.Description,
                    GrossValue = extraItem.Price?.GrossPrice ?? 0,
                    NetValue = extraItem.Price?.NetPrice ?? 0,
                    ImageUri = extraItem?.TraderItem?.ImageUri.ToUriString(),
                    SKU = extraItem.TraderItem.SKU,
                    TaxAmount = extraItem.Price?.TotalTaxAmount ?? 0,
                    GrossValueStr = (extraItem.Price?.GrossPrice ?? 0).ToCurrencySymbol(currencySetting),
                    NetValueStr = (extraItem.Price?.NetPrice ?? 0).ToCurrencySymbol(currencySetting),
                    TaxAmountStr = (extraItem.Price?.TotalTaxAmount ?? 0).ToCurrencySymbol(currencySetting),
                    Taxes = new List<Tax>(),                   
                };

                if (extraItem?.Price?.Taxes != null)
                {
                    extraCustomizedObject.Taxes = extraItem.Price.Taxes.Select(p => new Tax
                    {
                        AmountTax = p.Amount,
                        TaxName = p.TaxName,
                        TaxRate = p.Rate,
                        TraderId = p.Id
                    }).ToList();
                }

                lstExtras.Add(extraCustomizedObject);
            });
            itemDetail.Extras = lstExtras;


            //  Default variant object
            //      List Ids of variants
            //      Image Uri
            var defaultVariant = categoryItem.PosVariants.FirstOrDefault(p => p.IsDefault);
            itemDetail.DefaultVariant = new DefaultVariant()
            {
                ImageUri = categoryItem.ImageUri.ToUriString(),
                ListVariantOptions = new List<int>()
            };
            if (defaultVariant != null)
            {
                itemDetail.DefaultVariant.ImageUri = defaultVariant.ImageUri.ToUriString();
                itemDetail.DefaultVariant.ListVariantOptions = defaultVariant
                                                                    .VariantOptions.Select(p => p.Id).ToList();
            }

            return itemDetail;
        }

        public SelectedVariantModel GetVariantBySelectedOptions(List<int> listVariantOptionIds, int categoryItemId, int quantity)
        {
            var categoryItem = dbContext.PosCategoryItems.FirstOrDefault(p => p.Id == categoryItemId);

            var variantResult = new B2CRules(dbContext).GetVariantBySelectedOptions(listVariantOptionIds, categoryItemId, quantity);

            if (variantResult != null && variantResult.result)
            {
                return ((SelectedVariantModel)variantResult.Object);
            }

            return new SelectedVariantModel()
            {
                ImageUri = categoryItem?.ImageUri?.ToUriString() ?? "",
                Price = null,
                PriceStr = null
            };
        }

        /// <summary>
        /// Update the Order Add/Edit/Remove item
        /// </summary>
        /// <param name="b2COrder"></param>
        /// <returns></returns>
        public B2COrder B2COrderMicroUpdate(B2COrder b2COrder, bool isCreatorTheCustomer)
        {
            var b2cRules = new B2CRules(dbContext);
            b2cRules.ValidateB2COrder(b2COrder.OrderOrig);
            var orderOrigRequest = b2COrder.OrderOrig.Clone();

            var tradeOrder = b2cRules.GetTradeOrderById(b2COrder.Order.TradeOrderId);
            //tradeOrder.OrderJsonOrig = order.ToJson();

            b2cRules.UpdateB2COrderIsBusinessDiscount(orderOrigRequest, b2COrder.BusinessDiscount);


              //If b2COrder.BusinessDiscount = true -> update discount
            //If b2COrder.BusinessDiscount = false -> reset discount
            b2cRules.CalculationB2COrderJson(tradeOrder, orderOrigRequest, !b2COrder.BusinessDiscount);

            //Remove customer and business accepted flag
            tradeOrder.IsAgreedByBusiness = false;
            tradeOrder.IsAgreedByCustomer = false;
            //Update order will change order's status to draft;
            tradeOrder.OrderStatus = TradeOrderStatusEnum.Draft;

            dbContext.Entry(tradeOrder).State = EntityState.Modified;
            dbContext.SaveChanges();

            b2COrder.Order = tradeOrder.OrderJson.ParseAs<Order>();
            b2COrder.OrderOrig = tradeOrder.OrderJsonOrig.ParseAs<Order>();

            //=================================================

            var user = CurrentUser;
            var currentUser = new UserSetting
            {
                Id = CurrentUser.Id,
                DisplayName = string.IsNullOrEmpty(user.DisplayUserName) ? user.GetFullName() : user.DisplayUserName
            };

            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId((int)b2COrder.Order.TraderId);

            var discussionOrderBy = new B2CRules(dbContext).DiscussionOrderBy(discussion.Id, currentUser, "", discussion);

            new B2CRules(dbContext).B2CDicussionOrderSendMessage(isCreatorTheCustomer, ResourcesManager._L("B2C_CHANGED_ORDER",
                discussionOrderBy.DisplayName), (int)b2COrder.Order.TraderId, currentUser.Id, discussion.Qbicle.Id, currentUser.Id);

            return b2COrder;
        }

        public List<BaseModel> B2COrderGetDeliveryAddresses()
        {
            return CurrentUser.TraderAddresses.Select(e => new BaseModel
            {
                Id = e.Id,
                Name = e.ToAddress()
            }).ToList();
        }

        /// <summary>
        /// Customer confirm B2C order
        /// </summary>
        /// <param name="b2cOrder"></param>
        public ReturnJsonModel B2CCustomerOrderConfirm(B2CCustomerAcceptedInfo b2cOrder)
        {
            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId(b2cOrder.disId);

            //acceptedInfo.disId = string.IsNullOrEmpty(acceptedInfo.disKey) ? 0 : int.Parse(acceptedInfo.disKey.Decrypt());
            var accept = new B2CRules(dbContext).SetOrderAcceptedByCustomer(b2cOrder);
            if (accept.result == false)
                return accept;

            if (!string.IsNullOrEmpty(b2cOrder.note))
            {
                new B2CRules(dbContext).B2CDicussionOrderSendMessage(true, b2cOrder.note, b2cOrder.disId, CurrentUser.Id, discussion.Qbicle.Id, "");
            }
            var currentUser = new UserSetting
            {
                Id = CurrentUser.Id,
                DisplayName = CurrentUser.GetFullName()
            };

            var discussionOrderBy = new B2CRules(dbContext).DiscussionOrderBy(b2cOrder.disId, currentUser, "", discussion);

            new B2CRules(dbContext).B2CDicussionOrderSendMessage(true, ResourcesManager._L("B2C_AGREED_ORDER", discussionOrderBy.DisplayName), b2cOrder.disId, currentUser.Id, discussion.Qbicle.Id, "");

            return new ReturnJsonModel() { result = true };
        }


        /// <summary>
        /// Business confirm B2C order
        /// </summary>
        /// <param name="b2cOrder"></param>
        public void B2CBusinessOrderConfirm(B2CCustomerAcceptedInfo b2cOrder)
        {
            var discussion = dbContext.B2COrderCreations.Find(b2cOrder.disId);
            var tradeOrder = discussion.TradeOrder;
            tradeOrder.IsAgreedByBusiness = true;

            var currentUser = new UserSetting
            {
                Id = CurrentUser.Id,
                DisplayName = CurrentUser.GetFullName()
            };

            var discussionOrderBy = new B2CRules(dbContext).DiscussionOrderBy(b2cOrder.disId, currentUser,
                SystemPageConst.B2C, discussion);

            new B2CRules(dbContext).B2CDicussionOrderSendMessage(false, ResourcesManager._L("B2C_AGREED_ORDER", discussionOrderBy.DisplayName), b2cOrder.disId, currentUser.Id, discussion.Qbicle.Id, "");
        }



        /// <summary>
        /// B2C Order PaymentGet
        /// </summary>
        /// <param name="paymentId"></param>
        /// <param name="tradeId"></param>
        /// <returns>B2COderPayment object</returns>
        public object B2COrderPaymentGet(string paymentId, int tradeId)
        {
            var payments = new List<B2COderPayment>();

            var currentUser = CurrentUser;

            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByTradeorderId(tradeId);

            var tradeOrder = discussion.TradeOrder;

            var b2cqbicle = discussion.Qbicle as B2CQbicle;

            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(b2cqbicle.Business.Id);

            //var _order = new Order() { Items = new List<Item>() };
            //if (tradeOrder != null && tradeOrder.OrderJson != null)
            //   _order = JsonHelper.ParseAs<Order>(tradeOrder.OrderJson);

            //var voucher = dbContext.Vouchers.FirstOrDefault(e => e.Id == _order.VoucherId) ?? new Models.Loyalty.Voucher();
            //var orderVoucher = new VoucherModel { Id = voucher.Id, Name = voucher.Promotion?.Name, Key = voucher.Promotion?.Key, Code = voucher.Code };

            var pId = paymentId.Decrypt2Int();

            var p = dbContext.CashAccountTransactions.FirstOrDefault(c => c.Id == pId);

            var bank = "";
            var info = "";
            if (p.AssociatedInvoice.Sale != null)
            {
                bank = p.DestinationAccount?.Name;
                info = "Sale";
            }
            else if (p.AssociatedInvoice.Purchase != null)
            {
                bank = p.OriginatingAccount?.Name;
                info = "Purchase";
            }
            else if (p.AssociatedInvoice != null)
            {
                info = "Invoice";
            }
            //reuturn B2COderPayment
            return new
            {
                TradeId = tradeOrder.Id,
                PaymentId = p.Id.Encrypt(),
                Reference = p.Reference,
                Information = info,
                PaymentMethod = p.PaymentMethod?.Name ?? "",
                Date = p.CreatedDate.ConvertTimeFromUtc(currentUser.Timezone).ToString(currentUser.DateFormat),
                AmountString = p.Amount.ToCurrencySymbol(currencySetting),
                Amount = p.Amount,
                Bank = bank,
                Status = p.Status.GetDescription(),
                StatusId = p.Status.GetId(),
                Currency = new { Symbol = currencySetting.CurrencySymbol, Name = currencySetting.CurrencyName }
            };

        }


        public object B2COrderPaymentsGet(int tradeId)
        {
            var payments = new List<B2COderPayment>();

            var currentUser = CurrentUser;

            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByTradeorderId(tradeId);

            var tradeOrder = discussion.TradeOrder;

            var b2cqbicle = discussion.Qbicle as B2CQbicle;

            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(b2cqbicle.Business.Id);

            //var _order = new Order() { Items = new List<Item>() };
            //if (tradeOrder != null && tradeOrder.OrderJson != null)
            //   _order = JsonHelper.ParseAs<Order>(tradeOrder.OrderJson);

            //var voucher = dbContext.Vouchers.FirstOrDefault(e => e.Id == _order.VoucherId) ?? new Models.Loyalty.Voucher();
            //var orderVoucher = new VoucherModel { Id = voucher.Id, Name = voucher.Promotion?.Name, Key = voucher.Promotion?.Key, Code = voucher.Code };

            var paymentMethods = dbContext.PaymentMethods.Select(e => new { e.Name }).ToList();

            tradeOrder.Payments.ForEach(p =>
            {
                var bank = "";
                var info = "";
                if (p.AssociatedInvoice.Sale != null)
                {
                    bank = p.DestinationAccount?.Name;
                    info = "Sale";
                }
                else if (p.AssociatedInvoice.Purchase != null)
                {
                    bank = p.OriginatingAccount?.Name;
                    info = "Purchase";
                }
                else if (p.AssociatedInvoice != null)
                {
                    info = "Invoice";
                }

                payments.Add(new B2COderPayment
                {
                    Id = p.Id.ToString("D6"),
                    TradeId = tradeOrder.Id,
                    PaymentId = p.Id.Encrypt(),
                    Reference = p.Reference,
                    Information = info,
                    PaymentMethod = p.PaymentMethod?.Name ?? "",
                    Date = p.CreatedDate.ConvertTimeFromUtc(currentUser.Timezone).ToString(currentUser.DateFormat),
                    AmountString = p.Amount.ToCurrencySymbol(currencySetting),
                    Amount = p.Amount,
                    Bank = bank,
                    Status = p.Status.GetDescription(),
                    StatusId = p.Status.GetId(),
                });
            });

            return new { payments, paymentMethods, Currency = new { Symbol = currencySetting.CurrencySymbol, Name = currencySetting.CurrencyName } };
        }

        public B2COderPayment B2COrderPaymentCreate(B2COderPayment payment, ref string resultPayment)
        {
            var refModel = new B2CRules(dbContext).CreateB2CPayment(
                payment.TradeId.Encrypt(),
                new CashAccountTransaction
                {
                    Amount = payment.Amount,
                    Reference = payment.Reference,
                    PaymentMethod = new Models.Trader.PaymentMethod { Name = payment.PaymentMethod }
                },
                payment.IsCutomer,
                CurrentUser.Id);
            var p = (CashAccountTransaction)refModel.Object;

            var bank = "";
            var info = "";
            if (p.AssociatedInvoice.Sale != null)
            {
                bank = p.DestinationAccount?.Name;
                info = "Sale";
            }
            else if (p.AssociatedInvoice.Purchase != null)
            {
                bank = p.OriginatingAccount?.Name;
                info = "Purchase";
            }
            else if (p.AssociatedInvoice != null)
            {
                info = "Invoice";
            }


            var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByTradeorderId(payment.TradeId);
            var b2cqbicle = discussion.Qbicle as B2CQbicle;
            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(b2cqbicle.Business.Id);

            return new B2COderPayment
            {
                TradeId = payment.TradeId,
                PaymentId = p.Id.Encrypt(),
                Reference = p.Reference,
                Information = info,
                PaymentMethod = p.PaymentMethod?.Name ?? "",
                Date = p.CreatedDate.ConvertTimeFromUtc(CurrentUser.Timezone).ToString(CurrentUser.DateFormat),
                AmountString = p.Amount.ToDecimalPlace(currencySetting),
                Amount = p.Amount,
                Bank = bank,
                Status = p.Status.GetDescription(),
                StatusId = p.Status.GetId(),
            };
        }

        /// <summary>
        /// get Categories Items from catalog (pos_menu)
        /// </summary>
        /// <param name="catalogKey"></param>
        /// <param name="domainKey"></param>
        /// <returns></returns>
        public object GetCatalogCategoriesItem(B2CMenuItemsRequestModel request)
        {
            var catalogId = request.CatalogKey.Decrypt2Int();
            var catalog = dbContext.PosMenus.FirstOrDefault(e => e.Id == catalogId);

            var categories = catalog.Categories.Select(e => new Select2CustomeModel
            {
                id = e.Id,
                text = e.Name
            });

            request.bdomainId = catalog.Location.Domain.Key.Decrypt2Int();

            if (request.CatIds.Count == 0)
                request.CatIds = categories.Select(e => e.id).ToList();

            var allItems = new PosMenuRules(dbContext).LoadMenuItems(request, true);

            return new
            {
                Categories = categories,
                Items = allItems,
            };
        }

        public PaginationResponse GetCategoriesItems(B2CMenuItemsRequestModel request)
        {
            //request.CatIds = JsonConvert.DeserializeObject<List<int>>(scatids);
            request.bdomainId = request.DomainKey.Decrypt2Int();
            return new PosMenuRules(dbContext).LoadMenuItems(request, true);
        }

        public object GetB2COrderstatus(int id)
        {
            var tradeOrder = dbContext.TradeOrders.AsNoTracking().FirstOrDefault(e => e.Id == id);
            return new
            {
                label = tradeOrder.GetDescription(),
                color = tradeOrder.GetClassColor()
            };

        }
    }
}
