using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(MathsNode))]
	public class MathsNodeView : BaseNodeView
	{
		private MathsNode Node => AttachedNode<MathsNode>();
		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
			ScriptableObjectDropdown<FloatSO> alteredVariableDropdown = new("Change", Node.alteredVariableSO,
				evt => AttachedNode<MathsNode>().alteredVariableSO = evt,
				() => EditorGUIUtility.PingObject(Node.alteredVariableSO));
			DropdownField conditionDropdownField = ConditionDropdownField(AttachedNode<MathsNode>());
			FloatInputWithTextBox modifierVariableDropdown = new("", AttachedNode<MathsNode>().modifierVariableSO,
				evt => AttachedNode<MathsNode>().modifierVariableSO = evt, AttachedNode<MathsNode>().modifierVariable,
				newFloat => AttachedNode<MathsNode>().modifierVariable = newFloat,
				() => EditorGUIUtility.PingObject(AttachedNode<MathsNode>().modifierVariableSO));

			visualElement.Add(alteredVariableDropdown);
			visualElement.Add(conditionDropdownField);
			visualElement.Add(modifierVariableDropdown);

			return visualElement;
		}

		private static DropdownField ConditionDropdownField(MathsNode conditionNode)
		{
			DropdownField conditionDropdownField = new DropdownField();
			conditionDropdownField.choices.Add("Set");
			conditionDropdownField.choices.Add("Add");
			conditionDropdownField.choices.Add("Remove");
			conditionDropdownField.choices.Add("Multiply");
			conditionDropdownField.choices.Add("Divide");

			conditionDropdownField.index = (int)conditionNode.condition;
			conditionDropdownField.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				conditionNode.condition = (MathsNode.EMaths)conditionDropdownField.choices.FindIndex(t => t.Equals(evt.newValue));
			});
			return conditionDropdownField;
		}
	}

}