using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.FileStorage;
using Qbicles.Models.Trader.PoS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.PoS
{
    public class PosTableRules
    {
        ApplicationDbContext dbContext;

        public PosTableRules(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public POSTableLayout GetPosTableLayoutByLocation(int locationId)
        {
            return dbContext.PosTableLayouts.FirstOrDefault(p => p.Location.Id == locationId);
        }

        public POSTable GetPosTableById(int posTableId)
        {
            return dbContext.PosTables.Find(posTableId);
        }

        public List<POSTable> GetListPosTableByLocation(int locationId)
        {
            return dbContext.PosTables.Where(p => p.Location.Id == locationId).OrderBy(p => p.Name).ToList();
        }

        public ReturnJsonModel SavePosTable(POSTable table, int traderLocationId, string userId)
        {
            ReturnJsonModel result = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, table, traderLocationId, userId);

                var listTablesByLocation = dbContext.PosTables.Where(p => p.Location.Id == traderLocationId).ToList();
                if (listTablesByLocation.Any(t => t.Id != table.Id && t.Name == table.Name))
                {
                    result.result = false;
                    result.msg = "Another Table with the same name existed in the location.";
                    return result;
                }

                if (table.Id <= 0)
                {
                    var traderLocation = dbContext.TraderLocations.Find(traderLocationId);

                    result.actionVal = 1;
                    table.Location = traderLocation;
                    table.CreatedBy = dbContext.QbicleUser.Find(userId);
                    table.CreatedDate = DateTime.UtcNow;
                    dbContext.PosTables.Add(table);
                    dbContext.Entry(table).State = System.Data.Entity.EntityState.Added;
                    result.result = dbContext.SaveChanges() > 0;
                }
                else
                {
                    var posTableInDb = dbContext.PosTables.Find(table.Id);
                    if (posTableInDb == null)
                    {
                        result.result = false;
                        result.msg = "Can not find PosTable to edit!";
                        return result;
                    }

                    posTableInDb.Name = table.Name;
                    posTableInDb.Summary = table.Summary;
                    result.actionVal = 2;
                    dbContext.Entry(posTableInDb).State = System.Data.Entity.EntityState.Modified;
                    result.result = dbContext.SaveChanges() > 0;
                }
                return result; 
            } catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, table, traderLocationId, userId);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public ReturnJsonModel DeletePosTable(int posTableId)
        {
            var result = new ReturnJsonModel() { actionVal = 3 };
            try
            {
                var posTableInDb = dbContext.PosTables.Find(posTableId);
                if(posTableInDb != null)
                {
                    dbContext.PosTables.Remove(posTableInDb);
                    dbContext.Entry(posTableInDb).State = System.Data.Entity.EntityState.Deleted;
                    result.result = dbContext.SaveChanges() > 0;
                }
                else
                {
                    result.result = true;
                }
                return result;

            } catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, posTableId);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }

        public ReturnJsonModel SavePosTableLayout(int locationId, string userId, S3ObjectUploadModel uploadModel)
        {
            var resultJson = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, uploadModel, locationId, userId);

                //Start: Processing with file uploaded
                var uploadedFile = new StorageFile();
                if (uploadModel != null)
                {
                    if (!string.IsNullOrEmpty(uploadModel.FileKey))
                    {
                        var s3Rules = new AzureStorageRules(dbContext);

                        s3Rules.ProcessingMediaS3(uploadModel.FileKey);
                    }
                    else
                    {
                        resultJson.result = false;
                        resultJson.msg = "Uploaded Table layout image failed.";
                        return resultJson;
                    }
                }
                else
                {
                    resultJson.result = false;
                    resultJson.msg = "The image of Table Layout is required.";
                    return resultJson;
                }
                //End: processing with file uploaded

                var diagramByLocation = dbContext.PosTableLayouts.FirstOrDefault(p => p.Location.Id == locationId);
                if (diagramByLocation == null)
                {
                    resultJson.actionVal = 1;
                    diagramByLocation = new POSTableLayout()
                    {
                        ImageUri = uploadModel.FileKey,
                        ImageType = uploadModel.FileName.Split('.').LastOrDefault()?.ToUpper() ?? "",
                        CreatedBy = dbContext.QbicleUser.Find(userId),
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow,
                        Location = dbContext.TraderLocations.FirstOrDefault(p => p.Id == locationId)
                    };
                    dbContext.PosTableLayouts.Add(diagramByLocation);
                    dbContext.Entry(diagramByLocation).State = System.Data.Entity.EntityState.Added;
                    resultJson.result = dbContext.SaveChanges() > 0;
                }
                else
                {
                    resultJson.actionVal = 2;
                    diagramByLocation.ImageType = uploadModel.FileName.Split('.').LastOrDefault()?.ToUpper() ?? "";
                    diagramByLocation.ImageUri = uploadModel.FileKey;
                    diagramByLocation.LastUpdatedDate = DateTime.UtcNow;
                    dbContext.Entry(diagramByLocation).State = System.Data.Entity.EntityState.Modified;
                    resultJson.result = dbContext.SaveChanges() > 0;
                }
                return resultJson;
            } catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, uploadModel, locationId);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }

        public ReturnJsonModel DeletePosTableLayout(int locationId)
        {
            var resultJson = new ReturnJsonModel() { actionVal = 3 };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, locationId);

                var diagramByLocation = dbContext.PosTableLayouts.FirstOrDefault(p => p.Location.Id == locationId);
                if (diagramByLocation != null)
                {
                    dbContext.PosTableLayouts.Remove(diagramByLocation);
                    dbContext.Entry(diagramByLocation).State = System.Data.Entity.EntityState.Deleted;
                    resultJson.result = dbContext.SaveChanges() > 0;
                }
                else
                {
                    resultJson.result = true;
                }
                return resultJson;
            }catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, null, locationId);
                resultJson.result = false;
                resultJson.msg = ResourcesManager._L("ERROR_MSG_5");
                return resultJson;
            }
        }
    }
}
