namespace Qbicles.BusinessRules
{
    using Qbicles.BusinessRules.Helper;
    using Qbicles.BusinessRules.Model;
    using Qbicles.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection;

    public class FileTypeRules
    {
        ApplicationDbContext _db;

        public FileTypeRules(ApplicationDbContext context)
        {
            _db = context;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }
        /// <summary>
        /// Get all file type
        /// </summary>
        /// <returns>List<QbicleFileType></returns>
        public List<QbicleFileType> GetFileTypes()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get file types", null, null);

                return DbContext.QbicleFileTypes.OrderBy(e => e.Extension).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<QbicleFileType>();
            }
        }
        /// <summary>
        /// Get a File type by extenssion of file upload
        /// </summary>
        /// <param name="extension">string: extenssion file upload</param>
        /// <returns>QbicleFileType</returns>
        public QbicleFileType GetFileTypeByExtension(string extension)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get file by extension", null, null, extension);
                extension = extension.Replace(".", "");
                var fileType = DbContext.QbicleFileTypes.FirstOrDefault(e => e.Extension == extension);
                return fileType;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, extension);
                return new QbicleFileType();
            }
            finally
            {
            }
        }

        public List<string> GetExtension()
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get file by extension", null, null);
                return DbContext.QbicleFileTypes.Select(x => x.Extension).AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null);
                return new List<string>();
            }
        }
        public QbicleFileType GetFileTypeById(string ext)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get file type by id", null, null, ext);

                return DbContext.QbicleFileTypes.Find(ext);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, ext);
                return null;
            }
        }

        public List<string> GetExtension(string filetype)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get extension", null, null, filetype);

                return DbContext.QbicleFileTypes.Where(x => x.Type == filetype).Select(x => x.Extension).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, filetype);
                return new List<string>();
            }

        }

        public bool ValidFile(string filetype, string extenssion)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get extension", null, null, filetype, extenssion);

                var type = DbContext.QbicleFileTypes.Where(x => x.Type == filetype && x.Extension == extenssion).ToList();
                return type.Count > 0 ? true : false;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, filetype, extenssion);
                return false;
            }

        }

        public List<string> GetExtensionsByType(string filetype)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get extension", null, null, filetype);

                return DbContext.QbicleFileTypes.Where(x => x.Type == filetype).Select(e => e.Extension).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, filetype);
                return new List<string>();
            }
        }

    }
}
