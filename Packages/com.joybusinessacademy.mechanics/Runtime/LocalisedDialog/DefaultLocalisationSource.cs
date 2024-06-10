using System;
using UnityEngine.UIElements;

namespace DialogExporter
{
	[Serializable]
	public class DefaultLocalisationSource : ILocalisationSource 
	{
		
		public DefaultLocalisationSource()
		{
			this.dialog = "New dialog";
		}
		public DefaultLocalisationSource(string dialog = "New dialog")
		{
			this.dialog = dialog;
		}
		
		
		public string dialog = "New dialog";

		public bool CanBeEdited() => true;

		public void EditDialog(string newDialog)
		{
			if (CanBeEdited())
			{
				dialog = newDialog;
			}
		}

		public string Translation(string term)
		{
			return dialog;
		}
		
		public static VisualElement VisualElement(LocalisedString localisedString)
		{
			TextField textField = new()
			{
				value = localisedString.LocalisationSource.Translation(),
				multiline = true,
			};
			textField.style.flexGrow = 1;
			textField.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				localisedString.LocalisationSource = new DefaultLocalisationSource(evt.newValue);
			});

			return textField;
		}

		public void GetLocalisationItems()
		{
		}
	}
}