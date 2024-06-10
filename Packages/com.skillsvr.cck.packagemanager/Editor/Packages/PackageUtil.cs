using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager
{
    public static class PackageUtil
    {
        private static string GetCallerFilePath([CallerFilePath] string path = "")
        {
            return path;
        }

        private static string GetMyPackageFolderPath()
        {
            var filePath = GetCallerFilePath();
            var projDir = Directory.GetCurrentDirectory();
            filePath = filePath.Substring(projDir.Length).TrimStart('/', '\\').Replace("\\", "/");

            string[] packageSourceDirRoots = new string[] {
            "Packages/",
            "Library/PackageCache/"
        };
            var prefix = packageSourceDirRoots.FirstOrDefault(x => filePath.StartsWith(x));
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new System.Exception("Get package folder name fail: The entry path is not belong to a package.\r\nEntry Path: " + filePath);
            }
            filePath = filePath.Substring(prefix.Length);

            int index = filePath.IndexOf('/');
            if (index > 0)
            {
                string folderName = filePath.Substring(0, index);
                return $"{prefix}{folderName}";
            }

            return filePath;
        }

        [Serializable]
        private class PkgInfo
        {
            public string name;
            public string version;
        }
        private static string GetMyPackageVersion()
        {
            var projDir = Directory.GetCurrentDirectory();
            var dir = GetMyPackageFolderPath();
            var pkgJsonPath = Path.Combine(projDir, dir, "package.json");
            var json = File.ReadAllText(pkgJsonPath);
            var pkgInfo = JsonUtility.FromJson<PkgInfo>(json);
            return pkgInfo.version;
        }
    }
}