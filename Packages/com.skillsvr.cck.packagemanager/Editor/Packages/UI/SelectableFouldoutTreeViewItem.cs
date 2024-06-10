using SkillsVR.CCK.PackageManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class SelectableFouldoutTreeViewItem : VisualElement
    {
        protected Button selectStateButton;
        protected Button titleButton;
        protected Button countButton;
        protected Foldout foldout;

        protected VisualElement topBar;
        protected VisualElement childrenArea;


        public bool Selected { get; protected set; }

        public event Action<SelectableFouldoutTreeViewItem> OnItemChanged;
        public TreeViewItemData Data { get; protected set; }
        public SelectableFouldoutTreeViewItem(TreeViewItemData data)
        {
            Data = data;

            topBar = new VisualElement();
            topBar.name = "top-bar";
            topBar.style.flexDirection = FlexDirection.Row;
            topBar.style.flexShrink = 1;
            topBar.style.alignItems = Align.Center;
            this.Add(topBar);

            selectStateButton = new Button();
            selectStateButton.style.width = 18;
            selectStateButton.style.height = 18;
            selectStateButton.text = null;
            selectStateButton.style.backgroundColor = Color.clear;
            selectStateButton.clicked += ToggleSelection;
            selectStateButton.style.color = Color.black;
            selectStateButton.style.borderBottomColor = Color.white;
            selectStateButton.style.borderLeftColor = Color.white;
            selectStateButton.style.borderRightColor = Color.white;
            selectStateButton.style.borderTopColor = Color.white;
            
            selectStateButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            topBar.Add(selectStateButton); 

            titleButton = new Button();
            titleButton.text = Data.name;
            titleButton.style.minWidth = 60;
            titleButton.style.backgroundColor = Color.clear;
            titleButton.style.borderBottomColor = Color.clear;
            titleButton.style.borderLeftColor = Color.clear;
            titleButton.style.borderRightColor = Color.clear;
            titleButton.style.borderTopColor = Color.clear;
            titleButton.clicked += ToggleSelection;
            titleButton.style.unityTextAlign = TextAnchor.MiddleLeft;
            topBar.Add(titleButton);

            countButton = new Button();
            countButton.style.display = DisplayStyle.None;
            if (Data.count > 0)
            {
                countButton.text = $"({Data.count})";
                countButton.style.backgroundColor = Color.clear;
                countButton.style.borderBottomColor = Color.clear;
                countButton.style.borderLeftColor = Color.clear;
                countButton.style.borderRightColor = Color.clear;
                countButton.style.borderTopColor = Color.clear;
                countButton.style.unityTextAlign = TextAnchor.MiddleLeft;
                countButton.style.display = DisplayStyle.Flex;
            }
            countButton.clicked += ToggleSelection;
            topBar.Add(countButton);

            foldout = new Foldout();
            foldout.text = null;
            foldout.style.display = DisplayStyle.None;
            topBar.Add(foldout);

            foldout.RegisterValueChangedCallback<bool>(OnFoldoutValueChanged);
            foldout.value = true;

            childrenArea = new VisualElement();
            childrenArea.name = "children-area";
            childrenArea.style.paddingLeft = 20;


            tooltip = Data.fullName;

            this.Add(childrenArea);

            UpdateSelectStateIcon();
        }

        public void AddChildrenCategory(SelectableFouldoutTreeViewItem item)
        {
            childrenArea.Add(item);
            foldout.style.display = DisplayStyle.Flex;
        }

        void OnFoldoutValueChanged(ChangeEvent<bool> evt)
        {
            childrenArea.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void ToggleSelection()
        {
            SetSelection(!Selected);
        }

        public void SetSelection(bool selected)
        {
            if (selected == Selected)
            {
                UpdateSelectStateIcon();
                return;
            }
            Selected = selected;
            
            OnItemChanged?.Invoke(this);
            if (Selected)
            {
                SelectParent();
            }
            else
            {
                UnselectAllChildren();
                TryUnselectParent();
            }
            UpdateSelectStateIcon();
            TriggerRootEvent();
        }

        public void UpdateSelectStateIcon()
        {
            if (Selected)
            {
                bool anyChildSelected = AnyChildSelected();
                selectStateButton.text = anyChildSelected ? "-" : "✓";
                selectStateButton.style.backgroundColor = new Color(0.13f, 0.59f, 0.95f);
            }
            else
            {
                selectStateButton.text = "";
                selectStateButton.style.backgroundColor = Color.clear;
            }
        }
        public void UnselectAllChildren()
        {
            var children = childrenArea.Query<SelectableFouldoutTreeViewItem>().ToList();
            foreach(var child in children)
            {
                child.SetSelection(false);
            }
        }

        SelectableFouldoutTreeViewItem GetParent()
        {
            var element = this.parent;
            while (null != element)
            {
                if (element is SelectableFouldoutTreeViewItem item)
                {
                    return item;
                }
                element = element.parent;
            }
            return null;
        }

        void TriggerRootEvent()
        {
            var element = GetParent();
            while(null != element)
            {
                var p = element.GetParent();
                if (null == p)
                {
                    element.OnItemChanged?.Invoke(element);
                    return;
                }
                element = p;
            }
        }

        void SelectParent()
        {
            GetParent()?.SetSelection(true);
        }

        void TryUnselectParent()
        {
            var parent = GetParent();
            if (null == parent)
            {
                return;
            }
            if (parent.AnyChildSelected())
            {
                return;
            }
            parent.SetSelection(false);
        }

        bool AnyChildSelected()
        {
            return childrenArea.Query<SelectableFouldoutTreeViewItem>().ToList().Any(x=> x.Selected);
        }


        public IEnumerable<string> GetActiveItems()
        {
            var children = childrenArea.Children().Where(x => x is SelectableFouldoutTreeViewItem).Select(x => x as SelectableFouldoutTreeViewItem);

            List<string> items = new List<string>();
            if (null == children.FirstOrDefault(x=> null != x))
            {
                if (Selected)
                {
                    items.Add(this.Data.fullName);
                    return items;
                }
            }
            else
            {
                var childrenItems = children.SelectMany(x => x.GetActiveItems());
                if (null != childrenItems.FirstOrDefault(x=> null != x))
                {
                    return childrenItems;
                }
                else if (Selected)
                {
                    items.Add(this.Data.fullName);
                }
            }
            return items;
        }
    }
}