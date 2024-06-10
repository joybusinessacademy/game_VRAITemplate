using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scripts.Utility;
using SkillsVRNodes.Managers.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SkillsVRNodes.Scripts.CustomWindows
{
    public class AssetItemVisualElement : VisualElement, IDisposable
    {
        private ObjectReference objectObjectReference;
        private Label label;
        private TextField renameField;
        public static List<AssetItemVisualElement> AssetItemVisualElements = new();

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Selection.selectionChanged += SelectionChanged;
        }

        public AssetItemVisualElement(ObjectReference objectObjectReference)
        {
            AssetItemVisualElements.Add(this);
            
            this.objectObjectReference = objectObjectReference;
            
            focusable = true;
            label = new Label(objectObjectReference.Name);
            Add(label);

            // Double click
            RegisterCallback<ClickEvent>(Clicked);

            name = "asset-item";
            
            SelectionChanged();

            RegisterCallback<DetachFromPanelEvent>(_ => Dispose());
            RegisterCallback<AttachToPanelEvent>(_ => SetupCallbacks());
            RegisterCallback<KeyDownEvent>(KeyPress);
            
            renameField = new TextField {style = {display = DisplayStyle.None, flexGrow = 1}};
            Add(renameField);
            renameField.RegisterCallback<KeyDownEvent>(t =>
            {
                switch (t.keyCode)
                {
                    case KeyCode.Escape:
                        StopRename();
                        break;
                    case KeyCode.Return:
                        renameField.Blur();
                        break;
                }
            });
        }

        private void KeyPress(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.F2:
                    Rename();
                    break;
                case KeyCode.Delete:
                    Delete();
                    break;
                case KeyCode.Return:
                    OpenAsset();
                    break;
            }
        }

        private static void SelectionChanged()
        {
            List<string> paths = Selection.objects.Select(AssetDatabase.GetAssetPath).ToList();
            
            foreach (AssetItemVisualElement visualElement in AssetItemVisualElements)
            {
                visualElement.SetSelectedState(paths.Contains(visualElement.ObjectReference.Path));
            }
        }

        public void SetSelectedState(bool selected)
        {
            if (selected)
            {
                AddToClassList("selected");
            }
            else
            {
                RemoveFromClassList("selected");
            }
        }
        
        public bool IsSelected => GetClasses().Contains("selected");

        public ObjectReference ObjectReference => objectObjectReference;

        void RightClick(ContextualMenuPopulateEvent evt)
        {
            if (!IsSelected)
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(ObjectReference.Path);
            }
            evt.menu.AppendAction("Open", _ => OpenAsset());
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Show in explorer", _ => { EditorUtility.RevealInFinder(ObjectReference.Path); });
            evt.menu.AppendAction("Rename", _ => { Rename(); });
            evt.menu.AppendAction("Copy Path", _ => EditorGUIUtility.systemCopyBuffer = ObjectReference.Path);
            evt.menu.AppendSeparator();
            foreach (GraphProjectData projectData in GraphFinder.GetAllGraphData())
            {
                if (projectData.GetProjectName == ObjectReference.attachedProject)
                {
                    evt.menu.AppendAction("Move To Project/" + projectData.GetProjectName + " (Current)", action => { }, DropdownMenuAction.Status.Disabled);
                    continue;
                }

                evt.menu.AppendAction("Move To Project/" + projectData.GetProjectName, action => { MoveToProject(projectData); });
            }
            foreach (GraphProjectData projectData in GraphFinder.GetAllGraphData())
            {
                if (projectData.GetProjectName == ObjectReference.attachedProject)
                {
                    evt.menu.AppendAction("Copy To Project/" + projectData.GetProjectName + " (Current)", action => { }, DropdownMenuAction.Status.Disabled);
                    continue;
                }

                evt.menu.AppendAction("Copy To Project/" + projectData.GetProjectName, action => { CopyToProject(projectData); });
            }

            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Delete", _ => { Delete(); });
        }

        private static void Delete()
        {
            var message = "Are you sure you want to delete the selected assets?\n\n";

            var amount = 5;
            int lines = 0;
            foreach (var asset in Selection.objects)
            {
                lines++;
                if (lines > amount)
                {
                    message += $"And {Selection.objects.Length - amount} more...";
                    break;
                }
                
                message += asset.name + "\n";
            }

            if (!EditorUtility.DisplayDialog("Delete", message, "Yes", "No"))
            {
                return;
            }
            
            foreach (var asset in Selection.objects)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
            }

            AssetHandlerVisualElement.ResetAllAssets();
        }

        private void OpenAsset()
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<Object>(ObjectReference.Path));
        }

        public static AssetItemVisualElement CurrentlySelectedAsset;
        
        private void Clicked(ClickEvent evt)
        {
            if (evt.shiftKey)
            {
                CurrentlySelectedAsset ??= parent.GetFirstAncestorOfType<AssetItemVisualElement>();

                if (CurrentlySelectedAsset == null)
                {
                    CurrentlySelectedAsset = this;
                }
                else
                {
                    List<AssetItemVisualElement> assets = parent.Children().OfType<AssetItemVisualElement>().ToList();
                    int startIndex = assets.IndexOf(CurrentlySelectedAsset);
                    int endIndex = assets.IndexOf(this);
                    if (startIndex > endIndex)
                    {
                        (startIndex, endIndex) = (endIndex, startIndex);
                    }
                    
                    List<Object> selected = Selection.objects.ToList();
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        selected.Add(AssetDatabase.LoadAssetAtPath<Object>(assets[i].ObjectReference.Path));
                    }

                    Selection.objects = selected.ToArray();
                    return;
                }
            }
            CurrentlySelectedAsset = this;
            if (evt.ctrlKey)
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(ObjectReference.Path);
                List<Object> selected = Selection.objects.ToList();
                if (selected.Contains(asset))
                {
                    selected.Remove(asset);
                }
                else
                {
                    selected.Add(asset);
                }
                Selection.objects = selected.ToArray();
                return;
            }
            if (evt.clickCount == 2)
            {
                OpenAsset();
            }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(ObjectReference.Path);
        }
        
        private void CopyToProject(GraphProjectData projectData)
        {
            List<Tuple<string, string>> pathPair = new();
            foreach (Object asset in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                string newPath = GetNewPath(projectData, asset, path);
                
                if (newPath.IsNullOrWhitespace())
                {
                    continue;
                }

                pathPair.Add(new Tuple<string, string>(path, newPath));
            }
            AssetDatabase.Refresh();
            AssetDatabase.StartAssetEditing();
            
            foreach (Tuple<string, string> t in pathPair)
            {
                AssetDatabase.CopyAsset(t.Item1, t.Item2);
            }
            
            AssetDatabase.StopAssetEditing();
            AssetHandlerVisualElement.ResetAllAssets();
        }
        
        private static void MoveToProject(GraphProjectData projectData)
        {
            List<Tuple<string, string>> pathPair = new();
            foreach (Object asset in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(asset);
                string newPath = GetNewPath(projectData, asset, path);
                
                if (newPath.IsNullOrWhitespace())
                {
                    continue;
                }

                pathPair.Add(new Tuple<string, string>(path, newPath));
            }
            AssetDatabase.Refresh();
            AssetDatabase.StartAssetEditing();
            
            foreach (Tuple<string, string> t in pathPair)
            {
                AssetDatabase.MoveAsset(t.Item1, t.Item2);
            }
            
            AssetDatabase.StopAssetEditing();
            AssetHandlerVisualElement.ResetAllAssets();
        }

        private static string GetNewPath(GraphProjectData projectData, Object asset, string path)
        {
            string[] pathSplit = path.Split('/');
				
            pathSplit[2] = projectData.GetProjectName;
            string newPath = string.Join("/", pathSplit);

            string folder = newPath[..(newPath.LastIndexOf('/') + 1)];
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            
            if (!CanMoveToProject(path))
            {
                Debug.LogWarning("Cannot move asset from: " + path + " to: " + newPath, asset);
                return "";
            }

            return newPath;
        }

        private static bool CanMoveToProject(string path)
        {
            string[] pathList = path.Split('/');

            // If the asset is in a project
            if (pathList.First() == "Packages")
            {
                return false;
            }
			
            // If the Asset is not in a project
            if (pathList.Length <= 3 || pathList[1] != "Contexts")
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            if (CurrentlySelectedAsset == this)
            {
                CurrentlySelectedAsset = null;
            }
            AssetItemVisualElements.Remove(this);
            this.RemoveManipulator(manipulator);
        }

        private ContextualMenuManipulator manipulator;
        public void SetupCallbacks()
        {
            manipulator = new ContextualMenuManipulator(RightClick);
            this.AddManipulator(manipulator);
            AssetItemVisualElements.AddIfNotFound(this);
        }
        
        private void Rename()
        {
            renameField.style.display = DisplayStyle.Flex;
            label.style.display = DisplayStyle.None;

            renameField.value = label.text;

            renameField.RegisterCallback<FocusOutEvent>(RenameCallback);
            renameField.Focus();
        }

        private void RenameCallback(FocusOutEvent t)
        {
            if (renameField.style.display == DisplayStyle.None)
            {
                return;
            }
            
            string newName = renameField.value;
            if (string.IsNullOrEmpty(newName))
            {
                return;
            }
            renameField.UnregisterCallback<FocusOutEvent>(RenameCallback);

            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(Selection.activeObject), newName);
            SelectionChanged();
            StopRename();
        }

        private void StopRename()
        {
            renameField.UnregisterCallback<FocusOutEvent>(RenameCallback);
            
            renameField.style.display = DisplayStyle.None;
            label.style.display = DisplayStyle.Flex;
        }
    }
}