using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.BusinessRules.UserInformation;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.Qbicles;
using Qbicles.Models.Trader;
using Qbicles.Models.UserInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using static Qbicles.BusinessRules.HelperClass;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroUserProfilesRules : MicroRulesBase
    {
        public MicroUserProfilesRules(MicroContext microContext) : base(microContext)
        {
        }


        public List<BaseModel> GetPreferredQbiclesByDomainId(int domainId)
        {
            return dbContext.Domains.FirstOrDefault(d => d.Id == domainId)?.Qbicles.Select(d => new BaseModel { Id = d.Id, Name = d.Name }).ToList();
        }

        public object GetUserProfileOption()
        {
            var refModel = new
            {
                Sounds = EnumModel.ConvertEnumToList<NotificationSound>(),
                SendMethods = EnumModel.ConvertEnumToList<NotificationSendMethodEnum>(),
                PreferredDomains = CurrentUser.Domains.Select(d => new BaseModel { Id = d.Id, Name = d.Name }),
                //PreferredQbicles = CurrentUser.Qbicles.Select(d => new BaseModel { Id = d.Id, Name = d.Name }), get list by domain id
                DateFormats = new List<object>
                {
                    new {Key="dd/MM/yyyy", Value="DD/MM/YYYY"},
                    new {Key="MM/dd/yyyy", Value="MM/DD/YYYY"},
                },
                TimeFormats = new List<object>
                {
                    new {Key="HH:mm", Value="HH:mm"}
                },
                Timezones = TimeZoneInfo.GetSystemTimeZones().Select(tz => new
                {
                    Value = tz.DisplayName,
                    Key = tz.Id
                }),
                Countries = Country.All.Where(c => c.CountryCode != CountryCode.World).Select(c => new
                {
                    Value = c.CommonName,
                    Key = c.CountryCode
                })
            };

            return refModel;
        }


        public MicroUserProfile GetUserProfile(string id)
        {
            var currentUserId = CurrentUser.Id;

            //filter all the contacts that form the relationship
            var queryUsersInRelationShip = dbContext.C2CQbicles.Where(s => !s.IsHidden && s.Customers.Any(u => u.Id == currentUserId))
            .SelectMany(s => s.Customers.Select(u => new PeopleInfoModel
            {
                UserId = u.Id,                
                HasRemoved = s.RemovedForUsers.Any(r => r.Id == currentUserId),
                Type = 1,
                HasConnected = true,
                HasDefaultB2CRelationshipManager = true
            }));

            var isC2CConnected = dbContext.C2CQbicles.Any(p => p.Customers.Any(x => x.Id == id) && p.Customers.Any(x => x.Id == currentUserId));
            var isC2CAccepted = dbContext.C2CQbicles.Any(p => p.Customers.Any(x => x.Id == id) && p.Customers.Any(x => x.Id == currentUserId) && p.Status == CommsStatus.Approved);

            var connectStatus = UserConnectStatus.None;
            if (isC2CConnected && isC2CAccepted)
                connectStatus = UserConnectStatus.Connected;
            else if (isC2CConnected && !isC2CAccepted)
                connectStatus = UserConnectStatus.Pending;

            return dbContext.QbicleUser.Find(id).ToUserProfile(CurrentUser.Id, connectStatus);
        }


        public List<MicroUserSharedQbicles> GetUserSharedQbicles(string id)
        {
            return new QbicleRules(dbContext).GetQbiclesForProfileByUserId(CurrentUser.Id, id).ToUserSharedQbicles(CurrentUser.Id);
        }


        #region Update user profile

        public bool ChangeUserAvatar(MediaModel media)
        {
            return new UserRules(dbContext).SaveAvatar(media, false, media.Id);
        }


        public ReturnJsonModel UpdateGeneralInfo(MicroUserProfile user)
        {
            return new UserRules(dbContext).MicroSaveUserGeneralInfo(user);
        }

        public ReturnJsonModel UpdateSocialNetworks(MicroUserProfile user)
        {
            return new UserRules(dbContext).MicroSaveUserSocialNetworks(user);
        }

        public ReturnJsonModel UpdateUserSetting(MicroUserProfile user)
        {
            return new UserRules(dbContext).MicroSaveUserSetting(user);
        }


        public ReturnJsonModel AddRemoveInterest(int id, bool isDelete, string userId)
        {
            return new UserRules(dbContext).AddRemoveInterest(id, isDelete, userId);
        }

        public object GetInterests()
        {
            return CurrentUser.Interests?.Select(d => new BaseModel { Id = d.Id, Name = d.Name });
        }


        #endregion

        #region Showcase

        public List<MicroUserShowcase> GetUserShowcase(string id)
        {
            var total = 0;
            return new UserInformationRules(dbContext).GetListShowCase(id, "", ref total).ToUserShowcase();
        }


        public MicroUserShowcase GetUserShowcaseById(int id)
        {
            return new UserInformationRules(dbContext).GetShowcaseById(id).ToUserShowcase();
        }

        public ReturnJsonModel DeleteUserShowcase(int id)
        {
            return new UserInformationRules(dbContext).DeleteShowCase(id);
        }


        public ReturnJsonModel AddEditShowcase(Showcase showcase)
        {
            return new UserInformationRules(dbContext).SaveShowCase(showcase, CurrentUser.Id, new S3ObjectUploadModel { FileKey = showcase.ImageUri });
        }
        #endregion

        #region Skill

        public List<MicroUserSkill> GetUserSkill(string id)
        {
            return new UserInformationRules(dbContext).GetSkillsByUser(id).ToUserSkill();
        }


        public MicroUserSkill GetUserSkillById(int id)
        {
            return new UserInformationRules(dbContext).GetUserSkillById(id).ToUserSkill();
        }

        public ReturnJsonModel DeleteUserSkill(int id)
        {
            return new UserInformationRules(dbContext).DeleteUserSkill(id);
        }


        public ReturnJsonModel AddEditSkill(Skill skill)
        {
            return new UserInformationRules(dbContext).SaveUserSkill(skill, CurrentUser.Id);
        }
        #endregion

        #region Work

        public List<MicroUserExperience> GetUserWorkExperiences(string id)
        {
            return new UserInformationRules(dbContext).GetListUserWorkExp(id).ToUserWorkExperience($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}", CurrentUser.Timezone);
        }

        public MicroUserExperience GetWorkExperienceById(int id)
        {
            return new UserInformationRules(dbContext).GetUserWorkExpById(id).ToUserWorkExperience($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}", CurrentUser.Timezone);
        }

        public ReturnJsonModel DeleteUserWorkExperience(int id)
        {
            return new UserInformationRules(dbContext).DeleteUserWorkExperience(id);
        }



        public ReturnJsonModel AddEditWorkExperience(WorkExperience work)
        {
            work.StartDate = work.StartDate.ConvertTimeToUtc(CurrentUser.Timezone);
            if (work.EndDate != null)
                work.EndDate = work.EndDate.ConvertTimeToUtc(CurrentUser.Timezone);
            return new UserInformationRules(dbContext).SaveUserWorkExp(work, CurrentUser.Id);
        }
        #endregion

        #region education
        public List<MicroUserExperience> GetUserEducationExperiences(string id)
        {
            return new UserInformationRules(dbContext).GetListUserEducationExp(id).ToUserEducationExperience($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}", CurrentUser.Timezone);
        }

        public MicroUserExperience GetEducationExperienceById(int id)
        {
            return new UserInformationRules(dbContext).GetUserEducationExpById(id).ToUserEducationExperience($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}", CurrentUser.Timezone);
        }
        public ReturnJsonModel DeleteEduExperience(int id)
        {
            return new UserInformationRules(dbContext).DeleteEduExperience(id);
        }

        public ReturnJsonModel AddEditEduExperience(EducationExperience education)
        {
            education.StartDate = education.StartDate.ConvertTimeToUtc(CurrentUser.Timezone);
            if (education.EndDate != null)
                education.EndDate = education.EndDate.ConvertTimeToUtc(CurrentUser.Timezone);

            return new UserInformationRules(dbContext).SaveEduExp(education, CurrentUser.Id);
        }
        #endregion

        #region Address

        public List<MicroUserAddress> GetUserAddresses(string id)
        {
            return dbContext.QbicleUser.Find(id)?.TraderAddresses.ToUserAddress();
        }

        public ReturnJsonModel AddEditUserAddress(TraderAddress address, CountryCode countryCode)
        {
            address.Country = new CountriesRules().GetCountryByCode(countryCode);
            var saveAddress = new TraderLocationRules(dbContext).SaveAddress(address);

            return new MyDesksRules(dbContext).AddUserAddress(CurrentUser.Id, saveAddress);
        }
        public ReturnJsonModel DeleteUserAddress(int id)
        {
            return new TraderLocationRules(dbContext).DeleteUserLocation(id);
        }

        public MicroUserAddress GetUserAddressId(int id)
        {
            return dbContext.TraderAddress.FirstOrDefault(p => p.Id == id).ToUserAddress();
        }
        #endregion

        #region public file

        public List<MicroUserFileShare> GetUserPublicFiles(string id)
        {
            return new UserInformationRules(dbContext).GetUserPublicFiles(id, "").ToUserPublicFiles();
        }
        public ReturnJsonModel AddEditUserPublicFile(MicroUserFileShare file)
        {
            //UserProfileFile profileFile,  ;
            var pfile = new UserProfileFile
            {
                Id = file.Id,
                Description = file.Description,
                Title = file.Name,
                StoredFileName = file.StoredFileName
            };
            var uploadModel = new S3ObjectUploadModel
            {
                FileKey = file.StoredFileName,
                FileName = file.Name,
                FileType = file.FileType.Extension
            };
            return new UserInformationRules(dbContext).SaveUserProfileFileMicro(pfile, uploadModel, CurrentUser.Id);
        }
        public ReturnJsonModel DeleteUserPublicFile(int id)
        {
            return new UserInformationRules(dbContext).DeleteUserProfileFile(id);
        }

        public MicroUserFileShare GetUserPublicFileId(int id)
        {
            return new UserInformationRules(dbContext).GetUserPublicFileById(id).ToUserPublicFiles();
        }
        #endregion
    }
}
