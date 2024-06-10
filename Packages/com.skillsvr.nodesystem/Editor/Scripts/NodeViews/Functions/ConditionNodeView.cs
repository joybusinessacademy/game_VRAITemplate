using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{

	[NodeCustomEditor(typeof(ConditionNode))]
	public class ConditionNodeView : BaseNodeView
	{
		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
			// Gets the node
			if (nodeTarget is not ConditionNode conditionNode)
			{
				return visualElement;
			}
			
			FloatInputWithTextBox firstVariableDropdown = new("", AttachedNode<ConditionNode>().firstVariableSO,
				evt => conditionNode.firstVariableSO = evt, conditionNode.firstVariable,
				newFloat => conditionNode.firstVariable = newFloat, 
				() => EditorGUIUtility.PingObject(AttachedNode<ConditionNode>().firstVariableSO));
			DropdownField conditionDropdownField = ConditionDropdownField(conditionNode);
			FloatInputWithTextBox secondVariableDropdown = new("", AttachedNode<ConditionNode>().secondVariableSO,
				evt => conditionNode.secondVariableSO = evt, conditionNode.secondVariable,
				newFloat => conditionNode.secondVariable = newFloat,
				() => EditorGUIUtility.PingObject(AttachedNode<ConditionNode>().secondVariableSO));
			
			visualElement.Add(firstVariableDropdown);
			visualElement.Add(conditionDropdownField);
			visualElement.Add(secondVariableDropdown);

			return visualElement;
		}
		
		private static DropdownField ConditionDropdownField(ConditionNode conditionNode)
		{
			DropdownField conditionDropdownField = new DropdownField();
			conditionDropdownField.choices.Add("Equals");
			conditionDropdownField.choices.Add("Not Equals");
			conditionDropdownField.choices.Add("Less Than");
			conditionDropdownField.choices.Add("Less Than or Equals");
			conditionDropdownField.choices.Add("Greater Than");
			conditionDropdownField.choices.Add("Greater Than or Equals");
			conditionDropdownField.index = (int)conditionNode.condition;
			conditionDropdownField.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				conditionNode.condition = (ConditionNode.ECondition)conditionDropdownField.choices.FindIndex(t => t.Equals(evt.newValue));
			});
			return conditionDropdownField;
		}
	}
}