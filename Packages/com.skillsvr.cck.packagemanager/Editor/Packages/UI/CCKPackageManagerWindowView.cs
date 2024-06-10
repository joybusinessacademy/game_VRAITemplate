using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using SkillsVR.CCK.PackageManager.Registry;
using SkillsVR.CCK.PackageManager.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    // Toggle package/asset explorer views on event;
    // Pass top bar events to explorer views.
    // Init with first registry and assets view.
    public class CCKPackageManagerWindowView : VisualElement
    {
        [VisualBind]
        public CCKPackageManagerTopBar TopBar { get; private set; }

        [VisualBind]
        public CCKPackageExplorerView PackageExplorerView { get; private set; }

        [VisualBind]
        public CCKAssetExplorerView AssetExplorerView { get; private set; }


        public CCKPackageManagerWindowView()
        {
            CreateGUI();
            this.schedule.Execute(Init);

            this.RegisterCallback<AttachToPanelEvent>(OnAttached);
            this.RegisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        private void OnAttached(AttachToPanelEvent evt)
        {
            RegisterCallbacks();
        }
        private void OnDetached(DetachFromPanelEvent evt)
        {
            UnregisterCallbacks();
        }

        private void Init(TimerState obj)
        {
            TopBar.SelectRegistryByIndex(0);
            TopBar.RequestShowAssetsView();
        }

        protected void RegisterCallbacks()
        {
            TopBar.RegisterCallback<ShowPackageViewEvent>(ShowPackageView);
            TopBar.RegisterCallback<ShowAssetsViewEvent>(ShowAssetsView);
            TopBar.RegisterCallback<RegistrySelectedEvent>(BoardcastEvent);
            TopBar.RegisterCallback<RegistryLoadedEvent>(BoardcastEvent);
            TopBar.RegisterCallback<SearchTextChangedEvent>(BoardcastEvent);
            LocalPackageManager.Current.OnPackageRegistrationChanged += OnPackageRegistrationChanged;
        }

        protected void UnregisterCallbacks()
        {
            TopBar.UnregisterCallback<ShowPackageViewEvent>(ShowPackageView);
            TopBar.UnregisterCallback<ShowAssetsViewEvent>(ShowAssetsView);
            TopBar.UnregisterCallback<RegistrySelectedEvent>(BoardcastEvent);
            TopBar.UnregisterCallback<RegistryLoadedEvent>(BoardcastEvent);
            TopBar.UnregisterCallback<SearchTextChangedEvent>(BoardcastEvent);
            LocalPackageManager.Current.OnPackageRegistrationChanged -= OnPackageRegistrationChanged;
        }

        private void OnPackageRegistrationChanged(string packageName, PackageRegistrationState state)
        {
            BoardcastEvent(PackageInstallStateChangedEvent.GetPooled(packageName, state));
        }

        protected void BoardcastEvent(ICloneableUIEvent evt)
        {
            PackageExplorerView.SendCustomEvent(evt.Clone());
            AssetExplorerView.SendCustomEvent(evt.Clone());
        }

        private void ShowAssetsView(ShowAssetsViewEvent evt)
        {
            PackageExplorerView.SetDisplay(false);
            AssetExplorerView.SetDisplay(true);
        }

        private void ShowPackageView(ShowPackageViewEvent evt)
        {
            PackageExplorerView.SetDisplay(true);
            AssetExplorerView.SetDisplay(false);
        }

        protected void CreateGUI()
        {
            this.style.flexGrow = 1.0f;
            try
            {
                this.ReloadVisualTreeAssetByType();
                
            }
            catch(Exception e)
            {
                this.Add(new Label() { text = e.Message });
            }
        }

        protected IEnumerable<CCKPackageInfo> FilterPackages(CCKRegistry registry)
        {
            var pkgs =  registry?.packages ?? new CCKPackageInfo[0];
            return pkgs.Where(x => null != x && !x.HasFlag(Flags.Hide))
                .GroupBy(x => !x.HasFlag(Flags.Required))
                .OrderByDescending(g => g.Key)
                .SelectMany(g => g.OrderBy(x => x.GetValueInt(Flags.Order)).ThenBy(x => x.name));
        }
    }
}