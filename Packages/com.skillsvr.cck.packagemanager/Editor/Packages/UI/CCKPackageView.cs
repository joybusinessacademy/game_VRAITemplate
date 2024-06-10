using SkillsVR.CCK.PackageManager.AsyncOperation.CCKPackageOperations;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class CCKPackageView : AutoBindingVisualElement<CCKPackageInfo>, INotifyValueChanged<CCKPackageInfo>
    {
        public new class UxmlFactory : UxmlFactory<CCKPackageView, UxmlTraits> { }

        [VisualBind]
        protected ScrollView AssetsListView { get; set; }

        [VisualBind]
        protected Label AssetsTitleLabel { get; set; }

        [VisualBind]
        protected Button ButtonInstall { get; set; }

        [VisualBind]
        protected Button ButtonUninstall { get; set; }


        [VisualBind]
        VisualElement PackageViewContainer;

        [VisualBind]
        Label NoPackageInfo;

        public CCKPackageInfo value 
        {
            get => BindingData;
            set
            {
                var oldValue = this.value;
                SetValueWithoutNotify(value);
                this.SendValueChangedEvent(oldValue, value);
            }
        }

        public CCKPackageView()
        {
            CreateGUI();
            RegisterCallback<DetachFromPanelEvent>(OnDetached);
            RegisterCallback<AttachToPanelEvent>(OnAttached);
        }

        public void SetValueWithoutNotify(CCKPackageInfo newValue)
        {
            this.BindingData = newValue;
            bool valid = newValue.IsValid();
            SetValidPackageView(valid);
            DisableActionButtons();
            if (!valid)
            {
                return;
            }
            this.schedule.Execute(UpdateAssets);
            LocalPackageManager.Current.WaitReady(RefreshActionButtons).StartCoroutine();
        }

        protected void SetValidPackageView(bool valid)
        {
            NoPackageInfo.SetDisplay(!valid);
            PackageViewContainer.SetDisplay(valid);
        }

        public void OnPackageChanged(ChangeEvent<CCKPackageInfo> evt)
        {
            value = evt.newValue;
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
            if (null == BindingData
                || string.IsNullOrWhiteSpace(packageName)
                || packageName != BindingData.name)
            {
                return;
            }
            RefreshActionButtons();
        }

        public void CreateGUI()
        {
            this.ReloadVisualTreeAssetByType();
        }

        protected void InstallPackage()
        {
            new InstallCCKPackageAsync(BindingData).StartCoroutine();
        }

        protected void UninstallPackage()
        {
            new UninstallCCKPackageAsyncOperation(BindingData).StartCoroutine();
        }

        protected void DisableActionButtons()
        {
            ButtonInstall.SetDisplay(false);
            ButtonUninstall.SetDisplay(false);
        }
        protected void RefreshActionButtons()
        {
            DisableActionButtons();
            if (null == BindingData || !BindingData.IsValid())
            {
                return;
            }
            bool installed = BindingData.IsInstalled();
            var installedVerion = BindingData.GetInstalledVersion();
            var sourceVersion = BindingData.version;
            bool hasUpdate = installedVerion != sourceVersion;
            string installButtonTitle = installed ? "Reload" : "Install";
            ButtonInstall.text = installed && hasUpdate ? "Update to v" + sourceVersion : installButtonTitle;

            ButtonInstall.SetDisplay(true);

            bool required = BindingData.HasFlag(Flags.Required);
            ButtonUninstall.SetDisplay(installed && !required);
        }

        protected void UpdateAssets()
        {
            var root = AssetsListView.Q("unity-content-container");
            root.Clear();

            var assets = null == BindingData ? null : BindingData.assets;
            if (null == assets || 0 == assets.Count())
            {
                AssetsTitleLabel.SetDisplay(false);
                return;
            }
            AssetsTitleLabel.SetDisplay(true);

            foreach (var asset in assets)
            {
                var view = new CCKAssetView();
                view.Show(asset);
                view.LitMode(true);
                view.style.marginTop = 0;
                root.Add(view);
            }

            AssetsListView.style.width = AssetsListView.parent.worldBound.width;
        }

        public override ScriptableObject<CCKPackageInfo> GetWrapper()
        {
            return ScriptableObject.CreateInstance<DataWrapper>();
        }

        

        [Serializable]
        public class DataWrapper : ScriptableObject<CCKPackageInfo> { }
    }
}