using SkillsVR.CCK.PackageManager.AsyncOperation;
using SkillsVR.CCK.PackageManager.AsyncOperation.Registry;
using SkillsVR.CCK.PackageManager.Managers;
using SkillsVR.CCK.PackageManager.Registry;
using SkillsVR.CCK.PackageManager.UI.Events;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    // UI Options:
    // - Search Field: Send SearchTextChangedEvent
    //
    // - Assets/Package View Toggle: Send ShowPackageViewEvent or ShowAssetsViewEvent on click
    //
    // - Menu Button: Popup Menu
    // 
    // - Reload from Cloud Button:
    //           Run RefreshCCKRegistryRemote (AsyncOperation);
    //           And send RegistryLoadedEvent on done.
    //
    // - Registry Dropdown:
    //           Send ChangedEvent and RegistrySelectedEvent on value change;
    //           Then ReloadRegistryFromLocalCacheAsync;
    //           And send RegistryLoadedEvent on done;
    public class CCKPackageManagerTopBar : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<CCKPackageManagerTopBar, UxmlTraits> { }

        [VisualBind]
        public Label TitleLabel { get; protected set; }

        [VisualBind]
        public TextField SearchField { get; protected set; }

        [VisualBind]
        public CCKRegistryDropdown RegistryDropdown { get; protected set; }

        [VisualBind]
        protected Button AssetsViewButton;
        [VisualBind]
        protected Button PackageViewButton;
        [VisualBind]
        protected Button MenuButton;

        public CCKRegistry CurrentRegistry => RegistryDropdown?.value ?? null;

        public CCKPackageManagerTopBar()
        {
            CreateGUI();
            this.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            UnregisterCallbacks();
        }

        protected void CreateGUI()
        {
            this.ReloadVisualTreeAssetByType();
            RegisterCallbacks();
        }

        protected void RegisterCallbacks()
        {
            RegistryDropdown.RegisterValueChangedCallback<CCKRegistry>(OnRegistryChanged);
            SearchField.RegisterValueChangedCallback<string>(OnSearchTextChanged);
            this.BindButtons();
        }

        private void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            this.SendCustomEvent(SearchTextChangedEvent.GetPooled(evt.newValue));
        }

        protected void UnregisterCallbacks()
        {
            RegistryDropdown.UnregisterValueChangedCallback<CCKRegistry>(OnRegistryChanged);
            this.UnBindButtons();
        }

        public void RequestShowAssetsView()
        {
            TitleLabel.text = "Assets Explorer";
            this.SendCustomEvent(ShowAssetsViewEvent.GetPooled());
            PackageViewButton.SetEnabled(true);
            AssetsViewButton.SetEnabled(false);
        }

        public void RequestShowPackageView()
        {
            TitleLabel.text = "Package Manager";
            this.SendCustomEvent(ShowPackageViewEvent.GetPooled());
            PackageViewButton.SetEnabled(false);
            AssetsViewButton.SetEnabled(true);
        }

        public void RequestReload()
        {
            this.SendCustomEvent(ReloadViewEvent.GetPooled());
        }

        public void OnMenuButtonClicked()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reload View"), false, RequestReload);
            menu.DropDown(MenuButton.worldBound);
        }

        public void RequestReloadRegistryFromCloud()
        {
            ReloadRegistryFromCloudAsync(RegistryDropdown.value).StartCoroutine();
        }

        public void SelectRegistryByIndex(int index)
        {
            RegistryDropdown.SelectByIndex(index);
        }

        private void OnRegistryChanged(ChangeEvent<CCKRegistry> evt)
        {
            var registry = evt.newValue;
            this.SendValueChangedEvent(evt.previousValue, evt.newValue);
            this.SendCustomEvent(RegistrySelectedEvent.GetPooled(registry));
            ReloadRegistryFromLocalCacheAsync(registry).StartCoroutine();
        }

        protected IEnumerator ReloadRegistryFromCloudAsync(CCKRegistry registry)
        {
            var op = new RefreshCCKRegistryRemote(registry);
            yield return op;
            op.TryLogError();
            this.SendCustomEvent(RegistryLoadedEvent.GetPooled(registry));
        }

        protected IEnumerator ReloadRegistryFromLocalCacheAsync(CCKRegistry registry)
        {
            var op = new RefreshCCKRegistryFromLocalCache(registry);
            yield return op;
            this.SendCustomEvent(RegistryLoadedEvent.GetPooled(registry));
        }
    }
}