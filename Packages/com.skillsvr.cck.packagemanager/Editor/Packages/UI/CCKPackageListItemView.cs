using SkillsVR.CCK.PackageManager.AsyncOperation.CCKPackageOperations;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using System;
using System.Collections;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class CCKPackageListItemView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<CCKPackageListItemView, UxmlTraits> { }

        public CCKPackageInfo Package { get; protected set; }
        public PackageInfo LocalPackageInfo { get; protected set; }
        public bool installed => null != LocalPackageInfo;

        public string InstalledVersion => LocalPackageInfo?.version ?? "0.0.0";
        public string RemoteVersion => Package?.version ?? "0.0.0";

        [VisualBind]
        protected VisualElement Icon { get; set; }

        [VisualBind]
        protected Label Title { get; set; }

        [VisualBind]
        protected Label Version { get; set; }

        [VisualBind]
        protected VisualElement InstallStateIcon { get; set; }

        protected static Texture2D iconInstalled = Resources.Load<Texture2D>("Icon/Check");
        protected static Texture2D iconProcessing = Resources.Load<Texture2D>("Icon/Timer");
        protected static Texture2D iconPackaged = Resources.Load<Texture2D>("Icon/PackageClosed");
        public CCKPackageListItemView()
        {
            CreateGUI();
            this.schedule.Execute(Update);

            RegisterCallback<DetachFromPanelEvent>(OnDetached);
            RegisterCallback<AttachToPanelEvent>(OnAttached);
        }

        protected void OnAttached(AttachToPanelEvent evt)
        {
            LocalPackageManager.Current.OnPackageRegistrationChanged += OnLocalPackageRegistrationChanged;
        }
        protected void OnDetached(DetachFromPanelEvent evt)
        {
            LocalPackageManager.Current.OnPackageRegistrationChanged -= OnLocalPackageRegistrationChanged;
        }

        private void OnLocalPackageRegistrationChanged(string packageName, PackageRegistrationState state)
        {
            if (null == Package
                || string.IsNullOrWhiteSpace(packageName)
                || packageName != Package.name)
            {
                return;
            }
            OnReload();
        }

        private void Update(TimerState obj)
        {
            UpdateTitleLength();
            this.schedule.Execute(Update).ExecuteLater(20);
        }

        public void SetPackage(CCKPackageInfo package)
        {
            Package = package;
            LocalPackageInfo = LocalPackageManager.Current.Get(Package?.name ?? null);
            Refresh();
            LocalPackageManager.Current.WaitReady(OnReload).StartCoroutine();
        }

        void OnReload()
        {
            LocalPackageInfo = LocalPackageManager.Current.Get(Package?.name ?? null);
            Refresh();
        }

        public void RefreshInstalledState()
        {
            InstallStateIcon.SetBackgroundImage(installed ? iconInstalled : null);
        }

        protected void RefreshActionButton()
        {
            string installButtonTitle = installed ? "Reload" : "Install";
            installButtonTitle = installed && InstalledVersion != RemoteVersion ? "Update to v" + RemoteVersion : installButtonTitle; 
        }

        protected void RequestInstallPackage()
        {
           InstallPackageRoutine().StartCoroutine();
        }

        protected IEnumerator InstallPackageRoutine()
        {
            yield return new InstallCCKPackageAsync(Package);
        }

        protected void RequestRemovePackage()
        {
            string title = "Remove Package - " + Package.displayName + " v" + InstalledVersion;
            string msg = "You will load all your changes (if any) if you delete a package in development.\r\n" +
                "Are you sure?";
            if (!EditorUtility.DisplayDialog(title, msg, "Yes, Remove Package", "No"))
            {
                return;
            }
            UninstallPackageRoutine().StartCoroutine();
        }

        protected IEnumerator UninstallPackageRoutine()
        {
            EditorApplication.LockReloadAssemblies();
            EditorUtility.DisplayProgressBar("Uninstall Package...", Package.displayName, 0.99f);
            yield return new UninstallCCKPackageAsyncOperation(Package);
            EditorUtility.ClearProgressBar();
            EditorApplication.UnlockReloadAssemblies();
        }

        protected IEnumerator WaitForRequest(Request request)
        {
            while(null != request && !request.IsCompleted)
            {
                yield return null;
            }
            if (null != request.Error)
            {
                Debug.LogError(request.Error.message);
            }
        }

        public void RefreshPreinstalledState()
        {
            bool preinstalled = Package.HasFlag(Flags.Required);
            Icon.SetBackgroundImage(preinstalled ? iconPackaged : null);
            Icon.tooltip = preinstalled ? "CCK dependence package, cannot be removed." : "";
        }

        public void Refresh()
        {
            Title.text = Package.displayName;
            Title.tooltip = Package.name;

            if (Package.IsEmbed())
            {
                Title.tooltip += "\r\nEmbed";
            }

            Version.text = installed ? InstalledVersion : RemoteVersion;

            RefreshInstalledState();
            RefreshPreinstalledState();
        }

        protected void CreateGUI()
        {
            this.ReloadVisualTreeAssetByType();
            Title.style.overflow = Overflow.Hidden;
            Title.style.textOverflow = TextOverflow.Ellipsis;
            Title.style.unityTextOverflowPosition = TextOverflowPosition.End;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateTitleLength();
        }

        protected void UpdateTitleLength()
        {
            if (this.worldBound.width <= 0)
            {
                return;
            }
            Title.style.maxWidth = worldBound.width - 32 - Version.worldBound.width;
            Title.style.width = Title.style.maxWidth;
        }
    }
}