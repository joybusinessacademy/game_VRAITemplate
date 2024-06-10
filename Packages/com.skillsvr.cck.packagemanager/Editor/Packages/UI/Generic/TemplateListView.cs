using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class TemplateListView : ListView
    {
        public new class UxmlFactory : UxmlFactory<TemplateListView, UxmlTraits> { }

        public override VisualElement contentContainer => this.Q("unity-content-container");

        public new class UxmlTraits : ListView.UxmlTraits
        {
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get
                {
                    yield return new UxmlChildElementDescription(typeof(VisualElement));
                }
            }
        }

        bool init = false;
        public TemplateListView()
        {
            this.makeItem = CreateItemView;
            this.bindItem = BindItem;
            this.schedule.Execute(Init);
        }

        private void Init(TimerState obj)
        {
            init = true;
        }

        protected virtual void BindItem(VisualElement itemView, int index)
        {
            if (!init)
            {
                return;
            }
            if (null == itemsSource)
            {
                return;
            }
            object data = itemsSource[index];

            var methodName = "OnSetCustomData";
            var itemType = itemView.GetType();
            var method = itemType.GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(object) },
                null);

            if (null == method)
            {
                Debug.LogError($"Template list item {itemType.Name} not define method: void {methodName}(object data);");
                return;
            }
            method.Invoke(itemView, new object[] { data });
        }

        protected virtual VisualElement CreateItemView()
        {
            if (!init)
            {
                return null;
            }
            var template = contentContainer.Children().FirstOrDefault();
            if (null == template)
            {
                var label = new Label();
                label.text = "no item template";
                return label;
            }

            template.SetDisplay(false);
            var type = template.GetType();
            try
            {
                var item = (VisualElement)Activator.CreateInstance(type);
                return item;
            }
            catch
            {
                return new Label()
                {
                    text = $"Item type {type} should have a constractor without prarmeters.",
                };
            }
        }
    }
}