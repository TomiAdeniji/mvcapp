using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.UserInformation;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.IO;
using Qbicles.BusinessRules.Azure;

namespace Qbicles.BusinessRules.BusinessRules.UserInformation
{
    public class UserInformationRules
    {
        ApplicationDbContext dbContext;
        public UserInformationRules(ApplicationDbContext context)
        {
            this.dbContext = context;
        }

        #region Showcase
        public List<Showcase> GetListShowCase(string userId, string searchKey, ref int total)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, searchKey);

                var showcaseList = from showcase in dbContext.Showcases where (showcase.AssociatedUser.Id == userId) select showcase;
                total = showcaseList.Count();
                if (!string.IsNullOrEmpty(searchKey))
                {
                    showcaseList = showcaseList.Where(p => p.Title.ToLower().Contains(searchKey.ToLower())
                                                    || p.Caption.ToLower().Contains(searchKey.ToLower()));
                }

                return showcaseList.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, searchKey);
                return new List<Showcase>();
            }
        }

        public Showcase GetShowcaseById(int scId)
        {
            return dbContext.Showcases.Find(scId);
        }

        public ReturnJsonModel SaveShowCase(Showcase showcase, string userId, S3ObjectUploadModel uploadModel = null)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, showcase, userId, uploadModel);




                if (showcase.Id <= 0)
                {
                    //Process with upload model
                    if (uploadModel != null && !string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var awsRules = new AzureStorageRules(dbContext);
                        awsRules.ProcessingMediaS3(uploadModel.FileKey);
                    }
                    var user = dbContext.QbicleUser.Find(userId);
                    showcase.CreatedBy = user;
                    showcase.CreatedDate = DateTime.UtcNow;
                    showcase.AssociatedUser = user;
                    showcase.ImageUri = uploadModel?.FileKey ?? "";
                    dbContext.Showcases.Add(showcase);
                    dbContext.Entry(showcase).State = EntityState.Added;
                    dbContext.SaveChanges();

                    return new ReturnJsonModel() { actionVal = 1, result = true, msgId = showcase.Id.ToString() };
                }
                else
                {
                    var showCaseInDb = dbContext.Showcases.Find(showcase.Id);
                    if (showCaseInDb != null)
                    {
                        //Process with upload model
                        if (uploadModel != null && !string.IsNullOrEmpty(uploadModel.FileKey))
                        {
                            if (showCaseInDb.ImageUri != uploadModel.FileKey)
                            {
                                var awsRules = new AzureStorageRules(dbContext);
                                awsRules.ProcessingMediaS3(uploadModel.FileKey);
                                showCaseInDb.ImageUri = uploadModel.FileKey;
                            }
                        }

                        showCaseInDb.Title = showcase.Title; ;
                        showCaseInDb.Caption = showcase.Caption;
                        dbContext.Entry(showCaseInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();
                    }

                    return new ReturnJsonModel() { actionVal = 2, result = true, msgId = showcase.Id.ToString() };
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, showcase, userId, uploadModel);
                return new ReturnJsonModel()
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_5")
                };
            }


        }

        public ReturnJsonModel DeleteShowCase(int showcaseId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, showcaseId);
                var showcaseInDb = dbContext.Showcases.Find(showcaseId);
                if (showcaseInDb != null)
                {
                    dbContext.Showcases.Remove(showcaseInDb);
                    dbContext.Entry(showcaseInDb).State = EntityState.Deleted;
                    dbContext.SaveChanges();
                }
                return new ReturnJsonModel() { actionVal = 3, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, showcaseId);
                return new ReturnJsonModel() { actionVal = 3, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }
        #endregion

        #region Skills
        public List<Skill> GetSkillsByUser(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId);
                }

                var query = dbContext.Skills.Where(p => p.AssociatedUser.Id == userId).OrderBy(p => p.Proficiency)
                                .ThenBy(p => p.Name).ThenByDescending(p => p.CreatedDate);
                return query.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return new List<Skill>();
            }
        }

        public ReturnJsonModel SaveUserSkill(Skill userSkill, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userSkill, currentUserId);


                if (userSkill.Id <= 0)
                {
                    var currentUser = dbContext.QbicleUser.Find(currentUserId);
                    userSkill.AssociatedUser = currentUser;
                    userSkill.CreatedBy = currentUser;
                    userSkill.CreatedDate = DateTime.UtcNow;
                    dbContext.Skills.Add(userSkill);
                    dbContext.Entry(userSkill).State = EntityState.Added;
                    dbContext.SaveChanges();

                    return new ReturnJsonModel()
                    {
                        actionVal = 1,
                        msgId = userSkill.Id.ToString(),
                        result = true
                    };
                }
                else
                {
                    var userSkillInDb = dbContext.Skills.Find(userSkill.Id);
                    if (userSkillInDb != null)
                    {
                        userSkillInDb.Name = userSkill.Name;
                        userSkillInDb.Proficiency = userSkill.Proficiency;
                        dbContext.Entry(userSkillInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        return new ReturnJsonModel()
                        {
                            actionVal = 2,
                            msgId = userSkill.Id.ToString(),
                            result = true
                        };
                    }
                    else
                    {
                        return new ReturnJsonModel() { actionVal = 2, result = false, msg = "Can not find User Skill to edit." };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, userSkill, currentUserId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public ReturnJsonModel DeleteUserSkill(int skillId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, skillId);

                var skillInDb = dbContext.Skills.Find(skillId);
                if (skillInDb != null)
                {
                    dbContext.Skills.Remove(skillInDb);
                    dbContext.Entry(skillInDb).State = EntityState.Deleted;
                    dbContext.SaveChanges();
                }
                return new ReturnJsonModel() { actionVal = 3, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, skillId);
                return new ReturnJsonModel() { actionVal = 3, msg = ResourcesManager._L("ERROR_MSG_5"), result = false };
            }
        }

        public Skill GetUserSkillById(int skillId)
        {
            return dbContext.Skills.Find(skillId);
        }
        #endregion

        #region Experience
        public List<Experience> GetListUserExp(string userId, ExperienceType expType, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId);
                }
                var query = from userExp in dbContext.Experiences where userExp.AssociatedUser.Id == userId && userExp.Type == expType select userExp;
                var lstUserExps = query.OrderByDescending(p => p.CreatedDate).ToList();
                lstUserExps.ForEach(p =>
                {
                    p.StartDate = p.StartDate.ConvertTimeFromUtc(timeZone);
                    p.EndDate = p.EndDate?.ConvertTimeFromUtc(timeZone) ?? null;
                });
                return query.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return new List<Experience>();
            }
        }

        public Experience GetUserExpById(int expId, string timeZone)
        {
            var userExp = dbContext.Experiences.Find(expId);
            if (userExp != null)
            {
                userExp.StartDate = userExp.StartDate.ConvertTimeFromUtc(timeZone);
                if (userExp.EndDate != null)
                {
                    userExp.EndDate = userExp.EndDate.ConvertTimeFromUtc(timeZone);
                }
            }
            return userExp;
        }

        public List<WorkExperience> GetListUserWorkExp(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId);
                }
                var query = from userExp in dbContext.WorkExperiences where userExp.AssociatedUser.Id == userId select userExp;

                return query.OrderByDescending(c => c.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return new List<WorkExperience>();
            }
        }

        public List<EducationExperience> GetListUserEducationExp(string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                {
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId);
                }
                var query = from userExp in dbContext.EducationExperiences where userExp.AssociatedUser.Id == userId select userExp;

                return query.OrderByDescending(c => c.CreatedDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return new List<EducationExperience>();
            }
        }

        public WorkExperience GetUserWorkExpById(int id)
        {
            return dbContext.WorkExperiences.FirstOrDefault(e => e.Id == id);
        }

        public EducationExperience GetUserEducationExpById(int id)
        {
            return dbContext.EducationExperiences.FirstOrDefault(e => e.Id == id);
        }

        public ReturnJsonModel SaveUserWorkExp(WorkExperience workExp, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, workExp, currentUserId);


                if (workExp.Id <= 0)
                {
                    var currentUser = dbContext.QbicleUser.Find(currentUserId);
                    workExp.AssociatedUser = currentUser;
                    workExp.CreatedBy = currentUser;
                    workExp.CreatedDate = DateTime.UtcNow;
                    workExp.Type = ExperienceType.WorkExperience;

                    dbContext.WorkExperiences.Add(workExp);
                    dbContext.Entry(workExp).State = EntityState.Added;
                    dbContext.SaveChanges();

                    return new ReturnJsonModel() { actionVal = 1, msgId = workExp.Id.ToString(), result = true };
                }
                else
                {
                    var workExpInDb = dbContext.WorkExperiences.Find(workExp.Id);
                    if (workExpInDb != null)
                    {
                        workExpInDb.Company = workExp.Company;
                        workExpInDb.Role = workExp.Role;
                        workExpInDb.StartDate = workExp.StartDate;
                        workExpInDb.EndDate = workExp.EndDate;

                        workExpInDb.Summary = workExp.Summary;

                        dbContext.Entry(workExpInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        return new ReturnJsonModel() { actionVal = 2, msgId = workExp.Id.ToString(), result = true };
                    }
                    else
                    {
                        return new ReturnJsonModel() { actionVal = 2, result = false, msg = "Can not find the WorkExperience !" };
                    }
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, workExp, currentUserId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public ReturnJsonModel DeleteUserWorkExperience(int workExpId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, workExpId);
                var workExpInDb = dbContext.WorkExperiences.Find(workExpId);
                if (workExpInDb != null)
                {
                    dbContext.WorkExperiences.Remove(workExpInDb);
                    dbContext.Entry(workExpInDb).State = EntityState.Deleted;
                    dbContext.SaveChanges();
                }
                return new ReturnJsonModel()
                {
                    actionVal = 3,
                    result = true
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, workExpId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5"), actionVal = 3 };
            }
        }

        public ReturnJsonModel SaveEduExp(EducationExperience eduExp, string userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, eduExp, userId);



                if (eduExp.Id <= 0)
                {
                    var currentUser = dbContext.QbicleUser.Find(userId);
                    eduExp.AssociatedUser = currentUser;
                    eduExp.CreatedBy = currentUser;
                    eduExp.CreatedDate = DateTime.UtcNow;
                    eduExp.Type = ExperienceType.EducationExperience;

                    dbContext.EducationExperiences.Add(eduExp);
                    dbContext.Entry(eduExp).State = EntityState.Added;
                    dbContext.SaveChanges();
                    return new ReturnJsonModel() { actionVal = 1, result = true };
                }
                else
                {
                    var eduExpInDb = dbContext.EducationExperiences.Find(eduExp.Id);
                    if (eduExpInDb != null)
                    {
                        eduExpInDb.Institution = eduExp.Institution;
                        eduExpInDb.Course = eduExp.Course;
                        eduExpInDb.StartDate = eduExp.StartDate;
                        eduExpInDb.EndDate = eduExp.EndDate;
                        eduExpInDb.Summary = eduExp.Summary;

                        dbContext.Entry(eduExpInDb).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        return new ReturnJsonModel() { actionVal = 2, msgId = eduExp.Id.ToString(), result = true };
                    }
                    else
                    {
                        return new ReturnJsonModel() { actionVal = 2, result = false, msg = "Can not find Education Experience." };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, eduExp, userId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public ReturnJsonModel DeleteEduExperience(int eduExpId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, eduExpId);

                var eduExpInDb = dbContext.EducationExperiences.Find(eduExpId);
                if (eduExpInDb != null)
                {
                    dbContext.EducationExperiences.Remove(eduExpInDb);
                    dbContext.Entry(eduExpInDb).State = EntityState.Deleted;
                    dbContext.SaveChanges();
                }
                return new ReturnJsonModel()
                {
                    actionVal = 3,
                    result = true
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, eduExpId);
                return new ReturnJsonModel() { actionVal = 3, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }
        #endregion

        #region Public Files
        public List<UserProfileFile> GetUserPublicFiles(string userId, string keySearch="")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId, keySearch);

                var query = from files in dbContext.UserProfileFiles where files.AssociatedUser.Id == userId select files;
                if (!string.IsNullOrEmpty(keySearch))
                {
                    keySearch = keySearch.Trim();
                    query = query.Where(p => p.StoredFileName.ToLower().Contains(keySearch.ToLower()) || p.Description.ToLower().Contains(keySearch.ToLower()));
                }

                return query.ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, keySearch);
                return new List<UserProfileFile>();
            }
        }

        public UserProfileFile GetUserPublicFileById(int fileId)
        {
            return dbContext.UserProfileFiles.FirstOrDefault(e => e.Id == fileId);
        }

        public ReturnJsonModel DeleteUserProfileFile(int fileId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, fileId);

                var fileInDb = dbContext.UserProfileFiles.Find(fileId);
                if (fileInDb != null)
                {
                    dbContext.UserProfileFiles.Remove(fileInDb);
                    dbContext.Entry(fileInDb).State = EntityState.Deleted;
                    dbContext.SaveChanges();
                }
                return new ReturnJsonModel() { actionVal = 3, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, fileId);
                return new ReturnJsonModel() { actionVal = 3, result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        public ReturnJsonModel SaveUserProfileFile(UserProfileFile profileFile, S3ObjectUploadModel uploadModel, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, profileFile, uploadModel, currentUserId);
                //Process uploaded file
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);
                        s3Rules.ProcessingMediaS3(uploadModel.FileKey);
                    }

                    profileFile.StoredFileName = uploadModel.FileKey;
                    profileFile.FileType = new FileTypeRules(dbContext).GetFileTypeByExtension(Path.GetExtension(uploadModel.FileName));
                }
                var currentUser = dbContext.QbicleUser.Find(currentUserId);

                if (profileFile.Id <= 0)
                {
                    profileFile.CreatedDate = DateTime.UtcNow;
                    profileFile.AssociatedUser = currentUser;

                    dbContext.UserProfileFiles.Add(profileFile);
                    dbContext.Entry(profileFile).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
                else
                {
                    var fileInDb = dbContext.UserProfileFiles.Find(profileFile.Id);
                    if (fileInDb == null)
                    {
                        return new ReturnJsonModel() { actionVal = 2, result = false, msg = "Can not find User Profile File." };
                    }
                    if (uploadModel != null && !string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        fileInDb.StoredFileName = profileFile.StoredFileName;
                        fileInDb.FileType = profileFile.FileType;
                    }
                    fileInDb.Title = profileFile.Title;
                    fileInDb.Description = profileFile.Description;

                    dbContext.Entry(fileInDb).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                return new ReturnJsonModel() { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, profileFile, uploadModel, currentUserId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }


        public ReturnJsonModel SaveUserProfileFileMicro(UserProfileFile profileFile, S3ObjectUploadModel uploadModel, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, profileFile, uploadModel, currentUserId);
                //Process uploaded file
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var s3Rules = new Azure.AzureStorageRules(dbContext);
                        s3Rules.ProcessingMediaS3(uploadModel.FileKey);
                    }

                    profileFile.StoredFileName = uploadModel.FileKey;
                    profileFile.FileType = new FileTypeRules(dbContext).GetFileTypeByExtension(uploadModel.FileType);
                }
                var currentUser = dbContext.QbicleUser.Find(currentUserId);

                if (profileFile.Id <= 0)
                {
                    profileFile.CreatedDate = DateTime.UtcNow;
                    profileFile.AssociatedUser = currentUser;

                    dbContext.UserProfileFiles.Add(profileFile);
                    dbContext.Entry(profileFile).State = EntityState.Added;
                    dbContext.SaveChanges();
                }
                else
                {
                    var fileInDb = dbContext.UserProfileFiles.Find(profileFile.Id);
                    if (fileInDb == null)
                    {
                        return new ReturnJsonModel() { actionVal = 2, result = false, msg = "Can not find User Profile File." };
                    }
                    if (uploadModel != null && !string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        fileInDb.StoredFileName = profileFile.StoredFileName;
                        fileInDb.FileType = profileFile.FileType;
                    }
                    fileInDb.Title = profileFile.Title;
                    fileInDb.Description = profileFile.Description;

                    dbContext.Entry(fileInDb).State = EntityState.Modified;
                    dbContext.SaveChanges();
                }
                return new ReturnJsonModel() { result = true,msgId= profileFile.Id .ToString()};
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, profileFile, uploadModel, currentUserId);
                return new ReturnJsonModel() { result = false, msg = ResourcesManager._L("ERROR_MSG_5") };
            }
        }

        #endregion

    }
}
