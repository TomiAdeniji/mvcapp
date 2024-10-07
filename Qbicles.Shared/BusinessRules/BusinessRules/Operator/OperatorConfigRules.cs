using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Operator;
using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Qbicles.BusinessRules.Operator
{
    public class OperatorConfigRules
    {
        ApplicationDbContext dbContext;
        public OperatorConfigRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public OperatorSetting getSettingByDomainId(int domainId)
        {
            try
            {
                return dbContext.OperatorSettings.FirstOrDefault(s => s.Domain.Id == domainId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(),ex);
                return null;
            }
        }

        public ReturnJsonModel UpdateSetting(int id, QbicleDomain domain, int topicId, int qbicleId, string userId)
        {
            ReturnJsonModel refModel = new ReturnJsonModel { result = false };
            try
            {
                var setting = dbContext.OperatorSettings.Find(id);
                var sourceQbicle = dbContext.Qbicles.Find(qbicleId);
                if (sourceQbicle == null)
                {
                    refModel.msg = "No found the Qbicle!";
                    return refModel;
                }
                var defaultTopic = dbContext.Topics.Find(topicId);
                if (defaultTopic == null)
                {
                    refModel.msg = "No found the Topic!";
                    return refModel;
                }
                if (setting != null)
                {
                    setting.SourceQbicle = sourceQbicle;
                    setting.DefaultTopic = defaultTopic;
                    if (dbContext.Entry(setting).State == EntityState.Detached)
                        dbContext.OperatorSettings.Attach(setting);
                    dbContext.Entry(setting).State = EntityState.Modified;
                }
                else
                {
                    setting = new OperatorSetting();
                    setting.SourceQbicle = sourceQbicle;
                    setting.DefaultTopic = defaultTopic;
                    setting.Domain = domain;
                    setting.CreatedBy = dbContext.QbicleUser.Find(userId);
                    setting.CreatedDate = DateTime.UtcNow;
                    dbContext.OperatorSettings.Add(setting);
                    dbContext.Entry(setting).State = EntityState.Added;
                }

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

    }
}
