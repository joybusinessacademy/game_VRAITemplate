﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace GraphProcessor
{
	public class PortView : Port
	{
		public string fieldName => fieldInfo.Name;
		public Type fieldType => fieldInfo.FieldType;
		public new Type portType;
		public BaseNodeView owner { get; private set; }
		public PortData portData;

		public event Action<PortView, Edge> OnConnected;
		public event Action<PortView, Edge> OnDisconnected;

		protected FieldInfo fieldInfo;
		protected BaseEdgeConnectorListener	listener;

		string userPortStyleFile = "PortViewTypes";

		private List<EdgeView> edges = new List<EdgeView>();

		public int connectionCount => edges.Count;

		readonly string portStyle = "GraphProcessorStyles/PortView";

        public PortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
            : base(portData.vertical ? Orientation.Vertical : Orientation.Horizontal, direction, Capacity.Multi, portData.displayType ?? fieldInfo.FieldType)
		{
			this.fieldInfo = fieldInfo;
			this.listener = edgeConnectorListener;
			this.portType = portData.displayType ?? fieldInfo.FieldType;
			this.portData = portData;
			this.portName = fieldName;

			styleSheets.Add(Resources.Load<StyleSheet>(portStyle));

			UpdatePortSize();

			var userPortStyle = Resources.Load<StyleSheet>(userPortStyleFile);
			if (userPortStyle != null)
				styleSheets.Add(userPortStyle);
			
			if (portData.vertical)
				AddToClassList("Vertical");
			
			this.tooltip = portData.tooltip;
		}

		public static PortView CreatePortView(Direction direction, FieldInfo fieldInfo, PortData portData, BaseEdgeConnectorListener edgeConnectorListener)
		{
			var pv = new PortView(direction, fieldInfo, portData, edgeConnectorListener)
			{
				m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener)
			};
			pv.AddManipulator(pv.m_EdgeConnector);

			// Force picking in the port label to enlarge the edge creation zone
			VisualElement portLabel = pv.Q("type");
			if (portLabel != null)
			{
				portLabel.pickingMode = PickingMode.Position;
				portLabel.style.flexGrow = 1;
			}

			// hide label when the port is vertical
			if (portData.vertical && portLabel != null)
				portLabel.style.display = DisplayStyle.None;
			
			// Fixup picking mode for vertical top ports
			if (portData.vertical)
				pv.Q("connector").pickingMode = PickingMode.Position;

			return pv;
		}

		/// <summary>
		/// Update the size of the port view (using the portData.sizeInPixel property)
		/// </summary>
		public void UpdatePortSize()
		{
			int size = portData.sizeInPixel == 0 ? 15 : portData.sizeInPixel;
			VisualElement connector = this.Q("connector");
			connector.style.backgroundImage = Resources.Load<Texture2D>(connectionCount == 0 ? "Icon/Arrow-Inactive" : "Icon/Arrow-Active");
			VisualElement cap = connector.Q("cap");
			connector.style.width = size;
			connector.style.height = size;
			cap.style.width = 5;
			cap.style.height = 5;

			style.display = portName.IsNullOrWhitespace() ? DisplayStyle.None : DisplayStyle.Flex;

			// Update connected edge sizes:
			edges.ForEach(e => e.UpdateEdgeSize());
		}

		public virtual void Initialize(BaseNodeView nodeView, string name)
		{
			this.owner = nodeView;
			AddToClassList(fieldName);

			// Correct port type if port accept multiple values (and so is a container)
			if (direction == Direction.Input && portData.acceptMultipleEdges && portType == fieldType) // If the user haven't set a custom field type
			{
				if (fieldType.GetGenericArguments().Length > 0)
					portType = fieldType.GetGenericArguments()[0];
			}

			if (name != null)
				portName = name;
			visualClass = "Port_" + portType.Name;
			tooltip = portData.tooltip;
		}

		public override void Connect(Edge edge)
		{
			OnConnected?.Invoke(this, edge);

			base.Connect(edge);

			BaseNodeView inputNode = (edge.input as PortView)?.owner;
			BaseNodeView outputNode = (edge.output as PortView)?.owner;

			edges.Add(edge as EdgeView);

			inputNode?.OnPortConnected(edge.input as PortView);
			outputNode?.OnPortConnected(edge.output as PortView);
		}

		public override void Disconnect(Edge edge)
		{
			OnDisconnected?.Invoke(this, edge);

			base.Disconnect(edge);

			if (!((EdgeView)edge).isConnected)
			{
				return;
			}

			BaseNodeView inputNode = (edge.input as PortView)?.owner;
			BaseNodeView outputNode = (edge.output as PortView)?.owner;

			inputNode?.OnPortDisconnected(edge.input as PortView);
			outputNode?.OnPortDisconnected(edge.output as PortView);

			edges.Remove((EdgeView)edge);
		}

		public override void DisconnectAll()
		{
			EdgeView[] list = this.GetEdges().ToArray();
			foreach (EdgeView edge in list)
			{
				Disconnect(edge);
			}
		}

		public void UpdatePortView(PortData data)
		{
			if (data.displayType != null)
			{
				base.portType = data.displayType;
				portType = data.displayType;
				visualClass = "Port_" + portType.Name;
			}
			if (!String.IsNullOrEmpty(data.displayName))
				base.portName = data.displayName;

			portData = data;

			// Update the edge in case the port color have changed
			schedule.Execute(() => {
				foreach (EdgeView edge in edges)
				{
					edge.UpdateEdgeControl();
					edge.MarkDirtyRepaint();
				}
			}).ExecuteLater(50); // Hummm

			UpdatePortSize();
		}

		public List< EdgeView >	GetEdges()
		{
			return edges;
		}
	}
}