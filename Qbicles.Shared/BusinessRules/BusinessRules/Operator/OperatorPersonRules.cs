using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Operator.Team;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using static Qbicles.BusinessRules.Model.TB_Column;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorPersonRules
    {
        ApplicationDbContext dbContext;
        public OperatorPersonRules(ApplicationDbContext context)
        {
            dbContext = context;
        }
        public TeamPerson GetPersonById(int id)
        {
            try
            {
                return dbContext.OperatorTeamPersons.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }
        public ReturnJsonModel SaveTeamPerson(int id, string memberId, string userId, int[] lstRoleIds, int[] lstLocationIds, QbicleDomain domain)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (lstRoleIds == null || lstRoleIds.Count() == 0)
                {
                    refModel.msg = "ERROR_MSG_804";
                    return refModel;
                }

                if (lstLocationIds == null || lstLocationIds.Count() == 0)
                {
                    refModel.msg = "ERROR_MSG_805";
                    return refModel;
                }
                var dbPerson = dbContext.OperatorTeamPersons.Find(id);
                var lstOpLocations = dbContext.OperatorLocations.Where(o => lstLocationIds.Contains(o.Id)).ToList();
                var lstOpRoles = dbContext.OperatorRoles.Where(o => lstRoleIds.Contains(o.Id)).ToList();
                var member = dbContext.QbicleUser.Find(memberId);
                if (dbPerson != null)
                {
                    dbPerson.Locations.Clear();
                    dbPerson.Roles.Clear();
                    dbPerson.Locations.AddRange(lstOpLocations);
                    dbPerson.Roles.AddRange(lstOpRoles);
                    dbPerson.User = member;
                    if (dbContext.Entry(dbPerson).State == EntityState.Detached)
                        dbContext.OperatorTeamPersons.Attach(dbPerson);
                    dbContext.Entry(dbPerson).State = EntityState.Modified;
                }
                else
                {
                    dbPerson = new TeamPerson
                    {
                        Domain = domain
                    };
                    dbPerson.Locations.AddRange(lstOpLocations);
                    dbPerson.Roles.AddRange(lstOpRoles);
                    dbPerson.User = member;
                    dbPerson.IsHide = false;
                    dbContext.OperatorTeamPersons.Add(dbPerson);
                    dbContext.Entry(dbPerson).State = EntityState.Added;

                    #region Add Resource folder
                    var setting = new OperatorConfigRules(dbContext).getSettingByDomainId(domain.Id);
                    if (setting != null)
                    {
                        var folderName = AutoGenerateFolderName(setting.SourceQbicle);
                        var folder = dbContext.MediaFolders.FirstOrDefault(s => s.Name == folderName && s.Qbicle.Id == setting.SourceQbicle.Id);
                        if (folder == null)
                        {
                            folder = new MediaFolder
                            {
                                Name = folderName,
                                Qbicle = setting.SourceQbicle,
                                CreatedDate = DateTime.Now,
                                CreatedBy = dbContext.QbicleUser.Find(userId)
                            };
                            dbContext.MediaFolders.Add(folder);
                            dbContext.Entry(folder).State = EntityState.Added;
                        }
                        dbPerson.ResourceFolder = folder;
                    }

                    #endregion
                }
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
            }
            return refModel;
        }

        public DataTablesResponse SearchTeamPersons([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string teamPersonSearch, int teamPersonLocationSearch, int teamPersonRoleSearch, int domainId, string currentUserId, string dateFormat)
        {
            try
            {
                var query = dbContext.OperatorTeamPersons.Where(p => p.Domain.Id == domainId && !p.IsHide).AsQueryable();


                if (teamPersonLocationSearch != 0)
                {
                    query = query.Where(p => p.Locations.Any(l => l.Id == teamPersonLocationSearch));
                }

                if (teamPersonRoleSearch != 0)
                {
                    query = query.Where(p => p.Roles.Any(r => r.Id == teamPersonLocationSearch));
                }

                int totalPersons = query.Count();
                var newQuery = query.ToList().Select(q => new OperatorPersonModel
                {
                    Id = q.Id,
                    Name = HelperClass.GetFullNameOfUser(q.User, currentUserId),
                    MemberId = q.User.Id,
                    Avatar = q.User.ProfilePic.ToUriString(Enums.FileTypeEnum.Image, "T"),
                    Location = string.Join(", ", q.Locations.Select(l => l.Name)),
                    Role = string.Join(", ", q.Roles.Select(l => l.Name)),
                    Workgroup = string.Join(", ", q.User.TeamMembers.Where(s => s.WorkGroup.Domain.Id == domainId).Select(t => t.WorkGroup).Select(l => l.Name)),
                });

                if (!String.IsNullOrEmpty(teamPersonSearch))
                {
                    newQuery = newQuery.Where(t => t.Name.Trim().ToLower().Contains(teamPersonSearch.Trim().ToLower()) ||
                                              t.Location.Trim().ToLower().Contains(teamPersonSearch.Trim().ToLower()) ||
                                             t.Role.Trim().ToLower().Contains(teamPersonSearch.Trim().ToLower()) ||
                                             t.Workgroup.Trim().ToLower().Contains(teamPersonSearch.Trim().ToLower()));
                }

                var sortedColumn = requestModel.Columns.GetSortedColumns().FirstOrDefault();
                if (sortedColumn.Data.Equals("Person"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Name);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Name);
                    }
                }
                else if (sortedColumn.Data.Equals("Location"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Location);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Location);
                    }
                }
                else if (sortedColumn.Data.Equals("Role"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Role);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Role);
                    }
                }
                else if (sortedColumn.Data.Equals("Workgroup"))
                {
                    if (sortedColumn.SortDirection == OrderDirection.Ascendant)
                    {
                        newQuery = newQuery.OrderBy(s => s.Workgroup);
                    }
                    else
                    {
                        newQuery = newQuery.OrderByDescending(s => s.Workgroup);
                    }
                }
                var list = newQuery.Skip(requestModel.Start).Take(requestModel.Length).ToList();
                return new DataTablesResponse(requestModel.Draw, list, totalPersons, totalPersons);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0);
            }
        }

        public ReturnJsonModel RemoveTeamPerson(int id)
        {
            ReturnJsonModel refModel = new ReturnJsonModel { result = false };
            try
            {
                var person = dbContext.OperatorTeamPersons.Find(id);
                person.IsHide = true;
                refModel.result = dbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public string AutoGenerateFolderName(Qbicle qbicle)
        {
            try
            {
                if (qbicle != null)
                {
                    var random = new Random();
                    var randomNumber = random.Next(1, 999);
                    var sFolderName = "OP-ASSET-" + randomNumber.ToString("000");
                    for (int i = 0; i < 20; i++)
                    {
                        var isExist = dbContext.MediaFolders.Any(m => m.Qbicle.Id == qbicle.Id && m.Name == sFolderName);
                        if (!isExist)
                        {
                            return sFolderName;
                        }
                        else
                        {
                            sFolderName = "OP-ASSET-" + random.Next(1, 999).ToString("000");
                            continue;
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return "";
            }
        }

        private QbicleMedia AddMediaQbicle(MediaModel media, ApplicationUser user, Qbicle qbicle, MediaFolder folder, string name, string descript, Topic topic)
        {
            try
            {
                if (!string.IsNullOrEmpty(media.Name))
                {
                    //DbContext.Entry(media.Type).State = System.Data.Entity.EntityState.Modified;
                    //Media attach
                    var m = new QbicleMedia
                    {
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        Name = name,
                        FileType = media.Type,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic,
                        MediaFolder = folder,
                        Description = descript,
                        IsVisibleInQbicleDashboard = false
                    };
                    var versionFile = new VersionedFile
                    {
                        IsDeleted = false,
                        FileSize = media.Size,
                        UploadedBy = user,
                        UploadedDate = DateTime.UtcNow,
                        Uri = media.UrlGuid,
                        FileType = media.Type
                    };
                    m.VersionedFiles.Add(versionFile);

                    dbContext.Medias.Add(m);
                    dbContext.Entry(m).State = EntityState.Added;
                    return m;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public ReturnJsonModel SaveResource(MediaModel media, string userId, int qbicleId, int mediaFolderId, string name, string description, int topicId)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                var s3Rules = new Azure.AzureStorageRules(dbContext);
                s3Rules.ProcessingMediaS3(media.UrlGuid);

                var topic = new TopicRules(dbContext).GetTopicById(topicId);
                if (topic == null)
                {
                    refModel.msg = "Error not finding the current Topic!";
                    return refModel;
                }
                var folder = new MediaFolderRules(dbContext).GetMediaFolderById(mediaFolderId, qbicleId);
                if (folder == null)
                {
                    refModel.msg = "Error not finding the current Folder!";
                    return refModel;
                }
                var qbicle = dbContext.Qbicles.Find(qbicleId);
                if (qbicle == null)
                {
                    refModel.msg = "Error not finding the current Qbicle!";
                    return refModel;
                }
                var dbMedia = AddMediaQbicle(media, dbContext.QbicleUser.Find(userId), qbicle, folder, name, description, topic);
                if (dbMedia == null) return refModel;
                qbicle.Media.Add(dbMedia);
                if (dbContext.SaveChanges() > 0)
                    refModel.result = true;
                return refModel;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                refModel.msg = ex.Message;
                return refModel;
            }
        }

        public List<ApplicationUser> GetAllUsersAvailable(QbicleDomain domain, TeamPerson teamPerson)
        {
            try
            {
                var query = dbContext.OperatorTeamPersons.Where(p => p.Domain.Id == domain.Id).AsQueryable();
                var users = domain.Users.Where(u => !query.Any(q => q.User.Id == u.Id)).ToList();
                if (teamPerson != null)
                {
                    users.Add(teamPerson.User);
                }
                return users.OrderBy(u => u.Surname).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return new List<ApplicationUser>();
            }
        }

    }
}
