using GraphProcessor;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers.Utility;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.SearchService;


namespace SkillsVRNodes.Managers.Setup
{
	public static class SetupMainGraph
	{
		//[MenuItem("Skills Node/Current Scene/Set Up Main Graph")]
		public static void SetUpMainGraph(MainGraph graph = null)
		{
			MainGraphExecutorComponent graphExecutorComponent = GraphSetupTools.ValidateOrCreateGameObjectWithComponent<MainGraphExecutorComponent>("Main Graph Controller");

			if (graphExecutorComponent.graph == null)
			{
				string path = SceneManager.GetActiveScene().path;
				path = System.IO.Path.GetDirectoryName(path);

				if(graph == null)
					graph = ScriptableObjectManager.CreateScriptableObject<MainGraph>(path, SceneManager.GetActiveScene().name + "Graph");

				graphExecutorComponent.graph = graph;
				graph.position = new UnityEngine.Vector3(580,200,0);
				EditorCoroutineUtility.StartCoroutineOwnerless((Delayed(graphExecutorComponent.graph)));
			}

			//TODO: Remove Mechanics package Reference
			LogExporter logExporter = GraphSetupTools.ValidateOrCreateGameObjectWithComponent<LogExporter>("Log Exporter");
		}

		static IEnumerator Delayed(MainGraph graph)
		{
			yield return new EditorWaitForSeconds(0.1f);
			SkillsVRGraphWindow.RefreshGraph();
			SkillsVRGraphWindow.OpenGraph(graph);
		}
		
		//[MenuItem("Skills Node/New Scene/Create Main Graph")]
		public static void NewMainGraphScene()
		{
			string scenePath = GraphSetupTools.AskForNameAndCreateSceneAndAddToBuild(false);

			if (string.IsNullOrEmpty(scenePath))
			{
				return;
			}

			SetUpMainGraph();
			EditorSceneManager.SaveOpenScenes();
		}

		public static MainGraph InitOrNewMainGraphAndScene(string mainGraphName)
		{
			
			if (string.IsNullOrWhiteSpace(mainGraphName))
			{
				mainGraphName = "MainGraphGraph.asset";
			}
			if (!mainGraphName.EndsWith(".asset"))
			{
				mainGraphName += ".asset";
			}
			EditorUtility.DisplayProgressBar("Init Main Graph", "", 0.1f);
			string rawMainGraphName = mainGraphName.TrimEnd(".asset");
			string projectBaseLocation = "Assets" + GraphSetupTools.projectLocationFolder + "/" + rawMainGraphName;
			string projectBaseSceneLocation = projectBaseLocation + "/Scenes" + "/" + rawMainGraphName;
			string projectSceneLocationFolder = projectBaseLocation + "/Scenes" + "/" + mainGraphName;
			//string assetPath = "Assets" + GraphSetupTools.SceneFolder + "/" + mainGraphName;
			MainGraph graph = AssetDatabase.LoadAllAssetsAtPath(projectSceneLocationFolder).FirstOrDefault() as MainGraph;
			GraphProjectData graphProjectData = null;
			string projectDataPath = null;
			if (null == graph)
			{
                EditorUtility.DisplayProgressBar("Init Main Graph", "Creating new files...", 0.2f);
                graph = UnityEngine.ScriptableObject.CreateInstance<MainGraph>();
				graphProjectData = UnityEngine.ScriptableObject.CreateInstance<GraphProjectData>();

				if (!Directory.Exists(projectSceneLocationFolder))
					Directory.CreateDirectory(projectSceneLocationFolder);

				if (!Directory.Exists(projectBaseSceneLocation))
					Directory.CreateDirectory(projectBaseSceneLocation);

				AssetDatabase.CreateAsset(graph, projectSceneLocationFolder);
				
				AssetDatabase.SaveAssets();
                EditorUtility.DisplayProgressBar("Init Main Graph", "Setup project...", 0.5f);
                int projectID = 0;
				string projectPath = projectBaseSceneLocation + "/" + "Project" + projectID + ".asset";

				while (File.Exists(projectPath))
				{
					projectID++;
					projectPath = projectBaseSceneLocation + "/" + "Project" + projectID + ".asset";
				}

				graphProjectData.mainGraphData ??= new ProjectSceneData();

				graphProjectData.mainGraphData.graphGraph = graph;
				graphProjectData.mainGraphData.graphTitle = graph.ToString();

				AssetDatabase.Refresh();
                GraphFinder.CurrentActiveProject = graphProjectData;
                AssetDatabaseFileCache.ClearAllProjectData();
               

                string menuDataLocation = projectBaseLocation + "/MenuData";
				graphProjectData.packageNameScriptable = ScriptableObjectManager.CreateScriptableObject<PackageNameScriptable>
					(menuDataLocation, "Package");

				graphProjectData.packageNameScriptable.ProductName = rawMainGraphName;


                graphProjectData.brandingScriptable = ScriptableObjectManager.CreateScriptableObject<BrandingScriptable>
					(menuDataLocation, "Branding");

				AssetDatabase.CreateAsset(graphProjectData, projectPath);
				projectDataPath = projectPath;
                AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
            }

            EditorUtility.DisplayProgressBar("Init Main Graph", "Setup scenes...", 0.8f);
            string scenePath = graph.GetDefaultGraphScenePath();
			var sceneName = Path.GetFileNameWithoutExtension(scenePath);
			if (!string.IsNullOrWhiteSpace(GraphSetupTools.CreateSceneAndAddToBuild(sceneName, false, rawMainGraphName)))
			{
				MainGraphExecutorComponent graphExecutorComponent = GraphSetupTools.ValidateOrCreateGameObjectWithComponent<MainGraphExecutorComponent>("Main Graph Controller");
				graphExecutorComponent.graph = graph;

				//TODO: Remove Mechanics package Reference
				LogExporter logExporter = GraphSetupTools.ValidateOrCreateGameObjectWithComponent<LogExporter>("Log Exporter");
			}


			SaveOpenSceneExcludingUntitled();
            
			if (null == graphProjectData && !string.IsNullOrWhiteSpace(projectDataPath))
			{
                // This is a weird case.
                // The new created local var graphProjectData instance may lose here.
                // EditorSceneManager.NewScene() is the thing cause this.
                // so if has has data path, which means was created a new one,
                // try to load it here to get the instance back.

                graphProjectData = AssetDatabase.LoadAssetAtPath<GraphProjectData>(projectDataPath);
			}
            if (graphProjectData != null)
			{
                var sceneObj = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                graphProjectData.mainGraphData.scene = sceneObj;
            }
			else
			{
				Debug.LogError("Fail to create Project data");
			}
			EditorUtility.ClearProgressBar();
			return graph;
		}

		private static void SaveOpenSceneExcludingUntitled()
		{
            int sceneCount = EditorSceneManager.sceneCount;

            for (int i = 0; i < sceneCount; i++)
            {

                if (!string.IsNullOrEmpty(EditorSceneManager.GetSceneAt(i).path))
                {
                    EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(i));
                }

            }
        }

		public static void InitOrCreateMainGraphScene(this MainGraph graph)
		{
            string sceneName = graph.GetDefaultGraphScenePath();
            sceneName = Path.GetFileNameWithoutExtension(sceneName);

			string mainGraphName = graph.name;
			if (!string.IsNullOrWhiteSpace(GraphSetupTools.CreateSceneAndAddToBuild(sceneName, false, mainGraphName)))
            {
                MainGraphExecutorComponent graphExecutorComponent = GraphSetupTools.ValidateOrCreateGameObjectWithComponent<MainGraphExecutorComponent>("Main Graph Controller");
                graphExecutorComponent.graph = graph;
                SetUpMainGraph(graph);
                EditorSceneManager.SaveOpenScenes();
            }
        }
    }
}
