using SkillsVR.CCK.PackageManager.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{

    public class CCKPackageListView : ListView
    {
        public new class UxmlFactory : UxmlFactory<CCKPackageListView, UxmlTraits> { }

        public new List<CCKPackageInfo> itemsSource
        {
            get
            {
                return base.itemsSource as List<CCKPackageInfo>;
            }
            set
            {
                if (null == value)
                {
                    base.itemsSource = null;
                }
                base.itemsSource = value;
            }
        }

        public CCKPackageListView()
        {
            this.makeItem = OnMakeItem;
            this.bindItem = OnBindItem;
            this.onSelectionChange += CCKPackageListView_onSelectionChange; ;
        }

        private void CCKPackageListView_onSelectionChange(IEnumerable<object> obj)
        {
            this.SendValueChangedEvent<CCKPackageInfo>(null, obj.FirstOrDefault() as CCKPackageInfo);
        }

        private void OnBindItem(VisualElement itemView, int index)
        {
            if (null == itemsSource)
            {
                return;
            }
            var data = itemsSource[index];
            CCKPackageListItemView item = itemView as CCKPackageListItemView;
            item.SetPackage(data);
        }

        private VisualElement OnMakeItem()
        {
            return new CCKPackageListItemView();
        }
    }
}