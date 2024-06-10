using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SkillsVRNodes.Scripts.CustomWindows
{
    public class AssetPreviewVisualElement : VisualElement, IDisposable
    {
        public new class UxmlFactory : UxmlFactory<AssetPreviewVisualElement, UxmlTraits> { }

        
        public AssetPreviewVisualElement()
        {
            Resources.Load<VisualTreeAsset>("UXML/AssetPreview")?.CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/SkillsVR"));
            
            RegisterCallback<DetachFromPanelEvent>(_ => Dispose());
            RegisterCallback<AttachToPanelEvent>(_ => SetupCallbacks());
            SelectionChanged();
            
            this.Q<ScrollView>().contentContainer.style.flexDirection = FlexDirection.Row;
            this.Q<ScrollView>().contentContainer.style.flexWrap = Wrap.Wrap;
        }

        private void SelectionChanged()
        {
            this.Q<ScrollView>().Clear();
            this.Q<VisualElement>("asset-tags").Clear();
            this.Q<VisualElement>("asset-path").Clear();

            if (Selection.objects.Length > 1)
            {
                SetMultipleAssets(Selection.objects);
            }
            else if (Selection.activeObject != null)
            {
                SetAsset(Selection.activeObject);
            }
            else
            {
                this.Q<Label>("asset-name").text = "";
                this.Q<Label>("asset-type").text = "No Asset Selected";
            }
        }

        private void SetAsset(Object asset)
        {
            this.Q<Label>("asset-name").text = asset.name;
            this.Q<Label>("asset-type").text = asset.GetType().Name;

            VisualElement tagContainer = this.Q<VisualElement>("asset-tags");
            tagContainer.Add(new Label("Tags"));

            foreach (string label in AssetDatabase.GetLabels(asset))
            {
                Label tag = new(label) { name = "tag" };
                tag.AddManipulator(new ContextualMenuManipulator(t =>
                {
                    t.menu.AppendAction("Remove Tag", a =>
                    {
                        List<string> labels = AssetDatabase.GetLabels(asset).ToList();
                        labels.Remove(label);
                        AssetDatabase.SetLabels(asset, labels.ToArray());
                        SelectionChanged();
                    });
                }));
                this.Q<VisualElement>("asset-tags").Add(tag);
            }
            tagContainer.Add(new Button(() => AddTag(asset)) {text = "Add Tag", name = "tag"});
            
            this.Q<ScrollView>().Add(new AssetPreviewItem(asset));
            
            SetPathText(AssetDatabase.GetAssetPath(asset));
        }

        private void SetPathText(string path)
        {
            Label pathLabel = new(path) {name = "subtitle"};
            pathLabel.AddManipulator(new ContextualMenuManipulator(t =>
            {
                string fullPath = Application.dataPath;
                fullPath = fullPath.Remove(fullPath.Length - 6);
                fullPath += path;
                
                t.menu.AppendAction("Copy Full Path", a => EditorGUIUtility.systemCopyBuffer = fullPath);
            }));
            this.Q<VisualElement>("asset-path").Add(pathLabel);
        }

        private void AddTag(Object asset)
        {
            string tagName = GetTagName();
            if (string.IsNullOrEmpty(tagName))
            {
                return;
            }
            
            List<string> labels = AssetDatabase.GetLabels(asset).ToList();
            labels.AddIfNotFound(tagName);
            AssetDatabase.SetLabels(asset, labels.ToArray());
            SelectionChanged();
        }

        private void SetMultipleAssets(Object[] assets)
        {
            this.Q<Label>("asset-name").text = assets.Length + " Assets Selected";
            this.Q<Label>("asset-type").text = "Multiple Types";
            
            foreach (Object asset in assets)
            {
                this.Q<ScrollView>().Add(new AssetPreviewItem(asset));
            }
        }

        public void Dispose()
        {
            Selection.selectionChanged -= SelectionChanged;
        }

        private void SetupCallbacks()
        {
            Selection.selectionChanged += SelectionChanged;
        }

        private static string GetTagName()
        {
            return AskUserForString.Show("New Tag", "Please Insert the Tag" , "tag Name");            
        }
    }
}