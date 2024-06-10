using System;
using GraphProcessor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.Graph
{
	public class SkillsVRGraphHelperBar : VisualElement, IDisposable
	{
		private readonly VisualElement container;

		public BaseGraphView GraphView { get; private set; }

		public SkillsVRGraphHelperBar(BaseGraphView graphView)
		{
			GraphView = graphView;
			name = "helperbar";
			styleSheets.Add(Resources.Load<StyleSheet>("SkillsStyles/helperbar"));

			container = new VisualElement
			{
				name = "zoomContainer"
			};

			this.Add(container);

			Refresh();
		}

		public void Dispose()
		{
			GraphView = null;
		}

		public void Refresh()
		{
			AddButtons();
		}

		protected void AddButtons()
		{
			container.Add(new GraphWindowZoomUI());
		}

		public VisualElement AddTextButton(string text, Action onClick)
		{
			Button button = new Button()
			{
				text = text,
				name = "textButton"
			};

			button.clicked += onClick;
			return button;
		}

		public VisualElement AddIconButton(string iconName, Action onClick, string tooltip)
		{
			IconButton iconButton = new IconButton(iconName, 16, 4)
			{
				tooltip = tooltip
			};

			iconButton.clicked += onClick;
			return iconButton;
		}

		public VisualElement AddGap(int width)
		{
			VisualElement visualElement = new VisualElement()
			{
				name = "gap",
				style = { width = width },
			};
			return visualElement;
		}
	}
}