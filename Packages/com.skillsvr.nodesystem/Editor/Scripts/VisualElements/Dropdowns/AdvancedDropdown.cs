using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.VisualElements
{
	public class AdvancedDropdown : VisualElement
	{
		public bool Expanded;

		
		public AdvancedDropdown(string listLabel, VisualElementsToAdd action, bool startExpanded = false)
		{
			styleSheets.Add(Resources.Load<StyleSheet>("AdvancedDropdown"));;
			this.action = action;
			this.listLabel = listLabel;
			Expanded = startExpanded;
			Refresh();
		}
		private readonly VisualElementsToAdd action;
		private readonly string listLabel;

		public delegate VisualElement[] VisualElementsToAdd();

		private VisualElement elementContainer;
		
		public void Refresh()
		{
			Clear();
			Button slotsExpandButton = ExpandButton(listLabel, Expanded, () =>
			{
				Expanded = !Expanded;
				Refresh();
			});
			Add(slotsExpandButton);

			elementContainer = new VisualElement
			{
				name = "element-container"
			};
			
			Add(elementContainer);
			if (Expanded)
			{
				VisualElement[] elements = action.Invoke();
				foreach (VisualElement element in elements)
				{
					elementContainer.Add(element);
				}
			}
			else
			{
				slotsExpandButton.AddToClassList("collapsed");
			}
		}
		
		private static Button ExpandButton(string label, bool currentState, Action onClick)
		{
			Button button = new(onClick);

			string text = label;
			Label heading = new(text);
			button.Add(heading);

			
			Image visualElement = new()
			{
				image = Resources.Load<Texture2D>("Icon/" + (currentState ? "Up" : "Down")),
			};
			button.Add(visualElement);
			return button;
		}
	}
}