//using Qbicles.BusinessRules.Helper;
//using Qbicles.BusinessRules.Model;
//using Qbicles.Models.B2B;
//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace Qbicles.BusinessRules.Commerce
//{
//    public class B2BWorkgroupRules
//    {
//        ApplicationDbContext dbContext;
//        public B2BWorkgroupRules(ApplicationDbContext context)
//        {
//            dbContext = context;
//        }
//        public B2BWorkgroup GetB2bWorkgroupId(int wgId)
//        {
//            try
//            {
//                if (ConfigManager.LoggingDebugSet)
//                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, wgId);
//                return dbContext.B2BWorkgroups.Find(wgId);
//            }
//            catch (Exception ex)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, wgId);
//                return new B2BWorkgroup();
//            }
//        }
//        public ReturnJsonModel DeleteB2bWorkgroupById(int id,int domainId,string currentUserId)
//        {
//            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
//            try
//            {
//                if (ConfigManager.LoggingDebugSet)
//                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, id);
//                var oldPermissions = GetPermissionsByDomainId(domainId, currentUserId).Select(s => s.Id).ToList();
//                var workgroup = dbContext.B2BWorkgroups.Find(id);
//                if (workgroup != null)
//                {
//                    dbContext.B2BWorkgroups.Remove(workgroup);
//                    returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
//                    #region Check reload the Commerce APP
//                    var newPermissions = GetPermissionsByDomainId(domainId, currentUserId).Select(s => s.Id).ToList();
//                    List<int> onlyInA = oldPermissions.Except(newPermissions).ToList();
//                    List<int> onlyInB = newPermissions.Except(oldPermissions).ToList();
//                    returnJson.Object = new { refresh = (onlyInA.Any() || onlyInB.Any()) };
//                    #endregion
//                }
//            }
//            catch (Exception ex)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
//            }
//            return returnJson;
//        }
//        public ReturnJsonModel SaveB2BWorkgroup(B2BWorkgroup model)
//        {
//            ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
//            try
//            {
//                if (ConfigManager.LoggingDebugSet)
//                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, model);
//                #region Check Workgroup Name is exists
//                var isExist = dbContext.B2BWorkgroups.Any(s=>s.Id!=model.Id&&s.Name==model.Name&&s.Domain.Id==model.Domain.Id);
//                if(isExist)
//                {
//                    returnJson.msg = ResourcesManager._L("ERROR_MSG_257");
//                    return returnJson;
//                }
//                #endregion
//                var oldPermissions = GetPermissionsByDomainId(model.Domain.Id, model.CreatedBy.Id).Select(s => s.Id).ToList();
//                var workgroup = dbContext.B2BWorkgroups.Find(model.Id);
//                if (workgroup != null)
//                {
//                    workgroup.Name = model.Name;
//                    workgroup.Location = dbContext.TraderLocations.Find(model.Location.Id);
//                    workgroup.SourceQbicle = dbContext.Qbicles.Find(model.SourceQbicle.Id);
//                    workgroup.DefaultTopic = dbContext.Topics.Find(model.DefaultTopic.Id);
//                    workgroup.LastUpdatedBy = model.LastUpdatedBy;
//                    workgroup.LastUpdateDate = DateTime.UtcNow;
//                    workgroup.Processes.Clear();
//                    foreach (var item in model.Processes)
//                    {
//                        var process = dbContext.B2BProcesses.Find(item.Id);
//                        if (process != null)
//                            workgroup.Processes.Add(process);
//                    }
//                    //Invites
//                    workgroup.Members.Clear();
//                    foreach (var item in model.Members)
//                    {
//                        var user = dbContext.QbicleUser.Find(item.Id);
//                        if (user != null)
//                            workgroup.Members.Add(user);
//                    }
//                    //Reviewers
//                    workgroup.Reviewers.Clear();
//                    foreach (var item in model.Reviewers)
//                    {
//                        var user = dbContext.QbicleUser.Find(item.Id);
//                        if (user != null)
//                            workgroup.Reviewers.Add(user);
//                    }
//                    //Approvers
//                    workgroup.Approvers.Clear();
//                    foreach (var item in model.Approvers)
//                    {
//                        var user = dbContext.QbicleUser.Find(item.Id);
//                        if (user != null)
//                            workgroup.Approvers.Add(user);
//                    }
                    
//                }
//                else
//                {
//                    workgroup = new B2BWorkgroup();
//                    workgroup.Name = model.Name;
//                    workgroup.Domain = model.Domain;
//                    workgroup.Location = dbContext.TraderLocations.Find(model.Location.Id);
//                    workgroup.SourceQbicle = dbContext.Qbicles.Find(model.SourceQbicle.Id);
//                    workgroup.DefaultTopic = dbContext.Topics.Find(model.DefaultTopic.Id);
//                    workgroup.CreatedBy = model.CreatedBy;
//                    workgroup.CreatedDate = DateTime.UtcNow;
//                    workgroup.LastUpdateDate = workgroup.CreatedDate;
//                    foreach (var item in model.Processes)
//                    {
//                        var process = dbContext.B2BProcesses.Find(item.Id);
//                        if (process != null)
//                            workgroup.Processes.Add(process);
//                    }
//                    //Invites
//                    foreach (var item in model.Members)
//                    {
//                        var user = dbContext.QbicleUser.Find(item.Id);
//                        if (user != null)
//                            workgroup.Members.Add(user);
//                    }
//                    //Reviewers
//                    foreach (var item in model.Reviewers)
//                    {
//                        var user = dbContext.QbicleUser.Find(item.Id);
//                        if (user != null)
//                            workgroup.Reviewers.Add(user);
//                    }
//                    //Approvers
//                    foreach (var item in model.Approvers)
//                    {
//                        var user = dbContext.QbicleUser.Find(item.Id);
//                        if (user != null)
//                            workgroup.Approvers.Add(user);
//                    }
//                    workgroup.LastUpdatedBy = model.CreatedBy;
//                    workgroup.LastUpdateDate = model.CreatedDate;
//                    dbContext.B2BWorkgroups.Add(workgroup);
//                }

//                returnJson.result = dbContext.SaveChanges() > 0 ? true : false;
//                #region Check reload the Commerce APP
//                var newPermissions = GetPermissionsByDomainId(model.Domain.Id, model.CreatedBy.Id).Select(s => s.Id).ToList();
//                List<int> onlyInA = oldPermissions.Except(newPermissions).ToList();
//                List<int> onlyInB = newPermissions.Except(oldPermissions).ToList();
//                returnJson.Object = new { refresh = (onlyInA.Any() || onlyInB.Any()) };
//                #endregion
//            }
//            catch (Exception ex)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, model);
//            }
//            return returnJson;
//        }
//        public List<B2BWorkgroup> GetB2bWorkgroups(int domainId,string keyword,int lid, List<int> process)
//        {
//            try
//            {
//                if (ConfigManager.LoggingDebugSet)
//                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId, keyword, lid, process);
//                var query = dbContext.B2BWorkgroups.Where(s => s.Domain.Id == domainId);
//                if(!string.IsNullOrEmpty(keyword))
//                {
//                    keyword = keyword.ToLower();
//                    query = query.Where(s => s.Name.ToLower().Contains(keyword));
//                }
//                if(lid>0)
//                {
//                    query = query.Where(s => s.Location.Id==lid);
//                }
//                if (process!=null&& process.Any())
//                {
//                    query = query.Where(s => s.Processes.Any(p=>process.Contains(p.Id)));
//                }
//                return query.ToList();
//            }
//            catch (Exception ex)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, keyword, lid, process);
//                return new List<B2BWorkgroup>();
//            }
//        }
//        public bool GetCheckPermission(int domainId,string currentUserId,string process)
//        {
//            try
//            {
//                if (ConfigManager.LoggingDebugSet)
//                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId, process);
//                return dbContext.B2BWorkgroups.Any(s=>s.Domain.Id==domainId&&s.Members.Any(m=>m.Id==currentUserId)&&s.Processes.Any(p=>p.Name==process));
//            }
//            catch (Exception ex)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, process);
//                return false;
//            }
//        }
//        public List<B2BProcess> GetPermissionsByDomainId(int domainId, string currentUserId)
//        {
//            try
//            {
//                if (ConfigManager.LoggingDebugSet)
//                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, domainId);
//                return dbContext.B2BWorkgroups.Where(s => s.Domain.Id == domainId&&s.Members.Any(m=>m.Id == currentUserId)).SelectMany(s=>s.Processes).Distinct().ToList();
//            }
//            catch (Exception ex)
//            {
//                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
//                return new List<B2BProcess>();
//            }
//        }
//    }
//}
