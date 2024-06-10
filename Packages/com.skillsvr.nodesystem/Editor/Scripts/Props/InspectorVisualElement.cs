using GraphProcessor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Props
{
	public class InspectorVisualElement : VisualElement
	{
		private BaseNodeView baseNodeView;
		private readonly Label nodeType;
		private readonly TextField nodeName;
		private readonly VisualElement content;
		private readonly VisualElement icon;
		private readonly VisualElement SelectedItem;
		
		public InspectorVisualElement()
		{
			Resources.Load<VisualTreeAsset>("UXML/InspectorWindow")?.CloneTree(this);
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/SkillsVR"));
			nodeType = this.Q<Label>("type");
			nodeName = this.Q<TextField>("name");
			content = this.Q<VisualElement>("content");
			icon = this.Q<VisualElement>("icon");

			SelectedItem = new VisualElement()
			{
				name = "inspector-selected-item",
				pickingMode = PickingMode.Ignore,
			};
			SelectedItem.styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/InspectorSelected"));
			
			this.Q<VisualElement>("unity-content-container").style.paddingBottom = 22;
			this.Q<VisualElement>("unity-content-container").style.paddingRight = 22;
			this.Q<VisualElement>("unity-content-container").style.paddingTop = 22;
			this.Q<VisualElement>("unity-content-container").style.paddingLeft = 22;
			this.Q<VisualElement>("unity-content-container").style.minWidth = 200;
			
			style.flexGrow = 1;
			
			nodeName.RegisterValueChangedCallback(ChangeName);
			
			SetNode(null);
		}

		private void ChangeName(ChangeEvent<string> evt)
		{
			if (baseNodeView == null)
			{
				return;
			}
			baseNodeView.nodeTarget.SetCustomName(evt.newValue);
			baseNodeView.UpdateTitle();
		}


		public void SetNode(BaseNodeView nodeView)
		{
			if (nodeView == null)
			{
				Reset();
				return;
			}
			
			baseNodeView = nodeView;
			baseNodeView.RegisterCallback<DetachFromPanelEvent>(t => Reset());
			
			SelectedItem.style.display = DisplayStyle.Flex;
			baseNodeView.Add(SelectedItem);
			
			nodeName.SetEnabled(true);

			nodeType.text = ObjectNames.NicifyVariableName(baseNodeView.nodeTarget.GetType().Name);
			nodeName.value = baseNodeView.nodeTarget.GetCustomName();
			
			GenerateVisualElement();
		}

		public void GenerateVisualElement()
		{
			content.Clear();

			VisualElement visualElement = baseNodeView?.boundVisualElement?.GetNewVisualElement();

			if (visualElement != null)
			{
				content.Add(visualElement);
			}

		}

		public void Reset()
		{
			baseNodeView = null;
			nodeName.value = "No node selected";
			nodeType.text = "Select node to edit";
			nodeName.SetEnabled(false);
			content.Clear();
				
			SelectedItem.style.display = DisplayStyle.None;
		}
	}
}