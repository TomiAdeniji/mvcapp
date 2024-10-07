using Qbicles.BusinessRules;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.BusinessRules.AdminListing;
using Qbicles.BusinessRules.BusinessRules.UserInformation;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.UserInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Qbicles.BusinessRules.HelperClass;

namespace Qbicles.Web.Controllers
{
    public class UserInformationController : BaseController
    {
        // GET: UserInformation
        public ActionResult Index()
        {
            return View();
        }


        #region ShowCase
        public ActionResult AddEditShowcaseView(string showcaseKey)
        {
            var showcaseId = 0;
            if (!string.IsNullOrEmpty(showcaseKey?.Trim()))
            {
                showcaseId = Int32.Parse(EncryptionService.Decrypt(showcaseKey));
            }
            var sc = new UserInformationRules(dbContext).GetShowcaseById(showcaseId);
            return PartialView("_ShowcaseAddEdit", sc);
        }

        public ActionResult ListShowCasesPartial(string keySearch)
        {
            var total = 0;
            var lstShowCases = new UserInformationRules(dbContext).GetListShowCase(CurrentUser().Id, keySearch, ref total);
            ViewBag.keySearch = keySearch;
            ViewBag.Total = total;
            return PartialView("_ListShowCasesPartialView", lstShowCases);
        }

        public ActionResult SaveShowCase(Showcase sc, S3ObjectUploadModel uploadedFile)
        {
            var saveResult = new UserInformationRules(dbContext).SaveShowCase(sc, CurrentUser().Id, uploadedFile);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteShowCase(string showcaseKey)
        {
            var showcaseId = 0;
            if (!string.IsNullOrEmpty(showcaseKey?.Trim()))
            {
                showcaseId = Int32.Parse(EncryptionService.Decrypt(showcaseKey));
            }
            var deleteResult = new UserInformationRules(dbContext).DeleteShowCase(showcaseId);
            return Json(deleteResult, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region User Skills
        public ActionResult AddEditSkillView(string skillKey)
        {
            var skillId = 0;
            if (!string.IsNullOrEmpty(skillKey?.Trim()))
            {
                skillId = Int32.Parse(EncryptionService.Decrypt(skillKey));
            }
            var skill = new UserInformationRules(dbContext).GetUserSkillById(skillId);
            return PartialView("_AddEditSkillView", skill);
        }

        public ActionResult UserSkillList()
        {
            var lstSkills = new UserInformationRules(dbContext).GetSkillsByUser(CurrentUser().Id);
            return PartialView("_UserSkillsList", lstSkills);
        }

        public ActionResult SaveUserSkill(Skill userSkill)
        {
            var saveResult = new UserInformationRules(dbContext).SaveUserSkill(userSkill, CurrentUser().Id);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteUserSkill(string skillKey)
        {
            var skillId = 0;
            if (!string.IsNullOrEmpty(skillKey?.Trim()))
            {
                skillId = Int32.Parse(EncryptionService.Decrypt(skillKey));
            }
            var deleteResult = new UserInformationRules(dbContext).DeleteUserSkill(skillId);
            return Json(deleteResult, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Work Exp
        public ActionResult UserWorkExpList()
        {
            var userInfo = CurrentUser();
            var lstExp = new UserInformationRules(dbContext).GetListUserExp(userInfo.Id, ExperienceType.WorkExperience, userInfo.Timezone);
            return PartialView("_ListUserExperiences", lstExp);
        }

        public ActionResult AddEditWorkExpView(string expKey)
        {
            var expId = 0;
            if (!string.IsNullOrEmpty(expKey?.Trim()))
            {
                expId = Int32.Parse(EncryptionService.Decrypt(expKey));
            }
            var currentExp = new UserInformationRules(dbContext).GetUserExpById(expId, CurrentUser().Timezone);
            ViewBag.dateformatStr = CurrentUser().DateFormat;
            return PartialView("_AddEditWorkExpView", currentExp);
        }

        public ActionResult SaveWorkExp(WorkExperience work, string startDate, string endDate, bool isCurrentStillWork)
        {
            var userSetting = CurrentUser();


            var startTime = startDate.ConvertDateFormat(userSetting.DateFormat).ConvertTimeToUtc(userSetting.Timezone);
            var endTime = string.IsNullOrEmpty(endDate) ? DateTime.UtcNow : endDate.ConvertDateFormat(userSetting.DateFormat).ConvertTimeToUtc(userSetting.Timezone);
            if (!string.IsNullOrEmpty(work.Key?.Trim()))
            {
                work.Id = Int32.Parse(EncryptionService.Decrypt(work.Key));
            }
            work.StartDate = startTime;
            work.EndDate = endTime;
            if (isCurrentStillWork)
            {
                work.EndDate = null;
            }

            var saveResult = new UserInformationRules(dbContext).SaveUserWorkExp(work, userSetting.Id);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Education Exp
        public ActionResult UserEduExpList()
        {
            var userInfo = CurrentUser();
            var lstExp = new UserInformationRules(dbContext).GetListUserExp(userInfo.Id, ExperienceType.EducationExperience, userInfo.Timezone);
            return PartialView("_ListUserExperiences", lstExp);
        }
        public ActionResult AddEditEduExpView(string expKey)
        {
            var expId = 0;
            if (!string.IsNullOrEmpty(expKey?.Trim()))
            {
                expId = Int32.Parse(EncryptionService.Decrypt(expKey));
            }
            var currentExp = new UserInformationRules(dbContext).GetUserExpById(expId, CurrentUser().Timezone);
            ViewBag.dateFormatStr = CurrentUser().DateFormat;
            return PartialView("_AddEditEduExpView", currentExp);
        }
        public ActionResult SaveEduExp(EducationExperience exp, string startdate, string enddate, bool isStillWorkHere)
        {
            var userSetting = CurrentUser();
            var startTime = startdate.ConvertDateFormat(userSetting.DateFormat).ConvertTimeToUtc(userSetting.Timezone);
            var endTime = string.IsNullOrEmpty(enddate) ? DateTime.UtcNow : enddate.ConvertDateFormat(userSetting.DateFormat).ConvertTimeToUtc(userSetting.Timezone); ;
            if (!string.IsNullOrEmpty(exp.Key?.Trim()))
            {
                exp.Id = Int32.Parse(EncryptionService.Decrypt(exp.Key));
            }
            exp.StartDate = startTime;
            exp.EndDate = endTime;

            if (isStillWorkHere)
                exp.EndDate = null;

            var saveResult = new UserInformationRules(dbContext).SaveEduExp(exp, userSetting.Id);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region User Profile File
        public ActionResult UserProfileFileList(string keySearch)
        {
            var lstFiles = new UserInformationRules(dbContext).GetUserPublicFiles(CurrentUser().Id, keySearch);
            return PartialView("_ListUserPublicFiles", lstFiles);
        }

        public ActionResult AddEditProfileFileView(string fileKey)
        {
            var fileId = 0;
            if (!string.IsNullOrEmpty(fileKey?.Trim()))
            {
                fileId = Int32.Parse(EncryptionService.Decrypt(fileKey));
            }
            var fileItem = new UserInformationRules(dbContext).GetUserPublicFileById(fileId);
            return PartialView("_AddEditPublicFileView", fileItem);
        }

        public ActionResult DeleteUserProfileFile(string fileKey)
        {
            var fileId = 0;
            if (!string.IsNullOrEmpty(fileKey?.Trim()))
            {
                fileId = Int32.Parse(EncryptionService.Decrypt(fileKey));
            }
            var deleteResult = new UserInformationRules(dbContext).DeleteUserProfileFile(fileId);
            return Json(deleteResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveProfileFile(UserProfileFile pfile, S3ObjectUploadModel uploadModel = null)
        {
            if (!string.IsNullOrEmpty(pfile.Key?.Trim()))
            {
                pfile.Id = Int32.Parse(EncryptionService.Decrypt(pfile.Key));
            }
            var saveResult = new UserInformationRules(dbContext).SaveUserProfileFile(pfile, uploadModel, CurrentUser().Id);
            return Json(saveResult, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region User Address
        public ActionResult ListUserTraderAddressesShow()
        {
            var currentUser = new UserRules(dbContext).GetById(CurrentUser().Id);
            var lstAddresses = currentUser.TraderAddresses;
            return PartialView("_ListUserTraderAddresses", lstAddresses);
        }
        public ActionResult SaveAddress(TraderAddress mdAddress, string country, string userId)
        {
            var _countryModel = new CountriesRules().GetCountryByName(country);
            if (!string.IsNullOrEmpty(mdAddress.Key?.Trim()))
            {
                mdAddress.Id = Int32.Parse(EncryptionService.Decrypt(mdAddress.Key));
            }
            mdAddress.Country = _countryModel;
            var address = new TraderLocationRules(dbContext).SaveAddress(mdAddress);
            if (address == null)
            {
                return Json(new ReturnJsonModel { result = false, msg = "Address does not exist." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var mdRules = new MyDesksRules(dbContext);
                var addAddressToMyDeskResult = mdRules.AddUserAddress(CurrentUser().Id, address);
                return Json(addAddressToMyDeskResult, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SetUserDefaultLocation(string addressKey)
        {
            var addressId = 0;
            if (!string.IsNullOrEmpty(addressKey?.Trim()))
            {
                addressId = Int32.Parse(EncryptionService.Decrypt(addressKey));
            }
            var result = new TraderLocationRules(dbContext).SetUserDefaultLocation(addressId, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteUserAddress(string addressKey)
        {
            var addressId = 0;
            if (!string.IsNullOrEmpty(addressKey?.Trim()))
            {
                addressId = Int32.Parse(EncryptionService.Decrypt(addressKey));
            }
            var result = new TraderLocationRules(dbContext).DeleteUserLocation(addressId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TraderAddressEditViewShow(string addressKey)
        {
            var addressId = 0;
            if (!string.IsNullOrEmpty(addressKey?.Trim()))
            {
                addressId = Int32.Parse(EncryptionService.Decrypt(addressKey));
            }
            var _address = new TraderAddress();
            if (addressId != 0)
            {
                _address = dbContext.TraderAddress.FirstOrDefault(p => p.Id == addressId);
            }
            //ViewBag.MdOrderCreatorId = CurrentUser().Id;
            ViewBag.Countries = new CountriesRules().GetAllCountries();
            return PartialView("_MdAddressAddEdit", _address);
        }
        #endregion

        #region Public Profile View
        public ActionResult PublicProfileView(string userId)
        {
            var user = CurrentUser();
            if (string.IsNullOrEmpty(userId))
            {
                userId = user.Id;
            }

            ViewBag.lstShortlistGroups = dbContext.ShortListGroups.Where(p => p.AssociatedUser.Id == user.Id).ToList();

            var showcaseTotal = 0;
            var lstShowcases = new UserInformationRules(dbContext).GetListShowCase(userId, string.Empty, ref showcaseTotal);
            ViewBag.Showcases = lstShowcases;
            var lstSkills = new UserInformationRules(dbContext).GetSkillsByUser(userId);
            ViewBag.Skills = lstSkills;
            var lstWorkExps = new UserInformationRules(dbContext).GetListUserExp(userId, ExperienceType.WorkExperience, user.Timezone);
            ViewBag.WorkExp = lstWorkExps;
            var lstEduExps = new UserInformationRules(dbContext).GetListUserExp(userId, ExperienceType.EducationExperience, user.Timezone);
            ViewBag.EduExp = lstEduExps;
            var lstUserFiles = new UserInformationRules(dbContext).GetUserPublicFiles(userId, string.Empty);
            ViewBag.PublicFiles = lstUserFiles;
            //var lstSharedQbicles = new QbicleRules(dbContext).GetQbiclesForProfileByUserId(user.Id, userId);
            //ViewBag.SharedQbicles = lstSharedQbicles ?? new List<Qbicle>();
            ViewBag.CurrentUserId = user.Id;
            return View();
        }

        public ActionResult GetSharedQbicles(PaginationRequest request, string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = CurrentUser().Id;
            }

            var lstResult = new QbicleRules(dbContext).GetQbiclesForProfileByUserIdPagination(request, CurrentUser().Id, userId);
            return Json(lstResult, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region User Profile Wizard
        public ActionResult UserProfileWizard()
        {            
            var user = dbContext.QbicleUser.Find(CurrentUser().Id);

            if (user.IsUserProfileWizardRun)
                return RedirectToAction("Index", "Domain");

            var notificationMethod =
                    EnumModel.ConvertEnumToList<Notification.NotificationSendMethodEnum>();
            var notificationSound =
                EnumModel.ConvertEnumToList<Notification.NotificationSound>();
            ViewBag.notificationMethod = notificationMethod;
            ViewBag.notificationSound = notificationSound;
            var listInterests = new AdminListingRules(dbContext).GetAllBusinessCategories();
            ViewBag.listBusinessCategories = listInterests;

            var tzs = TimeZoneInfo.GetSystemTimeZones();
            var timezoneList = tzs.Select(tz => new SelectListItem
            {
                Text = tz.DisplayName,
                Value = tz.Id
            }).ToList();
            ViewBag.TimezoneList = timezoneList;
            //new UserRules(dbContext).ToggleUserProfileWizardRun(userId);
            return View(user);
        }

        public ActionResult UserListAddressWizard()
        {
            var user = dbContext.QbicleUser.Find(CurrentUser().Id);
            var addrs = user.TraderAddresses;
            return PartialView("_WizardListAddressPartial", addrs);
        }

        public ActionResult UserListShowcaseWizard()
        {
            var total = 0;
            var showcases = new UserInformationRules(dbContext).GetListShowCase(CurrentUser().Id, string.Empty, ref total);
            return PartialView("_WizardListShowcasesPartial", showcases);
        }

        public ActionResult ShowBusinessByInterests(List<int> interestIds)
        {
            if (interestIds == null)
                return PartialView("_WizardListBusinesses", new List<BusinessProfileAndInterest>());

            var listB2BProfiles = new AdminListingRules(dbContext).GetB2BProfilesByInterests(interestIds, CurrentUser().Id);
            return PartialView("_WizardListBusinesses", listB2BProfiles);
        }
        public ActionResult UpdateWizardFinishedStep(UserWizardStep step)
        {
            var updateResult = new UserRules(dbContext).UpdateUserWizardStep(CurrentUser().Id, step, UserWizardPlatformType.Website);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}