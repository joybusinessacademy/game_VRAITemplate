using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.PackageOperations
{
    public class ImportUnityAssetPackageAsync : CustomAsyncOperation
    {
        public string PackagePath { get; protected set; }
        public string PackageName { get; protected set; }
        public bool ShowManualImportGUI { get; protected set; }

        protected bool waitForImport;
        public ImportUnityAssetPackageAsync(string packageFilePath, bool interactive = false)
        {
            PackagePath = string.IsNullOrWhiteSpace(packageFilePath) ? "" : packageFilePath.Replace("\\", "/");
            PackageName = Path.GetFileNameWithoutExtension(PackagePath);
            ShowManualImportGUI = interactive;
        }

        ~ImportUnityAssetPackageAsync()
        {
            UnregisterImportPackageCallbacks();
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (string.IsNullOrWhiteSpace(PackagePath))
            {
                SetError("Package file path cannot be null or empty.");
                yield break;
            }

            RegisterImportPackageCallbacks();
            waitForImport = true;
            AssetDatabase.ImportPackage(PackagePath, ShowManualImportGUI);
            yield return new WaitWhile(() => waitForImport);
            UnregisterImportPackageCallbacks();
        }

        protected void RegisterImportPackageCallbacks()
        {
            AssetDatabase.importPackageCompleted += OnImportPakcageComplete;
            AssetDatabase.importPackageFailed += OnImportPakcageFail;
        }

        protected void UnregisterImportPackageCallbacks()
        {
            AssetDatabase.importPackageCompleted -= OnImportPakcageComplete;
            AssetDatabase.importPackageFailed -= OnImportPakcageFail;
        }

        protected void OnImportPakcageComplete(string receivedPackageName)
        {
            if (receivedPackageName != PackageName)
            {
                return;
            }
            waitForImport = false;
            Debug.Log($"Import Package Complete: {PackagePath}");
        }
        protected void OnImportPakcageFail(string receivedPackageName, string error)
        {
            if (receivedPackageName != PackageName)
            {
                return;
            }
            waitForImport = false;
            SetError(error);
        }
    }
}