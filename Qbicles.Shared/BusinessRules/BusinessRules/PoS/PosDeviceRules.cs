using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Qbicles.BusinessRules.PoS
{
    public class PosDeviceRules
    {
        ApplicationDbContext dbContext;

        public PosDeviceRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public PosDevice GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetById", null, null, id);

                return dbContext.PosDevices.Find(id);

            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                return null;
            }
        }

        public PosDevice GetBySerialNumber(string serialNumber)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetById", null, null, serialNumber);

                return dbContext.PosDevices.FirstOrDefault(e => e.SerialNumber == serialNumber && !e.Archived);

            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, serialNumber);
                return null;
            }
        }

        public List<PosDevice> GetByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetById", null, null, locationId);

                return dbContext.PosDevices.Where(e => e.Location.Id == locationId && !e.Archived).ToList();


            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return new List<PosDevice>();
            }
        }

        public List<PaymentMethod> GetPaymentMethods()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetById", null, null);


                return dbContext.PaymentMethods.OrderBy(n => n.Name).ToList();

            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null);
                return new List<PaymentMethod>();
            }
        }

        public List<TraderCashAccount> GetAccounts(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetAccounts", null, null, domainId);
                return dbContext.TraderCashAccounts.Where(q => q.Domain.Id == domainId).OrderBy(n => n.Name).ToList();


            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId);
                return new List<TraderCashAccount>();
            }
        }

        public List<PosDevice> SearchDevice(string name, int order, int locationId)
        {
            var devices = new List<PosDevice>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetById", null, null, name, order, locationId);


                switch (order)
                {
                    case 0:
                        devices = dbContext.PosDevices.Where(e => e.Name.Contains(name) && e.Location.Id == locationId && !e.Archived).ToList();
                        break;
                    case 1:
                        devices = dbContext.PosDevices.Where(e => e.Name.Contains(name) && e.Status == PosDeviceStatus.InActive && e.Location.Id == locationId && !e.Archived).ToList();
                        break;
                    case 2:
                        devices = dbContext.PosDevices.Where(e => e.Name.Contains(name) && e.Status == PosDeviceStatus.Active && e.Location.Id == locationId && !e.Archived).ToList();
                        break;
                }


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, name, order, locationId);
            }


            return devices;
        }

        public bool existsTabletPrefix(string tabletPrefix, int deviceId, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "existsTabletPrefix", null, null, tabletPrefix, deviceId, locationId);

                return dbContext.PosDevices.Any(q => q.TabletPrefix == tabletPrefix && q.Id != deviceId && q.Location.Id == locationId);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, tabletPrefix, deviceId, locationId);
                return false;
            }
        }

        public ReturnJsonModel CreateDevice(PosDevice device, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateDevice", null, null, device);

                var isValid = dbContext.PosDevices.Any(x =>
                     x.Name == device.Name && x.Location.Id == device.Location.Id && !x.Archived);
                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    return refModel;
                }
                if (device.PosDeviceType != null && device.PosDeviceType.Id > 0)
                {
                    device.PosDeviceType = dbContext.PosDeviceTypes.Find(device.PosDeviceType.Id);
                }
                else
                    device.PosDeviceType = null;

                device.CreatedBy = dbContext.QbicleUser.Find(userId);
                device.CreatedDate = DateTime.UtcNow;
                device.Status = PosDeviceStatus.InActive;
                device.Archived = false;
                if (device.PreparationQueue != null && device.PreparationQueue.Id > 0)
                {
                    device.PreparationQueue = dbContext.PrepQueues.Find(device.PreparationQueue.Id);
                }

                if (existsTabletPrefix(device.TabletPrefix, device.Id, device.Location.Id))
                {
                    refModel.actionVal = 3;
                    refModel.result = false;
                    return refModel;
                }
                dbContext.PosDevices.Add(device);
                dbContext.Entry(device).State = EntityState.Added;
                dbContext.SaveChanges();

                var deviceHtml = new StringBuilder();
                deviceHtml.Append($"");
                deviceHtml.Append($"<article class='col' id='pos-device-{device.Id}'>");
                deviceHtml.Append($"<div class='qbicle-opts dropdown'>");
                deviceHtml.Append($"<a href='javascript:' data-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>");
                deviceHtml.Append($"<i class='fa fa-cog'></i>");
                deviceHtml.Append($"</a>");
                deviceHtml.Append($"<ul class='dropdown-menu primary dropdown-menu-right' style='right: 0;'>");
                deviceHtml.Append($"<li>");
                deviceHtml.Append($"<a href='javascript:' onclick='ConfirmDeleteDevice({device.Id},\"{device.Name}\")' data-toggle='modal' data-target='#confirm-delete'>Delete</a>");
                deviceHtml.Append($"</li>");
                deviceHtml.Append($"</ul>");
                deviceHtml.Append($"</div>");
                deviceHtml.Append($"<a href='/PointOfSale/PoSDevice?id={device.Id}'>");
                deviceHtml.Append($"<div class='avatar' style='background-image: url(\"/Content/DesignStyle/img/icon_ipad.png\");'>&nbsp;</div>");
                deviceHtml.Append($"<h1 style='color: #333;'>{device.Name}</h1>");
                deviceHtml.Append($"</a>");
                deviceHtml.Append($"<p class='qbicle-detail' style='white-space: pre-wrap !important;'>{device.Summary}</p><br />");
                deviceHtml.Append($"<span class='label label-lg label-warning'>Inactive</span>");
                deviceHtml.Append($"<br /><br />");
                deviceHtml.Append($"</article>");


                refModel.msg = deviceHtml.ToString();

            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, device);

                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }
            //valid name

            return refModel;
        }

        /// <summary>
        /// Update device menu
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public ReturnJsonModel UpdateDeviceMenu(PosDevice device)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateDeviceMenu", null, null, device);

                var deviceUpdate = dbContext.PosDevices.Find(device.Id);
                if (deviceUpdate == null) return refModel;
                if (device.Menu.Id == 0)
                {
                    var menu = deviceUpdate.Menu;
                    menu.Devices.Remove(deviceUpdate);
                    if (dbContext.Entry(menu).State == EntityState.Detached)
                        dbContext.PosMenus.Attach(menu);
                    dbContext.Entry(menu).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    return refModel;
                }

                deviceUpdate.Menu = new PosMenuRules(dbContext).GetById(device.Menu.Id);
                if (dbContext.Entry(deviceUpdate).State == EntityState.Detached)
                    dbContext.PosDevices.Attach(deviceUpdate);
                dbContext.Entry(deviceUpdate).State = EntityState.Modified;                
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, device);

                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return refModel;
        }

        public ReturnJsonModel UpdateDeviceQueue(PosDevice device)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateDeviceQueue", null, null, device);


                var deviceUpdate = dbContext.PosDevices.Find(device.Id);
                if (deviceUpdate == null) return refModel;
                if (device.PreparationQueue.Id == 0)
                {
                    var preparationQueue = deviceUpdate.PreparationQueue;
                    preparationQueue.AssociatedPosDevices.Remove(deviceUpdate);
                    if (dbContext.Entry(preparationQueue).State == EntityState.Detached)
                        dbContext.PrepQueues.Attach(preparationQueue);
                    dbContext.Entry(preparationQueue).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    return refModel;
                }
                deviceUpdate.PreparationQueue = new PDSRules(dbContext).GetQueueById(device.PreparationQueue.Id);

                if (dbContext.Entry(deviceUpdate).State == EntityState.Detached)
                    dbContext.PosDevices.Attach(deviceUpdate);
                dbContext.Entry(deviceUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, device);

                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }
            return refModel;
        }

        public ReturnJsonModel SavePaymentMethod(PosDevice device, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SavePaymentMethod", null, null, device);


                if (device.MethodAccount.Count > 0)
                {
                    if ((device.MethodAccount[0].PaymentMethod == null || (device.MethodAccount[0].PaymentMethod != null && device.MethodAccount[0].PaymentMethod.Id == 0))
                        || (device.MethodAccount[0].CollectionAccount == null || (device.MethodAccount[0].CollectionAccount != null && device.MethodAccount[0].CollectionAccount.Id == 0)))
                    {
                        refModel.actionVal = 3;
                        refModel.msg = ResourcesManager._L("ERROR_MSG_671");
                        refModel.result = false;
                        return refModel;
                    }
                    var deviceUpdate = dbContext.PosDevices.Find(device.Id);
                    if (deviceUpdate.MethodAccount.Count > 0)
                    {
                        if (deviceUpdate.MethodAccount.Any(q => q.CollectionAccount.Id == device.MethodAccount[0].CollectionAccount.Id && q.PaymentMethod.Id == device.MethodAccount[0].PaymentMethod.Id && q.Id != device.MethodAccount[0].Id))
                        {
                            refModel.actionVal = 3;
                            refModel.msg = ResourcesManager._L("ERROR_MSG_679");
                            refModel.result = false;
                            return refModel;
                        }
                    }
                    var methodAcc = device.MethodAccount[0];
                    if (methodAcc.CollectionAccount != null && methodAcc.CollectionAccount.Id > 0)
                    {
                        methodAcc.CollectionAccount = dbContext.TraderCashAccounts.Find(methodAcc.CollectionAccount.Id);
                    }
                    else methodAcc.CollectionAccount = null;
                    if (methodAcc.PaymentMethod != null && methodAcc.PaymentMethod.Id > 0)
                    {
                        methodAcc.PaymentMethod = dbContext.PaymentMethods.Find(methodAcc.PaymentMethod.Id);
                    }
                    else methodAcc.PaymentMethod = null;
                    if (methodAcc.Id == 0)
                    {

                        methodAcc.CreatedBy = dbContext.QbicleUser.Find(userId);
                        methodAcc.CreatedDate = DateTime.UtcNow;
                        deviceUpdate.MethodAccount.Add(methodAcc);
                    }
                    else
                    {
                        var dmethodAcc = deviceUpdate.MethodAccount.FirstOrDefault(q => q.Id == methodAcc.Id);
                        dmethodAcc.PaymentMethod = methodAcc.PaymentMethod;
                        dmethodAcc.TabletDisplayName = methodAcc.TabletDisplayName;
                        dmethodAcc.CollectionAccount = methodAcc.CollectionAccount;
                        refModel.actionVal = 2;
                    }
                    if (deviceUpdate.Menu != null)
                        deviceUpdate.Menu.IsPOSSqliteDbBeingProcessed = true;
                    dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, device);
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
            }

            return refModel;
        }
        public ReturnJsonModel DeletePaymentMethod(PosDevice device)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeletePaymentMethod", null, null, device);


                if (device.MethodAccount.Count > 0)
                {
                    var deviceUpdate = dbContext.PosDevices.Find(device.Id);
                    var methodAcc = device.MethodAccount[0];

                    methodAcc = deviceUpdate.MethodAccount.FirstOrDefault(q => q.Id == methodAcc.Id);
                    deviceUpdate.MethodAccount.Remove(methodAcc);

                    refModel.actionVal = 2;
                    if (deviceUpdate.Menu != null)
                        deviceUpdate.Menu.IsPOSSqliteDbBeingProcessed = true;
                    dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, device);
            }

            return refModel;
        }

        public ReturnJsonModel UpdateDevice(PosDevice device)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateDevice", null, null, device);


                //valid name
                var isValid = dbContext.PosDevices.Any(x =>
                     x.Id != device.Id && x.Location.Domain.Id == device.Location.Domain.Id && x.Name == device.Name);
                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    return refModel;
                }
                if (existsTabletPrefix(device.TabletPrefix, device.Id, device.Location.Id))
                {
                    refModel.actionVal = 3;
                    refModel.result = false;
                    return refModel;
                }

                var deviceUpdate = dbContext.PosDevices.Find(device.Id);
                if (deviceUpdate == null) return refModel;

                deviceUpdate.Name = device.Name;
                deviceUpdate.SerialNumber = device.SerialNumber;
                deviceUpdate.Summary = device.Summary;
                deviceUpdate.TabletPrefix = device.TabletPrefix;

                if (device.PosDeviceType != null && device.PosDeviceType.Id > 0)
                {
                    deviceUpdate.PosDeviceType = dbContext.PosDeviceTypes.Find(device.PosDeviceType.Id);
                }

                if (dbContext.Entry(deviceUpdate).State == EntityState.Detached)
                    dbContext.PosDevices.Attach(deviceUpdate);
                dbContext.Entry(deviceUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, device);

            }

            return refModel;
        }

        public ReturnJsonModel ActiveDevice(PosDevice device)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "ActiveDevice", null, null, device);

                var deviceUpdate = dbContext.PosDevices.Find(device.Id);
                if (deviceUpdate == null) return refModel;

                deviceUpdate.Status = device.Status;

                if (dbContext.Entry(deviceUpdate).State == EntityState.Detached)
                    dbContext.PosDevices.Attach(deviceUpdate);
                dbContext.Entry(deviceUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, device);

            }
            return refModel;

        }

        public ReturnJsonModel DeleteDevice(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeleteDevice", null, null, id);


                var device = GetById(id);
                if (dbContext.PosApiRequestLogs.Any(d => d.Device.Id == id))
                {
                    device.Archived = true;
                    if (dbContext.Entry(device).State == EntityState.Detached)
                        dbContext.PosDevices.Attach(device);
                    dbContext.Entry(device).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModel.result = true;
                    return refModel;
                }

                dbContext.PosPaymentMethodAccountXrefs.RemoveRange(device.MethodAccount);
                dbContext.PosAdministrators.RemoveRange(device.Administrators);
                dbContext.PosTillManagers.RemoveRange(device.DeviceManagers);
                dbContext.PosSupervisors.RemoveRange(device.DeviceSupervisors);
                dbContext.PosCashiers.RemoveRange(device.DeviceCashiers);
                dbContext.DeviceUsers.RemoveRange(device.Users);


                dbContext.PosDevices.Remove(device);
                dbContext.SaveChanges();
                refModel.result = true;

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ResourcesManager._L("ERROR_DETAIL", ex.Message);
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
            }
            return refModel;
        }
    }
}
