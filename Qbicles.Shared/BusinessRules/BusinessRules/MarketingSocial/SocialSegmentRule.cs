using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class SocialSegmentRule
    {
        #region init class
        private ApplicationDbContext _db;
        private readonly string _currentTimeZone = "";

        public SocialSegmentRule()
        {
        }
        public ApplicationDbContext DbContext
        {
            get => _db ?? new ApplicationDbContext();
            private set => _db = value;
        }
        public SocialSegmentRule(ApplicationDbContext context, string currentTimeZone = "")
        {
            _db = context;
            _currentTimeZone = currentTimeZone;
        }
        #endregion
        public ReturnJsonModel SaveSegment(SegmentCustomeModel segment, MediaModel media, string userId)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save segment", userId, null, segment, media);

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(DbContext);

                    s3Rules.ProcessingMediaS3(media.UrlGuid);
                }
                var user = DbContext.QbicleUser.Find(userId);
                var dbsegment = DbContext.SMSegments.Find(segment.Id);
                if (dbsegment != null)
                {
                    dbsegment.Name = segment.Name;
                    dbsegment.Summary = segment.Summary;
                    dbsegment.LastUpdateDate = dbsegment.CreatedDate;
                    dbsegment.LastUpdatedBy = user;
                    dbsegment.Type = segment.Type;
                    if (media != null && !string.IsNullOrEmpty(media.UrlGuid))
                    {
                        dbsegment.FeaturedImageUri = media.UrlGuid;
                    }

                    #region Areas
                    dbsegment.Areas.Clear();
                    if (segment.Areas != null)
                    {
                        foreach (var item in segment.Areas)
                        {
                            var dbarea = DbContext.SMAreas.Find(item);
                            if (dbarea != null)
                            {
                                dbsegment.Areas.Add(dbarea);
                            }
                        }
                    }
                    #endregion
                    #region Query Clause
                    DbContext.SMSegmentQueryClauses.RemoveRange(dbsegment.Clauses);
                    if (segment.Criterias != null)
                    {
                        foreach (var item in segment.Criterias)
                        {
                            SegmentQueryClause queryClause = new SegmentQueryClause();
                            queryClause.CriteriaDefinition = DbContext.SMCustomCriteriaDefinitions.Find(item.CriteriaId);
                            foreach (var op in item.CriteriaValues)
                            {
                                var _dbOptioVals = DbContext.SMCustomOptions.Find(op);
                                if (_dbOptioVals != null)
                                {
                                    queryClause.Options.Add(_dbOptioVals);
                                }
                            }
                            DbContext.SMSegmentQueryClauses.Add(queryClause);
                            DbContext.Entry(queryClause).State = EntityState.Added;
                            dbsegment.Clauses.Add(queryClause);
                        }
                    }
                    #endregion
                    #region Segment Contacts
                    dbsegment.Contacts.Clear();
                    if (segment.Contacts != null)
                    {
                        foreach (var item in segment.Contacts)
                        {
                            var _dbContact = DbContext.SMContacts.Find(item);
                            if (_dbContact != null)
                            {
                                dbsegment.Contacts.Add(_dbContact);
                            }
                        }
                    }
                    #endregion
                    if (DbContext.Entry(dbsegment).State == EntityState.Detached)
                        DbContext.SMSegments.Attach(dbsegment);
                    DbContext.Entry(dbsegment).State = EntityState.Modified;
                }
                else
                {
                    dbsegment = new Segment
                    {
                        Name = segment.Name,
                        Summary = segment.Summary,
                        Domain = segment.Domain,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = user,
                        LastUpdatedBy = user,
                        LastUpdateDate = DateTime.UtcNow
                    };


                    dbsegment.Type = segment.Type;
                    if (media != null && !string.IsNullOrEmpty(media.UrlGuid))
                    {
                        dbsegment.FeaturedImageUri = media.UrlGuid;
                    }
                    #region Areas
                    if (segment.Areas != null)
                    {
                        foreach (var item in segment.Areas)
                        {
                            var dbarea = DbContext.SMAreas.Find(item);
                            if (dbarea != null)
                            {
                                dbsegment.Areas.Add(dbarea);
                            }
                        }
                    }
                    #endregion
                    #region Query Clause
                    if (segment.Criterias != null)
                    {
                        foreach (var item in segment.Criterias)
                        {
                            SegmentQueryClause queryClause = new SegmentQueryClause();
                            queryClause.CriteriaDefinition = DbContext.SMCustomCriteriaDefinitions.Find(item.CriteriaId);
                            foreach (var op in item.CriteriaValues)
                            {
                                var _dbOptioVals = DbContext.SMCustomOptions.Find(op);
                                if (_dbOptioVals != null)
                                {
                                    queryClause.Options.Add(_dbOptioVals);
                                }
                            }
                            DbContext.SMSegmentQueryClauses.Add(queryClause);
                            DbContext.Entry(queryClause).State = EntityState.Added;
                            dbsegment.Clauses.Add(queryClause);
                        }
                    }
                    #endregion
                    #region Segment Contacts
                    if (segment.Contacts != null)
                    {
                        foreach (var item in segment.Contacts)
                        {
                            var _dbContact = DbContext.SMContacts.Find(item);
                            if (_dbContact != null)
                            {
                                dbsegment.Contacts.Add(_dbContact);
                            }
                        }
                    }
                    #endregion
                    DbContext.SMSegments.Add(dbsegment);
                    DbContext.Entry(dbsegment).State = EntityState.Added;
                }
                returnJson.result = DbContext.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, segment, media);
                returnJson.msg = ex.Message;
            }
            return returnJson;
        }
        public List<Area> GetAreas(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get areas", null, null, domainId);

                return DbContext.SMAreas.Where(s => s.Domain.Id == domainId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return new List<Area>();
            }
        }
        public List<Select2CustomeModel> GetOptionValuesByCriteriaId(int criteriaId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get option values by criteria id", null, null, criteriaId);

                var ops = DbContext.SMCustomOptions.Where(s => s.CustomCriteriaDefinition.Id == criteriaId).Select(s => new Select2CustomeModel { id = s.Id, text = s.Label });
                return ops.ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, criteriaId);
                return new List<Select2CustomeModel>();
            }
        }
        public List<Segment> LoadSegmentsByDomainId(int domainId, int skip, int take, string keyword, int[] types, bool isLoadingHide, ref int totalCount)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load segments by domain id", null, null, domainId, skip, take, keyword, types, isLoadingHide, totalCount);

                var query = DbContext.SMSegments.Where(s => s.Domain.Id == domainId);
                if (!isLoadingHide)
                {
                    query = query.Where(s => !s.IsHidden);
                }
                if (!string.IsNullOrEmpty(keyword))
                    query = query.Where(s => s.Name.Contains(keyword) || s.Summary.Contains(keyword));
                if (types != null && types.Any())
                {
                    var _types = types.Select(s => (SegmentType)s).ToList();
                    query = query.Where(s => _types.Any(a => a == s.Type));
                }
                if (take == 0)
                {
                    totalCount = query.Count();
                    return new List<Segment>();
                }
                else
                {
                    totalCount = 0;
                    return query.OrderByDescending(s => s.CreatedDate).Skip(skip).Take(take).ToList();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId, skip, take, keyword, types, isLoadingHide, totalCount);
                return new List<Segment>();
            }
        }
        public Segment GetSegmentById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get segment by id", null, null, id);

                return DbContext.SMSegments.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new Segment();
            }
        }

        public ReturnJsonModel ShowOrHideSegment(int id)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Show or hide segment", null, null, id);

                var segment = DbContext.SMSegments.FirstOrDefault(s => s.Id == id);
                segment.IsHidden = !segment.IsHidden;
                if (DbContext.Entry(segment).State == EntityState.Detached)
                    DbContext.SMSegments.Attach(segment);
                DbContext.Entry(segment).State = EntityState.Modified;
                returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return returnModel;
        }

        public int CountSegment(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count segment", null, null, domainId);

                return DbContext.SMSegments.Where(s => s.Domain.Id == domainId).Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return 0;
            }
        }
    }
}
