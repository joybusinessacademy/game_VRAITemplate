using SkillsVR.CCK.PackageManager.AsyncOperation;
using SkillsVR.CCK.PackageManager.AsyncOperation.CCKAssetPreview;
using SkillsVR.CCK.PackageManager.AsyncOperation.CCKPackageOperations;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class CCKAssetView : AutoBindingVisualElement<CCKAssetInfo>
    {
        [VisualBind]
        VisualElement previewImage; 
        
        [VisualBind]
        Label authorLabel;

        [VisualBind]
        VisualElement OperationBar;

        [VisualBind]
        VisualElement stateBar;

        [VisualBind]
        VisualElement installBar;

        [VisualBind]
        Button menuButton;

        protected Texture2D SkillsVRLogoIcon { get; } = Resources.Load<Texture2D>("Icon/SkillsVRLogo");

        public class MyDataWrapper : ScriptableObject<CCKAssetInfo> { }
        public override ScriptableObject<CCKAssetInfo> GetWrapper()
        {
            return ScriptableObject.CreateInstance<MyDataWrapper>();
        }
        public CCKAssetView()
        {
            CreateGUI();

            RegisterCallback<DetachFromPanelEvent>(OnDetached);
            RegisterCallback<AttachToPanelEvent>(OnAttached);
        }

        public void LitMode(bool enable)
        {
            authorLabel.SetDisplay(!enable);
            OperationBar.SetDisplay(!enable);
        }

        protected void OnAttached(AttachToPanelEvent evt)
        {
            LocalPackageManager.Current.OnPackageRegistrationChanged += OnLocalPackageRegistrationChanged;
        }
        protected void OnDetached(DetachFromPanelEvent evt)
        {
            LocalPackageManager.Current.OnPackageRegistrationChanged -= OnLocalPackageRegistrationChanged;
        }

        protected void CreateGUI()
        {
            this.ReloadVisualTreeAssetByType();

            this.style.width = 215;
            this.style.minWidth = 215;
            this.style.maxWidth = 215;

            this.style.marginLeft = 10;
            this.style.marginRight = 10;
            this.style.marginTop = 20;
            this.style.marginBottom = 20;


            this.EjectVisualElementsToProperty();
        }

        public void OnAssetChanged(ChangeEvent<CCKAssetInfo> evt)
        {
            Show(evt.newValue);
        }

        public void Show(CCKAssetInfo assetInfo)
        {
            BindingData = assetInfo;
            GetPreview();
            if (null == BindingData)
            {
                return;
            }
            LocalPackageManager.Current.WaitReady(UpdateInstallState).StartCoroutine();
        }

        void UpdateInstallState()
        {
            bool installed = false;
            if (null != BindingData && null != BindingData.GetPackageInfo())
            {
                installed = LocalPackageManager.Current.IsInstalled(BindingData.GetPackageInfo().name);
            }
            UpdateOnPackageRegistrationStateChanged(installed ? PackageRegistrationState.Added : PackageRegistrationState.Removed);
        }

        protected void ShowAssetMenu()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Uninstall"), false, RequestRemovePackage);
            menu.DropDown(menuButton.worldBound);
        }

        protected void RequestRemovePackage()
        {
            if (null == BindingData)
            {
                return;
            }
            var pkg = BindingData.GetPackageInfo();
            new UninstallCCKPackageAsyncOperation(pkg.name).StartCoroutine();
        }


        
        protected void LocateAsset()
        {
            if (null == BindingData)
            {
                return;
            }

            var pkgPath = Path.Combine("Packages", BindingData.GetPackageInfo().name);
            var url = string.IsNullOrWhiteSpace(BindingData.url) ? "package.json" : BindingData.url;
            var path = Path.Combine(pkgPath, url);
            path = path.Replace("\\", "/");

            if (PingPath(path))
            {
                return;
            }
            else
            {
                Debug.Log("No asset found at " + path);
            }
        }

        protected bool PingPath(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (null != asset)
            {
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
            return null != asset;
        }

        void InstallAsset()
        {
            new InstallCCKPackageAsync(BindingData.GetPackageInfo()).StartCoroutine();
        }


        private void OnLocalPackageRegistrationChanged(string packageName, PackageRegistrationState state)
        {
            if (null == BindingData
                || string.IsNullOrWhiteSpace(packageName)
                || packageName != BindingData.GetPackageInfo().name)
            {
                return;
            }
            UpdateOnPackageRegistrationStateChanged(state);
        }
        void UpdateOnPackageRegistrationStateChanged(PackageRegistrationState state)
        {
            switch (state)
            {
                case PackageRegistrationState.NoChange:
                    break;
                case PackageRegistrationState.ChangedTo:
                case PackageRegistrationState.Added:
                    installBar.pickingMode = PickingMode.Position;
                    installBar.SetDisplay(false);
                    stateBar.SetDisplay(true);
                    break;
                case PackageRegistrationState.ChangedFrom:
                case PackageRegistrationState.Removed:
                    installBar.pickingMode = PickingMode.Position;
                    installBar.SetDisplay(true);
                    stateBar.SetDisplay(false);
                    break;
                case PackageRegistrationState.OnAdding:
                    installBar.pickingMode = PickingMode.Ignore;
                    installBar.SetDisplay(true);
                    stateBar.SetDisplay(false);
                    break;
                default:
                    break;
            }
            
        }

        void GetPreview()
        {
            previewImage.SetBackgroundImage(null);
            if (null == BindingData
                || null == BindingData.previews)
            {
                previewImage.SetBackgroundImage(SkillsVRLogoIcon);
                return;
            }

            ProcPreview(0).StartCoroutine();
        }


        IEnumerator ProcPreview(int index)
        {
            CCKAssetPreviewManager cm = new CCKAssetPreviewManager();

            string cachePath = cm.GetPreivewCacheFilePath(BindingData, index);
            var cacheOp = cm.GetCachedPreviewAsync(cachePath);

            yield return cacheOp;
            if (null != cacheOp.Result)
            {
                previewImage.SetBackgroundImage(cacheOp.Result);
                yield break;
            }

            var downloadOp = cm.DownloadAssetPreviewAsync(BindingData, index);
            yield return downloadOp;

            //downloadOp.TryLogError();
            if (null != downloadOp.Result)
            {
                previewImage.SetBackgroundImage(downloadOp.Result);
                cm.SetCachedPreviewAsync(cachePath, downloadOp.Result).StartCoroutine();
                yield break;
            }

            previewImage.SetBackgroundImage(SkillsVRLogoIcon);
        }
    }
}