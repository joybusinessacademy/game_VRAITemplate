using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Props;
using Scripts.VisualElements;
using SkillsVRNodes.Editor.NodeViews;
using SkillsVRNodes.Managers.GraphNavigator;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.Graph
{
	public class SkillsVRGraphView : BaseGraphView
	{
		public SkillsVRGraphToolbar toolbar { get; private set; }
		private SkillsVRGraphModifyView viewModing = new SkillsVRGraphModifyView();

		public SkillsVRGraphHelperBar helperbar { get; private set; }

		private VisualElement runtimePanel;
		public SkillsVRGraphView(BaseGraphWindow window) : base(window)
		{
			style.position = new StyleEnum<Position>(Position.Relative);
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/SkillsVR"));

			helperbar = new SkillsVRGraphHelperBar(this);
			toolbar = new SkillsVRGraphToolbar(this);
			Add(helperbar);
			Add(toolbar);

			VisualElement content = new VisualElement
			{
				name = "content",
				style = { flexGrow = 1 }
			};
			Add(content);
			content.pickingMode = PickingMode.Ignore;
			
			content.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

			VisualElement alertSpot = new VisualElement();
			alertSpot.name = "heading";

			content.Add(alertSpot);

			toolbar.CreateTitle();
			CreateRuntimePanel(content);

			this.PlayModeEventListener.OnPlayMode += runtimePanel.SetDisplay;

			BaseNode.customNodeEvent += OnReceiveNodeEvent;
		}


		public override void Dispose()
		{
			BaseNode.customNodeEvent -= OnReceiveNodeEvent;
			toolbar?.RemoveFromHierarchy();
			toolbar?.Dispose();
			toolbar = null;

			base.Dispose();
		}

		public override void HandleEvent(EventBase evt)
        {
			//viewModing.HandleEvent(evt, this);
			try
			{
				base.HandleEvent(evt);
			}
			catch (System.Exception e)
			{
				if (e.Message.Contains("entry"))
					Debug.LogWarning("Incorrect Usage of Graph: " + e.Message);
				else
					Debug.LogError(e.Message + "\r\n" + e.StackTrace);
			}
        }

		protected void CreateRuntimePanel(VisualElement content)
		{
			if (null != runtimePanel)
			{
				content.Remove(runtimePanel);
			}
			runtimePanel = new VisualElement();
			runtimePanel.pickingMode = PickingMode.Ignore;
            runtimePanel.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
			runtimePanel.style.paddingTop = 20;
            content.Add(runtimePanel);

			Image lockImage = new Image() {
				image = Resources.Load<Texture2D>("Icon/Lock"),
            };
			lockImage.style.height = 36;
            lockImage.style.width = 36;
            runtimePanel.Add(lockImage);

			Label text = new Label()
            {
                text = "Application is Playing\r\nRuntime Modification Not Allowed",
                name = "subHeading",
                pickingMode = PickingMode.Ignore
            };
			runtimePanel.Add(text);
        }

		public override void LoadGraph(BaseGraph graph)
		{
			BuildTitleByGraph(graph);
			base.LoadGraph(graph);
			RefreshElements();
			this.ExecOnceOnRenderReady(RefreshSceneActiveFromCurrentMainGraph);
		}

		public override IEnumerator LoadGraphRoutine(BaseGraph graphToLoad)
		{
			BuildTitleByGraph(graphToLoad);

			var runtimePanelLabel = runtimePanel.Q<Label>("subHeading");
			string originLable = runtimePanelLabel?.text;
			bool originDisplay = runtimePanel.GetDisplay();
			string loadingTitle = "Loading...\r\nPlease wait.";
			runtimePanelLabel.text = loadingTitle;
			runtimePanel.Show();

			bool originInteractable = this.Interactable;
			EnableInteraction(false);

			var loadingRoutine = base.LoadGraphRoutine(graphToLoad);
			yield return loadingRoutine.RunWithYieldReturnProxy((routine) =>
			{
				float proc = 0 == TotalElementCount? 0.0f : (float)LoadedElementCount / TotalElementCount;
				runtimePanelLabel.text = string.Format("Loading... {0}/{1} {2}%\r\nPlease Wait.", LoadedElementCount, TotalElementCount,
					 (proc * 100.0f).ToString("F2"));
			});

			EnableInteraction(originInteractable);

			runtimePanelLabel.text = originLable;
			runtimePanel.SetDisplay(originDisplay);

			RefreshElements();
			this.ExecOnceOnRenderReady(RefreshSceneActiveFromCurrentMainGraph);
		}

		private void BuildTitleByGraph(BaseGraph graph)
		{
			if (graph != null)
			{

				if (graph.GetType() != typeof(MainGraph))
				{
					toolbar.Breadcrumbs.SetupBreadcrumb(new Dictionary<string, Action>()
					{
						{ AssetDatabaseFileCache.GetCurrentMainGraphName(), GraphNavigatorVisualElement.OpenCurrentProject },
						{ ObjectNames.NicifyVariableName(graph.name.Replace("Graph", "")), null },
					});
				}
				else
				{
					toolbar.Breadcrumbs.SetupBreadcrumb(new Dictionary<string, Action>()
					{
						{ AssetDatabaseFileCache.GetCurrentMainGraphName(), null },
					});
				}
			}
			else
			{
				toolbar.Breadcrumbs.SetupBreadcrumb(new Dictionary<string, Action>() { { "Main Graph", null } });
			}

			RefreshElements();
		}

		private void RefreshSceneActiveFromCurrentMainGraph()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			var runningGraph = this.graph.FindRunningInstance();
			var activeNodes = runningGraph.GetAllActiveNodes();
			foreach (var node in activeNodes)
			{
				OnReceiveNodeActive(node, true);
			}
		}

		private void OnReceiveNodeEvent(BaseNode node, object key, object value)
		{
			switch (key)
			{
				case null:
					return;
				case "NodeActive":
					OnReceiveNodeActive(node, value);
					break;
			}
		}

		private void OnReceiveNodeActive(BaseNode node, object activeDataObj)
		{
			if (null == node || activeDataObj is not bool active)
			{
				return;
			}

			SceneNode sceneNode = node as SceneNode;
			if (null == sceneNode)
			{
				return;
			}
			OpenGraphBySceneNodeActive(sceneNode, active);
		}

		private void OpenGraphBySceneNodeActive(SceneNode sceneNode, bool active)
		{
			if (!active)
			{
				return;
			}

			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}
			if (null == this.graph || this.graph.GetDefaultGraphScenePath() == sceneNode.scenePath)
			{
				return;
			}
			BaseGraph graph = GraphFinder.GetDefaultGraphByScenePath(sceneNode.scenePath);
			if (graph != null && this.parentEditorWindow != null)
			{
				parentEditorWindow.LoadGraph(graph);
			}
		}

		public override void Refresh()
		{
			base.Refresh();
#if UNITY_EDITOR
			if (PropManager.Instance)
				PropManager.Instance.FindAndAddAllProps();
#endif
			SkillsVRGraphWindow.OpenGraph(SkillsVRGraphWindow.GetWindow.graph);
			RefreshElements();
			GraphNavigator.RefreshWindow();
		}

		public void RefreshElements()
		{
			toolbar?.Refresh();
		}
	}
}