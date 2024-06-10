using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.VisualElements
{
	public class TextLabel : VisualElement
	{
		public string Text
		{
			get => rightLabel.text;
			set
			{
				if (value.IsNullOrWhitespace())
				{
					value = "null";
					rightLabel.name = "null-item";
				}
				else
				{
					rightLabel.name = null;
				}
				rightLabel.text = value;
			}
		}

		public string Label
		{
			get => leftLabel.text;
			set => leftLabel.text = value;
		}

		public Label leftLabel;
		public Label rightLabel;

		

		public TextLabel(string name = "Label", string text = null)
		{
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/TextLabel"));

			leftLabel = new Label();
			Add(leftLabel);
			
			rightLabel = new Label();
			Add(rightLabel);
			
			Text = text;
			Label = name;
		}
	}
}