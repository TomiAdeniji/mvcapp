using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using static Qbicles.BusinessRules.HelperClass;
using static Qbicles.Models.Notification;
using static Qbicles.Models.QbicleActivity;

namespace Qbicles.BusinessRules
{
    public class QbicleRules
    {
        private ApplicationDbContext dbContext;

        public QbicleRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public bool SystemRoleValidation(string userId, string roleName)
        {
            var role = dbContext.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
                return false;
            if (!role.Users.Any(u => u.UserId == userId))
                return false;
            return true;
        }

        public List<Qbicle> GetQbicleByDomainId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Qbicle by domain id", null, null, id);

                return dbContext.Qbicles.Where(d => d.Domain.Id == id && !d.IsHidden).OrderByDescending(o => o.LastUpdated).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public List<Qbicle> GetQbicleByUserId(int domainId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Qbicle by user id", null, null, domainId, userId);

                return dbContext.Qbicles.Where(d => d.Domain.Id == domainId && (d.Manager.Id == userId || d.Members.Any(s => s.Id == userId))).OrderByDescending(o => o.LastUpdated).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, userId);
                return null;
            }
        }

        public ApplicationUser GetUser(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get user", null, null, userId);

                var ur = new UserRules(dbContext);
                return ur.GetUser(userId, 0);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return null;
            }
        }

        public Qbicle UpdateCurrentQbicle(int currentQbicleId, string curentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get user", null, null, currentQbicleId, curentUserId);

                var user = GetUser(curentUserId);
                var cubeChange = GetQbicleById(currentQbicleId);
                if (cubeChange == null)
                    return new Qbicle();

                if (dbContext.Entry(user).State == EntityState.Detached)
                    dbContext.QbicleUser.Attach(user);
                dbContext.Entry(user).State = EntityState.Modified;
                dbContext.SaveChanges();

                return cubeChange;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentQbicleId, curentUserId);
                return new Qbicle();
            }
        }


        /// <summary>
        ///     validate duplicate Qbicle name
        ///     But Domains another have same Qbicle name
        /// </summary>
        /// <param name="qbicId"></param>
        /// <param name="qbicName"></param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public bool DuplicateQbicleNameCheck(int qbicId, string qbicName, int domainId)
        {
            bool exist;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get user", null, null, qbicId, qbicName, domainId);

                //var user = dbContext.Qbicles.Find(qbicId);
                var lstQbicle = dbContext.Qbicles.Where(p => p.Domain.Id == domainId).ToList();
                if (lstQbicle.Any())
                {
                    exist = qbicId > 0 ? lstQbicle.Any(x => x.Id != qbicId && x.Name.Trim() == qbicName.Trim()) : lstQbicle.Any(x => x.Name.Trim() == qbicName.Trim());
                }
                else
                    exist = false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicId, qbicName, domainId);
                exist = false;
            }

            return exist;
        }


        /// <summary>
        /// Save a Qbicle
        /// </summary>
        /// <param name="token"></param>
        /// <param name="qbic"></param>
        /// <param name="qbicleUser"></param>
        /// <param name="guestsQbicle"></param>
        /// <param name="domainId"></param>
        /// <param name="curentUserId"></param>
        /// <param name="managerId">Manager Id</param>
        /// <returns>-1 Save false. 1 - add new. 2 - update</returns>
        public ReturnJsonModel SaveQbicle(Qbicle qbic, string[] qbicleUser, string[] guestsQbicle, int domainId, string curentUserId, string managerId, string originatingConnectionId = "")
        {

            var qbicId = qbic.Id;

            var newUserGuests = new List<ApplicationUser>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get user", null, null, qbic, qbicleUser, domainId, curentUserId);

                var ur = new UserRules(dbContext);
                var currentUser = ur.GetUser(curentUserId, 0);
                var dRules = new DomainRules(dbContext);

                var membersAdd = new List<string>();
                var membersRemove = new List<string>();

                if (qbic.Id == 0)
                {
                    if (!string.IsNullOrEmpty(qbic.LogoUri))
                        new Azure.AzureStorageRules(dbContext).ProcessingMediaS3(qbic.LogoUri);

                    qbic.StartedBy = currentUser;

                    qbic.StartedDate = DateTime.UtcNow;
                    qbic.LastUpdated = DateTime.UtcNow;
                    qbic.OwnedBy = currentUser;
                    qbic.Manager = currentUser;
                    var domain = dRules.GetDomainById(domainId);

                    qbic.Domain = domain;

                    if (qbicleUser != null)
                        foreach (var userId in qbicleUser)
                        {
                            var member = GetUser(userId);
                            if (member != currentUser)
                                qbic.Members.Add(member);
                        }

                    qbic.Members.Add(currentUser);

                    if (guestsQbicle != null)
                    {
                        var currentDomainUsers = dRules.GetUsersByDomainId(domainId);
                        foreach (var g in guestsQbicle)
                            if (HelperClass.IsValidEmail(g))
                            {
                                var u = ur.GetUserByUserName(g);
                                // Can the login name match the email?
                                // when add new guest username set defaut = email
                                if (u != null)
                                {
                                    if (currentDomainUsers.Any(x => x == u))
                                    {
                                        qbic.Members.Add(u);
                                    }
                                    else
                                    {

                                        dRules.AddGuestToDomain(qbic.Domain, u);
                                    }
                                }
                                else //create new user as guest
                                {
                                    var newUser = ur.CreateUserInvitedByEmail(g);

                                    dRules.AddGuestToDomain(qbic.Domain, newUser);
                                    newUserGuests.Add(newUser);
                                }
                            }
                            else
                            {
                                var user = ur.GetUserByEmail(g);
                                if (user != null)
                                {
                                    if (currentDomainUsers.Any(x => x == user))
                                    {
                                        qbic.Members.Add(user);
                                    }
                                    else
                                    {

                                        dRules.AddGuestToDomain(qbic.Domain, user);
                                    }
                                }
                            }
                    }

                    qbic.Domain.Account.QbiclesCount++;

                    // Add General media folder default 

                    dbContext.Qbicles.Add(qbic);
                    dbContext.Entry(qbic).State = EntityState.Added;

                    dbContext.SaveChanges();
                }
                else
                {
                    //get cube current edit
                    var cube = GetQbicleById(qbic.Id);

                    if (!string.IsNullOrEmpty(qbic.LogoUri) && cube.LogoUri != qbic.LogoUri)
                        new Azure.AzureStorageRules(dbContext).ProcessingMediaS3(qbic.LogoUri);

                    //var cubeDomain = cube.Domain;
                    //update entity
                    if (qbic.LogoUri != null)
                        cube.LogoUri = qbic.LogoUri;
                    cube.Name = qbic.Name;
                    cube.IsUsingApprovals = qbic.IsUsingApprovals;
                    cube.Description = qbic.Description;

                    cube.LastUpdated = DateTime.UtcNow;

                    //cube.Manager = null;
                    //cube.Manager = dbContext.QbicleUser.FirstOrDefault(u => u.Id == managerId);

                    membersRemove = cube.Members.Select(u => u.Id).Except(qbicleUser).ToList();
                    membersRemove.ForEach(u =>
                    {
                        cube.Members.Remove(GetUser(u));
                    });


                    membersAdd = qbicleUser.Except(cube.Members.Select(u => u.Id)).ToList();
                    membersAdd.ForEach(u =>
                    {
                        cube.Members.Add(GetUser(u));
                    });

                    ////current user always is a memeber
                    //cube.Members.Add(currentUser);


                    if (dbContext.Entry(cube).State == EntityState.Detached)
                        dbContext.Qbicles.Attach(cube);
                    dbContext.Entry(cube).State = EntityState.Modified;

                    dbContext.SaveChanges();
                }


                //ADD TOPPIC "General" FOR QBICLE
                if (qbicId == 0)
                {
                    var topicName = HelperClass.GeneralName;
                    new TopicRules(dbContext).SaveTopic(qbic.Id, topicName);
                    new MediaFolderRules(dbContext).InsertMediaFolder(HelperClass.GeneralName, curentUserId, qbic.Id);
                }
                //END ADD TOPIC


                if (membersRemove.Count > 0)
                    AddRemoveQbicMembers(domainId, curentUserId, membersRemove, qbicId, NotificationEventEnum.RemoveUserOutOfQbicle);

                if (membersAdd.Count > 0)
                    AddRemoveQbicMembers(domainId, curentUserId, membersAdd, qbicId, NotificationEventEnum.AddUserToQbicle);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = qbic.Id,
                    EventNotify = qbicId == 0 ? NotificationEventEnum.QbicleCreation : NotificationEventEnum.QbicleUpdate,
                    AppendToPageName = ApplicationPageName.Domain,
                    CreatedByName = currentUser.GetFullName(),
                    ReminderMinutes = 0,
                    CreatedById = curentUserId
                };
                new NotificationRules(dbContext).Notification2Qbicle(activityNotification);

                ChangeQbicleManager(qbic.Id, managerId);

                return new ReturnJsonModel
                {
                    actionVal = qbic.Id,
                    result = true,
                    Object = newUserGuests
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbic, qbicleUser, guestsQbicle, domainId, curentUserId);
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ex.Message,
                    actionVal = 2
                };
            }

        }

        private void ChangeQbicleManager(int qbicleId, string managerId)
        {
            var qbicle = dbContext.Qbicles.FirstOrDefault(u => u.Id == qbicleId);
            qbicle.Manager = dbContext.QbicleUser.FirstOrDefault(u => u.Id == managerId);
            dbContext.SaveChanges();
        }

        public bool CloseQbicle(int closeQbicleId, string curentUserId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Close qbicle", null, null, closeQbicleId, curentUserId);

                var user = GetUser(curentUserId);
                var qbic = dbContext.Qbicles.Find(closeQbicleId);
                //var domain = qbic.Domain; // reload Domain Lazy loading
                if (qbic != null)
                {
                    qbic.LastUpdated = DateTime.UtcNow;
                    qbic.ClosedDate = DateTime.UtcNow;
                    qbic.ClosedBy = user;
                    //var start = qbic.StartedBy;
                    //var ow = qbic.OwnedBy;
                    if (dbContext.Entry(qbic).State == EntityState.Detached)
                        dbContext.Qbicles.Attach(qbic);
                    dbContext.Entry(qbic).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    var nRule = new NotificationRules(dbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = qbic.Id,
                        EventNotify = NotificationEventEnum.QbicleUpdate,
                        AppendToPageName = ApplicationPageName.Domain,
                        CreatedByName = user.GetFullName(),
                        ReminderMinutes = 0,
                        CreatedById = user.Id
                    };
                    nRule.Notification2Qbicle(activityNotification);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, closeQbicleId, curentUserId);
                return false;
            }
        }

        public bool ReOpenQbicle(int openQbicleId, ApplicationUser user, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Close qbicle", user.Id, null, openQbicleId, user);

                var qbic = dbContext.Qbicles.Find(openQbicleId);
                //var domain = qbic.Domain; // reload Domain Lazy loading
                if (qbic != null)
                {
                    qbic.LastUpdated = DateTime.UtcNow;
                    qbic.ClosedDate = null;
                    qbic.ClosedBy = null;
                    //var start = qbic.StartedBy;
                    //var ow = qbic.OwnedBy;
                    if (dbContext.Entry(qbic).State == EntityState.Detached)
                        dbContext.Qbicles.Attach(qbic);
                    dbContext.Entry(qbic).State = EntityState.Modified;
                    dbContext.SaveChanges();
                    var nRule = new NotificationRules(dbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = qbic.Id,
                        EventNotify = NotificationEventEnum.QbicleUpdate,
                        AppendToPageName = ApplicationPageName.Domain,
                        CreatedByName = user.GetFullName(),
                        ReminderMinutes = 0,
                        CreatedById = user.Id
                    };
                    nRule.Notification2Qbicle(activityNotification);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id, openQbicleId, user);
                return false;
            }
        }

        /// <summary>
        ///     Get one Qbicle first or default
        /// </summary>
        public Qbicle GetQbicleFirstDefault()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get first qbicle", null, null);

                return dbContext.Qbicles.FirstOrDefault();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return null;
            }
        }

        /// <summary>
        ///     Get the a Qbicle by Qbicle Id
        /// </summary>
        /// <param name="id">int Qbicle Id</param>
        /// <returns></returns>
        public Qbicle GetQbicleById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get qbicle by id", null, null, id);

                return dbContext.Qbicles.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }

        public bool ShowOrHideQbicle(int id, bool isHidden)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show or hide qbile", null, null, id, isHidden);
                var qbicle = dbContext.Qbicles.Find(id);
                qbicle.IsHidden = isHidden;
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, isHidden);
                return false;
            }
        }

        public Qbicle GetQbicleNoTrackingById(int id)
        {
            var proxyCreation = dbContext.Configuration.ProxyCreationEnabled;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get qbicle no tracking by id", null, null, id);

                dbContext.Configuration.ProxyCreationEnabled = false;
                var cube = dbContext.Qbicles.AsNoTracking().FirstOrDefault(c => c.Id == id);
                return cube;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
            finally
            {
                dbContext.Configuration.ProxyCreationEnabled = proxyCreation;
            }
        }

        /// <summary>
        ///     Get list dates activity for Qbicle view on the tab Acitivity panel
        ///     this method will change when have logic of alert,media,...
        /// </summary>
        /// <param name="qbicleTaks"></param>
        /// <param name="qbicleDiscussions"></param>
        /// <param name="qbicleAlerts"></param>
        /// <param name="qbicleEvents"></param>
        /// <param name="qbicleMedias"></param>
        /// <param name="approvalsRequest"></param>
        /// <returns></returns>
        public IEnumerable<DateTime> GetAcivitiesDate(List<QbicleTask> qbicleTaks,
            List<QbicleDiscussion> qbicleDiscussions,
            List<QbicleAlert> qbicleAlerts, List<QbicleEvent> qbicleEvents, List<QbicleMedia> qbicleMedias,
            List<ApprovalReq> approvalsRequest)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activities date", null, null, qbicleTaks, qbicleDiscussions, qbicleAlerts, qbicleEvents, qbicleMedias, approvalsRequest);

                // get date group
                //1. Task date
                var taskDates = from t in qbicleTaks select t.TimeLineDate.Date;
                //2. Discussion date
                var discussionDates = from d in qbicleDiscussions select d.TimeLineDate.Date;

                var alertDates = from t in qbicleAlerts select t.TimeLineDate.Date;
                var mediaDates = from t in qbicleMedias select t.TimeLineDate.Date;
                var eventDates = from t in qbicleEvents select t.TimeLineDate.Date;
                var approvalDates = from t in approvalsRequest select t.TimeLineDate.Date;

                var disDates = discussionDates.Union(taskDates).ToList()
                    .Union(alertDates).Union(eventDates).Union(mediaDates).Union(approvalDates).Distinct();
                return disDates;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleTaks, qbicleDiscussions, qbicleAlerts, qbicleEvents, qbicleMedias, approvalsRequest);
                return new List<DateTime>();
            }
        }

        /// <summary>
        ///     Get Count Of Activities Date
        /// </summary>
        /// <param name="activitiesDate">IEnumerable<DateTime />
        /// </param>
        /// <returns></returns>
        public int CountActivitiesDate(IEnumerable<DateTime> activitiesDate)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activities date", null, null, activitiesDate);

                return activitiesDate.Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activitiesDate);
                return 0;
            }

        }

        /// <summary>
        ///     Re-Open Qbicle closed
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="openQbicleId"></param>
        /// <param name="curentUserId"></param>
        /// <returns></returns>
        public bool ReOpenQbicle(int openQbicleId, string curentUserId)
        {
            if (curentUserId == null) throw new ArgumentNullException(nameof(curentUserId));
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Reopen qbicle", null, null, openQbicleId, curentUserId);

                var user = GetUser(curentUserId);
                return ReOpenQbicle(openQbicleId, user);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, openQbicleId, curentUserId);
                return false;
            }
        }

        /// <summary>
        ///     Filter Qbicles by option search
        /// </summary>
        /// <param name="user"></param>
        /// <param name="qbicleParameters">QbicleSearchParameter class</param>
        /// <param name="domainId"></param>
        /// <returns></returns>
        public List<Qbicle> FilterQbicle(QbicleSearchParameter qbicleParameters)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Filter qbicle", qbicleParameters.UserId, null, qbicleParameters);

                var user = GetUser(qbicleParameters.UserId);

                var lstQbicle = dbContext.Qbicles.Where(d => d.Domain.Id == qbicleParameters.DomainId);
                if (!qbicleParameters.IsShowHidden)
                {
                    lstQbicle = lstQbicle.Where(d => !d.IsHidden);
                }
                var currentDomain = dbContext.Domains.Find(qbicleParameters.DomainId);
                if (currentDomain != null && !currentDomain.Administrators.Any(u => u.Id == user.Id))
                {
                    lstQbicle = lstQbicle.Where(sc => (sc.Manager.Id == user.Id || sc.Members.Any(s => s.Id == user.Id)));
                }
                var cubeStr = qbicleParameters.Name?.Replace("₩", " ").ToUpper();
                bool
                    cOpen = qbicleParameters.Open,
                    cClosed = qbicleParameters.Closed;

                if (!string.IsNullOrEmpty(cubeStr)) lstQbicle = lstQbicle.Where(c => c.Name.ToUpper().Contains(cubeStr));
                if (cOpen || cClosed)
                {
                    if (!cClosed)
                        lstQbicle = lstQbicle.Where(c => c.ClosedDate == null);
                    else if (!cOpen)
                        lstQbicle = lstQbicle.Where(c => c.ClosedDate != null);
                }
                // Topics
                if (qbicleParameters.Topics != null && qbicleParameters.Topics.ToList().TrueForAll(c => c != ""))
                {
                    var topicQbcileIds = dbContext.Topics.Where(x => qbicleParameters.Topics.ToList().Contains(x.Name))
                        .Select(c => c.Qbicle).Select(q => q.Id);
                    lstQbicle = lstQbicle.Where(c => topicQbcileIds.Contains(c.Id));
                }

                // member
                if (qbicleParameters.Peoples != null && qbicleParameters.Peoples.ToList().TrueForAll(c => c != ""))
                {
                    var users = dbContext.QbicleUser.Where(u => qbicleParameters.Peoples.ToList().Contains(u.Id))
                        .Select(u => u.Id).ToList();
                    lstQbicle = lstQbicle.Where(p => p.Members.Any(x => users.Contains(x.Id)));
                }

                //order by start date
                lstQbicle = lstQbicle.OrderByDescending(d => d.LastUpdated);

                var cubes = lstQbicle.BusinessMapping(user.Timezone);

                switch (qbicleParameters.Order)
                {
                    case QbicleOrder.NameAsc:
                        cubes = cubes.OrderBy(o => o.Name);
                        break;
                    case QbicleOrder.NameDesc:
                        cubes = cubes.OrderByDescending(o => o.Name);
                        break;
                    case QbicleOrder.DateAsc:
                        cubes = cubes.OrderBy(o => o.LastUpdated);
                        break;
                    case QbicleOrder.DateDesc:
                        cubes = cubes.OrderByDescending(o => o.LastUpdated);
                        break;
                }

                return cubes.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleParameters);
                return null;
            }
        }

        public CubeModel GetQbicleToEditView(int cubeId, string currentId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetQbicleToEditView", null, null, cubeId);

                var qbicle = GetQbicleById(cubeId);
                var members = "";
                foreach (var item in qbicle.Members)
                    members += item.Id + ",";
                members = members.TrimEnd(',');
                var guests = "";

                guests = guests.TrimEnd(',');
                //var app = new ApprovalsRules(dbContext).GetApprovalsByQbicleId(cubeId);
                var qManager = qbicle.Members.Select(s => new UserListModel
                {
                    DisplayName = s.DisplayUserName,
                    UserName = s.UserName,
                    ProfilePic = s.ProfilePic,
                    Forename = s.Forename,
                    Surname = s.Surname,
                    Id = s.Id
                }).ToList();
                var cube = new CubeModel
                {
                    QbicleKey = qbicle.Key,
                    Name = qbicle.Name,
                    Description = qbicle.Description,
                    DomainId = qbicle.Domain.Id,
                    LogoUri = qbicle.LogoUri,
                    CubeUser = members,
                    CubeGuest = guests,
                    Closed = qbicle.ClosedDate,
                    IsUsingApprovals = qbicle.IsUsingApprovals,
                    OwnedBy = qbicle.OwnedBy.Id,
                    QbicManager = qManager,
                    Manager = qbicle.Manager.Id,
                    IsMemberQbicle = qbicle.Members.Any(u => u.Id == currentId)

                };
                return cube;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId);
                return null;
            }

        }


        public QbicleActivity GetActivitiesByPostId(int postId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activities by post id", null, null, postId);

                var rs = dbContext.Activities.FirstOrDefault(x => x.Posts.Any(p => p.Id == postId));
                return rs;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, postId);
                return null;
            }
        }

        public QbicleStreamModel GetQbicleStreams(QbicleFillterModel fillterModel, string timeZone, string dateFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get qbicle streams", null, null, fillterModel, timeZone);


                QbicleStreamModel qbicleStream = new QbicleStreamModel();
                #region Map ActivityTypeEnum
                var activityTypes = new List<ActivityTypeEnum>();
                if (fillterModel.ActivityTypes != null)
                    foreach (var item in fillterModel.ActivityTypes)
                    {
                        switch (item)
                        {
                            case "Approvals":
                                activityTypes.Add(ActivityTypeEnum.ApprovalRequest);
                                activityTypes.Add(ActivityTypeEnum.ApprovalRequestApp);
                                break;
                            case "Tasks":
                                activityTypes.Add(ActivityTypeEnum.TaskActivity);
                                break;
                            case "Events":
                                activityTypes.Add(ActivityTypeEnum.EventActivity);
                                break;
                            case "Links":
                                activityTypes.Add(ActivityTypeEnum.Link);
                                break;
                            case "Media":
                                activityTypes.Add(ActivityTypeEnum.MediaActivity);
                                break;
                            case "Post":
                                activityTypes.Add(ActivityTypeEnum.PostActivity);
                                break;
                            case "Discussions":
                                activityTypes.Add(ActivityTypeEnum.DiscussionActivity);
                                break;
                            case "DiscussionsOrders":
                                activityTypes.Add(ActivityTypeEnum.DiscussionActivity);
                                qbicleStream.IsFilterDiscussionOrder = true;
                                break;
                            default:
                                break;
                        }
                    }
                #endregion

                //Get List Dates for QBICLE Stream
                #region Get List Dates for QBICLE Stream
                var _maxDate = DateTime.UtcNow.Date.AddDays(1);

                var query = dbContext.ViewLstDatesQbicles.Where(s => s.QbicleId == fillterModel.QbicleId && s.TimeLineDate < _maxDate);

                if (fillterModel.TopicIds != null && fillterModel.TopicIds.Count > 0)
                    query = query.Where(s => fillterModel.TopicIds.Any(t => t == s.TopicId));

                if (fillterModel.ActivityTypes != null && fillterModel.ActivityTypes.Count > 0)
                {
                    if (activityTypes.Any(t => t == ActivityTypeEnum.PostActivity))
                        query = query.Where(s => s.ActivityType == 99 || activityTypes.Any(t => (int)t == s.ActivityType));
                    else
                        query = query.Where(s => activityTypes.Any(t => (int)t == s.ActivityType));
                }
                if (fillterModel.Apps != null && fillterModel.Apps.Count > 0)
                    query = query.Where(s => fillterModel.Apps.Any(t => t == s.APP));//is ApprovalRequest|ApprovalRequestApp

                if (!string.IsNullOrEmpty(fillterModel.Daterange))
                {
                    DateTime startDate = DateTime.UtcNow;
                    DateTime endDate = DateTime.UtcNow;

                    if (!string.IsNullOrEmpty(dateFormat))
                    {
                        fillterModel.Daterange.ConvertDaterangeFormat(dateFormat, timeZone, out startDate, out endDate, endDateAddedType.day);
                        query = query.Where(s => s.TimeLineDate >= startDate && s.TimeLineDate < endDate);
                    }
                }
                var lstdates = query.Select(s => s.TimeLineDate).Distinct();
                #endregion
                qbicleStream.TotalCount = lstdates.Count();
                var _dates = lstdates.OrderByDescending(s => s).Skip(fillterModel.Size).Take(HelperClass.qbiclePageSize).ToList();
                var dates = new List<DatesQbicleStream>();
                foreach (var item in _dates)
                {
                    var dateQbicle = new DatesQbicleStream();

                    var _stardate = item;
                    var _endDate = _stardate.AddDays(1);

                    //Get List Qbicle Activities
                    var _activities = dbContext.ViewQbicleActivitiesActivities.Where(s => s.QbicleId == fillterModel.QbicleId
                        && s.TimeLineDate >= _stardate && s.TimeLineDate < _endDate).AsQueryable();
                    if (fillterModel.TopicIds != null && fillterModel.TopicIds.Count > 0)
                    {
                        _activities = _activities.Where(s => fillterModel.TopicIds.Any(t => t == s.TopicId));
                    }

                    if (fillterModel.ActivityTypes != null && fillterModel.ActivityTypes.Count > 0)
                    {
                        if (activityTypes.Any(t => t == ActivityTypeEnum.PostActivity))
                            _activities = _activities.Where(s => s.ActivityType == 99 || activityTypes.Any(t => (int)t == s.ActivityType));
                        else
                            _activities = _activities.Where(s => activityTypes.Any(t => t == (ActivityTypeEnum)s.ActivityType));
                    }
                    if (fillterModel.Apps != null && fillterModel.Apps.Count > 0)
                    {
                        _activities = from atv in _activities
                                      where
                                        fillterModel.Apps.Any(app => app == atv.APP.ToString())
                                      select atv;
                    }

                    var viewActivities = _activities.OrderByDescending(e => e.Id).ToList();
                    if (viewActivities.Any())
                    {
                        dateQbicle.Date = item;
                        dateQbicle.Activities = new List<object>();
                        foreach (var act in viewActivities)
                        {
                            switch (act.ActivityType)
                            {
                                case (int)ActivityTypeEnum.DiscussionActivity:
                                    var objAct = dbContext.Discussions.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.TaskActivity:
                                    var objAct1 = dbContext.QbicleTasks.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct1.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.AlertActivity:
                                    var objAct2 = dbContext.Alerts.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct2.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.EventActivity:
                                    var objAct3 = dbContext.Events.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct3.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.MediaActivity:
                                    var objAct4 = dbContext.Medias.FirstOrDefault(s => s.Id == act.Id && s.IsVisibleInQbicleDashboard);
                                    dateQbicle.Activities.Add(objAct4.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.PostActivity:
                                    var objAct5 = dbContext.HLSharedPosts.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct5.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.QbicleActivity:
                                    var objAct6 = dbContext.Activities.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct6.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.ApprovalRequest:
                                    var objAct7 = dbContext.ApprovalReqs.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct7.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.ApprovalActivity:
                                    var objAct8 = dbContext.ApprovalReqs.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct8.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.Domain:
                                    var objAct9 = dbContext.Activities.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct9.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.ApprovalRequestApp:
                                    var objAct10 = dbContext.ApprovalReqs.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct10.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.Link:
                                    var objAct11 = dbContext.QbicleLinks.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct11.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.RemoveQueue:
                                    var objAct12 = dbContext.Activities.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct12.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.SharedHLPost:
                                    var objAct13 = dbContext.HLSharedPosts.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct13.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.SharedPromotion:
                                    var objAct14 = dbContext.SharedPromotions.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct14.BusinessMapping(timeZone));
                                    break;
                                case (int)ActivityTypeEnum.OrderCancellation:
                                    var objAct15 = dbContext.Activities.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct15.BusinessMapping(timeZone));
                                    break;
                                case 99://Post
                                    var objAct16 = dbContext.Posts.FirstOrDefault(s => s.Id == act.Id);
                                    dateQbicle.Activities.Add(objAct16.BusinessMapping(timeZone));
                                    break;
                                default:
                                    break;
                            }
                        }
                        dateQbicle.Activities = new List<IEnumerable<dynamic>> { dateQbicle.Activities }
                                              .SelectMany(x => x).OrderByDescending(x => x.TimeLineDate).ToList();

                        dates.Add(dateQbicle);
                    }
                }
                qbicleStream.Dates = dates;
                qbicleStream.Pinneds = (from pin in dbContext.MyPins
                                        join desk in dbContext.MyDesks on pin.Desk.Id equals desk.Id
                                        where desk.Owner.Id == fillterModel.UserId && pin.PinnedActivity != null
                                        select pin.PinnedActivity.Id).ToList();
                return qbicleStream;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, fillterModel, timeZone);
                return new QbicleStreamModel();
            }
        }


        public IEnumerable<DateTime> LoadMoreQbicleActivities(int cubeId, int size,
            ref List<QbiclePost> topicPost, ref List<QbicleTask> taks,
            ref List<QbicleAlert> alerts, ref List<QbicleMedia> medias, ref List<QbicleEvent> events,
            ref int AcivitiesDateCount, ref List<ApprovalReq> approvals, ref List<QbicleLink> links,
            ref List<QbicleDiscussion> discussions,
            string CurrentTimeZone, int topicId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load more qbicle activities", null, null, cubeId, size, topicPost, taks, alerts, medias, events,
                       AcivitiesDateCount, approvals, links, discussions, CurrentTimeZone, topicId);

                var postRule = new PostsRules(dbContext);
                var tkRule = new TasksRules(dbContext);
                var alRule = new AlertsRules(dbContext);
                var meRule = new MediasRules(dbContext);
                var evRule = new EventsRules(dbContext);
                var appRule = new ApprovalsRules(dbContext);
                var lkRule = new LinksRules(dbContext);
                var dRule = new DiscussionsRules(dbContext);
                IEnumerable<DateTime> activitiesDate = null;

                var qbicletopicPost = postRule.GetPosts(cubeId, topicId).BusinessMapping(CurrentTimeZone);
                var qbicleTaks = tkRule.GetTasksByQbicleId(cubeId, topicId).BusinessMapping(CurrentTimeZone);
                var qbicleAlerts = alRule.GetAlertsByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                    .BusinessMapping(CurrentTimeZone);
                var qbicleMedias = meRule.GetMediasByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                    .BusinessMapping(CurrentTimeZone);
                var qbicleEvents = evRule.GetEventsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                    .BusinessMapping(CurrentTimeZone);
                var approvalRequest = appRule.GetApprovalsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                    .BusinessMapping(CurrentTimeZone);
                var qbiclelinks = lkRule.GetLinksOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                    .BusinessMapping(CurrentTimeZone);
                var qbicleDiscussions = dRule.GetDiscussionsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                    .BusinessMapping(CurrentTimeZone);
                //get date group
                //1. Task date
                var today = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 23, 59, 0);
                var taskDates = from t in qbicleTaks where t.ProgrammedStart <= today select t.ProgrammedStart.Value.Date;

                var topicPostDates = from d in qbicletopicPost select d.TimeLineDate.Date;

                var alertDates = from t in qbicleAlerts select t.TimeLineDate.Date;
                var mediaDates = from t in qbicleMedias select t.TimeLineDate.Date;
                var eventDates = from t in qbicleEvents where t.ProgrammedStart <= today select t.ProgrammedStart.Value.Date;
                var appDates = from t in approvalRequest select t.TimeLineDate.Date;
                var linkDates = from t in qbiclelinks select t.TimeLineDate.Date;
                var discussionDates = from t in qbicleDiscussions select t.TimeLineDate.Date;

                var dates = topicPostDates.Union(taskDates).Union(alertDates).Union(mediaDates).Union(eventDates)
                    .Union(appDates).Union(linkDates).Union(discussionDates);
                AcivitiesDateCount = dates.Count();

                dates = dates.OrderByDescending(d => d.Date.Date);
                topicPost = qbicletopicPost.Where(o => dates.Contains(o.TimeLineDate.Date)).ToList();
                taks = qbicleTaks.Where(o => dates.Any(a => a == o.ProgrammedStart)).ToList();
                alerts = qbicleAlerts.Where(o => dates.Contains(o.TimeLineDate.Date)).ToList();
                medias = qbicleMedias.Where(o => dates.Contains(o.TimeLineDate.Date)).ToList();
                events = qbicleEvents.Where(o => dates.Any(a => a == o.ProgrammedStart)).ToList();
                approvals = approvalRequest.Where(o => dates.Contains(o.TimeLineDate.Date)).ToList();
                links = qbiclelinks.Where(o => dates.Contains(o.TimeLineDate.Date)).ToList();
                discussions = qbicleDiscussions.Where(o => dates.Contains(o.TimeLineDate.Date)).ToList();
                activitiesDate = dates.Skip(size).Take(HelperClass.qbiclePageSize);
                return activitiesDate;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, size, topicPost, taks, alerts, medias, events,
                       AcivitiesDateCount, approvals, links, discussions, CurrentTimeZone, topicId);
                return new List<DateTime>();
            }

        }
        /// <summary>
        ///  Join activities (in the dates list shown) to a list and order by date
        /// </summary>
        /// <param name="activitiesDate"></param>
        /// <param name="posts"></param>
        /// <param name="taks"></param>
        /// <param name="alerts"></param>
        /// <param name="medias"></param>
        /// <param name="events"></param>
        /// <param name="approvals"></param>
        /// <param name="links"></param>
        /// <param name="discussions"></param>
        /// <returns></returns>
        public List<dynamic> GetActivities(IEnumerable<DateTime> activitiesDate,
            List<QbiclePost> posts, List<QbicleTask> taks,
            List<QbicleAlert> alerts, List<QbicleMedia> medias, List<QbicleEvent> events, List<ApprovalReq> approvals, List<QbicleLink> links, List<QbicleDiscussion> discussions)
        {
            var activities = new List<dynamic>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activities", null, null, activitiesDate, posts, taks, alerts, medias, events, approvals,
                        links, discussions);

                dbContext.Configuration.AutoDetectChangesEnabled = false;
                dbContext.Configuration.ValidateOnSaveEnabled = false;
                dbContext.Configuration.ProxyCreationEnabled = false;
                var dis = posts.Where(d => activitiesDate.Contains(d.TimeLineDate.Date)).ToList();
                var tk = taks.Where(d => activitiesDate.Any(a => a == d.ProgrammedStart)).ToList();
                var al = alerts.Where(d => activitiesDate.Contains(d.TimeLineDate.Date)).ToList();
                var med = medias.Where(d => activitiesDate.Contains(d.TimeLineDate.Date)).ToList();
                var ev = events.Where(d => activitiesDate.Any(a => a == d.ProgrammedStart)).ToList();
                var app = approvals.Where(x => activitiesDate.Contains(x.TimeLineDate.Date)).ToList();
                var lk = links.Where(x => activitiesDate.Contains(x.TimeLineDate.Date)).ToList();
                var dc = discussions.Where(x => activitiesDate.Contains(x.TimeLineDate.Date)).ToList();

                activities = new List<IEnumerable<dynamic>> { dis, tk, al, med, ev, app, lk, dc }
                    .SelectMany(x => x)
                    .OrderBy(x => x.TimeLineDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activitiesDate, posts, taks, alerts, medias, events, approvals, links, discussions);
            }
            finally
            {
                dbContext.Configuration.AutoDetectChangesEnabled = true;
                dbContext.Configuration.ValidateOnSaveEnabled = true;
                dbContext.Configuration.ProxyCreationEnabled = true;
            }

            return activities;
        }

        /// <summary>
        ///     get a activity by Id
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public QbicleActivity GetActivity(int activityId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activitie", null, null, activityId);

                return dbContext.Activities.FirstOrDefault(a => a.Id == activityId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activityId);
                return null;
            }

        }

        /// <summary>
        ///     Get list users associated with the Domain by domainId
        /// </summary>
        /// <param name="qbicleId"></param>
        /// <returns></returns>
        public List<ApplicationUser> GetUsersByQbicleId(int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get users by qbicle id", null, null, qbicleId);

                var cube = GetQbicleById(qbicleId);
                return cube.Members.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId);
                return new List<ApplicationUser>();
            }
        }
        public List<UserCustom> GetUsersCustomByQbicleId(int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get users custom by qbicle id", null, null, qbicleId);

                var cube = dbContext.Qbicles.Find(qbicleId);

                return cube.Members.Distinct().Select(c => new UserCustom
                {
                    Forename = c.Forename,
                    Surname = c.Surname,
                    UserName = c.UserName,
                    Id = c.Id,
                    GroupId = 1,
                    ProfilePic = c.ProfilePic
                }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId);
                return new List<UserCustom>();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cubeId"></param>
        /// <param name="size"></param>
        /// <param name="qbiclePost"></param>
        /// <param name="taks"></param>
        /// <param name="alerts"></param>
        /// <param name="medias"></param>
        /// <param name="events"></param>
        /// <param name="AcivitiesDateCount"></param>
        /// <param name="approvals"></param>
        /// <param name="links"></param>
        /// <param name="CurrentTimeZone"></param>
        /// <param name="activityFilters"></param>
        /// <param name="topicFilters"></param>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public IEnumerable<DateTime> LoadMoreQbicleActivitiesFilter(int cubeId, int size,
            ref List<QbiclePost> qbiclePost, ref List<QbicleTask> taks,
            ref List<QbicleAlert> alerts, ref List<QbicleMedia> medias, ref List<QbicleEvent> events,
            ref int AcivitiesDateCount, ref List<ApprovalReq> approvals, ref List<QbicleLink> links, string CurrentTimeZone,
            string[] activityFilters = null, string[] topicFilters = null, int topicId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load more qbicle activities filter", null, null, cubeId, size, qbiclePost, taks, alerts, medias,
                        events, AcivitiesDateCount, approvals, links, CurrentTimeZone, activityFilters, topicFilters, topicId);

                var pRule = new PostsRules(dbContext);
                var tkRule = new TasksRules(dbContext);
                var alRule = new AlertsRules(dbContext);
                var meRule = new MediasRules(dbContext);
                var evRule = new EventsRules(dbContext);
                var appRule = new ApprovalsRules(dbContext);
                var lkRule = new LinksRules(dbContext);
                IEnumerable<DateTime> activitiesDate = null;

                var qbicleTopicPosts = new List<QbiclePost>();
                var qbicleTaks = new List<QbicleTask>();
                var qbicleAlerts = new List<QbicleAlert>();
                var qbicleMedias = new List<QbicleMedia>();
                var qbicleEvents = new List<QbicleEvent>();
                var approvalRequest = new List<ApprovalReq>();
                var qbicleLinks = new List<QbicleLink>();

                if (activityFilters != null && topicFilters == null)
                {
                    var filterActivities = activityFilters
                        .Select(a => (Enums.QbicleModule)Enum.Parse(typeof(Enums.QbicleModule), a)).ToArray();

                    foreach (var activity in filterActivities)
                        switch (activity)
                        {
                            case Enums.QbicleModule.Post:
                                qbicleTopicPosts = pRule.GetPosts(cubeId).BusinessMapping(CurrentTimeZone);
                                break;
                            case Enums.QbicleModule.Tasks:
                                qbicleTaks = tkRule.GetTasksByQbicleId(cubeId).BusinessMapping(CurrentTimeZone);
                                break;
                            case Enums.QbicleModule.Alerts:
                                qbicleAlerts = alRule.GetAlertsByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                                    .BusinessMapping(CurrentTimeZone);
                                break;
                            case Enums.QbicleModule.Media:
                                qbicleMedias = meRule.GetMediasByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                                    .BusinessMapping(CurrentTimeZone);
                                break;
                            case Enums.QbicleModule.Events:
                                qbicleEvents = evRule.GetEventsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                                    .BusinessMapping(CurrentTimeZone);
                                break;
                            case Enums.QbicleModule.Approvals:
                                approvalRequest = appRule.GetApprovalsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                                    .BusinessMapping(CurrentTimeZone);
                                break;
                            case Enums.QbicleModule.Links:
                                qbicleLinks = lkRule.GetLinksOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                                    .BusinessMapping(CurrentTimeZone);
                                break;
                        }
                }
                else if (activityFilters == null && topicFilters != null)
                {
                    var filterTopics = topicFilters.Select(int.Parse).ToArray();
                    qbicleTopicPosts = pRule.GetPosts(cubeId).BusinessMapping(CurrentTimeZone)
                        .Where(x => filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                    qbicleTaks = tkRule.GetTasksByQbicleId(cubeId).BusinessMapping(CurrentTimeZone)
                        .Where(x => filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                    qbicleAlerts = alRule.GetAlertsByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                        .BusinessMapping(CurrentTimeZone)
                        .Where(x => filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                    qbicleMedias = meRule.GetMediasByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                        .BusinessMapping(CurrentTimeZone)
                        .Where(x => filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                    qbicleEvents = evRule.GetEventsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                        .BusinessMapping(CurrentTimeZone)
                        .Where(x => filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                    approvalRequest = appRule.GetApprovalsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                        .BusinessMapping(CurrentTimeZone)
                        .Where(x => filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                    qbicleLinks = lkRule.GetLinksOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                        .BusinessMapping(CurrentTimeZone)
                        .Where(x => filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                }
                else if (activityFilters != null)
                {
                    var filterActivities = activityFilters
                        .Select(a => (Enums.QbicleModule)Enum.Parse(typeof(Enums.QbicleModule), a)).ToArray();
                    var filterTopics = topicFilters.Select(int.Parse).ToArray();
                    foreach (var activity in filterActivities)
                        switch (activity)
                        {
                            case Enums.QbicleModule.Post:
                                qbicleTopicPosts = pRule.GetPosts(cubeId).BusinessMapping(CurrentTimeZone).Where(x =>
                                    filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                                break;
                            case Enums.QbicleModule.Tasks:
                                qbicleTaks = tkRule.GetTasksByQbicleId(cubeId).BusinessMapping(CurrentTimeZone).Where(x =>
                                    filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                                break;
                            case Enums.QbicleModule.Alerts:
                                qbicleAlerts = alRule.GetAlertsByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                                    .BusinessMapping(CurrentTimeZone).Where(x =>
                                        filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                                break;
                            case Enums.QbicleModule.Media:
                                qbicleMedias = meRule.GetMediasByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                                    .BusinessMapping(CurrentTimeZone).Where(x =>
                                        filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                                break;
                            case Enums.QbicleModule.Events:
                                qbicleEvents = evRule.GetEventsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                                    .BusinessMapping(CurrentTimeZone).Where(x =>
                                        filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                                break;
                            case Enums.QbicleModule.Approvals:
                                approvalRequest = appRule.GetApprovalsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                                    .BusinessMapping(CurrentTimeZone).Where(x =>
                                        filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                                break;
                            case Enums.QbicleModule.Links:
                                qbicleLinks = lkRule.GetLinksOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst)
                                    .BusinessMapping(CurrentTimeZone).Where(x =>
                                        filterTopics.Any(tp => x.Topic != null && x.Topic.Id == tp)).ToList();
                                break;
                        }
                }
                else
                {
                    qbicleTopicPosts = pRule.GetPosts(cubeId, topicId).BusinessMapping(CurrentTimeZone);
                    qbicleTaks = tkRule.GetTasksByQbicleId(cubeId, topicId).BusinessMapping(CurrentTimeZone);
                    qbicleAlerts = alRule.GetAlertsByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                        .BusinessMapping(CurrentTimeZone);
                    qbicleMedias = meRule.GetMediasByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                        .BusinessMapping(CurrentTimeZone);
                    qbicleEvents = evRule.GetEventsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                        .BusinessMapping(CurrentTimeZone);
                    approvalRequest = appRule.GetApprovalsOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                        .BusinessMapping(CurrentTimeZone);
                    qbicleLinks = lkRule.GetLinksOrderByQbicleId(cubeId, Enums.OrderByDate.NewestFirst, topicId)
                        .BusinessMapping(CurrentTimeZone);
                }

                //get date group
                var today = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 23, 59, 0);

                var taskDates = from t in qbicleTaks where t.ProgrammedStart <= today select t.ProgrammedStart.Value;
                var postDates = from t in qbicleTopicPosts select t.TimeLineDate.Date;
                var alertDates = from t in qbicleAlerts select t.TimeLineDate.Date;
                var mediaDates = from t in qbicleMedias select t.TimeLineDate.Date;
                var eventDates = from t in qbicleEvents where t.ProgrammedStart <= today select t.ProgrammedStart.Value;
                var appDates = from t in approvalRequest select t.TimeLineDate.Date;
                var lkDates = from t in qbicleLinks select t.TimeLineDate.Date;
                var disDates = taskDates.Union(alertDates).Union(mediaDates).Union(eventDates).Union(postDates)
                    .Union(appDates).Union(lkDates);
                AcivitiesDateCount = disDates.Count();

                disDates = disDates.OrderByDescending(d => d.Date.Date);

                qbiclePost = qbicleTopicPosts.Where(o => disDates.Contains(o.TimeLineDate.Date)).ToList();
                taks = qbicleTaks.Where(o => disDates.Any(a => a == o.ProgrammedStart)).ToList();
                alerts = qbicleAlerts.Where(p => disDates.Contains(p.TimeLineDate.Date)).ToList();
                medias = qbicleMedias.Where(p => disDates.Contains(p.TimeLineDate.Date)).ToList();
                events = qbicleEvents.Where(p => disDates.Any(a => a == p.ProgrammedStart)).ToList();
                approvals = approvalRequest.Where(p => disDates.Contains(p.TimeLineDate.Date)).ToList();
                links = qbicleLinks.Where(p => disDates.Contains(p.TimeLineDate.Date)).ToList();
                activitiesDate = disDates.Skip(size).Take(HelperClass.qbiclePageSize);
                return activitiesDate;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, size, qbiclePost, taks, alerts, medias,
                        events, AcivitiesDateCount, approvals, links, CurrentTimeZone, activityFilters, topicFilters, topicId);
                return new List<DateTime>();
            }

        }
        public List<CalendarColor> ActivitiesRecursExistListDate(int qbicleId, string currentTimeZone, int? year, int? month)
        {

            var dates = new List<DateTime>();
            var currentDate = DateTime.UtcNow;
            DateTime firstDate;
            if (year.HasValue && month.HasValue)
                firstDate = new DateTime(year.Value, month.Value, 1);
            else
                firstDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDate.AddMonths(1).AddDays(-1);
            for (var current = firstDate; current <= lastDayOfMonth; current = current.AddDays(1))
                dates.Add(current);
            for (var previous_week = firstDate.AddDays(-7); previous_week < firstDate; previous_week = previous_week.AddDays(1))
                dates.Add(previous_week);
            for (var next_week = lastDayOfMonth.AddDays(1); next_week < lastDayOfMonth.AddDays(7); next_week = next_week.AddDays(1))
                dates.Add(next_week);

            var objlst = new List<CalendarColor>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "ActivitiesRecursExistListDate", null, null, dates, qbicleId, currentTimeZone);

                var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                var activityTypeNotValid = new List<ActivityTypeEnum>() { ActivityTypeEnum.PostActivity, ActivityTypeEnum.Domain, ActivityTypeEnum.QbicleActivity };
                foreach (var item in dates)
                {
                    var startDateTime = DateTime.ParseExact(item.ToString("dd/MM/yyyy"), "dd/MM/yyyy", null);
                    var endDateTime = startDateTime.AddDays(1);
                    var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDateTime, tz);
                    var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDateTime, tz);
                    var existActivitesDate = dbContext.Activities.Any(s =>
                    s.Qbicle.Id == qbicleId
                    && !activityTypeNotValid.Any(t => t == s.ActivityType)
                    && s.IsVisibleInQbicleDashboard
                    && s.TimeLineDate >= startDateTimeUTC
                    && s.TimeLineDate < endDateTimeUTC);
                    var existTaskRecursDate = dbContext.QbicleTasks.Where(s =>
                    s.StartedDate >= startDateTimeUTC
                    && s.StartedDate < endDateTimeUTC
                    && s.Qbicle.Id == qbicleId).Select(s => s.isRecurs).ToList();
                    var existEventRecursDate = dbContext.Events.Where(s =>
                    s.TimeLineDate >= startDateTimeUTC
                    && s.TimeLineDate < endDateTimeUTC
                    && s.Qbicle.Id == qbicleId).Select(s => s.isRecurs).ToList();
                    var countTask = existTaskRecursDate.Count;
                    var countEvent = existEventRecursDate.Count;
                    if (existActivitesDate && (countTask == 0 && countEvent == 0))
                    {
                        objlst.Add(new CalendarColor { date = item.ToString("dd/MM/yyyy"), color = "<span class=\"dp-container\"><span class=\"dp-note\"></span></span>" });
                    }
                    else if (existActivitesDate && (countTask > 0 || countEvent > 0))
                    {
                        objlst.Add(new CalendarColor { date = item.ToString("dd/MM/yyyy"), color = "<span class=\"dp-container\"><span class=\"dp-note\"></span> <span class=\"dp-note-sm\"></span></span>" });
                    }
                    else if (countTask > 0 || countEvent > 0)
                    {
                        objlst.Add(new CalendarColor { date = item.ToString("dd/MM/yyyy"), color = "<span class=\"dp-container\"><span class=\"dp-note-sm\"></span></span>" });
                    }
                }
                return objlst;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, dates, qbicleId, currentTimeZone);
                return new List<CalendarColor>();
            }

        }
        public List<QbicleActivity> ActivitiesForCalendar(string CurrentTimeZone, int qbicleId, string type, string day, string keyword, string orderby, short[] types, int[] topics, string[] peoples, string[] apps, int pageSize, int pageIndex, ref int totalRecords, string formatDate)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get list activities for calendar", null, null, CurrentTimeZone, qbicleId, type, day, keyword,
                        orderby, types, topics, peoples, apps, pageSize, pageIndex, totalRecords, formatDate);

                IQueryable<QbicleActivity> query = dbContext.Activities;
                query = query.Where(s => s.Qbicle.Id == qbicleId);
                if (types != null && types.Length > 0)
                {
                    query = query.Where(s => types.Contains((short)s.ActivityType));
                }
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Name.Contains(keyword)
                    || (s.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity && dbContext.QbicleTasks.Any(t => t.Description.Contains(keyword) && t.Id == s.Id))
                    || (s.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity && dbContext.Events.Any(t => t.Description.Contains(keyword) && t.Id == s.Id))
                    || (s.ActivityType == QbicleActivity.ActivityTypeEnum.MediaActivity && dbContext.Medias.Any(t => t.Description.Contains(keyword) && t.Id == s.Id))
                    || (s.ActivityType == QbicleActivity.ActivityTypeEnum.Link && dbContext.QbicleLinks.Any(t => t.Description.Contains(keyword) && t.Id == s.Id))
                    || (s.ActivityType == QbicleActivity.ActivityTypeEnum.DiscussionActivity && dbContext.Discussions.Any(t => t.Summary.Contains(keyword) && t.Id == s.Id))
                    || (s.ActivityType == QbicleActivity.ActivityTypeEnum.AlertActivity && dbContext.Alerts.Any(t => t.Content.Contains(keyword) && t.Id == s.Id))
                    || (s.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequest && dbContext.ApprovalReqs.Any(t => t.Name.Contains(keyword) && t.Id == s.Id))
                    || (s.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequestApp && dbContext.ApprovalReqs.Any(t => t.Name.Contains(keyword) && t.Id == s.Id)));
                }
                if (topics != null && topics.Length > 0)
                {
                    query = query.Where(s => topics.Contains(s.Topic.Id));
                }
                if (type == "today")
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(CurrentTimeZone);
                    var startDateTime = string.IsNullOrEmpty(day) ? DateTime.UtcNow.Date : TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(day, formatDate, null), tz);
                    var endDateTime = startDateTime.AddDays(1);
                    query = query.Where(s => s.TimeLineDate >= startDateTime && s.TimeLineDate < endDateTime);
                }
                else if (type == "week")
                {
                    List<DateTime> lstThisWeek = HelperClass.GetListDateByThisWeek();
                    var firstDate = lstThisWeek.FirstOrDefault();
                    var lastDate = lstThisWeek.LastOrDefault().AddDays(1);
                    query = query.Where(s => s.TimeLineDate >= firstDate && s.TimeLineDate < lastDate);
                }
                else
                {
                    List<DateTime> lstThisMonth = HelperClass.GetListDateByThisMonth();
                    var firstDate = lstThisMonth.FirstOrDefault();
                    var lastDate = lstThisMonth.LastOrDefault().AddDays(1);
                    query = query.Where(s => s.TimeLineDate >= firstDate && s.TimeLineDate < lastDate);
                }
                if (peoples != null && peoples.Length > 0)
                {
                    query = query.Where(s => s.AssociatedSet.Peoples.Any(p => peoples.Contains(p.User.Id)));
                }
                if (apps != null && apps.Length > 0)
                {
                    query = query.Where(s => apps.Any(t => (s.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequest || s.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequestApp)
                        && s.App.ToString() == t));

                }
                totalRecords = query.Count();
                if (orderby == "TimelineDate asc")
                    query = query.OrderBy(s => s.TimeLineDate);
                else if (orderby == "TimelineDate desc")
                    query = query.OrderByDescending(s => s.TimeLineDate);
                else
                    query = query.OrderBy(s => s.isComplete);

                return query.Skip(pageIndex * pageSize).Take(pageSize).ToList().BusinessMapping(CurrentTimeZone);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, CurrentTimeZone, qbicleId, type, day, keyword,
                        orderby, types, topics, peoples, apps, pageSize, pageIndex, totalRecords, formatDate);
                return null;
            }

        }
        public void StoredUiSettings(List<UiSetting> uiSettings)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Store UI setting", null, null, uiSettings);

                foreach (var item in uiSettings)
                {
                    var ust = dbContext.UiSettings.FirstOrDefault(s => s.CurrentPage == item.CurrentPage && s.CurrentUser.Id == item.CurrentUser.Id && s.Key == item.Key);
                    if (ust != null)
                    {
                        ust.Value = item.Value;
                        if (dbContext.Entry(ust).State == EntityState.Detached)
                            dbContext.UiSettings.Attach(ust);
                        dbContext.Entry(ust).State = EntityState.Modified;
                    }
                    else
                    {
                        dbContext.UiSettings.Add(item);
                        dbContext.Entry(item).State = EntityState.Added;
                    }
                }
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, uiSettings);
            }
        }
        public List<UiSetting> LoadUiSettings(string currentPage, string uiD)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load UI setting", null, null, currentPage, uiD);

                return dbContext.UiSettings.Where(s => s.CurrentPage == currentPage && s.CurrentUser.Id == uiD).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentPage, uiD);
                return null;
            }
        }
        public List<dynamic> ActivitiesListDateDotMyDesk(List<DateTime> dates, string currentTimeZone, string userId)
        {
            List<dynamic> objlst = new List<dynamic>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "ActivitiesListDateDotMyDesk", null, null, dates, currentTimeZone, userId);


                var tz = TimeZoneInfo.FindSystemTimeZoneById(currentTimeZone);
                foreach (var item in dates)
                {
                    var startDateTime = DateTime.ParseExact(item.ToString("dd/MM/yyyy"), "dd/MM/yyyy", null);
                    var endDateTime = startDateTime.AddDays(1);
                    var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDateTime, tz);
                    var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDateTime, tz);
                    var existActivitesDate = dbContext.Activities.Any(s =>
                    (s.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity
                    || s.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity
                    )
                    && (s.ProgrammedEnd.Value >= startDateTimeUTC
                         && s.ProgrammedEnd.Value < endDateTimeUTC
                         && (s.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity
                         || s.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity)
                        )
                    && s.IsVisibleInQbicleDashboard
                    && (s.ActivityMembers.Any(x => x.Id == userId) || s.StartedBy.Id == userId)
                    );
                    var existTaskRecursDate = dbContext.QbicleTasks.Where(s =>
                    (s.ProgrammedEnd.Value >= startDateTimeUTC
                         && s.ProgrammedEnd.Value < endDateTimeUTC
                    )
                    && s.IsVisibleInQbicleDashboard
                    && (s.ActivityMembers.Any(x => x.Id == userId) || s.StartedBy.Id == userId)
                    && !s.isRecurs
                    ).Any();
                    var existEventRecursDate = dbContext.Events.Where(s =>
                    (s.ProgrammedEnd.Value >= startDateTimeUTC
                         && s.ProgrammedEnd.Value < endDateTimeUTC
                    )
                    && s.IsVisibleInQbicleDashboard
                    && (s.ActivityMembers.Any(x => x.Id == userId) || s.StartedBy.Id == userId)
                    && !s.isRecurs
                    ).Any();
                    if (existActivitesDate && !existTaskRecursDate && !existEventRecursDate)
                    {
                        objlst.Add(new { date = item.ToString("dd/MM/yyyy"), color = "dp-note-gray" });
                    }
                    else if (existActivitesDate)
                    {
                        objlst.Add(new { date = item.ToString("dd/MM/yyyy"), color = "dp-note" });
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, dates, currentTimeZone, userId);
            }
            return objlst;
        }
        public List<Qbicle> GetQbiclesForProfileByUserId(string currentUserId, string viewUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load UI setting", null, null, currentUserId, viewUserId);
                return dbContext.Qbicles.Where(q => q.Members.Any(s => s.Id == currentUserId) && q.Members.Any(s => s.Id == viewUserId)).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId, viewUserId);
                return null;
            }
        }

        public PaginationResponse GetQbiclesForProfileByUserIdPagination(PaginationRequest request, string currentUserId, string viewUserId)
        {
            PaginationResponse response = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load shared QBicles", null, null, currentUserId, viewUserId);


                var qU1 = dbContext.QbicleUser.Find(currentUserId).Qbicles;
                var qU2 = dbContext.QbicleUser.Find(viewUserId).Qbicles;
                var qbicles = qU1.Intersect(qU2);

                qbicles = qbicles.Where(e => !dbContext.CQbicles.Any(c => c.Id == e.Id));

                qbicles = qbicles.Where(e => !dbContext.B2BQbicles.Any(c => c.Id == e.Id));

                response.totalNumber = qbicles.Count();
                var lstQbicles = qbicles.OrderBy(p => p.Name).Skip((request.pageNumber - 1) * request.pageSize).Take(request.pageSize).ToList();
                var lstQbicleModels = new List<UserProfileSharedQbicleModel>();
                lstQbicles.ForEach(p =>
                {
                    var _domain = p.Domain;
                    var qModel = new UserProfileSharedQbicleModel()
                    {
                        LogoUri = p.LogoUri.ToDocumentUri().ToString(),
                        QbicleKey = p.Key,
                        QbicleName = p.Name,
                        DomainName = _domain.Name,
                    };
                    if ((_domain.OwnedBy != null && _domain.OwnedBy.Id == currentUserId) || _domain.Administrators.Any(s => s.Id == currentUserId))
                    {
                        qModel.DomainOwner = "(My Domain)";
                    }
                    else if (_domain.OwnedBy != null && _domain.OwnedBy.Id == viewUserId)
                    {
                        qModel.DomainOwner = _domain.OwnedBy != null ? $"({_domain.OwnedBy.Forename} Domain)" : "";
                    }

                    lstQbicleModels.Add(qModel);
                });

                response.items = lstQbicleModels;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentUserId, viewUserId);
                response.items = new List<UserProfileSharedQbicleModel>();
            }
            return response;
        }

        public void RemoveMemberQbicles(ApplicationUser removeUser, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Remove member from all B2CQbicle in Domain", null, null, removeUser, domainId);

                var qbicles = dbContext.Qbicles.Where(s => s.Domain.Id == domainId && s.Members.Any(u => u.Id == removeUser.Id)).ToList();
                if (qbicles != null && qbicles.Any())
                    foreach (var qbicle in qbicles)
                    {
                        qbicle.Members.Remove(removeUser);
                    }
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, removeUser, domainId);
            }

        }


        public ReturnJsonModel AddRemoveQbicMembers(int domainId, string currentUserId, List<string> userIds, int cubeId, NotificationEventEnum eventNotification, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add Removed User From Qbicle", currentUserId, null, domainId, currentUserId, userIds);

                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var rules = new NotificationRules(dbContext);
                userIds.ForEach(userId =>
                {
                    rules.Notification2UserCreateRemoveFromDomain(new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        DomainId = domainId,
                        QbicleId = cubeId,
                        EventNotify = eventNotification,//NotificationEventEnum.RemoveUserOutOfQbicle,
                        AppendToPageName = ApplicationPageName.Domain,
                        CreatedByName = currentUser.GetFullName(),
                        CreatedById = currentUserId,
                        ObjectById = userId,
                        ReminderMinutes = 0
                    });
                });


                return new ReturnJsonModel { result = true };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, domainId, currentUserId, userIds);
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ex.Message
                };
            }


        }

        public ReturnJsonModel RemoveQbicMember(int domainId, string currentUserId, string removedUserId, int cubeId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "RemovedUserFromDomain", currentUserId, null, domainId, currentUserId, removedUserId);

                var qbicle = dbContext.Qbicles.Find(cubeId);

                var removedUser = dbContext.QbicleUser.Find(removedUserId);
                qbicle.Members.Remove(removedUser);
                dbContext.SaveChanges();

                var currentUser = dbContext.QbicleUser.Find(currentUserId);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    DomainId = domainId,
                    QbicleId = cubeId,
                    EventNotify = NotificationEventEnum.RemoveUserOutOfQbicle,
                    AppendToPageName = ApplicationPageName.Domain,
                    CreatedByName = currentUser.GetFullName(),
                    CreatedById = currentUserId,
                    ObjectById = removedUserId,
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2UserCreateRemoveFromDomain(activityNotification);

                return new ReturnJsonModel { result = true };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, domainId, currentUserId, removedUserId);
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ex.Message
                };
            }


        }

        public ReturnJsonModel AddMemberToQbicle(int domainId, string currentUserId, string invitedUserId, int cubeId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "RemovedUserFromDomain", currentUserId, null, domainId, currentUserId, invitedUserId);

                var qbicle = dbContext.Qbicles.Find(cubeId);
                var inviteUser = dbContext.QbicleUser.Find(invitedUserId);
                if (qbicle.Members.Any(u => u.Id == invitedUserId))
                    return new ReturnJsonModel
                    {
                        msg = $"{inviteUser.GetFullName()} existing in current qbicle " + qbicle.Name,
                        result = false
                    };


                qbicle.Members.Add(inviteUser);
                dbContext.SaveChanges();

                var currentUser = dbContext.QbicleUser.Find(currentUserId);


                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    DomainId = domainId,
                    QbicleId = cubeId,
                    EventNotify = NotificationEventEnum.AddUserToQbicle,
                    AppendToPageName = ApplicationPageName.Domain,
                    CreatedByName = currentUser.GetFullName(),
                    CreatedById = currentUserId,
                    ObjectById = invitedUserId,
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2UserCreateRemoveFromDomain(activityNotification);

                return new ReturnJsonModel { result = true };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, domainId, currentUserId, invitedUserId);
                return new ReturnJsonModel
                {
                    result = false,
                    msg = ex.Message
                };
            }


        }

        public ReturnJsonModel CheckActivityAccibility(int activityId, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, activityId, userId);

                var activity = dbContext.Activities.Find(activityId);
                var isAccessable = false;
                if (activity != null && activity.Qbicle != null)
                {
                    isAccessable = activity.Qbicle.Members.Any(u => u.Id == userId);
                }
                return new ReturnJsonModel() { result = true, Object = isAccessable };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activityId, userId);
                return new ReturnJsonModel() { result = false, Object = false };
            }
        }
        public List<BaseModel> GetQbicleBase(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Qbicle by domain id", null, null, domainId);

                return dbContext.Qbicles.Where(d => d.Domain.Id == domainId).OrderByDescending(o => o.LastUpdated).Select(l => new BaseModel { Id = l.Id, Name = l.Name }).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<BaseModel>();
            }
        }
    }
}