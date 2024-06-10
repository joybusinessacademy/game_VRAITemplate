using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(SwitchNode))]
	public class SwitchNodeView : BaseNodeView
	{
		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
			if (nodeTarget is not SwitchNode switchNode)
			{
				return visualElement;
			}
			
			// Display first dropdown field
			DropdownField outputAmountDropdown = new DropdownField
			{
				label = "Amount",
				name = "output-amount-dropdown",
				value = switchNode.outputAmount.ToString()
			};

			for (int i = 1; i < 11; i++)
			{
				outputAmountDropdown.choices.Add(i.ToString());
			}

			outputAmountDropdown.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				switchNode.outputAmount = int.Parse(evt.newValue);
				RefreshPorts();
			});
			
			ScriptableObjectDropdown<FloatSO> scriptableObjectDropdown = new("Variable", switchNode.variable, evt => switchNode.variable = evt, () => EditorGUIUtility.PingObject(switchNode.variable));
			visualElement.Add(scriptableObjectDropdown);
			visualElement.Add(outputAmountDropdown);

			return visualElement;
		}

		public override void Dispose()
		{
			// Any ScriptableObjectDropdown<> items must dispose after use, otherwise cause memory leak.
			controlsContainer?.Query<ScriptableObjectDropdown<FloatSO>>().ForEach(x => x?.Dispose());
			base.Dispose();
		}

		public override bool RefreshPorts()
		{
			bool returnValue = base.RefreshPorts();

			if (nodeTarget is not SwitchNode switchNode)
			{
				return returnValue;
			}

			if (switchNode == null)
				return false;

			for (int i = 0; i < outputContainer.childCount; i++)
			{
                if (switchNode.outputAmount < i)
				{
					outputContainer[i].AddToClassList("hidden-output");
				}
				else
				{
					outputContainer[i].RemoveFromClassList("hidden-output");
				}
			}

            GetPortViewsFromFieldName("Default").ForEach(k => {
                k.RemoveFromClassList("hidden-output");
            });

            return returnValue;
		}
	}
}
