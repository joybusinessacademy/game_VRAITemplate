using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace VisualElements
{
	public class IconButton : Button
	{
		public Image iconImage;
		
		public IconButton(string iconName, int iconSize = -1, int padding = 1, bool isSquare = false) 
		{
			SetupIcon(iconName, iconSize, padding);

			if (isSquare)
			{
				style.width = iconSize + padding * 2;
				style.height = iconSize + padding * 2;
			}
		}

		public void SetupIcon(string iconName, int iconSize, int padding)
		{
			style.paddingBottom = padding;
			style.paddingLeft = padding;
			style.paddingRight = padding;
			style.paddingTop = padding;


			
			style.alignItems = new StyleEnum<Align>(Align.Center);

			iconImage = new()
			{
				image = Resources.Load<Texture2D>("Icon/" + iconName),
				name = "IconImage"
			};
			if (iconSize > 0)
			{
				iconImage.style.width = iconSize;
				iconImage.style.height = iconSize;
			}

			Add(iconImage);
		}

		public IconButton(Action action, string iconName, int iconSize = -1, int padding = 1, bool isSquare = false) : base(action)
		{
			SetupIcon(iconName, iconSize, padding);
			
			if (isSquare)
			{
				style.width = iconSize + padding * 2;
				style.height = iconSize + padding * 2;
			}
		}
	}
}
