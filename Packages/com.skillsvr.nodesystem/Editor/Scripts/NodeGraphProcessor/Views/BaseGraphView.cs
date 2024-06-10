using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;
using UnityEditor.SceneManagement;
using System.Reflection;
using SkillsVRNodes.Scripts.Nodes;
using Status = UnityEngine.UIElements.DropdownMenuAction.Status;
using Object = UnityEngine.Object;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes;
using Unity.EditorCoroutines.Editor;
using SkillsVRNodes.Editor.NodeViews;

namespace GraphProcessor
{
	/// <summary>
	/// Base class to write a custom view for a node
	/// </summary>
	public class BaseGraphView : GraphView, IDisposable
	{
		public delegate void ComputeOrderUpdatedDelegate();
		public delegate void NodeDuplicatedDelegate(BaseNode duplicatedNode, BaseNode newNode);

		/// <summary>
		/// Graph that owns of the node
		/// </summary>
		public BaseGraph graph;

		/// <summary>
		/// Connector listener that will create the edges between ports
		/// </summary>
		public BaseEdgeConnectorListener connectorListener;

		/// <summary>
		/// List of all node views in the graph
		/// </summary>
		/// <typeparam name="BaseNodeView"></typeparam>
		/// <returns></returns>
		public List<BaseNodeView> nodeViews = new List<BaseNodeView>();

		/// <summary>
		/// Dictionary of the node views accessed view the node instance, faster than a Find in the node view list
		/// </summary>
		/// <typeparam name="BaseNode"></typeparam>
		/// <typeparam name="BaseNodeView"></typeparam>
		/// <returns></returns>
		/// 
		public IReadOnlyDictionary<BaseNode, BaseNodeView> nodeViewsPerNode => ManagedNodeViewsPerNode;
		protected Dictionary<BaseNode, BaseNodeView> ManagedNodeViewsPerNode { get; } = new Dictionary<BaseNode, BaseNodeView>();

		public IReadOnlyDictionary<string, BaseNodeView> nodeViewsPerGUID => ManagedNodeViewsPerGUID;
		protected Dictionary<string, BaseNodeView> ManagedNodeViewsPerGUID { get; } = new Dictionary<string, BaseNodeView>();

		/// <summary>
		/// List of all edge views in the graph
		/// </summary>
		/// <typeparam name="EdgeView"></typeparam>
		/// <returns></returns>
		public List<EdgeView> edgeViews = new List<EdgeView>();

		/// <summary>
		/// List of all group views in the graph
		/// </summary>
		/// <typeparam name="GroupView"></typeparam>
		/// <returns></returns>
		public List<GroupView> groupViews = new List<GroupView>();

#if UNITY_2020_1_OR_NEWER
		/// <summary>
		/// List of all sticky note views in the graph
		/// </summary>
		/// <typeparam name="StickyNoteView"></typeparam>
		/// <returns></returns>
		public List<StickyNoteView> stickyNoteViews = new List<StickyNoteView>();
#endif

		/// <summary>
		/// List of all stack node views in the graph
		/// </summary>
		/// <typeparam name="BaseStackNodeView"></typeparam>
		/// <returns></returns>
		public List<BaseStackNodeView> stackNodeViews = new List<BaseStackNodeView>();

		Dictionary<Type, PinnedElementView> pinnedElements = new Dictionary<Type, PinnedElementView>();

		CreateNodeMenuWindow createNodeMenu;

		/// <summary>
		/// Triggered just after the graph is initialized
		/// </summary>
		public event Action initialized;

		/// <summary>
		/// Triggered just after the compute order of the graph is updated
		/// </summary>
		public event ComputeOrderUpdatedDelegate computeOrderUpdated;

		// Safe event relay from BaseGraph (safe because you are sure to always point on a valid BaseGraph
		// when one of these events is called), a graph switch can occur between two call tho
		/// <summary>
		/// Same event than BaseGraph.onExposedParameterListChanged
		/// Safe event (not triggered in case the graph is null).
		/// </summary>
		public event Action onExposedParameterListChanged;

		/// <summary>
		/// Same event than BaseGraph.onExposedParameterModified
		/// Safe event (not triggered in case the graph is null).
		/// </summary>
		public event Action<ExposedParameter> onExposedParameterModified;

		/// <summary>
		/// Triggered when a node is duplicated (crt-d) or copy-pasted (crtl-c/crtl-v)
		/// </summary>
		public event NodeDuplicatedDelegate nodeDuplicated;

		public event Action<BaseNodeView> onNodeViewAdded;

		public bool Interactable { get; private set; } = true;

		/// <summary>
		/// Object to handle nodes that shows their UI in the inspector.
		/// </summary>
		[SerializeField]
		protected NodeInspectorObject nodeInspector
		{
			get
			{
				if (!graph)
					return null;

				if (graph.nodeInspectorReference == null)
					graph.nodeInspectorReference = CreateNodeInspectorObject();
				return graph.nodeInspectorReference as NodeInspectorObject;
			}
		}

		/// <summary>
		/// Workaround object for creating exposed parameter property fields.
		/// </summary>
		public ExposedParameterFieldFactory exposedParameterFactory { get; private set; }

		public SerializedObject serializedGraph { get; private set; }

		public Dictionary<Type, (Type nodeType, MethodInfo initalizeNodeFromObject)> NodeTypePerCreateAssetType { get => nodeTypePerCreateAssetType; }
		Dictionary<Type, (Type nodeType, MethodInfo initalizeNodeFromObject)> nodeTypePerCreateAssetType = new Dictionary<Type, (Type, MethodInfo)>();

		public BaseGraphWindow parentEditorWindow { get; protected set; }

		public IPlayModeEventListener PlayModeEventListener { get; private set; } = new SmartPlayModeEventHandler();

		public event Action onDispose;

		public BaseGraphView(EditorWindow window)
		{
			serializeGraphElements = SerializeGraphElementsCallback;
			canPasteSerializedData = CanPasteSerializedDataCallback;
			unserializeAndPaste = UnserializeAndPasteCallback;
			graphViewChanged = GraphViewChangedCallback;
			viewTransformChanged = ViewTransformChangedCallback;
			elementResized = ElementResizedCallback;
			nodeCreationRequest = OnNodeCreationRequest;

			GridBackground gridBackground = new GridBackground();
			gridBackground.StretchToParentSize();
			Insert(0, gridBackground);

			RegisterCallback<KeyDownEvent>(KeyDownCallback);
			RegisterCallback<DragPerformEvent>(DragPerformedCallback);
			RegisterCallback<DragUpdatedEvent>(DragUpdatedCallback);
			RegisterCallback<MouseDownEvent>(MouseDownCallback);
			RegisterCallback<MouseUpEvent>(MouseUpCallback);

			InitializeManipulators();

			SetupZoom(0.05f, 2f);

			Undo.undoRedoPerformed += ReloadView;

			createNodeMenu = ScriptableObject.CreateInstance<CreateNodeMenuWindow>();
			createNodeMenu.Initialize(this, window);

			this.StretchToParentSize();
			parentEditorWindow = window as BaseGraphWindow;

			RegisterSaveGraphEvents();

			connectorListener = CreateEdgeConnectorListener();
		}

		/// <summary>
		/// Call this function when you want to remove this view
		/// </summary>
		public virtual void Dispose()
		{
			try
			{
				onDispose?.Invoke();
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}
			PlayModeEventListener?.Clear();
			PlayModeEventListener = null;

			parentEditorWindow = null;

			createNodeMenu?.Dispose();
			createNodeMenu = null;

			UnregisterSaveGraphEvent();
			RemoveFromHierarchy();
			Undo.undoRedoPerformed -= ReloadView;
			Object.DestroyImmediate(nodeInspector);

			DetachGroupsBeforeDispose();
			RemoveNodeViews();

			DeinitGraph();

			exposedParameterFactory?.Dispose();
			exposedParameterFactory = null;

			connectorListener?.Dispose();
			connectorListener = null;
		}
		
		public void DetachGroupsBeforeDispose()
		{
			foreach (var groupView in groupViews)
			{
				groupView.group = null;
			}
		}

		public void OpenGraph(BaseGraph baseGraph)
		{
			parentEditorWindow?.LoadGraph(baseGraph);

		}
		protected virtual NodeInspectorObject CreateNodeInspectorObject()
		{
			var inspector = ScriptableObject.CreateInstance<NodeInspectorObject>();
			inspector.name = "Node Inspector";
			inspector.hideFlags = HideFlags.HideAndDontSave ^ HideFlags.NotEditable;

			return inspector;
		}

		#region Callbacks

		protected override bool canCopySelection
		{
			get { return selection.Any(e => e is BaseNodeView || e is GroupView); }
		}

		protected override bool canCutSelection
		{
			get { return false; } //selection.Any(e => e is BaseNodeView || e is GroupView); }
		}


		string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
		{
			var data = new CopyPasteHelper();

			IEnumerable<GraphElement> enumerable = elements as GraphElement[] ?? elements.ToArray();
			foreach (GraphElement graphElement in enumerable.Where(e => e is BaseNodeView))
			{
				BaseNodeView nodeView = (BaseNodeView)graphElement;
				data.copiedNodes.Add(JsonSerializer.SerializeNode(nodeView.nodeTarget));
				foreach (var port in nodeView.nodeTarget.GetAllPorts())
				{
					if (port.portData.vertical)
					{
						foreach (var edge in port.GetEdges())
							data.copiedEdges.Add(JsonSerializer.Serialize(edge));
					}
				}
			}

			foreach (GraphElement graphElement in enumerable.Where(e => e is GroupView))
			{
				if (graphElement is LogicGroupView logicGroupView)
				{
					data.copiedLogicGroup.Add(JsonSerializer.Serialize(logicGroupView.GetLogicGroup));
				}
				else
				{
					GroupView groupView = (GroupView)graphElement;
					data.copiedGroups.Add(JsonSerializer.Serialize(groupView.group));
				}
			}

			foreach (GraphElement graphElement in enumerable.Where(e => e is EdgeView))
			{
				EdgeView edgeView = (EdgeView)graphElement;
				data.copiedEdges.Add(JsonSerializer.Serialize(edgeView.serializedEdge));
			}

			ClearSelection();

			return JsonUtility.ToJson(data, true);
		}

		bool CanPasteSerializedDataCallback(string serializedData)
		{
			if (!Interactable)
			{
				return false;
			}
			try {
				return JsonUtility.FromJson(serializedData, typeof(CopyPasteHelper)) != null;
			} catch {
				return false;
			}
		}



		void UnserializeAndPasteCallback(string operationName, string serializedData)
		{
			if (!Interactable)
			{
				return;
			}
			CopyPasteHelper data = JsonUtility.FromJson<CopyPasteHelper>(serializedData);

			RegisterCompleteObjectUndo(operationName);

			Dictionary<string, BaseNode> copiedNodesMap = new();

			List<Group> deserializedGroups = data.copiedGroups.Select(JsonSerializer.Deserialize<Group>).ToList();
			List<LogicGroup> deserializedLogicGroups = data.copiedLogicGroup.Select(JsonSerializer.Deserialize<LogicGroup>).ToList();

			// Nodes
			foreach (JsonElement serializedNode in data.copiedNodes)
			{
				BaseNode node = JsonSerializer.DeserializeNode(serializedNode);

				if (node is null or ExitGroupNode or EnterGroupNode)
				{
					continue;
				}

				string sourceGUID = node.GUID;
				graph.nodesPerGUID.TryGetValue(sourceGUID, out BaseNode sourceNode);
				//Call OnNodeCreated on the new fresh copied node
				node.createdFromDuplication = true;
				node.createdWithinGroup = deserializedGroups.Any(g => g.innerNodeGUIDs.Contains(sourceGUID));
				node.createdWithinGroup = deserializedLogicGroups.Any(g => g.innerNodeGUIDs.Contains(sourceGUID));
				node.OnNodeCreated();
				//And move a bit the new node
				node.position.position += new Vector2(20, 20);


				AddNode(node);

				// If the nodes were copied from another graph, then the source is null
				if (sourceNode != null)
				{
					nodeDuplicated?.Invoke(sourceNode, node);
				}

				copiedNodesMap[sourceGUID] = node;

				//Select the new node
				AddToSelection(nodeViewsPerNode[node]);
			}

			// Groups
			foreach (Group group in deserializedGroups)
			{
				// Same than for node
				group.OnCreated();

				// try to centre the created node in the screen
				group.position.position += new Vector2(20, 20);

				List<string> oldGUIDList = group.innerNodeGUIDs.ToList();
				group.innerNodeGUIDs.Clear();
				foreach (string guid in oldGUIDList)
				{
					graph.nodesPerGUID.TryGetValue(guid, out BaseNode node);

					// In case group was copied from another graph
					if (node == null)
					{
						copiedNodesMap.TryGetValue(guid, out node);
						group.innerNodeGUIDs.Add(node?.GUID);
					}
					else
					{
						group.innerNodeGUIDs.Add(copiedNodesMap[guid].GUID);
					}
				}

				AddGroup(group);
			}

			// LogicGroups
			foreach (LogicGroup group in deserializedLogicGroups)
			{
				//make a new logic group to ensure our custom initailisation
				LogicGroup newgroup = new LogicGroup();

				newgroup.OnCreated();
				// try to centre the created node in the screen
				newgroup.position.position += new Vector2(20, 20);

				//all edges in old group's enter
				foreach (var nodePort in group.GetEnterGroupNode(graph).GetAllPorts())
				{
					if (nodePort.fieldName == "executed")
					{
						foreach (SerializableEdge edge in nodePort.GetEdges())
						{
                            newgroup.entryOnCopy.nodePortsInput.Add(edge.outputPort);
						}
					}

					if (nodePort.fieldName == "Complete")
					{
						//these need to be picked from the appropriate copdied nodes, as they are inside our new group
						foreach (SerializableEdge edge in nodePort.GetEdges())
						{                          
							graph.nodesPerGUID.TryGetValue(copiedNodesMap[edge.inputNode.GUID].GUID, out BaseNode node);                      
							newgroup.entryOnCopy.nodePortsOutput.Add(node.GetPort(edge.inputPort.fieldName, edge.inputPort.portData.identifier));

                        }
					}
				}
				foreach (var nodePort in group.GetExitGroupNode(graph).GetAllPorts())
				{
					if (nodePort.fieldName == "Complete")
					{
						foreach (SerializableEdge edge in nodePort.GetEdges())
						{
							newgroup.exitOnCopy.nodePortsOutput.Add(edge.inputPort);
						}
					}

					if (nodePort.fieldName == "executed")
					{
						//these need to be picked from the appropriate copdied nodes, as they are inside our new group
						foreach (SerializableEdge edge in nodePort.GetEdges())
						{					
                            graph.nodesPerGUID.TryGetValue(copiedNodesMap[edge.outputNode.GUID].GUID, out BaseNode node);
                            newgroup.exitOnCopy.nodePortsInput.Add(node.GetPort(edge.outputPort.fieldName, edge.outputPort.portData.identifier));
                        }
					}
				}

				List<string> oldGUIDList = group.innerNodeGUIDs.ToList();


				newgroup.innerNodeGUIDs.Clear();
				foreach (string guid in oldGUIDList)
				{
					graph.nodesPerGUID.TryGetValue(guid, out BaseNode node);

					// In case group was copied from another graph
					if (node == null)
					{
						copiedNodesMap.TryGetValue(guid, out node);
						newgroup.innerNodeGUIDs.Add(node?.GUID);
					}
					else
					{
						newgroup.innerNodeGUIDs.Add(copiedNodesMap[guid].GUID);
					}
				}
				newgroup.title = group.title;
				AddGroup(newgroup);
			}

			//Just Copied One Node - Dont Want Edges
			if (data.copiedNodes.Count == 1)
			{
                if (deserializedLogicGroups.Count > 0)
                {
                    Refresh();
                }
                return;
			}
			// Edges
			foreach (SerializableEdge edge in data.copiedEdges.Select(JsonSerializer.Deserialize<SerializableEdge>))
			{

				edge.Deserialize();

				// Find port of new nodes:
				copiedNodesMap.TryGetValue(edge.inputNode.GUID, out var oldInputNode);
				copiedNodesMap.TryGetValue(edge.outputNode.GUID, out var oldOutputNode);

				// We avoid to break the graph by replacing unique connections:
				if (oldInputNode == null && !edge.inputPort.portData.acceptMultipleEdges || !edge.outputPort.portData.acceptMultipleEdges)
					continue;


				oldInputNode ??= edge.inputNode;
				oldOutputNode ??= edge.outputNode;

				////don't copy edges to enter / exit of logic groups or nulls
				if (oldInputNode is EnterGroupNode or ExitGroupNode || oldOutputNode is EnterGroupNode or ExitGroupNode)
				{
					continue;
				}


				NodePort inputPort = oldInputNode.GetPort(edge.inputPort.fieldName, edge.inputPortIdentifier);
				NodePort outputPort = oldOutputNode.GetPort(edge.outputPort.fieldName, edge.outputPortIdentifier);

				SerializableEdge newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);

				if (nodeViewsPerNode.ContainsKey(oldInputNode) && nodeViewsPerNode.ContainsKey(oldOutputNode))
				{
					EdgeView edgeView = CreateEdgeView();
					edgeView.userData = newEdge;
					edgeView.input = nodeViewsPerNode[oldInputNode].GetPortViewFromFieldName(newEdge.inputFieldName, newEdge.inputPortIdentifier);
					edgeView.output = nodeViewsPerNode[oldOutputNode].GetPortViewFromFieldName(newEdge.outputFieldName, newEdge.outputPortIdentifier);

					Connect(edgeView);
				}
			}

			if (deserializedLogicGroups.Count > 0)
			{
				Refresh();
			}
		}

		public virtual EdgeView CreateEdgeView()
		{
			return new EdgeView();
		}



		GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
		{
			if (changes.elementsToRemove != null)
			{
				RegisterCompleteObjectUndo("Remove Graph Elements");

				// Destroy priority of objects
				// We need nodes to be destroyed first because we can have a destroy operation that uses node connections
				changes.elementsToRemove.Sort((e1, e2) => {
					int GetPriority(GraphElement e)
					{
						if (e is BaseNodeView)
							return 0;
						else
							return 1;
					}
					return GetPriority(e1).CompareTo(GetPriority(e2));
				});

				//Handle ourselves the edge and node remove
				changes.elementsToRemove.RemoveAll(e => {

					switch (e)
					{
						case EdgeView edge:
							Disconnect(edge);
							return true;
						case BaseNodeView nodeView:
							// For vertical nodes, we need to delete them ourselves as it's not handled by GraphView
							foreach (var pv in nodeView.inputPortViews.Concat(nodeView.outputPortViews))
								if (pv.orientation == Orientation.Vertical)
									foreach (var edge in pv.GetEdges().ToList())
										Disconnect(edge);

							nodeInspector.NodeViewRemoved(nodeView);
							ExceptionToLog.Call(() => nodeView.OnRemoved());
							graph.RemoveNode(nodeView.nodeTarget);
							UpdateSerializedProperties();
							RemoveElement(nodeView);
							if (Selection.activeObject == nodeInspector)
								UpdateNodeInspectorSelection();

							SyncSerializedPropertyPathes();
							nodeView.Dispose();
							return true;
						case GroupView group:
							graph.RemoveGroup(group.group);
							UpdateSerializedProperties();
							RemoveElement(group);
							return true;
						case ExposedParameterFieldView blackboardField:
							graph.RemoveExposedParameter(blackboardField.parameter);
							UpdateSerializedProperties();
							return true;
						case BaseStackNodeView stackNodeView:
							graph.RemoveStackNode(stackNodeView.stackNode);
							UpdateSerializedProperties();
							RemoveElement(stackNodeView);
							return true;
#if UNITY_2020_1_OR_NEWER
						case StickyNoteView stickyNoteView:
							graph.RemoveStickyNote(stickyNoteView.note);
							UpdateSerializedProperties();
							RemoveElement(stickyNoteView);
							return true;
#endif
					}

					return false;
				});
			}

			return changes;
		}

        /// <summary>
        /// Override graph view RemoveElement in case to dispose the graph element except base node view type.
        /// The base node view will be disposed either in RemoveNodeView(s) or SyncSerializedPropertyPathes.
        /// </summary>
        /// <param name="graphElement"></param>
        public new void RemoveElement(GraphElement graphElement)
		{
			base.RemoveElement(graphElement);

			if (graphElement is BaseNodeView)
			{
                // There are some cases that require node view alive after remove from graph,
                // in order to process more custom actions with that node view.
                // So don't dispose node view here.
                // The node view dispose will be handled individually at 
                //    - RemoveNodeView()
                //    - RemoveNodeViews()
                //    - Delete node view in GraphViewChangedCallback()
                return;
			}
			(graphElement as IDisposable)?.Dispose();
		}


        void GraphChangesCallback(GraphChanges changes)
		{
			if (changes.removedEdge != null)
			{
				var edge = edgeViews.FirstOrDefault(e => e.serializedEdge == changes.removedEdge);

				DisconnectView(edge);
			}
		}

		void ViewTransformChangedCallback(GraphView view)
		{
			if (graph != null)
			{
				graph.position = viewTransform.position;
				graph.scale = viewTransform.scale;
			}
		}

		void ElementResizedCallback(VisualElement elem)
		{
			var groupView = elem as GroupView;

			if (groupView != null)
				groupView.group.size = groupView.GetPosition().size;
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			var compatiblePorts = new List<Port>();

			compatiblePorts.AddRange(ports.ToList().Where(p => {
				var portView = p as PortView;

				if (portView.owner == (startPort as PortView).owner)
					return false;

				if (p.direction == startPort.direction)
					return false;

				//Check for type assignability
				if (!BaseGraph.TypesAreConnectable(startPort.portType, p.portType))
					return false;

				//Check if the edge already exists
				if (portView.GetEdges().Any(e => e.input == startPort || e.output == startPort))
					return false;

				return true;
			}));

			return compatiblePorts;
		}

		/// <summary>
		/// Build the contextual menu shown when right clicking inside the graph view
		/// </summary>
		/// <param name="evt"></param>
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			if (graph == null)
				return;

			base.BuildContextualMenu(evt);
			BuildGroupContextualMenu(evt, 1);

			if(graph.GetType() != typeof(MainGraph))
				BuildLogicGroupContextualMenu(evt, 2);
			
			BuildStickyNoteContextualMenu(evt, 3);
			BuildViewContextualMenu(evt);
			//BuildSelectAssetContextualMenu(evt);
			//BuildSaveAssetContextualMenu(evt);
			//BuildHelpContextualMenu(evt);
		}

		/// <summary>
		/// Add the New Group entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected virtual void BuildGroupContextualMenu(ContextualMenuPopulateEvent evt, int menuPosition = -1)
		{
			if (menuPosition == -1)
				menuPosition = evt.menu.MenuItems().Count;
			Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
			evt.menu.InsertAction(menuPosition, "Create Group", (e) => AddSelectionsToGroup(AddGroup(new Group("Create Group", position))), DropdownMenuAction.AlwaysEnabled);
		}

		/// <summary>
		/// Add the Logic New Group entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected virtual void BuildLogicGroupContextualMenu(ContextualMenuPopulateEvent evt, int menuPosition = -1)
		{
			if (menuPosition == -1)
				menuPosition = evt.menu.MenuItems().Count;
			Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
			evt.menu.InsertAction(menuPosition, "Create Logic Group", (e) => {
				LogicGroupView lg = AddGroup(new LogicGroup("Created Logic Group", position));
				//lg.GetLogicGroup.Init(graph);
                AddSelectionsToGroup(lg);
			}, DropdownMenuAction.AlwaysEnabled);
		}



		/// <summary>
		/// -Add the New Sticky Note entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected virtual void BuildStickyNoteContextualMenu(ContextualMenuPopulateEvent evt, int menuPosition = -1)
		{
			if (menuPosition == -1)
				menuPosition = evt.menu.MenuItems().Count;
#if UNITY_2020_1_OR_NEWER
			Vector2 position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
			evt.menu.InsertAction(menuPosition, "Create Sticky Note", (e) => AddStickyNote(new StickyNote("Create Note", position)), DropdownMenuAction.AlwaysEnabled);
#endif
		}

		/// <summary>
		/// Add the View entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected virtual void BuildViewContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("View/Processor", (e) => ToggleView<ProcessorView>(), (e) => GetPinnedElementStatus<ProcessorView>());
		}

		/// <summary>
		/// Add the Select Asset entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected virtual void BuildSelectAssetContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Select Asset", (e) => EditorGUIUtility.PingObject(graph), DropdownMenuAction.AlwaysEnabled);
		}

		/// <summary>
		/// Add the Save Asset entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected virtual void BuildSaveAssetContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Save Asset", (e) => {
				EditorUtility.SetDirty(graph);
				AssetDatabase.SaveAssets();
			}, DropdownMenuAction.AlwaysEnabled);
		}

		/// <summary>
		/// Add the Help entry to the context menu
		/// </summary>
		/// <param name="evt"></param>
		protected void BuildHelpContextualMenu(ContextualMenuPopulateEvent evt)
		{
			evt.menu.AppendAction("Help/Reset Pinned Windows", e => {
				foreach (var kp in pinnedElements)
					kp.Value.ResetPosition();
			});
		}

		protected virtual void KeyDownCallback(KeyDownEvent e)
		{
			if (nodeViews.Count > 0 && e.actionKey && e.altKey)
			{
				//	Node Aligning shortcuts
				switch (e.keyCode)
				{
					case KeyCode.LeftArrow:
						nodeViews[0].AlignToLeft();
						e.StopPropagation();
						break;
					case KeyCode.RightArrow:
						nodeViews[0].AlignToRight();
						e.StopPropagation();
						break;
					case KeyCode.UpArrow:
						nodeViews[0].AlignToTop();
						e.StopPropagation();
						break;
					case KeyCode.DownArrow:
						nodeViews[0].AlignToBottom();
						e.StopPropagation();
						break;
					case KeyCode.C:
						nodeViews[0].AlignToCenter();
						e.StopPropagation();
						break;
					case KeyCode.M:
						nodeViews[0].AlignToMiddle();
						e.StopPropagation();
						break;
				}
			}
		}

		void MouseUpCallback(MouseUpEvent e)
		{
			schedule.Execute(() => {
				if (DoesSelectionContainsInspectorNodes())
					UpdateNodeInspectorSelection();
			}).ExecuteLater(1);
		}

		void MouseDownCallback(MouseDownEvent e)
		{
			// When left clicking on the graph (not a node or something else)
			if (e.button == 0)
			{
				// Close all settings windows:
				nodeViews.ForEach(v => v.CloseSettings());
			}

			if (DoesSelectionContainsInspectorNodes())
				UpdateNodeInspectorSelection();
		}

		bool DoesSelectionContainsInspectorNodes()
		{
			if (graph == null)
			{
				return false;
			}
			var selectedNodes = selection.Where(s => s is BaseNodeView).ToList();
			var selectedNodesNotInInspector = selectedNodes.Except(nodeInspector.selectedNodes).ToList();
			var nodeInInspectorWithoutSelectedNodes = nodeInspector.selectedNodes.Except(selectedNodes).ToList();

			return selectedNodesNotInInspector.Any() || nodeInInspectorWithoutSelectedNodes.Any();
		}

		void DragPerformedCallback(DragPerformEvent e)
		{
			var mousePos = (e.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, e.localMousePosition);
			var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;

			// Drag and Drop for elements inside the graph
			if (dragData != null)
			{
				var exposedParameterFieldViews = dragData.OfType<ExposedParameterFieldView>();
				if (exposedParameterFieldViews.Any())
				{
					foreach (var paramFieldView in exposedParameterFieldViews)
					{
						RegisterCompleteObjectUndo("Create Parameter Node");
						var paramNode = BaseNode.CreateFromType<ParameterNode>(mousePos);
						paramNode.parameterGUID = paramFieldView.parameter.guid;
						AddNode(paramNode);
					}
				}
			}

			// External objects drag and drop
			if (DragAndDrop.objectReferences.Length > 0)
			{
				RegisterCompleteObjectUndo("Create Node From Object(s)");
				foreach (var obj in DragAndDrop.objectReferences)
				{
					var objectType = obj.GetType();

					foreach (var kp in nodeTypePerCreateAssetType)
					{
						if (kp.Key.IsAssignableFrom(objectType))
						{
							try
							{
								var node = BaseNode.CreateFromType(kp.Value.nodeType, mousePos);
								if ((bool)kp.Value.initalizeNodeFromObject.Invoke(node, new[] { obj }))
								{
									AddNode(node);
									break;
								}
							}
							catch (Exception exception)
							{
								Debug.LogException(exception);
							}
						}
					}
				}
			}
		}

		void DragUpdatedCallback(DragUpdatedEvent e)
		{
			var dragData = DragAndDrop.GetGenericData("DragSelection") as List<ISelectable>;
			var dragObjects = DragAndDrop.objectReferences;
			bool dragging = false;

			if (dragData != null)
			{
				// Handle drag from exposed parameter view
				if (dragData.OfType<ExposedParameterFieldView>().Any())
				{
					dragging = true;
				}
			}

			if (dragObjects.Length > 0)
				dragging = true;

			if (dragging)
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

			UpdateNodeInspectorSelection();
		}

		#endregion

		#region Initialization

		void ReloadView()
		{
			// Force the graph to reload his data (Undo have updated the serialized properties of the graph
			// so the one that are not serialized need to be synchronized)
			graph.Deserialize();

			// Get selected nodes
			var selectedNodeGUIDs = new List<string>();
			foreach (var e in selection)
			{
				if (e is BaseNodeView v && this.Contains(v))
					selectedNodeGUIDs.Add(v.nodeTarget.GUID);
			}

			ClearGraphView();

			UpdateSerializedProperties();
			UpdateComputeOrder();

			BuildGraphView();

			OnReload();


			// Restore selection after re-creating all views
			// selection = nodeViews.Where(v => selectedNodeGUIDs.Contains(v.nodeTarget.GUID)).Select(v => v as ISelectable).ToList();
			foreach (var guid in selectedNodeGUIDs)
			{
				AddToSelection(nodeViews.FirstOrDefault(n => n.nodeTarget.GUID == guid));
			}

			UpdateNodeInspectorSelection();
		}

		protected virtual void InitGraph()
		{
			if (null == graph)
			{
				return;
			}
			graph.onExposedParameterListChanged += OnExposedParameterListChanged;
			graph.onExposedParameterModified += OnGraphExposedParameterModified;
			graph.onGraphChanges += GraphChangesCallback;

			exposedParameterFactory = new ExposedParameterFieldFactory(graph);
			NodeProvider.LoadGraph(graph);

			graph.Deserialize();

			graph?.BuildGraphElements();

			UpdateSerializedProperties();

			UpdateComputeOrder();

			RegistetCreateAssetTypeFromGraphNodes();
		}

		protected virtual void DeinitGraph()
		{
			exposedParameterFactory?.Dispose();
			exposedParameterFactory = null;

			if (null == this.graph)
			{
				return;
			}
			graph.onExposedParameterListChanged -= OnExposedParameterListChanged;
			graph.onExposedParameterModified -= OnGraphExposedParameterModified;
			graph.onGraphChanges -= GraphChangesCallback;

			NodeProvider.UnloadGraph(graph);

			UnregistetCreateAssetTypeFromGraphNodes();
		}

		protected virtual void BuildGraphView()
		{
			if (null == graph)
			{
				return;
			}
			CalcuateTotalElementCount();
			InitializeGraphViewTransform();
			InitializeNodeViews();
			InitializeEdgeViews();
			InitializeGroups();
			InitializePinnedViews();
			InitializeStickyNotes();
			InitializeStackNodes();
		}

		protected void CalcuateTotalElementCount()
		{
			if (null == graph)
			{
				TotalElementCount = 0;
				return;
			}
			TotalElementCount =
				graph.nodes.Count
				+ graph.edges.Count
				+ graph.groups.Count
				+ graph.stackNodes.Count
				+ graph.stickyNotes.Count;
		}

		protected virtual IEnumerator BuildGraphViewRoutine()
		{
			if (null == graph)
			{
				yield break;
			}
			CalcuateTotalElementCount();
			InitializeGraphViewTransform();
			yield return InitializeNodeViewsRoutine();
			yield return InitializeEdgeViewsRoutine();
			InitializeGroups();
			InitializePinnedViews();
			InitializeStickyNotes();
			InitializeStackNodes();
		}

		public int TotalElementCount { get; protected set; }
		public int LoadedElementCount { get; protected set; }

		protected virtual void ClearGraphView()
		{
			RemoveGroups();
			RemoveNodeViews();
			RemoveEdges();
			RemoveStackNodeViews();
			RemovePinnedElementViews();
#if UNITY_2020_1_OR_NEWER
			RemoveStrickyNotes();
#endif
		}

		public virtual void UnloadGraph()
		{
			SaveGraphToDisk();
			DeinitGraph();
		}
		public virtual void LoadGraph(BaseGraph graphToLoad)
		{
			UnloadGraph();
			ClearGraphView();
			this.graph = graphToLoad;

			if (null == graph)
			{
				return;
			}

			InitGraph();
			BuildGraphView();

			initialized?.Invoke();
			OnInitializeView();
		}

		public virtual IEnumerator LoadGraphRoutine(BaseGraph graphToLoad)
		{
			UnloadGraph();
			ClearGraphView();
			this.graph = graphToLoad;

			if (null == graph)
			{
				yield break;
			}

			InitGraph();
			yield return BuildGraphViewRoutine();

			initialized?.Invoke();
			OnInitializeView();
		}

		private void RegistetCreateAssetTypeFromGraphNodes()
		{
			// Register the nodes that can be created from assets
			foreach (var nodeInfo in NodeProvider.GetNodeMenuEntries(graph))
			{
				var interfaces = nodeInfo.type.GetInterfaces();
				var exceptInheritedInterfaces = interfaces.Except(interfaces.SelectMany(t => t.GetInterfaces()));
				foreach (var i in interfaces)
				{
					if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICreateNodeFrom<>))
					{
						var genericArgumentType = i.GetGenericArguments()[0];
						var initializeFunction = nodeInfo.type.GetMethod(
							nameof(ICreateNodeFrom<Object>.InitializeNodeFromObject),
							BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
							null, new Type[] { genericArgumentType }, null
						);

						// We only add the type that implements the interface, not it's children
						if (initializeFunction.DeclaringType == nodeInfo.type)
							nodeTypePerCreateAssetType[genericArgumentType] = (nodeInfo.type, initializeFunction);
					}
				}
			}
		}

		private void UnregistetCreateAssetTypeFromGraphNodes()
		{
			nodeTypePerCreateAssetType?.Clear();
		}

		private void OnGraphExposedParameterModified(ExposedParameter exposedParameter)
		{
			onExposedParameterModified?.Invoke(exposedParameter);
		}


		private void RegisterSaveGraphEvents()
		{
			// When pressing ctrl-s, we save the graph
			EditorSceneManager.sceneSaved += OnSceneSaved;
			RegisterCallback<KeyDownEvent>(CheckSaveOnKeyDown);
		}

		private void UnregisterSaveGraphEvent()
		{
			EditorSceneManager.sceneSaved -= OnSceneSaved;
			UnregisterCallback<KeyDownEvent>(CheckSaveOnKeyDown);
		}

		private void CheckSaveOnKeyDown(KeyDownEvent e)
		{
			if (e.keyCode == KeyCode.S && e.actionKey)
			{
				SaveGraphToDisk();
			}
		}

		private void OnSceneSaved(UnityEngine.SceneManagement.Scene savedScene)
		{
			SaveGraphToDisk();
		}


		void UpdateSerializedProperties()
		{
			serializedGraph = new SerializedObject(graph);
		}

		private void OnNodeCreationRequest(NodeCreationContext c)
		{
			if (!Interactable)
			{
				return; 
			}
			SearchWindow.Open(new SearchWindowContext(c.screenMousePosition), createNodeMenu);
		}

		/// <summary>
		/// Allow you to create your own edge connector listener
		/// </summary>
		/// <returns></returns>
		protected virtual BaseEdgeConnectorListener CreateEdgeConnectorListener()
		 => new BaseEdgeConnectorListener(this);

		private void InitializeGraphViewTransform()
		{
			viewTransform.position = graph.position;
			viewTransform.scale = graph.scale;
		}

		private void OnExposedParameterListChanged()
		{
			UpdateSerializedProperties();
			onExposedParameterListChanged?.Invoke();
		}

		private void InitializeNodeViews()
		{
			graph.Nodes.RemoveAll(n => n == null);

			foreach (var node in graph.Nodes)
			{
				AddNodeView(node);
			}
			LoadedElementCount += graph.Nodes.Count;
		}

		private IEnumerator InitializeNodeViewsRoutine(float maxLoadingTimePerFrame = 0.016f)
		{
			graph.Nodes.RemoveAll(n => n == null);
			yield return null;

			maxLoadingTimePerFrame = Mathf.Max(maxLoadingTimePerFrame, 0.0001f);
			float startTime = Time.realtimeSinceStartup;

			var orderedList = graph.Nodes.OrderBy(x => x.position.x);
			foreach (var node in orderedList)
			{
				var nodeView = AddNodeView(node);
				nodeView.SetInteractable(this.Interactable);
				++LoadedElementCount;
				if (Time.realtimeSinceStartup - startTime > maxLoadingTimePerFrame)
				{
					yield return null;
					startTime = Time.realtimeSinceStartup;
				}
			}

			InitializeGroups();
		}

		private void InitializeEdgeViews()
		{
			// Sanitize edges in case a node broke something while loading
			//graph.edges.RemoveAll(edge => edge == null || edge.inputNode == null || edge.outputNode == null);

			foreach (var serializedEdge in graph.edges)
			{
				BuildSerializedEdge(serializedEdge);
			}
			LoadedElementCount += graph.edges.Count;
		}

		private IEnumerator InitializeEdgeViewsRoutine(float maxLoadingTimePerFrame = 0.016f)
		{
			// Sanitize edges in case a node broke something while loading
			//graph.edges.RemoveAll(edge => edge == null || edge.inputNode == null || edge.outputNode == null);
			maxLoadingTimePerFrame = Mathf.Max(maxLoadingTimePerFrame, 0.0001f);
			float startTime = Time.realtimeSinceStartup;
			foreach (var serializedEdge in graph.edges)
			{
				BuildSerializedEdge(serializedEdge);
				++LoadedElementCount;
				if (Time.realtimeSinceStartup - startTime > maxLoadingTimePerFrame)
				{
					yield return null;
					startTime = Time.realtimeSinceStartup;
				}
			}
			
			InitializePinnedViews();
			InitializeStickyNotes();
			InitializeStackNodes();
		}

		private void BuildSerializedEdge(SerializableEdge serializedEdge)
		{
			// not deleting any line but allowing to continue and not have errors
			if (serializedEdge.inputNode == null || serializedEdge.outputNode == null)
				return;

			nodeViewsPerNode.TryGetValue(serializedEdge.inputNode, out var inputNodeView);
			nodeViewsPerNode.TryGetValue(serializedEdge.outputNode, out var outputNodeView);
			if (inputNodeView == null || outputNodeView == null)
				return;

			var edgeView = CreateEdgeView();
			edgeView.userData = serializedEdge;
			edgeView.input = inputNodeView.GetPortViewFromFieldName(serializedEdge.inputFieldName, serializedEdge.inputPortIdentifier);
			edgeView.output = outputNodeView.GetPortViewFromFieldName(serializedEdge.outputFieldName, serializedEdge.outputPortIdentifier);


			ConnectView(edgeView);
		}

		private void InitializePinnedViews()
		{
			foreach (var pinnedElement in graph.pinnedElements)
			{
				if (pinnedElement.opened)
				{
					OpenPinned(pinnedElement.editorType.type);
				}
			}
		}

        void InitializeGroups()
        {
	        foreach (var group in graph.groups)
	        {
                AddGroupView(group);
			}
			LoadedElementCount += graph.groups.Count;
		}

		void InitializeStickyNotes()
		{
#if UNITY_2020_1_OR_NEWER
			foreach (var group in graph.stickyNotes)
			{
				AddStickyNoteView(group);
			}

			LoadedElementCount += graph.stickyNotes.Count;
#endif
		}

		void InitializeStackNodes()
		{
			foreach (var stackNode in graph.stackNodes)
			{
				AddStackNodeView(stackNode);
			}
			LoadedElementCount += graph.stackNodes.Count;
		}

		protected virtual void InitializeManipulators()
		{
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
		}

		protected virtual void OnReload() {}

		#endregion

		#region Graph content modification

		public void UpdateNodeInspectorSelection()
		{
			if (nodeInspector.previouslySelectedObject != Selection.activeObject)
				nodeInspector.previouslySelectedObject = Selection.activeObject;

			HashSet<BaseNodeView> selectedNodeViews = new HashSet<BaseNodeView>();
			nodeInspector.selectedNodes.Clear();
			foreach (var e in selection)
			{
				if (e is BaseNodeView v && this.Contains(v) && v.nodeTarget.needsInspector)
					selectedNodeViews.Add(v);
			}

			nodeInspector.UpdateSelectedNodes(selectedNodeViews);
			if (Selection.activeObject != nodeInspector && selectedNodeViews.Count > 0)
				Selection.activeObject = nodeInspector;
		}

		public BaseNodeView AddNode(BaseNode node)
		{
			// This will initialize the node using the graph instance
			graph.AddNode(node);

			UpdateSerializedProperties();

			var view = AddNodeView(node);

			// Call create after the node have been initialized
			ExceptionToLog.Call(() => view.OnCreated());

			UpdateComputeOrder();

            //Logging Creation Of Node
            CCKDebug.Log("Node Created: Type - " + node.name);

			return view;
		}

		public BaseNodeView AddNodeView(BaseNode node)
		{
			var viewType = NodeProvider.GetNodeViewTypeFromType(node.GetType());

			if (viewType == null)
				viewType = typeof(BaseNodeView);

			var baseNodeView = Activator.CreateInstance(viewType) as BaseNodeView;
			baseNodeView.Initialize(this, node);
			AddElement(baseNodeView);

			nodeViews.Add(baseNodeView);
			ManagedNodeViewsPerNode[node] = baseNodeView;
			ManagedNodeViewsPerGUID[node.GUID] = baseNodeView;

			onNodeViewAdded?.Invoke(baseNodeView);
			return baseNodeView;
		}

		public void RemoveNode(BaseNode node)
		{
			var view = nodeViewsPerNode[node];
			RemoveNodeView(view);
			graph.RemoveNode(node);
		}

		public void RemoveNodeView(BaseNodeView nodeView)
		{
			RemoveElement(nodeView);
			nodeViews.Remove(nodeView);
			ManagedNodeViewsPerNode.Remove(nodeView.nodeTarget);
			ManagedNodeViewsPerGUID.Remove(nodeView.nodeTarget.GUID);
			nodeView.Dispose();
		}

		void RemoveNodeViews()
		{
			foreach (var nodeView in nodeViews)
			{
				RemoveElement(nodeView);
				nodeView.Dispose();
			}
			nodeViews.Clear();
			ManagedNodeViewsPerNode.Clear();
			ManagedNodeViewsPerGUID.Clear();
		}

		void RemoveStackNodeViews()
		{
			foreach (var stackView in stackNodeViews)
				RemoveElement(stackView);
			stackNodeViews.Clear();
		}

		void RemovePinnedElementViews()
		{
			foreach (var pinnedView in pinnedElements.Values)
			{
				if (Contains(pinnedView))
					Remove(pinnedView);
			}
			pinnedElements.Clear();
		}

        public GroupView AddGroup(Group block)
        {
            graph.AddGroup(block);
            block.OnCreated();
            return AddGroupView(block);
        }

		public GroupView AddGroupView(Group block)
		{
			GroupView groupView	 = block is LogicGroup ? new LogicGroupView() : new GroupView();

			groupView.Initialize(this, block);

			AddElement(groupView);

            groupViews.Add(groupView);
            return groupView;
		}

		public LogicGroupView AddGroup(LogicGroup block)
		{
			graph.AddGroup(block);
			block.OnCreated();
			return AddGroupView(block);
		}

		public LogicGroupView AddGroupView(LogicGroup block)
		{
			var c = new LogicGroupView();

			c.Initialize(this, block);

			AddElement(c);

			groupViews.Add(c);
			return c;
		}

		public BaseStackNodeView AddStackNode(BaseStackNode stackNode)
		{
			graph.AddStackNode(stackNode);
			return AddStackNodeView(stackNode);
		}

		public BaseStackNodeView AddStackNodeView(BaseStackNode stackNode)
		{
			var viewType = StackNodeViewProvider.GetStackNodeCustomViewType(stackNode.GetType()) ?? typeof(BaseStackNodeView);
			var stackView = Activator.CreateInstance(viewType, stackNode) as BaseStackNodeView;

			AddElement(stackView);
			stackNodeViews.Add(stackView);

			stackView.Initialize(this);

			return stackView;
		}

		public void RemoveStackNodeView(BaseStackNodeView stackNodeView)
		{
			stackNodeViews.Remove(stackNodeView);
			RemoveElement(stackNodeView);
		}

#if UNITY_2020_1_OR_NEWER
        public StickyNoteView AddStickyNote(StickyNote note)
        {
            graph.AddStickyNote(note);
            return AddStickyNoteView(note);
        }

		public StickyNoteView AddStickyNoteView(StickyNote note)
		{
			var c = new StickyNoteView();

			c.Initialize(this, note);

			AddElement(c);

            stickyNoteViews.Add(c);
            return c;
		}

		public void RemoveStickyNoteView(StickyNoteView view)
		{
			stickyNoteViews.Remove(view);
			RemoveElement(view);
		}

		public void RemoveStrickyNotes()
		{
			foreach (var stickyNodeView in stickyNoteViews)
				RemoveElement(stickyNodeView);
			stickyNoteViews.Clear();
		}
#endif

        public void AddSelectionsToGroup(GroupView view)
        {
            foreach (var selectedNode in selection)
            {
                if (selectedNode is BaseNodeView)
                {
                    if (groupViews.Exists(x => x.ContainsElement(selectedNode as BaseNodeView)))
                        continue;

                    view.AddElement(selectedNode as BaseNodeView);
                }
            }
        }

		public void AddSelectionsToGroup(LogicGroupView view)
		{
			List<BaseNodeView> nodesToAdd = new();
			
			foreach (ISelectable selectedNode in selection)
			{
				if (selectedNode is BaseNodeView node)
				{
					if (groupViews.Exists(x => x.ContainsElement(selectedNode as BaseNodeView)))
					{
						continue;
					}

					nodesToAdd.Add(node);
				}
			}

			view.AddElements(nodesToAdd);
		}

		public GroupView AddSelectionsToGroupAndReturn(GroupView view)
		{
			foreach (var selectedNode in selection)
			{
				if (selectedNode is BaseNodeView)
				{
					if (groupViews.Exists(x => x.ContainsElement(selectedNode as BaseNodeView)))
						continue;

					view.AddElement(selectedNode as BaseNodeView);
				}
			}

			return view;
		}

		public void RemoveGroups()
		{
			foreach (var groupView in groupViews)
			{
				RemoveElement(groupView);
				groupView?.Dispose();
			}
			groupViews.Clear();
		}

		public bool CanConnectEdge(EdgeView e, bool autoDisconnectInputs = true)
		{
			if (e.input == null || e.output == null)
				return false;

			var inputPortView = e.input as PortView;
			var outputPortView = e.output as PortView;
			var inputNodeView = inputPortView.node as BaseNodeView;
			var outputNodeView = outputPortView.node as BaseNodeView;

			if (inputNodeView == null || outputNodeView == null)
			{
				Debug.LogError("Connect aborted !");
				return false;
			}

			return true;
		}

		public bool ConnectView(EdgeView e, bool autoDisconnectInputs = true)
		{
			if (!CanConnectEdge(e, autoDisconnectInputs))
				return false;
			
			var inputPortView = e.input as PortView;
			var outputPortView = e.output as PortView;
			var inputNodeView = inputPortView.node as BaseNodeView;
			var outputNodeView = outputPortView.node as BaseNodeView;

			//If the input port does not support multi-connection, we remove them
			if (autoDisconnectInputs && !(e.input as PortView).portData.acceptMultipleEdges)
			{
				foreach (var edge in edgeViews.Where(ev => ev.input == e.input).ToList())
				{
					// TODO: do not disconnect them if the connected port is the same than the old connected
					DisconnectView(edge);
				}
			}
			// same for the output port:
			if (autoDisconnectInputs && !(e.output as PortView).portData.acceptMultipleEdges)
			{
				foreach (var edge in edgeViews.Where(ev => ev.output == e.output).ToList())
				{
					// TODO: do not disconnect them if the connected port is the same than the old connected
					DisconnectView(edge);
				}
			}

			AddElement(e);

			e.input.Connect(e);
			e.output.Connect(e);

			// If the input port have been removed by the custom port behavior
			// we try to find if it's still here
			if (e.input == null)
				e.input = inputNodeView.GetPortViewFromFieldName(inputPortView.fieldName, inputPortView.portData.identifier);
			if (e.output == null)
				e.output = inputNodeView.GetPortViewFromFieldName(outputPortView.fieldName, outputPortView.portData.identifier);

			edgeViews.Add(e);

			inputNodeView?.RefreshPorts();
			outputNodeView?.RefreshPorts();

			// In certain cases the edge color is wrong so we patch it
			schedule.Execute(() => {
				e.UpdateEdgeControl();
			}).ExecuteLater(1);

			e.isConnected = true;

			return true;
		}

		public bool Connect(PortView inputPortView, PortView outputPortView, bool autoDisconnectInputs = true)
		{
			var inputPort = inputPortView.owner.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.identifier);
			var outputPort = outputPortView.owner.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.identifier);

			// Checks that the node we are connecting still exists
			if (inputPortView.owner.parent == null || outputPortView.owner.parent == null)
				return false;

			var newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);

			var edgeView = CreateEdgeView();
			edgeView.userData = newEdge;
			edgeView.input = inputPortView;
			edgeView.output = outputPortView;


			return Connect(edgeView);
		}

		public bool Connect(EdgeView e, bool autoDisconnectInputs = true)
		{
			if (!CanConnectEdge(e, autoDisconnectInputs))
				return false;

			var inputPortView = e.input as PortView;
			var outputPortView = e.output as PortView;
			var inputNodeView = inputPortView.node as BaseNodeView;
			var outputNodeView = outputPortView.node as BaseNodeView;
			var inputPort = inputNodeView.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.identifier);
			var outputPort = outputNodeView.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.identifier);

			e.userData = graph.Connect(inputPort, outputPort, autoDisconnectInputs);

			ConnectView(e, autoDisconnectInputs);

			UpdateComputeOrder();

			return true;
		}

		public void DisconnectView(EdgeView e, bool refreshPorts = true)
		{
			if (e == null)
				return ;

			RemoveElement(e);

			if (e?.input?.node is BaseNodeView inputNodeView)
			{
				e.input.Disconnect(e);
				if (refreshPorts)
					inputNodeView.RefreshPorts();
			}
			if (e?.output?.node is BaseNodeView outputNodeView)
			{
				e.output.Disconnect(e);
				if (refreshPorts)
					outputNodeView.RefreshPorts();
			}

			edgeViews.Remove(e);
		}

		public void Disconnect(EdgeView e, bool refreshPorts = true)
		{
			if (null == e)
			{
				return;
			}
			// Remove the serialized edge if there is one
			if (e.userData is SerializableEdge serializableEdge)
				graph.Disconnect(serializableEdge.GUID);

			DisconnectView(e, refreshPorts);

			UpdateComputeOrder();
		}

		public void Disconnect(PortView portView, bool refreshPorts = true)
		{
			if (null == portView)
			{
				return;
			}
			var allEdges = portView.GetEdges().ToArray();
			foreach(var edge in allEdges)
			{
				Disconnect(edge as EdgeView, refreshPorts);
			}
		}

		public void RemoveEdges()
		{
			foreach (var edge in edgeViews)
				RemoveElement(edge);
			edgeViews.Clear();
		}

		public void UpdateComputeOrder()
		{
			graph.UpdateComputeOrder();

			computeOrderUpdated?.Invoke();
		}

		public void RegisterCompleteObjectUndo(string name)
		{
			Undo.RegisterCompleteObjectUndo(graph, name);
		}

		public void SaveGraphToDisk()
		{
			if (graph != null)
			{
				graph.SaveToDisk();
			}
		}

		public void ToggleView< T >() where T : PinnedElementView
		{
			ToggleView(typeof(T));
		}

		public void ToggleView(Type type)
		{
			PinnedElementView view;
			pinnedElements.TryGetValue(type, out view);

			if (view == null)
				OpenPinned(type);
			else
				ClosePinned(type, view);
		}

		public void OpenPinned< T >() where T : PinnedElementView
		{
			OpenPinned(typeof(T));
		}

		public void OpenPinned(Type type)
		{
			PinnedElementView view;

			if (type == null)
				return ;

			PinnedElement elem = graph.OpenPinned(type);

			if (!pinnedElements.ContainsKey(type))
			{
				view = Activator.CreateInstance(type) as PinnedElementView;
				if (view == null)
					return ;
				pinnedElements[type] = view;
				view.InitializeGraphView(elem, this);
			}
			view = pinnedElements[type];

			if (!Contains(view))
				Add(view);
		}

		public void ClosePinned< T >(PinnedElementView view) where T : PinnedElementView
		{
			ClosePinned(typeof(T), view);
		}

		public void ClosePinned(Type type, PinnedElementView elem)
		{
			pinnedElements.Remove(type);
			Remove(elem);
			graph.ClosePinned(type);
		}

		public Status GetPinnedElementStatus< T >() where T : PinnedElementView
		{
			return GetPinnedElementStatus(typeof(T));
		}

		public Status GetPinnedElementStatus(Type type)
		{
			var pinned = graph.pinnedElements.Find(p => p.editorType.type == type);

			if (pinned != null && pinned.opened)
				return Status.Normal;
			else
				return Status.Hidden;
		}

		public void ResetPositionAndZoom()
		{
			graph.position = Vector3.zero;
			graph.scale = Vector3.one;

			UpdateViewTransform(graph.position, graph.scale);
			
		}

		/// <summary>
		/// Deletes the selected content, can be called form an IMGUI container
		/// </summary>
		public void DelayedDeleteSelection() => this.schedule.Execute(() => DeleteSelectionOperation("Delete", AskUser.DontAskUser)).ExecuteLater(0);

		protected virtual void OnInitializeView() {}

		public virtual IEnumerable<(string path, Type type)> FilterCreateNodeMenuEntries()
		{
			// By default we don't filter anything
			foreach (var nodeMenuItem in NodeProvider.GetNodeMenuEntries(graph))
				yield return nodeMenuItem;

			// TODO: add exposed properties to this list
		}

		// public RelayNodeView AddRelayNode(PortView inputPort, PortView outputPort, Vector2 position)
		// {
		// 	var relayNode = BaseNode.CreateFromType<RelayNode>(position);
		// 	var view = AddNode(relayNode) as RelayNodeView;
		//
		// 	if (outputPort != null)
		// 		Connect(view.inputPortViews[0], outputPort);
		// 	if (inputPort != null)
		// 		Connect(inputPort, view.outputPortViews[0]);
		//
		// 	return view;
		// }

		/// <summary>
		/// Update all the serialized property bindings (in case a node was deleted / added, the property pathes needs to be updated)
		/// </summary>
		public void SyncSerializedPropertyPathes()
		{
			foreach (var nodeView in nodeViews)
				nodeView.SyncSerializedPropertyPathes();
			nodeInspector.RefreshNodes();
		}

		

        #endregion

        public virtual void Refresh()
        {

        }

		public void EnableInteraction(bool enabled)
		{
			Interactable = enabled;
			nodeViews.ForEach(x => x?.SetInteractable(enabled));
            groupViews.ForEach(x => { x.SetInteractable(enabled);});
            edgeViews.ForEach(x => { x.SetEnabled(enabled); });
            stackNodeViews.ForEach(x => { x.SetInteractable(enabled);});
        }
		
		public void SelectNodesOfType<TNode>() where TNode : BaseNode
		{
			foreach (BaseNodeView view in nodeViews)
			{
				if (view.nodeTarget.GetType() == typeof(TNode))
				{
					AddToSelection(view);
				}
			}
		}
	}
}
