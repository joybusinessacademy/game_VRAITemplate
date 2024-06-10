using System;
using System.Linq;
using Props;
using SceneNavigation;
using Scripts.VisualElements;
using SkillsVR.UnityExtenstion;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace Scripts.CustomWindows.Hierarchy
{
	public class PropHierarchyItem : VisualElement
	{
		public readonly PropComponent PropComponent;
		public readonly RenameVisualElement RenameField;
		private readonly Action<PropHierarchyItem, MouseDownEvent> onClicked;
        private Label editLable;
        public PropHierarchyItem(PropComponent propComponent, Action<PropHierarchyItem, MouseDownEvent> onClicked)
		{
			this.onClicked = onClicked;
			PropComponent = propComponent;
			focusable = true;
			tooltip = propComponent.PropName;

			Selection.selectionChanged += SelectionChanged;

			RegisterCallback<MouseDownEvent>(MouseDownEvent);

			if (propComponent == null)
			{
				return;
			}
			GameObject baseObject = propComponent.gameObject;
			if (baseObject == null)
			{
				return;
			}

			Clear();

			Toggle toggleActive = new() { value = propComponent.gameObject.activeSelf };
			toggleActive.RegisterValueChangedCallback(evt =>
			{
				baseObject.SetActive(evt.newValue);
				UpdateEnableVisuals(evt.newValue);
			});
			Add(toggleActive);


			RenameField = new RenameVisualElement(propComponent.PropName, RenameEvent, propComponent.CanSetName);
			UpdateEnableVisuals(propComponent.gameObject.activeSelf);

			Add(RenameField);
			

        }

		void MouseDownEvent(MouseDownEvent evt)
		{
			onClicked.Invoke(this, evt);
			CheckDisplayStatic();
		}

		public void SelectionChanged()
		{
			CheckDisplayStatic();
		}

		private void CheckDisplayStatic()
		{
			if (PropComponent == null || PropComponent.gameObject == null)
			{
				RemoveFromHierarchy();
				return;
			}
			if (Selection.objects.Contains(PropComponent.gameObject))
			{
				AddToClassList("selected");
			}
			else
			{
				RemoveFromClassList("selected");
			}
		}

		~PropHierarchyItem()
		{
			Selection.selectionChanged -= SelectionChanged;
		}


		public void UpdateEnableVisuals(bool enabled)
		{
			if (!enabled)
			{
				RenameField.AddToClassList("object-disabled");
			}
			else
			{
				RenameField.RemoveFromClassList("object-disabled");
			}
		}



        public void SetHighlighted(bool isHighlighted)
		{
          if (isHighlighted)
            {
                AddToClassList("object-highlighted");
                editLable = new Label("Editing");
				Add(editLable);
            }
            else
            {
                RemoveFromClassList("object-highlighted");
				Remove(editLable);
            }

        }

		private void RenameEvent(string newName)
		{
			Undo.RecordObject(PropComponent.gameObject, "Rename prop");
			PropComponent.TrySetName(newName);
			PropManager.PropListChanged();
		}
	}
}