
using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VisualElements
{
    /// <summary>
    /// Search Float scriptable object assets and show in dropdown. MUST DISPOSE after use, otherwise will cause deadlock reference and memory leak.
    /// </summary>
    public class StringInputWithTextBox : ScriptableObjectDropdown<TagSO>
    {
		private readonly FloatField secondVariableField;
		protected sealed override string DefaultName => "Number";

		public StringInputWithTextBox(string label, TagSO outputValue, ChangeDropdown changeEvent, float variable, ChangeFloat changeFloat, Action showObjectInProject)
			: base(label, outputValue, changeEvent, showObjectInProject)
		{
			// Display secondVariable
			secondVariableField = new FloatField()
			{
				value = variable
			};
			secondVariableField.RegisterCallback<ChangeEvent<float>>(evt =>
			{
				changeFloat.Invoke(evt.newValue);
			});

			Add(secondVariableField);
			style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
			DropdownField.RegisterCallback<ChangeEvent<string>>(evt => RefreshTextBox(evt.newValue == DefaultName));
			RefreshTextBox(outputValue == null);
		}

		public void RefreshTextBox(bool enable)
		{
			secondVariableField?.SetEnabled(enable);
		}

		public delegate void ChangeFloat(float newFloat);
	}
}
