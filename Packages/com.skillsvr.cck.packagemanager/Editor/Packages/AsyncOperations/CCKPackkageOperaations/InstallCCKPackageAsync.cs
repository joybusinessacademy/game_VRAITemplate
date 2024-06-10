using SkillsVR.CCK.PackageManager.AsyncOperation.PackageOperations;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.CCKPackageOperations
{
    // Read PkgResolver from package flags;
    // Get installer by resolver name;
    // Install package;
    // Try get flags and do post install process in order:
    //     - ExtImportDir: import extra .unitypackage assets;
    //     - Embed: embed package;
    //     - ReloadAfterInstall: reload scripts;
    public class InstallCCKPackageAsync : CustomAsyncOperation
    {
        public CCKPackageInfo Package { get; protected set; }
        protected string PackageIdOrName { get; set; }

        public InstallCCKPackageAsync(string packageIdOrName)
        {
            PackageIdOrName = packageIdOrName;
        }

        public InstallCCKPackageAsync(CCKPackageInfo package)
        {
            Package = package;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (null == Package)
            {
                Package = CCKPackageManagment.Main.GetPackageInfo(PackageIdOrName);
            }
            
            if (null == this.Package)
            {
                SetError("Cannot get CCK package by " + PackageIdOrName.ToText("id or name"));
                yield break;
            }

            string resolverName = Package.GetInhertValueString(Flags.PkgResolver);
            CustomAsyncOperation installer = null;
            switch(resolverName)
            {
                case PackageResolverKeys.AzureNpm: break;
                case PackageResolverKeys.Github: installer = new InstallPackageFromGithubAsync(Package); break;
                case PackageResolverKeys.Url: installer = new InstallPackageFromUrlAsync(Package); break;
                default:break;
            }

            if (null == installer)
            {
                SetError("Cannot get CCK package installer by " + resolverName.ToText("name"));
                yield break;
            }

            EditorApplication.LockReloadAssemblies();
            if (null != Package)
            {
                EditorUtility.DisplayProgressBar("Install Package...", Package.displayName, 0.99f);
                if (Package.IsEmbed())
                {
                    yield return new UninstallCCKPackageAsyncOperation(Package);
                }
                yield return installer.StartWithoutErrorBreak();
                installer.TryLogError();
                if (installer.State == OperationState.Success)
                {
                    yield return LocalPackageManager.Current.Reload();
                    while (null == LocalPackageManager.Current.Get(Package.name))
                    {
                        yield return null;
                    }
                    yield return PostProcessImportExtraAssets();
                    yield return PostProcessEmbed();
                }
                EditorUtility.ClearProgressBar();
            }
            EditorApplication.UnlockReloadAssemblies();
            this.TryLogError();
            PostProcessReload();
        }
        IEnumerator PostProcessImportExtraAssets()
        {
            var op = new ImportExtraCCKPackageAssetsAsync(Package);
            yield return op.StartWithoutErrorBreak();
            op.TryLogError();
        }
        IEnumerator PostProcessEmbed()
        {
            if (!Package.HasFlag(Flags.Embed))
            {
                yield break;
            }
            var op = new EmbedPackageAsync(Package.name);
            yield return op.StartWithoutErrorBreak();
            op.TryLogError();
        }

        void PostProcessReload()
        {
            if (!Package.HasFlag(Flags.ReloadAfterInstall))
            {
                return;
            }
            EditorApplication.UnlockReloadAssemblies();
            EditorUtility.RequestScriptReload();
        }
    }
}