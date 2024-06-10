using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Props;
using SceneNavigation;
using Scripts.CustomWindows.Hierarchy;
using SkillsVR.UnityExtenstion;
using SkillsVRNodes.Scripts.Hierarchy;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SkillsVRNodes.Props
{
	public class PropsHierarchyVisualElement : VisualElement
	{
		private VisualElement scrollView;
		private PropHierarchyItem mainSelectedItem;
		
		public IEnumerable<GameObject> SelectedProps => Selection.gameObjects.Where(t => t.GetComponent<PropComponent>() != null);
		public PropsHierarchyVisualElement()
		{
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/Hierarchy"));

			focusable = true;
			PropManager.OnPropListChanged(Refresh);
			Refresh();
			EditorApplication.hierarchyChanged += Refresh;
			
			this.AddManipulator(new ContextualMenuManipulator(evt =>
			{
				// TODO: Implement these
				//evt.menu.AppendAction("Cut", _ => { Cut(); });
				//evt.menu.AppendAction("Copy", _ => { Copy(); });
				//evt.menu.AppendAction("Paste", _ => { Paste(); });
				//evt.menu.AppendSeparator();
				
				evt.menu.AppendAction("Show In Project", ShowInScene);
				evt.menu.AppendAction("Reset/Position", ResetPosition, SelectedProps.Count() != 1 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
				evt.menu.AppendAction("Reset/Rotation", ResetRotation, SelectedProps.Count() != 1 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
				evt.menu.AppendAction("Reset/Both", ResetTransform, SelectedProps.Count() != 1 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
				evt.menu.AppendAction("Rename", RenameEvent, SelectedProps.Count() != 1 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
				evt.menu.AppendAction("Duplicate", DuplicateEvent, !SelectedProps.Any() ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
				evt.menu.AppendSeparator();
				evt.menu.AppendAction("Delete", DeleteAction, !SelectedProps.Any() ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
			}));
			
			RegisterCallback<KeyDownEvent>(KeyDownEvent);
			RegisterCallback<MouseDownEvent>(MouseDownEvent);
		}
		
		void MouseDownEvent(MouseDownEvent evt)
		{
			if (evt.button == 0)
			{
				Selection.activeGameObject = null;
			}
		}

		~PropsHierarchyVisualElement()
		{
			PropManager.RemovePropListChangedEvent(Refresh);
			EditorApplication.hierarchyChanged -= Refresh;
		}

		public void Refresh()
		{
			PropManager.Validate();
			
			Clear();
			
			VisualElement toolbar = new()
			{
				name = "tool-bar"
			};

			VisualElement addItem = new()
			{
				name = "add-button"
			};
			addItem.Add(new Image() { image = Resources.Load<Texture2D>("Icon/Add Dropdown"), name = "add-icon" });
			toolbar.Add(addItem);
			addItem.RegisterCallback<ClickEvent>(_ => { AddProp(); });
			ToolbarSearchField searchField = new();
			searchField.RegisterValueChangedCallback(evt => UpdateElements(evt.newValue));
			toolbar.Add(searchField);
			
			Add(toolbar);
			
			scrollView = new ScrollView();
			scrollView.name = "all-assets";
			Add(scrollView);
			UpdateElements();
			HighlightPropHierarchyItem(PropsHierarchyWindow.highlightedName, !PropsHierarchyWindow.highlightedName.IsNullOrWhitespace());
		}

		private void AddProp()
		{
			GenericMenu menu = new();
			

			Type baseType = typeof(PropType);
			Assembly assembly = baseType.Assembly;


			if (PropManager.Instance == null)
			{
				menu.AddDisabledItem(new GUIContent("No Prop Manager"));
			}
			else
			{
				IEnumerable<Type> allPropTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
				foreach (var propType in allPropTypes)
				{
					menu.AddItem(new GUIContent(propType.Name), false, () =>
					{
						PropManager.Instance.CreateNewPropFromGraph("new " + propType.Name, propType);
					});
				}
			}
			
			menu.ShowAsContext();
		}
		
		public void UpdateElements(string search = "")
		{
			if (scrollView == null)
			{
				scrollView = new ScrollView();
				scrollView.name = "all-assets";
				Add(scrollView);
			}
			
			scrollView.Clear();
			List<PropReferences> namedProps = PropManager.GetAllProps();
			
			if (!search.IsNullOrWhitespace())
			{
				namedProps = namedProps.Where(t => t.PropComponent.PropName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
			}
			
			if (!search.IsNullOrWhitespace() && namedProps.IsNullOrEmpty())
			{
				scrollView.Add(new Label("Cannot Find: " + search) { name = "empty-text" });
			}
			else if (namedProps.IsNullOrEmpty())
			{
				scrollView.Add(new Label("No Props in Scene") { name = "empty-text" });
			}
			else
			{
				foreach (PropReferences namedProp in namedProps.Where(namedProp => namedProp.PropComponent != null))
				{
					Action<PropHierarchyItem, MouseDownEvent> onClicked;
					scrollView.Add(new PropHierarchyItem(namedProp.PropComponent, OnVisualElementClicked));
				}
			}
		}	

		public void OnVisualElementClicked(PropHierarchyItem item, MouseDownEvent evt)
		{
			if (evt.button == 1)
			{
				if (!Selection.gameObjects.Contains(item.PropComponent.gameObject))
				{
					Selection.activeGameObject = item.PropComponent.gameObject;
					mainSelectedItem = item;
				}
			}
			if (evt.button != 0)
			{
				return;
			}

			switch (evt.clickCount)
			{
				case 1:
					if (evt.shiftKey)
					{
						List<VisualElement> allChildren = item.parent.Children().ToList();
						
						int firstIndex = allChildren.FindIndex(r=> r == item);
						int secondIndex = allChildren.FindIndex(r=> r == mainSelectedItem);
						List<VisualElement> result = allChildren.Skip(firstIndex).Take(1 + secondIndex - firstIndex).ToList();

						if (result.IsNullOrEmpty())
						{
							result = allChildren.Skip(secondIndex).Take(1 + firstIndex - secondIndex).ToList();
						}
						
						Selection.objects = result.Cast<PropHierarchyItem>().Select(t => t.PropComponent.gameObject).ToArray();
						break;
					}
					mainSelectedItem = item;
					
					if (evt.ctrlKey)
					{
						if (Selection.objects.Contains(item.PropComponent.gameObject))
						{
							List<Object> removedItems = Selection.objects.ToList();
							removedItems.Remove(item.PropComponent.gameObject);
							Selection.objects = removedItems.ToArray();
							break;
						}
						Selection.objects = Selection.objects.Add(item.PropComponent.gameObject);
						break;
						
					}
					
					Selection.activeGameObject = item.PropComponent.gameObject;
					break;
				case 2:
					SceneCameraAssistant.LookAt(item.PropComponent.transform);
					break;
			}
			evt.StopImmediatePropagation();
		}

        private void HighlightPropHierarchyItem(string name, bool isHighlighted)
        {
            foreach (var item in scrollView.Children())
            {
                if (item is PropHierarchyItem)
                {
                    if ((item as PropHierarchyItem).PropComponent.PropName == name)
                    {
                        (item as PropHierarchyItem).SetHighlighted(isHighlighted);
                    }
                }
            }
        }

        private void DeleteAction(DropdownMenuAction obj = null)
		{
			foreach (GameObject gameObject in SelectedProps)
			{
				Undo.DestroyObjectImmediate(gameObject);
			}
			PropManager.PropListChanged();
		}
		
		private void ShowInScene(DropdownMenuAction obj = null)
		{
			SceneView.lastActiveSceneView.FrameSelected();
		}
		
		private void RenameEvent(DropdownMenuAction obj = null)
		{
			PropHierarchyItem item = scrollView.contentContainer.Children().Cast<PropHierarchyItem>()
				.FirstOrDefault(p => p.PropComponent.gameObject == Selection.activeGameObject);
			item?.RenameField?.StartRename();
		}

		private void ResetPosition(DropdownMenuAction obj = null)
		{
			PropHierarchyItem item = scrollView.contentContainer.Children().Cast<PropHierarchyItem>()
				.FirstOrDefault(p => p.PropComponent.gameObject == Selection.activeGameObject);

			if(item != null)
				item.PropComponent.gameObject.transform.position = Vector3.zero;
		}

		private void ResetRotation(DropdownMenuAction obj = null)
		{
			PropHierarchyItem item = scrollView.contentContainer.Children().Cast<PropHierarchyItem>()
				.FirstOrDefault(p => p.PropComponent.gameObject == Selection.activeGameObject);
			
			if(item != null)
				item.PropComponent.gameObject.transform.rotation = Quaternion.identity;
		}


		private void ResetTransform(DropdownMenuAction obj = null)
		{
			ResetPosition();
			ResetRotation();
		}

		private void DuplicateEvent(DropdownMenuAction obj = null)
		{
			foreach (GameObject gameObject in SelectedProps)
			{
				Duplicate(gameObject.GetComponent<PropComponent>());
			}
		}
		
		private void Duplicate(PropComponent propComponent)
		{
			if (PropManager.Instance == null)
			{
				return;
			}

			string prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(propComponent.gameObject);
			Object instance = !prefab.IsNullOrWhitespace()
				? PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(prefab, typeof(Object)),
					PropManager.Instance.transform)
				: Object.Instantiate(propComponent.gameObject, PropManager.Instance.transform);

			string[] existingNames = PropManager.GetAllProps()?.Select(t => t.PropComponent.PropName).ToArray();
			instance.name = ObjectNames.GetUniqueName(existingNames, propComponent.PropName + " - Copy");
			PropManager.Instance.FindAndAddAllProps();
			
			Undo.RegisterCreatedObjectUndo(instance, "Duplicate Prop");
		}
		
		private void KeyDownEvent(KeyDownEvent evt)
		{
			switch (evt.keyCode)
			{
				case KeyCode.F2:
					RenameEvent();
					evt.StopPropagation();
					break;
				case KeyCode.Delete:
					DeleteAction();
					evt.StopPropagation();
					break;
				case KeyCode.DownArrow:
					if (mainSelectedItem.parent.IndexOf(mainSelectedItem) == mainSelectedItem.parent.childCount - 1)
					{
						break;
					}
					VisualElement newChild = mainSelectedItem.parent.ElementAt(mainSelectedItem.parent.IndexOf(mainSelectedItem) + 1);
					if (newChild is not PropHierarchyItem itemDown)
					{
						break;
					}
					mainSelectedItem = itemDown;
					itemDown.Focus();
					if (evt.shiftKey || evt.ctrlKey)
					{
						Selection.objects = Selection.objects.Add(itemDown.PropComponent.gameObject);
					}
					else
					{
						Selection.activeObject = itemDown.PropComponent.gameObject;
					}
					break;
				case KeyCode.UpArrow:
					if (mainSelectedItem.parent.IndexOf(mainSelectedItem) == 0)
					{
						break;
					}
					VisualElement newChildUp =  mainSelectedItem.parent.ElementAt(mainSelectedItem.parent.IndexOf(mainSelectedItem) - 1);
					if (newChildUp is not PropHierarchyItem itemUp)
					{
						break;
					}
					mainSelectedItem = itemUp;
					itemUp.Focus();
					if (evt.shiftKey || evt.ctrlKey)
					{
						Selection.objects = Selection.objects.Add(itemUp.PropComponent.gameObject);
					}
					else
					{
						Selection.activeObject = itemUp.PropComponent.gameObject;
					}

					break;
				case KeyCode.D:
					if (evt.ctrlKey)
					{
						DuplicateEvent();
					}
					break;
			}
		}
	}
}