using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.SalesMkt;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.BusinessRules.MarketingSocial
{
    public class SalesMaketingSettingRules
    {
        private ApplicationDbContext _db;

        public SalesMaketingSettingRules(ApplicationDbContext context)
        {
            _db = context;
        }

        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            set
            {
                _db = value;
            }
        }

        public bool CheckSMIsSetupComplete(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check sale marketing setup complete", null, null, domainId);

                var smSetting = DbContext.SalesMarketingSettings.FirstOrDefault(d => d.Domain.Id == domainId);
                if (smSetting != null) return smSetting.IsSetupCompleted == SMSetupCurrent.SMApp;
                var setting = new Settings
                {
                    Domain = DbContext.Domains.Find(domainId),
                    IsSetupCompleted = SMSetupCurrent.Contacts
                };
                DbContext.SalesMarketingSettings.Add(setting);
                DbContext.SaveChanges();
                return false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return false;
            }
            

        }

        public Settings GetSMSettingByDomain(int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get sale marketing setting by domain id", null, null, domainId);

                return DbContext.SalesMarketingSettings.FirstOrDefault(d => d.Domain.Id == domainId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return null;
            }
        }

        public bool UpdateSMIsSettingComplete(int domainId, SMSetupCurrent currentStep)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update sale marketing setting to complete", null, null, domainId);

                var sm = DbContext.SalesMarketingSettings.FirstOrDefault(d => d.Domain.Id == domainId);
                if (sm == null)
                    return false;
                sm.IsSetupCompleted = currentStep;

                if (DbContext.Entry(sm).State == EntityState.Detached)
                    DbContext.SalesMarketingSettings.Attach(sm);
                DbContext.Entry(sm).State = EntityState.Modified;
                DbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainId);
                return false;
            }
        }
        public List<QbicleMedia> GetMediasSMByDomain(int did, string currentTimeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get medias Sales & Marketing", null, null, did, currentTimeZone);
                var setting = DbContext.SalesMarketingSettings.FirstOrDefault(s => s.Domain.Id == did);
                var folder = DbContext.MediaFolders.FirstOrDefault(s => s.Name == HelperClass.GeneralName && s.Qbicle.Id == setting.SourceQbicle.Id);
                var lstMedias = folder.Media.Where(s => s.FileType.Type == "Image File").ToList();
                return lstMedias.BusinessMapping(currentTimeZone);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, did, currentTimeZone);
                return new List<QbicleMedia>();
            }
        }
    }
}
