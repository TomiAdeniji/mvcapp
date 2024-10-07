using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.CoreCompat;
using Newtonsoft.Json;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Community;
using Qbicles.Models.Invitation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules
{
    public class UserRules
    {
        ApplicationDbContext dbContext;

        public UserRules()
        {
            dbContext = new ApplicationDbContext();
        }
        public UserRules(ApplicationDbContext context)
        {
            dbContext = context;
        }        

        /// <summary>
        /// get user by user id
        /// </summary>
        /// <param name="userId">string</param>
        /// <param name="currentQbicleId"></param>
        /// <returns></returns>
        public ApplicationUser GetUser(String userId, int currentQbicleId)
        {
            try
            {
                var user = dbContext.QbicleUser.Find(userId);
                if (currentQbicleId > 0)
                {
                    var cube = new QbicleRules(dbContext).GetQbicleById(currentQbicleId);
                    var start = cube.StartedBy;
                    var date = cube.StartedDate;
                    var owned = cube.OwnedBy;
                }

                return user;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId);
                return null;
            }
        }

        public ApplicationUser GetUserById(string userId)
        {

            return dbContext.QbicleUser.AsNoTracking().FirstOrDefault(e => e.Id == userId);
        }



        public UserModel GetUserOnly(String id)
        {
            var user = dbContext.QbicleUser.Find(id);
            return new UserModel
            {
                Id = user.Id,
                UserName = user.GetFullName()
            };

        }

        /// <summary>
        /// find user in the system by email
        /// </summary>
        /// <param name="email">email</param>
        /// <returns></returns>
        public ApplicationUser GetUserByEmail(string email)
        {
            try
            {
                return dbContext.QbicleUser.FirstOrDefault(e => e.Email == email);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, email);
                return null;
            }
        }


        public List<ApplicationUser> GetListUserIdByQbicle(int qbicleId)
        {
            return dbContext.Qbicles.Find(qbicleId)?.Members ?? new List<ApplicationUser>();
        }
        public bool ValidDuplicateUserName(string userId, string UserName)
        {
            bool exist;
            try
            {
                if (userId.Length > 0)
                    exist = dbContext.QbicleUser.Any(x => (x.Id != userId && x.UserName == UserName));
                else
                    exist = dbContext.QbicleUser.Any(x => (x.UserName == UserName));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                exist = false;
            }
            return exist;
        }
        public ReturnJsonModel ValidDuplicateEmail(string email)
        {
            var refModel = new ReturnJsonModel()
            {
                msg = ""
            };
            try
            {
                var exist = dbContext.QbicleUser.Any(x => x.Email == email);
                if (exist)
                {
                    refModel.msgId = "2";
                    refModel.result = false;
                }
                else
                    refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                refModel.result = false;
            }
            return refModel;
        }

        public bool SetNormalizedUserNameToEmail(string userId)
        {
            try
            {
                //Update the NormalizedUserName to be the email.toUpper
                var user = dbContext.QbicleUser.Find(userId);
                user.NormalizedUserName = user.Email.ToUpper();

                if (dbContext.Entry(user).State == EntityState.Detached)
                    dbContext.QbicleUser.Attach(user);

                dbContext.Entry(user).State = EntityState.Modified;
                dbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }



        public ReturnJsonModel SaveUser(MyProfileModel user)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
                userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                { AllowOnlyAlphanumericUserNames = false };
                var userUpdate = dbContext.QbicleUser.Find(user.Id);
                if (userUpdate == null)
                {
                    refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_678", null));
                    return refModel;
                }
                if (!string.IsNullOrEmpty(user.profilePic))
                    userUpdate.ProfilePic = user.profilePic;

                userUpdate.Forename = user.Forename;
                userUpdate.Surname = user.Surname;
                userUpdate.DisplayUserName = user.DisplayUserName;
                userUpdate.Profile = user.Profile;
                userUpdate.Company = user.Company;
                userUpdate.JobTitle = user.JobTitle;
                userUpdate.Tell = user.Tell;
                userUpdate.TagLine = user.Tagline;
                userUpdate.Profile = user.Description;
                userUpdate.FacebookLink = user.FacebookLink;
                userUpdate.InstagramLink = user.InstagramLink;
                userUpdate.LinkedlnLink = user.LinkedlnLink;
                userUpdate.TwitterLink = user.TwitterLink;
                userUpdate.WhatsAppLink = user.WhatsApp;


                if (dbContext.Entry(userUpdate).State == EntityState.Detached)
                    dbContext.QbicleUser.Attach(userUpdate);
                dbContext.Entry(userUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();

                UpdateTraderContact(userUpdate);

                refModel.result = true;
                refModel.Object = userUpdate;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id);
                return refModel;
            }
        }

        private void UpdateTraderContact(ApplicationUser user)
        {
            var contacts = dbContext.TraderContacts.Where(e => e.QbicleUser.Id == user.Id).ToList();
            contacts.ForEach(c =>
            {
                c.PhoneNumber = user.Tell;
                c.Email = user.Email;
                c.AvatarUri = user.ProfilePic;
            });

            dbContext.SaveChanges();
        }

        public ReturnJsonModel SaveUserWizardProfile(MyProfileModel user)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
                userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                { AllowOnlyAlphanumericUserNames = false };
                var u = dbContext.QbicleUser.Find(user.Id);
                if (u == null)
                {
                    refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_678", null));
                    return refModel;
                }

                if (!string.IsNullOrEmpty(user.profilePic))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);

                    s3Rules.ProcessingMediaS3(user.profilePic);
                }

                if (!string.IsNullOrEmpty(user.profilePic))
                    u.ProfilePic = user.profilePic;

                //Update data
                u.DisplayUserName = user.DisplayUserName;
                u.TagLine = user.Tagline;
                u.Profile = user.Profile;
                u.Tell = user.Tell;
                u.Timezone = user.Timezone;
                u.ChosenNotificationMethod = user.ChosenNotificationMethod;
                u.NotificationSound = user.NotificationSound;
                u.TimeFormat = user.TimeFormat;
                u.DateFormat = user.DateFormat;

                dbContext.Entry(u).State = EntityState.Modified;
                dbContext.SaveChanges();
                refModel.result = true;
                refModel.Object = u;
                return refModel;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id);
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
                return refModel;
            }
        }

        public ReturnJsonModel SaveUserSetting(MyProfileModel user)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
                userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                { AllowOnlyAlphanumericUserNames = false };
                var u = dbContext.QbicleUser.Find(user.Id);
                if (u == null)
                {
                    refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_678", null));
                    return refModel;
                }

                u.DateFormat = user.DateFormat;
                u.TimeFormat = user.TimeFormat;

                if (user.PreferredDomain_Id != null)
                {
                    var _preferredDomain = dbContext.Domains.Find(user.PreferredDomain_Id);
                    if (_preferredDomain != null)
                    {
                        u.PreferredDomain = _preferredDomain;
                    }
                }

                if (user.PreferredQbicle_Id != null)
                {
                    var _preferredQbicle = dbContext.Qbicles.Find(user.PreferredQbicle_Id);
                    if (_preferredQbicle != null)
                    {
                        u.PreferredQbicle = _preferredQbicle;
                    }
                }
                u.ChosenNotificationMethod = user.ChosenNotificationMethod;
                u.NotificationSound = user.NotificationSound;
                u.Timezone = user.Timezone;

                if (dbContext.Entry(u).State == EntityState.Detached)
                    dbContext.QbicleUser.Attach(u);
                dbContext.Entry(u).State = EntityState.Modified;
                dbContext.SaveChanges();
                refModel.result = true;
                refModel.Object = u;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id);
                return refModel;
            }
        }

        public bool SaveAvatar(MediaModel media, bool isDefault, string userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);

                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }
                var user = dbContext.QbicleUser.Find(userId);
                user.ProfilePic = media.UrlGuid;
                var avatar = dbContext.UserAvatars.FirstOrDefault(s => s.AvatarName == media.Name);
                if (avatar == null)
                {
                    avatar = new UserAvatars
                    {
                        isDefault = isDefault,
                        URI = media.UrlGuid,
                        CreateDate = DateTime.UtcNow,
                        AvatarName = media.Name,
                        User = user
                    };
                    dbContext.UserAvatars.Add(avatar);
                    dbContext.SaveChanges();

                    UpdateTraderContact(user);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public bool SetAvatar(string userId, string uri)
        {
            try
            {
                var user = dbContext.QbicleUser.Find(userId);
                if (user != null)
                {
                    user.ProfilePic = uri;
                    dbContext.SaveChanges();
                    UpdateTraderContact(user);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public UserAvatars CheckAvatarExist(string userAvatar)
        {
            try
            {
                return dbContext.UserAvatars.FirstOrDefault(s => s.URI == userAvatar || s.AvatarName == userAvatar);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        public bool SetPrivacyOptions(string userId, MyProfilePrivacyOptionsModel optionsModel)
        {
            try
            {
                var user = dbContext.QbicleUser.Find(userId);
                if (user != null)
                {
                    user.isShareJobTitle = optionsModel.isShareJobTitle;
                    user.isShareEmail = optionsModel.isShareEmail;
                    user.isShareCompany = optionsModel.isShareCompany;
                    user.isAlwaysLimitMyContact = optionsModel.isAlwaysLimitMyContact;
                }
                return dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public List<Select2CustomeModel> loadPreferredQbicles(int domainId)
        {
            try
            {
                var domain = dbContext.Domains.Find(domainId);
                if (domain != null)
                {
                    var rsList = new List<Select2CustomeModel>();
                    rsList.Add(new Select2CustomeModel() { id = 0, text = "" });
                    rsList.AddRange(domain.Qbicles.Select(s => new Select2CustomeModel { id = s.Id, text = s.Name }).ToList());
                    return rsList;
                }
                return new List<Select2CustomeModel>();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new List<Select2CustomeModel>();
            }
        }
        public ReturnJsonModel SaveEmloymentHistory(EmploymentModel employment, UserProfilePage userProfile)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var _dbemployment = dbContext.Employments.Find(employment.Id);
                if (_dbemployment == null)
                {
                    var _dbuserProfile = dbContext.UserProfilePages.FirstOrDefault(s => s.AssociatedUser.Id == userProfile.AssociatedUser.Id);
                    if (_dbuserProfile == null)
                        _dbuserProfile = userProfile;
                    else
                    {
                        _dbuserProfile.ProfileText = userProfile.ProfileText;
                        _dbuserProfile.StoredLogoName = userProfile.StoredLogoName;
                        _dbuserProfile.StoredFeaturedImageName = userProfile.StoredFeaturedImageName;
                        _dbuserProfile.LastUpdated = DateTime.UtcNow;
                    }
                    _dbemployment = new Employment();
                    _dbemployment.AssociatedProfile = _dbuserProfile;
                    _dbemployment.Employer = employment.Employer;
                    _dbemployment.Role = employment.Role;
                    _dbemployment.Summary = employment.Summary;
                    _dbemployment.CreatedDate = DateTime.UtcNow;
                    _dbemployment.CreatedBy = employment.User;
                    var startDateTime = string.IsNullOrEmpty(employment.StartDate) ? DateTime.ParseExact(DateTime.UtcNow.ToString("dd/MM/yyyy"), "dd/MM/yyyy", null) : DateTime.ParseExact(employment.StartDate, "dd/MM/yyyy", null);
                    var endDateTime = string.IsNullOrEmpty(employment.EndDate) ? DateTime.ParseExact(DateTime.UtcNow.ToString("dd/MM/yyyy"), "dd/MM/yyyy", null) : DateTime.ParseExact(employment.EndDate, "dd/MM/yyyy", null);
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(employment.CurrentTimeZone);
                    var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDateTime, tz);
                    var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDateTime, tz);
                    _dbemployment.StartDate = startDateTimeUTC;
                    _dbemployment.EndDate = endDateTimeUTC;
                    dbContext.Employments.Add(_dbemployment);
                    dbContext.Entry(_dbemployment).State = EntityState.Added;
                    _dbuserProfile.Employments.Add(_dbemployment);

                }
                else
                {
                    _dbemployment.Employer = employment.Employer;
                    var startDateTime = string.IsNullOrEmpty(employment.StartDate) ? DateTime.ParseExact(DateTime.UtcNow.ToString("dd/MM/yyyy"), "dd/MM/yyyy", null) : DateTime.ParseExact(employment.StartDate, "dd/MM/yyyy", null);
                    var endDateTime = string.IsNullOrEmpty(employment.EndDate) ? DateTime.ParseExact(DateTime.UtcNow.ToString("dd/MM/yyyy"), "dd/MM/yyyy", null) : DateTime.ParseExact(employment.EndDate, "dd/MM/yyyy", null);
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(employment.CurrentTimeZone);
                    var startDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(startDateTime, tz);
                    var endDateTimeUTC = TimeZoneInfo.ConvertTimeToUtc(endDateTime, tz);
                    _dbemployment.StartDate = startDateTimeUTC;
                    _dbemployment.EndDate = endDateTimeUTC;
                    _dbemployment.Role = employment.Role;
                    _dbemployment.Summary = employment.Summary;
                }
                returnJson.result = dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
            return returnJson;
        }
        public ReturnJsonModel SaleUserMyFile(MyfileUploadModal myfileUpload, string userId)
        {
            if (!string.IsNullOrEmpty(myfileUpload.media.UrlGuid))
            {
                var s3Rules = new Azure.AzureStorageRules(dbContext);

                s3Rules.ProcessingMediaS3(myfileUpload.media.UrlGuid);
            }

            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                var user = dbContext.QbicleUser.Find(userId);
                var userProfile = new UserProfilePage
                {
                    AssociatedUser = user,
                    ProfileText = user.Profile,
                    StoredLogoName = user.ProfilePic,
                    StoredFeaturedImageName = user.ProfilePic,
                    StrapLine = user.DisplayUserName,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = user,
                    PageType = CommunityPageTypeEnum.UserProfile
                };
                myfileUpload.User = user;

                var _dbuserProfile = dbContext.UserProfilePages.FirstOrDefault(s => s.AssociatedUser.Id == userProfile.AssociatedUser.Id);
                if (_dbuserProfile == null)
                    _dbuserProfile = userProfile;
                else
                {
                    _dbuserProfile.ProfileText = userProfile.ProfileText;
                    _dbuserProfile.StoredLogoName = userProfile.StoredLogoName;
                    _dbuserProfile.StoredFeaturedImageName = userProfile.StoredFeaturedImageName;
                    _dbuserProfile.LastUpdated = DateTime.UtcNow;
                }
                var profileFile = new ProfileFile
                {
                    AssociatedProfile = _dbuserProfile,
                    Name = myfileUpload.Name,
                    StoredFileName = myfileUpload.media.UrlGuid,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = myfileUpload.User,
                    Description = myfileUpload.Description,
                    FileType = myfileUpload.media.Type
                };
                dbContext.ProfileFiles.Add(profileFile);
                dbContext.Entry(profileFile).State = EntityState.Added;
                _dbuserProfile.ProfileFiles.Add(profileFile);
                returnJson.result = dbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
            return returnJson;
        }
        public bool DeleteEmploymentById(int empId)
        {
            try
            {
                var empl = dbContext.Employments.Find(empId);
                if (empl != null)
                {
                    dbContext.Employments.Remove(empl);
                    return dbContext.SaveChanges() > 0 ? true : false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public bool DeleteProFileById(int fId)
        {
            try
            {
                var file = dbContext.ProfileFiles.Find(fId);
                if (file != null)
                {
                    dbContext.ProfileFiles.Remove(file);
                    return dbContext.SaveChanges() > 0 ? true : false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }
        }
        public EmploymentModel GetEmploymentById(int empId, string currentTimeZone)
        {
            try
            {
                var empl = dbContext.Employments.Find(empId);
                if (empl != null)
                {
                    return new EmploymentModel { Id = empl.Id, Employer = empl.Employer, Role = empl.Role, Summary = empl.Summary, StartDate = empl.StartDate.ConvertTimeFromUtc(currentTimeZone).ToString("dd/MM/yyyy"), EndDate = empl.EndDate.ConvertTimeFromUtc(currentTimeZone).ToString("dd/MM/yyyy") };
                }
                else
                    return new EmploymentModel();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new EmploymentModel();
            }
        }
        public List<EmploymentHistoryModel> GetEmploymentsByUserId(string userId, string currentTimeZone)
        {
            try
            {
                var dbprofile = dbContext.UserProfilePages.FirstOrDefault(s => s.AssociatedUser.Id == userId);
                if (dbprofile != null && dbprofile.Employments != null)
                {
                    var empHistories = dbprofile.Employments.OrderByDescending(s => s.EndDate).ToList();
                    var currentDate = empHistories.FirstOrDefault();
                    return empHistories.Select(s => new EmploymentHistoryModel { Id = s.Id, Role = s.Role, Employer = s.Employer, Summary = s.Summary, Dates = (s.StartDate.ConvertTimeFromUtc(currentTimeZone).ToString("MMM yyyy") + " - " + (s.EndDate == currentDate.EndDate ? "Present" : s.EndDate.ConvertTimeFromUtc(currentTimeZone).ToString("MMMM yyyy"))), EndDate = s.EndDate }).ToList();
                }
                return new List<EmploymentHistoryModel>(); ;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new List<EmploymentHistoryModel>();
            }
        }
        public List<MyfileModal> GetMyFilesByUserId(string userId, string currentTimeZone)
        {
            try
            {
                var dbprofile = dbContext.UserProfilePages.FirstOrDefault(s => s.AssociatedUser.Id == userId);
                if (dbprofile != null && dbprofile.ProfileFiles != null)
                {
                    var myFiles = dbprofile.ProfileFiles.OrderByDescending(s => s.CreatedDate).ToList();
                    return myFiles.Select(s => new MyfileModal { Id = s.Id, Title = s.Name, Added = s.CreatedDate.ConvertTimeFromUtc(currentTimeZone).ToString("dd/MM/yyyy hh:mmtt"), Type = Utility.GetFileTypeDescription(s.FileType?.Type), Description = s.Description }).ToList();
                }
                return new List<MyfileModal>(); ;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new List<MyfileModal>();
            }
        }
        public ApplicationUser GetUserByUserName(string userName)
        {
            try
            {
                return dbContext.QbicleUser.FirstOrDefault(x => x.UserName == userName);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        /// <summary>
        /// Create user by invited
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public ApplicationUser CreateUserInvitedByEmail(string email)
        {
            try
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
                userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                { AllowOnlyAlphanumericUserNames = false };
                var user = new ApplicationUser()
                {
                    UserName = email,
                    Email = email,
                    ChosenNotificationMethod = (Notification.NotificationSendMethodEnum)(int.Parse(ConfigurationManager.AppSettings["NotificationSendMethod"])),// get from web config
                    DateBecomesMember = DateTime.UtcNow,
                    Timezone = ConfigurationManager.AppSettings["Timezone"],
                    TimeFormat = "HH:mm",
                    DateFormat = "dd/MM/yyyy",
                    Profile = ""
                };
                var password = ConfigurationManager.AppSettings["CreateUserPasswordDefault"];// get from web config

                var adminResult = userManager.Create(user, password);

                return adminResult.Succeeded ? user : null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }
        /// <summary>
        /// Create new user when add user to Domain
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public ApplicationUser CreateCustomerUser(Models.TraderApi.Customer customer)
        {
            try
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
                userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = true
                };
                var user = new ApplicationUser()
                {
                    UserName = customer.Email,
                    Email = customer.Email,
                    DisplayUserName = customer.Name,
                    ChosenNotificationMethod = Notification.NotificationSendMethodEnum.Both,
                    DateBecomesMember = DateTime.UtcNow,
                    Timezone = "GMT Standard Time",
                    TimeFormat = "HH:mm",
                    DateFormat = "dd/MM/yyyy",
                    ProfilePic = ConfigManager.DefaultUserUrlGuid
                };

                var adminResult = userManager.Create(user, ConfigurationManager.AppSettings["CreateUserPasswordDefault"]);

                if (adminResult.Succeeded)
                    return user;
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        public List<ApplicationUser> GetListUserByDomainId(int domainId)
        {
            try
            {
                // Get user is Admin Domain  or Guest 
                return dbContext.QbicleUser.Where(x => (x.Domains.Where(y => y.Id == domainId).Count() > 0)).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new List<ApplicationUser>();
            }
        }

        public List<UserModel> GetUserByDomain(int domainId)
        {
            return dbContext.QbicleUser.AsNoTracking().Where(e => e.Domains.Any(d => d.Id == domainId)).
                Select(u => new UserModel
                {
                    Id = u.Id,
                    UserName = !string.IsNullOrEmpty(u.DisplayUserName) ? u.DisplayUserName :
                    (!string.IsNullOrEmpty(u.Forename) && !string.IsNullOrEmpty(u.Surname) ? u.Forename + " " + u.Surname : u.UserName)
                }
                ).ToList();
        }

        /// <summary>
        /// demote/promote user the
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="privilegeType"></param>
        /// <returns></returns>
        public bool ChangeUserPrivilege(string userId, string privilegeType, QbicleDomain CurrentDomain)
        {
            try
            {
                var user = this.GetUser(userId, 0);
                switch (privilegeType)
                {
                    case "promote":
                        CurrentDomain.Administrators.Add(user);
                        break;
                    case "demote":
                        CurrentDomain.Administrators.Remove(user);
                        break;
                }
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return false;
            }

        }

        public ApplicationUser GetById(string userId)
        {
            return dbContext.QbicleUser.Find(userId);
        }
        public UserCustom GetProfileByUserId(string userId)
        {
            var lstResult = from c in dbContext.QbicleUser.Where(a => a.Id == userId)
                            select new UserCustom
                            {
                                Id = c.Id,
                                Profile = c.Profile,
                                UserName = c.UserName,
                                Surname = c.Surname,
                                Forename = c.Forename,
                                ProfilePic = c.ProfilePic,
                                Email = c.Email,
                                PhoneNumber = c.PhoneNumber
                            };
            return lstResult.FirstOrDefault();
        }


        public List<UserCustom> GetAllBySystemAdmin(IDataTablesRequest requestModel, ref int TotalRecord)
        {
            var roles = dbContext.Roles.FirstOrDefault().Users;

            var lstResult = from c in dbContext.QbicleUser
                            join p in dbContext.profiles on c.Id equals p.CreatedById into TMP
                            from profile in TMP.DefaultIfEmpty()
                            select new UserCustom
                            {
                                Id = c.Id,
                                Expression = profile.Expression,
                                UserName = c.DisplayUserName,
                                Surname = c.Surname,
                                Forename = c.Forename,
                                ProfilePic = c.ProfilePic,
                                Email = c.Email,
                                PhoneNumber = c.PhoneNumber,
                                IsEnabled = c.IsEnabled ?? false,
                                Domains = c.Domains.Select(s => s.Name).ToList(),
                                SystemRoles = c.Roles.Select(r => new DomainRoleModel { Id = r.RoleId }).ToList(),
                            };
            if (!string.IsNullOrEmpty(requestModel.Search.Value))
            {
                var value = requestModel.Search.Value.Trim().ToLower();
                lstResult = lstResult.Where(p => (p.UserName + "").ToLower().Contains(value)
                                          || (p.Surname + "").ToLower().Contains(value)
                                          || (p.Forename + "").ToLower().Contains(value)
                                          || (p.Email + "").ToLower().Contains(value)
                                          || (p.Domains.Any(d => d.ToLower().Contains(value)))
                                          );
            }
            TotalRecord = lstResult.Count();

            #region Sorting

            // Sorting
            var sortedColumns = requestModel.Columns.GetSortedColumns();
            var orderByString = "";

            foreach (var column in sortedColumns)
            {
                switch (column.Data)
                {
                    case "UserName":
                        orderByString += orderByString != string.Empty ? "," : "";
                        orderByString += "Forename" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                        //orderByString += ",Surname" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                        //orderByString += ",UserName" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                        break;
                    case "Email":
                        orderByString += orderByString != string.Empty ? "," : "";
                        orderByString += "Email" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                        break;
                    case "IsEnabled":
                        orderByString += orderByString != string.Empty ? "," : "";
                        orderByString += "IsEnabled" + (column.SortDirection == OrderDirection.Ascendant ? " asc" : " desc");
                        break;
                }
            }



            lstResult = lstResult.OrderBy(orderByString);

            #endregion Sorting

            if (requestModel.Length > 0)
                lstResult = lstResult.Skip(requestModel.Start).Take(requestModel.Length);
            return lstResult.ToList();
        }



        public bool SuspendOrActive(string userId, int type)
        {
            var user = dbContext.QbicleUser.FirstOrDefault(a => a.Id == userId);
            if (user != null)
            {
                user.IsEnabled = type == 1 ? true : false;
                dbContext.SaveChanges();
                return true;
            }
            return false;
        }
        public List<UserCustom> getMyContacts(string currentUserId)
        {
            try
            {
                var lstUser = new List<UserCustom>();
                var users = new List<ApplicationUser>();
                var qbicles = dbContext.Qbicles.Where(q => q.Members.Any(s => s.Id == currentUserId)).ToList();
                foreach (var item in qbicles)
                {
                    users.AddRange(item.Members);
                }
                lstUser = users.Distinct().Where(s => s.Id != currentUserId).Select(c => new UserCustom
                {
                    Forename = c.Forename,
                    Surname = c.Surname,
                    UserName = c.UserName,
                    Id = c.Id,
                    Profile = c.Profile,
                    DisplayUserName = c.DisplayUserName,
                    DateBecomesMember = c.DateBecomesMember,
                    ProfilePic = c.ProfilePic
                }).ToList();
                return lstUser;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return new List<UserCustom>();
            }
        }
        public ReturnJsonModel InviteUserJoinQbicles(string email, string message, int domainId, string callbackUrl, string userId, bool fromWeb)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, email, message, domainId, callbackUrl);

                var domain = dbContext.Domains.Find(domainId);
                email = email.Trim();
                //Checks to see if the email address exists in the application
                var userNew = new UserRules(dbContext).GetUserByEmail(email) ?? new ApplicationUser();
                // If the application finds that the email address does NOT exist in the system
                if (domain.Users.Any(a => a.Id == userNew.Id))
                {
                    returnJson.msg = ResourcesManager._L("ERROR_MSG_EMAIL_EXISTED", email);
                    return returnJson;
                }

                if (fromWeb)
                    domain.WizardStep = DomainWizardStep.InviteUsers;
                else
                    domain.WizardStepMicro = DomainWizardStepMicro.Users;

                var currentUser = dbContext.QbicleUser.Find(userId);


                var invitation = new Invitation
                {
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    Domain = domain,
                    Email = email.Trim(),
                    LastUpdateDate = DateTime.UtcNow,
                    LastUpdatedBy = currentUser,
                    Status = InvitationStatusEnum.Pending,
                    Note = message
                };
                var log = new InvitationSentLog
                {
                    CreatedBy = currentUser,
                    CreatedDate = DateTime.UtcNow,
                    Invitation = invitation
                };
                invitation.Log.Add(log);
                dbContext.Invitations.Add(invitation);
                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                new EmailRules(dbContext).SendEmailInvitation(invitation.Id, currentUser, email, callbackUrl, domain.Id, domain.Name, email, message);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, email, message, domainId, callbackUrl);
                returnJson.msg = ex.Message;
            }
            return returnJson;
        }

        public ReturnJsonModel InviteUserJoinQbicles(string email, string message, string callbackUrl, string userId)
        {
            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, email, message, callbackUrl, userId);
                email = email.Trim();
                var maximumInvites = string.IsNullOrEmpty(ConfigManager.MaximumInvitesJoinQbiclesPerDay) ? 0 : Convert.ToInt32(ConfigManager.MaximumInvitesJoinQbiclesPerDay);
                var userRule = new UserRules(dbContext);
                //Checks to see if the email address exists in the application
                var userNew = userRule.GetUserByEmail(email);
                // If the application finds that the email address does NOT exist in the system
                if (userNew != null)
                {
                    returnJson.msg = ResourcesManager._L("ERROR_MSG_REGISTERED_EXISTED");
                    return returnJson;
                }
                var startDate = DateTime.UtcNow.Date;
                var endDate = startDate.AddDays(1);

                if (dbContext.InviteCounts.Any(s => s.User.Id == userId && s.InviteEmail == email && s.Count == maximumInvites
                && s.CountDate >= startDate && s.CountDate < endDate))
                {
                    returnJson.msg = ResourcesManager._L("ERROR_MSG_LIMIT_INVITES", maximumInvites);
                    return returnJson;
                }
                var currentUser = dbContext.QbicleUser.Find(userId);
                var sentComplete = new EmailRules(dbContext).SendEmailInvitation(currentUser, email, callbackUrl, "", email, message);
                if (sentComplete)
                {
                    var inviteCount = dbContext.InviteCounts.FirstOrDefault(s => s.User.Id == userId && s.InviteEmail == email);
                    if (inviteCount != null)
                    {
                        if (inviteCount.Count < maximumInvites)
                            inviteCount.Count += 1;
                        else
                            inviteCount.Count = 1;
                        inviteCount.CountDate = DateTime.UtcNow;
                    }
                    else
                    {
                        inviteCount = new InviteCount
                        {
                            User = currentUser,
                            InviteEmail = email,
                            Count = 1,
                            CountDate = DateTime.UtcNow
                        };
                        dbContext.InviteCounts.Add(inviteCount);
                    }
                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, email, message, callbackUrl, userId);
            }
            return returnJson;
        }

        public ReturnJsonModel ToggleUserProfileWizardRun(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId);
                var user = dbContext.QbicleUser.Find(userId);
                if (user == null)
                {
                    return new ReturnJsonModel() { result = false, msg = "Can not find User." };
                }
                user.IsUserProfileWizardRun = true;
                dbContext.Entry(user).State = EntityState.Modified;
                dbContext.SaveChanges();
                return new ReturnJsonModel() { result = true };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }
        public ReturnJsonModel UpdateUserWizardStep(string userId, UserWizardStep step, UserWizardPlatformType wizardPlatform)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, step, wizardPlatform);
                var user = dbContext.QbicleUser.Find(userId);
                if (user != null)
                {
                    user.WizardStep = step;
                    if (wizardPlatform == UserWizardPlatformType.Website)
                    {
                        if (step == UserWizardStep.BusinessesConnectStep)
                        {
                            user.IsUserProfileWizardRun = true;
                            user.IsUserProfileWizardRunMicro = true;
                        }
                        else
                        {
                            user.IsUserProfileWizardRun = false;
                            user.IsUserProfileWizardRunMicro = false;
                        }
                    }
                    else if (wizardPlatform == UserWizardPlatformType.MicroApp)
                    {
                        var microWizardStep = (MicroUserWizardStep)step.GetId();
                        if (microWizardStep == MicroUserWizardStep.Done)
                        {
                            user.IsUserProfileWizardRun = true;
                            user.IsUserProfileWizardRunMicro = true;
                        }
                        else
                        {
                            user.IsUserProfileWizardRun = false;
                            user.IsUserProfileWizardRunMicro = false;
                        }
                    }

                    dbContext.Entry(user).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                return new ReturnJsonModel() { actionVal = 2, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, step, wizardPlatform);
                return new ReturnJsonModel() { actionVal = 2, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public ReturnJsonModel MicroSaveUserGeneralInfo(MicroUserProfile user)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbContext));
                userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
                { AllowOnlyAlphanumericUserNames = false };
                var u = dbContext.QbicleUser.Find(user.Id);
                if (u == null)
                {
                    refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_678", null));
                    return refModel;
                }

                if (string.IsNullOrEmpty(u.Email) || dbContext.QbicleUser.Any(us => us.Id != u.Id && us.Email == user.Email))
                {
                    refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_632", new object[] { user.Email }));
                    return refModel;
                }
                if (!string.IsNullOrEmpty(user.Email))
                    u.Email = user.Email;
                if (!string.IsNullOrEmpty(user.UserName))
                    u.UserName = user.UserName;
                if (!string.IsNullOrEmpty(user.Forename))
                    u.Forename = user.Forename;
                if (!string.IsNullOrEmpty(user.Surname))
                    u.Surname = user.Surname;
                if (!string.IsNullOrEmpty(user.DisplayUserName))
                    u.DisplayUserName = user.DisplayUserName;
                if (!string.IsNullOrEmpty(user.Profile))
                    u.Profile = user.Profile;
                if (!string.IsNullOrEmpty(user.Company))
                    u.Company = user.Company;
                if (!string.IsNullOrEmpty(user.JobTitle))
                    u.JobTitle = user.JobTitle;
                if (!string.IsNullOrEmpty(user.Tell))
                    u.Tell = user.Tell;
                if (!string.IsNullOrEmpty(user.TagLine))
                    u.TagLine = user.TagLine;

                if (dbContext.Entry(u).State == EntityState.Detached)
                    dbContext.QbicleUser.Attach(u);
                dbContext.Entry(u).State = EntityState.Modified;
                dbContext.SaveChanges();
                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id);
                return refModel;
            }
        }


        public ReturnJsonModel MicroSaveUserSocialNetworks(MicroUserProfile user)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                var u = dbContext.QbicleUser.Find(user.Id);
                if (u == null)
                {
                    refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_678", null));
                    return refModel;
                }

                u.FacebookLink = user.FacebookLink;
                u.InstagramLink = user.InstagramLink;
                u.LinkedlnLink = user.LinkedlnLink;
                u.TwitterLink = user.TwitterLink;
                u.WhatsAppLink = user.WhatsAppLink;

                if (dbContext.Entry(u).State == EntityState.Detached)
                    dbContext.QbicleUser.Attach(u);
                dbContext.Entry(u).State = EntityState.Modified;
                dbContext.SaveChanges();
                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id);
                return refModel;
            }
        }

        public ReturnJsonModel MicroSaveUserSetting(MicroUserProfile user)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false };
            try
            {
                var u = dbContext.QbicleUser.Find(user.Id);
                if (u == null)
                {
                    refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_678", null));
                    return refModel;
                }

                u.DateFormat = user.DateFormat;
                u.TimeFormat = user.TimeFormat;
                u.Timezone = string.IsNullOrEmpty(user.Timezone) ? "W. Europe Standard Time" : user.Timezone;

                u.isShareJobTitle = user.IsShareJobTitle;
                u.isShareEmail = user.IsShareEmail;
                u.isShareCompany = user.IsShareCompany;
                u.isAlwaysLimitMyContact = user.IsAlwaysLimitMyContact;

                u.PreferredDomain = dbContext.Domains.Find(user.PreferredDomain?.Id ?? 0);
                u.PreferredQbicle = dbContext.Qbicles.Find(user.PreferredQbicle?.Id ?? 0);

                u.ChosenNotificationMethod = user.ChosenNotificationMethod;
                u.NotificationSound = user.NotificationSound;

                if (dbContext.Entry(u).State == EntityState.Detached)
                    dbContext.QbicleUser.Attach(u);
                dbContext.Entry(u).State = EntityState.Modified;
                dbContext.SaveChanges();
                refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, user.Id);
                return refModel;
            }
        }


        public ReturnJsonModel UpdateInterests(List<int> interests, string userId)
        {
            var returnjson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, interests, userId);
                if (interests == null || interests.Count == 0)
                {
                    returnjson.msg = ResourcesManager._L("ERROR_MSG_NOINTEREST");
                    return returnjson;
                }
                var user = dbContext.QbicleUser.Find(userId);
                user.Interests.Clear();
                if (user != null)
                    foreach (var catid in interests)
                    {
                        var category = dbContext.BusinessCategories.Find(catid);
                        if (category != null)
                            user.Interests.Add(category);
                    }
                dbContext.SaveChanges();
                returnjson.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, interests, userId);
            }
            return returnjson;
        }

        public ReturnJsonModel AddRemoveInterest(int id, bool isDelete, string userId)
        {
            var returnjson = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id, isDelete, userId);

                var interest = dbContext.BusinessCategories.FirstOrDefault(e => e.Id == id);
                if (interest == null)
                {
                    returnjson.msg = ResourcesManager._L("ERROR_MSG_DATA_NOT_FOUND", id);
                    returnjson.actionVal = 406;
                    return returnjson;
                }
                var user = dbContext.QbicleUser.Find(userId);

                if (isDelete)
                    user.Interests.Remove(interest);
                else
                {
                    if (user.Interests.Any(e => e.Id == id))
                    {
                        returnjson.msg = ResourcesManager._L("ERROR_DATA_EXISTED", interest.Name);
                        returnjson.actionVal = 406;
                        return returnjson;
                    }
                    user.Interests.Add(interest);
                }
                dbContext.SaveChanges();
                returnjson.result = true;
            }
            catch (Exception ex)
            {
                returnjson.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, isDelete, userId);
            }
            return returnjson;
        }

        public ReturnJsonModel ToggleUserNavBarOpenStt(string userId, bool isNavTabClosed)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, isNavTabClosed);
                }

                var currentUser = dbContext.QbicleUser.Find(userId);
                if (currentUser != null)
                {
                    currentUser.IsNavigationBarClosed = !isNavTabClosed;

                    dbContext.Entry(currentUser).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }

                return new ReturnJsonModel() { actionVal = 2, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, isNavTabClosed);
                return new ReturnJsonModel() { actionVal = 2, result = false, msg = "Error on updating Navigation Bar open status." };
            }
        }
        public ReturnJsonModel ChangeNewEmailAddress(string userId, string newEmailAddress, string callbackUrl)
        {
            var refModel = new ReturnJsonModel
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GeneratePin", null, null, userId);
                var user = dbContext.QbicleUser.Find(userId);
                if (user.Email == newEmailAddress)
                    return refModel;
                var tempEmailAddress = dbContext.TempEmailAddress.FirstOrDefault(s => s.User.Id == userId);
                if (tempEmailAddress == null)
                {
                    tempEmailAddress = new TempEmailAddress();
                    tempEmailAddress.Email = newEmailAddress;
                    tempEmailAddress.ExpiryTime = DateTime.UtcNow.AddDays(2);
                    tempEmailAddress.User = dbContext.QbicleUser.Find(userId);
                    tempEmailAddress.SendingTime = DateTime.UtcNow;
                    tempEmailAddress.CountSentPerDay = 0;
                    dbContext.TempEmailAddress.Add(tempEmailAddress);
                }
                //Allow sent < 5 times per day
                if (tempEmailAddress.CountSentPerDay > 5)
                {
                    refModel.msg = "ERROR_MSG_LIMITSENTEMAIL";
                    return refModel;
                }
                var rnd = new Random();
                var pin = rnd.Next(1000, 9999);
                var count = 0;
                while (string.IsNullOrEmpty(tempEmailAddress.PIN) || tempEmailAddress.ExpiryTime > DateTime.UtcNow)
                {
                    if (count > 4872)
                    {
                        refModel.result = false;
                        refModel.msg = ResourcesManager._L("ERROR_MSG_675");
                        return refModel;
                    }
                    pin = rnd.Next(1000, 9999);
                    if (tempEmailAddress.PIN != pin.ToString())
                    {
                        tempEmailAddress.PIN = pin.ToString();
                        tempEmailAddress.ExpiryTime = DateTime.UtcNow.AddDays(2);
                        break;
                    }

                    count++;
                }
                dbContext.SaveChanges();
                new EmailRules(dbContext).SendEmailNewEmailAddress(tempEmailAddress, callbackUrl, "_NewEmailVerify.html");
                if (tempEmailAddress.SendingTime.ToString("yyyyMMdd") == DateTime.UtcNow.ToString("yyyyMMdd"))
                {
                    tempEmailAddress.CountSentPerDay += 1;
                }
                else
                {
                    tempEmailAddress.CountSentPerDay = 1;
                    tempEmailAddress.SendingTime = DateTime.UtcNow;
                }
                refModel.result = true;
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);

            }

            return refModel;
        }
        /// <summary>
        /// 1: This PIN is not available
        /// 2: This PIN is expired time
        /// 0: This PIN is available
        /// 3: Only wrong PIN is allowed 5 times
        /// </summary>
        /// <param name="inputPIN">Input PIN</param>
        /// <param name="userId">Current User ID</param>
        /// <returns></returns>
        public short CheckPINNewEmail(string inputPIN, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GeneratePin", null, null, userId);

                var tempEmailAddress = dbContext.TempEmailAddress.FirstOrDefault(s => s.User.Id == userId && s.PIN == inputPIN);
                //Only wrong PIN is allowed 5 times
                if (tempEmailAddress != null && tempEmailAddress.CountVerifyFail > 5)
                {
                    dbContext.TempEmailAddress.Remove(tempEmailAddress);
                    return 3;
                }
                if (tempEmailAddress != null && tempEmailAddress.ExpiryTime < DateTime.UtcNow)
                {
                    tempEmailAddress.CountVerifyFail += 1;
                    dbContext.SaveChanges();
                    return 2;
                }
                else if (tempEmailAddress != null && tempEmailAddress.ExpiryTime >= DateTime.UtcNow)
                {
                    var user = dbContext.QbicleUser.Find(userId);
                    var isEmailExist = dbContext.QbicleUser.Any(s => s.Id != userId && s.Id == tempEmailAddress.Email);
                    if (!isEmailExist)
                    {
                        user.Email = tempEmailAddress.Email;
                        user.UserName = tempEmailAddress.Email;
                        dbContext.TempEmailAddress.Remove(tempEmailAddress);
                        tempEmailAddress.CountVerifyFail += 1;
                        dbContext.SaveChanges();
                    }
                    return 0;
                }
                else
                {
                    tempEmailAddress.CountVerifyFail += 1;
                    dbContext.SaveChanges();
                    return 1;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return 1;

            }
        }
        public bool CheckPendingVerifyNewEmail(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GeneratePin", null, null, userId);

                var tempEmailAddress = dbContext.TempEmailAddress.FirstOrDefault(s => s.User.Id == userId);
                if (tempEmailAddress != null && tempEmailAddress.ExpiryTime > DateTime.UtcNow)
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return false;

            }
        }
        public TempEmailAddress GetPendingVerifyNewEmail(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GeneratePin", null, null, userId);

                var tempEmailAddress = dbContext.TempEmailAddress.FirstOrDefault(s => s.User.Id == userId);
                if (tempEmailAddress != null && tempEmailAddress.ExpiryTime > DateTime.UtcNow)
                    return tempEmailAddress;
                else
                    return null;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return null;

            }
        }
    }
}