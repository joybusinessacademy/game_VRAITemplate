using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace Scripts.VisualElements
{
	public class ListDropdown<TListElement> : VisualElement
	where TListElement : new()
	{

		public bool showExpandButton = true;
		public int maxItems = 0;

		public ListDropdown(string listLabel, List<TListElement> listElement, VisualElementToAdd action, int maxItems = 0, bool hideAddButton = false , bool showExpandButton = true)
		{
			styleSheets.Add(Resources.Load<StyleSheet>("lists"));
			name = "list-container";
			this.listElement = listElement;
			this.action = action;
			this.listLabel = listLabel;
			this.maxItems = maxItems;
			this.hideAddButton = hideAddButton;
			this.showExpandButton = showExpandButton;

			Refresh();
		}

		readonly List<TListElement> listElement;
		private readonly VisualElementToAdd action;
		private readonly string listLabel;

		public bool Expanded = true;
		bool hideAddButton;

		public IconButton addButton;

		public delegate VisualElement VisualElementToAdd(TListElement listElement);
		
		public readonly List<VisualElement> ListVisualElements = new List<VisualElement>();

		public void Refresh()
		{
			Clear();
			ListVisualElements.Clear();
			if(showExpandButton)
			{
				Button slotsExpandButton = ExpandButton(listLabel, Expanded, () =>
				{
					Expanded = !Expanded;
					Refresh();
				}, listElement.Count);
				Add(slotsExpandButton);
			}
			else
			{
				Label label = new Label(listLabel);
				Add(label);
			}

			if (Expanded)
			{
				foreach (TListElement rankOrderSlot in listElement)
				{
					VisualElement element = action.Invoke(rankOrderSlot);
					element.name = "list-element";
					ListVisualElements.Add(element);
					
					Add(element);
				}
				
				if(!hideAddButton)
					Add(GetAddButton(listElement, Refresh));
			}

			onRefresh.Invoke();
		}
		
		public Action onRefresh = () => { };
		public Action onAddButtonClicked = () => { };
		
		private Button ExpandButton(string label, bool currentState, Action onClick, int elementAmount = -1)
		{
			var button = new Button(onClick);

			string text = label;
			button.Add(new Label(text));
			if (elementAmount >= 0)
			{
				Label element = new Label("(" + elementAmount + ")")
				{
					style =
					{
						flexGrow = 1,
						color = new StyleColor(new Color(0.55f, 0.55f, 0.55f))
					}
				};
				button.Add(element);
			}
			Image visualElement = new()
			{
				image = Resources.Load<Texture2D>("Icon/" + (currentState ? "Up" : "Down"))
			};
			button.Add(visualElement);
			return button;
		}
		
		private IconButton GetAddButton<TData>(ICollection<TData> list, Action test = null) where TData : new()
		{
			if(addButton == null)
			{
				addButton = new IconButton("Add", 16)
				{
					tooltip = "Add"
				};

				addButton.clicked += ()=> OnAddButtonClicked(list);

				if (test != null)
				{
					addButton.clicked += test;
				}
			}

			
			if (listElement.Count >= maxItems && maxItems > 0)
			{
				addButton.SetEnabled(false);
				tooltip = "Max Elements Reached";
			}
			else
			{
				addButton.SetEnabled(true);
				tooltip = "Add";
			}
			return addButton;
		}

		public void OnAddButtonClicked<TData>(ICollection<TData> list) where TData : new()
		{
			TData data = new TData();
			list.Add(data);

			onAddButtonClicked?.Invoke();
		}

	}
}