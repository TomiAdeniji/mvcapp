using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Reporting.WebForms;

namespace Qbicles.BusinessRules
{
    public static class ReportRules
    {
        public static byte[] RenderReport(List<ReportDataSource> dataSources, string reportFileName)
        {
            var startupPath = AppDomain.CurrentDomain.RelativeSearchPath;
            var reportPath = Path.Combine(startupPath, "ReportTemplates", reportFileName);


            var localReport = new LocalReport {ReportPath = reportPath };
            dataSources.ForEach(d =>
            {
                localReport.DataSources.Add(new ReportDataSource
                {
                    Name = d.Name,
                    Value = d.Value
                });
            });
            var reportType = "PDF";
            var mimeType = "";
            var endcoding = "";
            var filenameExtention = "pdf";
            Warning[] warnings;
            string[] stream;
            return localReport.Render(reportType, "", out mimeType, out endcoding, out filenameExtention, out stream,
                out warnings);
        }
    }
}