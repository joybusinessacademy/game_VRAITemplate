using System.Linq;
using System;
using GraphProcessor;
using SceneNavigation;
using SkillsVRNodes;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using SkillsVRNodes.Managers.Setup;
using Scripts.VisualElements;
using System.Collections;
using Unity.EditorCoroutines.Editor;


//[Serializable]
public abstract class BaseGraphWindow : EditorWindow
{
	public static BaseGraphWindow Instance {
		get {
			if (_instance == null)
			{
				_instance = (BaseGraphWindow)GetWindow(typeof(BaseGraphWindow), false, "CCK Graph");
			}
			return _instance;
		}
		private set { _instance = value; }
	}

	private static BaseGraphWindow _instance;

	public static bool IsOpen => _instance != null;

	public static event Action<BaseGraphWindow, bool> onGraphWindowActiveChanged;

	protected abstract void OnInitializeWindow(BaseGraph graph);

	protected VisualElement rootView;
	public BaseGraphView graphView { get; protected set; }

	public BaseGraph graph => null == graphView ? null : graphView.graph;

	readonly string graphWindowStyle = "GraphProcessorStyles/BaseGraphView";

	public bool IsGraphLoaded => graphView != null && graphView.graph != null;

	public event Action<BaseGraph> graphLoaded;
	public event Action<BaseGraph> graphUnloaded;

	public IPlayModeEventListener PlayModeEventListener { get; private set; } = new SmartPlayModeEventHandler();

	protected virtual void OnInitializeGraphView(BaseGraphView view) { }

	private EditorCoroutine loadingCoroutine;

	private bool enableRestoreGraphAfterPlay;
    /// <summary>
    /// Called by Unity when the window is enabled / opened
    /// </summary>
    protected virtual void OnEnable()
	{
		RegisterLinkedSceneEvent();
		//ProjectController projectController = new ProjectController();
		//projectController.CheckAllProjectData();

		//var defaultMainGraph = GraphFinder.LoadOrCreateDefaultMainGraph();
		//defaultMainGraph.InitOrCreateMainGraphScene();
		//defaultMainGraph.SetAsStartUpGraph();

		//if(Instance != null)
		//	Instance = this;

		InitializeRootView();

		enableRestoreGraphAfterPlay = false;

		// In some cases the scriptable object (graph) may lost its unity internal callbacks.
		// This may cause the graph asset will be not updated until press undo hot key or reload again later than the frame.
		// The followed schedule is a temp fix, init graph after unity engine ready make callback works.
		// This fix should be changed when have better solution.
		rootView.ExecOnceOnRenderReady(() => { 
			// Only load default graph in case of no manual opened graph.
			// which is for after reload script.
			if (null == this.graph)
			{
				LoadGraph(GraphFinder.CurrentGraph);
			}

			PlayModeEventListener.OnEditMode += graphView.EnableInteraction;
			PlayModeEventListener.OnEnterEditMode += OnEnterEditMode;

			// Don't trigger restore load graph twice on enable.
			enableRestoreGraphAfterPlay = true;

			onGraphWindowActiveChanged?.Invoke(this, true);
		}); 
	}  

	protected void RestoreCurrentGraph()
	{
		if (!enableRestoreGraphAfterPlay)
		{
			return;
		}
		graphView.ExecOnceOnRenderReady(() =>
		{
			var targetToLoad = GraphFinder.CurrentGraph;
			if (null == targetToLoad)
			{
				return;
			}
			// Alwasy load graph after stop playing in editor,
			// cause editor may kill some callbacks or scripable object when quit play mode.
			// Reload graph to make events/UI work as normal.
			LoadGraph(targetToLoad);
		});
	}

	/// <summary>
	/// Called by Unity when the window is disabled (happens on domain reload)
	/// </summary>
	protected virtual void OnDisable()
	{
		StopLoadingCoroutine();
		UnregisterLinkdedSceneEvent();
		onGraphWindowActiveChanged?.Invoke(this, false);

		PlayModeEventListener?.Clear();
		ReleaseGraphView();
	}

	/// <summary>
	/// Called by Unity when the window is closed
	/// </summary>
	protected virtual void OnDestroy()
	{
		if (this == _instance)
		{
			_instance = null;
		}
	}

	protected void TriggerUnloadGraphEvent()
	{
		if (null == this.graph)
		{
			return;
		}
		graphUnloaded?.Invoke(this.graph);
	}

	protected void StopLoadingCoroutine()
	{
		if (null != loadingCoroutine)
		{
			EditorCoroutineUtility.StopCoroutine(loadingCoroutine);
			loadingCoroutine = null;
		}
	}
	protected virtual void ReleaseGraphView()
	{
		if (null == graphView)
		{
			return;
		}

		if (rootView.Contains(graphView))
		{
			rootView.Remove(graphView);
		}
		graphView.StopTrackingNodeActivity();
		TriggerUnloadGraphEvent();
		graphView.UnloadGraph();
		graphView.Dispose();
		graphView = null;

		GC.Collect();
	}

	protected virtual void OnEnterEditMode()
	{
		graphView?.StopTrackingNodeActivity();
		if (enableRestoreGraphAfterPlay)
		{
			RestoreCurrentGraph();
		}
	}
	
	public void LoadGraph(BaseGraph graphToLoad, bool forceProgressiveLoading = false)
	{
		if (!EditorApplication.isPlayingOrWillChangePlaymode)
		{
			GraphFinder.CurrentGraph = graphToLoad;
		}

		StopLoadingCoroutine();
		ReleaseGraphView();
		//Initialize will provide the BaseGraphView
		OnInitializeWindow(graphToLoad);

		graphView = rootView.Children().FirstOrDefault(e => e is BaseGraphView) as BaseGraphView;

		if (graphView == null)
		{
			Debug.LogError("GraphView has not been added to the BaseGraph root view !");
			return;
		}

		int minLoadCount = 10;
		bool autoProgressiveLoading = null != graphToLoad
			&& (graphToLoad.Nodes.Count > minLoadCount
				|| graphToLoad.edges.Count > minLoadCount);
		if (autoProgressiveLoading 
			|| forceProgressiveLoading)
		{
			loadingCoroutine = EditorCoroutineUtility.StartCoroutine(LoadGraphRoutine(graphToLoad), this);
		}
		else
		{
			graphView.LoadGraph(graphToLoad);
			OnLoadGraphEnd();
		}
	}

	protected IEnumerator LoadGraphRoutine(BaseGraph graphToLoad)
	{
		if (graphView == null)
		{
			Debug.LogError("GraphView has not been added to the BaseGraph root view !");
			yield break;
		}

		yield return graphView.LoadGraphRoutine(graphToLoad);

		OnLoadGraphEnd();
		loadingCoroutine = null;
	}

	private void OnLoadGraphEnd()
	{
		OnInitializeGraphView(graphView);
		graphLoaded?.Invoke(graph);

		graphView?.EnableInteraction(!EditorApplication.isPlayingOrWillChangePlaymode);
		graphView?.StartTrackingNodeActivity();
	}

	public void InitializeRootView()
	{
		rootView = base.rootVisualElement;

		rootView.name = "graphRootView";

		rootView.styleSheets.Add(Resources.Load<StyleSheet>(graphWindowStyle));
	}

	#region Link Scene and Handle Scene Close Event
	private void RegisterLinkedSceneEvent()
	{
		EditorSceneManager.sceneClosed += CloseWindowIfIsLinkedScene;
	}
	private void UnregisterLinkdedSceneEvent()
	{
		EditorSceneManager.sceneClosed += CloseWindowIfIsLinkedScene;
	}
	private void CloseWindowIfIsLinkedScene(Scene closedScene)
	{
		if (null == graph)
		{
			return;
		}
		var targetScene = graph.GetLinkedScene();
		if (null == targetScene)
		{
			return;
		}

		if (targetScene != closedScene)
		{
			return;
		}
		Close();
	}
	#endregion

	public virtual void OnGraphDeleted()
	{
		TriggerUnloadGraphEvent();
		ReleaseGraphView();
	}

}
