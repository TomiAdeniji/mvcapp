using System.IO.Compression;

namespace Qbicles.BusinessRules
{
    public static class ZipHelper
    {
        public static void ZippedFile(string sourceDirectory, string zipFileName)
        {
            //Creates a new, blank zip file to work with - the file will be
            //finalized when the using statement completes
            using (ZipArchive newFile = ZipFile.Open($"{sourceDirectory}/{zipFileName}.zip", ZipArchiveMode.Create))
            {
                //Here are two hard-coded files that we will be adding to the zip
                //file.  If you don't have these files in your system, this will
                //fail.  Either create them or change the file names.
                newFile.CreateEntryFromFile($"{sourceDirectory}/{zipFileName}.db", $"{zipFileName}.db", CompressionLevel.Fastest);
            }
        }
    }
}
