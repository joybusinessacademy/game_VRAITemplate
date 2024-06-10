using System.Linq;
using System;
using GraphProcessor;
using NodeGraphProcessor.Examples;
using SkillsVRNodes;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts.SceneManegment;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

 
namespace SkillsVRNodes.Editor.Graph
{
	[Serializable]
	public class SkillsVRSceneGraphWindow : EditorWindow
	{
			
		
		protected VisualElement rootView;
		protected BaseGraphView graphView;

		[SerializeField]
		protected SceneGraph graph;
		protected NodeExecutor nodeExecutor;

		readonly string graphWindowStyle = "GraphProcessorStyles/BaseGraphView";

		public bool IsGraphLoaded => graphView != null && graphView.graph != null;

		private bool reloadWorkaround = false;

		public event Action<BaseGraph> graphLoaded;
		public event Action<BaseGraph> graphUnloaded;

		/// <summary>
		/// Called by Unity when the window is enabled / opened
		/// </summary>
		protected virtual void OnEnable()
		{
			InitializeRootView();

			if (graph != null)
				LoadGraph();
			else
				reloadWorkaround = true;
		}

		void LoadGraph()
		{
	        // We wait for the graph to be initialized
	        if (graph.isEnabled)
	            InitializeGraph(graph);
	        else
	            graph.onEnabled += () => InitializeGraph(graph);
		}

		/// <summary>
		/// Called by Unity when the window is disabled (happens on domain reload)
		/// </summary>
		protected virtual void OnDisable()
		{
			if (graph != null && graphView != null)
				graphView.SaveGraphToDisk();
		}
		
		/// <summary>
		/// Called by Unity when the window is closed
		/// </summary>
		protected virtual void OnDestroy() { }

		void InitializeRootView()
		{
			rootView = base.rootVisualElement;

			rootView.name = "graphRootView";

			rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
		}

		public void InitializeGraph(SceneGraph graph)
		{
			if (this.graph != null && graph != this.graph)
			{
				// Save the graph to the disk
				EditorUtility.SetDirty(this.graph);
				AssetDatabase.SaveAssets();
				// Unload the graph
				graphUnloaded?.Invoke(this.graph);
			}

			graphLoaded?.Invoke(graph);
			this.graph = graph;

			if (graphView != null)
				rootView.Remove(graphView);

			//Initialize will provide the BaseGraphView
			InitializeWindow(graph);

			graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;

			if (graphView == null)
			{
				Debug.LogError("GraphView has not been added to the BaseGraph root view !");
				return ;
			}

			graphView.Initialize(graph);

			InitializeGraphView(graphView);

			// TOOD: onSceneLinked...

			if (graph.IsLinkedToScene())
				LinkGraphWindowToScene(graph.GetLinkedScene());
			else
				graph.onSceneLinked += LinkGraphWindowToScene;
		}

		void LinkGraphWindowToScene(Scene scene)
		{
			EditorSceneManager.sceneClosed += CloseWindowWhenSceneIsClosed;

			void CloseWindowWhenSceneIsClosed(Scene closedScene)
			{
				if (scene == closedScene)
				{
					Close();
					EditorSceneManager.sceneClosed -= CloseWindowWhenSceneIsClosed;
				}
			}
		}

		public virtual void OnGraphDeleted()
		{
			if (graph != null && graphView != null)
				rootView.Remove(graphView);

			graphView = null;
		}

		protected void InitializeWindow(BaseGraph graph) 
		{
			titleContent = new GUIContent("Scene Flow");

			if (graphView == null)
			{
				graphView = new BaseGraphView(this);
				MiniMapView minimap = new(graphView);
				graphView.Add(minimap);
				graphView.Add(new SceneGraphToolbar(graphView));

			}

			rootView.Add(graphView);

		}
		protected virtual void InitializeGraphView(BaseGraphView view) {}
	}
}