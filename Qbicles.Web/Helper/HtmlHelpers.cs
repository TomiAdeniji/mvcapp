using System.Web;
using System.Web.Optimization;

namespace Qbicles.Web.Helper
{
    public class HtmlHelpers
    {
        /// <summary>
        /// Localized JavaScript bundle 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static HtmlString LocalizedJsBundle(string fileName)
        {
            bool temp = true;
            if(!BundleTable.EnableOptimizations)
            {
                temp = false;
                BundleTable.EnableOptimizations = true;
            }
                
            var output = (HtmlString)Scripts
                .Render(fileName);
            if (!temp)
                BundleTable.EnableOptimizations = false;
            return output;
        }
    }
}