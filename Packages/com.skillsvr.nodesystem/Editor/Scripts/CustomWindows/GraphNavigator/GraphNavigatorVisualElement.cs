using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BuildScripts.Build;
using GraphProcessor;
using JetBrains.Annotations;
using Scripts.VisualElements;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Editor.NodeViews;
using SkillsVRNodes.Managers.Setup;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VisualElements;
using Button = UnityEngine.UIElements.Button;

namespace SkillsVRNodes.Managers.GraphNavigator
{
	public class GraphNavigatorVisualElement : DisposableVisualElement, IDisposable
	{
		public static SkillsVRGraphWindow GraphWindow => EditorWindow.GetWindow<SkillsVRGraphWindow>();
		public static bool GetGraphWindowOpen => EditorWindow.HasOpenInstances<SkillsVRGraphWindow>();

		public static GraphProjectData CurrentActiveProject {
			get 
			{
				return GraphFinder.CurrentActiveProject;
            }
		}

		private const string SCENE_PATH_KEY = "xNodeScenePath";
		private const string BUILD_PICO_KEY = "BuildToPico";
		private const string BUILD_META_KEY = "BuildToMeta";
		
		private readonly Label projectName;
		private readonly Button ProjectOptions;
		private readonly ScrollView scenes;
		private readonly Button buildButton;
		private VisualElement currentProject;

		private HashSet<string> cachedActiveSceneNodeSet { get; } = new HashSet<string>();
		private SmartPlayModeEventHandler PlayModeEventHandler { get; } = new SmartPlayModeEventHandler();
		public GraphNavigatorVisualElement()
		{
			name = "graphNavigator";
			styleSheets.Add(Resources.Load<StyleSheet>("SkillsStyles/graphNavigator"));
			
			// start of toolbar
			VisualElement toolbar = new VisualElement
			{
				name = "toolbar",
			};
			Add(toolbar);
			
			buildButton = new Button
			{
				name = "build-button",
				text = "Build"
			};
			toolbar.Add(buildButton);
			buildButton.clicked += OnBuildButtonClicked;
			
			Button buildDropdown = new Button
			{
				name = "build-dropdown",
				tooltip = "Build Settings"
			};
			buildDropdown.Add(new Image() {image = Resources.Load<Texture2D>("Icon/Expand Down")});
			buildDropdown.clicked += BuildDropdown;
			toolbar.Add(buildDropdown);
			
			ProjectOptions = new Button
			{
				name = "current-project-button"
			};
			ProjectOptions.clicked += ProjectOptionsDropdown;
			ProjectOptions.Add(new Label("Options"));
			ProjectOptions.Add(new Image() {image = Resources.Load<Texture2D>("Icon/Expand Down")});
			toolbar.Add(ProjectOptions);
			
			// Start of project dropdown=
			currentProject = new VisualElement()
			{
				name = "project",
				tooltip = "Main Graph"
			};
			Add(currentProject);
			currentProject.RegisterCallback<ClickEvent>(evt =>
			{
				if (evt.clickCount == 2)
				{
					OpenCurrentProject();
				}
			});
			currentProject.RegisterCallback<ContextClickEvent>(evt =>
			{
				ProjectOptionsDropdown();
			});
			
			VisualElement projectButton = new VisualElement
			{
				name = "project-button"
			};
            projectName = new Label(CurrentActiveProject ? CurrentActiveProject.GetProjectName : "No Active Project");
			projectButton.Add(projectName);
			currentProject.Add(projectButton);
			
			// Start of scenes
			scenes = new ScrollView()
			{
				name = "content",
				style = { flexGrow = 1}
			};
			Add(scenes);

			RefreshSceneButtons();
			
            BaseNodeView.customNodeViewEvent += OnReceiveNodeViewEvent;
			BaseNode.customNodeEvent += OnReceiveNodeEvent;

			SetGraphWindow();
			BaseGraphWindow.onGraphWindowActiveChanged += OnAnyGraphWindowActiveChanged;
			this.ExecOnceOnRenderReady(RefreshSceneActiveFromCurrentMainGraph);

			PlayModeEventHandler.OnExitPlayMode += cachedActiveSceneNodeSet.Clear;
			PlayModeEventHandler.OnExitPlayMode += RefreshSceneButtons;

		}
		
		~GraphNavigatorVisualElement()
		{
			Dispose();
		}

		public static void OpenCurrentProject()
		{
			OpenMainGraph(CurrentActiveProject.mainGraphData.graphGraph, CurrentActiveProject.mainGraphData.GetGraphScenePath);
		}

		private void OnBuildButtonClicked()
		{
			if (EditorPrefs.GetBool(BUILD_PICO_KEY, false))
			{
				BuildAPK.BuildToHeadset(true);

			}

			if (EditorPrefs.GetBool(BUILD_META_KEY, false))
			{
				BuildAPK.BuildToHeadset(false);
			}
		}

		private void BuildDropdown()
		{
			bool picoBool = EditorPrefs.GetBool(BUILD_PICO_KEY, false);
			bool metaBool = EditorPrefs.GetBool(BUILD_META_KEY, false);

			bool sharableBool = SessionState.GetBool("GenerateShareableToggleValue", false);


			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Build Pico"), picoBool, () =>
			{
				EditorPrefs.SetBool(BUILD_PICO_KEY, !picoBool);
				ValidateBuildButton();
			});
			menu.AddItem(new GUIContent("Build Meta"), metaBool, () =>
			{
				EditorPrefs.SetBool(BUILD_META_KEY, !metaBool);
				ValidateBuildButton();
			});
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Generate Sharable APK"), sharableBool, () => { SessionState.SetBool("GenerateShareableToggleValue", !sharableBool); });
			menu.ShowAsContext();
		}

		public void ValidateBuildButton()
		{
			if (CurrentActiveProject == null)
			{
				buildButton.tooltip = "No Project open";
				buildButton.SetEnabled(false);
				return;
			}
			
			bool picoBool = EditorPrefs.GetBool(BUILD_PICO_KEY, false);
			bool metaBool = EditorPrefs.GetBool(BUILD_META_KEY, false);

			buildButton.SetEnabled(picoBool || metaBool);
			
			if (picoBool && metaBool)
			{
				buildButton.tooltip = "Build Pico & Meta";
			}
			else if (picoBool)
			{
				buildButton.tooltip = "Build Pico";
			}
			else if (metaBool)
			{
				buildButton.tooltip = "Build Meta";
			}
			else
			{
				buildButton.tooltip = "No build platform selected";
			}
		}

		public void RefreshSceneButtons()
		{
			ValidateBuildButton();
			projectName.text = CurrentActiveProject ? CurrentActiveProject.GetProjectName : "No Active Project";
			currentProject.SetEnabled(CurrentActiveProject != null);
			scenes.Clear();

			this.Q<Button>("new-project-button")?.RemoveFromHierarchy();

			if (CurrentActiveProject == null)
			{
				this.Q<VisualElement>("toolbar").Add(new Button(NewProjectWindow.Show) { name = "new-project-button", text = "New Project" });
				
				
				scenes.Add(new Label("No scenes in Project Graph...") { name = "no-scenes-label" });

				return;
			}
			
			
			if (CurrentActiveProject == null || CurrentActiveProject.mainGraphData == null || CurrentActiveProject.mainGraphData.graphGraph == null)
			{
				return;
			}
			SceneNode[] sceneNodeList = CurrentActiveProject.mainGraphData.graphGraph.Nodes.OfType<SceneNode>()
				.GroupBy(x=> x.scenePath).Select(x=> x.FirstOrDefault())
				.Where(x=> null != x).Where(x => !x.scenePath.IsNullOrEmpty()).ToArray();
			foreach (SceneNode sceneNode in sceneNodeList)
			{
				VisualElement newElement = CreateSceneButton(sceneNode);
				if (newElement != null)
				{
					scenes.Add(newElement);
				}
			}

			if (sceneNodeList.IsNullOrEmpty())
			{
				scenes.Add(new Label("No scenes in Project Graph...") { name = "no-scenes-label" });
			}

			currentProject.Q<VisualElement>("project-tick-container")?.RemoveFromHierarchy();
			
			
			if (!GraphWindow || GraphWindow.graph is MainGraph)
			{
				VisualElement tickContainer = new()
				{
					name = "project-tick-container"
				};
				tickContainer.Add(new Image(){ image = Resources.Load<Texture2D>("Icon/Check")});
				currentProject.Insert(0, tickContainer);
			}
		}

		[CanBeNull]
		public VisualElement CreateSceneButton(SceneNode sceneNode)
		{
			if (sceneNode.scenePath.IsNullOrEmpty())
			{
				return null;
			}
			
			VisualElement sceneButton = new()
			{
				name = "scene-button"
			};
			VisualElement tickContainer = new()
			{
				name = "tick-container"
			};
			
			sceneButton.Add(tickContainer);
			sceneButton.focusable = true;

			BaseGraph graph = GraphFinder.GetDefaultGraphByScenePath(sceneNode.scenePath);
			bool isActive = IsGraphActive(graph);

			if (isActive)
			{
				sceneButton.AddToClassList(IS_OPEN);
				tickContainer.Add(new Image(){ image = Resources.Load<Texture2D>("Icon/Check")});
			}
			else
			{
				sceneButton.RemoveFromClassList(IS_OPEN);
			}
			sceneButton.Add(new Label(Path.GetFileNameWithoutExtension(sceneNode.scenePath)));
						
			// Double click
			sceneButton.RegisterCallback<ClickEvent>(evt =>
			{
				if (evt.clickCount == 1)
				{
					EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneNode.scenePath));
				}
				else if (evt.clickCount == 2)
				{
					OpenSceneNode(sceneNode);
				}
			});
			
			// Right click menu
			sceneButton.AddManipulator(new ContextualMenuManipulator(evt =>
			{
				evt.menu.AppendAction("Open", _ => OpenSceneNode(sceneNode));
				evt.menu.AppendAction("Show In Project", _ => EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneNode.scenePath)));
				evt.menu.AppendAction("Rename Scene", _ =>
				{ 
					ProjectSceneData projectSceneData = CurrentActiveProject.sceneGraphs.FirstOrDefault(t => t.GetGraphScenePath == sceneNode.scenePath);
					if (projectSceneData != null)
					{
						TryRenameScene(projectSceneData);
					}
				});
				evt.menu.AppendSeparator();
				evt.menu.AppendAction("Delete", _ => DeleteSceneCallback(sceneNode.scenePath));
			}));
			
			return sceneButton;
		}

		private bool IsGraphActive(BaseGraph baseGraph)
		{
			if (null == baseGraph)
			{
				return false;
			}

			// If graph window is open and showing this graph
			if (GetGraphWindowOpen && baseGraph == GraphWindow?.graph)
			{
				return true;
			}

			// If triggered from node active events
			if (cachedActiveSceneNodeSet.Contains(baseGraph.GetDefaultGraphScenePath()))
			{
				return true;
			}
			
			return false;
		}

		private void ProjectOptionsDropdown()
		{
			GenericMenu menu = new();
			if (CurrentActiveProject == null)
			{
				menu.AddDisabledItem(new GUIContent("Project Settings"), false);
				
				// TODO: Implement Rename for projects
				menu.AddDisabledItem(new GUIContent("Rename Project"), false);
			}
			else
			{
				menu.AddItem(new GUIContent("Project Settings"), false, MenuWindow.OpenWindow);
				menu.AddItem(new GUIContent("Rename Project"), false, TryRenameProject);
			}

			if (GraphFinder.GetAllGraphData().Count > 0)
			{
				menu.AddSeparator("");
				foreach (GraphProjectData graphProjectData in GraphFinder.GetAllGraphData())
				{
					void Func() {
                        CCKDebug.Log("Project Item: Switching - Switching Project: " + projectName);
						OpenMainGraph(graphProjectData.mainGraphData.graphGraph, graphProjectData.mainGraphData.GetGraphScenePath);	
					}
					string graphProjectName = graphProjectData.GetProjectName;

					if (String.IsNullOrWhiteSpace(graphProjectName))
					{
						string graphPath = AssetDatabase.GetAssetPath(graphProjectData.mainGraphData.graphGraph.GetInstanceID());
						string scenePath = graphPath.Replace(".asset", ".unity");
						graphProjectData.mainGraphData.scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

						graphProjectName = graphProjectData.GetProjectName;
					}

					menu.AddItem(new GUIContent("Switch Project/" + graphProjectName),
						graphProjectData == GraphFinder.CurrentActiveProject, Func);
				}
			}
			
			menu.AddItem(new GUIContent("New Project"), false, NewProjectWindow.Show);
			menu.AddItem(new GUIContent("Import Project"), false, UploadProject);
			if (CurrentActiveProject == null)
			{
				menu.AddDisabledItem(new GUIContent("Export Project"), false);
				menu.AddSeparator("");
				menu.AddDisabledItem(new GUIContent("Delete Project"), false);
			}
			else
			{
				menu.AddItem(new GUIContent("Export Project"), false, ExportProjectWindow.Show);
				menu.AddSeparator("");
				menu.AddItem(new GUIContent("Delete Project"), false, DeleteProjectWindow.ShowWindow);
			}
			
			menu.ShowAsContext();
		}

		public override void Dispose()
		{
			PlayModeEventHandler.Clear();
			BaseNodeView.customNodeViewEvent -= OnReceiveNodeViewEvent;
			BaseNode.customNodeEvent -= OnReceiveNodeEvent;
			BaseGraphWindow.onGraphWindowActiveChanged -= OnAnyGraphWindowActiveChanged;
			UnregisterGraphWindowEvents(GraphWindow);
		}

		public void SetGraphWindow()
		{
			if (!GetGraphWindowOpen)
			{
				return;
			}
			UnregisterGraphWindowEvents(GraphWindow);
			RegisterGraphWindowEvents(GraphWindow);
		}

		private void OnAnyGraphWindowActiveChanged(BaseGraphWindow window, bool isActive)
		{
			if (null == window)
			{
				return;
			}

			if (isActive)
			{
				RegisterGraphWindowEvents(window);
			}
			else
			{
				UnregisterGraphWindowEvents(window);
			}
		}

		protected void RegisterGraphWindowEvents(BaseGraphWindow window)
		{
			if (null == window)
			{
				return;
			}
			window.graphLoaded += RefreshSceneButtons;
		}

		protected void UnregisterGraphWindowEvents(BaseGraphWindow window)
		{
			if (null == window)
			{
				return;
			}
			window.graphLoaded -= RefreshSceneButtons;
		}

		private void RefreshSceneButtons(BaseGraph obj)
		{
			RefreshSceneButtons();
		}

		private void RefreshSceneActiveFromCurrentMainGraph()
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}

			var runningMainGraph = CurrentActiveProject.mainGraphData.graphGraph.FindRunningInstance();
			var activeNodes = runningMainGraph.GetAllActiveNodes();
			foreach (var node in activeNodes)
			{
				OnReceiveNodeActive(node, true);
			}
		}

		private void OnReceiveNodeViewEvent(BaseNodeView nodeView, object key, object value)
		{
			switch (key)
			{
				case null:
					return;
				case "OnSceneNodeViewRefresh":
				case "RefreshGraphWindow":
					RefreshSceneButtons();
					break;
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

			string scenePath = sceneNode.scenePath;
			if (active)
			{
				cachedActiveSceneNodeSet.Add(scenePath);
			}
			else
			{
				cachedActiveSceneNodeSet.Remove(scenePath);
			}
			
			RefreshSceneButtons();
		}

		Image currentActiveProjectImage;
		private const string IS_OPEN = "is-open";

		private void UploadProject()
		{
			string sourceFolder = EditorUtility.OpenFolderPanel("Select a project to import", "", "");

			if (string.IsNullOrEmpty(sourceFolder))
			{
				return;
			}
			string targetFolder = "Assets/Contexts";

			// Get the source folder's name
			string sourceFolderName = new DirectoryInfo(sourceFolder).Name;
			string targetFolderPath = Path.Combine(targetFolder, sourceFolderName);

			if (Directory.Exists(targetFolderPath))
			{
				EditorUtility.DisplayDialog("Failed to import",
					$"Folder with the Name \"{sourceFolderName}\" already exists inside of: Assets/Contexts/{sourceFolderName}" +
					$"\n\nPlease rename the folder you are importing", "Ok");
				return;
			}

			Directory.CreateDirectory(targetFolder);
			FileUtil.CopyFileOrDirectory(sourceFolder, targetFolderPath);

			AssetDatabase.Refresh();

            CCKDebug.Log("Project Item: Import - Imported a new Project");

			MarkDirtyRepaint();

			if (CurrentActiveProject == null)
			{
				EditorCoroutineUtility.StartCoroutineOwnerless(DelayRefresh(sourceFolderName));
			}
		}

        private static IEnumerator DelayRefresh(string projectNamePass)
        {
            // Get the currently active scene
            Scene currentScene = SceneManager.GetActiveScene();

            // Unload the current scene
            yield  return SceneManager.UnloadSceneAsync(currentScene);
            // wait one frame in case of some actions cannot start from init on load callbacks
            // i.e. create new scene when build addressables.
            yield return null;

            MainGraph mainGraph = SetupMainGraph.InitOrNewMainGraphAndScene(projectNamePass);

            yield return null;

            GraphProjectData data = mainGraph.GetGraphData();
            OpenMainGraph(data.mainGraphData.graphGraph, data.mainGraphData.GetGraphScenePath);
        }

        private static List<string> GetAllScenePathInProject(GraphProjectData project)
		{
			List<string> sceneList = new();
			if (project == null)
			{
				return sceneList;
			}

			string mainGraphScene = project.mainGraphData?.graphGraph?.GetDefaultGraphScenePath();
			if (string.IsNullOrWhiteSpace(mainGraphScene))
			{
				return sceneList;
			}

			sceneList.Add(mainGraphScene);

			if (null == project.sceneGraphs)
			{
				return sceneList;
			}

			foreach (ProjectSceneData sceneGraph in project.sceneGraphs)
			{
				string scenePath = sceneGraph?.graphGraph?.GetDefaultGraphScenePath();
				if (string.IsNullOrWhiteSpace(scenePath))
				{
					continue;
				}
				sceneList.Add(scenePath);
			}
			return sceneList;
		}

		public static void ChangeBuildSettingsBasedOnGraphSet(BaseGraph graph)
		{
			string mainGraphItem = "";


			// Populate with all paths related to scene
			List<string> projectScenePathList = new();

			GraphProjectData projectGraphs = graph.GetGraphData();
			projectGraphs.packageNameScriptable.ProductName = graph.name;

            projectScenePathList = GetAllScenePathInProject(projectGraphs);
			mainGraphItem = projectGraphs?.mainGraphData?.graphGraph.GetDefaultGraphScenePath();

			IEnumerable<SceneNode> newSceneNodes = projectGraphs.mainGraphData.graphGraph.Nodes.OfType<SceneNode>();

			foreach (SceneNode item in newSceneNodes)
			{
				projectScenePathList.Add(item.scenePath);
				if (item.additiveScenePaths.IsNullOrEmpty())
				{
					continue;
				}
				item.additiveScenePaths.ForEach(x => projectScenePathList.Add(x));
			}

            GraphFinder.CurrentActiveProject = projectGraphs;
            AssetDatabaseFileCache.SwitchProject(projectGraphs.name);
            

            MainGraph mainGraph = (MainGraph)graph;
			mainGraph.AddMainGraphSceneAndChildrenScenesToBuildSettings();			

			EditorBuildSettingsScene[] tempBuildScenes = EditorBuildSettings.scenes;
			EditorBuildSettingsScene mainGraphScene = null;

			foreach (EditorBuildSettingsScene buildScene in tempBuildScenes)
			{
				buildScene.enabled = false;

				foreach (string path in projectScenePathList)
				{
					string sceneName = Path.GetFileNameWithoutExtension(buildScene.path);
					string graphName = Path.GetFileNameWithoutExtension(path);

					if (sceneName == graphName && sceneName.Contains(graphName))
					{
						if (path == mainGraphItem)
							mainGraphScene = buildScene;

						buildScene.enabled = true;
					}
				}
			}

			List<EditorBuildSettingsScene> editorBuildSettingsList = tempBuildScenes.ToList();
			if (mainGraphScene != null)
			{
				int mainGraphIndex = tempBuildScenes.ToList().IndexOf(mainGraphScene);

				editorBuildSettingsList.RemoveAt(mainGraphIndex);
				editorBuildSettingsList.Insert(0, mainGraphScene);

				tempBuildScenes = editorBuildSettingsList.ToArray();
			}
			
			EditorBuildSettings.scenes = tempBuildScenes;

			GraphNavigator.RefreshWindow();//;RefreshSceneButtons();
		}

		public void RemoveDeletedScenesFromBuildSettings()
		{
			// Get all scenes in the Build Settings
			EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

			// Create a list to store the scenes that should be kept
			List<EditorBuildSettingsScene> validScenes = new();

			foreach (EditorBuildSettingsScene buildScene in buildScenes)
			{
				if (File.Exists(buildScene.path))
				{
					validScenes.Add(buildScene);
				}
			}

			// Update the Build Settings with the valid scenes
			EditorBuildSettings.scenes = validScenes.ToArray();
		}


		private void DeleteSceneCallback(string scenePath)
		{
			if (!File.Exists(scenePath))
			{
				return;
			}

			if (!EditorUtility.DisplayDialog("Delete Scene?", "Are you sure you want to delete this Scene?", "Yes", "No"))
			{
				return;
			}
			
			FullyDeleteScene(scenePath);
			RefreshSceneButtons();

		}

		private void FullyDeleteScene(string scenePath)
		{
			SkillsVRGraphWindow.OpenHomeGraph();

			string sceneGraphPath = scenePath.Replace(".unity", "Graph.asset");

			//Delete Scene
			File.Delete(scenePath);

			//Delete Graph
			File.Delete(sceneGraphPath);

			//Clean Project Data
			foreach (GraphProjectData item in GraphFinder.GetAllGraphData())
			{
				foreach (ProjectSceneData sceneGraph in item.sceneGraphs.Where(sceneGraph => sceneGraph.GetGraphScenePath == scenePath))
				{
					item.sceneGraphs.Remove(sceneGraph);
					break;
				}
			}

			//Clean Main Graph Node
			if (null != CurrentActiveProject)
			{
                List<SceneNode> newSceneNodes = CurrentActiveProject.mainGraphData.graphGraph.Nodes.OfType<SceneNode>().ToList();
                foreach (SceneNode sceneNode in newSceneNodes)
                {
                    if (sceneNode.scenePath != scenePath)
                    {
                        continue;
                    }
                    List<SerializableEdge> edges = new();

                    edges.AddRange(sceneNode.GetAllEdges());
                    foreach (SerializableEdge edge in edges)
                    {
                        edge.Deserialize();
                        BaseGraphWindow.Instance.graph.Disconnect(edge);
                    }

                    BaseGraphWindow.Instance.graph.RemoveNode(sceneNode);
                    break;
                }
            }

			AssetDatabase.Refresh();

			//Clean Build Settings
			RemoveDeletedScenesFromBuildSettings();
			EditorCoroutineUtility.StartCoroutineOwnerless(RebuildAfterDelay());
		}

		private IEnumerator RebuildAfterDelay()
		{
			yield return new EditorWaitForSeconds(0.1f);

			MarkDirtyRepaint();
			RefreshSceneButtons();
			BaseGraphWindow.Instance.graphView.Refresh();
		}

		private void OpenSceneNode(SceneNode sceneNode)
		{
			BaseGraph graph = GraphFinder.GetDefaultGraphByScenePath(sceneNode.scenePath);

			if (!Application.isPlaying)
			{
				SceneNodeView.OpenScene(graph, sceneNode.scenePath, sceneNode.lightingScene, sceneNode.additiveScenePaths);
			}

			if (graph != null)
			{
				GraphWindow.LoadGraph(graph);
			}
			
			MarkDirtyRepaint();
			RefreshSceneButtons();
		}
		
		public static void OpenMainGraph(BaseGraph graph, string mainScene)
		{



            if (!Application.isPlaying)
			{
				SceneNodeView.OpenScene(graph, mainScene, mainScene, new List<string>());
			}

            ChangeBuildSettingsBasedOnGraphSet(graph);

            if (graph != null)
			{
				GraphWindow.LoadGraph(graph);
			}

			if (EditorWindow.HasOpenInstances<GraphNavigator>())
			{
				var window = EditorWindow.GetWindow<GraphNavigator>("Graph Navigator", false);
				window.GraphNavigatorVisualElement.MarkDirtyRepaint();
				window.GraphNavigatorVisualElement.RefreshSceneButtons();
			}
		}
		
		
		private static void TryRenameProject()
		{
			string projName = CurrentActiveProject.mainGraphData.GetGraphScenePath;
			string pureProjName = Path.GetFileNameWithoutExtension(projName);

			string newName = AskUserForString.Show("Rename Project", "Please Insert a name for your new Project", CurrentActiveProject.mainGraphData.GraphName);

			Dictionary<string, SceneAsset> pathAssetPair = CurrentActiveProject.sceneGraphs.ToDictionary(sceneData => sceneData.GetGraphScenePath, sceneData => sceneData.scene);

			if (Directory.Exists("Assets/Contexts/" + newName))
			{
				EditorUtility.DisplayDialog("Failed to rename",
					$"Project with the Name \"{Path.GetFileNameWithoutExtension(newName)}\" already exists inside of: {newName}" +
					$"\n\nPlease rename the project you are renaming", "Ok");
				return;
			}

            string packageDataLocation = "Assets/Contexts/" + pureProjName + "/MenuData/Package.asset";
            PackageNameScriptable asset = AssetDatabase.LoadAssetAtPath<PackageNameScriptable>(packageDataLocation);
			if (asset != null)
			{
				asset.ProductName = newName;      
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }else
			{
				//get the asset ref  in project data - a catch for old CCK project which had the redudant folder in their structure
				GraphProjectData data = GraphFinder.GetGraphData(CurrentActiveProject.mainGraphData.graphGraph);
				if (data != null)
				{
                    data.packageNameScriptable.ProductName = newName;
                    EditorUtility.SetDirty(data.packageNameScriptable);
                    AssetDatabase.SaveAssets();
                }
            }

            AssetDatabase.RenameAsset(CurrentActiveProject.mainGraphData.GetGraphScenePath, Path.GetFileNameWithoutExtension(newName));
			AssetDatabase.RenameAsset(CurrentActiveProject.mainGraphData.GetGraphAssetPath, Path.GetFileNameWithoutExtension(newName));
			AssetDatabase.Refresh();
			Rename("Assets/Contexts/" + Path.GetFileNameWithoutExtension(projName), "Assets/Contexts/" + newName);
			AssetDatabase.Refresh();
			
			List<SceneNode> allSceneNodes = CurrentActiveProject.mainGraphData.graphGraph.nodes.OfType<SceneNode>().ToList();
			foreach (var sceneNode in allSceneNodes)
			{
				string originalPath = sceneNode.scenePath;

				// Define the pattern to match the string between "Contexts/" and "/Scenes/"
				string pattern = @"(?<=Contexts\/)(.*)(?=\/Scenes)";

				// Replace the matched substring with the newContext
				string newPath = Regex.Replace(originalPath, pattern, newName);

				sceneNode.scenePath = newPath;
			}




            AssetDatabase.SaveAssets();
			SkillsVRGraphWindow.RefreshGraph();
		}
		
		private static bool TryRenameScene(ProjectSceneData sceneData)
		{
			string newName = AskUserForString.Show("Rename Scene", "Please insert a new name for your scene", sceneData.GraphName);

			string newScenePath = sceneData.GetGraphScenePath;
			string newGraphPath = sceneData.GetGraphAssetPath;
			newScenePath = newScenePath.Replace(Path.GetFileName(sceneData.GetGraphScenePath), newName + ".unity");
			newGraphPath = newGraphPath.Replace(Path.GetFileName(sceneData.GetGraphAssetPath), newName + ".asset");
			
			if (File.Exists(newScenePath) || File.Exists(newGraphPath))
			{
				EditorUtility.DisplayDialog("Failed to rename",
					$"Scene with the Name \"{Path.GetFileNameWithoutExtension(newName)}\" already exists", "Ok");
				return false;
			}

			
			// Makes sure all scene nodes now reference the correct node
			foreach (SceneNode sceneNode in CurrentActiveProject.mainGraphData.graphGraph.nodes.Where(t => t is SceneNode).Cast<SceneNode>())
			{
				if (sceneNode.scenePath == sceneData.GetGraphScenePath)
				{
					sceneNode.scenePath = newScenePath;
					break;
				}
			}

			AssetDatabase.RenameAsset(sceneData.GetGraphScenePath, Path.GetFileNameWithoutExtension(newName));
			AssetDatabase.RenameAsset(sceneData.GetGraphAssetPath, Path.GetFileNameWithoutExtension(newName));
			AssetDatabase.Refresh();
			
			SkillsVRGraphWindow.RefreshGraph();

			return true;
		}
		
		public static void Rename(string originalName, string newName)
		{
			try
			{
				Directory.Move(originalName, newName);
				File.Delete(originalName + ".meta");
			}
			catch(IOException ioe) 
			{
				Debug.LogError(ioe.ToString( ));
			}
			catch(Exception e) 
			{
				// catch any other exceptions
				Debug.LogError(e.ToString( ));
			} 
		}
	}
}
