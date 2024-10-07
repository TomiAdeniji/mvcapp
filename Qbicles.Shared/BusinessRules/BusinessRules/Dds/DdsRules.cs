using DocumentFormat.OpenXml.Drawing.Charts;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.MyBankMate;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.PoS
{
    public class DdsRules
    {
        ApplicationDbContext dbContext;

        public DdsRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public List<PrepQueue> GetPrepQueueFreeDelivery(int locationId, int deliveryQueueId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "MESSAGE_INPUT", null, null, locationId, deliveryQueueId);


                string sql =
                    $"select * from ods_PrepQueue where location_id = {locationId} and id not in(Select prepQueue_id from dds_DeliveryQueue where id <> {deliveryQueueId});";
                var prepQueues = dbContext.Database.SqlQuery<PrepQueue>(sql).ToList();
                return prepQueues;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId, deliveryQueueId);
                return new List<PrepQueue>();
            }
        }

        // -------- Delivery Queue -----------

        public List<DeliveryQueue> GetDeliveriesQueueByDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDeliveriesQueueByDomain", null, null, domainId);

                return dbContext.DeliveryQueues.Where(d => d.Location.Domain.Id == domainId).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId);
                return new List<DeliveryQueue>();
            }
        }

        public List<DeliveryQueue> GetDeliveriesQueueByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDeliveriesQueueByLocation", null, null, locationId);
                return dbContext.DeliveryQueues.Where(d => d.Location.Id == locationId).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return new List<DeliveryQueue>();
            }
        }
        public DeliveryQueue GetDeliveryQueueByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDeliveryQueueByLocation", null, null, locationId);

                return dbContext.DeliveryQueues.Where(e => e.Location.Id == locationId).FirstOrDefault();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return new DeliveryQueue();
            }

        }
        public DeliveryQueue GetDeliveriesQueueById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDeliveriesQueueById", null, null, id);


                return dbContext.DeliveryQueues.Find(id) ?? new DeliveryQueue();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                return new DeliveryQueue();
            }
        }

        public ReturnJsonModel CreateDeliveryQueue(DeliveryQueue deliveryQueue, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateDeliveryQueue", null, null, deliveryQueue);


                //valid name
                bool isValid;

                if (deliveryQueue.Id > 0)
                    isValid = dbContext.DeliveryQueues.Any(x => x.Id != deliveryQueue.Id && x.Name == deliveryQueue.Name && x.Location.Id == deliveryQueue.Location.Id);
                else
                    isValid = dbContext.DeliveryQueues.Any(x => x.Name == deliveryQueue.Name && x.Location.Id == deliveryQueue.Location.Id);

                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_DATA_EXISTED", deliveryQueue.Name);
                    return refModel;
                }

                deliveryQueue.CreatedDate = DateTime.UtcNow;
                deliveryQueue.PrepQueue = dbContext.PrepQueues.Find(deliveryQueue.PrepQueue.Id);


                var deliveryQueueArchive = new DeliveryQueueArchive
                {
                    CreatedBy = dbContext.QbicleUser.Find(userId),
                    Name = $"{deliveryQueue.Name} Archive",
                    CreatedDate = DateTime.UtcNow,
                    Location = deliveryQueue.Location,
                    ParentDeliveryQueue = deliveryQueue,
                    PrepQueue = deliveryQueue.PrepQueue
                };

                dbContext.DeliveryQueueArchives.Add(deliveryQueueArchive);
                dbContext.Entry(deliveryQueueArchive).State = EntityState.Added;

                dbContext.DeliveryQueues.Add(deliveryQueue);
                dbContext.Entry(deliveryQueue).State = EntityState.Added;
                dbContext.SaveChanges();

                var deviceHtml = new StringBuilder();
                deviceHtml.Append($"<article id='dds-queue-item-{deliveryQueue.Id}' class='col'>");
                deviceHtml.Append($"<div class='qbicle-opts'>");
                deviceHtml.Append($"<div class='dropdown'>");
                deviceHtml.Append($"<a href='javascript:void(0);' class='dropdown-toggle' data-toggle='dropdown'>");
                deviceHtml.Append($"<i class='fa fa-cog'></i>");
                deviceHtml.Append($"</a>");
                deviceHtml.Append($"<ul class='dropdown-menu dropdown-menu-right primary'>");
                deviceHtml.Append($"<li>");
                deviceHtml.Append($"<a href='javascript:' onclick='QueueAddEdit({deliveryQueue.Id},2)'>Edit</a>");
                deviceHtml.Append($"</li>");
                deviceHtml.Append($"<li>");
                deviceHtml.Append($"<a href='javascript:void(0)' onclick=\"ConfirmDeletePrepQueue({deliveryQueue.Id},2)\">Delete</a>");
                deviceHtml.Append($"</li>");
                deviceHtml.Append($"</ul>");
                deviceHtml.Append($"</div>");

                deviceHtml.Append($"</div>");



                deviceHtml.Append($"<a href='javascript:void(0);' style='cursor: initial !important;'>");
                deviceHtml.Append($"<div class='avatar' style='border-radius: 0; background-image: url(\"/Content/DesignStyle/img/takeaway.png\");'>&nbsp;</div>");
                deviceHtml.Append($"<h1 style='color: #333;'><span id='dds-queue-name-main-{deliveryQueue.Id}'>{deliveryQueue.Name}</span></h1>");
                deviceHtml.Append($"</a>");
                deviceHtml.Append($"<p class='qbicle-detail'>");
                deviceHtml.Append($"<label style='color: #333;'>DDS Devices</label>");
                deviceHtml.Append($"<br /><br />");
                deviceHtml.Append($"<label style='color: #333;'>Order Queue</label><br />{deliveryQueue.PrepQueue?.Name}");
                deviceHtml.Append($"</p>");
                deviceHtml.Append($"</article>");


                refModel.msg = deviceHtml.ToString();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, deliveryQueue);
                refModel.result = false;
                refModel.actionVal = -1;
            }
            return refModel;
        }

        public ReturnJsonModel UpdateDeliveryQueue(DeliveryQueue deliveryQueue)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateDeliveryQueue", null, null, deliveryQueue);


                bool isValid;

                if (deliveryQueue.Id > 0)
                    isValid = dbContext.DeliveryQueues.Any(x => x.Id != deliveryQueue.Id && x.Name == deliveryQueue.Name && x.Location.Id == deliveryQueue.Location.Id);
                else
                    isValid = dbContext.DeliveryQueues.Any(x => x.Name == deliveryQueue.Name && x.Location.Id == deliveryQueue.Location.Id);

                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_DATA_EXISTED", deliveryQueue.Name);
                    return refModel;
                }
                var deliveryQueueUpdate = dbContext.DeliveryQueues.Find(deliveryQueue.Id);
                if (deliveryQueueUpdate == null) return refModel;
                deliveryQueueUpdate.Name = deliveryQueue.Name;
                deliveryQueueUpdate.PrepQueue = dbContext.PrepQueues.Find(deliveryQueue.PrepQueue.Id);

                var deliveryQueueArchiveUpdate =
                    dbContext.DeliveryQueueArchives.FirstOrDefault(e => e.ParentDeliveryQueue.Id == deliveryQueueUpdate.Id);
                if (deliveryQueueArchiveUpdate == null)
                {
                    var prepQueueArchive = new DeliveryQueueArchive
                    {
                        CreatedBy = deliveryQueueUpdate.CreatedBy,
                        Name = $"{deliveryQueueUpdate.Name} Archive",
                        CreatedDate = DateTime.UtcNow,
                        Location = deliveryQueueUpdate.Location,
                        ParentDeliveryQueue = deliveryQueueUpdate,
                        PrepQueue = deliveryQueueUpdate.PrepQueue
                    };

                    dbContext.DeliveryQueueArchives.Add(prepQueueArchive);
                    dbContext.Entry(prepQueueArchive).State = EntityState.Added;
                }
                else
                {
                    deliveryQueueArchiveUpdate.Name = deliveryQueueUpdate.Name;
                    deliveryQueueArchiveUpdate.PrepQueue = deliveryQueueUpdate.PrepQueue;

                    if (dbContext.Entry(deliveryQueueArchiveUpdate).State == EntityState.Detached)
                        dbContext.DeliveryQueueArchives.Attach(deliveryQueueArchiveUpdate);
                    dbContext.Entry(deliveryQueueArchiveUpdate).State = EntityState.Modified;
                }



                if (dbContext.Entry(deliveryQueueUpdate).State == EntityState.Detached)
                    dbContext.DeliveryQueues.Attach(deliveryQueueUpdate);
                dbContext.Entry(deliveryQueueUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, deliveryQueue);
                refModel.result = false;
                refModel.actionVal = -1;
            }


            return refModel;
        }

        public ReturnJsonModel DeleteDeliveryQueue(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeleteDeliveryQueue", null, null, id);



                var queue = GetDeliveriesQueueById(id);
                var ddsDevices = dbContext.DdsDevice.Where(e => e.Queue.Id == id);
                if (ddsDevices.Any() || queue.Deliveries.Any())
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_DATA_IN_USED_CAN_NOT_DELETE", queue.Name);
                    return refModel;
                }


                if (queue.Id > 0)
                    dbContext.DeliveryQueues.Remove(queue);

                var deliveryQueueArchive =
                    dbContext.DeliveryQueueArchives.FirstOrDefault(e => e.ParentDeliveryQueue.Id == queue.Id);
                if (deliveryQueueArchive != null)
                    dbContext.DeliveryQueueArchives.Remove(deliveryQueueArchive);


                dbContext.SaveChanges();
                refModel.result = true;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);

            }
            return refModel;
        }

        // -------- Delivery Device -----------


        public List<DdsDevice> SearchDevice(string name, int ddsQueueId, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SearchDevice", null, null, name, ddsQueueId, locationId);


                if (ddsQueueId == 0)
                    return dbContext.DdsDevice.Where(e => e.Name.ToLower().Contains(name.ToLower()) && e.Location.Id == locationId).ToList();
                else
                    return dbContext.DdsDevice.Where(e => e.Name.ToLower().Contains(name.ToLower()) && e.Location.Id == locationId && e.Queue.Id == ddsQueueId).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, name, ddsQueueId, locationId);
                return new List<DdsDevice>();
            }

        }

        public List<DdsDevice> GetDdsDevicesByLocationId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDdsDevicesByLocationId", null, null, id);

                return dbContext.DdsDevice.Where(e => e.Location.Id == id).ToList();


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                return new List<DdsDevice>();
            }
        }
        public DdsDevice GetDdsDeviceById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDdsDeviceById", null, null, id);

                return dbContext.DdsDevice.Find(id) ?? new DdsDevice();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                return new DdsDevice();
            }

        }

        public ReturnJsonModel CreateDdsDevice(DdsDevice device, string adminIds, string userIds, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateDdsDevice", null, null, device, adminIds, userIds);



                //valid name
                bool isValid;

                if (device.Id > 0)
                    isValid = dbContext.DdsDevice.Any(x => x.Id != device.Id && x.Name == device.Name && x.Location.Id == device.Location.Id);
                else
                    isValid = dbContext.DdsDevice.Any(x => x.Name == device.Name && x.Location.Id == device.Location.Id);

                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_DATA_EXISTED", device.Name);
                    return refModel;
                }

                device.CreatedBy = dbContext.QbicleUser.Find(userId);
                device.CreatedDate = DateTime.UtcNow;
                device.Queue = GetDeliveriesQueueById(device.Queue.Id);
                device.Type = new PDSRules(dbContext).GetOdsDeviceTypeId(device.Type.Id);

                var posUserRule = new DeviceUserRules(dbContext);

                foreach (var id in userIds.Split(','))
                {
                    //making it work with current pos-user-roles:QBIC-5299
                    var posUser = posUserRule.GetById(int.Parse(id));

                    CreatePosUserTill(device, posUser);

                }


                var userRule = new UserRules(dbContext);
                foreach (var id in adminIds.Split(','))
                {
                    device.Administrators.Add(userRule.GetById(id));
                }


                dbContext.DdsDevice.Add(device);
                dbContext.Entry(device).State = EntityState.Added;
                dbContext.SaveChanges();

                var deviceHtml = $"<article id='dds-device-item-{device.Id}' class='col'>";
                deviceHtml += $"<span class='last-updated'><span id='device-queue-main-{device.Id}'></span>{device.Queue.Name}</span>";
                deviceHtml += $"<span style='top: 40px !important;' class='last-updated'><span id='device-type-main-{device.Id}'>{device.Type?.Name ?? "Device type is empty"}</span></span>";
                deviceHtml += "<div class='qbicle-opts'>";
                deviceHtml += "<div class='dropdown'>";
                deviceHtml += "<a href='javascript:' class='dropdown-toggle' data-toggle='dropdown'>";
                deviceHtml += "<i class='fa fa-cog'></i>";
                deviceHtml += "</a>";
                deviceHtml += "<ul class='dropdown-menu dropdown-menu-right primary'>";
                deviceHtml += $"<li><a onclick='DdsDeviceAddEdit({device.Id})'>Edit</a></li>";
                deviceHtml += $"<li><a onclick='ConfirmDeleteDevice({device.Id})'>Delete</a></li>";
                deviceHtml += "</ul>";
                deviceHtml += "</div>";
                deviceHtml += "</div>";
                deviceHtml += "<a href='javascript:' style='cursor: initial !important;'>";
                deviceHtml += "<div class='avatar' style='border-radius: 0; background-image: url(\"/Content/DesignStyle/img/dds.png\");'>&nbsp;</div>";
                deviceHtml += $"<h1 style='color: #333;'><span id='device-name-main-{device.Id}'>{device.Name}</span></h1>";
                deviceHtml += "</a>";
                deviceHtml += "<br />";
                deviceHtml += "<p class='qbicle-detail'>";
                deviceHtml += "<label style='color: #333;'>Administrators</label><br />";

                var adminStrings = string.Join(", ", device.Administrators.Select(e => e.DisplayUserName));

                deviceHtml += $"<span id='device-admin-name-{device.Id}'>{adminStrings}</span>";
                deviceHtml += "</p>";
                deviceHtml += "<br />";
                deviceHtml += "<p class='qbicle-detail'>";
                deviceHtml += "<label style='color: #333;'>Users</label><br />";

                var userStrings = string.Join(", ", device.Users.Select(e => e.User.DisplayUserName));

                deviceHtml += $"<span id='device-user-name-{device.Id}'>{userStrings}</span>";
                deviceHtml += "</p>";
                deviceHtml += "<br />";
                deviceHtml += "<p class='qbicle-detail'>";
                deviceHtml += "<label style='color: #333;'>Serial Number</label><br />";
                deviceHtml += $"<span>{device.SerialNumber}</span>";
                deviceHtml += "</p>";
                deviceHtml += "</article>";


                refModel.msg = deviceHtml;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, device, adminIds, userIds);
                refModel.result = false;
                refModel.actionVal = 1;
            }
            return refModel;
        }

        private static void CreatePosUserTill(DdsDevice device, PosRoleUsersViewModel posUser)
        {
            //TODO: Jira story must be open for it
            switch (posUser.PosUserType)
            {
                case PosUserType.None:
                case PosUserType.User:
                    device.Users.Add(new DeviceUser
                    {
                        CreatedDate = DateTime.UtcNow,
                        User = posUser.User,
                        CreatedBy = posUser.CreatedBy
                    });
                    break;
                case PosUserType.Cashier:
                    device.Users.Add(new DeviceUser
                    {
                        CreatedDate = DateTime.UtcNow,
                        User = posUser.User,
                        CreatedBy = posUser.CreatedBy
                    });
                    break;
                case PosUserType.Supervisor:
                    device.Users.Add(new DeviceUser
                    {
                        CreatedDate = DateTime.UtcNow,
                        User = posUser.User,
                        CreatedBy = posUser.CreatedBy
                    });
                    break;
                case PosUserType.Manager:
                    device.Users.Add(new DeviceUser
                    {
                        CreatedDate = DateTime.UtcNow,
                        User = posUser.User,
                        CreatedBy = posUser.CreatedBy
                    });
                    break;
            }
        }
        

        public ReturnJsonModel UpdateDdsDevice(DdsDevice device, string adminIds, string userIds)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateDdsDevice", null, null, device, adminIds, userIds);


                bool isValid;

                if (device.Id > 0)
                    isValid = dbContext.DdsDevice.Any(x => x.Id != device.Id && x.Name == device.Name && x.Location.Id == device.Location.Id);
                else
                    isValid = dbContext.DdsDevice.Any(x => x.Name == device.Name && x.Location.Id == device.Location.Id);

                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_643");
                    return refModel;
                }
                var deviceUpdate = GetDdsDeviceById(device.Id);
                if (deviceUpdate == null) return refModel;
                deviceUpdate.Name = device.Name;
                deviceUpdate.Queue = dbContext.DeliveryQueues.Find(device.Queue.Id);
                deviceUpdate.Type = new PDSRules(dbContext).GetOdsDeviceTypeId(device.Type.Id);

                deviceUpdate.Administrators.Clear();
                deviceUpdate.Users.Clear();


                var posUserRule = new DeviceUserRules(dbContext);

                foreach (var id in userIds.Split(','))
                {
                    var posUser = posUserRule.GetById(int.Parse(id));

                    CreatePosUserTill(device, posUser);
                }



                var userRule = new UserRules(dbContext);
                foreach (var id in adminIds.Split(','))
                {
                    deviceUpdate.Administrators.Add(userRule.GetById(id));
                }



                if (dbContext.Entry(deviceUpdate).State == EntityState.Detached)
                    dbContext.DdsDevice.Attach(deviceUpdate);
                dbContext.Entry(deviceUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, device, adminIds, userIds);
                refModel.result = false;
                refModel.actionVal = -1;
            }


            return refModel;
        }

        public ReturnJsonModel DeleteDdsDevice(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeleteDdsDevice", null, null, id);

                var device = GetDdsDeviceById(id);


                if (device.Id == 0)
                    return refModel;
                device.Administrators.Clear();
                device.Users.Clear();
                dbContext.DdsDevice.Remove(device);


                dbContext.SaveChanges();
                refModel.result = true;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);

            }

            return refModel;
        }

        // -------- Delivery Driver -----------


        public List<Driver> SearchDriver(string name, int locationId, int statusId, int domainId, bool isDelete = true)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SearchDriver", null, null, name, locationId, statusId, domainId);

                var drivers = dbContext.Drivers.Where(e => e.Domain.Id == domainId && e.IsDeleted == isDelete);
                if (string.IsNullOrEmpty(name) == false)
                    drivers = drivers.Where(e => e.User.User.DisplayUserName.ToLower().Contains(name.ToLower()));

                if (locationId > 0)
                    drivers = drivers.Where(e => e.EmploymentLocation.Id == locationId);

                if (statusId > 0)
                {
                    var status = (DriverStatus)statusId;
                    drivers = drivers.Where(e => e.Status == status);
                }
                return drivers.ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, name, locationId, statusId, domainId);

            }


            return new List<Driver>();
        }

        public List<TraderLocation> GetDdsLocationsQueue(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDdsLocationsQueue", null, null, domainId);


                string sql =
                    $"select * from trad_location where id in(Select Location_Id from dds_deliveryqueue) and Domain_Id={domainId};";
                var prepQueues = dbContext.Database.SqlQuery<TraderLocation>(sql).ToList();
                return prepQueues;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId);
                return new List<TraderLocation>();
            }
        }

        public List<Driver> GetDdsDriversByLocation(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDdsDriversByLocation", null, null, id);

                return dbContext.Drivers.Where(e => e.EmploymentLocation.Id == id).ToList();


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                return new List<Driver>();
            }
        }
        public List<Driver> GetDdsDriversByDomain(int domainId, bool isDelete)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDdsDriversByDomain", null, null, domainId);

                return dbContext.Drivers.Where(e => e.Domain.Id == domainId && e.IsDeleted == isDelete).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId);
                return new List<Driver>();
            }
        }

        public Driver GetDdsDriverById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDdsDriverById", null, null, id);
                return dbContext.Drivers.Find(id) ?? new Driver();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                return new Driver();
            }

        }

        public ReturnJsonModel CreateDdsDriver(string userIds, int locationId, int domainId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreateDdsDriver", null, null, userIds, locationId, domainId);

                var domain = new DomainRules(dbContext).GetDomainById(domainId);


                var drivers = new List<Driver>();
                var userRule = new DeviceUserRules(dbContext);

                var driversId = userIds.Split(',');
                foreach (var d in driversId)
                {
                    //TODO: Jira story must be open for it
                    var posUser = userRule.GetById(int.Parse(d));
                    switch (posUser.PosUserType)
                    {
                        case PosUserType.None:
                        case PosUserType.User:
                            drivers.Add(new Driver
                            {
                                //EmploymentLocation = location,The EmploymentLocation of the new Driver is not set.
                                Domain = domain,
                                Status = DriverStatus.IsAvailable,
                                AtWork = false,
                                User = new DeviceUser
                                {
                                    CreatedDate = DateTime.UtcNow,
                                    User = posUser.User,
                                    CreatedBy = posUser.CreatedBy
                                }
                            });
                            break;
                        case PosUserType.Cashier:
                            drivers.Add(new Driver
                            {
                                //EmploymentLocation = location,The EmploymentLocation of the new Driver is not set.
                                Domain = domain,
                                Status = DriverStatus.IsAvailable,
                                AtWork = false,
                                User = new DeviceUser
                                {
                                    CreatedDate = DateTime.UtcNow,
                                    User = posUser.User,
                                    CreatedBy = posUser.CreatedBy
                                }
                            });
                            break;
                        case PosUserType.Supervisor:
                            drivers.Add(new Driver
                            {
                                //EmploymentLocation = location,The EmploymentLocation of the new Driver is not set.
                                Domain = domain,
                                Status = DriverStatus.IsAvailable,
                                AtWork = false,
                                User = new DeviceUser
                                {
                                    CreatedDate = DateTime.UtcNow,
                                    User = posUser.User,
                                    CreatedBy = posUser.CreatedBy
                                }
                            });
                            break;
                        case PosUserType.Manager:
                            drivers.Add(new Driver
                            {
                                //EmploymentLocation = location,The EmploymentLocation of the new Driver is not set.
                                Domain = domain,
                                Status = DriverStatus.IsAvailable,
                                AtWork = false,
                                User = new DeviceUser
                                {
                                    CreatedDate = DateTime.UtcNow,
                                    User = posUser.User,
                                    CreatedBy = posUser.CreatedBy
                                }
                            });
                            break;
                    }

                   
                }


                dbContext.Drivers.AddRange(drivers);
                //dbContext.Entry(drivers).State = EntityState.Added;
                dbContext.SaveChanges();
                var ddsLocationsQueue = GetDdsLocationsQueue(domainId);
                var deviceHtml = "";
                var idReinit = Guid.NewGuid();

                drivers.ForEach(driver =>
                {
                    deviceHtml += $"<article id='dds-driver-item-{driver.Id}' class='col'>";
                    deviceHtml += $"<label class='label label-success label-lg' style='font-size: 11px !important; position: absolute; top: 10px; left: 8px;'>{driver.Status}</label>";
                    deviceHtml += "<div class='qbicle-opts'>";
                    deviceHtml += $"<a href='javascript:' onclick='ConfirmDeleteDriver({driver.Id})'>";
                    deviceHtml += "<i class='fa fa-trash'></i>";
                    deviceHtml += "</a>";
                    deviceHtml += "</div>";
                    deviceHtml += "<a href='javascript:' style='cursor: initial !important;'>";
                    deviceHtml += "<div class='avatar' style='border-radius: 0; background-image: url(\"/Content/DesignStyle/img/food-delivery.png\");'>&nbsp;</div>";
                    deviceHtml += $"<h1 style='color: #333;'><span id='driver-name-main-{driver.Id}'>{driver.User.User.DisplayUserName}</span></h1>";
                    deviceHtml += "</a>";
                    deviceHtml += "<br />";
                    deviceHtml += "<div class='row' style='text-align: left !important;'>";
                    deviceHtml += "<div class='col-xs-4 col-lg-3'>";
                    deviceHtml += "<label>On shift</label>";
                    deviceHtml += "<div class='checkbox toggle'>";
                    deviceHtml += "<label>";

                    var onShift = "";
                    if (driver.AtWork)
                    {
                        onShift = "checked";
                    }
                    deviceHtml += $"<input onchange='UpdateDriverShift(this.checked, {driver.Id})' class='apps-account {idReinit}' {onShift} data-toggle='toggle' data-size='small' data-onstyle='success' type='checkbox'>";

                    deviceHtml += "</label>";
                    deviceHtml += "</div>";
                    deviceHtml += "</div>";
                    deviceHtml += "<div class='col-xs-8 col-lg-9'>";
                    deviceHtml += " <div class='form-group' style='margin: 0;'>";
                    deviceHtml += "<label for='location'>Current Location</label>";
                    deviceHtml += $"<select id='driver-location-{driver.Id}' class='form-control {idReinit}' style='width: 100%;' onchange='UpdateDriverLocation({driver.Id})'>";
                    deviceHtml += "<option value='0' selected> </option>";
                    foreach (var l in ddsLocationsQueue)
                    {
                        if (l.Id == driver.EmploymentLocation?.Id)
                        {
                            deviceHtml += $"<option value = {l.Id} selected = ''> {l.Name}</option>";
                        }
                        else
                        {
                            deviceHtml += $"<option value = {l.Id}> {l.Name}</option>";
                        }
                    }


                    deviceHtml += "</select>";
                    deviceHtml += "</div>";
                    deviceHtml += "</div>";
                    deviceHtml += "</div>";
                    deviceHtml += "</article>";
                });

                refModel.msgName = idReinit.ToString();
                refModel.msg = deviceHtml;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, userIds, locationId, domainId);
                refModel.result = false;
                refModel.actionVal = -1;
            }

            return refModel;
        }

        public ReturnJsonModel UpdateDriverLocation(Driver driver)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateDriverLocation", null, null, driver);



                var driverUpdate = GetDdsDriverById(driver.Id);
                if (driverUpdate == null) return refModel;
                if (driver.EmploymentLocation.Id == 0)
                    driverUpdate.EmploymentLocation = null;
                else
                    driverUpdate.EmploymentLocation = dbContext.TraderLocations.Find(driver.EmploymentLocation.Id);



                if (dbContext.Entry(driverUpdate).State == EntityState.Detached)
                    dbContext.Drivers.Attach(driverUpdate);
                dbContext.Entry(driverUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, driver);
                refModel.result = false;
                refModel.actionVal = -1;
                refModel.msg = ex.Message;
            }



            return refModel;
        }

        public ReturnJsonModel UpdateDriverWorkLocation(Driver driver, bool isAdd)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update Driver Work Location", null, null, driver);

                var driverUpdate = GetDdsDriverById(driver.Id);
                if (driverUpdate == null) return refModel;
                var locationId = driver.WorkLocations.FirstOrDefault()?.Id ?? 0;
                var location = dbContext.TraderLocations.FirstOrDefault(e => e.Id == locationId);
                if (location == null) return refModel;

                if (isAdd)
                    driverUpdate.WorkLocations.Add(location);
                else
                    driverUpdate.WorkLocations.Remove(location);

                if (dbContext.Entry(driverUpdate).State == EntityState.Detached)
                    dbContext.Drivers.Attach(driverUpdate);
                dbContext.Entry(driverUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, driver);
                refModel.result = false;
                refModel.actionVal = -1;
                refModel.msg = ex.Message;
            }



            return refModel;
        }

        public ReturnJsonModel UpdateDriverShift(Driver driver)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdateDriverShift", null, null, driver);


                var driverUpdate = GetDdsDriverById(driver.Id);
                if (driverUpdate == null) return refModel;
                driverUpdate.AtWork = driver.AtWork;



                if (dbContext.Entry(driverUpdate).State == EntityState.Detached)
                    dbContext.Drivers.Attach(driverUpdate);
                dbContext.Entry(driverUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, driver);
                refModel.result = false;
                refModel.actionVal = -1;
                refModel.msg = ex.Message;
            }



            return refModel;
        }

        public ReturnJsonModel DeleteDdsDriver(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeleteDdsDriver", null, null, id);

                var driver = GetDdsDriverById(id);
                driver.IsDeleted = true;

                dbContext.SaveChanges();
                refModel.result = true;


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public List<PosUserModel> GetPosUserNotDriver(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosUserNotDriver", null, null, domainId);


                var userModels = from pu in dbContext.DeviceUsers
                                 join u in dbContext.Users on pu.User.Id equals u.Id
                                 where !(from o in dbContext.Drivers where o.Domain.Id == domainId select o.User.Id).Contains(pu.Id) && pu.Domain.Id == domainId
                                 select new PosUserModel { Id = pu.Id, Name = pu.User.DisplayUserName, Avatar = pu.User.ProfilePic }
                             ;

                return userModels.ToList();


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId);
                return new List<PosUserModel>();
            }
        }

        public DataTablesResponse SearchDrivers(IDataTablesRequest requestModel, int locationId, string keyword, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, locationId, keyword);
                int totalcount = 0;
                IQueryable<Driver> query = null;
                if (locationId > 0)
                    query = dbContext.Drivers.Where(s => s.EmploymentLocation.Id == locationId);
                else
                {
                    var domain = dbContext.Domains.Find(domainId);
                    var listLocationIDs = domain.TraderLocations.Select(s => s.Id).ToList();
                    query = dbContext.Drivers.Where(s => listLocationIDs.Contains(s.EmploymentLocation.Id));
                }
                #region Filter
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(q => (q.User.User.Forename + " " + q.User.User.Surname).Contains(keyword) || q.User.User.Email.Contains(keyword));
                totalcount = query.Count();
                #endregion
                #region Sorting
                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Driver":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "User.User.DisplayUserName" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "User.User.Forename" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "User.User.Surname" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "User.User.Email" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Email":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "User.User.Email" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "VehicleId":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Vehicle.Name" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                            break;
                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
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
                //Get itemlinks
                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Driver = HelperClass.GetFullNameOfUser(q.User.User),
                    DriverIcon = q.User.User.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T"),
                    q.User.User.Email,
                    VehicleId = q.Vehicle?.Id ?? 0,
                    q.Status
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalcount, totalcount);


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }
        public ReturnJsonModel UpdateStatusDriver(int id, DriverStatus status)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);


                var driver = GetDdsDriverById(id);
                if (driver != null)
                    driver.Status = status;
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
            }
            return refModel;
        }
        public ReturnJsonModel UpdateVehicleDriver(int id, int vehicleId)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, vehicleId);


                var driver = GetDdsDriverById(id);
                if (driver != null)
                    driver.Vehicle = dbContext.B2BVehicles.Find(vehicleId);
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id, vehicleId);
            }
            return refModel;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="posUId"></param>
        /// <param name="locationId"></param>
        /// <param name="domainId"></param>
        /// <param name="cashBank"></param>
        /// <param name="userId"></param>
        /// <param name="driverUserId">User Id will be create POS User if posUId=0</param>
        /// <returns></returns>
        public ReturnJsonModel AddDriver(int posUId, int locationId, int domainId, DriverBankmateAccount cashBank, string userId, string driverUserId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, posUId, locationId, domainId);

                var domain = new DomainRules(dbContext).GetDomainById(domainId);

                var location = dbContext.TraderLocations.Find(locationId);

                //if the Driver existed (ie pos_user existed), then create new driver only
                if (dbContext.Drivers.Any(s => s.User.Id == posUId))
                {
                    var posUser = new DeviceUserRules(dbContext).GetById(posUId);
                    var driver = new Driver
                    {
                        EmploymentLocation = location,//The EmploymentLocation of the new Driver is not set.
                        Domain = domain,
                        Status = DriverStatus.IsAvailable,
                        AtWork = false,
                        User = new DeviceUser
                        {
                            CreatedDate = DateTime.UtcNow,
                            User = posUser.User,
                            CreatedBy = posUser.CreatedBy
                        }
                    };
                    dbContext.Drivers.Add(driver);
                    if (cashBank != null)
                    {
                        cashBank.CreatedBy = dbContext.QbicleUser.Find(userId);
                        cashBank.CreatedDate = DateTime.UtcNow;
                        cashBank.Driver = driver;
                        cashBank.Domain = domain;
                        cashBank.Name = $"{HelperClass.GetFullNameOfUser(driver.User.User)}-MyBankMate";
                        cashBank.ImageUri = driver.User.User.ProfilePic;
                        cashBank.AssociatedBKAccount = new BKCoANodesRule(dbContext).GetAccountById(cashBank.AssociatedBKAccount.Id);
                        dbContext.DriverBankmateAccounts.Add(cashBank);
                    }
                    dbContext.SaveChanges();
                }
                else //QBIC-3857: Update Delivery Mgt to include Settings tab (psUserId =0 )
                {
                    var posUser = dbContext.DeviceUsers.FirstOrDefault(e => e.User.Id == driverUserId);
                    if (posUser == null)
                    {
                        posUser = new Models.Trader.PoS.DeviceUser
                        {
                            Domain = domain,
                            CreatedDate = DateTime.UtcNow,
                            User = dbContext.QbicleUser.FirstOrDefault(e => e.Id == driverUserId),
                            CreatedBy = dbContext.QbicleUser.Find(userId),
                        };

                        dbContext.DeviceUsers.Add(posUser);
                        dbContext.Entry(posUser).State = EntityState.Added;
                        dbContext.SaveChanges();
                    }


                    var driver = new Driver
                    {
                        EmploymentLocation = location,//The EmploymentLocation of the new Driver is not set.
                        Domain = domain,
                        Status = DriverStatus.IsAvailable,
                        AtWork = false,
                        User = posUser
                    };
                    dbContext.Drivers.Add(driver);
                    dbContext.Entry(driver).State = EntityState.Added;
                    dbContext.SaveChanges();
                    if (cashBank != null)
                    {
                        cashBank.CreatedBy = dbContext.QbicleUser.Find(userId);
                        cashBank.CreatedDate = DateTime.UtcNow;
                        cashBank.Driver = driver;
                        cashBank.Domain = domain;
                        cashBank.Name = $"{HelperClass.GetFullNameOfUser(driver.User.User)}-MyBankMate";
                        cashBank.ImageUri = driver.User.User.ProfilePic;
                        cashBank.AssociatedBKAccount = new BKCoANodesRule(dbContext).GetAccountById(cashBank.AssociatedBKAccount.Id);
                        dbContext.DriverBankmateAccounts.Add(cashBank);
                        dbContext.Entry(cashBank).State = EntityState.Added;
                        dbContext.SaveChanges();
                    }
                }
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, posUId, locationId, domainId);
                refModel.result = false;
            }

            return refModel;
        }
        public ReturnJsonModel SaveDeliverySettings(DeliverySettings settings, string userId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, settings);

                var userRule = new DeviceUserRules(dbContext);
                var deliverySettings = dbContext.DeliverySettings.FirstOrDefault(s => s.Location.Id == settings.Location.Id);
                if (deliverySettings == null)
                {
                    deliverySettings = new DeliverySettings
                    {
                        Location = settings.Location,
                        DeliveryService = dbContext.TraderItems.Find(settings.DeliveryService.Id),
                        CreatedBy = dbContext.QbicleUser.Find(userId),
                        CreatedDate = DateTime.UtcNow
                    };
                    deliverySettings.LastUpdatedBy = deliverySettings.CreatedBy;
                    deliverySettings.LastUpdatedDate = deliverySettings.CreatedDate;
                    dbContext.DeliverySettings.Add(deliverySettings);
                }
                else
                {
                    deliverySettings.LastUpdatedBy = dbContext.QbicleUser.Find(userId);
                    deliverySettings.LastUpdatedDate = DateTime.UtcNow;
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, settings);
                refModel.result = false;
            }

            return refModel;
        }
        public DeliverySettings GetDeliverySettingsByLocationId(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);
                return dbContext.DeliverySettings.FirstOrDefault(s => s.Location.Id == locationId) ?? new DeliverySettings();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return new DeliverySettings();
            }

        }

        public List<PosUserModel> GetPosUserNotDriver(int domainId, string keyword)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPosUserNotDriver", null, null, domainId, keyword);

                if (string.IsNullOrEmpty(keyword))
                    return new List<PosUserModel>();

                var userModels = from pu in dbContext.DeviceUsers
                                 join u in dbContext.QbicleUser on pu.User.Id equals u.Id
                                 join d in dbContext.Drivers on pu.Id equals d.User.Id into dr
                                 from driver in dr.DefaultIfEmpty()
                                 where driver == null && driver.IsDeleted == true
                                 && pu.Domain.Id == domainId
                                 &&
                                 (u.Forename.Contains(keyword) ||
                                 (u.Surname).Contains(keyword) ||
                                 (u.DisplayUserName).Contains(keyword))
                                 select new PosUserModel
                                 {
                                     Id = pu.Id,
                                     Name = u.DisplayUserName ?? u.Forename + "" + u.Surname,
                                     Avatar = pu.User.ProfilePic,
                                     ForenameGroup = u.Surname ?? u.DisplayUserName,
                                     UserId = u.Id,
                                     Email = u.Email,
                                     Phone = u.PhoneNumber,
                                     JobTitle = (u.isShareJobTitle.HasValue ? u.JobTitle : "")
                                 }
             ;

                return userModels.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId, keyword);
                return new List<PosUserModel>();
            }
        }

        public List<PosUserModel> GetDomainUserNotDriver(int domainId, string keyword)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDomainUserNotDriver", null, null, domainId, keyword);

                if (string.IsNullOrEmpty(keyword))
                    return new List<PosUserModel>();

                var userdriversId = dbContext.Drivers.Where(e => e.Domain.Id == domainId && e.IsDeleted == false).Select(u => u.User.User.Id).ToList();

                var users = dbContext.DeviceUsers.Where(e => e.Domain.Id == domainId && e.User.IsEnabled == true).Select(u => u.User);

                users = users.Where(u => !userdriversId.Contains(u.Id));
                users = users.Where(u => (u.Forename != null && u.Forename.Contains(keyword)) ||
                                    (u.Surname != null && u.Surname.Contains(keyword)) ||
                                    (u.DisplayUserName != null && u.DisplayUserName.Contains(keyword)));

                var drivers = users.ToList().Select(e => new PosUserModel
                {
                    Id = 0,
                    Name = e.DisplayUserName ?? e.Forename + " " + e.Surname,
                    Avatar = e.ProfilePic,
                    ForenameGroup = string.IsNullOrEmpty(e.Surname) ? e.DisplayUserName : e.Surname,
                    UserId = e.Id,
                    Email = e.Email,
                    Phone = e.PhoneNumber,
                    JobTitle = (e.isShareJobTitle.HasValue ? e.JobTitle : "")
                }).ToList();


                return drivers;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, domainId, keyword);
                return new List<PosUserModel>();
            }
        }

    }

}
