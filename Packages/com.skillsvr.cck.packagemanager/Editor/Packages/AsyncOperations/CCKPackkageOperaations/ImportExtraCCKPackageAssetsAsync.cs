using SkillsVR.CCK.PackageManager.AsyncOperation.PackageOperations;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using System.Collections;
using System.IO;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.CCKPackageOperations
{
    // Read the ExtImportDir from package flag;
    // Get the resolved dir;
    // Import all .unitypackage files under resolved dir to default location (Assets folder).
    public class ImportExtraCCKPackageAssetsAsync : CustomAsyncOperation
    {
        public string FolderDir { get; protected set; }
        public CCKPackageInfo Package { get; protected set; }

        public ImportExtraCCKPackageAssetsAsync(CCKPackageInfo package)
        {
            Package = package;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (null == Package)
            {
                SetError("CCK package info cannot be null or empty.");
                yield break;
            }

            var localPackageInfo = LocalPackageManager.Current.Get(Package.name);
            if (null == localPackageInfo)
            {
                SetError("No package info found. Install package first.\r\nPackage: " + Package);
                yield break;
            }
            var importDir = Package.GetValueString(Flags.ExtImportDir);

            if (string.IsNullOrWhiteSpace(importDir))
            {
                yield break;
            }

            var pkgDir = localPackageInfo.resolvedPath;
            FolderDir = Path.Combine(pkgDir, importDir);

            if (!Directory.Exists(FolderDir))
            {
                SetError("Not exists dir: " + FolderDir);
                yield break;
            }

            var pkgFiles = Directory.GetFiles(FolderDir, "*.unitypackage");
            if (null == pkgFiles || 0 == pkgFiles.Length)
            {
                yield break;
            }

            foreach (var pkg in pkgFiles)
            {
                var importOp = new ImportUnityAssetPackageAsync(pkg);
                yield return importOp;
            }
        }
    }
}