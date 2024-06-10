using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Registry;
using SkillsVR.CCK.PackageManager.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class CCKPackageExplorerView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<CCKPackageExplorerView, UxmlTraits> { }

        [VisualBind]
        protected CCKPackageListView PackageListView;

        [VisualBind]
        public CCKPackageView PackageView { get; protected set; }


        string lastSelectId;
        string lastSearchText;
        CCKRegistry lastRegistry;

        public CCKPackageExplorerView()
        {
            CreateGUI();
        }

        protected void CreateGUI()
        {
            try
            {
                this.ReloadVisualTreeAssetByType();
                RegistryCallbacks();
            }
            catch (Exception e)
            {
                this.Add(new Label() { text = e.Message });
            }
        }

        protected void RegistryCallbacks()
        {
            PackageListView.RegisterCallback<ChangeEvent<CCKPackageInfo>>(PackageView.OnPackageChanged);
            PackageListView.RegisterCallback<ChangeEvent<CCKPackageInfo>>(OnSelectionChanged);

            this.RegisterCallback<RegistryLoadedEvent>(OnRegistryLoaded);
            this.RegisterCallback<SearchTextChangedEvent>(OnSearchTextChanged);
        }

        private void OnSearchTextChanged(SearchTextChangedEvent evt)
        {
            lastSearchText = evt?.SearchText??string.Empty;
            lastSearchText = lastSearchText.ToLower();
            OnRegistryLoad(lastRegistry);
        }

        private void OnSelectionChanged(ChangeEvent<CCKPackageInfo> evt)
        {
            lastSelectId = null != evt.newValue ? evt.newValue.name : null;
        }

        protected void OnRegistryChanged(ChangeEvent<CCKRegistry> evt)
        {
            OnRegistryLoad(evt.newValue);
        }

        protected void OnRegistryLoaded(RegistryLoadedEvent evt)
        {
            OnRegistryLoad(evt.CCKRegistry);
        }

        protected void OnRegistryLoad(CCKRegistry registry)
        {
            lastRegistry = registry;
            var pkgs = FilterPackages(registry);

            PackageListView.itemsSource = pkgs.ToList();
            if (string.IsNullOrWhiteSpace(lastSelectId))
            {
                PackageListView.selectedIndex = 0;
            }
            var lsat = PackageListView.itemsSource.FirstOrDefault(x => x.name == lastSelectId);
            if (null != lsat)
            {
                var index = PackageListView.itemsSource.IndexOf(lsat);
                PackageListView.selectedIndex = index;
            }
            else
            {
                PackageListView.selectedIndex = 0;
            }
        }

        protected IEnumerable<CCKPackageInfo> FilterPackages(CCKRegistry registry)
        {
            var pkgs = registry?.packages ?? new CCKPackageInfo[0];
            var results = pkgs.Where(x => null != x && !x.HasFlag(Flags.Hide));
            if (!string.IsNullOrWhiteSpace(lastSearchText))
            {
                results = results.Where(pkg =>
                null != pkg
                && !string.IsNullOrWhiteSpace(pkg.displayName)
                && pkg.displayName.ToLower().Contains(lastSearchText)
                );
            }
            return results.GroupBy(x => !x.HasFlag(Flags.Required))
            .OrderByDescending(g => g.Key)
            .SelectMany(g => g.OrderBy(x => x.GetValueInt(Flags.Order)).ThenBy(x => x.name));
        }
    }
}