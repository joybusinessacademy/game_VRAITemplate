using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.VisualElements
{
	public class Divider : VisualElement
	{
		private const int defaultMargin = 3;
		
		public Divider(int margin = defaultMargin)
		{
			Setup(Color.gray, margin);
		}
		
		public Divider(Color color, int margin = defaultMargin)
		{
			Setup(color, margin);
		}
		
		public void Setup(Color color, int margin)
		{
			style.backgroundColor = new StyleColor(color);
			style.minWidth = 1;
			style.minHeight = 1;
			style.marginBottom = margin;
			style.marginTop = margin;
			style.marginLeft = margin;
			style.marginRight = margin;
		}
	}
}