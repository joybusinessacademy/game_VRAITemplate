using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.VisualElements
{
	public class Breadcrumbs : VisualElement
	{
		public Breadcrumbs()
		{
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/Breadcrumbs"));
			
			SetupBreadcrumb(new Dictionary<string, Action>()
			{
				{"Home", null}
			});
		}

		public Breadcrumbs(Dictionary<string, Action> breadcrumbs)
		{
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/Breadcrumbs"));
			
			SetupBreadcrumb(breadcrumbs);
		}
		
		public void SetupBreadcrumb(Dictionary<string, Action> breadcrumbs)
		{
			Clear();
			if (breadcrumbs.IsNullOrEmpty())
			{
				return;
			}
			Button item = new();
			Image image = new();
			foreach ((string key, Action value) in breadcrumbs)
			{
				item = AddItem(key, value);
				image = AddArrow();
			}
			
			item.AddToClassList("current-level");
			image.RemoveFromHierarchy();
		}



		public Image AddArrow()
		{
			var rightArrow = new Image
			{
				image = Resources.Load<Texture2D>("Icon/breadcrumb"),
				name = "chevron"
			};
			Add(rightArrow);
			return rightArrow;
		}
		
		public Button AddItem(string name, Action onClick)
		{
			Button item;
			if (onClick == null)
			{
				item = new Button()
				{
					text = name,
					style =
					{
						alignSelf = Align.Center,
						fontSize = 14
					}
				};
				item.style.backgroundColor = new Color(0, 0, 0, 0);
				Add(item);
				return item;
			}
			item = new Button(onClick)
			{
				text = name,
				style =
				{
					alignSelf = Align.Center,
					fontSize = 14
				}
			};
			Add(item);
			return item;
		}
	}
}