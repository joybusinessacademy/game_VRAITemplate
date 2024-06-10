using NUnit.Framework;
using SkillsVR.CCK.PackageManager.AsyncOperation;
using SkillsVR.CCK.PackageManager.AsyncOperation.CCKPackageOperations;
using SkillsVR.CCK.PackageManager.AsyncOperation.Registry;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using SkillsVR.CCK.PackageManager.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager
{
    public static class CCKPackageAutoInstaller
    {
        [MenuItem("Assets/SkillsVR CCK/Resolve Required CCK Packages")]
        [InitializeOnLoadMethod]
        private static void ResolveRequiredCCKPackages()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            InstallRequiredCCKPackages().StartCoroutine();
        }

        //[MenuItem("Assets/SkillsVR CCK/Reimport Default Setting Assets")]
        private static void ReimportDefaultSettingAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
            bool reload = EditorUtility.DisplayDialog("Reimport Default Setting Assets",
                "Warning: Confirming this action may result in existing settings files being reset to default values." +
                "\r\nAre you sure you want to proceed?", "Reimport Assets", "Cancel");

            if (!reload)
            {
                return;
            }
            PostImportExtraPackages().StartCoroutine();
        }

        private static IEnumerator PostImportExtraPackages()
        {
            throw new System.NotImplementedException();
            /*
            var pkgOp = new ListAllInstalledCCKPackagesAsync();
            yield return pkgOp;
            EditorApplication.LockReloadAssemblies();
            foreach (var pkg in pkgOp.Results)
            {
                yield return new PostProcessPackageImportExtraStorePackageFromDir(pkg.cckPackageInfo, pkg.localPackageInfo);
            }
            EditorApplication.UnlockReloadAssemblies();
            */
        }

        private static IEnumerator InstallRequiredCCKPackages()
        {
            yield return LocalPackageManager.Current.WaitReady();

            // In some cases the yield return above not really wait for ready.
            // manually check and wait here for sure.
            while(LocalPackageManager.Current.IsLoading)
            {
                yield return null;
            }

            var registrySettings = CCKProjectSettingsPackageManager.GetSettings().registries
                .Where(r => null != r
                        && !string.IsNullOrWhiteSpace(r.url)
                        && !string.IsNullOrWhiteSpace(r.name));

            List<CCKPackageInfo> notInstalledPackages = new List<CCKPackageInfo>();
            foreach (var registrySourceInfo in registrySettings)
            {
                var registry = CCKRegistryManager.Main.AddRegistry(registrySourceInfo);
                var loadLocalOp = new RefreshCCKRegistryFromLocalCache(registry);
                yield return loadLocalOp;
                if (loadLocalOp.State == OperationState.Failure)
                {
                    var downloadOp = new RefreshCCKRegistryRemote(registry);
                    yield return downloadOp;
                    downloadOp.TryLogError();
                }

                if (null != registry.packages)
                {
                    var pkgs = registry.packages
                        .Where(x => x.IsValid() 
                                 && x.HasFlag(Flags.Required) 
                                 && !LocalPackageManager.Current.IsInstalled(x.name));
                    notInstalledPackages.AddRange(pkgs);
                }
            }

            notInstalledPackages = notInstalledPackages
                .GroupBy(pkg => pkg.name)
                .Select(group => group.OrderByDescending(x => x.version).First())
                .Where(pkg => null != pkg && !pkg.IsInstalled())
                .OrderBy(x => x.GetValueInt(Flags.Order))
                .ToList();

            if (notInstalledPackages.Count() <= 0)
            {
                yield break;
            }

            EditorApplication.LockReloadAssemblies();
            EditorUtility.DisplayProgressBar("Init CCK Packages", "Please wait...", 0.0f);
            int installedCount = 0;
            int max = Mathf.Max(1, notInstalledPackages.Count());
            foreach (var pkg in notInstalledPackages)
            {
               
                string procTxt = installedCount + "/" + max;
                string pkgTxt = "Install " + pkg.displayName + " v" + pkg.version;
                float proc = (float)installedCount / (float)max;
                EditorUtility.DisplayProgressBar("Instal CCK Packages " + procTxt, pkgTxt, proc);
                yield return null;
                yield return null;
                yield return new InstallCCKPackageAsync(pkg);
                ++installedCount;
                yield return null;
            }
            EditorUtility.ClearProgressBar();
            EditorApplication.UnlockReloadAssemblies();
        }
    }
}