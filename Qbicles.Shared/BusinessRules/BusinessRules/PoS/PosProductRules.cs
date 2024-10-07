using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Model.SQLite;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.Pricing;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Category = Qbicles.BusinessRules.Model.SQLite.Category;
using CategoryItem = Qbicles.BusinessRules.Model.SQLite.CategoryItem;
using Extra = Qbicles.BusinessRules.Model.SQLite.Extra;

using Qbicles.Models.Trader.Pricing;

using System.Threading.Tasks;
using Qbicles.BusinessRules.Azure;

namespace Qbicles.BusinessRules.PoS
{
    public class PosProductRules
    {
        private ApplicationDbContext dbContext;

        public PosProductRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Database name</returns>
        public PosDevice GetMenuProduct(PosRequest request)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "api/pos/product SqLiteProduct", null, null, request);

                var user = new UserRules(dbContext).GetById(request.UserId);
                if (user == null)
                    return null;
                //Is user assigned as a user to UserDevice

                var device = new PosDeviceRules(dbContext).GetBySerialNumber(request.SerialNumber);
                if (device == null)
                    return null;
                if (device.Status == PosDeviceStatus.InActive) return null;
                if (device.Users.All(u => u.User.Id != request.UserId))
                    return null;

                if (device.Menu == null)
                    return null;

                new PosRequestRules(dbContext).HangfirePosRequestLog(request, device.Menu.Id.ToJson(), MethodBase.GetCurrentMethod().Name);
                return device;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
                return null;
            }
        }

        public async Task ProcessUpdateCatalogProductSqliteAsync(int catalogId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "SqLiteProduct", null, null, catalogId);

            var posMenu = dbContext.PosMenus.Find(catalogId);
            try
            {
                //var awsS3Bucket = AzureStorageHelper.AwsS3Client();

                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(ConfigManager.SQLitePOSMenuName);

                var tempPathRepository = ConfigManager.TempPathRepository;
                var databaseName = $"{Guid.NewGuid()}";
                var filePath = $"{tempPathRepository}{databaseName}";
                DirectoryHelper.DeleteFile($"{filePath}.db");
                DirectoryHelper.DeleteFile($"{filePath}.zip");

                using (FileStream outputFileStream = new FileStream($"{filePath}.db", FileMode.Create))
                {
                    s3Object.ObjectStream.CopyTo(outputFileStream);
                }

                ConfigManager.SQLiteConnectionString = $"Data Source={filePath}.db;Version=3;";

                var taxRates = new List<int>();
                var options = new List<int>();

                new MenuContext().Add(new Menu { Id = posMenu.Id, Name = posMenu.Name });

                posMenu.Categories.ForEach(menuCategory =>
                {
                    if (menuCategory.PosCategoryItems.Count == 0)
                        return;

                    new CategoryContext().Add(new Category
                    {
                        Id = menuCategory.Id,
                        Name = menuCategory.Name,
                        IsVisible = menuCategory.IsVisible ? 1 : 0,
                        Description = menuCategory.Description,
                        Menu_Id = posMenu.Id
                    });

                    //Items
                    menuCategory.PosCategoryItems.ForEach(menuCategoryItem =>
                    {
                        if (menuCategoryItem.PosVariants.Count == 0) return;
                        var variantDefaultId = menuCategoryItem.PosVariants.FirstOrDefault(e => e.IsDefault)?.Id ?? 0;

                        if (variantDefaultId <= 0) //Safe to verify
                            return;
                        // variant properties
                        var variantProperties = dbContext.PosVariantProperties.Where(c => c.CategoryItem.Id == menuCategoryItem.Id)
                                .Select(e => new Property { Id = e.Id, Name = e.Name, CategoryItem_Id = e.CategoryItem.Id }).ToList();

                        new PropertyContext().AddRange(variantProperties);

                        // variant options
                        var pIds = variantProperties.Select(e => e.Id);
                        var variantOptions = dbContext.PosVariantOptions.Where(e => pIds.Contains(e.VariantProperty.Id)).ToList();
                        variantOptions.ForEach(o =>
                        {
                            if (options.Any(e => e == o.Id)) return;
                            {
                                options.Add(o.Id);
                                new OptionContext().Add(new Option { Id = o.Id, Name = o.Name, Property_Id = o.VariantProperty.Id });
                            }
                        });

                        new CategoryItemContext().Add(new CategoryItem
                        {
                            Id = menuCategoryItem.Id,
                            Name = menuCategoryItem.Name,
                            Description = menuCategoryItem.Description,
                            ImageUri = menuCategoryItem.ImageUri.ToUriString(),
                            DefaultVariant = variantDefaultId,
                            Category_Id = menuCategory.Id
                        });
                        // variants
                        menuCategoryItem.PosVariants.ForEach(posVariant =>
                        {
                            if (string.IsNullOrEmpty(posVariant.TraderItem.SKU) || !posVariant.IsActive)
                                return;
                            //variant option exref
                            posVariant.VariantOptions.ForEach(vo =>
                            {
                                new VariantOptionContext().Add(new xref_variant_option
                                {
                                    Variant_Id = posVariant.Id,
                                    Option_Id = vo.Id
                                });
                            });

                            new VariantContext().Add(new Model.SQLite.Variant
                            {
                                Id = posVariant.Id,
                                Name = posVariant.Name,
                                Description = posVariant.TraderItem.Description,
                                SKU = posVariant.TraderItem.SKU,
                                Barcode = posVariant.TraderItem.Barcode,
                                ImageUri = posVariant.ImageUri.ToUriString(),
                                Unit = posVariant.Unit.Name,
                                NetValue = posVariant?.Price?.NetPrice ?? 0,
                                GrossValue = posVariant?.Price?.GrossPrice ?? 0,
                                TaxAmount = posVariant?.Price?.TotalTaxAmount ?? 0,
                                CategoryItem_Id = menuCategoryItem.Id
                            });
                            var variantTaxes = posVariant?.Price?.Taxes ?? new List<PriceTax>();
                            variantTaxes.ForEach(tax =>
                            {
                                if (taxRates.TrueForAll(t => t != tax.Id))
                                {
                                    taxRates.Add(tax.Id);
                                    new TaxRateContext().Add(new TaxRate
                                    {
                                        Id = tax.Id,
                                        Name = tax.TaxName,
                                        Rate = tax.Rate,
                                        Description = tax.TaxRate?.Description ?? ""
                                    });
                                }

                                new VariantTaxContext().Add(new xref_variant_tax
                                {
                                    TaxValue = tax.Amount,
                                    Tax_Id = tax.Id,
                                    Variant_Id = posVariant.Id
                                });
                            });
                        });
                        //Extra
                        menuCategoryItem.PosExtras.ForEach(menuCategoryItemExtra =>
                        {
                            if (string.IsNullOrEmpty(menuCategoryItemExtra.TraderItem.SKU))
                                return;
                            new ExtraContext().Add(new Extra
                            {
                                Id = menuCategoryItemExtra.Id,
                                Name = menuCategoryItemExtra.Name,
                                Description = menuCategoryItemExtra.TraderItem.Description,
                                SKU = menuCategoryItemExtra.TraderItem.SKU,
                                Barcode = menuCategoryItemExtra.TraderItem.Barcode,
                                ImageUri = menuCategoryItemExtra.TraderItem.ImageUri.ToUriString(),
                                NetValue = menuCategoryItemExtra.Price?.NetPrice ?? 0,
                                GrossValue = menuCategoryItemExtra.Price?.GrossPrice ?? 0,
                                TaxAmount = menuCategoryItemExtra.Price?.TotalTaxAmount ?? 0,
                                Unit = menuCategoryItemExtra.Unit.Name,
                                CategoryItem_Id = menuCategoryItem.Id
                            });

                            var extraTaxes = menuCategoryItemExtra.Price.Taxes ?? new List<PriceTax>();
                            extraTaxes.ForEach(tax =>
                            {
                                if (taxRates.TrueForAll(t => t != tax.Id))
                                {
                                    taxRates.Add(tax.Id);
                                    new TaxRateContext().Add(new TaxRate
                                    {
                                        Id = tax.Id,
                                        Name = tax.TaxName,
                                        Rate = tax.Rate,
                                        Description = tax.TaxRate?.Description ?? ""
                                    });
                                }

                                new ExtraTaxContext().Add(new xref_extra_tax
                                {
                                    TaxValue = tax.Amount,
                                    Tax_Id = tax.Id,
                                    Extra_Id = menuCategoryItemExtra.Id
                                });
                            });
                        });
                    });
                });

                ZipHelper.ZippedFile(tempPathRepository, databaseName);
                DirectoryHelper.DeleteFile($"{tempPathRepository}{databaseName}.db");

                var mediaProcess = new MediaProcess
                {
                    FileName = $"{databaseName}.zip",
                    ObjectKey = databaseName,
                    FilePath = $"{tempPathRepository}{databaseName}.zip"
                };

                await new AzureStorageRules(dbContext).UploadMediaFromPathByHangfireAsync(mediaProcess);

                posMenu.ProductFile = mediaProcess.ObjectKey;
                posMenu.IsPOSSqliteDbBeingProcessed = false;

                dbContext.SaveChanges();
            }
            catch
            {
                posMenu.IsPOSSqliteDbBeingProcessed = false;
                //LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, catalogId);
            }
            finally
            {
                //Ensure that if an error occurs in any of the product catalog hangfire processes
                //that the appropriate menu boolean is resent to false to ensure that if there is a problem the menu can be deleted.
                posMenu.IsPOSSqliteDbBeingProcessed = false;
                dbContext.SaveChanges();
            }
        }

        public async void CreateCatalogProductSqlite(int catalogId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "SqLiteProduct", null, null, catalogId);

            var posMenu = dbContext.PosMenus.Find(catalogId);
            try
            {
                var s3Object = await AzureStorageHelper.ReadObjectDataAsync(ConfigManager.SQLitePOSMenuName);

                var tempPathRepository = ConfigManager.TempPathRepository;
                var databaseName = $"{Guid.NewGuid()}";
                var filePath = $"{tempPathRepository}{databaseName}";
                DirectoryHelper.DeleteFile($"{filePath}.db");
                DirectoryHelper.DeleteFile($"{filePath}.zip");

                using (FileStream outputFileStream = new FileStream($"{filePath}.db", FileMode.Create))
                {
                    await s3Object.ObjectStream.CopyToAsync(outputFileStream);
                }

                ConfigManager.SQLiteConnectionString = $"Data Source={filePath}.db;Version=3;";

                var taxRates = new List<int>();
                var options = new List<int>();

                new MenuContext().Add(new Menu { Id = posMenu.Id, Name = posMenu.Name });

                posMenu.Categories.ForEach(menuCategory =>
                {
                    if (menuCategory.PosCategoryItems.Count == 0)
                        return;

                    new CategoryContext().Add(new Category
                    {
                        Id = menuCategory.Id,
                        Name = menuCategory.Name,
                        IsVisible = menuCategory.IsVisible ? 1 : 0,
                        Description = menuCategory.Description,
                        Menu_Id = posMenu.Id
                    });

                    //Items
                    menuCategory.PosCategoryItems.ForEach(menuCategoryItem =>
                    {
                        if (menuCategoryItem.PosVariants.Count == 0) return;
                        var variantDefaultId = menuCategoryItem.PosVariants.FirstOrDefault(e => e.IsDefault)?.Id ?? 0;

                        if (variantDefaultId <= 0) //Safe to verify
                            return;
                        // variant properties
                        var variantProperties = dbContext.PosVariantProperties.Where(c => c.CategoryItem.Id == menuCategoryItem.Id)
                                .Select(e => new Property { Id = e.Id, Name = e.Name, CategoryItem_Id = e.CategoryItem.Id }).ToList();

                        new PropertyContext().AddRange(variantProperties);

                        // variant options
                        var pIds = variantProperties.Select(e => e.Id);
                        var variantOptions = dbContext.PosVariantOptions.Where(e => pIds.Contains(e.VariantProperty.Id)).ToList();
                        variantOptions.ForEach(o =>
                        {
                            if (options.Any(e => e == o.Id)) return;
                            {
                                options.Add(o.Id);
                                new OptionContext().Add(new Option { Id = o.Id, Name = o.Name, Property_Id = o.VariantProperty.Id });
                            }
                        });

                        new CategoryItemContext().Add(new CategoryItem
                        {
                            Id = menuCategoryItem.Id,
                            Name = menuCategoryItem.Name,
                            Description = menuCategoryItem.Description,
                            ImageUri = menuCategoryItem.ImageUri.ToUriString(),
                            DefaultVariant = variantDefaultId,
                            Category_Id = menuCategory.Id
                        });
                        // variants
                        menuCategoryItem.PosVariants.ForEach(posVariant =>
                        {
                            if (string.IsNullOrEmpty(posVariant.TraderItem.SKU))
                                return;
                            //variant option exref
                            posVariant.VariantOptions.ForEach(vo =>
                            {
                                new VariantOptionContext().Add(new xref_variant_option
                                {
                                    Variant_Id = posVariant.Id,
                                    Option_Id = vo.Id
                                });
                            });

                            new VariantContext().Add(new Model.SQLite.Variant
                            {
                                Id = posVariant.Id,
                                Name = posVariant.Name,
                                Description = posVariant.TraderItem.Description,
                                SKU = posVariant.TraderItem.SKU,
                                Barcode = posVariant.TraderItem.Barcode,
                                ImageUri = posVariant.ImageUri.ToUriString(),
                                Unit = posVariant.Unit.Name,
                                NetValue = posVariant.Price?.NetPrice ?? 0,
                                GrossValue = posVariant.Price?.GrossPrice ?? 0,
                                TaxAmount = posVariant.Price?.TotalTaxAmount ?? 0,
                                CategoryItem_Id = menuCategoryItem.Id
                            });
                            var variantTaxes = posVariant.Price.Taxes ?? new List<PriceTax>();
                            variantTaxes.ForEach(tax =>
                            {
                                if (taxRates.TrueForAll(t => t != tax.Id))
                                {
                                    taxRates.Add(tax.Id);
                                    new TaxRateContext().Add(new TaxRate
                                    {
                                        Id = tax.Id,
                                        Name = tax.TaxName,
                                        Rate = tax.Rate,
                                        Description = tax.TaxRate?.Description ?? ""
                                    });
                                }

                                new VariantTaxContext().Add(new xref_variant_tax
                                {
                                    TaxValue = posVariant.Price.TotalTaxAmount,
                                    Tax_Id = tax.Id,
                                    Variant_Id = posVariant.Id
                                });
                            });
                        });
                        //Extra
                        menuCategoryItem.PosExtras.ForEach(menuCategoryItemExtra =>
                        {
                            if (string.IsNullOrEmpty(menuCategoryItemExtra.TraderItem.SKU))
                                return;
                            new ExtraContext().Add(new Extra
                            {
                                Id = menuCategoryItemExtra.Id,
                                Name = menuCategoryItemExtra.Name,
                                Description = menuCategoryItemExtra.TraderItem.Description,
                                SKU = menuCategoryItemExtra.TraderItem.SKU,
                                Barcode = menuCategoryItemExtra.TraderItem.Barcode,
                                ImageUri = menuCategoryItemExtra.TraderItem.ImageUri.ToUriString(),
                                NetValue = menuCategoryItemExtra.Price?.NetPrice ?? 0,
                                GrossValue = menuCategoryItemExtra.Price?.GrossPrice ?? 0,
                                TaxAmount = menuCategoryItemExtra.Price?.TotalTaxAmount ?? 0,
                                Unit = menuCategoryItemExtra.Unit.Name,
                                CategoryItem_Id = menuCategoryItem.Id
                            });

                            var extraTaxes = menuCategoryItemExtra.Price.Taxes ?? new List<PriceTax>();
                            extraTaxes.ForEach(tax =>
                            {
                                if (taxRates.TrueForAll(t => t != tax.Id))
                                {
                                    taxRates.Add(tax.Id);
                                    new TaxRateContext().Add(new TaxRate
                                    {
                                        Id = tax.Id,
                                        Name = tax.TaxName,
                                        Rate = tax.Rate,
                                        Description = tax.TaxRate?.Description ?? ""
                                    });
                                }

                                new ExtraTaxContext().Add(new xref_extra_tax
                                {
                                    TaxValue = menuCategoryItemExtra.Price.TotalTaxAmount,
                                    Tax_Id = tax.Id,
                                    Extra_Id = menuCategoryItemExtra.Id
                                });
                            });
                        });
                    });
                });

                ZipHelper.ZippedFile(tempPathRepository, databaseName);
                DirectoryHelper.DeleteFile($"{tempPathRepository}{databaseName}.db");

                var mediaProcess = new MediaProcess
                {
                    FileName = $"{databaseName}.zip",
                    ObjectKey = databaseName,
                    FilePath = $"{tempPathRepository}{databaseName}.zip"
                };

                posMenu.ProductFile = mediaProcess.ObjectKey;
                posMenu.IsPOSSqliteDbBeingProcessed = false;

                dbContext.SaveChanges();
            }
            catch
            {
                //LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, catalogId);
            }
            finally
            {
                //Ensure that if an error occurs in any of the product catalog hangfire processes
                //that the appropriate menu boolean is resent to false to ensure that if there is a problem the menu can be deleted.
                posMenu.IsPOSSqliteDbBeingProcessed = false;
                dbContext.SaveChanges();
            }
        }
    }
}