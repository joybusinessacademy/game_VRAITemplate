using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace Scripts.VisualElements.Dropdowns
{
	public class BaseDropdown : DisposableVisualElement
	{
		private readonly Label label;
		protected readonly DropdownField Dropdown;
		
		// TODO: Add button to go to selected
		public IconButton Button;

		/// <summary>
		/// The label text for the dropdown
		/// If set to null or empty string will not show a label
		/// </summary>
		public string LabelText
		{
			get => label.text;
			set
			{
				label.text = value;
				label.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
			}
		}
		protected KeyValuePair<string, Action> NullChoice;

		protected readonly Dictionary<string, Action> MainChoices;
		protected readonly Dictionary<string, Action> AlternateChoices;

		public BaseDropdown(KeyValuePair<string, Action> nullChoice = new(), Dictionary<string, Action> alternateChoices = null, Dictionary<string, Action> mainChoices = null)
		{
			NullChoice = nullChoice;
			
			MainChoices = mainChoices;
			MainChoices ??= new Dictionary<string, Action>();
			AlternateChoices = alternateChoices;
			AlternateChoices ??= new Dictionary<string, Action>();

			label = new Label();
			Add(label);
			

			
			Dropdown = new DropdownField
			{
				value = "null"
			};
			Dropdown.RegisterCallback<FocusEvent>(UpdateValues);
			Dropdown.RegisterValueChangedCallback(OnChange);
			Add(Dropdown);
		}

		private void UpdateValues(FocusEvent evt)
		{
			Dropdown.choices.Clear();
			
			if (!NullChoice.Key.IsNullOrWhitespace())
			{
				Dropdown.choices.Add(NullChoice.Key);
				if (!MainChoices.IsNullOrEmpty() || !AlternateChoices.IsNullOrEmpty())
				{
					Dropdown.choices.Add(null);
				}
			}
			
			if (!MainChoices.IsNullOrEmpty())
			{
				Dropdown.choices.AddRange(MainChoices.Keys);
				if (!AlternateChoices.IsNullOrEmpty())
				{
					Dropdown.choices.Add(null);
				}
			}
			
			Dropdown.choices.AddRange(AlternateChoices.Keys);
		}
		
		protected void OnChange(ChangeEvent<string> evt)
		{
			if (NullChoice.Key == evt.newValue)
			{
				NullChoice.Value?.Invoke();
				return;
			}
			if (AlternateChoices != null && AlternateChoices.TryGetValue(evt.newValue, out Action choice))
			{
				choice?.Invoke();
				return;
			}
			
			if (MainChoices != null && MainChoices.TryGetValue(evt.newValue, out choice))
			{
				choice?.Invoke();
			}
		}

		public override void Dispose()
		{
			
		}
	}
}