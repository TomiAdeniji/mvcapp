using System;
using System.IO;

namespace Qbicles.BusinessRules.Helper
{
    public class HtmlTemplate
    {
        private string _html;

        public HtmlTemplate(string templateName)
        {
            var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
            var pathTemplate = Path.Combine(startupPath, "Templates\\Activities", templateName);
            using (var reader = new StreamReader(templateName))
                _html = reader.ReadToEnd();
        }

        public string Render(object values)
        {
            string output = _html;
            foreach (var p in values.GetType().GetProperties())
                output = output.Replace("{" + p.Name + "}", (p.GetValue(values, null) as string) ?? string.Empty);
            return output;
        }
    }
}
