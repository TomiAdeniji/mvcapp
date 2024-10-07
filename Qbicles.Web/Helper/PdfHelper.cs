using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Qbicles.Web.Helper
{
    public sealed class PdfHelper
    {
        private PdfHelper()
        {
        }

        public static PdfHelper Instance { get; } = new PdfHelper();

        internal void SaveImageAsPdf(string imageFileName, string pdfFileName, int width = 600,
            bool deleteImage = false)
        {
            using (var document = new PdfDocument())
            {
                var page = document.AddPage();
                using (var img = XImage.FromFile(imageFileName))
                {
                    // Calculate new height to keep image ratio
                    var height = (int) (width / (double) img.PixelWidth * img.PixelHeight);

                    // Change PDF Page size to match image
                    page.Width = width;
                    page.Height = height;

                    var gfx = XGraphics.FromPdfPage(page);
                    gfx.DrawImage(img, 0, 0, width, height);
                }

                document.Save(pdfFileName);
            }

            if (deleteImage)
                File.Delete(imageFileName);
        }
    }
}