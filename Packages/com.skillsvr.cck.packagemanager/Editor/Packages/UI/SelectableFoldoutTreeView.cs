using SkillsVR.CCK.PackageManager.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class SelectableFoldoutTreeView: SelectableFouldoutTreeViewItem
    {
        public SelectableFoldoutTreeView(string title) : base(GetRootData(title))
        {
            selectStateButton.style.display = DisplayStyle.None;
            titleButton.style.fontSize = 26;
            this.style.flexShrink = 1;
            this.style.flexGrow = 1;
        }

        static TreeViewItemData GetRootData(string title)
        {
            return new TreeViewItemData() { name = title??"Filters", count = 0, fullName = "" };
        }
        
        public void GenerateTreeView(IEnumerable<TreeViewItemData> data)
        {
            List<SelectableFouldoutTreeViewItem> views = new List<SelectableFouldoutTreeViewItem>();
            foreach(var item in data)
            {
               views.Add(new SelectableFouldoutTreeViewItem(item));
            }
            foreach (var view in views)
            {
                var path = view.Data.fullName;
                var root = Path.GetDirectoryName(path).Replace("\\", "/");
                if (string.IsNullOrWhiteSpace(root))
                {
                    this.AddChildrenCategory(view);
                }
                else
                {
                    var parent = views.FirstOrDefault(x => x.Data.fullName == root);
                    if (null != parent)
                    {
                        parent.AddChildrenCategory(view);
                    }
                }
            }
        }

    }
}