using Qbicles.BusinessRules.Helper;
using System.Web.Optimization;

namespace Qbicles.Web.Helper
{
    public class JsTranslator : IBundleTransform
    {
        #region IBundleTransform

        /// <summary>
        /// IBundleTransform Process method
        /// </summary>
        /// <param name="context">Bundle Context</param>
        /// <param name="response">Bundle Response</param>
        public void Process(BundleContext context, BundleResponse response)
        {
            var resources = ResourcesManager.GetResourcesString();
            string translated = response.Content.Replace("$ResourceValues$", resources);
            response.Content = translated;
        }

        #endregion
    }
}