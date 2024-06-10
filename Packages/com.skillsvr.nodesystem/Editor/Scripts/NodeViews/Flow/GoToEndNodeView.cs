using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(GoToInNode))]
	public class GoToEndNodeView : BaseNodeView
	{
		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new();
			// Gets the node
			if (nodeTarget is not GoToInNode goToEndNode)
			{
				return visualElement;
			}
			
			DropdownField conditionDropdownField = new DropdownField();
			conditionDropdownField.choices.Add("Go To 1");
			conditionDropdownField.choices.Add("Go To 2");
			conditionDropdownField.choices.Add("Go To 3");
			conditionDropdownField.choices.Add("Go To 4");
			
			conditionDropdownField.index = (int)goToEndNode.goToNodes;
			conditionDropdownField.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				goToEndNode.goToNodes = (GoToInNode.GoToNodes)conditionDropdownField.choices.FindIndex(t => t.Equals(evt.newValue));
			});
			
			visualElement.Add(conditionDropdownField);

			return visualElement;
		}
	}
	
}