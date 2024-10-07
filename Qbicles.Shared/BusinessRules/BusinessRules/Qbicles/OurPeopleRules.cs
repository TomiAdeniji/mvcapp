using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Invitation;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Tweetinvi.Logic.Model;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class OurPeopleRules
    {
        ApplicationDbContext _db;
        public OurPeopleRules()
        {

        }
        public OurPeopleRules(ApplicationDbContext context)
        {
            _db = context;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }
        /// <summary>
        /// Get all users and guests of the Domain
        /// map to a list return
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public List<OurPeopleModel> GetAllOurPeopleByDomain(int domainId, string timezone, string userId = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get all our people by Domain", null, null, domainId, userId);

                var domain = DbContext.Domains.Find(domainId);

                var ourPeople = new List<OurPeopleModel>();
                //string label = "";
                if (domain.Administrators.Any(a => a.Id == userId))
                {
                    foreach (var user in domain.Users)
                    {
                        var people = new OurPeopleModel
                        {
                            Id = user.Id,
                            FullName = user.GetFullName(),
                            TypeUseFill = SystemRoles.DomainUser,
                            //TypeUser = label,
                            Image = user.ProfilePic,
                            ImageUri = user.ProfilePic.ToUri(),
                            QbiclesCount = user.Qbicles.Where(x => x.Domain.Id == domain.Id).Count(),
                            Forename = user.Forename,
                            Email = user.Email,
                            CreatedDate = user.DateBecomesMember.ConvertTimeToUtc(timezone),
                            lstRole = user.DomainRoles.Where(p => p.Domain.Id == domain.Id).Select(s => s.Name).ToList()
                        };
                        if (domain.Administrators.Any(u => u.Id == user.Id))
                        {
                            people.TypeUser = AdminLevel.Administrators.GetDescription();
                            people.TypeUserId = (int)AdminLevel.Administrators;
                        }
                        else if (domain.QbicleManagers.Any(u => u.Id == user.Id))
                        {
                            people.TypeUser = AdminLevel.QbicleManagers.GetDescription();
                            people.TypeUserId = (int)AdminLevel.QbicleManagers;
                        }
                        else
                        {
                            people.TypeUser = AdminLevel.Users.GetDescription();
                            people.TypeUserId = (int)AdminLevel.Users;
                        }
                        ourPeople.Add(people);
                    }
                }
                else if (domain.QbicleManagers.Any(a => a.Id == userId))
                {
                    foreach (var user in domain.Users)
                    {
                        if (!domain.Administrators.Any(u => u.Id == user.Id))
                        {

                            var people = new OurPeopleModel
                            {
                                Id = user.Id,
                                FullName = user.GetFullName(),
                                TypeUseFill = SystemRoles.DomainUser,

                                Image = user.ProfilePic,
                                ImageUri = user.ProfilePic.ToUri(),
                                QbiclesCount = user.Qbicles.Where(x => x.Domain.Id == domain.Id).Count(),
                                Forename = user.Forename,
                                Email = user.Email,
                                CreatedDate = user.DateBecomesMember,
                                lstRole = user.DomainRoles.Select(s => s.Name).Distinct().ToList()
                            };
                            if (domain.QbicleManagers.Any(u => u.Id == user.Id))
                            {
                                people.TypeUser = AdminLevel.QbicleManagers.GetDescription();
                                people.TypeUserId = (int)AdminLevel.QbicleManagers;
                            }
                            else
                            {
                                people.TypeUser = AdminLevel.Users.GetDescription();
                                people.TypeUserId = (int)AdminLevel.Users;
                            }
                            ourPeople.Add(people);
                        }

                    }
                }


                ourPeople = ourPeople.OrderBy(u => u.Forename).ToList();
                return ourPeople;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return new List<OurPeopleModel>();
            }

        }
        // refactor controller to Datatable format
        public DataTablesResponse GetAllOurPeopleByDomainDataTable(IDataTablesRequest requestModel, int domainId, string timezone, string userId = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get all our people by Domain", null, null, domainId, userId);
                var domain = DbContext.Domains.Find(domainId);
                var totalPeople = 0;
                var ourPeople = new List<OurPeopleModel>();
                var timeZoneUser = TimeZoneInfo.GetSystemTimeZones().Where(e => e.Id.Equals(timezone)).FirstOrDefault();
                //string label = "";
                if (domain.Administrators.Any(a => a.Id == userId))
                {
                    foreach (var user in domain.Users)
                    {
                        var people = new OurPeopleModel
                        {
                            Id = user.Id,
                            FullName = user.GetFullName(),
                            TypeUseFill = SystemRoles.DomainUser,
                            //TypeUser = label,
                            Image = user.ProfilePic,
                            ImageUri = user.ProfilePic.ToUri(),
                            QbiclesCount = user.Qbicles.Where(x => x.Domain.Id == domain.Id).Count(),
                            Forename = user.Forename,
                            Email = user.Email,
                            CreatedDate = user.DateBecomesMember.ConvertTimeToUtc(timezone),
                            lstRole = user.DomainRoles.Where(p => p.Domain.Id == domain.Id).Select(s => s.Name).ToList()
                        };
                        if (domain.Administrators.Any(u => u.Id == user.Id))
                        {
                            people.TypeUser = AdminLevel.Administrators.GetDescription();
                            people.TypeUserId = (int)AdminLevel.Administrators;
                        }
                        else if (domain.QbicleManagers.Any(u => u.Id == user.Id))
                        {
                            people.TypeUser = AdminLevel.QbicleManagers.GetDescription();
                            people.TypeUserId = (int)AdminLevel.QbicleManagers;
                        }
                        else
                        {
                            people.TypeUser = AdminLevel.Users.GetDescription();
                            people.TypeUserId = (int)AdminLevel.Users;
                        }
                        ourPeople.Add(people);
                    }
                }
                else if (domain.QbicleManagers.Any(a => a.Id == userId))
                {
                    foreach (var user in domain.Users)
                    {
                        if (!domain.Administrators.Any(u => u.Id == user.Id))
                        {

                            var people = new OurPeopleModel
                            {
                                Id = user.Id,
                                FullName = user.GetFullName(),
                                TypeUseFill = SystemRoles.DomainUser,

                                Image = user.ProfilePic,
                                ImageUri = user.ProfilePic.ToUri(),
                                QbiclesCount = user.Qbicles.Where(x => x.Domain.Id == domain.Id).Count(),
                                Forename = user.Forename,
                                Email = user.Email,
                                CreatedDate = user.DateBecomesMember,
                                lstRole = user.DomainRoles.Select(s => s.Name).Distinct().ToList()
                            };
                            if (domain.QbicleManagers.Any(u => u.Id == user.Id))
                            {
                                people.TypeUser = AdminLevel.QbicleManagers.GetDescription();
                                people.TypeUserId = (int)AdminLevel.QbicleManagers;
                            }
                            else
                            {
                                people.TypeUser = AdminLevel.Users.GetDescription();
                                people.TypeUserId = (int)AdminLevel.Users;
                            }
                            ourPeople.Add(people);
                        }

                    }
                }

                totalPeople = ourPeople.Count;
                ourPeople = ourPeople.OrderByDescending(u => u.CreatedDate).Skip(requestModel.Start).Take(requestModel.Length).ToList();
                //convert datetime and blid raw data
                var ourPeopleConverted = ourPeople.Select(e => new
                {
                    e.Id,
                    e.FullName,
                    e.TypeUseFill,
                    e.TypeUser,
                    e.Image,
                    e.QbiclesCount,
                    e.Forename,
                    e.Email,
                    CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(e.CreatedDate, timeZoneUser).ToString("dd.MM.yyyy"),
                    e.lstRole,
                    e.TypeUserId,
                    e.ImageUri
                }).ToList();

                return new DataTablesResponse(requestModel.Draw, ourPeopleConverted, totalPeople, totalPeople);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, timezone, userId);
                return new DataTablesResponse(requestModel.Draw, new List<OurPeopleModel>(), 0, 0);
            }
        }


        /// <summary>
        /// Get all users and guests of the Domain
        /// map to a list return
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public List<OurPeopleModel> SearchOurPeopleByDomain(int domainId, string Username, int roleLevel, int[] domainRole, string timezone, string userId = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Search our people by Domain", null, null, domainId, Username, roleLevel, domainRole, userId);

                var domain = DbContext.Domains.Find(domainId);
                var ourPeople = new List<OurPeopleModel>();
                string label = "";
                var lstUser = domain.Users;
                if (lstUser == null)
                    lstUser = new List<ApplicationUser>();
                if (!string.IsNullOrEmpty(Username))
                    lstUser = lstUser.Where(p => (!string.IsNullOrEmpty(p.Surname) ? (p.Surname.ToLower().Contains(Username.ToLower()) || p.Forename.ToLower().Contains(Username.ToLower())) : p.UserName.ToLower().Contains(Username.ToLower()))).ToList();
                if (roleLevel > 0)
                {
                    if (roleLevel == 1)
                        lstUser = lstUser.Where(p => !domain.Administrators.Any(a => a.Id == p.Id) && !domain.QbicleManagers.Any(q => q.Id == p.Id)).ToList();
                    else if (roleLevel == 2)
                        lstUser = lstUser.Where(p => domain.QbicleManagers.Any(q => q.Id == p.Id)).ToList();
                    else if (roleLevel == 3)
                        lstUser = lstUser.Where(p => domain.Administrators.Any(a => a.Id == p.Id)).ToList();
                }
                if (domainRole != null && domainRole.Length > 0)
                    lstUser = lstUser.Where(p => p.DomainRoles.Any(d => d.Domain.Id == domain.Id && domainRole.Any(a => a == d.Id))).ToList();
                if (domain.Administrators.Any(a => a.Id == userId))
                {
                    foreach (var user in lstUser)
                    {

                        var people = new OurPeopleModel
                        {
                            Id = user.Id,
                            FullName = (string.IsNullOrEmpty(user.Forename) || string.IsNullOrEmpty(user.Surname)) ? user.UserName : user.Forename + " " + user.Surname,
                            TypeUseFill = SystemRoles.DomainUser,
                            TypeUser = label,
                            Image = user.ProfilePic,
                            QbiclesCount = user.Qbicles.Where(x => x.Domain.Id == domain.Id).Count(),
                            Forename = user.Forename,
                            Email = user.Email,
                            CreatedDate = user.DateBecomesMember.ConvertTimeToUtc(timezone),
                            lstRole = user.DomainRoles.Where(p => p.Domain.Id == domain.Id).Select(s => s.Name).ToList()
                        };
                        if (domain.Administrators.Any(u => u.Id == user.Id))
                        {
                            people.TypeUser = AdminLevel.Administrators.GetDescription();
                            people.TypeUserId = (int)AdminLevel.Administrators;
                        }
                        else if (domain.QbicleManagers.Any(u => u.Id == user.Id))
                        {
                            people.TypeUser = AdminLevel.QbicleManagers.GetDescription();
                            people.TypeUserId = (int)AdminLevel.QbicleManagers;
                        }
                        else
                        {
                            people.TypeUser = AdminLevel.Users.GetDescription();
                            people.TypeUserId = (int)AdminLevel.Users;
                        }
                        ourPeople.Add(people);
                    }
                }
                else if (domain.QbicleManagers.Any(a => a.Id == userId))
                {
                    foreach (var user in lstUser)
                    {
                        if (!domain.Administrators.Any(u => u.Id == user.Id))
                        {
                            var people = new OurPeopleModel
                            {
                                Id = user.Id,
                                FullName = (string.IsNullOrEmpty(user.Forename) || string.IsNullOrEmpty(user.Surname)) ? user.UserName : user.Forename + " " + user.Surname,
                                TypeUseFill = SystemRoles.DomainUser,
                                TypeUser = label,
                                Image = user.ProfilePic,
                                QbiclesCount = user.Qbicles.Where(x => x.Domain.Id == domain.Id).Count(),
                                Forename = user.Forename,
                                Email = user.Email,
                                CreatedDate = user.DateBecomesMember,
                                lstRole = user.DomainRoles.Select(s => s.Name).Distinct().ToList()
                            };
                            if (domain.QbicleManagers.Any(u => u.Id == user.Id))
                            {
                                people.TypeUser = AdminLevel.QbicleManagers.GetDescription();
                                people.TypeUserId = (int)AdminLevel.QbicleManagers;
                            }
                            else
                            {
                                people.TypeUser = AdminLevel.Users.GetDescription();
                                people.TypeUserId = (int)AdminLevel.Users;
                            }
                            ourPeople.Add(people);
                        }

                    }
                }

                ourPeople = ourPeople.OrderBy(u => u.Forename).ToList();
                return ourPeople;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, Username, roleLevel, domainRole, userId);
                return new List<OurPeopleModel>();
            }

        }

        // refector controller to Datatable
        public DataTablesResponse SearchOurPeopleByDomainDataTable(IDataTablesRequest requestModel, int domainId, string keySearch, int roleLevel, int[] domainRole, string timezone, string userId = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Search our people by Domain", null, null, domainId, keySearch, roleLevel, domainRole, userId);

                var domain = DbContext.Domains.Find(domainId);
                var ourPeople = new List<OurPeopleModel>();
                string label = "";
                var lstUser = domain.Users?? new List<ApplicationUser>();
                var totalPeople = 0;
                var timeZoneUser = TimeZoneInfo.GetSystemTimeZones().Where(e => e.Id.Equals(timezone)).FirstOrDefault();
                
                if (roleLevel > 0)
                {
                    if (roleLevel == 1)
                        lstUser = lstUser.Where(p => !domain.Administrators.Any(a => a.Id == p.Id) && !domain.QbicleManagers.Any(q => q.Id == p.Id)).ToList();
                    else if (roleLevel == 2)
                        lstUser = lstUser.Where(p => domain.QbicleManagers.Any(q => q.Id == p.Id)).ToList();
                    else if (roleLevel == 3)
                        lstUser = lstUser.Where(p => domain.Administrators.Any(a => a.Id == p.Id)).ToList();
                }

                if (domainRole != null && domainRole.Length > 0)
                    lstUser = lstUser.Where(p => p.DomainRoles.Any(d => d.Domain.Id == domain.Id && domainRole.Any(a => a == d.Id))).ToList();

                if (domain.Administrators.Any(a => a.Id == userId))
                {
                    foreach (var user in lstUser)
                    {

                        var people = new OurPeopleModel
                        {
                            Id = user.Id,
                            FullName = user.GetFullName(),
                            TypeUseFill = SystemRoles.DomainUser,
                            TypeUser = label,
                            Image = user.ProfilePic,
                            QbiclesCount = user.Qbicles.Where(x => x.Domain.Id == domain.Id).Count(),
                            Forename = user.Forename,
                            Email = user.Email,
                            CreatedDate = user.DateBecomesMember.ConvertTimeToUtc(timezone),
                            lstRole = user.DomainRoles.Where(p => p.Domain.Id == domain.Id).Select(s => s.Name).ToList()
                        };
                        if (domain.Administrators.Any(u => u.Id == user.Id))
                        {
                            people.TypeUser = AdminLevel.Administrators.GetDescription();
                            people.TypeUserId = (int)AdminLevel.Administrators;
                        }
                        else if (domain.QbicleManagers.Any(u => u.Id == user.Id))
                        {
                            people.TypeUser = AdminLevel.QbicleManagers.GetDescription();
                            people.TypeUserId = (int)AdminLevel.QbicleManagers;
                        }
                        else
                        {
                            people.TypeUser = AdminLevel.Users.GetDescription();
                            people.TypeUserId = (int)AdminLevel.Users;
                        }
                        ourPeople.Add(people);
                    }
                }
                else if (domain.QbicleManagers.Any(a => a.Id == userId))
                {
                    foreach (var user in lstUser)
                    {
                        if (!domain.Administrators.Any(u => u.Id == user.Id))
                        {
                            var people = new OurPeopleModel
                            {
                                Id = user.Id,
                                FullName = (string.IsNullOrEmpty(user.Forename) || string.IsNullOrEmpty(user.Surname)) ? user.UserName : user.Forename + " " + user.Surname,
                                TypeUseFill = SystemRoles.DomainUser,
                                TypeUser = label,
                                Image = user.ProfilePic,
                                QbiclesCount = user.Qbicles.Where(x => x.Domain.Id == domain.Id).Count(),
                                Forename = user.Forename,
                                Email = user.Email,
                                CreatedDate = user.DateBecomesMember,
                                lstRole = user.DomainRoles.Select(s => s.Name).Distinct().ToList(),
                            };
                            if (domain.QbicleManagers.Any(u => u.Id == user.Id))
                            {
                                people.TypeUser = AdminLevel.QbicleManagers.GetDescription();
                                people.TypeUserId = (int)AdminLevel.QbicleManagers;
                            }
                            else
                            {
                                people.TypeUser = AdminLevel.Users.GetDescription();
                                people.TypeUserId = (int)AdminLevel.Users;
                            }
                            ourPeople.Add(people);
                        }

                    }
                }

                totalPeople = ourPeople.Count;

                if (!string.IsNullOrEmpty(keySearch))
                    ourPeople = ourPeople.Where(p => p.FullName.ToLower().Contains(keySearch.ToLower()) || p.Email.ToLower().Contains(keySearch.ToLower())).ToList();

                ourPeople = ourPeople.OrderByDescending(u => u.CreatedDate).Skip(requestModel.Start).Take(requestModel.Length).ToList();

                //convert datetime and blid raw data
                var ourPeopleConverted = ourPeople.Select(e => new
                {
                    e.Id,
                    e.FullName,
                    e.TypeUseFill,
                    e.TypeUser,
                    e.Image,
                    e.QbiclesCount,
                    e.Forename,
                    e.Email,
                    CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(e.CreatedDate, timeZoneUser).ToString("dd.MM.yyyy"),
                    e.lstRole,
                    e.TypeUserId,
                    e.ImageUri
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, ourPeopleConverted, totalPeople, totalPeople);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, domainId, timezone, userId, keySearch, roleLevel, domainRole);
                return new DataTablesResponse(requestModel.Draw, new List<OurPeopleModel>(), 0, 0);
            }
        }
        public bool PromoteOrDemoteUser(int domainId, string userId, AdminLevel promoteOrDemoteTo, AdminLevel currentPossition)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Search our people by Domain", null, null, domainId, userId, promoteOrDemoteTo, currentPossition);

                var domain = DbContext.Domains.Find(domainId);

                var isOk = false;
                switch (promoteOrDemoteTo)
                {
                    case AdminLevel.Users:
                        if (currentPossition == AdminLevel.Administrators)
                        {
                            var user = domain.Administrators.FirstOrDefault(p => p.Id == userId);
                            if (user != null)
                            {
                                domain.Administrators.Remove(user);
                                DbContext.SaveChanges();
                                isOk = true;
                            }
                        }
                        else if (currentPossition == AdminLevel.QbicleManagers)
                        {
                            var user = domain.QbicleManagers.FirstOrDefault(p => p.Id == userId);
                            if (user != null)
                            {
                                domain.QbicleManagers.Remove(user);
                                DbContext.SaveChanges();
                                isOk = true;
                            }
                        }
                        break;
                    case AdminLevel.QbicleManagers:
                        if (currentPossition == AdminLevel.Administrators)
                        {
                            var user = domain.Administrators.FirstOrDefault(p => p.Id == userId);
                            if (user != null)
                            {
                                domain.Administrators.Remove(user);
                                domain.QbicleManagers.Add(user);
                                DbContext.SaveChanges();
                                isOk = true;
                            }
                        }
                        else if (currentPossition == AdminLevel.Users)
                        {
                            var user = domain.Users.FirstOrDefault(p => p.Id == userId);
                            if (user != null)
                            {
                                domain.QbicleManagers.Add(user);
                                DbContext.SaveChanges();
                                isOk = true;
                            }
                        }
                        break;
                    case AdminLevel.Administrators:
                        if (currentPossition == AdminLevel.QbicleManagers)
                        {
                            var user = domain.QbicleManagers.FirstOrDefault(p => p.Id == userId);
                            if (user != null)
                            {
                                domain.Administrators.Add(user);
                                domain.QbicleManagers.Remove(user);
                                DbContext.SaveChanges();
                                isOk = true;
                            }
                        }
                        else if (currentPossition == AdminLevel.Users)
                        {
                            var user = domain.Users.FirstOrDefault(p => p.Id == userId);
                            if (user != null)
                            {
                                domain.Administrators.Add(user);
                                DbContext.SaveChanges();
                                isOk = true;
                            }
                        }
                        break;
                }
                return isOk;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, userId, promoteOrDemoteTo, currentPossition);
                return false;
            }

        }

        public List<InvitationCustom> GetAllInvitationByDomain(QbicleDomain domain)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get all invitation by domain", null, null, domain);

                var query = from c in DbContext.Invitations.Where(p => p.Domain.Id == domain.Id)
                            join u in DbContext.QbicleUser on c.Email equals u.Email into TMP
                            from user in TMP.DefaultIfEmpty()
                            select new InvitationCustom
                            {
                                Id = c.Id,
                                Email = c.Email,
                                CreatedDate = c.Log.OrderByDescending(o => o.Id).FirstOrDefault().CreatedDate,
                                Note = c.Note,
                                Status = c.Status,
                                Forename = user.Forename,
                                Surname = user.Surname,
                                UserName = user.UserName,
                                ProfilePic = user.ProfilePic,
                                UserId = user.Id
                            };
                return query.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domain);
                return new List<InvitationCustom>();
            }
        }

        public int NotificationCountPendingByUser(string email)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get invitation approver by user", null, null, email);

                return DbContext.Invitations.Where(p => p.Email == email && p.Status == InvitationStatusEnum.Pending).Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, email);
                return 0;
            }
        }

        public List<InvitationCustom> GetInvitationApproverByUser(string email)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get invitation approver by user", null, null, email);

                var query = from c in DbContext.Invitations.Where(p => p.Email == email && p.Status == InvitationStatusEnum.Pending)
                            join d in DbContext.Domains on c.Domain.Id equals d.Id
                            select new InvitationCustom
                            {
                                Id = c.Id,
                                CreatedDate = c.Log.OrderByDescending(o => o.Id).FirstOrDefault().CreatedDate,
                                Status = c.Status,
                                UserId = c.CreatedBy.Id,
                                DomainName = d.Name,
                                DomainPic = d.LogoUri,
                                InviteBy = (!string.IsNullOrEmpty(c.CreatedBy.Surname) ? c.CreatedBy.Forename + " " + c.CreatedBy.Surname : c.CreatedBy.UserName),
                                DomainId = d.Id
                            };
                return query.ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, email);
                return new List<InvitationCustom>();
            }

        }
        public bool RemovedUserFromDomain(int domainId, string currentUserId, string removedUserId, int cubeId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "RemovedUserFromDomain", currentUserId, null, domainId, currentUserId, removedUserId);

                var domain = DbContext.Domains.Find(domainId);
                var currentUser = DbContext.QbicleUser.Find(currentUserId);
                var removedUser = DbContext.QbicleUser.Find(removedUserId);
                var isOk = false;
                var user = domain.Administrators.Find(p => p.Id == removedUserId);
                if (user != null)
                {
                    domain.Administrators.Remove(user);
                    isOk = true;
                }
                user = domain.QbicleManagers.FirstOrDefault(p => p.Id == removedUserId);
                if (user != null)
                {
                    domain.QbicleManagers.Remove(user);
                    isOk = true;
                }
                user = domain.Users.FirstOrDefault(p => p.Id == removedUserId);
                if (user != null)
                {
                    domain.Users.Remove(user);
                    isOk = true;
                }
                if (isOk)
                {

                    if (_db.Entry(removedUser).State == EntityState.Detached)
                        _db.Users.Attach(removedUser);
                    _db.Entry(removedUser).State = EntityState.Modified;
                    DbContext.SaveChanges();
                    //Remove user from any list Members in Qbicle
                    new QbicleRules(DbContext).RemoveMemberQbicles(user, domain.Id);
                    new CommerceRules(DbContext).RemoveUserFromDefaultRelationshipManagers(user, domain.Id);
                    //end remove
                    var nRule = new NotificationRules(DbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        DomainId = domain.Id,
                        QbicleId = cubeId,
                        EventNotify = NotificationEventEnum.RemoveUserOutOfDomain,
                        AppendToPageName = ApplicationPageName.Domain,
                        CreatedByName = currentUser.GetFullName(),
                        CreatedById = currentUserId,
                        ObjectById = removedUserId,
                        ReminderMinutes = 0
                    };
                    nRule.Notification2UserCreateRemoveFromDomain(activityNotification);
                }
                return isOk;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, domainId, currentUserId, removedUserId);
                return false;
            }


        }

        public ReturnJsonModel ApproverOrRejectInvitation(int id, int status, int domainId, string note, string userId)
        {
            try
            {
                var result = new ReturnJsonModel { result = true };
                var invitationStatus = (InvitationStatusEnum)status;

                var domainInvitation = DbContext.Domains.FirstOrDefault(p => p.Id == domainId);
                var invitation = DbContext.Invitations.FirstOrDefault(p => p.Id == id);

                var userSlots = HelperClass.GetDomainUsersAllowed(domainId);
                var canAccepted = userSlots.ActualMembers < userSlots.UsersAllowed;
                if (!canAccepted && invitationStatus == InvitationStatusEnum.Accepted)
                    return new ReturnJsonModel { result = false, msg = $"We were unable to complete your request due to a lack of space in {domainInvitation.Name}. Please try again later, or contact the business for support." };

                var user = new UserRules(DbContext).GetById(userId);
                invitation.LastUpdateDate = DateTime.UtcNow;
                invitation.LastUpdatedBy = user;
                invitation.Status = invitationStatus;
                if (invitationStatus == InvitationStatusEnum.Accepted)
                {
                    if (!domainInvitation.Users.Any(e => e.Id == userId))
                    {
                        user.DateBecomesMember = DateTime.UtcNow;
                        domainInvitation.Users.Add(user);
                        #region push notification to update Members datatable in User who invited
                        var nRule = new NotificationRules(DbContext);
                        var approveNotification = new Notification
                        {
                            OriginatingConnectionId = "",
                            CreatedDate = DateTime.UtcNow,
                            CreatedBy = user,
                            SentDate = DateTime.UtcNow,
                            SentMethod = NotificationSendMethodEnum.Broadcast,
                            Event = NotificationEventEnum.UpdateMembersList,
                            NotifiedUser = invitation.CreatedBy,
                            IsAlertDisplay = true,
                            AssociatedDomain = domainInvitation,
                            AssociateInvitation = invitation,
                        };
                        DbContext.Notifications.Add(approveNotification);
                        DbContext.Entry(approveNotification).State = EntityState.Added;
                        DbContext.SaveChanges();
                        nRule.ReloadMemberDataTable(approveNotification);
                        #endregion
                    }
                    else
                    {
                        result.msg = $"Email {user.Email} is already a member of the {domainInvitation.Name} of domain!";
                        result.result = false;
                        return result;
                    }
                }
                else
                {
                    invitation.Note = note;
                }
                DbContext.SaveChanges();
                new NotificationRules(DbContext).MarkAsReadNotification(id);
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }
    }
}
