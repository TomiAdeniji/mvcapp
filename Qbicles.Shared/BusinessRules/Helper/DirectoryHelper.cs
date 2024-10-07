using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Qbicles.BusinessRules.Helper
{
    public static class DirectoryHelper
    {
        public static void CopyDir(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);
            if (!Directory.Exists(sourceDir))
                return;
            var mapping = new Dictionary<string, string>();
            var dir = new DirectoryInfo(sourceDir);
            /* collecting files and creating directory structure */
            LoopingIn(dir, file =>
            {
                var targetFile = file.FullName.Replace(sourceDir, targetDir);
                mapping.Add(file.FullName, targetFile);
            }, subdir =>
            {
                var targetPath = subdir.FullName.Replace(sourceDir, targetDir);
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
            });

            Parallel.ForEach(mapping, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, item =>
            {
                var fileInfo = new FileInfo(item.Key);
                if (fileInfo.Extension.ToLower() != ".zip")
                {
                    File.Copy(item.Key, item.Value, true);
                }
            });
        }

        public static void MoveDir(string sourceDir, string targetDir)
        {
            var source = new DirectoryInfo(sourceDir);
            foreach (var subDir in source.GetDirectories())
            {
                var target = Path.Combine(targetDir, subDir.Name);
                subDir.MoveTo(target);
            }

            foreach (var file in source.GetFiles())
            {
                var target = Path.Combine(targetDir, file.Name);
                file.MoveTo(target);
            }
        }

        private static void LoopingIn(DirectoryInfo dir, Action<FileInfo> fileAction, Action<DirectoryInfo> subDirAction)
        {
            foreach (var file in dir.GetFiles())
            {
                fileAction(file);
            }
            foreach (var subDir in dir.GetDirectories())
            {
                subDirAction(subDir);
                LoopingIn(subDir, fileAction, subDirAction);
            }
        }

        public static void CopyFile(string sourceFile, string targetFile)
        {
            if(File.Exists(targetFile))
                File.Delete(targetFile);
            File.Copy(sourceFile, targetFile, true);
        }

        public static void DeleteFile(string sourceFile)
        {
            if (File.Exists(sourceFile))
                File.Delete(sourceFile);
        }
    }
}
