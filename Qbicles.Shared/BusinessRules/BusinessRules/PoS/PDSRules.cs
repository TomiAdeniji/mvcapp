using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;

namespace Qbicles.BusinessRules.PoS
{
    public class PDSRules
    {
        private ApplicationDbContext dbContext;

        public PDSRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        // -------- PrepQueue -----------

        /// <summary>
        /// Get all Queues from PrepQueue AND DeliveryQueue
        /// </summary>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public List<PrepQueueModel> GetPrepAndDdsQueueByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPrepAndDdsQueueByLocation", null, null, locationId);

                var prepQueues = dbContext.PrepQueues.Where(e => e.Location.Id == locationId).ToList();
                var ddsQueues = dbContext.DeliveryQueues.Where(e => e.Location.Id == locationId).ToList();
                return MappingPrepQueueModels(prepQueues, ddsQueues);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return null;
            }
        }

        public PrepQueue GetPrepQueueByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPrepQueueByLocation", null, null, locationId);

                return dbContext.PrepQueues.Where(e => e.Location.Id == locationId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return new PrepQueue();
            }
        }

        public List<PrepQueueModel> SearchQueue(string name, int locationId, QueueType type)
        {
            var prepQueues = new List<PrepQueue>(); var ddsQueues = new List<DeliveryQueue>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SearchQueue", null, null, name, locationId, type);

                switch (type)
                {
                    case QueueType.All:
                        prepQueues = dbContext.PrepQueues.Where(e => e.Name.ToLower().Contains(name.ToLower()) && e.Location.Id == locationId).ToList();
                        ddsQueues = dbContext.DeliveryQueues.Where(e => e.Name.ToLower().Contains(name.ToLower()) && e.Location.Id == locationId).ToList();
                        break;

                    case QueueType.Order:
                        prepQueues = dbContext.PrepQueues.Where(e => e.Name.ToLower().Contains(name.ToLower()) && e.Location.Id == locationId).ToList();
                        break;

                    case QueueType.Delivery:
                        ddsQueues = dbContext.DeliveryQueues.Where(e => e.Name.ToLower().Contains(name.ToLower()) && e.Location.Id == locationId).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, name, locationId, type);
            }

            return MappingPrepQueueModels(prepQueues, ddsQueues);
        }

        private List<PrepQueueModel> MappingPrepQueueModels(List<PrepQueue> prepQueues, List<DeliveryQueue> ddsQueues)
        {
            var model = new List<PrepQueueModel>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "MappingPrepQueueModels", null, null, prepQueues, ddsQueues);

                prepQueues.ForEach(queue =>
                {
                    var posDevices = dbContext.PosDevices.Where(e => e.PreparationQueue.Id == queue.Id);

                    var pdsDevices = dbContext.PrepDisplayDevices.Where(e => e.Queue.Id == queue.Id);

                    model.Add(new PrepQueueModel
                    {
                        Id = queue.Id,
                        Name = queue.Name,
                        PosDevices = string.Join(", ", posDevices.Select(n => n.Name)),
                        PdsDevices = string.Join(", ", pdsDevices.Select(n => n.Name)),
                        CanDelete = (posDevices.Any()) || (pdsDevices.Any()),
                        QueueType = QueueType.Order
                    });
                });

                ddsQueues.ForEach(queue =>
                {
                    var ddsDevices = dbContext.DdsDevice.Where(e => e.Queue.Id == queue.Id).ToList();

                    model.Add(new PrepQueueModel
                    {
                        Id = queue.Id,
                        Name = queue.Name,
                        DdsDevices = string.Join(", ", ddsDevices.Select(n => n.Name)),
                        OrderQueue = queue.PrepQueue.Name,
                        CanDelete = ddsDevices.Any() || queue.Deliveries.Any(),
                        QueueType = QueueType.Delivery
                    });
                });
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, prepQueues, ddsQueues);
            }

            return model;
        }

        public PrepQueue GetQueueById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetQueueById", null, null, id);
                return dbContext.PrepQueues.Find(id) ?? new PrepQueue();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                return null;
            }
        }

        public ReturnJsonModel CreatePrepQueue(PrepQueue prepQueue, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreatePrepQueue", null, null, prepQueue);

                //valid name
                bool isValid;

                if (prepQueue.Id > 0)
                    isValid = dbContext.PrepQueues.Any(x => x.Id != prepQueue.Id && x.Name == prepQueue.Name && x.Location.Id == prepQueue.Location.Id);
                else
                    isValid = dbContext.PrepQueues.Any(x => x.Name == prepQueue.Name && x.Location.Id == prepQueue.Location.Id);

                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_DATA_EXISTED", prepQueue.Name);
                    return refModel;
                }

                prepQueue.CreatedDate = DateTime.UtcNow;
                prepQueue.CreatedBy = dbContext.QbicleUser.Find(userId);

                var prepQueueArchive = new PrepQueueArchive
                {
                    CreatedBy = prepQueue.CreatedBy,
                    Name = $"{prepQueue.Name} Archive",
                    CreatedDate = DateTime.UtcNow,
                    Location = prepQueue.Location,
                    ParentPrepQueue = prepQueue
                };

                dbContext.PrepQueueArchives.Add(prepQueueArchive);
                dbContext.Entry(prepQueueArchive).State = EntityState.Added;

                dbContext.PrepQueues.Add(prepQueue);
                dbContext.Entry(prepQueue).State = EntityState.Added;

                dbContext.SaveChanges();

                var deviceHtml = new StringBuilder();
                deviceHtml.Append($"<article id='prep-queue-item-{prepQueue.Id}' class='col'>");
                deviceHtml.Append($"<div class='qbicle-opts'>");
                deviceHtml.Append($"<div class='dropdown'>");
                deviceHtml.Append($"<a href='javascript:void(0);' class='dropdown-toggle' data-toggle='dropdown'>");
                deviceHtml.Append($"<i class='fa fa-cog'></i>");
                deviceHtml.Append($"</a>");
                deviceHtml.Append($"<ul class='dropdown-menu dropdown-menu-right primary'>");
                deviceHtml.Append($"<li>");
                deviceHtml.Append($"<a href='javascript:' onclick='QueueAddEdit({prepQueue.Id},1)'>Edit</a>");
                deviceHtml.Append($"</li>");
                deviceHtml.Append($"<li>");
                deviceHtml.Append($"<a href='javascript:void(0)' onclick=\"ConfirmDeletePrepQueue({prepQueue.Id},1)\">Delete</a>");
                deviceHtml.Append($"</li>");
                deviceHtml.Append($"</ul>");
                deviceHtml.Append($"</div>");

                deviceHtml.Append($"</div>");

                deviceHtml.Append($"<a href='javascript:void(0);' style='cursor: initial !important;'>");
                deviceHtml.Append($"<div class='avatar' style='border-radius: 0; background-image: url(\"/Content/DesignStyle/img/order.png\");'>&nbsp;</div>");
                deviceHtml.Append($"<h1 style='color: #333;'><span id='prep-queue-name-main-{prepQueue.Id}'>{prepQueue.Name}</span></h1>");
                deviceHtml.Append($"</a>");
                deviceHtml.Append($"<p class='qbicle-detail'>");
                deviceHtml.Append($"<label style='color: #333;'>POS Devices</label>");
                deviceHtml.Append($"<br /><br />");
                deviceHtml.Append($"<label style='color: #333;'>PDS Devices</label>");
                deviceHtml.Append($"</p>");
                deviceHtml.Append($"</article>");

                refModel.msg = deviceHtml.ToString();
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, prepQueue);
            }

            return refModel;
        }

        public ReturnJsonModel CreatePrepDeliveryQueue(int locationId, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreatePrepDeliveryQueue", null, null, locationId);

                var location = dbContext.TraderLocations.Find(locationId);
                if (location == null)
                {
                    refModel.result = false;
                    refModel.actionVal = 3;
                    refModel.msg = "Failed because the Location does not existed!";
                    return refModel;
                }
                var pre = dbContext.PrepQueues.FirstOrDefault(q => q.Location.Id == locationId);
                var preA = dbContext.PrepQueueArchives.FirstOrDefault(q => q.Location.Id == locationId);
                var delivery = dbContext.DeliveryQueues.FirstOrDefault(q => q.Location.Id == locationId);
                var deliveryA = dbContext.DeliveryQueueArchives.FirstOrDefault(q => q.Location.Id == locationId);

                var user = dbContext.QbicleUser.Find(userId);

                if (pre != null && pre.Id > 0)
                {
                    pre.Name = location.Name + " Preparation Queue";
                }
                else
                {
                    pre = new PrepQueue();
                    pre.Name = location.Name + " Preparation Queue";
                    pre.Location = location;
                    pre.CreatedBy = user;
                    pre.CreatedDate = DateTime.UtcNow;
                }

                if (preA != null && preA.Id > 0)
                {
                    preA.Name = location.Name + " Preparation Queue Archive";
                    preA.ParentPrepQueue = pre;
                    dbContext.Entry(preA).State = EntityState.Modified;
                }
                else
                {
                    preA = new PrepQueueArchive()
                    {
                        Name = location.Name + " Preparation Queue Archive",
                        Location = location,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        ParentPrepQueue = pre,
                    };
                    dbContext.PrepQueueArchives.Add(preA);
                    dbContext.Entry(preA).State = EntityState.Added;
                    dbContext.SaveChanges();
                }

                if (delivery != null && delivery.Id > 0)
                {
                    delivery.Name = location.Name + " Delivery Queue";
                    delivery.PrepQueue = pre;
                }
                else
                {
                    delivery = new DeliveryQueue();
                    delivery.Name = location.Name + " Delivery Queue";
                    delivery.Location = location;
                    delivery.CreatedBy = user;
                    delivery.CreatedDate = DateTime.UtcNow;
                    delivery.PrepQueue = pre;
                }
                if (deliveryA != null && deliveryA.Id > 0)
                {
                    deliveryA.Name = location.Name + " Delivery Queue Archive";
                    deliveryA.ParentDeliveryQueue = delivery;
                    dbContext.Entry(deliveryA).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                else
                {
                    deliveryA = new DeliveryQueueArchive()
                    {
                        Name = location.Name + " Delivery Queue Archive",
                        Location = location,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        ParentDeliveryQueue = delivery,
                        PrepQueue = pre
                    };
                    dbContext.DeliveryQueueArchives.Add(deliveryA);
                    dbContext.Entry(deliveryA).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);

                refModel.result = false;
                refModel.msg = ex.Message;
                refModel.actionVal = 3;
            }

            return refModel;
        }

        public ReturnJsonModel UpdatePrepQueue(PrepQueue prepQueue)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePrepQueue", null, null, prepQueue);

                bool isValid;

                if (prepQueue.Id > 0)
                    isValid = dbContext.PrepQueues.Any(x => x.Id != prepQueue.Id && x.Name == prepQueue.Name && x.Location.Id == prepQueue.Location.Id);
                else
                    isValid = dbContext.PrepQueues.Any(x => x.Name == prepQueue.Name && x.Location.Id == prepQueue.Location.Id);

                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_DATA_EXISTED", prepQueue.Name);
                    return refModel;
                }
                var queueUpdate = dbContext.PrepQueues.Find(prepQueue.Id);
                if (queueUpdate == null) return refModel;
                queueUpdate.Name = prepQueue.Name;

                var prepQueueArchiveUpdate =
                    dbContext.PrepQueueArchives.FirstOrDefault(e => e.ParentPrepQueue.Id == prepQueue.Id);
                if (prepQueueArchiveUpdate == null)
                {
                    var prepQueueArchive = new PrepQueueArchive
                    {
                        CreatedBy = queueUpdate.CreatedBy,
                        Name = $"{queueUpdate.Name} Archive",
                        CreatedDate = DateTime.UtcNow,
                        Location = queueUpdate.Location,
                        ParentPrepQueue = queueUpdate
                    };

                    dbContext.PrepQueueArchives.Add(prepQueueArchive);
                    dbContext.Entry(prepQueueArchive).State = EntityState.Added;
                }
                else
                {
                    prepQueueArchiveUpdate.Name = prepQueue.Name;
                    if (dbContext.Entry(prepQueueArchiveUpdate).State == EntityState.Detached)
                        dbContext.PrepQueueArchives.Attach(prepQueueArchiveUpdate);
                    dbContext.Entry(prepQueueArchiveUpdate).State = EntityState.Modified;
                }

                if (dbContext.Entry(queueUpdate).State == EntityState.Detached)
                    dbContext.PrepQueues.Attach(queueUpdate);
                dbContext.Entry(queueUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, prepQueue);
            }

            return refModel;
        }

        public ReturnJsonModel DeletePrepQueue(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPrepQueueByLocationId", null, null, id);

                var queue = GetQueueById(id);
                if (queue != null)
                {
                    queue.AssociatedPosDevices.Clear();
                    queue.QueueOrders.Clear();
                    dbContext.SaveChanges();
                    var prepQueueArchive =
                        dbContext.PrepQueueArchives.FirstOrDefault(e => e.ParentPrepQueue.Id == queue.Id);
                    if (prepQueueArchive != null)
                        dbContext.PrepQueueArchives.Remove(prepQueueArchive);
                    dbContext.PrepQueues.Remove(queue);
                    dbContext.SaveChanges();
                    refModel.result = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                refModel.result = false;
                refModel.actionVal = 9;
                refModel.msg = ex.Message;
            }

            return refModel;
        }

        // -------- PrepDisplayDevice -----------

        public List<PrepDisplayDevice> GetPrepDisplayDeviceByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPrepDisplayDeviceByLocation", null, null, locationId);
                return dbContext.PrepDisplayDevices.Where(e => e.Location.Id == locationId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return null;
            }
        }

        public List<PrepDisplayDevice> SearchPrepDisplayDevice(string name, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "SearchPrepDisplayDevice", null, null, name, locationId);

                return dbContext.PrepDisplayDevices.Where(e => e.Name.ToLower().Contains(name.ToLower()) && e.Location.Id == locationId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, name, locationId);
                return new List<PrepDisplayDevice>();
            }
        }

        public PrepDisplayDevice GetPrepDisplayDeviceId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPrepDisplayDeviceId", null, null, id);

                return dbContext.PrepDisplayDevices.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                return new PrepDisplayDevice();
            }
        }

        public PrepDisplayDevice GetPrepDisplayDeviceBySerialNumber(string serialNumber)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetPrepDisplayDeviceBySerialNumber", null, null, serialNumber);

                return dbContext.PrepDisplayDevices.FirstOrDefault(e => e.SerialNumber == serialNumber);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, serialNumber);
                return null;
            }
        }

        public ReturnJsonModel CreatePrepDisplayDevice(PrepDisplayDevice prepDisplayDevice, List<ApplicationUser> users)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreatePrepDisplayDevice", null, null, prepDisplayDevice, users);

                //valid name
                bool isValid;

                if (prepDisplayDevice.Id > 0)
                    isValid = dbContext.PrepDisplayDevices.Any(x => x.Id != prepDisplayDevice.Id && x.Name == prepDisplayDevice.Name && x.Location.Id == prepDisplayDevice.Location.Id);
                else
                    isValid = dbContext.PrepDisplayDevices.Any(x => x.Name == prepDisplayDevice.Name && x.Location.Id == prepDisplayDevice.Location.Id);

                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_PDS_DEVICE_EXISTED");
                    return refModel;
                }

                prepDisplayDevice.CreatedDate = DateTime.UtcNow;
                prepDisplayDevice.Type = new PDSRules(dbContext).GetOdsDeviceTypeId(prepDisplayDevice.Type.Id);

                prepDisplayDevice.Queue = dbContext.PrepQueues.Find(prepDisplayDevice.Queue.Id);
                var admins = new List<ApplicationUser>();
                prepDisplayDevice.Administrators.ForEach(admin =>
                {
                    admins.Add(users.FirstOrDefault(e => e.Id == admin.Id));
                });
                prepDisplayDevice.Administrators = admins;

                var posUserRule = new DeviceUserRules(dbContext);
                var pUsers = new List<DeviceUser>();
                prepDisplayDevice.Users.ForEach(u =>
                {
                    //TODO: Jira story must be open to take  Supervisor,Cashier,Manager
                    var posUser = posUserRule.GetById(u.Id);
                    pUsers.Add(new DeviceUser
                    {
                        CreatedDate = DateTime.UtcNow,
                        User = posUser.User,
                        Domain = prepDisplayDevice.Location.Domain,
                        CreatedBy = posUser.CreatedBy
                    });
                });
                prepDisplayDevice.Users = pUsers;

                if (!prepDisplayDevice.CategoryExclusionSets.IsNullOrEmpty())
                {
                    var listIdCategoryExclution = prepDisplayDevice.CategoryExclusionSets.Select(e => e.Id).ToList();
                    var listCategoryExclutions = ListCategoryExclutionSets(prepDisplayDevice.Location.Id, "0").Where(e => listIdCategoryExclution.Contains(e.Id)).ToList();
                    prepDisplayDevice.CategoryExclusionSets.Clear();
                    prepDisplayDevice.CategoryExclusionSets.AddRange(listCategoryExclutions);
                }

                dbContext.PrepDisplayDevices.Add(prepDisplayDevice);
                dbContext.Entry(prepDisplayDevice).State = EntityState.Added;
                dbContext.SaveChanges();

                var deviceHtml = new StringBuilder();
                deviceHtml.Append($"<article id='pds-item-{prepDisplayDevice.Id}' class='col'>");
                deviceHtml.Append($" <span class='last-updated'><span id='pds-queue-name-main-{prepDisplayDevice.Id}'>{prepDisplayDevice.Queue.Name}</span></span>");
                deviceHtml.Append($"<span style='top: 40px !important;' class='last-updated'><span id='pds-type-main-{prepDisplayDevice.Id}'>{prepDisplayDevice.Type?.Name ?? "Device type is empty"}</span></span>");
                deviceHtml.Append($"<div class='qbicle-opts'>");
                deviceHtml.Append($"<div class='dropdown'>");
                deviceHtml.Append($"<a href='javascript:' class='dropdown-toggle' data-toggle='dropdown'>");
                deviceHtml.Append($"<i class='fa fa-cog'></i>");
                deviceHtml.Append($"</a>");
                deviceHtml.Append($"<ul class='dropdown-menu dropdown-menu-right primary'>");
                deviceHtml.Append($"<li>");
                deviceHtml.Append($"<a href='javascript:' onclick='PrepDisplayDeviceAddEdit({prepDisplayDevice.Id})'>Edit</a>");
                deviceHtml.Append($"</li>");
                deviceHtml.Append($"<li>");
                deviceHtml.Append($"<a href='javascript:' onclick='ConfirmDeletePrepDisplayDevice({prepDisplayDevice.Id})'>Delete</a>");
                deviceHtml.Append($"</li>");
                deviceHtml.Append($"</ul>");
                deviceHtml.Append($"</div>");
                deviceHtml.Append($"</div>");
                deviceHtml.Append($"<a href='javascript:' style='cursor: initial !important;'>");
                deviceHtml.Append($"<div class='avatar' style='border-radius: 0; background-image: url(\"/Content/DesignStyle/img/icon_order.png\");'>&nbsp;</div>");
                deviceHtml.Append($"<h1 style='color: #333;'><span id='pds-name-main-{prepDisplayDevice.Id}'>{prepDisplayDevice.Name}</span></h1>");
                deviceHtml.Append($"</a>");
                deviceHtml.Append($" <br />");
                deviceHtml.Append($"<p class='qbicle-detail'>");
                deviceHtml.Append($"<label style='color: #333;'>Administrators</label><br />");
                deviceHtml.Append($"<span id='pds-admins-name-main-{prepDisplayDevice.Id}'>{string.Join(", ", prepDisplayDevice.Administrators.Select(e => e.DisplayUserName))}</span>");
                deviceHtml.Append($"</p>");
                deviceHtml.Append($"</article>");

                refModel.msg = deviceHtml.ToString();
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, prepDisplayDevice, users);
            }

            return refModel;
        }

        public ReturnJsonModel UpdatePrepDisplayDevice(PrepDisplayDevice prepDisplayDevice, List<ApplicationUser> users)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePrepDisplayDevice", null, null, prepDisplayDevice, users);

                bool isValid;

                if (prepDisplayDevice.Id > 0)
                    isValid = dbContext.PrepDisplayDevices.Any(x => x.Id != prepDisplayDevice.Id && x.Name == prepDisplayDevice.Name && x.Location.Id == prepDisplayDevice.Location.Id);
                else
                    isValid = dbContext.PrepDisplayDevices.Any(x => x.Name == prepDisplayDevice.Name && x.Location.Id == prepDisplayDevice.Location.Id);

                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_PDS_DEVICE_EXISTED");
                    return refModel;
                }
                var prepDisplayDeviceUpdate = dbContext.PrepDisplayDevices.Find(prepDisplayDevice.Id);
                if (prepDisplayDeviceUpdate == null) return refModel;
                prepDisplayDeviceUpdate.Name = prepDisplayDevice.Name;

                prepDisplayDeviceUpdate.Queue = dbContext.PrepQueues.Find(prepDisplayDevice.Queue.Id);
                prepDisplayDeviceUpdate.Administrators.Clear();
                prepDisplayDeviceUpdate.Users.Clear();

                prepDisplayDeviceUpdate.Type = new PDSRules(dbContext).GetOdsDeviceTypeId(prepDisplayDevice.Type.Id);

                var admins = new List<ApplicationUser>();
                prepDisplayDevice.Administrators.ForEach(admin =>
                {
                    admins.Add(users.FirstOrDefault(e => e.Id == admin.Id));
                });
                prepDisplayDeviceUpdate.Administrators = admins;

                var posUserRule = new DeviceUserRules(dbContext);

                var pUsers = new List<DeviceUser>();
                prepDisplayDevice.Users.ForEach(u =>
                {
                    //TODO: Jira story must be open to take  Supervisor,Cashier,Manager
                    var posUser = posUserRule.GetById(u.Id);
                    pUsers.Add(new DeviceUser
                    {
                        CreatedDate = DateTime.UtcNow,
                        User = posUser.User,
                        Domain = prepDisplayDevice.Location.Domain,
                        CreatedBy = posUser.CreatedBy
                    });
                });
                prepDisplayDeviceUpdate.Users = pUsers;

                var listIdCategoryExclution = prepDisplayDevice.CategoryExclusionSets.Select(e => e.Id).ToList();
                var listCategoryExclutions = ListCategoryExclutionSets(prepDisplayDeviceUpdate.Location.Id, "0").Where(e => listIdCategoryExclution.Contains(e.Id)).ToList();
                prepDisplayDeviceUpdate.CategoryExclusionSets.Clear();
                prepDisplayDeviceUpdate.CategoryExclusionSets.AddRange(listCategoryExclutions);

                if (dbContext.Entry(prepDisplayDeviceUpdate).State == EntityState.Detached)
                    dbContext.PrepDisplayDevices.Attach(prepDisplayDeviceUpdate);
                dbContext.Entry(prepDisplayDeviceUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();

                refModel.Object = new
                {
                    prepDisplayDeviceUpdate.Name,
                    Queue = prepDisplayDeviceUpdate.Queue?.Name ?? "",
                    Admins = string.Join(",", prepDisplayDeviceUpdate.Administrators.Select(e => e.DisplayUserName))
                };
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, prepDisplayDevice, users);
            }

            return refModel;
        }

        public ReturnJsonModel DeletePrepDisplayDevice(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeletePrepDisplayDevice", null, null, id);

                if (dbContext.PosDevices.Any(e => e.PreparationQueue.Id == id) || dbContext.PrepDisplayDevices.Any(x => x.Queue.Id == id))
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_645");
                    return refModel;
                }

                var prepDisplayDevice = GetPrepDisplayDeviceId(id);
                prepDisplayDevice.Administrators.Clear();
                prepDisplayDevice.CategoryExclusionSets.Clear();
                dbContext.PrepDisplayDevices.Remove(prepDisplayDevice);
                dbContext.SaveChanges();
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
            }

            return refModel;
        }

        // -------- OdsDeviceType -----------

        public List<OdsDeviceType> GetOdsDeviceTypeByLocation(int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);
                return dbContext.OdsDeviceTypes.Where(e => e.Location.Id == locationId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return null;
            }
        }

        public List<OdsDeviceType> SearchOdsDeviceTypeByName(string name, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, name, locationId);

                if (string.IsNullOrEmpty(name))
                    return dbContext.OdsDeviceTypes.Where(e => e.Location.Id == locationId).ToList();

                var deviceTypes = dbContext.OdsDeviceTypes.Where(e => e.Name.ToLower().Contains(name.ToLower()) && e.Location.Id == locationId).ToList();

                return deviceTypes;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, name, locationId);
                return new List<OdsDeviceType>();
            }
        }

        public List<OdsDeviceType> SearchOdsDeviceTypeByName(string name, int orderTypeId, int locationId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, name, locationId);

                var deviceTypes = dbContext.OdsDeviceTypes.Where(e => e.Location.Id == locationId);
                if (!string.IsNullOrEmpty(name))
                    deviceTypes = deviceTypes.Where(e => e.Name.ToLower().Contains(name.ToLower()));

                deviceTypes = deviceTypes.Where(e => e.AssociatedOrderTypes.Any(t => t.Id == orderTypeId));

                return deviceTypes.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, name, locationId);
                return new List<OdsDeviceType>();
            }
        }

        public OdsDeviceType GetOdsDeviceTypeId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);

                return dbContext.OdsDeviceTypes.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, id);
                return new OdsDeviceType();
            }
        }

        public ReturnJsonModel CreateOdsDeviceType(OdsDeviceType odsDeviceType, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, odsDeviceType);

                //valid name
                bool isValid;

                if (odsDeviceType.Id > 0)
                    isValid = dbContext.OdsDeviceTypes.Any(x => x.Id != odsDeviceType.Id && x.Name == odsDeviceType.Name && x.Location.Id == odsDeviceType.Location.Id);
                else
                    isValid = dbContext.OdsDeviceTypes.Any(x => x.Name == odsDeviceType.Name && x.Location.Id == odsDeviceType.Location.Id);

                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_DATA_EXISTED", $"Name of {odsDeviceType.Name}");
                    return refModel;
                }

                var deviceType = new OdsDeviceType
                {
                    Name = odsDeviceType.Name,
                    Location = odsDeviceType.Location,
                    CreatedBy = dbContext.QbicleUser.Find(userId),
                    CreatedDate = DateTime.UtcNow
                };
                odsDeviceType.OrderStatus.ForEach(s =>
                {
                    var status = new DeviceTypeStatus
                    {
                        Status = s.Status
                    };
                    deviceType.OrderStatus.Add(status);
                });
                odsDeviceType.AssociatedOrderTypes.ForEach(s =>
                {
                    var type = dbContext.PosOrderTypes.Find(s.Id);
                    deviceType.AssociatedOrderTypes.Add(type);
                });

                dbContext.OdsDeviceTypes.Add(deviceType);
                dbContext.Entry(deviceType).State = EntityState.Added;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, null, odsDeviceType);
            }

            return refModel;
        }

        public ReturnJsonModel UpdateOdsDeviceType(OdsDeviceType odsDeviceType, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, odsDeviceType);

                bool isValid;

                if (odsDeviceType.Id > 0)
                    isValid = dbContext.OdsDeviceTypes.Any(x => x.Id != odsDeviceType.Id && x.Name == odsDeviceType.Name && x.Location.Id == odsDeviceType.Location.Id);
                else
                    isValid = dbContext.OdsDeviceTypes.Any(x => x.Name == odsDeviceType.Name && x.Location.Id == odsDeviceType.Location.Id);

                if (isValid)
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_DATA_EXISTED", $"Name of {odsDeviceType.Name}");
                    return refModel;
                }
                var odsDeviceTypeUpdate = dbContext.OdsDeviceTypes.Find(odsDeviceType.Id);
                if (odsDeviceTypeUpdate == null) return refModel;
                odsDeviceTypeUpdate.Name = odsDeviceType.Name;
                odsDeviceTypeUpdate.AssociatedOrderTypes.Clear();
                odsDeviceTypeUpdate.OrderStatus.Clear();

                odsDeviceType.OrderStatus.ForEach(s =>
                {
                    var status = new DeviceTypeStatus
                    {
                        Status = s.Status
                    };
                    odsDeviceTypeUpdate.OrderStatus.Add(status);
                });
                odsDeviceType.AssociatedOrderTypes.ForEach(s =>
                {
                    var type = dbContext.PosOrderTypes.Find(s.Id);
                    odsDeviceTypeUpdate.AssociatedOrderTypes.Add(type);
                });

                if (dbContext.Entry(odsDeviceTypeUpdate).State == EntityState.Detached)
                    dbContext.OdsDeviceTypes.Attach(odsDeviceTypeUpdate);
                dbContext.Entry(odsDeviceTypeUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, odsDeviceType);
            }

            return refModel;
        }

        public ReturnJsonModel DeleteOdsDeviceType(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);

                var odsDeviceType = GetOdsDeviceTypeId(id);

                if (odsDeviceType.DdsDevices.Any() || odsDeviceType.PdsDevices.Any())
                {
                    refModel.actionVal = 9;
                    refModel.result = false;
                    refModel.msg = ResourcesManager._L("ERROR_DATA_IN_USED_CAN_NOT_DELETE", $"Device type {odsDeviceType}");
                    return refModel;
                }

                odsDeviceType.OrderStatus.Clear();
                odsDeviceType.AssociatedOrderTypes.Clear();
                dbContext.OdsDeviceTypes.Remove(odsDeviceType);
                dbContext.SaveChanges();
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }

            return refModel;
        }

        public PrepQueue GetOrCreatePrepQueue(int locationId, ApplicationUser user)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetOrCreatePrepQueue", null, null, locationId);

                var location = dbContext.TraderLocations.Find(locationId);
                var prepQueue = dbContext.PrepQueues.FirstOrDefault(q => q.Location.Id == locationId);
                var prepQueueArchive = dbContext.PrepQueueArchives.FirstOrDefault(q => q.Location.Id == locationId);
                var deliveryQueue = dbContext.DeliveryQueues.FirstOrDefault(q => q.Location.Id == locationId);
                var deliveryArchive = dbContext.DeliveryQueueArchives.FirstOrDefault(q => q.Location.Id == locationId);

                if (prepQueue == null)
                {
                    prepQueue = new PrepQueue
                    {
                        Name = location.Name + " Preparation Queue",
                        Location = location,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow
                    };

                    dbContext.PrepQueues.Add(prepQueue);
                    dbContext.Entry(prepQueue).State = EntityState.Added;
                    dbContext.SaveChanges();
                }

                if (prepQueueArchive == null)
                {
                    prepQueueArchive = new PrepQueueArchive()
                    {
                        Name = location.Name + " Preparation Queue Archive",
                        Location = location,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        ParentPrepQueue = prepQueue,
                    };
                    dbContext.PrepQueueArchives.Add(prepQueueArchive);
                    dbContext.Entry(prepQueueArchive).State = EntityState.Added;
                    dbContext.SaveChanges();
                }

                if (deliveryQueue == null)
                {
                    deliveryQueue = new DeliveryQueue
                    {
                        Name = location.Name + " Delivery Queue",
                        Location = location,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        PrepQueue = prepQueue
                    };
                    dbContext.DeliveryQueues.Add(deliveryQueue);
                    dbContext.Entry(deliveryQueue).State = EntityState.Added;
                    dbContext.SaveChanges();
                }

                if (deliveryArchive == null)
                {
                    deliveryArchive = new DeliveryQueueArchive()
                    {
                        Name = location.Name + " Delivery Queue Archive",
                        Location = location,
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        ParentDeliveryQueue = deliveryQueue,
                        PrepQueue = prepQueue
                    };
                    dbContext.DeliveryQueueArchives.Add(deliveryArchive);
                    dbContext.Entry(deliveryArchive).State = EntityState.Added;
                    dbContext.SaveChanges();
                }

                return prepQueue;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                return null;
            }
        }

        //-------------Category Exclusion-----------------
        public List<SimpleCategoryExclution> ListCategoriesByTraderLocation(int locationId, int categoryExclutionSetId = 0)
        {
            try
            {
                var categories = dbContext.PosMenus.Where(e => e.Location.Id == locationId && e.IsDeleted == false && e.SalesChannel == Models.Trader.SalesChannel.SalesChannelEnum.POS).SelectMany(e => e.Categories).GroupBy(e => e.Name).Select(e => e.FirstOrDefault());

                var result = categories.Select(e => new SimpleCategoryExclution
                {
                    Name = e.Name,
                    Id = e.Id,
                    IsSelected = false
                });

                if (categoryExclutionSetId != 0)
                {
                    var listCategoriesExclution = dbContext.CategoryExclusionSets.Where(e => e.Id == categoryExclutionSetId).SelectMany(e => e.CategoryNames);
                    var listNameCategoriesExclution = listCategoriesExclution.Select(e => e.CategoryName).ToList();

                    result = from category in categories
                             join categoryExclution in listCategoriesExclution on category.Name equals categoryExclution.CategoryName
                             into temporary
                             from cate in temporary.DefaultIfEmpty()
                             select new SimpleCategoryExclution
                             {
                                 Name = category.Name,
                                 Id = category.Id,
                                 IsSelected = !(cate.CategoryName.Equals("") || cate.CategoryName == null),
                             };
                }
                return result.OrderBy(e => e.Name).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId);
                return null;
            }
        }

        public List<CategoryExclusionSet> ListCategoryExclutionSets(int locationId, string currentUserId, int CategoryExclutionsSetId = 0)
        {
            try
            {
                var categoryExclusionSets = dbContext.CategoryExclusionSets.Where(e => e.Location.Id == locationId).OrderByDescending(e => e.CreatedDate);
                var result = categoryExclusionSets.ToList();
                if (CategoryExclutionsSetId != 0) result = categoryExclusionSets.Where(e => e.Id == CategoryExclutionsSetId).ToList();
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, currentUserId);
                return null;
            }
        }

        public ReturnJsonModel CreateNewCategoryExclustionSet(List<string> listCategoriesName, string name, string currentUserId, int traderLocationId, List<int> listPrepDevices)
        {
            var result = new ReturnJsonModel();
            result.msg = "Error";
            result.result = false;
            try
            {
                var location = dbContext.TraderLocations.FirstOrDefault(p => p.Id == traderLocationId);
                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var categories = dbContext.PosMenus
                    .Where(e => e.Location.Id == traderLocationId && e.IsDeleted == false && e.SalesChannel == Models.Trader.SalesChannel.SalesChannelEnum.POS)
                    .SelectMany(e => e.Categories)
                    .GroupBy(e => e.Name)
                    .Select(e => e.FirstOrDefault());
                var listCategoriesExclutions = categories.Where(e => listCategoriesName.Contains(e.Name));
                List<ExclusionCategoryName> exclusionCategoryList = new List<ExclusionCategoryName>();

                if (listCategoriesExclutions.Count() != listCategoriesName.Count())
                {
                    //list pushed from frontend has elements which are not in original list
                    result.msg = "Categories do not match the original category list.Please refresh your browser";
                    result.result = false;
                    return result;
                }

                //validate name
                if (IsNameExisted(traderLocationId, name))
                {
                    //name exist
                    result.msg = "The name is already exists. Please try another name";
                    result.result = false;
                    return result;
                }

                foreach (var category in listCategoriesExclutions)
                {
                    var exclusionCategoryName = new ExclusionCategoryName
                    {
                        CategoryName = category.Name
                    };
                    dbContext.Entry(exclusionCategoryName).State = EntityState.Added;
                    exclusionCategoryList.Add(exclusionCategoryName);
                }
                var prepDevices = GetPrepDisplayDeviceByLocation(location.Id).Where(e => listPrepDevices.Contains(e.Id)).ToList();
                var categoryExclusionSet = new CategoryExclusionSet
                {
                    CreatedDate = DateTime.Now,
                    Name = name,
                    Location = location,
                    CreatedBy = currentUser,
                    CategoryNames = exclusionCategoryList,
                    PrepDisplayDevices = prepDevices
                };

                dbContext.ExclusionCategoryNames.AddRange(exclusionCategoryList);
                dbContext.CategoryExclusionSets.Add(categoryExclusionSet);
                dbContext.Entry(categoryExclusionSet).State = EntityState.Added;
                dbContext.SaveChanges();
                result.msg = "Successful";
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, listCategoriesName, name, currentUserId, traderLocationId);
                return result;
            }
        }

        public ReturnJsonModel UpdateCategoryExclusionSet(List<string> listCategoriesName, string name, string currentUserId, int traderLocationId, int categoryExclusionSetId, List<int> listPrepDevices)
        {
            var result = new ReturnJsonModel();
            result.msg = "Error";
            result.result = false;
            try
            {
                var location = dbContext.TraderLocations.FirstOrDefault(p => p.Id == traderLocationId);
                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var categories = dbContext.PosMenus
                    .Where(e => e.Location.Id == traderLocationId && e.IsDeleted == false && e.SalesChannel == Models.Trader.SalesChannel.SalesChannelEnum.POS)
                    .SelectMany(e => e.Categories)
                    .GroupBy(e => e.Name)
                    .Select(e => e.FirstOrDefault());
                var listCategoriesExclutions = categories.Where(e => listCategoriesName.Contains(e.Name));
                List<ExclusionCategoryName> exclusionCategoryList = new List<ExclusionCategoryName>();

                //if (listCategoriesExclutions.Count() != listCategoriesName.Count())
                //{
                //    //list pushed from frontend has elements which are not in original list
                //    result.msg = "Categories are not matching original category list.Please refresh your broswer";
                //    result.result = false;
                //    return result;
                //}

                foreach (var category in listCategoriesExclutions)
                {
                    var exclusionCategoryName = new ExclusionCategoryName
                    {
                        CategoryName = category.Name
                    };
                    exclusionCategoryList.Add(exclusionCategoryName);
                }

                var currentCategoryExclustionSet = dbContext.CategoryExclusionSets.Where(e => e.Id == categoryExclusionSetId).FirstOrDefault();

                if (traderLocationId != currentCategoryExclustionSet.Location.Id)
                {
                    //traderLocation Wrong - cause user changes location in another tab
                    result.msg = "Trader location is not matching trader location in original category list";
                    result.result = false;
                    return result;
                }

                //check name
                if (IsNameExisted(traderLocationId, name, categoryExclusionSetId))
                {
                    result.msg = "The name is invalid. Please try another name";
                    result.result = false;
                    return result;
                }
                if (name != currentCategoryExclustionSet.Name)
                {
                    currentCategoryExclustionSet.Name = name;
                }

                var currentExclusionCategoryList = currentCategoryExclustionSet.CategoryNames;
                var nameCurrentExclusionCategoryList = currentCategoryExclustionSet.CategoryNames.Select(e => e.CategoryName);
                var nameExclusionCategoryList = exclusionCategoryList.Select(e => e.CategoryName);

                #region update

                var removeList = currentExclusionCategoryList.Where(e => !nameExclusionCategoryList.Contains(e.CategoryName)).ToList();
                var addList = exclusionCategoryList.Where(e => !nameCurrentExclusionCategoryList.Contains(e.CategoryName)).ToList();

                currentExclusionCategoryList = currentExclusionCategoryList.Except(removeList).ToList();
                currentExclusionCategoryList.AddRange(addList);

                currentCategoryExclustionSet.CategoryNames = currentExclusionCategoryList;

                #endregion update

                dbContext.ExclusionCategoryNames.RemoveRange(removeList);
                dbContext.ExclusionCategoryNames.AddRange(addList);

                var prepDevices = GetPrepDisplayDeviceByLocation(location.Id).Where(e => listPrepDevices.Contains(e.Id));
                currentCategoryExclustionSet.PrepDisplayDevices.Clear();
                currentCategoryExclustionSet.PrepDisplayDevices.AddRange(prepDevices);

                dbContext.SaveChanges();
                result.msg = "Update successful";

                //check if a category has been deteled but it still in currentExclusionCategoryList
                var nameCategoriesList = categories.Select(e => e.Name);
                var isnotExitsed = removeList.Where(e => !nameCategoriesList.Contains(e.CategoryName));
                if (isnotExitsed.Count() > 0)
                {
                    result.msg = "Update successful! - some categories have been removed cause of not exist";
                };
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, listCategoriesName, name, currentUserId, traderLocationId, categoryExclusionSetId);
                return result;
            }
        }

        public ReturnJsonModel DeleteCategoryExclusionSet(int categoryExclusionSetId, string name)
        {
            var result = new ReturnJsonModel();
            result.msg = "Error";
            result.result = false;
            try
            {
                var currentCategoryExclustionSet = dbContext.CategoryExclusionSets.Where(e => e.Id == categoryExclusionSetId && e.Name == name).FirstOrDefault();
                if (currentCategoryExclustionSet == null)
                {
                    result.msg = "Category Exclusion Set not found";
                    return result;
                }
                var currentExclusionCategoryList = currentCategoryExclustionSet.CategoryNames;
                if (currentExclusionCategoryList.Count > 0)
                {
                    dbContext.ExclusionCategoryNames.RemoveRange(currentExclusionCategoryList);
                }
                dbContext.CategoryExclusionSets.Remove(currentCategoryExclustionSet);
                dbContext.SaveChanges();
                result.msg = "Category Exclusion Set has been deleted";
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, categoryExclusionSetId);
                return result;
            }
        }

        public bool IsNameExisted(int locationId, string name, int categoryExclusionSetId = 0)
        {
            try
            {
                if (name.IsNullOrEmpty()) return true;

                var uniqName = 1;
                if (categoryExclusionSetId == 0)
                {
                    uniqName = dbContext.CategoryExclusionSets.Where(e => e.Name == name && e.Location.Id == locationId).Count();
                }
                else
                {
                    uniqName = dbContext.CategoryExclusionSets.Where(e => e.Name == name && e.Id != categoryExclusionSetId && e.Location.Id == locationId).Count();
                }
                if (uniqName > 0) return true;
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, locationId, name);
                return true;
            }
        }
    }
}