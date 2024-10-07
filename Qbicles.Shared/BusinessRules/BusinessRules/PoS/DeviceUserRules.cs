using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.ExtendedProperties;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Qbicles.BusinessRules.PoS
{
    public class DeviceUserRules
    {
        ApplicationDbContext dbContext;

        public DeviceUserRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public List<PosRoleUsersViewModel> GetAll(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetAll", null, null, domainId);

                var allPosUsers = AllPosUsersByDomain(domainId);
                return allPosUsers;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<PosRoleUsersViewModel>();
            }

        }

        private List<PosRoleUsersViewModel> AllPosUsersByDomain(int domainId)
        {
            var posUsers = dbContext.DeviceUsers.Where(e => e.Domain.Id == domainId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.User,
            }).ToList();
            var posCashiers = dbContext.PosCashiers.Where(e => e.Domain.Id == domainId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.Cashier,
            }).ToList();
            var posSupervisors = dbContext.PosSupervisors.Where(e => e.Domain.Id == domainId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.Supervisor
            }).ToList();
            var posManagers = dbContext.PosTillManagers.Where(e => e.Domain.Id == domainId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.Manager
            }).ToList();

            var allPosUsers = new[] { posUsers, posCashiers, posSupervisors, posManagers }.SelectMany(u => u).GroupBy(g => g.User.Id).Select(s => s.First()).OrderByDescending(o=>o.CreatedDate).ToList();
            return allPosUsers;
        }
        private PosRoleUsersViewModel GetPosUserById(int userId)
        {
            var posUsers = dbContext.DeviceUsers.Where(e => e.Id == userId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.User,
            }).ToList();
            var posCashiers = dbContext.PosCashiers.Where(e => e.Id == userId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.Cashier,
            }).ToList();
            var posSupervisors = dbContext.PosSupervisors.Where(e => e.Id == userId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.Supervisor
            }).ToList();
            var posManagers = dbContext.PosTillManagers.Where(e => e.Id == userId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.Manager
            }).ToList();

            var allPosUsers = new[] { posUsers, posCashiers, posSupervisors, posManagers }.SelectMany(u => u).GroupBy(g => g.Id).Select(s => s.First()).ToList();
            return allPosUsers.FirstOrDefault();
        }
      private PosRoleUsersViewModel GetPosUserByUserId(string userId)
        {
            var posUsers = dbContext.DeviceUsers.Where(e => e.User.Id == userId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.User,
            }).ToList();
            var posCashiers = dbContext.PosCashiers.Where(e => e.User.Id == userId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.Cashier,
            }).ToList();
            var posSupervisors = dbContext.PosSupervisors.Where(e => e.User.Id == userId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.Supervisor
            }).ToList();
            var posManagers = dbContext.PosTillManagers.Where(e => e.User.Id == userId).Select(u => new PosRoleUsersViewModel
            {
                Id = u.Id,
                CreatedBy = u.CreatedBy,
                CreatedDate = u.CreatedDate,
                Domain = u.Domain,
                Pin = u.Pin,
                User = u.User,
                PosUserType = PosUserType.Manager
            }).ToList();

            var allPosUsers = new[] { posUsers, posCashiers, posSupervisors, posManagers }.SelectMany(u => u).GroupBy(g => g.Id).Select(s => s.First()).ToList();
            return allPosUsers.FirstOrDefault();
        }

        /// <summary>
        /// Get DeviceUsers(pos_user) by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PosRoleUsersViewModel GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);
                return GetPosUserById(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public PosRoleUsersViewModel GetByUserId(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, userId);
                var dU= GetPosUserByUserId(userId);
                return dU;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return null;
            }
        }

        public ReturnJsonModel Create(CreatePosUserViewModel posUser, string userId)
        {
            var refModel = new ReturnJsonModel
            {

                result = true
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create DeviceUser", userId, null, posUser);

                
                switch (posUser.UserType)
                {
                    case "":
                    case "User":
                        dbContext.DeviceUsers.Add(new DeviceUser
                        {
                            CreatedDate = DateTime.UtcNow,
                            User = dbContext.QbicleUser.FirstOrDefault(e => e.Id == posUser.User.Id),
                            CreatedBy = dbContext.QbicleUser.Find(userId)
                        });
                        dbContext.Entry(posUser).State = EntityState.Added;
                        dbContext.SaveChanges();
                        break;
                    case "Cashier":
                        dbContext.PosCashiers.Add(new PosCashier
                        {
                            CreatedDate = DateTime.UtcNow,
                            User = dbContext.QbicleUser.FirstOrDefault(e => e.Id == posUser.User.Id),
                            CreatedBy = dbContext.QbicleUser.Find(userId)
                        });
                        dbContext.Entry(posUser).State = EntityState.Added;
                        dbContext.SaveChanges();
                        break;
                    case "Supervisor":
                        dbContext.PosSupervisors.Add(new PosSupervisor
                        {
                            CreatedDate = DateTime.UtcNow,
                            User = dbContext.QbicleUser.FirstOrDefault(e => e.Id == posUser.User.Id),
                            CreatedBy = dbContext.QbicleUser.Find(userId)
                        });
                        dbContext.Entry(posUser).State = EntityState.Added;
                        dbContext.SaveChanges();
                        break;
                    case "Manager":
                        dbContext.PosTillManagers.Add(new PosTillManager
                        {
                            CreatedDate = DateTime.UtcNow,
                            User = dbContext.QbicleUser.FirstOrDefault(e => e.Id == posUser.User.Id),
                            CreatedBy = dbContext.QbicleUser.Find(userId)
                        });
                        dbContext.Entry(posUser).State = EntityState.Added;
                        dbContext.SaveChanges();
                        break;
                }

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, posUser);

            }


            return refModel;
        }

        public ReturnJsonModel Delete(CreatePosUserViewModel posUser)
        {
            var refModel = new ReturnJsonModel
            {
                result = true
            };
            var found = false;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete pos user", null, null, posUser);

                var posUserDel = dbContext.DeviceUsers.FirstOrDefault(e => e.User.Id == posUser.User.Id && e.Domain.Id == posUser.Domain.Id);
                if (posUserDel != null)
                {
                    found = true;
                    dbContext.DeviceUsers.Remove(posUserDel);
                    dbContext.SaveChanges();
                }
                var posCashierDel = dbContext.PosCashiers.FirstOrDefault(e => e.User.Id == posUser.User.Id && e.Domain.Id == posUser.Domain.Id);
                if (posCashierDel != null)
                {
                    found = true;
                    dbContext.PosCashiers.Remove(posCashierDel);
                    dbContext.SaveChanges();
                }
                var posSUpervisorDel = dbContext.PosSupervisors.FirstOrDefault(e => e.User.Id == posUser.User.Id && e.Domain.Id == posUser.Domain.Id);
                if (posSUpervisorDel != null)
                {
                    found = true;
                    dbContext.PosSupervisors.Remove(posSUpervisorDel);
                    dbContext.SaveChanges();
                }
                var posManagerDel = dbContext.PosTillManagers.FirstOrDefault(e => e.User.Id == posUser.User.Id && e.Domain.Id == posUser.Domain.Id);
                if (posManagerDel != null)
                {
                    found = true;
                    dbContext.PosTillManagers.Remove(posManagerDel);
                    dbContext.SaveChanges();
                }

                if (!found)
                {
                    return refModel;
                }


            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posUser);
            }

            return refModel;
        }

        public ReturnJsonModel GeneratePin(CreatePosUserViewModel posUser)
        {
            var refModel = new ReturnJsonModel
            {
                result = true
            };
             var found = false;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GeneratePin", null, null, posUser);


                switch (posUser.UserType)
                {
                    case "":
                    case "User":
                        var posUserUpdate = dbContext.DeviceUsers.FirstOrDefault(e => e.User.Id == posUser.User.Id && e.Domain.Id == posUser.Domain.Id);
                        if (posUserUpdate != null)
                        {
                            found = true;
                            var domainPins = dbContext.DeviceUsers.Where(e => e.Domain.Id == posUser.Domain.Id).Select(p => p.Pin).ToList();
                            var rnd = new Random();
                            var pin = rnd.Next(1000, 9999);
                            var count = 0;
                            while (domainPins.Any(e => e.Equals(pin)))
                            {
                                if (count > 4872)
                                {
                                    refModel.result = false;
                                    refModel.msg = ResourcesManager._L("ERROR_MSG_675");
                                    return refModel;
                                }
                                pin = rnd.Next(1000, 9999); count++;
                            }

                            posUserUpdate.Pin = pin;

                            if (dbContext.Entry(posUserUpdate).State == EntityState.Detached)
                                dbContext.DeviceUsers.Attach(posUserUpdate);
                            dbContext.Entry(posUserUpdate).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            new EmailRules(dbContext).SendEmailPosUserPin(posUserUpdate);
                        }
                        break;
                    case "Cashier":
                        var posCashierUpdate = dbContext.PosCashiers.FirstOrDefault(e => e.User.Id == posUser.User.Id && e.Domain.Id == posUser.Domain.Id);
                        if (posCashierUpdate != null)
                        {
                            found = true;
                            var domainPins = dbContext.PosCashiers.Where(e => e.Domain.Id == posUser.Domain.Id).Select(p => p.Pin).ToList();
                            var rnd = new Random();
                            var pin = rnd.Next(1000, 9999);
                            var count = 0;
                            while (domainPins.Any(e => e.Equals(pin)))
                            {
                                if (count > 4872)
                                {
                                    refModel.result = false;
                                    refModel.msg = ResourcesManager._L("ERROR_MSG_675");
                                    return refModel;
                                }
                                pin = rnd.Next(1000, 9999); count++;
                            }

                            posCashierUpdate.Pin = pin;

                            if (dbContext.Entry(posCashierUpdate).State == EntityState.Detached)
                                dbContext.PosCashiers.Attach(posCashierUpdate);
                            dbContext.Entry(posCashierUpdate).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            var localPosUser = new DeviceUser
                            {
                                Domain = posUser.Domain,
                                User = posUser.User
                            };
                            new EmailRules(dbContext).SendEmailPosUserPin(localPosUser);
                        }
                        break;
                    case "Supervisor":

                        var posSupervisorUpdate = dbContext.PosSupervisors.FirstOrDefault(e => e.User.Id == posUser.User.Id && e.Domain.Id == posUser.Domain.Id);
                        if (posSupervisorUpdate != null)
                        {
                            found = true;
                            var domainPins = dbContext.PosSupervisors.Where(e => e.Domain.Id == posUser.Domain.Id).Select(p => p.Pin).ToList();
                            var rnd = new Random();
                            var pin = rnd.Next(1000, 9999);
                            var count = 0;
                            while (domainPins.Any(e => e.Equals(pin)))
                            {
                                if (count > 4872)
                                {
                                    refModel.result = false;
                                    refModel.msg = ResourcesManager._L("ERROR_MSG_675");
                                    return refModel;
                                }
                                pin = rnd.Next(1000, 9999); count++;
                            }

                            posSupervisorUpdate.Pin = pin;

                            if (dbContext.Entry(posSupervisorUpdate).State == EntityState.Detached)
                                dbContext.PosSupervisors.Attach(posSupervisorUpdate);
                            dbContext.Entry(posSupervisorUpdate).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            var localPosUser = new DeviceUser
                            {
                                Domain = posUser.Domain,
                                User = posUser.User
                            };
                            new EmailRules(dbContext).SendEmailPosUserPin(localPosUser);
                        }
                        break;
                    case "Manager":
                        var posManagerUpdate = dbContext.PosTillManagers.FirstOrDefault(e => e.User.Id == posUser.User.Id && e.Domain.Id == posUser.Domain.Id);
                        if (posManagerUpdate != null)
                        {
                            found = true;
                            var domainPins = dbContext.PosTillManagers.Where(e => e.Domain.Id == posUser.Domain.Id).Select(p => p.Pin).ToList();
                            var rnd = new Random();
                            var pin = rnd.Next(1000, 9999);
                            var count = 0;
                            while (domainPins.Any(e => e.Equals(pin)))
                            {
                                if (count > 4872)
                                {
                                    refModel.result = false;
                                    refModel.msg = ResourcesManager._L("ERROR_MSG_675");
                                    return refModel;
                                }
                                pin = rnd.Next(1000, 9999); count++;
                            }

                            posManagerUpdate.Pin = pin;

                            if (dbContext.Entry(posManagerUpdate).State == EntityState.Detached)
                                dbContext.PosTillManagers.Attach(posManagerUpdate);
                            dbContext.Entry(posManagerUpdate).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            var localPosUser = new DeviceUser
                            {
                                Domain = posUser.Domain,
                                User = posUser.User
                            };
                            new EmailRules(dbContext).SendEmailPosUserPin(localPosUser);
                        }
                        break;
                }

                if (!found)
                {
                    return refModel;
                }
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posUser);

            }

            return refModel;
        }

        public List<PosUserModel> GetDeviceUsersByDomain(int domainId, int deviceId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetDeviceUsersByDomain", null, null, domainId, deviceId);

                var posUsers = new List<PosUserModel>();
                var device = dbContext.PosDevices.FirstOrDefault(e => e.Id == deviceId);
                if (device == null) return new List<PosUserModel>();

                var types1 = new List<PosUserType>();
                var types2 = new List<PosUserType>();
                var types3 = new List<PosUserType>();
                var types4 = new List<PosUserType>();
                var types5 = new List<PosUserType>();
                var users = dbContext.DeviceUsers.Where(e => e.Domain.Id == domainId).Select(u=> new PosUserModel
                 {
                     Id = u.Id,
                     PosUserId = u.Id,
                     UserId = u.User.Id,
                     User = u.User,
                     Avatar = u.User.ProfilePic,
                     Pin = u.Pin,
                     Email = u.User.Email,
                     Phone = u.User.PhoneNumber,
                     Types = types1,
                 }).ToList();

                var cashiers = dbContext.PosCashiers.Where(l => l.Domain.Id == domainId && l.Devices.Any(m => m.Id == deviceId)).Distinct().Select(u => new PosUserModel
                {
                    Id = u.Id,
                    PosUserId = u.Id,
                    UserId = u.User.Id,
                    User = u.User,
                    Avatar = u.User.ProfilePic,
                    Pin = u.Pin,
                    Email = u.User.Email,
                    Phone = u.User.PhoneNumber,
                    Types = types2,
                }).ToList();
                
                var supervisors = dbContext.PosSupervisors.Where(l => l.Domain.Id == domainId && l.Devices.Any(m => m.Id == deviceId)).Distinct().Select(u => new PosUserModel
                {
                    Id = u.Id,
                    PosUserId = u.Id,
                    UserId = u.User.Id,
                    User = u.User,
                    Avatar = u.User.ProfilePic,
                    Pin = u.Pin,
                    Email = u.User.Email,
                    Phone = u.User.PhoneNumber,
                    Types = types3,
                }).ToList();
                 
                var managers = dbContext.PosTillManagers.Where(l => l.Domain.Id == domainId && l.Devices.Any(m => m.Id == deviceId)).Distinct().Select(u => new PosUserModel
                {
                    Id = u.Id,
                    PosUserId = u.Id,
                    UserId = u.User.Id,
                    User = u.User,
                    Avatar = u.User.ProfilePic,
                    Pin = u.Pin,
                    Email = u.User.Email,
                    Phone = u.User.PhoneNumber,
                    Types = types4,
                }).ToList();

               
                //var administrators = dbContext.PosAdministrators.Where(l => l.Domain.Id == domainId && l.Devices.Any(m => m.Id == deviceId)).Distinct().Select(u => new PosUserModel
                //{
                //    Id = u.Id,
                //    PosUserId = u.Id,
                //    UserId = u.User.Id,
                //    User = u.User,
                //    Avatar = u.User.ProfilePic,
                //    Email = u.User.Email,
                //    Phone = u.User.PhoneNumber,
                //    Types = types5,
                //}).ToList();

                var allUsers = new[] { users, cashiers, supervisors, managers }.SelectMany(u => u).ToList();
                foreach (var pU in allUsers)
                {
                    var forenameGroup = "";
                    if (!string.IsNullOrEmpty(pU.User.Forename))
                        forenameGroup = pU.User.Forename.ToCharArray()[0].ToString().ToUpper();
                    else
                        forenameGroup = pU.User.Email.ToCharArray()[0].ToString().ToUpper();


                    pU.ForenameGroup = forenameGroup;
                    pU.Name = GetFullName(pU.User, "");

                    if (device.Users.Any(e => e.User.Id == pU.UserId))
                    {
                        pU.Types.Add(PosUserType.User);
                    }

                    if (device.DeviceCashiers.Any(e => e.User.Id == pU.UserId))
                    {
                        pU.Id = device.DeviceCashiers.FirstOrDefault(e => e.User.Id == pU.UserId)?.Id ?? 0;
                        pU.Types.Add(PosUserType.Cashier);
                    }

                    if (device.DeviceSupervisors.Any(e => e.User.Id == pU.UserId))
                    {
                        pU.Id = device.DeviceSupervisors.FirstOrDefault(e => e.User.Id == pU.UserId)?.Id ?? 0;
                        pU.Types.Add(PosUserType.Supervisor);
                    }

                    if (device.DeviceManagers.Any(e => e.User.Id == pU.UserId))
                    {
                        pU.Id = device.DeviceManagers.FirstOrDefault(e => e.User.Id == pU.UserId)?.Id ?? 0;
                        pU.Types.Add(PosUserType.Manager);
                    }

                    if (device.Administrators.Any(e => e.User.Id == pU.UserId))
                    {
                        pU.Id = device.Administrators.FirstOrDefault(e => e.User.Id == pU.UserId)?.Id ?? 0;
                        pU.Types.Add(PosUserType.Admin);
                    }

                    posUsers.Add(pU);
                }


                return posUsers.GroupBy(u => u.UserId).Select(e => e.First()).OrderBy(e => e.ForenameGroup).ToList();
                //return posUsers.Distinct(new PosUserModelEqualityComparer()).OrderBy(e => e.ForenameGroup).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, deviceId);
                return new List<PosUserModel>();
            }

        }

        public ReturnJsonModel UpdatePosUser(PosUserDeviceModel model)
        {
            var refModel = new ReturnJsonModel
            {
                result = true
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosRoleUsers", null, null, model);

                var device = dbContext.PosDevices.Find(model.DeviceId);
                if (device == null)
                    return refModel;



                if (model.CurrentRole == "none")
                {
                    var admin = device.Administrators.FirstOrDefault(l => l.User.Id == model.UserId);
                    if (admin == null)
                    {
                            DeletePreviousRoleUser(model, device);
                            if (dbContext.Entry(device).State == EntityState.Detached)
                                dbContext.PosDevices.Attach(device);
                            dbContext.Entry(device).State = EntityState.Modified;
                            dbContext.SaveChanges();
                            refModel.msg = RenderPosUser(model);
                            return refModel;
                    }

                    DeletePreviousRoleUser(model, device);
                    if (dbContext.Entry(device).State == EntityState.Detached)
                        dbContext.PosDevices.Attach(device);
                    dbContext.Entry(device).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModel.msg = RenderPosUser(model);
                }

                if (model.CurrentRole == "user")
                {
                    var userNew = dbContext.DeviceUsers.FirstOrDefault(e =>
                    e.User.Id == model.UserId && e.Domain.Id == device.Location.Domain.Id);
                    if (userNew == null)
                        return refModel;

                    var pin = DeletePreviousRoleUser(model, device);
                    userNew.Pin = pin;
                    device.Users.Add(userNew);
                    if (dbContext.Entry(device).State == EntityState.Detached)
                        dbContext.PosDevices.Attach(device);
                    dbContext.Entry(device).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    refModel.msgId = userNew.Id.ToString();
                }

                if (model.CurrentRole == "cashier")
                {
                    var user = new UserRules(dbContext).GetById(model.UserId);

                    var pin = DeletePreviousRoleUser(model, device);

                    var userCashier = new PosCashier
                    {
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = dbContext.QbicleUser.Find(model.UserId),
                        Domain = device?.Location.Domain,
                        User = user,
                        Pin = pin,
                    };

                    device.DeviceCashiers.Add(userCashier);

                    if (dbContext.Entry(device).State == EntityState.Detached)
                        dbContext.PosDevices.Attach(device);
                    dbContext.Entry(device).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModel.msgId = userCashier.Id.ToString();
                }

                if (model.CurrentRole == "supervisor")
                {
                    var user = new UserRules(dbContext).GetById(model.UserId);

                    var pin = DeletePreviousRoleUser(model, device);

                    var userSupervisor = new PosSupervisor
                    {
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = dbContext.QbicleUser.Find(model.UserId),
                        Domain = device?.Location.Domain,
                        User = user,
                        Pin = pin
                    };

                    device.DeviceSupervisors.Add(userSupervisor);

                    if (dbContext.Entry(device).State == EntityState.Detached)
                        dbContext.PosDevices.Attach(device);
                    dbContext.Entry(device).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModel.msgId = userSupervisor.Id.ToString();
                }

                if (model.CurrentRole == "manager")
                {
                    var user = new UserRules(dbContext).GetById(model.UserId);

                    var pin = DeletePreviousRoleUser(model, device);

                    var userManager = new PosTillManager
                    {
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = dbContext.QbicleUser.Find(model.UserId),
                        Domain = device?.Location.Domain,
                        User = user,
                        Pin= pin
                    };

                    device.DeviceManagers.Add(userManager);

                    if (dbContext.Entry(device).State == EntityState.Detached)
                        dbContext.PosDevices.Attach(device);
                    dbContext.Entry(device).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    refModel.msgId = userManager.Id.ToString();
                }
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);

            }
            return refModel;
        }

        private int? DeletePreviousRoleUser(PosUserDeviceModel model, PosDevice device)
        {
            int? pin  = model.Pin;
            if (model.PreviousRole == "user")
            {
                var posUserDevice = device.Users.FirstOrDefault(e => e.User.Id == model.UserId);
                device.Users.Remove(posUserDevice);
            }

           if (model.PreviousRole == "cashier")
            {
                var cashier = device.DeviceCashiers.FirstOrDefault(l => l.User.Id == model.UserId);
                if (cashier != null)
                {
                    device.DeviceCashiers.Remove(cashier);
                    dbContext.PosCashiers.Remove(cashier);

                }
            }

            if (model.PreviousRole == "supervisor")
            {
                var supervisor = device.DeviceSupervisors.FirstOrDefault(l => l.User.Id == model.UserId);
                if (supervisor != null)
                {

                    device.DeviceSupervisors.Remove(supervisor);
                    dbContext.PosSupervisors.Remove(supervisor);

                }
            }
            if (model.PreviousRole == "manager")
            {
                var manager = device.DeviceManagers.FirstOrDefault(l => l.User.Id == model.UserId);
                if (manager != null)
                {
                    device.DeviceManagers.Remove(manager);
                    dbContext.PosTillManagers.Remove(manager);

                }
            }
            

            dbContext.SaveChanges();

            return pin;
        }

        //public ReturnJsonModel UpdatePosRoleUsers(PosUserDeviceModel model)
        //{
        //    var refModel = new ReturnJsonModel
        //    {
        //        result = true
        //    };
        //    try
        //    {
        //        if (ConfigManager.LoggingDebugSet)
        //            LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosUser", null, null, model);

        //        var device = dbContext.PosDevices.Find(model.DeviceId);
        //        if (device == null)
        //            return refModel;



        //        if (model.PreviousRole == "user")
        //        {
        //            var posUserDevice = device.Users.FirstOrDefault(e => e.User.Id == model.UserId);
        //            device.Users.Remove(posUserDevice);
        //        }

        //        if (model.PreviousRole == "cashier")
        //        {
        //            var cashier = device.DeviceCashiers.FirstOrDefault(l => l.User.Id == model.UserId);
        //            if (cashier != null)
        //            {
        //                device.DeviceCashiers.Remove(cashier);
        //                dbContext.PosCashiers.Remove(cashier);

        //            }
        //        }

        //        if (model.PreviousRole == "supervisor")
        //        {
        //            var manager = device.DeviceManagers.FirstOrDefault(l => l.User.Id == model.UserId);
        //            if (manager != null)
        //            {
        //                device.DeviceManagers.Remove(manager);
        //                dbContext.PosTillManagers.Remove(manager);

        //            }
        //        }

        //        if (model.PreviousRole == "manager")
        //        {
        //            var manager = device.DeviceManagers.FirstOrDefault(l => l.User.Id == model.UserId);
        //            if (manager != null)
        //            {
        //                device.DeviceManagers.Remove(manager);
        //                dbContext.PosTillManagers.Remove(manager);

        //            }
        //        }

        //        if (model.CurrentRole == "none")
        //        {
        //            var admin = device.Administrators.FirstOrDefault(l => l.User.Id == model.UserId);
        //            if (admin == null)
        //            {
        //                var posUserDevice = device.Users.FirstOrDefault(e => e.User.Id == model.UserId);
        //                device.Users.Remove(posUserDevice);

        //                if (dbContext.Entry(device).State == EntityState.Detached)
        //                    dbContext.PosDevices.Attach(device);
        //                dbContext.Entry(device).State = EntityState.Modified;
        //                dbContext.SaveChanges();
        //                refModel.msg = RenderPosUser(model);
        //                return refModel;
        //            }
        //        }



        //        if (model.CurrentRole == "user")
        //        {
        //            var userNew = dbContext.DeviceUsers.FirstOrDefault(e =>
        //            e.User.Id == model.UserId && e.Domain.Id == device.Location.Domain.Id);
        //            if (userNew == null)
        //                return refModel;

        //            device.Users.Add(userNew);
        //            if (dbContext.Entry(device).State == EntityState.Detached)
        //                dbContext.PosDevices.Attach(device);
        //            dbContext.Entry(device).State = EntityState.Modified;
        //            dbContext.SaveChanges();

        //            refModel.msgId = userNew.Id.ToString();
        //        }

        //        if (model.CurrentRole == "cashier")
        //        {
        //            var user = new UserRules(dbContext).GetById(model.UserId);

        //            var posUserDevice = device.Users.FirstOrDefault(e => e.User.Id == model.UserId);
        //            if (posUserDevice == null)
        //            {
        //                var userNew = dbContext.DeviceUsers.FirstOrDefault(e =>
        //                    e.User.Id == model.UserId && e.Domain.Id == device.Location.Domain.Id);

        //                device.Users.Add(userNew);
        //            }

        //            var userCashier = new PosCashier
        //            {
        //                CreatedDate = DateTime.UtcNow,
        //                CreatedBy = dbContext.QbicleUser.Find(model.UserId),
        //                Domain = device?.Location.Domain,
        //                User = user
        //            };

        //            device.DeviceCashiers.Add(userCashier);

        //            if (dbContext.Entry(device).State == EntityState.Detached)
        //                dbContext.PosDevices.Attach(device);
        //            dbContext.Entry(device).State = EntityState.Modified;
        //            dbContext.SaveChanges();
        //            refModel.msgId = userCashier.Id.ToString();
        //        }

        //        if (model.CurrentRole == "supervisor")
        //        {
        //            var user = new UserRules(dbContext).GetById(model.UserId);

        //            var posUserDevice = device.Users.FirstOrDefault(e => e.User.Id == model.UserId);
        //            if (posUserDevice == null)
        //            {
        //                var userNew = dbContext.DeviceUsers.FirstOrDefault(e =>
        //                    e.User.Id == model.UserId && e.Domain.Id == device.Location.Domain.Id);

        //                device.Users.Add(userNew);
        //            }

        //            var userSupervisor = new PosSupervisor
        //            {
        //                CreatedDate = DateTime.UtcNow,
        //                CreatedBy = dbContext.QbicleUser.Find(model.UserId),
        //                Domain = device?.Location.Domain,
        //                User = user
        //            };

        //            device.DeviceSupervisors.Add(userSupervisor);

        //            if (dbContext.Entry(device).State == EntityState.Detached)
        //                dbContext.PosDevices.Attach(device);
        //            dbContext.Entry(device).State = EntityState.Modified;
        //            dbContext.SaveChanges();
        //            refModel.msgId = userSupervisor.Id.ToString();
        //        }

        //        if (model.CurrentRole == "manager")
        //        {
        //            var user = new UserRules(dbContext).GetById(model.UserId);

        //            var posUserDevice = device.Users.FirstOrDefault(e => e.User.Id == model.UserId);
        //            if (posUserDevice == null)
        //            {
        //                var userNew = dbContext.DeviceUsers.FirstOrDefault(e =>
        //                    e.User.Id == model.UserId && e.Domain.Id == device.Location.Domain.Id);

        //                device.Users.Add(userNew);
        //            }

        //            var userManager = new PosTillManager
        //            {
        //                CreatedDate = DateTime.UtcNow,
        //                CreatedBy = dbContext.QbicleUser.Find(model.UserId),
        //                Domain = device?.Location.Domain,
        //                User = user
        //            };

        //            device.DeviceManagers.Add(userManager);

        //            if (dbContext.Entry(device).State == EntityState.Detached)
        //                dbContext.PosDevices.Attach(device);
        //            dbContext.Entry(device).State = EntityState.Modified;
        //            dbContext.SaveChanges();
        //            refModel.msgId = userManager.Id.ToString();
        //        }

        //        //if (model.IsDelete)
        //        //{
        //        //    var posUserDevice = device.Users.FirstOrDefault(e => e.User.Id == model.UserId);
        //        //    device.Users.Remove(posUserDevice);

        //        //    //var manager = device.DeviceManagers.FirstOrDefault(l => l.User.Id == model.UserId);
        //        //    //if (manager != null)
        //        //    //{
        //        //    //    device.DeviceManagers.Remove(manager);
        //        //    //    dbContext.PosTillManagers.Remove(manager);

        //        //    //}
        //        //    if (dbContext.Entry(device).State == EntityState.Detached)
        //        //        dbContext.PosDevices.Attach(device);
        //        //    dbContext.Entry(device).State = EntityState.Modified;
        //        //    dbContext.SaveChanges();
        //        //    refModel.msg = RenderPosUser(model);
        //        //    return refModel;
        //        //}

        //        //var userNew = dbContext.DeviceUsers.FirstOrDefault(e =>
        //        //    e.User.Id == model.UserId && e.Domain.Id == device.Location.Domain.Id);
        //        //if (userNew == null)
        //        //    return refModel;

        //        //device.Users.Add(userNew);
        //        //if (dbContext.Entry(device).State == EntityState.Detached)
        //        //    dbContext.PosDevices.Attach(device);
        //        //dbContext.Entry(device).State = EntityState.Modified;
        //        //dbContext.SaveChanges();

        //        //refModel.msgId = userNew.Id.ToString();

        //    }
        //    catch (Exception ex)
        //    {
        //        refModel.result = false;
        //        refModel.msg = ex.Message;
        //        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);

        //    }

        //    return refModel;
        //}
       
        //public ReturnJsonModel UpdatePosUserCashierDevice(PosUserDeviceModel model, string userId)
        //{
        //    var refModel = new ReturnJsonModel
        //    {
        //        result = true
        //    };
        //    try
        //    {
        //        if (ConfigManager.LoggingDebugSet)
        //            LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosUserCashierDevice", null, null, model);

        //        var device = dbContext.PosDevices.Find(model.DeviceId);
        //        if (device == null)
        //            return refModel;

        //        if (model.IsDelete)
        //        {
        //            var posCahierDevice = device.DeviceCashiers.FirstOrDefault(e => e.User.Id == model.UserId);
        //            device.DeviceCashiers.Remove(posCahierDevice);


                   
        //            if (dbContext.Entry(device).State == EntityState.Detached)
        //                dbContext.PosDevices.Attach(device);
        //            dbContext.Entry(device).State = EntityState.Modified;
        //            dbContext.SaveChanges();
        //            refModel.msg = RenderPosUser(model);
        //            return refModel;
        //        }

        //        var cashierNew = dbContext.PosCashiers.FirstOrDefault(e =>
        //            e.User.Id == model.UserId && e.Domain.Id == device.Location.Domain.Id);
        //        if (cashierNew == null)
        //            return refModel;

        //        var user = device.Users.FirstOrDefault(l => l.User.Id == model.UserId);
        //        if (user != null)
        //        {
        //            device.Users.Remove(user);
        //            dbContext.DeviceUsers.Remove(user);

        //        }

        //        var supervisor = device.DeviceSupervisors.FirstOrDefault(l => l.User.Id == model.UserId);
        //        if (supervisor != null)
        //        {
        //            device.DeviceSupervisors.Remove(supervisor);
        //            dbContext.PosSupervisors.Remove(supervisor);

        //        }

        //        var manager = device.DeviceManagers.FirstOrDefault(l => l.User.Id == model.UserId);
        //        if (manager != null)
        //        {
        //            device.DeviceManagers.Remove(manager);
        //            dbContext.PosTillManagers.Remove(manager);

        //        }

        //        device.DeviceCashiers.Add(cashierNew);
        //        if (dbContext.Entry(device).State == EntityState.Detached)
        //            dbContext.PosDevices.Attach(device);
        //        dbContext.Entry(device).State = EntityState.Modified;
        //        dbContext.SaveChanges();

        //        refModel.msgId = cashierNew.Id.ToString();

        //    }
        //    catch (Exception ex)
        //    {
        //        refModel.result = false;
        //        refModel.msg = ex.Message;
        //        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);

        //    }



        //    return refModel;
        //}
        
        //public ReturnJsonModel UpdatePosUserSupervisorDevice(PosUserDeviceModel model, string userId)
        //{
        //    var refModel = new ReturnJsonModel
        //    {
        //        result = true
        //    };
        //    try
        //    {
        //        if (ConfigManager.LoggingDebugSet)
        //            LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosUserSupervisorDevice", null, null, model);

        //        var device = dbContext.PosDevices.Find(model.DeviceId);
        //        if (device == null)
        //            return refModel;

        //        if (model.IsDelete)
        //        {
        //            var posCahierDevice = device.DeviceCashiers.FirstOrDefault(e => e.User.Id == model.UserId);
        //            device.DeviceCashiers.Remove(posCahierDevice);


        //            if (dbContext.Entry(device).State == EntityState.Detached)
        //                dbContext.PosDevices.Attach(device);
        //            dbContext.Entry(device).State = EntityState.Modified;
        //            dbContext.SaveChanges();
        //            refModel.msg = RenderPosUser(model);
        //            return refModel;
        //        }

        //        var cashierNew = dbContext.PosCashiers.FirstOrDefault(e =>
        //            e.User.Id == model.UserId && e.Domain.Id == device.Location.Domain.Id);
        //        if (cashierNew == null)
        //            return refModel;


        //        var user = device.Users.FirstOrDefault(l => l.User.Id == model.UserId);
        //        if (user != null)
        //        {
        //            device.Users.Remove(user);
        //            dbContext.DeviceUsers.Remove(user);

        //        }

        //        var cashier = device.DeviceCashiers.FirstOrDefault(l => l.User.Id == model.UserId);
        //        if (cashier != null)
        //        {
        //            device.DeviceCashiers.Remove(cashier);
        //            dbContext.PosCashiers.Remove(cashier);

        //        }

        //        var manager = device.DeviceManagers.FirstOrDefault(l => l.User.Id == model.UserId);
        //        if (manager != null)
        //        {
        //            device.DeviceManagers.Remove(manager);
        //            dbContext.PosTillManagers.Remove(manager);

        //        }

        //        device.DeviceCashiers.Add(cashierNew);
        //        if (dbContext.Entry(device).State == EntityState.Detached)
        //            dbContext.PosDevices.Attach(device);
        //        dbContext.Entry(device).State = EntityState.Modified;
        //        dbContext.SaveChanges();

        //        refModel.msgId = cashierNew.Id.ToString();

        //    }
        //    catch (Exception ex)
        //    {
        //        refModel.result = false;
        //        refModel.msg = ex.Message;
        //        LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);

        //    }

        //    return refModel;
        //}
        
        //public ReturnJsonModel UpdatePosUserManageDevice(PosUserDeviceModel model, string userId)
        //{
        //    var refModel = new ReturnJsonModel
        //    {
        //        result = true
        //    };
        //    try
        //    {
        //        if (ConfigManager.LoggingDebugSet)
        //            LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosUserManageDevice", userId, null, model);


        //        var device = dbContext.PosDevices.Find(model.DeviceId);
        //        if (device == null)
        //            return refModel;

        //        if (model.IsDelete)
        //        {
        //            var manager = device.DeviceManagers.FirstOrDefault(l => l.User.Id == model.UserId);
        //            if (manager != null)
        //            {
        //                device.DeviceManagers.Remove(manager);
        //                dbContext.PosTillManagers.Remove(manager);
        //            }
        //            dbContext.SaveChanges();
        //            return refModel;
        //        }

        //        var user = new UserRules(dbContext).GetById(model.UserId);

        //        var posUserDevice = device.Users.FirstOrDefault(e => e.User.Id == model.UserId);
        //        if (posUserDevice == null)
        //        {
        //            var userNew = dbContext.DeviceUsers.FirstOrDefault(e =>
        //                e.User.Id == model.UserId && e.Domain.Id == device.Location.Domain.Id);

        //            device.Users.Add(userNew);
        //        }

        //        var userManager = new PosTillManager
        //        {
        //            CreatedDate = DateTime.UtcNow,
        //            CreatedBy = dbContext.QbicleUser.Find(userId),
        //            Domain = device?.Location.Domain,
        //            User = user
        //        };

        //        device.DeviceManagers.Add(userManager);

        //        if (dbContext.Entry(device).State == EntityState.Detached)
        //            dbContext.PosDevices.Attach(device);
        //        dbContext.Entry(device).State = EntityState.Modified;
        //        dbContext.SaveChanges();
        //        refModel.msgId = userManager.Id.ToString();

        //    }
        //    catch (Exception ex)
        //    {

        //        LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
        //        refModel.result = false;
        //        refModel.msg = ex.Message;
        //    }


        //    return refModel;
        //}

        public ReturnJsonModel UpdatePosUserAdminDevice(PosUserDeviceModel model, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "UpdatePosUserAdminDevice", userId, null, model);


                var device = dbContext.PosDevices.Find(model.DeviceId);
                if (device == null)
                    return refModel;

                if (model.IsDelete)
                {
                    var admin = device.Administrators.FirstOrDefault(l => l.User.Id == model.UserId);
                    if (admin != null)
                    {
                        device.Administrators.Remove(admin);
                        dbContext.PosAdministrators.Remove(admin);
                    }
                    dbContext.SaveChanges();

                    refModel.msg = RenderPosUser(model);

                    return refModel;
                }

                var user = new UserRules(dbContext).GetById(model.UserId);

                var deviceDb = dbContext.PosDevices.FirstOrDefault(e => e.Id == model.DeviceId);

                var userAdmin = new PosAdministrator
                {
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = dbContext.QbicleUser.Find(userId),
                    Domain = deviceDb?.Location.Domain,
                    User = user
                };

                deviceDb.Administrators.Add(userAdmin);

                if (dbContext.Entry(deviceDb).State == EntityState.Detached)
                    dbContext.PosDevices.Attach(deviceDb);
                dbContext.Entry(deviceDb).State = EntityState.Modified;
                dbContext.SaveChanges();
                refModel.msgId = userAdmin.Id.ToString();

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);

            }
            return refModel;
        }

        public ReturnJsonModel DeletePosUserDevice(PosUserDeviceModel model)
        {
            var refModel = new ReturnJsonModel
            {
                result = true
            };
            var found = false;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "DeletePosUserDevice", null, null, model);


                var device = dbContext.PosDevices.Find(model.DeviceId);
                if (device == null)
                    return refModel;

                var posUser = dbContext.DeviceUsers.FirstOrDefault(e => e.Id == model.PosUserId);
                if (posUser != null)
                {
                    found = true;
                    device.Users.Remove(posUser);
                }
                    
                

                var cashier = device.DeviceCashiers.FirstOrDefault(l => l.User.Id == posUser.User.Id);
                if (cashier != null)
                {
                    found = true;
                    device.DeviceCashiers.Remove(cashier);
                    dbContext.PosCashiers.Remove(cashier);
                }

                var supervisor = device.DeviceSupervisors.FirstOrDefault(l => l.User.Id == posUser.User.Id);
                if (supervisor != null)
                {
                    found = true;
                    device.DeviceSupervisors.Remove(supervisor);
                    dbContext.PosSupervisors.Remove(supervisor);
                }

                var manager = device.DeviceManagers.FirstOrDefault(l => l.User.Id == posUser.User.Id);
                if (manager != null)
                {
                    found = true;
                    device.DeviceManagers.Remove(manager);
                    dbContext.PosTillManagers.Remove(manager);
                }

                var admin = device.Administrators.FirstOrDefault(l => l.User.Id == posUser.User.Id);
                if (admin != null)
                {
                    device.Administrators.Remove(admin);
                    dbContext.PosAdministrators.Remove(admin);
                }
                if (!found)
                {
                    return refModel;
                }
                //delete then delete Manager
                if (!model.IsDelete)
                    return refModel;

                if (dbContext.Entry(device).State == EntityState.Detached)
                    dbContext.PosDevices.Attach(device);
                dbContext.Entry(device).State = EntityState.Modified;
                dbContext.SaveChanges();
                refModel.msg = RenderPosUser(model);

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);

            }

            return refModel;

        }

        public ReturnJsonModel CreatePosUserDevice(List<PooledUserModel> pooledUsers, int deviceId, string userId)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "CreatePosUserDevice", null, null, pooledUsers, deviceId);


                var device = dbContext.PosDevices.FirstOrDefault(e => e.Id == deviceId);
                if (device == null)
                    return refModel;

                foreach (var model in pooledUsers)
                {
                    var user = new UserRules(dbContext).GetById(model.UserId);
                    var currentUser = dbContext.QbicleUser.Find(userId);
                    foreach (var pool in model.Pools)
                    {
                        switch (pool)
                        {
                            case PosUserType.None:
                                break;
                            case PosUserType.User:
                                var posUserDevice = device.Users.FirstOrDefault(e => e.User.Id == model.UserId);

                                if (posUserDevice == null)
                                {
                                    var userNew = dbContext.DeviceUsers.FirstOrDefault(e => e.User.Id == model.UserId && e.Domain.Id == device.Location.Domain.Id);

                                    device.Users.Add(userNew);
                                    if (dbContext.Entry(device).State == EntityState.Detached)
                                        dbContext.PosDevices.Attach(device);
                                    dbContext.Entry(device).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                }
                                break;
                            case PosUserType.Cashier:
                                var posCashierDevice = device.DeviceCashiers.FirstOrDefault(e => e.User.Id == model.UserId);

                                if (posCashierDevice == null)
                                {
                                    var userCashier = new PosCashier
                                    {
                                        CreatedDate = DateTime.UtcNow,
                                        CreatedBy = currentUser,
                                        Domain = device?.Location.Domain,
                                        User = user
                                    };

                                    device.DeviceCashiers.Add(userCashier);

                                    if (dbContext.Entry(device).State == EntityState.Detached)
                                        dbContext.PosDevices.Attach(device);
                                    dbContext.Entry(device).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                }

                               
                                break; 
                            case PosUserType.Supervisor:
                                var posSupervisorDevice = device.DeviceSupervisors.FirstOrDefault(e => e.User.Id == model.UserId);

                                if (posSupervisorDevice == null)
                                {
                                    var userSupervisor = new PosSupervisor
                                    {
                                        CreatedDate = DateTime.UtcNow,
                                        CreatedBy = currentUser,
                                        Domain = device?.Location.Domain,
                                        User = user
                                    };

                                    device.DeviceSupervisors.Add(userSupervisor);

                                    if (dbContext.Entry(device).State == EntityState.Detached)
                                        dbContext.PosDevices.Attach(device);
                                    dbContext.Entry(device).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                }
                                break; 
                            case PosUserType.Manager:
                                var posManagerDevice = device.DeviceManagers.FirstOrDefault(e => e.User.Id == model.UserId);

                                if (posManagerDevice == null)
                                {
                                    var userManager = new PosTillManager
                                    {
                                        CreatedDate = DateTime.UtcNow,
                                        CreatedBy = currentUser,
                                        Domain = device?.Location.Domain,
                                        User = user
                                    };

                                    device.DeviceManagers.Add(userManager);

                                    if (dbContext.Entry(device).State == EntityState.Detached)
                                        dbContext.PosDevices.Attach(device);
                                    dbContext.Entry(device).State = EntityState.Modified;
                                    dbContext.SaveChanges();
                                }
                                break;
                            case PosUserType.Admin:
                                var userAdmin = new PosAdministrator
                                {
                                    CreatedDate = DateTime.UtcNow,
                                    CreatedBy = currentUser,
                                    Domain = device?.Location.Domain,
                                    User = user
                                };

                                device.Administrators.Add(userAdmin);

                                if (dbContext.Entry(device).State == EntityState.Detached)
                                    dbContext.PosDevices.Attach(device);
                                dbContext.Entry(device).State = EntityState.Modified;
                                dbContext.SaveChanges();
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, pooledUsers, deviceId);

            }


            return refModel;
        }

        public string RenderPosUser(PosUserDeviceModel model)
        {
            var html = new StringBuilder();
            html.Append("<article>");
            html.Append("<a href='javascript:'>");
            html.Append("<div class='contact-avatar'>");
            html.Append($"<div style='background-image: url(\"{model.Avatar}\");'>&nbsp;</div>");
            html.Append("</div>");
            html.Append("<div class='contact-info'>");
            html.Append($"<h5 class='{model.Group}'>{model.Name}</h5>");
            html.Append($"<input class='user-id-hidden' hidden='' value='{model.UserId}' />");
            html.Append($"<select id='select-{model.UserId}' class='form-control select2 add-user-pool' multiple='' style='width: 500px;'>");
            html.Append($"<option class='{model.UserId}#{PosUserType.None}' value='{PosUserType.None}' selected=''>Don't include</option>");
            html.Append($"<option class='{model.UserId}#{PosUserType.None}' value='{PosUserType.Admin}'>Include as PoS Administrator</option>");
            html.Append($"<option class='{model.UserId}#{PosUserType.None}' value='{PosUserType.Manager}'>Include as Till Manager</option>");
            html.Append($"<option class='{model.UserId}#{PosUserType.None}' value='{PosUserType.Supervisor}'>Include as Till Supervisor</option>");
            html.Append($"<option class='{model.UserId}#{PosUserType.None}' value='{PosUserType.Cashier}'>Include as Till Cashier</option>");
            html.Append($"<option class='{model.UserId}#{PosUserType.None}' value='{PosUserType.User}'>Include as Till User</option>");
            html.Append("</select>");
            html.Append("</div>");
            html.Append("</a>");
            html.Append("</article>");

            return html.ToString();
        }
        private string GetFullName(ApplicationUser user, string currentUserId = "")
        {
            var result = "";
            if (user != null)
            {
                if (user.Id == currentUserId)
                    result = "Me";
                else
                {
                    if (!string.IsNullOrEmpty(user.DisplayUserName))
                        result = user.DisplayUserName;
                    else if (!string.IsNullOrEmpty(user.Forename) && !string.IsNullOrEmpty(user.Surname))
                        result = user.Forename + " " + user.Surname;
                    else
                        result = user.Email;
                }
            }
            return result;
        }
    }
}
