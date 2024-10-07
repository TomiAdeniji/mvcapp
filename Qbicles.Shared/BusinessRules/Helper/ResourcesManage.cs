using Qbicles.BusinessRules.Resources;
using System;
using System.Globalization;
using System.Text;

namespace Qbicles.BusinessRules.Helper
{
    public static class ResourcesManager
    {
        public static string GetResourcesString()
        {
            var resourceSet = QbiclesResources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            var sbInitial = " var _errormessages = {";
            var resources = new StringBuilder(sbInitial);
            var resEnum = resourceSet.GetEnumerator();
            while (resEnum.MoveNext())
            {
                if (resources.ToString() != sbInitial)
                {
                    resources.Append(",");
                }
                resources.Append("\"" + resEnum.Key + "\":\"" + resEnum.Value.ToString().Replace("\r\n", "").Replace("\"", "\\\"") + "\"");
            }
            resources.Append("}");
            return resources.ToString();
        }




        /// <summary>
        /// get ErrorMessage by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string _L(string name)
        {
            return QbiclesResources.ResourceManager.GetString(name);
        }
        /// <summary>
        /// get ErrorMessage by name
        /// </summary>
        /// <param name="name">key</param>
        /// <param name="pars">paramaters string.format</param>
        /// <returns></returns>
        public static string _L(string name, object pars)
        {
            try
            {
                return string.Format(QbiclesResources.ResourceManager.GetString(name) ?? throw new InvalidOperationException(), pars);
            }
            catch
            {
                return name;
            }
        }

        /// <summary>
        /// get ErrorMessage by name
        /// </summary>
        /// <param name="name">key</param>
        /// <param name="pars">paramaters string.format</param>
        /// <returns></returns>
        public static string _L(string name, object[] pars)
        {
            try
            {
                return string.Format(QbiclesResources.ResourceManager.GetString(name) ?? throw new InvalidOperationException(), pars);
            }
            catch (Exception)
            {
                return name;
            }
        }
    }
}
