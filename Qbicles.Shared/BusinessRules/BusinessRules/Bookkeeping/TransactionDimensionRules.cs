using Qbicles.BusinessRules.Model;
using Qbicles.Models.Bookkeeping;
using System;
using System.Data.Entity;
using System.Linq;
using Qbicles.BusinessRules.Helper;
using Qbicles.Models;
using System.Collections.Generic;
using System.Reflection;

namespace Qbicles.BusinessRules
{
    public class TransactionDimensionRules
    {
        ApplicationDbContext dbContext;

        public TransactionDimensionRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public bool CheckExistName(TransactionDimension dimension)
        {
            var dimensions = dbContext.TransactionDimensions.Where(q => q.Name == dimension.Name && q.Domain.Id == dimension.Domain.Id);
            if (dimension.Id > 0 && dimensions.Any())
                dimensions = dimensions.Where(q => q.Id != dimension.Id);
            if (dimensions.Any()) return true;
            else return false;
        }


        public List<TransactionDimension> GetByDomainId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "TransactionDimension GetByDomainId", null, null, id);

                return dbContext.TransactionDimensions.Where(q => q.Domain.Id == id).OrderBy(n => n.Name).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }


        public List<TraderConfigurationModel> GetTransactionDimension2TraderReportingFilters(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "TransactionDimension GetByDomainId", null, null, id);

                return (from d in dbContext.TransactionDimensions
                                   where d.Domain.Id == id
                                   select new TraderConfigurationModel
                                   {
                                       Id = d.Id,
                                       Name = d.Name,
                                       CreatedDate = d.CreatedDate,
                                       CreatedBy = d.CreatedBy,
                                       CanDelete = d.DefaultDimensionTransactions.Count == 0 && d.TraderTransactionItems.Count == 0 && d.PosMenus.Count == 0 && d.TransactionItemLogs.Count == 0
                                   }).OrderBy(n => n.Name).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }
        }
        public TransactionDimension GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Dimenssion By Id", null, null, id);

                var dimension = dbContext.TransactionDimensions.Find(id);
                return dimension;


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }

        }
        public object GetOnlyById(int id, string dateFormat)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetOnlyById", null, null, id, dateFormat);


                var di = dbContext.TransactionDimensions.Find(id);
                if (di == null) return null;

                return new BaseItemModel
                {
                    Id = di.Id,
                    Name = di.Name,
                    CreatedBy = HelperClass.GetFullNameOfUser(di.CreatedBy),
                    CreatedDate = di.CreatedDate.ToString(dateFormat)
                };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id, dateFormat);
                return null;
            }

        }
        public ReturnJsonModel SaveDimension(TransactionDimension transactionDimension, string userId, QbicleDomain domain)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save Dimension", null, null, transactionDimension);


                if (transactionDimension != null && !string.IsNullOrEmpty(transactionDimension.Name))
                {

                    if (transactionDimension.CreatedBy == null)
                        transactionDimension.CreatedBy = dbContext.QbicleUser.Find(userId);
                    transactionDimension.Domain = domain;

                    if (CheckExistName(transactionDimension))
                    {
                        refModel.actionVal = 3;
                        refModel.msg = ResourcesManager._L("ERROR_MSG_632", transactionDimension.Name);
                        refModel.msgId = transactionDimension.Id.ToString();
                        refModel.msgName = transactionDimension.Name;
                        refModel.result = true;
                        return refModel;
                    }
                    if (transactionDimension.Id > 0)
                    {
                        var dimension = dbContext.TransactionDimensions.FirstOrDefault(q => q.Id == transactionDimension.Id);
                        dimension.Name = transactionDimension.Name;
                        if (dbContext.Entry(dimension).State == EntityState.Detached)
                            dbContext.TransactionDimensions.Attach(dimension);
                        dbContext.Entry(dimension).State = EntityState.Modified;
                        dbContext.SaveChanges();
                        refModel.actionVal = 2;
                        refModel.msgId = dimension.Id.ToString();
                        refModel.msgName = dimension.Name;
                    }
                    else
                    {

                        transactionDimension.CreatedDate = DateTime.UtcNow;
                        dbContext.TransactionDimensions.Add(transactionDimension);
                        dbContext.Entry(transactionDimension).State = EntityState.Added;
                        dbContext.SaveChanges();
                        refModel.actionVal = 1;
                        //append to select group
                        refModel.msgId = transactionDimension.Id.ToString();
                        refModel.msgName = transactionDimension.Name;
                    }
                    refModel.result = true;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, transactionDimension);
                refModel.msg = ResourcesManager._L("ERROR_MSG_5");
            }

            return refModel;
        }

        public bool DeleteDimension(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete Dimension", null, null, id);

                var dimension = dbContext.TransactionDimensions.Find(id);
                if (dimension == null) return false;
                dbContext.TransactionDimensions.Remove(dimension);
                dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }

        }
    }
}
