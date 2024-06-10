using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(EndNode))]
	public class EndNodeView : BaseNodeView
	{
		public override VisualElement GetNodeVisualElement()
		{
			return null;
		}

		public override VisualElement GetInspectorVisualElement()
		{
			var inspectorVisualElement = new VisualElement();
			if (owner.graph is MainGraph)
			{
				return inspectorVisualElement;
			}
			
			EndNode endNode = AttachedNode<EndNode>();
			
			Toggle fadeToggle = endNode.CustomToggle(nameof(endNode.fade), "Fade out");
			fadeToggle.RegisterCallback<ChangeEvent<bool>>(_ => { RefreshNode(); });
            
			inspectorVisualElement.Add(fadeToggle);
			

			if (endNode.fade)
			{
				VisualElement fadeContainer = new();
				inspectorVisualElement.Add(fadeContainer);
				
				fadeContainer.name = "fade-container";
				fadeContainer.Add(new Label("Fade Settings:"));
				fadeContainer.Add(endNode.CustomFloatField(nameof(endNode.fadeDuration)));
				ColorField colorField = new ColorField
				{
					value = endNode.fadeColor,
					showAlpha = false,
					label = "Fade Color: ",
				};
				colorField.RegisterCallback<ChangeEvent<Color>>(evt => { endNode.fadeColor = evt.newValue; });
				fadeContainer.Add(colorField);
			}
			
			return inspectorVisualElement;
		}
	}
}