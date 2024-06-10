using SkillsVR.CCK.PackageManager.AsyncOperation;
using SkillsVR.CCK.PackageManager.AsyncOperations;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Registry;
using SkillsVR.CCK.PackageManager.UI.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    [Serializable]
    public class AssetExplorerViewData
    {
        public int size;
        public int page;
        public int maxPage;
        public string queryText;
        public List<CCKAssetInfo> assets = new List<CCKAssetInfo>();
        public float scale;
    }

    public class CCKAssetExplorerView : AutoBindingVisualElement<AssetExplorerViewData>
    {
        public new class UxmlFactory : UxmlFactory<CCKAssetExplorerView, UxmlTraits> { }

        CCKRegistry currRegistry;

        [VisualBind]
        VisualElement AssetsGrid;

        [VisualBind]
        VisualElement Right;

        [VisualBind]
        VisualElement PageGroup;

        [VisualBind]
        DropdownField ViewSizeDropdown;

        [VisualBind]
        Label resultText;

        PageNavigator pageNavigator = new PageNavigator();

        Queue<CCKAssetView> viewCache = new Queue<CCKAssetView>();

        SelectableFoldoutTreeView CategoryTreeRoot;

        IVisualElementScheduledItem queryScheduledItem;

        public CCKAssetExplorerView()
        {
            BindingData = new AssetExplorerViewData();
            CreateGUI();
        }

        void SetViewSizeFromDropdown(DropdownField dropdown)
        {
            string txt = dropdown.value;
            int size = 0;
            if (!int.TryParse(txt, out size))
            {
                size = 24;
            }
            ChangeViewResultsSize(size);
        }

        void ChangeViewResultsSize(int size)
        {
            BindingData.page = 0;
            BindingData.size = size;
            DelayQueryCurrent();
        }

        void ShowHome()
        {
            BindingData.page = 0;
            BindingData.queryText = null;
            CategoryTreeRoot.UnselectAllChildren();
            SetViewSizeFromDropdown(ViewSizeDropdown);
            DelayQueryCurrent();
        }

        IEnumerator QueryAsync(CCKAssetQueryArgs args)
        {
            int total = 0;
            if (null == currRegistry || null == currRegistry.packages)
            {
                BindingData.assets = new List<CCKAssetInfo>(0);
            }   
            else
            {
                var assets = currRegistry.packages.Where(pkg => null != pkg && null != pkg.assets)
                    .SelectMany(pkg => pkg.assets)
                    .Where(asset => null != asset);
                var queryOp = new QueryCCKAssetsFromCollection(args, assets);
                yield return queryOp;
                queryOp.TryLogError();
                BindingData.assets = queryOp.Result.Assets.ToList();
                total = queryOp.Result.TotalResultsCount;
            }
           

            int from = args.from;
            int size = Mathf.Max(1,args.size);
            int pageCount = Mathf.CeilToInt((float)total / size);
            int pageIndex = Mathf.FloorToInt((float)from / size);
            int currSize = Mathf.Min(BindingData.assets.Count(), size);

            var info = $"{from} - {from + currSize} ({currSize}) of {total} Results";
            resultText.text = info;

            pageNavigator.PageCount = pageCount;
            pageNavigator.SetValueWithoutNotify(pageIndex);

            RebuildAssetViews();
        }


        protected void DelayQueryCurrent()
        {
            if (null != queryScheduledItem)
            {
                queryScheduledItem.ExecuteLater(300);
            }
            else
            {
                queryScheduledItem = schedule.Execute(QueryCurrent);
                queryScheduledItem.ExecuteLater(300);
            }
        }

        protected void QueryCurrent()
        {
            queryScheduledItem = null;
            CCKAssetQueryArgs args = new CCKAssetQueryArgs();

            string text = string.IsNullOrWhiteSpace(BindingData.queryText) ? string.Empty : BindingData.queryText;
            args.KeyWords.TryAddQueryValues(text.Split(' '));
            args.Categories.TryAddQueryValues(CategoryTreeRoot?.GetActiveItems());
            args.SetFromPage(BindingData.page, BindingData.size);
            QueryAsync(args).StartCoroutine();
        }

        public void CreateGUI()
        {
            // Import UXML
            this.ReloadVisualTreeAssetByType();

            AssetsGrid.Clear();
            viewCache.Clear();
            RebuildAssetViews();
            PageGroup.Clear();
            PageGroup.Add(pageNavigator);

            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            pageNavigator.RegisterValueChangedCallback<int>((evt) => {
                int newPageIndex = evt.newValue;
                BindingData.page = newPageIndex;
                DelayQueryCurrent();
            });

            ViewSizeDropdown.RegisterValueChangedCallback<string>((evt) => {
                SetViewSizeFromDropdown(ViewSizeDropdown);
            });

            BindingData = BindingData;

            this.RegisterCallback<RegistryLoadedEvent>(OnRegistryLoaded);
            this.RegisterCallback<SearchTextChangedEvent>(OnSearchTextChanged);
        }

        private void OnSearchTextChanged(SearchTextChangedEvent evt)
        {
            BindingData.queryText = evt.SearchText;
            DelayQueryCurrent();
        }

        private void OnRegistryLoaded(RegistryLoadedEvent evt)
        {
            currRegistry = evt.CCKRegistry;
            RebuildCategory();
            ShowHome();
        }

        protected void RebuildCategory()
        {
            Right.Clear();
            Right.style.alignItems = Align.Center;
            CategoryTreeRoot = new SelectableFoldoutTreeView("All Categories");
            
            Right.Add(CategoryTreeRoot);

            var allCategoryData = currRegistry.FetchAllCategoryData();

            CategoryTreeRoot.GenerateTreeView(allCategoryData);

            CategoryTreeRoot.OnItemChanged += (v) => { DelayQueryCurrent(); };
        }

        protected void RebuildAssetViews()
        {
            var existings = AssetsGrid.Query<CCKAssetView>().ToList();
            foreach(var view in existings)
            {
                view.style.display = DisplayStyle.None;
                viewCache.Enqueue(view);
            }

            if (null == BindingData || null == BindingData.assets)
            {
                return;
            }
            foreach(var asset in BindingData.assets)
            {
                if (null == asset)
                {
                    continue;
                }

                CCKAssetView view = null;

                if (viewCache.Count() > 0)
                {
                    view = viewCache.Dequeue();
                }
                else
                {
                    view = new CCKAssetView();
                    AssetsGrid.Add(view);
                    view.style.width = 300;
                    view.style.height = 200;
                }
                view.style.display = DisplayStyle.Flex;
                view.Show(asset);
            }
        }

        protected class DataWrapper : ScriptableObject<AssetExplorerViewData> { }
        public override ScriptableObject<AssetExplorerViewData> GetWrapper()
        {
            return new DataWrapper();
        }
    }
}