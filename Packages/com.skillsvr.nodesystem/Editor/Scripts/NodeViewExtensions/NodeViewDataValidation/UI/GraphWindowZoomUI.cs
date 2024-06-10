using GraphProcessor;
using Scripts.VisualElements;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.Graph
{
	public class GraphWindowZoomUI : VisualElement
	{
		private readonly Vector3 minScaleAmount = new Vector3(0.15f, 0.15f, 0.80f);
		private readonly Vector3 maxScaleAmount = new Vector3(2.00f, 2.00f, 0.80f);

		public GraphWindowZoomUI()
		{
			CreateUI();
		}

		private void CreateUI()
		{
			this.SetPadding(0);
			this.SetMargin(4, 0, 0, 0);
			this.style.flexDirection = FlexDirection.Row;

			Label zoomText = new Label("Zoom");
			zoomText.style.alignSelf = Align.Center;
			IconButton zoomIn = AddIconButton("Add", OnZoomInClicked, "Will zoom into the graph view");
			IconButton zoomOut = AddIconButton("minus", OnZoomOutClicked, "Will zoom out of the graph view");

			this.Add(zoomText);
			this.Add(zoomIn);
			this.Add(zoomOut);
		}

		private void OnZoomInClicked()
		{
			BaseGraphView baseGraph = BaseGraphWindow.Instance.graphView;

			if (baseGraph != null)
			{
				Vector3 zoomedInScale = baseGraph.graph.scale * 1.2f;

				if (zoomedInScale.x > 2.00f || zoomedInScale.y > 2.00f)
					zoomedInScale = maxScaleAmount;

				baseGraph.UpdateViewTransform(baseGraph.graph.position, zoomedInScale);
			}
		}

		private void OnZoomOutClicked()
		{
			BaseGraphView baseGraph = BaseGraphWindow.Instance.graphView;

			if (baseGraph != null)
			{
				Vector3 zoomedOutScale = baseGraph.graph.scale * 0.8f;

				if (zoomedOutScale.x < 0.15f || zoomedOutScale.y < 0.15f)
					zoomedOutScale = minScaleAmount;

				baseGraph.UpdateViewTransform(baseGraph.graph.position, zoomedOutScale);
			}
		}

		public IconButton AddIconButton(string iconName, Action onClick, string tooltip)
		{
			IconButton iconButton = new IconButton(iconName, 16, 4)
			{
				tooltip = tooltip
			};

			iconButton.clicked += onClick;
			return iconButton;
		}
	}
}
