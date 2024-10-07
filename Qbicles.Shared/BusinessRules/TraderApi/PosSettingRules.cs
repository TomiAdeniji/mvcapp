using System;
using Qbicles.BusinessRules.Model;
using System.Linq;
using System.Net;
using Qbicles.Models.TraderApi;
using Qbicles.BusinessRules.Helper;
using System.Reflection;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.BusinessRules.Trader;

namespace Qbicles.BusinessRules.PoS
{
    public class PosSettingRules
    {
        ApplicationDbContext dbContext;

        public PosSettingRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public PosSettings GetByLocation(int locationId, string currentUserId, string timeZone = "")
        {

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetByLocation pos setting", null, null, locationId, timeZone);

                var posSetting = dbContext.PosSettings.FirstOrDefault(q => q.Location.Id == locationId);
                if (posSetting == null)
                {
                    posSetting = CreatePosSettingDefault(locationId, currentUserId);
                }
                if (string.IsNullOrEmpty(timeZone)) return posSetting;

                var utcDateTime = DateTime.UtcNow.Date + posSetting.RolloverTime;

                var zoneDateTime = utcDateTime.ConvertTimeFromUtc(timeZone);
                TimeSpan rollOutTimCurrent;

                if (!TimeSpan.TryParse(zoneDateTime.ToString("HH:mm"), out rollOutTimCurrent))
                {
                    rollOutTimCurrent = TimeSpan.Zero;
                }

                posSetting.RolloverTime = rollOutTimCurrent;

                return posSetting;

            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
                return null;
            }

        }

        public PosSettings CreatePosSettingDefault(int locationId, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create PosSetting Default", null, null, locationId);

                var location = dbContext.TraderLocations.Find(locationId);
                if (location == null) return null;
                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var posSetting = new PosSettings()
                {
                    Location = location,
                    ReceiptHeader = location.Name,
                    ReceiptFooter = location.Name,
                    RolloverTime = DateTime.UtcNow.TimeOfDay,
                    ProductPlaceholderImage = ConfigManager.DefaultProductPlaceholderImageUrl,
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                };
                dbContext.PosSettings.Add(posSetting);
                dbContext.SaveChanges();
                return posSetting;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
                return null;
            }

        }
        public ReturnJsonModel SaveSetting(PosSettings setting, string timeZone)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Pos SaveSetting", null, null, setting);

                if (!string.IsNullOrEmpty(setting.ProductPlaceholderImage))
                    new Azure.AzureStorageRules(dbContext).ProcessingMediaS3(setting.ProductPlaceholderImage);

                var currentDateTime = DateTime.Now.Date + setting.RolloverTime;

                var zoneDateTime = currentDateTime.ConvertTimeToUtc(timeZone);

                //var time = zoneDateTime.ToString("HH:mm");
                TimeSpan rollOutTimeUtc;

                if (!TimeSpan.TryParse(zoneDateTime.ToString("HH:mm"), out rollOutTimeUtc))
                {
                    rollOutTimeUtc = TimeSpan.Zero;
                }

                if (setting.DefaultWalkinCustomer != null && setting.DefaultWalkinCustomer.Id > 0)
                {
                    setting.DefaultWalkinCustomer = dbContext.TraderContacts.Find(setting.DefaultWalkinCustomer.Id);
                }
                else
                {
                    setting.DefaultWalkinCustomer = null;
                }
                if (setting.DefaultWorkGroup != null && setting.DefaultWorkGroup.Id > 0)
                {
                    setting.DefaultWorkGroup = dbContext.WorkGroups.Find(setting.DefaultWorkGroup.Id);
                }
                else
                {
                    setting.DefaultWalkinCustomer = null;
                }


                if (setting.Id == 0)
                {
                    if (string.IsNullOrEmpty(setting.ProductPlaceholderImage))
                        setting.ProductPlaceholderImage = ConfigManager.DefaultProductPlaceholderImageUrl;
                    setting.Location = new TraderLocationRules(dbContext).GetById(setting.Location.Id);
                    setting.RolloverTime = rollOutTimeUtc;
                    dbContext.PosSettings.Add(setting);
                    dbContext.Entry(setting).State = System.Data.Entity.EntityState.Added;
                    dbContext.SaveChanges();
                    refModel.msgId = setting.Id.ToString();
                }
                else
                {
                    var posSetting = dbContext.PosSettings.Find(setting.Id);
                    if (posSetting == null)
                    {
                        refModel.actionVal = 3;
                        refModel.result = false;
                        refModel.msg = "get setting error.";
                        return refModel;
                    }
                    posSetting.DefaultWalkinCustomer = setting.DefaultWalkinCustomer;
                    posSetting.DefaultWorkGroup = setting.DefaultWorkGroup;
                    posSetting.RolloverTime = rollOutTimeUtc;//setting.RolloverTime;
                    posSetting.MaxContactResult = setting.MaxContactResult;
                    posSetting.MoneyDecimalPlaces = setting.MoneyDecimalPlaces;
                    posSetting.MoneyCurrency = setting.MoneyCurrency;
                    posSetting.ReceiptHeader = setting.ReceiptHeader;
                    posSetting.ReceiptFooter = setting.ReceiptFooter;
                    if (!string.IsNullOrEmpty(setting.ProductPlaceholderImage))
                        posSetting.ProductPlaceholderImage = setting.ProductPlaceholderImage;
                    posSetting.OrderStatusWhenAddedToQueue = setting.OrderStatusWhenAddedToQueue;
                    dbContext.SaveChanges();
                    refModel.actionVal = 2;
                    refModel.msgId = posSetting.Id.ToString();
                }

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                refModel.actionVal = -1;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, setting);

            }


            return refModel;
        }

        public ReturnJsonModel SaveOrderDisplayRefreshSetting(PosSettings setting)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 2
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveOrderDisplayRefreshSetting", null, null, setting);


                var posSetting = dbContext.PosSettings.Find(setting.Id);
                if (posSetting != null && setting.Id > 0)
                {
                    posSetting.OrderDisplayRefreshInterval = setting.OrderDisplayRefreshInterval;
                    dbContext.SaveChanges();
                    refModel.msgId = posSetting.Id.ToString();
                }
                else
                {
                    refModel.actionVal = 3;
                    refModel.result = false;
                    refModel.msg = "get setting error.";
                }

            }
            catch (Exception ex)
            {
                refModel.msg = ex.Message;
                refModel.actionVal = 3;
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, setting);

            }

            return refModel;
        }
        public ReturnJsonModel SaveDeliveryDisplayRefreshSetting(PosSettings setting)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 2
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveDelivertyDisplayRefreshSetting", null, null, setting);


                var posSetting = dbContext.PosSettings.Find(setting.Id);
                if (posSetting != null && setting.Id > 0)
                {
                    posSetting.DeliveryDisplayRefreshInterval = setting.DeliveryDisplayRefreshInterval;
                    dbContext.SaveChanges();
                    refModel.msgId = posSetting.Id.ToString();
                }
                else
                {
                    refModel.actionVal = 3;
                    refModel.result = false;
                    refModel.msg = "get setting error.";
                }

            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, setting);

            }

            return refModel;
        }


        public ReturnJsonModel SaveDeliveryLingerTime(PosSettings setting)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 2
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveDeliveryLingerTime", null, null, setting);


                var posSetting = dbContext.PosSettings.Find(setting.Id);
                if (posSetting != null && setting.Id > 0)
                {
                    posSetting.LingerTime = setting.LingerTime;
                    dbContext.SaveChanges();
                    refModel.msgId = posSetting.Id.ToString();
                }
                else
                {
                    refModel.actionVal = 3;
                    refModel.result = false;
                    refModel.msg = "get setting error.";
                }

            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, setting);

            }

            return refModel;
        }


        public ReturnJsonModel SaveDeliveryThresholdTimeInterval(PosSettings setting)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 2
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SaveDeliveryThresholdTimeInterval", null, null, setting);


                var posSetting = dbContext.PosSettings.Find(setting.Id);
                if (posSetting != null && setting.Id > 0)
                {
                    posSetting.APICallThresholdTimeInterval = setting.APICallThresholdTimeInterval;

                    dbContext.SaveChanges();
                    refModel.msgId = posSetting.Id.ToString();
                }
                else
                {
                    refModel.actionVal = 3;
                    refModel.result = false;
                    refModel.msg = "get setting error.";
                }

            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, setting);

            }

            return refModel;
        }

        public ReturnJsonModel SaveSpeedDistance(PosSettings setting)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 2
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SpeedDistance", null, null, setting);


                var posSetting = dbContext.PosSettings.Find(setting.Id);
                if (posSetting != null && setting.Id > 0)
                {
                    posSetting.SpeedDistance = setting.SpeedDistance;
                    dbContext.SaveChanges();
                    refModel.msgId = posSetting.Id.ToString();
                }
                else
                {
                    refModel.actionVal = 3;
                    refModel.result = false;
                    refModel.msg = "get setting error.";
                }

            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, setting);

            }

            return refModel;
        }
        /// <summary>
        /// Trader Settings API for POS
        /// </summary>
        /// <returns></returns>
        public ReturnJsonModel GetPosSettings(PosRequest request)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosSettings", null, null, request);

                var device = dbContext.PosDevices.FirstOrDefault(e => e.SerialNumber == request.SerialNumber);

                if (device == null)
                {
                    refModel.Object = new IPosResult
                    {
                        IsTokenValid = true,
                        Message = "Device does not existed",
                        Status = HttpStatusCode.NotAcceptable
                    };
                    return refModel;
                }
                var posSetting = dbContext.PosSettings.FirstOrDefault(e => e.Location.Id == device.Location.Id);
                if (posSetting == null)
                {
                    refModel.Object = new IPosResult
                    {
                        IsTokenValid = true,
                        Message = "POS required config first.",
                        Status = HttpStatusCode.NotAcceptable
                    };
                    return refModel;
                }

                var user = new UserRules(dbContext).GetById(request.UserId);

                if (user == null)
                {
                    refModel.Object = new IPosResult
                    {
                        IsTokenValid = true,
                        Message = "The user does not existed",
                        Status = HttpStatusCode.NotAcceptable
                    };
                    return refModel;
                }

                //Get table list by location
                var listTables = new PosTableRules(dbContext).GetListPosTableByLocation(device.Location.Id);
                //Get table layout by location
                var tableLayout = new PosTableRules(dbContext).GetPosTableLayoutByLocation(device.Location.Id);


                refModel.result = true;
                var setting = new PosSettingsModel
                {
                    TabletPrefix = device.TabletPrefix,
                    MoneyCurrency = posSetting.MoneyCurrency,
                    MoneyDecimalPlaces = posSetting.MoneyDecimalPlaces,
                    ReceiptFooter = posSetting.ReceiptFooter,
                    ReceiptHeader = posSetting.ReceiptHeader,
                    RolloverTime = posSetting.RolloverTime,
                    Table = new Table
                    {
                        Tables = listTables.Select(t => new PosTableModel { Id = t.Id, Name = t.Name, Summary = t.Summary }).ToList(),
                        LayoutImage = tableLayout?.ImageUri.ToUri()
                    }
                };

                if (device.PosDeviceType?.PosOrderTypes.Count > 0)
                    setting.OrderTypes = device.PosDeviceType.PosOrderTypes.Select(e => new PosOrderTypeModel { Type = e.Name, Classification = (int)e.Classification }).ToList();

                device.MethodAccount?.ForEach(m =>
                {
                    setting.PaymentMethods.Add(new PaymentMethod
                    {
                        Id = m.Id,
                        Name = m.TabletDisplayName == "" ? m.PaymentMethod.Name : m.TabletDisplayName,
                        Type = m.PaymentMethod?.Name
                    });
                });

                refModel.Object = setting;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);
            }




            return refModel;
        }


        /// <summary>
        /// Preparation settings
        /// </summary>
        /// <returns></returns>
        public ReturnJsonModel GetPreparationSettings(PosRequest request)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPdsSettings", null, null, request);


                var deviceLocationId = dbContext.PrepDisplayDevices.FirstOrDefault(e => e.SerialNumber == request.SerialNumber)?.Location.Id;

                if (deviceLocationId == null)
                {
                    refModel.Object = new IPosResult
                    {
                        IsTokenValid = true,
                        Message = "Device does not existed",
                        Status = HttpStatusCode.NotAcceptable
                    };
                    return refModel;
                }

                var posSetting = dbContext.PosSettings.AsNoTracking().FirstOrDefault(e => e.Location.Id == deviceLocationId);
                if (posSetting == null)
                {
                    refModel.Object = new IPosResult
                    {
                        IsTokenValid = true,
                        Message = "POS required config first.",
                        Status = HttpStatusCode.NotAcceptable
                    };
                    return refModel;
                }

                refModel.result = true;
                refModel.Object = new PdsSettingsModel
                {
                    MoneyCurrency = posSetting.MoneyCurrency,
                    MoneyDecimalPlaces = posSetting.MoneyDecimalPlaces,
                    RefreshInterval = posSetting.OrderDisplayRefreshInterval
                };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);

            }

            return refModel;
        }


        /// <summary>
        /// Get Delivery setttings (DDS)
        /// </summary>
        /// <returns></returns>
        public ReturnJsonModel GetDeliverySettings(PosRequest request)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDdsSettings", null, null, request);

                var deviceLocation = dbContext.DdsDevice.AsNoTracking().FirstOrDefault(e => e.SerialNumber == request.SerialNumber)?.Location;

                if (deviceLocation == null)
                {
                    refModel.Object = new IPosResult
                    {
                        IsTokenValid = true,
                        Message = "Device does not existed",
                        Status = HttpStatusCode.NotAcceptable
                    };
                    return refModel;
                }

                var posSetting = dbContext.PosSettings.AsNoTracking().FirstOrDefault(e => e.Location.Id == deviceLocation.Id);
                if (posSetting == null)
                {
                    posSetting = CreatePosSettingDefault(deviceLocation.Id, request.UserId);
                }

                refModel.result = true;
                refModel.Object = new DdsSettingsModel
                {
                    MoneyCurrency = posSetting.MoneyCurrency,
                    MoneyDecimalPlaces = posSetting.MoneyDecimalPlaces,
                    RefreshInterval = posSetting.DeliveryDisplayRefreshInterval,
                    ReceiptFooter = posSetting.ReceiptFooter,
                    ReceiptHeader = posSetting.ReceiptHeader,
                    LingerTime = posSetting.LingerTime,
                    Location = new LocationModal
                    {
                        Latitude = deviceLocation.Address?.Latitude ?? 0,
                        Longitude = deviceLocation.Address?.Longitude ?? 0,
                    },
                    SpeedDistance = posSetting.SpeedDistance
                };

            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, request);

            }


            return refModel;
        }

    }
}
