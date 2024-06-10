using UnityEngine;
using UnityEngine.UIElements;

namespace VisualElements
{
	public class RenameVisualElement : VisualElement
	{
		TextField textField;
		Label label;
		private string currentValue; 
		private Rename renameEvent;
		private ValidateName validateEvent;
		
		public RenameVisualElement(string originalText, Rename renameEvent, ValidateName validateEvent)
		{
			currentValue = originalText;
			
			styleSheets.Add(Resources.Load<StyleSheet>("RenameVisualElement"));
			this.validateEvent = validateEvent;
			this.renameEvent = renameEvent;
			SetupLabel();
		}
        
		public delegate void Rename(string newName);
		public delegate bool ValidateName(string newName);

		private void SetupLabel()
		{
			Clear();
			VisualElement button = new();
			label = new Label(currentValue);
			button.Add(label);
			Add(button);
		}

		public void StartRename()
		{
			SetupTextField();
		}
		
		public void StopRename()
		{
			SetupLabel();
		}
		
		private void SetupTextField()
		{
			Clear();
			TextField textField = new TextField();
			textField.isDelayed = true;
			textField.value = currentValue;
			
			textField.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				string newValue = evt.newValue.Trim();
				if (validateEvent.Invoke(newValue))
				{
					renameEvent.Invoke(newValue);
					currentValue = newValue;
					SetupLabel();
				}
				else
				{
					SetupLabel();
				}
			});
			
			textField.Q(TextField.textInputUssName).RegisterCallback<KeyDownEvent>(e =>
			{
				if (e.keyCode == KeyCode.Escape)
				{
					SetupLabel();
				}
			});
			textField.RegisterCallback<BlurEvent>(evt =>
			{
				if (validateEvent.Invoke(textField.text))
				{
					renameEvent.Invoke(textField.text);
					currentValue = textField.text;
					SetupLabel();
				}
				else
				{
					SetupLabel();
				}
				SetupLabel();
			});

			
			
			Add(textField);
			textField.Focus();
		}
    }
}